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
    public class MessageRequest
    {
        /// <summary>
        /// event datetime
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// trx id
        /// </summary>
        public string TrxId { get; set; }
        /// <summary>
        /// MessageName
        /// </summary>
        public string TrxMsgName { get; set; }
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
    }
}
