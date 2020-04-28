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
        /// 6.59.	LotProcessAbnormalEnd       MES MessageSet : BC reports this event instead of ‘LotProcessEnd’ when Lot process is ended abnormally. 
        /// </summary>
        /// <param name="trxID">yyyyMMddHHmmssffff</param>
        /// <param name="port">Port Entity</param>
        /// <param name="cst">Cassette Entity</param>
        /// <param name="jobs">job Entity List</param>
        public void LotProcessAbnormalEnd(string trxID, Port port, Cassette cst, IList<Job> jobs)
        {
            try
            {
                bool _ttp = false;
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //string.Format("[LINENAME={1}] [BCS -> MES][{0}] LotProcessAbnormalEnd Send MES but OFF LINE LINENAME =[{1}).", trxID, line.Data.LINEID));
                    string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, trxID, MethodBase.GetCurrentMethod().Name));
                    return;
                }
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("LotProcessAbnormalEnd") as XmlDocument;
                SetTransactionID(xml_doc, trxID);

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

                XmlNode bodyNode = xml_doc[keyHost.MESSAGE][keyHost.BODY];
                bodyNode[keyHost.LINENAME].InnerText = ObjectManager.LineManager.GetLineID(line.Data.LINEID);
                bodyNode[keyHost.PORTNAME].InnerText = port.Data.PORTID;
                bodyNode[keyHost.CARRIERNAME].InnerText = port.File.CassetteID;
                bodyNode[keyHost.LINERECIPENAME].InnerText = cst.LineRecipeName;
                bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = cst.MES_CstData.LINERECIPENAME;

                //Jun Modify 20150520 For MES Spec 
                //ABORTFLAG indicates the Lot ends by Abort. If Lot started in Offline and ends normal in Online, still  report ABORTFLAG .
                //Watson Add 20141212 For MES Spec 
                //If Lot started in Offline and ends normal in Online, report ABORTFLAG as blank.
                //if (cst.IsOffLineProcessStarted)
                //    bodyNode[keyHost.ABORTFLAG].InnerText = "";
                //else
                bodyNode[keyHost.ABORTFLAG].InnerText = "Y";

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

                // [CSTMAPGRADE]  寫在Product 層，不用再每片判斷

                bodyNode[keyHost.TIMESTAMP].InnerText = GetTIMESTAMP();
                bodyNode[keyHost.CFSHORTCUTMODE].InnerText = "DISABLE";

                if (line.Data.FABTYPE == eFabType.CF.ToString())
                {
                    bodyNode[keyHost.CFSHORTCUTMODE].InnerText = (line.File.CFShortCutMode == eShortCutMode.Enable ? "ENABLE" : "DISABLE");
                }

                bodyNode[keyHost.PRODUCTQUANTITY].InnerText = jobs.Count().ToString();

                if ((port.Data.NODEID.Contains(keyCELLMachingName.CBBUR) || port.Data.NODEID.Contains(keyCELLMachingName.CBPLD))
                        && port.File.Type == ePortType.UnloadingPort)
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

                if (port.Data.PORTATTRIBUTE != "UPKPORT")
                {
                    foreach (Job job in jobs)
                    {
                        bodyNode[keyHost.LINERECIPENAME].InnerText = job.LineRecipeName;   //Watson modify 20141122 For 帶JOB層的

                        if (line.Data.LINETYPE.Contains(eLineType.CF.FCUPK_TYPE1) ||
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

                        if (job.MesCstBody.LOTLIST.Count != 0)
                        {
                            bodyNode[keyHost.HOSTLINERECIPENAME].InnerText = job.MesCstBody.LOTLIST[0].LINERECIPENAME; //Watson modify 20141122 For 帶JOB層的
                            product[keyHost.LOTNAME].InnerText = job.MesCstBody.LOTLIST[0].LOTNAME;
                            product[keyHost.PRODUCTSPECNAME].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                            product[keyHost.PROCESSOPERATIONNAME].InnerText = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                            product[keyHost.PRODUCTOWNER].InnerText = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                            product[keyHost.CFREWORKCOUNT].InnerText = job.MesCstBody.LOTLIST[0].CFREWORKCOUNT;
                        }

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
                        product[keyHost.PRODUCTGRADE].InnerText = GetProductGrade(line, fabType, job);
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
                        //#region [T2 ODF]
                        //if (line.Data.LINETYPE == eLineType.CELL.CBODF)
                        //    product[keyHost.PAIRPRODUCTNAME].InnerText = job.MesProduct.CFPRODUCTNAME;
                        //#endregion
                        product[keyHost.PRODUCTRECIPENAME].InnerText = job.LineRecipeName;
                        product[keyHost.HOSTPRODUCTRECIPENAME].InnerText = job.MesProduct.PRODUCTRECIPENAME;

                        product[keyHost.VCRREADFLAG].InnerText = ObjectManager.JobManager.P2M_GetVCRResult(job.VCR_Result);

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
                                        //code[keyHost.ABNORMALSEQ].InnerText = "SB_HP_NUM";
                                        code[keyHost.ABNORMALVALUE].InnerText = "SB_HP_NUM";
                                        code[keyHost.ABNORMALCODE].InnerText = job.ArraySpecial.DNS_SB_HP_NUM;
                                        abromalListNode.AppendChild(code);
                                        code = codeNode.Clone();
                                        // code[keyHost.ABNORMALSEQ].InnerText = "SB_CP_NUM";
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
                                IList<Equipment> Equipments = ObjectManager.EquipmentManager.GetEQPs();

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
                                                code[keyHost.ABNORMALCODE].InnerText = "";
                                                abromalListNode.AppendChild(code);
                                            }
                                        }
                                        #endregion

                                        #region ALNSIDE
                                        // 李泉表示BM/S LINE ALNSIDE 上報EQPID給MES 2015/9/7 modify Frank 
                                        _side = string.Empty;
                                        if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.ALNSIDE))
                                        {
                                            if (job.CfSpecial.TrackingData.BMPS_Exposure == "1") { _side = "EXPOSURE"; }
                                            if (job.CfSpecial.TrackingData.BMPS_Exposure2 == "1") { _side = "EXPOSURE2"; }
                                            foreach (Equipment eqp in Equipments)
                                            {
                                                if (eqp.Data.NODEATTRIBUTE == _side)
                                                {
                                                    code = codeNode.Clone();
                                                    //code[keyHost.ABNORMALSEQ].InnerText = "ALNSIDE";
                                                    code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                                                    code[keyHost.ABNORMALCODE].InnerText = eqp.Data.NODEID;
                                                    abromalListNode.AppendChild(code);
                                                    lock (job)
                                                    { job.CfSpecial.AbnormalCode.ALNSIDE = eqp.Data.NODEID; }
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
                                                    code[keyHost.ABNORMALVALUE].InnerText = "OVENSIDE";
                                                    code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                                    abromalListNode.AppendChild(code);
                                                    lock (job)
                                                    { job.CfSpecial.AbnormalCode.OVENSIDE = unit.Data.UNITID; }
                                                    ObjectManager.JobManager.EnqueueSave(job);
                                                    break;
                                                }
                                            }
                                            if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.OVENSIDE))
                                            {
                                                code = codeNode.Clone();
                                                code[keyHost.ABNORMALVALUE].InnerText = "OVENSIDE";
                                                code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                                abromalListNode.AppendChild(code);
                                            }
                                        }
                                        else
                                        {
                                            code = codeNode.Clone();
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
                                                    code[keyHost.ABNORMALVALUE].InnerText = "VCDSIDE";
                                                    code[keyHost.ABNORMALCODE].InnerText = unit.Data.UNITID;
                                                    abromalListNode.AppendChild(code);
                                                    lock (job)
                                                    { job.CfSpecial.AbnormalCode.VCDSIDE = unit.Data.UNITID; }
                                                    ObjectManager.JobManager.EnqueueSave(job);
                                                    break;
                                                }
                                            }
                                            if (string.IsNullOrEmpty(job.CfSpecial.AbnormalCode.VCDSIDE))
                                            {
                                                code = codeNode.Clone();
                                                code[keyHost.ABNORMALVALUE].InnerText = "VCDSIDE";
                                                code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                                abromalListNode.AppendChild(code);
                                            }
                                        }
                                        else
                                        {
                                            code = codeNode.Clone();
                                            code[keyHost.ABNORMALVALUE].InnerText = "VCDSIDE";
                                            code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                            abromalListNode.AppendChild(code);
                                        }
                                        #endregion

                                        #region COA2MASKEQPID
                                        code = codeNode.Clone();
                                        code[keyHost.ABNORMALVALUE].InnerText = "COA2MASKEQPID";
                                        if (job.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID.Equals("0"))
                                            code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                        else
                                            code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID;
                                        abromalListNode.AppendChild(code);
                                        #endregion

                                        #region COA2MASKNAME
                                        code = codeNode.Clone();
                                        code[keyHost.ABNORMALVALUE].InnerText = "COA2MASKNAME";
                                        code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.COA2MASKNAME;
                                        abromalListNode.AppendChild(code);
                                        #endregion

                                        #region PRLOT
                                        code = codeNode.Clone();
                                        code[keyHost.ABNORMALVALUE].InnerText = "PRLOT";
                                        code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.PRLOT;
                                        abromalListNode.AppendChild(code);
                                        #endregion

                                        #region CSPNUMBER
                                        code = codeNode.Clone();
                                        code[keyHost.ABNORMALVALUE].InnerText = "CSPNUMBER";
                                        code[keyHost.ABNORMALCODE].InnerText = job.CfSpecial.AbnormalCode.CSPNUMBER;
                                        abromalListNode.AppendChild(code);
                                        #endregion

                                        #region HPCHAMBER
                                        Unit unit_HP = Units.FirstOrDefault(u => u.Data.UNITATTRIBUTE.Equals("HP"));
                                        if (unit_HP == null) throw new Exception(string.Format("CAN'T FIND UNIT_ATTRIBUTE=[HP] IN DB!"));
                                        code = codeNode.Clone();
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
                                        code[keyHost.ABNORMALVALUE].InnerText = "CPCHAMBER";
                                        if (port.File.Type != ePortType.LoadingPort)
                                            code[keyHost.ABNORMALCODE].InnerText = unit_CP.Data.UNITID.Trim() + job.CfSpecial.CFSpecialReserved.CPSlotNumber.PadLeft(2, '0');
                                        else
                                            code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                        abromalListNode.AppendChild(code);
                                        #endregion

                                        #region DISPENSESPEED
                                        code = codeNode.Clone();
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
                                                code[keyHost.ABNORMALVALUE].InnerText = "ALNSIDE";
                                                code[keyHost.ABNORMALCODE].InnerText = string.Empty;
                                                abromalListNode.AppendChild(code);
                                            }
                                        }
                                        else
                                        {
                                            code = codeNode.Clone();
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

                        // MES SPEC 修改 2014.09.28 要重寫
                        //for (int i = 0; i < job.MesProduct.ABNORMALCODELIST.CODE.Count; i++)
                        //{
                        //    if (i > 0)
                        //    {
                        //        codeNode = codeNode.Clone();
                        //        abromalListNode.AppendChild(codeNode);
                        //    }
                        //    product[keyHost.ABNORMALSEQ].InnerText = job.MesProduct.ABNORMALCODELIST.CODE[i].ABNORMALSEQ;
                        //    product[keyHost.ABNORMALCODE].InnerText = job.MesProduct.ABNORMALCODELIST.CODE[i].ABNORMALCODE;
                        //}
                        #endregion

                        // 當 Over Q Time 或者 VCR Mismatch 發生時，HoldInforList 就會有紀錄。
                        if (job.HoldInforList.Count() > 0)
                        {
                            if (job.HoldInforList.Count() == 1 && job.TrackingDataBypassHoldFlag)
                                product[keyHost.HOLDFLAG].InnerText = "N";
                            else
                                product[keyHost.HOLDFLAG].InnerText = "Y";

                            string holdmachine = string.Empty;
                            string holdOperater = string.Empty;
                            string holdcomment = string.Empty;  // For T3 Hold Comment

                            for (int i = 0; i < job.HoldInforList.Count(); i++)
                            {
                                if (!string.IsNullOrEmpty(holdmachine)) holdmachine += ";";
                                holdmachine += string.Format("{0}_{1}", job.HoldInforList[i].NodeID, job.HoldInforList[i].HoldReason);

                                if (!string.IsNullOrEmpty(holdOperater)) holdOperater += ";";
                                holdOperater += job.HoldInforList[i].OperatorID;

                                // For T3 Hold Commnet
                                if (!string.IsNullOrEmpty(holdcomment)) holdcomment += ";";
                                holdcomment += job.HoldInforList[i].HoldReason;
                            }

                            product[keyHost.HOLDCOMMENT].InnerText = holdcomment;     //For T3 Hold Comment
                            product[keyHost.HOLDMACHINE].InnerText = holdmachine;
                            product[keyHost.HOLDOPERATOR].InnerText = holdOperater;
                        }

                        //Watson Add 20150126 目前不使用
                        XmlNode psheighList = product[keyHost.PSHEIGHTLIST];
                        XmlNode psNodeClone = psheighList[keyHost.SITEVALUE].Clone();
                        psheighList.RemoveAll();

                        product[keyHost.DUMUSEDCOUNT].InnerText = job.MesProduct.DUMUSEDCOUNT;
                        product[keyHost.CFTYPE1REPAIRCOUNT].InnerText = job.MesProduct.CFTYPE1REPAIRCOUNT;
                        product[keyHost.CFTYPE2REPAIRCOUNT].InnerText = job.MesProduct.CFTYPE2REPAIRCOUNT;
                        product[keyHost.CARBONREPAIRCOUNT].InnerText = job.MesProduct.CARBONREPAIRCOUNT;
                        product[keyHost.LASERREPAIRCOUNT].InnerText = ""; // MES會計算, 不用填
                        //product[keyHost.SHORTCUTFLAG].InnerText = GetShortCutFlag(line, job.CellSpecial.CuttingFlag); - 搬到EQP Flag拆解

                        //CELL SPECIAL - Finish
                        //product[keyHost.GMURAFLAG].InnerText = job.CellSpecial.CGMOFlag.ToString(); - 搬到EQP Flag拆解
                        //product[keyHost.QTAPFLAG].InnerText = job.FirstRunFlag == "1" ? "Y" : "N"; - 搬到EQP Flag拆解

                        //ODF Special SAMPLEFLAG
                        if (line.Data.LINETYPE == eLineType.CELL.CBODF)
                        {
                            //CELL SPECIAL
                            //product[keyHost.SAMPLEFLAG].InnerText = ""; - 搬到EQP Flag拆解
                        }
                        else
                        {
                            product[keyHost.SAMPLEFLAG].InnerText = "N";
                        }

                        if (line.Data.LINETYPE == eLineType.ARRAY.CVD_AKT ||
                            line.Data.LINETYPE == eLineType.ARRAY.CVD_ULVAC ||
                            line.Data.LINETYPE == eLineType.ARRAY.DRY_ICD ||
                            line.Data.LINETYPE == eLineType.ARRAY.DRY_YAC ||
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
                        //TODO: Array CF CELL 各自寫Material List部份
                        //<MATERIALLIST>
                        //         <MATERIAL>
                        //             <MATERIALTYPE></MATERIALTYPE>
                        //             <MATERIALNAME></MATERIALNAME>
                        //         </MATERIAL>
                        //  </MATERIALLIST>

                        Port sourcePort = ObjectManager.PortManager.GetPortByLineIDPortID(line.Data.LINEID, job.SourcePortID);
                        if (sourcePort != null)
                        {
                            switch (port.Data.PORTATTRIBUTE.Trim())
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

                        //TODO: CF - finish
                        //Download by MES CFShortCutGlassProcessEndReply, and report by CF Shortcut CST ULD TKOUT, 
                        //then MES will execute auto sampling by this value.
                        product[keyHost.SAMPLETYPE].InnerText = job.CfSpecial.SamplingValue;

                        //1.	Used count of material
                        //2.	Used  count  of  CF  ITO  Dummy
                        //3.	Used  count  of  Cell  UV  Mask
                        /*if (line.Data.LINETYPE == eLineType.CF.FBITO_TYPE1)
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

                       // product[keyHost.PROCESSFLAG].InnerText = job.MesProduct.PROCESSFLAG;

                        product[keyHost.PROCESSFLAG].InnerText = string.IsNullOrEmpty(job.MesProduct.MESPROCESSFLAG) ? job.MesProduct.PROCESSFLAG : job.MesProduct.MESPROCESSFLAG; //yang

                      //  product[keyHost.PROCESSFLAG].InnerText = job.MesProduct.MESPROCESSFLAG.Length ==0 ? job.MesProduct.PROCESSFLAG : job.MesProduct.MESPROCESSFLAG;    //keep MES validatereply的值

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

                        //CELL CSOT MES 會再針對一些FLAG做統整, 等新SPEC 出來後再寫 2014.09.29
                        //#region [CQLTFLAG]
                        // 有區分 優先等級, 要可設定 , (之後可能會與其他FLAG統整, 等MES出新的SPEC)
                        //product[keyHost.CQLTFLAG].InnerText = ""; - 搬到EQP Flag拆解
                        //#endregion

                        //CELL
                        //product[keyHost.FMAFLAG].InnerText = ""; - 搬到EQP Flag拆解

                        if (!string.IsNullOrEmpty(job.MES_PPID))
                        { product[keyHost.PPID].InnerText = job.MES_PPID; } //Watson Modify 20141128 job.PPID 用於機台，會有跨機台
                        else { product[keyHost.PPID].InnerText = job.PPID; }

                        product[keyHost.HOSTPPID].InnerText = job.MesProduct.PPID;

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
                        //if (line.Data.SERVERNAME == "CBPOL300")
                        if (line.Data.LINEID == "CBPOL300" || line.Data.LINEID == "CBPOL400")
                        {
                            if (port.File.Type == ePortType.LoadingPort)
                                product[keyHost.RTPFLAG].InnerText = job.CellSpecial.RTPFlag == string.Empty ? job.MesProduct.RTPFLAG : job.CellSpecial.RTPFlag;
                            else if (port.File.Type == ePortType.UnloadingPort)
                            {
                                if (job.JobProcessFlows.Count > 1 && job.SamplingSlotFlag == "1")
                                    product[keyHost.RTPFLAG].InnerText = "Y";
                                else
                                    product[keyHost.RTPFLAG].InnerText = "";  //Jun Modify 20150330 MES要求如果不是RTP的產生，RTPFlag填空
                            }
                        }

                        //EQP Flag Decode By Shop
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
                            case "CELL": CELL_Product_Special(ref bodyNode, ref product, line, job, port,ref cstGrade); break;
                        }

                        #region  判断CSTMAPGRADE 是否为NG or S0 by zhuxingxing
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
                }
                bodyNode.AppendChild(glsListNode);

                //SendToMES(xml_doc);
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
                    if (!ObjectManager.CassetteManager.FileSaveToLotEndExecute(port.Data.PORTID, port.File.CassetteID, trxID, xml_doc, out err, "ABN_"))
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
                string.Format("[LINENAME={1}] [BCS -> MES][{0}] LotProcessAbnormalEnd OK LINENAME =[{1}).",
                    trxID, ObjectManager.LineManager.GetLineID(line.Data.LINEID)));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 6.60.	LotProcessAbnormalEndReply      MES MessagetSet : Lot Process Abnormal End Reply to BC
        /// </summary>
        /// <param name="xmlDoc">Reply XML Document</param>
        public void MES_LotProcessAbnormalEndReply(XmlDocument xmlDoc)
        {
            try
            {
                // add by bruce 20160413 for T2 Issue
                #region Delay Send LotProcessEnd
                try
                {
                    lock (_timeSync)
                        _processEndMESReplyFlag = true;
                }
                catch (Exception ex)
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
                #endregion

                string returnCode = GetMESReturnCode(xmlDoc);
                string returnMessage = GetMESReturnMessage(xmlDoc);
                string lineName = GetLineName(xmlDoc); //MES Send to BC Line Name
                string trxID = GetTransactionID(xmlDoc);

                if (!CheckMESLineID(lineName))
                {
                    Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- MES] [{1}] MES Reply LineName =[{2}], mismatch =[{0}).", ServerName, trxID, lineName));
                }

                string portid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PORTNAME].InnerText;
                string cstid = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME].InnerText;
                string desc = string.Empty;

                if (ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portid).File.Type != ePortType.LoadingPort)
                {
                    if (!ObjectManager.CassetteManager.UpdateLotPorcessEndMesReplyToExecuteXmlAndDB(portid, cstid, trxID, returnCode == "0" ? "OK" : "NG", returnCode, returnMessage, out desc, "ABN_"))
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("[LINENAME={1}] [BCS <- MES][{0}] LotProcessAbnormalEndReply.UpdateLotPorcessAbnormalEndMesReplyToExecuteXmlAndDB  NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                                       trxID, lineName, portid, cstid, returnCode, returnMessage, desc));
                }

                if (returnCode != "0")
                {
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES] [{0}] LotProcessAbnormalAbnormalEndReply  NG LINENAME =[{1}],PORTNAME =[{2}],CARRIERNAME=[{3}],CODE =[{4}],MESSAGE =[{5}).",
                                        trxID, lineName, portid, cstid, returnCode, returnMessage));

                    if (ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portid).File.Type != ePortType.LoadingPort)
                    {
                        if (!ObjectManager.CassetteManager.FileMoveToIncompleteCST(portid, cstid, trxID, out desc, "ABN_"))
                            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={1}] [BCS <- MES][{0}] LotProcessAbnormalEndReply.FileMoveToIncompleteCST  NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                                            trxID, lineName, portid, cstid, returnCode, returnMessage, desc));
                    }
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={1}] [BCS <- MES] [{0}] LotProcessAbnormalEndReply  OK LINENAME =[{1}],PORTNAME =[{2}],CARRIERNAME =[{3}],CODE =[{4}],MESSAGE =[{5}).",
                                        trxID, lineName, portid, cstid, returnCode, returnMessage));

                    if (ObjectManager.PortManager.GetPortByLineIDPortID(lineName, portid).File.Type != ePortType.LoadingPort)
                    {
                        if (!ObjectManager.CassetteManager.FileMoveToCompleteCST(portid, cstid, trxID, out desc, "ABN_"))
                            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                               string.Format("[LINENAME={1}] [BCS <- MES][{0}] LotProcessAbnormalEndReply.FileMoveToCompleteCST NG LINENAME={1},PORTNAME={2},CARRIERNAME={3},CODE={4},MESSAGE={5},ErrMsg=[{6}]",
                                           trxID, lineName, portid, cstid, returnCode, returnMessage, desc));
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private string GetShortCutFlag(Line line, eCUTTING_FLAG flag)
        {
            switch (line.Data.LINETYPE)
            {
                case "CBDPK":
                case "CBLOI":
                case "CBOLS":
                    {
                        switch (flag)
                        {
                            case eCUTTING_FLAG.LSC_CUTTING_OK: return "LSCOK";
                            case eCUTTING_FLAG.OLS_CUTTING_OK: return "OLSOK";
                            case eCUTTING_FLAG.CUTTING_NG: return "NG";
                            default: return "NC";
                        }
                    }
                default: return "";
            }
        }

        private void P2M_DecodeEQPFlagBy_ARRAY(XmlNode product, Line line, Job job)
        {
            try
            {
                IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");

                if (subItem == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can't Decode Job=[{0}) Data detail", job.JobKey));
                    return;
                }


                if ((line.Data.LINETYPE == eLineType.ARRAY.MAC_CONTREL) || (line.Data.LINETYPE == eLineType.ARRAY.CLS_MACAOH))  //modify by bruce 20160107 for Macro glass turn use
                {
                    if (subItem.ContainsKey(eEQPFLAG.Array.MAC_TurnModeFlag))
                    {
                        if (subItem[eEQPFLAG.Array.MAC_TurnModeFlag] == "1") //modify for MAC 2016/01/04 cc.kuang
                        {
                            if (subItem[eEQPFLAG.Array.MAC_TurnTableFlag] == "1")
                                product[keyHost.GLASSTURNFLAG].InnerText = "N";
                            else
                                product[keyHost.GLASSTURNFLAG].InnerText = "Y";
                        }
                        else
                        {
                            if (subItem[eEQPFLAG.Array.MAC_TurnTableFlag] == "1")
                                product[keyHost.GLASSTURNFLAG].InnerText = "Y";
                            else
                                product[keyHost.GLASSTURNFLAG].InnerText = "N";
                        }
                    }
                    else
                    {
                        product[keyHost.GLASSTURNFLAG].InnerText = "N";
                    }
                }
                else
                {
                    product[keyHost.GLASSTURNFLAG].InnerText = " ";
                }


                //if ((line.Data.LINETYPE == eLineType.ARRAY.MAC_CONTREL) && (subItem.ContainsKey(eEQPFLAG.Array.MAC_TurnModeFlag)))
                //{
                //    if (subItem[eEQPFLAG.Array.MAC_TurnModeFlag] == "1") //modify for MAC 2016/01/04 cc.kuang
                //    {
                //        if (subItem[eEQPFLAG.Array.MAC_TurnTableFlag] == "1")
                //            product[keyHost.GLASSTURNFLAG].InnerText = "N";
                //        else
                //            product[keyHost.GLASSTURNFLAG].InnerText = "Y";
                //    }
                //    else
                //    {
                //        if (subItem[eEQPFLAG.Array.MAC_TurnTableFlag] == "1")
                //            product[keyHost.GLASSTURNFLAG].InnerText = "Y";
                //        else
                //            product[keyHost.GLASSTURNFLAG].InnerText = "N";
                //    }
                //}
                //else
                //{
                //      product[keyHost.GLASSTURNFLAG].InnerText = " ";
                //}
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void P2M_DecodeEQPFlagBy_CF(XmlNode product, Line line, Job job)
        {
            try
            {
                IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");

                if (subItem == null)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can't Decode Job=[{0}) Data detail", job.JobKey));
                    return;
                }

                if ((line.Data.LINETYPE == eLineType.CF.FCPSH_TYPE1) && (subItem.ContainsKey("TotalPitchOfflineInspectionFlag")))
                {
                    product[keyHost.INSPECTIONFLAG].InnerText = subItem["TotalPitchOfflineInspectionFlag"] == "1" ? "Y" : "N";
                }
                else
                {
                    product[keyHost.INSPECTIONFLAG].InnerText = "N";
                }


            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void P2M_DecodeProductFlagBy_CELL(XmlNode product, Line line, Job job)
        {
            try
            {
                IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");

                if (subItem == null)
                {
                    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can't Decode Job Data detail, CassetteSequenceNo=[{0}], JobSequenceNo=[{1}).", job.CassetteSequenceNo, job.JobSequenceNo));
                    return;
                }

                if (subItem.ContainsKey("CGMOFlag"))
                    product[keyHost.GMURAFLAG].InnerText = subItem["CGMOFlag"] == "1" ? "Y" : "N";
                else
                    product[keyHost.GMURAFLAG].InnerText = "N";

                if (subItem.ContainsKey("FirstRun"))
                    product[keyHost.QTAPFLAG].InnerText = subItem["FirstRun"] == "1" ? "Y" : "N";
                else
                    product[keyHost.QTAPFLAG].InnerText = "N";

                if (subItem.ContainsKey("CellGAPFlag"))
                    product[keyHost.GAPSAMPLEFLAG].InnerText = subItem["CellGAPFlag"] == "1" ? "Y" : string.Empty;//2016.12.26 huangjiayin modify: 给Y或空，不给N
                else
                    product[keyHost.GAPSAMPLEFLAG].InnerText = string.Empty;

                if (subItem.ContainsKey("ENGModeFlag"))//<CENGFLAG> is change to <ABNORMALENG> MES SPEC1.21 修改 shihyahg 20151029
                {//20160327待閻波確認文件再修改 
                    //if (line.Data.LINETYPE == eLineType.CELL.CCPIL)                        
                    //    product[keyHost.ABNORMALENG].InnerText = subItem["ENGModeFlag"] == "1" ? "PI" : "";
                    //else if (line.Data.LINETYPE == eLineType.CELL.CCODF)
                    //    product[keyHost.ABNORMALENG].InnerText = subItem["ENGModeFlag"] == "1" ? "ODF" : "";
                    product[keyHost.ABNORMALENG].InnerText = "";//TO DO
                }
                else
                    product[keyHost.ABNORMALENG].InnerText = "";

                //if (subItem.ContainsKey("CQLTModeFlag")//MES SPEC 1.21修改 shihyahg 20151029
                //    product[keyHost.CQLTFLAG].InnerText = subItem["CQLTModeFlag"] == "1" ? "Y" : "N";
                //else
                //    product[keyHost.CQLTFLAG].InnerText = "N";

                if (subItem.ContainsKey("CuttingFlag"))
                {
                    eCUTTING_FLAG CUTTING_FLAG;
                    Enum.TryParse<eCUTTING_FLAG>(subItem["CuttingFlag"], out CUTTING_FLAG);
                    product[keyHost.SHORTCUTFLAG].InnerText = GetShortCutFlag(line, CUTTING_FLAG);
                }
                else
                    product[keyHost.SHORTCUTFLAG].InnerText = "N";

                if (subItem.ContainsKey("FMAFlag"))
                    product[keyHost.FMAFLAG].InnerText = subItem["FMAFlag"] == "1" ? "Y" : "N";
                else
                    product[keyHost.FMAFLAG].InnerText = "N";

                if (subItem.ContainsKey("ShellFlag"))//MES 1.21 ADD PIL 用
                    product[keyHost.CELLSAMPLEFLAG].InnerText = subItem["ShellFlag"] == "1" ? "SHELL" : "";
                else if (subItem.ContainsKey("BubbleSampleFlag"))//MES 1.21 ADD PCS 用
                    product[keyHost.CELLSAMPLEFLAG].InnerText = subItem["BubbleSampleFlag"] == "1" ? "BUBBLE" : "";
                else
                    product[keyHost.CELLSAMPLEFLAG].InnerText = "";

                if (subItem.ContainsKey("OQCNGFlag"))//MES 1.21 ADD PCK 用
                    product[keyHost.OQCNGRESULT].InnerText = subItem["OQCNGFlag"] == "1" ? "Y" : "N";
                else
                    product[keyHost.OQCNGRESULT].InnerText = "";

                if (subItem.ContainsKey("TTPFlag"))//sy 20151206 add ODF USE 
                    product[keyHost.CELLTTPFLAG].InnerText = subItem["TTPFlag"] == "1" ? "I" : "";//sy add by MES閻波 ODF "I" 沒有打"" 
                else
                    product[keyHost.CELLTTPFLAG].InnerText = "";
                //目前LOTPROCESSEND,LOTPROCESSABNORMALEND都沒有開這個ITEM，不知道是不是要上報，先註解
                //if (subItem.ContainsKey("CellGAPFlag"))//sy 20151206 add ODF USE 
                //    product[keyHost.GMASAMPLEFLAG].InnerText = subItem["CellGAPFlag"] == "1" ? "Y" : "N";
                //else
                //    product[keyHost.GMASAMPLEFLAG].InnerText = "";

                if (subItem.ContainsKey("CellGMIFlag"))//sy 20151206 add ODF USE 
                    product[keyHost.GMISAMPLEFLAG].InnerText = subItem["CellGMIFlag"] == "1" ? "Y" : string.Empty;
                else
                    product[keyHost.GMISAMPLEFLAG].InnerText = string.Empty;

                if (subItem.ContainsKey("TAMFlag"))//sy 20151206 add PIL USE  JobdataIO TODO
                    product[keyHost.TAMFLAG].InnerText = subItem["TAMFlag"] == "1" ? "Y" : string.Empty;
                else
                    product[keyHost.TAMFLAG].InnerText = string.Empty;

                if (subItem.ContainsKey("PTHFlag"))//sy 20160318 Modify PIL USE  JobdataIO TODO
                    product[keyHost.PTHFLAG].InnerText = subItem["PTHFlag"] == "1" ? "Y" : string.Empty;
                else
                    product[keyHost.PTHFLAG].InnerText = string.Empty;

                //huangjiayin 20170608 add CEMFLAG
                if (subItem.ContainsKey("CEMFlag"))//PIL USE  JobdataIO TODO
                    product[keyHost.CEMFLAG].InnerText = subItem["CEMFlag"] == "1" ? "Y" : string.Empty;
                else
                    product[keyHost.CEMFLAG].InnerText = string.Empty;



                if (subItem.ContainsKey("CFSideResidueFlag"))//20151205 MES TEST ADD CUT 用 MES jckim 說Flag != 1 時要回空 不能回N ，Flag = 1  SAMPLEFLAG 也要回Y
                    product[keyHost.CFSIDERESIDUEFLAG].InnerText = subItem["CFSideResidueFlag"] == "1" ? "Y" : "";
                else
                    product[keyHost.CFSIDERESIDUEFLAG].InnerText = "";

                if (subItem.ContainsKey("RibMarkFlag"))//20151205 MES TEST ADD CUT 用 MES jckim 說Flag != 1 時要回空 不能回N ，Flag = 1  SAMPLEFLAG 也要回Y
                    product[keyHost.RIBMARKFLAG].InnerText = subItem["RibMarkFlag"] == "1" ? "Y" : "";
                else
                    product[keyHost.RIBMARKFLAG].InnerText = "";

                if (subItem.ContainsKey("CFSideResidueFlag") || subItem.ContainsKey("RibMarkFlag"))
                    product[keyHost.SAMPLEFLAG].InnerText = subItem["CFSideResidueFlag"] == "1" || subItem["RibMarkFlag"] == "1" ? "Y" : "";
                else
                    product[keyHost.SAMPLEFLAG].InnerText = "";
                //目前LOTPROCESSEND,LOTPROCESSABNORMALEND都沒有開這個ITEM，不知道是不是要上報，先註解
                //if (subItem.ContainsKey("MHUFLAG"))
                //    product[keyHost.MHUFLAG].InnerText = subItem["MHUFLAG"] == "1" ? "Y" : "N";
                //else
                //    product[keyHost.MHUFLAG].InnerText = "N";
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void P2M_DecodeAbnormalCodeFlagBy_CELL(XmlNode abromalListNode, XmlNode codeNode, XmlNode product, Line line, Job job)
        {
            try
            {
                XmlNode code;

                P2M_DecodeAbnormalCodeJobDataBy_CELL(abromalListNode, codeNode, product, line, job); //Job Data AbnormalCode 非EQPFlag 需上報MES的資料 by sy
                IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");

                if (subItem == null)
                {
                    NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("Can't Decode Job Data detail, CassetteSequenceNo=[{0}], JobSequenceNo=[{1}).", job.CassetteSequenceNo, job.JobSequenceNo));
                    return;
                }

                switch (line.Data.LINETYPE)
                {
                    
                    #region [PIL]
                    case eLineType.CELL.CCPIL:
                    case eLineType.CELL.CCPIL_2:
                        code = codeNode.Clone();
                        if (subItem.ContainsKey("CoaterFlag"))
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "COATERFLAG";
                            code[keyHost.ABNORMALCODE].InnerText = subItem["CoaterFlag"] == "1" ? "Y" : "N";
                            abromalListNode.AppendChild(code);
                        }
                        code = codeNode.Clone();
                        if (subItem.ContainsKey("LCIFlag"))
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "LCIFLAG";
                            code[keyHost.ABNORMALCODE].InnerText = subItem["LCIFlag"] == "1" ? "Y" : "N";
                            abromalListNode.AppendChild(code);
                        }
                        code = codeNode.Clone();
                        //if (subItem.ContainsKey("ShellFlag"))
                        //if (subItem.ContainsKey("BackCrackFlagY") || subItem.ContainsKey("BackCrackFlagY1") || subItem.ContainsKey("BackCrackFlagN"))//sy 20151206 add PIL USE 20160318 modify MES 佳音&閰波 確認
                        if (subItem.ContainsKey("BackCrackFlagY") || subItem.ContainsKey("BackCrackFlagN"))//Y1移除不用了sy edit 20160513
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "SHELLFLAG";
                            //if (subItem["BackCrackFlagY"] == "1" || subItem["BackCrackFlagY1"] == "1")
                            if (subItem["BackCrackFlagY"] == "1")
                                code[keyHost.ABNORMALCODE].InnerText = "Y";
                            else
                                code[keyHost.ABNORMALCODE].InnerText = "";

                            if (subItem["BackCrackFlagN"] == "1")
                                code[keyHost.ABNORMALCODE].InnerText = "N";
                            abromalListNode.AppendChild(code);
                        }
                        break;
                    #endregion
                    #region [ODF]
                    case eLineType.CELL.CCODF:
                    case eLineType.CELL.CCODF_2://sy add 20160907
                        //20151030 cy: Added it in P2M_DecodeAbnormalCodeJobDataBy_CELL
                        //code = codeNode.Clone();
                        //if (subItem.ContainsKey("GLASSCHANGEANGLE"))
                        //{
                        //    code[keyHost.ABNORMALVALUE].InnerText = "GLASSCHANGEANGLE";
                        //    code[keyHost.ABNORMALCODE].InnerText = job.CellSpecial.TurnAngle;
                        //    abromalListNode.AppendChild(code);
                        //}
                        code = codeNode.Clone();
                        if (subItem.ContainsKey("SealErrorFlag"))
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "SEALERRORFLAG";
                            code[keyHost.ABNORMALCODE].InnerText = subItem["SealErrorFlag"] == "1" ? "Y" : "N";
                            abromalListNode.AppendChild(code);
                        }
                        code = codeNode.Clone();
                        if (subItem.ContainsKey("ChippingFlag"))
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "CHIPPINGFLAG";
                            code[keyHost.ABNORMALCODE].InnerText = subItem["ChippingFlag"] == "1" ? "Y" : "N";
                            abromalListNode.AppendChild(code);
                        }
                        //20151030 cy:EQPFlag have two kind LCI flag.
                        code = codeNode.Clone();
                        if (subItem.ContainsKey("EnteredLCIBeforeSMOFlag") || subItem.ContainsKey("EnteredLCIAfterSMOFlag"))
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "LCIFLAG";
                            if (subItem["EnteredLCIBeforeSMOFlag"] == "1" || subItem["EnteredLCIAfterSMOFlag"] == "1")
                                code[keyHost.ABNORMALCODE].InnerText = "Y";
                            else
                                code[keyHost.ABNORMALCODE].InnerText = "N";
                            abromalListNode.AppendChild(code);
                        }
                        code = codeNode.Clone();
                        if (subItem.ContainsKey("BubbleSampleFlag"))//不寫在ABNORMALCODE
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "BUBBLESAMPLEFLAG";
                            code[keyHost.ABNORMALCODE].InnerText = subItem["BubbleSampleFlag"] == "1" ? "Y" : "N";
                            abromalListNode.AppendChild(code);
                        }
                        break;
                    #endregion
                    #region [PCS]
                    case eLineType.CELL.CCPCS:
                        code = codeNode.Clone();
                        if (subItem.ContainsKey("ChippingFlag"))
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "CHIPPINGFLAG";
                            code[keyHost.ABNORMALCODE].InnerText = subItem["ChippingFlag"] == "1" ? "Y" : "N";
                            abromalListNode.AppendChild(code);
                        }
                        code = codeNode.Clone();
                        if (subItem.ContainsKey("SealErrorFlag"))
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "SEALERRORFLAG";
                            code[keyHost.ABNORMALCODE].InnerText = subItem["SealErrorFlag"] == "1" ? "Y" : "N";
                            abromalListNode.AppendChild(code);
                        }
                        //code = codeNode.Clone();
                        //if (subItem.ContainsKey("BubbleSampleFlag"))//不寫在ABNORMALCODE
                        //{
                        //      code[keyHost.ABNORMALVALUE].InnerText = "BUBBLESAMPLEFLAG";
                        //      code[keyHost.ABNORMALCODE].InnerText = subItem["BubbleSampleFlag"] == "1" ? "Y" : "N";
                        //      abromalListNode.AppendChild(code);
                        //}
                        break;
                    #endregion
                    #region [PDR]
                    case eLineType.CELL.CCPDR:
                        code = codeNode.Clone();
                        if (subItem.ContainsKey("CoaterFlag"))
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "COATERFLAG";
                            code[keyHost.ABNORMALCODE].InnerText = subItem["CoaterFlag"] == "1" ? "Y" : "N";
                            abromalListNode.AppendChild(code);
                        }
                        break;
                    #endregion
                    #region [PTH]
                    case eLineType.CELL.CCPTH:
                        code = codeNode.Clone();
                        //if (subItem.ContainsKey("ShellFlag"))
                        if (subItem.ContainsKey("BackCrackFlagY") || subItem.ContainsKey("BackCrackFlagY1") || subItem.ContainsKey("BackCrackFlagN"))//sy 20151206 add PIL USE 20160318 modify MES 佳音&閰波 確認
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "SHELLFLAG";
                            if (subItem["BackCrackFlagY"] == "1" || subItem["BackCrackFlagY1"] == "1")
                                code[keyHost.ABNORMALCODE].InnerText = "Y";
                            else
                                code[keyHost.ABNORMALCODE].InnerText = "";

                            if (subItem["BackCrackFlagN"] == "1")
                                code[keyHost.ABNORMALCODE].InnerText = "N";
                            abromalListNode.AppendChild(code);
                        }
                        break;
                    #endregion
                    #region [RWT]
                    case eLineType.CELL.CCRWT:
                        code = codeNode.Clone();
                        if (subItem.ContainsKey("LOIFlag"))
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "LOIFLAG";
                            code[keyHost.ABNORMALCODE].InnerText = subItem["LOIFlag"] == "1" ? "Y" : "N";
                            abromalListNode.AppendChild(code);
                        }
                        break;
                    #endregion
                    #region [SOR]
                    case eLineType.CELL.CCSOR:
                        code = codeNode.Clone();
                        if (subItem.ContainsKey("DCRandSorterFlag"))
                        {
                            code[keyHost.ABNORMALVALUE].InnerText = "SORTFLAG";
                            switch (subItem["DCRandSorterFlag"])
                            {
                                case "1":
                                    code[keyHost.ABNORMALCODE].InnerText = "1";
                                    break;
                                case "2":
                                    code[keyHost.ABNORMALCODE].InnerText = "2";
                                    break;
                                case "3":
                                    code[keyHost.ABNORMALCODE].InnerText = "3";
                                    break;
                                default:
                                    code[keyHost.ABNORMALCODE].InnerText = "";
                                    break;
                            }
                            abromalListNode.AppendChild(code);
                        }
                        break;
                    #endregion
                    default:
                        if (line.Data.LINETYPE.Contains(keyCellLineType.CUT))//sy modify  20160705 放在deault
                            #region [CUT]
                        {
                            code = codeNode.Clone();
                            if (subItem.ContainsKey("BeveledFlag"))
                            {
                                code[keyHost.ABNORMALVALUE].InnerText = "BEVELEDFLAG";
                                code[keyHost.ABNORMALCODE].InnerText = subItem["BeveledFlag"] == "1" ? "Y" : "N";
                                abromalListNode.AppendChild(code);
                            }
                            code = codeNode.Clone();
                            if (subItem.ContainsKey("CFSideResidueFlag"))
                            {
                                code[keyHost.ABNORMALVALUE].InnerText = "CFSIDERESIDUEFLAG";
                                code[keyHost.ABNORMALCODE].InnerText = subItem["CFSideResidueFlag"] == "1" ? "Y" : "N";
                                abromalListNode.AppendChild(code);
                            }
                            code = codeNode.Clone();
                            if (subItem.ContainsKey("RibMarkFlag"))
                            {
                                code[keyHost.ABNORMALVALUE].InnerText = "RIBMARKFLAG";
                                code[keyHost.ABNORMALCODE].InnerText = subItem["RibMarkFlag"] == "1" ? "Y" : "N";
                                abromalListNode.AppendChild(code);
                            }
                            code = codeNode.Clone();
                            if (subItem.ContainsKey("DimpleFlag"))
                            {
                                code[keyHost.ABNORMALVALUE].InnerText = "DIMPLEFLAG";
                                code[keyHost.ABNORMALCODE].InnerText = subItem["DimpleFlag"] == "1" ? "Y" : "N";
                                abromalListNode.AppendChild(code);
                            }
                            code = codeNode.Clone();
                            if (subItem.ContainsKey("LOIFlag"))
                            {
                                code[keyHost.ABNORMALVALUE].InnerText = "LOIFLAG";
                                code[keyHost.ABNORMALCODE].InnerText = subItem["LOIFlag"] == "1" ? "Y" : "N";
                                abromalListNode.AppendChild(code);
                            }
                            code = codeNode.Clone();
                            if (subItem.ContainsKey("CutSlimReworkFlag"))
                            {
                                code[keyHost.ABNORMALVALUE].InnerText = "CUTSLIMREWORKFLAG";
                                code[keyHost.ABNORMALCODE].InnerText = subItem["CutSlimReworkFlag"] == "1" ? "Y" : "N";
                                abromalListNode.AppendChild(code);
                            }
                            code = codeNode.Clone();
                            if (subItem.ContainsKey("CellCutRejudgeCount"))
                            {
                                code[keyHost.ABNORMALVALUE].InnerText = "CELLCUTREJUDGECOUNT";
                                code[keyHost.ABNORMALCODE].InnerText = job.CellSpecial.RejudgeCount;
                                abromalListNode.AppendChild(code);
                            }
                            break;
                        }
                        #endregion
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void P2M_DecodeAbnormalCodeJobDataBy_CELL(XmlNode abromalListNode, XmlNode codeNode, XmlNode product, Line line, Job job)
        {
            try
            {
                XmlNode code;
                switch (line.Data.LINETYPE)
                {
                    #region [PIL]
                    case eLineType.CELL.CCPIL:
                    case eLineType.CELL.CCPIL_2:
                        code = codeNode.Clone();
                        code[keyHost.ABNORMALVALUE].InnerText = "PITYPE";
                        code[keyHost.ABNORMALCODE].InnerText = job.CellSpecial.PILiquidType;
                        abromalListNode.AppendChild(code);
                        code = codeNode.Clone();
                        code[keyHost.ABNORMALVALUE].InnerText = "GLASSCHANGEANGLE";
                        code[keyHost.ABNORMALCODE].InnerText = job.CellSpecial.TurnAngle;
                        abromalListNode.AppendChild(code);
                        break;
                    #endregion
                    #region [ODF]
                    case eLineType.CELL.CCODF:
                    case eLineType.CELL.CCODF_2://sy add 20160907
                        code = codeNode.Clone();
                        code[keyHost.ABNORMALVALUE].InnerText = "GLASSCHANGEANGLE";
                        code[keyHost.ABNORMALCODE].InnerText = job.CellSpecial.TurnAngle;
                        abromalListNode.AppendChild(code);
                        break;
                    #endregion
                    #region [CRP & RWT]
                    case eLineType.CELL.CCCRP:
                    case eLineType.CELL.CCCRP_2:
                    case eLineType.CELL.CCRWT:
                        code = codeNode.Clone();
                        code[keyHost.ABNORMALVALUE].InnerText = "POINTREWORKCOUNT";
                        code[keyHost.ABNORMALCODE].InnerText = job.CellSpecial.DotRepairCount;
                        abromalListNode.AppendChild(code);
                        code = codeNode.Clone();
                        code[keyHost.ABNORMALVALUE].InnerText = "LINEREWORKCOUNT";
                        code[keyHost.ABNORMALCODE].InnerText = job.CellSpecial.LineRepairCount;
                        abromalListNode.AppendChild(code);
                        break;
                    #endregion
                    #region [PDR]
                    case eLineType.CELL.CCPDR:
                        code = codeNode.Clone();
                        code[keyHost.ABNORMALVALUE].InnerText = "PDRREWORKCOUNT";
                        code[keyHost.ABNORMALCODE].InnerText = job.CellSpecial.CurrentRwkCount;
                        abromalListNode.AppendChild(code);
                        break;
                    #endregion
                    default:
                        if (line.Data.LINETYPE.Contains(keyCellLineType.POL))//sy modify  20160705 放在deault
                        #region [POL]
                        {
                            code = codeNode.Clone();
                            code[keyHost.ABNORMALVALUE].InnerText = "POINTREWORKCOUNT";
                            code[keyHost.ABNORMALCODE].InnerText = job.CellSpecial.DotRepairCount;
                            abromalListNode.AppendChild(code);
                            code = codeNode.Clone();
                            code[keyHost.ABNORMALVALUE].InnerText = "LINEREWORKCOUNT";
                            code[keyHost.ABNORMALCODE].InnerText = job.CellSpecial.LineRepairCount;
                            abromalListNode.AppendChild(code);
                        }
                        #endregion
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ARRAY_Product_Special(ref XmlNode product, Line line, Job job)
        {
            try
            {
                product[keyHost.ALIGNERNAME].InnerText = job.MesProduct.ALIGNERNAME;

                if (line.Data.LINETYPE == eLineType.ARRAY.TTP_VTEC)
                {
                    if (job.InspJudgedData.Trim().Equals("1"))  // modify by bruce 2015/0923 add CELLTTPFLAG Item
                    {
                        product[keyHost.TTPVALUE].InnerText = "0";  //modify by bruce 2015/12/23 for MES request from "NG" to "0"
                        product[keyHost.CELLTTPFLAG].InnerText = "N";
                    }
                    else
                    {
                        product[keyHost.TTPVALUE].InnerText = "1";
                        product[keyHost.CELLTTPFLAG].InnerText = "Y";
                    }
                }
                else
                {
                    product[keyHost.TTPVALUE].InnerText = " ";
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CF_Product_Special(ref XmlNode product, Line line, Job job)
        {
            try
            {
                if (line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCOPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCSPH_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCREW_TYPE1)
                {
                    product[keyHost.INLINERWCOUNT].InnerText = job.CfSpecial.InlineReworkRealCount.ToString();
                }

                if (line.Data.LINETYPE == eLineType.CF.FCSPH_TYPE1)
                {
                    if (job.CfSpecial.CFCurrentProcess == "BM")
                    {
                        product[keyHost.ALIGNERNAME].InnerText = string.Format("{0},{1}", job.CfSpecial.TrackingData.BMPS_Exposure, job.CfSpecial.TrackingData.BMPS_Exposure2);
                    }
                }
                if (line.Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_BMPS || line.Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_GRB)
                {
                    product[keyHost.MACROFLAG].InnerText = job.CFMarcoReserveFlag;
                }

                if (line.Data.LINETYPE == eLineType.CF.FCPSH_TYPE1)
                {
                    if (job.CfSpecial.RecyclingFlag == "Y" && job.CfSpecial.CFSpecialReserved.ForcePSHbit == "1")
                    {
                        product[keyHost.RECYCLINGFLAG].InnerText = "N";
                    }
                    else
                    {
                        product[keyHost.RECYCLINGFLAG].InnerText = job.CfSpecial.CFSpecialReserved.ForcePSHbit == "1" ? "Y" : "N";
                    }
                }
                else
                {
                    product[keyHost.RECYCLINGFLAG].InnerText = "N";
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CELL_Product_Special(ref XmlNode bodyNode, ref XmlNode product, Line line, Job job, Port port ,ref string cstGrade)
            {
                try
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                    IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");

                    if (subItem == null)
                    {
                        NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("Can't Decode Job Data detail, CassetteSequenceNo=[{0}], JobSequenceNo=[{1}).", job.CassetteSequenceNo, job.JobSequenceNo));
                        return;
                    }
                    //ALL
                    product[keyHost.BLOCKJUDGES].InnerText = job.CellSpecial.BlockOXInformation.Substring(0, int.Parse(job.CellSpecial.BlockCount));
                    //PIL
                    product[keyHost.PRODUCTTYPE].InnerText = ObjectManager.JobManager.P2M_GetJobType(job.JobType);
                    //CUT RWT NRP
                    product[keyHost.REASONCODE].InnerText = job.CellSpecial.DefectCode.Trim();//modify by huangjiayin:20161220 去掉收尾空格
                    //CUT
                    if (line.Data.LINETYPE.Contains(keyCellLineType.CUT) && job.SubstrateType == eSubstrateType.Chip)
                    {
                        product[keyHost.SUBPRODUCTGRADES].InnerText = ObjectManager.JobManager.P2M_CELL_PanelInt2OX(job.CellSpecial.PanelOXInformation);
                    }
                    //PIL\ODF\PDR\TAM\PTH\GAP        //MES PRODUCTJUDGE轉換PRODUCTGRADE//CUT 是by EQP                    
                    if (line.Data.LINEID.Contains(keyCellLineType.PIL) || line.Data.LINEID.Contains(keyCellLineType.ODF) || line.Data.LINETYPE == eLineType.CELL.CCPDR ||
                       line.Data.LINETYPE == eLineType.CELL.CCTAM || line.Data.LINETYPE == eLineType.CELL.CCPTH || line.Data.LINETYPE == eLineType.CELL.CCGAP
                        || (eqp.Data.NODEID.Contains(keyCELLMachingName.CCBUR)))//sy add 20160907
                    {
                        string mesGrade = string.Empty;
                        mesGrade = ConstantManager[string.Format("{0}_JOBJUDGE_CHANGE_GRADE_MES", line.Data.FABTYPE)][GetProductJudge(line, eFabType.CELL, job)].Value;
                        #region [CUT BUR Updata PRODUCTGRADE Rule]
                        if (eqp.Data.NODEID.Contains(keyCELLMachingName.CCBUR)) //sy add for 20160518羅俊 RJ => FP , RW => CG
                        {
                            switch (GetProductJudge(line, eFabType.CELL, job))
                            {
                                case "RJ":
                                    mesGrade = "FP";
                                    break;
                                case "R":
                                    mesGrade = "CG";
                                    break;
                                default:
                                    break;
                            }
                        }
                        #endregion

                        product[keyHost.PRODUCTGRADE].InnerText = mesGrade;
                        #region [CSTMAPGRADE]
                        if (cstGrade != mesGrade && cstGrade != string.Empty)
                        {
                            cstGrade = "MIX";
                        }
                        else
                        {
                            cstGrade = mesGrade;                                                    
                        }
                        bodyNode[keyHost.CSTMAPGRADE].InnerText = cstGrade;
                        #endregion
                    }
                    //PCS                              VCR mismatch  NG back flow  
                    if (line.Data.LINETYPE == eLineType.CELL.CCPCS && port.Data.NODENO == "L2" && subItem.ContainsKey("VCRMismatchAndBCReplyNG"))
                        product[keyHost.HOLDFLAG].InnerText = subItem["VCRMismatchAndBCReplyNG"] == "1" ? "Y" : "";
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                }
            }
    }
}
