using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    /// <summary>
    /// Robot Route MST
    /// </summary>
    [Serializable]
    public class RobotRoute:Entity
    {
        private RobotRouteMstEntityData _data;

        public RobotRouteMstEntityData Data {
            get {
                return _data;
            }
            private set {
                _data = value;
            }
        }

        public RobotRoute(RobotRouteMstEntityData data) {
            _data = data;
        }
    
    }
}
