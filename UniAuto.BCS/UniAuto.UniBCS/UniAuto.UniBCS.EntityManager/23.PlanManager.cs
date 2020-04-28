using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Log;
using System.Reflection;
using System.Collections;
using UniAuto.UniBCS.DB;
using System.Data;
using UniAuto.UniBCS.Core;

namespace UniAuto.UniBCS.EntityManager
{
    public class PlanManager : EntityManager,IDataSource
    {
        private PlanConstant _pLANDATA;
        private PlanConstant _standbyPLANDATA;

        public override EntityManager.FILE_TYPE GetFileType()
        {
            return FILE_TYPE.XML;
        }

        protected override string GetSelectHQL()
        {
            return string.Empty;
        }

        protected override Type GetTypeOfEntityData()
        {
            return typeof(LineEntityData);
        }

        protected override void AfterSelectDB(List<Entity.EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            Filenames = new List<string>();
            Filenames.Add("Plan.xml");
            Filenames.Add("StandbyPlan.xml");
        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(PlanConstant);
        }

        protected override Entity.EntityFile NewEntityFile(string Filename)
        {
            return new PlanConstant();
        }

        protected override void AfterInit(List<Entity.EntityData> entityDatas, List<Entity.EntityFile> entityFiles)
        {
            if (entityFiles.Count() > 0)
            {
                if (entityFiles.Count()==1)
                {
                    _pLANDATA = entityFiles[0] as PlanConstant;
                    _standbyPLANDATA = new PlanConstant();
                }
                if (entityFiles.Count() == 2)
                {
                    _pLANDATA = entityFiles[0] as PlanConstant;
                    _standbyPLANDATA = entityFiles[1] as PlanConstant;
                }
            }
            else
            {
                _pLANDATA = new PlanConstant();
                _standbyPLANDATA = new PlanConstant();
            }
        }

        public void AddOnlinePlans(string planID, IList<SLOTPLAN> plans)
        {
            if (plans == null) return;
            // 每次只會有一個Change Plan在執行, 當有下一個Plan執行時, 刪除掉之前的資料
            RemoveChangePlan();
            if (plans.Count() > 0)
            {
                Plan plan = new Plan();
                plan.PLAN_NAME = planID;

                foreach (SLOTPLAN s in plans)
                {
                    try
                    {
                        plan.PlanCollection.Add(s);
                    }
                    catch(Exception ex)
                    {
                        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                    }
                }
                _pLANDATA.PlanList.Add(plan);
                EnqueueSave(_pLANDATA);
            }
        }

        public void AddOnlinePlansStandby(string planID, IList<SLOTPLAN> plans)
        {
            if (plans == null) return;
            // 每次只會有一個Standby Plan在執行, 當有下一個Plan執行時, 刪除掉之前的資料
            _standbyPLANDATA.PlanList.Clear();
            if (plans.Count() > 0)
            {
                Plan plan = new Plan();
                plan.PLAN_NAME = planID;

                foreach (SLOTPLAN s in plans)
                {
                    try
                    {
                        plan.PlanCollection.Add(s);
                    }
                    catch (Exception ex)
                    {
                        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                    }
                }
                _standbyPLANDATA.PlanList.Add(plan);
                EnqueueSave(_standbyPLANDATA);
            }
        }

        /// <summary>
        /// Offline Reload DB Change Plan
        /// </summary>
        /// <param name="lineID"></param>
        public bool ReloadOfflinePlan(string lineID, string cstID)
        {
            try
            {
                // 先用LINE ID及 CST ID 取得PLAN ID
                IList tmpData = this.HibernateAdapter.GetObject_AND(
                                                            typeof(CHANGEPLAN),
                                                            new string[] { "LINEID", "SOURCECASSETTEID" },
                                                            new object[] { lineID, cstID },
                                                            null,
                                                            null);

                if (tmpData == null || tmpData.Count == 0) return false;

                string planID = ((CHANGEPLAN)tmpData[0]).PLANID;
                // 如果有值再使用LINE ID及 PLAN ID取得全部的PLAN DATA
                IList data = this.HibernateAdapter.GetObject_AND(
                                                            typeof(CHANGEPLAN),
                                                            new string[] { "LINEID", "PLANID" },
                                                            new object[] { lineID, planID },
                                                            null,
                                                            null);

                if (data != null && data.Count > 0)
                {
                    // 每次只會有一個Change Plan在執行, 當有下一個Plan執行時, 刪除掉之前的資料
                    RemoveChangePlan();

                    Plan plan = new Plan();
                    plan.PLAN_NAME = planID;

                    foreach (CHANGEPLAN c in data)
                    {
                        try
                        {
                            SLOTPLAN s = new SLOTPLAN()
                            {
                                PRODUCT_NAME = c.JOBID,
                                SOURCE_CASSETTE_ID = c.SOURCECASSETTEID,
                                TARGET_CASSETTE_ID = c.TARGETASSETTEID
                            };
                            int slot;
                            int.TryParse(c.SLOTNO, out slot);
                            s.SLOTNO = slot;
                            if (c.TARGETSLOTNO.Trim().Length == 0)
                                s.TARGET_SLOTNO = "000";
                            else
                                s.TARGET_SLOTNO = c.TARGETSLOTNO;
                            plan.PlanCollection.Add(s);
                        }
                        catch (Exception ex)
                        {
                            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        }
                    }
                    _pLANDATA.PlanList.Add(plan);
                    EnqueueSave(_pLANDATA);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        public bool ReloadOfflinePlanStandby(string lineID, string cstID)
        {
            try
            {
                // 先用LINE ID及 CST ID 取得PLAN ID
                IList tmpData = this.HibernateAdapter.GetObject_AND(
                                                            typeof(CHANGEPLAN),
                                                            new string[] { "LINEID", "SOURCECASSETTEID" },
                                                            new object[] { lineID, cstID },
                                                            null,
                                                            null);

                if (tmpData == null || tmpData.Count == 0) return false;

                string planID = ((CHANGEPLAN)tmpData[0]).PLANID;
                // 如果有值再使用LINE ID及 PLAN ID取得全部的PLAN DATA
                IList data = this.HibernateAdapter.GetObject_AND(
                                                            typeof(CHANGEPLAN),
                                                            new string[] { "LINEID", "PLANID" },
                                                            new object[] { lineID, planID },
                                                            null,
                                                            null);

                if (data != null && data.Count > 0)
                {
                    // 每次只會有一個Standby Plan在執行, 當有下一個Plan執行時, 刪除掉之前的資料
                    _standbyPLANDATA.PlanList.Clear();

                    Plan plan = new Plan();
                    plan.PLAN_NAME = planID;

                    foreach (CHANGEPLAN c in data)
                    {
                        try
                        {
                            SLOTPLAN s = new SLOTPLAN()
                            {
                                PRODUCT_NAME = c.JOBID,
                                SOURCE_CASSETTE_ID = c.SOURCECASSETTEID,
                                TARGET_CASSETTE_ID = c.TARGETASSETTEID
                            };
                            int slot;
                            int.TryParse(c.SLOTNO, out slot);
                            s.SLOTNO = slot;
                            if (c.TARGETSLOTNO.Trim().Length == 0)
                                s.TARGET_SLOTNO = "000";
                            else
                                s.TARGET_SLOTNO = c.TARGETSLOTNO;
                            plan.PlanCollection.Add(s);
                        }
                        catch (Exception ex)
                        {
                            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        }
                    }
                    _standbyPLANDATA.PlanList.Add(plan);
                    //EnqueueSave(_standbyPLANDATA);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        public bool ReloadOfflinePlan(string lineID, string cstID, string planID)
        {
            try
            {
                IList data = null;
                // 取得相同的Cst ID 但是不同的Plan ID
                IList tmpData = this.HibernateAdapter.GetObject_AND(
                                                            typeof(CHANGEPLAN),
                                                            new string[] { "LINEID", "SOURCECASSETTEID", "PLANID !" },
                                                            new object[] { lineID, cstID, planID },
                                                            null,
                                                            null);

                if (tmpData == null || tmpData.Count == 0)
                {
                    tmpData = this.HibernateAdapter.GetObject_AND(
                                                            typeof(CHANGEPLAN),
                                                            new string[] { "LINEID", "PLANID !" },
                                                            new object[] { lineID, planID },
                                                            null,
                                                            null);
                }

                if (tmpData == null || tmpData.Count == 0) return false;

                string nextPlanID = ((CHANGEPLAN)tmpData[0]).PLANID;

                // 如果有值再使用LINE ID及 PLAN ID取得全部的PLAN DATA
                data = this.HibernateAdapter.GetObject_AND(
                                                        typeof(CHANGEPLAN),
                                                        new string[] { "LINEID", "PLANID" },
                                                        new object[] { lineID, nextPlanID },
                                                        null,
                                                        null);

                if (data != null && data.Count > 0)
                {
                    // 每次只會有一個Change Plan在執行, 當有下一個Plan執行時, 刪除掉之前的資料
                    RemoveChangePlan();

                    Plan plan = new Plan();
                    plan.PLAN_NAME = nextPlanID;

                    foreach (CHANGEPLAN c in data)
                    {
                        try
                        {
                            SLOTPLAN s = new SLOTPLAN()
                            {
                                PRODUCT_NAME = c.JOBID,
                                SOURCE_CASSETTE_ID = c.SOURCECASSETTEID,
                                TARGET_CASSETTE_ID = c.TARGETASSETTEID
                            };
                            int slot;
                            int.TryParse(c.SLOTNO, out slot);
                            s.SLOTNO = slot;
                            if (c.TARGETSLOTNO.Trim().Length == 0)
                                s.TARGET_SLOTNO = "000";
                            else
                                s.TARGET_SLOTNO = c.TARGETSLOTNO;
                            plan.PlanCollection.Add(s);
                        }
                        catch (Exception ex)
                        {
                            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        }
                    }
                    _pLANDATA.PlanList.Add(plan);
                    EnqueueSave(_pLANDATA);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        public bool ReloadDBPlan(string lineID, string currentPlanID)
        {
            try
            {
                // 如果有值再使用LINE ID及 PLAN ID取得全部的PLAN DATA
                IList data = this.HibernateAdapter.GetObject_AND(
                                                            typeof(CHANGEPLAN),
                                                            new string[] { "LINEID", "PLANID" },
                                                            new object[] { lineID, currentPlanID },
                                                            null,
                                                            null);

                if (data != null && data.Count > 0)
                {
                    Plan plan = new Plan();
                    plan.PLAN_NAME = currentPlanID;

                    foreach (CHANGEPLAN c in data)
                    {
                        try
                        {
                            SLOTPLAN s = new SLOTPLAN()
                            {
                                PRODUCT_NAME = c.JOBID,
                                SOURCE_CASSETTE_ID = c.SOURCECASSETTEID,
                                TARGET_CASSETTE_ID = c.TARGETASSETTEID
                            };
                            int slot;
                            int.TryParse(c.SLOTNO, out slot);
                            s.SLOTNO = slot;
                            if (c.TARGETSLOTNO.Trim().Length == 0)
                                s.TARGET_SLOTNO = "000";
                            else
                                s.TARGET_SLOTNO = c.TARGETSLOTNO;
                            plan.PlanCollection.Add(s);
                        }
                        catch (Exception ex)
                        {
                            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        }
                    }
                    //_pLANDATA.PlanList.RemoveAll(p => p.PLAN_NAME.Trim().Equals(currentPlanID.Trim())); t3 just one plan
                    _pLANDATA.PlanList.Clear();
                    _pLANDATA.PlanList.Add(plan);
                    EnqueueSave(_pLANDATA);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// 刪除所有的Change Plan
        /// </summary>
        public void RemoveChangePlan()
        {
            _pLANDATA.PlanList.Clear();
            EnqueueSave(_pLANDATA);
        }

        public void RemoveChangePlanStandby()
        {
            _standbyPLANDATA.PlanList.Clear();
            EnqueueSave(_standbyPLANDATA);
        }

        public void RemoveChangePlan(string planID)
        {
            _pLANDATA.PlanList.RemoveAll(p => p.PLAN_NAME.Trim().Equals(planID.Trim()));
            EnqueueSave(_pLANDATA);
        }

        ///// <summary>
        ///// 依據Plan ID, Soruce Cst ID將Slot Plan設成已做過
        ///// </summary>
        ///// <param name="planID"></param>
        ///// <param name="sourceCstID"></param>
        //public void HaveBeenUsePlanByCstID(string planID, string sourceCstID)
        //{
        //    try
        //    {
        //        List<SLOTPLAN> plans = _pLANDATA.PlanCollection.Where(p => p.SOURCE_CASSETTE_ID.Equals(sourceCstID)).ToList<SLOTPLAN>();

        //        for (int i = 0; i < plans.Count(); i++)
        //        {
        //            plans[i].HAVE_BEEN_USED = true;
        //        }
        //        EnqueueSave(_pLANDATA);
        //        HibernateAdapter.DeleteObject_AND(typeof(CHANGEPLAN), new string[] { "PLANID", "SOURCECASSETTEID" },
        //                                          new object[] { planID, sourceCstID }, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        /// 依據Plan ID將DB的Plan刪除
        /// </summary>
        /// <param name="planID"></param>
        /// <param name="sourceCstID"></param>
        public void DeleteOffLinePlanInDB(string planID)
        {
            try
            {
                HibernateAdapter.DeleteObject_AND(typeof(CHANGEPLAN), new string[] { "PLANID" },
                                                  new object[] { planID }, null);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        ///  檢查 CST ID 是否在PLAN裡
        /// </summary>
        /// <param name="planID"></param>
        /// <param name="cstID"></param>
        /// <returns></returns>
        public bool CstExistInChangerPlan(string planID, string cstID)
        {
            try
            {
                if (string.IsNullOrEmpty(planID.Trim())) return false;
                Plan plan = _pLANDATA.PlanList.FirstOrDefault(p => p.PLAN_NAME.Trim().Equals(planID.Trim()));

                if (plan == null) return false;

                return plan.PlanCollection.Any(s => s.SOURCE_CASSETTE_ID.Trim().Equals(cstID.Trim()) || s.TARGET_CASSETTE_ID.Trim().Equals(cstID.Trim()));
            }
            catch { 
                return false;}
        }

        /// <summary>
        ///  檢查 CST ID 是否在StandByPLAN裡
        /// </summary>
        /// <param name="planID"></param>
        /// <param name="cstID"></param>
        /// <returns></returns>
        public bool CstExistInStandByChangerPlan(string standByplanID, string cstID)
        {
            try
            {
                if (string.IsNullOrEmpty(standByplanID.Trim())) return false;
                Plan standByplan = _standbyPLANDATA.PlanList.FirstOrDefault(p => p.PLAN_NAME.Trim().Equals(standByplanID.Trim()));

                if (standByplan == null) return false;

                return standByplan.PlanCollection.Any(s => s.SOURCE_CASSETTE_ID.Trim().Equals(cstID.Trim()) || s.TARGET_CASSETTE_ID.Trim().Equals(cstID.Trim()));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 依據Product Name將Slot Plan設成已做過
        /// </summary>
        /// <param name="planID"></param>
        /// <param name="sourceCstID"></param>
        public void HaveBeenUsePlanByProductName(string currentPlanID, string productName)
        {
            try
            {
                Plan plan = _pLANDATA.PlanList.FirstOrDefault(p => p.PLAN_NAME.Trim().Equals(currentPlanID.Trim()));
                if (plan == null) return;

                SLOTPLAN slot = plan.PlanCollection.FirstOrDefault(p => p.PRODUCT_NAME.Trim().Equals(productName.Trim()));

                if (slot != null)
                {
                    slot.HAVE_BEEN_USED = true;
                    EnqueueSave(_pLANDATA);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public IList<SLOTPLAN> GetProductPlans(out string planID)
        {
            planID = string.Empty;
            if (_pLANDATA.PlanList.Count() == 0)
            {
                return new List<SLOTPLAN>();
            }
            else
            {
                planID = _pLANDATA.PlanList[0].PLAN_NAME;
                return _pLANDATA.PlanList[0].PlanCollection;
            }
        }

        public IList<SLOTPLAN> GetProductPlansStandby(out string planID)
        {
            planID = string.Empty;
            if (_standbyPLANDATA.PlanList.Count() == 0)
            {
                return new List<SLOTPLAN>();
            }
            else
            {
                planID = _standbyPLANDATA.PlanList[0].PLAN_NAME;
                return _standbyPLANDATA.PlanList[0].PlanCollection;
            }
        }

        public IList<SLOTPLAN> GetProductPlans(string planID)
        {
            Plan plan = _pLANDATA.PlanList.FirstOrDefault(p => p.PLAN_NAME.Trim().Equals(planID.Trim()));

            if (plan != null)
                return plan.PlanCollection;
            else
                return new List<SLOTPLAN>();
        }

        public IList<SLOTPLAN> GetProductPlansByCstID(string planID, string sourceCstID)
        {
            List<SLOTPLAN> obj = new List<SLOTPLAN>();

            foreach (Plan plan in _pLANDATA.PlanList)
            {
                if (plan.PLAN_NAME.Trim() != planID.Trim()) continue;
                foreach (SLOTPLAN s in plan.PlanCollection)
                {
                    if (s.SOURCE_CASSETTE_ID.Trim().Equals(sourceCstID.Trim()) && s.HAVE_BEEN_USED == false)
                    {
                        obj.Add(s);
                    }
                }
            }
            return obj;
        }

        public IList<SLOTPLAN> GetProductStandByPlansByCstID(string planID, string sourceCstID)
        {
            List<SLOTPLAN> obj = new List<SLOTPLAN>();

            foreach (Plan plan in _standbyPLANDATA.PlanList)
            {
                if (plan.PLAN_NAME.Trim() != planID.Trim()) continue;
                foreach (SLOTPLAN s in plan.PlanCollection)
                {
                    if (s.SOURCE_CASSETTE_ID.Trim().Equals(sourceCstID.Trim()) && s.HAVE_BEEN_USED == false)
                    {
                        obj.Add(s);
                    }
                }
            }
            return obj;
        }

        public IList<SLOTPLAN> GetProductPlansByTargetCstID(string planID, string targetCstID)
        {
            List<SLOTPLAN> obj = new List<SLOTPLAN>();

            foreach (Plan plan in _pLANDATA.PlanList)
            {
                if (plan.PLAN_NAME.Trim() != planID.Trim()) continue;
                foreach (SLOTPLAN s in plan.PlanCollection)
                {
                    if (s.TARGET_CASSETTE_ID.Trim().Equals(targetCstID.Trim()) && s.HAVE_BEEN_USED == false)
                    {
                        obj.Add(s);
                    }
                }
            }
            return obj;
        }

        public void SavePlanStatusInDB(string planID, ePLAN_STATUS status)
        {
            string sourceCSTID = string.Empty;
            string targetCSTID = string.Empty;

            Plan plan = _pLANDATA.PlanList.FirstOrDefault(p => p.PLAN_NAME.Trim().Equals(planID.Trim()));

            if (plan != null)
            {
                List<string> sourceCstIDs = plan.PlanCollection.Select(p => p.SOURCE_CASSETTE_ID).Distinct().ToList<string>();
                List<string> targetCstIDs = plan.PlanCollection.Select(p => p.TARGET_CASSETTE_ID).Distinct().ToList<string>();

                foreach (string s in sourceCstIDs)
                {
                    if (string.IsNullOrEmpty(sourceCSTID))
                        sourceCSTID += s;
                    else
                        sourceCSTID += "," + s;
                }

                foreach (string t in targetCstIDs)
                {
                    if (string.IsNullOrEmpty(targetCSTID))
                        targetCSTID += t;
                    else
                        targetCSTID += "," + t;
                }
            }
            else if (status == ePLAN_STATUS.READY || status == ePLAN_STATUS.START || status == ePLAN_STATUS.REQUEST)
            {
                planID += "(Unknown)";
            }

            CHANGEPLANHISTORY history = new CHANGEPLANHISTORY()
            {
                PLANID = planID,
                SOURCECASSETTEID = sourceCSTID,
                TARGETASSETTEID = targetCSTID,
                PLANSTATUS = status.ToString(),
                UPDATETIME = DateTime.Now
            };
            InsertDB(history);
        }

        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("PlanManager");
            return entityNames;
        }

        /// <summary>
        /// 在Objects 画面显示Plan
        /// </summary>
        /// <returns></returns>
        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();

                ShowPlan file = new ShowPlan();

                DataTableHelp.DataTableAppendColumn(file, dt);

                foreach (Plan plan in _pLANDATA.PlanList)
                {
                    string planID = plan.PLAN_NAME;

                    foreach (SLOTPLAN s in plan.PlanCollection)
                    {
                        ShowPlan entity = new ShowPlan()
                        {
                            PLAN_ID = planID,
                            SLOTNO = s.SLOTNO,
                            PRODUCT_NAME = s.PRODUCT_NAME,
                            HAVE_BEEN_USED = s.HAVE_BEEN_USED,
                            SOURCE_CASSETTE_ID = s.SOURCE_CASSETTE_ID,
                            TARGET_CASSETTE_ID = s.TARGET_CASSETTE_ID,
                            TARGET_SLOTNO = s.TARGET_SLOTNO
                        };
                        DataRow dr = dt.NewRow();
                        DataTableHelp.DataRowAssignValue(entity, dr);
                        dt.Rows.Add(dr);
                    }
                }
                return dt;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
            
        }

        public Plan GetPlan(string planID)
        {
            Plan ret =null;
            foreach (Plan plan in _pLANDATA.PlanList)
            {
                if (plan.PLAN_NAME.Trim() != planID.Trim()) continue;
                ret = plan;
                return ret;
            }
            return ret;
        }

    }

    public class ShowPlan
    {
        private string _plinID = string.Empty;
        private string _pRODUCTNAME = string.Empty;
        private int _sLOTNO = 0;
        private bool _hAVEBEENUSED = false;
        private string _sOURCECASID = string.Empty;
        private string _tARGETCSTID = string.Empty;
        private string _tARGETSLOTNO = string.Empty;

        public string PLAN_ID
        {
            get { return _plinID; }
            set { _plinID = value; }
        }

        public string PRODUCT_NAME
        {
            get { return _pRODUCTNAME; }
            set { _pRODUCTNAME = value; }
        }

        public int SLOTNO
        {
            get { return _sLOTNO; }
            set { _sLOTNO = value; }
        }

        public bool HAVE_BEEN_USED
        {
            get { return _hAVEBEENUSED; }
            set { _hAVEBEENUSED = value; }
        }

        public string SOURCE_CASSETTE_ID
        {
            get { return _sOURCECASID; }
            set { _sOURCECASID = value; }
        }

        public string TARGET_CASSETTE_ID
        {
            get { return _tARGETCSTID; }
            set { _tARGETCSTID = value; }
        }

        public string TARGET_SLOTNO
        {
            get { return _tARGETSLOTNO; }
            set { _tARGETSLOTNO = value; }
        }
    }
}
