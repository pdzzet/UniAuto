using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    /// <summary>
    /// Robot Rule Fileter
    /// </summary>
    public class RobotRuleFilter : Entity
    {
        public RobotRuleFilterEntityData Data { get; private set; }

        public RobotRuleFilter(RobotRuleFilterEntityData data) {
            Data = data;
        }
    }
}
