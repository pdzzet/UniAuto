using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class RobotRuleSelect : Entity
    {
        public RobotRuleJobSelectEntityData Data { get; private set; }

        public RobotRuleSelect(RobotRuleJobSelectEntityData data)
        {
            Data = data;
        }
    }
}
