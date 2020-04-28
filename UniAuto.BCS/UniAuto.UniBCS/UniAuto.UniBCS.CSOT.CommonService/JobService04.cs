using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Reflection;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Log;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public partial class JobService
    {

        public void CFShortCutRecipeIDCheck(string trxid, Line line, Equipment eqp, Job job)
        {
            try
            {
                bool NGResult = false;
                string ppid = string.Empty;
                string mesppid = string.Empty;
                string errMsg = string.Empty;
                IList<Job> jobs = new List<Job>();
                IList<RecipeCheckInfo> nextIDCheckInfos = new List<RecipeCheckInfo>();
                IDictionary<string, IList<RecipeCheckInfo>> recipeIDCheckData = new Dictionary<string, IList<RecipeCheckInfo>>();

                switch (line.Data.LINETYPE)
                {
                    case eLineType.CF.FCMPH_TYPE1:
                    case eLineType.CF.FCRPH_TYPE1:
                    case eLineType.CF.FCGPH_TYPE1:
                    case eLineType.CF.FCBPH_TYPE1:

                        if (line.File.CFShortCutMode == eShortCutMode.Enable && eqp.Data.NODEATTRIBUTE == "CV01")
                        {

                            // 確認 LotList 是否有資料
                            if (job.MesCstBody.LOTLIST.Count == 0)
                            {
                                job.CfSpecial.CFShortCutrecipeParameterRequestResult = eRecipeCheckResult.NG;
                                ObjectManager.JobManager.EnqueueSave(job);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> NextBCS][{1}] Short Cut Recipea ID Request Result [Job Data imperfect]", eqp.Data.NODENO, trxid));
                                return;
                            }

                            // 取出原始 Port 資訊
                            Port port = ObjectManager.PortManager.GetPort(job.MesCstBody.PORTNAME);

                            // 取出 ProcessLineList 的內容
                            PROCESSLINEc nextLineInfos = job.MesCstBody.LOTLIST[0].PROCESSLINELIST.FirstOrDefault(l => l.LINENAME == line.Data.NEXTLINEID);
                            if (nextLineInfos == null)
                            {
                                job.CfSpecial.CFShortCutrecipeParameterRequestResult = eRecipeCheckResult.NG;
                                ObjectManager.JobManager.EnqueueSave(job);
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> NextBCS][{1}] Short Cut Recipea ID Request Result [Next Line Information imperfect]", eqp.Data.NODENO, trxid));
                                return;
                            }

                            // 取出下條 Line 的 PPID 資訊
                            string nextLineName = nextLineInfos.LINENAME;
                            string nextLineRecipeName = nextLineInfos.LINERECIPENAME;

                            if (!AnalysisMesPPID_CFShortCut(port, nextLineInfos, ref nextIDCheckInfos, out ppid, out mesppid, out errMsg))
                            {
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS -> NextBCS][{1}] Short Cut Recipea ID Register Error : [{2}]", eqp.Data.NODENO, trxid, errMsg));
                                return;
                            }

                            if (nextIDCheckInfos.Count() > 0)
                            {
                                if (recipeIDCheckData.ContainsKey(line.Data.NEXTLINEID))
                                {
                                    List<RecipeCheckInfo> rci = recipeIDCheckData[line.Data.NEXTLINEID] as List<RecipeCheckInfo>;
                                    for (int i = 0; i < nextIDCheckInfos.Count(); i++)
                                    {
                                        // 過濾掉重覆的部份
                                        if (rci.Any(r => r.EQPNo == nextIDCheckInfos[i].EQPNo && r.RecipeID == nextIDCheckInfos[i].RecipeID)) continue;
                                        rci.AddRange(nextIDCheckInfos);
                                    }
                                }
                                else
                                {
                                    recipeIDCheckData.Add(line.Data.NEXTLINEID, nextIDCheckInfos);
                                }
                            }

                            // Check Recipe ID 
                            if (recipeIDCheckData.Count() > 0)
                            {
                                Invoke(eServiceName.RecipeService, "RecipeRegisterValidationCommand", new object[] { trxid, recipeIDCheckData, new List<string>() });
                                //檢查完畢, 返回值
                                foreach (string key in recipeIDCheckData.Keys)
                                {
                                    IList<RecipeCheckInfo> recipeIDList = recipeIDCheckData[key];
                                    string log = string.Format("Line Name=[{0}] Recipe ID Check", key);
                                    string log2 = string.Empty;

                                    for (int i = 0; i < recipeIDList.Count; i++)
                                    {
                                        if (recipeIDList[i].Result == eRecipeCheckResult.NG ||
                                            recipeIDList[i].Result == eRecipeCheckResult.TIMEOUT)
                                        {
                                            NGResult = true;
                                        }
                                    }
                                }

                                jobs = ObjectManager.JobManager.GetJobs(job.CassetteSequenceNo).Where(u => u.CfSpecial.CFShortCutRecipeParameterCheckFlag == false).ToList<Job>();
                                if (jobs.Count != 0)
                                {
                                    foreach (Job _job in jobs)
                                    {
                                        lock (_job) _job.CfSpecial.CFShortCutRecipeIDCheckFlag = true;
                                        if (!NGResult)
                                            lock (_job) _job.CfSpecial.CfShortCutRecipeIDCheckResult = eRecipeCheckResult.OK;
                                        else
                                            lock (_job) _job.CfSpecial.CfShortCutRecipeIDCheckResult = eRecipeCheckResult.NG;
                                        ObjectManager.JobManager.EnqueueSave(_job);
                                    }
                                }

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [BCS <- NextBCS][{1}] Recipea ID Register Result [{2}]", eqp.Data.NODENO, trxid, job.CfSpecial.CfShortCutRecipeIDCheckResult));
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CFShortCutRecipeParameterCheck(string trxid, Line line, Equipment eqp, Job job)
        {
            IList<RecipeCheckInfo> nextParaCheckInfos = new List<RecipeCheckInfo>();
            IDictionary<string, IList<RecipeCheckInfo>> recipeParaCheckData = new Dictionary<string, IList<RecipeCheckInfo>>();
            IList<Job> jobs = new List<Job>();
            bool _check = false;

            try
            {
                switch (line.Data.LINETYPE)
                {
                    case eLineType.CF.FCMPH_TYPE1:
                    case eLineType.CF.FCRPH_TYPE1:
                    case eLineType.CF.FCGPH_TYPE1:
                    case eLineType.CF.FCBPH_TYPE1:
                        if (line.File.CFShortCutMode == eShortCutMode.Enable && eqp.Data.NODEATTRIBUTE == "OVEN")
                            _check = true;
                        break;
                }
                if (_check)
                {
                    // 1. 取出 No Check EQName 組成 List - MES 沒有在　PROCESSLINELIST　中開 RECIPEPARANOCHECKLIST 故無法取得，預設全檢。
                    IList<string> NoCheckList = new List<string>();

                    // 確認 LotList 是否有資料
                    if (job.MesCstBody.LOTLIST.Count == 0)
                    {
                        job.CfSpecial.CFShortCutrecipeParameterRequestResult = eRecipeCheckResult.NG;
                        ObjectManager.JobManager.EnqueueSave(job);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> NextBCS][{1}] Short Cut Recipea Parameter Request Result [Job Data imperfect]", eqp.Data.NODENO, trxid));
                        return;
                    }

                    // 取出 ProcessLineList 的內容
                    PROCESSLINEc nextLineInfos = job.MesCstBody.LOTLIST[0].PROCESSLINELIST.FirstOrDefault(l => l.LINENAME == line.Data.NEXTLINEID);
                    if (nextLineInfos == null)
                    {
                        job.CfSpecial.CFShortCutrecipeParameterRequestResult = eRecipeCheckResult.NG;
                        ObjectManager.JobManager.EnqueueSave(job);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> NextBCS][{1}] Short Cut Recipea Parameter Request Result [Next Line Information imperfect]", eqp.Data.NODENO, trxid));
                        return;
                    }

                    //1.若MES 要求需要check, 但是BC 端设置为不check, 则无需check.
                    //2.若MES 不要求check,但是BC 要求check, 则无需check.
                    if (nextLineInfos.RECIPEPARAVALIDATIONFLAG == "Y" && line.Data.CHECKCROSSRECIPE == "Y")
                    {
                        string ppid = string.Empty;
                        string mesppid = string.Empty;
                        string err = string.Empty;
                        // 取出原始 Port 資訊
                        Port port = ObjectManager.PortManager.GetPort(job.MesCstBody.PORTNAME);

                        // 取出下條 Line 的 PPID 資訊
                        string nextLineName = nextLineInfos.LINENAME;
                        string nextLineRecipeName = nextLineInfos.LINERECIPENAME;

                        // 解析 MES PPID
                        AnalysisMesParameter_CFShortCut(port, nextLineInfos, ref nextParaCheckInfos, out ppid, out mesppid, out err);

                        if (recipeParaCheckData.ContainsKey(nextLineName))
                        {
                            List<RecipeCheckInfo> rci = recipeParaCheckData[nextLineName] as List<RecipeCheckInfo>;
                            rci.AddRange(nextParaCheckInfos);
                        }
                        else
                        {
                            recipeParaCheckData.Add(nextLineName, nextParaCheckInfos);
                        }

                        #region Check Parameter
                        if (recipeParaCheckData.Count > 0)
                        {
                            object[] obj = new object[]
                                    {
                                        trxid,
                                        line.Data.NEXTLINEID,
                                        nextParaCheckInfos,
                                        NoCheckList,
                                        port.Data.PORTID,      //PORTID
                                        "",                    //Cassette ID 不要上報給 MES ,否則MES 會無法區分是不是ShortCut 報上去的.
                                    };
                            eRecipeCheckResult retCode = (eRecipeCheckResult)Invoke(eServiceName.RecipeService, "CF_RecipeParameterRequestCommandForBCS", obj);

                            //檢查完畢, 返回值
                            jobs = ObjectManager.JobManager.GetJobs(job.CassetteSequenceNo).Where(u => u.CfSpecial.CFShortCutRecipeParameterCheckFlag == false).ToList<Job>();
                            if (jobs.Count != 0)
                            {
                                foreach (Job _job in jobs)
                                {
                                    lock (_job) _job.CfSpecial.CFShortCutRecipeParameterCheckFlag = true;
                                    lock (_job) _job.CfSpecial.CFShortCutrecipeParameterRequestResult = retCode;
                                    ObjectManager.JobManager.EnqueueSave(_job);
                                }
                            }
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS <- NextBCS][{1}] Recipea Parameter Request Result [{2}]", eqp.Data.NODENO, trxid, retCode));
                        }
                        #endregion
                    }
                    else
                    {
                        jobs = ObjectManager.JobManager.GetJobs(job.CassetteSequenceNo).Where(u => u.CfSpecial.CFShortCutRecipeParameterCheckFlag == false).ToList<Job>();
                        if (jobs.Count != 0)
                        {
                            foreach (Job _job in jobs)
                            {
                                lock (_job) _job.CfSpecial.CFShortCutRecipeParameterCheckFlag = true;
                                lock (_job) _job.CfSpecial.CFShortCutrecipeParameterRequestResult = eRecipeCheckResult.OK;
                                ObjectManager.JobManager.EnqueueSave(_job);
                            }
                        }
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- NextBCS][{1}] Recipea Parameter Request Result [{2}]", eqp.Data.NODENO, trxid, "OK"));
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 分析MES PPID
        /// </summary>
        public bool AnalysisMesParameter_CFShortCut(Port port, PROCESSLINEc nextLineInfos, ref IList<RecipeCheckInfo> paraRecipeInfos, out string ppid, out string mesppid, out string errMsg)
        {
            mesppid = string.Empty;
            errMsg = string.Empty;
            ppid = string.Empty;
            try
            {

                string eqPPID = string.Empty;
                eqPPID = nextLineInfos.PPID;
                mesppid = eqPPID; //Keep MES Donwload PPID 不會有虛擬機台、跳號，隨時還要上報，不得更動

                //Watson Add 避免沒有要check的機台還加入Recipe Check，要下給機台的
                string eqFillnullPPID = string.Empty;
                string photo_EQP_PPID = string.Empty;
                Dictionary<string, string> EQP_RecipeNo = ObjectManager.JobManager.AddVirtualEQP_PPID_CFShortCut(nextLineInfos.LINENAME, eqPPID, out ppid);

                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1} is Error!!", nextLineInfos.LINENAME, eqPPID);
                    return false;
                }

                string productRcpName = nextLineInfos.LINERECIPENAME;
                       
                string eqpNo = string.Empty;
                foreach (string eqpno in EQP_RecipeNo.Keys)
                {
                    eqpNo = eqpno;
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        //ex: FBRPH Line 沒有機台L10, L15 and L20 不需要加入Recipe Check(不需丟給機台做Recipe Validate)
                        NLogManager.Logger.LogWarnWrite(null, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("CF Photo Line PPID Type is Special  Spec!! EQP No =[{0}] is not in DB, but must fill '00' in EQP PPID", eqpNo));
                        continue; //跨號機台不跳出!!
                    }

                    if (!paraRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpno])))
                    {
                        paraRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno]/*recipe*/, productRcpName));
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
        /// 分析MES PPID
        /// </summary>
        public bool AnalysisMesPPID_CFShortCut(Port port, PROCESSLINEc nextLineInfos, ref IList<RecipeCheckInfo> idRecipeInfos, out string ppid, out string mesppid, out string errMsg)
        {
            mesppid = string.Empty;
            errMsg = string.Empty;
            ppid = string.Empty;
            try
            {

                string eqPPID = string.Empty;
                eqPPID = nextLineInfos.PPID;
                mesppid = eqPPID; //Keep MES Donwload PPID 不會有虛擬機台、跳號，隨時還要上報，不得更動

                //Watson Add 避免沒有要check的機台還加入Recipe Check，要下給機台的
                string eqFillnullPPID = string.Empty;
                string photo_EQP_PPID = string.Empty;
                Dictionary<string, string> EQP_RecipeNo = ObjectManager.JobManager.AddVirtualEQP_PPID_CFShortCut(nextLineInfos.LINENAME, eqPPID, out eqFillnullPPID);

                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1} is Error!!", nextLineInfos.LINENAME, eqPPID);
                    return false;
                }

                string productRcpName = nextLineInfos.LINERECIPENAME;

                string eqpNo = string.Empty;
                foreach (string eqpno in EQP_RecipeNo.Keys)
                {
                    eqpNo = eqpno;
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        //ex: FBRPH Line 沒有機台L10, L15 and L20 不需要加入Recipe Check(不需丟給機台做Recipe Validate)
                        Logger.LogWarnWrite(null, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("CF Photo Line PPID Type is Special  Spec!! EQP No =[{0}] is not in DB, but must fill '00' in EQP PPID", eqpNo));
                        continue; //跨號機台不跳出!!
                    }

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpno])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno]/*recipe*/, productRcpName));
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

    }
}
