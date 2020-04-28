using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class eAgentName
    {
        public const string PLCAgent = "PLCAgent";
        public const string DBAgent = "DBAgent";
        public const string TRVAgent = "TRVAgent";
        public const string SECSAgent = "SECSAgent";
        public const string MESAgent = "MESAgent";
        public const string OEEAgent = "OEEAgent";
        public const string EDAAgent = "EDAAgent";
        public const string OPIAgent = "OPIAgent";
        public const string APCAgent = "APCAgent";
        public const string SerialPortAgent = "SerialAgent";
        public const string ActiveSocketAgent = "ActiveSocketAgent";
        public const string PassiveSocketAgent = "PassiveSocketAgent";
    }
    public class eServiceName
    {
        public const string LineService = "LineService";
        public const string APCService = "APCService";
        public const string MESService = "MESService";
        public const string OEEService = "OEEService";
        public const string EDAService = "EDAService";
        public const string EquipmentService = "EquipmentService";
        public const string CSOTSECSService = "CSOTSECSService";
        public const string NikonSECSService = "NikonSECSService";
        public const string DateTimeService = "DateTimeService";
        public const string UIService = "UIService";
        public const string ArraySpecialService = "ArraySpecialService";
        public const string CFSpecialService = "CFSpecialService";
        public const string CELLSpecialService = "CELLSpecialService";
        public const string MODULESpecialService = "MODULESpecialService";
        public const string MaterialService = "MaterialService";
        public const string AlarmService = "AlarmService";
        public const string RecipeService = "RecipeService";
        public const string PortService = "PortService";
        public const string CassetteService = "CassetteService";
        public const string JobService = "JobService";
        public const string CIMMessageService = "CIMMessageService";
        public const string SubBlockService = "SubBlockService";
        public const string VCRService = "VCRService";
        public const string ProcessDataService = "ProcessDataService";
        public const string DailyCheckService = "DailyCheckService";
        public const string DenseBoxService = "DenseBoxService";
        public const string PaperBoxService = "PaperBoxService";
        public const string PalletService = "PalletService";
        public const string ActiveSocketService = "ActiveSocketService";
        public const string PassiveSocketService = "PassiveSocketService";
        public const string MESMessageService = "MESService";
        public const string DenseBoxCassetteService = "DenseBoxCassetteService";
        public const string DenseBoxPortService = "DenseBoxPortService";
        public const string RobotStatusService = "RobotService";
        public const string EvisorService = "EvisorService";   //Watson Add 20150311 For 通知IT

        #region [ For Robot Use ]

        public const string RobotSelectJobService = "RobotSelectJobService";
        public const string RobotCoreService = "RobotCoreService";
        public const string RobotFilterService = "JobFilterService";
        public const string RobotOrderByService = "JobOrderByService";
        public const string RobotCommandService = "RobotCommandService";
        //20160525
        public const string RobotRouteStepJumpService = "JobRouteStepJumpService";
        public const string RobotSpecialService = "RobotSpecialService";
        #endregion

    }

    public class ArithmeticOperator
    {
        public const string Empty = " ";
        public const string PlusSign = "+";
        public const string MinusSign = "-";
        public const string TimesSign = "*";
        public const string DivisionSign = "/";
    }

    public class MES_ReasonCode
    {
        #region Loader
        #region---Cancel
        /// <summary>
        /// [Cancel] validate ng from mes at Loader
        /// </summary>
        public const string Loader_BC_Cancel_Validation_NG_From_MES = "LMESNG";
        /// <summary>
        /// [Cancel] cst map download, EQP reply NG at loader
        /// </summary>
        public const string Loader_BC_Cancel_EQ_Reply_CassetteControl_NG = "LCSTMAPNG";
        /// <summary>
        /// [Cancel] Data Transfer NG at Loader
        /// </summary>
        public const string Loader_BC_Cancel_Data_Transfer_NG = "LDTNG";
        /// <summary>
        /// [Cancel] BC Client Cancel at Loader
        /// </summary>
        public const string Loader_BC_Cancel_From_BC_Client = "LBCCS";
        /// <summary>
        /// [Cancel] EQP Cancel at Loader
        /// </summary>
        public const string Loader_EQ_Cancel = "LEQCS";
        /// <summary>
        /// [Cancel] OP Cancel at Loader
        /// </summary>
        public const string Loader_OP_Cancel = "LOPCS";
        #endregion
        //======================================
        #region---Abort
        /// <summary>
        /// [Abort] BC abort in bc client at Loader.
        /// </summary>
        public const string Loader_BC_Abort = "LBCAB";
        /// <summary>
        /// [Abort] OP abort at loader
        /// </summary>
        public const string Loader_OP_Abort = "LOPAB";
        /// <summary>
        /// [Abort] EQP abort at loader
        /// </summary>
        public const string Loader_EQ_Abort = "LEQAB";
        #endregion
        #endregion

        //**************************************

        #region Unloader
        #region---Cancel
        /// <summary>
        /// [Cancel] validate ng from mes at Unloader
        /// </summary>
        public const string Unloader_BC_Cancel_Validation_NG_From_MES = "UMESNG";
        #endregion
        //======================================
        #region---Abort
        /// <summary>
        /// [Abort] BC abort in bc client at Unloader (no neet change cst dsp flag).
        /// </summary>
        public const string Unloader_BC_Abort = "UBCAB";
        /// <summary>
        /// [Abort] OP abort at Unloader (no need change cst dsp flag)
        /// </summary>
        public const string Unloader_OP_Abort = "UOPAB";
        /// <summary>
        /// [Abort] EQP abort at Unloader
        /// </summary>
        public const string Unloader_EQ_Abort = "UEQAB";
        /// <summary>
        /// [Cancel] cst map download, EQP reply NG at Unloader
        /// </summary>
        public const string Unloader_BC_Cancel_EQ_Reply_CassetteControl_NG = "UCSTMAPNG";
        /// <summary>
        /// [Cancel] Data Transfer NG at Unloader (no need change cst dsp flag)
        /// </summary>
        public const string Unloader_BC_Cancel_Data_Transfer_NG = "UDTNG";
        /// <summary>
        /// [Cancel] BC Client Cancel at Unloader (no need change cst dsp flag)
        /// </summary>
        public const string Unloader_BC_Cancel_From_BC_Client = "UBCCS";
        /// <summary>
        /// [Cancel] EQP Cancel at Unloader (no need change cst dsp flag)
        /// </summary>
        public const string Unloader_EQ_Cancel = "UEQCS";
        /// <summary>
        /// [Cancel] OP Cancel at Unloader (no need change cst dsp flag)
        /// </summary>
        public const string Unloader_OP_Cancel = "UOPCS";
        #endregion
        #endregion
    }

    public class MES_ReasonText
    {
        #region Loader
        #region---Cancel
        /// <summary>
        /// [Cancel] validate ng from mes at Loader
        /// </summary>
        public const string Loader_BC_Cancel_Validation_NG_From_MES = "Loader BC Cancel Validation NG From MES";
        /// <summary>
        /// [Cancel] cst map download, EQP reply NG at loader
        /// </summary>
        public const string Loader_BC_Cancel_EQ_Reply_CassetteControl_NG = "Loader BC Cancel EQ Reply CassetteControl NG";
        /// <summary>
        /// [Cancel] Data Transfer NG at Loader
        /// </summary>
        public const string Loader_BC_Cancel_Data_Transfer_NG = "Loader BC Cancel Data Transfer NG";
        /// <summary>
        /// [Cancel] BC Client Cancel at Loader
        /// </summary>
        public const string Loader_BC_Cancel_From_BC_Client = "Loader BC Cancel From BC Client";
        /// <summary>
        /// [Cancel] EQP Cancel at Loader
        /// </summary>
        public const string Loader_EQ_Cancel = "Loader EQ Cancel";
        /// <summary>
        /// [Cancel] OP Cancel at Loader
        /// </summary>
        public const string Loader_OP_Cancel = "Loader OP Cancel";

        public const string MES_Download_Slot_Map_Mismatch = "MES Download Slot Map Mismatch";

        public const string MES_First_Glass_Check_Report_Reply_NG = "MES First Glass Check Report Reply NG";

        public const string T9_Timeout_Cassette_Have_Been_Cancel = "T9 Timeout Cassette Have Been Cancel";

        public const string Cassette_Data_Transfer_Error_BC_Abnormal_Exception_Error = "Cassette Data Transfer Error: BC Abnormal Exception Error";

        public const string Cassette_Data_Transfer_Error_Cassette_Have_Been_Cancel = "Cassette Data Transfer Error Cassette Have Been Cancel";

        public const string Recipe_Check_NG_From_EQP = "Recipe Check NG From EQP";

        public const string MES_Download_Recipe_Group_Count_Error = "MES Download Recipe Group Count Error";

        public const string MES_Validate_Reply_NG = "MES Validate Reply NG";
        #endregion
        //======================================
        #region---Abort
        /// <summary>
        /// [Abort] BC abort in bc client at Loader.
        /// </summary>
        public const string Loader_BC_Abort = "Loader BC Abort";
        /// <summary>
        /// [Abort] OP abort at loader
        /// </summary>
        public const string Loader_OP_Abort = "Loader OP Abort";
        /// <summary>
        /// [Abort] EQP abort at loader
        /// </summary>
        public const string Loader_EQ_Abort = "Loader EQ Abort";
        #endregion
        #endregion

        //**************************************

        #region Unloader
        #region---Cancel
        /// <summary>
        /// [Cancel] validate ng from mes at Unloader
        /// </summary>
        public const string Unloader_BC_Cancel_Validation_NG_From_MES = "Unloader BC Cancel Validation NG From MES";
        #endregion
        //======================================
        #region---Abort
        /// <summary>
        /// [Abort] BC abort in bc client at Unloader (no neet change cst dsp flag).
        /// </summary>
        public const string Unloader_BC_Abort = "Unloader BC Abort";
        /// <summary>
        /// [Abort] OP abort at Unloader (no need change cst dsp flag)
        /// </summary>
        public const string Unloader_OP_Abort = "Unloader OP Abort";
        /// <summary>
        /// [Abort] EQP abort at Unloader
        /// </summary>
        public const string Unloader_EQ_Abort = "Unloader EQ Abort";
        /// <summary>
        /// [Cancel] cst map download, EQP reply NG at Unloader
        /// </summary>
        public const string Unloader_BC_Cancel_EQ_Reply_CassetteControl_NG = "Unloader BC_Cancel EQ Reply CassetteControl NG";
        /// <summary>
        /// [Cancel] Data Transfer NG at Unloader (no need change cst dsp flag)
        /// </summary>
        public const string Unloader_BC_Cancel_Data_Transfer_NG = "Unloader BC Cancel Data Transfer NG";
        /// <summary>
        /// [Cancel] BC Client Cancel at Unloader (no need change cst dsp flag)
        /// </summary>
        public const string Unloader_BC_Cancel_From_BC_Client = "Unloader BC Cancel From BC_Client";
        /// <summary>
        /// [Cancel] EQP Cancel at Unloader (no need change cst dsp flag)
        /// </summary>
        public const string Unloader_EQ_Cancel = "Unloader EQ Cancel";
        /// <summary>
        /// [Cancel] OP Cancel at Unloader (no need change cst dsp flag)
        /// </summary>
        public const string Unloader_OP_Cancel = "Unloader OP Cancel";
        #endregion
        #endregion
    }
    public class keyHost
    {
        public const string HEADER = "HEADER";
        public const string MESSAGENAME = "MESSAGENAME";
        public const string REPLYSUBJECTNAME = "REPLYSUBJECTNAME";
        public const string LISTENER = "LISTENER";

        public const string MESSAGE = "MESSAGE";
        public const string HEAD = "HEAD";
        public const string BODY = "BODY";
        public const string RETURN = "RETURN";
        public const string TRANSACTIONID = "TRANSACTIONID";
        public const string INBOXNAME = "INBOXNAME";
        //--------------A------------------
        public const string ABNORMALCODE = "ABNORMALCODE";
        public const string ABNORMALVALUE = "ABNORMALVALUE";
        public const string ABNORMALSEQ = "ABNORMALSEQ";
        public const string ABNORMALENG = "ABNORMALENG";
        public const string ABORTFLAG = "ABORTFLAG";
        public const string ACDEFECTNAME = "ACDEFCTNAME";
        public const string ACDEFECTVALUE = "ACDEFECTVALUE";
        public const string ACKNOWLEDGE = "ACKNOWLEDGE";
        public const string ACCUMULATIONSTATUS = "ACCUMULATIONSTATUS";
        public const string AGINGENABLE = "AGINGENABLE";
        public const string ALNSIDE = "ALNSIDE";
        public const string ALARMCODE = "ALARMCODE";
        public const string ALARMLEVEL = "ALARMLEVEL";
        public const string ALARMSTATE = "ALARMSTATE";
        public const string ALARMTEXT = "ALARMTEXT";
        public const string ALARMTIMESTAMP = "ALARMTIMESTAMP";
        public const string ALIGNERNAME = "ALIGNERNAME";
        public const string AOIBYPASS = "AOIBYPASS";
        public const string ABNORMALCODELIST = "ABNORMALCODELIST";
        public const string AVERAGE = "AVERAGE";
        public const string AUTOLABELPRINTFLAG = "AUTOLABELPRINTFLAG";
        public const string AUTOCLAVESAMPLING = "AUTOCLAVESAMPLING";
        public const string AUTOCLAVESKIP = "AUTOCLAVESKIP";
        public const string ARRAYPRODUCTNAME = "ARRAYPRODUCTNAME";
        public const string ARRAYSUBPRODUCTGRADE = "ARRAYSUBPRODUCTGRADE";
        public const string AGINGTIME = "AGINGTIME";
        public const string AGINGPALLETID = "AGINGPALLETID";
        public const string ARRAYLOTNAME = "ARRAYLOTNAME";
        public const string ARRAYPRODUCTSPECNAME = "ARRAYPRODUCTSPECNAME";
        public const string AGINGPALLETNAME = "AGINGPALLETNAME";
        public const string ARRAYPRODUCTSPECVER = "ARRAYPRODUCTSPECVER";
        public const string ARRAYTTPEQVERCODE = "ARRAYTTPEQVERCODE";
        public const string ACTIONTYPE = "ACTIONTYPE";
        public const string ARRAYDEFECTCODES = "ARRAYDEFECTCODES";
        public const string ARRAYDEFECTADDRESS = "ARRAYDEFECTADDRESS";
        public const string ADJUSTFLAG="ADJUSTFLAG";//add by tom.bian for t3 module
        public const string ASSEMBLYNGFLAG="ASSEMBLYNGFLAG";
        public const string ASSEMBLYINSPECTIONNGFLAG="ASSEMBLYINSPECTIONNGFLAG";
        public const string AGINGSTAYTIME="AGINGSTAYTIME";
       
        //-------------B--------------------
        public const string BATCHID = "BATCHID";
        public const string BLOCKCUTSAMPLEFLAG = "BLOCKCUTSAMPLEFLAG";
        public const string BOXLIST = "BOXLIST";
        public const string BOXQUANTITY = "BOXQUANTITY";
        public const string BOXWEIGHT = "BOXWEIGHT";
        public const string BOXNAME = "BOXNAME";
        public const string BOXTYPE = "BOXTYPE";
        public const string BOXCAPACITY = "BOXCAPACITY";    //Add by marine for T3 MES 2015/9/8
        public const string BOX = "BOX";
        public const string BOXGRADE = "BOXGRADE";
        public const string BOXSETTINGCODE = "BOXSETTINGCODE";
        public const string BLNAME = "BLNAME";
        public const string BCREGISTERRESULT = "BCREGISTERRESULT";
        public const string BCRECIPEID = "BCRECIPEID";
        public const string BCPRODUCTTYPE = "BCPRODUCTTYPE";
        public const string BCPRODUCTID = "BCPRODUCTID";
        public const string BOXULDFLAG = "BOXULDFLAG";
        public const string BOMLIST = "BOMLIST";
        public const string BOM = "BOM";
        public const string BOMVERSION = "BOMVERSION";
        public const string BYPASSQTY = "BYPASSQTY";
        public const string BCSERIALNO = "BCSERIALNO";
        public const string BCRECIPEIDLENGTH = "BCRECIPEIDLENGTH";
        public const string BLOCKJUDGES = "BLOCKJUDGES";//Add by shihyang for T3 MES 20150928
        public const string BOXIDLIST = "BOXIDLIST";         //AddBy yangZhenteng
        public const string BOXID = "BOXID";                 //AddBy yangZhenteng
        public const string BOXIDQUANTITY = "BOXIDQUANTITY"; //AddBy yangZhenteng
        //-------------C---------------------
        public const string CASSETTESEQUENCENO = "CASSETTESEQUENCENO";//Add by Tom.bian for t3 module
        public const string CARRIERNAME = "CARRIERNAME";
        public const string CARRIERTYPE = "CARRIERTYPE";
        public const string CARTRIGELIFETIME = "CARTRIGELIFETIME";
        public const string CELLTTPFLAG = "CELLTTPFLAG";
        public const string CONTROLSTATENAME = "CONTROLSTATENAME";
        public const string CHANGEPLANTYPE = "CHANGEPLANTYPE";
        public const string CHAMBERLIST = "CHAMBERLIST";
        public const string CHAMBER = "CHAMBER";
        public const string CHAMBERID = "CHAMBERID";
        public const string CHAMBERRUNMODE = "CHAMBERRUNMODE";  //Add by marine 2015/7/10 for T3 MES
        public const string CSTOPERFLAG = "CSTOPERFLAG";
        public const string CSTCOMMAND = "CSTCOMMAND";
        public const string CODE = "CODE";
        public const string CSTOPERMODE = "CSTOPERMODE";
        public const string CLEANFLAG = "CLEANFLAG";
        public const string CLEANRESULT = "CLEANRESULT";
        public const string CARRIERSETCODE = "CARRIERSETCODE";
        public const string CFTYPE1REPAIRCOUNT = "CFTYPE1REPAIRCOUNT";
        public const string CFTYPE2REPAIRCOUNT = "CFTYPE2REPAIRCOUNT";
        public const string CARBONREPAIRCOUNT = "CARBONREPAIRCOUNT";
        public const string CARRIERSETTINGCODE = "CARRIERSETTINGCODE";
        public const string CASEID = "CASEID";
        public const string CARTNAME = "CARTNAME";
        public const string COUNT = "COUNT";
        public const string CFPRODUCTNAME = "CFPRODUCTNAME";
        public const string CSTMAPGRADE = "CSTMAPGRADE";
        public const string CFSUBPRODUCTGRADE = "CFSUBPRODUCTGRADE";
        public const string COFCASTRETE = "COFCASTRETE";
        public const string CREATEDCOUNT = "CREATEDCOUNT";
        public const string CURRENTPLANNAME = "CURRENTPLANNAME";
        public const string CURRENTTIME = "CURRENTTIME";
        public const string CHAMBERNAME = "CHAMBERNAME";
        public const string CIMOFFEQPLIST = "CIMOFFEQPLIST";
        public const string CIMMESSAGE = "CIMMESSAGE";
        public const string CQLTFLAG = "CQLTFLAG";
        public const string CFSHORTCUTMODE = "CFSHORTCUTMODE";
        public const string CURRENTRECIPEID = "CURRENTRECIPEID";
        public const string COA2MASKEQPID = "COA2MASKEQPID";
        public const string CHECKEQPFLAG = "CHECKEQPFLAG";
        public const string CASSETTEID = "CASSETTEID";
        public const string CURRENTSITE ="CURRENTSITE";
        public const string CURRENTFACTORYNAME ="CURRENTFACTORYNAME";
        public const string CROSSLINEFLAG = "CROSSLINEFLAG";
        public const string CFDEFECTCODES = "CFDEFECTCODES";
        public const string CFDEFECTADDRESS = "CFDEFECTADDRESS";
        public const string CAPACITY = "CAPACITY"; //Add by marine 2015/8/13 for T3 MES
        public const string CELLSAMPLEFLAG = "CELLSAMPLEFLAG"; //Add by shihyang 2015/8/18 for T3 MES
        public const string CFSIDERESIDUEFLAG = "CFSIDERESIDUEFLAG"; //Add by shihyang 2015/8/18 for T3 MES
        public const string CIMMODE = "CIMMODE";//add by tom.bian for t3 module
        public const string CHECKWORKORDERFLAG = "CHECKWORKORDERFLAG";//add by tom.bian for t3 module
        public const string CLEANBYPASFLAG="CLEANBYPASFLAG";//add by tom.bian for t3 module
        public const string CONSUMEDQTY = "CONSUMEDQTY";//add by Ray.Kuo for t3 module
        public const string COUNTRY = "COUNTRY";//Add by sy for T3 MES 2016/1/20
        public const string CARRIERHOLDFLAG = "CARRIERHOLDFLAG";//20170112 sy modify  by MES SPEC 1.58
        public const string CONSUMMATERIALID = "CONSUMMATERIALID";//Add by Huangjiayin 20170301 for APR-PI
        //------------D---------------------
        public const string DENSEBOXLIST = "DENSEBOXLIST";
        public const string DENSEBOXID = "DENSEBOXID";
        public const string DATETIME = "DATETIME";
        public const string DCNAME = "DCNAME";
        public const string DCTYPE = "DCTYPE";
        public const string DCVALUE = "DCVALUE";
        public const string DESCRIPTION = "DESCRIPTION";
        public const string DUMUSEDCOUNT = "DUMUSEDCOUNT";
        public const string DENSEBOXID1 = "DENSEBOXID1";
        public const string DENSEBOXID2 = "DENSEBOXID2";
        public const string DEFECTLIST = "DEFECTLIST";
        public const string DEFECTCODELIST = "DEFECTCODELIST"; //20150126
        public const string DOUBLERUNFLAG = "DOUBLERUNFLAG";//add for T3 Array Special
        public const string DISCARDJUDGES = "DISCARDJUDGES";//add for T3 by sy
        public const string DATATYPE = "DataType";//20161229 sy add Serial SPEC 
        public const string DATACOUNT = "DataCount";//20161229 sy add Serial SPEC  
        public const string DATAITEMLIST = "DataItemList";//20161229 sy add Serial SPEC 
        public const string DATAITEM = "DataItem";//20161229 sy add Serial SPEC 
        //------------E----------------------
        public const string EVENTUSER = "EVENTUSER";
        public const string EDAPROCESSDATALIST = "EDAProcessDataList";
        public const string EXPSAMPLING = "EXPSAMPLING";
        public const string EVENTCOMMENT = "EVENTCOMMENT";
        public const string EXERESULT = "EXERESULT";
        public const string ENVIRONMENTFLAG = "ENVIRONMENTFLAG";
        public const string EMPTYBOXFLAG = "EMPTYBOXFLAG";//add for T3 by sy
        //------------F--------------------------
        public const string FACILITYPARALIST = "FACILITYPARALIST";
        public const string FILENAME = "FILENAME";
        public const string FINALTESTNGFLAG="FINALTESTNGFLAG";
        //------------G--------------------------
        public const string GROUPID = "GROUPID";
        public const string GMURAFLAG = "GMURAFLAG";
        public const string GAPSAMPLEFLAG = "GAPSAMPLEFLAG";
        public const string GLASSSIZE = "GLASSSIZE";
        public const string GLASSTURNFLAG = "GLASSTURNFLAG";
        public const string GMISAMPLEFLAG = "GMISAMPLEFLAG";//Add by sy 2015/12/06 for T3 MES
        public const string GRADERANKGROUP = "GRADERANKGROUP";//Add by Menghui 20161207 for Cell Filedata

        //------------H-------------------------
        public const string HEADID = "HEADID";
        public const string HOSTPRODUCTNAME = "HOSTPRODUCTNAME";
        public const string HOLDFLAG = "HOLDFLAG";
        public const string HOLDSTATE = "HOLDSTATE";
        public const string HOLDMACHINE = "HOLDMACHINE";
        public const string HOLDCOMMENT = "HOLDCOMMENT";    //Add by marine 2015/7/17 for T3 MES
        public const string HOSTLINERECIPENAME = "HOSTLINERECIPENAME";
        public const string HOSTPRODUCTRECIPENAME = "HOSTPRODUCTRECIPENAME";
        public const string HOSTRECIPENAME = "HOSTRECIPENAME";
        public const string HOSTRECIPEID = "HOSTRECIPEID";
        public const string HOSTPPID = "HOSTPPID";
        //------------I------------------------
        public const string ITEMLIST = "ITEMLIST";
        public const string ITEM = "ITEM";
        public const string ITEMNAME = "ITEMNAME";
        public const string ITEMVALUE = "ITEMVALUE";
        public const string INDEXEROPERMODE = "INDEXEROPERMODE";
        public const string INLINEINFLAG = "INLINEINFLAG";  //Add by marine 2015/7/14 for T3 MES
        public const string INLINERWCOUNT = "INLINERWCOUNT";
        public const string INSPECTIONMODE = "INSPECTIONMODE";
        public const string INCOMPLETEDATE = "INCOMPLETEDATE";
        public const string INCOMPLETECASSETTEDATALIST = "INCOMPLETECASSETTEDATALIST";
        public const string ISPIREWORK = "ISPIREWORK";
        public const string INSPECTIONFLAG = "INSPECTIONFLAG";
        public const string INSPECTIONTIME = "INSPECTIONTIME";
        public const string INSPECTIONRESULT = "INSPECTIONRESULT";
        public const string ISRTP = "ISRTP";
        public const string ISMIXEDLAYOUT = "ISMIXEDLAYOUT"; //Add By Yangzhenteng 20190316;
        //------------J-----------------------
        public const string JOBRECIPENAME = "JOBRECIPENAME";
        public const string JOBSEQUENCENO = "JOBSEQUENCENO"; //Add by Tom.bian for t3 module
        public const string JOBJUDGE = "JOBJUDGE"; //add by tom.bian fo t3 module
        public const string JUDGERESULT = "JUDGERESULT";//Add By Yangzhenteng0420                                                   
        //-------------K-----------------------
        //-------------L----------------------
        public const string LOT = "LOT";
        public const string LOTPRIORITY = "LOTPRIORITY"; //ADD BY HUJUNPENG 20190617
        public const string LINENAME = "LINENAME";
        public const string LINEOPERMODE = "LINEOPERMODE";
        public const string LOTNAME = "LOTNAME";
        public const string LINERECIPENAME = "LINERECIPENAME";
        public const string LINERECIPENAMELIST = "LINERECIPENAMELIST";
        public const string LOTLIST = "LOTLIST";
        public const string LIMITLIST = "LIMITLIST";
        public const string LOWERLIMIT = "LOWERLIMIT";
        public const string LCDROPLIST = "LCDROPLIST";
        public const string LCDROPAMOUNT = "LCDROPAMOUNT";
        public const string LINESTATENAME = "LINESTATENAME";
        public const string LIFEQTIME = "LIFEQTIME";
        public const string LOTINFO = "LOTINFO";
        public const string LASTMAINCHAMBERNAME = "LASTMAINCHAMBERNAME";
        public const string LASERREPAIRCOUNT = "LASERREPAIRCOUNT";
        public const string LASTMAINEQPNAME = "LASTMAINEQPNAME";
        public const string LASTMAINPPID = "LASTMAINPPID";
        public const string LANGUAGE = "LANGUAGE";
        public const string LOCALRECIPENAME = "LOCALRECIPENAME";
        public const string LINEQTIMELIST = "LINEQTIMELIST";
        public const string LINEQTIME = "LINEQTIME";
        public const string LINELIST = "LINELIST";
        public const string LINE = "LINE";
        public const string LINERECIPE = "LINERECIPE";
        public const string LINECHANGEFLAG = "LINECHANGEFLAG";     //Add by marine for T3 MES 2015/9/9
        public const string LOGINUSERID = "LOGINUSERID";
        public const string LINKPROCESSFLAG = "LINKPROCESSFLAG";
        public const string LINKPROCESSRESULT = "LINKPROCESSRESULT";
        public const string LASTGLASSFLAG = "LASTGLASSFLAG";
        public const string LSRCFLAG = "LSRCFLAG";//add by tom.bian for t3 module
        //-------------M---------------------
        public const string MACHINENAME = "MACHINENAME";
        public const string MACHINESTATENAME = "MACHINESTATENAME";
        public const string MACHINEQTIMELIST = "MACHINEQTIMELIST";
        public const string MACHINEQTIME = "MACHINEQTIME";
        public const string MACROFLAG = "MACROFLAG";                       //Add For T3 CF Photo Reserve Flag Logic
        public const string MATERIALABNORMALCODE = "MATERIALABNORMALCODE";  //Add by marine 2015/7/9 for T3 MES
        public const string MATERIALWARNINGTIME = "MATERIALWARNINGTIME";    //Add by marine 2015/7/9 for T3 MES
        public const string MATERIALBATCHSAME = "MATERIALBATCHSAME";   //Add by matine 2018/2/27 for T3 MES
        public const string MATERIALCOUNT = "MATERIALCOUNT";    //Add by marine 2015/7/9 for T3 MES
        public const string MATERIALDURABLENAME = "MATERIALDURABLENAME";    //Add by marine 2015/7/9 for T3 MES
        public const string MATERIALTYPE = "MATERIALTYPE";
        public const string MATERIALNAME = "MATERIALNAME";
        public const string MATERIALPOSITION = "MATERIALPOSITION";    //Add by marine 2015/7/9 for T3 MES
        public const string MATERIALSTATE = "MATERIALSTATE";
        public const string MATERIALWEIGHT = "MATERIALWEIGHT";      //Add by marine 2015/7/8 for T3 MES
        public const string MACHINERECIPENAME = "MACHINERECIPENAME";
        public const string MATERIALCHANGEFLAG = "MATERIALCHANGEFLAG";
        public const string MACHINELIST = "MACHINELIST";
        public const string MACHINE = "MACHINE";
        public const string MAINPRODUCT = "MAINPRODUCT";
        public const string MASKLIST = "MASKLIST";
        public const string MASKLOCATION = "MASKLOCATION";
        public const string MASK = "MASK";
        public const string MASKPOSITION = "MASKPOSITION";
        public const string MASKNAME = "MASKNAME";
        public const string MASKSTATE = "MASKSTATE";
        public const string MASKUSECOUNT = "MASKUSECOUNT";
        public const string MATERIALLIST = "MATERIALLIST";
        public const string MATERIAL = "MATERIAL";
        public const string MACHINESTATE = "MACHINESTATE";
        public const string MAXINLINERWCOUNT = "MAXINLINERWCOUNT";
        public const string MAXRWCOUNT = "MAXRWCOUNT";
        public const string MAXUSECOUNT = "MAXUSECOUNT";
        public const string MODELNAME = "MODELNAME";
        public const string MODELVERSION = "MODELVERSION";
        public const string MESPROCESSDATALIST = "MESProcessDataList";
        public const string MATERIALMODE = "MATERIALMODE";
        public const string MACOJUDGE = "MACOJUDGE";
        public const string MURACODES = "MURACODES";
        public const string MESTRXID = "MESTRXID";
        public const string MACHINEENABLE = "MACHINEENABLE";
        public const string MASKCARRIERNAME = "MASKCARRIERNAME";
        public const string MASKDETAILTYPE = "MASKDETAILTYPE";
        public const string MASKLIMITUSECOUNT = "MASKLIMITUSECOUNT";
        public const string MASKACTION = "MASKACTION"; //Add by marine for T3 MES 2015/8/18
        public const string MATERIALCONSUMEDQUANTITY = "MATERIALCONSUMEDQUANTITY"; // Add by marine for t3 Module 2015/12/28
        public const string MATERIALSPECNAME = "MATERIALSPECNAME"; // Add by sy for t3 mes spec 1.43 20160601
        public const string MESPROCESSFLAG = "MESPROCESSFLAG"; //add by yang 20170119
        public const string MATERIALSITE = "MATERIALSITE";//add by hujunpeng 20190223
        //-------------N---------------------
        public const string NEXTPLANNAME = "NEXTPLANNAME";
        public const string NOPROCESSFLAG = "NOPROCESSFLAG";
        public const string NEWTANKNAME = "NEWTANKNAME";
        public const string NEWSITE = "NEWSITE";
        public const string NODESTACK = "NODESTACK";
        public const string NAME = "NAME";//Add by sy for T3 MES 2016/1/20
        public const string NOTE = "NOTE";//Add by sy for T3 MES 2016/1/20
        public const string NODENODEVICEID = "NodeNoDeviceID";//20161229 sy add Serial SPEC 
        //-------------O------------------
        public const string OCHNGFLAG = "OCHNGFLAG";
        public const string OCHDISABLEFLAG = "OCHDISABLEFLAG";
        public const string OCHBYPASSFLAG = "OCHBYPASSFLAG";
        public const string OQCNGRESULT = "OQCNGRESULT";
        public const string OWNERID = "OWNERID";
        public const string OPERATIONSTATIONNAME = "OPERATIONSTATIONNAME";
        public const string OWNERTYPE = "OWNERTYPE";
        public const string ORIGINALPRODUCTLIST = "ORIGINALPRODUCTLIST";
        public const string ORIGINALPRODUCT = "ORIGINALPRODUCT";
        public const string ORIGINALPRODUCTNAME = "ORIGINALPRODUCTNAME";
        public const string OVERRESULT = "OVERRESULT";
        public const string ORIENTEDFACTORYNAME = "ORIENTEDFACTORYNAME";
        public const string ORAIENTEDFACTORYNAME = "ORAIENTEDFACTORYNAME"; //For OEE 可能有错
        public const string ORIENTEDSITE ="ORIENTEDSITE";
        public const string OPI_PPID = "OPI_PPID";
        public const string OPERATOR = "OPERATOR";
        public const string OPI_PRODUCTRECIPENAME = "OPI_PRODUCTRECIPENAME";
        public const string OPI_LINERECIPENAME = "OPI_LINERECIPENAME";
        public const string OPI_CURRENTLINEPPID = "OPI_CURRENTLINEPPID";
        public const string OPI_CROSSLINEPPID = "OPI_CROSSLINEPPID";
        public const string OPI_CARRIERSETCODE = "OPI_CARRIERSETCODE";
        public const string OPI_PRDCARRIERSETCODE = "OPI_PRDCARRIERSETCODE";
        public const string OPI_PROCESSFLAG = "OPI_PROCESSFLAG"; //yang
        public const string ODFDEFECTCODES = "ODFDEFECTCODES";
        public const string ODFDEFECTADDRESS = "ODFDEFECTADDRESS";
        public const string OUTBOXNAME = "OUTBOXNAME";
        //----
        public const string OCHSHIFTDISABLEFLAG="OCHSHIFTDISABLEFLAG";//add by tom.bian for t3 module
        public const string OCHAKKCOMDISABLEFLAG="OCHAKKCOMDISABLEFLAG";//add by tom.bian for t3 module
        public const string OCHBURRDISABLEFLAG="OCHBURRDISABLEFLAG";
        public const string OCHSHIFTSAMPLINGPASSFLAG="OCHSHIFTSAMPLINGPASSFLAG";
        public const string OCHAKKCOMSAMPLINGPASSFLAG="OCHAKKCOMSAMPLINGPASSFLAG";
        public const string OCHBURRSAMPLINGPASSFLAG="OCHBURRSAMPLINGPASSFLAG";
        public const string OCHNOPROCESSINGFLAG="OCHNOPROCESSINGFLAG";
        public const string OQCBANK = "OQCBANK";

        
        
        //-------------P-----------------
        public const string PPID = "PPID";
        public const string POSITION = "POSITION";
        public const string PRODUCTNAME = "PRODUCTNAME";
        public const string PRODUCTGRADE = "PRODUCTGRADE";
        public const string PRODUCTLIST = "PRODUCTLIST";
        public const string PORTNAME = "PORTNAME";
        public const string PORTMODE = "PORTMODE";
        public const string RECIPELIST = "RECIPELIST";
        public const string PARTIALFULLFLAG = "PARTIALFULLFLAG";
        public const string PRODUCTOX = "PRODUCTOX";//ADD BY HUANGJIAIN: 20170425
        public const string PRODUCTQUANTITY = "PRODUCTQUANTITY";
        
        public const string PRODUCTSPECLAYOUT = "PRODUCTSPECLAYOUT";
        public const string SUBPRODUCTSPECLAYOUT = "SUBPRODUCTSPECLAYOUT";
        public const string PRODUCTSPECNAME = "PRODUCTSPECNAME";
        public const string PRODUCTSPECNAMEFORTFT = "PRODUCTSPECNAMEFORTFT";
        public const string PRODUCTSPECNAMEFORCF = "PRODUCTSPECNAMEFORCF";
        public const string PRODUCTSPECVER = "PRODUCTSPECVER";
        public const string PRODUCTSPECGROUP = "PRODUCTSPECGROUP";
        public const string PROCESSOPERATIONNAME = "PROCESSOPERATIONNAME";
        public const string PORTTYPE = "PORTTYPE";
        public const string PROCESSTYPE = "PROCESSTYPE";
        public const string PRODUCTOWNER = "PRODUCTOWNER";
        public const string PORTUSETYPE = "PORTUSETYPE";
        public const string PORTLIST = "PORTLIST";
        public const string PORT = "PORT";
        public const string PORTACCESSMODE = "PORTACCESSMODE";
        public const string PORTSTATENAME = "PORTSTATENAME";
        public const string PORTTRANSFERSTATE = "PORTTRANSFERSTATE";
        public const string PORTENABLEFLAG = "PORTENABLEFLAG";
        public const string PORTOPERMODE = "PORTOPERMODE";
        public const string PRODUCT = "PRODUCT";
        public const string PRODUCTJUDGE = "PRODUCTJUDGE";
        public const string PRODUCTGCPTYPE = "PRODUCTGCPTYPE";
        public const string PRODUCTPROCESSTYPE = "PRODUCTPROCESSTYPE";

        public const string PARA = "PARA";
        public const string PARANAME = "PARANAME";
        public const string PARAVALUE = "PARAVALUE";
        public const string PARTNO = "PARTNO";
        public const string PLANNEDPRODUCTSPECNAME = "PLANNEDPRODUCTSPECNAME";
        public const string PLANNEDPROCESSOPERATIONNAME = "PLANNEDPROCESSOPERATIONNAME";
        public const string PLANNEDSOURCEPART = "PLANNEDSOURCEPART";
        public const string PLANNEDPCPLANID = "PLANNEDPCPLANID"; // add upk plan ID
        public const string PAIRCARRIERNAME = "PAIRCARRIERNAME";
        public const string PLANNAME = "PLANNAME";
        public const string PAIRPRODUCT = "PAIRPRODUCT";
        public const string PROCESSRESULT = "PROCESSRESULT";
        public const string PROCESSFLOWNAME = "PROCESSFLOWNAME";
        public const string PALLETNAME = "PALLETNAME";
        public const string PARTIALFULLMODE = "PARTIALFULLMODE";
        public const string PLANNEDQUANTITY = "PLANNEDQUANTITY";
        public const string PLANLIST = "PLANLIST";
        public const string PRODUCTRECIPENAME = "PRODUCTRECIPENAME";
        public const string PAIRPRODUCTNAME = "PAIRPRODUCTNAME";
        public const string PRODUCTTYPE = "PRODUCTTYPE";
        public const string PROCESSINGTIME = "PROCESSINGTIME";
        public const string PSHEIGHTLIST = "PSHEIGHTLIST";
        public const string PSHEIGHT = "PSHEIGHT";  //Add by marine for T3 MES 2015/9/11
        public const string PRDCARRIERSETCODE = "PRDCARRIERSETCODE";
        public const string PAIRCARRIERSLOT = "PAIRCARRIERSLOT";
        public const string PRODUCTINFO = "PRODUCTINFO";
        public const string PUNCHERNAME = "PUNCHERNAME";
        public const string PUNCHERMAXIMUMCOUNT = "PUNCHERMAXIMUMCOUNT";
        public const string PUNCHERCURRENTCOUNT = "PUNCHERCURRENTCOUNT";
        public const string PRESSURECOUNT = "PRESSURECOUNT";
        public const string PUNCHCOUNT = "PUNCHCOUNT";
        public const string PRODCESSOPERATIONNAME = "PRODCESSOPERATIONNAME";
        public const string PROCESSSTATE = "PROCESSSTATE";
        public const string PULLMODEGRADE = "PULLMODEGRADE";
        public const string PROCESSLINENAME = "PROCESSLINENAME";
        public const string PROCESSLINE = "PROCESSLINE";
        public const string PROCESSLINELIST = "PROCESSLINELIST";
        public const string PAUSECOMMAND = "PAUSECOMMAND";
        public const string PORTID = "PORTID";
        public const string PORTNO = "PORTNO";
        public const string POLTYPE = "POLTYPE";    //Add by marine 2015/7/8 for T3 MES
        public const string PARANAMEORIENTED = "PARANAMEORIENTED";
        public const string PRODUCTSPECVERSION = "PRODUCTSPECVERSION";
        public const string PRODUCTTHICKNESS = "PRODUCTTHICKNESS";
        public const string PRODUCTSIZETYPE = "PRODUCTSIZETYPE";
        public const string PRODUCTSIZE = "PRODUCTSIZE";
        public const string PAIRPRODUCTSPECNAME = "PAIRPRODUCTSPECNAME";
        public const string PIDEFECTCODES = "PIDEFECTCODES";
        public const string PIDEFECTADDRESS = "PIDEFECTADDRESS";
        public const string PLANNEDGROUPNAME = "PLANNEDGROUPNAME";
        public const string PTHFLAG = "PTHFLAG";//Add by sy 2015/12/6 for T3 MES
        public const string CEMFLAG = "CEMFLAG";//Add by huangjiayin 2017/6/8 for PIL>>PDR.CEM
        public const string PANELNAME = "PANELNAME";//Add by Tom.Su 2015/12/24 for T3 ModMES
        public const string PCBNGFLAG="PCBNGFLAG";//add by tom.bian for t3 Module mes
        public const string PUNCHERLIST = "PUNCHERLIST";//add by tom.bian for t3 module mes
        public const string PLANQUANTITY = "PLANQUANTITY"; //add by RayKuo for t3 module mes
        public const string PARTID = "PARTID";//Add by sy for T3 MES 2016/1/20
        public const string PRODUCTDEFECTCODE = "PRODUCTDEFECTCODE";//Add by sy for T3 MES 2016/3/25
        public const string PAIRBOXNAME = "PAIRBOXNAME";//20170112 sy add SPEC 1.58
        public const string PANELIDLIST = "PANELIDLIST";//20180118 Add By Yangzhenteng
        public const string PANEL = "PANEL";//20180118 Add By Yangzhenteng
        public const string PANELQUANTITY = "PANELQUANTITY";//20180118 Add By Yangzhenteng
        public const string PCKPICKFLAG = "PCKPICKFLAG"; //20180428 ADD BY huangjiayin
        public const string DEFECTCODE = "DEFECTCODE";//20180926 Add BY Yangzhenteng For Panel Scrap;
        //-------------Q-----------------
        public const string QTIMEFROM = "QTIMEFROM";
        public const string QFROMNAME = "QFROMNAME";
        public const string QTIMETO = "QTIMETO";
        public const string QTONAME = "QTONAME";
        public const string QTIME = "QTIME";
        public const string QUANTITY = "QUANTITY";
        public const string QTAPFLAG = "QTAPFLAG";
        //-------------R-----------------
        public const string RECPARANAME = "RECPARANAME";
        public const string RECPARAVALUE = "RECPARAVALUE";
        public const string RECIPEPARAVALIDATIONFLAG = "RECIPEPARAVALIDATIONFLAG";
        public const string RECIPEREGISTERFLAG = "RECIPEREGISTERFLAG";
        public const string REASONCODE = "REASONCODE";
        public const string REASONCARRILERLIST = "REASONCARRILERLIST";  //Add by marine for T3 MES
        public const string REASONCARRILER = "REASONCARRILER";  //Add by marine for T3 MES 2015/9/11
        public const string REASONTEXT = "REASONTEXT";
        public const string RECIPEPARALIST = "RECIPEPARALIST";
        public const string RECYCLINGFLAG = "RECYCLINGFLAG";
        public const string RETURNCODE = "RETURNCODE";
        public const string RETURNMESSAGE = "RETURNMESSAGE";
        public const string REVPROCESSOPERATIONNAME = "REVPROCESSOPERATIONNAME";
        public const string REWORKLIST = "REWORKLIST";
        public const string REWORK = "REWORK";
        public const string REWORKFLOWNAME = "REWORKFLOWNAME";
        public const string REWORKCOUNT = "REWORKCOUNT";
        public const string RECIPENAME = "RECIPENAME";
        public const string RECIPEPARANOCHECKLIST = "RECIPEPARANOCHECKLIST";
        public const string RECIPEIDLIST = "RECIPEIDLIST";
        public const string RECIPEID = "RECIPEID";
        public const string RECIPENOPARAMETERNAME = "RECIPENOPARAMETERNAME";
        public const string RECIPEIDPARAMETERNAME = "RECIPEIDPARAMETERNAME";
        public const string RETURNMSG = "RETURNMSG";
        public const string RECIPENO = "RECIPENO";
        public const string RTPFLAG = "RTPFLAG";
        public const string RIBMARKFLAG = "RIBMARKFLAG";
        public const string REASONDESCRIPTION = "REASONDESCRIPTION"; //Add by marine for T3 module MES
        public const string RUNCARDNAME = "RUNCARDNAME";//add by tom.bian for t3 module MES
        public const string RESULTCODE = "RESULTCODE";//add by RayKuo for t3 module MES
        public const string RUNMODE = "RUNMODE";//add by sy for t3 MES 1.44
        public const string RANDOMFLAG = "RANDOMFLAG"; //add by zhuxingxing for t3 MES 
        //-------------S-----------------
        public const string SITELIST = "SITELIST";
        public const string SITE = "SITE";
        public const string SIDE = "SIDE";//Add By Yangzhenteng0420
        public const string SITENAME = "SITENAME";
        public const string SITEVALUE = "SITEVALUE";
        public const string SOURCELIST = "SOURCELIST";
        public const string SOURCE = "SOURCE";
        public const string SOURCELOTNAME = "SOURCELOTNAME";
        public const string SUBPRODUCT = "SUBPRODUCT";
        public const string SUBPRODUCTLIST = "SUBPRODUCTLIST";
        public const string SUBPRODUCTORIGINID = "SUBPRODUCTORIGINID";
        public const string SUBPRODUCTPOSITION = "SUBPRODUCTPOSITION";  //Add by marine for T3 MES
        public const string SUBPRODUCTSPECLIST = "SUBPRODUCTSPECLIST";        //Add by Yangzhenteng20190316;
        public const string SUBPRODUCTPOSITIONS = "SUBPRODUCTPOSITIONS";
        public const string SUBPRODUCTQUANTITY = "SUBPRODUCTQUANTITY";
        public const string SUBPRODUCTNAME = "SUBPRODUCTNAME";
        public const string SUBPRODUCTSPECNAME = "SUBPRODUCTSPECNAME";
        public const string SUBPRODUCTSIZE = "SUBPRODUCTSIZE";
        public const string SUBPRODUCTGRADES = "SUBPRODUCTGRADES";
        public const string SELECTEDPOSITIONMAP = "SELECTEDPOSITIONMAP";
        public const string SUBPRODUCTJPSGRADE = "SUBPRODUCTJPSGRADE";
        public const string SUBPRODUCTJPSCODE = "SUBPRODUCTJPSCODE";
        public const string SHORTCUTFLAG = "SHORTCUTFLAG";
        public const string SHIPID = "SHIPID";
        public const string SUBPRODUCTDEFECTCODE = "SUBPRODUCTDEFECTCODE";
        public const string SALEORDER = "SALEORDER";
        public const string SOURCEPART = "SOURCEPART";
        public const string SAMPLEFLAG = "SAMPLEFLAG";
        public const string SAMPLERATE = "SAMPLERATE";
        public const string SAMPLERATIO = "SAMPLERATIO";
        public const string SAMPLINGCHECKFLAG = "SAMPLINGCHECKFLAG";
        public const string STBPRODUCTSPECLIST = "STBPRODUCTSPECLIST";
        public const string STBPRODUCTSPEC = "STBPRODUCTSPEC";
        public const string SCRAPCUTFLAG = "SCRAPCUTFLAG";
        public const string SUBBOXLIST = "SUBBOXLIST";
        public const string SUBBOX = "SUBBOX";     //Add by marine for T3 MES 2015/9/11
        public const string SUBBOXNAME = "SUBBOXNAME";
        public const string SUBPRODUCTSPECS = "SUBPRODUCTSPECS";
        public const string SUBPRODUCTNAMES = "SUBPRODUCTNAMES";
        public const string SUBPRODUCTLINES = "SUBPRODUCTLINES";
        public const string SUBPRODUCTSIZETYPES = "SUBPRODUCTSIZETYPES";
        public const string SUBPRODUCTSIZES = "SUBPRODUCTSIZES";
        public const string STARTTIME = "STARTTIME";    //Add by marine for T3 MES
        public const string ENDTIME = "ENDTIME";    //Add by marine for T3 MES
        public const string SUBPRODUCTCARRIERSETCODES = "SUBPRODUCTCARRIERSETCODES";//sy add 20160121
        public const string SHIPPRODUCTSPECNAME = "SHIPPRODUCTSPECNAME"; //huangjiayin add 20170703
        public const string SUBPRODUCTSPEC = "SUBPRODUCTSPEC"; //Add By Yangzhenteng 20190316;
        //-------------T-----------------
        public const string TIMESTAMP = "TIMESTAMP";
        public const string TRACELEVEL = "TRACELEVEL";
        public const string TERMINALTEXT = "TERMINALTEXT";
        public const string TARGETLIST = "TARGETLIST";
        public const string TEMPERATURE = "TEMPERATURE";
        public const string TEMPERATURELIST = "TEMPERATURELIST";
        public const string TEMPERATUREITEM = "TEMPERATUREITEM";
        public const string TARGETPORTNAME = "TARGETPORTNAME";
        public const string TIMEOUTEQPLIST = "TIMEOUTEQPLIST";
        public const string TANKLIST = "TANKLIST";
        public const string TANKNAME = "TANKNAME";
        public const string TANK = "TANK";
        public const string TARGETNUMBER = "TARGETNUMBER";
        public const string TEMPERATUREFLAG = "TEMPERATUREFLAG";
        public const string TFTDEFECTCODES = "TFTDEFECTCODES";//add T3 MES 
        public const string TFTDEFECTADDRESS = "TFTDEFECTADDRESS";//add T3 MES 
        public const string TAMFLAG = "TAMFLAG";//Add by sy 2015/12/6 for T3 MES
        public const string TESTRESULT = "TESTRESULT";//Add by RayKuo 2015/12/25 for T3 MES
        public const string TRANSACTIONSTARTTIME = "TRANSACTIONSTARTTIME";//Add by huangjiayin 2018/03/08 for T3 MES
        //-------------U-----------------
        public const string UNITNAME = "UNITNAME";
        public const string UNITSTATENAME = "UNITSTATENAME";
        public const string UNITCOUNT = "UNITCOUNT";
        public const string USEDCOUNT = "USEDCOUNT";
        public const string UNITLIST = "UNITLIST";
        public const string UNIT = "UNIT";
        public const string UNITID = "UNITID";
        public const string UPPERLIMIT = "UPPERLIMIT";
        public const string ULDPORTLIST = "ULDPORTLIST";
        public const string UPKOWNERTYPE = "UPKOWNERTYPE";
        public const string USEDTIME = "USEDTIME";     //Add by marine 2015/7/9 for T3 MES
        public const string USERNAME = "USERNAME";
        public const string USERID = "USERID";//ADD BY huangjiayin 20180308
        public const string UVMASKNAMES = "UVMASKNAMES";//Add by sy 2016/3/19 for T3 MES
        //-------------V-----------------
        public const string VCRPRODUCTNAME = "VCRPRODUCTNAME";
        public const string VALIRESULT = "VALIRESULT";
        public const string VALUETYPE = "VALUETYPE";
        public const string VALIDATIONLIST = "VALIDATIONLIST";
        public const string VCRREADFLAG = "VCRREADFLAG";
        public const string VALICODE = "VALICODE";
        public const string VCRNAME = "VCRNAME";
        public const string VCRFLAG = "VCRFLAG";//add by hujunpeng 20190522 for auto change DCR status
        public const string VCRSTATENAME = "VCRSTATENAME";
        public const string VENDORNAME = "VENDORNAME";//Add by sy 2015/12/29 for T3 MES
        public const string VALUE = "VALUE";//Add by sy 2016/1/20 for T3 MES

        //-------------W-----------------
        public const string WEEKCODE = "WEEKCODE";
        public const string WAITTIME = "WAITTIME";
        public const string WARMCOUNT = "WARMCOUNT";//Add by hujunpeng 2018/5/8 
        public const string WORKORDER = "WORKORDER"; //add by marine for t3 module mes
        public const string WORKORDERNAME = "WORKORDERNAME"; //add by RayKuo for t3 module mes
        public const string WORKORDERSTATE = "WORKORDERSTATE"; //add by RayKuo for t3 module mes
        public const string WT = "WT";//Add by sy for T3 MES 2016/1/20
        //-------------X-----------------
        //-------------Y-----------------
        //-------------Z-----------------

        //Process End special 有空再整理到各字母 
        public const string LOI1DEFECTCODE = "LOI1DEFECTCODE";
        public const string LOI2DEFECTCODE = "LOI2DEFECTCODE";
        public const string GAPNGFLAG = "GAPNGFLAG";
        public const string SEALREPAIRFLAG = "SEALREPAIRFLAG";
        public const string SEALABNORMALFLAG = "SEALABNORMALFLAG";
        public const string TTPFLAG = "TTPFLAG";
        public const string TTPVALUE = "TTPVALUE";
        public const string HVACHIPPINGFLAG = "HVACHIPPINGFLAG";
        public const string NRPREPAIRRESULT = "NRPREPAIRRESULT";
        public const string FGRADERISKFLAG = "FGRADERISKFLAG";
        public const string CUTGRADEFLAG = "CUTGRADEFLAG";
        public const string ACGRAPFLAG = "ACGRAPFLAG";
        public const string OVENSIDE = "OVENSIDE";
        public const string VCDSIDE = "VCDSIDE";
        public const string PRLOT = "PRLOT";
        public const string CSPNUMBER = "CSPNUMBER";
        public const string HPCHAMBER = "HPCHAMBER";
        public const string DISPENSESPEED = "DISPENSESPEED";
        public const string DUMUSEDCOUNTE = "DUMUSEDCOUNT";
        public const string LASERREPAIRCOUN = "LASERREPAIRCOUN";
        public const string ITOSIDEFLAG = "ITOSIDEFLAG";
        public const string SOURCEDURABLETYPE = "SOURCEDURABLETYPE";
        public const string SAMPLETYPE = "SAMPLETYPE";
        public const string CENGFLAG = "CENGFLAG";
        public const string PROCESSFLAG = "PROCESSFLAG";
        public const string BCPROCESSFLAG = "BCPROCESSFLAG"; //add by yang 20161112
        public const string DPIPROCESSFLAG = "DPIPROCESSFLAG";
        public const string PROCESSCOMMUNICATIONSTATE = "PROCESSCOMMUNICATIONSTATE";
        public const string FMAFLAG = "FMAFLAG";
        public const string MHUFLAG = "MHUFLAG";
        public const string EQREGISTERRESULTLIST = "EQREGISTERRESULTLIST";
        public const string EQREGISTERRESULT = "EQREGISTERRESULT";
        public const string EQUIPMENTID = "EQUIPMENTID";
        public const string RETURNTEXT = "RETURNTEXT";
        public const string RECIPENUMBERLIST = "RECIPENUMBERLIST";
        public const string RECIPENUMBER = "RECIPENUMBER";
        public const string LOCALNAME = "LOCALNAME";
        public const string HOLDOPERATOR = "HOLDOPERATOR";
        public const string PERMITFLAG = "PERMITFLAG";
        public const string EVENTUSE = "EVENTUSE";

        public const string CFREWORKCOUNT = "CFREWORKCOUNT";
        public const string ACTCFREWORKCOUNT = "ACTCFREWORKCOUNT";
        public const string EXPOSUREDOPERATION = "EXPOSUREDOPERATION";
    }
    

    public class eLineType
    {
        public class ARRAY //modify t3 line type re-define cc.kuang 2015/07/03
        {
            public const string CVD_ULVAC = "CVD_ULVAC";
            public const string CVD_AKT = "CVD_AKT";
            public const string MSP_ULVAC = "MSP_ULVAC";
            public const string ITO_ULVAC = "ITO_ULVAC";
            public const string DRY_YAC = "DRY_YAC";
            public const string DRY_ICD = "DRY_ICD";
            public const string DRY_TEL = "DRY_TEL";  //add  by yang 2017/5/8
            public const string OVNITO_CSUN = "OVNITO_CSUN";
            public const string OVNPL_YAC = "OVNPL_YAC";
            public const string OVNSD_VIATRON = "OVNSD_VIATRON";
            public const string PHL_TITLE = "PHL_TITLE";
            public const string PHL_EDGEEXP = "PHL_EDGEEXP";
            public const string WET_DMS = "WET_DMS";
            public const string WEI_DMS = "WEI_DMS";
            public const string STR_DMS = "STR_DMS";
            public const string CLN_DMS = "CLN_DMS"; //add by qiumin 20171222
            public const string FLR_CHARM = "FLR_CHARM";
            public const string MAC_CONTREL = "MAC_CONTREL";
            public const string NAN_SEMILAB = "NAN_SEMILAB";
            public const string CDO_KOSAKA = "CDO_KOSAKA";
            public const string CDO_VTECKMAC = "CDO_VTECKMAC";
            public const string AOH_HBT = "AOH_HBT";
            public const string AOH_ORBOTECH = "AOH_ORBOTECH";
            public const string ATS_YANG = "ATS_YANG";
            public const string SCN_TOARY = "SCN_TOARY";
            public const string TEG_YANG = "TEG_YANG";
            public const string TTP_VTEC = "TTP_VTEC";
            public const string CHN_SEEC = "CHN_SEEC";
            public const string CAC_MYTEK = "CAC_MYTEK";
            public const string BFG_SHUZTUNG = "BFG_SHUZTUNG";
            public const string IMP_NISSIN = "IMP_NISSIN";
            public const string ELA_JSW = "ELA_JSW";
            public const string CLS_PROCDO = "CLS_PROCDO";
            public const string CLS_MACAOH = "CLS_MACAOH";
            public const string RTA_VIATRON = "RTA_VIATRON";
            public const string RSM_QUATEK = "RSM_QUATEK";
        }
        public class CELL
        {
            #region [T2]
            public const string CBPIL = "CBPIL";
            public const string CBODF = "CBODF";
            public const string CBHVA = "CBHVA";
            public const string CBCUT_1 = "CBCUT_1";
            public const string CBCUT_2 = "CBCUT_2";
            public const string CBCUT_3 = "CBCUT_3";
            public const string CBPOL_1 = "CBPOL_1";
            public const string CBPOL_2 = "CBPOL_2"; 
            public const string CBPOL_3 = "CBPOL_3";
            public const string CBDPK = "CBDPK";
            public const string CBPPK = "CBPPK";
            public const string CBPMT = "CBPMT";
            public const string CBGAP = "CBGAP";
            public const string CBPIS = "CBPIS";
            public const string CBPRM = "CBPRM";
            public const string CBGMO = "CBGMO";
            public const string CBLOI = "CBLOI";
            public const string CBNRP = "CBNRP";
            public const string CBOLS = "CBOLS";
            public const string CBSOR_1 = "CBSOR_1";
            public const string CBSOR_2 = "CBSOR_2";
            public const string CBDPS = "CBDPS";
            public const string CBATS = "CBATS";
            public const string CBDPI = "CBDPI";
            public const string CBUVA = "CBUVA";
            //public const string CBPTI = "CBPTI";  //Watson modify 20150227 PTI line 目前沒有要使用了以PMT為主line
            public const string CBMCL = "CBUAM";
            #endregion
            #region [T3]
            public const string CCPIL = "PIL";
            public const string CCPIL_2 = "PIL_2";//huangjiayin add 20180129
            public const string CCODF = "ODF";
            public const string CCODF_2 = "ODF_2";//sy add 20160907
            public const string CCPCS = "PCS";
            public const string CCCUT_1 = "CUT_1";
            public const string CCCUT_2 = "CUT_2";
            public const string CCCUT_3 = "CUT_3";
            public const string CCCUT_4 = "CUT_4";
            public const string CCCUT_5 = "CUT_5";//sy 20160705 特別  
            public const string CCCUT_6 = "CUT_6";
            public const string CCCUT_7 = "CUT_7";
            public const string CCCUT_8 = "CUT_8";
            public const string CCCUT_9 = "CUT_9";
            public const string CCCUT_10 = "CUT_10";
            public const string CCCUT_11 = "CUT_11";
            public const string CCCUT_12 = "CUT_12";
            public const string CCCUT_13 = "CUT_13";
            public const string CCCUT_14 = "CUT_14";
            public const string CCCUT_15 = "CUT_15";
            public const string CCCUT_16 = "CUT_16";
            public const string CCCUT_17 = "CUT_17";
            public const string CCCUT_18 = "CUT_18";
            public const string CCCUT_19 = "CUT_19";
            public const string CCCUT_20 = "CUT_20";
            public const string CCPOL_1 = "POL_1";
            public const string CCPOL_2 = "POL_2";
            public const string CCPOL_3 = "POL_3";
            public const string CCPOL_4 = "POL_4";
            public const string CCPOL_5 = "POL_5";
            public const string CCPOL_6 = "POL_6";
            public const string CCPOL_7 = "POL_7";
            public const string CCPOL_8 = "POL_8";
            public const string CCPOL_9 = "POL_9";
            public const string CCPOL_10 = "POL_10";
            public const string CCRWK = "RWK";
            public const string CCQUP = "QUP";
            public const string CCPCK = "PCK";
            public const string CCQPP = "QPP";
            public const string CCPPK = "PPK";
            public const string CCPDR = "PDR";
            public const string CCTAM = "TAM";
            public const string CCPTH = "PTH";
            public const string CCGAP = "GAP";
            public const string CCRWT = "RWT";
            public const string CCSOR = "SOR";
            public const string CCCHN = "CHN";
            public const string CCCRP = "CRP";
            public const string CCCRP_2 = "CRP_2";
            public const string CCCLN = "CLN";
            public const string CCQSR = "QSR";
            public const string CCOSM = "OSM";
            public const string CCOVP = "OVP";
            public const string CCCAC = "CAC";
//ADD BY huangjiayin for t3 notch
            public const string CCNLS = "NLS";
            public const string CCNRD = "NRD";
            //
            #endregion
        }
        public class CF
        {
            //20150710 Frank Add T3 CF Line Type
            public const string FCUPK_TYPE1 = "FCUPK_TYPE1";
            public const string FCUPK_TYPE2 = "FCUPK_TYPE2";
            public const string FCMPH_TYPE1 = "FCMPH_TYPE1";
            public const string FCRPH_TYPE1 = "FCRPH_TYPE1";
            public const string FCGPH_TYPE1 = "FCGPH_TYPE1";
            public const string FCBPH_TYPE1 = "FCBPH_TYPE1";
            public const string FCSPH_TYPE1 = "FCSPH_TYPE1";
            public const string FCOPH_TYPE1 = "FCOPH_TYPE1";
            public const string FCMSK_TYPE1 = "FCMSK_TYPE1";
            public const string FCPSH_TYPE1 = "FCPSH_TYPE1";
            public const string FCREW_TYPE1 = "FCREW_TYPE1";
            public const string FCSRT_TYPE1 = "FCSRT_TYPE1";
            public const string FCREP_TYPE1 = "FCREP_TYPE1";
            public const string FCREP_TYPE2 = "FCREP_TYPE2";
            public const string FCREP_TYPE3 = "FCREP_TYPE3";
            public const string FCMQC_TYPE1 = "FCMQC_TYPE1";
            public const string FCMQC_TYPE2 = "FCMQC_TYPE2";
            public const string FCMAC_TYPE1 = "FCMAC_TYPE1";
            public const string FCAOI_TYPE1 = "FCAOI_TYPE1";
        }
    }

    public class eJobDataLineType
    {
        public class ARRAY
        {
            public const string PROCESS_EQ = "TBPROCESS";
            /// <summary>
            /// 測試機台, Port至少三個, 會有Changer Mode
            /// </summary>
            public const string TEST_1 = "TBTEST_1";
            /// <summary>
            /// 測試機台, Port兩個以內, 不會有Changer Mode
            /// </summary>
            public const string TEST_2 = "TBTEST_2";

            public const string SORT = "TBSRT";
            public const string ABFG = "TBBFG";
        }
        public class CELL
        {
            public const string CBPIL = "CBPIL";
            //public const string CBODF = "CBODF";
            public const string CBHVA = "CBHVA";
            public const string CBCUT = "CBCUT";            
            public const string CBPOL = "CBPOL";
            public const string CBDPK = "CBDPK";
            public const string CBPPK = "CBPPK";
            public const string CBPMT = "CBPMT";
            public const string CBGAP = "CBGAP";
            public const string CBPIS = "CBPIS";
            public const string CBPRM = "CBPRM";
            public const string CBGMO = "CBGMO";
            public const string CBLOI = "CBLOI";
            public const string CBNRP = "CBNRP";
            public const string CBOLS = "CBOLS";
            public const string CBSOR = "CBSOR";
            public const string CBDPS = "CBDPS";
            public const string CBATS = "CBATS";
            public const string CBDPI = "CBDPI";
            public const string CBUVA = "CBUVA";
            public const string CBMCL = "CBMCL";
            public const string CCPIL = "CCPIL";
            public const string CCODF = "CCODF";
            public const string CCPCS = "CCPCS";
            public const string CCCUT = "CCCUT";
            public const string CCPOL = "CCPOL";
            public const string CCRWK = "CCRWK";
            public const string CCQUP = "CCQUP";
            public const string CCPCK = "CCPCK";
            public const string CCQPP = "CCQPP";
            public const string CCPPK = "CCPPK";
            public const string CCPDR = "CCPDR";
            public const string CCTAM = "CCTAM";
            public const string CCPTH = "CCPTH";
            public const string CCGAP = "CCGAP";
            public const string CCRWT = "CCRWT";
            public const string CCSOR = "CCSOR";
            public const string CCCHN = "CCCHN";
            public const string CCCRP = "CCCRP";
            public const string CCCLN = "CCCLN";
            public const string CCQSR = "CCQSR";
            //add by huangjiayin for t3 notch
            public const string CCNLS = "CCNLS";
            public const string CCNRD = "CCNRD";
        }
        public class CF
        {
            public const string PHOTO_BMPS = "FCPHO_BMPS";// FCOPH 也是使用BMPS
            public const string PHOTO_GRB = "FCPHO_GRB";
            public const string REWORK = "FCREW";
            public const string UNPACK = "FCUPK";
            public const string MASK = "FCMSK";
            public const string REPAIR = "FCREP";
            public const string MQC_1 = "FCMQC_1";
            public const string MQC_2 = "FCMQC_2";
            public const string FCMAC = "FCMAC";
            public const string FCSRT = "FCSRT";
            public const string FCPSH = "FCPSH";
            public const string FCAOI = "FCAOI";
        }
        public class MODULE
        {
            public const string MDABL = "MDABL";
            public const string MDOCR = "MDOCR";
            public const string MDBLL = "MDBLL";
            public const string MDRWR = "MDRWR";
            public const string MDPAK = "MDPAK";
        }
    }

    public class eMES_PRODUCT_TYPE
    {
        public const string NORMAL_TFT_PRODUCT = "TFT";
        public const string NORMAL_CF_PRODUCT = "CF";
        public const string GENERAL_DUMMY = "DM";
        public const string QC_DUMMY = "QC";
        public const string THROUGH_DUMMY = "TR";
        public const string ITO_DUMMY = "ITO";
        public const string MQC_DUMMY = "MQC";
        public const string THICKNESS_DUMMY = "TK";
        public const string BARE_DUMMY = "BD";
        public const string UV_MASK = "UV";
        public const string METAL1_DUMMY = "METAL1";//sy add by MES 1.21 20161119
        public const string NIP_DUMMY = "NIP";//sy add by MES 1.21 20161119
    }

    public class eMES_TEMPERATURE_FLAG
    {
        public const string HT = "HT";
        public const string LT = "LT";
    }

    public class eMES_PRODUCT_PROCESS_TYPE
    {
        public const string IGZO = "IGZO";
        public const string MMG = "MMG";
        public const string NORMAL = "NORMAL";
    }

    public class eMES_OWNER_TYPE
    {
        public const string PRODUCT = "OWNERP";
        public const string ENGINEER = "OWNERE";
        public const string DUMMY = "OWNERM";
    }

    public class eQtimeEventType
    {
        public const string Unknown = "NONE";
        public const string FetchOutEvent = "FETCH";//DB SBRM_QTIME_DEF
        public const string SendOutEvent = "SEND";//DB SBRM_QTIME_DEF
        public const string ReceiveEvent = "RECEIVE"; //DB SBRM_QTIME_DEF
        public const string StoreEvent = "STORE";//DB SBRM_QTIME_DEF
        public const string ProcCompEvent = "PROCCOMP"; //DB SBRM_QTIME_DEF CELL Special
        public const string ProcStartEvent = "PROCSTART";  //DB SBRM_QTIME_DEF CELL Special
    }
        
    public class eRUNMODE
    {
        public class ILC
        {
            public const string ILC_1 = "ILC";
            public const string FLC_2 = "FLC";
        }

        public class FLC
        {
            public const string NORMAL_1 = "NORMAL";
            public const string CVD_2 = "CVD";
        }
    }
     
    public class eMES_LINEOPERMODE
    {
        public const string NORMAL = "NORMAL"; // modify by bruce 2014/12/08 report mes big word
        //public const string MIX = "MIXEDRUNMODE";//modify by asir 2014/12/16 MIXRUNMODE->MIXEDRUNMODE
        public const string MIX = "MIX"; //Modified by dade 2015/12/13
        public const string MQC = "MQC";
        public const string CHANGER = "CHANGER";
        public const string SORTER = "SORTER";
        public const string EXCHANGE = "EXCHANGE"; //add for changer line exchange mode, cc.kuang 2016/02/22

        public const string ARRAY_2S = "2S";
        public const string ARRAY_2D = "2D";
        public const string ARRAY_4P1 = "4P1";
        public const string ARRAY_4P2 = "4P2";
        public const string ARRAY_2O = "2O";
        public const string ARRAY_2Q = "2Q";
        public const string ARRAY_4Q = "4Q";
        public const string ARRAY_ENG = "ENG";
        public const string ARRAY_IGZO = "IGZO";

        public const string CF_TFTDENSE = "TFTDENSE";
        public const string CF_CFDENSE = "CFDENSE";
        public const string CF_CLEANER = "CLEANER";
        public const string CF_INK = "INK";
        public const string CF_BMACRO = "BMARCO";
        public const string CF_FIMACRO = "FIMACRO";
        public const string CF_THROUGH = "THROUGH";
        public const string CF_THROUGH_MQC = "Through/MQC";

        public const string CELL_BYPASS = "BYPASS";
        public const string CELL_REPAIR = "REPAIR";
        public const string CELL_MSORT = "MSORT";
        public const string CELL_CSTMIXTOTRAY = "CTT1";
        public const string CELL_CSTTOTRAYMIX = "CTT2";
        public const string CELL_CSTTOTRAYALL = "CTT3";
        public const string CELL_TRAYMIXTOCST = "TTC1";
        public const string CELL_TRAYTOCSTALL = "TTC2";
        public const string CELL_SCRAP = "SCRAP";
        public const string CELL_CHANGER = "CHANGER";
        public const string CELL_RANDOMMODECHN = "RANDOM1";  //add RANDOM MODE BY zhuxingxing 20160923 //20170112 sy modify  
        public const string CELL_RANDOMMODESOR = "RANDOM2";  //20170112 sy modify
        public const string CELL_VCRMODECHN = "VCR1";  //20170112 sy modify
        public const string CELL_VCRMODESOR= "VCR2";  //20170112 sy modify
        public const string CELL_GRADE = "GRADE";//20170112 sy modify
        public const string CELL_FLAG = "FLAG";//20170112 sy modify
        public const string CELL_FLAGGRADE = "FLAGGRADE";//201703024 luojun modify for flaggrade mode
    }
     
    public class eEQPFLAG
    {
        public class Array
        {
            public const string RecipeGroupEndFlag = "RecipeGroupEndFlag";
            public const string Thickness = "Thickness";
            public const string OvenBakeFlag = "OvenBakeFlag";
            public const string AOI_Forced_Sampling = "AOIForcedSampling";
            public const string DNS_TurnTable_01 = "TurnTable_01";
            public const string DNS_TurnTable_02_1st = "TurnTable_02_1st";
            public const string DNS_TurnTable_02_2nd = "TurnTable_02_2nd";
            public const string DNS_TurnTable_03 = "TurnTable_03";
            public const string MAC_TurnModeFlag = "MarcoTurnModeFlag"; //modify for match DB 2016/01/04 cc.kuang
            public const string MAC_TurnTableFlag = "TurnTable#01(Marco)"; //modify for match DB 2016/01/04 cc.kuang
            public const string ToTotalPitch = "ToTotalPitch";
            public const string ToTotalPitchSubChamber = "ToTotalPitchSubChamber";
            public const string ScrapFlag = "ScrapFlag";
            public const string SmashFlag = "SmashFlag";
            public const string CutFlag = "CutFlag";
            public const string BackupProcessFlag = "BackupProcessFlag";    //add by bruce 2015/7/27 ELA cross line use
            public const string MQCInspectionFlag = "MQCInspectionFlag";    //add by bruce 2015/7/27 ELA cross line use
        }
        public class CF
        {


        }

        public class CELL
        {

        }
    }

    public class eTrackingData
    {

    }

    public class CassetteMapException : Exception
    {
        private string _eqpNo = string.Empty;
        private string _portNo = string.Empty;

        public string EQPNo
        {
            get { return _eqpNo; }
            set { _eqpNo = value; }
        }

        public string PortNo
        {
            get { return _portNo; }
            set { _portNo = value; }
        }
        public CassetteMapException(string eqpNo, string portNo, string aMessage)
            : base(aMessage)
        {
            _eqpNo = eqpNo;
            _portNo = portNo;
        }
    }
    public class ERR_CST_MAP
    {
        public const string INVALID_LINE_DATA = "INVALID LINE DATA";
        public const string INVALID_EQUIPMENT_DATA = "INVALID EQUIPMENT DATA";
        public const string VALIDATION_NG_FROM_MES = "VALIDATION NG FROM MES";
        public const string INVALID_PORT_DATA = "INVALID PORT DATA";
        public const string INVALID_PORT_STATUS = "INVALID PORT STATUS";
        public const string DIFFERENT_CST_SEQNO = "DIFFERENT_CST_SEQNO";
        public const string INVALID_CSTID = "INVALID CST ID";
        public const string UNEXPECTED_MES_MESSAGE = "UNEXPECTED MES MESSAGE";
        public const string SLOTMAP_MISMATCH = "SLOT MAP MISMATCH";
        public const string CIM_MODE_OFF = "CIM MODE OFF";
        public const string INVALID_GLASS_SIZE = "INVALID GLASS SIZE";
        public const string INVALID_BAKING_PARA = "INVALID BAKING PARAMETER";
        public const string INVALID_RUBBING_PARA = "INVALID RUBBING PARAMETER";
        public const string INVALID_CSTSETTING_CODE = "INVALID CSTSETTING CODE";
        public const string INVALID_PRODUCT_ID = "INVALID PRODUCT ID";
        public const string PPID_NODEFINE = "PPID NOT DEFINE";
        public const string INVALID_PPID = "INVALID PPID";
        public const string RECIPE_MISMATCH = "RECIPE MISMATCH";
        public const string HAVE_DIFFERENT_PRODUCT = "HAVE DIFFERENT PRODUCT";
        public const string CST_MAP_TRANSFER_ERROR = "CST MAP TRANSFER ERROR";
        public const string GLASS_DATA_TRANSFER_ERROR = "GLASS DATA TRANSFERERROR";
        public const string INVALID_LINEOPERATIONMODE = "INVALID LINE OPERATION MODE";
        public const string INVALID_JOB_TYPE = "INVALID JOB TYPE";
        public const string INVALID_DUMMY_TYPE = "INVALID DUMMY TYPE";
        public const string INVALID_OXF_INFO = "INVALID OXF INFO";
        public const string INVALID_DUMMY_PARA = "INVALID DUMMY PARAMETER";
        public const string INVALID_CFCHIP_JUDGE = "INVALID CF-CHIP JUDGE";
        public const string INVALID_DENSEBOX_INFO = "INVALID DENSE BOX INFO";
        public const string EAP_JOB_REJECT = "EAP_JOB_REJECT";
        public const string INVALID_PANEL_SEQUENCE = "INVALID PANEL SEQUENCE";
        public const string INVALID_JOB_ID = "INVALID JOB ID";
        public const string INVALID_INLINE_REWORK_PARA = "INVALID INLINE REWORK PARA";
        public const string INVALID_ITO_FLAG = "INVALID ITO FLAG";
        public const string INVALID_CRITERIAL_VALUE = "INVALID CRITERIAL VALUE";
        public const string HAVE_DIFFERENT_BATCH = "HAVE DIFFERENT BATCH";
        public const string INVALID_GROUPID = "INVALID GROUP ID";
        public const string INVALID_PRODUCT_TYPE = "INVALID PRODUCT TYPE";
        public const string INVALID_SELECTED_POSITION_MAP = "INVALID SELECTED POSITION MAP";
        public const string OVER_MAX_JOB_COUNT = "OVER MAX JOB COUNT";
        public const string DIFFERENT_PLANNED_PRODUCTSPECNAME_WITH_JOB = "DIFFERENT PLANNED PRODUCTSPECNAME WITH JOB";//add by fred liu at 2012.5.29
        public const string DIFFERENT_PLANNED_PRODUCTSPECNAME_WITH_CST = "DIFFERENT PLANNED PRODUCTSPECNAME WITH CST";//add by fred liu at 2012.5.29
        public const string DIFFERENT_SOURCEPART_WITH_JOB = "DIFFERENT SOURCEPART WITH JOB";//add by songhuan at 2012.06.22
        public const string DIFFERENTSOURCEPART_WITH_CST = "DIFFERENTSOURCEPART WITH CST";//add by songhuan at 2012.06.22

        public const string SAMPLINGFLAG_COUNT_0 = "SAMPLINGFLAG COUNT IS 0";//ADD BY cc.kuang 2016/05/20
        public const string INVALID_SAMPLINGFLAG = "INVALID SAMPLINGFLAG WITH ARRAY OVEN BOTH PORT TYPE";//ADD BY GladFu 20120807
        public const string INVALID_LINE_RUNMODE = "CVD BC RUN MODE DIFFERENT WITH MES";//ADD BY GLADFU 20120821

        public const string INVALID_EQP_RUNMODE = "TAMAC EQP RUN MODE IS REVERSE,can not permit 2 Lots RUN at the same time.";//ADD BY GladFu 20130322
        public const string INVALID_LINE_CONTROLMODE = "INVALID_LINE_CONTROLMODE.";//ADD BY GladFu 20130322
        public const string DIFFERENT_EQP_RUNMODE = "DIFFERENT_EQP_RUNMODE";
        public const string RECIPEID_VALIDATION_NG = "RECIPEID VALIDATION NG";
        public const string RECIPE_PARAMATER_VALIDATION_NG = "RECIPE PARAMETER VALIDATION NG";

        public const string MIX_RUNMODE_DIFFERENT_LOTDATA = "MIX RUN MODE DIFFERENT LOT DATA";
        public const string MIX_RUNMODE_DIFFERENT_RECIPE = "MIX RUN MODE DIFFERENT RECIPE";
        public const string ARRAY_FORCECLEANOUT = "ARRAY FORCE CLEAN OUT MODE";
        public const string ARRAY_CVD_DATA_CHECK_ERROR = "ARRAY CVD DATA CHECK ERROR";
        public const string ARRAY_DRY_DATA_CHECK_ERROR = "ARRAY DRY DATA CHECK ERROR";
        public const string ARRAY_PVD_DATA_CHECK_ERROR = "ARRAY PVD DATA CHECK ERROR";

        public const string CELL_POL_LD_CHECK_ERROR = "CELL POL LD CHECK ERROR";
        public const string CELL_POL_ULDPARTIALCST_CHECK_ERROR = "CELL POL ULD PARTIAL CST CHECK ERROR";
        public const string CELL_PRM_LD_CHECK_ERROR = "CELL PRM LD CHECK ERROR";
        public const string CELL_PMT_PORTMODE_CHECK_ERROR = "CELL PMT PORT MODE CHECK ERROR";
        public const string CELL_MES_DOWNLOAD_CSTSETTINGCODE_NULL = "CELL MES DOWNLOAD CST SETTING CODE NULL";
        public const string CELL_MES_DOWNLOAD_PRODUCTTYPEID_NULL = "CELL MES DOWNLOAD PRODUCTTYPEID NULL";
        public const string BOX_DATA_TRANSER_ERROR_ABNORMAL_EXCEPTION_ERROR = "BOX DATA TRANSER ERROR : ABNORMAL EXCEPTION ERROR";
        public const string CELL_MES_DOWNLOAD_SUBPRODUCTSPECS_ERROR = "CELL MES DOWNLOAD SUBPRODUCTSPECS ERROR";
        public const string CELL_SOR_CHN_RANDOMMODE_CHECK_ERROR = "CELL SOR CHN RANDOMMODE CHECK ERROR"; //20161019 add by zhuxingxing

        public const string CF_REWORK_WASHABLECOUNT_CHECK_NG = "CF REWORK WASHABLECOUNT CHECK NG";
        public const string CF_PORT_TYPE_MISSMATCH = "CF PORT TYPE MISSMATCH";
        public const string ROBOT_ROUTE_CREATER_FAILED = "ROBOT ROUTE CREATER FAILED";
        public const string PORY_ASSIGNMENT_UNKNOW = "PORY ASSIGNMENT UNKNOW"; //20160606 add by Frank

    }

    public class eCVD_RUN_MODE
    {
        //2S=2200_Single , 2D=2201_Double , 4P1=4200_PV1 , 4P2=4201_PV2 , IGZO 2O=SiOX , 2Q=2200_MQC , 4Q=4200_MQC
        public const string UNKNOWN = "";
        public const string HT_2200_SINGLE = "2S";
        public const string HT_2201_DOUBLE = "2D";
        public const string LT_4200_PV1 = "4P1";
        public const string LT_4201_PV2 = "4P2";
        public const string IGZO_SiOX = "2O";
        public const string MQC_2200_MQC = "2Q";
        public const string MQC_4200_MQC = "4Q";
    }

    public class eDRY_RUN_MODE
    {
        public const string UNKNOWN = "";
        public const string NORMAL = "NORMAL";
        public const string IGZO = "IGZO";
        public const string MQC = "MQC";
        public const string ENG = "ENG";
    }

    public class eLINE_STATUS
    {
        public const string DOWN = "DOWN";

        public const string EQALIVEDOWN = "EQALIVEDOWN";    //add by bruce 20160331 CSOT要求新增設定 Monitor Eq Alive Down 狀態

        public const string RUN = "RUN";

        public const string IDLE = "IDLE";

        public eLINE_STATUS()
        {
        }
    }

    public class eLOG_CONSTANT
    {
        public const string CAN_NOT_FIND_LINE = "CAN NOT FIND LINE_ID=[{0}] IN LINE OBJECT!";
        public const string CAN_NOT_FIND_LINE2 = "[{0}] CAN NOT FIND LINE_ID=[{1}] IN LINE OBJECT!";
        public const string CAN_NOT_FIND_EQP = "CAN NOT FIND EQUIPMENT_NO=[{0}] IN EQUIPMENT OBJECT!";
        public const string CAN_NOT_FIND_EQP2 = "[{0}] CAN NOT FIND EQUIPMENT_NO=[{1}] IN EQUIPMENT OBJECT!";
        public const string CAN_NOT_FIND_PORT = "CAN NOT FIND PORT=[{0}] IN PORT OBJECT!";
        public const string CAN_NOT_FIND_UNIT = "CAN NOT FIND UNIT=[{0}] IN UNIT OBJECT!";

        public const string EQP_REPORT_BIT_OFF = "[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].";
        //modify 20170207 qiumin
        public const string JOB_REMOVE_REPORT_MISMATCH = "JOB REMOVE REPORT EQP[{0}] IS NOT JOB CURRENT EQ[{1}] ";
        
        public const string MES_OFFLINE_SKIP = "[LINENAME={0}] [BCS -> MES][{1}] MES OFF-LINE, SKIP \"{2}\" REPORT.";
        public const string BCS_REPORT_MES = "[LINENAME={0}] [BCS -> MES][{1}] REPORT TO MES TRANSACTION=[{2}]";
    }

    public class eREPORT_SWITCH
    {
        public const string REMOTE_RECIPE_ID = "S_REMOTE_RECIPE_ID_CHECK";
        public const string LOCAL_RECIPE_ID = "S_LOCAL_RECIPE_ID_CHECK";
        public const string OFFLINE_RECIPE_ID = "S_OFFLINE_RECIPE_ID_CHECK";

        public const string REMOTE_RECIPE_PARAMETER = "S_REMOTE_RECIPE_PARAMETER_CHECK";
        public const string LOCAL_RECIPE_PARAMETER = "S_LOCAL_RECIPE_PARAMETER_CHECK";
        public const string OFFLINE_RECIPE_PARAMETER = "S_OFFLINE_RECIPE_PARAMETER_CHECK";

        public const string ONLINE_PORT_MODE = "S_ONLINE_PORT_MODE_CHECK";
        public const string OFFLINE_PORT_MODE = "S_OFFLINE_PORT_MODE_CHECK";

        public const string ONLINE_SPECIAL_RULE = "S_ONLINE_SPECIAL_RULE_CHECK";
        public const string OFFLINE_SPECIAL_RULE = "S_OFFLINE_SPECIAL_RULE_CHECK";

        public const string MANUAL_ALARM_MES = "S_MANUAL_ALARM_MES";
        public const string DATA_DOWNLOAD_REPLY_ERROR_AUTO_CANCEL = "S_DATA_DOWNLOAD_REPLY_ERROR_AUTO_CANCEL";

        public const string EQUIPMENT_STATUS_STOP_COMMAND = "S_EQUIPMENT_STATUS_STOP_COMMAND";

        public const string PORT_FUNCTION_CHECK_NO_CST = "S_PORT_FUNCTION_CHECK_NO_CST";

        public const string RECORD_OPI_STATUS_LOG = "S_RECORD_OPI_STATUS_LOG";
    }

    public class eCELL_SWITCH
    {
        public const string OFFLINE_REPLY_EQP ="OFFLINEREPLYEQP";	//Watson Add 20150318
        public const string CUT_OXINFO_COUNT_MISMATCH_CANCEL_CST = "CUT_OXINFO_COUNT_MISMATCH_CANCEL_CST"; //Watson Add 20150318
        public const string PANEL_RECIPE_ID_CHECK = "PANEL_RECIPE_ID_CHECK"; //sy add 20160906
        public const string PANEL_RECIPE_PARA_CHECK = "PANEL_RECIPE_PARA_CHECK"; //sy add 20160906
    }

    #region CELL Special
    public class keyCELLMachingName
    {
        #region [T2]
        public const string CBPPO = "CBPPO";
        public const string CBPMO = "CBPMO";
        public const string CBSUV = "CBSUV";
        public const string CBVAB = "CBVAB";
        public const string CBBOO = "CBBOO";
        public const string CBSDP = "CBSDP";
        public const string CBLCD = "CBLCD";
        public const string CBCUT = "CBCUT";
        public const string CBBUR = "CBBUR";
        public const string CBTCV = "CBTCV";
        public const string CBOCV = "CBBHV";
        public const string CBPLD = "CBPLD";
        public const string CBPMI = "CBPMI";
        public const string CBMAI = "CBMAI";
        public const string CBLCI = "CBLCI";
        public const string CBAOI = "CBAOI";
        public const string CBPIS = "CBPIS";
        public const string CBGMO = "CBGMO";
        public const string CBPIP = "CBPIP";
        #endregion
        #region [T3]
        public const string CCBPI = "CCBPI";
        public const string CCPIN = "CCPIN";
        public const string CCSLI = "CCSLI";
        public const string CCMAI = "CCMAI";
        public const string CCLCI = "CCLCI";
        public const string CCBUR = "CCBUR";
        public const string CCTST = "CCTST";
        public const string CCRWT = "CCRWT";
        public const string CCVAC = "CCVAC";
        public const string CCPPO = "CCPPO";
        public const string CCPMO = "CCPMO";
        public const string CCPIC = "CCPIC";
        public const string CCPPA = "CCPPA";
        public const string CCPAO = "CCPAO";
        public const string CCBOO = "CCBOO";
        public const string CCVPO = "CCVPO";
        public const string CCLCD = "CCLCD";
        public const string CCSUV = "CCSUV";

        public const string CCPCS = "CCPCS";
        public const string CCPCC = "CCPCC";
        public const string CCCUT = "CCCUT";
        public const string CCBEV = "CCBEV";
        public const string CCPIT3 = "CCPIT3";
        #endregion
    }
    public class keyCellLineType
    {
        public const string PIL = "PIL";
        public const string ODF = "ODF";
        public const string HVA = "HVA";
        public const string CUT = "CUT";
        public const string PCS = "PCS";
        public const string POL = "POL";
        public const string PPK = "PPK";
        public const string QPP = "QPP";
    }
    public class eCELLInLineOverQtimeODF
    {
        public bool UnKnow;      //"0000 0000 0000 0000"
        public bool BOO2VAC;  //"0000 0000 0000 0001"
        public bool VPO2VAC;   // "0000 0000 0000 0010"
        public bool LCD2VAC;  //"0000 0000 0000 0100"
        public bool VAC2SUV;
        public bool SUV2SMO;
    }
    public class eCELLInLineOverQtimePI
    {
        public bool UnKnow; //0000 0000 0000 0000"
        public bool PIP2PPO;//"0000 0000 0000 0001"
        public bool PPO2PMO; //"0000 0000 0000 0010"
        //T3 add cs.chou 
        public bool PIC2PPO;
        public bool PPO2PPA;
        public bool PMO2PPA;
        public bool PPA2PAO;
    }
    public class eCELLODFLineRecipeParaReply
    {
        public const string Unit_Recipe_Info ="Unit_Recipe_Info";
    }
    public class keyCELLPORTAtt
    {
        public const string NORMAL = "NORMAL";
        public const string DENSE = "DENSE"; 
        public const string BOX = "BOX";
        public const string VirtualPort = "VIRTUAL";
        public const string BOX_MANUAL = "BOX_MANUAL";//sy 20160105 add
        //public const string PPK_AUTO = "PPK_AUTO";//sy 20160105 mark
        //public const string PPK_MANUAL = "PPK_MANUAL";//sy 20160105 mark
        //public const string DENSECST = "DENSECST";//sy 20160105 mark
        public const string TRAY = "TRAY";
        public const string PALLET = "PALLET"; 
    }
    public class keyCELLDBRecipeCheckType
    {
        public const string NORMAL = "";
        public const string LOT = "LOT";
        public const string STB ="STB";
        public const string PROCESSLINE ="PROCESSLINE";
    }
    public class keyCELLPMTLINE
    {
        public const string CBPMI = "CBPMI";
        public const string CBPTI = "CBPTI";
    }
    //t3 cs.chou for VCRmismatch 20150923
    public class keyCELLGAPLINE
    {
        public const string CCGAP = "CCGAP";
        public const string CCGMI = "CCGMI";
    }
    public class keyCELLPTIParameter
    {
        public const string CELL_PTI_LINEID = "CELL_PTI_LINEID";
        public const string CELL_PTI_NODEID = "CELL_PTI_NODEID";
        public const string CELL_PTI_UNITID = "CELL_PTI_UNITID";
    }
    public class keyCELLROBOTProcessFlag
    {
        public const string NO_PROCESS = "NO_PROCESS";
        public const string WAIT_PROCESS = "WAIT_PROCESS";
        public const string ROBOT_PROCESS = "ROBOT_PROCESS";
        public const string ROBOT_PROCESSEND = "ROBOT_PROCESSEND";
    }
    #endregion

    #region [ For Robot Use ]
    public class eRobot_Check_ByPass
    {
        public const string ROBOT_CHECK_ROUTE_BYPASS = "ROBOT_CHECK_ROUTE_BYPASS";
    }
    //public class eLinekSignalDirect
    //{
    //    public const string UPSTREAM = "UPSTREAM";
    //    public const string DOWNSTREAM = "DOWNSTREAM";

    //    public eLinekSignalDirect()
    //    {

    //    }
    //}

    //public class eRobotStageType
    //{
    //    //DB Define
    //    //'PORT': for Cassette Port.
    //    //'STAGE': for Indexer inside Stage(such as VCR Table, Turn Table…)
    //    //'FIXBUFFER': for Indexer Fix Buffer.
    //    //'EQUIPMENT': for downstream or upstream Equipment.
    //    public const string PORT = "PORT";
    //    public const string STAGE = "STAGE";
    //    public const string FIXBUFFER = "FIXBUFFER";
    //    public const string EQUIPMENT = "EQUIPMENT";

    //    public eRobotStageType()
    //    {

    //    }
    //}

    #region [ For Robot Use 20150709 mark old Constants ]

    //public class eRobot_Command
    //{

    //    public const string GET_CMD = "GE";

    //    public const string PUT_CMD = "PU";

    //    public const string EXCHANGE = "EX";

    //    public const string PUT_READY = "PR";

    //    public const string GET_READY = "GR";

    //    public const string NO_COMMAND ="";// string.Empty;

    //    public eRobot_Command()
    //    {
    //    }

    //}

    //public class eRobot_ArmSelect
    //{

    //    public const string UPPER_ARM = " U";

    //    public const string LOWER_ARM = " L";

    //    /// <summary>
    //    /// All為雙Arm同時
    //    /// </summary>
    //    public const string ALL_ARM = " B";

    //    public const string NO_ARM_SELECT ="";// string.Empty;

    //    public eRobot_ArmSelect()
    //    {
    //    }

    //}

    //public class eRobot_RunMode
    //{

    //    public const string SEMI_MODE = "SEMI";

    //    public const string AUTO_MODE = "AUTO";

    //    public eRobot_RunMode()
    //    {
    //    }

    //}

    //public class eRobot_CmdStatus
    //{
    //    public const string EMPTY = "EMPTY";

    //    public const string CREATE = "CREATE";

    //    public const string EQRECEIVE = "EQRECEIVE";

    //    public const string COMPLETE= "COMPLETE";

    //    //20150601 modify CSOT :不要用Cancel 造成誤解 改成Clear
    //    public const string CANCEL= "CLEAR"; // "CANCEL";

    //    public eRobot_CmdStatus()
    //    {
    //    }
    //}

    //public class eRobotStageStatus
    //{
    //    //每次確認時的狀態
    //    public const string INIT_CHECK = "INITIAL";

    //    public const string SEND_OUT_READY = "UDRQ";

    //    public const string RECEIVE_READY = "LDRQ";

    //    public const string NO_REQUEST = "NOREQ";

    //    //20141218 add for Both Port
    //    public const string SEND_OUT_AND_RECEIVE_READY = "UDRQ_LDRQ";

    //    public eRobotStageStatus()
    //    {
    //    }
    //}

    //public class eDBFlagSetting
    //{
    //    public const string IS_ENABLE = "ENABLE";

    //    public const string IS_DISABLE = "DISABLE";

    //    public eDBFlagSetting()
    //    {
    //    }
    //}

    //public class eRobotJobStatus
    //{
    //    public const string INIT = "INIT";

    //    public const string WAIT_PROC = "WAIT";

    //    public const string INPROCESS = "PROCESS";

    //    public const string COMPLETE = "COMPLETE";

    //    public eRobotJobStatus()
    //    {
    //    }
    //}

    //public class ePortUDRQJOBReason
    // {

    //     /// <summary>
    //     /// CSTSeq>0 , JobSeq>0 ,Exist=2,In RobotJob WIP
    //     /// </summary>
    //     public const string REASON_OK = "0";

    //     /// <summary>
    //     /// CSTSeq>0 , JobSeq>0 ,Exist=2,
    //     /// </summary>
    //     public const string JOB_NOT_INWIP = "1";

    //     /// <summary>
    //     /// CSTSeq>=0 , JobSeq>=0 ,Exist=2,有料無帳
    //     /// </summary>
    //     public const string JOBNO_NOEXIST_JOB_EXIST = "2";

    //     /// <summary>
    //     /// CSTSeq>0 , JobSeq>0 ,Exist=1,有帳無料
    //     /// </summary>
    //     public const string JOBNO_EXIST_JOB_NOTEXIST = "3";

    //     /// <summary>
    //     /// //CSTSeq=0 , JobSeq=0 ,Exist=1
    //     /// </summary>
    //     public const string IS_EMPTY_SLOT = "4";

    //     public const string IS_EXCEPTION = "5";

    //     public const string OTHERS = "6";

    //    public ePortUDRQJOBReason()
    //    {
    //    }
    //}

    //public class eRobotCommandResult
    //{

    //    public const string RESULT_OK = "OK";
    //    public const string RESULT_NG = "NG";
    //    public const string RESULT_INIT = "";

    //    public eRobotCommandResult()
    //    {
    //    }
    //}



    //public class eRobotRouteRule
    //{

    //    public const string ONLY = "ONLY";
    //    public const string SELECT = "SELECT";
    //    public const string TRACKING = "TRACKING";
    //    public const string SEQUENCE = "SEQUENCE";
    //    public const string ULDDISPATCH = "ULDDISPATCH";


    //    public eRobotRouteRule()
    //    {
    //    }
    //}

    //public class eRobotStageCSTType
    //{
    //    //WIRE ,CELL ,RANDOM
    //    public const string WIRE_CST = "WIRE";
    //    public const string CELL_CST = "CELL";
    //    public const string RANDOM_CST = "RANDOM";

    //    public eRobotStageCSTType()
    //    {
    //    }
    //}

    //public class eRobotStageInOutType
    //{
    //    //PORT，PUT，GET，PUT&GET，EXCHANGE
    //    public const string IS_PORT = "PORT";
    //    public const string ONLY_PUT = "PUT";
    //    public const string ONLY_GET = "GET";
    //    public const string PUT_AND_GET = "PUT&GET";
    //    public const string IS_EXCHANGE = "EXCHANGE";

    //    public eRobotStageInOutType()
    //    {
    //    }
    //}

    ////20141218 add for Loader Port SendOut Priority 存放在RobotJobWIP中
    //public class eLoaderPortSendOutStatus
    //{

    //    public const string PORT_IN_PROCESS = "1";
    //    public const string PORT_WAIT_PROCESS = "2";
    //    public const string NOT_IN_PORT = "3";

    //    public eLoaderPortSendOutStatus()
    //    {
    //    }
    //}

    ////20141218 add for Unload Port Recive Priority 存放在Stage中
    //public class eUnloadPortReceiveStatus
    //{

    //    public const string BOTH_PORT_IN_PROCESS = "1";
    //    public const string BOTH_PORT_WAIT_PROCESS = "2";
    //    public const string ULD_PORT_IN_PROCESS = "3";
    //    public const string ULD_PORT_WAIT_PROCESS = "4";
    //    public const string OTHERS = "5";

    //    public eUnloadPortReceiveStatus()
    //    {
    //    }
    //}

    ////20141219 ass for Job Judge Use
    //public class eCELL_PMT_JobJudge
    //{

    //    public const string INSPSKIP_NOHUDGE = "0";
    //    public const string OK = "1";
    //    public const string NG = "2";
    //    public const string REQUIRED_REWORK = "3";
    //    public const string REJECT_SCRAP = "4";
    //    public const string REQUIRED_REPAIR = "5";
    //    public const string RE_JUDGE = "6";
    //    public const string PI_REWORK = "7";
    //    public const string INK_REPAIR = "8";

    //    public eCELL_PMT_JobJudge()
    //    {
    //    }
    //}

    ////20141222 add for SubRunMode
    //public class eCELL_SOR_SubRunMode
    //{
    //    //Common
    //    public const string TO_UNLOADER = "1";
    //    public const string TO_VCR_AND_GET_VCR = "3";
    //    public const string GET_FROM_LOADER = "4";
    //    //for CBSOR_2
    //    public const string TO_DPF = "2";
    //    public const string GET_FROM_DPF = "5";

    //    public eCELL_SOR_SubRunMode()
    //    {
    //    }
    //}

    #endregion

    #endregion

    public class eJOBDATA
    {
        public const string CassetteSequenceNo = "CassetteSequenceNo";
        public const string JobSequenceNo = "JobSequenceNo";
        public const string GroupIndex = "GroupIndex";
        public const string ProductType = "ProductType";
        public const string CSTOperationMode = "CSTOperationMode";
        public const string SubstrateType = "SubstrateType";
        public const string CIMMode = "CIMMode";
        public const string JobType = "JobType";
        public const string JobJudge = "JobJudge";
        public const string SamplingSlotFlag = "SamplingSlotFlag";
        public const string OXRInformationRequestFlag = "OXRInformationRequestFlag";
        public const string FirstRunFlag = "FirstRunFlag";
        public const string JobGrade = "JobGrade";
        public const string Glass_Chip_Mask_BlockID = "Glass/Chip/MaskID/BlockID"; //modify by bruce 2015/7/2 Item for T3
        public const string PPID = "PPID";
        public const string INSPReservations = "INSPReservations";
        public const string EQPReservations = "EQPReservations";
        public const string GlassFlowType = "GlassFlowType";
        public const string ProcessType = "ProcessType";
        public const string LastGlassFlag = "LastGlassFlag";
        public const string RTCFlag = "RTCFlag";
        public const string EQPRTCFlag = "EQPRTCFlag";   // yang for EQPRTC Use
        public const string MainEQInFlag = "MainEQInFlag";
        public const string CoaterCSPNo = "CoaterCSPNo";
        public const string OCFlag = "OCFlag";  //T3 CF Photo Line Use
        public const string LoaderBufferingFlag = "LoaderBufferingFlag";
        public const string InspJudgedData = "Insp.JudgedData";
        public const string InspJudgedData1 = "Insp.JudgedData1";   //T3 CF Photo Line Use
        public const string InspJudgedData2 = "Insp.JudgedData2";   //T3 CF Photo Line Use
        public const string SamplingValue = "SamplingValue";
        public const string ReworkMaxCount = "ReworkMaxCount";
        public const string ReworkRealCount = "ReworkRealCount";
        public const string TrackingData = "TrackingData";
        public const string CFReserved = "CFReserved";
        public const string CFSpecialReserved = "CFSpecialReserved";
        public const string SorterGrade = "SorterGrade";
        public const string EQPFlag = "EQPFlag";
        public const string EQPFlag1 = "EQPFlag1";  //T3 CF Photo Line Use
        public const string EQPFlag2 = "EQPFlag2";  //T3 CF Photo Line Use
        public const string OXRInformation = "OXRInformation";
        public const string ChipCount = "ChipCount";
        public const string OvenHPSlotNumber = "OvenHPSlotNumber";
        public const string COAVersion = "COAVersion";
        public const string RecipeGroupNumber = "RecipeGroupNumber";
        public const string SourcePortNo = "SourcePortNo";
        public const string TargetPortNo = "TargetPortNo";
        public const string TargetSlotNo = "TargetSlotNo";  //T3 CF Photo Line LotToLot Use
        public const string TargetCSTID = "TargetCSTID";
        public const string ArrayPhotoPre_InlineID = "ArrayPhotoPre-InlineID";
        public const string InlineReworkMaxCount = "InlineReworkMaxCount";  //T3 CF Photo Line Use
        public const string InlineReworkRealCount = "InlineReworkRealCount";    //T3 CF Photo Line Use
        public const string MarcoReserveFlag = "MarcoReserveFlag";  //T3 CF Photo Line Use
        public const string ProcessBackUp = "ProcessBackUp";    //T3 CF Photo Line Use
        public const string DummyUsedCount = "DummyUsedCount";
        public const string NetworkNo = "NetworkNo";
        public const string ProductID = "ProductID";
        public const string CassetteSettingCode = "CassetteSettingCode";
        public const string AbnormalCode = "AbnormalCode";
        public const string PanelSize = "PanelSize";
        public const string NodeStack = "NodeStack";
        public const string TurnAngle = "TurnAngle";
        public const string CrossLineCassetteSettingCode = "CrossLineCassetteSettingCode";
        public const string PanelSizeFlag = "PanelSizeFlag";
        public const string MMGFlag = "MMGFlag";
        public const string CrossLinePanelSize = "CrossLinePanelSize";
        public const string CUTProductID = "CUTProductID";
        public const string CUTCrossProductID = "CUTCrossProductID";
        public const string CUTProductType = "CUTProductType";
        public const string CUTCrossProductType = "CUTCrossProductType";
        public const string POLProductType = "POLProductType";
        public const string POLProductID = "POLProductID";
        public const string CrossLinePPID = "CrossLinePPID";
        public const string ControlMode = "ControlMode";
        public const string OwnerID = "OwnerID";
        public const string ReturnModeTurnAngle = "ReturnModeTurnAngle";
        public const string RepairResult = "RepairResult";
        public const string RunMode = "RunMode";
        public const string MaskID = "MaskID";
        public const string CFCasetteSeqNo = "CFCasetteSeqNo";
        public const string CFJobSeqno = "CFJobSeqno";
        public const string ODFBoxChamberOpenTime_01 = "ODFBoxChamberOpenTime#01";
        public const string ODFBoxChamberOpenTime_02 = "ODFBoxChamberOpenTime#02";
        public const string ODFBoxChamberOpenTime_03 = "ODFBoxChamberOpenTime#03";
        public const string UVMaskAlreadyUseCount = "UVMaskAlreadyUseCount";
        public const string PPOSlotNo = "PPOSlotNo";
        public const string ReworkCount = "ReworkCount";
        public const string TargetSequenceNo = "TargetSequenceNo"; //t3 Changer Function use cc.kuang 2015/07/02
        public const string TargetLoadLockNo = "TargetLoadLockNo"; //t3 array PVD use cc.kuang 2015/07/02
        public const string BlockOXInformation = "BlockOXInformation";//T3 shihyang 20150911 PIL
        public const string GlassThickness = "GlassThickness";//T3 shihyang 20150911 PIL
        public const string OperationID = "OperationID";//T3 shihyang 20150911 PIL
        public const string ProductOwner = "ProductOwner";//T3 shihyang 20150911 PIL
        public const string PILiquidType = "PILiquidType";//T3 shihyang 20150911 PIL 
        public const string AssembleSeqNo = "AssembleSeqNo";//T3 shihyang 20150911 ODF 
        public const string BlockSize = "BlockSize";//T3 shihyang 20150911 PCS 
        public const string PCSProductID = "PCSProductID";//T3 shihyang 20150911 PCS //by huangjiayin 20170714: no use anymore
        public const string PCSProductID2 = "PCSProductID2";//T3 shihyang 20151229 PCS  //by huangjiayin 20170714: no use anymore
        public const string PCSProductType = "PCSProductType";//T3 shihyang 20150911 PCS  //by huangjiayin 20170714: no use anymore
        public const string PCSProductType2 = "PCSProductType2";//T3 shihyang 20151229 PCS  //by huangjiayin 20170714: no use anymore
        public const string PCSCassetteSettingCodeList = "PCSCassetteSettingCodeList";//T3 huangjiayin 20170714 PCS
        public const string BlockSize1 = "BlockSize1";//T3 huangjiayin 20170724 PCS
        public const string BlockSize2 = "BlockSize2";//T3 huangjiayin 20170724 PCS
        public const string PCSBlockSizeList = "PCSBlockSizeList";//T3 huangjiayin 20170724 PCS
        public const string PCSCassetteSettingCode = "PCSCassetteSettingCode";//T3 shihyang 20160328 PCS 
        public const string PCSCassetteSettingCode2 = "PCSCassetteSettingCode2";//T3 shihyang 20160328 PCS 
        public const string PanelOXInformation = "PanelOXInformation";//T3 shihyang 20150911 CUT 
        public const string DefectCode = "DefectCode";//T3 shihyang 20150911 CUT 
        public const string MainDefectCode = "MainDefectCode";//T3 shihyang 20151029 POL 
        public const string RejudgeCount = "RejudgeCount";//T3 shihyang 20150911 CUT 
        public const string PanelGroup = "PanelGroup";//T3 shihyang 20150911 PCK 
        public const string MaxRwkCount = "MaxRwkCount";//T3 shihyang 20150911 PDR 
        public const string CurrentRwkCount = "CurrentRwkCount";//T3 shihyang 20150911 PDR 
        public const string DotRepairCount = "DotRepairCount";//T3 shihyang 20150911 RWT 
        public const string LineRepairCount = "LineRepairCount";//T3 shihyang 20150911 RWT
        public const string VendorName = "VendorName";//T3 shihyang 20150911 RWT 
        public const string BURCheckCount = "BURCheckCount";//T3 sy 20151229 CUT 
        public const string CUTCassetteSettingCode = "CUTCassetteSettingCode";//T3 sy 20151229 CUT
        public const string OQCBank = "OQCBank";//T3 sy 20160201 PCK 
        public const string SortFlagNo = "SortFlagNo";//T3 sy 20160703 SOR 
    }

    //add by bruce 2015/7/2 for get event item use
    public class ePLC
    {
        public const string BitResult_OFF = "0";
        public const string BitResult_ON ="1";
        public const string JobCount_TFT = "TotalTFTProductJobCount";
        public const string JobCount_UnassembledTFT = "TotalUnassembledTFTProductJobCount";
        public const string JobCount_CF = "TotalCFProductJobCount";
        public const string JobCount_DUMMY="TotalDummyJobCount";
        public const string JobCount_NIPDUMMY = "TotalNIPDummyJobCount";
        public const string JobCount_ITODUMMY = "TotalITODummyJobCount";
        public const string JobCount_MatelOneDUMMY = "TotalMetalOneDummyJobCount";
        public const string JobCount_ThroughDummy="TotalThroughDummyJobCount";
        public const string JobCount_ThicknessDummy="TotalThicknessDummyJobCount";
        public const string JobCount_UVMask="TotalUVMASKJobCount";
        public const string JobCount_Unit01_TFT= "Unit#01TFTProductJobCount"; 
        public const string JobCount_Unit01_CF="Unit#01CFProductJobCount";
        public const string JobCount_Unit01_MetalOneDummy = "Unit#01MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit01_ITODummy = "Unit#01ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit01_NIPDummy = "Unit#01NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit01_UnassembledTFT = "Unit#01UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit01_DUMMY = "Unit#01DummyJobCount";
        public const string JobCount_Unit01_ThroughDummy = "Unit#01ThroughDummyJobCount";
        public const string JobCount_Unit01_ThicknessDummy = "Unit#01ThicknessDummyJobCount";
        public const string JobCount_Unit01_UVMASK = "Unit#01UVMASKJobCount";
        public const string JobCount_Unit02_TFT="Unit#02TFTProductJobCount";
        public const string JobCount_Unit02_CF="Unit#02CFProductJobCount";
        public const string JobCount_Unit02_MetalOneDummy = "Unit#02MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit02_ITODummy = "Unit#02ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit02_NIPDummy = "Unit#02NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit02_UnassembledTFT = "Unit#02UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit02_DUMMY = "Unit#02DummyJobCount";
        public const string JobCount_Unit02_ThroughDummy = "Unit#02ThroughDummyJobCount";
        public const string JobCount_Unit02_ThicknessDummy = "Unit#02ThicknessDummyJobCount";
        public const string JobCount_Unit02_UVMASK = "Unit#02UVMASKJobCount";
        public const string JobCount_Unit03_TFT = "Unit#03TFTProductJobCount";
        public const string JobCount_Unit03_CF = "Unit#03CFProductJobCount";
        public const string JobCount_Unit03_MetalOneDummy = "Unit#03MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit03_ITODummy = "Unit#03ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit03_NIPDummy = "Unit#03NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit03_UnassembledTFT = "Unit#03UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit03_DUMMY = "Unit#03DummyJobCount";
        public const string JobCount_Unit03_ThroughDummy = "Unit#03ThroughDummyJobCount";
        public const string JobCount_Unit03_ThicknessDummy = "Unit#03ThicknessDummyJobCount";
        public const string JobCount_Unit03_UVMASK = "Unit#03UVMASKJobCount";
        public const string JobCount_Unit04_TFT = "Unit#04TFTProductJobCount";
        public const string JobCount_Unit04_CF = "Unit#04CFProductJobCount";
        public const string JobCount_Unit04_MetalOneDummy = "Unit#04MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit04_ITODummy = "Unit#04ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit04_NIPDummy = "Unit#04NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit04_UnassembledTFT = "Unit#04UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit04_DUMMY = "Unit#04DummyJobCount";
        public const string JobCount_Unit04_ThroughDummy = "Unit#04ThroughDummyJobCount";
        public const string JobCount_Unit04_ThicknessDummy = "Unit#04ThicknessDummyJobCount";
        public const string JobCount_Unit04_UVMASK = "Unit#04UVMASKJobCount";
        public const string JobCount_Unit05_TFT = "Unit#05TFTProductJobCount";
        public const string JobCount_Unit05_CF = "Unit#05CFProductJobCount";
        public const string JobCount_Unit05_MetalOneDummy = "Unit#05MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit05_ITODummy = "Unit#05ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit05_NIPDummy = "Unit#05NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit05_UnassembledTFT = "Unit#05UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit05_DUMMY = "Unit#05DummyJobCount";
        public const string JobCount_Unit05_ThroughDummy = "Unit#05ThroughDummyJobCount";
        public const string JobCount_Unit05_ThicknessDummy = "Unit#05ThicknessDummyJobCount";
        public const string JobCount_Unit05_UVMASK = "Unit#05UVMASKJobCount";
        public const string JobCount_Unit06_TFT = "Unit#06TFTProductJobCount";
        public const string JobCount_Unit06_CF = "Unit#06CFProductJobCount";
        public const string JobCount_Unit06_MetalOneDummy = "Unit#06MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit06_ITODummy = "Unit#06ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit06_NIPDummy = "Unit#06NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit06_UnassembledTFT = "Unit#06UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit06_DUMMY = "Unit#06DummyJobCount";
        public const string JobCount_Unit06_ThroughDummy = "Unit#06ThroughDummyJobCount";
        public const string JobCount_Unit06_ThicknessDummy = "Unit#06ThicknessDummyJobCount";
        public const string JobCount_Unit06_UVMASK = "Unit#06UVMASKJobCount";
        public const string JobCount_Unit07_TFT = "Unit#07TFTProductJobCount";
        public const string JobCount_Unit07_CF = "Unit#07CFProductJobCount";
        public const string JobCount_Unit07_MetalOneDummy = "Unit#07MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit07_ITODummy = "Unit#07ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit07_NIPDummy = "Unit#07NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit07_UnassembledTFT = "Unit#07UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit07_DUMMY = "Unit#07DummyJobCount";
        public const string JobCount_Unit07_ThroughDummy = "Unit#07ThroughDummyJobCount";
        public const string JobCount_Unit07_ThicknessDummy = "Unit#07ThicknessDummyJobCount";
        public const string JobCount_Unit07_UVMASK = "Unit#07UVMASKJobCount";
        public const string JobCount_Unit08_TFT = "Unit#08TFTProductJobCount";
        public const string JobCount_Unit08_CF = "Unit#08CFProductJobCount";
        public const string JobCount_Unit08_MetalOneDummy = "Unit#08MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit08_ITODummy = "Unit#08ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit08_NIPDummy = "Unit#08NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit08_UnassembledTFT = "Unit#08UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit08_DUMMY = "Unit#08DummyJobCount";
        public const string JobCount_Unit08_ThroughDummy = "Unit#08ThroughDummyJobCount";
        public const string JobCount_Unit08_ThicknessDummy = "Unit#08ThicknessDummyJobCount";
        public const string JobCount_Unit08_UVMASK = "Unit#08UVMASKJobCount";
        public const string JobCount_Unit09_TFT = "Unit#09TFTProductJobCount";
        public const string JobCount_Unit09_CF = "Unit#09CFProductJobCount";
        public const string JobCount_Unit09_MetalOneDummy = "Unit#09MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit09_ITODummy = "Unit#09ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit09_NIPDummy = "Unit#09NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit09_UnassembledTFT = "Unit#09UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit09_DUMMY = "Unit#09DummyJobCount";
        public const string JobCount_Unit09_ThroughDummy = "Unit#09ThroughDummyJobCount";
        public const string JobCount_Unit09_ThicknessDummy = "Unit#09ThicknessDummyJobCount";
        public const string JobCount_Unit09_UVMASK = "Unit#09UVMASKJobCount";
        public const string JobCount_Unit10_TFT = "Unit#10TFTProductJobCount";
        public const string JobCount_Unit10_CF = "Unit#10CFProductJobCount";
        public const string JobCount_Unit10_MetalOneDummy = "Unit#10MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit10_ITODummy = "Unit#10ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit10_NIPDummy = "Unit#10NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit10_UnassembledTFT = "Unit#10UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit10_DUMMY = "Unit#10DummyJobCount";
        public const string JobCount_Unit10_ThroughDummy = "Unit#10ThroughDummyJobCount";
        public const string JobCount_Unit10_ThicknessDummy = "Unit#10ThicknessDummyJobCount";
        public const string JobCount_Unit10_UVMASK = "Unit#10UVMASKJobCount";
        public const string JobCount_Unit11_TFT = "Unit#11TFTProductJobCount";
        public const string JobCount_Unit11_CF = "Unit#11CFProductJobCount";
        public const string JobCount_Unit11_MetalOneDummy = "Unit#11MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit11_ITODummy = "Unit#11ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit11_NIPDummy = "Unit#11NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit11_UnassembledTFT = "Unit#11UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit11_DUMMY = "Unit#11DummyJobCount";
        public const string JobCount_Unit11_ThroughDummy = "Unit#11ThroughDummyJobCount";
        public const string JobCount_Unit11_ThicknessDummy = "Unit#11ThicknessDummyJobCount";
        public const string JobCount_Unit11_UVMASK = "Unit#11UVMASKJobCount";
        public const string JobCount_Unit12_TFT = "Unit#12TFTProductJobCount";
        public const string JobCount_Unit12_CF = "Unit#12CFProductJobCount";
        public const string JobCount_Unit12_MetalOneDummy = "Unit#12MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit12_ITODummy = "Unit#12ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit12_NIPDummy = "Unit#12NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit12_UnassembledTFT = "Unit#12UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit12_DUMMY = "Unit#12DummyJobCount";
        public const string JobCount_Unit12_ThroughDummy = "Unit#12ThroughDummyJobCount";
        public const string JobCount_Unit12_ThicknessDummy = "Unit#12ThicknessDummyJobCount";
        public const string JobCount_Unit12_UVMASK = "Unit#12UVMASKJobCount";
        public const string JobCount_Unit13_TFT = "Unit#13TFTProductJobCount";
        public const string JobCount_Unit13_CF = "Unit#13CFProductJobCount";
        public const string JobCount_Unit13_MetalOneDummy = "Unit#13MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit13_ITODummy = "Unit#13ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit13_NIPDummy = "Unit#13NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit13_UnassembledTFT = "Unit#13UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit13_DUMMY = "Unit#13DummyJobCount";
        public const string JobCount_Unit13_ThroughDummy = "Unit#13ThroughDummyJobCount";
        public const string JobCount_Unit13_ThicknessDummy = "Unit#13ThicknessDummyJobCount";
        public const string JobCount_Unit13_UVMASK = "Unit#13UVMASKJobCount";
        public const string JobCount_Unit14_TFT = "Unit#14TFTProductJobCount";
        public const string JobCount_Unit14_CF = "Unit#14CFProductJobCount";
        public const string JobCount_Unit14_MetalOneDummy = "Unit#14MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit14_ITODummy = "Unit#14ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit14_NIPDummy = "Unit#14NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit14_UnassembledTFT = "Unit#14UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit14_DUMMY = "Unit#14DummyJobCount";
        public const string JobCount_Unit14_ThroughDummy = "Unit#14ThroughDummyJobCount";
        public const string JobCount_Unit14_ThicknessDummy = "Unit#14ThicknessDummyJobCount";
        public const string JobCount_Unit14_UVMASK = "Unit#14UVMASKJobCount";
        public const string JobCount_Unit15_TFT = "Unit#15TFTProductJobCount";
        public const string JobCount_Unit15_CF = "Unit#15CFProductJobCount";
        public const string JobCount_Unit15_MetalOneDummy = "Unit#015MetalOneDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit15_ITODummy = "Unit#15ITODummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit15_NIPDummy = "Unit#15NIPDummyJobCount"; //t3 cell add for PI
        public const string JobCount_Unit15_UnassembledTFT = "Unit#15UnassembledTFTProductJobCount"; //t3 cell add for PI
        public const string JobCount_Unit15_DUMMY = "Unit#15DummyJobCount";
        public const string JobCount_Unit15_ThroughDummy = "Unit#15ThroughDummyJobCount";
        public const string JobCount_Unit15_ThicknessDummy = "Unit#15ThicknessDummyJobCount";
        public const string JobCount_Unit15_UVMASK = "Unit#15UVMASKJobCount";
        public const string ProcessPauseCommand_ProcessPause = "ProcessPause";
        public const string ProcessPauseCommand_UnitNo = "UnitNo";
        public const string CSTOperationMode = "CSTOperationMode";
        public const string FetchOutJobData_UnitOrPort = "UnitorPort";
        public const string FetchOutJobData_UnitNo = "UnitNo";
        public const string FetchOutJobData_PortNo = "PortNo";
        public const string FetchOutJobData_SlotNo = "SlotNo";
        public const string StoreobData_UnitOrPort = "UnitorPort";
        public const string StoreJobData_UnitNo = "UnitNo";
        public const string StoreJobData_PortNo = "PortNo";
        public const string StoreJobData_SlotNo = "SlotNo";
        public const string RemoveJobData_UnitOrPort = "UnitorPort";
        public const string RemoveJobData_UnitNo = "UnitNo";
        public const string RemoveJobData_PortNo = "PortNo";
        public const string RemoveJobData_SlotNo = "SlotNo";
        public const string RemoveJobData_RemoveJobFlag = "RemoveJobFlag";
        public const string RemoveJobData_OperatorID = "OperatorID";
        public const string RemoveJobData_ManualOutFlag = "ManualOutFlag";
        public const string RemoveJobData_ReasonCode = "ReasonCode";
        public const string IndexerOperationMode = "IndexerOperationMode";
        public const string ProcessTime = "ProcessTime";
        public const string LineBackupModeChangeReport_ReturnCode = "LineBackupModeChangeReturnCode";
        public const string JobLineOutCheck_ReturnCode = "JobLineOutCheckRequestReturnCode";
        public const string LineBackupModeChangeReport_LineBackupMode = "LineBackupMode";
        public const string ForceCleanOutReport_Action = "Action";
        public const string ForceCleanOutReport_Type = "Type";
        public const string ForceCleanOutReport_ReturnCode = "ReturnCode";
        public const string MaterialStatusChange_UnitNo = "UnitNo";
        public const string MaterialStatusChange_MaterialStatus = "MaterialStatus";
        public const string MaterialStatusChange_OperatorID = "OperatorID";
        public const string MaterialStatusChange_MaterialValue = "MaterialValue";
        public const string MaterialStatusChange_SlotNo = "SlotNo";
        public const string MaterialStatusChange_MaterialID= "MaterialID";
    }

    public class eEQPUIPMENTATTRIBUTE
    {
        public string Loader = "LD";
        public string Unloader = "UD";
        public string LoaderUnloader = "LU";
        public string Normal = "NM";
        public string Inspection = "IN";
        public string Coater = "COATER";
        public string Exposure = "EXPOSURE";
    }

    // add by bruce 2016/3/9 for Array special Eq Cim Off then bc bypass eq ppid 
    public class eArrayPPIDByPass   
    {
        public const string TCELA100 = "ARRAY_PPIDBYPASS_TCELA100";
        public const string TCELA200 = "ARRAY_PPIDBYPASS_TCELA200";
        public const string TCELA300 = "ARRAY_PPIDBYPASS_TCELA300";
        public const string TCTEG200 = "ARRAY_PPIDBYPASS_TCTEG200";
        public const string TCTEG400 = "ARRAY_PPIDBYPASS_TCTEG400";
        public const string TCFLR200 = "ARRAY_PPIDBYPASS_TCFLR200";
        public const string TCFLR300 = "ARRAY_PPIDBYPASS_TCFLR300";
        public const string TCAOH800 = "ARRAY_PPIDBYPASS_TCAOH800";
        public const string TCAOH400 = "ARRAY_PPIDBYPASS_TCAOH400";
        public const string TCAOH300 = "ARRAY_PPIDBYPASS_TCAOH300";
        public const string TCAOH900 = "ARRAY_PPIDBYPASS_TCAOH900";
        public const string TCCDO400 = "ARRAY_PPIDBYPASS_TCCDO400";
        public const string TCCDO300 = "ARRAY_PPIDBYPASS_TCCDO300";
        public const string TCATS200 = "ARRAY_PPIDBYPASS_TCATS200";
        public const string TCATS400 = "ARRAY_PPIDBYPASS_TCATS400";
        public const string CELLREICPECHECKEQPLISTCST = "CELLREICPECHECKEQPLISTCST";   //Add By Yangzhenteng20190508
        public const string CELLREICPCHECKEQPLISTPANEL = "CELLREICPCHECKEQPLISTPANEL"; //Add By Yangzhenteng20190508
        public const string CELLPOLUPDATEJOBGRADEEQPLIST = "CELLPOLUPDATEJOBGRADEEQPLIST";//Add By Yangzhenteng20190616
    }

    // add by bruce 2016/3/28 for Array Main Eqp Tracking data check 
    public class eArrayMainEqpTrackingDataCheck
    {
        public const string LINETYPELIST = "ARRAY_MAINEQPTRACKINGDATACEHCK";
    }

    //add by yang 2017/1/4 for CPC First Glass Check
    public class eFirstGlassCheck
    {
        public const string LINETYPELIST = "FIRSTGLASSCHECKBYSEQUENCE";
    }

    //add by yang 2017/1/8 for APC Product In,Out Adjust Time Stamp which Trace Level is "M"
    public class eProductInOutTimeStamp
    {
        public const string PRODUCTINOUTTIMESTAMP = "PRODUCTINOUTTIMESTAMP";
    }
    //add by yang 2017/2/17 for Enable/Disable APPErrorSendToBMS
    public class eAPPErrorSend
    {
        public const string APPERRORSEND = "APPERRORSEND";
    }
    //add by yang 2017/5/25 for No Need Send To CLN  glass whick temporal RTC
    public class eNoNeedSendToCLN
    {
        public const string NONEEDSENDTOCLN = "NONEEDSENDTOCLN";
    }
}
