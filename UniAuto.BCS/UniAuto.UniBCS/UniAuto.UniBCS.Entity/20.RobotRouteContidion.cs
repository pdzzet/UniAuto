using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    /// <summary>
    /// Robot Route Condition
    /// </summary>
    [Serializable]
    public class RobotRouteCondition : Entity
    {
        public  RobotRouteConditionEntityData Data { get; private set; }


        public RobotRouteCondition(RobotRouteConditionEntityData robotRouteConditionEntityData) {
            Data = robotRouteConditionEntityData;
        }
    }
}
