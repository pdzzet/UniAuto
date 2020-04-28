using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniRCS.Core;


namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class RobotSpecialService : AbstractRobotService
    {
        //抽象類別的實作
        public override bool Init()
        {
            return true;
        }
        public void Destroy()
        {

        }

        /// <summary>
        /// Check WIP CVD Proportional Type and Robot Current Proportional Type.
        /// </summary>
        /// <param name="curRobot">Robot</param>
        /// <param name="jobs">全部WIP</param>
        /// <returns></returns>
        public bool CheckCVDProportionalType(Robot curRobot, List<Job> jobs)
        {
            try
            {

                string strlog = string.Empty;
                bool Force_ChangeCVDProportionalType = false;
                Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if (line == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1})  can not find Line by LineID({2})!",
                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }

                #region Force CVD Proportional Type Don't Care Tack Time.
                try
                {
                    if (ConstantManager[eConstantXML.CVD_ProportionalRule_Force][line.Data.LINEID].Value.ToUpper() == "TRUE")
                    {
                        Force_ChangeCVDProportionalType = true;
                    }
                }
                catch { }
                #endregion

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                if (eqp == null)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1})  can not find EQP by EQPNo({0})!",
                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }

                int cvdjobscount = jobs.Where(j => j.RobotWIP.CurLocation_StageType == eRobotStageType.PORT).ToList().Count;
                if (cvdjobscount <= 0)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = "Check CVD Proportional Type, WIP Current Location Stage Type Are Not From Port !";
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }

                cvdjobscount = jobs.Where(j => j.RobotWIP.CurStepNo == 1).ToList().Count;
                if (cvdjobscount <= 0)
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = "Check CVD Proportional Type, WIP Current Step No Are Not Step '1' !";
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }

                //modify by hujunpeng 20190425 for CVD700新增一个product进行混run逻辑,Deng,20190823
                if (curRobot.Data.LINEID != "TCCVD700")
                {
                    #region 一种product和MQC混run
                    //未設置抽片比例時，還是要給
                    if (curRobot.File.CurCVDProportionalRule == null)
                    {
                        curRobot.File.CurCVDProportionalRule = new CVDProportionalRule();
                        curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                        curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 1; //未設置就預設不為零
                        curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 1;//未設置就預設不為零
                        curRobot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;
                    }
                    //未設置就給1:1
                    if (curRobot.File.CVDProportionalRule.Count <= 0)
                    {
                        curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, 1);
                        curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, 1);
                    }

                    //現在正在抽片的種類及還沒抽完的數量
                    //Job Data : Process Type	Bit 06~11 : Process Type:   0: PROD   1: MQC
                    //所有的片中還有目前正在抽的種類片數有幾片?
                    string robotCVDType = curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD ? "0" : "1";
                    List<Job> jobs1 = jobs.Where(j => j.ArraySpecial.ProcessType == robotCVDType && j.RobotWIP.CurStepNo == 1).ToList();

                    #region CVDProportionalRule Debug Force_ChangeCVDProportionalType时计算当前可控片，如果既不能去CVD又不能去CLN就从可控片中删除此job
                    //20181027 add by hujunpeng
                    if (curRobot.noSendToCLN)
                    {
                        List<Job> deljobs = new List<Job>();
                        foreach (Job job in jobs1)
                        {
                            IDictionary<string, string> dicJobTrackingData = ObjectManager.SubJobDataManager.Decode(job.TrackingData, eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

                            if (dicJobTrackingData == null)
                            {
                                #region  [DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by current Job TrackingData({4})!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, job.CassetteSequenceNo, job.JobSequenceNo,
                                                            job.TrackingData);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                            }

                            if (dicJobTrackingData.ContainsKey("CDC/DHF"))
                            {
                                if (dicJobTrackingData["CDC/DHF"] == "1")
                                {
                                    continue;
                                }
                                if (dicJobTrackingData["CDC/DHF"] == "0")
                                {
                                    #region  [DebugLog]
                                    if (IsShowDetialLog == true)
                                    {
                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass Tracking Data not processed by CLN. Don't Jump to CVD ",
                                            job.CassetteSequenceNo, job.JobSequenceNo, curRobot.Data.NODENO));
                                    }
                                    #endregion
                                    deljobs.Add(job);
                                }

                            }
                        }
                        jobs1.RemoveAll(it => deljobs.Contains(it));

                    }
                    #endregion

                    cvdjobscount = jobs1.Count;

                    if (cvdjobscount <= 0)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("Check CVD Proportional Type,  CVD Fetch Proportional Rule Type Mismatch WIP And Robot({0})!", curRobot.File.CurCVDProportionalRule.curProportionalType.ToString());
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        if (!Force_ChangeCVDProportionalType)
                        {
                            #region 確定沒有可以抽的片，就必須更換計算比例
                            if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD)
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                                else
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 1;
                            }
                            else
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
                                else
                                    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 1;
                            }
                            #endregion
                        }
                        else
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                int cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD ? curRobot.File.CurCVDProportionalRule.curProportionalPRODCount : curRobot.File.CurCVDProportionalRule.curProportionalMQCCount;

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Force CVD Proportional Rule is True,  No glass could Fetch now !! Must be Wait Another Process Type[{2}] glass count[{3}] = 0 will Change other Type.",
                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalType.ToString(), cvdPropTypeCount);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                    }
                    return true;
                    #endregion
                }
                else {
                    #region 曾经需求
                    /*
                    //未設置抽片比例時，還是要給
                    if (curRobot.File.CurCVDProportionalRule == null)
                    {
                        curRobot.File.CurCVDProportionalRule = new CVDProportionalRule();
                        curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                        curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 0; //未設置就預設不為零
                        curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 4;//未設置就預設不為零
                        curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 1;//未設置就預設不為零
                        curRobot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;
                    }
                    //未設置就給1:1
                    if (curRobot.File.CVDProportionalRule.Count <= 0)
                    {
                      //curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, 0);
                        curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, 4);
                        curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1,1);
                    }
                    
                    string robotCVDType = string.Empty;
                    switch(curRobot.File.CurCVDProportionalRule.curProportionalType)
                    {
                        case eCVDIndexRunMode.PROD :
                            robotCVDType="0";
                            break;
                        case eCVDIndexRunMode.MQC:
                            robotCVDType="1";
                            break;
                        case eCVDIndexRunMode.PROD1:
                            robotCVDType="2";
                            break;
                    }
                    List<Job> jobs1 = jobs.Where(j => j.ArraySpecial.ProcessType == robotCVDType && j.RobotWIP.CurStepNo == 1).ToList();

                    #region CVDProportionalRule Debug Force_ChangeCVDProportionalType时计算当前可控片，如果既不能去CVD又不能去CLN就从可控片中删除此job
                    //20181027 add by hujunpeng
                    if (curRobot.noSendToCLN)
                    {
                        List<Job> deljobs = new List<Job>();
                        foreach (Job job in jobs1)
                        {
                            IDictionary<string, string> dicJobTrackingData = ObjectManager.SubJobDataManager.Decode(job.TrackingData, eRobotCommonConst.DB_SUBJOBDATA_ITEM_TRACKINGDATA);

                            if (dicJobTrackingData == null)
                            {
                                #region  [DebugLog]
                                if (IsShowDetialLog == true)
                                {
                                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) Job CassetteSequenceNo({2}) JobSequenceNo({3}) can not Decode TrackingData Info by current Job TrackingData({4})!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, job.CassetteSequenceNo, job.JobSequenceNo,
                                                            job.TrackingData);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }
                                #endregion
                            }

                            if (dicJobTrackingData.ContainsKey("CDC/DHF"))
                            {
                                if (dicJobTrackingData["CDC/DHF"] == "1")
                                {
                                    continue;
                                }
                                if (dicJobTrackingData["CDC/DHF"] == "0")
                                {
                                    #region  [DebugLog]
                                    if (IsShowDetialLog == true)
                                    {
                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={2}] CST_SEQNO=[{0}], JOB_SEQNO=[{1}] glass Tracking Data not processed by CLN. Don't Jump to CVD ",
                                            job.CassetteSequenceNo, job.JobSequenceNo, curRobot.Data.NODENO));
                                    }
                                    #endregion
                                    deljobs.Add(job);
                                }

                            }
                        }
                        jobs1.RemoveAll(it => deljobs.Contains(it));

                    }
                    #endregion

                    cvdjobscount = jobs1.Count;

                    if (cvdjobscount <= 0)
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("Check CVD Proportional Type,  CVD Fetch Proportional Rule Type Mismatch WIP And Robot({0})!", curRobot.File.CurCVDProportionalRule.curProportionalType.ToString());
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        if (!Force_ChangeCVDProportionalType)
                        {
                            #region 確定沒有可以抽的片，就必須更換計算比例
                            if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.PROD)
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                //if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                                //    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];          
                            }
                            else if (curRobot.File.CurCVDProportionalRule.curProportionalType == eCVDIndexRunMode.MQC)
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                //if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                                //    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1];
                            }
                            else
                            {
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                //if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                                //    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
                            }
                            #endregion
                        }
                        else
                        {
                            #region  [DebugLog]
                            if (IsShowDetialLog == true)
                            {
                                int cvdPropTypeCount = 0;
                                switch (curRobot.File.CurCVDProportionalRule.curProportionalType)
                                {
                                    case eCVDIndexRunMode.PROD:
                                        cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curProportionalPRODCount;
                                        break;
                                    case eCVDIndexRunMode.MQC:
                                        cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curProportionalMQCCount;
                                        break;
                                    case eCVDIndexRunMode.PROD1:
                                        cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count;
                                        break;
                                    default:
                                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("curProportionalType{0} is unknow!", curRobot.File.CurCVDProportionalRule.curProportionalType.ToString()));
                                        break;
                                }


                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Force CVD Proportional Rule is True,  No glass could Fetch now !! Must be Wait Another Process Type[{2}] glass count[{3}] = 0 will Change other Type.",
                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalType.ToString(), cvdPropTypeCount);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }
                            #endregion
                        }
                    }
                    return true;*/
                    #endregion
                    
                    //現在正在抽片的種類及還沒抽完的數量
                    //Job Data : Process Type	Bit 06~11 : Process Type:   0: PROD   1: MQC
                    //所有的片中還有目前正在抽的種類片數有幾片?

                    #region 两种product和MQC混run
                    List<Job> Productjobs = jobs.Where(j => j.ArraySpecial.ProcessType == "0" && j.RobotWIP.CurStepNo == 1).ToList();
                    List<Job> MQCjobs = jobs.Where(j => j.ArraySpecial.ProcessType == "1" && j.RobotWIP.CurStepNo == 1).ToList();
                    List<Job> Product1jobs = jobs.Where(j => j.ArraySpecial.ProcessType == "2" && j.RobotWIP.CurStepNo == 1).ToList();

                    if ((Productjobs.Count > 0 && Product1jobs.Count > 0) || (Product1jobs.Count > 0 && Productjobs.Count > 0 && MQCjobs.Count > 0))//只有PORD和PROD1或者三种都有
                    {
                        //未設置就給1:1
                        if (curRobot.File.CVDProportionalRule.Count <= 0)
                        {
                            curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, 4);
                            curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, 1);
                        }
                        else//设置的抽片比例不对时也要重设
                        {
                            if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD) && curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                            {
                                if (curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD] > 0 && curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1] > 0)
                                { }
                                else
                                {
                                    curRobot.File.CVDProportionalRule.Clear();
                                    curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, 4);
                                    curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, 1);
                                }
                            }
                            else
                            {
                                curRobot.File.CVDProportionalRule.Clear();
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, 4);
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, 1);
                            }

                        }
                        //未設置抽片比例時，還是要給
                        if (curRobot.File.CurCVDProportionalRule == null)
                        {
                            curRobot.File.CurCVDProportionalRule = new CVDProportionalRule();
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                            curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 4; //未設置就預設不為零
                            curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 1;//未設置就預設不為零
                            curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 0;
                            curRobot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;
                        }
                        else//设置的抽片比例不对时也要重设
                        {
                            if (curRobot.File.CurCVDProportionalRule.curProportionalMQCCount <= 0 &&
                                (curRobot.File.CurCVDProportionalRule.FirstProportionaltype == eCVDIndexRunMode.PROD || curRobot.File.CurCVDProportionalRule.FirstProportionaltype == eCVDIndexRunMode.PROD1))
                            { }
                            else
                            {
                                //curRobot.File.CurCVDProportionalRule = new CVDProportionalRule();
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 4; //未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 1;//未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 0;
                                curRobot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;
                            }
                        }
                    }
                    if (Productjobs.Count > 0 && MQCjobs.Count > 0)//只有PROD和MQC
                    {
                        //未設置就給1:1
                        if (curRobot.File.CVDProportionalRule.Count <= 0)
                        {
                            curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, 1);
                            curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, 1);
                        }
                        else//设置的抽片比例不对时也要重设
                        {
                            if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD) && curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                            {
                                if (curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD] > 0 && curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC] > 0)
                                { }
                                else
                                {
                                    curRobot.File.CVDProportionalRule.Clear();
                                    curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, 1);
                                    curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, 1);
                                }
                            }
                            else
                            {
                                curRobot.File.CVDProportionalRule.Clear();
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, 1);
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, 1);
                            }
                        }
                        //未設置抽片比例時，還是要給
                        if (curRobot.File.CurCVDProportionalRule == null)
                        {
                            curRobot.File.CurCVDProportionalRule = new CVDProportionalRule();
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                            curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 1; //未設置就預設不為零
                            curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 1;//未設置就預設不為零
                            curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 0;
                            curRobot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;
                        }
                        else//设置的抽片比例不对时也要重设
                        {
                            if (curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count <= 0 &&
                                (curRobot.File.CurCVDProportionalRule.FirstProportionaltype == eCVDIndexRunMode.PROD || curRobot.File.CurCVDProportionalRule.FirstProportionaltype == eCVDIndexRunMode.MQC))
                            { }
                            else
                            {
                                //curRobot.File.CurCVDProportionalRule = new CVDProportionalRule();
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 1; //未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 1;//未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 0;
                                curRobot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;
                            }
                        }
                    }
                    if (Product1jobs.Count > 0 && MQCjobs.Count > 0)//只有PROD1和MQC
                    {
                        //未設置就給1:1
                        if (curRobot.File.CVDProportionalRule.Count <= 0)
                        {
                            curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, 1);
                            curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, 1);
                        }
                        else//设置的抽片比例不对时也要重设
                        {
                            if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1) && curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                            {
                                if (curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1] > 0 && curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC] > 0)
                                { }
                                else
                                {
                                    curRobot.File.CVDProportionalRule.Clear();
                                    curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, 1);
                                    curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, 1);
                                }
                            }
                            else
                            {
                                curRobot.File.CVDProportionalRule.Clear();
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, 1);
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, 1);
                            }
                        }
                        //未設置抽片比例時，還是要給
                        if (curRobot.File.CurCVDProportionalRule == null)
                        {
                            curRobot.File.CurCVDProportionalRule = new CVDProportionalRule();
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                            curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 1; //未設置就預設不為零
                            curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 1;//未設置就預設不為零
                            curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 0;
                            curRobot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD1;
                        }
                        else//设置的抽片比例不对时也要重设
                        {
                            if (curRobot.File.CurCVDProportionalRule.curProportionalPRODCount <= 0 &&
                                (curRobot.File.CurCVDProportionalRule.FirstProportionaltype == eCVDIndexRunMode.PROD1 || curRobot.File.CurCVDProportionalRule.FirstProportionaltype == eCVDIndexRunMode.MQC))
                            { }
                            else
                            {
                                //curRobot.File.CurCVDProportionalRule = new CVDProportionalRule();
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 1; //未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 1;//未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 0;
                                curRobot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD1;
                            }
                        }
                    }
                    if (!Force_ChangeCVDProportionalType)
                    {
                        if (Productjobs.Count > 0 && Product1jobs.Count <= 0 && MQCjobs.Count <= 0)
                        {
                            //未設置抽片比例時，還是要給
                            if (curRobot.File.CurCVDProportionalRule == null)
                            {
                                curRobot.File.CurCVDProportionalRule = new CVDProportionalRule();
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 4; //未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 1;//未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 0;
                                curRobot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD;
                            }
                            //未設置就給1:1
                            if (curRobot.File.CVDProportionalRule.Count <= 0)
                            {
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD, 4);
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, 1);
                            }
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD;
                            //if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD))
                            //    curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD];
                        }
                        if (Productjobs.Count <= 0 && Product1jobs.Count > 0 && MQCjobs.Count <= 0)
                        {
                            //未設置抽片比例時，還是要給
                            if (curRobot.File.CurCVDProportionalRule == null)
                            {
                                curRobot.File.CurCVDProportionalRule = new CVDProportionalRule();
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                                curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 1; //未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 1;//未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 0;
                                curRobot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.PROD1;
                            }
                            //未設置就給1:1
                            if (curRobot.File.CVDProportionalRule.Count <= 0)
                            {
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, 1);
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, 1);
                            }
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.PROD1;
                            //if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.PROD1))
                            //    curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.PROD1];
                        }
                        if (Productjobs.Count <= 0 && Product1jobs.Count <= 0 && MQCjobs.Count > 0)
                        {
                            //未設置抽片比例時，還是要給
                            if (curRobot.File.CurCVDProportionalRule == null)
                            {
                                curRobot.File.CurCVDProportionalRule = new CVDProportionalRule();
                                curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                                curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count = 1; //未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = 1;//未設置就預設不為零
                                curRobot.File.CurCVDProportionalRule.curProportionalPRODCount = 0;
                                curRobot.File.CurCVDProportionalRule.FirstProportionaltype = eCVDIndexRunMode.MQC;
                            }
                            //未設置就給1:1
                            if (curRobot.File.CVDProportionalRule.Count <= 0)
                            {
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.PROD1, 1);
                                curRobot.File.CVDProportionalRule.Add(eCVDIndexRunMode.MQC, 1);
                            }
                            curRobot.File.CurCVDProportionalRule.curProportionalType = eCVDIndexRunMode.MQC;
                            //if (curRobot.File.CVDProportionalRule.ContainsKey(eCVDIndexRunMode.MQC))
                            //    curRobot.File.CurCVDProportionalRule.curProportionalMQCCount = curRobot.File.CVDProportionalRule[eCVDIndexRunMode.MQC];
                        }
                    }
                    else
                    {
                        #region  [DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            int cvdPropTypeCount = 0;
                            switch (curRobot.File.CurCVDProportionalRule.curProportionalType)
                            {
                                case eCVDIndexRunMode.PROD:
                                    cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curProportionalPRODCount;
                                    break;
                                case eCVDIndexRunMode.MQC:
                                    cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curProportionalMQCCount;
                                    break;
                                case eCVDIndexRunMode.PROD1:
                                    cvdPropTypeCount = curRobot.File.CurCVDProportionalRule.curPorportionalPROD1Count;
                                    break;
                                default:
                                    Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("curProportionalType{0} is unknow!", curRobot.File.CurCVDProportionalRule.curProportionalType.ToString()));
                                    break;
                            }


                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS]Robot({1}) Force CVD Proportional Rule is True,  No glass could Fetch now !! Must be Wait Another Process Type[{2}] glass count[{3}] = 0 will Change other Type.",
                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.File.CurCVDProportionalRule.curProportionalType.ToString(), cvdPropTypeCount);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                    }
                    return true;
                    #endregion
                
                }
                
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }
        /// <summary>
        /// Yang
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curStage"></param>
        /// 
        //add by qiumin 20171017   CHECK go ELA1 or ELA2 glass 
        public bool CheckELAOneByOneRun(Robot curRobot, List<Job> jobs)
        {
            try
            {

                string strlog = string.Empty;
                Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
                if (line == null)
                {
                    curRobot.File.CurELAEQPChangeflag = "Y"; //add by qiumin 20171019 没有重新赋值就需要持续扫描进入
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1})  can not find Line by LineID({2})!",
                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }


                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                if (eqp == null)
                {
                    curRobot.File.CurELAEQPChangeflag = "Y"; //add by qiumin 20171019 没有重新赋值就需要持续扫描进入
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1})  can not find EQP by EQPNo({0})!",
                                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }

                int elajobscount = jobs.Where(j => j.RobotWIP.CurLocation_StageType == eRobotStageType.PORT).ToList().Count;
                if (elajobscount <= 0)
                {
                    curRobot.File.CurELAEQPChangeflag = "Y"; //add by qiumin 20171019 没有重新赋值就需要持续扫描进入

                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = "Check ELA One By One Run, WIP Current Location Stage Type Are Not From Port,ELA EQP Change flag Change to Y !";
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }

                elajobscount = jobs.Where(j => j.RobotWIP.CurStepNo == 1).ToList().Count;
                if (elajobscount <= 0)
                {
                    curRobot.File.CurELAEQPChangeflag = "Y";//add by qiumin 20171019 没有重新赋值就需要持续扫描进入
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = "Check ELA One By One Run, WIP Current Step No Are Not Step '1', ELA EQP Change flag Change to Y !";
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return false;
                }
                int ela1jobcount = jobs.Where(j => j.RobotWIP.CurStepNo == 1 && j.ArraySpecial.ELA1BY1Flag == "L4" && j.SamplingSlotFlag == "1" && j.ArraySpecial.ProcessType.Trim().Equals("0")).ToList().Count;
                int ela2jobcount = jobs.Where(j => j.RobotWIP.CurStepNo == 1 && j.ArraySpecial.ELA1BY1Flag == "L5" && j.SamplingSlotFlag == "1" && j.ArraySpecial.ProcessType.Trim().Equals("0")).ToList().Count;
                int ela12jobcount = jobs.Where(j => j.RobotWIP.CurStepNo == 1 && j.ArraySpecial.ELA1BY1Flag == "L45" && j.SamplingSlotFlag == "1" && j.ArraySpecial.ProcessType.Trim().Equals("0")).ToList().Count;
                int elaMQCjobcount = jobs.Where(j => j.RobotWIP.CurStepNo == 1 && j.ArraySpecial.ProcessType.Trim().Equals("1") && j.SamplingSlotFlag == "1").ToList().Count;
                Equipment   ela1 = ObjectManager.EquipmentManager.GetEQP("L4");
                Equipment   ela2 = ObjectManager.EquipmentManager.GetEQP("L5");
                if (elaMQCjobcount > 0)
                {
                    if (curRobot.File.CurELAEQPType != "M")
                    {
                        curRobot.File.CurELAEQPType = "M";
                        curRobot.File.CurELAEQPChangeflag = "N";
                        strlog = string.Format("Check ELA one by one, goto MQCELA glass count({0}),Robot CurELAEQPType change to({1}),ELA EQP Change flag({2})!", elaMQCjobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        return true;
                    }
                    if (ela1jobcount == 0 && ela2jobcount == 0 && ela12jobcount == 0)
                    {
                        curRobot.File.CurELAEQPType = "M";
                        curRobot.File.CurELAEQPChangeflag = "N";
                        strlog = string.Format("Check ELA one by one, goto MQCELA glass count({0}),Robot CurELAEQPType change to({1}),ELA EQP Change flag({2})!", elaMQCjobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        return true;
                    }
                }
                if (ela2jobcount > 0 || ela12jobcount > 0 || ela1jobcount>0)
                {
                    if (ela2jobcount > 0 && ela12jobcount > 0)
                    {
                        if (ela2jobcount > 0 && curRobot.File.CurELAEQPType != "L5" && (ela2.File.Status == eEQPStatus.RUN || ela2.File.Status == eEQPStatus.IDLE))
                        {
                            curRobot.File.CurELAEQPType = "L5";
                            curRobot.File.CurELAEQPChangeflag = "N";
                            strlog = string.Format("Check ELA one by one, goto ELA2 glass count({0}),Robot CurELAEQPType change to({1}),,ELA EQP Change flag({2})!", ela2jobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            return true;
                        }
                        if (ela12jobcount > 0 && curRobot.File.CurELAEQPType != "L45" && (ela1.File.Status == eEQPStatus.RUN || ela1.File.Status == eEQPStatus.IDLE|| ela2.File.Status == eEQPStatus.RUN || ela2.File.Status == eEQPStatus.IDLE))
                        {
                            curRobot.File.CurELAEQPType = "L45";
                            curRobot.File.CurELAEQPChangeflag = "N";
                            strlog = string.Format("Check ELA one by one, goto ELA2 or ELA1 glass count({0}),Robot CurELAEQPType change to({1}),ELA EQP Change flag({2})!", ela12jobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            return true;
                        }

                    }
                    if (ela1jobcount > 0 && ela12jobcount > 0)
                    {
                        if (ela1jobcount > 0 && curRobot.File.CurELAEQPType != "L4" && (ela1.File.Status == eEQPStatus.RUN || ela1.File.Status == eEQPStatus.IDLE))
                        {
                            curRobot.File.CurELAEQPType = "L4";
                            curRobot.File.CurELAEQPChangeflag = "N";
                            strlog = string.Format("Check ELA one by one, goto ELA1 glass count({0}),Robot CurELAEQPType change to({1}),,ELA EQP Change flag({2})!", ela1jobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            return true;
                        }
                        if (ela12jobcount > 0 && curRobot.File.CurELAEQPType != "L45" && (ela1.File.Status == eEQPStatus.RUN || ela1.File.Status == eEQPStatus.IDLE || ela2.File.Status == eEQPStatus.RUN || ela2.File.Status == eEQPStatus.IDLE))
                        {
                            curRobot.File.CurELAEQPType = "L45";
                            curRobot.File.CurELAEQPChangeflag = "N";
                            strlog = string.Format("Check ELA one by one, goto ELA2 or ELA1 glass count({0}),Robot CurELAEQPType change to({1}),ELA EQP Change flag({2})!", ela12jobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            return true;
                        }

                    }
                    if (ela2jobcount > 0 && ela1jobcount > 0)
                    {
                        if (ela2jobcount > 0 && curRobot.File.CurELAEQPType != "L5" && (ela2.File.Status == eEQPStatus.RUN || ela2.File.Status == eEQPStatus.IDLE))
                        {
                            curRobot.File.CurELAEQPType = "L5";
                            curRobot.File.CurELAEQPChangeflag = "N";
                            strlog = string.Format("Check ELA one by one, goto ELA2 glass count({0}),Robot CurELAEQPType change to({1}),,ELA EQP Change flag({2})!", ela2jobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            return true;
                        }
                        if (ela1jobcount > 0 && curRobot.File.CurELAEQPType != "L4" && (ela1.File.Status == eEQPStatus.RUN || ela1.File.Status == eEQPStatus.IDLE) )
                        {
                            curRobot.File.CurELAEQPType = "L4";
                            curRobot.File.CurELAEQPChangeflag = "N";
                            strlog = string.Format("Check ELA one by one, goto ELA1  glass count({0}),Robot CurELAEQPType change to({1}),ELA EQP Change flag({2})!", ela1jobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            return true;
                        }

                    }
                    if (ela2jobcount > 0 && ela12jobcount == 0 && ela1jobcount == 0 && (ela2.File.Status == eEQPStatus.RUN || ela2.File.Status == eEQPStatus.IDLE))
                    {
                        curRobot.File.CurELAEQPType = "L5";
                        curRobot.File.CurELAEQPChangeflag = "N";
                        strlog = string.Format("Check ELA one by one, goto ELA2 or ELA1 glass count({0}), goto ELA2 glass count({1}),Robot CurELAEQPType change to({2}),ELA EQP Change flag({3})!", ela12jobcount, ela2jobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        return true;
                    }

                    if (ela2jobcount == 0 && ela12jobcount > 0 && ela1jobcount == 0 && ((ela1.File.Status == eEQPStatus.RUN || ela1.File.Status == eEQPStatus.IDLE) || (ela2.File.Status == eEQPStatus.RUN || ela2.File.Status == eEQPStatus.IDLE)))
                    {
                        curRobot.File.CurELAEQPType = "L45";
                        curRobot.File.CurELAEQPChangeflag = "N";
                        strlog = string.Format("Check ELA one by one, goto ELA2 or ELA1 glass count({0}), goto ELA2 glass count({1}),Robot CurELAEQPType change to({2}),ELA EQP Change flag({3})!", ela12jobcount, ela2jobcount,curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        return true;
                    }
                    if (ela1jobcount > 0 && ela12jobcount == 0 && ela2jobcount == 0 && (ela1.File.Status == eEQPStatus.RUN || ela1.File.Status == eEQPStatus.IDLE))
                    {
                        curRobot.File.CurELAEQPType = "L4";
                        curRobot.File.CurELAEQPChangeflag = "N";
                        strlog = string.Format("Check ELA one by one, goto ELA2 or ELA1 glass count({0}), goto ELA1 glass count({1}),Robot CurELAEQPType change to({2}),ELA EQP Change flag({3})!", ela12jobcount, ela1jobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        return true;
                    }
                }
                else
                {
                    curRobot.File.CurELAEQPChangeflag = "Y";
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("Check ELA one by one NO can in ELA GLASS,ELA1 or ELA2 status is not IDLE OR RUN, goto ELA2 or ELA1 glass count({0}),only goto ELA1 glass count({1})only goto ELA2 glass count({2}),Robot CurELAEQPType change to({3}),ELA EQP Change flag({4})!", ela12jobcount, ela1jobcount, ela2jobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    return true;
                }
                curRobot.File.CurELAEQPChangeflag = "N";
                strlog = string.Format("Check ELA one by one Only, goto ELA2 or ELA1 glass count({0}),only goto ELA1 glass count({1})only goto ELA2 glass count({2}),Robot CurELAEQPType change to({3}),ELA EQP Change flag({4})!", ela12jobcount, ela1jobcount, ela2jobcount, curRobot.File.CurELAEQPType, curRobot.File.CurELAEQPChangeflag);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }
        public void MixNoAdd(Robot curRobot)
        {
                lock (curRobot)
                {
                    curRobot.MixNo++;
                    if (curRobot.MixNo == 0 || curRobot.MixNo >= 5) curRobot.MixNo = 1;                 
                }
         }
        /// <summary>
        /// Yang transfer for DRY chamber mode
        /// </summary>
        /// <param name="chambermode"></param>
        /// <returns></returns>
        public string ChamberModeTransfer(string chambermode)
        {
            switch (chambermode.Trim())
            {
                case "MQC":
                    return "1";
                case "PS":
                    return "2";
                case "GE":
                    return "3";
                case "ILD":
                    return "4";
                case "SD":
                    return "5";
                case "PV":
                    return "6";
                case "ASH":
                    return "7";
                case "PLN":
                    return "8";
                default:
                    return "0";
            }
        }
        /// <summary>
        /// Yang
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="jobs"></param>
        /// <param name="MixNo"></param>
        /// <returns></returns>
        public bool CheckDRYMixFetchOutByUnitNo(Robot curRobot, List<Job> jobs)
        {
            int curUnitNo;
            string MixNochambermode;
            if (!curRobot.ReCheck)
            {
                if (curRobot.MixNo == 0 || curRobot.MixNo >= 5) curRobot.MixNo = 1;
                curUnitNo = curRobot.MixNo + 3;
            }
            else
            {
                curRobot.MixNo++;
                if (curRobot.MixNo == 0 || curRobot.MixNo >= 5) curRobot.MixNo = 1;
                curUnitNo = curRobot.MixNo + 3;
            }
            Unit curUnit = ObjectManager.UnitManager.GetUnit("L4", curUnitNo.ToString());
            if(curRobot.Data.LINETYPE.Contains("_YAC"))  MixNochambermode = ChamberModeTransfer(curUnit.File.ChamberRunMode);
            else   MixNochambermode = ChamberModeTransfer(curUnit.File.RunMode);

            curRobot.Context.AddParameter(eRobotContextParameter.UnitNo, curUnitNo.ToString());
            curRobot.Context.AddParameter(eRobotContextParameter.chambermode, MixNochambermode);

            List<string> curProcessTypes = new List<string>();

            foreach (Job job in jobs.Where(s => s.RobotWIP.CurStepNo == 1))
            {
                if (!curProcessTypes.Contains(job.ArraySpecial.ProcessType)) curProcessTypes.Add(job.ArraySpecial.ProcessType);

                //对joblist做排序
                //当前要去的unit的type的job,优先级最高
                //对step1的job,塞processtypepriority属性,保证当前要服务的unit对应的job排在list前面,否则要通过filter去找到这个服务的unit的job
                if (job.ArraySpecial.ProcessType == MixNochambermode) job.RobotWIP.dryprocesstypepriority = curUnitNo;
            }
            return true;

        }

        /// <summary>
        /// Jack.Wang 获取实时DYR 设备可以进去的ChamberMode
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public bool CheckDRYMixFetchOutByUnitNo2(Robot curRobot, List<Job> jobs)
        {
            curRobot.File.DryRealTimeChamberMode="";
            List<string> listUnit=getDryUnitList(curRobot);
            foreach (string unitChamber in listUnit)
            {
                if (jobs.Where(t => t.ArraySpecial.ProcessType == unitChamber).Count() > 0)
                {
                    curRobot.File.DryRealTimeChamberMode = unitChamber;
                    break;
                }
            }

            if (curRobot.File.DryRealTimeChamberMode == "") return false;
            else return true;
        }
        /// <summary>
        /// 对DRY unit ChamberMode 排序
        /// </summary>
        /// <param name="curRobot"></param>
        /// <returns></returns>
        private List<string>  getDryUnitList(Robot curRobot)
        {
            List<Unit> units = (List<Unit>)ObjectManager.UnitManager.GetUnitsByEQPNo("L4").Where(t => t.Data.UNITTYPE.Trim() == "CHAMBER").OrderBy(t => t.Data.UNITNO);
            units = (List<Unit>)units.Where(t => t.File.Status == eEQPStatus.IDLE || t.File.Status == eEQPStatus.RUN);
            List<string> unitChamberModeList = new List<string>();
            foreach(Unit unit in units){
                unitChamberModeList.Add(unit.File.ChamberRunMode);
            }

            List<string> realUnitChamberList = new List<string>();
            if (!unitChamberModeList.Contains(curRobot.File.DryLastProcessType)) return unitChamberModeList;
            int realIndex = realUnitChamberList.IndexOf(curRobot.File.DryLastProcessType);
            IEnumerable<string> UnitCollection = unitChamberModeList.Skip(realIndex + 1); if (UnitCollection == null || UnitCollection.Count() == 0) return unitChamberModeList;
            realUnitChamberList.AddRange(UnitCollection);
            realUnitChamberList.RemoveRange(realIndex, unitChamberModeList.Count - realIndex - 1);
            realUnitChamberList.AddRange(unitChamberModeList); return realUnitChamberList;
        }


    }
}
