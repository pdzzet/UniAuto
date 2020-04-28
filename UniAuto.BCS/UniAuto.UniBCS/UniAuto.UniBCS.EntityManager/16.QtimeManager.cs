using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Log;
using System.Collections;

namespace UniAuto.UniBCS.EntityManager
{
    public class QtimeManager : EntityManager, IDataSource
    {
        //Qtime 的設定檔 Qtime Spec
        public Dictionary<string, QtimeEntityData> _entitiesDB = new Dictionary<string, QtimeEntityData>();
        //計時中的Qtime JOB
        public Dictionary<string, Qtime> _entities = new Dictionary<string, Qtime>();

        public override EntityManager.FILE_TYPE GetFileType()
        {
            return FILE_TYPE.BIN;
        }

        protected override string GetSelectHQL()
        {
            return string.Format("from QtimeEntityData where SERVERNAME = '{0}'", BcServerName);
        }

        protected override Type GetTypeOfEntityData()
        {
            return typeof(QtimeEntityData);
        }

        protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            foreach (EntityData entity_data in EntityDatas)
            {
                QtimeEntityData qtime_entity_data = entity_data as QtimeEntityData;
                if (qtime_entity_data != null)
                {
                    _entitiesDB.Add(qtime_entity_data.QTIMEID, qtime_entity_data);
                }
            }
            Filenames = new List<string>();
            Filenames.Add("*.bin");
        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(QtimeEntityFile);
        }

        protected override EntityFile NewEntityFile(string Filename)
        {
            return new QtimeEntityFile();
        }

        //目前不需要，歸為job管理，由job save file
        protected override void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles)
        {
            try
            {
                //_isRunning = false;//关闭EntityManager的Thread 20141112  Tom
                //foreach (EntityData entity_data in entityDatas)
                //{
                //    QtimeEntityData qtime_entity_data = entity_data as QtimeEntityData;
                //    if (qtime_entity_data != null)
                //    {
                //        if (entityFiles.Count <= 0)
                //        {

                //        }
                //        foreach (EntityFile entity_file in entityFiles)
                //        {
                //            QtimeEntityFile qtime_entity_file = entity_file as QtimeEntityFile;
                //            if (qtime_entity_file != null)
                //            {
                //                string fextname = qtime_entity_file.GetFilename();
                //                string fname = Path.GetFileNameWithoutExtension(fextname);
                //                if (string.Compare(qtime_entity_file.JobNo, fname, true) == 0)
                //                {
                //                    if (!_entities.ContainsKey(qtime_entity_file.JobNo))
                //                        _entities.Add(qtime_entity_file.JobNo, new Qtime(qtime_entity_data, qtime_entity_file));
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 直接从DB中取得资料
        /// </summary>
        /// <param name="namelist">Entity 的属性名称</param>
        /// <param name="valuelist">Entity 的属性的值</param>
        /// <returns></returns>
        public IList<QtimeEntityData> Find(string[] namelist, object[] valuelist)
        {
            try
            {
                IList<QtimeEntityData> list = new List<QtimeEntityData>();
                if (this.HibernateAdapter != null)
                {
                    IList list2 = HibernateAdapter.GetObject_AND(typeof(QtimeEntityData), namelist, valuelist, null, null);

                    if (list2 == null)
                    {
                        return list;
                    }
                    foreach (QtimeEntityData local in list2)
                    {
                        list.Add(local);
                    }
                    return list;
                }
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return null;
        }

        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("QtimeManager");
            return entityNames;
        }

        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();
                QtimeEntityData data = new QtimeEntityData();
                //QtimeEntityFile file = new QtimeEntityFile();
                DataTableHelp.DataTableAppendColumn(data, dt);
                //DataTableHelp.DataTableAppendColumn(file, dt);

                foreach (QtimeEntityData qtime in _entitiesDB.Values)
                {
                    DataRow dr = dt.NewRow();
                    DataTableHelp.DataRowAssignValue(qtime, dr);
                    //DataTableHelp.DataRowAssignValue(qtime.File, dr);
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

        //public bool GetQtimeisOver(string cstseqno,string jobseqno)
        //{
        //     try
        //    {
        //        string jobno = string.Format("{0}_{1}", cstseqno, jobseqno);

        //        if (!_entities.ContainsKey(jobno))
        //            return false;

        //        Qtime qtime = _entities[jobno];
                
        //        TimeSpan qtimesec = (DateTime.Now - qtime.File.updateTime);

        //        if (qtimesec.TotalSeconds >= qtime.Data.SETTIMEVALUE)
        //        {
        //            string info = string.Format("Cassette seq No[{0}] Job Seq No[{1}] Qtime Time[{2}] is up!!", cstseqno, jobseqno, qtime.Data.SETTIMEVALUE.ToString());
        //            Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
        //            return true;
        //        }
        //        else
        //            return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        /// 查詢Job的Qtime的結果，Q time過了就是NG
        /// </summary>
        /// <param name="cstseqno">Job.cstseqNo</param>
        /// <param name="jobseqno">Job.jobseqNo</param>
        /// <param name="QueryEQPID">MachineName,Eqp ID</param>
        /// <returns>3 結果:OK,NG,RW</returns>
        public string GetQtimeisOver(string cstseqno, string jobseqno,Equipment  eqp)
        {
            try
            {

                Job job = ObjectManager.JobManager.GetJob(cstseqno, jobseqno);

                if (job == null)
                {
                    string err = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{1}]  WIP IS NULL!!", eqp.Data.NODENO,cstseqno, jobseqno);
                    Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return "OK";
                }

                if (job.QtimeList == null)
                {
                    string err = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{1}]  QTime IS NULL!!Qtime Is Not Start!", eqp.Data.NODENO, cstseqno, jobseqno);
                    Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return "OK";
                }

                foreach (Qtimec qtime in job.QtimeList)
                {
                    if (qtime.LINENAME != eqp.Data.LINEID)
                        continue;
                    if (qtime.ENDMACHINE == eqp.Data.NODEID)
                    {
                        //if (qtime.RECIPEID.Trim() != string.Empty) 
                        //{
                        //    if (eqp.File.CurrentRecipeID != qtime.RECIPEID)
                        //    {
                        //        string err = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID=[{3}] RECIPEID=[{4}] MISMATCH!!", eqp.Data.NODENO, cstseqno, jobseqno, job.GlassChipMaskBlockID, qtime.RECIPEID);
                        //        Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                        //        continue;
                        //    }
                        //}

                        if (!qtime.ENABLED) //Watson Add 20150520 For CSOT鄒揚要求，設為零或Disable就算超時也不可回覆NG
                        {
                            string err = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID=[{3}] IS DISABLE", eqp.Data.NODENO, cstseqno, jobseqno, job.GlassChipMaskBlockID);
                            Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                            continue;
                        }

                        if (qtime.QTIMEVALUE == "0") //Watson Add 20150520 For CSOT鄒揚要求，設為零或Disable就算超時也不可回覆NG
                        {
                            string err = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID=[{3}] QTime Value = '0' ,QTime IS DISABLE!!", eqp.Data.NODENO, cstseqno, jobseqno, job.GlassChipMaskBlockID);
                            Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                            continue;
                        }

                        if (!qtime.STARTQTIMEFLAG)
                        {
                            string err = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID=[{3}] START_NODE_ID=[{4}] START_EVENT=[{5}] IS NOT TIRGGER!!", eqp.Data.NODENO, cstseqno, jobseqno, job.GlassChipMaskBlockID, qtime.STARTMACHINE, qtime.STARTEVENT);
                            Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                            continue;
                        }
                        #region Watson Add 20150317 EQ Request Maybe Before JobEvent. For CF &憲昌討論
                        //機台詢問可能在超時結束事件之前，所以就算Qtime早就到了，還是可能沒有符合超時的條件
                        //直接在機台詢問時判斷是否已超時，回覆機台
                        if (JudgeQTimeOver(cstseqno, jobseqno, qtime.QTIMEVALUE, qtime))
                        {
                            qtime.OVERQTIMEFLAG = true;
                            string err = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID=[{3}] NG_QTIME_VLAUE=[{4}] START_DATETIME=[{5}]  IS TIME UP!!", eqp.Data.NODENO, cstseqno, jobseqno, job.GlassChipMaskBlockID, qtime.QTIMEVALUE, qtime.STARTQTIME);
                            Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                        }
                        else
                        {
                            string info = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID=[{3}] NG_QTIME_VLAUE=[{4}] START_DATETIME=[{5}] IS NOT OVER NG_QTIME", eqp.Data.NODENO, cstseqno, jobseqno, job.GlassChipMaskBlockID, qtime.CFRWQTIME, qtime.STARTQTIME);
                            Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                        }

                        if (!qtime.OVERQTIMEFLAG)
                        {
                            if (JudgeQTimeOver(cstseqno, jobseqno, qtime.CFRWQTIME, qtime))
                            {
                                qtime.CFRWQTIMEFLAG = true;
                                string err = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID=[{3}] CF_REWORK_QTIME=[{4}] START_DATETIME=[{5}] IS TIME UP!!", eqp.Data.NODENO, cstseqno, jobseqno, job.GlassChipMaskBlockID, qtime.CFRWQTIME, qtime.STARTQTIME);
                                Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                            }
                            else
                            {
                                string info = string.Format("[EQUIPMENT={0}] CASSETTE_SEQ_NO=[{1}] JOB_SEQ_NO=[{2}] GLASSID=[{3}] CF_REWORK_QTIME=[{4}] START_DATETIME=[{5}] IS  NOT OVER CF_RWWOR_QTIME", eqp.Data.NODENO, cstseqno, jobseqno, job.GlassChipMaskBlockID, qtime.CFRWQTIME, qtime.STARTQTIME);
                                Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", info);
                            }
                        }
                        #endregion

                        if (qtime.OVERQTIMEFLAG)
                            return "NG";
                        else
                        {
                            if (qtime.CFRWQTIMEFLAG)
                                return "RW";
                            return "OK";
                        }
                    }
                    else
                        continue;
                }
                return "OK";
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return "OK";
            }
        }

        /// <summary>
        /// Watson Add 20150317 For Qtime Judge 以Qtime 或CF Rework Qtime
        /// </summary>
        /// <param name="cstseqno"></param>
        /// <param name="jobseqno"></param>
        /// <param name="qTimeVlaue"></param>
        /// <returns></returns>
        public bool JudgeQTimeOver(string cstseqno, string jobseqno, string qTimeVlaue, Qtimec qtime)
        {
            Job job = ObjectManager.JobManager.GetJob(cstseqno, jobseqno);

            if (job == null)
            {
                string err = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}]  WIP IS NULL!!", cstseqno, jobseqno);
                Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                return false;
            }

            TimeSpan qtimesec = (DateTime.Now - qtime.STARTQTIME);
            int setvalue = 0;
            if (!int.TryParse(qTimeVlaue, out setvalue))
            {
                string err = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}]  GLASSID =[{2}] QTIME VALUE=[{3}] FORMAT ERROR!!", cstseqno, jobseqno, job.GlassChipMaskBlockID, qTimeVlaue);
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                return false;
            }

            if (setvalue == 0)
            {
                string err = string.Format("CASSETTE_SEQ_NO=[{0}] JOB_SEQ_NO=[{1}]  GLASSID=[{2}] QTIMEVLAUE=[{3}]  IS NOT START !!", cstseqno, jobseqno, job.GlassChipMaskBlockID, qTimeVlaue, qtimesec.TotalSeconds.ToString(), qtime.STARTQTIME);
                Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                return false;
            }

            if (qtimesec.TotalSeconds > setvalue)
                return true;

            return false;
        }

        public void ValidateQTimeSetting(Job job)
        {
            try
            {
                if (!ValidateMESQTime(job))
                {
                    if (!ValidateDBQTime(job))
                    {
                        string war = "No Q Time Setting ,Please Check MES Download or DB Setting!!";
                        Log.NLogManager.Logger.LogWarnWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", war);
                    }
                    else
                    {
                        
                    }
                }

            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        //MES Q time Download Move to Job Qtime 
        private bool ValidateMESQTime(Job job)
        {
            try
            {
                if (job == null)
                    return false;
                if (job.MesCstBody == null)
                    return false;
                if (job.MesCstBody.LOTLIST.Count == 0)
                    return false;
                if (job.MesCstBody.LOTLIST[0].LINEQTIMELIST.Count == 0)
                    return false;

                if (job.MesCstBody.LOTLIST[0].LINEQTIMELIST[0].MACHINEQTIMELIST == null)
                    return false;

                //取得MES Download 的q time setting.
                job.QtimeList = new List<Qtimec>();
                int i = 0,intTime = 0;
                foreach (LINEQTIMEc lotlist in job.MesCstBody.LOTLIST[0].LINEQTIMELIST)
                {
                    Qtimec qtimec = new Qtimec();
                    qtimec.LINENAME = lotlist.LINENAME;
                    foreach (MACHINEQTIMEc mesqtime in lotlist.MACHINEQTIMELIST)
                    {
                        qtimec.QTIMEID = i.ToString();
                        if (int.TryParse(mesqtime.CFRWQTIME, out intTime))
                            qtimec.CFRWQTIME = intTime.ToString();
                        else
                            qtimec.CFRWQTIME = "0";
                        qtimec.ENDEVENT = mesqtime.ENDEVENT;
                        qtimec.ENDMACHINE = mesqtime.ENDMACHINE;
                        qtimec.ENDUNITS = mesqtime.ENDUNITS;
                        if (int.TryParse(mesqtime.QTIME, out intTime))
                            qtimec.QTIMEVALUE = intTime.ToString();
                        else
                            qtimec.QTIMEVALUE = "0";
                        qtimec.RECIPEID = mesqtime.RECIPEID;
                        qtimec.STARTEVENT = mesqtime.STARTEVENT;
                        qtimec.STARTMACHINE = mesqtime.STARTMACHINE;
                        qtimec.STARTUNITS = mesqtime.STARTUNITS;
                        qtimec.OVERQTIMEFLAG = false;
                        qtimec.ENABLED = true;
                        job.QtimeList.Add(qtimec);
                        i++;
                    }
                }
                //EnqueueSave(job);
                return true;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        //DB Q time Setting Move to Job Qtime 
        public  bool ValidateDBQTime(Job job)
        {
            try
            {
                if (job == null)
                    return false;

                job.QtimeList = new List<Qtimec>();

                foreach (QtimeEntityData entitydata in _entitiesDB.Values)
                {
                    Qtimec qtimec = new Qtimec();
                    if (entitydata.LINEID != BcServerName)
                        continue;
                    qtimec.LINENAME = entitydata.LINEID;
                    qtimec.QTIMEID = entitydata.QTIMEID;
                    qtimec.CFRWQTIME = (entitydata.CFRWQTIME).ToString();
                    qtimec.ENDEVENT = entitydata.ENDEVENTMSG;
                    qtimec.ENDMACHINE = entitydata.ENDNODEID;
                    qtimec.ENDUNITS = entitydata.ENDNUNITID;
                    qtimec.QTIMEVALUE = (entitydata.SETTIMEVALUE).ToString();
                    qtimec.RECIPEID = entitydata.STARTNODERECIPEID;
                    qtimec.STARTEVENT = entitydata.STARTEVENTMSG;
                    qtimec.STARTMACHINE = entitydata.STARTNODEID;
                    qtimec.STARTUNITS = entitydata.STARTNUNITID;
                    qtimec.OVERQTIMEFLAG = false;
                    qtimec.ENABLED = entitydata.ENABLED =="Y"? true : false;
                    qtimec.REMARK = entitydata.REMARK;
                    job.QtimeList.Add(qtimec);
                    
                }
                //EnqueueSave(job);
                return true;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        public void ReloadQTime()
        {
            try
            {
                IList entiyDatas = HibernateAdapter.GetObjectByQuery(GetSelectHQL());

                Dictionary<string, QtimeEntityData> tempObject = new Dictionary<string, QtimeEntityData>();

                if (entiyDatas != null)    //add by sy.wu
                {
                    foreach (QtimeEntityData obj in entiyDatas)
                    {
                        if (tempObject.ContainsKey(obj.QTIMEID))
                            tempObject.Remove(obj.QTIMEID);
                        tempObject.Add(obj.QTIMEID, obj);
                    }
                    lock (_entitiesDB)
                        _entitiesDB = tempObject;

                    //Watson Add
                    ResetJobQtime();
                }
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return;
            }
        }

        private void ResetJobQtime()
        {
            try
            {
                foreach (Job job in ObjectManager.JobManager.GetJobs())
                {
                    if (job.QtimeList != null)
                    {
                        #region Updata Job Qtime
                        for (int i = job.QtimeList.Count - 1; i >= 0;i-- )
                        {
                            Qtimec qtimec = job.QtimeList[i];
                            if (_entitiesDB.ContainsKey(qtimec.QTIMEID))
                            {
                                qtimec.CFRWQTIME = (_entitiesDB[qtimec.QTIMEID].CFRWQTIME).ToString();
                                qtimec.ENDEVENT = _entitiesDB[qtimec.QTIMEID].ENDEVENTMSG;
                                qtimec.ENDMACHINE = _entitiesDB[qtimec.QTIMEID].ENDNODEID;
                                qtimec.ENDUNITS = _entitiesDB[qtimec.QTIMEID].ENDNUNITID;
                                qtimec.QTIMEVALUE = (_entitiesDB[qtimec.QTIMEID].SETTIMEVALUE).ToString();
                                qtimec.RECIPEID = _entitiesDB[qtimec.QTIMEID].STARTNODERECIPEID;
                                qtimec.STARTEVENT = _entitiesDB[qtimec.QTIMEID].STARTEVENTMSG;
                                qtimec.STARTMACHINE = _entitiesDB[qtimec.QTIMEID].STARTNODEID;
                                qtimec.STARTUNITS = _entitiesDB[qtimec.QTIMEID].STARTNUNITID;
                                qtimec.ENABLED = _entitiesDB[qtimec.QTIMEID].ENABLED == "Y" ? true : false;
                                qtimec.REMARK = _entitiesDB[qtimec.QTIMEID].REMARK;
                                //不得更新
                                //qtimec.STARTQTIMEFLAG
                                //qtimec.CFRWQTIMEFLAG
                                //qtimec.OVERQTIMEFLAG

                                string str = string.Format("QTIME RELOAD  CST_SEQ_NO=[{0}], JOB_SEQ_NO=[{1}] GLASSID[{2}]  QTIMEID =[{3}] QTIME VALUE=[{4}] QTIME ENABLE=[{5}].", job.CassetteSequenceNo,
                                    job.JobSequenceNo,job.GlassChipMaskBlockID, qtimec.QTIMEID, qtimec.QTIMEVALUE, qtimec.ENABLED);
                                Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", str);
                            }
                            else
                            {
                                job.QtimeList.RemoveAt(i);

                                string str = string.Format("QTIME DELETE  CST_SEQ_NO=[{0}], JOB_SEQ_NO=[{1}] GLASSID[{2}]  QTIMEID =[{3}] QTIME VALUE=[{4}] QTIME ENABLE=[{5}].", job.CassetteSequenceNo,
                                     job.JobSequenceNo,job.GlassChipMaskBlockID, qtimec.QTIMEID, qtimec.QTIMEVALUE, qtimec.ENABLED);
                                Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", str);
                            }
                        }
                        #endregion
                        #region DB Add
                        foreach (string dbQtimeID in _entitiesDB.Keys)
                        {
                            bool bolAlread = false;
                            foreach(Qtimec qtimec in job.QtimeList)
                            {
                                if (qtimec.QTIMEID == dbQtimeID)
                                {
                                    bolAlread = true;
                                    break;
                                }
                            }
                            if (bolAlread)
                                continue;
                            //Add 
                            Qtimec nqtimec = new Qtimec();
                            if (_entitiesDB[dbQtimeID].LINEID != BcServerName)
                                continue;
                            nqtimec.LINENAME = _entitiesDB[dbQtimeID].LINEID;
                            nqtimec.QTIMEID = _entitiesDB[dbQtimeID].QTIMEID;
                            nqtimec.CFRWQTIME = (_entitiesDB[dbQtimeID].CFRWQTIME).ToString();
                            nqtimec.ENDEVENT = _entitiesDB[dbQtimeID].ENDEVENTMSG;
                            nqtimec.ENDMACHINE = _entitiesDB[dbQtimeID].ENDNODEID;
                            nqtimec.ENDUNITS = _entitiesDB[dbQtimeID].ENDNUNITID;
                            nqtimec.QTIMEVALUE = (_entitiesDB[dbQtimeID].SETTIMEVALUE).ToString();
                            nqtimec.RECIPEID = _entitiesDB[dbQtimeID].STARTNODERECIPEID;
                            nqtimec.STARTEVENT = _entitiesDB[dbQtimeID].STARTEVENTMSG;
                            nqtimec.STARTMACHINE = _entitiesDB[dbQtimeID].STARTNODEID;
                            nqtimec.STARTUNITS = _entitiesDB[dbQtimeID].STARTNUNITID;
                            nqtimec.OVERQTIMEFLAG = false;
                            nqtimec.ENABLED = _entitiesDB[dbQtimeID].ENABLED == "Y" ? true : false;
                            nqtimec.REMARK = _entitiesDB[dbQtimeID].REMARK;
                            job.QtimeList.Add(nqtimec);

                            string str = string.Format("QTIME ADDING  CST_SEQ_NO=[{0}], JOB_SEQ_NO=[{1}] GLASSID[{2}]  QTIMEID =[{3}] QTIME VALUE=[{4}] QTIME ENABLE=[{4}].", job.CassetteSequenceNo,
                                job.JobSequenceNo,job.GlassChipMaskBlockID, nqtimec.QTIMEID, nqtimec.QTIMEVALUE, nqtimec.ENABLED);
                            Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", str);
                        }
                        #endregion 

                        EnqueueSave(job);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

    }
}
