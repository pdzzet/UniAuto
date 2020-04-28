using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class RobotPosition : Entity
    {
        public RobotPositionEntityData Data { get; private set; }

        public RobotPosition(RobotPositionEntityData data)
        {
            Data = data;
        }

        private eRobotOperationMode _mode = eRobotOperationMode.Unknown;
        private eRobotOperationAction _action = eRobotOperationAction.Unknown;

        public eRobotOperationMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public eRobotOperationAction Action
        {
            get { return _action; }
            set { _action = value; }
        }
    }
}
