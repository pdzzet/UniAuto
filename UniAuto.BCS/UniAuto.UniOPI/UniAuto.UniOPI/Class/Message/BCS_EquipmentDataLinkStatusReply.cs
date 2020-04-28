using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class BCS_EquipmentDataLinkStatusReply
    {
        public bool IsReply { get; set; }  //判斷EquipmentDataLinkStatusReply是否已回復

        public string BatonPassStatus_W { get; set; }
        public string BatonPassInterruption_W { get; set; }
        public string DataLinkStop_W { get; set; }
        public string StationLoopStatus_W { get; set; }
        public string BatonPassStatus_B { get; set; }
        public string CyclicTransmissionStatus_B { get; set; }
        public DateTime LastRequestDate { get; set; } //最後發送request的時間    

        public BCS_EquipmentDataLinkStatusReply()
        {
            IsReply = true;

            BatonPassStatus_W = "0000" ;
            BatonPassInterruption_W = "0000" ;
            DataLinkStop_W = "0000" ;
            StationLoopStatus_W = "0000" ;

            BatonPassStatus_B = "0".PadLeft(128, '0');
            CyclicTransmissionStatus_B = "0".PadLeft(128, '0');

            LastRequestDate = Convert.ToDateTime("2010-01-01 00:00:00");
        }

        public string BatonPassStatus_Desc()
        {
            //“0000”: Data linking
            //“0001”: Data link stop in execution
            //“0002”: Baton pass being executed
            //“0003”: Baton pass stop in execution
            //“0004”: Test being executed
            //“0005”: Offline
            //Other: Unknow
            switch (BatonPassStatus_W)
            {
                case "0000": return "0000 : Data linking";
                case "0001": return "0001 : Data link stop in execution";
                case "0002": return "0002 : Baton pass being executed";
                case "0003": return "0003 : Baton pass stop in execution";
                case "0004": return "0004 : Test being executed";
                case "0005": return "0005 : Offline";
                default: return BatonPassStatus_W + " : Unknow";
            }
        }

        public string BatonPassInterruption_Desc()
        {
            //“0000”: Normal communication
            //“0030”: Cable disconnection or power-on
            //“0031”: Cable insertion error
            //“0032”: Cable IN-OUT checking
            //“0033”: Disconnection or reconnection processing
            //“0040”: Offline mode
            //“0041”: Hardware test
            //“0042”: Self-loopback test
            //“0050”: Self-diagnostics in execution
            switch (BatonPassInterruption_W)
            {
                case "0000": return "0000 : Normal communication";
                case "0030": return "0030 : Cable disconnection or power-on";
                case "0031": return "0031 : Cable insertion error";
                case "0032": return "0032 : Cable IN-OUT checking";
                case "0033": return "0033 : Disconnection or reconnection processing";
                case "0040": return "0040 : Offline mode";
                case "0041": return "0041 : Hardware test";
                case "0042": return "0042 : Self-loopback test";
                case "0050": return "0050 : Self-diagnostics in execution";
                default: return BatonPassInterruption_W + " : Unknow";
            }
        }

        public string DataLinkStop_Desc()
        {
            //“0000”: Normal communication
            //“0001”: Stop directed
            //“0002”: Monitoring timeout ( Page 162, Section 6.3)
            //“0003”: Circuit test being executed
            //“0010”: Parameter unreceived
            //“0011”: Own station No. that is out of range
            //“0012”: Setting where own station is reserved station
            //“0013”: Own station No. duplication
            //“0014”: Control station duplication
            //“0015”: Control station or own station No. duplication
            //“0016”: Station No. unset
            //“0017”: Network No. illegality
            //“0018”: Parameter error
            //“0019”: Parameter communication in execution
            //“0020”: CPU module stop error
            //“0021”: CPU module power stop error
            switch (DataLinkStop_W)
            {
                case "0000": return "0000 : Normal communication";
                case "0001": return "0001 : Stop directed";
                case "0002": return "0002 : Monitoring timeout ( Page 162, Section 6.3)";
                case "0003": return "0003 : Circuit test being executed";
                case "0010": return "0010 : Parameter unreceived";
                case "0011": return "0011 : Own station No. that is out of range";
                case "0012": return "0012 : Setting where own station is reserved station";
                case "0013": return "0013 : Own station No. duplication";
                case "0014": return "0014 : Control station duplication";
                case "0015": return "0015 : Control station or own station No. duplication";
                case "0016": return "0016 : Station No. unset";
                case "0017": return "0017 : Network No. illegality";
                case "0018": return "0018 : Parameter error";
                case "0019": return "0019 : Parameter communication in execution";
                case "0020": return "0020 : CPU module stop error";
                case "0021": return "0021 : CPU module power stop error";
                default: return DataLinkStop_W + " : Unknow";
            }
        }

        public string StationLoopStatus_Desc()
        {
            //“0000”: Normal
            //“0012”: IN-side loopback (OUT-side cable disconnection)
            //“0013”: IN-side loopback (OUT-side cable insertion error)
            //“0014”: IN-side loopback (OUT-side line establishing)
            //“0021”: OUT-side loopback (IN-side cable disconnection)
            //“0031”: OUT-side loopback (IN-side cable insertion error)
            //“0041”: OUT-side loopback (IN-side line establishing)
            //“0022”: Disconnecting (IN-side or OUT-side cable disconnection)
            //“0023”: Disconnecting (IN-side cable disconnection, OUT-side cable insertion error)
            //“0024”: Disconnecting (IN-side cable disconnection, OUT-side line establishing)
            //“0032”: Disconnecting (IN-side cable insertion error, OUT-side cable disconnection)
            //“0033”: Disconnecting (IN-side or OUT-side cable insertion error)
            //“0034”: Disconnecting (IN-side cable insertion error, OUT-side line establishing)
            //“0042”: Disconnecting (IN-side line establishing, OUT-side cable disconnection)
            //“0043”: Disconnecting (IN-side line establishing, OUT-side cable insertion error)
            //“0044”: Disconnecting (IN-side or OUT-side line establishing)
            switch (StationLoopStatus_W)
            {
                case "0000": return "0000 : Normal";
                case "0012": return "0012 : IN-side loopback (OUT-side cable disconnection)";
                case "0013": return "0013 : IN-side loopback (OUT-side cable insertion error)";
                case "0014": return "0014 : IN-side loopback (OUT-side line establishing)";
                case "0021": return "0021 : OUT-side loopback (IN-side cable disconnection)";
                case "0031": return "0031 : OUT-side loopback (IN-side cable insertion error)";
                case "0041": return "0041 : OUT-side loopback (IN-side line establishing)";
                case "0022": return "0022 : Disconnecting (IN-side or OUT-side cable disconnection)";
                case "0023": return "0023 : Disconnecting (IN-side cable disconnection, OUT-side cable insertion error)";
                case "0024": return "0024 : Disconnecting (IN-side cable disconnection, OUT-side line establishing)";
                case "0032": return "0032 : Disconnecting (IN-side cable insertion error, OUT-side cable disconnection)";
                case "0033": return "0033 : Disconnecting (IN-side or OUT-side cable insertion error)";
                case "0034": return "0034 : Disconnecting (IN-side cable insertion error, OUT-side line establishing)";
                case "0042": return "0042 : Disconnecting (IN-side line establishing, OUT-side cable disconnection)";
                case "0043": return "0043 : Disconnecting (IN-side line establishing, OUT-side cable insertion error)";
                case "0044": return "0044 : Disconnecting (IN-side or OUT-side line establishing)";
                default: return StationLoopStatus_W + " : Unknow";
            }
        }
    }
}
