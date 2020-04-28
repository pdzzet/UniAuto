using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;

namespace UniAuto.UniRCS.Core
{
    /// <summary>
    /// Robot Check Rule Method Parameter
    /// </summary>
    public class RobotContext:IRobotContext
    {
        public const string RETURNMESSAGE = "ReturnMessage";
        public const string RETURNCODE = "ReturnCode";

        private Dictionary<string, object> _data;

        public Dictionary<string, object> Data {
            get { return _data; }
            set { _data = value; }
        }

        private DateTime _createTime=DateTime.Now;
        
        public RobotContext() {
            _data = new Dictionary<string, object>();
            _data.Add(RETURNMESSAGE, "");
            _data.Add(RETURNCODE, "0");
        }

        /// <summary>
        /// Get Parameter by Key if Not found Key return NULL
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key] {
            get {
                if (ContainsKey(key))
                    return _data[key];
                return null;
            }
        }

        /// <summary>
        /// Add Parameter If a key exists then remove old value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public void AddParameter(string key, object obj) {
            if (_data.ContainsKey(key)) _data.Remove(key);
            _data.Add(key, obj);
        }

        /// <summary>
        /// Check key exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key) {
            return _data.ContainsKey(key);
        }

        /// <summary>
        /// Get Return Message 
        /// </summary>
        /// <returns></returns>
        public string GetReturnMessage() {
            return _data[RETURNMESSAGE].ToString();
        }

        /// <summary>
        /// Get Return Code
        /// </summary>
        /// <returns></returns>
        public string GetReturnCode() {
            return _data[RETURNCODE].ToString();
        }

        /// <summary>
        /// set Return Message
        /// </summary>
        /// <param name="message"></param>
        public void SetReturnMessage(string message) {
            _data[RETURNMESSAGE] = message;
        }

        /// <summary>
        /// set Return Code
        /// </summary>
        /// <param name="code"></param>
        public void SetReturnCode(string code) {
            _data[RETURNCODE] = code;
        }

        /// <summary>
        /// set Return Message and Code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public void SetReturnCodeAndMessage(string code, string message) {
            SetReturnCode(code);
            SetReturnMessage(message);
        }


        public DateTime CreateTime {
            get { return _createTime; }
        }

        public void Clear()
        {
            _data = new Dictionary<string, object>();
            _data.Add(RETURNMESSAGE, "");
            _data.Add(RETURNCODE, "0");
        }
    }
}
