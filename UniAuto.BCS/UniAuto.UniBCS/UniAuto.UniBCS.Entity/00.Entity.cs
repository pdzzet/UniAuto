using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace UniAuto.UniBCS.Entity
{
    [Serializable]
    /// <summary>
    /// 對應DB Table Schema, 每個Property都對應Table的欄位, 修改Property不會影響DB
    /// </summary>
    public abstract class EntityData
    {
    }

    /// <summary>
    /// 對應File, 修改Property後呼叫Save(), 會序列化存檔
    /// </summary>
    [Serializable]
    public abstract class EntityFile
    {
        /// <summary>
        /// 主檔名+副檔名(不含路徑)
        /// </summary>
        [XmlIgnore]
        [NonSerialized]
        protected string _filename = string.Empty;

        protected bool _writeFlag = true;

        /// <summary>
        /// True : Write Backup Data; False : Delete Backup Data
        /// </summary>
        public bool WriteFlag //false is delete file
        {
            get { return _writeFlag; }
            set { _writeFlag = value; }
        }

        public EntityFile()
        {
        }

        /// <summary>
        /// 主檔名 +   扩展名
        /// </summary>
        /// <returns></returns>
        public string GetFilename()
        {
            return _filename;
        }

        /// <summary>
        /// 主檔名 包含 扩展名
        /// </summary>
        /// <returns></returns>
        public void SetFilename(string filename)
        {
            _filename = filename;
        }
    }

    /// <summary>
    /// Entity內含有對應DB的資料, 與對應File的資料
    /// Entity內不宣告EntityData與EntityFile, 由繼承的子類宣告
    /// </summary>
    public abstract class Entity
    {
    }

    // ===
   
}
