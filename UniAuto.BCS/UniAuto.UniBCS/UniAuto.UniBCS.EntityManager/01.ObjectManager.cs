using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.EntityManager
{
    public class ObjectManager
    {
        public static LineManager LineManager { get; set; }
        public static EquipmentManager EquipmentManager { get; set; }
        public static PortManager PortManager { get; set; }
        public static CassetteManager CassetteManager { get; set; }
        public static UnitManager UnitManager { get; set; }
        public static AlarmManager AlarmManager { get; set; }
        public static CIMMessageManager CIMMessageManager { get; set; }
        public static JobManager JobManager { get; set; }
        public static ProcessDataManager ProcessDataManager { get; set; }
        public static APCDataReportManager APCDataReportManager { get; set; }
        public static APCDataDownloadManager APCDataDownloadManager { get; set; }
        public static DailyCheckManager DailyCheckManager { get; set; }
        public static EnergyVisualizationManager EnergyVisualizationManager { get; set; }
        public static MaterialManager MaterialManager { get; set; }
        public static QtimeManager QtimeManager { get; set; }
        public static RecipeManager RecipeManager { get; set; }
        public static SubJobDataManager SubJobDataManager { get; set; }
        public static SubBlockManager SubBlockManager {get;set; }
        public static PalletManager PalletManager { get; set; }
        public static PlanManager PlanManager { get; set; }
        public static ProductTypeManager ProductTypeManager { get; set; }
        public static ProductIDManager ProductIDManager { get; set; }
        public static PositionManager PositionManager { get; set; }// sy add 20160928

        //20140928 add for RoborManager
        public static RobotManager RobotManager { get; set; }
        public static RobotStageManager RobotStageManager { get; set; }
      
        public static RBPositionManager RobotPositionManager { get; set; }
        //public static RobotJobManager RobotJobManager { get; set; }

    }
}
