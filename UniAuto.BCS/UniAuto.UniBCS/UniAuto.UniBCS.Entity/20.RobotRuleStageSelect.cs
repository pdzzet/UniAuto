using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{

    public class RobotRuleStageSelect : Entity
    {
        public RobotRouteRuleStageSelectEntityData Data { get; private set; }

        public RobotRuleStageSelect(RobotRouteRuleStageSelectEntityData data)
        {
            Data = data;
        }
    }

}
