using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Xml;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using System.IO;

namespace UniAuto.UniBCS.EntityManager
{
    public class JobManager : EntityManager, IDataSource
    {
        private Dictionary<string, Job> _entities = new Dictionary<string, Job>();
        private Dictionary<string, ProductType> _productTypeData = new Dictionary<string, ProductType>();
        private int _jobTimeOver = 7; //Defaule 7 
        /// <summary>
        /// get /Set Job Time over Day;
        /// </summary>
        public int JobTimeOver 
        {
            get { return _jobTimeOver; }
            set { _jobTimeOver = value; }
        }

        ///Array PVD , ITO 尾部補上L3:AA PHOTO補上L7在L4之後
        //避免同樣的NODENO為key存不入，以L3_2存入，再以L3下Recipe to EQ
        public const string L3_Second = "L3_2"; 

        private Timer _timer;//定时删除Job的Timer 默认值24小时执行一次

        public override EntityManager.FILE_TYPE GetFileType()
        {
            return FILE_TYPE.BIN;
        }

        protected override string GetSelectHQL()
        {
            return string.Empty;
        }

        protected override Type GetTypeOfEntityData()
        {
            return null;
        }

        protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            Filenames = new List<string>();
            Filenames.Add("*.bin");
        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(Job);
        }

        protected override EntityFile NewEntityFile(string Filename)
        {

            string[] substrs = Filename.Split('_');
            if (substrs != null && substrs.Length == 2)
            {
                int cstSeqNo = Convert.ToInt32(substrs[0]);
                int slotNo = Convert.ToInt32(substrs[1]);
                return new Job(cstSeqNo, slotNo);
            }
            return null;
        }

        protected override void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles)
        {
            foreach (EntityFile entity_file in entityFiles)
            {
                try
                {
                    Job job_entity_file = entity_file as Job;
                    if (job_entity_file != null)
                    {
                        if (!_entities.ContainsKey(job_entity_file.JobKey))
                        {
                            _entities.Add(job_entity_file.JobKey, job_entity_file);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }

            }
            _timer = new Timer(12*3600 * 1000); //12H执行一次
            _timer.AutoReset = true;
            _timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            _timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {

                if (_entities != null && _entities.Count > 0)
                {
                    ParameterManager para = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                    IList<Job> jobs = null;
                    //Product Glass Delete
                    lock (_entities)
                    {
                        DateTime now = DateTime.Now;
                        // Add By Yangzhenteng20181016 For CUT断开线体CUT Remove的Job每日删除
                        if (BcServerName.Contains("CCCUT300")||BcServerName.Contains("CCCUT400")||BcServerName.Contains("CCCUT500")||BcServerName.Contains("CCCUT900")||
                            BcServerName.Contains("CCCUTA00")||BcServerName.Contains("CCCUTL00")||BcServerName.Contains("CCCUTJ00")||
                            BcServerName.Contains("CCCUTH00")||BcServerName.Contains("CCCUTM00")||BcServerName.Contains("CCCUTN00")||BcServerName.Contains("CCCUTP00")||
                            BcServerName.Contains("CCCUTE00")||BcServerName.Contains("CCCUTF00")||BcServerName.Contains("CCCUTG00"))
                        {
                            jobs = _entities.Values.Where(j => (j.LastUpdateTime.AddDays(para["ProdcutOverTimer"].GetInteger()).CompareTo(now) < 0)||
                                                               (j.CurrentEQPNo.Contains("L3") && (j.RemoveFlag == true || j.LastUpdateTime.AddDays(para["JobDataOverTimer"].GetInteger()).CompareTo(now) < 0))).ToList();
                        }
                        if (BcServerName.Contains("CCCUT600") || BcServerName.Contains("CCCUT700"))
                        {
                            jobs = _entities.Values.Where(j => (j.LastUpdateTime.AddDays(para["ProdcutOverTimer"].GetInteger()).CompareTo(now) < 0) ||
                                                               (j.CurrentEQPNo.Contains("L3") && (j.RemoveFlag == true && j.LastUpdateTime.AddDays(para["JobDataOverTimer"].GetInteger()).CompareTo(now) < 0))).ToList();
                        }
                        else if (BcServerName.Contains("CCNRD"))
                        {
                            jobs = _entities.Values.Where(j => (j.LastUpdateTime.AddDays(para["ProdcutOverTimer"].GetInteger()).CompareTo(now) < 0)||
                                                               (j.CurrentEQPNo.Contains("L6") && (j.RemoveFlag == true || j.LastUpdateTime.AddDays(para["JobDataOverTimer"].GetInteger()).CompareTo(now) < 0))).ToList();
                        }
                        else if (BcServerName.Contains("CCCUTK00") || BcServerName.Contains("CCCUT100") || BcServerName.Contains("CCCUT200") || BcServerName.Contains("CCCUT800") || BcServerName.Contains("CCCUTB00"))
                        {
                            jobs = _entities.Values.Where(j => (j.LastUpdateTime.AddDays(para["ProdcutOverTimer"].GetInteger()).CompareTo(now) < 0) ||
                                                               (j.CurrentEQPNo.Contains("L4") && (j.RemoveFlag == true || j.LastUpdateTime.AddDays(para["JobDataOverTimer"].GetInteger()).CompareTo(now) < 0))).ToList();
                        }
                        else if (BcServerName.Contains("CCCUTC00") || BcServerName.Contains("CCCUTD00"))
                        {
                            jobs = _entities.Values.Where(j => (j.LastUpdateTime.AddDays(para["ProdcutOverTimer"].GetInteger()).CompareTo(now) < 0) ||
                                                               (j.CurrentEQPNo.Contains("L7") && (j.RemoveFlag == true || j.LastUpdateTime.AddDays(para["JobDataOverTimer"].GetInteger()).CompareTo(now) < 0))).ToList();
                        }
                        else if (BcServerName.Contains("CCNLS"))
                        {
                            jobs = _entities.Values.Where(j => (j.LastUpdateTime.AddDays(para["ProdcutOverTimer"].GetInteger()).CompareTo(now) < 0)||
                                                               (j.CurrentEQPNo.Contains("L2") && (j.RemoveFlag == true || j.LastUpdateTime.AddDays(para["JobDataOverTimer"].GetInteger()).CompareTo(now) < 0))).ToList();
                        }
                        else if (BcServerName.Contains("CCPOL700") || BcServerName.Contains("CCPOL800") || BcServerName.Contains("CCPOL900") || BcServerName.Contains("CCPOLJ00") || BcServerName.Contains("CCPOLK00"))
                        {
                            jobs = _entities.Values.Where(j => (j.LastUpdateTime.AddDays(para["ProdcutOverTimer"].GetInteger()).CompareTo(now) < 0) ||
                                                               (j.CurrentEQPNo.Contains("L6") && (j.RemoveFlag == true || j.LastUpdateTime.AddDays(para["JobDataOverTimer"].GetInteger()).CompareTo(now) < 0))).ToList();
                        }
                        else if (BcServerName.Contains("CCPOLQ00"))
                        {
                            jobs = _entities.Values.Where(j => (j.LastUpdateTime.AddDays(para["ProdcutOverTimer"].GetInteger()).CompareTo(now) < 0) ||
                                       (j.CurrentEQPNo.Contains("L4") && (j.RemoveFlag == true || j.LastUpdateTime.AddDays(para["JobDataOverTimer"].GetInteger()).CompareTo(now) < 0))).ToList();
                        }
                        else if (BcServerName.Contains("CCPOL400") || BcServerName.Contains("CCPOLE00"))
                        {
                            jobs = _entities.Values.Where(j => (j.LastUpdateTime.AddDays(para["ProdcutOverTimer"].GetInteger()).CompareTo(now) < 0) ||
                                       (j.CurrentEQPNo.Contains("L7") && (j.RemoveFlag == true || j.LastUpdateTime.AddDays(para["JobDataOverTimer"].GetInteger()).CompareTo(now) < 0))).ToList();
                        }

                        else
                        {
                            jobs = _entities.Values.Where(job => job.LastUpdateTime.AddDays(para["ProdcutOverTimer"].GetInteger()).CompareTo(now) < 0
                                    && (job.JobType == eJobType.CF || job.JobType == eJobType.TFT) && int.Parse(job.CassetteSequenceNo) < 60000).ToList(); //qiumin modif 20170118 don't delete CF daily check glass
                        }                                        
                    }
                    if (jobs != null)
                    {
                        this.DeleteJobs(jobs);
                        //Delete File Data
                        List<Line> line=ObjectManager.LineManager.GetLines();
                        Workbench.Instance.Invoke("CassetteService", "DeleteFileData", new object[] { line[0], jobs });
                    }

                    //Dummy Glass Delete
                    lock (_entities)
                    {
                        DateTime now = DateTime.Now;
                        jobs = _entities.Values.Where(job => job.LastUpdateTime.AddDays(para["DummyOverTimer"].GetInteger()).CompareTo(now) < 0
                                                        && (job.JobType == eJobType.DM || job.JobType == eJobType.TK || job.JobType == eJobType.TR
                                                        || job.JobType == eJobType.METAL1 || job.JobType == eJobType.ITO || job.JobType == eJobType.NIP)).ToList();//sy modify 20160824 add other Dummy glasses)).ToList();
                    }
                    if (jobs != null)
                    {
                        this.DeleteJobs(jobs);
                        //Delete File Data
                        List<Line> line = ObjectManager.LineManager.GetLines();
                        Workbench.Instance.Invoke("CassetteService", "DeleteFileData", new object[] { line[0], jobs });
                    }

                    //UV Mask 保留60天以上再砍
                    lock (_entities)
                    {
                        DateTime now = DateTime.Now;
                        jobs = _entities.Values.Where(job => job.LastUpdateTime.AddDays(para["UVMaskOverTimer"].GetInteger()).CompareTo(now) < 0 
                                                        && job.JobType == eJobType.UV).ToList();
                    }
                    if (jobs != null)
                    {
                        this.DeleteJobs(jobs);
                        //Delete File Data
                        List<Line> line = ObjectManager.LineManager.GetLines();
                        Workbench.Instance.Invoke("CassetteService", "DeleteFileData", new object[] { line[0], jobs });
                    }
                }

            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 將EntityFile放入Queue, 由Thread存檔
        /// </summary>
        /// <param name="file">JobEntityFile(job.File)</param>
        public override void EnqueueSave(EntityFile file)
        {
            if (file is Job)
            {
                Job job = file as Job;

                if (job.CassetteSequenceNo == "0")
                    return;
                if (job.JobSequenceNo == "0")
                    return;

                lock (job)
                {
                    job.LastUpdateTime = DateTime.Now;
                }

                string job_no = string.Format("{0}_{1}", job.CassetteSequenceNo, job.JobSequenceNo);
                //string fname = string.Format("{0}.bin", job_no);
                string fname = string.Format("{0}.{1}", job_no,GetFileExtension());
                file.SetFilename(fname);
                base.EnqueueSave(file);
            }
        }

        public void AddJobs(IList<Job> jobs)
        {
            if (jobs == null) return;

            foreach (Job j in jobs)
            {
                try
                {
                    lock (_entities)
                    {
                        if (_entities.ContainsKey(j.JobKey))
                        {
                            _entities.Remove(j.JobKey);
                        }
                        _entities.Add(j.JobKey, j);
                        EnqueueSave(j);
                    }
                }
                catch (Exception ex)
                {
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
        }
        /// <summary>
        /// 將Job加入Dictionary
        /// </summary>
        /// <param name="Job">若Job Key有重複, 會throw exception</param>
        public void AddJob(Job job)
        {
            if (job.CassetteSequenceNo == "0")
                return;
            if (job.JobSequenceNo == "0")
                return;

            lock (_entities)
            {
                if (_entities.ContainsKey(job.JobKey))
                {
                    _entities.Remove(job.JobKey);
                    DeleteJob(job);
                }
                _entities.Add(job.JobKey, job);
                EnqueueSave(job);
            }
        }
        /// <summary>
        /// 取得line Job 以List 方式傳回
        /// </summary>
        /// <returns>Job List</returns>
        public IList<Job> GetJobs()
        {
            IList<Job> ret = null;
            lock (_entities)
            {
                ret = _entities.Values.ToList();
            }
            return ret;
        }

        public bool DeleteJob(Job job)
        {
            try
            {
                if (BcServerName.Contains("CCCUT100") || BcServerName.Contains("CCCUT200") || BcServerName.Contains("CCCUT300") || BcServerName.Contains("CCCUT400") || BcServerName.Contains("CCCUT500") ||
                    BcServerName.Contains("CCCUT600") || BcServerName.Contains("CCCUT700") || BcServerName.Contains("CCCUT800") || BcServerName.Contains("CCCUT900") || BcServerName.Contains("CCCUTA00") ||
                    BcServerName.Contains("CCCUTB00") || BcServerName.Contains("CCCUTK00") || BcServerName.Contains("CCCUTL00") || BcServerName.Contains("CCCUTJ00") || BcServerName.Contains("CCCUTH00") || 
                    BcServerName.Contains("CCCUTC00") || BcServerName.Contains("CCCUTD00") || BcServerName.Contains("CCCUTE00") || BcServerName.Contains("CCCUTF00")|| BcServerName.Contains("CCCUTG00")|| 
                    BcServerName.Contains("CCCUTM00") || BcServerName.Contains("CCCUTN00") || BcServerName.Contains("CCCUTP00") || BcServerName.Contains("CCNLS") || BcServerName.Contains("CCNRD")||
                    BcServerName.Contains("CCPOL700") || BcServerName.Contains("CCPOL800") || BcServerName.Contains("CCPOL900") || BcServerName.Contains("CCPOLJ00") ||
                    BcServerName.Contains("CCPOLK00") || BcServerName.Contains("CCPOLQ00"))
                {
                    System.Threading.Thread.Sleep(200);//Add By Yangzhenteng For CUT/NRD Job Data Delete;
                }
                else
                { }
                if (_entities.ContainsKey(job.JobKey))
                {
                    lock (_entities)
                    {
                        _entities.Remove(job.JobKey);
                    }
                    lock (job)
                    {
                        job.WriteFlag = false;
                    }

                    EnqueueSave(job);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        public void DeleteJobs(IList<Job> jobs)
        {
            if (jobs == null) return;
            foreach (Job j in jobs)
            {
                DeleteJob(j);
            }
        }

        public bool MoveJob(string sourceFile, string targetPath, Job job)
        {//20161031 sy add Move job, Delete wip
            try
            {
                if (_entities.ContainsKey(job.JobKey))
                {                                      
                    //string sourceFile = Path.Combine(FilePath, sourceSubPath);
                    string fileName = job.JobKey+ ".bin";
                    if (!Directory.Exists(sourceFile))
                    {
                        return false;
                    }
                    //20161110 sy modify 手投sequence >= 5000 不刪除空資料夾待上相同sequence超過指定天數處理，目標資料夾固定 5000
                    string descFile = Path.Combine(targetPath, DateTime.Now.ToString("yyyyMMdd"), (int.Parse(job.CassetteSequenceNo) >= 50000 ? "50000" : job.JobKey.Split('_')[0]));
                    if (!Directory.Exists(descFile))
                    {
                        Directory.CreateDirectory(descFile);
                    }
                    string file = Path.Combine(sourceFile, fileName);
                    if (File.Exists(file))
                    {
                        if (File.Exists(descFile + "\\" + fileName)) File.Delete(descFile + "\\" + fileName);//目标文件存在时，先删除，再Move
                        //File.Delete(file);
                        File.Move(file, descFile+"\\" + fileName);
                    }
                    string[] files = Directory.GetFiles(sourceFile);
                    if (files.Length == 0)
                    {
                        Directory.Delete(sourceFile, true);
                    }
                    lock (_entities)
                    {
                        _entities.Remove(job.JobKey);
                    }
                    //lock (job) //20161104 sy modify
                    //{
                    //    job.WriteFlag = false;
                    //}

                    //EnqueueSave(job);  
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        public void MoveJobs(Line line, IList<Job> jobs ,int delayTime)
        {//20161031 sy add Move job, Delete wip
            if (jobs == null) return;
            string sourceFile = string.Format("..\\Data\\{0}\\Job\\", line.Data.SERVERNAME);
            string targetPath = string.Format("D:\\UnicomLog\\{0}\\Job\\", line.Data.SERVERNAME);
            foreach (Job j in jobs)
            {
                MoveJob(sourceFile, targetPath, j);
                if (delayTime != 0)
                    System.Threading.Thread.Sleep(delayTime);//20161115 sy add 
            }
        }

        /// <summary>
        /// 取得line Job 以List 方式傳回
        /// </summary>
        /// <returns>Job List</returns>
        public IList<Job> GetJobs(string cstSeqNo)
        {
            IList<Job> ret = null;
            lock (_entities)
            {
                ret = _entities.Values.Where(w => w.CassetteSequenceNo == cstSeqNo).ToList();
            }
            return ret;
        }

        /// <summary>
        /// 以Job No Work No (Cassette Sequence No + Job Sequence No)取得Job(WIP)
        /// </summary>
        /// <param name="cassetteSequenceNo">Cassette SEQ No</param>
        /// <param name="jobSequenceNo">Job SEQ No</param>
        /// <returns>Job</returns>
        public Job GetJob(string cassetteSequenceNo, string jobSequenceNo)
        {
            string job_no = string.Format("{0}_{1}", cassetteSequenceNo, jobSequenceNo);
            Job ret = null;
            lock (_entities)
            {
                if (_entities.ContainsKey(job_no))
                {
                    ret = _entities[job_no];
                }
            }
            return ret;
        }

        /// <summary>
        /// 以x秒内UnloadingPort中的最新1片取得Job(WIP)
        /// </summary>
        /// <param name="seconds">seconds</param>
        /// <returns>Job</returns>
        public Job GetJob(int _sec)
        {
            Job ret = null;
            lock (_entities)
            {
                ret = _entities.Values.Where(j => j.LastUpdateTime.AddSeconds(_sec) > DateTime.Now).OrderByDescending(j => j.LastUpdateTime).First(j => !string.IsNullOrEmpty(j.ToCstID));
            }
            return ret;
        }

        /// <summary>
        /// 以Glass or Chip or Mask or Cut ID來取得WIP(Job)
        /// </summary>
        /// <param name="GlassChipMaskBlockID">ID</param>
        /// <returns>Job</returns>
        public Job GetJob(string jobID)
        {
            try
            {
                lock (_entities)
                {
                    //20150417 cy:先以wip的create time做排序,避免取到舊資料
                    //Job ret = _entities.Values.FirstOrDefault(w => w.GlassChipMaskBlockID == jobID);
                    Job ret = _entities.Values.OrderByDescending(w => w.CreateTime).FirstOrDefault(j => j.GlassChipMaskBlockID == jobID); //.Select(w => w.GlassChipMaskBlockID == jobID)
                    if (ret != null)
                    {
                        return ret;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                //20170823 huangjiayin: 不能在catch内部抛异常，否则started无法上报MES
                //Debug.Print(ex.Message);
                //throw new Exception(string.Format("Job ID=[{0}] is already Exist", jobID));
                NLogManager.Logger.LogErrorWrite(this.LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        /// <summary>
        /// 以Glass or Chip or Mask or Cut ID來取得WIP(Job: SubStrateType=Block)
        /// </summary>
        /// <param name="GlassChipMaskBlockID">ID</param>
        /// <returns>Job</returns>
        public Job GetBlockJob(string jobID)
        {
            try
            {
                lock (_entities)
                {
                    //20150417 cy:先以wip的create time做排序,避免取到舊資料
                    //Job ret = _entities.Values.FirstOrDefault(w => w.GlassChipMaskBlockID == jobID);
                    Job ret = _entities.Values.OrderByDescending(w => w.CreateTime).FirstOrDefault(j => j.GlassChipMaskBlockID == jobID && j.SubstrateType==eSubstrateType.Block); //add by huangjiayin: 20170425
                    if (ret != null)
                    {
                        return ret;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                throw new Exception(string.Format("Job ID=[{0}] is already Exist", jobID));
            }
        }

        /// <summary>
        /// Get JobID by EQPNO
        /// </summary>
        /// <param name="eqpNo">Equipment No</param>
        /// <returns>Glass ID</returns>
        public string GetJobIDbyEQPNO(string eqpNo)
        {
            try
            {               
                Job ret = null;
                DateTime _startTime = new DateTime();
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", eqpNo));

                lock (_entities)
                {
                    #region Add by Kasim 20150330 按CSOT要求，改成取出最後進機台的玻璃。
                    /*
                    DateTime.Compare(t1,t2)比较两个日期大小，排前面的小，排在后面的大，比如：2011-2-1就小于2012-3-2
                    返回值小于零：  t1 小于 t2。 
                    返回值等于零 ： t1 等于 t2。 
                    返回值大于零：  t1 大于 t2。 
                    */
                    foreach (Job job in _entities.Values)
                    {
                        foreach (ProcessFlow processFlow in job.JobProcessFlows.Values)
                        {                           
                            if (processFlow.MachineName == eqp.Data.NODEID)
                            {                                
                                if (ret == null)
                                {
                                    _startTime = processFlow.StartTime;
                                    ret = job;
                                }
                                else
                                {
                                    if (DateTime.Compare(_startTime, processFlow.StartTime) < 0)
                                    {
                                        _startTime = processFlow.StartTime;
                                        ret = new Job();
                                        ret = job;
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    //Job ret = _entities.Values.FirstOrDefault(w => w.CurrentEQPNo == eqpNo);
                    if (ret != null)
                    {
                        return ret.GlassChipMaskBlockID;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                throw new Exception(string.Format("Job isn't Exist {0}", eqpNo));
            }

        }

        /// <summary>
        /// Get JobID by EQPList
        /// </summary>
        /// <param name="eqpNo">Equipment No</param>
        /// <returns>Glass ID</returns>
        public string GetJobIDbyEQPList(IList<Equipment> EQPList)
        {
            try
            {
                Job ret = null;
                DateTime _startTime = new DateTime();
                
                lock (_entities)
                {
                    #region Add by Kasim 20150330 按CSOT要求，改成取出最後進機台的玻璃。
                    /*
                    DateTime.Compare(t1,t2)比较两个日期大小，排前面的小，排在后面的大，比如：2011-2-1就小于2012-3-2
                    返回值小于零：  t1 小于 t2。 
                    返回值等于零 ： t1 等于 t2。 
                    返回值大于零：  t1 大于 t2。 
                    */
                    foreach (Job job in _entities.Values)
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(job.GlassChipMaskBlockID, "^[0-9a-zA-Z]*$")) continue;//20161115 sy modify
                        foreach (Equipment eqp in EQPList)
	                    {
                            if (job.CurrentEQPNo == eqp.Data.NODENO)
                            {
                                foreach (ProcessFlow processFlow in job.JobProcessFlows.Values)
                                {
                                    if (processFlow.MachineName == eqp.Data.NODEID)
                                    {
                                        if (ret == null)
                                        {
                                            _startTime = processFlow.StartTime;
                                            ret = job;
                                        }
                                        else
                                        {
                                            if (DateTime.Compare(_startTime, processFlow.StartTime) < 0)
                                            {
                                                _startTime = processFlow.StartTime;
                                                ret = new Job();
                                                ret = job;
                                            }
                                        }
                                    }
                                }
                            }
	                    }                       
                    }
                    #endregion

                    //Job ret = _entities.Values.FirstOrDefault(w => w.CurrentEQPNo == eqpNo);
                    if (ret != null)
                    {
                        return ret.GlassChipMaskBlockID;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                throw new Exception(string.Format("Job isn't Exist {0}", ""));
            }

        }

        /// <summary>
        /// Get Jobs by EQPList
        /// </summary>
        /// <param name="eqpNoList">Equipment No</param>
        /// <returns>jobs</returns>
        public List<Job> GetJobsbyEQPList(List<string> EQPNoList) //Add By Huangjiayin 2017/12/20 For PIL
        {
            List<Job> rtn_jobs = new List<Job>();
            try
            {
                lock (_entities)
                {
                    rtn_jobs = _entities.Values.Where(w => EQPNoList.Contains(w.CurrentEQPNo)&&!w.RemoveFlag).ToList<Job>();//Remove掉的不抓，只抓当前机台WIP
                }
                return rtn_jobs;
            }
            catch (Exception ex)
            {
                return null;
            }
 
        }
        //Add By Wangshengjun20191030
        /// <summary>
        /// Get Jobs by EQPList
        /// </summary>
        /// <param name="eqpNoList">Equipment No</param>
        /// <returns>jobs</returns>
        public List<Job> GetSpecialJobsbyEQPList(List<string> EQPNoList) 
        {
            List<Job> rtn_jobs = new List<Job>();
            try
            {
                lock (_entities)
                {
                    rtn_jobs = _entities.Values.Where(w => EQPNoList.Contains(w.CurrentEQPNo) &&w.TrackingData.Equals(new string('0',32))&&!w.RemoveFlag).ToList<Job>();//Remove掉的不抓，只抓当前机台WIP
                   // rtn_jobs = _entities.Values.Where(w => EQPNoList.Contains(w.CurrentEQPNo) && !w.RemoveFlag).ToList<Job>();//Remove掉的不抓，只抓当前机台WIP
                }
                return rtn_jobs;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public int GetJobCount()
        {
            try
            {
                return _entities.Count;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return 0;
            }
        }

        public void RemoveJobByUI(string eqpNo = "", string cstSeq = "", string slotNo = "")
        {
            try
            {
                List<Job> jobs = GetJobs() as List<Job>;
                if (!string.IsNullOrEmpty(eqpNo) && jobs != null)
                {
                    jobs = jobs.Where(w => w.CurrentEQPNo == eqpNo && w.RemoveFlag == false).ToList();
                }
                if (!string.IsNullOrEmpty(cstSeq) && jobs != null)
                {
                    jobs = jobs.Where(w => w.CassetteSequenceNo == cstSeq).ToList();
                }
                if (!string.IsNullOrEmpty(slotNo) && jobs != null)
                {
                    jobs = jobs.Where(w => w.JobSequenceNo == slotNo).ToList();
                }
                if (jobs != null)
                {
                    foreach (Job job in jobs)
                    {
                        job.RemoveFlag = true;
                        NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Remove Job wiht BCS UI CST_SEQNO=[{0}] JOB_SEQNO=[{1}] GLASS_ID=[{2}]", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID));
                    }
                }
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        
        }

        public void RecoveryJobByUI(string eqpNo = "", string cstSeq = "", string slotNo = "")
        {
            
            try
            {
                List<Job> jobs = GetJobs() as List<Job>;
                if (!string.IsNullOrEmpty(eqpNo) && jobs != null)
                {
                    jobs = jobs.Where(w => w.CurrentEQPNo == eqpNo && w.RemoveFlag == false).ToList();
                }
                if (!string.IsNullOrEmpty(cstSeq) && jobs != null)
                {
                    jobs = jobs.Where(w => w.CassetteSequenceNo == cstSeq).ToList();
                }
                if (!string.IsNullOrEmpty(slotNo) && jobs != null)
                {
                    jobs = jobs.Where(w => w.JobSequenceNo == slotNo).ToList();
                }
                if (jobs != null)
                {
                    foreach (Job job in jobs)
                    {
                        job.RemoveFlag = false;
                        
                        NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Remove Job wiht BCS UI CST_SEQNO=[{0}] JOB_SEQNO=[{1}] GLASS_ID=[{2}]", job.CassetteSequenceNo, job.JobSequenceNo, job.GlassChipMaskBlockID));
                    }
                }
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite("LoggerName", this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        
        }
        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("JobManager");
            //entityNames.Add("ProductType");
            return entityNames;
        }

        public System.Data.DataTable GetDataTable(string entityName)
        {
            switch (entityName)
            {
                case "JobManager":
                    return GetJobManagerDataTable();
                case "ProductType":
                    return GetProductTypeDataTable();
                default:
                    return null;

            }

        }

        private DataTable GetProductTypeDataTable()
        {
            try
            {
                DataTable dt = new DataTable();
                ProductType file = new ProductType();
                DataTableHelp.DataTableAppendColumn(file, dt);

                foreach (ProductType product in _productTypeData.Values)
                {
                    DataRow dr = dt.NewRow();

                    DataTableHelp.DataRowAssignValue(product, dr);
                    dt.Rows.Add(dr);
                }
                return dt;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        private DataTable GetJobManagerDataTable()
        {
            try
            {
                DataTable dt = new DataTable();
                Job file = new Job();
                DataTableHelp.DataTableAppendColumn(file, dt);
                dt.Columns.Add("No");//增加一个序列号
                IList<Job> jobs = GetJobs();
                int i = 1;
                foreach (Job job in jobs)
                {
                    DataRow dr = dt.NewRow();
                    dr["No"] = i++;
                    DataTableHelp.DataRowAssignValue(job, dr);
                    dt.Rows.Add(dr);
                }
                return dt;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        public void ConvertXMLToObject(object obj, XmlNode node)
        {
            try
            {
                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();//BindingFlags.Public);
                foreach (PropertyInfo info in properties)
                {
                    try
                    {
                        if (info.PropertyType != typeof(string)) continue;
                        if (info.PropertyType != typeof(List<>) && node[info.Name] != null && node[info.Name].InnerText != null)
                        {
                            info.SetValue(obj, node[info.Name].InnerText, null);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Property Name=[{0}], getValue error.\r", info.Name) + ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void HoldEventRecord(Job job, HoldInfo data)
        {
            if (job == null || data == null) return;

            lock (job)
            {
                List<HoldInfo> jobs = (from j in job.HoldInforList where j.NodeID == data.NodeID && j.UnitID == data.UnitID  && j.HoldReason == data.HoldReason select j).ToList<HoldInfo>();  
                if (jobs.Count()== 0)   //modify by bruce 20160204 避免機台相同Unit重複上報相同 hold reason
                    job.HoldInforList.Add(data);
            }
            EnqueueSave(job);
        }

        public string Covert_PPID_To_MES_FORMAT(string ppid)
        {
            if (ppid.Trim() == string.Empty)
                return string.Empty;

            string data = string.Empty;
            IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

            for (int i = 0; i < eqps.Count(); i++)
            {

                if (!string.IsNullOrEmpty(data)) data += ";";
                data += ppid.Substring(eqps[i].Data.RECIPEIDX, eqps[i].Data.RECIPELEN);
            }
            return data;
        }

        #region Create Job Data時, 會使用到的Method
        /// <summary>
        /// 分析MES PPID
        /// </summary>
        public bool AnalysisMesPPID_AC(XmlNode productNode, Line line, Port port, string processType, ref IList<RecipeCheckInfo> idRecipeInfos,
            ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string mesppid, out string errMsg)
        {
            mesppid = string.Empty;
            errMsg = string.Empty;
            ppid = string.Empty;
            try
            {

                string eqPPID = string.Empty;
                if (line.Data.JOBDATALINETYPE != eJobDataLineType.CF.UNPACK)
                {
                    // 如果OPI的部份有值時, 使用OPI的值 
                    if (string.IsNullOrEmpty(productNode[keyHost.OPI_PPID].InnerText.Trim()))
                        eqPPID = productNode[keyHost.PPID].InnerText;
                    else
                        eqPPID = productNode[keyHost.OPI_PPID].InnerText;

                    if (string.IsNullOrEmpty(eqPPID.Trim())) return true;
                }
                else
                {
                    if (line.File.HostMode == eHostMode.OFFLINE)
                    {
                        eqPPID = productNode[keyHost.OPI_PPID].InnerText;
                    }
                    else
                    {
                        if (port.File.Type == ePortType.BothPort || port.File.Type == ePortType.UnloadingPort)// Unloader use Product PPID
                        {
                            if (string.IsNullOrEmpty(productNode[keyHost.OPI_PPID].InnerText.Trim()))
                                eqPPID = productNode[keyHost.PPID].InnerText;
                            else
                                eqPPID = productNode[keyHost.OPI_PPID].InnerText;

                            if (string.IsNullOrEmpty(eqPPID.Trim())) return true;
                        }
                        else// Unpacker Use Body PPID
                        {
                            if (string.IsNullOrEmpty(productNode[keyHost.MESSAGE][keyHost.BODY][keyHost.OPI_PPID].InnerText.Trim()))
                                eqPPID = productNode[keyHost.MESSAGE][keyHost.BODY][keyHost.PPID].InnerText;
                            else
                                eqPPID = productNode[keyHost.MESSAGE][keyHost.BODY][keyHost.OPI_PPID].InnerText;
                        }
                    }
                }
                mesppid = eqPPID; //Keep MES Donwload PPID 不會有虛擬機台、跳號，隨時還要上報，不得更動

                //Watson Add 避免沒有要check的機台還加入Recipe Check，要下給機台的
                string eqFillnullPPID = string.Empty;
                string photo_EQP_PPID = string.Empty;
                Dictionary<string, string> EQP_RecipeNo = AddVirtualEQP_PPID(line, eqPPID, out eqFillnullPPID);

                if (eqFillnullPPID == "ALL_EQ_RECIPE_BYPASS")   // add by bruce 2016/05/16 
                {
                    errMsg = string.Format("{0} some EQ maybe CIM Off, All EQ Recipe byPass PPID {1}.", line.Data.LINEID, eqPPID);
                    return false;
                }

                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1} is Error!!", line.Data.LINEID, eqPPID);
                    return false;
                }

                string productRcpName = string.Empty;
                if (line.Data.JOBDATALINETYPE != eJobDataLineType.CF.UNPACK) //modify by yang 20161122  recipe要和recipe name对应
                {
                    if (line.File.HostMode == eHostMode.OFFLINE)
                    {
                        productRcpName = productNode[keyHost.PRODUCTRECIPENAME].InnerText;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(productNode[keyHost.OPI_PRODUCTRECIPENAME].InnerText))
                            productRcpName = productNode[keyHost.PRODUCTRECIPENAME].InnerText;
                        else
                            productRcpName = productNode[keyHost.OPI_PRODUCTRECIPENAME].InnerText;
                    }
                }
                else
                {
                    if (line.File.HostMode == eHostMode.OFFLINE)
                    {
                        productRcpName = productNode[keyHost.PRODUCTRECIPENAME].InnerText;
                    }
                    else
                    {
                        if (port.File.Type == ePortType.BothPort || port.File.Type == ePortType.UnloadingPort)// Unloader use Product PPID
                        {
                            productRcpName = productNode[keyHost.PRODUCTRECIPENAME].InnerText;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(productNode[keyHost.MESSAGE][keyHost.BODY][keyHost.OPI_PPID].InnerText.Trim()))
                                productRcpName = productNode[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                            else
                                productRcpName = productNode[keyHost.PRODUCTRECIPENAME].InnerText;
                        }
                    }
                }

                string eqpNo = string.Empty;
                foreach (string eqpno in EQP_RecipeNo.Keys)
                {
                    eqpNo = eqpno;
                    if (eqpno == L3_Second) //從自組的Recipe Dictionary 取出，可能會有L3_2
                    {
                        //ex: TBITO or TBPVD Line PPID:L2:AA;L3:BB;L4:CC;L3:DD 會有重複的EQP NO, 一定要做 Recipe Check)
                        eqpNo = "L3";
                        eqFillnullPPID += EQP_RecipeNo[eqpno];
                        NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("Array ITO or PVD Line PPID Type is Special  Spec!! Recipe is =[{0}]", EQP_RecipeNo[eqpno]));
                    }

                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        //ex: FCRPH Line 沒有機台L10, L15 and L20 不需要加入Recipe Check(不需丟給機台做Recipe Validate)
                        NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("CF Photo Line PPID Type is Special  Spec!! EQP No =[{0}] is not in DB, but must fill '00' in EQP PPID", eqpNo));
                        continue; //跨號機台不跳出!!
                    }

                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    // string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN); 

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpno])))
                    {
                        // Array TBPHL 曝光機為CANON且Product Process Type為MMG時, 要分兩筆給CANON
                        if (eqp.Data.NODEATTRIBUTE.Equals("CANON"))
                        {
                            idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno].Substring(0, 4)/* recipe.Substring(0, 4)*/, productRcpName));

                            if (processType.Equals("MMG"))
                            {
                                idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno].Substring(4, 4) /*recipe.Substring(4, 4)*/, productRcpName));
                            }
                        }
                        else if (eqp.Data.NODEATTRIBUTE.Equals("NIKON"))
                        {
                            //idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno].Substring(0, 4)/* recipe.Substring(0, 4)*/, productRcpName));
                            //idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno].Substring(0, 8)/* recipe.Substring(0, 4)*/, productRcpName));//NIKON SECS 会自己处理多出来的‘0000’ 2015-4-28 tom
                            idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno], productRcpName));//NIKON SECS 會補足四碼, MES超出四碼 SECS直接回NG 2015-9-18 cc.kuang
                        }
                        else
                        {
                            //idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno]/* recipe*/, productRcpName));
                            idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, ObjectManager.EquipmentManager.GetEQPID(eqpNo), 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno]/* recipe*/, productRcpName));
                        }
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpno])))
                    {
                        if (line.File.HostMode != eHostMode.OFFLINE)  //modify by yang 20161202
                        {
                          //  if (!string.IsNullOrEmpty(productNode[keyHost.OPI_PRODUCTRECIPENAME].InnerText))  // yang mark 20161214 不管recipe是否有做修改,都需要para check
                         //   {
                         //   }
                         //   else
                         //   {
                                // Array TBPHL 曝光機為CANON且Product Process Type為MMG時, 要分兩筆給CANON
                                if (eqp.Data.NODEATTRIBUTE.Equals("CANON"))
                                {
                                    paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno].Substring(0, 4)/*recipe.Substring(0, 4)*/, productRcpName));
                                    if (processType.Equals("MMG"))
                                    {
                                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno].Substring(0, 4)/* recipe.Substring(0, 4)*/, productRcpName));
                                    }
                                }
                                else if (eqp.Data.NODEATTRIBUTE.Equals("NIKON"))
                                {
                                    //paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno].Substring(0, 4)/*recipe.Substring(0, 4)*/, productRcpName));
                                    //paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno].Substring(0, 8)/*recipe.Substring(0, 4)*/, productRcpName)); //NIKON SECS 会自己处理多出来的‘0000’ 2015-4-28 tom
                                    paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno], productRcpName)); //NIKON SECS 會補足四碼, MES超出四碼 SECS直接回NG 2015-9-18 cc.kuang
                                }
                                else
                                {
                                    //paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno]/*recipe*/, productRcpName));
                                    paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, ObjectManager.EquipmentManager.GetEQPID(eqpNo), 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno]/*recipe*/, productRcpName));
                                }
                         //   }
                        }
                    }
                }

                //寫給機台的PPID不能有分號並且補足00
                ppid = eqFillnullPPID;

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 取得Cell Group Index
        /// </summary>
        public string M2P_GetCellGroupIndex(Line line, string groupID)
        {
            try
            {
                if (string.IsNullOrEmpty(groupID))
                    return "0";

                foreach (string key in line.File.CellGroupIndex.Keys)
                {
                    if (line.File.CellGroupIndex[key].GroupID == groupID)
                    {
                        line.File.CellGroupIndex[key].DateTime = DateTime.Now;
                        return key;
                    }
                }

                if (line.File.CellGroupIndex.Count >= 15)
                {
                    string tmpKey = string.Empty;
                    DateTime tmpTime = DateTime.Now;

                    foreach (string key in line.File.CellGroupIndex.Keys)
                    {
                        if (DateTime.Now.Subtract(line.File.CellGroupIndex[key].DateTime).TotalMilliseconds > DateTime.Now.Subtract(tmpTime).TotalMilliseconds)
                        {
                            tmpKey = key;
                            tmpTime = line.File.CellGroupIndex[key].DateTime;
                        }
                    }

                    if (tmpKey == string.Empty)
                        return "0";

                    line.File.CellGroupIndex[tmpKey].GroupID = groupID;
                    line.File.CellGroupIndex[tmpKey].DateTime = DateTime.Now;
                    return tmpKey;
                }
                else
                {
                    clsGroupIndex tmpGroupIndex = new clsGroupIndex();
                    tmpGroupIndex.GroupID = groupID;
                    tmpGroupIndex.DateTime = DateTime.Now;

                    for (int i = 1; i < 16; i++)
                    {
                        if (!line.File.CellGroupIndex.ContainsKey(i.ToString()))
                        {
                            line.File.CellGroupIndex.Add(i.ToString(), tmpGroupIndex);
                            return i.ToString();
                        }
                    }
                }

                return "0";
                //}

                //if (groupList.ContainsKey(groupID))
                //{
                //    return groupList[groupID].ToString();
                //}
                //else
                //{
                //    if (_entities.Count == 0)
                //    {
                //        if (groupList.Count > 0)
                //        {
                //            int max_nodeStackValue = 0;
                //            foreach (int val in groupList.Values)
                //            {
                //                int currentVal = val;
                //                if (currentVal > max_nodeStackValue)
                //                    max_nodeStackValue = currentVal;
                //            }
                //            max_nodeStackValue = max_nodeStackValue + 1;

                //            groupList.Add(groupID, max_nodeStackValue);
                //            return max_nodeStackValue.ToString();
                //        }

                //        groupList.Add(groupID, 1);
                //        return "1";
                //    }
                //    Job job = _entities.Values.FirstOrDefault(j => j.MesProduct.GROUPID == groupID);
                //    if (job != null)
                //    {
                //        groupList.Add(groupID, int.Parse(job.GroupIndex));
                //        return job.GroupIndex;
                //    }
                //    else//换GroupID的第一片
                //    {
                //        int maxGroupIndex = _entities.Values.Max(j => int.Parse(j.GroupIndex));

                //        int max_groupIDValue = 0;
                //        foreach (int val in groupList.Values)
                //        {
                //            int currentVal = val;
                //            max_groupIDValue = currentVal;
                //        }

                //        if (maxGroupIndex > max_groupIDValue)//换Group ID的一个片此处max_groupIDvalue=0
                //        {
                //            int retVal = maxGroupIndex + 1;
                //            if (retVal > 65535)
                //                retVal = 1;

                //            groupList.Add(groupID, retVal);

                //            return retVal.ToString();
                //        }
                //        else//同一个Lot的中两个不同的GroupID
                //        {
                //            int retVal = max_groupIDValue + 1;
                //            if (retVal > 65535)
                //                retVal = 1;

                //            groupList.Add(groupID, retVal);

                //            return retVal.ToString();
                //        }

                //    }
                //}
            }
            catch
            {
                return "0";
            }
        }

        /// <summary>
        /// 取得Cell Node Stack
        /// </summary>
        public string M2P_GetCellNodeStack(string nodeStack, Dictionary<string, int> nodeStackList)
        {
            try
            {
                if (string.IsNullOrEmpty(nodeStack))
                    return "0";
                if (nodeStackList.ContainsKey(nodeStack))
                {
                    return nodeStackList[nodeStack].ToString();
                }
                else
                {
                    if (_entities.Count == 0)
                    {
                        if (nodeStackList.Count > 0)
                        {
                            int max_nodeStackValue = 0;
                            foreach (int val in nodeStackList.Values)
                            {
                                int currentVal = val;
                                if (currentVal > max_nodeStackValue)
                                    max_nodeStackValue = currentVal;
                            }
                            max_nodeStackValue = max_nodeStackValue + 1;

                            nodeStackList.Add(nodeStack, max_nodeStackValue);
                            return max_nodeStackValue.ToString();
                        }

                        nodeStackList.Add(nodeStack, 1);
                        return "1";
                    }

                    Job job = _entities.Values.FirstOrDefault(j => j.MesCstBody.LOTLIST[0].NODESTACK == nodeStack);
                    if (job != null)
                    {
                        nodeStackList.Add(nodeStack, int.Parse(job.CellSpecial.NodeStack));
                        return job.CellSpecial.NodeStack;
                    }
                    else//换Node Stack的第一片
                    {
                        int maxNodeStack = _entities.Values.Max(j => int.Parse(j.CellSpecial.NodeStack));

                        int max_nodeStackValue = 0;
                        foreach (int val in nodeStackList.Values)
                        {
                            int currentVal = val;
                            max_nodeStackValue = currentVal;
                        }

                        if (maxNodeStack > max_nodeStackValue)//换Node Stack的一个片此处max_nodeStackValue=0
                        {
                            int retVal = maxNodeStack + 1;
                            if (retVal > 255)
                                retVal = 1;

                            nodeStackList.Add(nodeStack, retVal);

                            return retVal.ToString();
                        }
                        else//同一个Lot的中两个不同的Node Stack
                        {
                            int retVal = max_nodeStackValue + 1;
                            if (retVal > 255)
                                retVal = 1;

                            nodeStackList.Add(nodeStack, retVal);

                            return retVal.ToString();
                        }

                    }
                }
            }
            catch
            {
                return "0";
            }
        }

        public eSubstrateType M2P_GetSubstrateType(string data)
        {
            switch (data)
            {
                case "G": return eSubstrateType.Glass;
                case "P": return eSubstrateType.Chip;
                case "C": return eSubstrateType.Block;
                default: return eSubstrateType.Glass;  //Jun Modify 20150411 登京老闆說默認值是Glass不是Cassette
                //default: return eSubstrateType.Cassette; //默认值填写成Cassette 20150408 Tom
                //default: return eSubstrateType.Cassette;
            }
        }

        public eJobType M2P_GetJobType(string data)
        {
            // add just for array use 2016/03/17 cc.kuang
            Line line = ObjectManager.LineManager.GetLines()[0];
            eFabType fabType;
            Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
            if (fabType == eFabType.ARRAY)
            {
                if (line.Data.LINETYPE == eLineType.ARRAY.DRY_ICD || line.Data.LINETYPE == eLineType.ARRAY.DRY_YAC ||
                    line.Data.LINETYPE == eLineType.ARRAY.CVD_AKT || line.Data.LINETYPE == eLineType.ARRAY.CVD_ULVAC ||
                    line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC)
                {
                    return eJobType.TFT;
                }
                else
                {
                    if (data == eMES_PRODUCT_TYPE.NORMAL_TFT_PRODUCT || data == eMES_PRODUCT_TYPE.MQC_DUMMY)
                    {
                        return eJobType.TFT; 
                    }
                    else if (data == eMES_PRODUCT_TYPE.NORMAL_CF_PRODUCT)
                    {
                        return eJobType.CF; 
                    }
                    else
                    {
                        return eJobType.Unknown;
                    }
                }
            }
            if (fabType == eFabType.CELL)//sy add by CELL 20160322
            {
                switch (data)
                {
                    case eMES_PRODUCT_TYPE.NORMAL_TFT_PRODUCT: return eJobType.TFT;
                    case eMES_PRODUCT_TYPE.NORMAL_CF_PRODUCT: return eJobType.CF;
                    case eMES_PRODUCT_TYPE.THROUGH_DUMMY: return eJobType.TR;
                    case eMES_PRODUCT_TYPE.THICKNESS_DUMMY: return eJobType.TK;
                    case eMES_PRODUCT_TYPE.UV_MASK: return eJobType.UV;
                    case eMES_PRODUCT_TYPE.ITO_DUMMY: return eJobType.ITO;
                    case eMES_PRODUCT_TYPE.METAL1_DUMMY: return eJobType.METAL1;
                    case eMES_PRODUCT_TYPE.NIP_DUMMY: return eJobType.NIP;
                    case eMES_PRODUCT_TYPE.GENERAL_DUMMY:
                    case eMES_PRODUCT_TYPE.QC_DUMMY:
                    case eMES_PRODUCT_TYPE.MQC_DUMMY:
                    case eMES_PRODUCT_TYPE.BARE_DUMMY: return eJobType.DM;
                    default: return eJobType.Unknown;
                }
            }
            switch (data)
            {
                case eMES_PRODUCT_TYPE.NORMAL_TFT_PRODUCT: return eJobType.TFT; 
                case eMES_PRODUCT_TYPE.NORMAL_CF_PRODUCT: return eJobType.CF; 
                case eMES_PRODUCT_TYPE.THROUGH_DUMMY: return eJobType.TR; 
                case eMES_PRODUCT_TYPE.THICKNESS_DUMMY: return eJobType.TK; 
                case eMES_PRODUCT_TYPE.UV_MASK: return eJobType.UV;
                case eMES_PRODUCT_TYPE.GENERAL_DUMMY:
                case eMES_PRODUCT_TYPE.QC_DUMMY:
                case eMES_PRODUCT_TYPE.ITO_DUMMY:
                case eMES_PRODUCT_TYPE.MQC_DUMMY:
                case eMES_PRODUCT_TYPE.BARE_DUMMY: return eJobType.DM;
                case eMES_PRODUCT_TYPE.METAL1_DUMMY: return eJobType.METAL1;//sy add by MES 1.21 20151119
                case eMES_PRODUCT_TYPE.NIP_DUMMY: return eJobType.NIP;//sy add by MES 1.21 20151119
                default: return eJobType.Unknown;
            }
        }

        /// <summary>
        /// Array/CF OXR Information, MES Format轉成IO Format (4 bit)
        /// </summary>
        public string M2P_AC_OXRInformation(string data, int chipCount)
        {
            try
            {   // XRLOF
                string oxr = string.Empty;

                char[] mesOXRs = data.ToCharArray();

                for (int i = 0; i < chipCount; i++)
                {
                    if (i <= mesOXRs.Length)
                    {
                        switch (mesOXRs[i])
                        {
                            case 'R': oxr += "1"; break;
                            case 'L': oxr += "2"; break;
                            case 'O': oxr += "3"; break;
                            case 'F': oxr += "4"; break;
                            default: oxr += "0"; break;
                        }
                    }
                    else
                    {
                        oxr += "0";
                    }
                }
                return oxr;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// CELL OXR Information, MES Format轉成IO Format (2 bit)
        /// </summary>
        public string M2P_CELL_OXRInformation(string data, int chipCount)
        {
            try
            {   // XRLOF
                string oxr = string.Empty;

                char[] mesOXRs = data.ToCharArray();

                for (int i = 0; i < chipCount; i++)
                {
                    if (i <= mesOXRs.Length)
                    {
                        switch (mesOXRs[i])
                        {
                            case 'R': oxr += "1"; break;
                            case 'O': oxr += "2"; break;
                            default: oxr += "0"; break;
                        }
                    }
                    else
                    {
                        oxr += "0";
                    }
                }
                return oxr;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// CELL Panel OXR Information, MES Format轉成IO Format (4 bit)
        /// </summary>
        public string M2P_CELL_PanelOX2Bin(string data)
        {
            try
            {   // XRLOF
                string oxr = string.Empty;

                char[] mesOXRs = data.ToCharArray();

                for (int i = 0; i < mesOXRs.Length; i++)
                {      
                    switch (mesOXRs[i])
                        {
                            case 'O': oxr += "0000"; break;
                            case 'X': oxr += "1000"; break;
                            case 'A': oxr += "0100"; break;
                            case 'B': oxr += "1100"; break;
                            case 'C': oxr += "0010"; break;
                            case 'D': oxr += "1010"; break;
                            case 'E': oxr += "0110"; break;
                            case 'F': oxr += "1110"; break;
                            case 'G': oxr += "0001"; break;
                            case 'H': oxr += "1001"; break;
                            case 'I': oxr += "0101"; break;
                            case 'J': oxr += "1101"; break;
                            case 'K': oxr += "0011"; break;
                            case 'L': oxr += "1011"; break;
                            case 'M': oxr += "0111"; break;
                            case 'N': oxr += "1111"; break;
                            default: oxr += "0000"; break;
                        }                   
                }
                return oxr;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// CELL Panel OXR Information, MES Format轉成IO Format (4 bit)
        /// </summary>
        public string M2P_CELL_PanelOX2Int(string data)
        {
            try
            {   // XRLOF
                    switch (data)
                    {
                        case "O":return "0"; 
                        case "X": return "1"; 
                        case "A": return "2"; 
                        case "B": return "3"; 
                        case "C": return "4"; 
                        case "D": return "5"; 
                        case "E": return "6"; 
                        case "F": return "7"; 
                        case "G": return "8"; 
                        case "H": return "9";
                        case "I": return "10";
                        case "J": return "11";
                        case "K": return "12";
                        case "L": return "13";
                        case "M": return "14";
                        case "N": return "15"; 
                        default: return "0"; 
                    }              
            }
            catch
            {
                return "0";
            }
        }
        /// <summary>
        /// CELL Block OXR Information, MES Format轉成IO Format (4 bit)
        /// </summary>
        public string M2P_CELL_BlockOX2Bin(string data)
        {
            try
            {   // XRLOF
                string oxr = string.Empty;

                char[] mesOXRs = data.ToCharArray();

                for (int i = 0; i < mesOXRs.Length; i++)
                {
                    switch (mesOXRs[i])
                    {
                        case 'O': oxr += "0000"; break;
                        case 'C': oxr += "1000"; break;
                        case 'A': oxr += "0100"; break;
                        case 'X': oxr += "1100"; break;//20170425: huangjiayin add X for PCS
                        default: oxr += "0000"; break;
                    }
                }
                return oxr;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Array/CF OXR Information, IO Format轉成MES Format (4 bit)
        /// </summary>
        public string P2M_AC_OXRInformation(string data, int chipCount)
        {
            try
            {   // XRLOF
                string oxr = string.Empty;

                char[] ioOXRs = data.ToCharArray();

                for (int i = 0; i < chipCount; i++)
                {
                    if (i <= ioOXRs.Length)
                    {
                        switch (ioOXRs[i])
                        {
                            case '0': oxr += "X"; break;
                            case '1': oxr += "R"; break;
                            case '2': oxr += "L"; break;
                            case '3': oxr += "O"; break;
                            case '4': oxr += "F"; break;
                            default: oxr += " "; break;
                        }
                    }
                    else
                    {
                        oxr += " ";
                    }
                }
                return oxr;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// CELL OXR Information, IO Format轉成MES Format (2 bit)
        /// </summary>
        public string P2M_CELL_OXRInformation(string data, int chipCount)
        {
            try
            {   // XRLOF
                string oxr = string.Empty;

                char[] ioOXRs = data.ToCharArray();

                for (int i = 0; i < chipCount; i++)
                {
                    if (i <= ioOXRs.Length)
                    {
                        switch (ioOXRs[i])
                        {
                            case '0': oxr += "X"; break;
                            case '1': oxr += "R"; break;
                            case '2': oxr += "O"; break;
                            default: oxr += " "; break;
                        }
                    }
                    else
                    {
                        oxr += " ";
                    }
                }
                return oxr;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string P2M_CELL_PanelBin2OX(string data, int chipCount)
        {
            try
            {   // XRLOF
                string oxr = string.Empty;
                for (int chipNo = 1; chipNo <= chipCount; chipNo++)
                {
                    switch (data.Substring((chipNo - 1) * 4, 4))
                    {
                        case "0000": oxr += "O"; break;
                        case "1000": oxr += "X"; break;
                        case "0100": oxr += "A"; break;
                        case "1100": oxr += "B"; break;
                        case "0010": oxr += "C"; break;
                        case "1010": oxr += "D"; break;
                        case "0110": oxr += "E"; break;
                        case "1110": oxr += "F"; break;
                        case "0001": oxr += "G"; break;
                        case "1001": oxr += "H"; break; ;
                        case "0101": oxr += "I"; break;
                        case "1101": oxr += "J"; break;
                        case "0011": oxr += "K"; break;
                        case "1011": oxr += "L"; break;
                        case "0111": oxr += "M"; break; 
                        case "1111": oxr += "N"; break; 
                        default: oxr += "O"; break;
                    }
                }
                return oxr;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string P2M_CELL_PanelInt2OX(string data)
        {
            try
            {   // XRLOF
                switch (data)
                {
                    case "0": return "O";
                    case "1": return "X";
                    case "2": return "A";
                    case "3": return "B";
                    case "4": return "C";
                    case "5": return "D";
                    case "6": return "E";
                    case "7": return "F";
                    case "8": return "G";
                    case "9": return "H";
                    case "10": return "I";
                    case "11": return "J";
                    case "12": return "K";
                    case "13": return "L";
                    case "14": return "M";
                    case "15": return "N";
                    default: return "O";
                }
            }
            catch
            {
                return "O";
            }
        }

        public string P2M_CELL_BlockBin2OX(string data, int BlockCount)
        {
            try
            {   // XRLOF
                string oxr = string.Empty;
                for (int chipNo = 1; chipNo <= BlockCount; chipNo++)
                {
                    switch (data.Substring((chipNo - 1) * 4, 4))
                    {
                        case "0000": oxr += "O"; break;
                        case "1000": oxr += "C"; break;
                        case "0100": oxr += "A"; break;
                        default: oxr += "O"; break;
                    }
                }
                return oxr;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string P2M_CELL_BlockOXR_OX(string data)
        {
            try
            {   // XRLOF
                string oxr = string.Empty;
                for (int chipNo = 1; chipNo <= (data.Length/4); chipNo++)
                {
                    switch (data.Substring((chipNo - 1) * 4, 4))
                    {
                        case "0000": oxr += "O"; break;
                        case "0001": oxr += "C"; break;
                        case "0010": oxr += "A"; break;
                        default: oxr += "O"; break;
                    }
                }
                return oxr;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string P2M_GetVCRResult(eVCR_EVENT_RESULT value)
        {
            //VCR Result
            //OK      : VCR Read OK & HostID = ReadID
            //NG      : VCR Read Fail
            //PASS    : VCR Read Pass
            //UNMATCH : VCR Read OK & HostID <> ReadID
            switch (value)
            {
                case eVCR_EVENT_RESULT.READING_OK_MATCH_JOB: return "OK";
                case eVCR_EVENT_RESULT.READING_OK_MISMATCH_JOB:
                case eVCR_EVENT_RESULT.READING_FAIL_KEY_IN_MISMATCH_JOB: return "UNMATCH";
                case eVCR_EVENT_RESULT.READING_FAIL_KEY_IN_MATCH_JOB: return "NG";
                case eVCR_EVENT_RESULT.READING_FAIL_PASS: return "PASS";
                default: return "";
            }
        }

        public string P2M_GetJobType(eJobType data)//sy add by MES 1.21 20151119
        {
            switch (data)
            {
                case eJobType.TFT: return eMES_PRODUCT_TYPE.NORMAL_TFT_PRODUCT;
                case eJobType.CF: return eMES_PRODUCT_TYPE.NORMAL_CF_PRODUCT;
                case eJobType.TR: return eMES_PRODUCT_TYPE.THROUGH_DUMMY;
                case eJobType.TK: return eMES_PRODUCT_TYPE.THICKNESS_DUMMY;
                case eJobType.UV: return eMES_PRODUCT_TYPE.UV_MASK;
                case eJobType.DM: return data.ToString();
                case eJobType.METAL1: return eMES_PRODUCT_TYPE.METAL1_DUMMY;
                case eJobType.ITO: return eMES_PRODUCT_TYPE.ITO_DUMMY;
                case eJobType.NIP: return eMES_PRODUCT_TYPE.NIP_DUMMY;
                default: return data.ToString();
            }
        }
        /// <summary>
        ///  New Job Create Mes 層，避免直接用時Exception
        /// </summary>
        /// <param name="job">Job Entity</param>
        public void NewJobCreateMESDataEmpty(Job job)
        {
            try
            {
                if (job == null) return;
                #region MES Cassette
                if (job.MesCstBody.LOTLIST.Count <= 0)
                {
                    LOTc lot = new LOTc();
                    job.MesCstBody.LOTLIST.Add(lot);
                    //MES Cassette LOT
                    //BOMc bom = new BOMc();
                    //job.MesCstBody.LOTLIST[0].BOMLIST.Add(bom);
                    PRODUCTc prod = new PRODUCTc();

                    LINEQTIMEc lineqtime = new LINEQTIMEc();
                    job.MesCstBody.LOTLIST[0].LINEQTIMELIST.Add(lineqtime);
                    PROCESSLINEc procline = new PROCESSLINEc();
                    job.MesCstBody.LOTLIST[0].PROCESSLINELIST.Add(procline);
                    STBPRODUCTSPECc stb = new STBPRODUCTSPECc();
                    job.MesCstBody.LOTLIST[0].STBPRODUCTSPECLIST.Add(stb);
                    MACHINEQTIMEc machqtime = new MACHINEQTIMEc();
                    job.MesCstBody.LOTLIST[0].STBPRODUCTSPECLIST.Add(stb);
                    SUBPRODUCTSPECc sub = new SUBPRODUCTSPECc();//Add By Yangzhenteng20190316;
                    job.MesCstBody.LOTLIST[0].SUBPRODUCTSPECLIST.Add(sub);

                }
                #endregion

                #region MES Product
                if (job.MesProduct.ABNORMALCODELIST.Count <= 0)
                {
                    CODEc code = new CODEc();
                    job.MesProduct.ABNORMALCODELIST.Add(code);
                }
                if (job.MesProduct.REWORKLIST.Count <= 0)
                {
                    REWORKc rework = new REWORKc();
                    job.MesProduct.REWORKLIST.Add(rework);
                }
                if (job.MesProduct.DEFECTLIST.Count <= 0)
                {
                    DEFECTc def = new DEFECTc();
                    job.MesProduct.DEFECTLIST.Add(def);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        public bool GetProductType(eFabType fabtype, string ownerType, IList<Job> jobs, Job job, out string err)
        {
            err = string.Empty;
            try
            {
                Job tmpJob = null;

                // 1. 先從此次CST資料的去找
                tmpJob = jobs.FirstOrDefault(j => j.ProductType.Equals(job.ProductType));
                if (tmpJob != null)
                {
                    job.ProductType.Value = tmpJob.ProductType.Value;
                    return true;
                }

                List<ProductType> AllProducts = ObjectManager.ProductTypeManager.GetProductTypes() as List<ProductType>;

                // 2. 從ProductTypeManager去找整線的
                ProductType type = AllProducts.FirstOrDefault(t => t.Equals(job.ProductType));
                if (type != null)
                {
                    job.ProductType = type;
                    ObjectManager.ProductTypeManager.UpdateLastUseTime(type);
                    return true;
                }

                // 3. 從全部的JobData去找, 防止有人刪掉PorductType的資料
                lock (_entities)
                {
                    tmpJob = _entities.Values.FirstOrDefault(j => j.ProductType.Equals(job.ProductType));
                }
                if (tmpJob != null)
                {
                    job.ProductType.Value = tmpJob.ProductType.Value;
                    ObjectManager.ProductTypeManager.AddNewProductType(job.ProductType);
                    return true;
                }

                // 4. 之前都找不到時, 重新產生新的Product Type
                if (fabtype == eFabType.ARRAY)
                {
                    #region Array Rule
                    int arrayJobType = 0;
                    //Engineer :1  DUMMY :2  Product :3
                    switch (ownerType.Substring(ownerType.Length - 1, 1))
                    {
                        case "D": arrayJobType = 4; break; //add for cell Dummy glass to array process 2016/05/16 cc.kuang
                        case "P": arrayJobType = 3; break;
                        case "M": arrayJobType = 2; break;
                        case "E": arrayJobType = 1; break;
                        default:
                            err = "OwnerType value Invalid, get product type error!";
                            return false;
                    }

                    List<int> tpList = AllProducts.Select(t => t.Value).Where(t => t > 100 && ((t % 10) == arrayJobType)).ToList<int>();

                    if (tpList.Count() >= 6544)
                    {
                        // 已全部使用, 要從前面號碼開始找沒使用的Product Type
                        List<ProductType> tpList2;
                        lock (_entities)
                        {
                            tpList2 = AllProducts.Where(t => t.Value > 100 && ((t.Value % 10) == arrayJobType) &&
                                !_entities.Values.Any(j => j.ProductType.Value.Equals(t.Value))).ToList<ProductType>();
                        }

                        if (tpList2.Count() > 0)
                        {
                            ObjectManager.ProductTypeManager.ReplaceNewProductType(job, tpList2);
                            return true;
                        }
                        else
                        {
                            err = "Each product type for online has been used.";
                            return false;
                        }
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder(new string('0', 6554));

                        for (int i = 0; i < tpList.Count(); i++)
                        {
                            sb.Replace('0', '1', tpList[i] / 10, 1);
                        }

                        int lastNo = sb.ToString().LastIndexOf('1');
                        if (lastNo.Equals(-1))
                        {
                            job.ProductType.Value = 100 + arrayJobType;
                            ObjectManager.ProductTypeManager.AddNewProductType(job.ProductType);
                            return true;
                        }
                        else if (lastNo == 6553)
                        {
                            //應該不會有以下Case, 如果值為6553表示都使用完了, 防呆
                            int no = sb.ToString().IndexOf('0', 10);
                            if (no == -1)
                            {
                                err = "Each product type for online has been used.";
                                return false;
                            }
                            else
                            {
                                job.ProductType.Value = no * 10 + arrayJobType;
                                ObjectManager.ProductTypeManager.AddNewProductType(job.ProductType);
                                return true;
                            }
                        }
                        else
                        {
                            int no = sb.ToString().IndexOf('0', lastNo);
                            if (no == -1)
                            {
                                int no2 = sb.ToString().IndexOf('0', 10);
                                if (no2 == -1)
                                {
                                    err = "Each product type for online has been used.";
                                    return false;
                                }
                                else
                                {
                                    job.ProductType.Value = no2 * 10 + arrayJobType;
                                    ObjectManager.ProductTypeManager.AddNewProductType(job.ProductType);
                                    return true;
                                }
                            }
                            else
                            {
                                job.ProductType.Value = no * 10 + arrayJobType;
                                ObjectManager.ProductTypeManager.AddNewProductType(job.ProductType);
                                return true;
                            }
                        }
                    }
                    #endregion === Array Rule
                }
                else
                {
                    #region CF CELL Rule
                    List<int> tpList = AllProducts.Select(t => t.Value).Where(t => t > 100).ToList<int>();

                    if (tpList.Count() >= 65435)
                    {
                        // 已全部使用, 要從前面號碼開始找沒使用的Product Type
                        List<ProductType> tpList2; 
                        lock (_entities)
                        {
                             tpList2 = AllProducts.Where(t => t.Value > 100 &&
                                !_entities.Values.Any(j => j.ProductType.Value.Equals(t.Value))).ToList<ProductType>();
                        }

                        if (tpList2.Count() > 0)
                        {
                            ObjectManager.ProductTypeManager.ReplaceNewProductType(job, tpList2);
                            return true;
                        }
                        else
                        {
                            err = "Each product type for online has been used.";
                            return false;
                        }
                    }

                    StringBuilder sb = new StringBuilder(new string('0', 65535));

                    for (int i = 0; i < tpList.Count(); i++)
                    {
                        sb.Replace('0', '1', tpList[i] - 1, 1);
                    }

                    int lastNo = sb.ToString().LastIndexOf('1');
                    if (lastNo.Equals(-1))
                    {
                        job.ProductType.Value = 101;
                        ObjectManager.ProductTypeManager.AddNewProductType(job.ProductType);
                        return true;
                    }
                    else if (lastNo == 65534)
                    {
                        int no = sb.ToString().IndexOf('0', 100);
                        if (no == -1)
                        {
                            err = "Each product type for online has been used.";
                            return false;
                        }
                        else
                        {
                            job.ProductType.Value = no + 1;
                            ObjectManager.ProductTypeManager.AddNewProductType(job.ProductType);
                            return true;
                        }
                    }
                    else
                    {
                        int no = sb.ToString().IndexOf('0', lastNo);
                        if (no == -1)
                        {
                            int no2 = sb.ToString().IndexOf('0', 100);
                            if (no2 == -1)
                            {
                                err = "Each product type for online has been used.";
                                return false;
                            }
                            else
                            {
                                job.ProductType.Value = no2 + 1;
                                ObjectManager.ProductTypeManager.AddNewProductType(job.ProductType);
                                return true;
                            }
                        }
                        else
                        {
                            job.ProductType.Value = no + 1;
                            ObjectManager.ProductTypeManager.AddNewProductType(job.ProductType);
                            return true;
                        }
                    }
                    #endregion =======
                }
            }
            catch (Exception ex)
            {
                err = ex.ToString();
                return false;
            }

        }

        public bool GetProductID(Port port, IList<Job> jobs, Job job, out string err)
        {
            err = string.Empty;
            try
            {
                Job tmpJob = null;

                // 1. 先從此次CST資料的去找
                tmpJob = jobs.FirstOrDefault(j => j.ProductID.Equals(job.ProductID));
                if (tmpJob != null)
                {
                    job.ProductID.Value = tmpJob.ProductID.Value;
                    return true;
                }

                List<ProductID> AllProducts = ObjectManager.ProductIDManager.GetProductIDs() as List<ProductID>;

                // 2. 從ProductIDManager去找整線的
                ProductID type = AllProducts.FirstOrDefault(t => t.Equals(job.ProductID));
                if (type != null)
                {
                    job.ProductID = type;
                    ObjectManager.ProductIDManager.UpdateLastUseTime(type);
                    return true;
                }

                // 3. 從全部的JobData去找, 防止有人刪掉PorductType的資料
                lock (_entities)
                {
                    tmpJob = _entities.Values.FirstOrDefault(j => j.ProductID.Equals(job.ProductID));
                }
                if (tmpJob != null)
                {
                    job.ProductID.Value = tmpJob.ProductID.Value;
                    ObjectManager.ProductIDManager.AddNewProductID(job.ProductID);
                    return true;
                }

                // 4. 之前都找不到時, 重新產生新的Product Type
                #region CELL Rule
                List<int> tpList = AllProducts.Select(t => t.Value).Where(t => t > 100).ToList<int>();

                if (tpList.Count() >= 65435)
                {
                    // 已全部使用, 要從前面號碼開始找沒使用的Product Type
                    lock (_entities)
                    {
                        List<ProductID> tpList2 = AllProducts.Where(t => t.Value > 100 &&
                            !_entities.Values.Any(j => j.ProductID.Value.Equals(t.Value))).ToList<ProductID>();


                        if (tpList2.Count() > 0)
                        {
                            ObjectManager.ProductIDManager.ReplaceNewProductID(job, tpList2);
                            return true;
                        }
                        else
                        {
                            err = "Each product type for online has been used.";
                            return false;
                        }
                    }
                }

                StringBuilder sb = new StringBuilder(new string('0', 65535));

                for (int i = 0; i < tpList.Count(); i++)
                {
                    sb.Replace('0', '1', tpList[i] - 1, 1);
                }

                int lastNo = sb.ToString().LastIndexOf('1');
                if (lastNo.Equals(-1))
                {
                    job.ProductID.Value = 101;
                    ObjectManager.ProductIDManager.AddNewProductID(job.ProductID);
                    return true;
                }
                else if (lastNo == 65534)
                {
                    int no = sb.ToString().IndexOf('0', 100);
                    if (no == -1)
                    {
                        err = "Each product type for online has been used.";
                        return false;
                    }
                    else
                    {
                        job.ProductID.Value = no + 1;
                        ObjectManager.ProductIDManager.AddNewProductID(job.ProductID);
                        return true;
                    }
                }
                else
                {
                    int no = sb.ToString().IndexOf('0', lastNo);
                    if (no == -1)
                    {
                        int no2 = sb.ToString().IndexOf('0', 100);
                        if (no2 == -1)
                        {
                            err = "Each product type for online has been used.";
                            return false;
                        }
                        else
                        {
                            job.ProductID.Value = no2 + 1;
                            ObjectManager.ProductIDManager.AddNewProductID(job.ProductID);
                            return true;
                        }
                    }
                    else
                    {
                        job.ProductID.Value = no + 1;
                        ObjectManager.ProductIDManager.AddNewProductID(job.ProductID);
                        return true;
                    }
                }
                #endregion =======
            }
            catch (Exception ex)
            {
                err = ex.ToString();
                return false;
            }

        }

        public void SaveLineRecipeToDB(Line line, string hostRecipeName, string ppID, string ip)
        {
            try
            {
                IList tmpData = HibernateAdapter.GetObject_AND(typeof(RECIPE),
                                                        new string[] { "LINETYPE", "ONLINECONTROLSTATE", "LINERECIPENAME", "UPDATELINEID" },
                                                        new object[] { line.Data.LINETYPE, "REMOTE", hostRecipeName, line.Data.LINEID },
                                                        null, null);

                if (tmpData == null) return;

                RECIPE obj = null;
                if (tmpData.Count > 0)
                {
                    obj = tmpData[0] as RECIPE;
                    obj.PPID = ppID;
                    obj.LASTUPDATEDT = DateTime.Now;
                    obj.UPDATEOPERATOR = "MES";
                    obj.UPDATEPCIP = ip;
                    obj.RECIPETYPE = "LOT";
                    HibernateAdapter.UpdateObject(obj);
                }
                else
                {
                    obj = new RECIPE()
                    {
                        FABTYPE = line.Data.FABTYPE,
                        LINETYPE = line.Data.LINETYPE,
                        ONLINECONTROLSTATE = "REMOTE",
                        LINERECIPENAME = hostRecipeName,
                        UPDATELINEID = line.Data.LINEID,
                        PPID = ppID,
                        LASTUPDATEDT = DateTime.Now,
                        UPDATEOPERATOR = "MES",
                        UPDATEPCIP = ip,
                        RECIPETYPE = "LOT",
                    };
                    HibernateAdapter.SaveObject(obj);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public Tuple<string, Dictionary<string, string>> ParsePPID(string sPPID)
        {
            string retData1 = string.Empty;
            Dictionary<string, string> retData2 = new Dictionary<string, string>();

            try
            {
                string[] sData1 = sPPID.Split(';');
                for (int i = 0; i < sData1.Length; i++)
                {
                    string[] sData2 = sData1[i].Split(':');
                    if (sData2.Length.Equals(2))
                    {
                        retData1 += sData2[1];
                        retData2.Add(sData2[0], sData2[1]);
                    }
                    else
                    {
                        retData1 += sData1[i];
                        retData2.Add(string.Format("L{0}", (i + 2)), sData1[i]);
                    }
                }
                return new Tuple<string, Dictionary<string, string>>(retData1, retData2);
            }
            catch
            {
                return new Tuple<string, Dictionary<string, string>>(retData1, retData2);
            }
        }

        #endregion

        /// <summary>
        /// MES Download PPID will lost  Virtual Machine and Discontinuous EQP No
        /// BC Auto Add Eqpno Recipe ID in PPID.
        /// ex: FCRPH Line 沒有機台L10, L15 and L20 仍然需要寫JOBData給機台，讓機台從JobData取得
        /// ex:Local 10, 15 and 20 There are filled  by space or zero(0).
        /// </summary>
        /// <param name="mesPPID">MES PPID :L2:AA;L3:BB;L4:CC;L5:DD;L6:EE;L7:FF;L9:GG....</param>
        /// <out param name="fill_Null_EQPPID">補齊EQP PPID: AABBCCDDEEFF00GG</param>
        /// <returns>{AA,BB,CC,DD,EE,FF,00,GG} 收集以Recipe Service 給機台下Recipe ID Validate or Parametere...</returns>
        //Jun Modify 20141204 將Method改為Public 不然其他cs沒辦法使用
        //Watson Modify 20141215 For Array Line Special Rule
        public Dictionary<string, string> AddVirtualEQP_PPID2(Line line, string mesPPID, out string fill_Null_EQPPID)
        {
            try
            {
                //L2:AA;L3:BB;L4:CC;L5:DD...
                string[] recipeIDs = mesPPID.Split(';');
                fill_Null_EQPPID = string.Empty;
                Dictionary<string, string> RecipeDic = new Dictionary<string, string>();
                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string[] eqreci = recipeIDs[i].Split(':'); //L2:AA
                    if (eqreci.Length < 2) //AA;BB;CC;DD;EE;FF;GG
                    {
                        NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("MES PPID Type is For Old Spec!!=[{0}]", mesPPID));
                        int j = 0;

                        foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())  // .GetEQPsByLine(line.Data.LINEID))
                        {
                            if (recipeIDs.Length <= j)
                            {
                                NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("MES PPID Length=[{0}] and DB EQP Count=[{1}] is different!!", recipeIDs.Length.ToString(), ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).Count.ToString()));
                                RecipeDic.Add(eqp.Data.NODENO, new string('0', eqp.Data.RECIPELEN));  //AA;BB;CC;DD;EE;FF;GG
                            }
                            else
                            {
                                RecipeDic.Add(eqp.Data.NODENO, recipeIDs[j]);  //AA;BB;CC;DD;EE;FF;GG
                            }
                            j++;
                        }
                        if (recipeIDs.Length > j) //t3 add avoid loss PVD, ELA PPID for 2nd clean, cc.kuang 2015/07/03
                        {
                            RecipeDic.Add(L3_Second, recipeIDs[j]);
                        }
                        break;
                    }
                    if (!RecipeDic.ContainsKey(eqreci[0]))
                        RecipeDic.Add(eqreci[0], eqreci[1].Trim());
                    else
                    {       //For Array ITO PVD  L3 PPID可能重複了， 直接指定L3為L5 在後續再轉回
                        if (!RecipeDic.ContainsKey(L3_Second))
                        {
                            RecipeDic.Add(L3_Second, eqreci[1].Trim());
                        }
                    }
                }

                #region CF Photo Line 無論有沒有機台都要補的PPID

                //AA;BB;CC;DD;EE;FF;GG;HH;II;JJ;KK;LL;MM;NN;OO;PP;QQ;RR;SSS
                if ((line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1) || (line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1) ||
                            (line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1) || (line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1) ||
                                (line.Data.LINETYPE == eLineType.CF.FCSPH_TYPE1) || (line.Data.LINETYPE == eLineType.CF.FCOPH_TYPE1))
                {
                    int reciplen = 2;
                    for (int i = 2; i <= 20; i++)
                    {
                        if (i == 20) //L20
                            reciplen = 3;
                        if (RecipeDic.ContainsKey("L" + i.ToString()))
                            fill_Null_EQPPID += RecipeDic["L" + i.ToString()].Trim();
                        else
                            fill_Null_EQPPID += new string('0', reciplen);
                    }
                    return RecipeDic;
                }

                #endregion

                #region Array PVD , ITO 尾部補上L3:AA PHOTO補上L7在L4之後
                if (line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW)
                {
                    if (!RecipeDic.ContainsKey(L3_Second))
                        RecipeDic.Add(L3_Second, "0000");  //AA;BBBB;CCCC;DDDD;EEEEEEEEEEEEEEE;FF
                }

                if (line.Data.LINETYPE == eLineType.ARRAY.PHL_TITLE || line.Data.LINETYPE == eLineType.ARRAY.PHL_EDGEEXP) //Array Photo Line
                {
                    if (!RecipeDic.ContainsKey("L4"))
                        RecipeDic.Add("L4", "0000");  //AA;BBBB;CCCC;DDDD;EEEEEEEEEEEEEEE;FF
                }
                #endregion

                if (line.Data.LINETYPE == eLineType.ARRAY.PHL_TITLE || line.Data.LINETYPE == eLineType.ARRAY.PHL_EDGEEXP)
                {
                    fill_Null_EQPPID = Array_PhotoLine_PPID(RecipeDic);
                    return RecipeDic;
                }


                //下個機台的PPID是這種格式：AABBCCDDEEFF00GG
                //但上報MES不是，而是要加上機台NO L2:AA;L3:BB;L4:CC;L5:DD;L6:EE;L7:FF;L9:GG
                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())  // .GetEQPsByLine(line.Data.LINEID))
                {
                    string eqpno = eqp.Data.NODENO;
                    if (RecipeDic.ContainsKey(eqpno))
                    {
                        fill_Null_EQPPID += RecipeDic[eqpno].Trim();
                    }
                    else
                    {
                        fill_Null_EQPPID += new string('0', eqp.Data.RECIPELEN);
                    }
                }
                return RecipeDic;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                fill_Null_EQPPID = string.Empty;
                return null;
            }
        }

        private string Array_PhotoLine_PPID(Dictionary<string, string> RecipeDic)
        {
            string fill_ppid = string.Empty;
            //L2,L3;L7;L4;L5;L6
            for (int i = 0; i <= 7; i++)
            {
                string eqpno = "L" + (i + 2).ToString();
                if (RecipeDic.ContainsKey(eqpno))
                {
                    if (eqpno == "L4") //Array Photo Line
                    {
                        fill_ppid += RecipeDic["L7"].Trim().PadRight(8, '0') + RecipeDic["L4"].Trim();
                    }
                    else
                    {
                        //Array PHOTO補上L7在L4之後
                        if (eqpno == "L7") //Array Photo Line
                            continue;
                        fill_ppid += RecipeDic[eqpno].Trim();
                    }
                }

            }
            return fill_ppid;
        }

        public Dictionary<string, string> AddVirtualEQP_PPID(Line line, string mesPPID, out string fill_Null_EQPPID)
        {
            try
            {
                //L2:AA;L3:BB;L4:CC;L5:DD...
                string[] recipeIDs = mesPPID.Split(';');
                fill_Null_EQPPID = string.Empty;
                Dictionary<string, string> RecipeDic = new Dictionary<string, string>();
                Dictionary<string, string> byPassRecipeDic = new Dictionary<string, string>();  // add by bruce 2016/05/16
                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string[] eqreci = recipeIDs[i].Split(':'); //L2:AA
                    if (eqreci.Length < 2) //AA;BB;CC;DD;EE;FF;GG
                    {
                        NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("MES PPID Type is For Old Spec!!=[{0}]", mesPPID));
                        int j = 0;

                        //Watson Add 20150520 防止 OLD SPEC Recipe 沒有帶機台Local No.在特殊Line(POL400)會出錯
                        List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                        if (line.Data.LINEID == "CBPOL400")
                            eqps = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID);

                        foreach (Equipment eqp in eqps)  // .GetEQPsByLine(line.Data.LINEID))
                        {
                            if (recipeIDs.Length <= eqps.Count)
                            {
                                NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("MES PPID Length=[{0}] and DB EQP Count=[{1}] is different!!", recipeIDs.Length.ToString(), eqps.Count.ToString()));
                                RecipeDic.Add(eqp.Data.NODENO, new string('0', eqp.Data.RECIPELEN));  //AA;BB;CC;DD;EE;FF;GG
                            }
                            else
                            {
                                if (!RecipeDic.ContainsKey(eqp.Data.NODENO))
                                    RecipeDic.Add(eqp.Data.NODENO, recipeIDs[j]);  //AA;BB;CC;DD;EE;FF;GG
                            }
                            j++;
                        }
                        break;
                    }
                     //add for CLN 2nd run, MES "L3" download 8码recipe--add by yang 2016/8/12
                    if (eqreci[0] == "L3" && eqreci[1].Length == 8 && line.File.HostMode != eHostMode.OFFLINE)
                    {
                        RecipeDic.Add("L3", eqreci[1].Substring(0, 4).Trim());
                        RecipeDic.Add(L3_Second, eqreci[1].Substring(4, 4).Trim());  //For Array ITO 、PVD
                    }
                    if (!RecipeDic.ContainsKey(eqreci[0]))
                        RecipeDic.Add(eqreci[0], eqreci[1].Trim());
                    else
                    {       //For Array ITO PVD  L3 PPID可能重複了， 直接指定L3為L5 在後續再轉回
                        if (!RecipeDic.ContainsKey(L3_Second))
                        {
                            RecipeDic.Add(L3_Second, eqreci[1].Trim());
                        }
                    }
                }

                #region CF Photo Line 無論有沒有機台都要補的PPID

                //AA;BB;CC;DD;EE;FF;GG;HH;II;JJ;KK;LL;MM;NN;OO;PP;QQ;RR;SSS
                //20160106 Add by Frank T3 Photo Line L2~L24 
                if ((line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1) || (line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1) ||
                            (line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1) || (line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1) ||
                                (line.Data.LINETYPE == eLineType.CF.FCSPH_TYPE1) || (line.Data.LINETYPE == eLineType.CF.FCOPH_TYPE1))
                {
                    int reciplen = 2;
                    for (int i = 2; i <= 24; i++)
                    {
                        if (i == 24) //T3 L24
                            reciplen = 3;
                        if (RecipeDic.ContainsKey("L" + i.ToString()))
                            fill_Null_EQPPID += RecipeDic["L" + i.ToString()].Trim();
                        else
                            fill_Null_EQPPID += new string('0', reciplen);
                    }
                    return RecipeDic;
                }

                #endregion

                #region Array PVD , ITO 尾部補上L3:AA PHOTO補上L7在L4之後
                if (line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW)
                {
                    if (!RecipeDic.ContainsKey(L3_Second))
                        RecipeDic.Add(L3_Second, "0000");  //AA;BBBB;CCCC;DDDD;EEEEEEEEEEEEEEE;FF
                }
                //2; 4;8;4 ;12;2
                //L2;L3;L7;L4;L5;L6
                #endregion

                //MAX LEN
                int maxlen = ObjectManager.EquipmentManager.GetEQPs().Max(eqp => eqp.Data.RECIPEIDX + eqp.Data.RECIPELEN);
                string maxppidstring = new string('0', maxlen);
                if (maxlen == 0)
                {
                    NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("NODE RECIPE INDEX IS ZERO!!", maxlen));
                    return RecipeDic;
                }
                fill_Null_EQPPID = maxppidstring;
                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
                {
                    string eqpno = eqp.Data.NODENO;
                    if (RecipeDic.ContainsKey(eqpno))
                    {
                        if (fill_Null_EQPPID.Length < (eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX))
                            fill_Null_EQPPID += new string('0', eqp.Data.RECIPELEN);

                        #region add by bruce 20160321 Array Special EQ CIM Off 時 , by Pass 機台Recipe 自動改為0
                        bool IsBypassRecipe=false ;
                        ParameterManager para = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                        switch (eqp.Data.LINEID)
                        { 
                            case "TCATS200":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCATS200))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCATS200].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCATS400":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCATS400))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCATS400].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCCDO400":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCCDO400))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCCDO400].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCCDO300":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCCDO300))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCCDO300].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCAOH800":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCAOH800))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCAOH800].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCAOH300":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCAOH300))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCAOH300].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCAOH900":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCAOH900))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCAOH900].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCAOH400":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCAOH400))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCAOH400].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCELA100":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCELA100))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCELA100].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCELA300":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCELA300))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCELA300].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCELA200":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCELA200))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCELA200].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCFLR200":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCFLR200))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCFLR200].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TCFLR300":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCFLR300))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCFLR300].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TETEG200":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCTEG200))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCTEG200].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "TETEG400":
                                if (para.Parameters.ContainsKey(eArrayPPIDByPass.TCTEG400))
                                {
                                    string[] byPasseqps = para[eArrayPPIDByPass.TCTEG400].GetString().Split(',');
                                    foreach (string byPasseqp in byPasseqps)
                                    {
                                        if (eqp.Data.NODENO == byPasseqp && eqp.File.CIMMode == eBitResult.OFF)
                                        {
                                            IsBypassRecipe = true;
                                            break;
                                        }
                                    }
                                }
                                break;
                        }
                        #endregion

                        if (IsBypassRecipe) // for Array Special eq use
                        {
                            fill_Null_EQPPID = fill_Null_EQPPID.Substring(0, eqp.Data.RECIPEIDX) + "0".Trim().PadRight(eqp.Data.RECIPELEN, '0') + fill_Null_EQPPID.Substring((eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX), fill_Null_EQPPID.Length - (eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX));
                            byPassRecipeDic.Add(eqp.Data.NODENO , "0".Trim().PadRight(eqp.Data.RECIPELEN, '0'));    //add by bruce 2016/05/16 
                            NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("{0} EQ CIM Mode Off , MES PPID =[{1}] change to PPID =[{2}]", eqp.Data.NODENO, RecipeDic[eqpno], "0".Trim().PadRight(eqp.Data.RECIPELEN, '0')));
                        }
                        else
                        {
                            fill_Null_EQPPID = fill_Null_EQPPID.Substring(0, eqp.Data.RECIPEIDX) + RecipeDic[eqpno].Trim().PadRight(eqp.Data.RECIPELEN, '0') + fill_Null_EQPPID.Substring((eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX), fill_Null_EQPPID.Length - (eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX));
                            byPassRecipeDic.Add(eqp.Data.NODENO, RecipeDic[eqpno].Trim().PadRight(eqp.Data.RECIPELEN, '0'));    //add by bruce 2016/05/16 
                        }
                    }
                    else
                    {
                        if (fill_Null_EQPPID.Length < (eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX))
                            fill_Null_EQPPID = fill_Null_EQPPID.Substring(0, eqp.Data.RECIPEIDX) + new string('0', eqp.Data.RECIPELEN) + fill_Null_EQPPID.PadRight(fill_Null_EQPPID.Length - eqp.Data.RECIPELEN);
                        else
                        {

                        }
                    }
                }

                #region add by bruce 2016/05/16 for eq cim off , all recipe bypass , online bc auto cancel cst ,offline bc pop message 
                bool Ismodify = false;
                ParameterManager pm = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                switch (line.Data.LINEID)
                {
                    case "TCATS200":
                        if (pm.Parameters.ContainsKey(eArrayPPIDByPass.TCATS200))
                        {
                            string[] byPasseqps = pm[eArrayPPIDByPass.TCATS200].GetString().Split(',');
                            foreach (string byPasseqp in byPasseqps)
                            {
                                if (byPassRecipeDic.ContainsKey(byPasseqp))
                                {
                                    if (byPassRecipeDic[byPasseqp] != RecipeDic[byPasseqp])
                                    {
                                        Ismodify = true;
                                    }
                                }
                            }
                            if (Ismodify)
                            {
                                bool checkbypassEq = false;
                                foreach (string byPasseqp in byPasseqps)
                                {
                                    if (byPassRecipeDic.ContainsKey(byPasseqp))
                                    {
                                        string emptrecipe = new string('0', byPassRecipeDic[byPasseqp].Length);
                                        if (emptrecipe == byPassRecipeDic[byPasseqp])
                                            checkbypassEq = true;
                                        else
                                            checkbypassEq = false;
                                    }
                                    if (!checkbypassEq) break;
                                }
                                if (checkbypassEq)
                                {
                                    fill_Null_EQPPID = "ALL_EQ_RECIPE_BYPASS";
                                }
                            }
                        }
                        break;
                    case "TCATS400":
                        if (pm.Parameters.ContainsKey(eArrayPPIDByPass.TCATS400))
                        {
                            string[] byPasseqps = pm[eArrayPPIDByPass.TCATS400].GetString().Split(',');
                            foreach (string byPasseqp in byPasseqps)
                            {
                                if (byPassRecipeDic.ContainsKey(byPasseqp))
                                {
                                    if (byPassRecipeDic[byPasseqp] != RecipeDic[byPasseqp])
                                    {
                                        Ismodify = true;
                                    }
                                }
                            }
                            if (Ismodify)
                            {
                                bool checkbypassEq = false;
                                foreach (string byPasseqp in byPasseqps)
                                {
                                    if (byPassRecipeDic.ContainsKey(byPasseqp))
                                    {
                                        string emptrecipe = new string('0', byPassRecipeDic[byPasseqp].Length);
                                        if (emptrecipe == byPassRecipeDic[byPasseqp])
                                            checkbypassEq = true;
                                        else
                                            checkbypassEq = false;
                                    }
                                    if (!checkbypassEq) break;
                                }
                                if (checkbypassEq)
                                {
                                    fill_Null_EQPPID = "ALL_EQ_RECIPE_BYPASS";
                                }
                            }
                        }
                        break;
                    case "TCCDO400":
                        if (pm.Parameters.ContainsKey(eArrayPPIDByPass.TCCDO400))
                        {
                            string[] byPasseqps = pm[eArrayPPIDByPass.TCCDO400].GetString().Split(',');
                            foreach (string byPasseqp in byPasseqps)
                            {
                                if (byPassRecipeDic.ContainsKey(byPasseqp))
                                {
                                    if (byPassRecipeDic[byPasseqp] != RecipeDic[byPasseqp])
                                    {
                                        Ismodify = true;
                                    }
                                }
                            }
                            if (Ismodify)
                            {
                                bool checkbypassEq = false;
                                foreach (string byPasseqp in byPasseqps)
                                {
                                    if (byPassRecipeDic.ContainsKey(byPasseqp))
                                    {
                                        string emptrecipe = new string('0', byPassRecipeDic[byPasseqp].Length);
                                        if (emptrecipe == byPassRecipeDic[byPasseqp])
                                            checkbypassEq = true;
                                        else
                                            checkbypassEq = false;
                                    }
                                    if (!checkbypassEq) break;
                                }
                                if (checkbypassEq)
                                {
                                    fill_Null_EQPPID = "ALL_EQ_RECIPE_BYPASS";
                                }
                            }
                        }
                        break;
                    case "TCAOH800":
                        if (pm.Parameters.ContainsKey(eArrayPPIDByPass.TCAOH800))
                        {
                            string[] byPasseqps = pm[eArrayPPIDByPass.TCAOH800].GetString().Split(',');
                            foreach (string byPasseqp in byPasseqps)
                            {
                                if (byPassRecipeDic.ContainsKey(byPasseqp))
                                {
                                    if (byPassRecipeDic[byPasseqp] != RecipeDic[byPasseqp])
                                    {
                                        Ismodify = true;
                                    }
                                }
                            }
                            if (Ismodify)
                            {
                                bool checkbypassEq = false;
                                foreach (string byPasseqp in byPasseqps)
                                {
                                    if (byPassRecipeDic.ContainsKey(byPasseqp))
                                    {
                                        string emptrecipe = new string('0', byPassRecipeDic[byPasseqp].Length);
                                        if (emptrecipe == byPassRecipeDic[byPasseqp])
                                            checkbypassEq = true;
                                        else
                                            checkbypassEq = false;
                                    }
                                    if (!checkbypassEq) break;
                                }
                                if (checkbypassEq)
                                {
                                    fill_Null_EQPPID = "ALL_EQ_RECIPE_BYPASS";
                                }
                            }
                        }
                        break;
                    case "TCFLR200":
                        if (pm.Parameters.ContainsKey(eArrayPPIDByPass.TCFLR200))
                        {
                            string[] byPasseqps = pm[eArrayPPIDByPass.TCFLR200].GetString().Split(',');
                            foreach (string byPasseqp in byPasseqps)
                            {
                                if (byPassRecipeDic.ContainsKey(byPasseqp))
                                {
                                    if (byPassRecipeDic[byPasseqp] != RecipeDic[byPasseqp])
                                    {
                                        Ismodify = true;
                                    }
                                }
                            }
                            if (Ismodify)
                            {
                                bool checkbypassEq = false;
                                foreach (string byPasseqp in byPasseqps)
                                {
                                    if (byPassRecipeDic.ContainsKey(byPasseqp))
                                    {
                                        string emptrecipe = new string('0', byPassRecipeDic[byPasseqp].Length);
                                        if (emptrecipe == byPassRecipeDic[byPasseqp])
                                            checkbypassEq = true;
                                        else
                                            checkbypassEq = false;
                                    }
                                    if (!checkbypassEq) break;
                                }
                                if (checkbypassEq)
                                {
                                    fill_Null_EQPPID = "ALL_EQ_RECIPE_BYPASS";
                                }
                            }
                        }
                        break;
                    case "TCFLR300":
                        if (pm.Parameters.ContainsKey(eArrayPPIDByPass.TCFLR300))
                        {
                            string[] byPasseqps = pm[eArrayPPIDByPass.TCFLR300].GetString().Split(',');
                            foreach (string byPasseqp in byPasseqps)
                            {
                                if (byPassRecipeDic.ContainsKey(byPasseqp))
                                {
                                    if (byPassRecipeDic[byPasseqp] != RecipeDic[byPasseqp])
                                    {
                                        Ismodify = true;
                                    }
                                }
                            }
                            if (Ismodify)
                            {
                                bool checkbypassEq = false;
                                foreach (string byPasseqp in byPasseqps)
                                {
                                    if (byPassRecipeDic.ContainsKey(byPasseqp))
                                    {
                                        string emptrecipe = new string('0', byPassRecipeDic[byPasseqp].Length);
                                        if (emptrecipe == byPassRecipeDic[byPasseqp])
                                            checkbypassEq = true;
                                        else
                                            checkbypassEq = false;
                                    }
                                    if (!checkbypassEq) break;
                                }
                                if (checkbypassEq)
                                {
                                    fill_Null_EQPPID = "ALL_EQ_RECIPE_BYPASS";
                                }
                            }
                        }
                        break;
                    case "TCTEG200":
                        if (pm.Parameters.ContainsKey(eArrayPPIDByPass.TCTEG200))
                        {
                            string[] byPasseqps = pm[eArrayPPIDByPass.TCTEG200].GetString().Split(',');
                            foreach (string byPasseqp in byPasseqps)
                            {
                                if (byPassRecipeDic.ContainsKey(byPasseqp))
                                {
                                    if (byPassRecipeDic[byPasseqp] != RecipeDic[byPasseqp])
                                    {
                                        Ismodify = true;
                                    }
                                }
                            }
                            if (Ismodify)
                            {
                                bool checkbypassEq = false;
                                foreach (string byPasseqp in byPasseqps)
                                {
                                    if (byPassRecipeDic.ContainsKey(byPasseqp))
                                    {
                                        string emptrecipe = new string('0', byPassRecipeDic[byPasseqp].Length);
                                        if (emptrecipe == byPassRecipeDic[byPasseqp])
                                            checkbypassEq = true;
                                        else
                                            checkbypassEq = false;
                                    }
                                    if (!checkbypassEq) break;
                                }
                                if (checkbypassEq)
                                {
                                    fill_Null_EQPPID = "ALL_EQ_RECIPE_BYPASS";
                                }
                            }
                        }
                        break;
                    case "TCTEG400":
                        if (pm.Parameters.ContainsKey(eArrayPPIDByPass.TCTEG400))
                        {
                            string[] byPasseqps = pm[eArrayPPIDByPass.TCTEG400].GetString().Split(',');
                            foreach (string byPasseqp in byPasseqps)
                            {
                                if (byPassRecipeDic.ContainsKey(byPasseqp))
                                {
                                    if (byPassRecipeDic[byPasseqp] != RecipeDic[byPasseqp])
                                    {
                                        Ismodify = true;
                                    }
                                }
                            }
                            if (Ismodify)
                            {
                                bool checkbypassEq = false;
                                foreach (string byPasseqp in byPasseqps)
                                {
                                    if (byPassRecipeDic.ContainsKey(byPasseqp))
                                    {
                                        string emptrecipe = new string('0', byPassRecipeDic[byPasseqp].Length);
                                        if (emptrecipe == byPassRecipeDic[byPasseqp])
                                            checkbypassEq = true;
                                        else
                                            checkbypassEq = false;
                                    }
                                    if (!checkbypassEq) break;
                                }
                                if (checkbypassEq)
                                {
                                    fill_Null_EQPPID = "ALL_EQ_RECIPE_BYPASS";
                                }
                            }
                        }
                        break;
                    case "TCELA100":
                        if (pm.Parameters.ContainsKey(eArrayPPIDByPass.TCELA100))
                        {
                            string[] byPasseqps = pm[eArrayPPIDByPass.TCELA100].GetString().Split(',');
                            foreach (string byPasseqp in byPasseqps)
                            {
                                if (byPassRecipeDic.ContainsKey(byPasseqp))
                                {
                                    if (byPassRecipeDic[byPasseqp] != RecipeDic[byPasseqp])
                                    {
                                        Ismodify = true;
                                    }
                                }
                            }
                            if (Ismodify)
                            {
                                bool checkbypassEq = false;
                                foreach (string byPasseqp in byPasseqps)
                                {
                                    if (byPassRecipeDic.ContainsKey(byPasseqp))
                                    {
                                        string emptrecipe = new string('0', byPassRecipeDic[byPasseqp].Length);
                                        if (emptrecipe == byPassRecipeDic[byPasseqp])
                                            checkbypassEq = true;
                                        else
                                            checkbypassEq = false;
                                    }
                                    if (!checkbypassEq) break;
                                }
                                if (checkbypassEq)
                                {
                                    fill_Null_EQPPID = "ALL_EQ_RECIPE_BYPASS";
                                }
                            }   
                        }
                        break;
                    case "TCELA300":
                        if (pm.Parameters.ContainsKey(eArrayPPIDByPass.TCELA300))
                        {
                            string[] byPasseqps = pm[eArrayPPIDByPass.TCELA300].GetString().Split(',');
                            foreach (string byPasseqp in byPasseqps)
                            {
                                if (byPassRecipeDic.ContainsKey(byPasseqp))
                                {
                                    if (byPassRecipeDic[byPasseqp] != RecipeDic[byPasseqp])
                                    {
                                        Ismodify = true;
                                    }
                                }
                            }
                            if (Ismodify)
                            {
                                bool checkbypassEq = false;
                                foreach (string byPasseqp in byPasseqps)
                                {
                                    if (byPassRecipeDic.ContainsKey(byPasseqp))
                                    {
                                        string emptrecipe = new string('0', byPassRecipeDic[byPasseqp].Length);
                                        if (emptrecipe == byPassRecipeDic[byPasseqp])
                                            checkbypassEq = true;
                                        else
                                            checkbypassEq = false;
                                    }
                                    if (!checkbypassEq) break;
                                }
                                if (checkbypassEq)
                                {
                                    fill_Null_EQPPID = "ALL_EQ_RECIPE_BYPASS";
                                }
                            }
                        }
                        break;
                    case "TCELA200":
                        if (pm.Parameters.ContainsKey(eArrayPPIDByPass.TCELA200))
                        {
                            string[] byPasseqps = pm[eArrayPPIDByPass.TCELA200].GetString().Split(',');
                            foreach (string byPasseqp in byPasseqps)
                            {
                                if (byPassRecipeDic.ContainsKey(byPasseqp))
                                {
                                    if (byPassRecipeDic[byPasseqp] != RecipeDic[byPasseqp])
                                    {
                                        Ismodify = true;
                                    }
                                }
                            }
                            if (Ismodify)
                            {
                                bool checkbypassEq = false;
                                foreach (string byPasseqp in byPasseqps)
                                {
                                    if (byPassRecipeDic.ContainsKey(byPasseqp))
                                    {
                                        string emptrecipe = new string('0', byPassRecipeDic[byPasseqp].Length);
                                        if (emptrecipe == byPassRecipeDic[byPasseqp])
                                            checkbypassEq = true;
                                        else
                                            checkbypassEq = false;
                                    }
                                    if (!checkbypassEq) break;
                                }
                                if (checkbypassEq)
                                {
                                    fill_Null_EQPPID = "ALL_EQ_RECIPE_BYPASS";
                                }
                            }
                        }
                        break;
                }
                #endregion

                return RecipeDic;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                fill_Null_EQPPID = string.Empty;
                return null;
            }
        }

        //Add By Yangzhenteng 20190508 For BEOL CUT/POL中投Recipe分开检测
        public Dictionary<string, string> AddVirtualEQP_PPID_T3CELL_Special_Cassette(Line line, string mesPPID, out string fill_Null_EQPPID)
        {
            try
            {
                //L2:AA;L3:BB;L4:CC;L5:DD...                
                string[] recipeIDs = mesPPID.Split(';');
                fill_Null_EQPPID = string.Empty;
                Dictionary<string, string> RecipeDic = new Dictionary<string, string>();
                Dictionary<string, string> byPassRecipeDic = new Dictionary<string, string>();  // add by bruce 2016/05/16          
                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                ParameterManager _ParameterManager = Workbench.Instance.GetObject("ParameterManager") as ParameterManager; //Add By Yangzhenteng20190507;
                if (line.Data.LINEID.Contains("CCCUT") || line.Data.LINEID.Contains("CCPOL"))
                {
                    string[] CellRecipeCheckEQPList = _ParameterManager[eArrayPPIDByPass.CELLREICPECHECKEQPLISTCST].GetString().Split(',');
                    eqps = ObjectManager.EquipmentManager.GetEQPsByEQPNOList(CellRecipeCheckEQPList.ToList());
                }
                else
                { }
                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string[] eqreci = recipeIDs[i].Split(':'); //L2:AA
                    if (eqreci.Length < 2) //AA;BB;CC;DD;EE;FF;GG
                    {
                        NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("MES PPID Type is For Old Spec!!=[{0}]", mesPPID));
                        int j = 0;
                        foreach (Equipment eqp in eqps)  // .GetEQPsByLine(line.Data.LINEID))
                        {
                            if (recipeIDs.Length <= eqps.Count)
                            {
                                NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("MES PPID Length=[{0}] and DB EQP Count=[{1}] is different!!", recipeIDs.Length.ToString(), eqps.Count.ToString()));
                                RecipeDic.Add(eqp.Data.NODENO, new string('0', eqp.Data.RECIPELEN));  //AA;BB;CC;DD;EE;FF;GG
                            }
                            else
                            {
                                if (!RecipeDic.ContainsKey(eqp.Data.NODENO))
                                    RecipeDic.Add(eqp.Data.NODENO, recipeIDs[j]);  //AA;BB;CC;DD;EE;FF;GG
                            }
                            j++;
                        }
                        break;
                    }
                    if (!RecipeDic.ContainsKey(eqreci[0]))
                    {
                        foreach (Equipment eqp in eqps)
                        {
                            if (eqreci[0].Contains(eqp.Data.NODENO))
                            {
                                RecipeDic.Add(eqreci[0], eqreci[1].Trim());
                                break;
                            }
                            else
                            { }
                        }
                    }
                }
                //MAX LEN
                int maxlen = ObjectManager.EquipmentManager.GetEQPs().Max(eqp => eqp.Data.RECIPEIDX + eqp.Data.RECIPELEN);
                string maxppidstring = new string('0', maxlen);
                if (maxlen == 0)
                {
                    NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("NODE RECIPE INDEX IS ZERO!!", maxlen));
                    return RecipeDic;
                }
                fill_Null_EQPPID = maxppidstring;
                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
                {
                    string eqpno = eqp.Data.NODENO;
                    if (RecipeDic.ContainsKey(eqpno))
                    {
                        if (fill_Null_EQPPID.Length < (eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX))
                        {
                            fill_Null_EQPPID += new string('0', eqp.Data.RECIPELEN);
                        }
                        fill_Null_EQPPID = fill_Null_EQPPID.Substring(0, eqp.Data.RECIPEIDX) + RecipeDic[eqpno].Trim().PadRight(eqp.Data.RECIPELEN, '0') + fill_Null_EQPPID.Substring((eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX), fill_Null_EQPPID.Length - (eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX));
                        byPassRecipeDic.Add(eqp.Data.NODENO, RecipeDic[eqpno].Trim().PadRight(eqp.Data.RECIPELEN, '0'));    //add by bruce 2016/05/16                            
                    }
                    else
                    {
                        if (fill_Null_EQPPID.Length < (eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX))
                            fill_Null_EQPPID = fill_Null_EQPPID.Substring(0, eqp.Data.RECIPEIDX) + new string('0', eqp.Data.RECIPELEN) + fill_Null_EQPPID.PadRight(fill_Null_EQPPID.Length - eqp.Data.RECIPELEN);
                        else
                        { }
                    }
                }
                return RecipeDic;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                fill_Null_EQPPID = string.Empty;
                return null;
            }
        }
        
        //Add By Yangzhenteng 20190508 For BEOL CUT/POL中投Recipe分开检测
        public Dictionary<string, string> AddVirtualEQP_PPID_T3CELL_Special_Panel(Line line, string mesPPID, out string fill_Null_EQPPID)
        {
            try
            {
                //L2:AA;L3:BB;L4:CC;L5:DD...                
                string[] recipeIDs = mesPPID.Split(';');
                fill_Null_EQPPID = string.Empty;
                Dictionary<string, string> RecipeDic = new Dictionary<string, string>();
                Dictionary<string, string> byPassRecipeDic = new Dictionary<string, string>();  // add by bruce 2016/05/16          
                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                ParameterManager _ParameterManager = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                if (line.Data.LINEID.Contains("CCPOL") || line.Data.LINEID.Contains("CCCUT"))
                {
                    string[] CellRecipeCheckEQPList = _ParameterManager[eArrayPPIDByPass.CELLREICPCHECKEQPLISTPANEL].GetString().Split(',');
                    eqps = ObjectManager.EquipmentManager.GetEQPsByEQPNOList(CellRecipeCheckEQPList.ToList());
                }
                else
                { }
                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string[] eqreci = recipeIDs[i].Split(':'); //L2:AA
                    if (eqreci.Length < 2) //AA;BB;CC;DD;EE;FF;GG
                    {
                        NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("MES PPID Type is For Old Spec!!=[{0}]", mesPPID));
                        int j = 0;
                        foreach (Equipment eqp in eqps)  // .GetEQPsByLine(line.Data.LINEID))
                        {
                            if (recipeIDs.Length <= eqps.Count)
                            {
                                NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("MES PPID Length=[{0}] and DB EQP Count=[{1}] is different!!", recipeIDs.Length.ToString(), eqps.Count.ToString()));
                                RecipeDic.Add(eqp.Data.NODENO, new string('0', eqp.Data.RECIPELEN));  //AA;BB;CC;DD;EE;FF;GG
                            }
                            else
                            {
                                if (!RecipeDic.ContainsKey(eqp.Data.NODENO))
                                    RecipeDic.Add(eqp.Data.NODENO, recipeIDs[j]);  //AA;BB;CC;DD;EE;FF;GG
                            }
                            j++;
                        }
                        break;
                    }
                    if (!RecipeDic.ContainsKey(eqreci[0]))
                    {
                        foreach (Equipment eqp in eqps)
                        {
                            if (eqreci[0].Contains(eqp.Data.NODENO))
                            {
                                RecipeDic.Add(eqreci[0], eqreci[1].Trim());
                                break;
                            }
                            else
                            { }
                        }
                    }
                }
                //MAX LEN
                int maxlen = ObjectManager.EquipmentManager.GetEQPs().Max(eqp => eqp.Data.RECIPEIDX + eqp.Data.RECIPELEN);
                string maxppidstring = new string('0', maxlen);
                if (maxlen == 0)
                {
                    NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                           string.Format("NODE RECIPE INDEX IS ZERO!!", maxlen));
                    return RecipeDic;
                }
                fill_Null_EQPPID = maxppidstring;
                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
                {
                    string eqpno = eqp.Data.NODENO;
                    if (RecipeDic.ContainsKey(eqpno))
                    {
                        if (fill_Null_EQPPID.Length < (eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX))
                        {
                            fill_Null_EQPPID += new string('0', eqp.Data.RECIPELEN);
                        }
                        fill_Null_EQPPID = fill_Null_EQPPID.Substring(0, eqp.Data.RECIPEIDX) + RecipeDic[eqpno].Trim().PadRight(eqp.Data.RECIPELEN, '0') + fill_Null_EQPPID.Substring((eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX), fill_Null_EQPPID.Length - (eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX));
                        byPassRecipeDic.Add(eqp.Data.NODENO, RecipeDic[eqpno].Trim().PadRight(eqp.Data.RECIPELEN, '0'));    //add by bruce 2016/05/16                            
                    }
                    else
                    {
                        if (fill_Null_EQPPID.Length < (eqp.Data.RECIPELEN + eqp.Data.RECIPEIDX))
                            fill_Null_EQPPID = fill_Null_EQPPID.Substring(0, eqp.Data.RECIPEIDX) + new string('0', eqp.Data.RECIPELEN) + fill_Null_EQPPID.PadRight(fill_Null_EQPPID.Length - eqp.Data.RECIPELEN);
                        else
                        { }
                    }
                }
                return RecipeDic;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                fill_Null_EQPPID = string.Empty;
                return null;
            }
        }       

        public Dictionary<string, string> AddVirtualEQP_PPID_CFShortCut(string NextLineID, string mesPPID, out string fill_Null_EQPPID)
        {
            try
            {
                //L2:AA;L3:BB;L4:CC;L5:DD...
                string[] recipeIDs = mesPPID.Split(';');
                fill_Null_EQPPID = string.Empty;
                Dictionary<string, string> RecipeDic = new Dictionary<string, string>();
                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string[] eqreci = recipeIDs[i].Split(':'); //L2:AA
                    if (eqreci.Length < 2) //AA;BB;CC;DD;EE;FF;GG
                    {
                        NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("MES PPID Type is For Old Spec!!=[{0}]", mesPPID));
                        int j = 0;

                        foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPsByLine(NextLineID))
                        {
                            if (recipeIDs.Length <= j)
                            {
                                NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("MES PPID Length=[{0}] and DB EQP Count=[{1}] is different!!", recipeIDs.Length.ToString(), ObjectManager.EquipmentManager.GetEQPsByLine(NextLineID).Count.ToString()));
                                RecipeDic.Add(eqp.Data.NODENO, new string('0', eqp.Data.RECIPELEN));  //AA;BB;CC;DD;EE;FF;GG
                            }
                            else
                            {
                                RecipeDic.Add(eqp.Data.NODENO, recipeIDs[j]);  //AA;BB;CC;DD;EE;FF;GG
                            }
                            j++;
                        }
                        break;
                    }
                    if (!RecipeDic.ContainsKey(eqreci[0]))
                        RecipeDic.Add(eqreci[0], eqreci[1].Trim());
                }

                #region CF Photo Line 無論有沒有機台都要補的PPID
                //AA;BB;CC;DD;EE;FF;GG;HH;II;JJ;KK;LL;MM;NN;OO;PP;QQ;RR;SSS
                //Line line = ObjectManager.LineManager.GetLine(NextLineID);
                //if ((line.Data.LINETYPE == eLineType.CF.FCMPH_TYPE1) || (line.Data.LINETYPE == eLineType.CF.FCRPH_TYPE1) ||
                //            (line.Data.LINETYPE == eLineType.CF.FCGPH_TYPE1) || (line.Data.LINETYPE == eLineType.CF.FCBPH_TYPE1) ||
                //                (line.Data.LINETYPE == eLineType.CF.FCSPH_TYPE1) || (line.Data.LINETYPE == eLineType.CF.FCOPH_TYPE1))
                if ((NextLineID == "FCMPH100") || (NextLineID == "FCRPH100") || (NextLineID == "FCGPH100") || (NextLineID == "FCBPH100") ||
                    (NextLineID == "FCOPH100") || (NextLineID == "FCSPH100"))
                {
                    int reciplen = 2;
                    for (int i = 2; i <= 24; i++)
                    {
                        if (i == 24) //L24
                            reciplen = 3;
                        if (RecipeDic.ContainsKey("L" + i.ToString()))
                            fill_Null_EQPPID += RecipeDic["L" + i.ToString()].Trim();
                        else
                            fill_Null_EQPPID += new string('0', reciplen);
                    }
                }
                    #endregion

                return RecipeDic;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                fill_Null_EQPPID = string.Empty;
                return null;
            }
        }

        public Dictionary<string, string> AddVirtualEQP_PPID_CrossLine(Line line, string mesPPID, out string fill_Null_EQPPID)
        {
            try
            {
                //L2:AA;L3:BB;L4:CC;L5:DD...
                string[] recipeIDs = mesPPID.Split(';');
                fill_Null_EQPPID = string.Empty;
                Dictionary<string, string> RecipeDic = new Dictionary<string, string>();
                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string[] eqreci = recipeIDs[i].Split(':'); //L2:AA
                    if (eqreci.Length < 2) //AA;BB;CC;DD;EE;FF;GG
                    {
                        NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("MES PPID Type is For Old Spec!!=[{0}]", mesPPID));

                        for (int j = 0; j < 15; j++)
                        {
                            if (recipeIDs.Length < 15)
                            {
                                NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("PPID Length[{0}]  is Not ENOUGH!! PPID=[{1}]",recipeIDs.Length, mesPPID));
                                fill_Null_EQPPID = string.Empty;
                                return null;
                            }
                            RecipeDic.Add("L" + (j + 2).ToString(), recipeIDs[j]);
                        }
                        //foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())  // .GetEQPsByLine(line.Data.LINEID))
                        //{
                        //    if (recipeIDs.Length <= j)
                        //    {
                        //        NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        //      string.Format("MES PPID Length=[{0}] and DB EQP Count=[{1}] is different!!", recipeIDs.Length.ToString(), ObjectManager.EquipmentManager.GetEQPs().Count.ToString()));
                        //        RecipeDic.Add(eqp.Data.NODENO, new string('0', eqp.Data.RECIPELEN));  //AA;BB;CC;DD;EE;FF;GG
                        //    }
                        //    else
                        //    {
                        //        RecipeDic.Add(eqp.Data.NODENO, recipeIDs[j]);  //AA;BB;CC;DD;EE;FF;GG
                        //    }
                        //}
                        break;
                    }
                    if (!RecipeDic.ContainsKey(eqreci[0]))
                        RecipeDic.Add(eqreci[0], eqreci[1].Trim());
                    else
                    {       //For Array ITO PVD 應該是L3 重複了， 直接指定L3為L5 在後續再轉回
                        if (!RecipeDic.ContainsKey(L3_Second))
                        {
                            RecipeDic.Add(L3_Second, eqreci[1].Trim());
                        }
                    }
                }

                //下個機台的PPID是這種格式：AABBCCDDEEFF00GG
                //但上報MES不是，而是要加上機台NO L2:AA;L3:BB;L4:CC;L5:DD;L6:EE;L7:FF;L9:GG
                for (int i = 0; i < 14; i++)
                {
                    string eqpno = "L" + (i + 2).ToString();
                    if (RecipeDic.ContainsKey(eqpno))
                    {
                        fill_Null_EQPPID += RecipeDic[eqpno].Trim();
                    }
                    else
                    {
                        fill_Null_EQPPID += new string('0', 2);
                    }
                }

                //foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())  // .GetEQPsByLine(line.Data.LINEID))
                //{
                //    string eqpno = eqp.Data.NODENO;
                //    if (RecipeDic.ContainsKey(eqpno))
                //    {
                //        fill_Null_EQPPID += RecipeDic[eqpno].Trim();
                //    }
                //    else
                //    {
                //        fill_Null_EQPPID += new string('0', eqp.Data.RECIPELEN);
                //    }
                //}

                return RecipeDic;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                fill_Null_EQPPID = string.Empty;
                return null;
            }
        }

        public bool AnalysisMesPPID_CELL_PMT(XmlNode productNode, Line line, Port port, ref IList<RecipeCheckInfo> idRecipeInfos,
    ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string mesppid, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            mesppid = string.Empty;
            try
            {
                string eqPPID = string.Empty;
                // 如果OPI的部份有值時, 使用OPI的值
                if (string.IsNullOrEmpty(productNode[keyHost.OPI_PPID].InnerText.Trim()))
                    eqPPID = productNode[keyHost.PPID].InnerText;
                else
                    eqPPID = productNode[keyHost.OPI_PPID].InnerText;
                mesppid = eqPPID;

                string productRcpName = productNode[keyHost.PRODUCTRECIPENAME].InnerText;

                //Watson Add 避免沒有要check的機台還加入Recipe Check
                string eqFillnullPPID = string.Empty;
                Dictionary<string, string> EQP_RecipeNo = AddVirtualEQP_PPID(line, eqPPID, out eqFillnullPPID);

                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1}.", line.Data.LINEID, eqPPID);
                    return false;
                }

                foreach (string eqpNo in EQP_RecipeNo.Keys)
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        return false;
                    }

                    //RecipeID in EQP = EQP_RecipeNo[eqpNo]
                    if (EQP_RecipeNo[eqpNo].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            line.Data.LINEID, eqpNo, EQP_RecipeNo[eqpNo], eqp.Data.RECIPELEN);
                        return false;
                    }

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }
                }

                ppid = eqFillnullPPID;  
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        public bool AnalysisMesPPID_CELLNormal(XmlNode productNode, Line line, Port port, ref IList<RecipeCheckInfo> idRecipeInfos,
            ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string mesppid, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            mesppid = string.Empty;
            try
            {
                string eqPPID = string.Empty;
                // 如果OPI的部份有值時, 使用OPI的值
                if (string.IsNullOrEmpty(productNode[keyHost.OPI_PPID].InnerText.Trim()))
                {
                    if (productNode[keyHost.PPID] != null)
                        eqPPID = productNode[keyHost.PPID].InnerText;
                }
                else
                    eqPPID = productNode[keyHost.OPI_PPID].InnerText;
                mesppid = eqPPID;

                string productRcpName = productNode[keyHost.PRODUCTRECIPENAME].InnerText;

                //Watson Add 避免沒有要check的機台還加入Recipe Check
                string eqFillnullPPID = string.Empty;
                //Dictionary<string, string> EQP_RecipeNo = AddVirtualEQP_PPID(line, eqPPID, out eqFillnullPPID);
                //Add By Yangzhenteng 20190508
                Dictionary<string, string> EQP_RecipeNo = new Dictionary<string, string>();
                ParameterManager ParameterManager_ = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
                bool RecipeSpecialCheckFlag = ParameterManager_.ContainsKey("RecipeSpecialCheckFlag") ? ParameterManager_["RecipeSpecialCheckFlag"].GetBoolean() : false;
                if (RecipeSpecialCheckFlag && (line.Data.LINEID.Contains("CCCUT") || line.Data.LINEID.Contains("CCPOL")))
                {
                    EQP_RecipeNo = AddVirtualEQP_PPID_T3CELL_Special_Cassette(line, eqPPID, out eqFillnullPPID);
                }
                else
                {
                    EQP_RecipeNo = AddVirtualEQP_PPID(line, eqPPID, out eqFillnullPPID);
                }
                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1}.", line.Data.LINEID, eqPPID);
                    return false;
                }

                foreach (string eqpNo in EQP_RecipeNo.Keys)
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        errMsg += string.Format("MES/OPI RECIP ENAME=[{0}] LENGTH <>  LINENAME=[{1}] TOTAL EQP COUNT!",eqPPID ,line.Data.LINEID);
                        return false;
                    }

                    //RecipeID in EQP = EQP_RecipeNo[eqpNo]
                    if (EQP_RecipeNo[eqpNo].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            line.Data.LINEID, eqpNo, EQP_RecipeNo[eqpNo], eqp.Data.RECIPELEN);
                        return false;
                    }

                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    //string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN);

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }

                    //20161128 huangjiayin: Cell Local Mode Recipe Name被改到的，不做Recipe Parameter Check
                    if (line.File.HostMode == eHostMode.LOCAL && productRcpName != productNode[keyHost.OPI_PRODUCTRECIPENAME].InnerText)
                    { }
                    else                    
                    {
                        if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                        {
                            paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                        }
                    }
                }

                //job.PPID用於機台
                //因為機台PPID需要跨號補00，或虛擬機台等，所以與MES PPID不是一樣的值
                //mesPPID = L2:AA;L3:BB;L4:CC;L6:DD;L7:EE....job.PPID=AABB00CC00DDEE.....
                ppid = eqFillnullPPID;  //Watson Modify 20141126 寫入機台的ppid，任何跨機台都要寫入，ex: FCRPH Line 沒有這些機台L10, L15 and L20 
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        public bool AnalysisMesPPID_CELLPanel(XmlNode body, Line line, ref IList<RecipeCheckInfo> idRecipeInfos,
           ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string mesppid, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            mesppid = string.Empty;
            try
            {
                string eqPPID = string.Empty;
                    eqPPID = body[keyHost.PPID].InnerText;
                mesppid = eqPPID;

                string productRcpName = body[keyHost.PRODUCTRECIPENAME].InnerText;

                string eqFillnullPPID = string.Empty;
                //Dictionary<string, string> EQP_RecipeNo = AddVirtualEQP_PPID(line, eqPPID, out eqFillnullPPID);
                //Add By Yangzhenteng20190508
                Dictionary<string, string> EQP_RecipeNo = new Dictionary<string, string>();
                ParameterManager _parameterManager_ = Workbench.Instance.GetObject("ParameterManager") as ParameterManager; //Add By Yangzhenteng20190508;
                bool RecipeSpecialCheckFlag = _parameterManager_.ContainsKey("RecipeSpecialCheckFlag") ? _parameterManager_["RecipeSpecialCheckFlag"].GetBoolean() : false;
                if (RecipeSpecialCheckFlag && line.Data.LINEID.Contains("CCCUT") || line.Data.LINEID.Contains("CCPOL"))
                {
                    EQP_RecipeNo = AddVirtualEQP_PPID_T3CELL_Special_Panel(line, eqPPID, out eqFillnullPPID);
                }
                else
                {
                    EQP_RecipeNo = AddVirtualEQP_PPID(line, eqPPID, out eqFillnullPPID);
                }

                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1}.", line.Data.LINEID, eqPPID);
                    return false;
                }

                foreach (string eqpNo in EQP_RecipeNo.Keys)
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        errMsg += string.Format("MES/OPI RECIP ENAME=[{0}] LENGTH <>  LINENAME=[{1}] TOTAL EQP COUNT!", eqPPID, line.Data.LINEID);
                        return false;
                    }

                    //RecipeID in EQP = EQP_RecipeNo[eqpNo]
                    if (EQP_RecipeNo[eqpNo].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            line.Data.LINEID, eqpNo, EQP_RecipeNo[eqpNo], eqp.Data.RECIPELEN);
                        return false;
                    }

                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    //string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN);

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, 0, EQP_RecipeNo[eqpNo], productRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, 0, EQP_RecipeNo[eqpNo], productRcpName));
                    }
                }

                //job.PPID用於機台
                //因為機台PPID需要跨號補00，或虛擬機台等，所以與MES PPID不是一樣的值
                //mesPPID = L2:AA;L3:BB;L4:CC;L6:DD;L7:EE....job.PPID=AABB00CC00DDEE.....
                ppid = eqFillnullPPID;  //Watson Modify 20141126 寫入機台的ppid，任何跨機台都要寫入，ex: FCRPH Line 沒有這些機台L10, L15 and L20 
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        public bool AnalysisMesPPID_CFUPKLine(XmlDocument xmlDoc, Port port, ref IList<RecipeCheckInfo> idRecipeInfos, ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string errMsg)
        {

            errMsg = string.Empty;
            ppid = string.Empty;
            try
            {
                string lineRcpName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                string[] recipeIDs = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PPID].InnerText.Split(';');
                if (eqps.Count != recipeIDs.Length)
                {
                    errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                    return false;
                }

                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string eqpNo = "L" + (i + 2).ToString();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", "", eqpNo);
                        return false;
                    }

                    if (recipeIDs[i].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            "", eqpNo, recipeIDs[i], eqp.Data.RECIPELEN);
                        return false;
                    }

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i], lineRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i], lineRcpName));
                    }

                    ppid += recipeIDs[i].PadRight(eqp.Data.RECIPELEN);
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        public bool AnalysisMesPPID_CELL_CUT_1(XmlNode body, XmlNode lot, XmlNode productNode, Line line, Port port, ref IList<RecipeCheckInfo> idRecipeInfos,
            ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string crossPPID, out string mesPPID, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            mesPPID = string.Empty;
            crossPPID = string.Empty;
            string productRcpName = string.Empty;

            try
            {
                //IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                string eqCurrentPPID = string.Empty;
                string eqCrossPPID = string.Empty;
                // 如果OPI的部份有值時, 使用OPI的值
                if (string.IsNullOrEmpty(lot[keyHost.OPI_CURRENTLINEPPID].InnerText.Trim()))
                {
                    if (port.Data.NODENO == "L2")
                    {
                        #region L2 上Port PPID組成
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        if (lotPPID.Length < 2)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            return false;
                        }

                        XmlNodeList processList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                        if (processList.Count == 0)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of PROCESSLINELIST PPID is not enough.";
                            return false;
                        }

                        string[] processPPID = processList[0][keyHost.PPID].InnerText.Split(';');

                        for (int i = 0; i < 2; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }
                        for (int i = 2; i < processPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + processPPID[i] + ";";
                        }

                        if (processList.Count > 1)
                        {
                            string[] crossProcessID = processList[1][keyHost.PPID].InnerText.Split(';');

                            List<Equipment> processEQPs = ObjectManager.EquipmentManager.GetEQPsByLine(processList[1][keyHost.LINENAME].InnerText);
                            if (processEQPs == null)
                            {
                                errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                                return false;
                            }
                            if (processEQPs.Count != crossProcessID.Length)
                            {
                                //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                                //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                                //return false;
                            }

                            for (int i = 0; i < crossProcessID.Length; i++)
                            {
                                eqCrossPPID = eqCrossPPID + crossProcessID[i] + ";";
                            }
                        }

                        productRcpName = processList[0][keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                    else if (port.Data.NODENO == "L4" || port.Data.NODENO == "L8")
                    {
                        #region L4 上Port PPID組成
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');

                        for (int i = 0; i < lotPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }

                        productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                }
                else
                {
                    eqCurrentPPID = lot[keyHost.OPI_CURRENTLINEPPID].InnerText;
                    eqCrossPPID = lot[keyHost.OPI_CROSSLINEPPID].InnerText;
                    productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                }

                
                //Watson Add 20141126 For BCRECIPEID或BC的正在使用的PPID需帶在JOB中原job.PPID已用於機台
                //因為機台PPID需要跨號補00，或虛擬機台等，所以不會是一樣的值
                //mesPPID = L2:AA;L3:BB;L4:CC;L6:DD;L7:EE....job.PPID=AABB00CC00DDEE.....

                //Jun Modify 20141205
                if (eqCurrentPPID.Length > 0)
                {
                    if (eqCurrentPPID.Substring(eqCurrentPPID.Length - 1, 1) == ";")
                        eqCurrentPPID = eqCurrentPPID.Substring(0, eqCurrentPPID.Length - 1);
                }

                mesPPID = eqCurrentPPID;

                //Watson Add 避免沒有要check的機台還加入Recipe Check
                string bcFillnullPPID = string.Empty;
                Dictionary<string, string> EQP_RecipeNo = AddVirtualEQP_PPID(line, eqCurrentPPID, out bcFillnullPPID);
                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1} IS ERROR!!", line.Data.LINEID, eqCurrentPPID);
                    return false;
                }

                foreach (string eqpNo in EQP_RecipeNo.Keys)
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        return false;
                    }

                    if (EQP_RecipeNo[eqpNo].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            line.Data.LINEID, eqpNo, EQP_RecipeNo[eqpNo], eqp.Data.RECIPELEN);
                        return false;
                    }

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }
                }

                //job.PPID用於機台
                //因為機台PPID需要跨號補00，或虛擬機台等，所以跟MES PPID不是一樣的值
                //mesPPID = L2:AA;L3:BB;L4:CC;L6:DD;L7:EE....job.PPID=AABB00CC00DDEE.....
                ppid = bcFillnullPPID;

                if (eqCrossPPID.Length > 0)
                {
                    string[] crossRecipeIDs = eqCrossPPID.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    Dictionary<string, string> EQP_CrossRecipeNo = AddVirtualEQP_PPID(line, eqCrossPPID, out bcFillnullPPID);

                    if (EQP_CrossRecipeNo == null)
                    {
                        errMsg = string.Format("{0} CUT CROSS LINE PPID{1} IS ERROR!!", line.Data.LINEID, eqCrossPPID);
                        return false;
                    }
                    //job.PPID用於機台"AABB00CC00DDEE....."
                    //因為機台PPID需要跨號補00，或虛擬機台等，所以跟MES PPID不是一樣的值
                    //mesPPID = "L2:AA;L3:BB;L4:CC;L6:DD;L7:EE...." job.PPID="AABB00CC00DDEE....."
                    crossPPID = bcFillnullPPID;
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        public bool AnalysisMesPPID_CELL_CUT_2(XmlNode body, XmlNode lot, XmlNode productNode, Line line, Port port, ref IList<RecipeCheckInfo> idRecipeInfos,
            ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string crossPPID, out string mesPPID, out string errMsg)
        {
            mesPPID = string.Empty;
            errMsg = string.Empty;
            ppid = string.Empty;
            crossPPID = string.Empty;
            string productRcpName = string.Empty;

            try
            {
                //IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                string eqCurrentPPID = string.Empty;
                string eqCrossPPID = string.Empty;
                // 如果OPI的部份有值時, 使用OPI的值
                if ((string.IsNullOrEmpty(lot[keyHost.OPI_CURRENTLINEPPID].InnerText.Trim())) || (string.IsNullOrEmpty(lot[keyHost.OPI_CROSSLINEPPID].InnerText.Trim())))
                {
                    if (port.Data.NODENO == "L2")
                    {
                        #region L2 上Port 組PPID
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        if (lotPPID.Length < 2)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            return false;
                        }

                        XmlNodeList processList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                        if (processList.Count == 0)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of PROCESSLINELIST PPID is not enough.";
                            return false;
                        }

                        string[] processPPID = processList[0][keyHost.PPID].InnerText.Split(';');
                        for (int i = 0; i < 2; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }
                        for (int i = 0; i < processPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + processPPID[i] + ";";
                        }

                        XmlNodeList stbList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                        if (processList.Count > 1 && stbList.Count == 0)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of STBPRODUCTSPECLIST PPID is not enough.";
                            return false;
                        }

                        if (processList.Count > 1)
                        {
                            string[] crossProcessID = processList[1][keyHost.PPID].InnerText.Split(';');
                            string[] stbPPID = stbList[0][keyHost.PPID].InnerText.Split(';');

                            List<Equipment> processEQPs = ObjectManager.EquipmentManager.GetEQPsByLine(processList[1][keyHost.LINENAME].InnerText);
                            List<Equipment> stbEQPs = ObjectManager.EquipmentManager.GetEQPsByLine(stbList[0][keyHost.LINENAME].InnerText);
                            if (processEQPs == null || stbEQPs == null)
                            {
                                errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                                return false;
                            }
                            if ((processEQPs.Count + stbEQPs.Count) != (crossProcessID.Length + stbPPID.Length))
                            {
                                //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                                //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                                //return false;
                            }

                            for (int i = 0; i < crossProcessID.Length; i++)
                            {
                                eqCrossPPID = eqCrossPPID + crossProcessID[i] + ";";
                            }
                            for (int i = 0; i < stbPPID.Length; i++)
                            {
                                eqCrossPPID = eqCrossPPID + stbPPID[i] + ";";
                            }
                        }

                        productRcpName = processList[0][keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                    else if (port.Data.NODENO == "L4" || port.Data.NODENO == "L8")
                    {
                        #region L4 上Port PPID組成
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');

                        for (int i = 0; i < lotPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }

                        productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                }
                else
                {
                    eqCurrentPPID = lot[keyHost.OPI_CURRENTLINEPPID].InnerText;
                    eqCrossPPID = lot[keyHost.OPI_CROSSLINEPPID].InnerText;
                    productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                }

                //Watson Add 20141126 For BCRECIPEID或BC的正在使用的PPID需帶在JOB中原job.PPID已用於機台
                //因為機台PPID需要跨號補00，或虛擬機台等，所以不是一樣的值
                //mesPPID = L2:AA;L3:BB;L4:CC;L6:DD;L7:EE....job.PPID=AABB00CC00DDEE.....

                //Jun Modify 20141205
                if (eqCurrentPPID.Length > 0)
                {
                    if (eqCurrentPPID.Substring(eqCurrentPPID.Length - 1, 1) == ";")
                        eqCurrentPPID = eqCurrentPPID.Substring(0, eqCurrentPPID.Length - 1);
                }

                if (eqCrossPPID.Length > 0)
                {
                    if (eqCrossPPID.Substring(eqCrossPPID.Length - 1, 1) == ";")
                        eqCrossPPID = eqCrossPPID.Substring(0, eqCrossPPID.Length - 1);
                }

                mesPPID = eqCurrentPPID;

                string bcFillnullPPID = string.Empty;
                Dictionary<string, string> EQP_RecipeNo = AddVirtualEQP_PPID(line, eqCurrentPPID, out bcFillnullPPID);
                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1}.", line.Data.LINEID, eqCurrentPPID);
                    return false;
                }
                foreach (string eqpNo in EQP_RecipeNo.Keys)
                {
                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    //string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN);

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }
                }
                //job.PPID用於機台"AABB00CC00DDEE....."
                //因為機台PPID需要跨號補00，或虛擬機台等，所以跟MES PPID不是一樣的值
                //mesPPID = "L2:AA;L3:BB;L4:CC;L6:DD;L7:EE...." job.PPID="AABB00CC00DDEE....."
                ppid = bcFillnullPPID;


                if (eqCrossPPID.Length > 0)
                {
                    string[] crossRecipeIDs = eqCrossPPID.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    Dictionary<string, string> EQP_CrossRecipeNo = AddVirtualEQP_PPID_CrossLine(line, eqCrossPPID, out bcFillnullPPID);

                    if (EQP_CrossRecipeNo == null)
                    {
                        errMsg = string.Format("{0} CUT CROSS LINE PPID[{1}] IS ERROR!!", line.Data.LINEID, eqCrossPPID);
                        return false;
                    }
                    //job.PPID用於機台"AABB00CC00DDEE....."
                    //因為機台PPID需要跨號補00，或虛擬機台等，所以跟MES PPID不是一樣的值
                    //mesPPID = "L2:AA;L3:BB;L4:CC;L6:DD;L7:EE...." job.PPID="AABB00CC00DDEE....."
                    crossPPID = bcFillnullPPID;
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        public bool AnalysisMesPPID_CELL_CUT_3(XmlNode body, XmlNode lot, XmlNode productNode, Line line, Port port, ref IList<RecipeCheckInfo> idRecipeInfos,
            ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string mesPPID, out string errMsg)
        {
            mesPPID = string.Empty;
            errMsg = string.Empty;
            ppid = string.Empty;
            string productRcpName = string.Empty;

            try
            {
                string eqCurrentPPID = string.Empty;
                string eqCrossPPID = string.Empty;
                // 如果OPI的部份有值時, 使用OPI的值
                if (string.IsNullOrEmpty(lot[keyHost.OPI_CURRENTLINEPPID].InnerText.Trim()))
                {
                    if (port.Data.NODENO == "L2")
                    {
                        #region L2 上Port 組PPID
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        if (lotPPID.Length < 2)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            return false;
                        }

                        XmlNodeList processList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                        if (processList.Count == 0)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of PROCESSLINELIST PPID is not enough.";
                            return false;
                        }

                        //Jun Modify 20141223 因為CUT500有可能只跑到L9, STB就不會有資料, 所以先組成CUT線的, 然後再判斷是否要跑到POL, 再組POL資料
                        string[] processPPID = processList[0][keyHost.PPID].InnerText.Split(';');
                        for (int i = 0; i < 2; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }
                        for (int i = 2; i < processPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + processPPID[i] + ";";
                        }

                        XmlNodeList stbList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                        if (stbList.Count == 0)
                        {
                            //errMsg = "Cassette Data Transfer Error: The count of STBPRODUCTSPECLIST PPID is not enough.";
                            //return false;
                        }
                        else if (stbList.Count > 0)
                        {
                            string[] stbPPID = stbList[0][keyHost.PPID].InnerText.Split(';');
                            //if (eqps.Count != (processPPID.Length + stbPPID.Length))
                            //{
                            //    //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                            //    //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            //    //return false;
                            //}

                            for (int i = 0; i < stbPPID.Length; i++)
                            {
                                eqCurrentPPID = eqCurrentPPID + stbPPID[i] + ";";
                            }
                        }

                        productRcpName = processList[0][keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                    else if (port.Data.NODENO == "L4" || port.Data.NODENO == "L7")
                    {
                        #region L4 上Port 組PPID
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        for (int i = 0; i < lotPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }

                        //Jun Modify 20141223 因為CUT500有可能只跑到L9, STB就不會有資料, 所以先組成CUT線的, 然後再判斷是否要跑到POL, 再組POL資料
                        XmlNodeList stbList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                        if (stbList.Count == 0)
                        {
                            //errMsg = "Cassette Data Transfer Error: The count of STBPRODUCTSPECLIST PPID is not enough.";
                            //return false;
                        }
                        else if (stbList.Count > 0)
                        {
                            string[] stbPPID = stbList[0][keyHost.PPID].InnerText.Split(';');
                            //if (eqps.Count != (lotPPID.Length + stbPPID.Length))
                            //{
                            //    //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                            //    //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            //    //return false;
                            //}

                            for (int i = 0; i < stbPPID.Length; i++)
                            {
                                eqCurrentPPID = eqCurrentPPID + stbPPID[i] + ";";
                            }
                        }

                        productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                    else if (port.Data.NODENO == "L9")
                    {
                        #region L9 上Port 組PPID
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        if (lotPPID.Length < 9)
                        {
                            //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                            //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            //return false;
                        }

                        for (int i = 0; i < lotPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }

                        for (int i = lotPPID.Length; i < ObjectManager.EquipmentManager.GetEQPs().Count; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + "00;";
                        }

                        productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                    else if (port.Data.NODENO == "L11" || port.Data.NODENO == "L13" || port.Data.NODENO == "L14" || port.Data.NODENO == "L15")
                    {
                        #region L11 上Port 組PPID
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        if (lotPPID.Length < 5)
                        {
                            //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                            //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            //return false;
                        }

                        for (int i = 0; i < lotPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }
                        productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                }
                else
                {
                    eqCurrentPPID = lot[keyHost.OPI_CURRENTLINEPPID].InnerText;
                    productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                }

                //Watson Add 20141126 For BCRECIPEID或BC的正在使用的PPID需帶在JOB中原job.PPID已用於機台
                //因為機台PPID需要跨號補00，或虛擬機台等，所以不是一樣的值
                //mesPPID ="L2:AA;L3:BB;L4:CC;L6:DD;L7:EE...."  job.PPID= "AABB00CC00DDEE....."

                //Jun Modify 20141205
                if (eqCurrentPPID.Length > 0)
                {
                    if (eqCurrentPPID.Substring(eqCurrentPPID.Length - 1, 1) == ";")
                        eqCurrentPPID = eqCurrentPPID.Substring(0, eqCurrentPPID.Length - 1);
                }

                mesPPID = eqCurrentPPID;

                string bcFillnullPPID = string.Empty;
                Dictionary<string, string> EQP_RecipeNo = AddVirtualEQP_PPID(line, eqCurrentPPID, out bcFillnullPPID);

                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1} IS ERROR!!", line.Data.LINEID, eqCurrentPPID);
                    return false;
                }

                foreach (string eqpNo in EQP_RecipeNo.Keys)
                {
                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    //string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN);

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }

                }

                //job.PPID 用於機台
                //因為機台PPID需要跨號補00，或虛擬機台等，所以跟MES PPID (BCRECIPEID) 不是一樣的值
                //mesPPID = "L2:AA;L3:BB;L4:CC;L6:DD;L7:EE...." job.PPID="AABB00CC00DDEE....."
                ppid = bcFillnullPPID;
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        public bool AnalysisMesPPID_CELLBOX(XmlNode body, XmlNode productNode, Line line, Port port, ref IList<RecipeCheckInfo> idRecipeInfos,
    ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string mesppid, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            mesppid = string.Empty;
            try
            {
                string eqPPID = string.Empty;
                // 如果OPI的部份有值時, 使用OPI的值
                if (string.IsNullOrEmpty(productNode[keyHost.OPI_PPID].InnerText.Trim()))
                    if (productNode[keyHost.PPID] == null)
                        eqPPID = body[keyHost.BOXLIST][keyHost.BOX][keyHost.PPID].InnerText;
                    else
                        eqPPID = productNode[keyHost.PPID].InnerText;
                else
                    eqPPID = productNode[keyHost.OPI_PPID].InnerText;
                mesppid = eqPPID;

                //string productRcpName = string.Empty;
                string productRcpName = body[keyHost.LINERECIPENAME].InnerText;//sy add 20151207 

                //Watson Add 避免沒有要check的機台還加入Recipe Check
                string eqFillnullPPID = string.Empty;
                Dictionary<string, string> EQP_RecipeNo = AddVirtualEQP_PPID(line, eqPPID, out eqFillnullPPID);

                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1} IS ERROR!!", line.Data.LINEID, eqPPID);
                    return false;
                }

                foreach (string eqpNo in EQP_RecipeNo.Keys)
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        return false;
                    }

                    //RecipeID in EQP = EQP_RecipeNo[eqpNo]
                    if (EQP_RecipeNo[eqpNo].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            line.Data.LINEID, eqpNo, EQP_RecipeNo[eqpNo], eqp.Data.RECIPELEN);
                        return false;
                    }

                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    //string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN);

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }
                }

                //job.PPID用於機台
                //因為機台PPID需要跨號補00，或虛擬機台等，所以與MES PPID不是一樣的值
                //mesPPID = L2:AA;L3:BB;L4:CC;L6:DD;L7:EE....job.PPID=AABB00CC00DDEE.....
                ppid = eqFillnullPPID;  //Watson Modify 20141126 寫入機台的ppid，任何跨機台都要寫入，ex: FCRPH Line 沒有這些機台L10, L15 and L20 
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        public bool AnalysisMesPPID_CELLOUTBOX(XmlNode body, Line line, Port port, ref IList<RecipeCheckInfo> idRecipeInfos,
    ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string mesppid, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            mesppid = string.Empty;
            try
            {

                mesppid =  body[keyHost.BOXLIST][keyHost.BOX][keyHost.PPID].InnerText;

                //string productRcpName = string.Empty;
                string productRcpName = body[keyHost.LINERECIPENAME].InnerText;//sy add 20151207 

                //Watson Add 避免沒有要check的機台還加入Recipe Check
                string eqFillnullPPID = string.Empty;
                Dictionary<string, string> EQP_RecipeNo = AddVirtualEQP_PPID(line, mesppid, out eqFillnullPPID);

                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1} IS ERROR!!", line.Data.LINEID, mesppid);
                    return false;
                }

                foreach (string eqpNo in EQP_RecipeNo.Keys)
                {
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        return false;
                    }

                    //RecipeID in EQP = EQP_RecipeNo[eqpNo]
                    if (EQP_RecipeNo[eqpNo].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            line.Data.LINEID, eqpNo, EQP_RecipeNo[eqpNo], eqp.Data.RECIPELEN);
                        return false;
                    }

                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    //string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN);

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpNo])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpNo], productRcpName));
                    }
                }

                //job.PPID用於機台
                //因為機台PPID需要跨號補00，或虛擬機台等，所以與MES PPID不是一樣的值
                //mesPPID = L2:AA;L3:BB;L4:CC;L6:DD;L7:EE....job.PPID=AABB00CC00DDEE.....
                ppid = eqFillnullPPID;  //Watson Modify 20141126 寫入機台的ppid，任何跨機台都要寫入，ex: FCRPH Line 沒有這些機台L10, L15 and L20 
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        #region 原AnalysisMesPPID Backup

        public bool AnalysisMesPPID_AC_Backup(XmlNode productNode, Line line, Port port, string processType, ref IList<RecipeCheckInfo> idRecipeInfos,
            ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            try
            {
                IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                string eqPPID = string.Empty;
                // 如果OPI的部份有值時, 使用OPI的值 
                if (string.IsNullOrEmpty(productNode[keyHost.OPI_PPID].InnerText.Trim()))
                    eqPPID = productNode[keyHost.PPID].InnerText;
                else
                    eqPPID = productNode[keyHost.OPI_PPID].InnerText;

                string[] recipeIDs = eqPPID.Split(';');
                ////Watson add 20141118 For MES Spec Virtual Machine or Jump EQP No
                //recipeIDs = AddVirtualEQP_PPID(eqPPID, recipeIDs);
                int eqpcount = eqps.Count;
                if (line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW)
                    eqpcount++;

                if (eqpcount != recipeIDs.Length)
                {
                    errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                    return false;

                }

                string productRcpName = productNode[keyHost.PRODUCTRECIPENAME].InnerText;

                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string eqpNo = "L" + (i + 2).ToString();
                    if (i == 3 && (line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ELA_JSW))
                    {
                        eqpNo = "L3";
                    }

                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        return false;
                    }

                    if (recipeIDs[i].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            line.Data.LINEID, eqpNo, recipeIDs[i], eqp.Data.RECIPELEN);
                        return false;
                    }

                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    // string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN); 

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        // Array TBPHL 曝光機為CANON且Product Process Type為MMG時, 要分兩筆給CANON
                        if (eqp.Data.NODEATTRIBUTE.Equals("CANON"))
                        {
                            idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i].Substring(0, 4)/* recipe.Substring(0, 4)*/, productRcpName));

                            if (processType.Equals("MMG"))
                            {
                                idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i].Substring(4, 4) /*recipe.Substring(4, 4)*/, productRcpName));
                            }
                        }
                        else if (eqp.Data.NODEATTRIBUTE.Equals("NIKON"))
                        {
                            idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i].Substring(0, 4)/* recipe.Substring(0, 4)*/, productRcpName));

                        }
                        else
                        {
                            idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i]/* recipe*/, productRcpName));
                        }
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        // Array TBPHL 曝光機為CANON且Product Process Type為MMG時, 要分兩筆給CANON
                        if (eqp.Data.NODEATTRIBUTE.Equals("CANON"))
                        {
                            paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i].Substring(0, 4)/*recipe.Substring(0, 4)*/, productRcpName));
                            if (processType.Equals("MMG"))
                            {
                                paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i].Substring(0, 4)/* recipe.Substring(0, 4)*/, productRcpName));
                            }
                        }
                        else if (eqp.Data.NODEATTRIBUTE.Equals("NIKON"))
                        {
                            paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i].Substring(0, 4)/*recipe.Substring(0, 4)*/, productRcpName));

                        }
                        else
                        {
                            paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i]/*recipe*/, productRcpName));
                        }
                    }

                    ppid += recipeIDs[i];
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        public bool AnalysisMesPPID_CELLNormal_Backup(XmlNode productNode, Line line, Port port, ref IList<RecipeCheckInfo> idRecipeInfos,
    ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            try
            {
                IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                string eqPPID = string.Empty;
                // 如果OPI的部份有值時, 使用OPI的值
                if (string.IsNullOrEmpty(productNode[keyHost.OPI_PPID].InnerText.Trim()))
                    eqPPID = productNode[keyHost.PPID].InnerText;
                else
                    eqPPID = productNode[keyHost.OPI_PPID].InnerText;

                string[] recipeIDs = eqPPID.Split(';');
                //Watson add 20141118 For MES Spec Virtual Machine or Jump EQP No
                //recipeIDs = AddVirtualEQP_PPID(eqPPID, recipeIDs);

                if (eqps.Count != recipeIDs.Length)
                {
                    errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                    return false;
                }

                string productRcpName = productNode[keyHost.PRODUCTRECIPENAME].InnerText;

                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string eqpNo = "L" + (i + 2).ToString();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        return false;
                    }

                    if (recipeIDs[i].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            line.Data.LINEID, eqpNo, recipeIDs[i], eqp.Data.RECIPELEN);
                        return false;
                    }

                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    //string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN);

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i], productRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i], productRcpName));
                    }

                    ppid += recipeIDs[i];
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }
        public bool AnalysisMesPPID_CELL_CUT_1_Backup(XmlNode body, XmlNode lot, XmlNode productNode, Line line, Port port, ref IList<RecipeCheckInfo> idRecipeInfos,
            ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            string productRcpName = string.Empty;

            try
            {
                IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                string eqCurrentPPID = string.Empty;
                string eqCrossPPID = string.Empty;
                // 如果OPI的部份有值時, 使用OPI的值
                if (string.IsNullOrEmpty(lot[keyHost.OPI_CURRENTLINEPPID].InnerText.Trim()))
                {
                    if (port.Data.NODENO == "L2")
                    {
                        #region L2 上Port PPID組成
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        if (lotPPID.Length < 2)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            return false;
                        }

                        XmlNodeList processList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                        if (processList.Count == 0)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            return false;
                        }

                        string[] processPPID = processList[0][keyHost.PPID].InnerText.Split(';');
                        if (eqps.Count != processPPID.Length)
                        {
                            //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                            //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            //return false;
                        }

                        for (int i = 0; i < 2; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }
                        for (int i = 2; i < processPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + processPPID[i] + ";";
                        }

                        productRcpName = processList[0][keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                    else if (port.Data.NODENO == "L4" || port.Data.NODENO == "L8")
                    {
                        #region L4 上Port PPID組成
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        if (eqps.Count != lotPPID.Length)
                        {
                            //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            //return false;
                        }

                        for (int i = 0; i < lotPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }

                        productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                }
                else
                {
                    eqCurrentPPID = lot[keyHost.OPI_CURRENTLINEPPID].InnerText;
                    productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                }

                string[] recipeIDs = eqCurrentPPID.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string eqpNo = "L" + (i + 2).ToString();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        return false;
                    }

                    if (recipeIDs[i].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            line.Data.LINEID, eqpNo, recipeIDs[i], eqp.Data.RECIPELEN);
                        return false;
                    }

                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    //string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN);

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i], productRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i], productRcpName));
                    }

                    ppid += recipeIDs[i];
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }
        public bool AnalysisMesPPID_CELL_CUT_2_Backup(XmlNode body, XmlNode lot, XmlNode productNode, Line line, Port port, ref IList<RecipeCheckInfo> idRecipeInfos,
            ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string crossPPID, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            crossPPID = string.Empty;
            string productRcpName = string.Empty;

            try
            {
                IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                string eqCurrentPPID = string.Empty;
                string eqCrossPPID = string.Empty;
                // 如果OPI的部份有值時, 使用OPI的值
                if ((string.IsNullOrEmpty(lot[keyHost.OPI_CURRENTLINEPPID].InnerText.Trim())) || (string.IsNullOrEmpty(lot[keyHost.OPI_CROSSLINEPPID].InnerText.Trim())))
                {
                    if (port.Data.NODENO == "L2")
                    {
                        #region L2 上Port 組PPID
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        if (lotPPID.Length < 2)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            return false;
                        }

                        XmlNodeList processList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                        if (processList.Count == 0)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of PROCESSLINELIST PPID is not enough.";
                            return false;
                        }

                        string[] processPPID = processList[0][keyHost.PPID].InnerText.Split(';');
                        for (int i = 0; i < 2; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }
                        for (int i = 2; i < processPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + processPPID[i] + ";";
                        }

                        XmlNodeList stbList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                        if (processList.Count > 1 && stbList.Count == 0)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of STBPRODUCTSPECLIST PPID is not enough.";
                            return false;
                        }

                        if (processList.Count > 1)
                        {
                            string[] crossProcessID = processList[1][keyHost.PPID].InnerText.Split(';');
                            string[] stbPPID = stbList[0][keyHost.PPID].InnerText.Split(';');

                            List<Equipment> processEQPs = ObjectManager.EquipmentManager.GetEQPsByLine(processList[1][keyHost.LINENAME].InnerText);
                            List<Equipment> stbEQPs = ObjectManager.EquipmentManager.GetEQPsByLine(stbList[0][keyHost.LINENAME].InnerText);
                            if (processEQPs == null || stbEQPs == null)
                            {
                                errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                                return false;
                            }
                            if ((processEQPs.Count + stbEQPs.Count) != (crossProcessID.Length + stbPPID.Length))
                            {
                                //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                                //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                                //return false;
                            }

                            for (int i = 0; i < processPPID.Length; i++)
                            {
                                eqCrossPPID = eqCrossPPID + processPPID[i] + ";";
                            }
                            for (int i = processPPID.Length; i < stbPPID.Length; i++)
                            {
                                eqCrossPPID = eqCrossPPID + stbPPID[i] + ";";
                            }
                        }

                        productRcpName = processList[0][keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                    else if (port.Data.NODENO == "L4" || port.Data.NODENO == "L8")
                    {
                        #region L4 上Port PPID組成
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');

                        for (int i = 0; i < lotPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }

                        productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                }
                else
                {
                    eqCurrentPPID = lot[keyHost.OPI_CURRENTLINEPPID].InnerText;
                    eqCrossPPID = lot[keyHost.OPI_CROSSLINEPPID].InnerText;
                    productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                }

                string[] recipeIDs = eqCurrentPPID.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string eqpNo = "L" + (i + 2).ToString();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        return false;
                    }

                    if (recipeIDs[i].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            line.Data.LINEID, eqpNo, recipeIDs[i], eqp.Data.RECIPELEN);
                        return false;
                    }

                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    //string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN);

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i], productRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i], productRcpName));
                    }

                    ppid += recipeIDs[i];
                }

                if (eqCrossPPID.Length > 0)
                {
                    string[] crossRecipeIDs = eqCrossPPID.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    //Watson add 20141118 For MES Spec Virtual Machine or Jump EQP No
                    //crossRecipeIDs = Add_CELL_CUTMAX_PPID(eqCrossPPID, crossRecipeIDs);

                    for (int i = 0; i < crossRecipeIDs.Length; i++)
                    {
                        //string eqpNo = "L" + (i + 10).ToString();
                        //Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        //if (eqp == null)
                        //{
                        //    errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        //    return false;
                        //}

                        //if (recipeIDs[i].Trim().Length > eqp.Data.RECIPELEN)
                        //{
                        //    errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                        //        line.Data.LINEID, eqpNo, recipeIDs[i], eqp.Data.RECIPELEN);
                        //    return false;
                        //}

                        //string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN);

                        crossPPID += recipeIDs[i];
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }
        public bool AnalysisMesPPID_CELL_CUT_3_Backup(XmlNode body, XmlNode lot, XmlNode productNode, Line line, Port port, ref IList<RecipeCheckInfo> idRecipeInfos,
            ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            string productRcpName = string.Empty;

            try
            {
                IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                string eqCurrentPPID = string.Empty;
                string eqCrossPPID = string.Empty;
                // 如果OPI的部份有值時, 使用OPI的值
                if (string.IsNullOrEmpty(lot[keyHost.OPI_CURRENTLINEPPID].InnerText.Trim()))
                {
                    if (port.Data.NODENO == "L2")
                    {
                        #region L2 上Port 組PPID
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        if (lotPPID.Length < 2)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            return false;
                        }

                        XmlNodeList processList = lot[keyHost.PROCESSLINELIST].ChildNodes;
                        if (processList.Count == 0)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            return false;
                        }

                        XmlNodeList stbList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                        if (stbList.Count == 0)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            return false;
                        }

                        string[] processPPID = processList[0][keyHost.PPID].InnerText.Split(';');
                        string[] stbPPID = stbList[0][keyHost.PPID].InnerText.Split(';');
                        if (eqps.Count != (processPPID.Length + stbPPID.Length))
                        {
                            //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                            //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            //return false;
                        }

                        for (int i = 0; i < 2; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }
                        for (int i = 2; i < processPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + processPPID[i] + ";";
                        }
                        for (int i = processPPID.Length; i < stbPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + stbPPID[i] + ";";
                        }

                        productRcpName = processList[0][keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                    else if (port.Data.NODENO == "L4" || port.Data.NODENO == "L7")
                    {
                        #region L4 上Port 組PPID
                        XmlNodeList stbList = lot[keyHost.STBPRODUCTSPECLIST].ChildNodes;
                        if (stbList.Count == 0)
                        {
                            errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            return false;
                        }

                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        string[] stbPPID = stbList[0][keyHost.PPID].InnerText.Split(';');
                        if (eqps.Count != (lotPPID.Length + stbPPID.Length))
                        {
                            //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                            //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            //return false;
                        }

                        for (int i = 0; i < lotPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }
                        for (int i = lotPPID.Length; i < stbPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + stbPPID[i] + ";";
                        }

                        productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                    else if (port.Data.NODENO == "L9")
                    {
                        #region L9 上Port 組PPID
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        if (lotPPID.Length < 9)
                        {
                            //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                            //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            //return false;
                        }

                        for (int i = 0; i < lotPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }
                        for (int i = lotPPID.Length; i < eqps.Count; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + "00;";
                        }

                        productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                    else if (port.Data.NODENO == "L11" || port.Data.NODENO == "L13" || port.Data.NODENO == "L14" || port.Data.NODENO == "L15")
                    {
                        #region L11 上Port 組PPID
                        string[] lotPPID = lot[keyHost.PPID].InnerText.Split(';');
                        if (lotPPID.Length < 5)
                        {
                            //Watson 20141118 Modify MES not download Virtual Machine and Discontinuous EQPNo 
                            //errMsg = "Cassette Data Transfer Error: The count of MES PPID is not enough.";
                            //return false;
                        }

                        //for (int i = 0; i < (eqps.Count - lotPPID.Length); i++)
                        //{
                        //    eqCurrentPPID = eqCurrentPPID + "00;";
                        //}
                        //for (int i = 0; i < lotPPID.Length; i++)
                        //{
                        //    eqCurrentPPID = eqCurrentPPID + lotPPID[i + (eqps.Count - lotPPID.Length)] + ";";
                        //}
                        for (int i = 0; i < lotPPID.Length; i++)
                        {
                            eqCurrentPPID = eqCurrentPPID + lotPPID[i] + ";";
                        }
                        productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                        #endregion
                    }
                }
                else
                {
                    eqCurrentPPID = lot[keyHost.OPI_CURRENTLINEPPID].InnerText;
                    productRcpName = lot[keyHost.LINERECIPENAME].InnerText;
                }

                string[] recipeIDs = eqCurrentPPID.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                //Watson add 20141118 For MES Spec Virtual Machine or Jump EQP No
                //recipeIDs = AddVirtualEQP_PPID(eqCurrentPPID, recipeIDs);

                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string eqpNo = "L" + (i + 2).ToString();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: Cannot found Equipment Object, Line Name={0}, Equipment No={1}.", line.Data.LINEID, eqpNo);
                        return false;
                    }

                    if (recipeIDs[i].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("Cassette Data Transfer Error: MES PPID Length Error, Line Name={0} Equipment No={1} PPID={2} > Recipe Len={3}.",
                            line.Data.LINEID, eqpNo, recipeIDs[i], eqp.Data.RECIPELEN);
                        return false;
                    }

                    //登京说了MES Download的 PPID 会补Recipe 20141118 Tom
                    //string recipe = recipeIDs[i].PadRight(eqp.Data.RECIPELEN);

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i], productRcpName));
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(recipeIDs[i])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), recipeIDs[i], productRcpName));
                    }

                    ppid += recipeIDs[i];

                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// For CELL Cut MAX Line Cross Line PPID
        /// MES Download PPID will lost  Virtual Machine and Discontinuous EQP No
        /// BC Auto Add Eqpno Recipe ID in PPID.
        /// </summary>
        /// <param name="mesPPID">MES PPID :L2:AA;L3:BB;L4:CC;L5:DD;L6:EE;L7:FF;L8:GG....</param>
        /// <param name="recipeIDs">{L2:AA,L3:BB,L4:CC,L5:DD,L6:EE,L7:FF,L8:GG...}</param>
        /// <returns>{AA,BB,CC,DD,EE,FF,00,GG}</returns>
        private string[] Add_CELL_CUTMAX_PPID(string mesPPID, string[] recipeIDs)
        {
            try
            {
                //L2:AA;L3:BB;L4:CC;L5:DD...
                //L2~L15
                string[] rr = new string[14]; //HOT Code Cut Max Line L2~L15
                Dictionary<string, string> RecipeDic = new Dictionary<string, string>();
                for (int i = 0; i < recipeIDs.Length; i++)  //MES Download 多少就加多少
                {
                    string[] eqreci = recipeIDs[i].Split(':'); //L2:AA
                    RecipeDic.Add(eqreci[0], eqreci[1]);
                }

                for (int j = 2; j < 16; j++)  //HOT Code Cut Max Line L2~L15
                {
                    string eqpNo = "L" + j.ToString();
                    if (!RecipeDic.ContainsKey(eqpNo))
                    {
                        RecipeDic.Add(eqpNo, "00");  //hot code recipe length = 2 (CELL Recipe length always = 2) 
                    }
                    rr[j] = RecipeDic[eqpNo];
                }
                return rr;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return recipeIDs;
            }
        }
        #endregion

        #region UPK Line Create Job Data
        public void UPK_CREATE_JOBDataFile(XmlDocument xmlDoc, Equipment eqp, Port port, Line line, ref IList<RecipeCheckInfo> idRecipeInfos,
            ref IList<RecipeCheckInfo> paraRecipeInfos, ref IList<Job> jobs)
        {
            try
            {
                string err = string.Empty;
                string lineNo = string.Empty;
                string jobID = string.Empty;
                string ppID = string.Empty;
                string mesPPID = string.Empty;

                // 獲取MES Body層資料
                XmlNode body = xmlDoc.SelectSingleNode("//MESSAGE/BODY");

                //解析 PPID
                if (!ObjectManager.JobManager.AnalysisMesPPID_AC(xmlDoc, line, port, "", ref idRecipeInfos, ref paraRecipeInfos, out ppID, out mesPPID, out err))
                {
                    throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                }

                if(line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1)
                    lineNo = "1";

                #region 產生ProductType
                List<string> items = new List<string>();
                //items.Add(keyHost.PLANNEDPRODUCTSPECNAME + "_" + body[keyHost.PLANNEDPRODUCTSPECNAME].InnerText);
                //items.Add(keyHost.PLANNEDPROCESSOPERATIONNAME + "_" + body[keyHost.PLANNEDPROCESSOPERATIONNAME].InnerText);
                //items.Add(keyHost.UPKOWNERTYPE + "_" + body[keyHost.UPKOWNERTYPE].InnerText);
                items.Add(keyHost.PLANNEDSOURCEPART + "_" + body[keyHost.PLANNEDSOURCEPART].InnerText);
                items.Add(keyHost.PLANNEDPCPLANID + "_" + body[keyHost.PLANNEDPCPLANID].InnerText);// qiumin 20170324 PRODUCT type create add planID
                #endregion

                for (int i = 1; i <= int.Parse(port.File.ProductQuantity); i++)
                {
                    Job job = new Job(int.Parse(port.File.CassetteSequenceNo), i); //Slot 流水號從1開始

                    if (job == null)
                        continue;

                    jobID = lineNo + DateTime.Now.ToString("yyyyMMdd") + port.File.CassetteSequenceNo.PadLeft(5, '0') + i.ToString().PadLeft(4, '0');
                    job.MesProduct.PRODUCTNAME = jobID;
                    job.GroupIndex = "0";
                    job.CSTOperationMode = eqp.File.CSTOperationMode;
                    job.SubstrateType = eSubstrateType.Glass; //機台只接受Glass
                    job.CIMMode = eqp.File.CIMMode;
                    //job.MesCstBody.LOTLIST[0].LOTNAME = "UPK" + DateTime.Now.ToString("yyMMddHHmmss");// Add by qiumin for FDC upk lotname 20170613
                    
                    switch (port.File.Mode)
                    {
                        case ePortMode.TFT:
                            job.JobType = eJobType.TFT;
                            break;
                        case ePortMode.CF:
                            job.JobType = eJobType.CF;
                            break;
                        default:
                            job.JobType = eJobType.DM;
                            break;
                    }

                    job.JobJudge = "1";
                    job.JobGrade = "OK";
                    job.SamplingSlotFlag = "0";
                    job.FirstRunFlag = "0";
                    job.GlassChipMaskBlockID = jobID;
                    job.PPID = ppID;
                    job.MesProduct.PPID = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.PPID].InnerText;
                    job.MesProduct.DENSEBOXID = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.CARRIERNAME].InnerText;
                    

                    if (i == int.Parse(port.File.PlannedQuantity))
                        job.LastGlassFlag = "1";
                    // 初始化參數內容 20160405 Modify by Frank
                    job.EQPReservations = SpecialItemInitial("EQPReservations", 6);
                    job.INSPReservations = SpecialItemInitial("INSPReservations", 6);
                    job.InspJudgedData = SpecialItemInitial("Insp.JudgedData", 16);
                    job.TrackingData = SpecialItemInitial("TrackingData", 16);
                    job.CFSpecialReserved = SpecialItemInitial("CFSpecialReserved", 16);
                    job.EQPFlag = SpecialItemInitial("EQPFlag", 32);

                    //job.INSPReservations = "";
                    //job.EQPReservations = "";
                    //job.InspJudgedData = "";
                    //job.CFSpecialReserved = "";
                    //job.TrackingData = "";
                    //job.EQPFlag = "";
                    job.ChipCount = 310; //2015/8/26 modify by Frank T3長度固定310
                    //job.OXRInformation = "OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO"; //ZY答覆報O即可
                    //job.OXRInformationRequestFlag = job.ChipCount > 56 ? "1" : "0";
                    job.CfSpecial.COAversion = "";

                    job.SourcePortID = port.Data.PORTID;
                    job.TargetPortID = "0";
                    job.FromCstID = port.File.CassetteID;
                    job.ToCstID = string.Empty;
                    job.FromSlotNo = (i).ToString();
                    job.ToSlotNo = "0";
                    job.CurrentSlotNo = "0";    // add by bruce 20160412 for T2 Issue
                    job.CurrentEQPNo = eqp.Data.NODENO;
                    job.LineRecipeName = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.LINERECIPENAME].InnerText;
                    job.ProductType.SetItemData(items);

                    if (!ObjectManager.JobManager.GetProductType(eFabType.CF, "", jobs, job, out err))
                    {
                        throw new CassetteMapException(port.Data.NODENO, port.Data.PORTNO, err);
                    }
                    #endregion

                    job.HostDefectCodeData = "";
                    job.CfSpecial.PlannedSourcePart = port.File.PlannedSourcePart;
                    job.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID = "";
                    job.CfSpecial.UPKOWNERTYPE = xmlDoc[keyHost.MESSAGE][keyHost.BODY][keyHost.UPKOWNERTYPE].InnerText;

                    // 填入 MES 資料
                    ObjectManager.JobManager.ConvertXMLToObject(job.MesCstBody, body);

                    jobs.Add(job);

                }

                if (jobs.Count > 0)
                {
                    Job tmpJob = jobs.FirstOrDefault(j => j.CassetteSequenceNo == port.File.CassetteSequenceNo);
                    if (tmpJob != null)
                        port.File.ProductType = tmpJob.ProductType.Value.ToString();
                }
                else
                {
                    Job j = new Job();
                    j.ProductType.SetItemData(items);
                    port.File.ProductType = j.ProductType.Value.ToString();
                    DeleteJob(j);
                }
                
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private string SpecialItemInitial(string itemName, int defaultLen)
        {
            try
            {
                int len = ObjectManager.SubJobDataManager.GetItemLenth(itemName);
                if (len != 0)
                    return new string('0', len);
                else
                    return new string('0', defaultLen);
            }
            catch
            {
                return new string('0', defaultLen);
            }
        }


        #region Job History
        /// <summary>
        /// Save JobHistory by All Shop (Validate Cassette) eJobEvent
        /// </summary>
        /// <param name="jobs"> Job List </param>
        /// <param name="eventname"> event Name </param>
        private void Job2History(IList<Job> jobs, string nodeID, string nodeNo, string unitNo, string portNo, string slotNo, string eventName, string transactionID, string vcrNo,string notChipID)
        {
            try
            {
                if (jobs != null)
                {
                    IList<JOBHISTORY> jobHistorys = new List<JOBHISTORY>();
                    foreach (Job job in jobs)
                    {
                        int i = 0;
                        JOBHISTORY jobHis = new JOBHISTORY();

                        jobHis.UPDATETIME = DateTime.Now;
                        jobHis.EVENTNAME = eventName;
                        jobHis.CASSETTESEQNO = int.TryParse(job.CassetteSequenceNo, out i) == true ? i : 0;
                        jobHis.JOBSEQNO = int.TryParse(job.JobSequenceNo, out i) == true ? i : 0;
                        jobHis.JOBID = job.GlassChipMaskBlockID;
                        jobHis.GROUPINDEX = int.TryParse(job.GroupIndex, out i) == true ? i : 0;
                        jobHis.PRODUCTTYPE = job.ProductType.Value;
                        jobHis.CSTOPERATIONMODE = job.CSTOperationMode.ToString();
                        jobHis.SUBSTRATETYPE = job.SubstrateType.ToString();
                        jobHis.CIMMODE = job.CIMMode.ToString();
                        jobHis.JOBTYPE = job.JobType.ToString();
                        jobHis.JOBJUDGE = GetProductJudge(nodeNo, job);
                        jobHis.SAMPLINGSLOTFLAG = job.SamplingSlotFlag;
                        jobHis.OXRINFORMATIONREQUESTFLAG = job.OXRInformationRequestFlag;
                        jobHis.FIRSTRUNFLAG = job.FirstRunFlag;
                        jobHis.JOBGRADE = job.JobGrade;
                        jobHis.PPID = job.PPID;
                        jobHis.INSPRESERVATIONS = job.INSPReservations;
                        jobHis.LASTGLASSFLAG = job.LastGlassFlag;
                        jobHis.INSPJUDGEDDATA = job.InspJudgedData;
                        jobHis.TRACKINGDATA = job.TrackingData;
                        jobHis.EQPFLAG = job.EQPFlag;
                        jobHis.OXRINFORMATION = job.OXRInformation;
                        jobHis.CHIPCOUNT = job.ChipCount;
                        jobHis.NODENO = nodeNo;
                        jobHis.UNITNO = unitNo;
                        jobHis.PORTNO = portNo;
                        if (eventName == eJobEvent.Delete_CST_Complete.ToString())
                            jobHis.SLOTNO = job.ToSlotNo; // Add by Kasim 20150508
                        else
                            jobHis.SLOTNO = slotNo == string.Empty ? job.FromSlotNo : slotNo;
                        jobHis.NODEID = nodeID;
                        jobHis.SOURCECASSETTEID = job.FromCstID;
                        jobHis.CURRENTCASSETTEID = job.ToCstID;
                        jobHis.PATHNO = string.Empty;  //目前不知道要填什麼
                        jobHis.VCRREADGLASSID = job.VCRJobID;                        
                        jobHis.COAVERSION = job.CfSpecial.COAversion;
                        jobHis.SAMPLINGVALUE = job.CfSpecial.SamplingValue;
                        jobHis.TARGETCASSETTEID = job.TargetCSTID;
                        jobHis.TRANSACTIONID = transactionID;

                        Unit unit = ObjectManager.UnitManager.GetUnit(nodeNo, unitNo);
                        if (unit != null)
                        {
                            jobHis.UNITID = unit.Data.UNITID;
                        }

                        if (vcrNo != string.Empty)
                        {
                            jobHis.VCRNO = vcrNo;
                            jobHis.VCRRESULT = job.VCR_Result.ToString();
                        }

                        if (notChipID != string.Empty)
                        {
                            jobHis.CHIPNAME = job.GlassChipMaskBlockID;
                            jobHis.JOBID = notChipID;
                        }

                        if (job.CellSpecial != null)
                        {
                            jobHis.TARGETCASSETTESETTINGCODE = job.CellSpecial == null ? string.Empty : job.CellSpecial.CassetteSettingCode;
                            jobHis.TURNANGLE = job.CellSpecial == null ? string.Empty : job.CellSpecial.TurnAngle;
                        }

                        if (job.MesCstBody != null)
                        {
                            #region ABNORMALCODE以逗號分開
                            for (int ii = 0; ii < job.MesProduct.ABNORMALCODELIST.Count;ii++ )
                            {
                                if (i ==  job.MesProduct.ABNORMALCODELIST.Count-1)
                                    jobHis.ABNORMALCODE += job.MesProduct.ABNORMALCODELIST[ii].ABNORMALCODE;
                                else
                                    jobHis.ABNORMALCODE += job.MesProduct.ABNORMALCODELIST[ii].ABNORMALCODE + ",";
                            }
                            #endregion

                            jobHis.RUNMODE = job.MesProduct.CHAMBERRUNMODE;
                            jobHis.PRODUCTNAME = job.MesProduct.PRODUCTNAME;
                            jobHis.GROUPID = job.MesProduct.GROUPID;
                            jobHis.OWNERTYPE = job.MesProduct.OWNERTYPE;
                        }

                        if (job.MesCstBody.LOTLIST.Count >= 1)
                        {
                            jobHis.PRODUCTSPECNAME = job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME;
                            jobHis.PRODUCTSPECVER = job.MesCstBody.LOTLIST[0].PRODUCTSPECVER;
                            jobHis.PROCESSFLOWNAME = job.MesCstBody.LOTLIST[0].PROCESSFLOWNAME;
                            jobHis.PROCESSOPERATIONNAME = job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME;
                            jobHis.PRODUCTOWNER = job.MesCstBody.LOTLIST[0].PRODUCTOWNER;
                            jobHis.PRODUCTSIZE = job.MesCstBody.LOTLIST[0].PRODUCTSIZE;
                            jobHis.LINERECIPENAME = job.MesCstBody.LOTLIST[0].LINERECIPENAME;
                            jobHis.NODESTACK = job.MesCstBody.LOTLIST[0].NODESTACK;
                        }
                        jobHistorys.Add(jobHis);
                    }
                    ObjectManager.JobManager.InsertAllDB(jobHistorys.ToArray<JOBHISTORY>());
                }
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }
        
        public void RecordJobsHistory(IList<Job> jobs, string nodeID, string nodeNo, string portno, string eventName, string transactionID)
        {
            try
            {
                Job2History(jobs, nodeID, nodeNo, string.Empty, portno, string.Empty, eventName, transactionID,string.Empty,string.Empty);
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }

        public void RecordJobHistory(Job job, string nodeId, string nodeNo, string unitNo, string portNo, string slotNo, string eventname, string transactionID)
        {
            try
            {
                IList<Job> jobs = new List<Job>();
                jobs.Add(job);
                Job2History(jobs, nodeId, nodeNo, unitNo, portNo, slotNo, eventname, transactionID, string.Empty,string.Empty);
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        // add by box.zhai 2016/12/16  VCR Report History 增加保存VCR No和VCR Result
        public void RecordJobHistory(Job job, string nodeId, string nodeNo, string unitNo, string portNo, string slotNo, string eventname, string transactionID, string vcrNo, string notChipID)
        {
            try
            {
                IList<Job> jobs = new List<Job>();
                jobs.Add(job);
                Job2History(jobs, nodeId, nodeNo, unitNo, portNo, slotNo, eventname, transactionID, vcrNo, string.Empty);
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //public void RecordJobHistory(Job job, string nodeId, string nodeNo, string unitNo, string portNo, string slotNo, string eventname, string pathNo = "")
        //{
        //    try
        //    {
        //        // Save DB
        //        if (unitNo == "0") unitNo = string.Empty;
        //        if (portNo == "00") portNo = string.Empty;
        //        int i = 0;
        //        JOBHISTORY his = new JOBHISTORY()
        //        {
        //            UPDATETIME = DateTime.Now,
        //            EVENTNAME = eventname,
        //            CASSETTESEQNO = int.TryParse(job.CassetteSequenceNo, out i) == true ? i : 0,
        //            JOBSEQNO = int.TryParse(job.JobSequenceNo, out i) == true ? i : 0,
        //            JOBID = job.GlassChipMaskBlockID,
        //            GROUPINDEX = int.TryParse(job.GroupIndex, out i) == true ? i : 0,
        //            PRODUCTTYPE = job.ProductType.Value,
        //            CSTOPERATIONMODE = job.CSTOperationMode.ToString(),
        //            SUBSTRATETYPE = job.SubstrateType.ToString(),
        //            CIMMODE = job.CIMMode.ToString(),
        //            JOBTYPE = job.JobType.ToString(),
        //            JOBJUDGE = job.JobJudge.ToString(),
        //            SAMPLINGSLOTFLAG = job.SamplingSlotFlag,
        //            OXRINFORMATIONREQUESTFLAG = job.OXRInformationRequestFlag,
        //            FIRSTRUNFLAG = job.FirstRunFlag,
        //            JOBGRADE = job.JobGrade,
        //            PPID = job.PPID,
        //            INSPRESERVATIONS = job.INSPReservations,
        //            LASTGLASSFLAG = job.LastGlassFlag,
        //            INSPJUDGEDDATA = job.InspJudgedData,
        //            TRACKINGDATA = job.TrackingData,
        //            EQPFLAG = job.EQPFlag,
        //            OXRINFORMATION = job.OXRInformation,
        //            CHIPCOUNT = job.ChipCount,
        //            NODENO = nodeNo,//Watson Modify 20150209 以機台上報的資料為準
        //            UNITNO = unitNo,//Watson Modify 20150209 以機台上報的資料為準
        //            PORTNO = portNo, //Watson Modify 20150209 以機台上報的資料為準
        //            SLOTNO = slotNo, //Watson Modify 20150209 以機台上報的資料為準
        //            NODEID = nodeId, //Watson Modify 20150209 以機台上報的資料為準
        //            SOURCECASSETTEID = job.FromCstID, //
        //            CURRENTCASSETTEID = job.ToCstID,
        //            PATHNO = pathNo
        //        };
        //        ObjectManager.JobManager.InsertDB(his);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        //private void RecordJobHistoryForErrorData(Job errJob, Equipment currentEqp, string unitNo, string portNo, string slotNo, string eventname)
        //{
        //    try
        //    {
        //        // Save DB
        //        IList<Job> jobs = new List<Job>();
        //        jobs.Add(errJob);
        //        Job2History(jobs, currentEqp.Data.NODEID, currentEqp.Data.NODENO, unitNo, portNo, slotNo, eventname);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}
        #endregion

        private string GetProductJudge(string nodeNo, Job job)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(nodeNo);
                if (eqp == null)
                    return job.JobJudge;
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null)
                    return job.JobJudge;

                if (line.Data.LINETYPE == eLineType.CF.FCSRT_TYPE1 || line.Data.FABTYPE  == eFabType.ARRAY.ToString())
                {
                    return job.JobGrade;
                }
                else
                {
                    ConstantManager cn = Workbench.Instance.GetObject("ConstantManager") as ConstantManager;
                    if (cn == null)
                        return job.JobJudge;
                    //Watson Modify 20150428 JUDGE: "1-OK ,2-NG"
                    return job.JobJudge +"-" + cn[string.Format("{0}_JOBJUDGE_MES", line.Data.FABTYPE)][job.JobJudge].Value;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return job.JobJudge;
            }
        }

        #region Test Create Job MES Data
        public void TEST_CREATE_OneJOBDataFile(ref Job job)
        {
            try
            {
                Random rand = new Random();
                if (job == null)
                    job = new Job();
                job.CassetteSequenceNo = "1";
                job.JobSequenceNo = "1";
                job.SamplingSlotFlag = rand.Next(2).ToString();
                job.FromSlotNo = job.JobSequenceNo;
                job.GroupIndex = rand.Next(65535).ToString();
                job.ProductType.Value = rand.Next(65535);
                job.CSTOperationMode = (eCSTOperationMode)int.Parse(rand.Next(2).ToString());
                job.SubstrateType = (eSubstrateType)int.Parse(rand.Next(4).ToString());
                job.CIMMode = (eBitResult)int.Parse(rand.Next(2).ToString());
                job.JobType = (eJobType)int.Parse(rand.Next(7).ToString());
                job.JobJudge = rand.Next(9).ToString();

                job.OXRInformationRequestFlag = rand.Next(2).ToString();
                //job. = eVent.Items[11].Value.ToString(); //Reserve
                job.FirstRunFlag = rand.Next(2).ToString();
                job.JobGrade = rand.Next(2).ToString();
                job.GlassChipMaskBlockID = "GLASS" + rand.Next(11111111, 99999999).ToString();
                job.PPID = "PPID3143141341341";
                job.INSPReservations = rand.Next(0, 768).ToString();
                job.EQPReservations = rand.Next(0, 235).ToString();
                if (job.CfSpecial == null)
                    job.CfSpecial = new JobCfSpecial();
                job.InspJudgedData = rand.Next(2).ToString();
                job.CFSpecialReserved = rand.Next(2).ToString();
                job.TrackingData = rand.Next(2).ToString();
                job.CFSpecialReserved = rand.Next(2).ToString();
                job.OXRInformation = rand.Next(5).ToString();
                job.ChipCount = rand.Next(256);
                job.CfSpecial.COAversion = "AB";
                job.CfSpecial.DummyUsedCount = rand.Next(6).ToString();
                job.CellSpecial.CutCompleteFlag = eBitResult.ON;
                job.CellSpecial.CurrentRunMode = "R01";

                job.MesProduct.PRODUCTNAME = job.GlassChipMaskBlockID;
                job.MesProduct.POSITION = job.JobSequenceNo;

                job.MesCstBody.AOIBYPASS = "Y";
                job.MesCstBody.CARRIERNAME = job.FromCstID;
                job.MesCstBody.LINEOPERMODE = "X1341432";
                LOTc lot = new LOTc();
                lot.BCPRODUCTID = "P13314324";
                lot.BCPRODUCTTYPE = "1242";

                lot.CURRENTSITE = "!#13434";
                lot.PPID = "PP3211414341343413";
                //lot.SUBPRODUCTNAMES = "G341341314234";  cc.kuang t3 not use this item 20150701
                lot.SUBPRODUCTSPECS = "ABCEEREW3";
                //lot.SUBPRODUCTNAMES = "02;03;05;06;09;10;11"; cc.kuang t3 not use this item 20150701
                lot.SUBPRODUCTSIZETYPES = "BIG;SMALL;NORMAL;SMALL;BIG;NORMAL";
                lot.SUBPRODUCTSIZES = "A2;AB;C05;06D;09E;1F0;1H1";

                lot.NODESTACK = "N13-013";
                lot.LINERECIPENAME = "R31434132413413243";
                STBPRODUCTSPECc stb = new STBPRODUCTSPECc();
                stb.BCPRODUCTID = "stbP113252";
                stb.BCPRODUCTTYPE = "stbB311441";
                stb.CARRIERSETCODE = "stbCAS23413412";
                stb.LINERECIPENAME = "stb#!$#$!31341341";


                lot.STBPRODUCTSPECLIST.Add(stb);
                lot.PROCESSOPERATIONNAME = "P1324341";
                job.MesCstBody.LOTLIST.Add(lot);
                ProcessFlow processFlow = new ProcessFlow()
                {
                    MachineName = "L2",
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                };
                job.JobProcessFlows.Add(processFlow.MachineName, processFlow);
                ObjectManager.JobManager.AddJob(job);
                ObjectManager.JobManager.EnqueueSave(job);
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void TEST_CREATE_JOBDataFile()
        {
            try
            {
                Random rand = new Random();
                List<Job> jobs = new List<Job>();
                for (int i = 1; i <= 200; i++)
                {
                    Job job = new Job(10000, i);


                    //job.SamplingSlotFlag = rand.Next(2).ToString();
                    job.SamplingSlotFlag = "1";
                    if (job == null)
                        continue;
                    if (i == 500)
                        job.LastGlassFlag = "1";
                    if (i == 499)
                        job.LastGlassFlag = "1";
                    if (i == 498)
                        job.LastGlassFlag = "1";
                    job.FromSlotNo = (i).ToString();
                    job.GroupIndex = rand.Next(65535).ToString();
                    job.ProductType.Value = rand.Next(65535);
                    job.CSTOperationMode = (eCSTOperationMode)int.Parse(rand.Next(2).ToString());
                    job.SubstrateType = (eSubstrateType)int.Parse(rand.Next(4).ToString());
                    job.CIMMode = (eBitResult)int.Parse(rand.Next(2).ToString());
                    job.JobType = (eJobType)int.Parse(rand.Next(7).ToString());
                    job.JobJudge = rand.Next(9).ToString();

                    job.OXRInformationRequestFlag = rand.Next(2).ToString();
                    //job. = eVent.Items[11].Value.ToString(); //Reserve
                    job.FirstRunFlag = rand.Next(2).ToString();
                    job.JobGrade = rand.Next(2).ToString();
                    job.GlassChipMaskBlockID = "GLASS" + rand.Next(11111111, 99999999).ToString();
                    job.PPID = "PPID3143141341341";
                    //job.INSPReservations = rand.Next(0, 768).ToString();
                    //job.EQPReservations = rand.Next(0, 235).ToString();
                    //job. = eVent.Items[19].Value.ToString();//Reserve
                    if (job.CfSpecial == null)
                        job.CfSpecial = new JobCfSpecial();
                    job.InspJudgedData = rand.Next(2).ToString();
                    job.CFSpecialReserved = rand.Next(2).ToString();
                    job.TrackingData = rand.Next(2).ToString();
                    job.CFSpecialReserved = rand.Next(2).ToString();
                    //job.EQPFlag = rand.Next(0, 1).ToString();
                    job.OXRInformation = rand.Next(5).ToString();
                    job.ChipCount = rand.Next(256);
                    //job.File = eVent.Items[27].Value.ToString();//Reserve
                    job.CfSpecial.COAversion = "AB";
                    job.CfSpecial.DummyUsedCount = rand.Next(6).ToString();
                    job.CurrentEQPNo = "L2";
                    ProcessFlow processFlow = new ProcessFlow()
                    {
                        MachineName = "L2",
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now,
                    };
                    job.JobProcessFlows.Add(processFlow.MachineName, processFlow);
                    jobs.Add(job);
                    //System.Threading.Thread.Sleep(300);
                }
                //ObjectManager.JobManager.AddJobs(jobs);
                #region [計時]
                System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); //  开始监视代码


                //程式寫在這裏
                //ObjectManager.JobManager.RecordJobHistory(jobs, string.Empty, string.Empty, string.Empty, "TEST_CREATE");
                //ObjectManager.JobManager.RecordJobsHistory(jobs, string.Empty, string.Empty, string.Empty, "TEST_CREATE");
                
                stopwatch.Stop(); //  停止监视
                TimeSpan timeSpan = stopwatch.Elapsed; //  获取总时间
                double milliseconds = timeSpan.TotalMilliseconds;  //  毫秒数


                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("INSERT DB1=[{0}]",milliseconds));
                #endregion

            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        /// <summary>
        /// Save ODF Line Assembly History 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="cfJob"></param>
        /// <param name="tftJob"></param>
        public void RecordAssemblyHistory(string nodeId, Job cfJob, Job tftJob)
        {
            try
            {
                ASSEMBLYHISTORY assemblyHistory = new ASSEMBLYHISTORY()
                {
                    UPDATETIME=DateTime.Now,
                    NODEID=nodeId,
                    TFTCASSETTESEQNO=tftJob.CassetteSequenceNo,
                    TFTJOBSEQNO=tftJob.JobSequenceNo,
                    TFTJOBID=tftJob.GlassChipMaskBlockID,
                    CFCASSETTESEQNO=cfJob.CassetteSequenceNo,
                    CFJOBSEQNO=cfJob.JobSequenceNo,
                    CFJOBID=cfJob.GlassChipMaskBlockID
                };
                HibernateAdapter.SaveObject(assemblyHistory);
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void RecordAssemblyHistory(string nodeId, string cfCstSeqNo, string cfJobSeqNo, string cfGlassID, string tftCstSeqNo, string tftJobSeqNo, string tftGlassID)
        {
            try
            {
                ASSEMBLYHISTORY assemblyHistory = new ASSEMBLYHISTORY()
                {
                    UPDATETIME = DateTime.Now,
                    NODEID = nodeId,
                    TFTCASSETTESEQNO = tftCstSeqNo,
                    TFTJOBSEQNO = tftJobSeqNo,
                    TFTJOBID = tftGlassID,
                    CFCASSETTESEQNO = cfCstSeqNo,
                    CFJOBSEQNO = cfJobSeqNo,
                    CFJOBID = cfGlassID
                };
                HibernateAdapter.SaveObject(assemblyHistory);
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// RecordDefectCodeHistory
        /// </summary>
        /// <param name="job"></param>
        /// <param name="code"></param>
        /// <param name="nodeID"></param>
        public void RecordDefectCodeHistory(string trxID ,Job job, DefectCode code, string nodeID)
        {
            try
            {
                DEFECTCODEHISTORY defectCodeHistory = new DEFECTCODEHISTORY();
                defectCodeHistory.UPDATETIME = DateTime.Now;
                defectCodeHistory.NODEID = nodeID;
                defectCodeHistory.NODENO = code.EqpNo;
                defectCodeHistory.JOBID = job.GlassChipMaskBlockID;
                defectCodeHistory.PLANID = job.GlassChipMaskBlockID + code.ChipPostion.PadLeft(2, '0');
                defectCodeHistory.DEFECTCODES = code.DefectCodes;
                defectCodeHistory.REMARK = "";
                defectCodeHistory.TRANSACTIONID = trxID;
                try
                {
                    defectCodeHistory.CASSETTESEQNO = int.Parse(job.CassetteSequenceNo);
                    defectCodeHistory.JOBSEQNO = int.Parse(job.JobSequenceNo);
                }
                catch
                {
                }
                HibernateAdapter.SaveObject(defectCodeHistory);
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
    }
}
