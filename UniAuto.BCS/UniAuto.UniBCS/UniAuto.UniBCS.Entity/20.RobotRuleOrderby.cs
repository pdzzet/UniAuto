using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class RobotRuleOrderby : Entity
    {
        public RobotRuleOrderbyEntityData Data { get; private set; }

        public RobotRuleOrderby(RobotRuleOrderbyEntityData data) {
            Data = data;
        }

    }
}
