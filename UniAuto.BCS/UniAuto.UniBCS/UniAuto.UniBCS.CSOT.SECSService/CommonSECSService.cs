using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Reflection;
using UniAuto.UniBCS.Core.Generic;
using System.Collections.Concurrent;
using UniAuto.UniBCS.MesSpec;
using System.IO;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.CSOT.SECSService
{
    public class CommonSECSService
    {
        private string _logName = string.Empty;
        private AbstractService _service = null;
        //<EqpNo, <RecipeID, RecipeInCheck>>
        private ConcurrentDictionary<string, ConcurrentDictionary<string, RecipeInCheck>> _waitCheckRecipeID;
        private ConcurrentDictionary<string, ConcurrentDictionary<string, RecipeInCheck>> _waitCheckRecipeParameter;
        //<EqpNo, <GlassID, <SubCD, Data>>>
        private ConcurrentDictionary<string, ConcurrentDictionary<string, ProcessDataSpool>> _processData;
        //<EqpNo, <CSTSeqNo, <SubCD, Data>>>
        private ConcurrentDictionary<string, ConcurrentDictionary<string, ProcessDataSpool>> _lotProcessData;

        public CommonSECSService(string logName, AbstractService service)
        {
            _logName = logName;
            _service = service;
            _waitCheckRecipeID = new ConcurrentDictionary<string, ConcurrentDictionary<string, RecipeInCheck>>();
            _waitCheckRecipeParameter = new ConcurrentDictionary<string, ConcurrentDictionary<string, RecipeInCheck>>();
            _processData = new ConcurrentDictionary<string, ConcurrentDictionary<string, ProcessDataSpool>>();
            _lotProcessData = new ConcurrentDictionary<string, ConcurrentDictionary<string, ProcessDataSpool>>();
        }

        #region [For Log Format]
        public void LogAgentRaiseEvent(XmlDocument trx, string clsname, string mthname, string trxname)
        {
            if (trxname.Contains("T3"))
                NLogManager.Logger.LogWarnWrite(_logName, clsname, mthname + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Receive agent raise ({2}).", trx["secs"]["message"].Attributes["node"].InnerText.Trim(),
                                    trx["secs"]["message"].Attributes["tid"].InnerText.Trim(), trxname));
            else
                NLogManager.Logger.LogInfoWrite(_logName, clsname, mthname + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Receive agent raise ({2}).", trx["secs"]["message"].Attributes["node"].InnerText.Trim(),
                                    trx["secs"]["message"].Attributes["tid"].InnerText.Trim(), trxname));
            //NLogManager.Logger.LogTrxWrite(_logName, trxname + "\r\n" + trx.OuterXml); 20150603  暂时先不要记录Log 文件太大了
        }

        public void LogAgentRaiseEvent(XmlDocument trx, string clsname, string mthname, string trxname, bool receive)
        {
            if (trxname.Contains("T3"))
                NLogManager.Logger.LogWarnWrite(_logName, clsname, mthname + "()",
                    string.Format("{0} Receive agent raise ({1}).",
                        GenerateLogHeader(trx["secs"]["message"].Attributes["node"].InnerText.Trim(), receive, trx["secs"]["message"].Attributes["tid"].InnerText.Trim()), trxname));
            else
                NLogManager.Logger.LogInfoWrite(_logName, clsname, mthname + "()",
                    string.Format("{0} Receive agent raise ({1}).",
                        GenerateLogHeader(trx["secs"]["message"].Attributes["node"].InnerText.Trim(), receive, trx["secs"]["message"].Attributes["tid"].InnerText.Trim()), trxname));
              //NLogManager.Logger.LogTrxWrite(_logName, trxname + "\r\n" + trx.OuterXml); 20150603  暂时先不要记录Log 文件太大了
        }

        public void LogInfo(string className, string methodName, string eqpNo, bool receive, string trxID, string msg)
        {
            NLogManager.Logger.LogInfoWrite(_logName, className, methodName, GenerateLogHeader(eqpNo, receive, trxID) + msg);
        }
        public void LogError(string className, string methodName, string eqpNo, bool receive, string trxID, string msg)
        {
            NLogManager.Logger.LogErrorWrite(_logName, className, methodName, GenerateLogHeader(eqpNo, receive, trxID) + msg);
        }
        public void LogWarn(string className, string methodName, string eqpNo, bool receive, string trxID, string msg)
        {
            NLogManager.Logger.LogWarnWrite(_logName, className, methodName, GenerateLogHeader(eqpNo, receive, trxID) + msg);
        }
        public string GenerateLogHeader(string eqpno, bool receive, string trxid)
        {
            return string.Format("[EQUIPMENT={0}] [BCS {1} EQP][{2}] ", eqpno, receive ? "<-" : "->", trxid);
        }
        #endregion

        public void CloneChildNode(XmlNode parent, int idx)
        {
            if (idx != 0)
            {
                XmlNode cNode = parent.FirstChild.CloneNode(true);
                parent.AppendChild(parent.FirstChild.CloneNode(true));
            }
        }

        public void SetItemData(XmlNode x, string data)
        {
            switch (x.Attributes["type"].InnerText.Trim().ToUpper())
            {
                case "A":
                    if (string.IsNullOrEmpty(data))
                        data = string.Empty;
                    if (x.Attributes["len"].InnerText == "?" || x.Attributes["fixlen"].InnerText.ToUpper() == "FALSE")
                        x.Attributes["len"].InnerText = data.Length.ToString();
                    if (data.Length > int.Parse(x.Attributes["len"].InnerText.Trim()))
                        data = data.Substring(0, int.Parse(x.Attributes["len"].InnerText.Trim()));
                    else
                        data = data.PadRight(int.Parse(x.Attributes["len"].InnerText.Trim()), ' ');
                    x.InnerText = data;
                    break;
                case "BOOLEAN":
                    if (data.ToUpper() == "TRUE" || data == "1")
                        x.InnerText = "1";
                    else
                        x.InnerText = "0";
                    break;
                default:
                    x.InnerText = data;
                    break;
            }
        }

        private void GetAlarm(string eqpno, string unitno, string alarmid, out string alarmText, out string alarmTime)
        {
            alarmText = string.Empty;
            alarmTime = string.Empty;
            if (!string.IsNullOrEmpty(alarmid) && !alarmid.Equals("0"))
            {
                HappeningAlarm happenAlarm = ObjectManager.AlarmManager.GetAlarm(eqpno, alarmid);
                if (happenAlarm == null)
                {
                    AlarmEntityData alarm = ObjectManager.AlarmManager.GetAlarmProfile(eqpno, unitno, alarmid);
                    if (alarm != null)
                    {
                        alarmText = alarm.ALARMTEXT;
                    }
                    alarmTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                }
                else
                {
                    alarmText = happenAlarm.Alarm.ALARMTEXT;
                    alarmTime = happenAlarm.OccurDateTime.ToString("yyyyMMddHHmmss");
                }
            }
        }

        public void HandleEquipmentStatus(Equipment eqp, string trxid, string className, string methodName)
        {
            //lock (eqp)
            //{
                string alarmText = string.Empty;
                string alarmTime = string.Empty;
                #region [Equipment Status]
                //get happing alarm
                GetAlarm(eqp.Data.NODENO, "0", eqp.File.CurrentAlarmCode, out alarmText, out alarmTime);
                if (eqp.File.PreStatus != eqp.File.Status)
                {
                    NLogManager.Logger.LogInfoWrite(_logName, className, methodName + "()",
                                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Current Equipment Status ({2}), Previous Equipment Status ({3}), Alarm[{4}:{5} {6}].",
                                                eqp.Data.NODENO, trxid, eqp.File.Status.ToString(), eqp.File.PreStatus.ToString(), eqp.File.CurrentAlarmCode, alarmText, alarmTime));
                }
                if (eqp.File.PreMesStatus != eqp.File.MESStatus)
                {
                    NLogManager.Logger.LogInfoWrite(_logName, className, methodName + "()",
                                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Current Host Equipment Status ({2}), Previous Host Equipment Status({3}), Alarm({4}:{5} {6}).", 
                                                eqp.Data.NODENO, trxid, eqp.File.MESStatus, eqp.File.PreMesStatus, eqp.File.CurrentAlarmCode, alarmText, alarmTime));
                    object[] parm = new object[5]
                        {
                            trxid,                   /*0 trxID*/
                            eqp,                     /*1 EQP object*/
                            eqp.File.CurrentAlarmCode,    /*2 alarmID*/
                            alarmText,  /*3 alarmText*/
                            alarmTime, /*4 alarmTime*/
                        };
                    _service.Invoke(eServiceName.MESService, "MachineStateChanged", parm);
                    //20150428 cy:增加調用CheckLineState,可記錄line history
                    _service.Invoke(eServiceName.LineService, "CheckLineState", new object[] { trxid, eqp.Data.LINEID });
                }
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
            //20150427 cy:增加record to history的功能
                ObjectManager.EquipmentManager.RecordEquipmentHistory(trxid, eqp);
                #endregion
                #region [Unit Status]
                IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);
                if (units != null && units.Count != 0)
                {
                    string logBC = string.Empty;
                    string logMES = string.Empty;
                    foreach (Unit u in units)
                    {
                        if (u == null || u.Data.NODENO.Equals("0") || string.IsNullOrEmpty(u.Data.UNITID) || u.File.Status == eEQPStatus.NOUNIT || u.Data.UNITATTRIBUTE == "VIRTUAL") continue;
                        //get happing alarm
                        GetAlarm(eqp.Data.NODENO, u.Data.NODENO, eqp.File.CurrentAlarmCode, out alarmText, out alarmTime);
                        if (u.File.PreStatus != u.File.Status)
                        {
                            logBC += string.Format(" Unit#{0}{{Current Status ({1}), Previous Status ({2}), Alarm ({3}:{4} {5})}}",
                                                    u.Data.UNITNO, u.File.Status.ToString(), u.File.PreStatus, u.File.CurrentAlarmCode, alarmText, alarmTime);
                        }
                        if (u.File.PreMesStatus != u.File.MESStatus)
                        {
                            logMES += string.Format(" Unit#{0}{{Current Host Status ({1}), Previous Host Status ({2}), Alarm ({3}:{4} {5})}}",
                                                    u.Data.UNITNO, u.File.MESStatus, u.File.PreMesStatus, u.File.CurrentAlarmCode, alarmText, alarmTime);
                            object[] obj = new object[]
                            {
                                trxid,                   /*0 TrackKey*/
                                u,                                  /*1 Unit*/
                                u.File.CurrentAlarmCode,   /*2 AlarmID*/
                                alarmText,                           /*3 Alarm Text*/
                                alarmTime                            /*4 Alarm Time*/
                            };

                            _service.Invoke(eServiceName.MESService, "UnitStateChanged", obj);
                        }
                        ObjectManager.UnitManager.EnqueueSave(u.File);

                              //20150616 cy:增加record to history的功能
                              if (u.File.PreStatus != u.File.Status)

                                  ObjectManager.UnitManager.RecordUnitHistory(trxid, u);
                    }
                    if (!string.IsNullOrEmpty(logBC))
                        NLogManager.Logger.LogInfoWrite(_logName, className, methodName + "()", logBC);
                    if (!string.IsNullOrEmpty(logMES))
                        NLogManager.Logger.LogInfoWrite(_logName, className, methodName + "()", logMES);
                }
                #endregion
                //Report to OPI
                _service.Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { trxid, eqp });
                //}
                #region nikon down hold job in it add by hujunpeng 20181023
            if(eqp.Data.LINEID.Contains("TCPHL"))
            {
            if (eqp.Data.NODENAME == "NikonExposure" && (eqp.File.Status != eEQPStatus.IDLE && eqp.File.Status != eEQPStatus.RUN))
                {
                    string[] eqpList = { "L4" };
                    List<Job> jobs = ObjectManager.JobManager.GetJobsbyEQPList(eqpList.ToList<string>());
                    if (jobs == null)
                    {
                        NLogManager.Logger.LogErrorWrite(_logName, className, methodName + "()", "job in Nikon is Null when Nikon is down state");
                    }
                    if(jobs!=null)
                    {
                    HoldInfo _hd = new HoldInfo();
                    foreach (Job job in jobs)
                    {
                        bool _isNikon = false;
                        if (job.HoldInforList.Count > 0)
                        {
                            for (int i = 0; i < job.HoldInforList.Count; i++)
                            {
                                if (job.HoldInforList[i].HoldReason.Contains("Nikon"))
                                    _isNikon = true;
                                break;
                            }
                        }
                        if (_isNikon) continue;
                        _hd.NodeID = eqp.Data.NODEID;
                        _hd.OperatorID = "BCAuto";
                        _hd.HoldReason = "NikonExposure is down,hold job in it.";
                        job.HoldInforList.Add(_hd);
                    }
                    }
                }
            }
                #endregion
        }
        
        //public void HandleMaskStatus(Equipment eqp, string slot, string name, eMaterialStatus status, string trxid)
        //{
        //    if (eqp == null)
        //    {
        //        throw new Exception("The parameter of Equipment object is null. TrxID=" + trxid);
        //        //NLogManager.Logger.LogErrorWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
        //        //                                "The parameter of Equipment object is null. TrxID=" + trxid);
        //        //return;
        //    }
        //    MaterialEntity maskEntity = null;
        //    if (string.IsNullOrEmpty(slot))
        //    {
        //        if (string.IsNullOrEmpty(name))
        //        {
        //            throw new Exception("The parameter of mask slot and name is empty. TrxID=" + trxid);
        //            //NLogManager.Logger.LogErrorWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
        //            //                                "The parameter of mask slot and name is empty. TrxID=" + trxid);
        //            //return;
        //        }
        //        else
        //            maskEntity = ObjectManager.MaterialManager.GetMaterialByName(eqp.Data.NODENO, name);
        //    }
        //    else
        //        maskEntity = ObjectManager.MaterialManager.GetMaterialBySlot(eqp.Data.NODENO, slot);

        //    bool blnReportMES = false;
        //    if (maskEntity == null)
        //    {
        //        maskEntity = new MaterialEntity();
        //        maskEntity.NodeNo = eqp.Data.NODENO;
        //        maskEntity.UnitNo = "0";
        //        maskEntity.MaterialID = name;
        //        maskEntity.MaterialSlotNo = slot;
        //        maskEntity.MaterialStatus = status;
        //        ObjectManager.MaterialManager.AddMaterial(maskEntity);
        //        blnReportMES = true;
        //    }
        //    lock (maskEntity)
        //    {
        //        eMaterialStatus preMaskST = maskEntity.MaterialStatus;
        //        //原來的mask跟收到的不同,且原來的不為空,把原來的unmount, 
        //        if (maskEntity.MaterialID != name && !string.IsNullOrEmpty(maskEntity.MaterialID.Trim()))
        //        {
        //            //前一mask不是dismount才要報,其餘設為empty
        //            if (maskEntity.MaterialStatus != eMaterialStatus.DISMOUNT)
        //            {
        //                object[] _data = new object[7]
        //            { 
        //            trxid,  /*0 TrackKey*/
        //            eqp.Data.LINEID,    /*1 LineName*/
        //            eqp.Data.NODEID,    /*2 EQPID*/
        //            maskEntity.MaterialID,          /*3 maskid*/
        //            maskEntity.MaterialSlotNo,            /*4 position*/ 
        //            eMaterialStatus.DISMOUNT.ToString(),           /*5 status*/
        //            0,           /*6 count*/
        //            };
        //                //呼叫MES方法
        //                _service.Invoke(eServiceName.MESService, "MaskStateChanged", _data);
        //                NLogManager.Logger.LogInfoWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
        //                            string.Format("Set Mask ({0}) in slot ({1}) as DISMOUNT.", maskEntity.MaterialID, maskEntity.MaterialSlotNo));
        //            }
        //            maskEntity.UnitNo = "0";
        //            maskEntity.MaterialID = string.Empty;
        //            maskEntity.MaterialStatus = eMaterialStatus.EMPTY;
        //        }

        //        //name相同下,狀態與新狀態不同,更新並上報
        //        if (status != maskEntity.MaterialStatus)
        //        {
        //            maskEntity.UnitNo = "0";
        //            maskEntity.MaterialID = name;
        //            maskEntity.MaterialStatus = status;
        //            blnReportMES = true;
        //        }

        //        if (blnReportMES)
        //        {
        //            object[] _data = new object[7]
        //        { 
        //            trxid,  /*0 TrackKey*/
        //            eqp.Data.LINEID,    /*1 LineName*/
        //            eqp.Data.NODEID,    /*2 EQPID*/
        //            maskEntity.MaterialID,          /*3 maskid*/
        //            maskEntity.MaterialSlotNo,            /*4 position*/ 
        //            maskEntity.MaterialStatus.ToString(),           /*5 status*/
        //            0,           /*6 count*/
        //        };
        //            //呼叫MES方法
        //            _service.Invoke(eServiceName.MESService, "MaskStateChanged", _data);

        //            NLogManager.Logger.LogInfoWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
        //                        string.Format("Set Mask ({0}) in slot ({1}) as ({2}).", maskEntity.MaterialID, maskEntity.MaterialSlotNo, maskEntity.MaterialStatus.ToString()));

        //        }

        //        ObjectManager.MaterialManager.EnqueueSave(maskEntity);
        //    }
        //}

        public string ConvertMesStatus(eEQPStatus eqpStatus)
        {
            switch (eqpStatus)
            {
                case eEQPStatus.IDLE:
                case eEQPStatus.RUN:
                    return eqpStatus.ToString();
                default:
                    return "DOWN";
            }
        }

        #region [For Recipe Registe Check Operation]
        public void AddCheckingRecipeID(RecipeCheckInfo check)
        {
            if (_waitCheckRecipeID == null)
                return;
            ConcurrentDictionary<string, RecipeInCheck> info;
            if (_waitCheckRecipeID.ContainsKey(check.EQPNo.Trim()))
                info = _waitCheckRecipeID[check.EQPNo.Trim()];
            else
            {
                info = new ConcurrentDictionary<string, RecipeInCheck>();
                _waitCheckRecipeID.TryAdd(check.EQPNo.Trim(), info);
            }

            if (info.ContainsKey(check.RecipeID))
                return;
            RecipeInCheck ric = new RecipeInCheck() { RequestTime = DateTime.Now, Info = check };
            info.TryAdd(check.RecipeID, ric);
        }

        public ConcurrentDictionary<string, RecipeInCheck> GetCheckingRecipeIDs(string eqpno)
        {

            if (string.IsNullOrEmpty(eqpno))
                return null;
            if (_waitCheckRecipeID == null)
                return null;
            if (!_waitCheckRecipeID.ContainsKey(eqpno))
                return null;
            ConcurrentDictionary<string, RecipeInCheck> info;
            if (!_waitCheckRecipeID.TryGetValue(eqpno, out info))
                return null;
            return info;
        }

        public RecipeInCheck GetCheckingRecipeID(string eqpno, string recipeid)
        {
            RecipeInCheck ric = new RecipeInCheck();
            if (string.IsNullOrEmpty(eqpno) || string.IsNullOrEmpty(recipeid))
                return ric;
            if (_waitCheckRecipeID == null)
                return ric;

            ConcurrentDictionary<string, RecipeInCheck> info = null;
            if (_waitCheckRecipeID.TryGetValue(eqpno, out info))
                info.TryGetValue(recipeid, out ric);

            return ric;
        }

        public void RemoveCheckingRecipeID(string eqpno, string recipeid)
        {
            if (string.IsNullOrEmpty(eqpno) || string.IsNullOrEmpty(recipeid))
                return;
            if (_waitCheckRecipeID == null)
                return;
            ConcurrentDictionary<string, RecipeInCheck> info = null;
            RecipeInCheck ric;
            string rKey = string.Empty;
            if (_waitCheckRecipeID.TryGetValue(eqpno, out info))
            {
                if (info.Count > 0)
                {
                    //逐筆檢查,符合的reciper或太久的都砍掉
                    foreach (string key in info.Keys)
                    {
                        if (key == recipeid)
                        {
                            if (info.TryRemove(key, out ric))
                            {
                                NLogManager.Logger.LogDebugWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("Remove checking recipe id from queue. RecipeID ({0})", key));
                            }
                        }
                        else
                        {
                            if ((DateTime.Now - info[key].RequestTime).TotalMinutes > 1)
                            {
                                if (info.TryRemove(key, out ric))
                                {
                                    NLogManager.Logger.LogDebugWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                        string.Format("Remove checking recipe id from queue which is over 1 minute. RecipeID ({0})", key));
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RemoveCheckingRecipeID(string eqpno)
        {
            if (string.IsNullOrEmpty(eqpno))
                return;
            if (_waitCheckRecipeID == null)
                return;

            ConcurrentDictionary<string, RecipeInCheck> info = null;
            if (_waitCheckRecipeID.ContainsKey(eqpno))
                if (_waitCheckRecipeID.TryRemove(eqpno, out info))
                {
                    NLogManager.Logger.LogDebugWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("({0}) Remove all checking recipe id from queue.", eqpno));
                }

        } 
        #endregion

        #region [For Recipe Parameter Check Operation]
        public void AddCheckingRecipeParameter(RecipeCheckInfo check)
        {
            if (_waitCheckRecipeParameter == null)
                return;
            ConcurrentDictionary<string, RecipeInCheck> info;
            if (_waitCheckRecipeParameter.ContainsKey(check.EQPNo.Trim()))
                info = _waitCheckRecipeParameter[check.EQPNo.Trim()];
            else
            {
                info = new ConcurrentDictionary<string, RecipeInCheck>();
                _waitCheckRecipeParameter.TryAdd(check.EQPNo.Trim(), info);
            }

            if (info.ContainsKey(check.RecipeID))
                return;
            RecipeInCheck ric = new RecipeInCheck() { RequestTime = DateTime.Now, Info = check };
            info.TryAdd(check.RecipeID, ric);

        }

        public ConcurrentDictionary<string, RecipeInCheck> GetCheckingRecipeParameters(string eqpno)
        {
            if (string.IsNullOrEmpty(eqpno))
                return null;
            if (_waitCheckRecipeParameter == null)
                return null;
            if (!_waitCheckRecipeParameter.ContainsKey(eqpno))
                return null;
            ConcurrentDictionary<string, RecipeInCheck> info;
            if (!_waitCheckRecipeParameter.TryGetValue(eqpno, out info))
                return null;
            return info;
        }

        public RecipeInCheck GetCheckingRecipeParameter(string eqpno, string recipeid)
        {
            RecipeInCheck ric = new RecipeInCheck();
            if (string.IsNullOrEmpty(eqpno) || string.IsNullOrEmpty(recipeid))
                return ric;
            if (_waitCheckRecipeParameter == null)
                return ric;

            ConcurrentDictionary<string, RecipeInCheck> info = null;
            if (_waitCheckRecipeParameter.TryGetValue(eqpno, out info))
                info.TryGetValue(recipeid, out ric);

            return ric;
        }

        public void RemoveCheckingRecipeParameter(string eqpno, string recipeid)
        {
            if (string.IsNullOrEmpty(eqpno) || string.IsNullOrEmpty(recipeid))
                return;
            if (_waitCheckRecipeParameter == null)
                return;
            ConcurrentDictionary<string, RecipeInCheck> info = null;
            RecipeInCheck ric;
            string rKey = string.Empty;
            if (_waitCheckRecipeParameter.TryGetValue(eqpno, out info))
            {
                if (info.Count > 0)
                {
                    //逐筆檢查,符合的reciper或太久的都砍掉
                    foreach (string key in info.Keys)
                    {
                        if (key == recipeid)
                        {
                            if (info.TryRemove(key, out ric))
                            {
                                NLogManager.Logger.LogDebugWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("Remove checking recipe parameter from queue. RecipeID ({0})", key));
                            }
                            continue;
                        }
                        if ((DateTime.Now - info[key].RequestTime).TotalMinutes > 1)
                        {
                            if (info.TryRemove(key, out ric))
                            {
                                NLogManager.Logger.LogDebugWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("Remove checking recipe parameter from queue which is over 1 minute. RecipeID ({0})", key));
                            }
                        }
                    }
                }
            }
        }

        public void RemoveCheckingRecipeParameter(string eqpno)
        {
            if (string.IsNullOrEmpty(eqpno))
                return;
            if (_waitCheckRecipeParameter == null)
                return;

            ConcurrentDictionary<string, RecipeInCheck> info = null;
            if (_waitCheckRecipeParameter.ContainsKey(eqpno))
                if (_waitCheckRecipeParameter.TryRemove(eqpno, out info))
                {
                    NLogManager.Logger.LogDebugWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                    string.Format("({0}) Remove all checking recipe parameter from queue.", eqpno));
                }

        } 
        #endregion

        //public void ProcessDataReport(Equipment eqp, Job job, IList<ProductProcessData.ITEMc> mesdic, string trxid)
        //{
        //    try
        //    {
        //        //TODO:mesdic沒資料時,把job資料加到BCS的history,並產生一個空file
        //        if (mesdic == null)
        //        {
        //            NLogManager.Logger.LogWarnWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
        //                                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Process data collection is null.", eqp.Data.NODENO, trxid));
        //            return;
        //        }
        //        if (mesdic.Count == 0)
        //        {
        //            NLogManager.Logger.LogWarnWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
        //                                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Process data collection is empty.", eqp.Data.NODENO, trxid));
        //            return;
        //        }
               
        //        Dictionary<string, List<string>> edalis = new Dictionary<string, List<string>>();
        //        StringBuilder fileContent = new StringBuilder();
        //        foreach (ProductProcessData.ITEMc item in mesdic)
        //        {
        //            foreach (ProductProcessData.SITEc site in item.SITELIST)
        //            {
        //                #region For EDA Data
        //                //20141014,發現method大改,重新修正
        //                if (edalis.ContainsKey(item.ITEMNAME))
        //                {
        //                    List<string> list = edalis[item.ITEMNAME];
        //                    list.Add(site.SITENAME + ";" + site.SITEVALUE);
        //                }
        //                else
        //                {
        //                    List<string> list = new List<string>();
        //                    list.Add(site.SITENAME + ";" + site.SITEVALUE);
        //                    edalis.Add(item.ITEMNAME, list);
        //                }
        //                #endregion
        //                #region For BC History
        //                fileContent.AppendFormat("{0}_{1}={2},", item.ITEMNAME, site.SITENAME, site.SITEVALUE);
        //                #endregion
        //            }
        //        }
        //        //20150127 cy:增加判斷,避免沒資料
        //        if (fileContent.Length > 0)
        //            fileContent.Remove(fileContent.Length - 1, 1);

        //        #region [Report to MES]
        //        object[] _mes = new object[12]
        //        { 
        //            trxid,                              /*0 TrackKey*/
        //            eqp.Data.LINEID,                    /*1 LineName*/
        //            eqp.Data.NODEID,                    /*2 MachineName*/
        //            "",                                 /*3 UnitName*/
        //            job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].LOTNAME,  /*4 LotName*/
        //            job.FromCstID,         /*5 CarrierName*/
        //            job.GlassChipMaskBlockID,             /*6 ProductName*/
        //            job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME, /*7 ProductSpecName*/
        //            job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].PRODUCTSPECVER, /*8 ProductSpecVer*/
        //            job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME, /*9 ProcessOperationName*/
        //            job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].LINERECIPENAME, /*10 LineRecipeName*/
        //            mesdic             /*11 ItemList*/
        //        };

        //        _service.Invoke(eServiceName.MESService, "ProductProcessData", _mes); 
        //        #endregion
        //        #region [Repot to EDA]
        //        //20141014,發現method大改,重新修正
        //        //20150306 cy, start time/end time付值 (注意:由於EDAReport內在上報時間時,會做一個("20" + eqpStartTime)的動作,所以yy只能取2碼)
        //        object[] _eda = new object[8]
        //        { 
        //            trxid,  /*0 TrackKey*/
        //            eqp.Data.LINEID,    /*1 LineName*/
        //            eqp.Data.NODEID,    /*2 MachineName*/
        //            (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].StartTime.ToString("yyMMddHHmmss") : string.Empty,    /*3 EQP Start Time*/
        //            (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].EndTime.ToString("yyMMddHHmmss") : string.Empty,    /*4 EQP End Time*/
        //            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),    /*5 Report Time*/
        //            job,                 /*2 Job*/
        //            edalis,
        //        };
        //        //to EDA format: List<item;site;value>
        //        _service.Invoke(eServiceName.EDAService, "EDAReport", _eda); 
        //        #endregion
        //        #region Save to File
        //        //Save to File
        //        ObjectManager.ProcessDataManager.MakeProcessDataValuesToFile(eqp.Data.NODEID, job.CassetteSequenceNo, job.JobSequenceNo, trxid, fileContent.ToString());
        //        #endregion
        //        #region Save to BCS History
        //        //Save to DB
        //        PROCESSDATAHISTORY paraHistory = new PROCESSDATAHISTORY();
        //        paraHistory.CASSETTESEQNO = int.Parse(job.CassetteSequenceNo);
        //        paraHistory.JOBSEQNO = int.Parse(job.JobSequenceNo);
        //        paraHistory.JOBID = job.GlassChipMaskBlockID;
        //        paraHistory.TRXID = trxid;
        //        paraHistory.NODEID = eqp.Data.NODEID;
        //        paraHistory.UPDATETIME = DateTime.Now;
        //        paraHistory.FILENAMA = string.Format("{0}_{1}_{2}_{3}", job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, trxid);
        //        paraHistory.PROCESSTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? (job.JobProcessFlows[eqp.Data.NODEID].EndTime - job.JobProcessFlows[eqp.Data.NODEID].StartTime).Seconds : 0;
        //        paraHistory.LOCALPROCESSSTARTTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].StartTime.ToString("yyyyMMddHHmmss") : string.Empty;
        //        paraHistory.LOCALPROCSSSENDTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].EndTime.ToString("yyyyMMddHHmmss") : string.Empty;
        //        ObjectManager.ProcessDataManager.SaveProcessDataHistory(paraHistory); 
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        //20150325 cy:增加以判斷區別是否有Group
        public void ProcessDataReport(Equipment eqp, Job job, IList<ProductProcessData.ITEMc> mesdic, string trxid, bool naGroup)
        {
            try
            {
                string localProcessStartTime = string.Empty; // add for BC EDC History Save use 2016/04/01
                //TODO:mesdic沒資料時,把job資料加到BCS的history,並產生一個空file
                if (mesdic == null)
                {
                    NLogManager.Logger.LogWarnWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Process data collection is null.", eqp.Data.NODENO, trxid));
                    return;
                }
                if (mesdic.Count == 0)
                {
                    NLogManager.Logger.LogWarnWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Process data collection is empty.", eqp.Data.NODENO, trxid));
                    return;
                }

                Dictionary<string, List<string>> edalis = new Dictionary<string, List<string>>();
                StringBuilder fileContent = new StringBuilder();
                foreach (ProductProcessData.ITEMc item in mesdic)
                {
                    foreach (ProductProcessData.SITEc site in item.SITELIST)
                    {

                        if (naGroup)
                        {
                            #region For EDA Data
                            if (edalis.ContainsKey("NA"))
                            {
                                List<string> list = edalis["NA"];
                                list.Add(item.ITEMNAME + ";" + site.SITEVALUE);
                            }
                            else
                            {
                                List<string> list = new List<string>();
                                list.Add(item.ITEMNAME + ";" + site.SITEVALUE);
                                edalis.Add("NA", list);
                            }
                            #endregion
                            #region For BC History
                            fileContent.AppendFormat("{0}={1},", item.ITEMNAME, site.SITEVALUE);
                            #endregion
                        }
                        else
                        {
                            #region For EDA Data
                            //20141014,發現method大改,重新修正
                            if (edalis.ContainsKey(item.ITEMNAME))
                            {
                                List<string> list = edalis[item.ITEMNAME];
                                list.Add(site.SITENAME + ";" + site.SITEVALUE);
                            }
                            else
                            {
                                List<string> list = new List<string>();
                                list.Add(site.SITENAME + ";" + site.SITEVALUE);
                                edalis.Add(item.ITEMNAME, list);
                            }
                            #endregion
                            #region For BC History
                            fileContent.AppendFormat("{0}={1},", site.SITENAME, site.SITEVALUE);
                            #endregion
                        }
                    }
                }
                //20150127 cy:增加判斷,避免沒資料
                if (fileContent.Length > 0)
                    fileContent.Remove(fileContent.Length - 1, 1);

                #region [Report to MES]
                object[] _mes = new object[12]
                { 
                    trxid,                              /*0 TrackKey*/
                    eqp.Data.LINEID,                    /*1 LineName*/
                    eqp.Data.NODEID,                    /*2 MachineName*/
                    "",                                 /*3 UnitName*/
                    job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].LOTNAME,  /*4 LotName*/
                    job.FromCstID,         /*5 CarrierName*/
                    job.GlassChipMaskBlockID,             /*6 ProductName*/
                    job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME, /*7 ProductSpecName*/
                    job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].PRODUCTSPECVER, /*8 ProductSpecVer*/
                    job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME, /*9 ProcessOperationName*/
                    job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].LINERECIPENAME, /*10 LineRecipeName*/
                    mesdic             /*11 ItemList*/
                };

                _service.Invoke(eServiceName.MESService, "ProductProcessData", _mes);
                #endregion
                #region [Repot to EDA]
                //20141014,發現method大改,重新修正
                //20150306 cy, start time/end time付值 (注意:由於EDAReport內在上報時間時,會做一個("20" + eqpStartTime)的動作,所以yy只能取2碼)
                object[] _eda = new object[8]
                { 
                    trxid,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 MachineName*/
                    (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].StartTime.ToString("yyMMddHHmmss") : string.Empty,    /*3 EQP Start Time*/
                    
                    //(job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].EndTime.ToString("yyMMddHHmmss") : string.Empty,    /*4 EQP End Time*/
                    DateTime.Now.ToString("yyMMddHHmmss"),//上报process Data 时 还没有上报send out，故此时还没有EndTime 20150601 tom 
                    
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),    /*5 Report Time*/
                    job,                 /*2 Job*/
                    edalis,
                };

                // add for BC EDC History Save use 2016/04/01 cc.kuang
                if (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID))
                {
                    localProcessStartTime = job.JobProcessFlows[eqp.Data.NODEID].StartTime.ToString("yyMMddHHmmss");
                }

                //to EDA format: List<item;site;value>
                _service.Invoke(eServiceName.EDAService, "EDAReport", _eda);
                #endregion
                #region Save to File
                //Save to File
                ObjectManager.ProcessDataManager.MakeProcessDataValuesToFile(eqp.Data.NODEID, job.CassetteSequenceNo, job.JobSequenceNo, trxid, fileContent.ToString());
                #endregion
                #region Save to BCS History
                //Save to DB
                PROCESSDATAHISTORY paraHistory = new PROCESSDATAHISTORY();
                paraHistory.CASSETTESEQNO = int.Parse(job.CassetteSequenceNo);
                paraHistory.JOBSEQNO = int.Parse(job.JobSequenceNo);
                paraHistory.JOBID = job.GlassChipMaskBlockID;
                paraHistory.TRXID = trxid;
                paraHistory.NODEID = eqp.Data.NODEID;
                paraHistory.UPDATETIME = DateTime.Now;
                paraHistory.FILENAMA = string.Format("{0}_{1}_{2}_{3}", job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, trxid);
                //paraHistory.PROCESSTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? (job.JobProcessFlows[eqp.Data.NODEID].EndTime - job.JobProcessFlows[eqp.Data.NODEID].StartTime).Seconds : 0;
                //paraHistory.PROCESSTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? (DateTime.Now - job.JobProcessFlows[eqp.Data.NODEID].StartTime).Seconds : 0;
                //modify for get total seconds 2016/04/07 cc.kuang
                paraHistory.PROCESSTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? (int)Math.Round((DateTime.Now - job.JobProcessFlows[eqp.Data.NODEID].StartTime).TotalSeconds) : 0;
                //paraHistory.LOCALPROCESSSTARTTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].StartTime.ToString("yyyyMMddHHmmss") : string.Empty;
                paraHistory.LOCALPROCESSSTARTTIME = localProcessStartTime; //add for BC EDC History Save use 2016/04/01 cc.kuang
                //paraHistory.LOCALPROCSSSENDTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].EndTime.ToString("yyyyMMddHHmmss") : string.Empty;
                paraHistory.LOCALPROCSSSENDTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
                ObjectManager.ProcessDataManager.SaveProcessDataHistory(paraHistory);

                ////将文件记录到Trace  20150603 Tom
                //NLogManager.Logger.LogTrxWrite(_logName,string.Format("[EQUIPMENT={0}] [TrackKey={1}] [CstSeqNo={2}] [JobSeqNo={3}] [GlsID={4}] [LocalProcessingTime={5}] [LocalProcessStartTime={6}] [LocalProcessEndTime={7}]\r\n[RAWDATA={8}]",
                //        eqp.Data.NODENO, trxid, paraHistory.CASSETTESEQNO,
                //        paraHistory.JOBSEQNO, paraHistory.JOBID, paraHistory.PROCESSTIME,
                //        paraHistory.LOCALPROCESSSTARTTIME, paraHistory.LOCALPROCSSSENDTIME, fileContent.Replace(',', '\n').ToString()));              

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //Dictionary<string, Tuple<string, string, string, string, string>> = Dictionary<ParameterName, Tuple<PDCName,Item,Site,Type,Value>>; Item=Group
        public void ProcessDataReport(Equipment eqp, Job job, Dictionary<string, Tuple<string, string, string, string, string>> pdata, string trxid)
        {
            try
            {
                  //mesdic沒資料時,把job資料加到BCS的history,並產生一個空file
                StringBuilder fileContent = new StringBuilder();
                if (pdata == null)
                {
                    NLogManager.Logger.LogWarnWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Process data collection is null.", eqp.Data.NODENO, trxid));
                    goto SaveHistory;
                }
                if (pdata.Count == 0)
                {
                    NLogManager.Logger.LogWarnWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Process data collection is empty.", eqp.Data.NODENO, trxid));
                    goto SaveHistory;
                }

                IList<ProductProcessData.ITEMc> mesdic = new List<ProductProcessData.ITEMc>();
                Dictionary<string, List<string>> edalis = new Dictionary<string, List<string>>();

                //取得EDC Format
                //IList<ProcessData> processDatas = ObjectManager.ProcessDataManager.GetProcessData(eqp.Data.NODENO);

                foreach(string key in pdata.Keys)
                {
                    string reportTo = "MES,EDA";
                    //if (processDatas != null)
                    //{
                    //    ProcessData pd = processDatas.FirstOrDefault(p => p.Data.PARAMETERNAME == pdata[i].Item1);
                    //    if (pd != null)
                    //        reportTo = pd.Data.REPORTTO.Trim();
                    //}

                    #region MES Data
                    //沒指定或指定報MES
                    if (string.IsNullOrEmpty(reportTo) || reportTo.Contains("MES"))
                    {
                        ProductProcessData.ITEMc itemC = new ProductProcessData.ITEMc();
                        itemC.ITEMNAME = key;
                        itemC.SITELIST.Add(new ProductProcessData.SITEc() { SITENAME = pdata[key].Item3.Trim(), SITEVALUE = pdata[key].Item5.Trim() });
                        mesdic.Add(itemC);
                    }
                    #endregion
                    #region EDA Data
                    //沒指定或指定報EDA
                    if (string.IsNullOrEmpty(reportTo) || reportTo.Contains("EDA"))
                    {
                          //20150603 cy:value為空時報NA
                          string val = string.IsNullOrEmpty(pdata[key].Item5.Trim()) ? "NA" : pdata[key].Item5.Trim();
                        if (edalis.ContainsKey(pdata[key].Item2.Trim()))
                        {
                            List<string> list = edalis[pdata[key].Item2.Trim()];
                            list.Add(key + ";" + val);//pdata[key].Item5.Trim());
                        }
                        else
                        {
                            List<string> list = new List<string>();
                            list.Add(key + ";" + val);//pdata[key].Item5.Trim());
                            edalis.Add(pdata[key].Item2.Trim(), list);
                        }
                    }
                    #endregion
                    #region BC History
                    fileContent.AppendFormat("{0}={1},", key, pdata[key].Item5.Trim());
                    #endregion
                }

                //20150127 cy:增加判斷,避免沒資料
                if (fileContent.Length > 0)
                    fileContent.Remove(fileContent.Length - 1, 1);

                #region [Report to MES]
                object[] _mes = new object[12]
                { 
                    trxid,                              /*0 TrackKey*/
                    eqp.Data.LINEID,                    /*1 LineName*/
                    eqp.Data.NODEID,                    /*2 MachineName*/
                    "",                                 /*3 UnitName*/
                    job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].LOTNAME,  /*4 LotName*/
                    job.FromCstID,         /*5 CarrierName*/
                    job.GlassChipMaskBlockID,             /*6 ProductName*/
                    job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME, /*7 ProductSpecName*/
                    job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].PRODUCTSPECVER, /*8 ProductSpecVer*/
                    job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME, /*9 ProcessOperationName*/
                    job.MesCstBody.LOTLIST.Count==0?string.Empty:job.MesCstBody.LOTLIST[0].LINERECIPENAME, /*10 LineRecipeName*/
                    mesdic             /*11 ItemList*/
                };

                _service.Invoke(eServiceName.MESService, "ProductProcessData", _mes);
                #endregion
                #region [Repot to EDA]
                //20141014,發現method大改,重新修正
                //20150306 cy, start time/end time付值 (注意:由於EDAReport內在上報時間時,會做一個("20" + eqpStartTime)的動作,所以yy只能取2碼)
                object[] _eda = new object[8]
                { 
                    trxid,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 MachineName*/
                    (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].StartTime.ToString("yyMMddHHmmss") : string.Empty,    /*3 EQP Start Time*/
                    //(job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].EndTime.ToString("yyMMddHHmmss") : string.Empty,    /*4 EQP End Time*/
                    DateTime .Now.ToString("yyMMddHHmmss"),//上报process Data 时 还没有上报send out，故此时还没有EndTime 20150601 tom 
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),    /*5 Report Time*/
                    job,                 /*2 Job*/
                    edalis,
                };
                //to EDA format: List<item;site;value>
                _service.Invoke(eServiceName.EDAService, "EDAReport", _eda);
                #endregion

            SaveHistory:
                #region Save to File
                //Save to File
                ObjectManager.ProcessDataManager.MakeProcessDataValuesToFile(eqp.Data.NODEID, job.CassetteSequenceNo, job.JobSequenceNo, trxid, fileContent.ToString());

                 #endregion
                #region Save to BCS History
                //Save to DB
                PROCESSDATAHISTORY paraHistory = new PROCESSDATAHISTORY();
                paraHistory.CASSETTESEQNO = int.Parse(job.CassetteSequenceNo);
                paraHistory.JOBSEQNO = int.Parse(job.JobSequenceNo);
                paraHistory.JOBID = job.GlassChipMaskBlockID;
                paraHistory.TRXID = trxid;
                paraHistory.NODEID = eqp.Data.NODEID;
                paraHistory.UPDATETIME = DateTime.Now;
                paraHistory.FILENAMA = string.Format("{0}_{1}_{2}_{3}", job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODEID, trxid);
                //paraHistory.PROCESSTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? (DateTime.Now - job.JobProcessFlows[eqp.Data.NODEID].StartTime).Seconds : 0;
                //modify for get total seconds 2016/04/07 cc.kuang
                paraHistory.PROCESSTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? (int)Math.Round((DateTime.Now - job.JobProcessFlows[eqp.Data.NODEID].StartTime).TotalSeconds) : 0;
                paraHistory.LOCALPROCESSSTARTTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].StartTime.ToString("yyyyMMddHHmmss") : string.Empty;
                //paraHistory.LOCALPROCSSSENDTIME = (job.JobProcessFlows.ContainsKey(eqp.Data.NODEID)) ? job.JobProcessFlows[eqp.Data.NODEID].EndTime.ToString("yyyyMMddHHmmss") : string.Empty;
                paraHistory.LOCALPROCSSSENDTIME = DateTime.Now.ToString("yyyyMMddHHmmss"); // Process Data Report 是在Send Out之前上报所以可能会没有时间 20150601 Tom
                ObjectManager.ProcessDataManager.SaveProcessDataHistory(paraHistory);
                
                ////将文件记录到Trace  20150603 Tom
                //NLogManager.Logger.LogTrxWrite(_logName,string.Format("[EQUIPMENT={0}] [TrackKey={1}] [CstSeqNo={2}] [JobSeqNo={3}] [GlsID={4}] [LocalProcessingTime={5}] [LocalProcessStartTime={6}] [LocalProcessEndTime={7}]\r\n[RAWDATA={8}]",
                //            eqp.Data.NODENO, trxid, paraHistory.CASSETTESEQNO, 
                //            paraHistory.JOBSEQNO, paraHistory.JOBID, paraHistory.PROCESSTIME,
                //            paraHistory.LOCALPROCESSSTARTTIME, paraHistory.LOCALPROCSSSENDTIME, fileContent.Replace(',', '\n').ToString()));              

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void LotProcessDataReport(Equipment eqp, string cstSeqNo, IList<ProductProcessData.ITEMc> mesdic, string trxid)
        {
            try
            {
                if (mesdic == null)
                {
                    NLogManager.Logger.LogInfoWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Lot process data collection is null.", eqp.Data.NODENO, trxid));
                    return;
                }
                if (mesdic.Count == 0)
                {
                    NLogManager.Logger.LogInfoWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Lot process data collection is empty.", eqp.Data.NODENO, trxid));
                    return;
                }
                StringBuilder fileContent = new StringBuilder();
                foreach (ProductProcessData.ITEMc item in mesdic)
                {
                    foreach (ProductProcessData.SITEc site in item.SITELIST)
                    {
                        fileContent.AppendFormat("{0}_{1}={2}", item.ITEMNAME, site.SITENAME, site.SITEVALUE);
                    }
                }
                fileContent.Remove(fileContent.Length - 1, 1);

                //Save to File
                string fileName = string.Empty;
                ObjectManager.ProcessDataManager.MakeLotProcessDataValuesToFile(eqp.Data.NODEID, cstSeqNo, trxid, fileContent.ToString(), out fileName);

                if (!string.IsNullOrEmpty(fileName))
                {
                    //Save to DB
                    PROCESSDATAHISTORY paraHistory = new PROCESSDATAHISTORY();
                    paraHistory.CASSETTESEQNO = int.Parse(cstSeqNo);
                    paraHistory.JOBSEQNO = 0;
                    paraHistory.JOBID = string.Empty;
                    paraHistory.TRXID = trxid;
                    paraHistory.NODEID = eqp.Data.NODEID;
                    paraHistory.UPDATETIME = DateTime.Now;
                    paraHistory.FILENAMA = fileName;
                    ObjectManager.ProcessDataManager.SaveProcessDataHistory(paraHistory);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(_logName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region [Process Data Collection Operation]
        public void AddRawProcessData(string eqpno, string jobid, string subcd, XmlDocument doc)
        {
            ConcurrentDictionary<string, ProcessDataSpool> glass = new ConcurrentDictionary<string, ProcessDataSpool>();
            if (_processData.ContainsKey(eqpno))
                _processData.TryGetValue(eqpno, out glass);
            else
                _processData.TryAdd(eqpno, glass);

            ProcessDataSpool data = new ProcessDataSpool();
            if (glass.ContainsKey(jobid))
                glass.TryGetValue(jobid, out data);
            else
                glass.TryAdd(jobid, data);

            if (data.ProcessDataRaw.ContainsKey(subcd))
                data.ProcessDataRaw.Remove(subcd);

            data.LastReceiveSUBCD = subcd;
            data.LastReceiveTime = DateTime.Now;
            data.ProcessDataRaw.Add(subcd, doc);
        }

        public ConcurrentDictionary<string, ProcessDataSpool> GetRawProcessData(string eqpno)
        {
            ConcurrentDictionary<string, ProcessDataSpool> glass = null;
            if (_processData.ContainsKey(eqpno))
                glass = _processData[eqpno];
            return glass;
        }

        public ProcessDataSpool GetRawProcessData(string eqpno, string jobid)
        {
            ProcessDataSpool data = null;
            if (_processData.ContainsKey(eqpno))
            {
                ConcurrentDictionary<string, ProcessDataSpool> glass = _processData[eqpno];
                if (glass.ContainsKey(jobid))
                    data = glass[jobid];
            }
            return data;
        }

        public void RemoveRawProcessData(string eqpno, string jobid)
        {
            if (_processData.ContainsKey(eqpno))
            {
                if (_processData[eqpno].ContainsKey(jobid))
                {
                    ProcessDataSpool pds = new ProcessDataSpool();
                    _processData[eqpno].TryRemove(jobid, out pds);
                }
            }
        }

        public void RemoveAllRawProcessData(string eqpno)
        {
            if (_processData.ContainsKey(eqpno))
                _processData[eqpno].Clear();
        } 
        #endregion

        #region [For Lot Process Data Operation]
        public void AddRawLotProcessData(string eqpno, string cstseq, string subcd, XmlDocument doc)
        {
            ConcurrentDictionary<string, ProcessDataSpool> lot = new ConcurrentDictionary<string, ProcessDataSpool>();
            if (_lotProcessData.ContainsKey(eqpno))
                _lotProcessData.TryGetValue(eqpno, out lot);
            else
                _lotProcessData.TryAdd(eqpno, lot);

            ProcessDataSpool data = new ProcessDataSpool();
            if (lot.ContainsKey(cstseq))
                lot.TryGetValue(cstseq, out data);
            else
                lot.TryAdd(cstseq, data);

            if (data.ProcessDataRaw.ContainsKey(subcd))
                data.ProcessDataRaw.Remove(subcd);

            data.LastReceiveSUBCD = subcd;
            data.LastReceiveTime = DateTime.Now;
            data.ProcessDataRaw.Add(subcd, doc);
        }

        public ConcurrentDictionary<string, ProcessDataSpool> GetRawLotProcessData(string eqpno)
        {
            ConcurrentDictionary<string, ProcessDataSpool> glass = null;
            if (_lotProcessData.ContainsKey(eqpno))
                glass = _lotProcessData[eqpno];
            return glass;
        }

        public ProcessDataSpool GetRawLotProcessData(string eqpno, string cstseq)
        {
            ProcessDataSpool data = null;
            if (_lotProcessData.ContainsKey(eqpno))
            {
                ConcurrentDictionary<string, ProcessDataSpool> glass = _lotProcessData[eqpno];
                if (glass.ContainsKey(cstseq))
                    data = glass[cstseq];
            }
            return data;
        }

        public void RemoveRawLotProcessData(string eqpno, string cstseq)
        {
            if (_lotProcessData.ContainsKey(eqpno))
            {
                if (_lotProcessData[eqpno].ContainsKey(cstseq))
                {
                    ProcessDataSpool pds = new ProcessDataSpool();
                    _lotProcessData[eqpno].TryRemove(cstseq, out pds);
                }
            }
        }

        public void RemoveAllRawLotProcessData(string eqpno)
        {
            if (_lotProcessData.ContainsKey(eqpno))
                _lotProcessData[eqpno].Clear();
        }  
        #endregion

        public string ToFormatString(string xml)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            XmlTextWriter xtw = null;
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);
                xtw = new XmlTextWriter(sw);
                xtw.Formatting = Formatting.Indented;
                xtw.Indentation = 1;
                xtw.IndentChar = '\t';
                xmldoc.WriteTo(xtw);
            }
            finally
            {
                if (xtw != null)
                    xtw.Close();
            }
            return sb.ToString();
        }

        public string GetMaterialType(Equipment eqp)
        {
            string type = string.Empty;
            switch (eqp.Data.NODEATTRIBUTE)
            {
                case "DNS":
                    type = "PR";
                    break;
                case "DRY":
                    type = "DRYPOLE";
                    break;
                case "CANON":
                case "NIKON":
                    type = "MASK";
                    break;
            }
            return type;
        }

        public bool CheckEquipmentSelectedToSend(string eqpno, string trxid, string className, string methodName)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                if (eqp == null)
                {
                    LogError(className, methodName, eqpno, false, trxid,
                            "Can not find Equipment Number in EquipmentEntity!");
                    return false;
                }
                if (!eqp.HsmsSelected)
                {
                    LogError(className, methodName, eqpno, false, trxid,
                            "HSMS connection status is NOT_SELECTED, can not send message.");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogError(className, methodName, eqpno, false, trxid,  "Check Equipment HSMS Selected Status Fail. :" + ex);
                return false;
            }
        }

        public DateTime DateTime14ToDateTime(string str)
        {
            try
            {
                if (str.Length == "yyyyMMddHHmmss".Length)
                {
                    string dtstr = string.Format("{0}-{1}-{2} {3}:{4}:{5}",
                    str.Substring(0, 4),
                    str.Substring(4, 2),
                    str.Substring(6, 2),
                    str.Substring(8, 2),
                    str.Substring(10, 2),
                    str.Substring(12, 2));
                    return DateTime.Parse(dtstr);
                }
                return Convert.ToDateTime(str);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }

    public struct RecipeInCheck
    {
        private DateTime _requestTime;
        private RecipeCheckInfo _info;

        public RecipeCheckInfo Info
        {
            get { return _info; }
            set { _info = value; }
        }

        public DateTime RequestTime
        {
            get { return _requestTime; }
            set { _requestTime = value; }
        }
    }

    public class ProcessDataSpool
    {
        private DateTime _startTime;
        private DateTime _lastReceiveTime;
        private string _lastReceiveSUBCD;
        Dictionary<string, XmlDocument> _processData;

        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        public DateTime LastReceiveTime
        {
            get { return _lastReceiveTime; }
            set { _lastReceiveTime = value; }
        }

        public string LastReceiveSUBCD
        {
            get { return _lastReceiveSUBCD; }
            set { _lastReceiveSUBCD = value; }
        }

        public Dictionary<string, XmlDocument> ProcessDataRaw
        {
            get { return _processData; }
            set { _processData = value; }
        }

        public ProcessDataSpool()
        {
            _startTime = DateTime.Now;
            _lastReceiveTime = DateTime.Now;
            _lastReceiveSUBCD = string.Empty;
            _processData = new Dictionary<string, XmlDocument>();
        }

    }
}
