using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;

namespace UniAuto.UniRCS.Core
{
    public  interface IRunnable
    {
        void UpdateStatus(Robot robot);
        void AutoRun(Robot robot,IList<RobotStage> robotStages);
        void SemiRun(Robot robot, IList<RobotStage> robotStages);
    }
}
