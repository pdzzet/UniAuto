#region [Using]
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Unicom.UniAuto.Net.Socket;
#endregion	
 
namespace UniOPI
{
    public class MessageResponse
    {
        #region [const]
        /// <summary>
        /// OK constant=0
        /// </summary>
        public const int OK = 0;
        /// <summary>
        /// NG constant=-1
        /// </summary>
        public const int NG = -1;
        /// <summary>
        /// EXCEPTION constant=-1
        /// </summary>
        public const int EXCEPTION = -2;
        #endregion

        #region [public]
        /// <summary>
        /// return code
        /// </summary>
        public int RetCode = MessageResponse.OK;
        /// <summary>
        /// return message
        /// </summary>
        public string RetMsg = string.Empty;
        /// <summary>
        /// operation complete timestamp
        /// </summary>
        public DateTime CompDT { get; set; }
        /// <summary>
        /// is operation time out occured
        /// </summary>
        public bool IsTimeOut { get; set; }
        /// <summary>
        /// trx id
        /// </summary>
        public string TrxId { get; set; }
        /// <summary>
        /// xml message
        /// </summary>
        public string Xml { get; set; }
        /// <summary>
        /// session id
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// relation key
        /// </summary>
        public string RelationKey { get; set; }
        #endregion

        #region [public method]
        /// <summary>
        /// set code & message
        /// </summary>
        /// <param name="code">code</param>
        /// <param name="errMsg">msg</param>
        public void Set(int code, string msg)
        {
            RetCode = code;
            RetMsg = msg;
        }
        #endregion
    }
}
