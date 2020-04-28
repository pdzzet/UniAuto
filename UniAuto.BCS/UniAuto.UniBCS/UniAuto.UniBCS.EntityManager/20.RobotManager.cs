using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;
using System.IO;
using System.Data;
using UniAuto.UniBCS.Log;
using System.Reflection;
using System.Collections;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.EntityManager
{
    public class RobotManager : EntityManager, IDataSource
    {
        /// <summary> Robot Entity 記錄所有Robot, Key=RobotName
        ///
        /// </summary>
        private Dictionary<string, Robot> _entities = new Dictionary<string, Robot>();

        /// <summary> Route Mst Entity .紀錄總共有多少Routes, Key=RobotName
        ///
        /// </summary>
        private Dictionary<string, Dictionary<string, RobotRoute>> _routes = new Dictionary<string, Dictionary<string, RobotRoute>>();

        /// <summary> Route Condition .紀錄總共有多少RouteCondition, Key=RobotName
        ///
        /// </summary>
        private Dictionary<string, Dictionary<string, List<RobotRouteCondition>>> _conditions = new Dictionary<string, Dictionary<string, List<RobotRouteCondition>>>();

        /// <summary> Route Steps .紀錄總共有多少RouteSteps設定, Key=RobotName , Val=各Route的StepList
        ///
        /// </summary>
        private Dictionary<string,Dictionary<string, List<RobotRouteStep>>> _steps= new Dictionary<string,Dictionary<string,List<RobotRouteStep>>>();

        /// <summary> Route Rule Filter .紀錄總共有多少Filter設定, Key=RobotName , Val=各RouteID的所有StepList的FilterList
        /// 
        /// </summary>
        private Dictionary<string,Dictionary<string, Dictionary<int, List<RobotRuleFilter>>>> _filters=new Dictionary<string,Dictionary<string,Dictionary<int,List<RobotRuleFilter>>>>();

        /// <summary> Robot Rule Job Select .紀錄總共有多少Select設定, Key=RobotName , Val=各Robot的所有SelectList
        ///
        /// </summary>
        private Dictionary<string,Dictionary<string, List<RobotRuleSelect>>> _selects=new Dictionary<string,Dictionary<string,List<RobotRuleSelect>>>();

        /// <summary> Route Rule OrderBy .紀錄總共有多少OrderBy設定, Key=RobotName , Val=各RouteID的所有StepList的OrderByList
        ///
        /// </summary>
        private Dictionary<string,Dictionary<string, Dictionary<int, List<RobotRuleOrderby>>>> _orderbys=new Dictionary<string,Dictionary<string,Dictionary<int,List<RobotRuleOrderby>>>>();

        /// <summary> Route Process Result Hanlde List .紀錄總共有多少ProcessResult設定, Key=RobotName , Val=各RouteID的所有StepList的ResultByList
        /// 
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<int, List<RobotRouteResultHandle>>>> _resultHandles = new Dictionary<string, Dictionary<string, Dictionary<int, List<RobotRouteResultHandle>>>>();

        //20150929 add StageSelect Enitiy
        /// <summary> Route Rule Stage Select.紀錄總共有多少Stage Select設定, Key=RobotName , Val=各RouteID的所有StepList的Stage Select
        /// 
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<int, List<RobotRuleStageSelect>>>> _stageSelects = new Dictionary<string, Dictionary<string, Dictionary<int, List<RobotRuleStageSelect>>>>();

        //20151007 add Rule RouteStepByPass Entity
        /// <summary> Route Rule RouteStepByPass .紀錄總共有多少RouteStepByPass設定, Key=RobotName , Val=各RouteID的所有StepList的RouteStepByPassList
        /// 
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<int, List<RobotRuleRouteStepByPass>>>> _routeStepByPasss = new Dictionary<string, Dictionary<string, Dictionary<int, List<RobotRuleRouteStepByPass>>>>();

        //20151007 add Rule RouteStepJump Entity
        /// <summary> Route Rule RouteStepJump .紀錄總共有多少RouteStepJump設定, Key=RobotName , Val=各RouteID的所有StepList的RouteStepJumpList
        /// 
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<int, List<RobotRuleRouteStepJump>>>> _routeStepJumps = new Dictionary<string, Dictionary<string, Dictionary<int, List<RobotRuleRouteStepJump>>>>();

        private List<EntityData> _tempEntityDatas = null;

        #region 繼承EntityManager

        public override EntityManager.FILE_TYPE GetFileType()
        {
            return FILE_TYPE.BIN;
        }

        protected override string GetSelectHQL()
        {
            return string.Format("from RobotEntityData where SERVERNAME = '{0}'", BcServerName);
        }

        protected override Type GetTypeOfEntityData()
        {
            return typeof(RobotEntityData);
        }

        //產生序列化文件
        protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            Filenames = new List<string>();
            _tempEntityDatas = EntityDatas;

            foreach (EntityData entity_data in EntityDatas)
            {
                RobotEntityData robot_entity_data = entity_data as RobotEntityData;
                if (robot_entity_data != null)
                {
                    //string file_name = string.Format("{0}.bin", robot_entity_data.ROBOTNAME);
                    string file_name = string.Format("{0}.{1}", robot_entity_data.ROBOTNAME,GetFileExtension());
                    Filenames.Add(file_name);
                }
            }

        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(RobotEntityFile);
        }

        //確認檔案是否存在
        protected override EntityFile NewEntityFile(string Filename)
        {
            //return new RobotEntityFile();

            string robot_id = Filename.Substring(0, Filename.Length - ".bin".Length);
            bool armDoubleSubstrateFlag = false;
            int curRobotArmCount = 1;
            int maxArmJobCount = 1;

            foreach (RobotEntityData data in _tempEntityDatas)
            {

                if (data.ROBOTNAME == robot_id)
                {
                    //Get Robot Arm Count
                    if (data.ROBOTARMQTY > 0)
                    {
                        curRobotArmCount = data.ROBOTARMQTY;
                    }

                    //Get Robot Arm Job Count
                    if (data.ARMJOBQTY > 0)
                    {
                        maxArmJobCount = data.ARMJOBQTY;
                    }

                    //Check Use Double Substrate Flag
                    if (maxArmJobCount > 1)
                    {
                        armDoubleSubstrateFlag = true;

                    }
                    else
                    {
                        armDoubleSubstrateFlag = false;
                    }
                        
                    break;
                }

            }

            return new RobotEntityFile(curRobotArmCount, armDoubleSubstrateFlag);
            
            ////找不到檔案就New 新File
            //if (findfile == false)
            //{
            //    return new RobotEntityFile(curRobotArmCount,armDoubleSubstrateFlag);
            //}
            //else
            //{
            //    return new RobotEntityFile(curRobotArmCount, armDoubleSubstrateFlag);
            //}

        }

        protected override void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles)
        {

            foreach (EntityData entity_data in entityDatas)
            {
                RobotEntityData robot_entity_data = entity_data as RobotEntityData;

                if (robot_entity_data != null)
                {
                    foreach (EntityFile entity_file in entityFiles)
                    {
                        RobotEntityFile robot_entity_file = entity_file as RobotEntityFile;
                        if (robot_entity_file != null)
                        {
                            string fextname = robot_entity_file.GetFilename();
                            string fname = Path.GetFileNameWithoutExtension(fextname);
                            if (string.Compare(robot_entity_data.ROBOTNAME, fname, true) == 0)
                                _entities.Add(robot_entity_data.ROBOTNAME, new Robot(robot_entity_data, robot_entity_file));
                        }
                    }
                }
            }
            // Load Robot Other Table 20150610 tom
            foreach (Robot robot in _entities.Values)
            {
                Dictionary<string,RobotRoute> routes = LoadRobotRoute(robot);
                if (routes != null)
                    _routes.Add(robot.Data.ROBOTNAME, routes);
                Dictionary<string, List<RobotRouteCondition>> conditions = LoadRobotRouteCondition(robot);
                if (conditions != null)
                    _conditions.Add(robot.Data.ROBOTNAME, conditions);

                Dictionary<string, List<RobotRuleSelect>> selects = LoadRobotRuleSelect(robot);
                if (selects != null)
                    _selects.Add(robot.Data.ROBOTNAME, selects);

                Dictionary<string, Dictionary<int, List<RobotRuleFilter>>> filters = LoadRobotRuleFilter(robot);
                if (filters != null)
                    _filters.Add(robot.Data.ROBOTNAME, filters);
                Dictionary<string, List<RobotRouteStep>> steps = LoadRobotRouteStep(robot);
                if (steps != null)
                    _steps.Add(robot.Data.ROBOTNAME, steps);
                Dictionary<string, Dictionary<int, List<RobotRuleOrderby>>> orderbys=LoadRobotRuleOrderby(robot);
                if (orderbys != null)
                    _orderbys.Add(robot.Data.ROBOTNAME, orderbys);

                Dictionary<string, Dictionary<int, List<RobotRouteResultHandle>>> resultHandles = LoadRobotRouteResultHandle(robot);
                if (resultHandles != null)
                    _resultHandles.Add(robot.Data.ROBOTNAME, resultHandles);

                //20150929 add Stage Select
                Dictionary<string, Dictionary<int, List<RobotRuleStageSelect>>> stageSelects = LoadRobotRuleStageSelect(robot);
                if (stageSelects != null)
                    _stageSelects.Add(robot.Data.ROBOTNAME, stageSelects);
                
                //20151008 add RouteStep ByPass and Jump
                Dictionary<string, Dictionary<int, List<RobotRuleRouteStepByPass>>> routeStepByPasss = LoadRobotRuleRouteStepByPass(robot);
                if (routeStepByPasss != null)
                    _routeStepByPasss.Add(robot.Data.ROBOTNAME, routeStepByPasss);

                Dictionary<string, Dictionary<int, List<RobotRuleRouteStepJump>>> routeStepJumps = LoadRobotRuleRouteStepJump(robot);
                if (routeStepJumps != null)
                    _routeStepJumps.Add(robot.Data.ROBOTNAME, routeStepJumps);

            }
        }

        #endregion

        //Load/Reload Robot Route Master Info ================================================================================================================================

        /// <summary> load robot Route Master Table Setting
        /// Tom 
        /// 20150609
        ///
        /// </summary>
        /// <param name="robot">Robot Entity</param>
        /// <returns>RobotRoute Collection</returns>
        public Dictionary<string,RobotRoute> LoadRobotRoute(Robot robot) {
            //20151028 add for 要按照Route Priority排序
            string hql = string.Format("from RobotRouteMstEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and ISENABLED='Y' order by ROUTEPRIORITY desc", robot.Data.SERVERNAME, robot.Data.ROBOTNAME);
            IList list=HibernateAdapter.GetObjectByQuery(hql);
            Dictionary<string,RobotRoute> routes=new Dictionary<string,RobotRoute>();
            if (list != null)
            {

                foreach (RobotRouteMstEntityData item in list)
                {
                    if (routes.ContainsKey(item.ROUTEID))
                    {
                        routes.Remove(item.ROUTEID);
                    }
                    routes.Add(item.ROUTEID, new RobotRoute(item));
                }
                return routes;
            }

            return null;

        }

        /// <summary> load robot Route Master Table Setting
        /// 
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="routeID"></param>
        /// <returns></returns>
        public RobotRoute LoadRobotRoute(Robot robot, string routeID) 
        {
            //20151028 add for 要按照Route Priority排序
            string hql = string.Format("from RobotRouteMstEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and ROUTEID='{2}' and ISENABLED='Y' order by ROUTEPRIORITY desc",
                                        robot.Data.SERVERNAME, robot.Data.ROBOTNAME,routeID);
            IList list = HibernateAdapter.GetObjectByQuery(hql);

            if (list != null) {
                RobotRouteMstEntityData data = list[0] as RobotRouteMstEntityData;
                RobotRoute route = new RobotRoute(data);
                return route;
            }

            return null;
        }

        /// <summary> Reload robot Route Master Table Setting
        /// 
        /// </summary>
        /// <param name="robotName"></param>
        public void ReloaRobotRoute(string robotName) {
            Robot robot = GetRobotByRobotName(robotName);
            if (robot != null) {
                Dictionary<string, RobotRoute> routes = LoadRobotRoute(robot);
                if (routes != null) {
                    lock (_routes) {
                        _routes[robotName] = routes;
                    }
                }
            }
        }

        /// <summary> Reload robot Route Master Table Setting
        /// 
        /// </summary>
        /// <param name="robotName"></param>
        /// <param name="routeId"></param>
        public void ReloadRobotRoute(string robotName, string routeId) {
            Robot robot = GetRobotByRobotName(robotName);

            RobotRoute route = LoadRobotRoute(robot, routeId);
            if (route != null) {
                lock (_routes) {
                    _routes[robotName][routeId] = route;
                }
            }
        }

        /// <summary> Get Robot Route
        ///
        /// </summary>
        /// <param name="robotName">Robot Name </param>
        /// <param name="routeID">Robot ID</param>
        /// <returns>RobotRoute object or Null</returns>
        public RobotRoute GetRoute(string robotName, string routeID) {
            if (_routes.ContainsKey(robotName)) {
                if (_routes[robotName].ContainsKey(routeID)) {
                    return _routes[robotName][routeID];
                }
            }
            return null;
        }

        public void ReloadAllRobotRouteMaster()
        {
            foreach (string robotName in _entities.Keys)
            {
                ReloaRobotRoute(robotName);
            }

        }

        //Load/Reload Robot Route Condition Info ================================================================================================================================

        /// <summary> Load Robot Route Condition
        /// Tom
        /// 20150609
        ///
        /// </summary>
        /// <param name="robot"> Robot </param>
        /// <returns>Robot Route Condition Collection</returns>
        public Dictionary<string, List<RobotRouteCondition>> LoadRobotRouteCondition(Robot robot) {
            
            //20151028 add for 要按照Route Priority排序
            string hql = string.Format("from RobotRouteConditionEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and ISENABLED='Y' order by ROUTEPRIORITY desc , CONDITIONSEQ desc", robot.Data.SERVERNAME, robot.Data.ROBOTNAME);
            IList list= HibernateAdapter.GetObjectByQuery(hql);          

            Dictionary<string, List<RobotRouteCondition>> routeConditions = new Dictionary<string, List<RobotRouteCondition>>();
            if (list != null)
            {
                foreach (RobotRouteConditionEntityData item in list)
                {
                    if (routeConditions.ContainsKey(item.ROUTEID))
                    {
                        routeConditions[item.ROUTEID].Add(new RobotRouteCondition(item));
                    }
                    else
                    {
                        List<RobotRouteCondition> items = new List<RobotRouteCondition>();
                        items.Add(new RobotRouteCondition(item));
                        routeConditions.Add(item.ROUTEID, items);
                    }
                }

                return routeConditions;
            }
            return null;
        }

        /// <summary> Load Robot Route Condition by route ID
        ///
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="routeID"></param>
        /// <returns>Robot Route Condition List</returns>
        public List<RobotRouteCondition> LoadRobotRouteCondition(Robot robot, string routeID) {

            //20151028 add for 要按照Route Priority排序
            string hql = string.Format("from RobotRouteConditionEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and ROUTEID='{2}' and ISENABLED='Y' order by ROUTEPRIORITY desc , CONDITIONSEQ desc", robot.Data.SERVERNAME, robot.Data.ROBOTNAME, routeID);
            IList list = HibernateAdapter.GetObjectByQuery(hql);
            List<RobotRouteCondition> conditions ;
            if (list != null) {
                conditions = new List<RobotRouteCondition>();
                foreach (RobotRouteConditionEntityData data in list) {
                    conditions.Add(new RobotRouteCondition(data));
                }
                return conditions;
            }
            return null;
        }

        /// <summary> Reload Robot Route Condition by Robot Name 
        ///
        /// </summary>
        /// <param name="robotName"></param>
        public void ReloadRobotRouteConditionByRobotName(string robotName) {

            Robot robot = GetRobotByRobotName(robotName);
            if (robot != null) {
                Dictionary<string, List<RobotRouteCondition>> conditions = LoadRobotRouteCondition(robot);
                if (conditions != null) {
                    lock (_conditions) {
                        _conditions[robotName] = conditions;
                    }
                }
            }
        }

        /// <summary> Reload Server All Robot Route Conditions
        ///
        /// </summary>
        public void ReloadRobotRouteCondition()
        {
            foreach (string robotName in _entities.Keys)
            {
                ReloadRobotRouteConditionByRobotName(robotName);
            }

        }

        /// <summary> reload Route Condition by Robot name and route ID
        ///
        /// </summary>
        /// <param name="robotName"></param>
        /// <param name="routeID"></param>
        public void ReloadRouteRouteCondition(string robotName, string routeID) {
            Robot robot = GetRobotByRobotName(robotName);
            if (robot != null) {
                List<RobotRouteCondition> contidions = LoadRobotRouteCondition(robot, routeID);
                if (contidions != null) {
                    lock (_conditions) {
                        _conditions[robotName][routeID] = contidions;
                    }
                }
            }
        }

        /// <summary> Get Route Condition by robot name  and route id
        ///
        /// </summary>
        /// <param name="robotName">robot name</param>
        /// <param name="routeID">route id</param>
        /// <returns>robot coute codition collection or null</returns>
        public List<RobotRouteCondition> GetRouteCondition(string robotName, string routeID) {
            if (_conditions.ContainsKey(robotName)) {
                if (_conditions[robotName].ContainsKey(routeID)) {
                    return _conditions[robotName][routeID];
                }
            }
            return null;
        }

        /// <summary> Get Route Conditions by robot name
        ///
        /// </summary>
        /// <param name="robotName"></param>
        /// <returns></returns>
        public Dictionary<string, List<RobotRouteCondition>> GetRouteConditionsByRobotName(string robotName)
        {

            if (_conditions.ContainsKey(robotName))
            {
                 return _conditions[robotName];
            }
            return null;

        }

        /// <summary> Load Robot Route Step
        /// Tom
        /// 20150609
        ///
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public Dictionary<string, List<RobotRouteStep>> LoadRobotRouteStep(Robot robot) {

            string hql = string.Format("from RobotRouteStepEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}'", robot.Data.SERVERNAME, robot.Data.ROBOTNAME);
            IList list = HibernateAdapter.GetObjectByQuery(hql);
            Dictionary<string, List<RobotRouteStep>> routeSteps= new Dictionary<string, List<RobotRouteStep>>();
            if (list != null)
            {
                foreach (RobotRouteStepEntityData item in list)
                {
                    if (routeSteps.ContainsKey(item.ROUTEID))
                    {
                        routeSteps[item.ROUTEID].Add(new RobotRouteStep(item));
                    }
                    else
                    {
                        List<RobotRouteStep> items = new List<RobotRouteStep>();
                        items.Add(new RobotRouteStep(item));
                        routeSteps.Add(item.ROUTEID, items);
                    }
                }

                return routeSteps;
            }
            return null;
        }

        /// <summary> Load Robot Route Step
        /// 
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="routeID"></param>
        /// <returns></returns>
        public List<RobotRouteStep> LoadRobotRouteStep(Robot robot, string routeID) {
            string hql = string.Format("from RobotRouteStepEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and ROUTEID='{2}'", robot.Data.SERVERNAME, robot.Data.ROBOTNAME, routeID);
            IList list = HibernateAdapter.GetObjectByQuery(hql);
            List<RobotRouteStep> steps = null;
            if (list != null) {
                steps=new List<RobotRouteStep>();
                foreach (RobotRouteStepEntityData data in list) {
                    steps.Add(new RobotRouteStep(data));
                }
                return steps;
            }
            return null;
        }

        /// <summary> Reload Robot Route Step
        /// 
        /// </summary>
        /// <param name="robotName"></param>
        public void ReloadRobotRouteStep(string robotName) {
            Robot robot = GetRobotByRobotName(robotName);
            if (robot != null) {
                Dictionary<string,List<RobotRouteStep>> steps=LoadRobotRouteStep(robot);
                if(steps!=null){
                lock(_steps){
                    _steps[robotName] = steps;
                    }
                }
            }
        }

        /// <summary> Reload Robot Route Step
        /// 
        /// </summary>
        /// <param name="robotName"></param>
        /// <param name="routeID"></param>
        public void ReloadRobotRouteStep(string robotName, string routeID) {
            Robot robot = GetRobotByRobotName(robotName);
            if (robot != null) {
                List<RobotRouteStep> steps = LoadRobotRouteStep(robot, routeID);
                if (steps != null) {
                    lock (_steps) {
                        _steps[robotName][routeID] = steps;
                    }
                }
            }
        }

        /// <summary> Reload Server All Robot Route Step
        ///
        /// </summary>
        public void ReloadALLRobotRouteStep()
        {
            foreach (string robotName in _entities.Keys)
            {
                ReloadRobotRouteStep(robotName);
            }

        }

        //public List<RobotRouteStep> GetRouteStep(string robotName, string routeID) {
        //    if (_steps.ContainsKey(robotName)) {
        //        if (_steps[robotName].ContainsKey(routeID)) {
        //            return _steps[robotName][routeID];
        //        }
        //    }
        //    return null;
        //}

        public SerializableDictionary<int, RobotRouteStep> GetRouteStepList(string robotName, string routeID)
        {
            SerializableDictionary<int, RobotRouteStep> stepLists = new SerializableDictionary<int, RobotRouteStep>();
            
            if (_steps.ContainsKey(robotName))
            {
                //找到對應Robot所有的RouteIDList
                if (_steps[robotName].ContainsKey(routeID))
                {
                    lock (_steps[robotName][routeID])
                    {
                        foreach (RobotRouteStep routeStep in _steps[robotName][routeID])
                        {
                            stepLists.Add(routeStep.Data.STEPID, routeStep.Clone() as RobotRouteStep);
                  
                        }
                    }

                }
            }
            return stepLists;

        }

        //public SerializableDictionary<int, RobotRouteStep> GetDailyCheckRouteStepList(string robotName, string routeID)
        //{
        //    SerializableDictionary<int, RobotRouteStep> stepLists = new SerializableDictionary<int, RobotRouteStep>();

        //    if (_steps.ContainsKey(robotName))
        //    {
        //        //找到對應Robot所有的RouteIDList
        //        if (_steps[robotName].ContainsKey(routeID))
        //        {
        //            lock (_steps[robotName][routeID])
        //            {
        //                foreach (RobotRouteStep routeStep in _steps[robotName][routeID])
        //                {
        //                    if (robotName.ToUpper().Contains("DAILY_CHECK"))
        //                        stepLists.Add(routeStep.Data.STEPID, routeStep.Clone() as RobotRouteStep);

        //                }
        //            }

        //        }
        //    }
        //    return stepLists;

        //}

        //Rule Stage Select Function List 20150929 add ========================================================================================================================

        /// <summary>Load Robot Rule Stage Select
        /// 
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<int, List<RobotRuleStageSelect>>> LoadRobotRuleStageSelect(Robot robot)
        {
            //Priority 1<2<3....所以要改成DESC
            string hql = string.Format("from RobotRouteRuleStageSelectEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' order by STEPID,ITEMSEQ DESC",
                                        robot.Data.SERVERNAME, robot.Data.ROBOTNAME);

            IList list = HibernateAdapter.GetObjectByQuery(hql);

            Dictionary<string, Dictionary<int, List<RobotRuleStageSelect>>> ruleStageSelects = new Dictionary<string, Dictionary<int, List<RobotRuleStageSelect>>>();
            
            if (list != null)
            {

                foreach (RobotRouteRuleStageSelectEntityData item in list)
                {
                    if (ruleStageSelects.ContainsKey(item.ROUTEID))
                    {
                        if (ruleStageSelects[item.ROUTEID].ContainsKey(item.STEPID))
                        {

                            ruleStageSelects[item.ROUTEID][item.STEPID].Add(new RobotRuleStageSelect(item));
                        
                        }
                        else
                        {

                            List<RobotRuleStageSelect> items = new List<RobotRuleStageSelect>();

                            items.Add(new RobotRuleStageSelect(item));
                            ruleStageSelects[item.ROUTEID].Add(item.STEPID, items);
                        }

                    }
                    else
                    {
                        Dictionary<int, List<RobotRuleStageSelect>> stageSelects = new Dictionary<int, List<RobotRuleStageSelect>>();
                        List<RobotRuleStageSelect> items = new List<RobotRuleStageSelect>();
                        items.Add(new RobotRuleStageSelect(item));
                        stageSelects.Add(item.STEPID, items);
                        ruleStageSelects.Add(item.ROUTEID, stageSelects);
                    }

                }

                return ruleStageSelects;
            }

            return null;
        }

        public Dictionary<int, List<RobotRuleStageSelect>> LoadRobotRuleStageSelect(Robot robot, string routeID)
        {
            //Priority 1<2<3....所以要改成DESC
            string hql = string.Format("from RobotRouteRuleStageSelectEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and ROUTEID='{2}' order by STEPID,ITEMSEQ DESC",
                                        robot.Data.SERVERNAME, robot.Data.ROBOTNAME, routeID);
            IList list = HibernateAdapter.GetObjectByQuery(hql);

            Dictionary<int, List<RobotRuleStageSelect>> stageSelects = null;

            if (list != null)
            {

                stageSelects = new Dictionary<int, List<RobotRuleStageSelect>>();

                foreach (RobotRouteRuleStageSelectEntityData data in list)
                {
                    if (stageSelects.ContainsKey(data.STEPID))
                    {
                        stageSelects[data.STEPID].Add(new RobotRuleStageSelect(data));
                    }
                    else
                    {
                        List<RobotRuleStageSelect> datas = new List<RobotRuleStageSelect>();
                        datas.Add(new RobotRuleStageSelect(data));
                        stageSelects.Add(data.STEPID, datas);
                    }
                }

                return stageSelects;
            }

            return null;

        }

        public void ReloadRobotRuleStageSelect(string robotName)
        {

            Robot robot = GetRobotByRobotName(robotName);

            if (robot != null)
            {

                Dictionary<string, Dictionary<int, List<RobotRuleStageSelect>>> stageSelects = LoadRobotRuleStageSelect(robot);

                if (stageSelects != null)
                {
                    lock (_stageSelects)
                    {
                        _stageSelects[robotName] = stageSelects;
                    }
                }
            }
        }

        public void ReloadRobotRuleStageSelect(string robotName, string routeID)
        {
            Robot robot = GetRobotByRobotName(robotName);

            if (robot != null)
            {
                Dictionary<int, List<RobotRuleStageSelect>> stageSelects = LoadRobotRuleStageSelect(robot, routeID);
                if (stageSelects != null)
                {
                    lock (_stageSelects)
                    {
                        _stageSelects[robotName][routeID] = stageSelects;
                    }
                }

            }

        }

        public List<RobotRuleStageSelect> GetRuleStageSelect(string robotName, string routeID, int stepID)
        {
            if (_stageSelects.ContainsKey(robotName))
            {
                if (_stageSelects[robotName].ContainsKey(routeID))
                {
                    if (_stageSelects[robotName][routeID].ContainsKey(stepID))
                    {
                        return _stageSelects[robotName][routeID][stepID];
                    }
                }
            }
            return null;
        }

        public Dictionary<int, List<RobotRuleStageSelect>> GetRuleStageSelect(string robotName, string routeID)
        {
            if (_stageSelects.ContainsKey(robotName))
            {
                if (_stageSelects[robotName].ContainsKey(routeID))
                {
                    return _stageSelects[robotName][routeID];
                }
            }
            return null;
        }

        public Dictionary<string, Dictionary<int, List<RobotRuleStageSelect>>> GetRuleStageSelect(string robotName)
        {
            if (_stageSelects.ContainsKey(robotName))
            {
                return _stageSelects[robotName];
            }
            return null;
        }

        public void ReloadAllRobotRuleStageSelect()
        {
            foreach (string robotName in _entities.Keys)
            {
                ReloadRobotRuleStageSelect(robotName);
            }

        }

        //Rule Route Step By Pass Function List ===========================================================================================================================================

        public Dictionary<string, Dictionary<int, List<RobotRuleRouteStepByPass>>> LoadRobotRuleRouteStepByPass(Robot robot)
        {
            //Priority 1<2<3....所以要改成DESC
            //GlobalAssemblyVersion v1.0.0.26-20151015, modified by dade, JUMPITEMSEQ->BYPASSITEMSEQ
            string hql = string.Format("from RobotRuleRouteStepByPassEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' order by STEPID,BYPASSITEMSEQ DESC",
                                        robot.Data.SERVERNAME, robot.Data.ROBOTNAME);

            IList list = HibernateAdapter.GetObjectByQuery(hql);
            Dictionary<string, Dictionary<int, List<RobotRuleRouteStepByPass>>> ruleRouteStepByPasss = new Dictionary<string, Dictionary<int, List<RobotRuleRouteStepByPass>>>();

            if (list != null)
            {
                foreach (RobotRuleRouteStepByPassEntityData item in list)
                {
                    if (ruleRouteStepByPasss.ContainsKey(item.ROUTEID))
                    {
                        if (ruleRouteStepByPasss[item.ROUTEID].ContainsKey(item.STEPID))
                        {
                            ruleRouteStepByPasss[item.ROUTEID][item.STEPID].Add(new RobotRuleRouteStepByPass(item));
                        }
                        else
                        {
                            List<RobotRuleRouteStepByPass> items = new List<RobotRuleRouteStepByPass>();
                            items.Add(new RobotRuleRouteStepByPass(item));
                            ruleRouteStepByPasss[item.ROUTEID].Add(item.STEPID, items);
                        }
                    }
                    else
                    {
                        Dictionary<int, List<RobotRuleRouteStepByPass>> routeStepByPasss = new Dictionary<int, List<RobotRuleRouteStepByPass>>();
                        List<RobotRuleRouteStepByPass> items = new List<RobotRuleRouteStepByPass>();
                        items.Add(new RobotRuleRouteStepByPass(item));
                        routeStepByPasss.Add(item.STEPID, items);
                        ruleRouteStepByPasss.Add(item.ROUTEID, routeStepByPasss);
                    }

                }

                return ruleRouteStepByPasss;
            }
            return null;


        }

        public Dictionary<int, List<RobotRuleRouteStepByPass>> LoadRobotRuleRouteStepByPass(Robot robot, string routeID)
        {
            //Priority 1<2<3....所以要改成DESC
            //GlobalAssemblyVersion v1.0.0.26-20151015, modified by dade, JUMPITEMSEQ->BYPASSITEMSEQ
            string hql = string.Format("from RobotRuleRouteStepByPassEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and ROUTEID='{2}' order by STEPID,BYPASSITEMSEQ DESC",
                                        robot.Data.SERVERNAME, robot.Data.ROBOTNAME, routeID);

            IList list = HibernateAdapter.GetObjectByQuery(hql);
            Dictionary<int, List<RobotRuleRouteStepByPass>> routeStepByPasss = null;
            if (list != null)
            {
                routeStepByPasss = new Dictionary<int, List<RobotRuleRouteStepByPass>>();
                foreach (RobotRuleRouteStepByPassEntityData data in list)
                {
                    if (routeStepByPasss.ContainsKey(data.STEPID))
                    {
                        routeStepByPasss[data.STEPID].Add(new RobotRuleRouteStepByPass(data));
                    }
                    else
                    {
                        List<RobotRuleRouteStepByPass> datas = new List<RobotRuleRouteStepByPass>();
                        datas.Add(new RobotRuleRouteStepByPass(data));
                        routeStepByPasss.Add(data.STEPID, datas);
                    }
                }
                return routeStepByPasss;
            }
            return null;
        }

        public void ReloadRobotRuleRouteStepByPass(string robotName)
        {
            Robot robot = GetRobotByRobotName(robotName);

            if (robot != null)
            {
                Dictionary<string, Dictionary<int, List<RobotRuleRouteStepByPass>>> routeStepByPasss = LoadRobotRuleRouteStepByPass(robot);
                if (routeStepByPasss != null)
                {
                    lock (_routeStepByPasss)
                    {
                        _routeStepByPasss[robotName] = routeStepByPasss;
                    }
                }
            }

        }

        public void ReloadRobotRuleRouteStepByPass(string robotName, string routeID)
        {

            Robot robot = GetRobotByRobotName(robotName);

            if (robot != null)
            {
                Dictionary<int, List<RobotRuleRouteStepByPass>> routeStepByPasss = LoadRobotRuleRouteStepByPass(robot, routeID);
                if (routeStepByPasss != null)
                {
                    lock (_routeStepByPasss)
                    {
                        _routeStepByPasss[robotName][routeID] = routeStepByPasss;
                    }
                }
            }

        }

        public List<RobotRuleRouteStepByPass> GetRuleRouteStepByPass(string robotName, string routeID, int stepID)
        {
            if (_routeStepByPasss.ContainsKey(robotName))
            {
                if (_routeStepByPasss[robotName].ContainsKey(routeID))
                {
                    if (_routeStepByPasss[robotName][routeID].ContainsKey(stepID))
                    {
                        return _routeStepByPasss[robotName][routeID][stepID];
                    }
                }
            }
            return null;
        }

        public Dictionary<int, List<RobotRuleRouteStepByPass>> GetRuleRouteStepByPass(string robotName, string routeID)
        {
            if (_routeStepByPasss.ContainsKey(robotName))
            {
                if (_routeStepByPasss[robotName].ContainsKey(routeID))
                {
                    return _routeStepByPasss[robotName][routeID];
                }
            }
            return null;
        }

        public Dictionary<string, Dictionary<int, List<RobotRuleRouteStepByPass>>> GetRuleRouteStepByPass(string robotName)
        {
            if (_routeStepByPasss.ContainsKey(robotName))
            {
                return _routeStepByPasss[robotName];
            }
            return null;
        }

        public void ReloadAllRobotRuleRouteStepByPass()
        {
            foreach (string robotName in _entities.Keys)
            {
                ReloadRobotRuleRouteStepByPass(robotName);
            }

        }

        //Rule Route Step Jump Function List ===========================================================================================================================================

        public Dictionary<string, Dictionary<int, List<RobotRuleRouteStepJump>>> LoadRobotRuleRouteStepJump(Robot robot)
        {
            //Priority 1<2<3....所以要改成DESC
            string hql = string.Format("from RobotRuleRouteStepJumpEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' order by STEPID,JUMPITEMSEQ DESC",
                                        robot.Data.SERVERNAME, robot.Data.ROBOTNAME);
            IList list = HibernateAdapter.GetObjectByQuery(hql);
            Dictionary<string, Dictionary<int, List<RobotRuleRouteStepJump>>> ruleRouteStepJumps = new Dictionary<string, Dictionary<int, List<RobotRuleRouteStepJump>>>();

            if (list != null)
            {
                foreach (RobotRuleRouteStepJumpEntityData item in list)
                {
                    if (ruleRouteStepJumps.ContainsKey(item.ROUTEID))
                    {
                        if (ruleRouteStepJumps[item.ROUTEID].ContainsKey(item.STEPID))
                        {
                            ruleRouteStepJumps[item.ROUTEID][item.STEPID].Add(new RobotRuleRouteStepJump(item));
                        }
                        else
                        {
                            List<RobotRuleRouteStepJump> items = new List<RobotRuleRouteStepJump>();
                            items.Add(new RobotRuleRouteStepJump(item));
                            ruleRouteStepJumps[item.ROUTEID].Add(item.STEPID, items);
                        }
                    }
                    else
                    {
                        Dictionary<int, List<RobotRuleRouteStepJump>> routeStepJumps = new Dictionary<int, List<RobotRuleRouteStepJump>>();
                        List<RobotRuleRouteStepJump> items = new List<RobotRuleRouteStepJump>();
                        items.Add(new RobotRuleRouteStepJump(item));
                        routeStepJumps.Add(item.STEPID, items);
                        ruleRouteStepJumps.Add(item.ROUTEID, routeStepJumps);
                    }

                }

                return ruleRouteStepJumps;
            }
            return null;
        }

        public Dictionary<int, List<RobotRuleRouteStepJump>> LoadRobotRuleRouteStepJump(Robot robot, string routeID)
        {
            //Priority 1<2<3....所以要改成DESC
            string hql = string.Format("from RobotRuleRouteStepJumpEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and ROUTEID='{2}' order by STEPID,JUMPITEMSEQ DESC",
                                        robot.Data.SERVERNAME, robot.Data.ROBOTNAME, routeID);

            IList list = HibernateAdapter.GetObjectByQuery(hql);
            Dictionary<int, List<RobotRuleRouteStepJump>> routeStepJumps = null;
            
            if (list != null)
            {
                routeStepJumps = new Dictionary<int, List<RobotRuleRouteStepJump>>();
                foreach (RobotRuleRouteStepJumpEntityData data in list)
                {
                    if (routeStepJumps.ContainsKey(data.STEPID))
                    {
                        routeStepJumps[data.STEPID].Add(new RobotRuleRouteStepJump(data));
                    }
                    else
                    {
                        List<RobotRuleRouteStepJump> datas = new List<RobotRuleRouteStepJump>();
                        datas.Add(new RobotRuleRouteStepJump(data));
                        routeStepJumps.Add(data.STEPID, datas);
                    }
                }
                return routeStepJumps;
            }
            return null;
        }

        public void ReloadRobotRuleRouteStepJump(string robotName)
        {
            Robot robot = GetRobotByRobotName(robotName);

            if (robot != null)
            {
                Dictionary<string, Dictionary<int, List<RobotRuleRouteStepJump>>> routeStepJumps = LoadRobotRuleRouteStepJump(robot);

                if (routeStepJumps != null)
                {
                    lock (_routeStepJumps)
                    {
                        _routeStepJumps[robotName] = routeStepJumps;
                    }
                }
            }

        }

        public void ReloadRobotRuleRouteStepJump(string robotName, string routeID)
        {

            Robot robot = GetRobotByRobotName(robotName);

            if (robot != null)
            {
                Dictionary<int, List<RobotRuleRouteStepJump>> routeStepJumps = LoadRobotRuleRouteStepJump(robot, routeID);
                if (routeStepJumps != null)
                {
                    lock (_routeStepJumps)
                    {
                        _routeStepJumps[robotName][routeID] = routeStepJumps;
                    }
                }
            }

        }

        public List<RobotRuleRouteStepJump> GetRuleRouteStepJump(string robotName, string routeID, int stepID)
        {
            if (_routeStepJumps.ContainsKey(robotName))
            {
                if (_routeStepJumps[robotName].ContainsKey(routeID))
                {
                    if (_routeStepJumps[robotName][routeID].ContainsKey(stepID))
                    {
                        return _routeStepJumps[robotName][routeID][stepID];
                    }
                }
            }
            return null;
        }

        public Dictionary<int, List<RobotRuleRouteStepJump>> GetRuleRouteStepJump(string robotName, string routeID)
        {
            if (_routeStepJumps.ContainsKey(robotName))
            {
                if (_routeStepJumps[robotName].ContainsKey(routeID))
                {
                    return _routeStepJumps[robotName][routeID];
                }
            }
            return null;
        }

        public Dictionary<string, Dictionary<int, List<RobotRuleRouteStepJump>>> GetRuleRouteStepJump(string robotName)
        {
            if (_routeStepJumps.ContainsKey(robotName))
            {
                return _routeStepJumps[robotName];
            }
            return null;
        }

        public void ReloadAllRobotRuleRouteStepJump()
        {
            foreach (string robotName in _entities.Keys)
            {
                ReloadRobotRuleRouteStepJump(robotName);
            }

        }

        //Rule Filter Function List ===========================================================================================================================================

        /// <summary> Load Robot Rule Filter
        /// Tom 
        /// 20150609
        ///
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<int, List<RobotRuleFilter>>> LoadRobotRuleFilter(Robot robot) {
            //Priority 1<2<3....所以要改成DESC
            string hql = string.Format("from RobotRuleFilterEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' order by STEPID,ITEMSEQ DESC", 
                                        robot.Data.SERVERNAME, robot.Data.ROBOTNAME);
            IList list = HibernateAdapter.GetObjectByQuery(hql);
            Dictionary<string, Dictionary<int, List<RobotRuleFilter>>> ruleFilters = new Dictionary<string, Dictionary<int, List<RobotRuleFilter>>>();
            if (list != null)
            {
                foreach (RobotRuleFilterEntityData item in list)
                {
                    if (ruleFilters.ContainsKey(item.ROUTEID))
                    {
                        if (ruleFilters[item.ROUTEID].ContainsKey(item.STEPID))
                        {
                            ruleFilters[item.ROUTEID][item.STEPID].Add(new RobotRuleFilter(item));
                        }
                        else
                        {
                            List<RobotRuleFilter> items = new List<RobotRuleFilter>();
                            items.Add(new RobotRuleFilter(item));
                            ruleFilters[item.ROUTEID].Add(item.STEPID, items);
                        }
                    }
                    else
                    {
                        Dictionary<int, List<RobotRuleFilter>> filters = new Dictionary<int, List<RobotRuleFilter>>();
                        List<RobotRuleFilter> items = new List<RobotRuleFilter>();
                        items.Add(new RobotRuleFilter(item));
                        filters.Add(item.STEPID, items);
                        ruleFilters.Add(item.ROUTEID, filters);
                    }

                }

                return ruleFilters;
            }
            return null;
        }

        public Dictionary<int, List<RobotRuleFilter>> LoadRobotRuleFilter(Robot robot, string routeID) {
            //Priority 1<2<3....所以要改成DESC
            string hql = string.Format("from RobotRuleFilterEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and ROUTEID='{2}' order by STEPID,ITEMSEQ DESC",
                                        robot.Data.SERVERNAME, robot.Data.ROBOTNAME,routeID);
            IList list = HibernateAdapter.GetObjectByQuery(hql);
            Dictionary<int, List<RobotRuleFilter>> filters=null;
            if (list != null) {
                filters = new Dictionary<int, List<RobotRuleFilter>>();
                foreach (RobotRuleFilterEntityData data in list) {
                    if (filters.ContainsKey(data.STEPID)) {
                        filters[data.STEPID].Add(new RobotRuleFilter(data));
                    }
                    else {
                        List<RobotRuleFilter> datas = new List<RobotRuleFilter>();
                        datas.Add(new RobotRuleFilter(data));
                        filters.Add(data.STEPID, datas);
                    }
                }
                return filters;
            }
            return null;
        }

        public void ReloadRobotRuleFilter(string robotName) {
            Robot robot = GetRobotByRobotName(robotName);
            if (robot != null) {
                Dictionary<string, Dictionary<int, List<RobotRuleFilter>>> filters = LoadRobotRuleFilter(robot);
                if (filters != null) {
                    lock (_filters) {
                        _filters[robotName] = filters;
                    }
                }
            }
        }

        public void ReloadRobotRuleFilter(string robotName, string routeID) {
            Robot robot = GetRobotByRobotName(robotName);
            if (robot != null) {
                Dictionary<int, List<RobotRuleFilter>> filters = LoadRobotRuleFilter(robot, routeID);
                if (filters != null) {
                    lock (_filters) {
                        _filters[robotName][routeID] = filters;
                    }
                }
            }
        }

        public List<RobotRuleFilter> GetRuleFilter(string robotName, string routeID, int stepID) {
            if (_filters.ContainsKey(robotName)) {
                if (_filters[robotName].ContainsKey(routeID)) {
                    if (_filters[robotName][routeID].ContainsKey(stepID)) {
                        return _filters[robotName][routeID][stepID];
                    }
                }
            }
            return null;
        }      

        public Dictionary<int, List<RobotRuleFilter>> GetRuleFilter(string robotName, string routeID) {
            if (_filters.ContainsKey(robotName)) {
                if (_filters[robotName].ContainsKey(routeID)) {
                    return _filters[robotName][routeID];
                }
            }
            return null;
        }

        public Dictionary<string, Dictionary<int, List<RobotRuleFilter>>> GetRuleFilter(string robotName) {
            if (_filters.ContainsKey(robotName)) {
                return _filters[robotName];
            }
            return null;
        }

        public void ReloadAllRobotRuleFilter()
        {
            foreach (string robotName in _entities.Keys)
            {
                ReloadRobotRuleFilter(robotName);
            }

        }

        //Rule Select Function List ===========================================================================================================================================

        /// <summary> Load Robot Rule Select
        /// Tom
        /// 20150609
        ///
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public Dictionary<string, List<RobotRuleSelect>> LoadRobotRuleSelect(Robot robot) {
            string hql = string.Format("from RobotRuleJobSelectEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' order by SELECTTYPE,ITEMSEQ",robot.Data.SERVERNAME,robot.Data.ROBOTNAME);
            IList list = HibernateAdapter.GetObjectByQuery(hql);
            Dictionary<string, List<RobotRuleSelect>> ruleSelects = new Dictionary<string, List<RobotRuleSelect>>();
            if (list != null)
            {
                foreach (RobotRuleJobSelectEntityData item in list)
                {
                    if (ruleSelects.ContainsKey(item.SELECTTYPE))
                    {
                        ruleSelects[item.SELECTTYPE].Add(new RobotRuleSelect(item));
                    }
                    else
                    {
                        List<RobotRuleSelect> items = new List<RobotRuleSelect>();
                        items.Add(new RobotRuleSelect(item));
                        ruleSelects.Add(item.SELECTTYPE, items);
                    }
                }

                return ruleSelects;
            }
            return null;

        }

        public List<RobotRuleSelect> LoadRobotRuleSelect(Robot robot, string selectType) {
            string hql = string.Format("from RobotRuleSelectEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and SELECTTYPE='{2}'", robot.Data.SERVERNAME, robot.Data.ROBOTNAME, selectType);
            IList list = HibernateAdapter.GetObjectByQuery(hql);
            List<RobotRuleSelect> selects = null;
            if (list != null) {
                selects = new List<RobotRuleSelect>();
                foreach (RobotRuleJobSelectEntityData data in list) {
                    selects.Add(new RobotRuleSelect(data));
                }
                return selects;
            }
            return null;
        }

        public void ReloadRobotRuleSelect(string robotName) {
            Robot robot = GetRobotByRobotName(robotName);
            if (robot != null) {
                Dictionary<string, List<RobotRuleSelect>> selects = LoadRobotRuleSelect(robot);
                if (selects != null) {
                    lock (_selects) {
                        _selects[robotName] = selects;
                    }
                }
            }
        }

        /// <summary> Reload Robot Rule Select  只能用于Update 不能对不存在的RouteID 做Reload
        ///
        ///
        /// </summary>
        /// <param name="robotName"></param>
        /// <param name="routeID"></param>
        public void ReloadRobotRuleSelect(string robotName,string routeID) {
            Robot robot = GetRobotByRobotName(robotName);
            if (robot != null) {
                List<RobotRuleSelect> selects = LoadRobotRuleSelect(robot, routeID);
                if (selects != null) {
                    lock (_selects) {
                        _selects[robotName][routeID] = selects;
                    }
                }
            }
        }

        public List<RobotRuleSelect> GetRuleSelect(string robotName, string routeID) {
            if (_selects.ContainsKey(robotName)) {
                if (_selects[robotName].ContainsKey(routeID)) {
                    return _selects[robotName][routeID];
                }
            }
            return null;
        }

        public Dictionary<string, List<RobotRuleSelect>> GetRouteSelect(string robotName) {
            if (_selects.ContainsKey(robotName)) {
                return _selects[robotName];
            }
            return null;
        }
        
        public void ReloadAllRobotRuleSelect()
        {
            foreach (string robotName in _entities.Keys)
            {
                ReloadRobotRuleSelect(robotName);
            }

        }

        //Rule OrderBy Function List ===========================================================================================================================================

        /// <summary>
        /// Tom
        /// 201506010
        /// Load  Robot Rule order by
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<int, List<RobotRuleOrderby>>> LoadRobotRuleOrderby(Robot robot) {

            //Priority 1<2<3....所以要改成DESC
            string hql = string.Format("from RobotRuleOrderbyEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' order by STEPID,ITEMSEQ DESC", robot.Data.SERVERNAME, robot.Data.ROBOTNAME);

            IList list = HibernateAdapter.GetObjectByQuery(hql);

            Dictionary<string, Dictionary<int, List<RobotRuleOrderby>>> ruleOrderbys = new Dictionary<string, Dictionary<int, List<RobotRuleOrderby>>>();
            if (list != null)
            {
                foreach (RobotRuleOrderbyEntityData item in list)
                {
                    if (ruleOrderbys.ContainsKey(item.ROUTEID))
                    {
                        if (ruleOrderbys[item.ROUTEID].ContainsKey(item.STEPID))
                        {
                            ruleOrderbys[item.ROUTEID][item.STEPID].Add(new RobotRuleOrderby(item));
                        }
                        else
                        {
                            List<RobotRuleOrderby> items = new List<RobotRuleOrderby>();
                            items.Add(new RobotRuleOrderby(item));
                            ruleOrderbys[item.ROUTEID].Add(item.STEPID, items);
                        }
                    }
                    else
                    {
                        Dictionary<int, List<RobotRuleOrderby>> orderbys = new Dictionary<int, List<RobotRuleOrderby>>();
                        List<RobotRuleOrderby> items = new List<RobotRuleOrderby>();
                        items.Add(new RobotRuleOrderby(item));
                        orderbys.Add(item.STEPID, items);
                        ruleOrderbys.Add(item.ROUTEID, orderbys);
                    }
                }

                return ruleOrderbys;
            }
            return null;
        }

        public Dictionary<int, List<RobotRuleOrderby>> LoadRobotRuleOrderby(Robot robot, string routeID) {
            //Priority 1<2<3....所以要改成DESC
            string hql = string.Format("from RobotRuleOrderbyEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and ROUTEID='{2}' order by STEPID,ITEMSEQ DESC",
                robot.Data.SERVERNAME, robot.Data.ROBOTNAME,routeID);
            IList list = HibernateAdapter.GetObjectByQuery(hql);
            Dictionary<int, List<RobotRuleOrderby>> orderbys = null;
            if (list != null) {
                orderbys = new Dictionary<int, List<RobotRuleOrderby>>();
                foreach (RobotRuleOrderbyEntityData data in list) {
                    if (orderbys.ContainsKey(data.STEPID)) {
                        orderbys[data.STEPID].Add(new RobotRuleOrderby(data));
                    }
                    else {
                        List<RobotRuleOrderby> datas = new List<RobotRuleOrderby>();
                        datas.Add(new RobotRuleOrderby(data));
                        orderbys.Add(data.STEPID, datas);
                    }
                }
                return orderbys;
            }
            return null;
        }

        public void ReloadRobotRuleOrderby(string robotName) {
            Robot robot = GetRobotByRobotName(robotName);
            if (robot != null) {
                Dictionary<string, Dictionary<int, List<RobotRuleOrderby>>> orderbys = LoadRobotRuleOrderby(robot);
                if (orderbys != null) {
                    lock (_orderbys) {
                        _orderbys[robotName] = orderbys;
                    }
                }
            }
        }

        public void ReloadRobotRuleOrderby(string robotName, string routeID) {
            Robot robot = GetRobotByRobotName(robotName);
            if (robot != null) {
                Dictionary<int, List<RobotRuleOrderby>> orderbys = LoadRobotRuleOrderby(robot, routeID);
                if (orderbys != null) {
                    lock (_orderbys) {
                        _orderbys[robotName][routeID] = orderbys;
                    }
                }
            }
        }

        public List<RobotRuleOrderby> GetRuleOrderby(string robotName, string routeID, int stepID) {
            if (_orderbys.ContainsKey(robotName)) {
                if (_orderbys[robotName].ContainsKey(routeID)) {
                    if (_orderbys[robotName][routeID].ContainsKey(stepID)) {
                        return _orderbys[robotName][routeID][stepID];
                    }
                }
            }
            return null;
        }

        public Dictionary<int, List<RobotRuleOrderby>> GetRuleOrderby(string robotName, string routeID) {
            if (_orderbys.ContainsKey(robotName)) {
                if (_orderbys[robotName].ContainsKey(routeID)) {
                    return _orderbys[robotName][routeID];
                }
            }
            return null;
        }

        public Dictionary<string, Dictionary<int, List<RobotRuleOrderby>>> GetRuleOrderby(string robotName) {
            if (_orderbys.ContainsKey(robotName)) {
                return _orderbys[robotName];
            }
            return null;
        }

        //20151201 add for Reloader OrderBy
        public void ReloadAllRobotRuleOrderBy()
        {
            foreach (string robotName in _orderbys.Keys)
            {
                ReloadRobotRuleOrderby(robotName);
            }

        }

        //Rule Result Handle Function List ===========================================================================================================================================

        /// <summary> Load  Robot Route Result Handle by Robot Entity
        /// 
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<int, List<RobotRouteResultHandle>>> LoadRobotRouteResultHandle(Robot robot)
        {

            //Priority 1<2<3....所以要改成DESC
            string hql = string.Format("from RobotRouteResultHandleEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' order by STEPID,ITEMSEQ DESC", robot.Data.SERVERNAME, robot.Data.ROBOTNAME);

            IList list = HibernateAdapter.GetObjectByQuery(hql);

            Dictionary<string, Dictionary<int, List<RobotRouteResultHandle>>> routeResultHandles = new Dictionary<string, Dictionary<int, List<RobotRouteResultHandle>>>();

            if (list != null)
            {
                foreach (RobotRouteResultHandleEntityData item in list)
                {

                    if (routeResultHandles.ContainsKey(item.ROUTEID))
                    {
                        if (routeResultHandles[item.ROUTEID].ContainsKey(item.STEPID))
                        {
                            routeResultHandles[item.ROUTEID][item.STEPID].Add(new RobotRouteResultHandle(item));
                        }
                        else
                        {
                            List<RobotRouteResultHandle> items = new List<RobotRouteResultHandle>();
                            items.Add(new RobotRouteResultHandle(item));
                            routeResultHandles[item.ROUTEID].Add(item.STEPID, items);
                        }
                    }
                    else
                    {
                        Dictionary<int, List<RobotRouteResultHandle>> resultHandles = new Dictionary<int, List<RobotRouteResultHandle>>();
                        List<RobotRouteResultHandle> items = new List<RobotRouteResultHandle>();
                        items.Add(new RobotRouteResultHandle(item));
                        resultHandles.Add(item.STEPID, items);
                        routeResultHandles.Add(item.ROUTEID, resultHandles);
                    }
                }

                return routeResultHandles;
            }
            return null;
        }

        /// <summary> Load  Robot Route Result Handle by Robot and RouteID 
        /// 
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="routeID"></param>
        /// <returns></returns>
        public Dictionary<int, List<RobotRouteResultHandle>> LoadRobotRouteResultHandle(Robot robot, string routeID)
        {
            //Priority 1<2<3....所以要改成DESC
            string hql = string.Format("from RobotRouteResultHandleEntityData where SERVERNAME='{0}' and ROBOTNAME='{1}' and ROUTEID='{2}' order by STEPID,ITEMSEQ DESC",
                robot.Data.SERVERNAME, robot.Data.ROBOTNAME, routeID);

            IList list = HibernateAdapter.GetObjectByQuery(hql);
            Dictionary<int, List<RobotRouteResultHandle>> resultHandles = null;

            if (list != null)
            {

                resultHandles = new Dictionary<int, List<RobotRouteResultHandle>>();

                foreach (RobotRouteResultHandleEntityData data in list)
                {

                    if (resultHandles.ContainsKey(data.STEPID))
                    {
                        resultHandles[data.STEPID].Add(new RobotRouteResultHandle(data));
                    }
                    else
                    {
                        List<RobotRouteResultHandle> datas = new List<RobotRouteResultHandle>();
                        datas.Add(new RobotRouteResultHandle(data));
                        resultHandles.Add(data.STEPID, datas);
                    }
                
                }
                return resultHandles;
            }
            return null;
        }

        /// <summary> Reload Robot Route Result Handle by Robot Entity
        /// 
        /// </summary>
        /// <param name="robotName"></param>
        public void ReloadRobotRouteResultHandle(string robotName)
        {
            Robot robot = GetRobotByRobotName(robotName);

            if (robot != null)
            {
                Dictionary<string, Dictionary<int, List<RobotRouteResultHandle>>> resultHandles = LoadRobotRouteResultHandle(robot);
                if (resultHandles != null)
                {
                    lock (_resultHandles)
                    {
                        _resultHandles[robotName] = resultHandles;
                    }
                }
            }
        }

        /// <summary> Reload Robot Route Result Handle by Robot and RouteID
        /// 
        /// </summary>
        /// <param name="robotName"></param>
        /// <param name="routeID"></param>
        public void ReloadRobotRouteResultHandle(string robotName, string routeID)
        {
            Robot robot = GetRobotByRobotName(robotName);

            if (robot != null)
            {
                Dictionary<int, List<RobotRouteResultHandle>> resultHandles = LoadRobotRouteResultHandle(robot, routeID);
                if (resultHandles != null)
                {
                    lock (_resultHandles)
                    {
                        _resultHandles[robotName][routeID] = resultHandles;
                    }
                }
            }
        }

        public List<RobotRouteResultHandle> GetRouteResultHandle(string robotName, string routeID, int stepID)
        {
            if (_resultHandles.ContainsKey(robotName))
            {
                if (_resultHandles[robotName].ContainsKey(routeID))
                {
                    if (_resultHandles[robotName][routeID].ContainsKey(stepID))
                    {
                        return _resultHandles[robotName][routeID][stepID];
                    }
                }
            }
            return null;
        }

        public Dictionary<int, List<RobotRouteResultHandle>> GetRouteResultHandle(string robotName, string routeID)
        {
            if (_resultHandles.ContainsKey(robotName))
            {
                if (_resultHandles[robotName].ContainsKey(routeID))
                {
                    return _resultHandles[robotName][routeID];
                }
            }
            return null;
        }

        public Dictionary<string, Dictionary<int, List<RobotRouteResultHandle>>> GetRouteResultHandle(string robotName)
        {
            if (_resultHandles.ContainsKey(robotName))
            {
                return _resultHandles[robotName];
            }
            return null;
        }

        public void ReloadAllRobotRouteResultHandle()
        {
            foreach (string robotName in _entities.Keys)
            {
                ReloadRobotRouteResultHandle(robotName);
            }

        }

        //Common Function List ===========================================================================================================================================

        //根據ServerName取得Robot
        public Robot GetRobotbySeverName(string serverName)
        {
            Robot ret = null;

            foreach (Robot robotitem in _entities.Values)
            {
                if (robotitem.Data.SERVERNAME == serverName)
                {
                    ret = robotitem;
                    break;
                }
            }

            return ret;

        }

        //目前不會出現一個Node 2隻以上的Robot 所以可透過Node取得Robot
        public Robot GetRobot(string nodeNo)
        {
            Robot ret = null;

            foreach (Robot robotitem in _entities.Values)
            {
                if (robotitem.Data.NODENO == nodeNo)
                {
                    ret = robotitem;
                    break;
                }   
            }

            return ret;
        }

        public Robot GetRobotByRobotName(string robotName)
        {
            Robot ret = null;

            foreach (Robot robotitem in _entities.Values)
            {
                if (robotitem.Data.ROBOTNAME == robotName)
                {
                    ret = robotitem;
                    break;
                }
            }

            return ret;
        }

        public List<Robot> GetRobots()
        {
            List<Robot> ret = new List<Robot>();
            foreach (Robot entity in _entities.Values)
            {
                ret.Add(entity);
            }
            return ret;
        }

        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("Robot");
            entityNames.Add("Route");
            entityNames.Add("Condition");
            entityNames.Add("Step");
            entityNames.Add("Select");
            //20150929 add StageSelect
            //entityNames.Add("StageSelect");
            entityNames.Add("RouteStepByPass");
            entityNames.Add("RouteStepJump"); 
            entityNames.Add("Filter");
            entityNames.Add("Oderby");
            entityNames.Add("RouteResultHandle");

            return entityNames;
        }

        /// <summary>
        /// Export Robot to DataTable
        /// </summary>
        /// <returns></returns>
        public DataTable ExportRobot() {
            DataTable dt = new DataTable();
            RobotEntityData data = new RobotEntityData();
            RobotEntityFile file = new RobotEntityFile();
            DataTableHelp.DataTableAppendColumn(data, dt);
            DataTableHelp.DataTableAppendColumn(file, dt);

            List<Robot> robot_entities = GetRobots();
            foreach (Robot entity in robot_entities) {
                DataRow dr = dt.NewRow();
                DataTableHelp.DataRowAssignValue(entity.Data, dr);
                DataTableHelp.DataRowAssignValue(entity.File, dr);
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// Export Robot route to DataTable
        /// </summary>
        /// <returns></returns>
        public DataTable ExportRoute() {
            DataTable dt = new DataTable();
            RobotRouteMstEntityData data = new RobotRouteMstEntityData();
            DataTableHelp.DataTableAppendColumn(data, dt);
            foreach (string key in _routes.Keys) {
                foreach (string key2 in _routes[key].Keys) {
                    DataRow dr = dt.NewRow();
                    DataTableHelp.DataRowAssignValue(_routes[key][key2].Data, dr);
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        /// <summary>
        /// Export Robot route Condition to DataTable
        /// </summary>
        /// <returns></returns>
        public DataTable ExportCodition() {
            DataTable dt = new DataTable();
            RobotRouteConditionEntityData data = new RobotRouteConditionEntityData();
            DataTableHelp.DataTableAppendColumn(data, dt);
            foreach (string key in _conditions.Keys) {
                foreach (string k2 in _conditions[key].Keys) {
                    foreach(RobotRouteCondition condition in _conditions[key][k2]){
                        DataRow dr = dt.NewRow();
                        DataTableHelp.DataRowAssignValue(condition.Data, dr);
                        dt.Rows.Add(dr);
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// Export Robot route step to DataTable
        /// </summary>
        /// <returns></returns>
        public DataTable ExportSetp() {
            DataTable dt = new DataTable();
            RobotRouteStepEntityData data = new RobotRouteStepEntityData();
            DataTableHelp.DataTableAppendColumn(data, dt);
            foreach (string key1 in _steps.Keys) {
                foreach (string key2 in _steps[key1].Keys) {
                    foreach (RobotRouteStep step in _steps[key1][key2]) {
                        DataRow dr = dt.NewRow();
                        DataTableHelp.DataRowAssignValue(step.Data, dr);
                        dt.Rows.Add(dr);
                    }
                }
            }
            return dt;
        }

        /// <summary> Export Robot route step to DataTable
        ///
        /// </summary>
        /// <returns></returns>
        public DataTable ExportFilter() {
            DataTable dt = new DataTable();
            RobotRuleFilterEntityData data = new RobotRuleFilterEntityData();
            DataTableHelp.DataTableAppendColumn(data, dt);
            foreach (string key1 in _filters.Keys) {
                foreach (string key2 in _filters[key1].Keys) {
                    foreach (int key3 in _filters[key1][key2].Keys) {
                        foreach (RobotRuleFilter filter in _filters[key1][key2][key3]) {
                            DataRow dr = dt.NewRow();
                            DataTableHelp.DataRowAssignValue(filter.Data, dr);
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            return dt;
        }

        //20150929 add Stage Select Function
        public DataTable ExportStageSelect()
        {
            DataTable dt = new DataTable();
            RobotRouteRuleStageSelectEntityData data = new RobotRouteRuleStageSelectEntityData();
            DataTableHelp.DataTableAppendColumn(data, dt);

            foreach (string key1 in _stageSelects.Keys)
            {
                foreach (string key2 in _stageSelects[key1].Keys)
                {
                    foreach (int key3 in _stageSelects[key1][key2].Keys)
                    {
                        foreach (RobotRuleStageSelect stageSelect in _stageSelects[key1][key2][key3])
                        {
                            DataRow dr = dt.NewRow();
                            DataTableHelp.DataRowAssignValue(stageSelect.Data, dr);
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// Export Robot Route Select to DataTable
        /// </summary>
        /// <returns></returns>
        public DataTable ExportSelect() {
            DataTable dt = new DataTable();
            RobotRuleJobSelectEntityData data = new RobotRuleJobSelectEntityData();
            DataTableHelp.DataTableAppendColumn(data, dt);
            foreach (string key1 in _selects.Keys) {
                foreach (string key2 in _selects[key1].Keys) {
                    foreach (RobotRuleSelect select in _selects[key1][key2]) {
                        DataRow dr = dt.NewRow();
                        DataTableHelp.DataRowAssignValue(select.Data, dr);
                        dt.Rows.Add(dr);
                    }
                }
            }
            return dt;
        }

        /// <summary> Export Robot Route Order by to DataTable
        ///
        /// </summary>
        /// <returns></returns>
        public DataTable ExportOrderby() {
            DataTable dt = new DataTable();
            RobotRuleOrderbyEntityData data = new RobotRuleOrderbyEntityData();
            DataTableHelp.DataTableAppendColumn(data, dt);
            foreach (string key1 in _orderbys.Keys) {
                foreach (string key2 in _orderbys[key1].Keys) {
                    foreach (int key3 in _orderbys[key1][key2].Keys) {
                        foreach (RobotRuleOrderby orderby in _orderbys[key1][key2][key3]) {
                            DataRow dr = dt.NewRow();
                            DataTableHelp.DataRowAssignValue(orderby.Data, dr);
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            return dt;
        }

        /// <summary> Export Robot Route result Handle DataTable
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable ExportRouteResultHandle()
        {
            DataTable dt = new DataTable();

            RobotRouteResultHandleEntityData data = new RobotRouteResultHandleEntityData();
            DataTableHelp.DataTableAppendColumn(data, dt);
            
            foreach (string key1 in _resultHandles.Keys)
            {
                foreach (string key2 in _resultHandles[key1].Keys)
                {
                    foreach (int key3 in _resultHandles[key1][key2].Keys)
                    {
                        foreach (RobotRouteResultHandle resultHandle in _resultHandles[key1][key2][key3])
                        {
                            DataRow dr = dt.NewRow();
                            DataTableHelp.DataRowAssignValue(resultHandle.Data, dr);
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            return dt;
        }

        //20151007 add for RouteStepByPass
        /// <summary> Export Robot route step By Pass to DataTable
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable ExportRouteStepByPass()
        {
            DataTable dt = new DataTable();
            RobotRuleRouteStepByPassEntityData data = new RobotRuleRouteStepByPassEntityData();

            DataTableHelp.DataTableAppendColumn(data, dt);

            foreach (string key1 in _routeStepByPasss.Keys)
            {
                foreach (string key2 in _routeStepByPasss[key1].Keys)
                {
                    foreach (int key3 in _routeStepByPasss[key1][key2].Keys)
                    {
                        foreach (RobotRuleRouteStepByPass routeStepByPass in _routeStepByPasss[key1][key2][key3])
                        {
                            DataRow dr = dt.NewRow();
                            DataTableHelp.DataRowAssignValue(routeStepByPass.Data, dr);
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            return dt;
        }

        //20151007 add for RouteStepJump
        /// <summary> Export Robot route step Jump to DataTable
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable ExportRouteStepJump()
        {
            DataTable dt = new DataTable();
            RobotRuleRouteStepJumpEntityData data = new RobotRuleRouteStepJumpEntityData();

            DataTableHelp.DataTableAppendColumn(data, dt);

            foreach (string key1 in _routeStepJumps.Keys)
            {
                foreach (string key2 in _routeStepJumps[key1].Keys)
                {
                    foreach (int key3 in _routeStepJumps[key1][key2].Keys)
                    {
                        foreach (RobotRuleRouteStepJump routeStepJump in _routeStepJumps[key1][key2][key3])
                        {
                            DataRow dr = dt.NewRow();
                            DataTableHelp.DataRowAssignValue(routeStepJump.Data, dr);
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            return dt;
        }

        public System.Data.DataTable GetDataTable(string entityName) {
            switch (entityName) {
                case "Robot":
                    return ExportRobot();
                case "Route":
                    return ExportRoute();
                case "Condition":
                    return ExportCodition();
                case "Step":
                    return ExportSetp();
                case "Select":
                    return ExportSelect();
                //20150929 add for Stage Select
                case "StageSelect":
                    return ExportStageSelect();
                case "RouteStepByPass":
                    return ExportRouteStepByPass();
                case "RouteStepJump":
                    return ExportRouteStepJump();
                case "Filter":
                    return ExportFilter();
                case "Oderby":
                    return ExportOrderby();
                case "RouteResultHandle":
                    return ExportRouteResultHandle();
                default:
                    return null;

            }
        }

    }
}
