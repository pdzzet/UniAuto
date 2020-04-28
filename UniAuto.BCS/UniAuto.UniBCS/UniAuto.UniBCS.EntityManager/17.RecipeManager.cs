using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.DB;
using UniAuto.UniBCS.Log;
using System.Reflection;
using System.Collections;
using UniAuto.UniBCS.Core;
using System.Text.RegularExpressions;
using System.Data;
namespace UniAuto.UniBCS.EntityManager
{
    public class RecipeManager : IDataSource
    {
        /// <summary>
        /// 各机台的Recipe Parameter设定档
        /// </summary>
        private IDictionary<string, IList<RecipeParameter>> _recipeParameters = new Dictionary<string, IList<RecipeParameter>>();

        /// <summary>
        /// NLog Logger Name
        /// </summary>
        public string LoggerName { get; set; }

        /// <summary>
        /// 用來讀取DB
        /// </summary>
        public HibernateAdapter HibernateAdapter { get; set; }

        public void Init()
        {
            _recipeParameters = Reload();
            
        }

        protected IDictionary<string,IList<RecipeParameter>> Reload()
        {
            try
            {
                IDictionary<string, IList<RecipeParameter>> recipeParameters = new Dictionary<string, IList<RecipeParameter>>();
                string hql = string.Format("from RecipeParameterEntityData where SERVERNAME = '{0}' order by NODENO, OBJECTKEY", Workbench.ServerName);
                IList list=this.HibernateAdapter.GetObjectByQuery(hql);
                IList<RecipeParameter> recipeList=null;
                if (list != null)
                {
                    foreach(RecipeParameterEntityData recipe in list)
                    {
                        //过滤一些特殊字符
                        //recipe.PARAMETERNAME = recipe.PARAMETERNAME.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                        recipe.PARAMETERNAME = Regex.Replace(recipe.PARAMETERNAME, @"[/| |?|/|<|>|'|\-|,]", "_");
                        if (!recipeParameters.ContainsKey(recipe.NODENO))
                        {
                            recipeList = new List<RecipeParameter>();
                            recipeParameters.Add(recipe.NODENO, recipeList);
                        }
                        recipeParameters[recipe.NODENO].Add(new RecipeParameter(recipe));
                        
                    }
                }
                return recipeParameters;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        /// <summary>
        /// By Equipment No Reload Reipe Parameter
        /// </summary>
        /// <param name="nodeNo"></param>
        public void ReloadByEqpNO(string nodeNo)
        {
            try
            {
                string hql = string.Format("from RecipeParameterEntityData where SERVERNAME = '{0}' and NODENO='{1}' order by OBJECTKEY ", Workbench.ServerName, nodeNo);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<RecipeParameter> recipeList = new List<RecipeParameter>();
                if (list != null)
                {
                    foreach (RecipeParameterEntityData data in list)
                    {
                        //过滤一些特殊字符
                        //data.PARAMETERNAME = data.PARAMETERNAME.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                        data.PARAMETERNAME = Regex.Replace(data.PARAMETERNAME, @"[/| |?|/|<|>|'|\-|,]", "_");
                        recipeList.Add(new RecipeParameter(data));
                    }
                    lock (_recipeParameters)
                    {
                        if (_recipeParameters.ContainsKey(nodeNo))
                        {
                            _recipeParameters.Remove(nodeNo);
                        }
                        _recipeParameters.Add(nodeNo, recipeList);
                    }
                }
                NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, 
                            string.Format("Reload Recipe Parameter EQUIPMENT=[{0}]",nodeNo));

            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        /// <summary>
        /// Relaod All Equipment RecipeParameter
        /// </summary>
        public void ReloadAll()
        {
            try
            {
                IDictionary<string, IList<RecipeParameter>> tempDic = Reload();

                if (tempDic != null)
                {
                    lock (_recipeParameters)
                    {
                        _recipeParameters = tempDic;
                    }
                    
                }
                NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                           string.Format("Reload Recipe Parameter all Equipment."));
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
           

        }

        /// <summary>
        /// By Equipment No Get RecipeParameter 
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <returns></returns>
        public IList<RecipeParameter> GetRecipeParameter(string equipmentNo)
        {
            try
            {
                if (_recipeParameters.ContainsKey(equipmentNo))
                {
                    return _recipeParameters[equipmentNo];
                }
                return null;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        public RECIPE FindOne(string lineType, string lineRecipeName, string hostMode)
        {
            try
            {
                if (this.HibernateAdapter != null)
                {
                    IList list2 = HibernateAdapter.GetObject_AND(typeof(RECIPE), 
                                        new string[]{"LINETYPE","LINERECIPENAME","ONLINECONTROLSTATE"}, 
                                        new object[]{lineType,lineRecipeName,hostMode}, null, null);

                    if (list2 != null && list2.Count > 0)
                    {
                        return (RECIPE)list2[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return null;
        }

        public RECIPECROSS FindOneForCross(string lineType, string lineRecipeName, string hostMode)
        {
            try
            {
                if (this.HibernateAdapter != null)
                {
                    IList list2 = HibernateAdapter.GetObject_AND(typeof(RECIPECROSS),
                                        new string[] { "LINETYPE", "LINERECIPENAME", "ONLINECONTROLSTATE" },
                                        new object[] { lineType, lineRecipeName, hostMode }, null, null);

                    if (list2 != null && list2.Count > 0)
                    {
                        return (RECIPECROSS)list2[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return null;
        }

        public void SaveRecipeObject(Line line, string lineRecipeName, string ppid,string ip,string recipeType)
        {
            try
            {
                if (ppid.Substring(ppid.Length - 1, 1) == ";")
                {
                    ppid = ppid.Substring(0, ppid.Length - 1);
                }
                //(string lineType, string lineRecipeName, string hostMode)
                RECIPE recipe = FindOne(line.Data.LINETYPE, lineRecipeName, line.File.HostMode.ToString());
                if (recipe != null)
                {
                    recipe.PPID = ppid;
                    recipe.LASTUPDATEDT=DateTime.Now;
                    HibernateAdapter.UpdateObject(recipe);
                }
                else
                {
                    recipe = new RECIPE();
                    
                    recipe.LINETYPE=line.Data.LINETYPE;
                    recipe.FABTYPE=line.Data.FABTYPE;
                    recipe.PPID = ppid;
                    recipe.LINERECIPENAME = lineRecipeName;
                    recipe.LASTUPDATEDT = DateTime.Now;
                    recipe.UPDATELINEID = line.Data.LINEID;
                    recipe.UPDATEOPERATOR = "BCS";
                    recipe.UPDATEPCIP = ip;
                    //recipe.RECIPECHECK = "";
                    recipe.ONLINECONTROLSTATE = line.File.HostMode.ToString();
                    recipe.REMARK = "";
                    recipe.RECIPETYPE = recipeType;
                    //recipe.RECIPECHECK = recipeCheck;
                    HibernateAdapter.SaveObject(recipe); 
                    
                }
               
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SaveRecipeObjectForCross(Line line, string lineRecipeName, string ppid, string ip, string recipeType, string saveLineType)
        {
            try
            {
                if (ppid.Substring(ppid.Length - 1, 1) == ";")
                {
                    ppid = ppid.Substring(0, ppid.Length - 1);
                }
                //(string lineType, string lineRecipeName, string hostMode)
                RECIPECROSS recipe = FindOneForCross(saveLineType, lineRecipeName, line.File.HostMode.ToString());
                if (recipe != null)
                {
                    recipe.PPID = ppid;
                    recipe.LASTUPDATEDT = DateTime.Now;
                    HibernateAdapter.UpdateObject(recipe);
                }
                else
                {
                    recipe = new RECIPECROSS();

                    recipe.LINETYPE = saveLineType;
                    recipe.FABTYPE = line.Data.FABTYPE;
                    recipe.PPID = ppid;
                    recipe.LINERECIPENAME = lineRecipeName;
                    recipe.LASTUPDATEDT = DateTime.Now;
                    recipe.UPDATELINEID = line.Data.LINEID;
                    recipe.UPDATEOPERATOR = "BCS";
                    recipe.UPDATEPCIP = ip;
                    //recipe.RECIPECHECK = "";
                    recipe.ONLINECONTROLSTATE = line.File.HostMode.ToString();
                    recipe.REMARK = "";
                    recipe.RECIPETYPE = recipeType;
                    //recipe.RECIPECHECK = recipeCheck;
                    HibernateAdapter.SaveObject(recipe);

                }

            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void SaveCUTMaxRecipeObject(eHostMode LineHostMode, string lineRecipeName, string ppid, string recipeCheck)
        {
            try
            {
                if (ppid.Substring(ppid.Length - 1, 1) == ";")
                {
                    ppid = ppid.Substring(0, ppid.Length - 1);
                }


                RECIPE recipe = FindOne(eFabType.CELL.ToString(), eLineType.CELL.CBCUT_3.ToString(), LineHostMode.ToString());
                if (recipe != null)
                {
                    recipe.PPID = ppid;
                    recipe.LASTUPDATEDT = DateTime.Now;
                    HibernateAdapter.UpdateObject(recipe);
                }
                else
                {
                    recipe = new RECIPE();

                    recipe.LINETYPE = eLineType.CELL.CBCUT_3.ToString();
                    recipe.FABTYPE = eFabType.CELL.ToString();
                    recipe.PPID = ppid;
                    recipe.LINERECIPENAME = lineRecipeName;
                    recipe.LASTUPDATEDT = DateTime.Now;
                    recipe.UPDATELINEID = "CBCUT500";
                    recipe.UPDATEOPERATOR = "BCS";
                    recipe.UPDATEPCIP = "127.0.0.1";
                    //recipe.RECIPECHECK = "";
                    recipe.ONLINECONTROLSTATE = LineHostMode.ToString();
                    recipe.REMARK = "";
                    //recipe.RECIPECHECK = recipeCheck;
                    HibernateAdapter.SaveObject(recipe);
                }

            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();

                RecipeParameterEntityData file = new RecipeParameterEntityData();
                DataTableHelp.DataTableAppendColumn(file, dt);


                foreach (IList<RecipeParameter> list in _recipeParameters.Values)
                {
                    foreach (RecipeParameter pd in list)
                    {
                        DataRow dr = dt.NewRow();
                        DataTableHelp.DataRowAssignValue(pd.Data, dr);
                        dt.Rows.Add(dr);
                    }
                }
                return dt;
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("RecipeManager");
            return entityNames;
        }
    }
}
