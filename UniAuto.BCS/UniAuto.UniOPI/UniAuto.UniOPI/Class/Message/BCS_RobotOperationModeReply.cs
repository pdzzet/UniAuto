using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class BCS_RobotOperationModeReply
    {
        public bool IsReply { get; set; }  //判斷SlotPositionReply是否已回復

        public DateTime LastRequestDate { get; set; } //最後發送request的時間     

        public List<IndexerRobotStage> IndexerRobotStages { get; set; }

        public BCS_RobotOperationModeReply()
        {
            IsReply = true;
 
            LastRequestDate = Convert.ToDateTime("2010-01-01 00:00:00");

            IndexerRobotStages = new List<IndexerRobotStage>();

        }
    }
}
