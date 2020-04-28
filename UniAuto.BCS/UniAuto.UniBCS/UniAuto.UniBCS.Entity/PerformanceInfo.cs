using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class PerformanceInfo
    {
        public long WorkingSet { get; set; }
        public long PrivateMemeorySize { set; get; }
        public long ThreadCount { get; set; }
        public string processName { get; set; }
    }
}
