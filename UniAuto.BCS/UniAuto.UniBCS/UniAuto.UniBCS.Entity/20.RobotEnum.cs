using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    //for RCS Use Only
    public enum eRobotStatus
    {
        //1 : Setup  2 : Stop  3 : Pause  4 : Idle  5 : Running
        UNKNOWN = 0,
        SETUP = 1,
        STOP = 2,
        PAUSE = 3,
        IDLE = 4,
        RUNNING = 5
    }

    public enum eRobotHasCommandStatus
    {
        NO_COMMAND_ON_ROBOT=0,
        HAS_COMMAND_ON_ROBOT=1
    }

    public enum eRobotCmdReturnCode
    {

        //0	None
        //1	Acknowledge is OK.
        //2	Busy, Try again
        //3	CIM Mode is Offline
        //4	Command is already executing
        //5	Cannot perform now (Hardware problem)
        //6 ~ 100	Reserved define by CSOT.
        //101 ~ 60000	Definition by Equipment Maker 
   
        UNKNOWN = 0,
        OK = 1,
        BUSY_TRY_AGAIN = 2,
        CIMMODE_IS_OFFLINE = 3,
        ALREADY_EXECUTING = 4,
        CANNOT_PERFORM_NOW = 5,
        BC_Cmd_No_Err = 101,
        BC_Position_No_Err = 102,
        BC_Arm_No_Err = 103,
        BC_Slot_No_Err = 104,
        BC_Cmd_Hold = 110,
        Indexer_EQ_Status_NotRun = 200,
        Robot_NotIdle = 300,
        Robot_Arm_Abnormal = 301,
        CST_Cmd_CannotAct = 400,
        CST_Port_Status_Abnormal = 401,
        EQ_Cmd_CannotAct = 500,
        EQ_PIO_SignalErr = 501,
        EQ_GLS_DataErr = 502,
        EQ_LinkSignalErr = 503
    }

    public enum eRobotCmdActionCode
    {
        //DB定義 'PUT' / 'GET' / 'PUTREADY' / 'GETREADY'
        //SPEC定義
        //0: None
        //1: Put
        //2: Get
        //4: Exchange
        //8: Put Ready
        //16: Get Ready
        //32: Get/Put
        NONE = 0,
        PUT = 1,
        GET = 2,
        EXCHANGE = 4,
        PUTREADY = 8,
        GETREADY = 16,
        GETPUT = 32

    }

    public enum eRobotCmdArmSelectCode
    {
        //DB定義 
        //'UP':Upper Arm 
        //'LOW':Lower Arm
        //'ANY':Any Arm
        //'ALL':All Arm

        //SPEC定義
        //0: None
        //1: Upper/Left Arm
        //2: Lower/Left Arm
        //3: Left Both Arm 
        //4: Upper/Right Arm
        //8: Lower/Right Arm
        //12: Right Both Arm
        
        //20160127 add 5 and 10
        //5: Upper Both Arm 
        //10: Lower Both Arm

        NONE = 0,
        UP = 1,
        LOW = 2,
        BOTH = 3,
        UP2 = 4,
        UPBOTH=5,
        LOW2 = 8,
        LOWBOTH=10,
        BOTH2 = 12

    }

    public enum eArmDisableStatus
    {
        Enable = 0,
        Disable = 1
    }

    /// <summary>
    /// 與IO Mapping,OPI 相同 , Watson Add 20151015 Add 
    ///   0: PROD
    ///   1: MQC
    /// </summary>
    public enum eCVDIndexRunMode
    {
        PROD = 0,
        MQC = 1,
        PROD1 = 2//modify by hujunpeng 20190425 for CVD700新增一个product进行混run逻辑,Deng,20190823
    }

    //20160113 add Cell Special Arm Code
    public enum eCellSpecialRobotCmdArmSelectCode
    {

        //SPEC定義
        //0: None
        //1: Upper/Left Arm
        //2: Lower/Left Arm
        //3: Left Both Arm 
        //4: Upper/Right Arm
        //8: Lower/Right Arm
        //12: Right Both Arm

        //20160127 add 5 and 10
        //5: Upper Both Arm 
        //10: Lower Both Arm
        NONE = 0,
        UP_LEFT = 1,
        LOW_LEFT = 2,
        BOTH_LEFT = 3,
        UP_RIGHT = 4,
        UPBOTH = 5,
        LOW_RIGHT = 8,
        LOWBOTH = 10,
        BOTH_RIGHT = 12

    }


}
