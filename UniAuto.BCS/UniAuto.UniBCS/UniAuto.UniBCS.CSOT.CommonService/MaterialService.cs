using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.OpiSpec;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.MesSpec;
using System.Globalization;

namespace UniAuto.UniBCS.CSOT.CommonService
{
      public class MaterialService : AbstractService
      {
            private const string ConsumeEventReportTimeout = "ConsumeEventReportTimeout";
            private const string MaterialStatusChangeReportTimeout = "MaterialStatusChangeReportTimeout";
            private const string MaterialWeightReportTimeout = "MaterialWeightReportTimeout";
            private const string PIMaterialWeightRequestCommandTimeout = "PIMaterialWeightRequestCommandTimeout";//20171128 by huangjiayin
            // private IDictionary<string, bool> _mesReplyFlag = new Dictionary<string, bool>();
            private eMaterialStatus materialStatus1;//add by hujunpeng
            private string materialID1;//add by hujunpeng
            private string materialslotno1;//add by hujunpeng
            #region[For MaterialRealWeight]
            //Add BY Yangzhenteng&Hujunpeng For OPI Display20180904
          
            private System.Timers.Timer CommandSetTimer;
            private void InitSetTimer()
           {
                   CommandSetTimer = new System.Timers.Timer();
                   CommandSetTimer.AutoReset = true;
                   CommandSetTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnSetTimedCommandEvent);
                   int MaterialRealSendTime = ParameterManager["MaterialRealSendTime"].GetInteger();
                   CommandSetTimer.Interval = MaterialRealSendTime;
                   CommandSetTimer.Start();

            }
            private void OnSetTimedCommandEvent(object source, System.Timers.ElapsedEventArgs e) //Modify By Hujunpeng20180823
            {
                System.Timers.Timer tm = source as System.Timers.Timer;

                try
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L6");
                    Equipment eqp1 = ObjectManager.EquipmentManager.GetEQP("L2");
                    if (eqp.Data.LINEID == "CCPIL100" || eqp.Data.LINEID == "CCPIL200")
                    {
                        if (!Repository.ContainsKey("PIMaterialWeightRequestCommand"))
                        {
                            Invoke(eServiceName.MaterialService, "PIMaterialWeightRequestCommand", new object[] { "L6", eBitResult.ON, "" });
                        }
                        Invoke(eServiceName.UIService, "PIJobCountReport", new object[] { System.DateTime.Now.ToString("yyyyMMddHHmmssffff"), eqp1.File.PIJobCount });//add by hujunpeng 20190723 for PIJobCount Monitor
                    }
                    if (eqp.Data.LINEID == "FCMPH100" || eqp.Data.LINEID == "FCSPH100" || eqp.Data.LINEID == "FCOPH100" || eqp.Data.LINEID == "FCRPH100" || eqp.Data.LINEID == "FCGPH100" || eqp.Data.LINEID == "FCBPH100")
                    {
                        Invoke("EquipmentService", "MaterialStatusRequest", new object[] { "L6", "MaterialStatusRequest", System.DateTime.Now.ToString("yyyyMMddHHmmssffff") });
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
                finally
                {
                    tm.Interval = ParameterManager["MaterialRealSendTime"].GetInteger();
                }
            }
            
            #endregion
            public override bool Init()
            {
                // Add By Yangzhenteng For OPI Display 20180904
                if (ServerName.ToUpper().Contains("CCPIL") 
                    ||ServerName.ToUpper().Contains("FCMPH")||ServerName.ToUpper().Contains("FCSPH")||ServerName.ToUpper().Contains("FCGPH") 
                    ||ServerName.ToUpper().Contains("FCBPH")||ServerName.ToUpper().Contains("FCOPH")||ServerName.ToUpper().Contains("FCRPH"))
                {
                    InitSetTimer();
                }
                  return true;
            }
            #region [Consume Event Report]
            public void ConsumeEventReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger == true) return;
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                        if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                        #endregion
                        eFabType fabType;
                        Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                        if (fabType == eFabType.CELL)
                        {
                              MaterialConsumableReport_CELL(inputData);
                              return;
                        }
                        else if (fabType == eFabType.ARRAY)
                        {
                              MaterialConsumableReport_ARRAY(inputData);
                              return;
                        }
                        else
                        {
                              MaterialConsumableReport_CF(inputData);
                        }
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #region[T2 USE]
            public void ConsumeEventReport_POL(Trx inputData)
            {
                  try
                  {
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));

                        //Jun Modify 20141209 IO有改 結構變了
                        //MaterialUsedReportForPAMBlock
                        string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string jobSequenceNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string tftMaterialId = inputData.EventGroups[0].Events[0].Items[3].Value;
                        string tftPosition = inputData.EventGroups[0].Events[0].Items[4].Value;
                        string tftLotNo = inputData.EventGroups[0].Events[0].Items[5].Value;
                        string tftPartNo = inputData.EventGroups[0].Events[0].Items[6].Value;
                        string cfMaterialId = inputData.EventGroups[0].Events[0].Items[7].Value;
                        string cfPosition = inputData.EventGroups[0].Events[0].Items[8].Value;
                        string cfLotNo = inputData.EventGroups[0].Events[0].Items[9].Value;
                        string cfPartNo = inputData.EventGroups[0].Events[0].Items[10].Value;
                        string polMaterialType = inputData.EventGroups[0].Events[0].Items[11].Value;

                        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        if (triggerBit == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              ConsumeEventReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], CASSETTE SEQUENCE NO =[{4}], JOB SEQUENCE NO =[{5}], TFT MATERIAL ID =[{6}], TFT POSITION =[{7}], TFT LOT NO =[{8}], TFT PART NO =[{9}], CF MATERIAL ID =[{10}], CF POSITION =[{11}], CF LOT NO =[{12}], CF PART NO =[{13}], BIT (ON)",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, cassetteSequenceNo, jobSequenceNo, tftMaterialId, tftPosition, tftLotNo, tftPartNo, cfMaterialId, cfPosition, cfLotNo, cfPartNo));

                        ConsumeEventReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);

                        //Jun Add 20141209 Report to MES
                        IList<POLAttachComplete.MATERIALc> polList = new List<POLAttachComplete.MATERIALc>();
                        POLAttachComplete.MATERIALc tftPolAtt = new POLAttachComplete.MATERIALc();
                        POLAttachComplete.MATERIALc cfPolAtt = new POLAttachComplete.MATERIALc();
                        //20150120 modify by edison:陈忠要求如果为空，就不上报了
                        //20150121 modify by asir (1)tftMaterialId.Trim()==""change to tftMaterialId.Trim()!="" (2)cfMaterialId.Trim() == ""change to cfMaterialId.Trim() != ""
                        if (tftMaterialId.Trim() != "")
                        {
                              tftPolAtt.CARTNAME = tftMaterialId;  //tftLotNo;  Jun Modify 20150521 CARTNAME跟MATERIALNAME 的內容填錯需要互換
                              tftPolAtt.MATERIALNAME = tftLotNo;  //tftMaterialId;
                              tftPolAtt.MATERIALTYPE = "T";
                              tftPolAtt.PARTNO = tftPartNo;
                              tftPolAtt.POSITION = tftPosition;
                              tftPolAtt.ISRTP = polMaterialType;  //Jun Add 20150330 For MES New Spec
                              polList.Add(tftPolAtt);
                        }
                        if (cfMaterialId.Trim() != "")
                        {
                              cfPolAtt.CARTNAME = cfMaterialId;  //cfLotNo;  Jun Modify 20150521 CARTNAME跟MATERIALNAME 的內容填錯需要互換
                              cfPolAtt.MATERIALNAME = cfLotNo;  //cfMaterialId;
                              cfPolAtt.MATERIALTYPE = "F";
                              cfPolAtt.PARTNO = cfPartNo;
                              cfPolAtt.POSITION = cfPosition;
                              cfPolAtt.ISRTP = polMaterialType;  //Jun Add 20150330 For MES New Spec
                              polList.Add(cfPolAtt);
                        }

                        Job job = ObjectManager.JobManager.GetJob(cassetteSequenceNo, jobSequenceNo);
                        string prodName = string.Empty; string hostProdName = string.Empty;
                        if (job != null)
                        {
                              prodName = job.GlassChipMaskBlockID;
                              hostProdName = job.MesProduct.PRODUCTNAME;
                        }

                        //string trxID, string lineName, string machineName, string productName, string hostProductName, IList<POLAttachComplete.MATERIALc> materialData
                        Invoke(eServiceName.MESService, "POLAttachComplete", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, prodName, hostProdName, polList });
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void ConsumeEventReport_PI(Trx inputData)
            {
                  try
                  {
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));

                        string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string jobSequenceNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string glassID = inputData.EventGroups[0].Events[0].Items[3].Value;
                        string tankNo = inputData.EventGroups[0].Events[0].Items[4].Value;
                        string materialID = inputData.EventGroups[0].Events[0].Items[5].Value;
                        string numerator01 = inputData.EventGroups[0].Events[0].Items[6].Value;
                        string denominator01 = inputData.EventGroups[0].Events[0].Items[7].Value;

                        List<MaterialEntity> materialList = new List<MaterialEntity>();
                        MaterialEntity material = new MaterialEntity();
                        material.MaterialID = materialID.Trim();
                        double qty = 0;
                        double de = double.Parse(denominator01);
                        double nu = double.Parse(numerator01);


                        if (de == 0)
                        {
                              Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON) BUT DATA FAILED,DENOMINATOR[{2}] = 0!", inputData.Metadata.NodeNo, inputData.TrackKey
                                  , denominator01));
                              de = 1;
                        }

                        qty = nu / de;

                        material.MaterialValue = qty.ToString();
                        material.MaterialType = eMaterialType_LCD.TANK.ToString();  //先寫死，等與MES 測試才知道
                        materialList.Add(material);

                        // string sideInformation = string.Empty;
                        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        if (triggerBit == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              ConsumeEventReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], CASSETTE SEQUENCE NO =[{4}], JOB SEQUENCE N0 =[{5}], GLASS ID =[{6}], TANK NO =[{7}], MATERIAL ID =[{8}], NUMERATOR#01 =[{9}] DENOMINATOR#01 =[{10}], BIT (ON)",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, cassetteSequenceNo, jobSequenceNo, glassID, tankNo, materialID, numerator01, denominator01));

                        //MaterialConsumableRequest(string trxID, string lineName, Equipment eqp, Job job, IList<MaterialEntity> materilst)
                        if (materialList != null)
                        {
                              object[] _dataPrepare = new object[5]
                        { 
                            inputData.TrackKey,  /*0 TrackKey*/
                            eqp.Data.LINEID,    /*1 LineName*/
                            eqp,    /*2 Equipment*/
                            glassID,          /*3 Job*/
                            materialList,            /*4 materilst*/ 
                        };
                              Invoke(eServiceName.MESService, "MaterialConsumableRequest", _dataPrepare);
                        }

                        ConsumeEventReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void ConsumeEventReport_ODF(Trx inputData)
            {
                  try
                  {
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));

                        string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string jobSequenceNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string glassID = inputData.EventGroups[0].Events[0].Items[3].Value.Trim();
                        eMaterialType_LCD materialType = (eMaterialType_LCD)int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value);
                        string materialID = string.Empty;
                        string numerator = string.Empty;
                        string denominator = string.Empty;
                        StringBuilder strWeight = new StringBuilder();
                        //Watson Modify Report to MES 不是只記log
                        List<MaterialEntity> materialList = new List<MaterialEntity>();
                        for (int i = 5; i < 59; i = i + 3)
                        {

                              MaterialEntity material = new MaterialEntity();
                              materialID = inputData.EventGroups[0].Events[0].Items[i].Value;
                              numerator = inputData.EventGroups[0].Events[0].Items[i + 1].Value;
                              denominator = inputData.EventGroups[0].Events[0].Items[i + 2].Value;

                              material.MaterialID = materialID.Trim();
                              double qty = 0;
                              double de = double.Parse(denominator);
                              double nu = double.Parse(numerator);

                              if (material.MaterialID.Trim() != string.Empty)
                              {
                                    if (de == 0)
                                    {
                                          Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                              string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON) BUT DATA FAILED ,DENOMINATOR[{2}] = 0!", inputData.Metadata.NodeNo, inputData.TrackKey
                                              , denominator));
                                          de = 1;
                                    }

                                    qty = nu / de;

                                    material.MaterialValue = qty.ToString();
                                    material.MaterialType = materialType.ToString();
                                    materialList.Add(material);
                              }
                              materialID = "Material ID#" + (i / 3).ToString().PadLeft(2, '0') + " [" + materialID + "] ";
                              numerator = "Numerator#" + (i / 3).ToString().PadLeft(2, '0') + " [" + numerator + "] ";
                              denominator = "Denominato#" + (i / 3).ToString().PadLeft(2, '0') + " [" + denominator + "] ";
                              strWeight.Append(materialID + numerator + denominator);
                        }
                        //string sideInformation = string.Empty;

                        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        if (triggerBit == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              ConsumeEventReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], CASSETTE SEQUENCE NO =[{4}], JOB SEQUENCE NO =[{5}], GLASS ID =[{6}], MATERIAL TYPE =[{7}) {8}, BIT (ON)",
                                eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, cassetteSequenceNo, jobSequenceNo, glassID, materialType, strWeight.ToString()));

                        //MaterialConsumableRequest(string trxID, string lineName, Equipment eqp, Job job, IList<MaterialEntity> materilst)
                        if (materialList != null)
                        {
                              object[] _dataPrepare = new object[5]
                        { 
                            inputData.TrackKey,  /*0 TrackKey*/
                            eqp.Data.LINEID,    /*1 LineName*/
                            eqp,    /*2 Equipment*/
                            glassID,          /*3 Job*/
                            materialList,            /*4 materilst*/ 
                        };
                              Invoke(eServiceName.MESService, "MaterialConsumableRequest", _dataPrepare);
                        }

                        ConsumeEventReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            public void MaterialConsumableReport_CF(Trx inputData)
            {
                  try
                  {
                        #region [取得EQP資訊]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null)
                              throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));
                        #endregion

                        #region [取得Line資訊]
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                              throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                        #endregion

                        #region [拆出PLCAgent Data]
                        string unitNo = inputData[0][0][0].Value;
                        string cstSeqNo = inputData[0][0][1].Value;
                        string jobSeqNo = inputData[0][0][2].Value;

                        string materialID = inputData[0][1][0].Value;
                        string materialType = inputData[0][1][1].Value;
                        string numerator = inputData[0][1][2].Value;
                        string denominator = inputData[0][1][3].Value;

                        eBitResult bitResult = (eBitResult)int.Parse(inputData[0][2][0].Value);
                        #endregion

                        #region [Report Off]
                        if (bitResult == eBitResult.OFF)
                        {
                              LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              ConsumeEventReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }
                        else
                        {
                              LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], CASSETTE SEQUENCE NO =[{4}], JOB SEQUENCE N0 =[{5}],  MATERIAL ID =[{6}], NUMERATOR#01 =[{7}] DENOMINATOR#01 =[{8}], BIT (ON)",
                                      eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, cstSeqNo, jobSeqNo, materialID, numerator, denominator));
                        }
                        #endregion

                        #region Offline Skip to send MES
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={1}] [BCS -> MES][{0}] Material Consumable Request Send MES but OFFLINE LINENAME=[{1}].",
                                  inputData.TrackKey, eqp.Data.LINEID));
                              ConsumeEventReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                              return;
                        }
                        #endregion

                        #region Materila Entity Collection
                        List<MaterialEntity> materialList = new List<MaterialEntity>();
                        MaterialEntity materialE = new MaterialEntity();
                        double quanity = 0;
                        double de = double.Parse(denominator);
                        double nu = double.Parse(numerator);
                        if (de == 0)
                        {
                              LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON) BUT DATA FAILED, DENOMINATOR[{2}] = 0!",
                                  inputData.Metadata.NodeNo, inputData.TrackKey, denominator));
                              return;
                        }
                        quanity = nu / de;

                        materialE.NodeNo = eqp.Data.NODENO;
                        materialE.UnitNo = unitNo;
                        materialE.MaterialType = materialType;
                        materialE.MaterialID = materialID.Trim();
                        materialE.MaterialValue = quanity.ToString();
                        materialList.Add(materialE);
                        ObjectManager.MaterialManager.AddMaterial(materialE);
                        #endregion

                        #region [MES Data]
                        if (materialList != null)
                        {
                              string glassId = "";
                             // glassId = ObjectManager.JobManager.GetJobIDbyEQPNO(inputData.Metadata.NodeNo);
                              glassId = ObjectManager.JobManager.GetJob(cstSeqNo, jobSeqNo).GlassChipMaskBlockID;// 物料上报时GetJobIDbyEQPNO，会抓到最后进EQ的glassID,没有抓到当前jobid by qiumin 20170105
                              object[] _dataPrepare = new object[6]
                    { 
                        inputData.TrackKey, /*0 TrackKey*/
                        eqp.Data.LINEID,    /*1 LineName*/
                        eqp,                /*2 Equipment*/
                        glassId,            /*3 ProductName*/
                        "",                 /*4 MaterialDurableName*/
                        materialList,       /*5 materilst*/ 
                    };
                              Invoke(eServiceName.MESService, "MaterialConsumableRequest", _dataPrepare);
                        }
                        ConsumeEventReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                        #endregion

                  }
                  catch (Exception ex)
                  {
                        LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void MaterialConsumableReport_ARRAY(Trx inputData)
            {
                  try
                  {
                        #region [取得EQP資訊]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null)
                              throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));
                        #endregion

                        #region [取得Line資訊]
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                              throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                        #endregion

                        #region [拆出PLCAgent Data]
                        string unitNo = inputData[0][0][0].Value;
                        string cstSeqNo = inputData[0][0][1].Value;
                        string jobSeqNo = inputData[0][0][2].Value;

                        eBitResult bitResult = (eBitResult)int.Parse(inputData[0][2][0].Value);
                        #endregion

                        #region [Report Off]
                        if (bitResult == eBitResult.OFF)
                        {
                              LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              ConsumeEventReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }
                        #endregion

                        #region Offline Skip to send MES
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={1}] [BCS -> MES][{0}] Material Consumable Request Send MES but OFFLINE LINENAME=[{1}].",
                                  inputData.TrackKey, eqp.Data.LINEID));
                              ConsumeEventReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                              return;
                        }
                        #endregion

                        #region Materila Entity Collection
                        List<MaterialEntity> materialList = new List<MaterialEntity>();
                        for (int i = 0; i < 4; i++)
                        {
                              string materialID = inputData[0][1][4 * i + 0].Value;
                              string materialType = inputData[0][1][4 * i + 1].Value;
                              string numerator = inputData[0][1][4 * i + 2].Value;
                              string denominator = inputData[0][1][4 * i + 3].Value;
                              if (materialID.Trim().Length == 0)
                                    continue;

                              MaterialEntity materialE = new MaterialEntity();
                              double quanity = 0;
                              double de = double.Parse(denominator);
                              double nu = double.Parse(numerator);
                              if (de == 0)
                              {
                                    LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON) BUT DATA FAILED, DENOMINATOR[{2}] = 0!",
                                        inputData.Metadata.NodeNo, inputData.TrackKey, denominator));
                                    return;
                              }
                              quanity = nu / de;

                              materialE.NodeNo = eqp.Data.NODENO;
                              materialE.UnitNo = unitNo;
                              materialE.MaterialType = materialType;
                              materialE.MaterialID = materialID.Trim();
                              materialE.MaterialValue = quanity.ToString();
                              materialList.Add(materialE);
                              ObjectManager.MaterialManager.AddMaterial(materialE);
                              LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], CASSETTE SEQUENCE NO =[{4}], JOB SEQUENCE N0 =[{5}],  MATERIAL ID =[{6}], NUMERATOR#01 =[{7}] DENOMINATOR#01 =[{8}], BIT (ON)",
                                  eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, cstSeqNo, jobSeqNo, materialID, numerator, denominator));
                        }
                        #endregion

                        #region [MES Data]
                        if (materialList != null)
                        {
                              Job job = ObjectManager.JobManager.GetJob(cstSeqNo, jobSeqNo);
                              string glassId = "";
                              if (job != null)
                                    glassId = job.GlassChipMaskBlockID;

                              object[] _dataPrepare = new object[6]
                    { 
                        inputData.TrackKey, /*0 TrackKey*/
                        eqp.Data.LINEID,    /*1 LineName*/
                        eqp,                /*2 Equipment*/
                        glassId,            /*3 ProductName*/
                        "",                 /*4 MaterialDurableName*/
                        materialList,       /*5 materilst*/ 
                    };
                              Invoke(eServiceName.MESService, "MaterialConsumableRequest", _dataPrepare);
                        }
                        ConsumeEventReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                        #endregion

                  }
                  catch (Exception ex)
                  {
                        LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void MaterialConsumableReport_CELL(Trx inputData)
            {
                  try
                  {
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                        if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                        #endregion
                        string CommandNo = string.Empty;
                        if (inputData.Name.Split(new char[] { '#' }).Length == 2)
                            CommandNo = inputData.Name.Split(new char[] { '#' })[1];//Add By Yangzhenteng For POL PAM
                        #region [PLCAgent Data Bit]
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[2].Items[0].Value);
                        #endregion
                        #region[If Bit Off->Return]
                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              if (eqp.Data.NODEID.Contains("CCPAM"))
                              {
                                  ConsumeEventReportReplyForPAM(CommandNo, inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              }
                              else
                              {
                                  ConsumeEventReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              } 
                            return;
                        }
                        #endregion
                        #region [PLCAgent Data Word]
                        //Word#01
                        string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                        string recipeID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                        string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
                        string jobSequenceNo = inputData.EventGroups[0].Events[0].Items[3].Value.Trim();
                        //Word#02
                        string materialID01 = inputData.EventGroups[0].Events[1].Items[0].Value.Trim();
                        string materialType01 = inputData.EventGroups[0].Events[1].Items[1].Value.Trim();
                        string numerator01 = inputData.EventGroups[0].Events[1].Items[2].Value.Trim();
                        string denominator01 = inputData.EventGroups[0].Events[1].Items[3].Value.Trim();
                        string otherlog = string.Empty;
                        int materialCount = 1; string materialID02 = string.Empty; string materialType02 = string.Empty; string numerator02 = string.Empty; string denominator02 = string.Empty;
                        string materialDurableName = string.Empty;  
                      switch (line.Data.LINETYPE)//by line setting 
                        {
                            //PAM
                            case eLineType.CELL.CCPIL:
                            case eLineType.CELL.CCPIL_2:
                                materialID02 = inputData.EventGroups[0].Events[1].Items[4].Value.Trim();
                                materialType02 = inputData.EventGroups[0].Events[1].Items[5].Value.Trim();
                                numerator02 = inputData.EventGroups[0].Events[1].Items[6].Value.Trim();
                                denominator02 = inputData.EventGroups[0].Events[1].Items[7].Value.Trim();
                                materialCount = 2;
                                otherlog = string.Format("[materialID02={0}],[materialType02={1}],[numerator02={2}],[denominator02={3}]", materialID02, materialType02, numerator02, denominator02);
                                break;
                            case eLineType.CELL.CCRWK:
                                materialDurableName = materialID01;
                                string LlotID = inputData.EventGroups[0].Events[1].Items[4].Value.Trim();
                                materialID01 = LlotID;//POL MES 需要的materialID 
                                string LlotNo = inputData.EventGroups[0].Events[1].Items[5].Value.Trim();
                                string LlotCount = inputData.EventGroups[0].Events[1].Items[6].Value.Trim();
                                otherlog = string.Format("[LlotID={0}],[LlotNo={1}],[LlotCount={2}]", LlotID, LlotNo, LlotCount);
                                break;
                            default:
                                if (line.Data.LINETYPE.Contains(keyCellLineType.POL))//sy modify  20160705 放在deault
                                #region [POL]
                                {
                                    materialDurableName = materialID01;
                                    string LlotIDPol = inputData.EventGroups[0].Events[1].Items[4].Value.Trim();
                                    materialID01 = LlotIDPol;//POL MES 需要的materialID 
                                    string LlotNoPol = inputData.EventGroups[0].Events[1].Items[5].Value.Trim();
                                    string LlotCountPol = inputData.EventGroups[0].Events[1].Items[6].Value.Trim();
                                    otherlog = string.Format("[LlotID={0}],[LlotNo={1}],[LlotCount={2}]", LlotIDPol, LlotNoPol, LlotCountPol);
                                }
                                #endregion
                                break;
                        }
                        #endregion

                        List<MaterialEntity> materialList = new List<MaterialEntity>();
                        MaterialEntity material = new MaterialEntity();
                        material.MaterialID = materialID01.Trim();

                        double qty = 0;
                        double de = double.Parse(denominator01);
                        double nu = double.Parse(numerator01);

                        if (de == 0)
                        {
                              Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON) BUT DATA FAILED] DENOMINATOR[{2}]= 0! ", inputData.Metadata.NodeNo, inputData.TrackKey, denominator01
                                  ));
                              de = 1;
                        }
                        qty = nu / de;

                        material.MaterialValue = qty.ToString();
                        material.MaterialType = materialType01;

                      //20161121 huangjiayin edit for cell shop: if eqp report material type like:M01~M99;
                      //Get Material Manager >>Material Type(Mount Reply By Mes)
                      //if Material Type is null, use the eqp report(MES no Reply Type. Or the Material is invalid on eqp...)
                        //Only Modify materail01, PIL 2nd no care...
                      //20170317 huangjiayin: ODF  use mes  type anyway
                        #region CellMaterialTypeConvert

                        if(
                            line.Data.LINEID.ToUpper().Contains("CCODF")
                            && (eqp.Data.NODEID.ToUpper().Contains("CCSDP")
                            || eqp.Data.NODEID.ToUpper().Contains("CCLCD"))
                            )
                        {
                            List<MaterialEntity> allmaterials = new List<MaterialEntity>();
                            allmaterials = ObjectManager.MaterialManager.GetMaterials();

                            foreach (MaterialEntity mt in allmaterials)
                            {
                                if (mt == null) continue;

                                if (mt.MaterialID == material.MaterialID && !String.IsNullOrEmpty(mt.MaterialType))
                                {
                                    material.MaterialType = mt.MaterialType;
                                    break;
                                 }
 
                            }

                        }
                        #endregion

                        materialList.Add(material);
                        if (materialCount == 2)
                        {
                            MaterialEntity material2 = new MaterialEntity();
                            material2.MaterialID = materialID02.Trim();
                            double qty2 = 0;
                            double de2 = double.Parse(denominator02);
                            double nu2 = double.Parse(numerator02);

                            if (de2 == 0)
                            {
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON) BUT DATA FAILED] DENOMINATOR[{2}]= 0! Materail02", inputData.Metadata.NodeNo, inputData.TrackKey, denominator02
                                    ));
                                de2 = 1;
                            }
                            qty2 = nu2 / de2;

                            material2.MaterialValue = qty2.ToString();
                            material2.MaterialType = materialType02;
                            materialList.Add(material2);
                            //20170301 APR-PI Special : huangjiayin
                            try
                            {
                                if (material2.MaterialType.Trim() == "APR")
                                {
                                    if (!string.IsNullOrEmpty(material.MaterialID.Trim()) && !string.IsNullOrEmpty(material2.MaterialID.Trim()))
                                    {
                                        MaterialEntity APR_CON = ObjectManager.MaterialManager.GetMaterialByName(eqp.Data.NODENO,material2.MaterialID.Trim());
                                        if (APR_CON != null)
                                        {
                                            APR_CON.MaterialSpecName = inputData.TrackKey + "_" + material.MaterialID.Trim();
                                            ObjectManager.MaterialManager.AddMaterial(APR_CON);
                                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "APR:" + APR_CON.MaterialID + " Make Consume PI:" + material.MaterialID.Trim());
                                        } 
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                            }

                        }
                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], CASSETTE SEQUENCE NO =[{4}], JOB SEQUENCE N0 =[{5}],  MATERIAL ID =[{6}], RECIPEID =[{7}], Numerator =[{8}], Denominato =[{9}] ,[{10}] , BIT (ON)",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, cassetteSequenceNo, jobSequenceNo, materialID01, recipeID, numerator01, denominator01, otherlog));
                        #endregion
                        #region [MES Data]
                        if (materialList != null)
                        {
                              string glassID = "";
                              //2015/09/01 cs.chou 增加尋找glassid
                              Job job = ObjectManager.JobManager.GetJob(cassetteSequenceNo, jobSequenceNo);
                              glassID = job == null ? string.Empty : job.EQPJobID;    //20151021 cy:增加判斷是否找到WIP,避免因null物件而失敗

                              object[] _data = new object[6]
                        { 
                            inputData.TrackKey,  /*0 TrackKey*/
                            eqp.Data.LINEID,    /*1 LineName*/
                            eqp,    /*2 Equipment*/
                            glassID,          /*3 glassID*/
                            materialDurableName,     /*4 materialDurableName*/
                            materialList,            /*4 materilst*/ 
                        };
                              //MaterialConsumableRequest(string trxID, string lineName, Equipment eqp, string glassID, string materialDurableName, IList<MaterialEntity> materilst)
                              Invoke(eServiceName.MESService, "MaterialConsumableRequest", _data);
                        }
                        #endregion
                        if (eqp.Data.NODEID.Contains("CCPAM")) //Add By Yangzhenteng For PAM;
                        {
                            ConsumeEventReportReplyForPAM(CommandNo, inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                        }
                        else
                        {
                            ConsumeEventReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                        }

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            private void ConsumeEventReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        //MaterialUsedReportReply
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_MaterialConsumableReportReply") as Trx;
                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + ConsumeEventReportTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + ConsumeEventReportTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + ConsumeEventReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ConsumeEventReportReplyTimeout), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,CONSUME EVENT REPORT REPLY SET BIT =[{2}].",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            //Add By Yangzhenteng For PAM;
            private void ConsumeEventReportReplyForPAM(string CommandNo, string eqpNo, eBitResult value, string trackKey)
            {
                try
                {
                    //MaterialUsedReportReply
                    Trx outputdata;
                    if (string.IsNullOrEmpty(CommandNo))
                        outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_MaterialConsumableReportReply") as Trx;
                    else
                        outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_MaterialConsumableReportReply#" + CommandNo) as Trx;

                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    if (_timerManager.IsAliveTimer(eqpNo + "_" + ConsumeEventReportTimeout + "_" + CommandNo))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + ConsumeEventReportTimeout + "_" + CommandNo);
                    }
                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(eqpNo + "_" + ConsumeEventReportTimeout + "_" + CommandNo, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ConsumeEventReportReplyTimeoutForPAM), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,CONSUME EVENT REPORT REPLY SET BIT =[{2}].",
                        eqpNo, trackKey, value.ToString()));
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            private void ConsumeEventReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP REPLY, CONSUME EVENT REPORT TIMEOUT SET BIT (OFF).", sArray[0], trackKey));

                        ConsumeEventReportReply(sArray[0], eBitResult.OFF, trackKey);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            //Add By Yangzhenteng For PAM;
            private void ConsumeEventReportReplyTimeoutForPAM(object subjet, System.Timers.ElapsedEventArgs e)
            {
                try
                {
                    UserTimer timer = subjet as UserTimer;
                    string tmp = timer.TimerId;
                    string trackKey = timer.State.ToString();
                    string[] sArray = tmp.Split('_');
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP REPLY, CONSUME EVENT REPORT TIMEOUT SET BIT (OFF).", sArray[0], trackKey));
                    ConsumeEventReportReplyForPAM(sArray[2], sArray[0], eBitResult.OFF, trackKey);
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            #endregion

            #region [Material Status Change Report]
            public void MaterialStatusChangeReport(Trx inputData)
            {
                  try
                  {
                        #region [取得EQP資訊]
                        Equipment eqp;
                        string eqpNo = inputData.Metadata.NodeNo;
                        eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                        #endregion
                        //Watson Modify 改變順序，以LineID取的Line, 不可以用SERVERNAME, 否則會直接取不到(CUTMAX,PMT)
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        if (line != null && line.Data.FABTYPE == eFabType.ARRAY.ToString())
                        {
                              MaterialStatusChangeReport_Array(inputData);
                              return;
                        }

                        if (inputData.IsInitTrigger) return;
                        eFabType fabType;
                        Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                        //by shop
                        #region[CELL]
                        if (fabType == eFabType.CELL)
                        {
                              switch (line.Data.LINETYPE)
                              {
                                    case eLineType.CELL.CCRWK:
                                          MaterialStatusChangeReport_CELL_PAM(inputData);
                                          break;
                                    default:
                                          if (line.Data.LINETYPE.Contains(keyCellLineType.POL))//sy modify  20160705 放在deault                                          
                                          {
                                              MaterialStatusChangeReport_CELL_PAM(inputData);
                                          }                                          
                                          else
                                          {
                                              MaterialStatusChangeReport_CELL(inputData);
                                          }                                          
                                          break;
                              }
                        }
                        #endregion
                        else if (fabType == eFabType.CF)
                        {
                              if (eqp.Data.NODEATTRIBUTE == "COATER")
                              {
                                    MaterialStatusChangeReport_CF(inputData);
                              }
                              else if (eqp.Data.NODEATTRIBUTE == "EXPOSURE")
                              {
                                    MaskStateChanged_CF(inputData);
                              }
                              else
                              {
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] MATERIAL STATUS CHANGE REPORT,BUT BC NOT PROCESS.", eqp.Data.NODENO, inputData.TrackKey));
                              }
                        }
                        else if (fabType == eFabType.ARRAY)
                        {
                              Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] MATERIAL STATUS CHANGE REPORT,BUT BC NOT PROCESS.", eqp.Data.NODENO, inputData.TrackKey));
                        }
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void MaterialStatusChangeReport_Array(Trx inputData)
            {
                  try
                  {
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        string unitNo = inputData.EventGroups[0].Events[0].Items[ePLC.MaterialStatusChange_UnitNo].Value;
                        string unitId = "";
                        if (!unitNo.Equals("0"))
                        {
                              Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                              if (unit != null)
                              {
                                    unitId = unit.Data.UNITID;
                              }
                        }
                        string operatorID = inputData.EventGroups[0].Events[0].Items[ePLC.MaterialStatusChange_OperatorID].Value;
                        eMaterialStatus materialStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.MaterialStatusChange_MaterialStatus].Value);
                        string materialValue = inputData.EventGroups[0].Events[0].Items[ePLC.MaterialStatusChange_MaterialValue].Value;
                        string slotNo = inputData.EventGroups[0].Events[0].Items[ePLC.MaterialStatusChange_SlotNo].Value;
                        string materialID = inputData.EventGroups[0].Events[0].Items[ePLC.MaterialStatusChange_MaterialID].Value;

                        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        if (triggerBit == eBitResult.OFF)
                        {
                              MaterialEntity material = new MaterialEntity();
                              LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              MaterialStatusChangeReportReply(eqpNo, unitNo, "", material, eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey, true);
                              return;
                        }

                        LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO=[{3}], OPERATOR ID =[{4}], MATERIAL STATUS =[{5}], MATERIAL VALUE =[{6}], MATERIAL =[{7}], BIT (ON)",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, operatorID, materialStatus, materialValue, materialID));

                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={1}] [BCS -> MES][{0}] Mask State Changed Send MES but OFFLINE LINENAME=[{1}].",
                                  inputData.TrackKey, eqp.Data.LINEID));
                              return;
                        }

                        //取出特定幾台機台內任一片玻璃的 Job ID 上報，若則無須上報。
                        IList<Equipment> EQPList = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).
                            Where(e => e.Data.NODENO == "L2" || e.Data.NODENO == "L3" || e.Data.NODENO == "L4").ToList();
                        string productName = ObjectManager.JobManager.GetJobIDbyEQPList(EQPList);

                        string materialType = string.Empty;
                        if (line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC)
                              materialType = "TARGET";
                        else
                              materialType = "NotDefine";

                        #region Report to MES
                        //呼叫MES方法
                        object[] _data = new object[16]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,     /*1 LineName*/
                    eqp.Data.NODEID,     /*2 EQPID*/
                    "",                  /*3 LINERECIPENAME*/
                    "",                  /*4 MATERIALMODE*/ 
                    productName,         /*5 PRODUCTNAME*/
                    materialID.Trim(),   /*6 MATERIALNAME*/
                    materialStatus,      /*7 MATERIALSTATE*/
                    materialValue,		 /*8 MATERIALWEIGHT*/ //add by Yang 2016/8/22,array MES识别WEIGHT作为物料用量
                    materialType,        /*9 MATERIALTYPE*/ 
                    "",                  /*10 USEDCOUNT*/
                    "",                  /*11 LIFEQTIME*/
                    "",                  /*12 GROUPID*/
                    unitId,              /*13 UNITID*/
                    slotNo ,             /*14 HEADID*/ 
                    "MaterialStatusChangeReport"
                };
                        Invoke(eServiceName.MESService, "MaterialStateChanged", _data);
                        #endregion

                        #region Materila Entity Collection
                        //To Do
                        MaterialEntity materialE = new MaterialEntity();
                        materialE.EQType = eMaterialEQtype.Normal;
                        materialE.NodeNo = eqp.Data.NODENO;
                        materialE.OperatorID = operatorID;
                        materialE.MaterialID = materialID;
                        materialE.UnitNo = unitNo;
                        materialE.MaterialStatus = materialStatus;
                        materialE.MaterialValue = materialValue;
                        materialE.MaterialType = materialType;

                        ObjectManager.MaterialManager.AddMaterial(materialE);
                        #endregion

                        string timerID = string.Format("MaterialStatusChangeReport_{0}_{1}_{2}_MaterialStateChanged", eqp.Data.NODENO, unitNo, inputData.TrackKey);
                        if (this.Timermanager.IsAliveTimer(timerID))
                        {
                              Timermanager.TerminateTimer(timerID);
                        }
                        Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStatusChangeMESTimeout), eqpNo);
                  }
                  catch (Exception ex)
                  {
                        this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void MaskStateChanged_CF(Trx inputData)
            {
                  try
                  {
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        string unitNo = inputData[0][0][0].Value;
                        string unitId = "";
                        #region [Check Unit]
                        if (!unitNo.Equals("0"))
                        {
                              Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                              if (unit != null)
                              {
                                    unitId = unit.Data.UNITID;
                              }
                        }
                        #endregion//呼叫MES方法
                        string operatorID = inputData[0][0][1].Value;
                        eMaterialStatus materialStatus = (eMaterialStatus)int.Parse(inputData[0][0][2].Value);
                        string materialValue = inputData[0][0][3].Value;
                        string slotNo = inputData[0][0][4].Value;
                        string materialID = inputData[0][0][5].Value.Trim();

                        List<MaskStateChanged.MASKc> maskList = new List<MaskStateChanged.MASKc>();
                        MaskStateChanged.MASKc mask = new MaskStateChanged.MASKc();
                        mask.MASKPOSITION = slotNo;
                        mask.MASKNAME = materialID;
                        mask.MASKSTATE = materialStatus.ToString();
                        mask.UNITNAME = unitId;
                        mask.MASKUSECOUNT = materialValue;
                        maskList.Add(mask);

                        eBitResult triggerBit = (eBitResult)int.Parse(inputData[0][1][0].Value);
                        if (triggerBit == eBitResult.OFF)
                        {
                              MaterialEntity material = new MaterialEntity();
                              LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              MaterialStatusChangeReportReply(eqpNo, unitNo, "", material, eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey, true);
                              return;
                        }

                        LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO=[{3}], OPERATOR ID =[{4}], MATERIAL STATUS =[{5}], MATERIAL VALUE =[{6}], SLOT NO =[{7}], MATERIAL =[{8}], BIT (ON)",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, operatorID, materialStatus, materialValue, slotNo, materialID));

                        #region Offline Skip to send MES
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={1}] [BCS -> MES][{0}] Mask State Changed Send MES but OFFLINE LINENAME=[{1}].",
                                  inputData.TrackKey, eqp.Data.LINEID));
                              return;
                        }
                        #endregion

                        #region Materila Entity Collection
                        //To Do
                        MaterialEntity materialE = new MaterialEntity();
                        materialE.EQType = eMaterialEQtype.MaskEQ;
                        materialE.NodeNo = eqp.Data.NODENO;
                        materialE.OperatorID = operatorID;
                        materialE.MaterialID = materialID;
                        materialE.MaterialSlotNo = slotNo;
                        materialE.UnitNo = unitNo;
                        materialE.MaterialStatus = materialStatus;
                        materialE.MaterialPosition = slotNo;
                        materialE.MaterialValue = materialValue;

                        ObjectManager.MaterialManager.AddMaterial(materialE);
                        #endregion

                        object[] _data = new object[7]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,     /*1 LineName*/
                    eqp.Data.NODEID,     /*2 EQPID*/
                    "",                  /*3 LINERECIPENAME*/
                    operatorID,          /*4 eventUse*/ 
                    maskList,             /*5 maskList*/
                    "MaterialStatusChangeReport"
                };
                        Repository.Add("L9_MaskStateChanged", _data);

                        Invoke(eServiceName.MESService, "MaskStateChanged", _data);
                        ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, materialE, string.Empty, inputData.TrackKey);

                        //string timerID = string.Format("MaterialStatusChangeReport_{0}_{1}_MaterialStateChanged_{2}", eqp.Data.NODENO, inputData.TrackKey, unitNo);
                        // Modified by zhangwei 20161116
                        string timerID = string.Format("MaterialStatusChangeReport_{0}_{1}_{2}_MaskStateChanged", eqp.Data.NODENO, unitNo, inputData.TrackKey);  
                        
                        if (this.Timermanager.IsAliveTimer(timerID))
                        {
                              Timermanager.TerminateTimer(timerID);
                        }
                        Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStatusChangeMESTimeout), eqpNo);
                  }
                  catch (Exception ex)
                  {
                        this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            //Modify By Hujunpeng20180904
            public void MaterialStatusChangeReport_CF(Trx inputData) 
            {
                try
                {
                    string headID = string.Empty;
                    string eqpNo = inputData.Metadata.NodeNo;
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    //Equipment eqp2 = ObjectManager.EquipmentManager.GetEQP("L4");
                    if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));
                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                    string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                    string unitID = string.Empty;
                    string operatorID = inputData.EventGroups[0].Events[0].Items[1].Value;
                    eMaterialStatus materialStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value);
                    materialStatus1 = materialStatus;//add by hujunpeng 20180425
                    string materialValue = inputData.EventGroups[0].Events[0].Items[3].Value;
                    string materialValue1 = int.Parse(materialValue) > 20000 ? "20000" : materialValue;
                    switch (materialValue1.Length)
                    {
                        case 4:
                            materialValue1 = "0" + materialValue1;
                            break;

                        case 3:
                            materialValue1 = "00" + materialValue1;
                            break;

                        case 2:
                            materialValue1 = "000" + materialValue1;
                            break;

                        case 1:
                            materialValue1 = "0000" + materialValue1;
                            break;

                        default:
                            break;

                    }
                    string slotNo = inputData.EventGroups[0].Events[0].Items[4].Value;
                    materialslotno1 = slotNo;
                    if (slotNo == "1" || slotNo == "2")
                        headID = "ASIDE";
                    else
                        headID = "BSIDE";
                    string materialID = inputData.EventGroups[0].Events[0].Items[5].Value.Trim();
                    materialID1 = materialID;//add by hujunpeng 20180425
                    switch (slotNo)
                    {
                        case "1":
                        case "3":
                            lock (eqp.File)
                            {
                                //if (materialStatus.ToString() == "INUSE" || materialStatus.ToString() == "MOUNT")

                                //eqp.File.MaterialStatus1 = materialStatus;
                                //eqp.File.MaterialValue1 = materialValue;
                                //eqp.File.MaterialName1 = materialID;
                                //eqp.File.Slot1 = slotNo;
                                //eqp.Data.NODENAME = (materialID + (materialStatus.ToString()).Substring(0, 1) + materialValue1).ToUpper();
                                //ObjectManager.EquipmentManager.UpdateDB(eqp.Data);

                                eqp.File.PrID1 = (materialID + (materialStatus.ToString()).Substring(0, 1) + materialValue1).ToUpper();
                                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                List<MaterialEntity> materialList1 = new List<MaterialEntity>();
                                MaterialEntity materialE1 = new MaterialEntity();
                                materialE1.MaterialWeight = materialValue1;
                                materialE1.MaterialID = materialID;
                                materialE1.MaterialStatus = materialStatus;
                                materialE1.MaterialSlotNo = "1";
                                materialList1.Add(materialE1);
                                ObjectManager.MaterialManager.AddMaterial(materialE1);
                                Invoke(eServiceName.UIService, "MaterialRealWeightReport", new object[] { inputData.TrackKey, materialList1 });
                            }
                            break;

                        case "2":
                        case "4":
                            lock (eqp.File)
                            {
                                //if (materialStatus.ToString() == "INUSE" || materialStatus.ToString() == "MOUNT")

                                //eqp.File.MaterialStatus2 = materialStatus;
                                //eqp.File.MaterialValue2 = materialValue;
                                //eqp.File.MaterialName2 = materialID;
                                //eqp.File.Slot2 = slotNo;
                                //ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                //eqp2.Data.NODENAME = (materialID + (materialStatus.ToString()).Substring(0, 1) + materialValue1).ToUpper();
                                //ObjectManager.EquipmentManager.UpdateDB(eqp2.Data);

                                eqp.File.PrID2 = (materialID + (materialStatus.ToString()).Substring(0, 1) + materialValue1).ToUpper();
                                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                List<MaterialEntity> materialList1 = new List<MaterialEntity>();
                                MaterialEntity materialE1 = new MaterialEntity();
                                materialE1.MaterialWeight = materialValue1;
                                materialE1.MaterialID = materialID;
                                materialE1.MaterialStatus = materialStatus;
                                materialE1.MaterialSlotNo = "2";
                                materialList1.Add(materialE1);
                                ObjectManager.MaterialManager.AddMaterial(materialE1);
                                Invoke(eServiceName.UIService, "MaterialRealWeightReport", new object[] { inputData.TrackKey, materialList1 });
                            }
                            break;
                        //case "3":
                        //    lock (eqp.File)
                        //    {
                        //        //if (materialStatus.ToString() == "INUSE" || materialStatus.ToString() == "MOUNT")

                        //        //eqp.File.MaterialStatus1 = materialStatus;
                        //        //eqp.File.MaterialValue1 = materialValue;
                        //        //eqp.File.MaterialName1 = materialID;
                        //        //eqp.File.Slot1 = slotNo;
                        //        //eqp.Data.NODENAME = (materialID + (materialStatus.ToString()).Substring(0, 1) + materialValue1).ToUpper();
                        //        //ObjectManager.EquipmentManager.UpdateDB(eqp.Data);

                        //        eqp.File.PrID3 = (materialID + (materialStatus.ToString()).Substring(0, 1) + materialValue1).ToUpper();
                        //        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        //        List<MaterialEntity> materialList1 = new List<MaterialEntity>();
                        //        MaterialEntity materialE1 = new MaterialEntity();
                        //        materialE1.MaterialWeight = materialValue1;
                        //        materialE1.MaterialID = materialID;
                        //        materialE1.MaterialStatus = materialStatus;
                        //        materialE1.MaterialSlotNo = "3";
                        //        materialList1.Add(materialE1);
                        //        ObjectManager.MaterialManager.AddMaterial(materialE1);
                        //        Invoke(eServiceName.UIService, "MaterialRealWeightReport", new object[] { inputData.TrackKey, materialList1 });
                        //    }
                        //    break;
                        //case "4":
                        //    lock (eqp.File)
                        //    {
                        //        //if (materialStatus.ToString() == "INUSE" || materialStatus.ToString() == "MOUNT")

                        //        //eqp.File.MaterialStatus1 = materialStatus;
                        //        //eqp.File.MaterialValue1 = materialValue;
                        //        //eqp.File.MaterialName1 = materialID;
                        //        //eqp.File.Slot1 = slotNo;
                        //        //eqp.Data.NODENAME = (materialID + (materialStatus.ToString()).Substring(0, 1) + materialValue1).ToUpper();
                        //        //ObjectManager.EquipmentManager.UpdateDB(eqp.Data);

                        //        eqp.File.PrID4 = (materialID + (materialStatus.ToString()).Substring(0, 1) + materialValue1).ToUpper();
                        //        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        //        List<MaterialEntity> materialList1 = new List<MaterialEntity>();
                        //        MaterialEntity materialE1 = new MaterialEntity();
                        //        materialE1.MaterialWeight = materialValue1;
                        //        materialE1.MaterialID = materialID;
                        //        materialE1.MaterialStatus = materialStatus;
                        //        materialE1.MaterialSlotNo = "4";
                        //        materialList1.Add(materialE1);
                        //        ObjectManager.MaterialManager.AddMaterial(materialE1);
                        //        Invoke(eServiceName.UIService, "MaterialRealWeightReport", new object[] { inputData.TrackKey, materialList1 });
                        //    }
                        //    break;
                        default:
                            break;
                    }
                    //{
                    //    List<MaterialEntity> materialList1= new List<MaterialEntity>();
                    //    MaterialEntity materialE1= new MaterialEntity();
                    //    materialE1.MaterialWeight = string.Empty;
                    //    materialE1.MaterialID = string.Empty;
                    //    materialE1.MaterialStatus = eMaterialStatus.NONE;
                    //    materialE1.MaterialSlotNo = "0";
                    //    materialList1.Add(materialE1);
                    //    ObjectManager.MaterialManager.AddMaterial(materialE1);
                    //    Invoke(eServiceName.UIService, "MaterialRealWeightReport", new object[] { inputData.TrackKey, materialList1});
                    //}
                    //20180523 by hujunpeng
                    eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                    MaterialEntity material = new MaterialEntity();
                    if (triggerBit == eBitResult.OFF)
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                        MaterialStatusChangeReportReply(eqpNo, unitNo, "", material, eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey, true);
                        return;
                    }

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], OPERATOR ID =[{4}], MATERIAL STATUS =[{5}], MATERIAL VALUE =[{6}], SLOT NO =[{7}], MATERIAL ID =[{8}], BIT (ON)",
                        eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, operatorID, materialStatus, materialValue, slotNo, materialID));

                    #region Offline Skip to send MES
                    if (line.File.HostMode == eHostMode.OFFLINE)
                    {
                        LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] Mask State Changed Send MES but OFFLINE LINENAME=[{1}].",
                            inputData.TrackKey, eqp.Data.LINEID));
                        //Offline下，always reply "NG" 2015/9/8 add by Frank
                        MaterialStatusChangeReportReply(eqpNo, unitNo, "", material, eBitResult.ON, eReturnCode1.NG, inputData.TrackKey, false);
                        return;
                    }
                    #endregion

                    #region Materila Entity Collection
                    List<MaterialEntity> materialList = new List<MaterialEntity>();
                    MaterialEntity materialE = new MaterialEntity();
                    materialE.EQType = eMaterialEQtype.Normal;
                    materialE.NodeNo = eqp.Data.NODENO;
                    materialE.OperatorID = operatorID;
                    materialE.MaterialID = materialID;
                    materialE.MaterialSlotNo = slotNo;
                    materialE.UnitNo = unitNo;
                    materialE.MaterialStatus = materialStatus;
                    materialE.MaterialPosition = slotNo;
                    materialE.MaterialValue = materialValue;
                    materialE.MaterialType = "PR";  //2015/8/28 add by Frank CF固定填PR
                    materialList.Add(materialE);

                    ObjectManager.MaterialManager.AddMaterial(materialE);
                    #endregion

                    #region [Check Unit]
                    if (!unitNo.Equals("0"))
                    {
                        Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                        if (unit != null)
                        {
                            unitID = unit.Data.UNITID;
                        }
                    }
                    #endregion

                    //取出特定幾台機台內任一片玻璃的 Job ID 上報，若則無須上報。
                    IList<Equipment> EQPList = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).
                        Where(e => e.Data.NODENO == "L2" || e.Data.NODENO == "L3" || e.Data.NODENO == "L4").ToList();
                    string productName = ObjectManager.JobManager.GetJobIDbyEQPList(EQPList);

                    #region Report to MES
                    //呼叫MES方法
                    if (!(materialStatus == eMaterialStatus.MOUNT || materialStatus == eMaterialStatus.DISMOUNT)) //MOUNT和DISMOUNT不需要MaterialStateChanged,20151009 jm.pan
                    {
                        object[] _data = new object[16]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,     /*1 LineName*/
                    eqp.Data.NODEID,     /*2 EQPID*/
                    "",                  /*3 LINERECIPENAME*/
                    "",                  /*4 MATERIALMODE*/ 
                    productName,         /*5 PRODUCTNAME*/
                    materialID.Trim(),   /*6 MATERIALNAME*/
                    materialStatus,      /*7 MATERIALSTATE*/
                    materialValue,		 /*8 MATERIALWEIGHT*/ //20160901 Modified by zhangwei
                    "PR",                /*9 MATERIALTYPE  CF固定填PR*/ 
                    "",                  /*10 USEDCOUNT*/    //后面根据需求再变更
                    "",                  /*11 LIFEQTIME*/
                    "",                  /*12 GROUPID*/
                    unitID,              /*13 UNITID*/
                    headID ,             /*14 HEADID　用來上報Coater 光阻桶的側別 Slot1~2:ASIDE, Slot3~4:BSIDE*/ 
                    "MaterialStatusChangeReport"
                };
                        Invoke(eServiceName.MESService, "MaterialStateChanged", _data);
                    }
                    #endregion

                    #region Mount/Dismount Report MES
                    //2015/8/28 add by Frank
                    //T3 Mount/Dismount should report MES
                    if (materialStatus == eMaterialStatus.MOUNT)
                    {
                        object[] _data1 = new object[6]
                    { 
                        inputData.TrackKey,  /*0 TrackKey*/
                        eqp,                 /*1 EQP*/
                        productName,         /*2 GlassID*/
                        "",                  /*3 MaterialDurableName*/
                        "",                  /*4 PolType*/
                        materialList         /*5 MaterialList*/
                    };
                        Invoke(eServiceName.MESService, "MaterialMount", _data1);
                    }

                    if (materialStatus == eMaterialStatus.DISMOUNT)
                    {
                        object[] _data2 = new object[5]
                    { 
                        inputData.TrackKey,  /*0 TrackKey*/
                        eqp,                 /*1 EQP*/
                        productName,         /*2 GlassID*/
                        "",                  /*3 MaterialDurableName*/
                        materialList         /*5 MaterialList*/
                    };
                        Invoke(eServiceName.MESService, "MaterialDismountReport", _data2);
                    }
                    #endregion

                    ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, materialE, string.Empty, inputData.TrackKey);
                    //string timerID = string.Format("MaterialStatusChangeReport_{0}_{1}_MaterialStateChanged_{2}", eqp.Data.NODENO, inputData.TrackKey, unitNo);
                    // Modified by zhangwei 20161104
                    string timerID = string.Format("MaterialStatusChangeReport_{0}_{1}_{2}_MaterialStateChanged", eqp.Data.NODENO, unitNo, inputData.TrackKey);
                    if (this.Timermanager.IsAliveTimer(timerID))
                    {
                        Timermanager.TerminateTimer(timerID);
                    }

                    if (materialStatus == eMaterialStatus.DISMOUNT)
                    {
                        MaterialStatusChangeReportReply(eqpNo, unitNo, "", material, eBitResult.ON, eReturnCode1.OK, inputData.TrackKey, false);
                        //MaterialStatusChangeReportReply(eqpNo, unitNo, "", material, eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey, true);
                        return;
                    }
                    else
                    {
                        Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStatusChangeMESTimeout), unitNo);
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            } 

            #region[T2 USE (CELL)]
            public void MaterialStatusChangeReport_CELL_PIP(Trx inputData)
            {
                  try
                  {
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string recipeID = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string materialID = inputData.EventGroups[0].Events[0].Items[2].Value;
                        eMaterialStatus materialStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value);
                        string tankNo = inputData.EventGroups[0].Events[0].Items[4].Value;
                        string numerator = inputData.EventGroups[0].Events[0].Items[5].Value;
                        string denominator = inputData.EventGroups[0].Events[0].Items[6].Value;
                        string operatorID = inputData.EventGroups[0].Events[0].Items[7].Value;
                        string useCount = GetUseCount(numerator, denominator);

                        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        MaterialEntity material = new MaterialEntity();
                        if (triggerBit == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              MaterialStatusChangeReportReply(eqpNo, unitNo, "", material, eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey, true);
                              return;
                        }

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], RECIPE ID =[{4}], MATERIAL ID =[{5}], MATERIAL STATUS =[{6}], TANK NO =[{7}], NUMERATOR =[{8}], DENOMINATOR =[{9}], OPERATOR ID =[{10}], BIT (ON)",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, recipeID, materialID, materialStatus, tankNo, numerator, denominator, operatorID));

                        #region Materila Entity Collection
                        //To Do
                        MaterialEntity materialE = new MaterialEntity();
                        materialE.NodeNo = eqp.Data.NODENO;
                        materialE.OperatorID = operatorID;
                        materialE.MaterialID = materialID;
                        //materialE.MaterialSlotNo = slotNo;
                        materialE.UnitNo = unitNo;
                        materialE.MaterialStatus = materialStatus;
                        materialE.MaterialPosition = tankNo;
                        materialE.MaterialRecipeID = recipeID;
                        materialE.MaterialType = "PI";  //Jun Modify 20141202 PIP固定填PI
                        ObjectManager.MaterialManager.AddMaterial(materialE);

                        ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, materialE, string.Empty, inputData.TrackKey, "", useCount);
                        #endregion


                        object[] _data = new object[15]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 EQPID*/
                    recipeID,          /*3 LINERECIPENAME*/
                    "",            /*4 MATERIALMODE*/ 
                    "",           /*5 PRODUCTNAME*/
                    materialID,           /*6 MATERIALNAME*/
                    materialStatus,           /*7 MATERIALSTATE*/
                    "PI"            ,/*8 MATERIALTYPE*/  //Jun Modify 20141202 PIP固定填PI_LNK
                    useCount            , /*9 USEDCOUNT*/
                    ""            ,/*10 LIFEQTIME*/
                    ""            ,/*11 GROUPID*/
                    ""            ,/*12 UNITID*/
                    tankNo,            /*13 HEADID*/
                    "MaterialStatusChangeReport"
                };
                        #region [Check Unit]
                        if (!unitNo.Equals("0"))
                        {
                              Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                              if (unit != null)
                              {
                                    _data[12] = unit.Data.UNITID;
                              }
                        }
                        #endregion
                        //呼叫MES方法
                        Invoke(eServiceName.MESService, "MaterialStateChanged", _data);
                        string timerID = string.Format("MaterialStatusChangeReport_{0}_{1}_{2}_MaterialStateChanged", eqp.Data.NODENO, unitNo, inputData.TrackKey);
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              return;
                        }
                        if (this.Timermanager.IsAliveTimer(timerID))
                        {
                              Timermanager.TerminateTimer(timerID);
                        }
                        Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStatusChangeMESTimeout), eqpNo);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            //T2
            public void MaterialStatusChangeReport_CELL_CUT(Trx inputData)
            {
                  try
                  {
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        string headNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        eMaterialStatus newCutWheelStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[1].Value);
                        string newCutWheelSettingUse = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string newCutWheelID = inputData.EventGroups[0].Events[0].Items[3].Value;
                        eMaterialStatus oldCutWheelStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value);
                        string oldCutWheelSettingUse = inputData.EventGroups[0].Events[0].Items[5].Value;
                        string oldCutWheelID = inputData.EventGroups[0].Events[0].Items[6].Value;
                        string defectCode = inputData.EventGroups[0].Events[0].Items[7].Value;
                        string recipeID = inputData.EventGroups[0].Events[0].Items[8].Value;
                        string operatorID = inputData.EventGroups[0].Events[0].Items[9].Value;
                        List<MaskStateChanged.MASKc> maskList = new List<MaskStateChanged.MASKc>();
                        if (oldCutWheelID.Trim() != "")  //Jun Add 20141204 如果沒有上報就不要加到List裡
                        {
                              MaskStateChanged.MASKc maskOld = new MaskStateChanged.MASKc();
                              maskOld.MASKPOSITION = headNo;
                              maskOld.MASKNAME = oldCutWheelID;  //"OLD";  //Jun Modify 20141204 上報正確資料 不是HeadCode的資料
                              maskOld.MASKSTATE = oldCutWheelStatus.ToString(); ;  //"DISMOUNT";  //Jun Modify 20141204 上報正確資料 不是HeadCode的資料
                              maskOld.MASKUSECOUNT = oldCutWheelSettingUse;
                              maskOld.REASONCODE = defectCode;
                              maskList.Add(maskOld);
                        }
                        if (newCutWheelID.Trim() != "")  //Jun Add 20141204 如果沒有上報就不要加到List裡
                        {
                              MaskStateChanged.MASKc maskNew = new MaskStateChanged.MASKc();
                              maskNew.MASKPOSITION = headNo;
                              maskNew.MASKNAME = newCutWheelID;  //"NEW";  //Jun Modify 20141204 上報正確資料 不是HeadCode的資料
                              maskNew.MASKSTATE = newCutWheelStatus.ToString();//"MOUNT";  //Jun Modify 20141204 上報正確資料 不是HeadCode的資料
                              maskNew.MASKUSECOUNT = newCutWheelSettingUse;
                              maskList.Add(maskNew);
                        }

                        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        MaterialEntity material = new MaterialEntity();
                        if (triggerBit == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              MaterialStatusChangeReportReply(eqpNo, "0", headNo, material, eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey, true);
                              return;
                        }

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], HEAD NO =[{3}], NEW CUTTING WHEEL STATUS =[{4}], NEW CUTTING WHEEL SETTING USAGE =[{5}], NEW CUTTING WHEEL ID =[{6}], OLD CUTTING WHEEL STATUS =[{7}], OLD CUTTING WHEEL USAGE =[{8}], OLD CUTTING WHEEL ID =[{9}], DEFECT CODE =[{10}], RECIPE ID =[{11}], OPERATOR ID =[{12}], BIT (ON)",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, headNo, newCutWheelStatus, newCutWheelSettingUse, newCutWheelID, oldCutWheelStatus, oldCutWheelSettingUse, oldCutWheelID, defectCode, recipeID, operatorID));

                        #region Materila Entity Collection
                        //To Do               
                        if (newCutWheelID.Trim() != "")  //Jun Add 20141223 將資料寫入Material Manager
                        {
                              MaterialEntity materialNew = new MaterialEntity();
                              materialNew.NodeNo = eqp.Data.NODENO;
                              materialNew.OperatorID = operatorID;
                              materialNew.MaterialID = newCutWheelID;
                              //materialNew.MaterialSlotNo = slotNo;
                              //materialNew.UnitNo = unitNo;
                              materialNew.MaterialStatus = newCutWheelStatus;
                              materialNew.MaterialPosition = headNo;
                              materialNew.MaterialRecipeID = recipeID;
                              materialNew.EQType = eMaterialEQtype.MaskEQ;
                              materialNew.MaterialType = "Mask";
                              ObjectManager.MaterialManager.AddMaterial(materialNew);

                              ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, "0", materialNew, string.Empty, inputData.TrackKey, "", newCutWheelSettingUse);
                        }
                        if (oldCutWheelID.Trim() != "")  //Jun Add 20141223 將資料寫入Material Manager
                        {
                              MaterialEntity materialOld = new MaterialEntity();
                              materialOld.NodeNo = eqp.Data.NODENO;
                              materialOld.OperatorID = operatorID;
                              materialOld.MaterialID = oldCutWheelID;
                              //materialNew.MaterialSlotNo = slotNo;
                              //materialNew.UnitNo = unitNo;
                              materialOld.MaterialStatus = oldCutWheelStatus;
                              materialOld.MaterialPosition = headNo;
                              materialOld.MaterialRecipeID = recipeID;
                              materialOld.EQType = eMaterialEQtype.MaskEQ;
                              materialOld.MaterialType = "Mask";
                              ObjectManager.MaterialManager.AddMaterial(materialOld);

                              ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, "0", materialOld, string.Empty, inputData.TrackKey, "", oldCutWheelSettingUse);
                        }

                        #endregion

                        object[] _data = new object[7]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 EQPID*/
                    "",          /*3 LINERECIPENAME*/
                    operatorID,            /*4 eventUse*/ 
                    maskList,          /*5 maskList*/
                    "MaterialStatusChangeReport"
                };

                        Invoke(eServiceName.MESService, "MaskStateChanged", _data);
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              return;
                        }
                        string timerID = string.Format("MaterialStatusChangeReport_{0}_{1}_{2}_MaskStateChanged", eqp.Data.NODENO, "0", inputData.TrackKey);
                        if (this.Timermanager.IsAliveTimer(timerID))
                        {
                              Timermanager.TerminateTimer(timerID);
                        }
                        Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStatusChangeMESTimeout), eqpNo);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            //T2
            public void MaterialStatusChangeReport_CELL_PAM_T2(Trx inputData)
            {
                  try
                  {
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));

                        string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string recipeID = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string materialID = inputData.EventGroups[0].Events[0].Items[2].Value;
                        eMaterialStatus materialStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value);
                        string position = inputData.EventGroups[0].Events[0].Items[4].Value;
                        string polType = inputData.EventGroups[0].Events[0].Items[5].Value;
                        string polMaterialType = inputData.EventGroups[0].Events[0].Items[18].Value;
                        string polCount = "0";
                        List<POLStateChanged.MATERIALc> materialList = new List<POLStateChanged.MATERIALc>();
                        if (materialStatus == eMaterialStatus.DISMOUNT)
                        {
                              int divisor = 1; int i = 0;
                              if (polMaterialType == "1") divisor = 10;
                              POLStateChanged.MATERIALc data = new POLStateChanged.MATERIALc();
                              data.POSITION = "1";
                              data.MATERIALNAME = inputData.EventGroups[0].Events[0].Items[6].Value;
                              data.COUNT = int.TryParse(inputData.EventGroups[0].Events[0].Items[7].Value, out i) == true ? (i / divisor).ToString() : inputData.EventGroups[0].Events[0].Items[7].Value;  //inputData.EventGroups[0].Events[0].Items[7].Value;
                              polCount = data.COUNT + ",";
                              materialList.Add(data);
                              data = new POLStateChanged.MATERIALc();
                              data.POSITION = "2";
                              data.MATERIALNAME = inputData.EventGroups[0].Events[0].Items[8].Value;
                              data.COUNT = int.TryParse(inputData.EventGroups[0].Events[0].Items[9].Value, out i) == true ? (i / divisor).ToString() : inputData.EventGroups[0].Events[0].Items[9].Value;  //inputData.EventGroups[0].Events[0].Items[9].Value;
                              polCount = data.COUNT + ",";
                              materialList.Add(data);
                              data = new POLStateChanged.MATERIALc();
                              data.POSITION = "3";
                              data.MATERIALNAME = inputData.EventGroups[0].Events[0].Items[10].Value;
                              data.COUNT = int.TryParse(inputData.EventGroups[0].Events[0].Items[11].Value, out i) == true ? (i / divisor).ToString() : inputData.EventGroups[0].Events[0].Items[11].Value;  //inputData.EventGroups[0].Events[0].Items[11].Value;
                              polCount = data.COUNT + ",";
                              materialList.Add(data);
                              data = new POLStateChanged.MATERIALc();
                              data.POSITION = "4";
                              data.MATERIALNAME = inputData.EventGroups[0].Events[0].Items[12].Value;
                              data.COUNT = int.TryParse(inputData.EventGroups[0].Events[0].Items[13].Value, out i) == true ? (i / divisor).ToString() : inputData.EventGroups[0].Events[0].Items[13].Value;  //inputData.EventGroups[0].Events[0].Items[13].Value;
                              polCount = data.COUNT + ",";
                              materialList.Add(data);
                              data = new POLStateChanged.MATERIALc();
                              data.POSITION = "5";
                              data.MATERIALNAME = inputData.EventGroups[0].Events[0].Items[14].Value;
                              data.COUNT = int.TryParse(inputData.EventGroups[0].Events[0].Items[15].Value, out i) == true ? (i / divisor).ToString() : inputData.EventGroups[0].Events[0].Items[15].Value;  //inputData.EventGroups[0].Events[0].Items[15].Value;
                              polCount = data.COUNT;
                              materialList.Add(data);
                        }
                        string partNo = inputData.EventGroups[0].Events[0].Items[16].Value;
                        string operatorID = inputData.EventGroups[0].Events[0].Items[17].Value;

                        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        MaterialEntity material = new MaterialEntity();
                        if (triggerBit == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              PolStatusChangeReportReply(eqpNo, "", "", "", "", materialList, eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey, true);
                              return;
                        }

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], RECIPE ID =[{4}], MATERIAL ID =[{5}], MATERIAL STATUS =[{6}], POSITION =[{7}], POL TYPE =[{8}],  PART NO =[{9}], OPERATOR ID =[{10}], BIT (ON)",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, recipeID, materialID, materialStatus, position, polType, partNo, operatorID));

                        #region Materila Entity Collection
                        MaterialEntity materialE = new MaterialEntity();
                        materialE.NodeNo = eqp.Data.NODENO;
                        materialE.OperatorID = operatorID;
                        materialE.MaterialID = materialID;
                        //materialE.MaterialSlotNo = slotNo;
                        materialE.UnitNo = unitNo;
                        materialE.MaterialStatus = materialStatus;
                        materialE.MaterialPosition = position;
                        materialE.MaterialRecipeID = recipeID;
                        materialE.PartNo = partNo;
                        //2014/01/20 Asir Modify MATERIALTYPE=[T|F] where ‘T’ is POL for Array, ‘F’ is POL for CF
                        //materialE.MaterialType = polType.Trim() == "T" ? "TFT" : "CF";
                        materialE.MaterialType = polType.Trim();
                        materialE.PolMaterialType = polMaterialType;
                        foreach (POLStateChanged.MATERIALc materialC in materialList)
                        {
                              materialE.CellPOLMaterial.Add(materialC.POSITION + "," + materialC.MATERIALNAME + "," + materialC.COUNT);
                        }
                        ObjectManager.MaterialManager.AddMaterial(materialE);


                        //Jun Add 20150428 Only For POL Material Save to DB
                        MaterialEntity materialPol = new MaterialEntity();
                        materialPol.NodeNo = eqp.Data.NODENO;
                        materialPol.OperatorID = operatorID;
                        materialPol.MaterialID = materialID;
                        materialPol.UnitNo = unitNo;
                        materialPol.MaterialStatus = materialStatus;
                        materialPol.MaterialPosition = position;
                        materialPol.MaterialRecipeID = recipeID;
                        materialPol.PartNo = partNo;
                        materialPol.MaterialType = polMaterialType == "0" ? "PCS-" + polType.Trim() : "ROL-" + polType.Trim();
                        ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, materialPol, string.Empty, inputData.TrackKey, "", polCount);
                        #endregion

                        object[] _data = new object[10]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 EQPID*/
                    recipeID,          /*3 LINERECIPENAME*/
                    materialStatus,
                    //2014/01/20 Asir Modify MATERIALTYPE=[T|F] where ‘T’ is POL for Array, ‘F’ is POL for CF
                    // polType.Trim() == "T" ? "TFT" : "CF",            /*4 MATERIALTYPE*/ 
                    //polType.Trim(),            /*4 MATERIALTYPE*/ 
                    //Watson modify 20150305 For MES Spec 張偉
                    polType.Trim() == "T" ? "T" : "F",
                    polMaterialType,  //Jun Add 20150330 For MES New Spec
                    materialID,           /*5 CARTNAME*/
                    partNo,           /*6 PARTNO*/
                    materialList,           /*7 List<POLStateChanged.MATERIALc> */
                };
                        #region [Check Unit]
                        //if (!unitNo.Equals("0"))
                        //{
                        //    Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                        //    if (unit != null)
                        //    {
                        //        _data[12] = unit.Data.UNITID;
                        //    }
                        //}
                        #endregion

                        #region Add Reply Key
                        //MES Reply no Unit No (PLC Write Key),BC Add Repository 自行處理加入倉庫
                        string key = keyBoxReplyPLCKey.POLStateChangedReply;
                        string rep = unitNo;
                        if (Repository.ContainsKey(key))
                              Repository.Remove(key);
                        Repository.Add(key, rep);
                        #endregion

                        //呼叫MES方法
                        Invoke(eServiceName.MESService, "POLStateChanged", _data);
                        string timerID = string.Format("{0}_{1}_{2}_POLStateChanged", eqp.Data.NODENO, unitNo, inputData.TrackKey);
                        if (this.Timermanager.IsAliveTimer(timerID))
                        {
                              Timermanager.TerminateTimer(timerID);
                        }
                        Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(POLStateChangedMESTimeout), eqpNo);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            //T2
            public void MaterialStatusChangeReport_CELL_PUM(Trx inputData)
            {
                  try
                  {
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);


                        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);


                        if (triggerBit == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              MaterialStatusChangeReportReply_PUM(eqpNo, string.Empty, string.Empty, new List<MaterialEntity>(), eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey);
                              return;
                        }

                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));
                        MaterialEntity material = new MaterialEntity();
                        string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string recipeID = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string materialID = inputData.EventGroups[0].Events[0].Items[2].Value;
                        eMaterialStatus materialStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value);
                        string operatorID = inputData.EventGroups[0].Events[0].Items[16].Value;

                        List<MaterialEntity> materialList = new List<MaterialEntity>();
                        List<MaskStateChanged.MASKc> maskList = new List<MaskStateChanged.MASKc>();

                        for (int i = 1; i <= 6; i++)
                        {
                              MaterialEntity data = new MaterialEntity();
                              data.MaterialSlotNo = i.ToString();
                              data.MaterialPosition = i.ToString();
                              //Asir 2015/01/23 (1)Modify Items取值不对,MaterialID不是maskID
                              data.MaterialID = inputData.EventGroups[0].Events[0].Items[i * 2 + 2].Value;
                              data.UseCount = inputData.EventGroups[0].Events[0].Items[i * 2 + 3].Value;
                              data.MaterialValue = inputData.EventGroups[0].Events[0].Items[i * 2 + 3].Value;

                              data.MaterialStatus = materialStatus;
                              data.EQType = eMaterialEQtype.Normal;
                              data.NodeNo = eqp.Data.NODENO;
                              data.OperatorID = operatorID;
                              data.UnitNo = unitNo;
                              //Asir 2015/01/23 陈忠说如果MaskSlot是空的就不用上报了！
                              if (data.MaterialID == null || data.MaterialID.Trim().Equals("")) continue;
                              materialList.Add(data);

                              ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, data, string.Empty, inputData.TrackKey, "", data.UseCount);

                              MaskStateChanged.MASKc mask = new MaskStateChanged.MASKc();
                              mask.MASKPOSITION = i.ToString();
                              mask.MASKNAME = data.MaterialID;
                              mask.MASKSTATE = materialStatus.ToString();
                              mask.UNITNAME = unitNo;
                              mask.MASKUSECOUNT = data.UseCount;
                              maskList.Add(mask);
                        }

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], RECIPE ID =[{4}], MATERIAL ID =[{5}], MATERIAL STATUS =[{6}],OPERATOR ID =[{7}], BIT (ON)",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, recipeID, materialID, materialStatus, operatorID));


                        switch (materialStatus)
                        {
                              case eMaterialStatus.MOUNT:
                                    #region MES Report string trxID, string lineName, string maskid

                                    #region Add Reply Key
                                    //MES Reply no Mechine Name (PLC Write Key),BC Add Repository 自行處理加入倉庫
                                    string key = keyBoxReplyPLCKey.ValidateMaskByCarrierReply;
                                    string rep = eqp.Data.NODENO;
                                    if (Repository.ContainsKey(key))
                                          Repository.Remove(key);
                                    Repository.Add(key, rep);
                                    #endregion

                                    object[] _data = new object[4]
                        { 
                            inputData.TrackKey,  /*0 TrackKey*/
                            eqp.Data.LINEID,    /*1 LineName*/
                            string.Empty,
                            materialID    /*2 maskid*/
                        };

                                    //呼叫MES方法
                                    Invoke(eServiceName.MESService, "ValidateMaskByCarrierRequest_HVA2", _data);
                                    #endregion
                                    break;
                              case eMaterialStatus.PREPARE:
                                    #region
                                    Invoke(eServiceName.MESService, "MaskStateChanged", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, recipeID, eMaterialStatus.PREPARE.ToString(), maskList });
                                    string timeID = string.Format("{0}_{1}_MaskStateChanged", eqp.Data.NODENO, inputData.TrackKey);
                                    if (this.Timermanager.IsAliveTimer(timeID))
                                          Timermanager.TerminateTimer(timeID);
                                    Timermanager.CreateTimer(timeID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaskStateChangedReplyTimeout), eqpNo);
                                    #endregion
                                    break;
                              case eMaterialStatus.INUSE:
                                    #region
                                    Invoke(eServiceName.MESService, "MaskStateChanged", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, recipeID, eMaterialStatus.PREPARE.ToString(), maskList });
                                    string timeId = string.Format("{0}_{1}_MaskStateChanged", eqp.Data.NODENO, inputData.TrackKey);
                                    if (this.Timermanager.IsAliveTimer(timeId))
                                          Timermanager.TerminateTimer(timeId);
                                    Timermanager.CreateTimer(timeId, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaskStateChangedReplyTimeout), eqpNo);

                                    #endregion
                                    break;
                              case eMaterialStatus.DISMOUNT:
                                    #region  string trxID, string lineName, string machineName, string machineRecipeName, bool abortflag,IList<MaskStateChanged.MASKc> maskList
                                    Invoke(eServiceName.MESService, "MaskProcessEnd_UVA", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, materialID, materialList });
                                    #endregion
                                    break;
                        }

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        MaterialStatusChangeReportReply_PUM(inputData.Metadata.NodeNo, string.Empty, string.Empty, new List<MaterialEntity>(), eBitResult.ON, eReturnCode1.Unknown, inputData.TrackKey);

                  }
            }
            //T2
            public void MaterialStatusChangeReport_CELL_SDP(Trx inputData)
            {
                  try
                  {
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string recipeID = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string materialID = inputData.EventGroups[0].Events[0].Items[2].Value;
                        eMaterialStatus materialStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value);
                        eMaterialType_SDP materialType = (eMaterialType_SDP)int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value);
                        string headNo = inputData.EventGroups[0].Events[0].Items[5].Value;
                        eCompleteStatus completeStatus = (eCompleteStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[6].Value);
                        string numerator = inputData.EventGroups[0].Events[0].Items[7].Value;
                        string denominator = inputData.EventGroups[0].Events[0].Items[8].Value;
                        string operatorID = inputData.EventGroups[0].Events[0].Items[9].Value;
                        string useCount = GetUseCount(numerator, denominator);

                        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        MaterialEntity material = new MaterialEntity();
                        if (triggerBit == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              MaterialStatusChangeReportReply(eqpNo, unitNo, "", material, eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey, true);
                              return;
                        }

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode =[{2}], UNIT_NO =[{3}], RECIPE ID =[{4}], MATERIAL ID =[{5}], MATERIAL STATUS =[{6}], MATERIAL TYPE =[{7}], HEAD NO =[{8}], COMPLETE STATUS =[{9}], NUMERATOR =[{10}], DENOMINATOR =[{11}], OPERATOR ID =[{12}], BIT (ON)",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, recipeID, materialID, materialStatus, materialType, headNo, completeStatus, numerator, denominator, operatorID));

                        #region Materila Entity Collection
                        //To Do
                        MaterialEntity materialE = new MaterialEntity();
                        materialE.NodeNo = eqp.Data.NODENO;
                        materialE.OperatorID = operatorID;
                        materialE.MaterialID = materialID;
                        //materialE.MaterialSlotNo = slotNo;
                        materialE.UnitNo = unitNo;
                        materialE.MaterialStatus = materialStatus;
                        materialE.MaterialPosition = headNo;
                        materialE.MaterialRecipeID = recipeID;
                        materialE.MaterialType = "SEAL-" + materialType.ToString();  //Jun Modify 20141202 SDP填SEAL-AU, SEAL-GF
                        materialE.MaterialCompleteStatus = completeStatus.ToString();
                        ObjectManager.MaterialManager.AddMaterial(materialE);

                        ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, materialE, string.Empty, inputData.TrackKey, "", useCount);
                        #endregion

                        object[] _data = new object[15]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 EQPID*/
                    recipeID,          /*3 LINERECIPENAME*/
                    "",            /*4 MATERIALMODE*/ 
                    "",           /*5 PRODUCTNAME*/
                    materialID,           /*6 MATERIALNAME*/
                    materialStatus,           /*7 MATERIALSTATE*/
                    "SEAL-" + materialType.ToString()            ,/*8 MATERIALTYPE*/  //Jun Modify 20141202 SDP填SEAL-AU, SEAL-GF
                    useCount            , /*9 USEDCOUNT*/
                    ""            ,/*10 LIFEQTIME*/
                    ""            ,/*11 GROUPID*/
                    ""            ,/*12 UNITID*/
                    headNo  ,          /*13 HEADID*/
                    "MaterialStatusChangeReport"
                };
                        #region [Check Unit]
                        if (!unitNo.Equals("0"))
                        {
                              Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                              if (unit != null)
                              {
                                    _data[12] = unit.Data.UNITID;
                              }
                        }
                        #endregion
                        //呼叫MES方法
                        Invoke(eServiceName.MESService, "MaterialStateChanged", _data);
                        string timerID = string.Format("MaterialStatusChangeReport_{0}_{1}_{2}_MaterialStateChanged", eqp.Data.NODENO, unitNo, inputData.TrackKey);
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              return;
                        }
                        if (this.Timermanager.IsAliveTimer(timerID))
                        {
                              Timermanager.TerminateTimer(timerID);
                        }
                        Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStatusChangeMESTimeout), eqpNo);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            //T2
            public void MaterialStatusChangeReport_CELL_LCD(Trx inputData)
            {
                  try
                  {
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string recipeID = inputData.EventGroups[0].Events[0].Items[1].Value;
                        eMaterialType_LCD materialType = (eMaterialType_LCD)int.Parse(inputData.EventGroups[0].Events[0].Items[2].Value);
                        string materialID = inputData.EventGroups[0].Events[0].Items[3].Value;
                        eMaterialStatus materialStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[4].Value);
                        string groupID = inputData.EventGroups[0].Events[0].Items[5].Value;
                        string headNo = inputData.EventGroups[0].Events[0].Items[6].Value;
                        string numerator = inputData.EventGroups[0].Events[0].Items[7].Value;
                        string denominator = inputData.EventGroups[0].Events[0].Items[8].Value;
                        string operatorID = inputData.EventGroups[0].Events[0].Items[9].Value;
                        string cartridgeID = inputData.EventGroups[0].Events[0].Items[10].Value;
                        string useCount = GetUseCount(numerator, denominator);

                        eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        MaterialEntity material = new MaterialEntity();
                        if (triggerBit == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              MaterialStatusChangeReportReply(eqpNo, unitNo, "", material, eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey, true);
                              return;
                        }

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], RECIPE ID =[{4}], MATERIAL TYPE =[{5}], MATERIAL ID =[{6}], MATERIAL STATUS =[{7}], GROUP ID =[{8}], HEAD NO =[{9}], NUMERATOR =[{10}], DENOMINATOR =[{11}], OPERATOR ID =[{12}], CARTRIDGE ID =[{13}], BIT (ON)",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, recipeID, materialType, materialID, materialStatus, groupID, headNo, numerator, denominator, operatorID, cartridgeID));

                        #region Materila Entity Collection
                        //To Do
                        MaterialEntity materialE = new MaterialEntity();
                        materialE.NodeNo = eqp.Data.NODENO;
                        materialE.OperatorID = operatorID;
                        materialE.MaterialID = materialID;
                        //materialE.MaterialSlotNo = slotNo;
                        materialE.UnitNo = unitNo;
                        materialE.MaterialStatus = materialStatus;
                        materialE.MaterialPosition = headNo;
                        materialE.MaterialRecipeID = recipeID;
                        materialE.GroupId = groupID;
                        materialE.MaterialCartridgeID = cartridgeID;
                        materialE.MaterialType = "LC";  //Jun Modify 20141205 HardCode LC
                        ObjectManager.MaterialManager.AddMaterial(materialE);

                        ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, materialE, string.Empty, inputData.TrackKey, "", useCount);
                        #endregion


                        object[] _data = new object[15]
                { 
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 EQPID*/
                    recipeID,          /*3 LINERECIPENAME*/
                    "",            /*4 MATERIALMODE*/ 
                    "",           /*5 PRODUCTNAME*/
                    materialID,           /*6 MATERIALNAME*/
                    materialStatus,           /*7 MATERIALSTATE*/
                    "LC"            ,/*8 MATERIALTYPE*/  //Jun Modify 20141205 HardCode LC
                    useCount            , /*9 USEDCOUNT*/
                    ""            ,/*10 LIFEQTIME*/
                    groupID            ,/*11 GROUPID*/
                    ""            ,/*12 UNITID*/
                    headNo,            /*13 HEADID*/
                    "MaterialStatusChangeReport"
                };
                        #region [Check Unit]
                        if (!unitNo.Equals("0"))
                        {
                              Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                              if (unit != null)
                              {
                                    _data[12] = unit.Data.UNITID;
                              }
                        }
                        #endregion
                        //呼叫MES方法
                        Invoke(eServiceName.MESService, "MaterialStateChanged", _data);
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              return;
                        }
                        string timerID = string.Format("MaterialStatusChangeReport_{0}_{1}_{2}_MaterialStateChanged", eqp.Data.NODENO, unitNo, inputData.TrackKey);
                        if (this.Timermanager.IsAliveTimer(timerID))
                        {
                              Timermanager.TerminateTimer(timerID);
                        }
                        Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStatusChangeMESTimeout), eqpNo);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion
            //T3
            public void MaterialStatusChangeReport_CELL(Trx inputData)
            {
                  try
                  {
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                        if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                        #endregion
                        #region [PLCAgent Data Bit]
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion
                        #region[If Bit Off->Return]
                        MaterialEntity material = new MaterialEntity();
                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                              MaterialStatusChangeReportReply(inputData.Metadata.NodeNo, "", "", material, eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey, true);
                              return;
                        }
                        #endregion
                        #region [PLCAgent Data Word]
                        string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();//unit No
                        string recipeID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();//recipe ID
                        string materialID = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();//material ID
                        eMaterialStatus materialStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value);//material Status
                        string operatorID = inputData.EventGroups[0].Events[0].Items["OperatorID"].Value.Trim();//operator ID

                        string UVMaskUseCount = string.Empty; string lotID = string.Empty; string lotNo = string.Empty; string count = string.Empty;
                        string feedingPortID = string.Empty; string materialType = string.Empty; string otherLog = string.Empty;
                        switch (line.Data.LINETYPE)//by line setting
                        {
                            case eLineType.CELL.CCODF://sy modify to use [Use Count]
                            case eLineType.CELL.CCODF_2://sy add 20160907
                                count = inputData.EventGroups[0].Events[0].Items["UseCount"].Value;//sy 20151207 dismount use
                                otherLog = string.Format("Use Count = [{0}]", count);

                                if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCSUV)) materialType = "UVMask";//sy modify 20160809 by MES spec 1.47
                                otherLog += string.Format("Material Type = [{0}]", materialType);
                                break;
                            case eLineType.CELL.CCPIL:
                            case eLineType.CELL.CCPIL_2:
                            case eLineType.CELL.CCPCS:
                                count = inputData.EventGroups[0].Events[0].Items["UseCount"].Value;//sy 20151207 dismount use
                                otherLog = string.Format("Use Count = [{0}]", count);
                                break;
                            default:
                                if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))//sy modify  20160705 放在deault
                                {
                                    //materialType = "CutKnife";//BC Mount前不會知道，EQ都不會知道 
                                    count = inputData.EventGroups[0].Events[0].Items["UseCount"].Value;//sy 20151207 dismount use
                                    otherLog = string.Format("Use Count = [{0}]", count);
                                }
                                break;
                        }
                        #endregion
                        #region [Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], RECIPE ID =[{4}], MATERIAL ID =[{5}], MATERIAL STATUS =[{6}], OPERATOR ID =[{7}], BIT (ON) [{8}]",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, recipeID, materialID, materialStatus, operatorID, otherLog));
                        #endregion
                        #region Offline to send MES & Reply EQ OK/NG by Setting
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              eReturnCode1 RtnCode = eReturnCode1.OK;
                              if (!ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
                              {
                                    RtnCode = eReturnCode1.NG;
                              }
                              LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[LINENAME={1}] [BCS -> MES][{0}] Material State Changed Send MES but OFFLINE LINENAME=[{1}].",
                                  inputData.TrackKey, eqp.Data.LINEID));
                              MaterialStatusChangeReportReply(inputData.Metadata.NodeNo, unitNo, "", material, eBitResult.ON, RtnCode, inputData.TrackKey, false);
                              return;
                        }
                        #endregion
                        #region Materila Entity Collection
                        List<MaterialEntity> materialList = new List<MaterialEntity>();
                        MaterialEntity materialE = ObjectManager.MaterialManager.GetMaterialByName(eqp.Data.NODENO, materialID);
                        if (materialStatus ==eMaterialStatus.MOUNT)
                        {
                            materialE = new MaterialEntity();
                            materialE.NodeNo = eqp.Data.NODENO;
                            materialE.OperatorID = operatorID;
                            materialE.MaterialID = materialID;
                            materialE.UnitNo = unitNo;
                            materialE.MaterialStatus = materialStatus;
                            materialE.MaterialRecipeID = recipeID;
                            //materialE.UVMaskUseCount = UVMaskUseCount;//sy modify to use [Use Count] 20160114
                            materialE.UseCount = count;
                            materialE.MaterialValue = count;//DisMount report MES sy 20151207
                            materialE.MaterialType = materialType;//BC Mount前不會知道，EQ都不會知道 sy 20151207 //MES 要求UVMASK要上報TYPE
                            materialE.InUseTime = (long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo))).ToString().PadLeft(14, '0');
                        }
                        else
                        {
                            if (materialE == null)
                            {
                                MaterialStatusChangeReportReply(inputData.Metadata.NodeNo, unitNo, "", material, eBitResult.ON, eReturnCode1.NG, inputData.TrackKey, false);
                                throw new Exception(string.Format("Can't find Material ID =[{0}] in MaterialEntity!", materialID));
                            }
                            materialE.OperatorID = operatorID;
                            materialE.MaterialStatus = materialStatus;
                            materialE.UseCount = count;
                            materialE.MaterialValue = count;//DisMount report MES sy 20151207
                            DateTime dt;//20161115 sy modify 
                            if (DateTime.TryParseExact(materialE.InUseTime, "yyyyMMddHHmmss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
                            {
                                TimeSpan ts = DateTime.Now.Subtract(dt);
                                materialE.UsedTime = Math.Round(ts.TotalMinutes).ToString();
                            }
                        }
                        

                        materialList.Add(materialE);
                        ObjectManager.MaterialManager.AddMaterial(materialE);
                        ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, materialE, string.Empty, inputData.TrackKey, "", "");
                        #endregion
                        #region [Check Unit]
                        string unitID = string.Empty;
                        if (!unitNo.Equals("0"))
                        {
                              Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                              if (unit != null)
                              {
                                    unitID = unit.Data.UNITID;
                              }
                        }
                        #endregion
                        //取出特定幾台機台內任一片玻璃的 Job ID 上報，若則無須上報。
                        IList<Equipment> EQPList = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).
                            Where(e => e.Data.NODENO == "L2" || e.Data.NODENO == "L3" || e.Data.NODENO == "L4").ToList();
                        string productName = ObjectManager.JobManager.GetJobIDbyEQPList(EQPList);

                        #region [Material MES]
                        switch (materialStatus)
                        {
                              case eMaterialStatus.MOUNT:
                                    object[] _data1 = new object[6]
                                      { 
                                          inputData.TrackKey,  /*0 TrackKey*/
                                          eqp,                 /*1 EQP*/
                                          productName,         /*2 GlassID*/
                                          "",                  /*3 MaterialDurableName*/
                                          "",                  /*4 PolType*/
                                          materialList         /*5 MaterialList*/
                                      };
                                    Invoke(eServiceName.MESService, "MaterialMount", _data1);

                                    break;
                              case eMaterialStatus.DISMOUNT:
                                    object[] _data2 = new object[5]
                                      { 
                                          inputData.TrackKey,  /*0 TrackKey*/
                                          eqp,                 /*1 EQP*/
                                          productName,         /*2 GlassID*/
                                          "",                  /*3 MaterialDurableName*/
                                          materialList         /*5 MaterialList*/
                                      };
                                    Invoke(eServiceName.MESService, "MaterialDismountReport", _data2);
                                    MaterialStatusChangeReportReply(inputData.Metadata.NodeNo, unitNo, "", material, eBitResult.ON, eReturnCode1.OK, inputData.TrackKey, false);
                                    #region [MES Data Material Status Change Report]
                                    //        object[] _data3 = new object[16]
                                    //{ 
                                    //    inputData.TrackKey,  /*0 TrackKey*/
                                    //    eqp.Data.LINEID,     /*1 LineName*/
                                    //    eqp.Data.NODEID,     /*2 EQPID*/
                                    //    recipeID,                  /*3 LINERECIPENAME*/
                                    //    "",                  /*4 MATERIALMODE*/ 
                                    //    productName,         /*5 PRODUCTNAME*/
                                    //    materialID.Trim(),   /*6 MATERIALNAME*/
                                    //    materialStatus,      /*7 MATERIALSTATE*/
                                    //    "",					 /*8 MATERIALWEIGHT*/
                                    //    materialType,                /*9 MATERIALTYPE  CF固定填PR*/ 
                                    //    UVMaskUseCount,       /*10 USEDCOUNT*/
                                    //    "",                  /*11 LIFEQTIME*/
                                    //    "",                  /*12 GROUPID*/
                                    //    unitID,              /*13 UNITID*/
                                    //    "" ,             /*14 HEADID*/ 
                                    //    "MaterialStatusChangeReport"
                                    //};
                                    //        Invoke(eServiceName.MESService, "MaterialStateChanged", _data3);
                                    #endregion
                                    return;
                              default:
                                    #region [MES Data Material Status Change Report]
                                    object[] _data4 = new object[16]
                                        { 
                                            inputData.TrackKey,  /*0 TrackKey*/
                                            eqp.Data.LINEID,     /*1 LineName*/
                                            eqp.Data.NODEID,     /*2 EQPID*/
                                            recipeID,                  /*3 LINERECIPENAME*/
                                            "",                  /*4 MATERIALMODE*/ 
                                            productName,         /*5 PRODUCTNAME*/
                                            materialID.Trim(),   /*6 MATERIALNAME*/
                                            materialStatus,      /*7 MATERIALSTATE*/
                                            "",					 /*8 MATERIALWEIGHT*/
                                            "",                /*9 MATERIALTYPE  CF固定填PR*/ //BC Mount前不會知道，EQ都不會知道 sy 20151207
                                            UVMaskUseCount,       /*10 USEDCOUNT*/
                                            "",                  /*11 LIFEQTIME*/
                                            "",                  /*12 GROUPID*/
                                            unitID,              /*13 UNITID*/
                                            "" ,             /*14 HEADID*/ 
                                            "MaterialStatusChangeReport"
                                        };
                                    Invoke(eServiceName.MESService, "MaterialStateChanged", _data4);
                                    #endregion
                                    break;
                        }
                        #endregion
                        #region [Timer]
                        //20151110 cy:設定Timermanager時,將UnitNo帶在最後一個參數,在取得Timer時可以取得,Key值只要有TransactionKey即可
                        string timerID = string.Format("MaterialStatusChangeReport_CELL_{0}_{1}_MaterialStateChanged", eqp.Data.NODENO, inputData.TrackKey);
                        if (this.Timermanager.IsAliveTimer(timerID))
                        {
                              Timermanager.TerminateTimer(timerID);
                        }
                        Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), 
                              new System.Timers.ElapsedEventHandler(MaterialStatusChangeMESTimeout_CELL), Tuple.Create(inputData.Metadata.NodeNo, unitNo));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void MaterialStatusChangeReport_CELL_PAM(Trx inputData)
            {
                try
                {
                    #region[Get EQP & LINE]
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                    if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                    if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                    #endregion
                    #region [PLCAgent Data Bit]
                    eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                    #endregion
                    #region[If Bit Off->Return]
                    if (bitResult == eBitResult.OFF)
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                        //MaterialStatusChangeReportReply_CELL(string eqpNo, string unitNo, string headNo, List<MaterialEntity> materialDataList, eBitResult value, eReturnCode1 value1, string trackKey, bool flgDisable)
                        MaterialStatusChangeReportReply_CELL_PAM(inputData.Metadata.NodeNo, "", "", null, eBitResult.OFF, eReturnCode1.Unknown, inputData.TrackKey, true);
                        return;
                    }
                    #endregion
                    #region [PLCAgent Data Word]
                    string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();//unit No
                    string recipeID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();//recipe ID
                    string materialID = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();//material ID
                    eMaterialStatus materialStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value);//material Status
                    string operatorID = inputData.EventGroups[0].Events[0].Items[4].Value.Trim();//operator ID
                    string lotID_1 = inputData.EventGroups[0].Events[0].Items[5].Value.Trim();
                    string lotNo_1 = inputData.EventGroups[0].Events[0].Items[6].Value.Trim();
                    string count_1 = inputData.EventGroups[0].Events[0].Items[7].Value.Trim();
                    string lotID_2 = inputData.EventGroups[0].Events[0].Items[8].Value.Trim();
                    string lotNo_2 = inputData.EventGroups[0].Events[0].Items[9].Value.Trim();
                    string count_2 = inputData.EventGroups[0].Events[0].Items[10].Value.Trim();
                    string lotID_3 = inputData.EventGroups[0].Events[0].Items[11].Value.Trim();
                    string lotNo_3 = inputData.EventGroups[0].Events[0].Items[12].Value.Trim();
                    string count_3 = inputData.EventGroups[0].Events[0].Items[13].Value.Trim();
                    string lotID_4 = inputData.EventGroups[0].Events[0].Items[14].Value.Trim();
                    string lotNo_4 = inputData.EventGroups[0].Events[0].Items[15].Value.Trim();
                    string count_4 = inputData.EventGroups[0].Events[0].Items[16].Value.Trim();
                    string lotID_5 = inputData.EventGroups[0].Events[0].Items[17].Value.Trim();
                    string lotNo_5 = inputData.EventGroups[0].Events[0].Items[18].Value.Trim();
                    string count_5 = inputData.EventGroups[0].Events[0].Items[19].Value.Trim();
                    string lotID_6 = inputData.EventGroups[0].Events[0].Items[20].Value.Trim();//20151208 MES閻波 最多10層
                    string lotNo_6 = inputData.EventGroups[0].Events[0].Items[21].Value.Trim();
                    string count_6 = inputData.EventGroups[0].Events[0].Items[22].Value.Trim();
                    string lotID_7 = inputData.EventGroups[0].Events[0].Items[23].Value.Trim();
                    string lotNo_7 = inputData.EventGroups[0].Events[0].Items[24].Value.Trim();
                    string count_7 = inputData.EventGroups[0].Events[0].Items[25].Value.Trim();
                    string lotID_8 = inputData.EventGroups[0].Events[0].Items[26].Value.Trim();
                    string lotNo_8 = inputData.EventGroups[0].Events[0].Items[27].Value.Trim();
                    string count_8 = inputData.EventGroups[0].Events[0].Items[28].Value.Trim();
                    string lotID_9 = inputData.EventGroups[0].Events[0].Items[29].Value.Trim();
                    string lotNo_9 = inputData.EventGroups[0].Events[0].Items[30].Value.Trim();
                    string count_9 = inputData.EventGroups[0].Events[0].Items[31].Value.Trim();
                    string lotID_10 = inputData.EventGroups[0].Events[0].Items[32].Value.Trim();
                    string lotNo_10 = inputData.EventGroups[0].Events[0].Items[33].Value.Trim();
                    string count_10 = inputData.EventGroups[0].Events[0].Items[34].Value.Trim();
                    string feedingPortID = inputData.EventGroups[0].Events[0].Items[35].Value.Trim();//Feeding Port ID
                    feedingPortID = feedingPortID == "CF" ? "CF" : "TFT";//20151208 MES閻波 要報CF or TFT
                    #endregion
                    #region [Log]
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], RECIPE ID =[{4}], MATERIAL ID =[{5}], MATERIAL STATUS =[{6}], LotIDLIST =[{7}], TYPE =[{8}], OPERATOR ID =[{9}], BIT (ON)",
                        eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, recipeID, materialID, materialStatus, string.Format("{0},{1},{2},{3},{4}.{5},{6},{7},{8},{9}", lotID_1, lotID_2, lotID_3, lotID_4, lotID_5, lotID_6, lotID_7, lotID_8, lotID_9, lotID_10), feedingPortID, operatorID));
                    #endregion
                    #region Offline to send MES & Reply EQ OK/NG by Setting
                    if (line.File.HostMode == eHostMode.OFFLINE)
                    {
                        eReturnCode1 RtnCode = eReturnCode1.OK;
                        if (!ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean())
                        {
                            RtnCode = eReturnCode1.NG;
                        }
                        //eReturnCode1 RtnCode = eReturnCode1.NG;
                        LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS -> MES][{0}] PAM Material State Changed Send MES but OFFLINE LINENAME=[{1}].",
                            inputData.TrackKey, eqp.Data.LINEID));
                        MaterialStatusChangeReportReply_CELL_PAM(inputData.Metadata.NodeNo, "", "", null, eBitResult.ON, RtnCode, inputData.TrackKey, false);
                        return;
                    }
                    #endregion
                    #region [Check Unit]
                    string unitID = string.Empty;
                    if (!unitNo.Equals("0"))
                    {
                        Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                        if (unit != null)
                        {
                            unitID = unit.Data.UNITID;
                        }
                    }
                    #endregion

                    #region [POL Material Save to DB]
                    List<MaterialEntity> subMaterials = new List<MaterialEntity>();
                    MaterialEntity materialPol = ObjectManager.MaterialManager.GetMaterialByName(eqp.Data.NODENO, materialID);
                    if (materialPol == null)
                    {
                        materialPol = new MaterialEntity();
                        materialPol.NodeNo = eqp.Data.NODENO;
                        materialPol.OperatorID = operatorID;
                        materialPol.MaterialID = materialID;
                        materialPol.UnitNo = unitNo;
                        materialPol.MaterialStatus = materialStatus;
                        materialPol.MaterialRecipeID = recipeID;
                        materialPol.MaterialType = feedingPortID.Trim();
                        ObjectManager.MaterialManager.AddMaterial(materialPol);
                    }
                    else //sy add materialPol 找到 更新 不然history 是舊的
                    {
                        materialPol.NodeNo = eqp.Data.NODENO;
                        materialPol.OperatorID = operatorID;
                        materialPol.MaterialID = materialID;
                        materialPol.UnitNo = unitNo;
                        materialPol.MaterialStatus = materialStatus;
                        materialPol.MaterialRecipeID = recipeID;
                        materialPol.MaterialType = feedingPortID.Trim();
                    }
                    string polCount = "0";
                    switch (materialStatus)
                    {
                        case eMaterialStatus.MOUNT:
                            materialPol.CellPAMMateril.Clear();
                            break;
                        case eMaterialStatus.DISMOUNT://2015 1208 MAX 10 
                            polCount = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", count_1, count_2, count_3, count_4, count_5, count_6, count_7, count_8, count_9, count_10);
                            //Dismount時,以機台報的資料上報
                            for (int i = 5; i < (5 + (3 * 10)); i++)
                            {
                                MaterialEntity material = new MaterialEntity();
                                string lotid = inputData.EventGroups[0].Events[0].Items[i++].Value.Trim();
                                string lotno = inputData.EventGroups[0].Events[0].Items[i++].Value.Trim();
                                string count = inputData.EventGroups[0].Events[0].Items[i].Value.Trim();
                                material.NodeNo = eqp.Data.NODENO;
                                material.MaterialType = feedingPortID.Trim();
                                material.MaterialID = lotid;
                                material.MaterialStatus = materialStatus;
                                material.UnitNo = unitNo;
                                material.MaterialPosition = lotno;
                                material.MaterialCount = count;
                                material.MaterialPort = lotno;

                                subMaterials.Add(material);
                            }
                            ////Postion,LotID,Count,LotNo
                            //materialPol.CellPOLMaterial.Add(string.Format("1,{0},{1},{2}", lotID_1, count_1, lotNo_1));
                            //materialPol.CellPOLMaterial.Add(string.Format("2,{0},{1},{2}", lotID_2, count_2, lotNo_2));
                            //materialPol.CellPOLMaterial.Add(string.Format("3,{0},{1},{2}", lotID_3, count_3, lotNo_3));
                            //materialPol.CellPOLMaterial.Add(string.Format("4,{0},{1},{2}", lotID_4, count_4, lotNo_4));
                            //materialPol.CellPOLMaterial.Add(string.Format("5,{0},{1},{2}", lotID_5, count_5, lotNo_5));
                            break;
                        case eMaterialStatus.INUSE:
                        case eMaterialStatus.PREPARE:
                            polCount = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", count_1, count_2, count_3, count_4, count_5, count_6, count_7, count_8, count_9, count_10);
                            break;
                        default:
                            break;
                    }
                    //20151110 cy:Main Material才產生物件,並加到History
                    ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, materialPol, string.Empty, inputData.TrackKey, "", polCount);

                    #endregion
                    //取出特定幾台機台內任一片玻璃的 Job ID 上報，若則無須上報。
                    IList<Equipment> EQPList = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).
                        Where(e => e.Data.NODENO == "L2" || e.Data.NODENO == "L3" || e.Data.NODENO == "L4").ToList();
                    string productName = ObjectManager.JobManager.GetJobIDbyEQPList(EQPList);
                    #region Add Reply Key [20151110 mark by cy]
                    //20151110 cy:設定Timermanager時,可以帶在最後一個參數,在取得Timer時可以取得,Key值只要有TransactionKey即可
                    ////MES Reply no Unit No (PLC Write Key),BC Add Repository 自行處理加入倉庫
                    //string key = keyBoxReplyPLCKey.POLStateChangedReply;
                    //string rep = unitNo;
                    //if (Repository.ContainsKey(key))
                    //    Repository.Remove(key);
                    //Repository.Add(key, rep);
                    #endregion
                    #region Materila Entity Collection  [20151110 mark by cy]
                    //20151110 cy:SubMaterial的資訊記到CellPOLMaterial內
                    //List<MaterialEntity> materialList = new List<MaterialEntity>();
                    //List<MaterialEntity> materialListE = new List<MaterialEntity>();
                    //materialListE =ObjectManager.MaterialManager.GetMaterials();
                    //MaterialEntity materialE; MaterialEntity materialEE; string lotID = string.Empty; string lotNo = string.Empty; string count = string.Empty;
                    //for (int i = 0; i < 5; i++)
                    //{
                    //    lotID = inputData.EventGroups[0].Events[0].Items[i * 3 + 5].Value.Trim();
                    //    lotNo = inputData.EventGroups[0].Events[0].Items[i * 3 + 6].Value.Trim();
                    //    count = inputData.EventGroups[0].Events[0].Items[i * 3 + 7].Value.Trim();
                    //    materialE = new MaterialEntity();
                    //    materialE.NodeNo = eqp.Data.NODENO;
                    //    materialE.OperatorID = operatorID;
                    //    materialE.MaterialID = lotID;//MATERIALNAME
                    //    materialE.UnitNo = unitNo;//UNITID
                    //    materialE.MaterialStatus = materialStatus;//MATERIALSTATE
                    //    materialE.MaterialRecipeID = recipeID;
                    //    materialE.MaterialValue = "";//USEDCOUNT
                    //    materialE.MaterialType = "";//MATERIALTYPE TO DO
                    //    materialE.MaterialPosition = lotNo;//MATERIALPOSITION //Dismount
                    //    materialE.MaterialCount = count;//MATERIALCOUNT //Dismount
                    //    //取出MES MountReply的Materail
                    //    materialEE = ObjectManager.MaterialManager.GetMaterialByKey(string.Format("{0}_{1}_{2}_{3}", eqp.Data.NODENO, unitNo, lotNo, lotID));
                    //    if (materialEE != null)
                    //    {
                    //        switch (materialStatus)
                    //        {
                    //            case eMaterialStatus.MOUNT:
                    //                break;
                    //            case eMaterialStatus.DISMOUNT:
                    //                materialE.MaterialAbnormalCode = materialEE.MaterialAbnormalCode;//MATERIALABNORMALCODE //Dismount
                    //                materialE.UsedTime = ( long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo)) -long.Parse(materialEE.InUseTime) ).ToString().PadLeft(14,'0');
                    //                break;
                    //            case eMaterialStatus.INUSE:
                    //                materialEE.InUseTime = DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    //                ObjectManager.MaterialManager.AddMaterial(materialEE);
                    //                break;
                    //            case eMaterialStatus.PREPARE:
                    //                break;
                    //            case eMaterialStatus.NONE:
                    //                break;
                    //            default:
                    //                break;
                    //        }
                    //    }
                    //    materialList.Add(materialE);
                    //    ObjectManager.MaterialManager.AddMaterial(materialE);
                    //    ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, materialE, string.Empty, "", "");
                    //}
                    #endregion
                    #region [Material MES]
                    switch (materialStatus)
                    {
                        case eMaterialStatus.MOUNT:
                            object[] _data1 = new object[6]
                                      { 
                                          inputData.TrackKey,  /*0 TrackKey*/
                                          eqp,                 /*1 EQP*/
                                          productName,         /*2 GlassID*/
                                          materialID,                  /*3 MaterialDurableName*/
                                          feedingPortID.Trim(),                  /*4 PolType*/
                                          //materialList         /*5 MaterialList*/
                                          new List<MaterialEntity>(){materialPol}
                                      };
                            Invoke(eServiceName.MESService, "MaterialMount_CELL_PAM", _data1);     //20151111 cy:叫用獨立的Method

                            break;
                        case eMaterialStatus.DISMOUNT:      //MESService增加MaterialDismountReport_PAM,改叫用這個
                            object[] _data2 = new object[5]
                                      { 
                                          inputData.TrackKey,  /*0 TrackKey*/
                                          eqp,                 /*1 EQP*/
                                          productName,         /*2 GlassID*/
                                          materialID,                  /*3 MaterialDurableName*/
                                          subMaterials         /*5 MaterialList*/
                                          //new List<MaterialEntity>(){materialPol}
                                      };
                            Invoke(eServiceName.MESService, "MaterialDismountReport_CELL_PAM", _data2);
                            MaterialStatusChangeReportReply_CELL_PAM(inputData.Metadata.NodeNo, unitNo, "", null, eBitResult.ON, eReturnCode1.OK, inputData.TrackKey, false);
                            //20151111 cy:DismountReport不會有Reply,所以不用下去計算TimeOut
                            return;
                        #region [MES Data Material Status Change Report] Do not need.
                        //        object[] _data3 = new object[16]
                        //{ 
                        //    inputData.TrackKey,  /*0 TrackKey*/
                        //    eqp.Data.LINEID,     /*1 LineName*/
                        //    eqp.Data.NODEID,     /*2 EQPID*/
                        //    recipeID,                  /*3 LINERECIPENAME*/
                        //    "",                  /*4 MATERIALMODE*/ 
                        //    "",         /*5 PRODUCTNAME*/
                        //    materialID.Trim(),   /*6 MATERIALNAME*/
                        //    materialStatus,      /*7 MATERIALSTATE*/
                        //    "",					 /*8 MATERIALWEIGHT*/
                        //    feedingPortID,                /*9 MATERIALTYPE */ 
                        //    "",       /*10 USEDCOUNT*/
                        //    "",                  /*11 LIFEQTIME*/
                        //    "",                  /*12 GROUPID*/
                        //    unitID,              /*13 UNITID*/
                        //    "" ,             /*14 HEADID*/ 
                        //    "MaterialStatusChangeReport"
                        //};
                        //        Invoke(eServiceName.MESService, "MaterialStateChanged", _data3);
                        #endregion
                        //break;
                        default:
                            #region [MES Data Material Status Change Report]
                            object[] _data4 = new object[16]
                                        { 
                                            inputData.TrackKey,  /*0 TrackKey*/
                                            eqp.Data.LINEID,     /*1 LineName*/
                                            eqp.Data.NODEID,     /*2 EQPID*/
                                            recipeID,                  /*3 LINERECIPENAME*/
                                            "",                  /*4 MATERIALMODE*/ 
                                            "",         /*5 PRODUCTNAME*/
                                            materialID.Trim(),   /*6 MATERIALNAME*/
                                            materialStatus,      /*7 MATERIALSTATE*/
                                            "",					 /*8 MATERIALWEIGHT*/
                                            feedingPortID,                /*9 MATERIALTYPE  */ 
                                            "",       /*10 USEDCOUNT*/
                                            "",                  /*11 LIFEQTIME*/
                                            "",                  /*12 GROUPID*/
                                            unitID,              /*13 UNITID*/
                                            "" ,             /*14 HEADID*/ 
                                            "MaterialStatusChangeReport"
                                        };
                            Invoke(eServiceName.MESService, "MaterialStateChanged", _data4);
                            #endregion
                            break;
                    }
                    #endregion
                    #region [Timer]
                    //20151110 cy:設定Timermanager時,將UnitNo帶在最後一個參數,在取得Timer時可以取得,Key值只要有TransactionKey即可
                    //string timerID = string.Format("MaterialStatusChangeReport_CELL_{0}_{1}_{2}_MaterialStateChanged", eqp.Data.NODENO, unitNo, inputData.TrackKey);
                    string timerID = string.Format("MaterialStatusChangeReport_CELL_{0}_{1}_MaterialStateChanged", eqp.Data.NODENO, inputData.TrackKey);
                    if (this.Timermanager.IsAliveTimer(timerID))
                    {
                        Timermanager.TerminateTimer(timerID);
                    }
                    Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(),
                          new System.Timers.ElapsedEventHandler(MaterialStatusChangeMESTimeout_CELL_PAM),
                          Tuple.Create(inputData.Metadata.NodeNo, unitNo, operatorID, feedingPortID));
                    #endregion
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            private string GetUseCount(string numerator, string denominator)
            {
                  try
                  {
                        string useCount = "";

                        if (numerator != "" && denominator != "")
                        {
                              //Watson Add 20150226 For EQP Report use count = 0.
                              if ((numerator == "0") || (denominator == "0"))
                              {
                                    useCount = "0";
                              }
                              double count = double.Parse(numerator) / double.Parse(denominator);
                              useCount = count.ToString();
                        }

                        return useCount;
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        return "";
                  }
            }

            public void MaterialStatusChangeReportReply(string eqpNo, string unitNo, string headNo, MaterialEntity material, eBitResult value, eReturnCode1 value1, string trackKey, bool flgDisable)
            {
                try
                {
                    Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_MaterialStatusChangeReportReply") as Trx;
                    #region[Get EQP ]
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, eqpNo));
                    #endregion
                    Line line = ObjectManager.LineManager.GetLine(ServerName);
                    eFabType fabType;
                    Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                    if (flgDisable)
                    {
                        outputdata.EventGroups[0].Events[0].IsDisable = true;
                    }
                    else
                    {
                        
                        if (fabType == eFabType.CF)
                        {
                            outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value1).ToString();
                            outputdata.EventGroups[0].Events[0].Items[1].Value = unitNo;
                            if (material.MaterialAliveTime == string.Empty)
                                material.MaterialAliveTime = "0";
                            outputdata.EventGroups[0].Events[0].Items[2].Value = material.MaterialAliveTime.ToString();
                        }
                        else if (fabType == eFabType.CELL)//sy CELL 修改增加
                        {
                            //MES 要求qTime unit 2.00 Min to PLC => 200
                            double qTime = 0;
                            if (double.TryParse(material.MaterialAliveTime.ToString() == string.Empty ? "0" : material.MaterialAliveTime.ToString(), out qTime))
                            {
                                //qTime = qTime * 100;//20160712 sy modify by 佳音 改取整數部份
                                qTime = Math.Truncate(qTime);
                            }
                            outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value1).ToString();
                            outputdata.EventGroups[0].Events[0].Items[1].Value = unitNo;

                            //20170317 huangjiayin: usecount在PLC是int型，mes可能给小数，新增防呆设定
                            if (string.IsNullOrEmpty(material.UseCount))
                            {
                                outputdata.EventGroups[0].Events[0].Items[2].Value ="0";
                            }
                            else if (!material.UseCount.Contains("."))
                            {
                                outputdata.EventGroups[0].Events[0].Items[2].Value = material.UseCount;
                            }
                            else
                            {
                                outputdata.EventGroups[0].Events[0].Items[2].Value = material.UseCount.Split('.')[0]; 
                            }
                            //
                            outputdata.EventGroups[0].Events[0].Items[3].Value = qTime.ToString();
                            if (outputdata.EventGroups[0].Events[0].Items["MaterialSpecName"] != null)
                                outputdata.EventGroups[0].Events[0].Items["MaterialSpecName"].Value = material.MaterialSpecName;
                        }
                        else
                        {
                            //GetWEVENT(fabType, ref outputdata, material);
                            outputdata.EventGroups[0].Events[0].Items["UnitNo"].Value = unitNo;
                            outputdata.EventGroups[0].Events[0].Items["MaterialStatusChangeReturnCode"].Value = ((int)value1).ToString();
                        }
                    }
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                    outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    if (fabType == eFabType.CF&&line.Data.LINEID!="FCOPH100")
                    {
                        if (eqpNo == "L6")
                        {
                            if (!string.IsNullOrEmpty(eqp.File.LastMaterialID))
                            {
                                if (materialStatus1.ToString() == "INUSE")
                                {
                                    if (eqp.File.LastMaterialID.Count() != 24)
                                    {
                                        lock (eqp.File)
                                        {
                                            eqp.File.LastMaterialID = "000000000000000000000000";
                                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                        }
                                    }
                                    if (eqp.File.LastMaterialID.Substring(9, 4) != materialID1.Substring(9, 4))
                                    {
                                        Invoke(eServiceName.EquipmentService, "TransferStopCommand", new object[] { "L5", "1", outputdata.TrackKey });
                                    }
                                    switch (materialslotno1)
                                    {
                                        case "1":
                                        case "3":
                                            if (!string.IsNullOrEmpty(eqp.File.Slot1WarmCountFlag))
                                            {
                                                if (int.Parse(eqp.File.Slot1WarmCountFlag) > 1)
                                                {
                                                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trackKey, "L6", "Warm count greater than one", "MES" });
                                                    Invoke(eServiceName.EquipmentService, "TransferStopCommand", new object[] { "L5", "1", trackKey });
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(eqp.File.LastWarmCount))
                                                    {
                                                        if (int.Parse(eqp.File.LastWarmCount) > 1)
                                                        {
                                                            Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trackKey, "L6", "Last Warm count greater than one", "MES" });
                                                            Invoke(eServiceName.EquipmentService, "TransferStopCommand", new object[] { "L5", "1", trackKey });
                                                        }
                                                    }
                                                }
                                                lock (eqp.File)
                                                {
                                                    eqp.File.LastWarmCount = eqp.File.Slot1WarmCountFlag;
                                                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                                }
                                            }
                                            break;
                                        case "2":
                                        case "4":
                                            if (!string.IsNullOrEmpty(eqp.File.Slot2WarmCountFlag))
                                            {
                                                if (int.Parse(eqp.File.Slot2WarmCountFlag) > 1)
                                                {
                                                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trackKey, "L6", "Warm count greater than one", "MES" });
                                                    Invoke(eServiceName.EquipmentService, "TransferStopCommand", new object[] { "L5", "1", trackKey });
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(eqp.File.LastWarmCount))
                                                    {
                                                        if (int.Parse(eqp.File.LastWarmCount) > 1)
                                                        {
                                                            Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trackKey, "L6", "Last Warm count greater than one", "MES" });
                                                            Invoke(eServiceName.EquipmentService, "TransferStopCommand", new object[] { "L5", "1", trackKey });
                                                        }
                                                    }
                                                }
                                                lock (eqp.File)
                                                {
                                                    eqp.File.LastWarmCount = eqp.File.Slot2WarmCountFlag;
                                                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                                }
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else if (materialStatus1.ToString() == "MOUNT")//add by hujunpeng 20190423 for mount时做提醒
                                {
                                    if (eqp.File.LastMaterialID.Count() != 24)
                                    {
                                        lock (eqp.File)
                                        {
                                            eqp.File.LastMaterialID = "000000000000000000000000";
                                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                        }
                                    }
                                    if (eqp.File.LastMaterialID.Substring(9, 4) != materialID1.Substring(9, 4))
                                    {
                                        Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trackKey, "L6", "PRID is changed", "MES" });
                                    }
                                    switch (materialslotno1)
                                    {
                                        case "1":
                                        case "3":
                                            if (!string.IsNullOrEmpty(eqp.File.Slot1WarmCountFlag))
                                            {
                                                if (int.Parse(eqp.File.Slot1WarmCountFlag) > 1)
                                                {
                                                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trackKey, "L6", "Warm count greater than one", "MES" });                                                   
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(eqp.File.LastWarmCount))
                                                    {
                                                        if (int.Parse(eqp.File.LastWarmCount) > 1)
                                                        {
                                                            Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trackKey, "L6", "Last Warm count greater than one", "MES" });         
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        case "2":
                                        case "4":
                                            if (!string.IsNullOrEmpty(eqp.File.Slot2WarmCountFlag))
                                            {
                                                if (int.Parse(eqp.File.Slot2WarmCountFlag) > 1)
                                                {
                                                    Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trackKey, "L6", "Warm count greater than one", "MES" });
                                                }
                                                else
                                                {
                                                    if (!string.IsNullOrEmpty(eqp.File.LastWarmCount))
                                                    {
                                                        if (int.Parse(eqp.File.LastWarmCount) > 1)
                                                        {
                                                            Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommand", new object[] { trackKey, "L6", "Last Warm count greater than one", "MES" });
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            lock (eqp)
                            {
                                if (materialStatus1.ToString() == "INUSE")
                                {
                                    eqp.File.LastMaterialID = materialID1;
                                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                }
                            }
                        }
                    }//20180601 by hujunpeng
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + MaterialStatusChangeReportTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + MaterialStatusChangeReportTimeout);
                    }

                    //20151113 cy:BC reply bit ON,要監測機台是否OFF,所以不能Mark掉這個//Bit off log不記 RETURN CODE by sy 20160130
                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(eqpNo + "_" + MaterialStatusChangeReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStatusChangeReportReplyTimeout), trackKey);
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MATERIAL STATUS CHANGE REPORT REPLY SET BIT =[{2}] RETURN CODE =[{3}].",
                             eqpNo, trackKey, value.ToString(), value1.ToString()));
                    }
                    else
                    {
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MATERIAL STATUS CHANGE REPORT REPLY SET BIT =[{2}].",
                                                    eqpNo, trackKey, value.ToString()));
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }

            public void MaterialStatusChangeReportReply_CELL_PAM(string eqpNo, string unitNo, string headNo, MaterialEntity material, eBitResult value, eReturnCode1 value1, string trackKey, bool flgDisable)
            {
                try
                {
                    Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_MaterialStatusChangeReportReply") as Trx;
                    string materialog = string.Empty;
                    if (flgDisable)
                    {
                        outputdata.EventGroups[0].Events[0].IsDisable = true;
                    }
                    else
                    {
                        Line line = ObjectManager.LineManager.GetLine(ServerName);
                        int lotCount = 10;
                        //先清資料
                        outputdata.EventGroups[0].Events[0].Items["LifeQTime"].Value = "0";
                        for (int i = 1; i <= lotCount; i++)
                        {
                            outputdata.EventGroups[0].Events[0].Items["LotID#" + i.ToString().PadLeft(2, '0')].Value = "";//Lot ID#
                            outputdata.EventGroups[0].Events[0].Items["LotNo#" + i.ToString().PadLeft(2, '0')].Value = "0";//Lot No#
                            outputdata.EventGroups[0].Events[0].Items["Count#" + i.ToString().PadLeft(2, '0')].Value = "0";//Count#
                            outputdata.EventGroups[0].Events[0].Items["AbnormalCode#" + i.ToString().PadLeft(2, '0')].Value = "";//ABNORMALCODE# sy 20160123 add
                        }
                        //若是Mount,且有SubMaterial的資料再指定
                        if (material != null && material.MaterialStatus == eMaterialStatus.MOUNT && material.CellPAMMateril.Count > 0)
                        {
                            material.CellPAMMateril.Sort((x, y) => { return x.MaterialPosition.CompareTo(y.MaterialPosition); });
                            foreach (MaterialEntity materialData in material.CellPAMMateril)
                            {
                                //SubMaterial的資料較多,就不再塞資料,記Log跳出
                                //if (steatint >= outputdata.EventGroups[0].Events[0].Items.Count)
                                if (int.Parse(materialData.MaterialPosition) > lotCount)
                                {
                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                          string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}], Cartrige material({2}) count of lot info from MES({3}) is more then PLC set({4}).",
                                                eqpNo, trackKey, material.MaterialID, material.CellPAMMateril.Count, outputdata.EventGroups[0].Events[0].Items.Count));
                                    break;
                                }
                                if (int.Parse(materialData.MaterialPosition) == 1)//只記第一筆 的Qtime Mes 閻波                        
                                    outputdata.EventGroups[0].Events[0].Items["LifeQTime"].Value = materialData.MaterialAliveTime;//Qtime
                                if (string.IsNullOrEmpty(outputdata.EventGroups[0].Events[0].Items["LifeQTime"].Value) || outputdata.EventGroups[0].Events[0].Items["LifeQTime"].Value == "0")//防止第一層沒給 後面有的 取下一個 sy 20161020
                                    outputdata.EventGroups[0].Events[0].Items["LifeQTime"].Value = materialData.MaterialAliveTime;
                                outputdata.EventGroups[0].Events[0].Items["LotID#" + materialData.MaterialPosition.PadLeft(2, '0')].Value = materialData.MaterialID;//Lot ID#
                                outputdata.EventGroups[0].Events[0].Items["LotNo#" + materialData.MaterialPosition.PadLeft(2, '0')].Value = materialData.MaterialPosition;//Lot No#
                                outputdata.EventGroups[0].Events[0].Items["Count#" + materialData.MaterialPosition.PadLeft(2, '0')].Value = materialData.MaterialCount;//Count#
                                outputdata.EventGroups[0].Events[0].Items["AbnormalCode#" + materialData.MaterialPosition.PadLeft(2, '0')].Value = materialData.MaterialAbnormalCode;//ABNORMALCODE# sy 20160123 add                                
                                materialog = materialog + materialData.MaterialPosition + "," + materialData.MaterialID + "," + materialData.MaterialCount + "," + materialData.MaterialAbnormalCode + "~";
                            }
                        }
                    }

                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value1).ToString();//returncode
                    outputdata.EventGroups[0].Events[0].Items[1].Value = unitNo == "" ? "0" : unitNo;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    if (_timerManager.IsAliveTimer(eqpNo + "_" + MaterialStatusChangeReportTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + MaterialStatusChangeReportTimeout);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(eqpNo + "_" + MaterialStatusChangeReportTimeout, false, ParameterManager["T2"].GetInteger(),
                              new System.Timers.ElapsedEventHandler(MaterialStatusChangeReportReply_CELL_PAMTimeout), trackKey);
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MATERIAL STATUS CHANGE REPORT REPLY SET BIT =[{2}] RETURN CODE =[{3}].[{4}]",
                            eqpNo, trackKey, value.ToString(), value1.ToString(), materialog));
                        return;
                    }
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MATERIAL STATUS CHANGE REPORT REPLY SET BIT =[{2}]",
                        eqpNo, trackKey, value.ToString()));
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            private void MaterialStatusChangeReportReply_CELL_PAMTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        if (_timerManager.IsAliveTimer(tmp))
                        {
                              _timerManager.TerminateTimer(tmp);
                        }

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP REPLY, MATERIAL STATUS CHANGE REPORT TIMEOUT SET BIT (OFF).", sArray[0], trackKey));
                        MaterialEntity material = new MaterialEntity(); 
                        MaterialStatusChangeReportReply_CELL_PAM(sArray[0], "0", "", material, eBitResult.OFF, eReturnCode1.Unknown, trackKey, true);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void GetWEVENT(eFabType fabType, ref Trx outputdata, MaterialEntity material)
            {
                  if (fabType == eFabType.CELL)
                  {
                        string numerator = "";
                        string denominator = "";
                        if ((ServerName.ToUpper().Contains("ODF")))
                        {
                              outputdata.EventGroups[0].Events[0].Items["UnitNo"].Value = (material.UnitNo == "" ? "0" : material.UnitNo);
                              outputdata.EventGroups[0].Events[0].Items["MaterialType"].Value = (material.MaterialType == "" ? "0" : material.MaterialType);
                              outputdata.EventGroups[0].Events[0].Items["HeadNo"].Value = (material.TankNo == "" ? "0" : material.TankNo);
                              outputdata.EventGroups[0].Events[0].Items["MaterialID"].Value = material.MaterialID;
                              outputdata.EventGroups[0].Events[0].Items["GroupID"].Value = material.GroupId;
                              GetNumerator(material.UseCount, ref numerator, ref denominator);
                              outputdata.EventGroups[0].Events[0].Items["Numerator"].Value = (numerator == "" ? "0" : numerator);
                              outputdata.EventGroups[0].Events[0].Items["Denominator"].Value = (denominator == "" ? "0" : denominator);
                              outputdata.EventGroups[0].Events[0].Items["MaterialAliveTime"].Value = (material.MaterialAliveTime == "" ? "0" : material.MaterialAliveTime);
                        }
                        else if (ServerName.ToUpper().Contains("PIL"))
                        {
                              outputdata.EventGroups[0].Events[0].Items["UnitNo"].Value = (material.UnitNo == "" ? "0" : material.UnitNo);
                              outputdata.EventGroups[0].Events[0].Items["TankNo"].Value = (material.TankNo == "" ? "0" : material.TankNo);
                              outputdata.EventGroups[0].Events[0].Items["MaterialID"].Value = material.MaterialID;
                              GetNumerator(material.UseCount, ref numerator, ref denominator);
                              outputdata.EventGroups[0].Events[0].Items["Numerator"].Value = (numerator == "" ? "0" : numerator);
                              outputdata.EventGroups[0].Events[0].Items["Denominator"].Value = (denominator == "" ? "0" : denominator);
                              outputdata.EventGroups[0].Events[0].Items["MaterialAliveTime"].Value = (material.MaterialAliveTime == "" ? "0" : material.MaterialAliveTime);
                        }
                  }
            }

            private void GetNumerator(string p, ref  string numerator, ref string denominator)
            {
                  try
                  {
                        if (p.Contains("."))
                        {
                              int index = p.IndexOf('.');
                              int pointLen = p.Length - 1 - index;
                              denominator = Math.Pow(10, pointLen).ToString();
                              numerator = p.Remove(index, 1);
                        }
                        else if (!string.IsNullOrEmpty(p))
                        {
                              denominator = "1";
                              numerator = p;
                        }
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }

            }

            private void MaterialStatusChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        if (_timerManager.IsAliveTimer(tmp))
                        {
                              _timerManager.TerminateTimer(tmp);
                        }

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP REPLY, MATERIAL STATUS CHANGE REPORT TIMEOUT SET BIT (OFF).", sArray[0], trackKey));
                        MaterialEntity material = new MaterialEntity();
                        MaterialStatusChangeReportReply(sArray[0], "0", "0", material, eBitResult.OFF, eReturnCode1.Unknown, trackKey, true);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            private void MaterialStatusChangeMESTimeout(object subject, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subject as UserTimer;
                        string tmp = timer.TimerId;
                        if (Timermanager.IsAliveTimer(tmp))
                        {
                              Timermanager.TerminateTimer(tmp);
                        }
                        string[] sArray = tmp.Split('_');
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[EQUIPMENT={0}] [BCS <- MES][{1}] MES REPLY MATERIAL STATUS CHANGE  TIMEOUT.", sArray[1], sArray[3]));
                        MaterialEntity material = new MaterialEntity();
                        //20160718 Modify by Frank
                        MaterialStatusChangeReportReply(sArray[1], sArray[4], "0", material, eBitResult.ON, eReturnCode1.NG, sArray[2], false);
                  }
                  catch (System.Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void MaterialStatusChangeMESTimeout_CELL(object subject, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        //MaterialStatusChangeReport_CELL_{NodeNo}_{TrxID}_MaterialStateChanged
                        UserTimer timer = subject as UserTimer;
                        string tmp = timer.TimerId;
                        if (Timermanager.IsAliveTimer(tmp))
                        {
                              Timermanager.TerminateTimer(tmp);
                        }
                        Tuple<string, string> tuple = timer.State as Tuple<string, string>;
                        
                        string[] sArray = tmp.Split('_');
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[EQUIPMENT={0}] [BCS <- MES][{1}] MES REPLY MATERIAL STATUS CHANGE  TIMEOUT.", sArray[2], sArray[3]));
                        MaterialEntity material = new MaterialEntity();
                        MaterialStatusChangeReportReply(sArray[2], tuple.Item2, "0", material, eBitResult.ON, eReturnCode1.NG, sArray[3], false);
                  }
                  catch (System.Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void MaterialStatusChangeMESTimeout_CELL_PAM(object subject, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        //MaterialStatusChangeReport_CELL_{NodeNo}_{TrxID}_MaterialStateChanged
                        UserTimer timer = subject as UserTimer;
                        string tmp = timer.TimerId;
                        if (Timermanager.IsAliveTimer(tmp))
                        {
                              Timermanager.TerminateTimer(tmp);
                        }
                        Tuple<string, string, string, string> tuple = timer.State as Tuple<string, string, string, string>;

                        string[] sArray = tmp.Split('_');
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[EQUIPMENT={0}] [BCS <- MES][{1}] MES REPLY MATERIAL STATUS CHANGE  TIMEOUT.", sArray[2], sArray[3]));
                        MaterialEntity material = new MaterialEntity();
                        MaterialStatusChangeReportReply_CELL_PAM(sArray[2], tuple.Item2, "0", material, eBitResult.ON, eReturnCode1.NG, sArray[3], false);
                  }
                  catch (System.Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void MaterialStatusChangeReportReply_PUM(string eqpNo, string unitNo, string materialID, List<MaterialEntity> materialList, eBitResult value, eReturnCode1 returncode, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_MaterialStatusChangeReportReply") as Trx;
                        if (value == eBitResult.OFF)
                        {
                              outputdata.EventGroups[0].Events[0].IsDisable = true;
                        }
                        else
                        {
                              Line line = ObjectManager.LineManager.GetLine(ServerName);
                              eFabType fabType;
                              Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                              outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)returncode).ToString(); //returncode
                              int unitno = 0;
                              int.TryParse(unitNo, out unitno);
                              outputdata.EventGroups[0].Events[0].Items[1].Value = unitno.ToString(); //Unit No
                              outputdata.EventGroups[0].Events[0].Items[2].Value = materialID;    //Mini Cassette ID

                              #region 可能會沒有值，必須先填入空白或零
                              outputdata.EventGroups[0].Events[0].Items[3].Value = string.Empty;  //Mask ID#01
                              outputdata.EventGroups[0].Events[0].Items[4].Value = "0";
                              outputdata.EventGroups[0].Events[0].Items[5].Value = string.Empty;  //Mask ID#02
                              outputdata.EventGroups[0].Events[0].Items[6].Value = "0";
                              outputdata.EventGroups[0].Events[0].Items[7].Value = string.Empty;  //Mask ID#03
                              outputdata.EventGroups[0].Events[0].Items[8].Value = "0";
                              outputdata.EventGroups[0].Events[0].Items[9].Value = string.Empty;  //Mask ID#04
                              outputdata.EventGroups[0].Events[0].Items[10].Value = "0";
                              outputdata.EventGroups[0].Events[0].Items[11].Value = string.Empty;  //Mask ID#05
                              outputdata.EventGroups[0].Events[0].Items[12].Value = "0";
                              outputdata.EventGroups[0].Events[0].Items[13].Value = string.Empty;  //Mask ID#06
                              outputdata.EventGroups[0].Events[0].Items[14].Value = "0";
                              #endregion

                              int i = 3;
                              foreach (MaterialEntity material in materialList)
                              {
                                    outputdata.EventGroups[0].Events[0].Items[i].Value = material.MaterialID;  //Mask ID#01
                                    int usecount = 0;
                                    int.TryParse(material.UseCount, out usecount);
                                    outputdata.EventGroups[0].Events[0].Items[i + 1].Value = usecount.ToString();  //Use Count#01
                                    i += 2;
                              }

                              string timerID = string.Format("{0}_{1}_PUMMaterialStateChanged", trackKey, eqpNo);
                              if (this.Timermanager.IsAliveTimer(timerID))
                                    Timermanager.TerminateTimer(timerID);
                              Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStatusChangeReportReply_PUMTimeout), eqpNo);

                        }

                        outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                        //Write Word dely 200 ms then Bit On
                        outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MATERIAL STATUS CHANGE REPORT REPLY SET BIT =[{2}] RETURN CODE =[{3}],UNITNO =[{4}].",
                            eqpNo, trackKey, value.ToString(), returncode.ToString(), unitNo));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void MaterialStatusChangeReportReply_PUMTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string[] sArray = tmp.Split('_');
                        string trackKey = sArray[0];
                        string eqpno = sArray[1];
                        string timerID = string.Format("{0}_{1}_PUMMaterialStateChanged", trackKey, eqpno);
                        if (_timerManager.IsAliveTimer(timerID))
                              _timerManager.TerminateTimer(timerID);

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP REPLY, MATERIAL STATUS CHANGE REPORT FOR PUM TIMEOUT SET BIT (OFF).", sArray[0], trackKey));
                        MaterialStatusChangeReportReply_PUM(eqpno, string.Empty, string.Empty, new List<MaterialEntity>(), eBitResult.OFF, eReturnCode1.Unknown, trackKey);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void MaskStateChangedReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        if (_timerManager.IsAliveTimer(tmp))
                        {
                              _timerManager.TerminateTimer(tmp);
                        }

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP REPLY, MES_MASKSTATECHANGEREPLYTIMEOUT SET BIT (OFF).", sArray[0], trackKey));
                        MaterialEntity material = new MaterialEntity();

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }


            public void PolStatusChangeReportReply(string eqpNo, string unitNo, string materialStatus, string materialId, string partNo, List<POLStateChanged.MATERIALc> materialList, eBitResult value, eReturnCode1 value1, string trackKey, bool flgDisable)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_MaterialStatusChangeReportReply") as Trx;
                        if (flgDisable)
                        {
                              outputdata.EventGroups[0].Events[0].IsDisable = true;
                        }
                        else
                        {
                              outputdata.EventGroups[0].Events[0].Items["MaterialStatusChangeReturnCode"].Value = ((int)value1).ToString();
                              //2014/01/20 Edison 注释掉
                              //20150302 Watson Modify 把注釋拿掉!!
                              outputdata.EventGroups[0].Events[0].Items["UnitNo"].Value = unitNo;
                              outputdata.EventGroups[0].Events[0].Items["MaterialID"].Value = materialId;
                              outputdata.EventGroups[0].Events[0].Items["PartNo"].Value = partNo;
                              for (int j = 0; j < 5; j++)
                              {
                                    outputdata.EventGroups[0].Events[0].Items[j * 2 + 4].Value = "";
                                    outputdata.EventGroups[0].Events[0].Items[j * 2 + 5].Value = "0";
                              }
                              if (materialStatus == "MOUNT")
                              {
                                    for (int i = 0; i < materialList.Count; i++)
                                    {
                                          outputdata.EventGroups[0].Events[0].Items[i * 2 + 4].Value = materialList[i].MATERIALNAME;
                                          outputdata.EventGroups[0].Events[0].Items[i * 2 + 5].Value = (materialList[i].COUNT == "" ? "0" : materialList[i].COUNT);
                                    }
                              }
                        }
                        outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                        //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                        outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + MaterialStatusChangeReportTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + MaterialStatusChangeReportTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + MaterialStatusChangeReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PolStatusChangeReportReplyTimeout), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,MATERIAL STATUS CHANGE REPORT REPLY SET BIT =[{2}], RETURN CODE =[{3}].",
                            eqpNo, trackKey, value.ToString(), value1.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            private void PolStatusChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        if (_timerManager.IsAliveTimer(tmp))
                        {
                              _timerManager.TerminateTimer(tmp);
                        }

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP REPLY, MATERIAL STATUS CHANGE REPORT TIMEOUT SET BIT (OFF).", sArray[0], trackKey));
                        List<POLStateChanged.MATERIALc> materialList = new List<POLStateChanged.MATERIALc>();
                        PolStatusChangeReportReply(sArray[0], "", "", "", "", materialList, eBitResult.OFF, eReturnCode1.Unknown, trackKey, true);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            private void POLStateChangedMESTimeout(object subject, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subject as UserTimer;
                        string tmp = timer.TimerId;
                        if (Timermanager.IsAliveTimer(tmp))
                        {
                              Timermanager.TerminateTimer(tmp);
                        }
                        string[] sArray = tmp.Split('_');
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[EQUIPMENT={0}] [BCS <- MES][{1}] MES REPLY POL STATE CHANGED TIMEOUT.", sArray[0], sArray[2]));
                        List<POLStateChanged.MATERIALc> materialList = new List<POLStateChanged.MATERIALc>();
                        PolStatusChangeReportReply(sArray[0], "0", "", "", "", materialList, eBitResult.ON, eReturnCode1.NG, sArray[2], false);
                  }
                  catch (System.Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                  }
            }

            #endregion

            #region [MaterialWeightReport]        
            public void MaterialWeightReport(Trx inputData)
            {
                try
                {
                    #region[Get EQP & LINE]
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                    if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                    if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                    #endregion
                    #region [PLCAgent Data Bit]
                    eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                    #endregion
                    #region[If Bit Off->Return]
                    MaterialEntity material = new MaterialEntity();
                    if (bitResult == eBitResult.OFF)
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                        MaterialWeightReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                        return;
                    }
                    #endregion
                    #region [PLCAgent Data Word]
                    string unitNo = inputData.EventGroups[0].Events[0].Items[0].Value;//unit No
                    string recipeID = inputData.EventGroups[0].Events[0].Items[1].Value;//recipe ID
                    string materialID = inputData.EventGroups[0].Events[0].Items[2].Value;//material ID
                    eMaterialStatus materialStatus = (eMaterialStatus)int.Parse(inputData.EventGroups[0].Events[0].Items[3].Value);//material Status
                    string materialWeight = inputData.EventGroups[0].Events[0].Items["MaterialWeight"].Value;//MaterialWeight

                    string UVMaskUseCount = string.Empty; string lotID = string.Empty; string lotNo = string.Empty; string count = string.Empty;
                    string feedingPortID = string.Empty; string materialType = string.Empty; string otherLog = string.Empty;
                    switch (line.Data.LINETYPE)//by line setting
                    {
                        case eLineType.CELL.CCPIL:
                        case eLineType.CELL.CCPIL_2:
                            if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCPIC)) materialType = "PI";
                            break;


                        //20161220: add by huangjiayin ODF的重量需要除以分母
                        case eLineType.CELL.CCODF:
                        case eLineType.CELL.CCODF_2:

                            #region ODF Material Weight Special
                            ParameterManager para = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                            if (para.ContainsKey("SEAL_LC_RATE"))
                            {
                               string[] rates= para["SEAL_LC_RATE"].GetString().Split(',');//rate0:SDP;rate1:LCD

                               if (eqp.Data.NODENO == "L7")
                               {
                                   materialWeight = (((double)Convert.ToInt32(materialWeight)) / Convert.ToInt32(rates[0])).ToString();
                                }
                               else if (eqp.Data.NODENO == "L13")
                               {
                                   materialWeight = (((double)Convert.ToInt32(materialWeight)) / Convert.ToInt32(rates[1])).ToString();
                               }

                            }
#endregion

                            break;
                        default:
                            break;
                    }
                    #endregion
                    MaterialWeightReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                    #region [Log]
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], UNIT_NO =[{3}], RECIPE ID =[{4}], MATERIAL ID =[{5}], MATERIAL STATUS =[{6}], Weight =[{7}], BIT (ON)",
                        eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, unitNo, recipeID, materialID, materialStatus, materialWeight));
                    #endregion

                    #region Materila Entity Collection
                    List<MaterialEntity> materialList = new List<MaterialEntity>();
                    MaterialEntity materialE = new MaterialEntity();
                    materialE.NodeNo = eqp.Data.NODENO;
                    materialE.MaterialWeight = materialWeight;
                    materialE.MaterialID = materialID;
                    materialE.UnitNo = unitNo;
                    materialE.MaterialStatus = materialStatus;
                    materialE.MaterialRecipeID = recipeID;
                    materialE.UVMaskUseCount = UVMaskUseCount;
                    materialE.UseCount = count;
                    materialE.MaterialType = materialType;

                    materialList.Add(materialE);
                    ObjectManager.MaterialManager.AddMaterial(materialE);
                    //Add By Yangzhenteng/20171024/Only For Cell PIC
                    if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCPIC))
                    {
                        ObjectManager.MaterialManager.RecordMaterialHistory_PIC(eqp.Data.NODEID, unitNo, materialE, string.Empty, inputData.TrackKey, recipeID, materialWeight);
                    }
                    else
                    {
                        ObjectManager.MaterialManager.RecordMaterialHistory(eqp.Data.NODEID, unitNo, materialE, string.Empty, inputData.TrackKey, "", "");
                    }                    
                    #endregion
                    #region [Check Unit]
                    string unitID = string.Empty;
                    if (!unitNo.Equals("0"))
                    {
                        Unit unit = ObjectManager.UnitManager.GetUnit(eqp.Data.NODENO, unitNo);
                        if (unit != null)
                        {
                            unitID = unit.Data.UNITID;
                        }
                    }
                    #endregion
                    //取出特定幾台機台內任一片玻璃的 Job ID 上報，若則無須上報。
                    IList<Equipment> EQPList = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).
                        Where(e => e.Data.NODENO == "L2" || e.Data.NODENO == "L3" || e.Data.NODENO == "L4").ToList();
                    string productName = ObjectManager.JobManager.GetJobIDbyEQPList(EQPList);

                    #region [Material MES]
                            object[] _data1 = new object[5]
                                      { 
                                          inputData.TrackKey,  /*0 TrackKey*/
                                          eqp,                 /*1 EQP*/
                                         "" ,         /*2 materialMode*/
                                          productName,                  /*3 panelID*/
                                          materialList         /*5 MaterialList*/
                                      };
                    //MaterialWeightReport(string trxID, Equipment eqp, string materialMode, string panelID, List<MaterialEntity> materialList)
                            Invoke(eServiceName.MESService, "MaterialWeightReport", _data1);
                    #endregion
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            private void MaterialWeightReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                try
                {

                    Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_MaterialWeightReportReply") as Trx;
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    if (_timerManager.IsAliveTimer(eqpNo + "_" + MaterialWeightReportTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + MaterialWeightReportTimeout);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(eqpNo + "_" + MaterialWeightReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialWeightReportReplyTimeout), trackKey);
                    }
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,CONSUME EVENT REPORT REPLY SET BIT =[{2}].",
                        eqpNo, trackKey, value.ToString()));
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            private void MaterialWeightReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                try
                {
                    UserTimer timer = subjet as UserTimer;
                    string tmp = timer.TimerId;
                    string trackKey = timer.State.ToString();
                    string[] sArray = tmp.Split('_');

                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP REPLY, CONSUME EVENT REPORT TIMEOUT SET BIT (OFF).", sArray[0], trackKey));

                    MaterialWeightReportReply(sArray[0], eBitResult.OFF, trackKey);

                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            #endregion
            public void PIMaterialWeightRequestCommand(string eqpNo, eBitResult bitResult, string trackKey)
            {
                try
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));

                    Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_PIMaterialWeightRequestCommand") as Trx;
                                if (outputData == null)
                                {
                                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] CAN'T FIND \"PI Material Weight Request Command\"!.", eqp.Data.NODENO));
                                    return;
                                }
                                outputData.EventGroups[0].Events[0].Items[0].Value = ((int)bitResult).ToString();
                                outputData.TrackKey = trackKey;
                                SendPLCData(outputData);

                                string timeName = string.Format("{0}_{1}", eqpNo, PIMaterialWeightRequestCommandTimeout);

                                if (bitResult == eBitResult.ON)
                                {
                                    if (_timerManager.IsAliveTimer(timeName))
                                    {
                                        _timerManager.TerminateTimer(timeName);
                                    }
                                    _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(PiMaterialWeightRequestCommandTimeout), outputData.TrackKey);
                               }
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PIMaterialWeightRequestCommand, SET BIT=[{2}].", eqp.Data.NODENO,
                    outputData.TrackKey,bitResult==eBitResult.ON?"ON":"OFF"));
         

                }                
          catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            public void PIMaterialWeightRequestCommandReply(Trx inputData)
            {
                if (inputData.IsInitTrigger) return;
                try
                {
                    #region[Get EQP & LINE]
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                    if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                    if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                    #endregion
                    #region [PLCAgent Data Bit]
                    eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                    #endregion
                    #region[If Bit Off->Return]
                    MaterialEntity material = new MaterialEntity();
                    if (bitResult == eBitResult.OFF)
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                        PIMaterialWeightRequestCommand(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                        return;
                    }
                    #endregion
                    #region [PLCAgent Data Word]
                    List<MaterialEntity> materialList = new List<MaterialEntity>();
                    for (int eventItemIndex = 0; eventItemIndex < inputData.EventGroups[0].Events[0].Items.Count; eventItemIndex+=2)
                    {
                        string PIMaterialID = inputData.EventGroups[0].Events[0].Items[eventItemIndex].Value.Trim();
                        string PIMaterialWeight = inputData.EventGroups[0].Events[0].Items[eventItemIndex+1].Value.Trim();
                        if (line.Data.LINEID == "CCPIL100" || line.Data.LINEID == "CCPIL200")
                        { }
                        else
                        {
                            if (string.IsNullOrEmpty(PIMaterialID)) continue;
                        }
                        MaterialEntity materialE = new MaterialEntity();
                        materialE.NodeNo = eqp.Data.NODENO;
                        materialE.MaterialWeight = (int.Parse(PIMaterialWeight)*10).ToString();
                        materialE.MaterialID = PIMaterialID;
                        materialE.UnitNo = "";
                        materialE.MaterialStatus = eMaterialStatus.INUSE;
                        materialE.MaterialRecipeID = "";
                        materialE.UVMaskUseCount = "0";
                        materialE.UseCount = PIMaterialWeight;
                        materialE.MaterialType = "PI";
                        materialList.Add(materialE);
                        ObjectManager.MaterialManager.AddMaterial(materialE);
                    }
                    
                    #endregion
                    PIMaterialWeightRequestCommand(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    #region [Log]
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE =[{2}], PIMaterialWeightRequestCommandReply, BIT (ON)",
                        eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode));
                    #endregion

                    //取出特定幾台機台內任一片玻璃的 Job ID 上報，若則無須上報。
                    IList<Equipment> EQPList = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).
                        Where(e => e.Data.NODENO == "L2" || e.Data.NODENO == "L3" || e.Data.NODENO == "L4").ToList();
                    string productName = ObjectManager.JobManager.GetJobIDbyEQPList(EQPList);
                    //Add By Yangzhenteng For OPI Display 20180904
                    if (Repository.ContainsKey("PIMaterialWeightRequestCommand")) //Add By Yangzhenteng For PIL OPI Display 20180823
                    {
                        Repository.Remove("PIMaterialWeightRequestCommand");
                        object[] _data1 = new object[5]
                                      { 
                                          inputData.TrackKey,  /*0 TrackKey*/
                                          eqp,                 /*1 EQP*/
                                          "" ,                 /*2 materialMode*/
                                          productName,         /*3 panelID*/
                                          materialList         /*5 MaterialList*/
                                      };
                        Invoke(eServiceName.MESService, "MaterialConsumeChangeReport", _data1);
                    }
                    if (line.Data.LINEID == "CCPIL100" || line.Data.LINEID == "CCPIL200")
                    {
                        Invoke(eServiceName.UIService, "MaterialRealWeightReport", new object[] { inputData.TrackKey, materialList });
                    }


                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            private void PiMaterialWeightRequestCommandTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                try
                {
                    UserTimer timer = subjet as UserTimer;
                    string tmp = timer.TimerId;
                    string trackKey = timer.State.ToString();
                    string[] sArray = tmp.Split('_');
                    Repository.Remove("PIMaterialWeightRequestCommand");
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP REPLY,  PIMaterialWeightRequestCommandReply TIMEOUT SET BIT (OFF).", sArray[0], trackKey));

                    PIMaterialWeightRequestCommand(sArray[0], eBitResult.OFF, trackKey);

                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            private void SendPLCData(Trx outputData)
            {
                  xMessage msg = new xMessage();
                  msg.Data = outputData;
                  msg.ToAgent = eAgentName.PLCAgent;
                  PutMessage(msg);
            }
      }
}
