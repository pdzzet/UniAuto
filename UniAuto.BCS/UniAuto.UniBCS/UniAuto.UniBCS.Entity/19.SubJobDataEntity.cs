using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class SubJobData : Entity
    {
        public SubJobDataEntityData Data { get; set; }

        public SubJobData(SubJobDataEntityData data)
        {
            Data = data;
        }
    }
}
