using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class Robot
    {
        public string LineType { get; set; }
        public string ServerName { get; set; }
        public string LineID { get; set; }
        public string NodeNo { get; set; }
        public string UnitNo { get; set; }

        public string RobotName { get; set; }        
        public int RobotArmCount { get; set; }   //Robot 手臂的數量.(Default=1)
        public int ArmMaxJobCount { get; set; }   //1 or 2 ,Arm上面可放glass 最大數量


        public string RobotControlMode { get; set; }  //SEMI , AUTO
        public eRobotStatus RobotStatus { get; set; }
        public bool HaveRobotCommand { get; set; }
        public string CurrentPosition { get; set; }   //記錄Robot 停在哪個Stage前面 00-99
        public bool HoldStatus { get; set; }  // true : Hold, false:Release
        public bool SampEQPFlag { get; set; }  // true : Same EQP, false: allow not same EQP -- just for ARRAY 相同性質的insp機台 針對有2台以上檢測機的Line 要提供設定 ( 當A CST 第一片進入這台 檢測機時.之後所有同CST的 Job只能進入這台(不可進入其他台))
        public List<ArmInfo> LstArms { get; set; } 
        //public eRobotArmStatus UpperArmStatus  { get; set; }
        //public string UpperArmCstSeqNo  { get; set; }
        //public string UpperArmJobSeqNo { get; set; }

        //public eRobotArmStatus LowerArmStatus  { get; set; }
        //public string LowerArmCstSeqNo { get; set; }
        //public string LowerArmJobSeqNo { get; set; }       
    }


    public class RobotStage
    {
        public string NodeNo { get; set; }  
        public string RobotName { get; set; }   
        public string StageID { get; set; }
        public string StageName { get; set; }
        public string StageType { get; set; }        
        public eRobotStageStatus StageStatus { get; set; }
        public int SlotMaxCount { get; set; }
        //public string StageJobSeqNo { get; set; }
        public string TrackDataEQList { get; set; }
        public BCS_StagePositionInfoReply BC_StagePositionInfoReply { get; set; }
        public List< StageJobData> Lst_JobData { get; set; }
        //public Dictionary<string, StageJobData> Dic_JobData { get; set; }
    }

    public class ArmInfo
    {
        public string ArmNo { get; set; }    //01-04
        public string ArmName { get; set; }  //Arm01-Arm04
        public bool ArmEnable { get; set; }
        public eRobotJobStatus JobExist_Front { get; set; } //Front Arm 是否有玻璃存在        
        public string CstSeqNo_Front { get; set; } //Front Cassette Seq No  
        public string JobSeqNo_Front { get; set; } //Front Job Seq No     
        public string TrackingData_Front { get; set; } //Front Job Tracking Data    
        public eRobotJobStatus JobExist_Back { get; set; }  //Back Arm 是否有玻璃存在        
        public string CstSeqNo_Back { get; set; }  //Back Cassette Seq No  
        public string JobSeqNo_Back { get; set; }  //Back Job Seq No       
        public string TrackingData_Back { get; set; } //Back Job Tracking Data    
    }

    public class StageJobData
    {
        //當對應arm count = 4時 Slot No = 01:lift  Front ; Slot No = 02 : lift  Back;Slot No = 03 : Right Front;Slot No = 04 : Right Back
        public string SlotNo { get; set; }  //當stage 對應多組job 時，請給對應的slot no , 

        public eRobotJobStatus JobExist { get; set; } //是否有玻璃存在        
        public string CstSeqNo { get; set; } //Cassette Seq No  
        public string JobSeqNo { get; set; } //Job Seq No    
        public string TrackingData { get; set; } //Job Tracking Data    
    }
}
