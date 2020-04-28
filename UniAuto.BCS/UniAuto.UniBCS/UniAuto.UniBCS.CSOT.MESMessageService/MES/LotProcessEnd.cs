using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.MesSpec;
using System.Collections;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;


namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public partial class MESService
    {
        /// <summary>
        ///  6.63.	LotProcessEnd       MES MessageSet : BC reports when Lot in EQP was completed and all substrates of the Lot have been stored in to cassette for moving out of the EQP.
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="lineName">LineID</param>
        /// <param name="port">Port Entity</param>
        ///  <param name="cst">Cassette Entity</param>
        /// <param name="jobs">job Entity List</param>
        public void LotProcessEnd(string trxID, Port port, Cassette cst, IList<Job> jobs)
        {
            // (string trxID, string portNo, IList<Job> jobs)
            // 生成的XML要存檔
            try
            {
                bool _ttp = false;
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            //string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " Send MES but OFF LINE LINENAME =[{1}).", trxID, line.Data.LINEID));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessEnd") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];

                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;


                //Watson Modify 20150313 For 福杰、博章 always相信機台，由人員選擇不可以任意修改
                //sort 有多種容量type，無法以db設定為準
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    bodyNode[keyHost.PARTIALFULLFLAG].InnerText = (port.File.PartialFullFlag == eParitalFull.PartialFull ? "Y" : "N");
                    if (line.Data.LINEID.Contains("CUT"))//sy add 2015 12 04
                    {
                        if (port.File.Mode == ePortMode.ReJudge)
                        {
                            bodyNode[keyHost.PARTIALFULLFLAG].InnerText = "Y";
                        }
                    }
                }
                else
                {
                    //Watson Modify 20141125 For 寶哥、登京
                    if (port.Data.MAXCOUNT == jobs.Count)
                        bodyNode[keyHost.PARTIALFULLFLAG].InnerText = "N";
                    else
                        bodyNode[keyHost.PARTIALFULLFLAG].InnerText = (port.File.PartialFullFlag == eParitalFull.PartialFull ? "Y" : "N");
                }

                bodyNode[keyHost.AOIBYPASS].InnerText = cst.MES_CstData.AOIBYPASS;
                bodyNode[keyHost.EXPSAMPLING].InnerText = cst.MES_CstData.EXPSAMPLING;
                bodyNode[keyHost.AUTOCLAVESAMPLING].InnerText = cst.MES_CstData.AUTOCLAVESAMPLING;

                //if (line.Data.LINETYPE == eLineType.CELL.CBPOL_1 || line.Data.LINETYPE == eLineType.CELL.CBPOL_2 || line.Data.LINETYPE == eLineType.CELL.CBPOL_3)
                //    bodyNode[keyHost.AUTOCLAVESKIP].InnerText = port.File.AutoClaveByPass == eBitResult.ON ? "Y" : "N";
                //else
                bodyNode[keyHost.AUTOCLAVESKIP].InnerText = cst.MES_CstData.AUTOCLAVESKIP;

                if (line.Data.LINEID.Contains("CCPOL")) // Add By Yangzhenteng For POL 20190124;
                {
                    bodyNode[keyHost.PRODUCTQUANTITY].InnerText = port.File.JobCountInCassette.ToString();
                }
                else
                {
                    bodyNode[keyHost.PRODUCTQUANTITY].InnerText = jobs.Count().ToString();
                }
                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();

                if (line.Data.FABTYPE == eFabType.CF.ToString())
                {
                    bodyNode[keyHost.CFSHORTCUTMODE].InnerText = (line.File.CFShortCutMode == eShortCutMode.Enable ? "ENABLE" : "DISABLE");
                }

                if (port.Data.NODEID.Contains(keyCELLMachingName.CBBUR) && port.File.Type == ePortType.UnloadingPort)
                    bodyNode[keyHost.NOPROCESSFLAG].InnerText = "Y";
                else
                    bodyNode[keyHost.NOPROCESSFLAG].InnerText = "N";


                if (jobs.Count() > 0)
                {
                    List<Job> result = (from job in jobs orderby int.Parse(job.ToSlotNo) ascending select job).ToList<Job>();
                    if (!string.IsNullOrEmpty(result[0].MES_PPID))
                    { bodyNode[keyHost.PPID].InnerText = result[0].MES_PPID; } //Watson modify 20141212 For 
                    else { bodyNode[keyHost.PPID].InnerText = result[0].PPID; }
                    bodyNode[keyHost.HOSTPPID].InnerText = result[0].MesProduct.PPID;
                }


                XmlNode glsListNode = bodyNode[keyHost.PRODUCTLIST];
                XmlNode glsNode = glsListNode[keyHost.PRODUCT].Clone();
                glsListNode.RemoveAll();

                string cstGrade = string.Empty;//sy add 20160805 for judge change CSTMAPGRADE [BC CR for MES]

                foreach (Job job in jobs)
                {
                    bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;   //Watson modify 20141122 For 帶JOB層的

                    if(line.Data.LINETYPE.Contains(eLineType.CF.FCUPK_TYPE1) ||
                       line.Data.LINETYPE.Contains(eLineType.CF.FCUPK_TYPE2))
                    {
                        bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = job.LineRecipeName;
                    }

                    XmlNode product = glsNode.Clone();

                    product[keyHost.POSITION].InnerText = job.ToSlotNo;
                    product[keyHost.PRODUCTNAME].InnerText = job.GlassChipMaskBlockID;

                    product[keyHost.HOSTPRODUCTNAME].InnerText = job.MesProduct.PRODUCTNAME;
                    product[keyHost.DENSEBOXID].InnerText = job.MesProduct.DENSEBOXID;

                    product[keyHost.PRODUCTJUDGE].InnerText = GetProductJudge(line, fabType, job);

                    #region [CSTMAPGRADE]  CELL SHOP專用:
                    //寫在Product 層，不用再每片判斷
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())
                    {
                        //// POL LINE 直接抓 機台報的結果, 
                        //if (line.Data.LINETYPE == eLineType.CELL.CBPOL_1 || line.Data.LINETYPE == eLineType.CELL.CBPOL_2 || line.Data.LINETYPE == eLineType.CELL.CBPOL_3)
                        //{
                        //    // POL Line 的PLC Map在v1.30版, 在Post Status才有開Mapping Grade的欄位
                        //    bodyNode[keyHost.CSTMAPGRADE].InnerText = port.File.MappingGrade;
                        //}
                        //else  // Other Line 其他LINE要去比對所有的JOB的GRADE是否一致,若一致直接報同樣的GRADE, 若不同報"MIX"  
                        //{
                        if ((bodyNode[keyHost.CSTMAPGRADE].InnerText != job.JobGrade) &&
                            (bodyNode[keyHost.CSTMAPGRADE].InnerText != string.Empty))
                        {
                            bodyNode[keyHost.CSTMAPGRADE].InnerText = "MIX";
                        }
                        else
                        {
                            bodyNode[keyHost.CSTMAPGRADE].InnerText = job.JobGrade;
                        }
                        //}
                    }
                    #endregion
                    else
                    {
                        bodyNode[keyHost.CSTMAPGRADE].InnerText = job.JobGrade;
                    }

                    product[keyHost.PRODUCTGRADE].InnerText = GetProductGrade(line, fabType, job);//CELL 特殊LINE CELL_Product_Special 會再蓋過一次
                    #region [OXR]
                    if (job.ChipCount != 0)//CUT & chip Will update
                    {
                        if (job.JobJudge == "3") //20160711 add by Frank 若RW的玻璃，OXR上報MES原始資訊。
                            product[keyHost.SUBPRODUCTGRADES].InnerText = job.MesProduct.SUBPRODUCTGRADES.PadRight(job.ChipCount).Substring(0, job.MesProduct.SUBPRODUCTGRADES.Length);
                        else
                            product[keyHost.SUBPRODUCTGRADES].InnerText = job.OXRInformation.PadRight(job.ChipCount).Substring(0, job.ChipCount);
                    }
                    else
                        product[keyHost.SUBPRODUCTGRADES].InnerText = job.OXRInformation.PadRight(job.ChipCount).Substring(0, job.MesProduct.SUBPRODUCTGRADES.Length);
                    #endregion

                    product[keyHost.PRODUCTRECIPENAME].InnerText = job.LineRecipeName;
                    product[keyHost.HOSTPRODUCTRECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;
                    product[keyHost.VCRREADFLAG].InnerText = ObjectManager.JobManager.P2M_GetVCRResult(job.VCR_Result);
                     //#region [T2 ODF]
                    //if (line.Data.LINETYPE == eLineType.CELL.CBODF)
                    //    product[keyHost.PAIRPRODUCTNAME].InnerText = job.MesProduct.CFPRODUCTNAME;
                    //#endregion
                    if (job.MesCstBody.LOTLIST.Count != 0)
                    {
                        bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = job.MesCstBody.LOTLIST[0].LINERECIPENAME; //Watson modify 20141122 For 帶JOB層的
                        product[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                        product[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                        product[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                        product[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                        product[keyHost.CFREWORKCOUNT].InnerText = job.MesCstBody.LOTLIST[0].CFREWORKCOUNT;
                    }

                    product[keyHost.PROCESSRESULT].InnerText = GetPROCESSRESULT(line, port, cst, job, out _ttp);

                    #region [Abnormal Code List]
                    XmlNode abromalListNode = product[keyHost.ABNORMALCODELIST];
                    XmlNode codeNode = abromalListNode[keyHost.CODE];
                    abromalListNode.RemoveAll();
                    XmlNode code;

                    switch (fabType)
                    {
                        case eFabType.ARRAY:
                            switch (line.Data.LINETYPE)
                            {
                                case eLineType.ARRAY.TTP_VTEC:
                                    if (_ttp)
                                    {
                                        code = codeNode.Clone();
                                        //code[keyHost.ABNORMALSEQ].InnerText = "TTPFlag";
                                        code[keyHost.ABNORMALVALUE].InnerText = "TTPFlag";
                                        code[keyHost.ABNORMALCODE].InnerText = "Y";
                                        abromalListNode.AppendChild(code);
                                    }
                                    break;
                                case eLineType.ARRAY.PHL_TITLE:
                                case eLineType.ARRAY.PHL_EDGEEXP:
                                    code = codeNode.Clone();
                                    // modify by bruce 2015/09/24  ABNORMALSEQ to ABNORMALVALUE
                                    //code[keyHost.ABNORMALSEQ].InnerText = "SB_HP_NUM";
                                    code[keyHost.ABNORMALVALUE].InnerText = "SB_HP_NUM";
                                    code[keyHost.ABNORMALCODE].InnerText = job.ArraySpecial.DNS_SB_HP_NUM;
                                    abromalListNode.AppendChild(code);
                                    code = codeNode.Clone();
                                    //code[keyHost.ABNORMALSEQ].InnerText = "SB_CP_NUM";
                                    code[keyHost.ABNORMALVALUE].InnerText = "SB_CP_NUM";
                                    code[keyHost.ABNORMALCODE].InnerText = job.ArraySpecial.DNS_SB_CP_NUM;
                                    abromalListNode.AppendChild(code);
                                    code = codeNode.Clone();
                                    //code[keyHost.ABNORMALSEQ].InnerText = "HB_HP_NUM";
                                    code[keyHost.ABNORMALVALUE].InnerText = "HB_HP_NUM";
                                    code[keyHost.ABNORMALCODE].InnerText = job.ArraySpecial.DNS_HB_HP_NUM;
                                    abromalListNode.AppendChild(code);
                                    code = codeNode.Clone();
                                    //code[keyHost.ABNORMALSEQ].InnerText = "VCD_NUM";
                                    code[keyHost.ABNORMALVALUE].InnerText = "VCD_NUM";
                                    code[keyHost.ABNORMALCODE].InnerText = job.ArraySpecial.DNS_VCD_NUM;
                                    abromalListNode.AppendChild(code);
                                    break;
                            }
                            break;
                        case eFabType.CF:
                            #region CF
                            string _side = string.Empty;
                            IList<Unit> Units = ObjectManager.UnitManager.GetUnits();

                            switch (line.Data.JOBDATALINETYPE)
                            {
                                case eJobDataLineType.CF.PHOTO_BMPS:
                                    #region TTPFlag
                                    if (line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1)
                                    {
                                        if (_ttp)
                                        {
                                            code = codeNode.Clone();
                                           
                                            //code[keyHost.ABNORMALSEQ].InnerText = "TTPFlag";
                                            code[keyHost.ABNORMALVALUE].InnerText = "TTPFlag";
                                            code[keyHost.ABNORMALCODE].InnerText = "Y";
                                            abromalListNode.AppendChild(code);
                                        }
                                        else
                                        {
                                            code = codeNode.Clone();
                                            //code[keyHost.ABNORMALSEQ].InnerText = "TTPFlag";
                                            code[keyHost.ABNORMALVALUE].InnerText = "TTPFlag";
                                            code[keyHost.ABNORMALCODE].InnerText = "N";
                                            abromalListNode.AppendChild(code);
                                        }
                                    }
                                    #endregion

                                    #region ALNSIDE
                                    //BM/PS LINE做法待確認 2015/9/6 Frank
                                    _side = string.Empty;
                                    if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.ALNSIDE))
                                    {
                                        if (job.CfSpecial.TrackingData.BMPS_Exposure == "1") { _side = "CP1"; }
                                        if (job.CfSpecial.TrackingData.BMPS_Exposure2 == "1") { _side = "CP2"; }
                                        foreach (Unit unit in Units)
                                        {
                                            if (unit.Data.UNITATTRIBUTE == _side)
                                            {
                                                code = codeNode.Clone();
                                                //code[keyHost.ABNORMALSEQ].InnerText = "ALNSIDE";
                                                code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                                                code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                                abromalListNode.AppendChild(code);
                                                lock (job)
                                                { job.CfSpecial.AbnormalCode.ALNSIDE = unit.Data.UNITID; }
                                                ObjectManager.JobManager.EnqueueSave(job);
                                            }
                                        }
                                        if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.ALNSIDE))
                                        {
                                            code = codeNode.Clone();
                                            //code[keyHost.ABNORMALSEQ].InnerText = "ALNSIDE";
                                            code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                                            code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                            abromalListNode.AppendChild(code);
                                        }
                                    }
                                    else
                                    {
                                        code = codeNode.Clone();
                                        //code[keyHost.ABNORMALSEQ].InnerText = "ALNSIDE";
                                        code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                                        code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                        abromalListNode.AppendChild(code);
                                    }
                                    #endregion

                                    #region OVENSIDE
                                    _side = string.Empty;
                                    if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.OVENSIDE))
                                    {
                                        if (job.CfSpecial.TrackingData.Photo_OvenHP01 == "1") { _side = "OVENHP1"; }
                                        if (job.CfSpecial.TrackingData.Photo_OvenHP02 == "1") { _side = "OVENHP2"; }
                                        foreach (Unit unit in Units)
                                        {
                                            if (unit.Data.UNITATTRIBUTE == _side)
                                            {
                                                code = codeNode.Clone();
                                                //code[keyHost.ABNORMALSEQ].InnerText = "OVENSIDE";
                                                code[keyHost.ABNORMALVALUE].InnerText = "OVENSIDE";
                                                code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                                abromalListNode.AppendChild(code);
                                                lock (job)
                                                { job.CfSpecial.AbnormalCode.OVENSIDE = unit.Data.UNITID; }
                                                ObjectManager.JobManager.EnqueueSave(job);
                                                break;
                                            }                                           
                                        }
                                        if(string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.OVENSIDE))
                                        {
                                            code = codeNode.Clone();
                                            //code[keyHost.ABNORMALSEQ].InnerText = "OVENSIDE";
                                            code[keyHost.ABNORMALVALUE].InnerText = "OVENSIDE";
                                            code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                            abromalListNode.AppendChild(code);
                                        }
                                    }
                                    else
                                    {
                                        code = codeNode.Clone();
                                        //code[keyHost.ABNORMALSEQ].InnerText = "OVENSIDE";
                                        code[keyHost.ABNORMALVALUE].InnerText = "OVENSIDE";
                                        code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                        abromalListNode.AppendChild(code);
                                    }
                                    #endregion

                                    #region VCDSIDE
                                    _side = string.Empty;
                                    if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.VCDSIDE))
                                    {

                                        if (job.CfSpecial.TrackingData.CoaterVCD01 == "1") { _side = "VCD1"; }
                                        if (job.CfSpecial.TrackingData.CoaterVCD02 == "1") { _side = "VCD2"; }
                                        foreach (Unit unit in Units)
                                        {
                                            if (unit.Data.UNITATTRIBUTE == _side)
                                            {
                                                code = codeNode.Clone();
                                                //code[keyHost.ABNORMALSEQ].InnerText = "VCDSIDE";
                                                code[keyHost.ABNORMALVALUE].InnerText = "VCDSIDE";
                                                code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                                abromalListNode.AppendChild(code);
                                                lock (job)
                                                { job.CfSpecial.AbnormalCode.VCDSIDE = unit.Data.UNITID; }
                                                ObjectManager.JobManager.EnqueueSave(job);
                                                break;
                                            }
                                        }
                                        if(string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.VCDSIDE))
                                        {
                                            code = codeNode.Clone();
                                            //code[keyHost.ABNORMALSEQ].InnerText = "VCDSIDE";
                                            code[keyHost.ABNORMALVALUE].InnerText = "VCDSIDE";
                                            code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                            abromalListNode.AppendChild(code);
                                        }
                                    }
                                    else
                                    {
                                        code = codeNode.Clone();
                                        //code[keyHost.ABNORMALSEQ].InnerText = "VCDSIDE";
                                        code[keyHost.ABNORMALVALUE].InnerText = "VCDSIDE";
                                        code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                        abromalListNode.AppendChild(code);
                                    }
                                    #endregion

                                    #region COA2MASKEQPID
                                    code = codeNode.Clone();
                                    //code[keyHost.ABNORMALSEQ].InnerText = "COA2MASKEQPID";
                                    code[keyHost.ABNORMALVALUE].InnerText = "COA2MASKEQPID";
                                    if (job.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID.Equals("0"))
                                        code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                    else
                                        code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID;
                                    abromalListNode.AppendChild(code);
                                    #endregion

                                    #region COA2MASKNAME
                                    code = codeNode.Clone();
                                    //code[keyHost.ABNORMALSEQ].InnerText = "COA2MASKNAME";
                                    code[keyHost.ABNORMALVALUE].InnerText = "COA2MASKNAME";
                                    code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.COA2MASKNAME;
                                    abromalListNode.AppendChild(code);
                                    #endregion
                                    
                                    #region PRLOT
                                    code = codeNode.Clone();
                                    //code[keyHost.ABNORMALSEQ].InnerText = "PRLOT";
                                    code[keyHost.ABNORMALVALUE].InnerText = "PRLOT";
                                    code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.PRLOT;
                                    abromalListNode.AppendChild(code);
                                    #endregion

                                    #region CSPNUMBER
                                    code = codeNode.Clone();
                                    //code[keyHost.ABNORMALSEQ].InnerText = "CSPNUMBER";
                                    code[keyHost.ABNORMALVALUE].InnerText = "CSPNUMBER";
                                    code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.CSPNUMBER;
                                    abromalListNode.AppendChild(code);
                                    #endregion
                                    
                                    #region HPCHAMBER
                                    Unit unit_HP = Units.FirstOrDefault(u => u.Data.UNITATTRIBUTE.Equals("HP"));
                                    if (unit_HP == null) throw new Exception(string.Format("CAN'T FIND UNIT_ATTRIBUTE=[HP] IN DB!"));
                                    code = codeNode.Clone();
                                    //code[keyHost.ABNORMALSEQ].InnerText = "HPCHAMBER";
                                    code[keyHost.ABNORMALVALUE].InnerText = "HPCHAMBER";
                                    if (port.File.Type != ePortType.LoadingPort)
                                        code[keyHost.ABNORMALCODE].InnerText = unit_HP.Data.UNITID.Trim() + job.CfSpecial.CFSpecialReserved.HPSlotNumber.PadLeft(2, '0');
                                    else
                                        code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                    abromalListNode.AppendChild(code);
                                    #endregion

                                    #region CPCHAMBER
                                    Unit unit_CP = Units.FirstOrDefault(u => u.Data.UNITATTRIBUTE.Equals("CP"));
                                    if (unit_CP == null) throw new Exception(string.Format("CAN'T FIND UNIT_ATTRIBUTE=[CP] IN DB!"));
                                    code = codeNode.Clone();
                                    //code[keyHost.ABNORMALSEQ].InnerText = "CPCHAMBER";
                                    code[keyHost.ABNORMALVALUE].InnerText = "CPCHAMBER";
                                    if (port.File.Type != ePortType.LoadingPort)
                                        code[keyHost.ABNORMALCODE].InnerText = unit_CP.Data.UNITID.Trim() + job.CfSpecial.CFSpecialReserved.CPSlotNumber.PadLeft(2, '0');
                                    else
                                        code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                    abromalListNode.AppendChild(code);
                                    #endregion

                                    #region DISPENSESPEED
                                    code = codeNode.Clone();
                                    //code[keyHost.ABNORMALSEQ].InnerText = "DISPENSESPEED";
                                    code[keyHost.ABNORMALVALUE].InnerText = "DISPENSESPEED";
                                    code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.DISPENSESPEED;
                                    abromalListNode.AppendChild(code);
                                    #endregion

                                    break;
                                case eJobDataLineType.CF.PHOTO_GRB:
                                    #region ALNSIDE
                                    _side = string.Empty;
                                    if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.ALNSIDE))
                                    {
                                        if (job.CfSpecial.TrackingData.RGB_ExposureCP01 == "1") { _side = "CP1"; }
                                        if (job.CfSpecial.TrackingData.RGB_ExposureCP02 == "1") { _side = "CP2"; }
                                        foreach (Unit unit in Units)
                                        {
                                            if (unit.Data.UNITATTRIBUTE == _side)
                                            {
                                                code = codeNode.Clone();
                                                //code[keyHost.ABNORMALSEQ].InnerText = "ALNSIDE";
                                                code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                                                code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                                abromalListNode.AppendChild(code);
                                                lock (job)
                                                { job.CfSpecial.AbnormalCode.ALNSIDE = unit.Data.UNITID; }
                                                ObjectManager.JobManager.EnqueueSave(job);
                                            }
                                        }
                                        if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.ALNSIDE))
                                        {
                                            code = codeNode.Clone();
                                            //code[keyHost.ABNORMALSEQ].InnerText = "ALNSIDE";
                                            code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                                            code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                            abromalListNode.AppendChild(code);
                                        }
                                    }
                                    else
                                    {
                                        code = codeNode.Clone();
                                        //code[keyHost.ABNORMALSEQ].InnerText = "ALNSIDE";
                                        code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                                        code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                        abromalListNode.AppendChild(code);
                                    }
                                    #endregion

                                    break;
                                case eJobDataLineType.CF.FCAOI:
                                    //20160810 add by Frank
                                    #region AOIJUDGESCRAP
                                    if (job.JobJudge == "1")    //Judge = "OK"
                                    {
                                        code = codeNode.Clone();
                                        code[keyHost.ABNORMALVALUE].InnerText = "AOIJUDGESCRAP";
                                        code[keyHost.ABNORMALCODE].InnerText = "";
                                        abromalListNode.AppendChild(code);
                                    }

                                    if (job.JobJudge == "2")    //Judge = "NG"
                                    {
                                        if (job.InspJudgedData == new string('0', job.InspJudgedData.Length))
                                        {
                                            code = codeNode.Clone();
                                            code[keyHost.ABNORMALVALUE].InnerText = "AOIJUDGESCRAP";
                                            code[keyHost.ABNORMALCODE].InnerText = "N";
                                            abromalListNode.AppendChild(code);
                                        }
                                        else
                                        {
                                            code = codeNode.Clone();
                                            code[keyHost.ABNORMALVALUE].InnerText = "AOIJUDGESCRAP";
                                            code[keyHost.ABNORMALCODE].InnerText = "Y";
                                            abromalListNode.AppendChild(code);
                                        }
                                    }
                                    #endregion
                                    break;

                                case eJobDataLineType.CF.REWORK:
                                    //add by hujunpeng 20199128
                                    code = codeNode.Clone();
                                    code[keyHost.ABNORMALVALUE].InnerText = "PDRREWORKCOUNT";
                                    code[keyHost.ABNORMALCODE].InnerText = job.CellSpecial.CurrentRwkCount;
                                    abromalListNode.AppendChild(code);
                                    break;
                            }
                            #endregion
                            break;
                        case eFabType.CELL:
                            P2M_DecodeAbnormalCodeFlagBy_CELL(abromalListNode, codeNode, product, line, job);
                            break;
                    }

                    #endregion

                    #region Hold 
                    // 當 Over Q Time 或者 VCR Mismatch 發生時，HoldInforList 就會有紀錄。
                    if (job.HoldInforList.Count() > 0)
                    {
                        if (job.HoldInforList.Count() == 1 && job.TrackingDataBypassHoldFlag)
                            product[keyHost.HOLDFLAG].InnerText = "N";
                        else
                            product[keyHost.HOLDFLAG].InnerText = "Y";

                        string holdmachine = string.Empty;
                        string holdoperater = string.Empty;
                        string holdcomment = string.Empty;  // For T3 Hold Commnet

                        for (int i = 0; i < job.HoldInforList.Count(); i++)
                        {
                            if (!string.IsNullOrEmpty(holdmachine)) holdmachine += ";";
                            holdmachine += string.Format("{0}_{1}", job.HoldInforList[i].NodeID, job.HoldInforList[i].HoldReason);

                            if (!string.IsNullOrEmpty(holdoperater)) holdoperater += ";";
                            holdoperater += job.HoldInforList[i].OperatorID;


                            //// For T3 Hold Commnet
                            if (!string.IsNullOrEmpty(holdcomment)) holdcomment += ";";
                            holdcomment += job.HoldInforList[i].HoldReason;

                        }

                        product[keyHost.HOLDCOMMENT].InnerText = holdcomment;    // For T3 Hold Commnet

                        product[keyHost.HOLDMACHINE].InnerText = holdmachine;
                        product[keyHost.HOLDOPERATOR].InnerText = holdoperater;
                    }
                    #endregion

                    //目前不使用
                    XmlNode psheighList = product[keyHost.PSHEIGHTLIST];
                    XmlNode psNodeClone = psheighList[keyHost.SITEVALUE].Clone();

                    psheighList.RemoveAll();
                    //<PSHEIGHTLIST>
                    //<SITEVALUE></SITEVALUE>   //No Download
                    //</PSHEIGHTLIST>

                    product[keyHost.DUMUSEDCOUNT].InnerText = job.MesProduct.DUMUSEDCOUNT;
                    product[keyHost.CFTYPE1REPAIRCOUNT].InnerText = job.MesProduct.CFTYPE1REPAIRCOUNT;
                    product[keyHost.CFTYPE2REPAIRCOUNT].InnerText = job.MesProduct.CFTYPE2REPAIRCOUNT;
                    product[keyHost.CARBONREPAIRCOUNT].InnerText = job.MesProduct.CARBONREPAIRCOUNT;
                    product[keyHost.LASERREPAIRCOUNT].InnerText = ""; // MES會計算, 不用填
                    //product[keyHost.SHORTCUTFLAG].InnerText = GetShortCutFlag(line, job.CellSpecial.CuttingFlag); - 搬到EQP Flag拆解



                    //ODF Special   SAMPLEFLAG
                    if (line.Data.LINETYPE.Contains(eLineType.CELL.CBODF))
                    {
                        //CELL SPECIAL
                        //product[keyHost.SAMPLEFLAG].InnerText = ""; - 搬到EQP Flag拆解
                    }
                    else
                    {
                        product[keyHost.SAMPLEFLAG].InnerText = "N";
                    }

                    if (line.Data.LINETYPE == eLineType.ARRAY.CVD_ULVAC ||
                        line.Data.LINETYPE == eLineType.ARRAY.CVD_AKT ||
                        line.Data.LINETYPE == eLineType.ARRAY.DRY_YAC ||
                        line.Data.LINETYPE == eLineType.ARRAY.DRY_ICD ||
                        line.Data.LINETYPE == eLineType.ARRAY.DRY_TEL ||
                        line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC ||
                        line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC
                        )
                    {
                        product[keyHost.CHAMBERNAME].InnerText = job.ChamberName;
                    }

                    //TODO: 玻璃為Mask ID的才需要填                    
                    product[keyHost.MASKNAME].InnerText = job.ArraySpecial.ExposureMaskID;

                    if (line.Data.LINETYPE == eLineType.CF.FCMSK_TYPE1)
                    {
                        //CF Mack Cleaner Line 須把機台上報的 Mask ID 轉填過來.
                        product[keyHost.MASKNAME].InnerText = job.GlassChipMaskBlockID;
                    }

                    if (line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCOPH_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCSPH_TYPE1)
                    {
                        //CF Photo Line 上報ALIGNER側別 UnitID.
                        product[keyHost.CHAMBERNAME].InnerText = job.CfSpecial.AbnormalCode.ALNSIDE;
                        //CF Photo Line Aligner - 此片玻璃若是有做曝光的話，則把PROCESSOPERATIONNAME轉填到EXPOSUREDOPERATION.
                        if (job.CfSpecial.EQPFlag2.ExposureProcessFlag.Equals("1"))
                            product[keyHost.EXPOSUREDOPERATION].InnerText = job.CfSpecial.ExposureDoperation;
                        //CF Photo Line Aligner - 從機台上報Process Data中的 Mask ID 轉填過來.
                        product[keyHost.MASKNAME].InnerText = job.CfSpecial.MaskID;
                    }

                    XmlNode materialList = product[keyHost.MATERIALLIST];
                    XmlNode materialClone = materialList[keyHost.MATERIAL].Clone();
                    materialList.RemoveAll();

                    Port sourcePort = ObjectManager.PortManager.GetPortByLineIDPortID(line.Data.LINEID,job.SourcePortID);
                    if (sourcePort != null)
                    {
                        switch (sourcePort.Data.PORTATTRIBUTE.Trim())
                        {
                            case keyCELLPORTAtt.DENSE:
                            case keyCELLPORTAtt.BOX: product[keyHost.SOURCEDURABLETYPE].InnerText = "B"; break;
                            default: product[keyHost.SOURCEDURABLETYPE].InnerText = "C"; break;
                        }
                    }
                    else
                    {
                        NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                             string.Format("CSTSEQ =[{0}], SLOTNO =[{1}], GLASSID =[{2}], can't find source port, port id =[{3}]",
                             job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID, job.SourcePortID));
                        product[keyHost.SOURCEDURABLETYPE].InnerText = "C";
                    }

                    //Download by MES CFShortCutGlassProcessEndReply, and report by CF Shortcut CST ULD TKOUT, 
                    //then MES will execute auto sampling by this value.
                    product[keyHost.SAMPLETYPE].InnerText = job.CfSpecial.SamplingValue;

                    //1.	Used count of material
                    //2.	Used  count  of  CF  ITO  Dummy
                    //3.	Used  count  of  Cell  UV  Mask
                    /*if (line.Data.LINETYPE == eLineType.CF.FCITO_TYPE1)
                    {
                        //ITO Line 用法： 把 DummyUsedCount 轉填過來.
                        product[keyHost.USEDCOUNT].InnerText = job.CfSpecial.DummyUsedCount;

                        //ITO Line 用法： BC 解析tracking data，sputter#1 上報1A,Sputter#2上報1B. 在Lot process end時上報給MES.
                        if (job.CfSpecial.TrackingData.Sputter1 == "1")
                        { product[keyHost.ITOSIDEFLAG].InnerText = "1A"; }
                        else if (job.CfSpecial.TrackingData.Sputter2 == "1")
                        { product[keyHost.ITOSIDEFLAG].InnerText = "1B"; }

                    }
                     */
                    if (line.Data.LINETYPE == eLineType.CELL.CBODF)
                    {
                        //CELL - Finish
                        product[keyHost.USEDCOUNT].InnerText = job.CellSpecial.UVMaskUseCount;
                    }

                    //PI, ODF, HVA
                    //product[keyHost.CENGFLAG].InnerText = ""; - 搬到EQP Flag拆解

                    product[keyHost.ABNORMALENG].InnerText = ""; //add by bruce 2015/9/21 MES要求上報,先報空值

                    product[keyHost.PROCESSFLAG].InnerText = string.IsNullOrEmpty(job.MesProduct.MESPROCESSFLAG) ? job.MesProduct.PROCESSFLAG : job.MesProduct.MESPROCESSFLAG;//yang

                 //   product[keyHost.PROCESSFLAG].InnerText = job.MesProduct.MESPROCESSFLAG.Length == 0 ? job.MesProduct.PROCESSFLAG : job.MesProduct.MESPROCESSFLAG;    //keep MES validatereply的值

                    product[keyHost.BCPROCESSFLAG].InnerText = job.MesProduct.PROCESSFLAG; // add by yang 2016/11/12 ,MES要求BC将really Process Flag报在这里

                    //Watson Modify For MES Spec 20141125
                    //‘OffLine’ - equipment is in Off-line mode.
                    //‘OnLineRemote’ - equipment is in Online Remote mode.
                    //‘OnLineLocal’ - equipment is in Online Local mode. 
                    if (line.File.HostMode == eHostMode.LOCAL)
                        product[keyHost.PROCESSCOMMUNICATIONSTATE].InnerText = "OnLineLocal";
                    if (line.File.HostMode == eHostMode.OFFLINE)
                        product[keyHost.PROCESSCOMMUNICATIONSTATE].InnerText = "OffLine";
                    if (line.File.HostMode == eHostMode.REMOTE)
                        product[keyHost.PROCESSCOMMUNICATIONSTATE].InnerText = "OnLineRemote";

                    if (!string.IsNullOrEmpty(job.MES_PPID))
                    { product[keyHost.PPID].InnerText = job.MES_PPID; } //Watson Modify 20141128 job.PPID 用於機台，會有跨機台
                    else { product[keyHost.PPID].InnerText = job.PPID; }

                    product[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID ;

                    if (line.Data.FABTYPE == eFabType.ARRAY.ToString())
                    {
                        product[keyHost.ALIGNERNAME].InnerText = job.MesProduct.ALIGNERNAME;
                    }
                    else
                    {
                        product[keyHost.ALIGNERNAME].InnerText = job.CfSpecial.EQPFlag2.ExposureProcessFlag == "1" ? "Y" : "N";
                    }

                    //CF Rework Line 上報實際 Rework 次數，Loader Port只須報0即可,Unloader Port 要依機台實際Rework 次數上報。
                    if (line.Data.LINETYPE == eLineType.CF.FCREW_TYPE1)
                    {
                        product[keyHost.ACTCFREWORKCOUNT].InnerText = job.CfSpecial.ReworkRealCount;
                    }

                    //Jun Add 20141128 For Spec 修改
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())
                    {
                        if (line.Data.LINETYPE == eLineType.CELL.CBCUT_1 || line.Data.LINETYPE == eLineType.CELL.CBCUT_2 || line.Data.LINETYPE == eLineType.CELL.CBCUT_3)
                        {
                            product[keyHost.CROSSLINEFLAG].InnerText = job.CellSpecial.CrossLineFlag;
                        }
                    }

                    //Watson Add 20141220 For MES New Spec
                    //Asir Modify 2014/01/21 陈忠说CBPOL400也要上报RTPFLAG,所有我有加port.Data.LINEID == "CBPOL400"
                    //if (line.Data.SERVERNAME == "CBPOL300" || port.Data.LINEID == "CBPOL400")
                    if (line.Data.LINEID == "CBPOL300" || line.Data.LINEID == "CBPOL400")
                    {
                        if (port.File.Type == ePortType.LoadingPort)
                            product[keyHost.RTPFLAG].InnerText = job.CellSpecial.RTPFlag == string.Empty ? job.MesProduct.RTPFLAG : job.CellSpecial.RTPFlag;
                        else if (port.File.Type == ePortType.UnloadingPort)
                        {
                            if (job.JobProcessFlows.Count > 1 && job.SamplingSlotFlag == "1")
                                product[keyHost.RTPFLAG].InnerText = "Y";
                            else
                                product[keyHost.RTPFLAG].InnerText = "N";
                        }
                    }

                    //EQP Flag Decode By Shot
                    switch (line.Data.FABTYPE)
                    {
                        case "ARRAY": P2M_DecodeEQPFlagBy_ARRAY(product, line, job); break;
                        case "CF": P2M_DecodeEQPFlagBy_CF(product, line, job); break;
                        case "CELL": P2M_DecodeProductFlagBy_CELL(product, line, job); break;
                    }

                    //Special Logic By Shop
                    switch (line.Data.FABTYPE)
                    {
                        case "ARRAY": ARRAY_Product_Special(ref product, line, job); break;
                        case "CF": CF_Product_Special(ref product, line, job); break;
                        case "CELL": CELL_Product_Special(ref bodyNode, ref product, line, job, port, ref cstGrade); break;
                    }

                    #region  判断CUT/POL 线UnLoader CSTMAPGRADE 是否为 NG or S0，然后写RANDOMFLAG值  by zhuxingxing
                    string sCSTMAPGRADE = bodyNode[keyHost.CSTMAPGRADE].InnerText;
                    if (line.Data.FABTYPE == eFabType.CELL.ToString() && port.File.Type == ePortType.UnloadingPort)
                    {
                        if (line.Data.LINEID.Contains("CUT") || line.Data.LINEID.Contains("POL"))
                        {
                            if (sCSTMAPGRADE == "NG" || sCSTMAPGRADE == "S0")
                            {
                                bodyNode[keyHost.RANDOMFLAG].InnerText = "Y";
                            }
                            else
                            {
                                bodyNode[keyHost.RANDOMFLAG].InnerText = "N";
                            }
                        }
                    }
                    #endregion




                    glsListNode.AppendChild(product);
                }

                #region Save File 移至Send之前，避免MES Reply太快，檔案還沒存完
                if (port.File.Type == ePortType.LoadingPort)
                {
                    if (ParameterManager["MESSYNCREPORT"].GetBoolean())// 判断是否需要同步上报MES Change Line Target CST 是Loading Port会和Unload 一起上报 20150826 Tom
                        SendToQueue(xml_doc);
                    else
                        SendToMES(xml_doc);
                }
                else
                {
                    // 將檔案存檔 後再送
                    string err;
                    if (!ObjectManager.CassetteManager.FileSaveToLotEndExecute(port.Data.PORTID, port.File.CassetteID, trxID, xml_doc, out err))
                    {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", err);
                    }
                    if (ParameterManager["MESSYNCREPORT"].GetBoolean())// 判断是否需要同步上报MES
                        SendToQueue(xml_doc);
                    else
                        SendToMES(xml_doc);
                    
                }
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [BCS -> MES][{0}] " + MethodBase.GetCurrentMethod().Name + " OK LINENAME =[{1}).",
                        trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.64.	LotProcessEndReply      MES MessagetSet : Lot Process End Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_LotProcessEndReply(XmlDocument xmlDoc)
        {
            try
            {
                #region Delay Send LotProcessEnd
                try
                {
                    //if (_processEndQueue.Count > 0)
                    //{
                    //    xMessage msg = null;
                    //    bool done = _processEndQueue.TryDequeue(out msg);
                    //    if (done)
                    //    {
                    //        lock (_timeSync)
                    //            _processEndLastReportTime = DateTime.Now;
                    //        PutMessage(msg);
                    //    }
                    //}
                    lock (_timeSync)    // modify by bruce 20160413 for T2 Issue
                        _processEndMESReplyFlag = true;
                }
                catch (Exception ex)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",ex);
                }
                #endregion
                
                string returnCode = GetMESReturnCode(xmlDoc).Trim();
                string returnMessage = GetMESReturnMessage(xmlDoc).Trim();
                string lineName = GetLineName(xmlDoc).Trim(); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc).Trim();

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LineName={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}] mismatch [{0}].", ServerName, trxID, lineName));
                }

                string portid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText.Trim();
                string cstid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME].InnerText.Trim();

                string doublerunflag = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.DOUBLERUNFLAG].InnerText.Trim();   //modify by bruce 20160124 for Array CVD line use
                if (xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.DOUBLERUNFLAG] != null)
                    if (xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.DOUBLERUNFLAG].InnerText != null)
                        doublerunflag = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.DOUBLERUNFLAG].InnerText.Trim();//Add For T3 Array

                string desc = string.Empty;


                if (ObjectManager.PortManager.GetPortByLineIDPortID(lineName,portid).File.Type != ePortType.LoadingPort)
                {
                    if (!ObjectManager.CassetteManager.UpdateLotPorcessEndMesReplyToExecuteXmlAndDB(portid, cstid, trxID, returnCode == "0" ? "OK" : "NG", returnCode, returnMessage, out desc))
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[BCS <- MES][{0}] LotProcessEndReply.UpdateLotPorcessEndMesReplyToExecuteXmlAndDB  NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                                       trxID, lineName, portid, cstid, returnCode, returnMessage, desc));
                }

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] LotProcessEndReply  NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5}.",
                                        trxID, lineName, portid, cstid, returnCode, returnMessage));

                    if (ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portid).File.Type != ePortType.LoadingPort)
                    {
                        if (!ObjectManager.CassetteManager.FileMoveToIncompleteCST(portid, cstid, trxID, out desc))
                            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[BCS <- MES][{0}] LotProcessEndReply.FileMoveToIncompleteCST  NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                                            trxID, lineName, portid, cstid, returnCode, returnMessage, desc));
                    }
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[BCS <- MES][{0}] LotProcessEndReply  OK LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5}.",
                                        trxID, lineName, portid, cstid, returnCode, returnMessage));

                    if (ObjectManager.PortManager.GetPortByLineIDPortID(lineName,portid).File.Type != ePortType.LoadingPort)
                    {
                        if (!ObjectManager.CassetteManager.FileMoveToCompleteCST(portid, cstid, trxID, out desc))
                            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                               string.Format("[BCS <- MES][{0}] LotProcessEndReply.FileMoveToCompleteCST NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                                           trxID, lineName, portid, cstid, returnCode, returnMessage, desc));
                    }
                }

                //object[] _data = new object[5]
                //{ 
                //    trxID,                           /*1 trackKey */
                //    lineName,
                //    cstid,
                //    portid,
                //    doublerunflag
                //};

                string timeoutName = string.Format("{0}_MES_LotProcessEndReply", lineName);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                // add by bruce 20160124 Array CVD Double run use
                Line line = ObjectManager.LineManager.GetLine(lineName);
                string nodeno = ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portid).Data.NODENO;
                
                if (line.Data.LINETYPE==eLineType.ARRAY.CVD_ULVAC || line.Data.LINETYPE==eLineType.ARRAY.CVD_AKT)
                {
                    if (doublerunflag=="Y")
                        Invoke(eServiceName.CassetteService,"CassetteDoubleRun",new object[]{nodeno, portid });
                    else
                        Invoke(eServiceName.CassetteService,"CassetteUnload",new object[]{nodeno, portid});
                }

                //Invoke(eServiceName.DenseBoxService, "DenseBoxLabelInformationRequestReply", _data);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public string GetPROCESSRESULT(Line line, Port port, Cassette cst, Job job, out bool TTPFlag)
        {
            TTPFlag = false;
            try
            {
                eFabType fabtype;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabtype);
                bool isM = false; //for partial abnormal process 2016/06/23 cc.kuang

                switch (fabtype)
                {
                    case eFabType.ARRAY:
                        #region [ARRAY]
                        //TODO: ARRAY  新版的值會有M的部份
                        {
                            /*
                                1,PHL,nikon Line 进机台就上报Y.其它主制程Line需要看机台tracking data上报
                                   Y(normal Process),
                                   N(No process/process skip)
                                   M(abnormal process).
                                2，主制程Line 根据机台tracking data上报Y,N,M.
                                3，量测Line同CF 量测Line.
                                4,Sorter,changer同CF.
                                */
                            string trackData = "00";
                            int QueryMainEqpTrackingData = 0;
                            if (ParameterManager.ContainsKey(eArrayMainEqpTrackingDataCheck.LINETYPELIST))
                            {
                                string[] linetypelist = ParameterManager[eArrayMainEqpTrackingDataCheck.LINETYPELIST].GetString().Split(',');
                                QueryMainEqpTrackingData = linetypelist.Where(p => p.ToString() == line.Data.LINETYPE).ToList().Count();
                            }

                            if (QueryMainEqpTrackingData > 0)  // 有設定的line 照原本Main Process Eqp check Tracking Data
                            {
                                HoldInfo hd = new HoldInfo();
                                switch (line.Data.LINETYPE)
                                {
                                    //case eLineType.ARRAY.SRT:
                                    case eLineType.ARRAY.CHN_SEEC:
                                        if (job.TargetPortID != "" && job.TargetPortID != "0" && (job.SourcePortID != job.TargetPortID)) return "Y";
                                                                                
                                        hd.NodeID = line.Data.LINEID; // add 2016/05/30 cc.kuang
                                        hd.OperatorID = "BCAuto";
                                        hd.HoldReason = "Tracking data is abnormal";
                                        job.HoldInforList.Add(hd);
                                        job.TrackingDataBypassHoldFlag = true;
                                        return "N";
                                    //case eLineType.ARRAY.PHOTO:
                                    case eLineType.ARRAY.PHL_TITLE:
                                    case eLineType.ARRAY.PHL_EDGEEXP:
                                        int x = (int)Convert.ToSingle(job.ArraySpecial.DNS_LCCTCtPrsAve);//add by qiumin 201711227
                                        if (job.ArraySpecial.ProcessType.Trim().Equals("0"))  //add by qiumin 201711227
                                        {
                                            if (job.SamplingSlotFlag.Equals("1") && !job.TrackingData.Equals(new string('0', 32))) return "Y";
                                            if (job.ArraySpecial.PhotoIsProcessed) return "Y";

                                            hd.NodeID = line.Data.LINEID; // add 2016/05/30 cc.kuang
                                            hd.OperatorID = "BCAuto";
                                            hd.HoldReason = "Tracking data is abnormal";
                                            job.HoldInforList.Add(hd);
                                            job.TrackingDataBypassHoldFlag = true;
                                            return "N";
                                        }
                                        else
                                        {
                                            if (job.SamplingSlotFlag.Equals("1") && !job.TrackingData.Equals(new string('0', 32))&&x>0) return "Y";
                                            if (job.ArraySpecial.PhotoIsProcessed&&x>0) return "Y";
                                            if (job.SamplingSlotFlag.Equals("1") && !job.TrackingData.Equals(new string('0', 32)) && x == 0) return "M";
                                            //Add By Yangzhenteng 20191107 For PHL_EDGEEXP
                                            if (job.ArraySpecial.ProcessDataNGFlag.Equals("1")) return "N";

                                            hd.NodeID = line.Data.LINEID; // add 2016/05/30 cc.kuang
                                            hd.OperatorID = "BCAuto";
                                            hd.HoldReason = "Tracking data is abnormal";
                                            job.HoldInforList.Add(hd);
                                            job.TrackingDataBypassHoldFlag = true;
                                            return "N";

                                        }
                                    case eLineType.ARRAY.WET_DMS:
                                    case eLineType.ARRAY.WEI_DMS:
                                    case eLineType.ARRAY.STR_DMS:
                                    case eLineType.ARRAY.CLN_DMS:  //add by qiumin 20171222
                                        trackData = job.TrackingData.Substring(0, 2);
                                        trackData = UtilityMethod.Reverse(trackData);

                                        switch (System.Convert.ToInt32(trackData, 2))
                                        {
                                            case 1: return "Y";
                                            case 2: 
                                                hd.NodeID = line.Data.LINEID; // add 2016/06/23 cc.kuang
                                                hd.OperatorID = "BCAuto";
                                                hd.HoldReason = "Tracking data is abnormal";
                                                job.HoldInforList.Add(hd);
                                                job.TrackingDataBypassHoldFlag = true;
                                                return "M";
                                            default: 
                                                hd.NodeID = line.Data.LINEID; // add 2016/05/30 cc.kuang
                                                hd.OperatorID = "BCAuto";
                                                hd.HoldReason = "Tracking data is abnormal";
                                                job.HoldInforList.Add(hd);
                                                job.TrackingDataBypassHoldFlag = true;
                                                return "N";
                                        }
                                    case eLineType.ARRAY.CVD_AKT:
                                    case eLineType.ARRAY.CVD_ULVAC:
                                        if (job.ArraySpecial.GlassFlowType == "1")
                                        {
                                            //取得Cleaner的Tracking Data Value
                                            trackData = job.TrackingData.Substring(0, 2);
                                        }
                                        else
                                        {
                                            //取得主製程設備的Chamber Tracking Data Value
                                            for (int i = 10; i <= 20; i = i + 2)
                                            {
                                                trackData = job.TrackingData.Substring(i, 2);
                                                if (System.Convert.ToInt32(trackData, 2) > 0)
                                                    break;
                                            }
                                        }

                                        trackData = UtilityMethod.Reverse(trackData);
                                        switch (System.Convert.ToInt32(trackData, 2))
                                        {
                                            case 1: return "Y";
                                            case 2: 
                                                hd.NodeID = line.Data.LINEID; // add 2016/06/23 cc.kuang
                                                hd.OperatorID = "BCAuto";
                                                hd.HoldReason = "Tracking data is abnormal";
                                                job.HoldInforList.Add(hd);
                                                job.TrackingDataBypassHoldFlag = true;
                                                return "M";
                                            default: 
                                                hd.NodeID = line.Data.LINEID; // add 2016/05/30 cc.kuang
                                                hd.OperatorID = "BCAuto";
                                                hd.HoldReason = "Tracking data is abnormal";
                                                job.HoldInforList.Add(hd);
                                                job.TrackingDataBypassHoldFlag = true;
                                                return "N";
                                        }
                                    case eLineType.ARRAY.MSP_ULVAC:
                                    case eLineType.ARRAY.ITO_ULVAC:
                                    case eLineType.ARRAY.DRY_ICD:
                                    case eLineType.ARRAY.DRY_YAC:
                                    case eLineType.ARRAY.DRY_TEL:
                                        if (job.ArraySpecial.GlassFlowType == "1")
                                        {
                                            //取得Cleaner的Tracking Data Value
                                            trackData = job.TrackingData.Substring(0, 2);
                                        }
                                        else
                                        {
                                            //取得主製程設備的Tracking Data Value
                                            trackData = job.TrackingData.Substring(2, 2);
                                        }

                                        trackData = UtilityMethod.Reverse(trackData);
                                        switch (System.Convert.ToInt32(trackData, 2))
                                        {
                                            case 1: return "Y";
                                            case 2:
                                                hd.NodeID = line.Data.LINEID; // add 2016/06/23 cc.kuang
                                                hd.OperatorID = "BCAuto";
                                                hd.HoldReason = "Tracking data is abnormal";
                                                job.HoldInforList.Add(hd);
                                                job.TrackingDataBypassHoldFlag = true;
                                                return "M";
                                            default:
                                                hd.NodeID = line.Data.LINEID; // add 2016/05/30 cc.kuang
                                                hd.OperatorID = "BCAuto";
                                                hd.HoldReason = "Tracking data is abnormal";
                                                job.HoldInforList.Add(hd);
                                                job.TrackingDataBypassHoldFlag = true;
                                                return "N";
                                        }
                                    case eLineType.ARRAY.TTP_VTEC:
                                        {
                                            //TrackingData
                                            IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");

                                            if (trackings.Count() > 0)
                                            {
                                                if (trackings.ElementAt(0).Value.Equals("1"))
                                                {
                                                    TTPFlag = true;
                                                    return "Y";
                                                }
                                            }

                                            hd.NodeID = line.Data.LINEID; // add 2016/05/30 cc.kuang
                                            hd.OperatorID = "BCAuto";
                                            hd.HoldReason = "Tracking data is abnormal";
                                            job.HoldInforList.Add(hd);
                                            job.TrackingDataBypassHoldFlag = true;
                                            return "N";
                                        }
                                    default:
                                        {
                                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                                            {
                                                if (job.TargetPortID != "" && job.TargetPortID != "0" && (job.SourcePortID != job.TargetPortID)) return "Y";
                                            }
                                            else
                                            {
                                                //TrackingData
                                                IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                                                foreach (string v in trackings.Values)
                                                {
                                                    //if (v.Equals("1") || v.Equals("2")) return "Y"; 2016/05/30 mark cc.kuang
                                                    if (v.Equals("1")) return "Y";
                                                    /* not use M 2016/06/24 cc.kuang
                                                    if (v.Equals("2"))
                                                    {
                                                        hd.NodeID = line.Data.LINEID; // add 2016/06/23 cc.kuang
                                                        hd.OperatorID = "BCAuto";
                                                        hd.HoldReason = "Tracking data is abnormal";
                                                        job.HoldInforList.Add(hd);
                                                        job.TrackingDataBypassHoldFlag = true;
                                                        return "M";
                                                    }
                                                    */
                                                }
                                            }

                                            hd.NodeID = line.Data.LINEID; // add 2016/05/30 cc.kuang
                                            hd.OperatorID = "BCAuto";
                                            hd.HoldReason = "Tracking data is abnormal";
                                            job.HoldInforList.Add(hd);
                                            job.TrackingDataBypassHoldFlag = true;
                                            return "N";
                                        }
                                }
                            }
                            else
                            {
                                bool isAbnormalProcess = false;
                                string nodeno = string.Empty;
                                bool isELANormalProcess = false;
                                bool isELAAbnormalProcess=false;
                                if (job.SamplingSlotFlag == "1")    // add by bruce 20160330 只處理有取出的 Glass
                                {
                                    //TrackingData
                                    IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                                    IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                                    foreach (string v in trackings.Keys)
                                    {
                                        bool isEqInfo = false;
                                        bool isRecipebyPassEQ = false;  // modify by bruce 20160411 fix bug
                                        for (int i = 0; i < eqps.Count(); i++)
                                        {
                                            if (v.IndexOf(eqps[i].Data.NODENAME) >= 0)
                                            {
                                                isEqInfo = true;
                                                nodeno = eqps[i].Data.NODENO;
                                                if (!string.IsNullOrEmpty(job.PPID))
                                                {
                                                    string eqrecipe = new string('0', eqps[i].Data.RECIPELEN);

                                                    if (job.PPID.Substring(eqps[i].Data.RECIPEIDX, eqps[i].Data.RECIPELEN).Equals(eqrecipe))
                                                    {
                                                        isRecipebyPassEQ = true;
                                                    }
                                                }
                                            }
                                        }

                                        if (!isRecipebyPassEQ && isEqInfo)  //非recipe by pass ,且是機台資訊的才處理
                                        {
                                            if (!trackings[v].Equals("1"))
                                            {
                                                if (trackings[v].Equals("2")) //2016/06/23 cc.kuang
                                                    isM = true;

                                                HoldInfo hd = new HoldInfo();
                                                switch (line.Data.LINETYPE)
                                                {
                                                    case eLineType.ARRAY.ELA_JSW:
                                                        if (v.IndexOf("ELA") >= 0) 
                                                        {
                                                            isELAAbnormalProcess=true;  //先記錄起來
                                                        }
                                                        else
                                                        {
                                                            hd.NodeID = line.Data.LINEID; // add 2016/05/27 cc.kuang
                                                            hd.OperatorID = "BCAuto";
                                                            hd.HoldReason = nodeno + " tracking data is abnormal";
                                                            job.HoldInforList.Add(hd);
                                                            job.TrackingDataBypassHoldFlag = true;
                                                            isAbnormalProcess = true;
                                                        }
                                                        break ;
                                                    default :
                                                        hd.NodeID = line.Data.LINEID; // add 2016/05/27 cc.kuang
                                                        hd.OperatorID = "BCAuto";
                                                        hd.HoldReason = nodeno + " tracking data is abnormal";
                                                        job.HoldInforList.Add(hd);
                                                        job.TrackingDataBypassHoldFlag = true;
                                                        isAbnormalProcess = true;
                                                        break ;
                                                }
                                            }
                                            else
                                            {
                                                switch (line.Data.LINETYPE)
                                                {
                                                    case eLineType.ARRAY.ELA_JSW:
                                                        if (v.IndexOf("ELA") >= 0) isELANormalProcess = true;
                                                        break;
                                                }
                                            }

                                        }
                                    }

                                    if (!isELANormalProcess && isELAAbnormalProcess)
                                    {
                                        HoldInfo _hd = new HoldInfo();
                                        _hd.NodeID = line.Data.LINEID; // add 2016/05/27 cc.kuang
                                        _hd.OperatorID = "BCAuto";
                                        _hd.HoldReason = "ELA tracking data is abnormal";
                                        job.HoldInforList.Add(_hd);
                                        job.TrackingDataBypassHoldFlag = true;
                                        isAbnormalProcess = true;
                                    }
                                }
                                else
                                {
                                    isAbnormalProcess = true;
                                }

                                if (isAbnormalProcess)
                                {
                                    return "N";
                                    /* not use M 2016/06/24 cc.kuang
                                    if (isM)
                                        return "M";
                                    else
                                        return "N";
                                    */
                                }
                                else
                                {
                                    return "Y";
                                }
                            }
                        }
                        #endregion
                    case eFabType.CF:
                        #region [CF]
                        //TODO: CF
                        {
                            /*
                              + 1.CF ITO Line ITO Cleaner Mode时，即时未做Sputter 制程,BC 需要上报Y.
                              + 2.Sorter ,Changer Mode:如果PORT TYPE 是UnloadingPort ,退PORT時,即全为Y.
                              + 3.量测Line: 
                                  (1)Process Flag(MES Download)为Y,tracking data为0,则process result上报为N.
                                  (2)Process Flag(MES Download)为N,tracking data为0,则Process Result上报给N.
                                  (3)Process Flag(MES Download)为N,tracking data为1,则process result上报Y.
                              + 4.Photo Line Unloader :
                                  (1)Judge Value为NG,RW的glass,上报Process Result为Y. 
                                  (2)Judge Value为OK 的glass,如果Aligner Process Flag(eqp flag)为1的glass,上报Process Result为Y.(反之 0 -> N)
                              + 5.Loader: Process Flag 为N 的glass，Process Result也为N.
                              + 6.Rework Line:如果Process Flag为Y,如果实际rework次数<CF Rework max count且>1, 则上报Process Result为M.
                                  如果CF 实际rework次数为0,则上报Process Result为N. 如果实际rework次数=CF Rework max count，则上报Y.
                              + 7.UPK Line 的 Unloader 機台
                                  (1)Equipment Run Mode為Normal時一律都報Y.
                                  (2)Equipment Run Mode為Re-Clean時由TrackingData來判斷.                  
                             */

                            switch (line.Data.LINETYPE)
                            {
                                case eLineType.CF.FCMPH_TYPE1:
                                case eLineType.CF.FCRPH_TYPE1:
                                case eLineType.CF.FCGPH_TYPE1:
                                case eLineType.CF.FCBPH_TYPE1:
                                case eLineType.CF.FCOPH_TYPE1:
                                case eLineType.CF.FCSPH_TYPE1:
                                    #region Photo Rule

                                    switch (job.JobType)
	                                {
                                        case eJobType.TFT:
                                        case eJobType.CF:
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                    if (port.File.PartialFullFlag == eParitalFull.PartialFull)
                                                    {
                                                        if (cst.CassetteSequenceNo == job.JobSequenceNo)
                                                        {
                                                            //Add by Kasim in 20150526 
                                                            return "N";
                                                        }
                                                    }
                                                    // 20160626 Add by Frank
                                                    // CF Photo Line 只要是做Back Up "OC"，PROCESSRESULT皆Return "Y"。
                                                    if (job.CFProcessBackUp.Equals("00001000"))
                                                    {
                                                        return "Y";
                                                    }
                                                    else
                                                    {

                                                        switch (job.JobJudge)
                                                        {
                                                            case "1": //OK
                                                            case "4": //PD
                                                            case "5": //RP
                                                                if (job.CfSpecial.EQPFlag2.ExposureProcessFlag.Equals("1"))
                                                                    return "Y";
                                                                else
                                                                    return "N";
                                                            case "2": //NG
                                                            case "3": //RW
                                                            case "6": //IR
                                                                return "Y";
                                                        }
                                                        Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                            string.Format("CAN'T FIND JOB JUDGE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                            job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                                        return "N";
                                                    }

                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                        case eJobType.DM:
                                            switch (port.File.Type)
	                                       {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                case ePortType.BothPort:
                                                    return "Y";
	                                        }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                    }
                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T FIND JOB TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                    return "N";

                                    #endregion
                                case eLineType.CF.FCREW_TYPE1:
                                    #region Rework Rule

                                    switch (job.JobType)
                                    {
                                        case eJobType.TFT:
                                        case eJobType.CF:
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                case ePortType.BothPort:
                                                    if (port.File.PartialFullFlag == eParitalFull.PartialFull && port.File.Type == ePortType.UnloadingPort)
                                                    {
                                                        //if (cst.CassetteSequenceNo == job.JobSequenceNo)
                                                        if (cst.CassetteSequenceNo == job.CassetteSequenceNo) //2016/07/06 cc.kuang
                                                        {
                                                            //Add by Kasim in 20150526 
                                                            return "N";
                                                        }
                                                    }
                                                
                                                    //if (int.Parse(job.CfSpecial.ReworkRealCount) > 1 && int.Parse(job.CfSpecial.ReworkRealCount) < int.Parse(job.CfSpecial.ReworkMaxCount)) return "M"; 2016/07/06 cc.kuang modify
                                                    //else if (int.Parse(job.CfSpecial.ReworkRealCount) == int.Parse(job.CfSpecial.ReworkMaxCount)) return "Y"; 2016/07/06 cc.kuang modify
                                                    if (int.Parse(job.CfSpecial.ReworkRealCount) > 0 && int.Parse(job.CfSpecial.ReworkRealCount) < int.Parse(job.CfSpecial.ReworkMaxCount)) return "M";
                                                    else if (job.CfSpecial.ReworkRealCount.Equals("0")) return "N";
                                                    else if (int.Parse(job.CfSpecial.ReworkRealCount) == int.Parse(job.CfSpecial.ReworkMaxCount)) return "Y";

                                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                        string.Format("CHECK REWORK COUNT,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}],ReworkMaxCount:[{6}],ReworkRealCount:[{7}]",
                                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag, job.CfSpecial.ReworkMaxCount, job.CfSpecial.ReworkRealCount));
                                                    return "N";

                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                        case eJobType.DM:
                                        case eJobType.ITO:   //add by qiumin 20180711 for cell glass to CF Rework
                                        case eJobType.METAL1:
                                        case eJobType.NIP:
                                        case eJobType.TK:
                                        case eJobType.TR:
                                        
                                      
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                case ePortType.BothPort:
                                                    return "Y";
                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                    }
                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T FIND JOB TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                    return "N";

                                    #endregion
                                case eLineType.CF.FCSRT_TYPE1:
                                    #region Sorter Rule

                                    switch (job.JobType)
                                    {
                                        case eJobType.TFT:
                                        case eJobType.CF:
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                    if (port.File.PartialFullFlag == eParitalFull.PartialFull)
                                                    {
                                                        //if (cst.CassetteSequenceNo == job.JobSequenceNo)
                                                        if (cst.CassetteSequenceNo == job.CassetteSequenceNo) // T2,T3 check modify 2016/04/22 cc.kuang
                                                        {
                                                            //Add by Kasim in 20150526 
                                                            return "N";
                                                        }
                                                    }
                                                    return "Y";
                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                        case eJobType.DM:
                                            switch (port.File.Type)
                                            {
                                                case ePortType.LoadingPort:
                                                    return "N";
                                                case ePortType.UnloadingPort:
                                                case ePortType.BothPort:
                                                    return "Y";
                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                    }
                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T FIND JOB TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                    return "N";

                                    //// 20150417 邏輯跟Array一樣
                                    //if (job.TargetPortID != "" && job.TargetPortID != "0" && (job.SourcePortID != job.TargetPortID)) return "Y";

                                    //// 以下寫的有問題
                                    ////if (port.File.Type == ePortType.LoadingPort)
                                    ////{
                                    ////    if (job.MesProduct.PROCESSFLAG.Equals("Y")) return "Y";
                                    ////}
                                    ////else
                                    ////{
                                    ////    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE && port.File.Type == ePortType.UnloadingPort) return "Y";
                                    ////}

                                    #endregion
                                case eLineType.CF.FCREP_TYPE1:
                                case eLineType.CF.FCREP_TYPE2:
                                case eLineType.CF.FCREP_TYPE3:
                                case eLineType.CF.FCAOI_TYPE1:
                                case eLineType.CF.FCMQC_TYPE1:
                                case eLineType.CF.FCMQC_TYPE2:
                                case eLineType.CF.FCMAC_TYPE1:
                                case eLineType.CF.FCPSH_TYPE1:
                                    #region REP,MQC,FIP Rule

                                    switch (job.JobType)
                                    {
                                        case eJobType.TFT:
                                        case eJobType.CF:
                                        case eJobType.DM:  //CF dm glass 也需要如实上报  qiumin 20180103
                                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                                            {
                                                switch (port.File.Type)
                                                {
                                                    case ePortType.LoadingPort:
                                                        return "N";
                                                    case ePortType.UnloadingPort:
                                                        return "Y";
                                                }
                                            }
                                            else
                                            {
                                                IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                                                switch (port.File.Type)
                                                {
                                                    case ePortType.LoadingPort:
                                                    case ePortType.BothPort:
                                                        foreach (string v in trackings.Values)
                                                        {
                                                            if (v.Equals("1")) return "Y";
                                                        }
                                                        return "N";
                                                    case ePortType.UnloadingPort:
                                                        if (port.File.PartialFullFlag == eParitalFull.PartialFull)
                                                        {
                                                            if (cst.CassetteSequenceNo == job.JobSequenceNo)
                                                            {
                                                                //Add by Kasim in 20150526 
                                                                return "N";
                                                            }
                                                        }
                                                        foreach (string v in trackings.Values)
                                                        {
                                                            if (v.Equals("1")) return "Y";
                                                        }
                                                        return "N";
                                                }
                                            }
                                            Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                            return "N";
                                   /*  case eJobType.DM:
                                               switch (port.File.Type)
                                               {
                                                   case ePortType.LoadingPort:
                                                       return "N";
                                                   case ePortType.UnloadingPort:
                                                   case ePortType.BothPort:
                                                       return "Y";
                                               } 
                                               Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                   string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                                   job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                               return "N"; */
                                    }
                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T FIND JOB TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                    return "N";

                                    #endregion
                                case eLineType.CF.FCUPK_TYPE1:
                                    #region Unpacker Rule
                                    List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID);
                                    string ULDRunmode = string.Empty;
                                    foreach(Equipment eqp in eqps)
                                    {
                                        if(eqp.Data.NODEATTRIBUTE == "UD")
                                            ULDRunmode = eqp.File.EquipmentRunMode;
                                    }
                                    switch (port.File.Type)
                                    {
                                        case ePortType.LoadingPort:
                                            return "N";
                                        case ePortType.UnloadingPort:
                                            return "Y";
                                        case ePortType.BothPort:
                                        //2015/9/21 add by Frank
                                            if (ULDRunmode == "RE-CLEAN")
                                            {
                                                if (job.TrackingData.Substring(0, 1).Equals("1"))   //20160725 Modify by Frank
                                                    return "Y";
                                                else
                                                    return "N";
                                            }
                                            return "N";           
                                    }

                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T FIND PORT TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                    return "N";

                                    #endregion
                                default:
                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("CAN'T LINE TYPE,PROCESSRESULT:[N],JobID:[{0}],LineType:[{1}],PortType:[{2}],JobJudge:[{3}],AlignerProcessFlag:[{4}],EQP Flag:[{5}]",
                                        job.GlassChipMaskBlockID, line.Data.LINETYPE, port.File.Type, job.JobJudge, job.CfSpecial.EQPFlag2.ExposureProcessFlag, job.EQPFlag));
                                    return "N";
                            }
                        }
                        #endregion
                    case eFabType.CELL:
                        #region [CELL]
                        {
                            switch (line.Data.LINETYPE)//T3 都走 Default
                            {
                        #region [T2]
                                //case eLineType.CELL.CBPMT:
                                //    if (job.JobProcessFlows.Count > 1) //Jun Modify 20150402 不需判斷SamplingSlotFlag或ProcessFlag  && job.SamplingSlotFlag == "1")
                                //        return "Y";
                                //    else
                                //        return "N"; //Watson Add 20141225 For MES Spec

                                case eLineType.CELL.CBPOL_1:
                                case eLineType.CELL.CBPOL_2:
                                case eLineType.CELL.CBPOL_3:
                                    if (port.File.Type == ePortType.LoadingPort)
                                    {
                                        return "N";
                                    }
                                    else if (port.File.Type == ePortType.UnloadingPort)
                                    {
                                        if (cst.MES_CstData.AUTOCLAVESKIP == "N")
                                            return "Y";
                                        else
                                            return "N";
                                    }
                                    return "N";

                                case eLineType.CELL.CBGAP:
                                case eLineType.CELL.CBPMT:
                                case eLineType.CELL.CBDPS:
                                case eLineType.CELL.CBDPI:
                                case eLineType.CELL.CBATS:
                                case eLineType.CELL.CBLOI:
                                case eLineType.CELL.CBSOR_1:
                                case eLineType.CELL.CBSOR_2:
                                    if (job.CreateTime != job.JobProcessStartTime)  //Jun Modify 20150402 && job.SamplingSlotFlag == "1")
                                        return "Y";
                                    else
                                        return "N";

                        #endregion
                                default://T3 Use modify by sy 20160525
                                    if (port.File.Type == ePortType.LoadingPort)
                                        return "N";
                                    else if (port.File.Type == ePortType.UnloadingPort)
                                    {
                                        if (line.Data.LINEID.Contains("CCPIL") )//add by hujunpeng for get PI trackingdata for process result 20181017
                                        {
                                            if (job.JobType != eJobType.TFT && job.JobType != eJobType.CF) return "Y";
                                            else
                                            {
                                                IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");

                                                if ((trackings["PICCoater#01"].Equals("1") || trackings["PICCoater#02"].Equals("1")) && (trackings["PPAPA#01"].Equals("1") || trackings["PPAPA#02"].Equals("1")))
                                                    return "Y";
                                                else
                                                {
                                                    if ((trackings["PICCoater#01"].Equals("1") || trackings["PICCoater#02"].Equals("1")) && (trackings["PPAPA#01"].Equals("0") && trackings["PPAPA#02"].Equals("0")))
                                                    {
                                                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "PPA PROCESS was not done");
                                                    }
                                                    if ((trackings["PPAPA#01"].Equals("1") || trackings["PPAPA#02"].Equals("1")) && (trackings["PICCoater#01"].Equals("0") && trackings["PICCoater#02"].Equals("0")))
                                                    {
                                                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "PIC PROCESS was not done");
                                                    }
                                                    if ((trackings["PICCoater#01"].Equals("0") && trackings["PICCoater#02"].Equals("0")) && (trackings["PPAPA#01"].Equals("0") && trackings["PPAPA#02"].Equals("0")))
                                                    {
                                                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "PIC&PPA PROCESS was not done");
                                                    }
                                                    return "N";
                                                }
                                            }
                                        }
                                        else 
                                            return "Y";
                                    }
                                    else
                                    {
                                        IDictionary<string, string> trackings = ObjectManager.SubJobDataManager.Decode(job.TrackingData, "TrackingData");
                                        foreach (string v in trackings.Values)
                                        {
                                            if (v.Equals("1")) return "Y";
                                        }
                                        return "N";
                                    }
                                    
                                    //if (job.JobProcessFlows.Count > 1)  //Jun Modify 20150402 && job.SamplingSlotFlag == "1")
                                    //    return "Y";
                                    //else
                                    //    return "N";
                            }
                        }
                        #endregion
                    default: return "N";
                }
            }
            catch(Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return "N";
            }
        }

        public string GetProductJudge(Line line, eFabType fabType, Job job)
        {
            try
            {
                // Array不用報, 只有CELL需要報
                if (fabType == eFabType.CELL)
                {
                    return ConstantManager[string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE)][job.JobJudge].Value;
                }
                // 20160628 Add by Frank CF用Job Grade上報給MES 
                if (fabType == eFabType.CF)
                {
                    return job.JobGrade;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }
        }

        public string GetProductGrade(Line line, eFabType fabType, Job job)
        {
            try
            {
                if (line.Data.LINETYPE == eLineType.CF.FCSRT_TYPE1 ||
                    fabType == eFabType.ARRAY || fabType == eFabType.CELL)
                {
                    return job.JobGrade;
                }
                else
                {
                    string CFJudge =ConstantManager[string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE)][job.JobJudge].Value;
                    return CFProductGradeRule(job.JobGrade, CFJudge);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }
        }
        //20160628 Add by Frank according to CF new PRODUCTGRADE Rule
        public string CFProductGradeRule(string grade, string judge)
        {
            try
            {
                switch (judge)
                {
                    case "NG":  //judge="NG"，即上報NG
                        return "NG";
                    case "RW":  //judge="RW"，即上報RW
                        return "RW";
                    case "IR":  //judge="IR"，即上報IR
                        return "IR";
                    case "RP":  //judge="RP"，即上報RP
                        return "RP";
                    case "PD":  //judge="PD"，即上報PD
                        return "PD";
                    case "OK":
                        {
                            if (grade == "RP") //若judge="OK"，grade="RP"，則上報RP
                                return "RP";
                            if (grade == "IR") //若judge="OK"，grade="IR"，則上報IR
                                return "IR";
                            return "OK";
                        }                        
                }
                return judge;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }
        }
    }
}
