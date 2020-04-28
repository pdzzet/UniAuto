using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;
using System.IO;
using System.Data;
using UniAuto.UniBCS.Log;
using System.Reflection;
using System.Diagnostics;
using UniAuto.UniBCS.Core;
using System.Timers;

namespace UniAuto.UniBCS.EntityManager
{
    //public class RobotJobManager : EntityManager, IDataSource
    //{
    //    private Dictionary<string, RobotJob> _entities = new Dictionary<string, RobotJob>();

    //    #region 繼承EntityManager 與Init Setting

    //    public override EntityManager.FILE_TYPE GetFileType()
    //    {
    //        return FILE_TYPE.BIN;
    //    }

    //    protected override string GetSelectHQL()
    //    {
    //        return string.Empty;
    //    }

    //    protected override Type GetTypeOfEntityData()
    //    {
    //        return null;
    //    }

    //    //產生序列化文件
    //    protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
    //    {
    //        Filenames = new List<string>();
    //        Filenames.Add("*.bin");
    //    }

    //    protected override Type GetTypeOfEntityFile()
    //    {
    //        return typeof(RobotJob);
    //    }

    //    //確認檔案是否存在
    //    protected override EntityFile NewEntityFile(string Filename)
    //    {
    //        string[] substrs = Filename.Split('_');
    //        if (substrs != null && substrs.Length == 2)
    //        {
    //            int cstSeqNo = Convert.ToInt32(substrs[0]);
    //            int slotNo = Convert.ToInt32(substrs[1]);
    //            return new RobotJob(cstSeqNo, slotNo);
    //        }
    //        return null;
    //    }

    //    protected override void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles)
    //    {

    //        foreach (EntityFile entity_file in entityFiles)
    //        {
    //            try
    //            {
    //                RobotJob robotJob_entity_file = entity_file as RobotJob;

    //                if (robotJob_entity_file != null)
    //                {
    //                    if (!_entities.ContainsKey(robotJob_entity_file.JobKey))
    //                    {
    //                        _entities.Add(robotJob_entity_file.JobKey, robotJob_entity_file);
    //                    }
    //                }
    //            }
    //            catch (System.Exception ex)
    //            {
    //                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
    //            }

    //        }
    //        //20150205 add for 定時刪除RobotJob
    //        _deleteRobotJobtimer = new Timer(24 * 3600 * 1000);
    //        //_deleteRobotJobtimer = new Timer(20000);
    //        _deleteRobotJobtimer.AutoReset = true;
    //        _deleteRobotJobtimer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
    //        _deleteRobotJobtimer.Start();
    //    }

    //    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    //    {
    //        string strlog = string.Empty;

    //        try
    //        {
    //            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Auto Delete RobotJob Over {1} days by RobotJob CreateTime Start .**********************************************************", "L1", JobTimeOver);
    //            Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

    //            if (_entities != null && _entities.Count > 0)
    //            {
    //                IList<RobotJob> robotJobs = null;
    //                lock (_entities)
    //                {
    //                    DateTime now = DateTime.Now;
    //                    robotJobs = _entities.Values.Where(robotJob => robotJob.CreateTime.AddDays(JobTimeOver).CompareTo(now) < 0).ToList();
    //                }
    //                if (robotJobs != null)
    //                {
    //                    this.DeleteRobotJobs(robotJobs);
    //                }
    //            }

    //            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Auto Delete RobotJob Over {1} days by RobotJob CreateTime End .************************************************************", "L1", JobTimeOver);
    //            Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


    //        }
    //        catch (System.Exception ex)
    //        {
    //            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
    //        }
    //    }

    //    public IList<string> GetEntityNames()
    //    {
    //        IList<string> entityNames = new List<string>();
    //        entityNames.Add("RobotJobManager");
    //        return entityNames;
    //    }

    //    public System.Data.DataTable GetDataTable(string entityName)
    //    {
    //        try
    //        {

    //            DataTable dt = new DataTable();
    //            RobotJob file = new RobotJob();

    //            DataTableHelp.DataTableAppendColumn(file, dt);

    //            IList<RobotJob> robotjobs = GetRobotJobs();

    //            foreach (RobotJob robotjob in robotjobs)
    //            {
    //                DataRow dr = dt.NewRow();
    //                DataTableHelp.DataRowAssignValue(robotjob, dr);
    //                dt.Rows.Add(dr);
    //            }

    //            return dt;

    //        }
    //        catch (System.Exception ex)
    //        {
    //            NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
    //            return null;
    //        }

    //    }

    //    #endregion

    //    /// <summary>
    //    /// 將EntityFile放入Queue, 由Thread存檔
    //    /// </summary>
    //    /// <param name="file">JobEntityFile(job.File)</param>
    //    public override void EnqueueSave(EntityFile file)
    //    {
    //        if (file is RobotJob)
    //        {
    //            RobotJob robotjob = file as RobotJob;
    //            string job_no = string.Format("{0}_{1}", robotjob.CstSeqNo, robotjob.JobSeqNo);
    //            string fname = string.Format("{0}.bin", job_no);
    //            file.SetFilename(fname);
    //            base.EnqueueSave(file);
    //        }
    //    }

    //    public void AddJobs(IList<RobotJob> robotJobs)
    //    {
    //        if (robotJobs == null) return;

    //        foreach (RobotJob j in robotJobs)
    //        {
    //            try
    //            {
    //                if (_entities.ContainsKey(j.JobKey))
    //                {
    //                    _entities.Remove(j.JobKey);
    //                }
    //                _entities.Add(j.JobKey, j);
    //                EnqueueSave(j);
    //            }
    //            catch (Exception ex)
    //            {
    //                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 將RobotJob加入Dictionary
    //    /// </summary>
    //    /// <param name="Job">若Job Key有重複, 會throw exception</param>
    //    public void AddJob(RobotJob robotjob)
    //    {
    //        if (robotjob.CstSeqNo == "0")
    //            return;
    //        if (robotjob.JobSeqNo == "0")
    //            return;

    //        lock (_entities)
    //        {
    //            if (_entities.ContainsKey(robotjob.JobKey))
    //            {
    //                _entities.Remove(robotjob.JobKey);
    //                DeleteRobotJob(robotjob);
    //            }
    //            _entities.Add(robotjob.JobKey, robotjob);
    //            EnqueueSave(robotjob);
    //        }
    //    }

    //    public void DeleteRobotJobs(IList<RobotJob> robotJobs)
    //    {
    //        if (robotJobs == null) return;
    //        foreach (RobotJob j in robotJobs)
    //        {
    //            DeleteRobotJob(j);
    //        }
    //    }

    //    public bool DeleteRobotJob(RobotJob robotJob)
    //    {
    //        string strlog = string.Empty;

    //        try
    //        {
    //            if (_entities.ContainsKey(robotJob.JobKey))
    //            {
    //                lock (_entities)
    //                {
    //                    _entities.Remove(robotJob.JobKey);
    //                }
    //                lock (robotJob)
    //                {
    //                    robotJob.WriteFlag = false;
    //                }

    //                EnqueueSave(robotJob);

    //                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Delete CassetteSequenceNo({1}) JobSequenceNo({2}) CreateTime({3}) !", "L1", robotJob.CstSeqNo,robotJob.JobSeqNo,robotJob.CreateTime.ToString());
    //                Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

    //                return true;
    //            }
    //            return false;
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
    //            return false;
    //        }
    //    }

    //    /// <summary>
    //    /// 取得line Job 以List 方式傳回
    //    /// </summary>
    //    /// <returns>Job List</returns>
    //    public IList<RobotJob> GetRobotJobs()
    //    {
    //        IList<RobotJob> ret = null;
    //        lock (_entities)
    //        {
    //            ret = _entities.Values.ToList();
    //        }
    //        return ret;
    //    }

    //    /// <summary>
    //    /// 透過CST Seq取得line Job 以List 方式傳回
    //    /// </summary>
    //    /// <returns>Job List</returns>
    //    public IList<RobotJob> GetRobotJobs(string cstSeqNo)
    //    {
    //        IList<RobotJob> ret = null;
    //        lock (_entities)
    //        {
    //            ret = _entities.Values.Where(w => w.CstSeqNo == cstSeqNo).ToList();
    //        }
    //        return ret;
    //    }

    //    /// <summary>
    //    /// 以Job No Work No (Cassette Sequence No + Job Sequence No)取得RobotJob(WIP)
    //    /// </summary>
    //    /// <param name="cassetteSequenceNo">Cassette SEQ No</param>
    //    /// <param name="jobSequenceNo">Job SEQ No</param>
    //    /// <returns>Job</returns>
    //    public RobotJob GetRobotJob(string cassetteSequenceNo, string jobSequenceNo)
    //    {
    //        string job_no = string.Format("{0}_{1}", cassetteSequenceNo, jobSequenceNo);
    //        RobotJob ret = null;
    //        lock (_entities)
    //        {
    //            if (_entities.ContainsKey(job_no))
    //            {
    //                ret = _entities[job_no];
    //            }
    //        }
    //        return ret;
    //    }

    //    /// <summary>
    //    /// 以JobKey取得RobotJob(WIP)
    //    /// </summary>
    //    /// <param name="jobKey"></param>
    //    /// <returns></returns>
    //    public RobotJob GetRobotJob(string jobKey)
    //    {
    //        try
    //        {
    //            RobotJob ret = null;
    //            lock (_entities)
    //            {
    //                if (_entities.ContainsKey(jobKey))
    //                {
    //                    ret = _entities[jobKey];
    //                }
    //            }
    //            return ret;
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.Print(ex.Message);
    //            return null;
    //        }
    //    }

    //    public void CreateRobotJobsByBCSJobs(string eqpNo,IList<Job> bcsJobs)
    //    {

    //        if (bcsJobs == null) return;

    //        foreach (Job curBcsJob in bcsJobs)
    //        {
    //            try
    //            {
    //                CreateRobotJobByBCSJob(eqpNo,curBcsJob);

    //            }
    //            catch (Exception ex)
    //            {
    //                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
    //            }
    //        }

    //    }

    //    public bool CreateRobotJobByBCSJob(string eqpNo,Job curBcsJob)
    //    {
    //        string strlog=string.Empty;

    //        try
    //        {

    //            Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);

    //            if (curRobot == null)
    //            {

    //                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Can not Find Robot by EQPNo({1}) !", eqpNo, eqpNo);
    //                Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

    //                //20150128 modify CBSOR300  Robot屬於L2 但是會出現L3 DP Port下Command會找不到,所以透過ServerName來取得
    //                curRobot = ObjectManager.RobotManager.GetRobotbySeverName(Workbench.ServerName);
                    
    //                if (curRobot == null)
    //                {

    //                    strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Can not Find Robot by ServerName({1}) !", eqpNo, Workbench.ServerName);
    //                    Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

    //                    return false;
    //                }

    //            }

    //            if (_entities.ContainsKey(curBcsJob.JobKey))
    //            {
    //                _entities.Remove(curBcsJob.JobKey);
    //            }

    //            #region Create New RobotJob

    //            RobotJob robotJob = new RobotJob();

    //            robotJob.CstSeqNo = curBcsJob.CassetteSequenceNo;
    //            robotJob.JobSeqNo = curBcsJob.JobSequenceNo;
    //            robotJob.JobKey = curBcsJob.JobKey;
    //            robotJob.CurJobStatus = eRobotJobStatus.WAIT_PROC;  //BCS呼叫此Function表示下貨完成等候抽片         
    //            robotJob.CurStepNo = 1;  //從1開始
    //            //20150327 add NotRecipeByPassStepNo
    //            robotJob.NotRecipeByPassStepNo = 0;
    //            robotJob.CurStageNo = curBcsJob.SourcePortID;
    //            robotJob.InputTrackData = new string('0', 32);
    //            robotJob.LinkSignalTrackData = new string('0', 32);

    //            //Set Robot Route by BCS RunMode 
    //            string bcsJobRunMode = string.Empty;
    //            bcsJobRunMode =curBcsJob.CellSpecial.RunMode;

    //            robotJob.CurRunMode = bcsJobRunMode;
    //            robotJob.RobotRoutes = ObjectManager.JobFlowManager.GetRobotRoute(curRobot.Data.ROBOTNAME, bcsJobRunMode);

    //            robotJob.CreateTime = DateTime.Now;

    //            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] RobotJob JobKey({1}) add RunMode({2}) RouteCount({3}) !", eqpNo, robotJob.JobKey, bcsJobRunMode, robotJob.RobotRoutes.Count.ToString());
    //            Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

    //            #endregion

    //            _entities.Add(robotJob.JobKey, robotJob);
    //            EnqueueSave(robotJob);

    //            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] RobotJob JobKey({1}) is Create !", eqpNo, robotJob.JobKey);
    //            Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
    //            return false;
    //        }

    //    }

    //    public void DeleteRobotJobsByBCSJobs(string eqpNo, IList<Job> bcsJobs)
    //    {

    //        if (bcsJobs == null) return;

    //        foreach (Job curBcsJob in bcsJobs)
    //        {
    //            try
    //            {
    //                DeleteRobotJobByBCSJob(eqpNo,curBcsJob);

    //            }
    //            catch (Exception ex)
    //            {
    //                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
    //            }
    //        }

    //    }

    //    public bool DeleteRobotJobByBCSJob(string eqpNo,Job curBcsJob)
    //    {
    //        string strlog = string.Empty;

    //        try
    //        {

    //            RobotJob robotJob = GetRobotJob(curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

    //            if (robotJob == null)
    //            {
    //                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Delete RobotJob can not find Robot Job by CassetteSequenceNo({1}) JobSequenceNo({2})!",
    //                                                                   eqpNo, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);
    //                Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

    //                return false;
    //            }

    //            if (_entities.ContainsKey(robotJob.JobKey))
    //            {
    //                lock (_entities)
    //                {
    //                    _entities.Remove(robotJob.JobKey);
    //                }
    //                lock (robotJob)
    //                {
    //                    robotJob.WriteFlag = false;
    //                }

    //                EnqueueSave(robotJob);

    //                strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Delete CassetteSequenceNo({1}) JobSequenceNo({2}) CreateTime({3}) by BCSJob !", 
    //                                                                  eqpNo, robotJob.CstSeqNo, robotJob.JobSeqNo, robotJob.CreateTime.ToString());
    //                Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

    //                return true;
    //            }
    //            return false;

    //        }
    //        catch (Exception ex)
    //        {
    //            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
    //            return false;
    //        }
    //    }

    //    //public bool CreateRobotJob(string eqpNo,string runMode, string curStageNo, string cstSeq, string jobSeq)
    //    //{
    //    //    string strlog = string.Empty;

    //    //    try
    //    //    {
    //    //        if (cstSeq == "0")
    //    //        {
    //    //            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM]  CST Seq=(0) can not Create Robot Job!");
    //    //            Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

    //    //            return false;
    //    //        }
    //    //        if (jobSeq == "0")
    //    //        {
    //    //            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM]  JOB Seq=(0) can not Create Robot Job!");
    //    //            Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

    //    //            return false;
    //    //        }

    //    //        Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);
    //    //        if (curRobot == null)
    //    //        {

    //    //            strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Can not Finc Robot by EQPNo({1}) !", eqpNo, eqpNo);
    //    //            Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

    //    //            return false;
    //    //        }
             
    //    //        #region Create New RobotJob

    //    //        RobotJob robotJob = new RobotJob();

    //    //        robotJob.CstSeqNo = cstSeq;
    //    //        robotJob.JobSeqNo = jobSeq;
    //    //        robotJob.JobKey = string.Format("{0}_{1}");
    //    //        robotJob.CurJobStatus = eRobotJobStatus.WAIT_PROC;  //BCS呼叫此Function表示下貨完成等候抽片
    //    //        robotJob.CurStepNo = 1;  //從0開始
    //    //        robotJob.CurStageNo = curStageNo;
    //    //        //Set Robot Route by RunMode
    //    //        robotJob.RobotRoutes = ObjectManager.JobFlowManager.GetRobotRoute(curRobot.Data.ROBOTNAME, runMode);

    //    //        #endregion

    //    //        if (_entities.ContainsKey(robotJob.JobKey))
    //    //        {
    //    //            _entities.Remove(robotJob.JobKey);
    //    //        }

    //    //        _entities.Add(robotJob.JobKey, robotJob);
    //    //        EnqueueSave(robotJob);
    //    //        return true;
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
    //    //        return false;
    //    //    }

    //    //}

    //    private int _robotJobTimeOver = 7; //Defaule 7 

    //    /// <summary>
    //    /// get /Set Robot Job Time over Day;
    //    /// </summary>
    //    public int JobTimeOver
    //    {
    //        get { return _robotJobTimeOver; }
    //        set { _robotJobTimeOver = value; }
    //    }

    //    private Timer _deleteRobotJobtimer;//定时删除RobotJob的Timer 默认值24小时执行一次

    //}
}
