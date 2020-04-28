using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.EDASpec
{
	public class Spec
	{
        private static SortedDictionary<string, string> _replyMap = new SortedDictionary<string,string>();
        private static bool _initReplyMap = false;

        /// <summary>
        /// XML反序列化並檢查XML格式
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
        /// XML反序列化並檢查XML格式
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static Message CheckXMLFormat(XmlDocument xmlDoc)
        {
            try
            {
                Message msg = XMLtoMessage(xmlDoc);
                string msg_xml = msg.WriteToXml();
                XmlDocument msg_xml_doc = new XmlDocument();
                msg_xml_doc.LoadXml(msg_xml);
                if (string.Compare(xmlDoc.OuterXml, msg_xml_doc.OuterXml) != 0)
                {
                    throw new Exception(string.Format("Message[{0}] ID[{1}], XML Format is not match.", msg.HEADER.MESSAGENAME, msg.HEADER.TRANSACTIONID));
                }
                return msg;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occured in CheckXMLFormat(XmlDocument)", ex);
            }
        }

        /// <summary>
        /// XML反序列化但不檢查XML格式
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
        /// XML反序列化但不檢查XML格式
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
		public static Message XMLtoMessage(XmlDocument xmlDoc)
		{
			try
			{
				Message ret = null;
				string msg_name = xmlDoc.SelectSingleNode("//message/message_id").InnerText;
				Assembly asm = Assembly.GetExecutingAssembly();
				foreach (Type type in asm.GetTypes())
				{
					if (!type.IsAbstract && typeof(Message).IsAssignableFrom(type))
					{
						if (string.Compare(msg_name, type.Name, true) == 0)
						{
							XmlSerializer xmlserializer = new XmlSerializer(type);
							StringReader sr = new StringReader(xmlDoc.InnerXml);
							ret = (Message)xmlserializer.Deserialize(sr);
							break;
						}
					}
				}
                if (ret == null)
                    throw new Exception(string.Format("Message[{0}], Message name cannot find in MesSpec.dll", msg_name));
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

        /// <summary>
        /// 回傳MES SPEC規範的TransactionID
        /// </summary>
        /// <returns></returns>
		public static string GetTransactionID()
		{
            return DateTime.Now.ToString("yyyyMMddHHmmssfff", System.Globalization.DateTimeFormatInfo.InvariantInfo);
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

		public enum DirType
		{
			UNKNOWN = 0,
			BC_TO_EDA = 1,
			EDA_TO_BC = 2
		}
	}

	internal class DateTimeFormat
	{
		//yyyyMMddHHmmss
		public static string Format(string value)
		{
			if (value.Length != "yyyyMMddHHmmss".Length)
			{
				throw new FormatException(String.Format("The strlen of arg must be {0}", "yyyyMMddHHmmss".Length), null);
			}

			return string.Format("{0}-{1}-{2} {3}:{4}:{5}",
				value.Substring(0, 4),
				value.Substring(4, 2),
				value.Substring(6, 2),
				value.Substring(8, 2),
				value.Substring(10, 2),
				value.Substring(12, 2));
		}
	}
}
