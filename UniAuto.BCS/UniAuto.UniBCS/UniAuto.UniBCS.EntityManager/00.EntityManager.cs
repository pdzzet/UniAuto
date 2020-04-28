using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.DB;
using Newtonsoft.Json;

namespace UniAuto.UniBCS.EntityManager
{
	public abstract class EntityManager
	{
        public class Info
        {
            public string DataFilePath { get; set; }
            public EntityManager.FILE_TYPE FileType { get; set; }
            public string Extension { get; set;}
            public Info()
            {
                DataFilePath = string.Empty;
                FileType = FILE_TYPE.NO_FILE;
                Extension = "Bin";
            }
        }

        /// <summary>
        /// 檔案讀寫類型
        /// </summary>
        public enum FILE_TYPE
        {
            /// <summary>
            /// 不需檔案
            /// </summary>
            NO_FILE = 0,

            /// <summary>
            /// 檔案為Binary序列化資料
            /// </summary>
            BIN = 1,

            /// <summary>
            /// 檔案為XML序列化資料
            /// </summary>
            XML = 2,
            
            /// <summary>
            /// 档案为JSON序列化
            /// </summary>
            JSON=3
        }
       
       
        private Info _info = new Info();
		protected string BcServerName
		{
			get
			{
				return Workbench.ServerName;
			}
		}
        
        #region 由Spring給值
        /// <summary>
        /// NLog Logger Name
        /// </summary>
        public string LoggerName { get; set; }

        /// <summary>
        /// 用來讀取DB
        /// </summary>
        public HibernateAdapter HibernateAdapter { get; set; }

        /// <summary>
        /// 檔案路徑
        /// </summary>
        public string DataFilePath { get { return _info.DataFilePath; } set { _info.DataFilePath = value; } }
        #endregion

        /// <summary>
        /// 由Spring呼叫, 讀取DB資料並實例化EntityData及啟動Thread
        /// </summary>
        public void Init()
		{
			NLogManager.Logger.LogDebugWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "Begin");
            _info.FileType = GetFileType();

            if (string.IsNullOrEmpty(DataFilePath))
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "DataFilePath is null or empty");
                throw new Exception("DataFilePath is null or empty");
            }

            DataFilePath = DataFilePath.Replace("{ServerName}", Workbench.ServerName);
            if (DataFilePath[DataFilePath.Length - 1] != '\\')
                DataFilePath += "\\";
               
            if (!Directory.Exists(DataFilePath))
                Directory.CreateDirectory(DataFilePath);

            List<EntityData> entity_datas = new List<EntityData>();
            List<EntityFile> entity_files = new List<EntityFile>();
            List<string> file_names = null;

            Type type_of_entity_data = GetTypeOfEntityData();
            string hql = GetSelectHQL();
            if (type_of_entity_data != null && !string.IsNullOrEmpty(hql))
                entity_datas = FindByQuery(hql);//(List<EntityData>)HibernateAdapter.GetObject(type_of_entity_data,)
                
            if (GetFileType() != FILE_TYPE.NO_FILE)
                AfterSelectDB(entity_datas, DataFilePath, out file_names);

            if (file_names != null)
            {
                if (file_names.Count == 1 && file_names[0].IndexOf("*.") == 0)
                {
                    string[] fnames = Directory.GetFiles(DataFilePath, file_names[0]);
                    file_names.Clear();
                    file_names.AddRange(fnames);
                }
                else
                {
                    for (int i = 0; i < file_names.Count; i++)
                        file_names[i] = Path.Combine(DataFilePath, file_names[i]);
                }
                foreach(string file_name in file_names)
                {
                    EntityFile entity_file = null;
                    string fname = Path.GetFileName(file_name);
                    if (File.Exists(file_name))
                    {
                        entity_file = LoadEntityFile(file_name);
                    }
                    else
                    {
                        entity_file = NewEntityFile(fname);
                    }
                    if (entity_file != null)
                    {
                       entity_file.SetFilename(fname);
                        //entity_file.SetFilename(string.Format("{0}.{1}",fname,GetFileExtension()));
                        entity_files.Add(entity_file);
                    }
                    else
                    {
                        try
                        {
                            File.Delete(file_name);
                            NLogManager.Logger.LogWarnWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", 
                                    string.Format("Delete Error File ({0}).",file_name));
                        }
                        catch (System.Exception ex)
                        {
                            NLogManager.Logger.LogErrorWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        }
                        //File 无法序列化Delete File 并生产新的File
                        entity_file = NewEntityFile(string.Format("{0}.{1}",fname,GetFileExtension()));
                        entity_file.SetFilename(fname);
                        entity_files.Add(entity_file);
                    }
                }
            }
            AfterInit(entity_datas, entity_files);
            SaveFileThread.Init();
			NLogManager.Logger.LogDebugWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "End");
		}

        /// <summary>
        /// 由Spring或測試程式呼叫, 終止Thread
        /// </summary>
		public void Destroy()
		{
            SaveFileThread.Destroy();
		}

        /// <summary>
        /// 將EntityFile放入Queue, 由Thread存檔
        /// </summary>
        /// <param name="file"></param>
		public virtual void EnqueueSave(EntityFile file)
		{
            SaveFileThread.EnqueueSave(new SaveFileThread.Item(_info, file));
		}

        public virtual void InsertDB(EntityData data)
        {
            try
            {
                NLogManager.Logger.LogDebugWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", data.GetType().Name);
                HibernateAdapter.SaveObject(data);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        public virtual void InsertAllDB(EntityData[] data)
        {
            try
            {
                if (data != null && data.Length > 0)
                {
                    NLogManager.Logger.LogDebugWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", data[0].GetType().Name);
                    HibernateAdapter.SaveObjectAll(data);
                }
            }
            catch
            {
            }
        }

        public virtual void UpdateDB(EntityData data)
        {
            try
            {
                NLogManager.Logger.LogDebugWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", data.GetType().Name);
                HibernateAdapter.UpdateObject(data);
            }
            catch
            {
            }
        }

        public virtual void UpdateAllDB(EntityData[] data)
        {
            try
            {
                if (data != null && data.Length > 0)
                {
                    NLogManager.Logger.LogDebugWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", data[0].GetType().Name);
                    HibernateAdapter.UpdateObject(data);
                }
            }
            catch
            {
            }
        }

        public virtual void DeleteDB(EntityData data)
        {
            try
            {
                NLogManager.Logger.LogDebugWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", data.GetType().Name);
                HibernateAdapter.DeleteObject(data);
            }
            catch
            {
            }
        }

        public virtual void DeleteAllDB(EntityData[] data)
        {
            try
            {
                if (data != null && data.Length > 0)
                {
                    NLogManager.Logger.LogDebugWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", data[0].GetType().Name);
                    HibernateAdapter.DeleteObject(data);
                }
            }
            catch
            {
            }
        }

        public abstract FILE_TYPE GetFileType();

        /// <summary>
        /// 取得 File 扩展名
        /// </summary>
        /// <returns></returns>
        public virtual string GetFileExtension() {
            if (GetFileType() == FILE_TYPE.BIN) {
                return "bin";
            } else if (GetFileType() == FILE_TYPE.XML) {
                return "xml";
            } else if (GetFileType() == FILE_TYPE.JSON) {
                return "json";
            } else {
                return "bin";
            }
            
        }
        /// <summary>
        /// 為了讀取DB資料, 需要子類提供Select SQL語句
        /// </summary>
        /// <returns></returns>
		protected abstract string GetSelectHQL();

        /// <summary>
        /// 讀取DB資料的同時, 會將資料實例化成EntityData, 故需要子類提供EntityData的Type
        /// </summary>
        /// <returns></returns>
		protected abstract Type GetTypeOfEntityData();

        /// <summary>
        /// 當子類的FILE_TYPE不是NO_FILE, 讀取完DB後由子類決定要讀取檔案路徑下的哪些檔案
        /// </summary>
        /// <param name="EntityDatas">父類給予子類讀取DB後取得的資料</param>
        /// <param name="FilePath">父類給予子類檔案路徑</param>
        /// <param name="Filenames">主副檔名不含路徑. 子類指定要讀取哪些檔案, 若指定的檔案名稱不存在, 則父類會呼叫NewEntityFile(). 若Filenames只有一個且值為'*.副檔名', 則父類會讀取檔案路徑下全部'*.副檔名'</param>
        protected abstract void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames);

        /// <summary>
        /// 讀取XML檔案時, 會將XML反序列化成EntityFile, 故需要子類提供EntityFile的Type
        /// </summary>
        /// <returns></returns>
        protected abstract Type GetTypeOfEntityFile();

        /// <summary>
        /// 根據子類指定的檔名找不到檔案, 因檔案不存在而無法反序列化時, 由子類生成EntityFile
        /// </summary>
        /// <param name="Filename">子類指定的主檔名+副檔名, 不含路徑</param>
        /// <returns></returns>
		protected abstract EntityFile NewEntityFile(string Filename);

        /// <summary>
        /// 讀取DB及檔案結束後, 全部的EntityData及EntityFile交由子類保管, 父類不保管Entity
        /// </summary>
        /// <param name="Entities"></param>
		protected abstract void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles);
		
        /// <summary>
        /// 讀取Binary序列化檔案
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private EntityFile LoadBinFile(string filename)
        {
            FileStream fs = System.IO.File.OpenRead(filename);
            byte[] int_bytes = new byte[sizeof(int)];//byte個數
            fs.Read(int_bytes, 0, int_bytes.Length);//byte個數
            int data_len = BitConverter.ToInt32(int_bytes, 0);//byte個數
            byte[] data_bytes = new byte[data_len];//byte buffer
            fs.Read(data_bytes, 0, data_bytes.Length);//byte buffer
            fs.Close();
            return BinaryDeserialize(data_bytes);
        }

        /// <summary>
        /// 讀取Xml序列化(*.xml)檔案
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private EntityFile LoadXmlFile(string filename)
        {
            Type type = GetTypeOfEntityFile();
            EntityFile ret = null;
            if(type != null)
            {
                using (FileStream fs = System.IO.File.OpenRead(filename))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string xml = sr.ReadToEnd();
                        ret = XmlDeserialize(type, xml);
                    }
                }
                    
            }
            return ret;
        }

        private EntityFile LoadJsonFile(string fileName) {
            Type type = GetTypeOfEntityFile();
            EntityFile ret = null;
            if (type != null) {
                using (FileStream fs = System.IO.File.OpenRead(fileName)) {
                    using (StreamReader sr = new StreamReader(fs)) {
                        string str = sr.ReadToEnd();
                        ret = JsonDeserialize(str);
                    }
                }
            }
            return ret;
        }

        private EntityFile LoadEntityFile(string Filename)
        {
            EntityFile ret = null;
            try
            {
                if (GetFileType() == FILE_TYPE.BIN)
                    ret = LoadBinFile(Filename);
                else if (GetFileType() == FILE_TYPE.XML)
                    ret = LoadXmlFile(Filename);
                else if (GetFileType() == FILE_TYPE.JSON)
                    ret = LoadJsonFile(Filename);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
            return ret;
        }

        /// <summary>
        /// Binary反序列化
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
		private EntityFile BinaryDeserialize(byte[] bytes)
		{
            MemoryStream ms = new MemoryStream(bytes);
			BinaryFormatter bf = new BinaryFormatter();
			return (EntityFile)bf.Deserialize(ms);
		}

        /// <summary>
        /// Xml反序列化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        private EntityFile XmlDeserialize(Type type, string str)
        {
            XmlSerializer xmlserializer = new XmlSerializer(type);
            StringReader sr = new StringReader(str);
            return (EntityFile)xmlserializer.Deserialize(sr);
        }

        /// <summary>
        /// Json 序列化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        private EntityFile JsonDeserialize( string str) {
            EntityFile file = JsonConvert.DeserializeObject<EntityFile>(str);
            return file;
        }

        /// <summary>
        /// 从DB中获取对象
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        protected List<EntityData> FindByQuery(string hql)
        {
            List<EntityData> list = new List<EntityData>();
            try
            {
                IList list2 = this.HibernateAdapter.GetObjectByQuery(hql);
                if (list2 != null)
                {
                    foreach (EntityData t_data in list2)
                    {
                        list.Add(t_data);
                    }
                }
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite("", this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return list;
        }

        /// <summary>
        /// Update Entity Data to DB
        /// </summary>
        /// <param name="obj"></param>
        public void UpdateObject(EntityData obj)
        {
            try
            {
                HibernateAdapter.UpdateObject(obj);
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite("", this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
	}

    internal class StringWriterUTF8 : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
