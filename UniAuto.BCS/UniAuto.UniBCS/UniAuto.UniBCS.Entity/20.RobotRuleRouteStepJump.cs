using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class RobotRuleRouteStepJump : Entity
    {
        public RobotRuleRouteStepJumpEntityData Data { get; private set; }

        public RobotRuleRouteStepJump(RobotRuleRouteStepJumpEntityData data)
        {
            Data = data;
        }
    }
}
