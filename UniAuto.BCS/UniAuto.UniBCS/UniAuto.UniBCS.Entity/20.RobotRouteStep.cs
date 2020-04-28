using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    [Serializable]
    //20150817 modify Clone ,注意要同步修改SBRM_ROBOT_ROUTE_STEP.cs
    //public class RobotRouteStep : Entity
    public class RobotRouteStep : ICloneable  
    {
        public RobotRouteStepEntityData Data { get;  set; }

        public RobotRouteStep(RobotRouteStepEntityData data) {
            Data = data;
        }

        public RobotRouteStep()
        {

        }

        public object Clone()
        {
            RobotRouteStep routeStep = (RobotRouteStep)this.MemberwiseClone();
            routeStep.Data = (RobotRouteStepEntityData)this.Data.Clone();
            return routeStep;
        }
    }
}
