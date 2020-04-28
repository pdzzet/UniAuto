using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public class Node
    {
        #region DB
        public string LineID { get; set; }
        public string ServerName { get; set; }
        public string NodeNo { get; set; }
        public string NodeID { get; set; }
        public string NodeName { get; set; }

        public string ReportMode { get; set; }  
        public string NodeAttribute { get; set; }
        public string OPISpecialType { get; set; }
        public string DefaultRecipeNo { get; set; }

        public int UnitCount { get; set; }
        public int RecipeLen { get; set; }
        public List<int> RecipeSeq { get; set; }
               
        public bool RecipeRegisterCheck { get; set; }
        public bool RecipeParameterCheck { get; set; }

        public bool APCReport { get; set; }
        public bool EnergyReport { get; set; }
        public int APCReportTime { get; set; }
        public int EnergyReportTime { get; set; }

        public string UseRunMode { get; set; } //Y : 有run mode 且OPI提供run mode切換, R : 有run mode 且OPI不提供run mode切換 ,N: 沒有run mode
        public bool UseIndexerMode { get; set; }
        public bool MaterialStatus { get; set; }  //0: OFF ,1: ON

        public string UseEDCReport { get; set; }

        public string LastReceiveGlassID { get; set; }  //最後收片的glass id
        public string LastReceiveDateTime { get; set; } //最後收片的時間
        public bool IsOverTackTime { get; set; }        //是否超過tack time設定未收片
        #endregion

        public eEQPStatus EQPStatus { get; set; }

        public string RecipeName { get; set; }

        public string EQPRunMode { get; set; }

        public bool OxinfoCheckMode { get; set; } //add by qiumin 20180607
        //public string EQPRunMode2 { get; set; }

        //public int FlowPriority { get; set; }

        public List<string> AllowUnitRunMode { get; set; }  //記錄哪些EQ run mode下允許設定unit run mode 
        public List<LineRunMode> LineRunModes { get; set; }  //EQ Run mode
        public List<LineRunMode> LineUnitRunModes { get; set; } //Unit Run Mode

        ////for ATS Loader Operation Mode (CBATS )
        //public eLoaderOperationMode LoaderOperationMode { get; set; }

        public eEQPOperationMode OperationMode { get; set; }

        #region count
        public int TotalCount { get; set; }
        public int TFTJobCount { get; set; }
        public int CFJobCount { get; set; }
        public int DummyJobCount { get; set; }
        public int ThroughDummyJobCount { get; set; }
        public int ThicknessDummyJobCount { get; set; }
        public int UVMASKJobCount { get; set; }
        public int UnassembledTFTJobCount { get; set; }//sy add 20160826
        public int ITODummyJobCount { get; set; }//sy add 20160826
        public int NIPDummyJobCount { get; set; }//sy add 20160826
        public int MetalOneDummyJobCount { get; set; }//sy add 20160826
        #endregion
                  
        #region Equipment Status
        public string EquipmentAlive { get; set; }
        public eCIMMode CIMMode { get; set; }
        public string UpStreamInlineMode { get; set; }
        public string DownStreamInlineMode { get; set; }
        public bool AlarmStatus { get; set; }
        public string AutoRecipeChange { get; set; }
        public string PartialFullMode { get; set; }
        #endregion

        #region Special Mode Report
        public string ByPassMode { get; set; }
        public bool TurnTableMode { get; set; }
        public bool ByPassInsp01Mode { get; set; }
        public bool ByPassInsp02Mode { get; set; }        
        public bool NextLineBCStatus { get; set; }
        public bool CV07Status { get; set; }         //for CF Photo
        public eHighCVMode HighCVMode { get; set; }
        #endregion

        #region Job Data Check Mode
        public bool JobDataCheckMode { get; set; }
        public bool COAVersionCheckMode  { get; set; }
        public bool JobDuplicateCheckMode { get; set; }
        public bool PorductIDCheckMode { get; set; }
        public bool GroupIndexCheckMode { get; set; }
        public bool ProductTypeCheckMode { get; set; }
        public bool RecipeIDCheckMode { get; set; }
        #endregion

        public int CassetteQTime { get; set; }
        public string CSTOperationMode { get; set; }
        public string HSMSStatus { get; set; }
        public string HSMSControlMode { get; set; }

        public int InspectionIdleTime { get; set; }

        public List<string> Lst_TrackTimeUnit { get; set; }
        public Dictionary<string, string> Dic_TrackDelayTime { get; set; }

        public List<VCR> VCRs { get; set; }
        public List<InterLock> InterLocks { get; set; }
        //public List<LineRunMode> LineRunModes { get; set; }
        public List<SamplingSide> SamplingSides { get; set; }        
        //public List<SBRM_OPI_OBJECT_DEF> HideContrls { get; set; }

        public Dictionary<string, BCS_EachPositionReply> Dic_Position { get; set; } //Key: position unit no (兩碼)
        public BCS_EquipmentAlarmStatusReply BC_EquipmentAlarmStatusReply;
        public BCS_IonizerFanModeReportReply BC_IonizerFanModeReportReply;
        public BCS_DefectCodeReply BC_DefectCodeReportReply;
        public Node()
        {
            EQPRunMode = string.Empty;
            UpStreamInlineMode = string.Empty;
            DownStreamInlineMode = string.Empty;
            RecipeName = string.Empty;
            OxinfoCheckMode = false;  //add by qiumin 20180607
            LastReceiveGlassID=string.Empty ;
            LastReceiveDateTime=string.Empty ;
            IsOverTackTime=false;

            AllowUnitRunMode = new List<string>();
            LineRunModes = new List<LineRunMode>();
            LineUnitRunModes = new List<LineRunMode>();

            InspectionIdleTime = 0;

            RecipeSeq = new List<int>();

            Dic_Position = new Dictionary<string, BCS_EachPositionReply>();
            
            Lst_TrackTimeUnit = new List<string>();
            Dic_TrackDelayTime = new Dictionary<string, string>();

            BC_EquipmentAlarmStatusReply = new BCS_EquipmentAlarmStatusReply();
            BC_IonizerFanModeReportReply = new BCS_IonizerFanModeReportReply();
            BC_DefectCodeReportReply = new BCS_DefectCodeReply();
            //BC_ProductTypeInfoRequestReply = new BCS_ProductTypeInfoRequestReply();
        }

        #region Get Node Info -- EquipmentStatusReport
        public void SetNodeInfo(EquipmentStatusReport NodeData)
        {
            int _num = 0;

            #region Count
            this.CFJobCount = (int.TryParse(NodeData.BODY.CFJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.CFJOBCNT) : 0;
            this.DummyJobCount = (int.TryParse(NodeData.BODY.DMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.DMYJOBCNT) : 0;
            this.TFTJobCount = (int.TryParse(NodeData.BODY.TFTJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.TFTJOBCNT) : 0;
            this.ThicknessDummyJobCount = (int.TryParse(NodeData.BODY.THICKNESSDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.THICKNESSDMYJOBCNT) : 0;
            this.ThroughDummyJobCount = (int.TryParse(NodeData.BODY.THROUGHDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.THROUGHDMYJOBCNT) : 0;
            this.UnassembledTFTJobCount = (int.TryParse(NodeData.BODY.UNASSEMBLEDTFTDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.UNASSEMBLEDTFTDMYJOBCNT) : 0;//sy add 20160826
            this.ITODummyJobCount = (int.TryParse(NodeData.BODY.ITODMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.ITODMYJOBCNT) : 0;//sy add 20160826
            this.NIPDummyJobCount = (int.TryParse(NodeData.BODY.NIPDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.NIPDMYJOBCNT) : 0;//sy add 20160826
            this.MetalOneDummyJobCount = (int.TryParse(NodeData.BODY.METALONEDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.METALONEDMYJOBCNT) : 0;//sy add 20160826
            this.UVMASKJobCount = (int.TryParse(NodeData.BODY.UVMASKJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.UVMASKJOBCNT) : 0;
            #endregion

            #region Word
            this.OperationMode = NodeData.BODY.EQPOPERATIONMODE == string.Empty ? eEQPOperationMode.Unknown : (eEQPOperationMode)int.Parse(NodeData.BODY.EQPOPERATIONMODE);
            this.CIMMode = NodeData.BODY.CIMMODE == string.Empty ? eCIMMode.OFF : (eCIMMode)int.Parse(NodeData.BODY.CIMMODE);
            this.RecipeName = NodeData.BODY.CURRENTRECIPEID;
            this.EQPStatus = NodeData.BODY.CURRENTSTATUS == string.Empty ? eEQPStatus.UnKnown : (eEQPStatus)int.Parse(NodeData.BODY.CURRENTSTATUS);
            this.AlarmStatus = NodeData.BODY.LOCALALARMSTATUS == "ON" ? true : false;
            this.CassetteQTime = (int.TryParse(NodeData.BODY.CASSETTEQTIME, out _num) == true) ? int.Parse(NodeData.BODY.CASSETTEQTIME) : 0;
            this.InspectionIdleTime = (int.TryParse(NodeData.BODY.INSPECTIONIDLETIME, out _num) == true) ? int.Parse(NodeData.BODY.INSPECTIONIDLETIME) : 0;
            this.HSMSStatus = NodeData.BODY.HSMSSTATUS.ToString();
            this.HSMSControlMode = NodeData.BODY.HSMSCONTROLMODE.ToString();
            this.EquipmentAlive = NodeData.BODY.EQUIPMENTALIVE == "1" ? "Alive" : "Down";
            this.DownStreamInlineMode = NodeData.BODY.DOWNSTREAMINLINEMODE;
            this.UpStreamInlineMode = NodeData.BODY.UPSTREAMINLINEMODE;
            this.AutoRecipeChange = NodeData.BODY.AUTORECIPECHANGEMODE;
            this.ByPassMode = NodeData.BODY.BYPASSMODE;
            this.PartialFullMode = NodeData.BODY.PARTIALFULLMODE == "1" ? "ON" : "OFF";
            this.LastReceiveGlassID = NodeData.BODY.LASTGLASSID==null? string.Empty : NodeData.BODY.LASTGLASSID;
            this.LastReceiveDateTime = NodeData.BODY.LASTRECIVETIME == null ? string.Empty : NodeData.BODY.LASTRECIVETIME;
            this.HighCVMode = NodeData.BODY.HIGHCVMODE == string.Empty ? eHighCVMode.UnKnown : (eHighCVMode)int.Parse(NodeData.BODY.HIGHCVMODE);
            //this.LoaderOperationMode = NodeData.BODY.LOADEROPERATIONMODE_ATS==string.Empty ?  eLoaderOperationMode.Unknown : (eLoaderOperationMode)int.Parse(NodeData.BODY.LOADEROPERATIONMODE_ATS);

           
            if (NodeData.BODY.EQUIPMENTRUNMODE == string.Empty)
            {
                if (FormMainMDI.G_OPIAp.CurLine.LineType.Equals("FCUPK_TYPE1") && NodeNo.Equals("L5"))
                {
                    if (this.EQPRunMode != string.Empty) FormMainMDI.G_OPIAp.RunModeHaveChange = true;
                }

                this.EQPRunMode = string.Empty;
            }
            else
            {
                string _newRunMode = GetRunModeDesc(NodeData.BODY.EQUIPMENTRUNMODE);

                if (FormMainMDI.G_OPIAp.CurLine.LineType.Equals("FCUPK_TYPE1") && NodeNo.Equals("L5"))
                {
                    if (this.EQPRunMode != _newRunMode) FormMainMDI.G_OPIAp.RunModeHaveChange = true;
                }

                this.EQPRunMode = _newRunMode;
                
            }

            //if (NodeData.BODY.EQUIPMENTRUNMODE2 == string.Empty) this.EQPRunMode2 = string.Empty;
            //else
            //{
            //    this.EQPRunMode2 = GetRunModeDesc(NodeData.BODY.EQUIPMENTRUNMODE2);
            //}

            #region CST Operation Mode -- 僅有LD / LU / UPK會上報
            if (NodeAttribute == "LD" || NodeAttribute == "LU" || NodeAttribute == "UPK")
            {
                //KTOK = 0, CTOC = 1, LTOL = 2
                if (NodeData.BODY.CSTOPERMODE.ToString() == "KTOK") FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.KindToKind;
                else if (NodeData.BODY.CSTOPERMODE.ToString() == "CTOC") FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.CassetteToCassette;
                else if (NodeData.BODY.CSTOPERMODE.ToString() == "LTOL") FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.LotToLot;
                else FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.Unknown;
            }
            #endregion
            
            #endregion

            #region Bit
            TurnTableMode = NodeData.BODY.TURNTABLEMODE=="1"?true:false ;

            this.ByPassInsp01Mode = NodeData.BODY.BYPASSINSP01MODE == "1" ? true : false;
            this.ByPassInsp02Mode = NodeData.BODY.BYPASSINSP02MODE == "1" ? true : false;            
            this.NextLineBCStatus = NodeData.BODY.NEXTLINEBCSTATUS == "1" ? true : false;
            this.JobDataCheckMode = NodeData.BODY.JOBDATACHECKMODE == "1" ? true : false;
            this.COAVersionCheckMode = NodeData.BODY.COAVERSIONCHECKMODE == "1" ? true : false;
            this.JobDuplicateCheckMode = NodeData.BODY.JOBDUPLICATECHECKMODE == "1" ? true : false;
            this.ProductTypeCheckMode = NodeData.BODY.PRODUCTTYPECHECKMODE == "1" ? true : false;
            this.GroupIndexCheckMode = NodeData.BODY.GROUPINDEXCHECKMODE == "1" ? true : false;
            this.RecipeIDCheckMode = NodeData.BODY.RECIPEIDCHECKMODE == "1" ? true : false;
            this.PorductIDCheckMode = NodeData.BODY.PRODUCTIDCHECKMODE == "1" ? true : false;
            this.CV07Status = NodeData.BODY.CV07_STATUS == "1" ? true : false;

            MaterialStatus = NodeData.BODY.MATERIALSTATUS == "1" ? true : false;  //0: OFF,1: ON
            IsOverTackTime = NodeData.BODY.TIMEOUTFLAG == "1" ? true : false;


            #endregion

            #region 更新partial full mode給 port物件
            foreach (Port _port in FormMainMDI.G_OPIAp.Dic_Port.Values)
            {
                if (_port.NodeNo == NodeData.BODY.EQUIPMENTNO)
                    _port.PartialFullMode = NodeData.BODY.PARTIALFULLMODE == "1" ? true : false;
            }
            #endregion

            #region List
            foreach (EquipmentStatusReport.UNITc _unit in NodeData.BODY.UNITLIST)
            {
                string _unitKey = NodeData.BODY.EQUIPMENTNO.PadRight(3, ' ') + _unit.UNITNO.PadLeft(2, '0');
                if (FormMainMDI.G_OPIAp.Dic_Unit.ContainsKey(_unitKey))
                {
                    FormMainMDI.G_OPIAp.Dic_Unit[_unitKey].SetUnitInfo(_unit);
                }
            }

            foreach (EquipmentStatusReport.VCRc _vcr in NodeData.BODY.VCRLIST)
            {
                VCR vcr = this.VCRs.Find(d => d.VCRNO.Equals(_vcr.VCRNO.ToString().PadLeft(2,'0')));
                if (vcr == null) continue;
                vcr.Status = _vcr.VCRENABLEMODE==string.Empty ? eVCRMode.DISABLE: (eVCRMode)int.Parse(_vcr.VCRENABLEMODE);
            }

            foreach (EquipmentStatusReport.INTERLOCKc _interlock in NodeData.BODY.INTERLOCKLIST)
            {
                InterLock interlock = this.InterLocks.Find(d => d.PLCTrxNo.Equals(_interlock.INTERLOCKNO.ToString().PadLeft(2, '0')));
                if (interlock == null) continue;
                interlock.Status = _interlock.INTERLOCKSTATUS==string.Empty ? eInterlockMode.OFF :(eInterlockMode)int.Parse(_interlock.INTERLOCKSTATUS);
            }

            foreach (EquipmentStatusReport.SAMPLINGSIDEc _side in NodeData.BODY.SAMPLINGSIDELIST)
            {
                SamplingSide samplingSize = this.SamplingSides.Find(d => d.ItemName.Equals(_side.ITEMNAME.ToString()));
                if (samplingSize == null) continue;
                samplingSize.SideStatus = _side.SIDESTATUS ;
            }
            #endregion
        }
        #endregion

        #region Get Node Info -- EquipmentStatusReply
        public void SetNodeInfo(EquipmentStatusReply NodeData)
        {
            int _num = 0;

            #region Count
            this.CFJobCount = (int.TryParse(NodeData.BODY.CFJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.CFJOBCNT) : 0;
            this.DummyJobCount = (int.TryParse(NodeData.BODY.DMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.DMYJOBCNT) : 0;
            this.TFTJobCount = (int.TryParse(NodeData.BODY.TFTJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.TFTJOBCNT) : 0;
            this.ThicknessDummyJobCount = (int.TryParse(NodeData.BODY.THICKNESSDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.THICKNESSDMYJOBCNT) : 0;
            this.ThroughDummyJobCount = (int.TryParse(NodeData.BODY.THROUGHDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.THROUGHDMYJOBCNT) : 0;
            this.UnassembledTFTJobCount = (int.TryParse(NodeData.BODY.UNASSEMBLEDTFTDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.UNASSEMBLEDTFTDMYJOBCNT) : 0;//sy add 20160826
            this.ITODummyJobCount = (int.TryParse(NodeData.BODY.ITODMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.ITODMYJOBCNT) : 0;//sy add 20160826
            this.NIPDummyJobCount = (int.TryParse(NodeData.BODY.NIPDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.NIPDMYJOBCNT) : 0;//sy add 20160826
            this.MetalOneDummyJobCount = (int.TryParse(NodeData.BODY.METALONEDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.METALONEDMYJOBCNT) : 0;//sy add 20160826
            this.UVMASKJobCount = (int.TryParse(NodeData.BODY.UVMASKJOBCNT, out _num) == true) ? int.Parse(NodeData.BODY.UVMASKJOBCNT) : 0;
            #endregion

            #region Word
            this.OperationMode = NodeData.BODY.EQPOPERATIONMODE == string.Empty ? eEQPOperationMode.Unknown : (eEQPOperationMode)int.Parse(NodeData.BODY.EQPOPERATIONMODE);
            this.CIMMode = (eCIMMode)int.Parse(NodeData.BODY.CIMMODE);
            this.RecipeName = NodeData.BODY.CURRENTRECIPEID;
            this.EQPStatus = NodeData.BODY.CURRENTSTATUS == string.Empty ? eEQPStatus.UnKnown : (eEQPStatus)int.Parse(NodeData.BODY.CURRENTSTATUS);
            this.EQPRunMode = NodeData.BODY.EQUIPMENTRUNMODE;
            this.AlarmStatus = (NodeData.BODY.LOCALALARMSTATUS == "ON" ? true : false);
            this.CassetteQTime = (int.TryParse(NodeData.BODY.CASSETTEQTIME, out _num) == true) ? int.Parse(NodeData.BODY.CASSETTEQTIME) : 0;
            this.InspectionIdleTime = (int.TryParse(NodeData.BODY.INSPECTIONIDLETIME, out _num) == true) ? int.Parse(NodeData.BODY.INSPECTIONIDLETIME) : 0;
            this.HSMSStatus = NodeData.BODY.HSMSSTATUS.ToString();
            this.HSMSControlMode = NodeData.BODY.HSMSCONTROLMODE.ToString();
            this.EquipmentAlive = NodeData.BODY.EQUIPMENTALIVE == "1" ? "Alive" : "Down";
            this.DownStreamInlineMode = NodeData.BODY.DOWNSTREAMINLINEMODE;
            this.UpStreamInlineMode = NodeData.BODY.UPSTREAMINLINEMODE;
            this.AutoRecipeChange = NodeData.BODY.AUTORECIPECHANGEMODE;
            this.ByPassMode = NodeData.BODY.BYPASSMODE;
            this.PartialFullMode =  NodeData.BODY.PARTIALFULLMODE == "1" ? "ON" : "OFF";
            //this.LoaderOperationMode = NodeData.BODY.LOADEROPERATIONMODE_ATS==string.Empty ?  eLoaderOperationMode.Unknown : (eLoaderOperationMode)int.Parse(NodeData.BODY.LOADEROPERATIONMODE_ATS);
            this.LastReceiveGlassID = NodeData.BODY.LASTGLASSID == null ? string.Empty : NodeData.BODY.LASTGLASSID;
            this.LastReceiveDateTime = NodeData.BODY.LASTRECIVETIME == null ? string.Empty : NodeData.BODY.LASTRECIVETIME;
            this.HighCVMode = this.HighCVMode = NodeData.BODY.HIGHCVMODE == string.Empty ? eHighCVMode.UnKnown : (eHighCVMode)int.Parse(NodeData.BODY.HIGHCVMODE);      

            if (NodeData.BODY.EQUIPMENTRUNMODE == string.Empty)
            {
                if (FormMainMDI.G_OPIAp.CurLine.LineType.Equals("FCUPK_TYPE1") && NodeNo.Equals("L5"))
                {
                    if (this.EQPRunMode != string.Empty) FormMainMDI.G_OPIAp.RunModeHaveChange = true;
                }

                this.EQPRunMode = string.Empty;
            }
            else
            {
                string _newRunMode =GetRunModeDesc(NodeData.BODY.EQUIPMENTRUNMODE);

                if (FormMainMDI.G_OPIAp.CurLine.LineType.Equals("FCUPK_TYPE1") && NodeNo.Equals("L5"))
                {
                    if (this.EQPRunMode != _newRunMode) FormMainMDI.G_OPIAp.RunModeHaveChange = true;
                }

                this.EQPRunMode = _newRunMode;

            }

            //if (NodeData.BODY.EQUIPMENTRUNMODE2 == string.Empty) this.EQPRunMode2 = string.Empty;
            //else
            //{
            //    //int.TryParse(NodeData.BODY.EQUIPMENTRUNMODE2, out _num);

            //    this.EQPRunMode2 = GetRunModeDesc(NodeData.BODY.EQUIPMENTRUNMODE2);

            //}

            #region CST Operation Mode -- 僅有LD / LU / UPK會上報
            if (NodeAttribute == "LD" || NodeAttribute == "LU" || NodeAttribute == "UPK")
            {
                //KTOK = 0, CTOC = 1, LTOL = 2
                if (NodeData.BODY.CSTOPERMODE.ToString() == "KTOK") FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.KindToKind;
                else if (NodeData.BODY.CSTOPERMODE.ToString() == "CTOC") FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.CassetteToCassette;
                else if (NodeData.BODY.CSTOPERMODE.ToString() == "LTOL") FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.LotToLot;
                else FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.Unknown;
            }
            #endregion

            #endregion

            #region Bit
            this.TurnTableMode = NodeData.BODY.TURNTABLEMODE == "1" ? true : false;
            this.ByPassInsp01Mode = NodeData.BODY.BYPASSINSP01MODE == "1" ? true : false;
            this.ByPassInsp02Mode = NodeData.BODY.BYPASSINSP02MODE == "1" ? true : false;            
            this.NextLineBCStatus = NodeData.BODY.NEXTLINEBCSTATUS == "1" ? true : false;
            this.JobDataCheckMode = NodeData.BODY.JOBDATACHECKMODE == "1" ? true : false;
            this.COAVersionCheckMode = NodeData.BODY.COAVERSIONCHECKMODE == "1" ? true : false;
            this.JobDuplicateCheckMode = NodeData.BODY.JOBDUPLICATECHECKMODE == "1" ? true : false;
            this.ProductTypeCheckMode = NodeData.BODY.PRODUCTTYPECHECKMODE == "1" ? true : false;
            this.GroupIndexCheckMode = NodeData.BODY.GROUPINDEXCHECKMODE == "1" ? true : false;
            this.RecipeIDCheckMode = NodeData.BODY.RECIPEIDCHECKMODE == "1" ? true : false;
            this.PorductIDCheckMode = NodeData.BODY.PRODUCTIDCHECKMODE == "1" ? true : false;
            this.MaterialStatus = NodeData.BODY.MATERIALSTATUS == "1" ? true : false;  //0: OFF,1: ON
            this.IsOverTackTime = NodeData.BODY.TIMEOUTFLAG == "1" ? true : false;
            this.CV07Status = NodeData.BODY.CV07_STATUS == "1" ? true : false;
            

            #endregion
          
            #region List
            foreach (EquipmentStatusReply.UNITc _unit in NodeData.BODY.UNITLIST)
            {

                string _unitKey = NodeData.BODY.EQUIPMENTNO.PadRight(3, ' ') + _unit.UNITNO.PadLeft(2, '0');
                if (FormMainMDI.G_OPIAp.Dic_Unit.ContainsKey(_unitKey))
                {
                    FormMainMDI.G_OPIAp.Dic_Unit[_unitKey].SetUnitInfo(_unit);
                }
            }

            foreach (EquipmentStatusReply.VCRc _vcr in NodeData.BODY.VCRLIST)
            {
                VCR vcr = this.VCRs.Find(d => d.VCRNO.Equals(_vcr.VCRNO.ToString().PadLeft(2, '0')));
                if (vcr == null) continue;
                vcr.Status = _vcr.VCRENABLEMODE==string.Empty ? eVCRMode.DISABLE : (eVCRMode)int.Parse(_vcr.VCRENABLEMODE);
            }

            foreach (EquipmentStatusReply.INTERLOCKc _interlock in NodeData.BODY.INTERLOCKLIST)
            {
                InterLock interlock = this.InterLocks.Find(d => d.PLCTrxNo.Equals(_interlock.INTERLOCKNO.ToString().PadLeft(2, '0')));
                if (interlock == null) continue;
                interlock.Status = _interlock.INTERLOCKSTATUS==string.Empty ? eInterlockMode.OFF : (eInterlockMode)int.Parse(_interlock.INTERLOCKSTATUS);
            }

            foreach (EquipmentStatusReply.SAMPLINGSIDEc _side in NodeData.BODY.SAMPLINGSIDELIST)
            {
                SamplingSide samplingSize = this.SamplingSides.Find(d => d.ItemName.Equals(_side.ITEMNAME.ToString()));
                if (samplingSize == null) continue;
                samplingSize.SideStatus = _side.SIDESTATUS;
            }
            #endregion
        }
        #endregion

        #region Get Node Info -- AllDataUpdateReply
        public void SetNodeInfo(AllDataUpdateReply.EQUIPMENTc NodeData)
        {
            int _num = 0;

            #region Count
            this.CFJobCount = (int.TryParse(NodeData.CFJOBCNT, out _num) == true) ? int.Parse(NodeData.CFJOBCNT) : 0;
            this.DummyJobCount = (int.TryParse(NodeData.DMYJOBCNT, out _num) == true) ? int.Parse(NodeData.DMYJOBCNT) : 0;
            this.TFTJobCount = (int.TryParse(NodeData.TFTJOBCNT, out _num) == true) ? int.Parse(NodeData.TFTJOBCNT) : 0;
            this.ThicknessDummyJobCount = (int.TryParse(NodeData.THICKNESSDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.THICKNESSDMYJOBCNT) : 0;
            this.ThroughDummyJobCount = (int.TryParse(NodeData.THROUGHDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.THROUGHDMYJOBCNT) : 0;
            this.UnassembledTFTJobCount = (int.TryParse(NodeData.UNASSEMBLEDTFTDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.UNASSEMBLEDTFTDMYJOBCNT) : 0;//sy add 20160826
            this.ITODummyJobCount = (int.TryParse(NodeData.ITODMYJOBCNT, out _num) == true) ? int.Parse(NodeData.ITODMYJOBCNT) : 0;//sy add 20160826
            this.NIPDummyJobCount = (int.TryParse(NodeData.NIPDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.NIPDMYJOBCNT) : 0;//sy add 20160826
            this.MetalOneDummyJobCount = (int.TryParse(NodeData.METALONEDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.METALONEDMYJOBCNT) : 0;//sy add 20160826
            this.UVMASKJobCount = (int.TryParse(NodeData.UVMASKJOBCNT, out _num) == true) ? int.Parse(NodeData.UVMASKJOBCNT) : 0;
            #endregion

            #region Word
            this.OperationMode = NodeData.EQPOPERATIONMODE == string.Empty ? eEQPOperationMode.Unknown : (eEQPOperationMode)int.Parse(NodeData.EQPOPERATIONMODE);
            this.CIMMode = (eCIMMode)int.Parse(NodeData.CIMMODE);
            this.RecipeName = NodeData.CURRENTRECIPEID;
            this.EQPStatus = NodeData.CURRENTSTATUS == string.Empty ? eEQPStatus.UnKnown : (eEQPStatus)int.Parse(NodeData.CURRENTSTATUS);
            //this.EQPRunMode = NodeData.EQUIPMENTRUNMODE;
            this.AlarmStatus = NodeData.LOCALALARMSTATUS == "ON" ? true : false;
            this.CassetteQTime = (int.TryParse(NodeData.CASSETTEQTIME, out _num) == true) ? int.Parse(NodeData.CASSETTEQTIME) : 0;
            this.InspectionIdleTime = (int.TryParse(NodeData.INSPECTIONIDLETIME, out _num) == true) ? int.Parse(NodeData.INSPECTIONIDLETIME) : 0;
            this.HSMSStatus = NodeData.HSMSSTATUS.ToString();
            this.HSMSControlMode = NodeData.HSMSCONTROLMODE.ToString();
            this.EquipmentAlive = NodeData.EQUIPMENTALIVE == "1" ? "Alive" : "Down";
            this.DownStreamInlineMode = NodeData.DOWNSTREAMINLINEMODE;
            this.UpStreamInlineMode = NodeData.UPSTREAMINLINEMODE;
            this.AutoRecipeChange = NodeData.AUTORECIPECHANGEMODE;
            this.ByPassMode = NodeData.BYPASSMODE;
            this.PartialFullMode =  NodeData.PARTIALFULLMODE == "1" ? "ON" : "OFF";
            //this.LoaderOperationMode = NodeData.LOADEROPERATIONMODE_ATS == string.Empty ? eLoaderOperationMode.Unknown : (eLoaderOperationMode)int.Parse(NodeData.LOADEROPERATIONMODE_ATS);
            this.LastReceiveGlassID = NodeData.LASTGLASSID == null ? string.Empty : NodeData.LASTGLASSID;
            this.LastReceiveDateTime = NodeData.LASTRECIVETIME == null ? string.Empty : NodeData.LASTRECIVETIME;
            this.HighCVMode = this.HighCVMode = NodeData.HIGHCVMODE == string.Empty ? eHighCVMode.UnKnown : (eHighCVMode)int.Parse(NodeData.HIGHCVMODE);      


            if (NodeData.EQUIPMENTRUNMODE == string.Empty)
            {
                if (FormMainMDI.G_OPIAp.CurLine.LineType.Equals("FCUPK_TYPE1") && NodeNo.Equals("L5"))
                {
                    if (this.EQPRunMode != string.Empty) FormMainMDI.G_OPIAp.RunModeHaveChange = true;
                }

                this.EQPRunMode = string.Empty;
            }
            else
            {
                string _newRunMode = GetRunModeDesc(NodeData.EQUIPMENTRUNMODE);

                if (FormMainMDI.G_OPIAp.CurLine.LineType.Equals("FCUPK_TYPE1") && NodeNo.Equals("L5"))
                {
                    if (this.EQPRunMode != _newRunMode) FormMainMDI.G_OPIAp.RunModeHaveChange = true;
                }

                this.EQPRunMode = _newRunMode;

            }

            //if (NodeData.EQUIPMENTRUNMODE2 == string.Empty) this.EQPRunMode2 = string.Empty;
            //else
            //{
            //    this.EQPRunMode2 = GetRunModeDesc(NodeData.EQUIPMENTRUNMODE2);
            //}

            #region CST Operation Mode -- 僅有LD / LU / UPK會上報
            if (NodeAttribute == "LD" || NodeAttribute == "LU" || NodeAttribute == "UPK")
            {
                //KTOK = 0, CTOC = 1, LTOL = 2
                if (NodeData.CSTOPERMODE.ToString() == "KTOK") FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.KindToKind;
                else if (NodeData.CSTOPERMODE.ToString() == "CTOC") FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.CassetteToCassette;
                else if (NodeData.CSTOPERMODE.ToString() == "LTOL") FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.LotToLot;
                else FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode = eCSTOperationMode.Unknown;
            }
            #endregion

            #endregion

            #region Bit
            this.TurnTableMode = NodeData.TURNTABLEMODE=="1" ? true:false;
            this.ByPassInsp01Mode = NodeData.BYPASSINSP01MODE=="1"?true:false;
            this.ByPassInsp02Mode = NodeData.BYPASSINSP02MODE=="1"?true:false;
            this.NextLineBCStatus = NodeData.NEXTLINEBCSTATUS == "1" ? true : false;
            this.JobDataCheckMode = NodeData.JOBDATACHECKMODE=="1" ? true:false;
            this.COAVersionCheckMode = NodeData.COAVERSIONCHECKMODE=="1" ? true:false;
            this.JobDuplicateCheckMode = NodeData.JOBDUPLICATECHECKMODE=="1" ? true:false;
            this.ProductTypeCheckMode = NodeData.PRODUCTTYPECHECKMODE == "1" ? true : false;
            this.GroupIndexCheckMode = NodeData.GROUPINDEXCHECKMODE=="1" ? true :false;
            this.RecipeIDCheckMode = NodeData.RECIPEIDCHECKMODE == "1" ? true : false;
            this.PorductIDCheckMode = NodeData.PRODUCTIDCHECKMODE == "1" ? true : false;
            this.MaterialStatus = NodeData.MATERIALSTATUS == "1" ? true : false;  //0: OFF,1: ON
            this.IsOverTackTime = NodeData.TIMEOUTFLAG == "1" ? true : false;
            this.CV07Status = NodeData.CV07_STATUS == "1" ? true : false;
            #endregion

            #region List
            foreach (AllDataUpdateReply.UNITc _unit in NodeData.UNITLIST)
            {
                string _unitKey = NodeData.EQUIPMENTNO.PadRight(3, ' ') + _unit.UNITNO.PadLeft(2, '0');
                if (FormMainMDI.G_OPIAp.Dic_Unit.ContainsKey(_unitKey))
                {
                    FormMainMDI.G_OPIAp.Dic_Unit[_unitKey].SetUnitInfo(_unit);
                }
            }

            foreach (AllDataUpdateReply.PORTc _port in NodeData.PORTLIST)
            {
                string _portKey = NodeData.EQUIPMENTNO.PadRight(3, ' ') + _port.PORTNO.PadRight(2, '0');
                if (FormMainMDI.G_OPIAp.Dic_Port.ContainsKey(_portKey))
                {
                    FormMainMDI.G_OPIAp.Dic_Port[_portKey].SetPortInfo(_port);
                }
            }

            foreach (AllDataUpdateReply.DENSEBOXc _dense in NodeData.DENSEBOXLIST)
            {
                string _denseKey = NodeData.EQUIPMENTNO.PadRight(3, ' ') + _dense.PORTNO.PadRight(2, '0');
                if (FormMainMDI.G_OPIAp.Dic_Dense.ContainsKey(_denseKey))
                {
                    FormMainMDI.G_OPIAp.Dic_Dense[_denseKey].SetDenseInfo(_dense);
                }
            }

            foreach (AllDataUpdateReply.VCRc _vcr in NodeData.VCRLIST)
            {
                VCR vcr = this.VCRs.Find(d => d.VCRNO.Equals(_vcr.VCRNO.ToString().PadLeft(2, '0')));
                if (vcr == null) continue;
                vcr.Status = _vcr.VCRENABLEMODE==string.Empty ? eVCRMode.DISABLE : (eVCRMode)int.Parse(_vcr.VCRENABLEMODE);
            }

            foreach (AllDataUpdateReply.INTERLOCKc _interlock in NodeData.INTERLOCKLIST)
            {
                InterLock interlock = this.InterLocks.Find(d => d.PLCTrxNo.Equals(_interlock.INTERLOCKNO.ToString().PadLeft(2, '0')));
                if (interlock == null) continue;
                interlock.Status =_interlock.INTERLOCKSTATUS==string.Empty ? eInterlockMode.OFF: (eInterlockMode)int.Parse(_interlock.INTERLOCKSTATUS);
            }

            foreach (AllDataUpdateReply.SAMPLINGSIDEc _side in NodeData.SAMPLINGSIDELIST)
            {
                SamplingSide _samplingSize = this.SamplingSides.Find(d => d.ItemName.Equals(_side.ITEMNAME.ToString()));
                if (_samplingSize == null) continue;
                _samplingSize.SideStatus = _side.SIDESTATUS;
            }
            #endregion
        }
        #endregion

        #region Get Node Info -- AllEquipmentStatusReply
        public void SetNodeInfo(AllEquipmentStatusReply.EQUIPMENTc NodeData)
        {
            int _num = 0;

            #region Count
            this.CFJobCount = (int.TryParse(NodeData.CFJOBCNT, out _num) == true) ? int.Parse(NodeData.CFJOBCNT) : 0;
            this.DummyJobCount = (int.TryParse(NodeData.DMYJOBCNT, out _num) == true) ? int.Parse(NodeData.DMYJOBCNT) : 0;
            this.TFTJobCount = (int.TryParse(NodeData.TFTJOBCNT, out _num) == true) ? int.Parse(NodeData.TFTJOBCNT) : 0;
            this.ThicknessDummyJobCount = (int.TryParse(NodeData.THICKNESSDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.THICKNESSDMYJOBCNT) : 0;
            this.ThroughDummyJobCount = (int.TryParse(NodeData.THROUGHDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.THROUGHDMYJOBCNT) : 0;
            this.UnassembledTFTJobCount = (int.TryParse(NodeData.UNASSEMBLEDTFTDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.UNASSEMBLEDTFTDMYJOBCNT) : 0;//sy add 20160826
            this.ITODummyJobCount = (int.TryParse(NodeData.ITODMYJOBCNT, out _num) == true) ? int.Parse(NodeData.ITODMYJOBCNT) : 0;//sy add 20160826
            this.NIPDummyJobCount = (int.TryParse(NodeData.NIPDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.NIPDMYJOBCNT) : 0;//sy add 20160826
            this.MetalOneDummyJobCount = (int.TryParse(NodeData.METALONEDMYJOBCNT, out _num) == true) ? int.Parse(NodeData.METALONEDMYJOBCNT) : 0;//sy add 20160826
            this.UVMASKJobCount = (int.TryParse(NodeData.UVMASKJOBCNT, out _num) == true) ? int.Parse(NodeData.UVMASKJOBCNT) : 0;
            #endregion

            #region Word
            this.OperationMode =NodeData.EQPOPERATIONMODE==string.Empty ? eEQPOperationMode.Unknown: (eEQPOperationMode)int.Parse(NodeData.EQPOPERATIONMODE); 
            this.CIMMode = (eCIMMode)int.Parse(NodeData.CIMMODE);
            this.RecipeName = NodeData.CURRENTRECIPEID;
            this.EQPStatus =NodeData.CURRENTSTATUS==string.Empty ? eEQPStatus.UnKnown: (eEQPStatus)int.Parse(NodeData.CURRENTSTATUS);
            //this.EQPRunMode = NodeData.EQUIPMENTRUNMODE;
            this.AlarmStatus = (NodeData.LOCALALARMSTATUS == "ON" ? true : false);
            this.CassetteQTime = (int.TryParse(NodeData.CASSETTEQTIME, out _num) == true) ? int.Parse(NodeData.CASSETTEQTIME) : 0;
            this.InspectionIdleTime = (int.TryParse(NodeData.INSPECTIONIDLETIME, out _num) == true) ? int.Parse(NodeData.INSPECTIONIDLETIME) : 0;
            this.HSMSStatus = NodeData.HSMSSTATUS.ToString();
            this.HSMSControlMode = NodeData.HSMSCONTROLMODE.ToString();
            this.EquipmentAlive = NodeData.EQUIPMENTALIVE == "1" ? "Alive" : "Down";
            this.DownStreamInlineMode = NodeData.DOWNSTREAMINLINEMODE;
            this.UpStreamInlineMode = NodeData.UPSTREAMINLINEMODE;
            this.AutoRecipeChange = NodeData.AUTORECIPECHANGEMODE;
            this.ByPassMode = NodeData.BYPASSMODE;
            this.PartialFullMode =  NodeData.PARTIALFULLMODE == "1" ? "ON" : "OFF";
            //this.LoaderOperationMode = NodeData.LOADEROPERATIONMODE_ATS == string.Empty ? eLoaderOperationMode.Unknown : (eLoaderOperationMode)int.Parse(NodeData.LOADEROPERATIONMODE_ATS);
            this.LastReceiveGlassID = NodeData.LASTGLASSID == null ? string.Empty : NodeData.LASTGLASSID;
            this.LastReceiveDateTime = NodeData.LASTRECIVETIME == null ? string.Empty : NodeData.LASTRECIVETIME;
            this.HighCVMode = this.HighCVMode = NodeData.HIGHCVMODE == string.Empty ? eHighCVMode.UnKnown : (eHighCVMode)int.Parse(NodeData.HIGHCVMODE);      

            if (NodeData.EQUIPMENTRUNMODE == string.Empty)
            {
                if (FormMainMDI.G_OPIAp.CurLine.LineType.Equals("FCUPK_TYPE1") && NodeNo.Equals("L5"))
                {
                    if (this.EQPRunMode != string.Empty) FormMainMDI.G_OPIAp.RunModeHaveChange = true;
                }

                this.EQPRunMode = string.Empty;
            }
            else
            {
                string _newRunMode = GetRunModeDesc(NodeData.EQUIPMENTRUNMODE);

                if (FormMainMDI.G_OPIAp.CurLine.LineType.Equals("FCUPK_TYPE1") && NodeNo.Equals("L5"))
                {
                    if (this.EQPRunMode != _newRunMode) FormMainMDI.G_OPIAp.RunModeHaveChange = true;
                }

                this.EQPRunMode = _newRunMode;

            }

            //if (NodeData.EQUIPMENTRUNMODE2 == string.Empty) this.EQPRunMode2 = string.Empty;
            //else
            //{
            //    //int.TryParse(NodeData.EQUIPMENTRUNMODE2, out _num);

            //    this.EQPRunMode2 = GetRunModeDesc(NodeData.EQUIPMENTRUNMODE2);

            //}
            #endregion

            #region Bit
            this.TurnTableMode = NodeData.TURNTABLEMODE == "1" ? true : false ;
            this.ByPassInsp01Mode = NodeData.BYPASSINSP01MODE == "1" ? true : false;
            this.ByPassInsp02Mode = NodeData.BYPASSINSP02MODE == "1" ? true : false;
            this.NextLineBCStatus = NodeData.NEXTLINEBCSTATUS == "1" ? true : false;
            this.JobDataCheckMode = NodeData.JOBDATACHECKMODE == "1" ? true : false;
            this.COAVersionCheckMode = NodeData.COAVERSIONCHECKMODE == "1" ? true : false;
            this.JobDuplicateCheckMode = NodeData.JOBDUPLICATECHECKMODE == "1" ? true : false;
            this.ProductTypeCheckMode = NodeData.PRODUCTTYPECHECKMODE == "1" ? true : false;
            this.GroupIndexCheckMode = NodeData.GROUPINDEXCHECKMODE == "1" ? true : false;
            this.RecipeIDCheckMode = NodeData.RECIPEIDCHECKMODE == "1" ? true : false;
            this.PorductIDCheckMode = NodeData.PRODUCTIDCHECKMODE == "1" ? true : false;
            MaterialStatus = NodeData.MATERIALSTATUS == "1" ? true : false;  //0: OFF,1: ON
            this.IsOverTackTime = NodeData.TIMEOUTFLAG == "1" ? true : false;
            this.CV07Status = NodeData.CV07_STATUS == "1" ? true : false;
            this.OxinfoCheckMode = NodeData.OXINFOCHECKFLAG == "1" ? true : false;
            #endregion

            #region List
            foreach (AllEquipmentStatusReply.VCRc _vcr in NodeData.VCRLIST)
            {
                VCR vcr = this.VCRs.Find(d => d.VCRNO.Equals(_vcr.VCRNO.ToString().PadLeft(2, '0')));
                if (vcr == null) continue;
                vcr.Status = _vcr.VCRENABLEMODE==string.Empty ? eVCRMode.DISABLE:(eVCRMode)int.Parse(_vcr.VCRENABLEMODE);
            }

            foreach (AllEquipmentStatusReply.INTERLOCKc _interlock in NodeData.INTERLOCKLIST)
            {
                InterLock interlock = this.InterLocks.Find(d => d.PLCTrxNo.Equals(_interlock.INTERLOCKNO.ToString().PadLeft(2, '0')));
                if (interlock == null) continue;
                interlock.Status = _interlock.INTERLOCKSTATUS==string.Empty ? eInterlockMode.OFF : (eInterlockMode)int.Parse(_interlock.INTERLOCKSTATUS);
            }

            foreach (AllEquipmentStatusReply.SAMPLINGSIDEc _side in NodeData.SAMPLINGSIDELIST)
            {
                SamplingSide samplingSize = this.SamplingSides.Find(d => d.ItemName.Equals(_side.ITEMNAME.ToString()));
                if (samplingSize == null) continue;
                samplingSize.SideStatus = _side.SIDESTATUS ;
            }
            #endregion
        }
        #endregion

        #region Get RunMode Desc
        public string GetRunModeDesc(string RunMode)
        {
            int _num = 0;

            //若傳送值為數值型態，否則則當為傳送run mode描述 --2015-03-12 by 昌爺
            bool _isNum = int.TryParse(RunMode, out _num);

            if (_isNum)
            {
                if (_num == 0) return string.Format("{0}-UnKnown", RunMode);

                LineRunMode _runMode =LineRunModes.Find(r => r.RunModeNo.Equals(_num));

                if (_runMode == null) return string.Format("{0}-UnKnown", RunMode);

                return _runMode.RunModeDesc;
            }
            else 
            { 
                return RunMode;
            }            
        }
        #endregion
    } 
    
    public class SamplingSide
    {
        private string itemName = string.Empty;
        private string sideSataus = string.Empty ;

        public string ItemName
        {
            get { return itemName; }
            set { itemName = value; }
        }

        public string SideStatus
        {
            get { return sideSataus; }
            set { sideSataus = value == "1" ? "1:Enable" : "0:Disable" ; }
        }
       
    }
    public class IndexerRobotStage
    {
        //public bool IsReply { get; set; }  //判斷RobotOperationModeReply是否已回復
        private eRobotOperationMode operationMode = eRobotOperationMode.UnKnown;
        private string robotPosNo = string.Empty;
        private string direction = string.Empty;
        private string desc = string.Empty;
        private string localNo = string.Empty;
        private string localID = string.Empty;

        public string LocalNo
        {
            get { return localNo; }
            set { localNo = value; }
        }

        public string LocalID
        {
            get { return localID; }
            set { localID = value; }
        }

        public string RobotPosNo
        {
            get { return robotPosNo; }
            set { robotPosNo = value; }
        }

        //1：Received, 2：Sent,3：Both
        public string Direction
        {
            get { return direction; }
            set 
            { 
                switch (value)
                {
                    case "1": direction = "1:Received"; break;
                    case "2": direction = "2:Sent"; break;
                    case "3": direction = "3:Both"; break;
                    default: direction = "Unknown";  break;
                }
            }
        }

        public string Description
        {
            get { return desc; }
            set { desc = value; }
        }

        public eRobotOperationMode OperationMode
        {
            get { return operationMode; }
            set { operationMode = value; }
        }
    }
    public class VCR
    {
        //private string unitNO = string.Empty;
        //private string vcrID = string.Empty;
        private string vcrNO = string.Empty;
        //private string plcTrxNo = string.Empty;
        //public string UnitNo
        //{
        //    get { return unitNO; }
        //    set { unitNO = value; }
        //}
        //public string VCRID
        //{
        //    get { return vcrID; }
        //    set { vcrID = value; }
        //}
        public string VCRNO
        {
            get { return vcrNO; }
            set { vcrNO = value; }
        }

        public eVCRMode Status { get; set; }
        //public eVCRMode Status_Layout { get; set; }
    }

    public class InterLock
    {
        //private string unitNO = string.Empty;
        //private string unitID = string.Empty;
        private string plcTrxNo = string.Empty;
        //private string plcTrxReplyNo = string.Empty;
        private string plcDescription = string.Empty;

        //public string UnitNo
        //{
        //    get { return unitNO; }
        //    set { unitNO = value; }
        //}

        //public string UnitID
        //{
        //    get { return unitID; }
        //    set { unitID = value; }
        //}

        public string PLCTrxNo
        {
            get { return plcTrxNo.PadLeft(2,'0'); }
            set { plcTrxNo = value.PadLeft(2, '0'); }
            //防止人員在DB只輸入一碼,故補位2碼 add by sy.wu
        }
        //public string PLCTrxReplyNo
        //{
        //    get { return plcTrxReplyNo; }
        //    set { plcTrxReplyNo = value; }
        //}
        public string Description
        {
            get { return plcDescription; }
            set { plcDescription = value; }
        }
        public eInterlockMode Status { get; set; }

    }

    //public class Alarm
    //{
    //    private string alarmID = string.Empty;
    //    private string alarmText = string.Empty;
    //    private string alarmLevel = string.Empty;
    //    private string alarmUnit = string.Empty;
    //    private string alarmCode = string.Empty;

    //    public string AlarmID
    //    {
    //        get { return alarmID; }
    //        set { alarmID = value; }
    //    }
    //    public string AlarmText
    //    {
    //        get { return alarmText; }
    //        set { alarmText = value; }
    //    }
    //    public string AlarmLevel
    //    {
    //        get { return alarmLevel; }
    //        set { alarmLevel = value; }
    //    }
    //    public string AlarmUnit
    //    {
    //        get { return alarmUnit; }
    //        set { alarmUnit = value; }
    //    }

    //    public string AlarmCode
    //    {
    //        get { return alarmCode; }
    //        set { alarmCode = value; }
    //    }
    //}
}
