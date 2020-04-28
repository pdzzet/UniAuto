using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.Entity;
using System.Xml;
using UniAuto.UniBCS.Core;
using System.Collections;
using System.Threading;

namespace UniAuto.UniBCS.EntityManager
{
    public class CassetteManager : EntityManager, IDataSource
    {
        private Dictionary<string, Cassette> _entities = new Dictionary<string, Cassette>();

        private string _CompleteCSTPath = string.Empty;
        private string _IncompleteCSTPath = string.Empty;
        private string _LotEndExecuteCSTPath = string.Empty;

        public string CompleteCSTPath 
        {
            get { return this._CompleteCSTPath.Replace("{ServerName}", Workbench.ServerName); }
            set { this._CompleteCSTPath = value; }
        }

        public string IncompleteCSTPath
        {
            get { return this._IncompleteCSTPath.Replace("{ServerName}", Workbench.ServerName); }
            set { this._IncompleteCSTPath = value; }
        }

        public string LotEndExecuteCSTPath
        {
            get { return this._LotEndExecuteCSTPath.Replace("{ServerName}", Workbench.ServerName); }
            set { this._LotEndExecuteCSTPath = value; }
        }

        public override EntityManager.FILE_TYPE GetFileType()
        {
            return FILE_TYPE.BIN;
        }

        protected override Type GetTypeOfEntityData()
        {
            return null;// typeof(CassetteEntityData);
        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(Cassette);
        }

        protected override void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles)
        {           
            foreach (EntityFile file in entityFiles)
            {
                Cassette cst = file as Cassette;
                if (!_entities.ContainsKey(cst.CassetteSequenceNo))
                    _entities.Add(cst.CassetteSequenceNo, cst);
            }
        }

        public Cassette GetCassette(string lineID, string eqpNo, string portNo)
        {
            Cassette ret = null;
            foreach (Cassette entity in _entities.Values)
            {
                if (entity.LineID == lineID && entity.NodeNo == eqpNo && entity.PortNo == portNo)
                {
                    ret = entity;
                    break;
                }
            }
            return ret;
        }

        public Cassette GetCassette(string eqpID, string portNo)
        {
            Cassette ret = null;
            foreach (Cassette entity in _entities.Values)
            {
                if (entity.NodeID == eqpID && entity.PortNo == portNo)
                {
                    ret = entity;
                    break;
                }
            }
            return ret;
        } 

        public Cassette GetCassette(string cstID)
        {
            lock (_entities) //Lcok By Yangzhenteng For PPK Errors 20190624;
            {
                Cassette ret = null;
                //watson modify 20141120 For 避免找不到Cassette
                //ret = _entities.Values.FirstOrDefault(c => c.CassetteID.Equals(cstID));
                ret = _entities.Values.FirstOrDefault(c => c.CassetteID.Trim().Equals(cstID.Trim()));
                return ret;           
            }
        }

        public Cassette GetCassette(int cassetteSequenceNo)
        {
            Cassette ret = null;
            //_entities.Add("0",new Cassette());    //for T3 MES Test not get object cst
            if (_entities.ContainsKey(cassetteSequenceNo.ToString()))
                ret = _entities[cassetteSequenceNo.ToString()];
            return ret;
        }

        public void CreateCassette(Cassette cassette)
        {
            lock (_entities)
            {
                if (_entities.ContainsKey(cassette.CassetteSequenceNo))
                    _entities.Remove(cassette.CassetteSequenceNo);

                IList<Cassette> csts = _entities.Values.Where(c =>
                    c.CassetteID.Trim().Equals(cassette.CassetteID.Trim())).ToList<Cassette>();

                foreach (Cassette cst in csts)
                {
                    DeleteCassette(cst.CassetteSequenceNo);
                }

                //cassette.SetFilename(string.Format("{0}_{1}_{2}.bin", cassette.PortID, cassette.CassetteSequenceNo, cassette.CassetteID));
                cassette.SetFilename(string.Format("{0}_{1}_{2}.{3}", cassette.PortID, cassette.CassetteSequenceNo, cassette.CassetteID,GetFileExtension()));
                _entities.Add(cassette.CassetteSequenceNo, cassette);
                EnqueueSave(cassette);
            }
        }

        //Watson Add 20141230 For DenseBox
        public void CreateBox(Cassette cassette)
        {
            //Watson Modify 20150130 For DenseBox Create
            //程式重開讀檔時一定會發生 以seqno為key值的BOX
            //AC廠 Normal Cassette 是以Cassette Seq NO. 為 key.
            //但是CELL廠的 DenseBox 則以BOX ID(Cassette ID.)為 key

            //BC Initial ,讀取檔案仍以Seq NO當key 放入Cassette Entity 中. => Dictionary.Add(seqNo,Cassette)
            //這樣在特殊的DenseBox Line，就可能產生不同key，box卻相同的Entity.(如1001,CAS001)
            //為避免重複的BOX 出現在Cassette Entity中，在每次產生新的BOX時, 就檢查Cassette Entity
            //先移除集合中同樣BOXID 但以seqno為key值的BOX，
            //再移除集合中同樣BOXID 為 key值的BOX
            //最後產生以BOXID當key值的 NEW BOX (Cassette Entity)
            //避免同時存在太多殘留的帳
            //List<Cassette> cstlist = _entities.Values.Where(cst => cst.CassetteID == cassette.CassetteID).ToList();
            List<Cassette> cstlist = new List<Cassette>(_entities.Values.ToArray());

            foreach (Cassette cst in cstlist)
            {

                if (cst.CassetteSequenceNo == cassette.CassetteSequenceNo)
                {
                    string err = string.Format("OLD BOX ID=[{0}], OLD BOX_SEQ_NO IS EQUAL Create NEW BOX_SEQ_NO=[{1}] Delete OLD BOX=[{2}] ,SAVE NEW BOX=[{3}]",
                        cst.CassetteID, cst.CassetteSequenceNo, cst.CassetteID, cassette.CassetteID);
                    Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    _entities.Remove(cst.CassetteSequenceNo);
                    cst.WriteFlag = false;
                    EnqueueSave(cst);
                }
                //Watson Add 讀檔後，可能會有殘帳的BOX, 但是KEY不是id而是seqno.
                if (cst.CassetteID == cassette.CassetteID)
                {
                    string err = string.Format("OLD BOX ID=[{0}] IS EQUAL Create NEW BOX ID=[{1}] Delete OLD BOX=[{0}] ,OLD BOX_SEQ_NO=[{2}] ,SAVE NEW BOX=[{1}] NEW BOX_SEQ_NO=[{3}]",
                        cst.CassetteID, cassette.CassetteID, cst.CassetteSequenceNo, cassette.CassetteSequenceNo);
                    Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    _entities.Remove(cst.CassetteSequenceNo); 
                    cst.WriteFlag = false;
                    EnqueueSave(cst);
                }
            }

            if (_entities.ContainsKey(cassette.CassetteID))
            {
                Cassette ret = _entities[cassette.CassetteID];

                string err = string.Format("OLD BOX ID=[{0}], OLD BOX ID IS EQUAL Create NEW BOX ID, Delete OLD BOX BOX_SEQ_NO=[{1}], SAVE NEW BOX=[{2}], NEW BOX_SEQ_NO=[{3}]",
                   ret.CassetteID, ret.CassetteSequenceNo, cassette.CassetteID, cassette.CassetteSequenceNo);
                Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                _entities.Remove(cassette.CassetteID);
                ret.WriteFlag = false;
                EnqueueSave(ret);
            }

            //cassette.SetFilename(string.Format("{0}_{1}_{2}.bin", cassette.PortID, cassette.CassetteSequenceNo, cassette.CassetteID));
            cassette.SetFilename(string.Format("{0}_{1}_{2}.{3}", cassette.PortID, cassette.CassetteSequenceNo, cassette.CassetteID,GetFileExtension()));
            _entities.Add(cassette.CassetteID, cassette);
            EnqueueSave(cassette);
        }        
        public void DeleteCassette(string CassetteSequenceNo)
        {
            lock (_entities)
            {
                if (_entities.ContainsKey(CassetteSequenceNo))
                {
                    Cassette cst = _entities[CassetteSequenceNo];
                    _entities.Remove(CassetteSequenceNo);
                    cst.WriteFlag = false;
                    EnqueueSave(cst);
                }
            }
        }

        //sy Add 20151210 For DenseBox
        public void CreateBoxforPacking(Cassette cassette)
        {
            //CreateBox 去掉 不判斷CassetteSequenceNo 不然會將CassetteSequenceNo = 0 的不斷覆蓋
            //keyNo = CassetteSequenceNo
            lock (_entities)  //lock By Yangzhenteng For PPK Errors 20190624;
            {
                Cassette newCst = (Cassette)cassette.Clone();
                int keyNo = 0;
                while (true)
                {
                    if (_entities.ContainsKey(keyNo.ToString())) keyNo++;
                    else break;
                }
                newCst.CassetteSequenceNo = keyNo.ToString();
                List<Cassette> cstlist = new List<Cassette>(_entities.Values.ToArray());//裡面不包括 將要新增的
                foreach (Cassette cst in cstlist)
                {
                    if (cst == null) continue;//20161031 sy modify 防止cst 為null 
                    ////還是需要Key值，程式重開讀檔時一定會發生，讀取Key相同，讀不到的問題，所以再生成前自行給
                    //for (int i = 0; i < 65534; i++)
                    //{
                    //    if (CassetteSequenceNo == 65534) CassetteSequenceNo = 0;                   

                    //    if (cst.CassetteSequenceNo == CassetteSequenceNo.ToString())
                    //        CassetteSequenceNo++;
                    //    else
                    //        break;                   
                    //}
                    if (cst.CassetteID == newCst.CassetteID)
                    {
                        string err = string.Format("OLD BOX ID=[{0}] IS EQUAL Create NEW BOX ID=[{1}] Delete OLD BOX=[{0}] ,OLD BOX_SEQ_NO=[{2}] ,SAVE NEW BOX=[{1}] NEW BOX_SEQ_NO=[{3}]",
                            cst.CassetteID, newCst.CassetteID, cst.CassetteSequenceNo, newCst.CassetteSequenceNo);
                        Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                        _entities.Remove(cst.CassetteSequenceNo);//20161102 sy modify    
                        cst.WriteFlag = false;
                        EnqueueSave(cst);
                    }
                }
                if (_entities.ContainsKey(newCst.CassetteSequenceNo))//20161102 sy modify
                {
                    Cassette ret = _entities[newCst.CassetteSequenceNo];//20161102 sy modify
                    string err = string.Format("OLD BOX ID=[{0}], OLD BOX ID IS EQUAL Create NEW BOX ID, Delete OLD BOX BOX_SEQ_NO=[{1}], SAVE NEW BOX=[{2}], NEW BOX_SEQ_NO=[{3}]",
                       ret.CassetteID, ret.CassetteSequenceNo, cassette.CassetteID, cassette.CassetteSequenceNo);
                    Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    _entities.Remove(cassette.CassetteSequenceNo);//20161102 sy modify                
                    ret.WriteFlag = false;
                    EnqueueSave(ret);
                }
                //cassette.CassetteSequenceNo = CassetteSequenceNo.ToString();
                newCst.SetFilename(string.Format("P{0}_JS{1}_CST{2}.{3}", newCst.PortID == string.Empty ? "00" : newCst.PortID, newCst.CassetteSequenceNo, newCst.CassetteID, GetFileExtension()));
                _entities.Add(newCst.CassetteSequenceNo, newCst);
                newCst.WriteFlag = true;
                EnqueueSave(newCst);         
            }
        }

        public void DeleteBoxforPacking(Cassette cassette)
        {
            List<Cassette> cstlist = new List<Cassette>(_entities.Values.ToArray());
            foreach (Cassette cst in cstlist)
            {
                if (cst == null) continue;//20161031 sy modify 防止cst 為null
                if (cst.CassetteID == cassette.CassetteID)
                {
                    _entities.Remove(cst.CassetteSequenceNo);//20161031 sy modify Key 為CassetteID // 20161102改回
                    cst.WriteFlag = false;
                    EnqueueSave(cst);
                }
            }
        }

        //Watson Add 20150102 For 刪除cassette by Cassette ID
        public void DeleteBox(string cassetteID)
        {
            List<Cassette> cstlist = new List<Cassette>(_entities.Values.ToArray());
            foreach (Cassette cst in cstlist)
            {
                _entities.Remove(cassetteID);
                cst.WriteFlag = false;
                EnqueueSave(cst);
            }
        }
        public List<Cassette> GetCassettes()
        {
            List<Cassette> ret = new List<Cassette>();
            foreach (Cassette entity in _entities.Values)
            {
                ret.Add(entity);
            }
            return ret;
        }

        protected override string GetSelectHQL()
        {
            return string.Empty;
        }

        protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            Filenames = new List<string>();
            Filenames.Add("*.Bin");
        }

        protected override EntityFile NewEntityFile(string Filename)
        {
            return null;
        }

        //private void DataTableAppendColumn(object obj, DataTable dataTable)
        //{
        //    Type type = obj.GetType();
        //    PropertyInfo[] properties = type.GetProperties();
        //    foreach (PropertyInfo prop in properties)
        //    {
        //        dataTable.Columns.Add(prop.Name, typeof(string));
        //    }
        //}

        //private void DataRowAssignValue(object obj, DataRow dataRow)
        //{
        //    Type type = obj.GetType();
        //    PropertyInfo[] properties = type.GetProperties();
        //    foreach (PropertyInfo prop in properties)
        //    {
        //        object val = prop.GetValue(obj, null);
        //        dataRow[prop.Name] = val.ToString();
        //    }
        //}

        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("CassetteManager");
            return entityNames;
        }

        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();
                Cassette file = new Cassette();
                DataTableHelp.DataTableAppendColumn(file, dt);

                IList<Cassette> cst_entities = GetCassettes();
                foreach (Cassette cst in cst_entities)
                {
                    DataRow dr = dt.NewRow();
                    DataTableHelp.DataRowAssignValue(cst, dr);
                    dt.Rows.Add(dr);
                }
                return dt;
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
           
        }

        /// <summary> 
        /// LotEndExecute - [XmlDocument Data] ->  [LotEndExecute]
        /// 描述：Xml資料寫入作業。
        /// 1. 將傳入的 XmlDocument 資料寫入至設定的 LotEndExecuteCSTPath 路徑中。
        /// 2. Xml 檔名格式為 Pord ID + Cassette ID + Trx ID。
        /// 3. 若目標位置有相同的檔案，會將其覆蓋。
        /// </summary>
        /// <param name="strPordID">Pord ID</param>
        /// <param name="strCassetteID">Cassette ID</param>
        /// <param name="strMesTrxID">MES Trx ID - [yyyyMMddhhmmssfff]</param>
        /// <param name="xmlDocument">已組好的 XmlDocument</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <returns></returns>
        public bool FileSaveToLotEndExecute(string strPordID, string strCassetteID, string strMesTrxID, XmlDocument xmlDocument, out string strDescription, string strFileType = "")
        {
            try
            {
                if (string.IsNullOrEmpty(this.LotEndExecuteCSTPath))
                {
                    string strErr = string.Format("FileSaveToLotEndExecute NG - CassetteManager.LotEndExecuteCSTPath is null or empty!  ,PordID:{0}, CassetteID:{1}, MesTrxID:{2}"
                        , strPordID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }

                if (this.LotEndExecuteCSTPath[this.LotEndExecuteCSTPath.Length - 1] != '\\')
                    this.LotEndExecuteCSTPath += "\\";

                if (Directory.Exists(this.LotEndExecuteCSTPath) == false)
                    Directory.CreateDirectory(this.LotEndExecuteCSTPath);

                StringBuilder sbErr = new StringBuilder();
                if(string.IsNullOrEmpty(strPordID.Trim()))
                    sbErr.Append("[PordID is null or empty] ");
                if (string.IsNullOrEmpty(strCassetteID.Trim()))
                    sbErr.Append("[CassetteID is null or empty] ");
                if (string.IsNullOrEmpty(strMesTrxID.Trim()))
                    sbErr.Append("[MesTrxID is null or empty] ");
                if (sbErr.Length > 0)
                {
                    string strErr = string.Format("FileSaveToLotEndExecute NG - Parameters Error - {0}", sbErr.ToString());
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }

                strPordID = strPordID.Trim();
                strCassetteID = strCassetteID.Trim();
                strMesTrxID = strMesTrxID.Trim();

                string strFilePath = string.Format("{0}{1}_{2}_{3}.xml", this.LotEndExecuteCSTPath, strFileType + strPordID, strCassetteID, strMesTrxID);
                xmlDocument.Save(strFilePath);
                strDescription = "FileSaveToLotEndExecute OK";
                return true;
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("FileSaveToLotEndExecute NG - Exception Message:{0}  ,PordID:{1}, CassetteID:{2}, MesTrxID:{3}"
                    , ex.Message, strPordID, strCassetteID, strMesTrxID);
                return false;
            }
        }

        /// <summary> 
        /// FileMoveToCompleteCST - [LotEndExecute] -> [CompleteCST]
        /// 描述：檔案搬移作業。
        /// 1. 從 LotEndExecute\ Folder 中，將 Xml 文件搬移至 CompleteCST\yyyyMMdd\ Folder 內。
        /// 2. 尋找目標 Xml 文件的檔名格式為 Pord ID + Cassette ID + Trx ID。
        /// 3. 若目標位置有相同的檔案，會將其覆蓋。
        /// </summary>
        /// <param name="strPordID">Pord ID</param>
        /// <param name="strCassetteID">Cassette ID</param>
        /// <param name="strMesTrxID">MES Trx ID - [yyyyMMddhhmmssfff]</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <returns></returns>
        public bool FileMoveToCompleteCST(string strPordID, string strCassetteID, string strMesTrxID, out string strDescription, string strFileType = "")
        {
            try
            {
                string strDate = strMesTrxID.Substring(0, 8);

                #region [Check CassetteManager Path]
                StringBuilder sb = new StringBuilder();
                if (string.IsNullOrEmpty(this.LotEndExecuteCSTPath))
                    sb.Append("[CassetteManager.LotEndExecuteCSTPath is null or empty! - core.xml] ");
                if (string.IsNullOrEmpty(this.CompleteCSTPath))
                    sb.Append("[CassetteManager.CompleteCSTPath is null or empty! - core.xml] ");
                if (sb.Length > 0)
                {
                    string strErr = "FileMoveToCompleteCST NG - " + sb.ToString();
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                if (this.LotEndExecuteCSTPath[this.LotEndExecuteCSTPath.Length - 1] != '\\')
                    this.LotEndExecuteCSTPath += "\\";
                if (this.CompleteCSTPath[this.CompleteCSTPath.Length - 1] != '\\')
                    this.CompleteCSTPath += "\\";
                #endregion

                #region [Check File Name & Format]
                StringBuilder sbErr = new StringBuilder();
                if (string.IsNullOrEmpty(strPordID.Trim()))
                    sbErr.Append("[PordID is null or empty] ");
                if (string.IsNullOrEmpty(strCassetteID.Trim()))
                    sbErr.Append("[CassetteID is null or empty] ");
                if (string.IsNullOrEmpty(strMesTrxID.Trim()))
                    sbErr.Append("[MesTrxID is null or empty] ");
                if (sbErr.Length > 0)
                {
                    string strErr = string.Format("FileMoveToCompleteCST NG - Parameters Error - {0}", sbErr.ToString());
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }

                strPordID = strPordID.Trim();
                strCassetteID = strCassetteID.Trim();
                strMesTrxID = strMesTrxID.Trim();

                string strFileName = string.Format("{0}_{1}_{2}.xml", strFileType + strPordID, strCassetteID, strMesTrxID);
                string strTargetPath = string.Format("{0}{1}\\", this.CompleteCSTPath, strDate);
                string strSourceFile = string.Format("{0}{1}", this.LotEndExecuteCSTPath, strFileName);
                string strTargetFile = string.Format("{0}{1}", strTargetPath, strFileName);
                #endregion

                #region [File Process]
                //Create Folder
                if (Directory.Exists(strTargetPath) == false)
                    Directory.CreateDirectory(strTargetPath);
                //Check File Exists
                if (!File.Exists(strSourceFile))
                {
                    string strErr = string.Format("FileMoveToCompleteCST NG - Source File: {0} , It's not exist.  PordID:{1}, CassetteID:{2}, MesTrxID:{3}", strSourceFile, strPordID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                //Move File
                File.Delete(strTargetFile);
                File.Move(strSourceFile, strTargetFile);
                #endregion

                strDescription = "FileMoveToCompleteCST OK";
                return true;
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("FileMoveToCompleteCST NG - Exception Message:{0} ,PordID:{1}, CassetteID:{2}, MesTrxID:{3}", ex.Message, strPordID, strCassetteID, strMesTrxID);
                return false;
            }
        }

        /// <summary> 
        /// FileMoveToIncompleteCST - [LotEndExecute] -> [IncompleteCST]
        /// 描述：檔案搬移作業。
        /// 1. 從 LotEndExecute\ Folder 中，將 Xml 文件搬移至 IncompleteCST\yyyyMMdd\ Folder 內。
        /// 2. 尋找目標 Xml 文件的檔名格式為 Pord ID + Cassette ID + Trx ID。
        /// 3. 若目標位置有相同的檔案，會將其覆蓋。
        /// </summary>
        /// <param name="strPordID">Pord ID</param>
        /// <param name="strCassetteID">Cassette ID</param>
        /// <param name="strMesTrxID">MES Trx ID - [yyyyMMddhhmmssfff]</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <returns></returns>
        public bool FileMoveToIncompleteCST(string strPordID, string strCassetteID, string strMesTrxID, out string strDescription, string strFileType = "")
        {
            try
            {
                string strDate = strMesTrxID.Substring(0, 8);

                #region [Check CassetteManager Path]
                StringBuilder sb = new StringBuilder();
                if (string.IsNullOrEmpty(this.LotEndExecuteCSTPath))
                    sb.Append("[CassetteManager.LotEndExecuteCSTPath is null or empty! - core.xml] ");
                if (string.IsNullOrEmpty(this.IncompleteCSTPath))
                    sb.Append("[CassetteManager.IncompleteCSTPath is null or empty! - core.xml] ");
                if (sb.Length > 0)
                {
                    string strErr = string.Format("FileMoveToIncompleteCST NG - {0} , PordID:{1}, CassetteID:{2}, MesTrxID:{3}", sb.ToString(), strPordID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                if (this.LotEndExecuteCSTPath[this.LotEndExecuteCSTPath.Length - 1] != '\\')
                    this.LotEndExecuteCSTPath += "\\";
                if (this.IncompleteCSTPath[this.IncompleteCSTPath.Length - 1] != '\\')
                    this.IncompleteCSTPath += "\\";
                #endregion

                #region [Check File Name & Format]

                StringBuilder sbErr = new StringBuilder();
                if (string.IsNullOrEmpty(strPordID.Trim()))
                    sbErr.Append("[PordID is null or empty] ");
                if (string.IsNullOrEmpty(strCassetteID.Trim()))
                    sbErr.Append("[CassetteID is null or empty] ");
                if (string.IsNullOrEmpty(strMesTrxID.Trim()))
                    sbErr.Append("[MesTrxID is null or empty] ");
                if (sbErr.Length > 0)
                {
                    string strErr = string.Format("FileMoveToIncompleteCST NG - Parameters Error - {0}", sbErr.ToString());
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }

                strPordID = strPordID.Trim();
                strCassetteID = strCassetteID.Trim();
                strMesTrxID = strMesTrxID.Trim();

                string strFileName = string.Format("{0}_{1}_{2}.xml", strFileType + strPordID, strCassetteID, strMesTrxID);
                string strTargetPath = string.Format("{0}{1}\\", this.IncompleteCSTPath, strDate);
                string strSourceFile = string.Format("{0}{1}", this.LotEndExecuteCSTPath, strFileName);
                string strTargetFile = string.Format("{0}{1}", strTargetPath, strFileName);
                #endregion

                #region [File Process]
                //Create Folder
                if (Directory.Exists(strTargetPath) == false)
                    Directory.CreateDirectory(strTargetPath);
                //Check File Exists
                if (!File.Exists(strSourceFile))
                {
                    string strErr = string.Format("FileMoveToIncompleteCST NG - Source File: {0} , It's not exist. PordID:{1}, CassetteID:{2}, MesTrxID:{3}", strSourceFile, strPordID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                //Move File
                File.Delete(strTargetFile);
                File.Move(strSourceFile, strTargetFile);
                #endregion

                strDescription = "FileMoveToIncompleteCST - OK";
                return true;
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("FileMoveToIncompleteCST NG - Exception Message:{0} ,PordID:{1}, CassetteID:{2}, MesTrxID:{3}", ex.Message, strPordID, strCassetteID, strMesTrxID);
                return false;
            }
        }

        /// <summary>
        /// IncompleteCassetteDataReplyFull - [IncompleteCST] Return XmlDocument
        /// 描述：IncompleteCST Folder 下的 Xml 資料回傳作業。
        /// </summary>
        /// <param name="strXmlFileName"> XML 文件名稱 - 格式:[Name or Name.xml]</param>
        /// <param name="strDate">XML 文件所屬的資料夾名稱 - 格式:[yyyyMMdd]</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <param name="xmlDocument">接收回傳的 XmlDocument 物件</param>1
        /// <returns></returns>
        public bool IncompleteCassetteDataReplyFull(string strXmlFileName, string strDate, out string strDescription, out XmlDocument xmlDocument)
        {
            try
            {
                #region [check CassetteManager path]
                if (string.IsNullOrEmpty(this.IncompleteCSTPath))
                {
                    string strErr = string.Format("IncompleteCassetteDataReplyFull NG - CassetteManager.IncompleteCSTPath is null or empty! - core.xml , XmlFileName:{0}, Date:{1}", strXmlFileName, strDate);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                if (this.IncompleteCSTPath[this.IncompleteCSTPath.Length - 1] != '\\')
                    this.IncompleteCSTPath += "\\";
                #endregion

                #region [check parameters]
                StringBuilder sbDescription = new StringBuilder();
                if (string.IsNullOrEmpty(strXmlFileName))
                    sbDescription.Append("[strXmlFileName is null or empty.] ");
                if (string.IsNullOrEmpty(strDate))
                    sbDescription.Append("[strDate is null or empty.] ");
                if (sbDescription.Length > 0)
                {
                    string strErr = string.Format("IncompleteCassetteDataReplyFull NG - Parameters Error - {0} , XmlFileName:{1}, Date:{2}", sbDescription.ToString(), strXmlFileName, strDate);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                strXmlFileName = strXmlFileName.Trim();
                strDate = strDate.Trim();
                #endregion

                #region [file name format]
                string _strXmlFileName = strXmlFileName;
                if (strXmlFileName.Length <= 4 || strXmlFileName.Substring(strXmlFileName.Length - 4, 4).ToLower() != ".xml")
                    _strXmlFileName += ".xml";
                string strFilePath = string.Format("{0}{1}\\{2}", this.IncompleteCSTPath, strDate, _strXmlFileName);
                #endregion

                #region [check file]
                if (!File.Exists(strFilePath))
                {
                    string strErr = string.Format("IncompleteCassetteDataReplyFull NG - File is not Exist. XmlFileName:{0}, Date:{1}", strXmlFileName, strDate);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                #endregion

                //讀取 Incomplete path 中指定的檔案，並回傳 XmlDocument ]
                xmlDocument = new XmlDocument();
                xmlDocument.Load(strFilePath);
                strDescription = "IncompleteCassetteDataReplyFull OK";
                return true;
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("IncompleteCassetteDataReplyFull NG - Exception Message:{0} , XmlFileName:{1}, Date:{2}", ex.Message, strXmlFileName, strDate);
                xmlDocument = null;
                return false;
            }
        }

        /// <summary>
        /// IncompleteCassetteDataReply - [IncompleteCST] Return XmlDocument
        /// 描述：IncompleteCST Folder 下的 Xml 資料回傳作業。
        /// </summary>
        /// <param name="strPortID">Pord ID</param>
        /// <param name="strCassetteID">Cassette ID</param>
        /// <param name="strMesTrxID">MES Trx ID - [yyyyMMddhhmmssfff]</param>
        /// <param name="strXmlFileName"> XML 文件名稱 - 格式:[Name or Name.xml]</param>
        /// <param name="strDate">XML 文件所屬的資料夾名稱 - 格式:[yyyyMMdd]</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <param name="xmlDocument">OPI XML - IncompleteCassetteDataReply</param>
        /// <returns></returns>
        public bool IncompleteCassetteDataReply(string strPortID, string strCassetteID, string strMesTrxID, string strXmlFileName, string strDate, out string strDescription, out XmlDocument xmlDocument)
        {
            try
            {
                strDate = strMesTrxID.Substring(0, 8);

                #region [check CassetteManager path]
                if (string.IsNullOrEmpty(this.IncompleteCSTPath))
                {
                    string strErr = string.Format("IncompleteCassetteDataReply NG - IncompleteCSTPath_Error - [CassetteManager.IncompleteCSTPath is null or empty. PortID:{0}, CassetteID:{1}, MesTrxID:{2}, XmlFileName:{3}, Date:{4}]"
                        , strPortID, strCassetteID, strMesTrxID, strXmlFileName, strDate);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                if (this.IncompleteCSTPath[this.IncompleteCSTPath.Length - 1] != '\\')
                    this.IncompleteCSTPath += "\\";

                //this.IncompleteCSTPath = @"D:\XML\CSOT_XML\IncompleteCST\"; //測試用
                #endregion

                #region [check parameters]
                StringBuilder sbDescription = new StringBuilder();
                if (string.IsNullOrEmpty(strPortID))
                    sbDescription.Append("[strPordID is null or empty.] ");
                if (string.IsNullOrEmpty(strCassetteID))
                    sbDescription.Append("[strCassetteID is null or empty.] ");
                if (string.IsNullOrEmpty(strMesTrxID))
                    sbDescription.Append("[strMesTrxID is null or empty.] ");
                if (string.IsNullOrEmpty(strXmlFileName))
                    sbDescription.Append("[strFileName is null or empty.] ");
                if (string.IsNullOrEmpty(strDate))
                    sbDescription.Append("[strDate is null or empty.] ");
                if (sbDescription.Length > 0)
                {
                    string strErr = string.Format("IncompleteCassetteDataReply NG - Parameters_Error - {0} ,PortID:{1}, CassetteID:{2}, MesTrxID:{3}, XmlFileName:{4}, Date:{5}"
                        , sbDescription.ToString(), strPortID, strCassetteID, strMesTrxID, strXmlFileName, strDate);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                strPortID = strPortID.Trim();
                strCassetteID = strCassetteID.Trim();
                strMesTrxID = strMesTrxID.Trim();
                strXmlFileName = strXmlFileName.Trim();
                strDate = strDate.Trim();
                #endregion

                #region [file name format]
                string _strXmlFileName = strXmlFileName;
                if (strXmlFileName.Length <= 4 || strXmlFileName.Substring(strXmlFileName.Length - 4, 4).ToLower() != ".xml")
                    _strXmlFileName += ".xml";
                string strFilePath = string.Format("{0}{1}\\{2}", this.IncompleteCSTPath, strDate, _strXmlFileName);
                 #endregion

                #region [check file]
                if (!File.Exists(strFilePath))
                {
                    string strErr = string.Format("IncompleteCassetteDataReply NG - File_Error - [File is not exist in =[{0}],PortID:{1}, CassetteID:{2}, MesTrxID:{3}, XmlFileName:{4}, Date:{5}]"
                        , strFilePath, strPortID, strCassetteID, strMesTrxID, strXmlFileName, strDate);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                #endregion

                #region [讀取 Incomplete path 中指定的檔案，並結創建回傳的 XmlDocument 結構]
                //建立要回傳的 XmlDocument
                XmlDocument xmlDocumentReturn = new XmlDocument();
                XmlDeclaration xmlDeclaration = xmlDocumentReturn.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDocumentReturn.AppendChild(xmlDeclaration);

                //建立根目錄
                XmlElement xmlElmRoot = xmlDocumentReturn.CreateElement(keyHost.MESSAGE);
                xmlDocumentReturn.AppendChild(xmlElmRoot);

                //取得 Incomplete path Xml 檔案
                XmlDocument xmlDocumentSource = new XmlDocument();
                xmlDocumentSource.Load(strFilePath);
                XmlNode xmlNodeBody = xmlDocumentSource.SelectSingleNode(string.Format("{0}/{1}", keyHost.MESSAGE, keyHost.BODY));

                XmlElement xmlEmtLineKey = xmlDocumentReturn.CreateElement(keyHost.LINENAME);
                xmlEmtLineKey.InnerText = xmlNodeBody.SelectSingleNode(keyHost.LINENAME).InnerText;
                xmlElmRoot.AppendChild(xmlEmtLineKey);

                XmlElement xmlEmtIncompletedate = xmlDocumentReturn.CreateElement(keyHost.INCOMPLETEDATE);
                xmlEmtIncompletedate.InnerText = strDate;
                xmlElmRoot.AppendChild(xmlEmtIncompletedate);

                XmlElement xmlEmtPortID = xmlDocumentReturn.CreateElement(keyHost.PORTID);
                xmlEmtPortID.InnerText = strPortID;
                xmlElmRoot.AppendChild(xmlEmtPortID);

                XmlElement xmlEmtCassetteID = xmlDocumentReturn.CreateElement(keyHost.CASSETTEID);
                xmlEmtCassetteID.InnerText = strCassetteID;
                xmlElmRoot.AppendChild(xmlEmtCassetteID);

                XmlElement xmlEmtCarrierName = xmlDocumentReturn.CreateElement(keyHost.CARRIERNAME);
                xmlEmtCarrierName.InnerText = xmlNodeBody.SelectSingleNode(keyHost.CARRIERNAME).InnerText;
                xmlElmRoot.AppendChild(xmlEmtCarrierName);

                XmlElement xmlEmtLineRecipeName = xmlDocumentReturn.CreateElement(keyHost.LINERECIPENAME);
                xmlEmtLineRecipeName.InnerText = xmlNodeBody.SelectSingleNode(keyHost.LINERECIPENAME).InnerText;
                xmlElmRoot.AppendChild(xmlEmtLineRecipeName);

                XmlElement xmlEmtHostLineRecipeName = xmlDocumentReturn.CreateElement(keyHost.HOSTLINERECIPENAME);
                xmlEmtHostLineRecipeName.InnerText = xmlNodeBody.SelectSingleNode(keyHost.HOSTLINERECIPENAME).InnerText;
                xmlElmRoot.AppendChild(xmlEmtHostLineRecipeName);

                XmlElement xmlEmtPPId = xmlDocumentReturn.CreateElement(keyHost.PPID);
                xmlEmtPPId.InnerText = xmlNodeBody.SelectSingleNode(keyHost.PPID).InnerText;
                xmlElmRoot.AppendChild(xmlEmtPPId);

                XmlElement xmlEmtHostPPid = xmlDocumentReturn.CreateElement(keyHost.HOSTPPID);
                xmlEmtHostPPid.InnerText = xmlNodeBody.SelectSingleNode(keyHost.HOSTPPID).InnerText;
                xmlElmRoot.AppendChild(xmlEmtHostPPid);

                XmlElement xmlEmtReturnMsg = xmlDocumentReturn.CreateElement(keyHost.RETURNMSG);
                string mes_return_code = xmlDocumentSource.SelectSingleNode(string.Format("{0}/{1}/{2}", keyHost.MESSAGE, keyHost.RETURN, keyHost.RETURNCODE)).InnerText;
                string mes_return_msg = xmlDocumentSource.SelectSingleNode(string.Format("{0}/{1}/{2}", keyHost.MESSAGE, keyHost.RETURN, keyHost.RETURNMESSAGE)).InnerText;
                xmlEmtReturnMsg.InnerText = string.Format("[{0}]{1}", mes_return_code, mes_return_msg);
                xmlElmRoot.AppendChild(xmlEmtReturnMsg);

                XmlElement xmlEmtMesTrxID = xmlDocumentReturn.CreateElement(keyHost.MESTRXID);
                xmlEmtMesTrxID.InnerText = strMesTrxID;
                xmlElmRoot.AppendChild(xmlEmtMesTrxID);

                XmlElement xmlEmtFileName = xmlDocumentReturn.CreateElement(keyHost.FILENAME);
                xmlEmtFileName.InnerText = strXmlFileName;
                xmlElmRoot.AppendChild(xmlEmtFileName);

                XmlElement xmlEmtIncompleteCassetteDataList = xmlDocumentReturn.CreateElement(keyHost.INCOMPLETECASSETTEDATALIST);
                xmlElmRoot.AppendChild(xmlEmtIncompleteCassetteDataList);

                XmlNodeList xnlProduct = xmlNodeBody.SelectNodes(string.Format("{0}/{1}", keyHost.PRODUCTLIST, keyHost.PRODUCT));
                foreach (XmlNode xnProduct in xnlProduct)
                {
                    XmlElement xmlProduct = xmlDocumentReturn.CreateElement(keyHost.PRODUCT);
                    xmlEmtIncompleteCassetteDataList.AppendChild(xmlProduct);

                    XmlElement xmlEmtPosition = xmlDocumentReturn.CreateElement(keyHost.POSITION);
                    xmlEmtPosition.InnerText = xnProduct.SelectSingleNode(keyHost.POSITION).InnerText;
                    xmlProduct.AppendChild(xmlEmtPosition);

                    XmlElement xmlEmtProductName= xmlDocumentReturn.CreateElement(keyHost.PRODUCTNAME);
                    xmlEmtProductName.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTNAME).InnerText;
                    xmlProduct.AppendChild(xmlEmtProductName);

                    XmlElement xmlEmtHostProductName = xmlDocumentReturn.CreateElement(keyHost.HOSTPRODUCTNAME);
                    xmlEmtHostProductName.InnerText = xnProduct.SelectSingleNode(keyHost.HOSTPRODUCTNAME).InnerText;
                    xmlProduct.AppendChild(xmlEmtHostProductName);

                    XmlElement xmlEmtDenseBoxID = xmlDocumentReturn.CreateElement(keyHost.DENSEBOXID);
                    xmlEmtDenseBoxID.InnerText = xnProduct.SelectSingleNode(keyHost.DENSEBOXID).InnerText;
                    xmlProduct.AppendChild(xmlEmtDenseBoxID);

                    XmlElement xmlEmtProductJudge = xmlDocumentReturn.CreateElement(keyHost.PRODUCTJUDGE);
                    xmlEmtProductJudge.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTJUDGE).InnerText;
                    xmlProduct.AppendChild(xmlEmtProductJudge);

                    XmlElement xmlEmtProductGrade = xmlDocumentReturn.CreateElement(keyHost.PRODUCTGRADE);
                    xmlEmtProductGrade.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTGRADE).InnerText;
                    xmlProduct.AppendChild(xmlEmtProductGrade);

                    XmlElement xmlEmtSubProductGrades = xmlDocumentReturn.CreateElement(keyHost.SUBPRODUCTGRADES);
                    xmlEmtSubProductGrades.InnerText = xnProduct.SelectSingleNode(keyHost.SUBPRODUCTGRADES).InnerText;
                    xmlProduct.AppendChild(xmlEmtSubProductGrades);

                    XmlElement xmlEmtPairProductName = xmlDocumentReturn.CreateElement(keyHost.PAIRPRODUCTNAME);
                    xmlEmtPairProductName.InnerText = xnProduct.SelectSingleNode(keyHost.PAIRPRODUCTNAME).InnerText;
                    xmlProduct.AppendChild(xmlEmtPairProductName);

                    XmlElement xmlEmtLotName = xmlDocumentReturn.CreateElement(keyHost.LOTNAME);
                    xmlEmtLotName.InnerText = xnProduct.SelectSingleNode(keyHost.LOTNAME).InnerText;
                    xmlProduct.AppendChild(xmlEmtLotName);

                    XmlElement xmlEmtProductRecipeName = xmlDocumentReturn.CreateElement(keyHost.PRODUCTRECIPENAME);
                    xmlEmtProductRecipeName.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTRECIPENAME).InnerText;
                    xmlProduct.AppendChild(xmlEmtProductRecipeName);

                    XmlElement xmlEmtHostProductRecipeName = xmlDocumentReturn.CreateElement(keyHost.HOSTPRODUCTRECIPENAME);
                    xmlEmtHostProductRecipeName.InnerText = xnProduct.SelectSingleNode(keyHost.HOSTPRODUCTRECIPENAME).InnerText;
                    xmlProduct.AppendChild(xmlEmtHostProductRecipeName);

                    XmlElement xmlEmtProductSpecName = xmlDocumentReturn.CreateElement(keyHost.PRODUCTSPECNAME);
                    xmlEmtProductSpecName.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTSPECNAME).InnerText;
                    xmlProduct.AppendChild(xmlEmtProductSpecName);

                    XmlElement xmlEmtProcessOperationName = xmlDocumentReturn.CreateElement(keyHost.PROCESSOPERATIONNAME);
                    xmlEmtProcessOperationName.InnerText = xnProduct.SelectSingleNode(keyHost.PROCESSOPERATIONNAME).InnerText;
                    xmlProduct.AppendChild(xmlEmtProcessOperationName);

                    XmlElement xmlEmtProductOwner = xmlDocumentReturn.CreateElement(keyHost.PRODUCTOWNER);
                    xmlEmtProductOwner.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTOWNER).InnerText;
                    xmlProduct.AppendChild(xmlEmtProductOwner);

                    XmlElement xmlEmtVcrreadFlag = xmlDocumentReturn.CreateElement(keyHost.VCRREADFLAG);
                    xmlEmtVcrreadFlag.InnerText = xnProduct.SelectSingleNode(keyHost.VCRREADFLAG).InnerText;
                    xmlProduct.AppendChild(xmlEmtVcrreadFlag);

                    XmlElement xmlEmtShortCutFlag = xmlDocumentReturn.CreateElement(keyHost.SHORTCUTFLAG);
                    xmlEmtShortCutFlag.InnerText = xnProduct.SelectSingleNode(keyHost.SHORTCUTFLAG).InnerText;
                    xmlProduct.AppendChild(xmlEmtShortCutFlag);

                    XmlElement xmlEmtSampleFlag = xmlDocumentReturn.CreateElement(keyHost.SAMPLEFLAG);
                    xmlEmtSampleFlag.InnerText = xnProduct.SelectSingleNode(keyHost.SAMPLEFLAG).InnerText;
                    xmlProduct.AppendChild(xmlEmtSampleFlag);

                    XmlElement xmlEmtProcessFlag = xmlDocumentReturn.CreateElement(keyHost.PROCESSFLAG);
                    xmlEmtProcessFlag.InnerText = xnProduct.SelectSingleNode(keyHost.PROCESSFLAG).InnerText;
                    xmlProduct.AppendChild(xmlEmtProcessFlag);

                    XmlElement xmlEmtProcessCommunicationSate = xmlDocumentReturn.CreateElement(keyHost.PROCESSCOMMUNICATIONSTATE);
                    xmlEmtProcessCommunicationSate.InnerText = xnProduct.SelectSingleNode(keyHost.PROCESSCOMMUNICATIONSTATE).InnerText;
                    xmlProduct.AppendChild(xmlEmtProcessCommunicationSate);
                }
                #endregion

                //設定回傳值
                xmlDocument = xmlDocumentReturn;
                strDescription = "IncompleteCassetteDataReply OK";
                return true;
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("IncompleteCassetteDataReply NG - Exception_Error - [Message:{0} ,PortID:{1}, CassetteID:{2}, MesTrxID:{3}, XmlFileName:{4}, Date:{5}]"
                    , ex.Message, strPortID, strCassetteID, strMesTrxID, strXmlFileName, strDate);
                xmlDocument = null;
                return false;
            }
        }

        /// <summary>
        /// IncompleteCassetteEditSaveReply
        /// 描述: OPI 上傳要回存至 Incomplete Xml 檔案的 XmlDocument 資料。
        /// </summary>
        /// <param name="strXmlFileName"> XML 文件名稱 - 格式:[Name or Name.xml]</param>
        /// <param name="strDate">XML 文件所屬的資料夾名稱 - 格式:[yyyyMMdd]</param>
        /// <param name="xmlDocument">OPI XML - IncompleteCassetteEditSave</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <returns></returns>
        public bool IncompleteCassetteEditSaveReply(string strPortID, string strCassetteID, string strMesTrxID, string strXmlFileName, string strDate, XmlDocument xmlDocument, out string strDescription)
        {
            try
            {
                strDate = strMesTrxID.Substring(0, 8);

                #region [check CassetteManager path]
                if (string.IsNullOrEmpty(this.IncompleteCSTPath))
                {
                    string strErr = string.Format("IncompleteCassetteEditSaveReply NG - IncompleteCSTPath_Error - [CassetteManager.IncompleteCSTPath is null or empty. XmlFileName:{0}, Date:{1}, PortID:{2}, CassetteID:{3}, MesTrxID:{4}]"
                        , strXmlFileName, strDate, strPortID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                if (this.IncompleteCSTPath[this.IncompleteCSTPath.Length - 1] != '\\')
                    this.IncompleteCSTPath += "\\";
                #endregion

                #region [check parameters]
                StringBuilder sbDescription = new StringBuilder();
                if (string.IsNullOrEmpty(strPortID))
                    sbDescription.Append("[strPortID is null or empty.] ");
                if (string.IsNullOrEmpty(strCassetteID))
                    sbDescription.Append("[strCassetteID is null or empty.] ");
                if (string.IsNullOrEmpty(strMesTrxID))
                    sbDescription.Append("[strMesTrxID is null or empty.] ");
                if (string.IsNullOrEmpty(strXmlFileName))
                    sbDescription.Append("[strFileName is null or empty.] ");
                if (string.IsNullOrEmpty(strDate))
                    sbDescription.Append("[strDate is null or empty.] ");
                if (sbDescription.Length > 0)
                {
                    string strErr = string.Format("IncompleteCassetteEditSaveReply NG - Parameters_Error - {0} , XmlFileName:{1}, Date:{2}, PortID:{3}, CassetteID:{4}, MesTrxID:{5}"
                        , sbDescription.ToString(), strXmlFileName, strDate, strPortID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                strPortID = strPortID.Trim();
                strCassetteID = strCassetteID.Trim();
                strMesTrxID = strMesTrxID.Trim();
                strXmlFileName = strXmlFileName.Trim();
                strDate = strDate.Trim();
                #endregion

                #region [file name format]
                string _strXmlFileName = strXmlFileName;
                if (strXmlFileName.Length <= 4 || strXmlFileName.Substring(strXmlFileName.Length - 4, 4).ToLower() != ".xml")
                    _strXmlFileName += ".xml";
                string strFilePath = string.Format("{0}{1}\\{2}", this.IncompleteCSTPath, strDate, _strXmlFileName);
                #endregion

                #region [check file]
                if (!File.Exists(strFilePath))
                {
                    string strErr = string.Format("IncompleteCassetteEditSaveReply NG - File_Error - [File is not exist in =[{0}],XmlFileName:{1}, Date:{2}, PortID:{3}, CassetteID:{4}, MesTrxID:{5}]"
                        , strFilePath, strXmlFileName, strDate, strPortID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                #endregion

                #region [讀取 Incomplete path 中指定的檔案，並結創建回傳的 XmlDocument 結構]
                //取得 Incomplete path Xml 檔案
                XmlDocument xmlDocumenIncomplete = new XmlDocument();
                xmlDocumenIncomplete.Load(strFilePath);
                XmlNode xmlNodeBodyIncomplete = xmlDocumenIncomplete.SelectSingleNode(string.Format("{0}/{1}", keyHost.MESSAGE, keyHost.BODY));
                XmlNode xmlNodeBody = xmlDocument.SelectSingleNode(string.Format("{0}/{1}", keyHost.MESSAGE, keyHost.BODY));

                xmlNodeBodyIncomplete.SelectSingleNode(keyHost.LINENAME).InnerText = xmlNodeBody.SelectSingleNode(keyHost.LINENAME).InnerText;
                xmlNodeBodyIncomplete.SelectSingleNode(keyHost.PORTNAME).InnerText = xmlNodeBody.SelectSingleNode(keyHost.PORTID).InnerText;
                xmlNodeBodyIncomplete.SelectSingleNode(keyHost.CARRIERNAME).InnerText = xmlNodeBody.SelectSingleNode(keyHost.CARRIERNAME).InnerText;
                xmlNodeBodyIncomplete.SelectSingleNode(keyHost.LINERECIPENAME).InnerText = xmlNodeBody.SelectSingleNode(keyHost.LINERECIPENAME).InnerText;
                xmlNodeBodyIncomplete.SelectSingleNode(keyHost.HOSTLINERECIPENAME).InnerText = xmlNodeBody.SelectSingleNode(keyHost.HOSTLINERECIPENAME).InnerText;

                xmlNodeBodyIncomplete.SelectSingleNode(keyHost.PPID).InnerText = xmlNodeBody.SelectSingleNode(keyHost.PPID).InnerText;
                xmlNodeBodyIncomplete.SelectSingleNode(keyHost.HOSTPPID).InnerText = xmlNodeBody.SelectSingleNode(keyHost.HOSTPPID).InnerText;

                XmlNodeList xnlProduct = xmlNodeBody.SelectNodes(string.Format("{0}/{1}", keyHost.INCOMPLETECASSETTEDATALIST, keyHost.PRODUCT));
                XmlNodeList xnlProductIncomplete = xmlNodeBodyIncomplete.SelectNodes(string.Format("{0}/{1}", keyHost.PRODUCTLIST, keyHost.PRODUCT));
                foreach (XmlNode xnProduct in xnlProduct)
                {
                    bool blFineNode = false;
                    foreach(XmlNode xnProductIncomplete in xnlProductIncomplete)
                    {
                        if (xnProduct.SelectSingleNode(keyHost.POSITION).InnerText == xnProductIncomplete.SelectSingleNode(keyHost.POSITION).InnerText)
                        {
                            xnProductIncomplete.SelectSingleNode(keyHost.PRODUCTNAME).InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTNAME).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.HOSTPRODUCTNAME).InnerText = xnProduct.SelectSingleNode(keyHost.HOSTPRODUCTNAME).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.DENSEBOXID).InnerText = xnProduct.SelectSingleNode(keyHost.DENSEBOXID).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.PRODUCTJUDGE).InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTJUDGE).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.PRODUCTGRADE).InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTGRADE).InnerText;

                            xnProductIncomplete.SelectSingleNode(keyHost.SUBPRODUCTGRADES).InnerText = xnProduct.SelectSingleNode(keyHost.SUBPRODUCTGRADES).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.PAIRPRODUCTNAME).InnerText = xnProduct.SelectSingleNode(keyHost.PAIRPRODUCTNAME).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.LOTNAME).InnerText = xnProduct.SelectSingleNode(keyHost.LOTNAME).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.PRODUCTRECIPENAME).InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTRECIPENAME).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.HOSTPRODUCTRECIPENAME).InnerText = xnProduct.SelectSingleNode(keyHost.HOSTPRODUCTRECIPENAME).InnerText;

                            xnProductIncomplete.SelectSingleNode(keyHost.PRODUCTSPECNAME).InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTSPECNAME).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.PROCESSOPERATIONNAME).InnerText = xnProduct.SelectSingleNode(keyHost.PROCESSOPERATIONNAME).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.PRODUCTOWNER).InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTOWNER).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.VCRREADFLAG).InnerText = xnProduct.SelectSingleNode(keyHost.VCRREADFLAG).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.SHORTCUTFLAG).InnerText = xnProduct.SelectSingleNode(keyHost.SHORTCUTFLAG).InnerText;

                            xnProductIncomplete.SelectSingleNode(keyHost.SAMPLEFLAG).InnerText = xnProduct.SelectSingleNode(keyHost.SAMPLEFLAG).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.PROCESSFLAG).InnerText = xnProduct.SelectSingleNode(keyHost.PROCESSFLAG).InnerText;
                            xnProductIncomplete.SelectSingleNode(keyHost.PROCESSCOMMUNICATIONSTATE).InnerText = xnProduct.SelectSingleNode(keyHost.PROCESSCOMMUNICATIONSTATE).InnerText;
                            
                            blFineNode = true;
                            break;
                        }
                    }

                    //判斷若在 Incomplete Cassette File 中找不到相對應的 Product ，就將此 Product 添加進去。
                    if (!blFineNode)
                    {
                        #region [Add XmlElement & Set Values]
                        XmlNode xnlProductList = xmlNodeBodyIncomplete.SelectSingleNode(keyHost.PRODUCTLIST);
                        XmlElement xmlProduct = xmlDocumenIncomplete.CreateElement(keyHost.PRODUCT);
                        xnlProductList.AppendChild(xmlProduct);

                        XmlElement xmlEmtPosition = xmlDocumenIncomplete.CreateElement(keyHost.POSITION);
                        xmlEmtPosition.InnerText = xnProduct.SelectSingleNode(keyHost.POSITION).InnerText;
                        xmlProduct.AppendChild(xmlEmtPosition);

                        XmlElement xmlEmtProductName = xmlDocumenIncomplete.CreateElement(keyHost.PRODUCTNAME);
                        xmlEmtProductName.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTNAME).InnerText;
                        xmlProduct.AppendChild(xmlEmtProductName);

                        XmlElement xmlEmtHostProductName = xmlDocumenIncomplete.CreateElement(keyHost.HOSTPRODUCTNAME);
                        xmlEmtHostProductName.InnerText = xnProduct.SelectSingleNode(keyHost.HOSTPRODUCTNAME).InnerText;
                        xmlProduct.AppendChild(xmlEmtHostProductName);

                        XmlElement xmlEmtDenseBoxID = xmlDocumenIncomplete.CreateElement(keyHost.DENSEBOXID);
                        xmlEmtDenseBoxID.InnerText = xnProduct.SelectSingleNode(keyHost.DENSEBOXID).InnerText;
                        xmlProduct.AppendChild(xmlEmtDenseBoxID);

                        XmlElement xmlEmtProductJudge = xmlDocumenIncomplete.CreateElement(keyHost.PRODUCTJUDGE);
                        xmlEmtProductJudge.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTJUDGE).InnerText;
                        xmlProduct.AppendChild(xmlEmtProductJudge);

                        XmlElement xmlEmtProductGrade = xmlDocumenIncomplete.CreateElement(keyHost.PRODUCTGRADE);
                        xmlEmtProductGrade.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTGRADE).InnerText;
                        xmlProduct.AppendChild(xmlEmtProductGrade);

                        XmlElement xmlEmtSubProductGrades = xmlDocumenIncomplete.CreateElement(keyHost.SUBPRODUCTGRADES);
                        xmlEmtSubProductGrades.InnerText = xnProduct.SelectSingleNode(keyHost.SUBPRODUCTGRADES).InnerText;
                        xmlProduct.AppendChild(xmlEmtSubProductGrades);

                        XmlElement xmlEmtPairProductName = xmlDocumenIncomplete.CreateElement(keyHost.PAIRPRODUCTNAME);
                        xmlEmtPairProductName.InnerText = xnProduct.SelectSingleNode(keyHost.PAIRPRODUCTNAME).InnerText;
                        xmlProduct.AppendChild(xmlEmtPairProductName);

                        XmlElement xmlEmtLotName = xmlDocumenIncomplete.CreateElement(keyHost.LOTNAME);
                        xmlEmtLotName.InnerText = xnProduct.SelectSingleNode(keyHost.LOTNAME).InnerText;
                        xmlProduct.AppendChild(xmlEmtLotName);

                        XmlElement xmlEmtProductRecipeName = xmlDocumenIncomplete.CreateElement(keyHost.PRODUCTRECIPENAME);
                        xmlEmtProductRecipeName.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTRECIPENAME).InnerText;
                        xmlProduct.AppendChild(xmlEmtProductRecipeName);

                        XmlElement xmlEmtHostProductRecipeName = xmlDocumenIncomplete.CreateElement(keyHost.HOSTPRODUCTRECIPENAME);
                        xmlEmtHostProductRecipeName.InnerText = xnProduct.SelectSingleNode(keyHost.HOSTPRODUCTRECIPENAME).InnerText;
                        xmlProduct.AppendChild(xmlEmtHostProductRecipeName);

                        XmlElement xmlEmtProductSpecName = xmlDocumenIncomplete.CreateElement(keyHost.PRODUCTSPECNAME);
                        xmlEmtProductSpecName.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTSPECNAME).InnerText;
                        xmlProduct.AppendChild(xmlEmtProductSpecName);

                        XmlElement xmlEmtProcessOperationName = xmlDocumenIncomplete.CreateElement(keyHost.PROCESSOPERATIONNAME);
                        xmlEmtProcessOperationName.InnerText = xnProduct.SelectSingleNode(keyHost.PROCESSOPERATIONNAME).InnerText;
                        xmlProduct.AppendChild(xmlEmtProcessOperationName);

                        XmlElement xmlEmtProductOwner = xmlDocumenIncomplete.CreateElement(keyHost.PRODUCTOWNER);
                        xmlEmtProductOwner.InnerText = xnProduct.SelectSingleNode(keyHost.PRODUCTOWNER).InnerText;
                        xmlProduct.AppendChild(xmlEmtProductOwner);

                        XmlElement xmlEmtVcrreadFlag = xmlDocumenIncomplete.CreateElement(keyHost.VCRREADFLAG);
                        xmlEmtVcrreadFlag.InnerText = xnProduct.SelectSingleNode(keyHost.VCRREADFLAG).InnerText;
                        xmlProduct.AppendChild(xmlEmtVcrreadFlag);

                        XmlElement xmlEmtShortCutFlag = xmlDocumenIncomplete.CreateElement(keyHost.SHORTCUTFLAG);
                        xmlEmtShortCutFlag.InnerText = xnProduct.SelectSingleNode(keyHost.SHORTCUTFLAG).InnerText;
                        xmlProduct.AppendChild(xmlEmtShortCutFlag);

                        XmlElement xmlEmtSampleFlag = xmlDocumenIncomplete.CreateElement(keyHost.SAMPLEFLAG);
                        xmlEmtSampleFlag.InnerText = xnProduct.SelectSingleNode(keyHost.SAMPLEFLAG).InnerText;
                        xmlProduct.AppendChild(xmlEmtSampleFlag);

                        XmlElement xmlEmtProcessFlag = xmlDocumenIncomplete.CreateElement(keyHost.PROCESSFLAG);
                        xmlEmtProcessFlag.InnerText = xnProduct.SelectSingleNode(keyHost.PROCESSFLAG).InnerText;
                        xmlProduct.AppendChild(xmlEmtProcessFlag);

                        XmlElement xmlEmtProcessCommunicationSate = xmlDocumenIncomplete.CreateElement(keyHost.PROCESSCOMMUNICATIONSTATE);
                        xmlEmtProcessCommunicationSate.InnerText = xnProduct.SelectSingleNode(keyHost.PROCESSCOMMUNICATIONSTATE).InnerText;
                        xmlProduct.AppendChild(xmlEmtProcessCommunicationSate);
                        #endregion
                    }
                }
                #endregion

                xmlDocumenIncomplete.Save(strFilePath);
                strDescription = "IncompleteCassetteEditSaveReply OK";
                return true;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("IncompleteCassetteEditSaveReply NG - Exception_Error - [Message:{0} ,XmlFileName:{1}, Date:{2}, PortID:{3}, CassetteID:{4}, MesTrxID:{5}]"
                    , ex.Message, strXmlFileName, strDate, strPortID, strCassetteID, strMesTrxID);
                return false;
            }
        }

        /// <summary>
        /// IncompleteCassetteCommandReply
        /// 描述: OPI 傳送指令，做 DELETE or RESEND 。
        /// </summary>
        /// <param name="strCommand">Command [ DELETE | RESEND ]</param>
        /// <param name="strPortID">Pord ID</param>
        /// <param name="strCassetteID">Cassette ID</param>
        /// <param name="strMesTrxID">MES Trx ID - [yyyyMMddhhmmssfff]</param>
        /// <param name="State">SBCS_INCOMPLETECST.STATE 的狀態資料 - [ OK | NG | CLOSE ]</param>
        /// <param name="strXmlFileName"> XML 文件名稱 - 格式:[Name or Name.xml]</param>
        /// <param name="strDate">XML 文件所屬的資料夾名稱 - 格式:[yyyyMMdd]</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <returns></returns>
        public bool IncompleteCassetteCommandReply(string strCommand, string strPortID, string strCassetteID, string strMesTrxID, string strXmlFileName, string strDate, out string strDescription, out XmlDocument xmlDocument)
        {
            try
            {
                strDate = strMesTrxID.Substring(0, 8);

                #region [Check CassetteManager Path]
                StringBuilder sbMsg = new StringBuilder();
                if (string.IsNullOrEmpty(this.IncompleteCSTPath))
                    sbMsg.Append("[CassetteManager.IncompleteCSTPath is null or empty.]");
                if (string.IsNullOrEmpty(this.CompleteCSTPath))
                    sbMsg.Append("[CassetteManager.CompleteCSTPath is null or empty.]");
                if (string.IsNullOrEmpty(this.LotEndExecuteCSTPath))
                    sbMsg.Append("[CassetteManager.LotEndExecuteCSTPath is null or empty.]");
                if (sbMsg.Length > 0)
                {
                    string strErr = string.Format("IncompleteCassetteCommandReply NG - Path_Error - {0} , Command:{1}, PortID:{2}, CassetteID:{3}, MesTrxID:{4}"
                        , sbMsg.ToString(), strCommand, strPortID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                if (this.IncompleteCSTPath[this.IncompleteCSTPath.Length - 1] != '\\')
                    this.IncompleteCSTPath += "\\";
                if (this.CompleteCSTPath[this.CompleteCSTPath.Length - 1] != '\\')
                    this.CompleteCSTPath += "\\";
                if (this.LotEndExecuteCSTPath[this.LotEndExecuteCSTPath.Length - 1] != '\\')
                    this.LotEndExecuteCSTPath += "\\";
                #endregion

                #region [Check Parameters]
                if (string.IsNullOrEmpty(strCommand))
                    sbMsg.Append("[strCommand is null or empty.] ");
                if (string.IsNullOrEmpty(strPortID))
                    sbMsg.Append("[strPordID is null or empty.] ");
                if (string.IsNullOrEmpty(strCassetteID))
                    sbMsg.Append("[strCassetteID is null or empty.] ");
                if (string.IsNullOrEmpty(strMesTrxID))
                    sbMsg.Append("[strMesTrxID is null or empty.] ");
                if (string.IsNullOrEmpty(strXmlFileName))
                    sbMsg.Append("[strFileName is null or empty.] ");
                if (string.IsNullOrEmpty(strDate))
                    sbMsg.Append("[strDate is null or empty.] ");
                if (sbMsg.Length > 0)
                {
                    string strErr = string.Format("IncompleteCassetteCommandReply NG - Parameters_Error - {0} , Command:{1}, PortID:{2}, CassetteID:{3}, MesTrxID:{4}"
                        , sbMsg.ToString(), strCommand, strPortID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                strCommand = strCommand.Trim();
                strPortID = strPortID.Trim();
                strCassetteID = strCassetteID.Trim();
                strMesTrxID = strMesTrxID.Trim();
                strXmlFileName = strXmlFileName.Trim();
                strDate = strDate.Trim();
                #endregion

                #region [File Name Format]
                if (strXmlFileName.Length <= 4 || strXmlFileName.Substring(strXmlFileName.Length - 4, 4).ToLower() != ".xml")
                    strXmlFileName += ".xml";
                string strSourcePathFile = string.Format("{0}{1}\\{2}", this.IncompleteCSTPath, strDate, strXmlFileName);
                string strCompletePath = string.Format("{0}{1}\\", this.CompleteCSTPath, strDate);
                string strCompletePathFile = string.Format("{0}{1}", strCompletePath, strXmlFileName);
                string strExecutePathPathFile = string.Format("{0}{1}", this.LotEndExecuteCSTPath, strXmlFileName);
                #endregion

                #region [Check File]
                if (!File.Exists(strSourcePathFile))
                    sbMsg.Append(string.Format("[File is not exist in ({0})]", strSourcePathFile));
                if (sbMsg.Length > 0)
                {
                    string strErr = string.Format("IncompleteCassetteCommandReply NG - File_Error - =[{0}], Command:{1}, PortID:{2}, CassetteID:{3}, MesTrxID:{4}"
                        , sbMsg.ToString(), strCommand, strPortID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                #endregion

                #region [Move File  & Update DB]
                if (strCommand.ToUpper() == "DELETE")
                {
                    //Update DB
                    if (!UpdateIncompleteCassetteToDB(strPortID, strCassetteID, strMesTrxID, "CLOSE", out strDescription))
                    {
                        string strErr = string.Format("IncompleteCassetteCommandReply NG - UpdateDB_Error - [Message:{0} , Command:{1}, PortID:{2}, CassetteID:{3}, MesTrxID:{4} ] "
                            , strDescription, strCommand, strPortID, strCassetteID, strMesTrxID);
                        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                        strDescription = strErr;
                        xmlDocument = null;
                        return false;
                    }

                    //Create Folder
                    if (Directory.Exists(strCompletePath) == false)
                        Directory.CreateDirectory(strCompletePath);

                    //Move File
                    if (File.Exists(strCompletePathFile))
                        File.Delete(strCompletePathFile);
                    File.Move(strSourcePathFile, strCompletePathFile);
                    xmlDocument = null;
                }
                else if (strCommand.ToUpper() == "RESEND")
                {
                    //Create Folder
                    if (Directory.Exists(this.LotEndExecuteCSTPath) == false)
                        Directory.CreateDirectory(this.LotEndExecuteCSTPath);

                    //Move File
                    if (File.Exists(strExecutePathPathFile))
                        File.Delete(strExecutePathPathFile);
                    File.Move(strSourcePathFile, strExecutePathPathFile);

                    //Update Xml Return Code & Return Message
                    xmlDocument = new XmlDocument();
                    xmlDocument.Load(strExecutePathPathFile);
                    xmlDocument.SelectSingleNode(string.Format("{0}/{1}/{2}", keyHost.MESSAGE, keyHost.RETURN, keyHost.RETURNCODE)).InnerText = "0";
                    xmlDocument.SelectSingleNode(string.Format("{0}/{1}/{2}", keyHost.MESSAGE, keyHost.RETURN, keyHost.RETURNMESSAGE)).InnerText = string.Empty;
                    xmlDocument.Save(strExecutePathPathFile);
                }
                else
                {
                    string strErr = string.Format("IncompleteCassetteCommandReply NG - Command_Error - [Command's value is =[{0}], it's not equals (DELETE) or (RESEND).  PortID:{1}, CassetteID:{2}, MesTrxID:{3}]"
                        , strCommand, strPortID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                #endregion

                strDescription = "IncompleteCassetteCommandReply OK";
                return true;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("IncompleteCassetteCommandReply NG - Exception_Error - [Message:{0} , Command:{1}, PortID:{2}, CassetteID:{3}, MesTrxID:{4}]"
                    , ex.Message, strCommand, strPortID, strCassetteID, strMesTrxID);
                xmlDocument = null;
                return false;
            }
        }

        /// <summary>
        /// UpdateLotPorcessEndMesReplyToExecuteXml
        /// 描述: LotProcessEnd 根據 MES Reply 回覆的 XmlDocument 中的 Return Code 與 Return Message ，回存 Executing Folder 中相對應的 Xml File 中的 Return Code、Return Message
        /// </summary>
        /// <param name="strPortID">Pord ID</param>
        /// <param name="strCassetteID">Cassette ID</param>
        /// <param name="strMesTrxID">MES Trx ID - [yyyyMMddhhmmssfff]</param>
        /// <param name="strState">State - [ OK | NG ]</param>
        /// <param name="strReturnCode">MES Reply 回報的 Return Code</param>
        /// <param name="strReturnMessage">MES Reply 回報的 Return Message</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <returns></returns>
        public bool UpdateLotPorcessEndMesReplyToExecuteXmlAndDB(string strPortID, string strCassetteID, string strMesTrxID, string strState, string strReturnCode, string strReturnMessage, out string strDescription, string strFileType = "")
        {
            try
            {
                #region [Check CassetteManager Path
                if (string.IsNullOrEmpty(this.LotEndExecuteCSTPath))
                {
                    string strErr = string.Format("UpdateLotPorcessEndMesReplyToExecuteXmlAndDB NG - CassetteManager.LotEndExecuteCSTPath is null or empty! , PortID:{0}, CassetteID:{1}, MesTrxID:{2} State:{3} ReturnCode:{4} ReturnMessage:{5}",
                        strPortID, strCassetteID, strMesTrxID, strState, strReturnCode, strReturnMessage);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                if (this.LotEndExecuteCSTPath[this.LotEndExecuteCSTPath.Length - 1] != '\\')
                    this.LotEndExecuteCSTPath += "\\";
                #endregion

                #region [Check File]
                string strFilePath = string.Format("{0}{1}_{2}_{3}.xml", this.LotEndExecuteCSTPath, strFileType + strPortID, strCassetteID, strMesTrxID);
                if (!File.Exists(strFilePath))
                {
                    string strErr = string.Format("UpdateLotPorcessEndMesReplyToExecuteXmlAndDB NG - File_Error - [File is not exist in =[{0}], PortID:{1}, CassetteID:{2}, MesTrxID:{3} State:{4} ReturnCode:{5} ReturnMessage:{6}"
                        , strFilePath, strPortID, strCassetteID, strMesTrxID, strState, strReturnCode, strReturnMessage);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                #endregion

                #region [Check Parameters]
                StringBuilder sbMsg = new StringBuilder();
                if (string.IsNullOrEmpty(strPortID))
                    sbMsg.Append("[strPordID is null or empty.] ");
                if (string.IsNullOrEmpty(strCassetteID))
                    sbMsg.Append("[strCassetteID is null or empty.] ");
                if (string.IsNullOrEmpty(strMesTrxID))
                    sbMsg.Append("[strMesTrxID is null or empty.] ");
                if (string.IsNullOrEmpty(strState))
                    sbMsg.Append("[strState is null or empty.] ");
                if (sbMsg.Length > 0)
                {
                    string strErr = string.Format("IncompleteCassetteCommandReply NG - Parameters_Error - {0} , PortID:{1}, CassetteID:{2}, MesTrxID:{3}"
                        , sbMsg.ToString(), strPortID, strCassetteID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                strPortID = strPortID.Trim();
                strCassetteID = strCassetteID.Trim();
                strMesTrxID = strMesTrxID.Trim();
                strState = strState.Trim();
                #endregion

                #region [Update Executing Xml File Data]
                XmlDocument xDocument = new XmlDocument();
                xDocument.Load(strFilePath);
                xDocument.SelectSingleNode(string.Format("{0}/{1}/{2}", keyHost.MESSAGE, keyHost.RETURN, keyHost.RETURNCODE)).InnerText = strReturnCode;
                xDocument.SelectSingleNode(string.Format("{0}/{1}/{2}", keyHost.MESSAGE, keyHost.RETURN, keyHost.RETURNMESSAGE)).InnerText = strReturnMessage;  
                xDocument.Save(strFilePath);
                #endregion

                #region [Update DB INCOMPLETECST State]
                INCOMPLETECST inComplete = new INCOMPLETECST();
                IList list = HibernateAdapter.GetObjectByQuery(string.Format("from INCOMPLETECST where PORTID='{0}' and CASSETTEID='{1}' and MESTRXID='{2}'", strPortID, strCassetteID, strMesTrxID));
                if (strState.ToUpper() == "OK")
                {
                    if (list != null && list.Count > 0)
                    {
                        if (!UpdateIncompleteCassetteToDB(strPortID, strCassetteID, strMesTrxID, "OK", strReturnMessage, out strDescription))
                        {
                            string strErr = string.Format("UpdateLotPorcessEndMesReplyToExecuteXmlAndDB NG - UpdateDB_Error - [Message:{0} , PortID:{1}, CassetteID:{2}, MesTrxID:{3} State:{4} ReturnCode:{5} ReturnMessage:{6}]"
                                , strDescription, strPortID, strCassetteID, strMesTrxID, strState, strReturnCode, strReturnMessage);
                            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            strDescription = strErr;
                            return false;
                        }
                    }
                }
                else if (strState.ToUpper() == "NG")
                {
                    if (list != null && list.Count > 0)
                    {
                        if (!UpdateIncompleteCassetteToDB(strPortID, strCassetteID, strMesTrxID, "NG", strReturnMessage, out strDescription))
                        {
                            string strErr = string.Format("UpdateLotPorcessEndMesReplyToExecuteXmlAndDB NG - UpdateDB_Error - [Message:{0}] , PortID:{1}, CassetteID:{2}, MesTrxID:{3} State:{4} ReturnCode:{5} ReturnMessage:{6}"
                                , strDescription, strPortID, strCassetteID, strMesTrxID, strState, strReturnCode, strReturnMessage);
                            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            strDescription = strErr;
                            return false;
                        }
                    }
                    else
                    {
                        if (!SaveIncompleteCassetteToDB(strPortID, strCassetteID, strMesTrxID, 0, "NG", strReturnMessage, out strDescription, strFileType))
                        {
                            string strErr = string.Format("UpdateLotPorcessEndMesReplyToExecuteXmlAndDB NG - InsertDB_Error - [Message:{0}] , PortID:{1}, CassetteID:{2}, MesTrxID:{3} State:{4} ReturnCode:{5} ReturnMessage:{6}"
                                , strDescription, strPortID, strCassetteID, strMesTrxID, strState, strReturnCode, strReturnMessage);
                            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            strDescription = strErr;
                            return false;
                        }
                    }
                }
                else
                {
                    string strErr = string.Format("UpdateLotPorcessEndMesReplyToExecuteXmlAndDB NG - Command_Error - [State:{0} is not [OK] or [NG] , PortID:{1}, CassetteID:{2}, MesTrxID:{3} ReturnCode:{4} ReturnMessage:{5}]"
                                ,strState, strPortID, strCassetteID, strMesTrxID, strReturnCode, strReturnMessage);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                #endregion



                strDescription = "UpdateLotPorcessEndMesReplyToExecuteXmlAndDB OK";
                return true;
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("NG - Exception Message:{0} , PortID:{1}, CassetteID:{2}, MesTrxID:{3} State:{4} ReturnCode:{5} ReturnMessage:{6}"
                    , ex.Message, strPortID, strCassetteID, strMesTrxID, strState, strReturnCode, strReturnMessage);
                return false;
            }
        }

        /// <summary>
        /// IncompleteCassetteDataReply - [IncompleteCST] Return XmlDocument
        /// 描述：IncompleteCST Folder 下的 Xml 資料回傳作業。
        /// </summary>
        /// <param name="strPortID">Pord ID</param>
        /// <param name="strCassetteID">Cassette ID</param>
        /// <param name="strMesTrxID">MES Trx ID - [yyyyMMddhhmmssfff]</param>
        /// <param name="strXmlFileName"> XML 文件名稱 - 格式:[Name or Name.xml]</param>
        /// <param name="strDate">XML 文件所屬的資料夾名稱 - 格式:[yyyyMMdd]</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <param name="xmlDocument">MES XML - BoxProcessEnd</param>
        /// <returns></returns>
        public bool IncompleteBoxDataReply(string strPortID, string strMesTrxID, string strXmlFileName, string strDate, out string strDescription, out XmlDocument xmlDocument)
        {
            try
            {
                strDate = strMesTrxID.Substring(0, 8);

                #region [check CassetteManager path]
                if (string.IsNullOrEmpty(this.IncompleteCSTPath))
                {
                    string strErr = string.Format("IncompleteBoxDataReply NG - IncompleteCSTPath_Error - [CassetteManager.IncompleteCSTPath is null or empty. PortID:{0}, MesTrxID:{1}, XmlFileName:{2}, Date:{3}]"
                        , strPortID, strMesTrxID, strXmlFileName, strDate);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                if (this.IncompleteCSTPath[this.IncompleteCSTPath.Length - 1] != '\\')
                    this.IncompleteCSTPath += "\\";

                #endregion

                #region [check parameters]
                StringBuilder sbDescription = new StringBuilder();
                if (string.IsNullOrEmpty(strPortID))
                    sbDescription.Append("[strPordID is null or empty.] ");
                if (string.IsNullOrEmpty(strMesTrxID))
                    sbDescription.Append("[strMesTrxID is null or empty.] ");
                if (string.IsNullOrEmpty(strXmlFileName))
                    sbDescription.Append("[strFileName is null or empty.] ");
                if (string.IsNullOrEmpty(strDate))
                    sbDescription.Append("[strDate is null or empty.] ");
                if (sbDescription.Length > 0)
                {
                    string strErr = string.Format("IncompleteBoxDataReply NG - Parameters_Error - {0} ,PortID:{1}, MesTrxID:{2}, XmlFileName:{3}, Date:{4}"
                        , sbDescription.ToString(), strPortID, strMesTrxID, strXmlFileName, strDate);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                strPortID = strPortID.Trim();
                strMesTrxID = strMesTrxID.Trim();
                strXmlFileName = strXmlFileName.Trim();
                strDate = strDate.Trim();
                #endregion

                #region [file name format]
                string _strXmlFileName = strXmlFileName;
                if (strXmlFileName.Length <= 4 || strXmlFileName.Substring(strXmlFileName.Length - 4, 4).ToLower() != ".xml")
                    _strXmlFileName += ".xml";
                string strFilePath = string.Format("{0}{1}\\{2}", this.IncompleteCSTPath, strDate, _strXmlFileName);
                #endregion

                #region [check file]
                if (!File.Exists(strFilePath))
                {
                    string strErr = string.Format("IncompleteBoxDataReply NG - File_Error - [File is not exist in =[{0}],PortID:{1}, MesTrxID:{2}, XmlFileName:{3}, Date:{4}]"
                        , strFilePath, strPortID, strMesTrxID, strXmlFileName, strDate);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    xmlDocument = null;
                    return false;
                }
                #endregion

                xmlDocument = new XmlDocument();
                xmlDocument.Load(strFilePath);//MES XML - BoxProcessEnd
                strDescription = "IncompleteBoxDataReply OK";
                return true;
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("IncompleteBoxDataReply NG - Exception_Error - [Message:{0} ,PortID:{1}, MesTrxID:{2}, XmlFileName:{3}, Date:{4}]"
                    , ex.Message, strPortID, strMesTrxID, strXmlFileName, strDate);
                xmlDocument = null;
                return false;
            }
        }

        /// <summary>
        /// IncompleteBoxEditSaveReply
        /// 描述: OPI 上傳要回存至 Incomplete Xml 檔案的 XmlDocument 資料。
        /// </summary>
        /// <param name="strXmlFileName"> XML 文件名稱 - 格式:[Name or Name.xml]</param>
        /// <param name="strDate">XML 文件所屬的資料夾名稱 - 格式:[yyyyMMdd]</param>
        /// <param name="xmlDocument">MES XML - BoxProcessEnd</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <returns></returns>
        public bool IncompleteBoxEditSaveReply(string strPortID, string strMesTrxID, string strXmlFileName, string strDate, MesSpec.BoxProcessEnd.TrxBody boxProcessEndBody, out string strDescription)
        {
            try
            {
                strDate = strMesTrxID.Substring(0, 8);

                #region [check CassetteManager path]
                if (string.IsNullOrEmpty(this.IncompleteCSTPath))
                {
                    string strErr = string.Format("IncompleteBoxEditSaveReply NG - IncompleteCSTPath_Error - [CassetteManager.IncompleteCSTPath is null or empty. XmlFileName:{0}, Date:{1}, PortID:{2}, MesTrxID:{3}]"
                        , strXmlFileName, strDate, strPortID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                if (this.IncompleteCSTPath[this.IncompleteCSTPath.Length - 1] != '\\')
                    this.IncompleteCSTPath += "\\";
                #endregion

                #region [check parameters]
                StringBuilder sbDescription = new StringBuilder();
                if (string.IsNullOrEmpty(strPortID))
                    sbDescription.Append("[strPortID is null or empty.] ");
                if (string.IsNullOrEmpty(strMesTrxID))
                    sbDescription.Append("[strMesTrxID is null or empty.] ");
                if (string.IsNullOrEmpty(strXmlFileName))
                    sbDescription.Append("[strFileName is null or empty.] ");
                if (string.IsNullOrEmpty(strDate))
                    sbDescription.Append("[strDate is null or empty.] ");
                if (sbDescription.Length > 0)
                {
                    string strErr = string.Format("IncompleteBoxEditSaveReply NG - Parameters_Error - {0} , XmlFileName:{1}, Date:{2}, PortID:{3}, MesTrxID:{4}"
                        , sbDescription.ToString(), strXmlFileName, strDate, strPortID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                strPortID = strPortID.Trim();
                strMesTrxID = strMesTrxID.Trim();
                strXmlFileName = strXmlFileName.Trim();
                strDate = strDate.Trim();
                #endregion

                #region [file name format]
                string _strXmlFileName = strXmlFileName;
                if (strXmlFileName.Length <= 4 || strXmlFileName.Substring(strXmlFileName.Length - 4, 4).ToLower() != ".xml")
                    _strXmlFileName += ".xml";
                string strFilePath = string.Format("{0}{1}\\{2}", this.IncompleteCSTPath, strDate, _strXmlFileName);
                #endregion

                #region [check file]
                if (!File.Exists(strFilePath))
                {
                    string strErr = string.Format("IncompleteBoxEditSaveReply NG - File_Error - [File is not exist in =[{0}],XmlFileName:{1}, Date:{2}, PortID:{3}, MesTrxID:{4}]"
                        , strFilePath, strXmlFileName, strDate, strPortID, strMesTrxID);
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    strDescription = strErr;
                    return false;
                }
                #endregion

                XmlDocument old_xml = new XmlDocument();
                old_xml.Load(strFilePath);
                MesSpec.BoxProcessEnd old = (MesSpec.BoxProcessEnd)MesSpec.Spec.XMLtoMessage(old_xml);
                old.BODY = boxProcessEndBody;
                string xml = old.WriteToXml();
                XmlDocument new_xml = new XmlDocument();
                new_xml.LoadXml(xml);
                new_xml.Save(strFilePath);
                strDescription = "IncompleteBoxEditSaveReply OK";
                return true;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("IncompleteBoxEditSaveReply NG - Exception_Error - [Message:{0} ,XmlFileName:{1}, Date:{2}, PortID:{3}, MesTrxID:{4}]"
                    , ex.Message, strXmlFileName, strDate, strPortID, strMesTrxID);
                return false;
            }
        }

        /// <summary>
        /// SaveIncompleteCassetteToDB
        /// 描述:依據傳入的參數，於 SBCS_INCOMPLETECST.STATE 新增一筆資料
        /// </summary>
        /// <param name="strPordID">Pord ID</param>
        /// <param name="strCassetteID">Cassette ID</param>
        /// <param name="strMesTrxID">MES Trx ID - [yyyyMMddhhmmssfff]</param>
        /// <param name="strCassetteSeqNO"></param>
        /// <param name="State">SBCS_INCOMPLETECST.STATE 的狀態資料 - [ OK | NG | CLOSE ]</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <returns></returns>
        public bool SaveIncompleteCassetteToDB(string strPordID, string strCassetteID, string strMesTrxID, int intCassetteSeqNO, string State, string strNGReason, out string strDescription, string strFileType = "")
        {
            try
            {
                INCOMPLETECST inComplete = new INCOMPLETECST();
                inComplete.CASSETTEID = strCassetteID;
                inComplete.CASSETTESEQNO = intCassetteSeqNO;
                inComplete.FILENAME = string.Format("{0}_{1}_{2}", strFileType + strPordID, strCassetteID, strMesTrxID);
                inComplete.MESTRXID = strMesTrxID;
                inComplete.PORTID = strPordID;
                inComplete.STATE = State;
                inComplete.UPDATETIME = DateTime.Now;
                inComplete.NGREASON = strNGReason;
                this.InsertDB(inComplete);

                strDescription = "SaveIncompleteCassetteToDB OK";
                return true;
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("SaveIncompleteCassetteToDB NG - Exception Message:{0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// UpdateIncompleteCassetteToDB
        /// 描述:依據 Port ID, Cassette ID, Mes Trx ID, 於 SBCS_INCOMPLETECST 找到其資料並更新其 State 欄位的狀態
        /// </summary>
        /// <param name="strPordID">Pord ID</param>
        /// <param name="strCassetteID">Cassette ID</param>
        /// <param name="strMesTrxID">MES Trx ID - [yyyyMMddhhmmssfff]</param>
        /// <param name="State">SBCS_INCOMPLETECST.STATE 的狀態資料 - [ OK | NG | CLOSE ]</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <returns></returns>
        public bool UpdateIncompleteCassetteToDB(string strPordID, string strCassetteID, string strMesTrxID, string State, out string strDescription)
        {
            try
            {
                INCOMPLETECST inComplete = new INCOMPLETECST();
                IList list = HibernateAdapter.GetObjectByQuery(string.Format("from INCOMPLETECST where PORTID='{0}' and CASSETTEID='{1}' and MESTRXID='{2}'", strPordID, strCassetteID, strMesTrxID));
                if (list != null)
                {
                    inComplete = list[0] as INCOMPLETECST;
                    inComplete.STATE = State;
                    inComplete.UPDATETIME = DateTime.Now;
                    HibernateAdapter.UpdateObject(inComplete);

                    strDescription = "UpdateIncompleteCassetteToDB OK";
                    return true;
                }
                else
                {
                    strDescription = "UpdateIncompleteCassetteToDB NG - It's not find the data.";
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strDescription);
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("UpdateIncompleteCassetteToDB Exception Message:{0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// UpdateIncompleteCassetteToDB
        /// 描述:依據 Port ID, Cassette ID, Mes Trx ID, 於 SBCS_INCOMPLETECST 找到其資料並更新其 State 欄位的狀態
        /// </summary>
        /// <param name="strPordID">Pord ID</param>
        /// <param name="strCassetteID">Cassette ID</param>
        /// <param name="strMesTrxID">MES Trx ID - [yyyyMMddhhmmssfff]</param>
        /// <param name="State">SBCS_INCOMPLETECST.STATE 的狀態資料 - [ OK | NG | CLOSE ]</param>
        /// <param name="strDescription">執行結果描述 - 格式:[(OK or NG) + Message]</param>
        /// <returns></returns>
        public bool UpdateIncompleteCassetteToDB(string strPordID, string strCassetteID, string strMesTrxID, string State, string ReturnMessage, out string strDescription)
        {
            try
            {
                INCOMPLETECST inComplete = new INCOMPLETECST();
                IList list = HibernateAdapter.GetObjectByQuery(string.Format("from INCOMPLETECST where PORTID='{0}' and CASSETTEID='{1}' and MESTRXID='{2}'", strPordID, strCassetteID, strMesTrxID));
                if (list != null)
                {
                    inComplete = list[0] as INCOMPLETECST;
                    inComplete.STATE = State;
                    inComplete.UPDATETIME = DateTime.Now;
                    inComplete.NGREASON = ReturnMessage;
                    HibernateAdapter.UpdateObject(inComplete);

                    strDescription = "UpdateIncompleteCassetteToDB OK";
                    return true;
                }
                else
                {
                    strDescription = "UpdateIncompleteCassetteToDB NG - It's not find the data.";
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strDescription);
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                strDescription = string.Format("UpdateIncompleteCassetteToDB Exception Message:{0}", ex.Message);
                return false;
            }
        }
    }

}
