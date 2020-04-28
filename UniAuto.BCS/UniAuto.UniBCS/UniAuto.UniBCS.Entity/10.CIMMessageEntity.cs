using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class eCIMMESSAGE_STATE
    {
        public const string CLEAR = "CLEAR";

        public const string SET = "SET";

        public const string COMFIRM = "COMFIRM";
    }

    [Serializable]
    public class CIMMessage
    {
        public string MessageID{get;set;}
        public string NodeNo { get; set; }
        public string Message { get; set; }
        public string OperatorID { get; set; }
        public string MessageStatus { get; set; }
        public string TouchPanelNo { get; set; }
        public DateTime OccurDateTime { get; set; }
        public bool IsSend { get; set; }
        public bool IsFinish { get; set; }
        public string TrxID { get; set; }

        public CIMMessage(string messageID,string  nodeNo,string msg,string operId,string tpno,string state,bool IsSend=false,bool isFinish=false)
        {
            MessageID = messageID;
            NodeNo = nodeNo;
            OperatorID = operId;
            MessageStatus = state;
            Message = msg;
            this.IsFinish = isFinish;
            this.IsSend = IsSend;
            OccurDateTime = DateTime.Now;
            TrxID = OccurDateTime.ToString("yyyyMMddHHmmssfff");
            TouchPanelNo = tpno;
        }
    }

    //[Serializable]
    //public class CIMMessageHistoryFile : EntityFile
    //{
    //    private string _nodeNo = string.Empty;
    //    private string _nodeID = string.Empty;

    //    public string NodeNo
    //    {
    //        get { return _nodeNo; }
    //        set { _nodeNo = value; }
    //    }

    //    public string NodeID
    //    {
    //        get { return _nodeID; }
    //        set
    //        {
    //            _nodeID = value;
    //            SetFilename(_nodeID);
    //        }
    //    }

    //    public CIMMessageHistoryFile() { }

    //    public CIMMessageHistoryFile(string nodeNo, string nodeID)
    //    {
    //        NodeNo = nodeNo;
    //        NodeID = nodeID;
    //    }

    //    public Dictionary<string, CIMMessageData> HappingCIMMessages = new Dictionary<string, CIMMessageData>();
    //}

    //[Serializable]
    //public class CIMMessage : Entity
    //{
    //    public CIMMESSAGEHISTORY Data { get; private set; }

    //    public CIMMessageHistoryFile File { get; private set; }

    //    public CIMMessage(CIMMESSAGEHISTORY data, CIMMessageHistoryFile file)
    //    {
    //        Data = data;
    //        File = file;
    //    }
    //}

}
