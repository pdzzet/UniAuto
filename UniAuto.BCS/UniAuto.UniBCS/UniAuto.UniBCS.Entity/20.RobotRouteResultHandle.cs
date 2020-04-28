using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class RobotRouteResultHandle : Entity
    {
        public RobotRouteResultHandleEntityData Data { get; private set; }

        public RobotRouteResultHandle(RobotRouteResultHandleEntityData data)
        {
            Data = data;
        }
    }
}
