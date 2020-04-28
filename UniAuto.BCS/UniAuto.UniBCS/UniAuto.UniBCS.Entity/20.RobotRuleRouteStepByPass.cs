using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{

    public class RobotRuleRouteStepByPass : Entity
    {
        public RobotRuleRouteStepByPassEntityData Data { get; private set; }

        public RobotRuleRouteStepByPass(RobotRuleRouteStepByPassEntityData data)
        {
            Data = data;
        }
    }
}
