using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public class Port
    {
        //public ePortType PortType { get; set; }

        // DB Data
        public string LineID { get; set; }
        public string ServerName { get; set; }
        public string NodeNo { get; set; }
        public string NodeID { get; set; }
        public string PortNo { get; set; }
        public string RelationPortNo { get; set; }  //for DPI ,紀錄有關連的port no (port no by node)
        public string PortID { get; set; }        
        public string PortAttribute { get; set; }
        public string ProcessStartType { get; set; }
        public string PortName { get; set; }
        public int MaxCount { get; set; }
        public bool MapplingEnable { get; set; }
        public bool IsEmptyCST { get; set; }  //是否為空CST - 空CST可不輸入 Port & Cassette 層資料 --for offline下貨畫面使用

        public ePortStatus PortStatus { get; set; }
        public eCassetteStatus CassetteStatus { get; set; }
        public ePortType PortType { get; set; }//LoadingPort,UnloadingPort
        public eCassetteType CassetteType { get; set; }
        public ePortDown PortDown { get; set; }
        public ePortMode PortMode { get; set; }
        public ePortEnable PortEnable { get; set; }
        public ePortTransfer PortTransfer { get; set; }
        public eLoadingCassetteType LoadingCassetteType { get; set; }
        public bool PartialFullMode { get; set; }
        public string CassetteSeqNo { get; set; }
        public string CassetteID { get; set; }                
        //public string PortOperationMode { get; set; }
        public string PortGrade { get; set; }
        public string ProductType { get; set; }
        public int PortGlassCount { get; set; }  //下貨數量
        public string ProcessType_Array { get; set; }
        public string PortAssignment { get; set; }  //for CELL GAP Line - 1：for GAP , 2：for GMI

        public UnloaderDispatch RobotUnloaderDispatch { get; set; } //Robot Unloader Dispatch

        public string FlowPriority { get; set; }//for MQC insp Flow Priority ，兩碼機台表示 - 030405 => 順序為L3->L4->L5 ; 030000=>第一優先為L3，之後無設定

        //"": No Status
        //"WACSTEDIT": Wait for OPI Edit Cassette Data (for online local/Offline)
        //"WAREMAPEDIT": Wait for OPI remap edit Cassette Data.
        //"WASTART": Wait for OPI Start command
        public string SubCassetteStatus { get; set; } 

        public string JobExistenceSlot { get; set; }  //001 ~ 999 有存在玻璃的slot (00001111...)

        public BCS_SlotPositionReply BC_SlotPositionReply;

        #region for Dense
        public ePackingMode PackingMode { get; set; }
        public string BoxID01 { get; set; }
        public string BoxID02 { get; set; }
        public eUnpackSource UnpackingSource { get; set; }
        public bool DenseBoxDataRequest { get; set; }  //是否可下貨s
        #endregion

        public Port()
        {
            //PortOperationMode = string.Empty;
            PortGrade = string.Empty;
            CassetteSeqNo = string.Empty;
            CassetteID = string.Empty;
            ProductType = string.Empty;
            ProcessType_Array = string.Empty;
            FlowPriority = string.Empty;
            PortAssignment = string.Empty;

            PortGlassCount = 0;

            BC_SlotPositionReply = new BCS_SlotPositionReply();
            RobotUnloaderDispatch = new UnloaderDispatch();

            
        }

        public void SetPortInfo(PortCSTStatusReport PortData)
        {
            int _num=0;

            this.LineID = PortData.BODY.LINEID;
            this.CassetteID = PortData.BODY.CASSETTEID;
            this.CassetteSeqNo = PortData.BODY.CASSETTESEQNO;
            this.CassetteStatus = PortData.BODY.CASSETTESTATUS == string.Empty ? eCassetteStatus.UnKnown : (eCassetteStatus)int.Parse(PortData.BODY.CASSETTESTATUS);
            this.PortGlassCount = (int.TryParse(PortData.BODY.PORTCNT, out _num) == true) ? int.Parse(PortData.BODY.PORTCNT) : 0;
            this.PortEnable =PortData.BODY.PORTENABLEMODE==string.Empty ? ePortEnable.Unknown: (ePortEnable)int.Parse(PortData.BODY.PORTENABLEMODE);
            this.PortGrade = PortData.BODY.PORTGRADE;
            this.PortMode = PortData.BODY.PORTMODE == string.Empty ? ePortMode.Unknown : (ePortMode)int.Parse(PortData.BODY.PORTMODE); 
            this.PortID = PortData.BODY.PORTID;
            this.PortNo = PortData.BODY.PORTNO;
            this.PortStatus = PortData.BODY.PORTSTATUS==string.Empty ? ePortStatus.UnKnown : (ePortStatus)int.Parse(PortData.BODY.PORTSTATUS);
            this.PortTransfer = PortData.BODY.PORTTRANSFERMODE==string.Empty ? ePortTransfer.Unknown : (ePortTransfer)int.Parse(PortData.BODY.PORTTRANSFERMODE);
            this.PortType = PortData.BODY.PORTTYPE==string.Empty ? ePortType.UnKnown : (ePortType)int.Parse(PortData.BODY.PORTTYPE); 
            this.JobExistenceSlot = PortData.BODY.JOBEXISTSLOT.PadRight(MaxCount,'0');
            this.SubCassetteStatus = PortData.BODY.SUBCSTSTATE;
            this.PortDown = PortData.BODY.PORTDOWN==string.Empty ? ePortDown.Down : (ePortDown)int.Parse(PortData.BODY.PORTDOWN);
            this.LoadingCassetteType = PortData.BODY.LOADINGCASSETTETYPE == string.Empty ? eLoadingCassetteType.Unknown : (eLoadingCassetteType)int.Parse(PortData.BODY.LOADINGCASSETTETYPE);
            ProductType = PortData.BODY.PRODUCTTYPE;
            ProcessType_Array = PortData.BODY.PROCESSTYPE_ARRAY;

            if (PortData.BODY.ASSIGNMENT_GAP == string.Empty) PortAssignment = "Unknown";
            else if (PortData.BODY.ASSIGNMENT_GAP == "1") PortAssignment = "1:For GAP";
            else if (PortData.BODY.ASSIGNMENT_GAP == "2") PortAssignment = "2:For GMI";
            else if (PortData.BODY.ASSIGNMENT_GAP == "3") PortAssignment = "1:For PDR";
            else if (PortData.BODY.ASSIGNMENT_GAP == "4") PortAssignment = "2:For CEM";
            else PortAssignment = PortData.BODY.ASSIGNMENT_GAP;
        }

        public void SetPortInfo(PortStatusReply PortData)
        {
            int _num = 0;

            this.LineID = PortData.BODY.LINEID;
            this.CassetteID = PortData.BODY.CASSETTEID;
            this.CassetteSeqNo = PortData.BODY.CASSETTESEQNO;
            this.CassetteStatus = PortData.BODY.CASSETTESTATUS == string.Empty ? eCassetteStatus.UnKnown : (eCassetteStatus)int.Parse(PortData.BODY.CASSETTESTATUS);
            this.PortGlassCount = (int.TryParse(PortData.BODY.PORTCNT, out _num) == true) ? int.Parse(PortData.BODY.PORTCNT) : 0;
            this.PortEnable = PortData.BODY.PORTENABLEMODE==string.Empty ? ePortEnable.Unknown : (ePortEnable)int.Parse(PortData.BODY.PORTENABLEMODE);  //PortData.BODY.PORTENABLEMODE;
            this.PortGrade = PortData.BODY.PORTGRADE;
            this.PortMode =PortData.BODY.PORTMODE==string.Empty ? ePortMode.Unknown: (ePortMode)int.Parse(PortData.BODY.PORTMODE);  //PortData.BODY.PORTMODE;
            this.PortID = PortData.BODY.PORTID;
            this.PortNo = PortData.BODY.PORTNO;
            this.PortStatus = PortData.BODY.PORTSTATUS==string.Empty ? ePortStatus.UnKnown : (ePortStatus)int.Parse(PortData.BODY.PORTSTATUS);
            this.PortTransfer = PortData.BODY.PORTTRANSFERMODE == string.Empty ? ePortTransfer.Unknown : (ePortTransfer)int.Parse(PortData.BODY.PORTTRANSFERMODE);
            this.PortType =PortData.BODY.PORTTYPE==string.Empty ? ePortType.UnKnown: (ePortType)int.Parse(PortData.BODY.PORTTYPE);
            this.JobExistenceSlot = PortData.BODY.JOBEXISTSLOT.PadRight(MaxCount, '0');
            this.SubCassetteStatus = PortData.BODY.SUBCSTSTATE;
            this.PortDown =PortData.BODY.PORTDOWN==string.Empty ? ePortDown.Down: (ePortDown)int.Parse(PortData.BODY.PORTDOWN);
            this.PartialFullMode = PortData.BODY.PARTIALFULLMODE == "1" ? true:false;
            this.LoadingCassetteType = PortData.BODY.LOADINGCASSETTETYPE == string.Empty ? eLoadingCassetteType.Unknown : (eLoadingCassetteType)int.Parse(PortData.BODY.LOADINGCASSETTETYPE);
            ProductType = PortData.BODY.PRODUCTTYPE;
            ProcessType_Array = PortData.BODY.PROCESSTYPE_ARRAY;

            if (PortData.BODY.ASSIGNMENT_GAP == string.Empty) PortAssignment = "Unknown";
            else if (PortData.BODY.ASSIGNMENT_GAP == "1") PortAssignment = "1:For GAP";
            else if (PortData.BODY.ASSIGNMENT_GAP == "2") PortAssignment = "2:For GMI";
            else if (PortData.BODY.ASSIGNMENT_GAP == "3") PortAssignment = "1:For PDR";
            else if (PortData.BODY.ASSIGNMENT_GAP == "4") PortAssignment = "2:For CEM";
            else PortAssignment = PortData.BODY.ASSIGNMENT_GAP;
        }

        public void SetPortInfo(AllDataUpdateReply.PORTc PortData)
        {
            int _num = 0;

            this.LineID = PortData.LINEID;
            this.CassetteID = PortData.CASSETTEID;
            this.CassetteSeqNo = PortData.CASSETTESEQNO;
            this.CassetteStatus = PortData.CASSETTESTATUS == string.Empty ? eCassetteStatus.UnKnown : (eCassetteStatus)int.Parse(PortData.CASSETTESTATUS);
            this.PortGlassCount = (int.TryParse(PortData.PORTCNT, out _num) == true) ? int.Parse(PortData.PORTCNT) : 0;
            this.PortEnable = PortData.PORTENABLEMODE==string.Empty ? ePortEnable.Unknown:(ePortEnable)int.Parse(PortData.PORTENABLEMODE);
            this.PortGrade = PortData.PORTGRADE;
            this.PortMode =PortData.PORTMODE==string.Empty ? ePortMode.Unknown: (ePortMode)int.Parse(PortData.PORTMODE);  //PortData.PORTMODE;
            this.PortID = PortData.PORTID;
            this.PortNo = PortData.PORTNO;
            //this.PortOperationMode = PortData.PORTOPERMODE;
            this.PortStatus = PortData.PORTSTATUS==string.Empty ? ePortStatus.UnKnown : (ePortStatus)int.Parse(PortData.PORTSTATUS);
            this.PortTransfer = PortData.PORTTRANSFERMODE==string.Empty ? ePortTransfer.Unknown: (ePortTransfer)int.Parse(PortData.PORTTRANSFERMODE);  
            this.PortType =PortData.PORTTYPE==string.Empty ? ePortType.UnKnown : (ePortType)int.Parse(PortData.PORTTYPE);
            this.JobExistenceSlot = PortData.JOBEXISTSLOT.PadRight(MaxCount, '0');
            this.SubCassetteStatus = PortData.SUBCSTSTATE;
            this.PortDown =PortData.PORTDOWN==string.Empty ? ePortDown.Down : (ePortDown)int.Parse(PortData.PORTDOWN);
            this.PartialFullMode = PortData.PARTIALFULLMODE == "1" ? true : false;
            this.LoadingCassetteType = PortData.LOADINGCASSETTETYPE == string.Empty ? eLoadingCassetteType.Unknown : (eLoadingCassetteType)int.Parse(PortData.LOADINGCASSETTETYPE);
            ProductType = PortData.PRODUCTTYPE;
            ProcessType_Array = PortData.PROCESSTYPE_ARRAY;

            if (PortData.ASSIGNMENT_GAP == string.Empty) PortAssignment = "Unknown";
            else if (PortData.ASSIGNMENT_GAP == "1") PortAssignment = "1:For GAP";
            else if (PortData.ASSIGNMENT_GAP == "2") PortAssignment = "2:For GMI";
            else if (PortData.ASSIGNMENT_GAP == "3") PortAssignment = "1:For PDR";
            else if (PortData.ASSIGNMENT_GAP == "4") PortAssignment = "2:For CEM";
            else PortAssignment = PortData.ASSIGNMENT_GAP;
        }
    }

    public class UnloaderDispatch           
    {
        public string Grade01 = string.Empty;
        public string Grade02 = string.Empty;
        public string Grade03 = string.Empty;
        //public string Grade04 = string.Empty;
        //public string AbnormalCode01 = string.Empty;
        //public string AbnormalCode02 = string.Empty;
        //public string AbnormalCode03 = string.Empty;
        //public string AbnormalCode04 = string.Empty;
        //public string AbnormalFlag = string.Empty;
        public string OperatorID = string.Empty;
    }
}
