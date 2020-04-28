using System;
using System.Collections.Generic;

namespace UniOPI
{
    public class Interface
    {
        public string PipeKey = string.Empty;
        public string UpstreamNodeNo=string.Empty ;
        public string UpstreamUnitNo=string.Empty ;
        public string UpstreamBitAddress = string.Empty;
        public string UpstreamSignal=string.Empty ;
        public string UpstreamSeqNo = string.Empty;
        public List<JobData> UpstreamJobData;
        public string DownstreamNodeNo=string.Empty ;
        public string DownstreamUnitNo=string.Empty ;
        public string DownstreamBitAddress = string.Empty;
        public string DownstreamSignal=string.Empty ;
        public string DownstreamSeqNo = string.Empty;
        public List<JobData> DownstreamJobData;



        public DateTime LastReceiveMsgDateTime;   //最後收到訊息的時間
        public bool IsReply = true;  //Trx 是否已經回覆
        public bool IsDisplay = false; //是否為開啟狀態

        public Interface()
        {
            UpstreamJobData = new List<JobData>();
            DownstreamJobData = new List<JobData>();

            UpstreamSignal = "0".PadLeft(32, '0');
            DownstreamSignal = "0".PadLeft(32, '0');

            UpstreamBitAddress = "0000";
            DownstreamBitAddress = "0000";

            IsDisplay = true;
            IsReply = true;

            LastReceiveMsgDateTime = new DateTime();
        }
    }

    public class JobData
    {
        public string JobAddress = string.Empty;
        public string CassetteSeqNo = string.Empty ;
        public string JobSeqNo = string.Empty ;
        public string ProductType = string.Empty ;
        public string SubStrateType = string.Empty ;
        public string JobType = string.Empty ;
        public string JobJudge = string.Empty ;
        public string JobGrade = string.Empty ;
        public string GlassID = string.Empty ;
        public string PPID = string.Empty ;
        public string TrackingData = string.Empty ;
        public string EQPFlag = string.Empty ;
    }

    public class LinkSignalType
    {
        public string UpStreamLocalNo = string.Empty;
        public string DownStreamLocalNo = string.Empty;
        public string SeqNo = string.Empty;
        public string LinkType = string.Empty;
        public string TimingChart = string.Empty;
    }

    public class LinkSignalBitDesc
    {
        public Dictionary<int,string> UpStreamBit;
        public Dictionary<int, string> DownStreamBit;

        public LinkSignalBitDesc()
        {
            UpStreamBit = new Dictionary<int, string>();
            DownStreamBit = new Dictionary<int, string>();
        }
    }

    public class LinkSignalBit
    {
        public int SeqNo = 0;
        public string Description = string.Empty;        
    }


}
