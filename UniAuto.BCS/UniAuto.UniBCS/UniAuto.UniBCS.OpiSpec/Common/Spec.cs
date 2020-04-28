using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	public class Spec
	{
        private static SortedDictionary<string, string> _replyMap = new SortedDictionary<string,string>();
        private static bool _initReplyMap = false;

        /// <summary>
        /// 移除XML中的不可見字元後做XML反序列化但不檢查XML格式
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static Message CheckXMLFormat(string xml)
        {
            try
            {
                XmlDocument xml_doc = new XmlDocument();
                xml_doc.LoadXml(xml);
                return CheckXMLFormat(xml_doc);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occured in CheckXMLFormat(string)", ex);
            }
        }

        /// <summary>
        /// 移除XML中的不可見字元後做XML反序列化但不檢查XML格式
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static Message CheckXMLFormat(XmlDocument xmlDoc)
        {
            try
            {
                Message msg = XMLtoMessage(xmlDoc);
                //string msg_xml = msg.WriteToXml();
                //XmlDocument msg_xml_doc = new XmlDocument();
                //msg_xml_doc.LoadXml(msg_xml);
                //if (string.Compare(xmlDoc.OuterXml, msg_xml_doc.OuterXml) != 0)
                //{
                //    throw new Exception(string.Format("Message[{0}] ID[{1}], XML Format is not match.", msg.HEADER.MESSAGENAME, msg.HEADER.TRANSACTIONID));
                //}
                return msg;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occured in CheckXMLFormat(XmlDocument)", ex);
            }
        }

        /// <summary>
        /// 移除XML中的不可見字元後做XML反序列化但不檢查XML格式
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
		public static Message XMLtoMessage(string xml)
		{
			try
			{
				XmlDocument xml_doc = new XmlDocument();
				xml_doc.LoadXml(xml);
				return XMLtoMessage(xml_doc);
			}
			catch (Exception ex)
			{
				throw new Exception("Exception occured in XMLtoMessage(string)", ex);
			}
		}

        /// <summary>
        /// 移除XML中的不可見字元後做XML反序列化但不檢查XML格式
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
		public static Message XMLtoMessage(XmlDocument xmlDoc)
		{
			try
			{
				Message ret = null;
				string msg_name = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/MESSAGENAME").InnerText;
				Assembly asm = Assembly.GetExecutingAssembly();
				foreach (Type type in asm.GetTypes())
				{
					if (!type.IsAbstract && typeof(Message).IsAssignableFrom(type))
					{
						if (string.Compare(msg_name, type.Name, true) == 0)
						{
							XmlSerializer xmlserializer = new XmlSerializer(type);
                            string tmp_xml = RemoveUnvisibleChar(xmlDoc.InnerXml);
                            StringReader sr = new StringReader(tmp_xml);
							ret = (Message)xmlserializer.Deserialize(sr);
							break;
						}
					}
				}
                if (ret == null)
                    throw new Exception(string.Format("Message[{0}], Message name cannot find in OpiSpec.dll", msg_name));
				return ret;
			}
			catch (Exception ex)
			{
				throw new Exception("Exception occured in XMLtoMessage(XmlDocument)", ex);
			}
		}

        /// <summary>
        /// 指定Message名稱, 回傳Message實例
        /// </summary>
        /// <param name="messageName"></param>
        /// <returns></returns>
		public static Message NewMessage(string messageName)
		{
			try
			{
				Message ret = null;
				Assembly asm = Assembly.GetExecutingAssembly();
				foreach (Type type in asm.GetTypes())
				{
					if (!type.IsAbstract && typeof(Message).IsAssignableFrom(type))
					{
						if (string.Compare(messageName, type.Name, true) == 0)
						{
							ret = Activator.CreateInstance(type) as Message;
							break;
						}
					}
				}
				return ret;
			}
			catch (Exception ex)
			{
				throw new Exception("Exception occured in NewMessage(string)", ex);
			}
		}

        private static long trxid = 0;

        /// <summary>
        /// 回傳OPI SPEC規範的TransactionID
        /// "yyyyMMddHHmmssff"+"0~9"
        /// </summary>
        /// <returns></returns>
        public static string GetTransactionID()
        {
            //靜態變數累加1, 0~9循環, 避免每十毫秒取兩次TransactionID, 但不能避免十毫秒內取十次TransactionID
            //當trxid=long.MaxValue時, Interlocked.Increment會使trxid變成負數(long.MinValue), 因此轉型為ulong
            ulong tmp = (ulong)System.Threading.Interlocked.Increment(ref trxid);
            tmp = tmp % 10;
            string str = DateTime.Now.ToString("yyyyMMddHHmmssff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            return string.Format("{0}{1}", str, tmp);//"yyyyMMddHHmmssff"+"0~9"
        }

        /// <summary>
        /// 程式啟動時呼叫一次, 建立Reply與Command對應表
        /// </summary>
        private static void InitReplyMap()
        {
            lock (_replyMap)
            {
                _replyMap.Clear();
                Assembly asm = Assembly.GetExecutingAssembly();
                foreach (Type type in asm.GetTypes())
                {
                    if (!type.IsAbstract && typeof(Message).IsAssignableFrom(type))
                    {
                        Message msg = Activator.CreateInstance(type) as Message;
                        if (!string.IsNullOrEmpty(msg.WaitReply))
                        {
                            _replyMap.Add(msg.WaitReply, msg.HEADER.MESSAGENAME);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 傳入Message名稱, 找出該Message是用來Reply哪一個Command
        /// </summary>
        /// <param name="ReplyName">Reply的Message名稱</param>
        /// <returns>Command的Message名稱, 若無則為空字串</returns>
        public static string GetMessageByReply(string ReplyName)
        {
            if (!_initReplyMap)
            {
                InitReplyMap();
                _initReplyMap = true;
            }

            string ret = string.Empty;
            lock (_replyMap)
            {
                if (_replyMap.ContainsKey(ReplyName))
                {
                    ret = _replyMap[ReplyName];
                }
            }
            return ret;
        }

        /// <summary>
        /// 移除字串中的不可見字元
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveUnvisibleChar(string str)
        {
            // 字串值在序列化成XML時, 不可見字元及特殊字元會被XmlSerializer另外編碼
            // (char)1 轉成 &#x1;
            // (char)27 轉成 &#x1B;
            // '<' 轉成 &lt;
            // '>' 轉成 &gt;

            string[] unvisibles = new string[30]{  "&#x0;",  "&#x1;",  "&#x2;",  "&#x3;",  "&#x4;",  "&#x5;",  "&#x6;",  "&#x7;",  "&#x8;",  "&#xB;",
                                                   "&#xC;",  "&#xE;",  "&#xF;", "&#x10;", "&#x11;", "&#x12;", "&#x13;", "&#x14;", "&#x15;", "&#x16;",
                                                  "&#x17;", "&#x18;", "&#x19;", "&#x1A;", "&#x1B;", "&#x1C;", "&#x1D;", "&#x1E;", "&#x1F;", "&#x7F;"};
            StringBuilder sb = new StringBuilder(str);
            foreach (string unvisible in unvisibles)
            {
                sb.Replace(unvisible, string.Empty);
            }
            return sb.ToString();
        }

		public enum DirType
		{
			UNKNOWN = 0,
			BC_TO_OPI = 1,
			OPI_TO_BC = 2,
		}
	}

	internal class DateTimeFormat
	{
		//yyyyMMddHHmmss
		public static string Format(string value)
		{
			if (value.Length == "yyyyMMddHHmmss".Length)
			{
                return string.Format("{0}-{1}-{2} {3}:{4}:{5}",
                value.Substring(0, 4),
                value.Substring(4, 2),
                value.Substring(6, 2),
                value.Substring(8, 2),
                value.Substring(10, 2),
                value.Substring(12, 2));
			}
            else if (value.Length == "yyyyMMdd".Length)
            {
                return string.Format("{0}-{1}-{2} 00:00:00",
                value.Substring(0, 4),
                value.Substring(4, 2),
                value.Substring(6, 2));
            }
            throw new FormatException(String.Format("The strlen of arg must be {0} or {1}", "yyyyMMddHHmmss".Length, "yyyyMMdd".Length), null);
		}
	}
}
