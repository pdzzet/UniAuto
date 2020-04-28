using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
//<BODY>
//  <LINENAME></LINENAME>
//  <TIMESTAMP></TIMESTAMP>
//  <PORTLIST>
//    <PORT>
//      <PORTNAME></PORTNAME>
//      <PORTTRANSFERSTATE></PORTTRANSFERSTATE>
//      <CARRIERNAME></CARRIERNAME>
//      <PRODUCTNAME></PRODUCTNAME>
//      <PRODUCTGRADE></PRODUCTGRADE>
//    </PORT>
//    <ULDPORTLIST><!--特殊格式, PORTLIST中裡面還有ULDPORTLIST-->
//      <PORT>
//        <PORTNAME></PORTNAME>
//        <PRODUCTNAME></PRODUCTNAME>
//      </PORT>
//    </ULDPORTLIST>
//  </PORTLIST>
//</BODY>

    //特殊格式, 無法用CreatePatternClass工具產生此Class
	[XmlRoot("MESSAGE")]
	public class PortTransferStateChanged : Message, IXmlSerializable
	{
        public class ULDPORTc
        {
            public string PORTNAME { get; set; }

            public string PRODUCTNAME { get; set; }

            public ULDPORTc()
			{
				PORTNAME = string.Empty;
                PRODUCTNAME = string.Empty;
            }
        }

		public class PORTc
		{
			public string PORTNAME { get; set; }

			public string PORTTRANSFERSTATE { get; set; }

			public string CARRIERNAME { get; set; }

			public string PRODUCTNAME { get; set; }

			public string PRODUCTGRADE { get; set; }

			public PORTc()
			{
				PORTNAME = string.Empty;
				PORTTRANSFERSTATE = string.Empty;
				CARRIERNAME = string.Empty;
				PRODUCTNAME = string.Empty;
				PRODUCTGRADE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			[XmlIgnore]
			public DateTime TIMESTAMPdt { get; set; }

			public string TIMESTAMP
			{
				get { return this.TIMESTAMPdt.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo); }
				set
				{
					if(string.IsNullOrEmpty(value))
						this.TIMESTAMPdt = DateTime.MinValue;
					else
						this.TIMESTAMPdt = DateTime.Parse(value);
				}
			}

			public PORTc PORT { get; set; }

            public ULDPORTc ULDPORT { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				TIMESTAMPdt = DateTime.MinValue;
                PORT = new PORTc();
                ULDPORT = new ULDPORTc();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public PortTransferStateChanged()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = string.Empty;
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}

        /// <summary>
        /// 實現XML序列及反序列化
        /// </summary>
        /// <returns></returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        private void ReadReflection(XmlReader reader, object obj)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();//讀取<HEADER />
                return;
            }
            reader.Read();//讀取<HEADER>
            {
                PropertyInfo[] properties = obj.GetType().GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo prop = null;
                    foreach (PropertyInfo p in properties)
                    {
                        if (p.Name == reader.Name)
                        {
                            prop = p;
                            break;
                        }
                    }
                    if (prop != null)
                    {
                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                        }
                        else
                        {
                            reader.Read();
                            prop.SetValue(obj, reader.ReadString(), null);
                            reader.ReadEndElement();
                        }
                    }
                }
            }
            reader.ReadEndElement();//讀取</HEADER>
        }

        private void ReadHeader(XmlReader reader)
        {
            ReadReflection(reader, HEADER);
        }

        private void ReadBodyPortList(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();//讀取<PORTLIST />
                return;
            }

            reader.Read();// 讀取<PORTLIST>
            {
                for (int i = 0; i < 2; i++)
                {
                    if (reader.IsEmptyElement)
                        reader.Read();
                    else
                    {
                        if (reader.Name == "PORT")
                        {
                            ReadReflection(reader, BODY.PORT);
                        }
                        else if (reader.Name == "ULDPORTLIST")
                        {
                            if (reader.IsEmptyElement)
                                reader.Read();// 讀取<ULDPORTLIST />
                            else
                            {
                                reader.Read();// 讀取<ULDPORTLIST>
                                ReadReflection(reader, BODY.ULDPORT);
                                reader.ReadEndElement();// 讀取</ULDPORTLIST>
                            }
                        }
                    }
                }
            }
            reader.ReadEndElement();// 讀取</PORTLIST>
        }

        private void ReadBody(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();//讀取<BODY />
                return;
            }
            reader.Read();
            {
                for (int i = 0; i < 3; i++)
                {
                    if(reader.IsEmptyElement)
                        reader.Read();
                    else
                    {
                        if (reader.Name == "LINENAME")
                        {
                            reader.Read();
                            BODY.LINENAME = reader.ReadString();
                            reader.ReadEndElement();
                        }
                        else if (reader.Name == "TIMESTAMP")
                        {
                            reader.Read();
                            BODY.TIMESTAMP = reader.ReadString();
                            reader.ReadEndElement();
                        }
                        else if (reader.Name == "PORTLIST")
                        {
                            ReadBodyPortList(reader);
                        }
                    }
                }
            }
            reader.ReadEndElement();// 讀取</BODY>
        }

        private void ReadReturn(XmlReader reader)
        {
            ReadReflection(reader, RETURN);
        }

        /// <summary>
        /// 實現XML反序列化
        /// </summary>
        /// <returns></returns>
        public void ReadXml(XmlReader reader)
        {
            if (reader.Name != "MESSAGE")
                return;

            if (reader.IsEmptyElement)
            {
                reader.Read();// 讀取<MESSAGE />
                return;
            }

            reader.ReadStartElement("MESSAGE");
            {
                for (int i = 0; i < 3; i++)
                {
                    if (reader.Name == "HEADER")
                        ReadHeader(reader);
                    else if (reader.Name == "BODY")
                        ReadBody(reader);
                    else if (reader.Name == "RETURN")
                        ReadReturn(reader);
                }
            }
            reader.ReadEndElement();// 讀取</MESSAGE>
        }

        /// <summary>
        /// 實現XML序列化
        /// </summary>
        /// <returns></returns>
        public void WriteXml(XmlWriter writer)
        {
            XmlSerializerNamespaces names = new XmlSerializerNamespaces();
            names.Add(string.Empty, string.Empty);//移除xmlns:xsi與xmlns:xsd

            #region HEADER
            {
                writer.WriteStartElement("HEADER");
                {
                    writer.WriteStartElement("MESSAGENAME");
                    writer.WriteString(HEADER.MESSAGENAME);
                    writer.WriteEndElement();

                    writer.WriteStartElement("TRANSACTIONID");
                    writer.WriteString(HEADER.TRANSACTIONID);
                    writer.WriteEndElement();

                    writer.WriteStartElement("REPLYSUBJECTNAME");
                    writer.WriteString(HEADER.REPLYSUBJECTNAME);
                    writer.WriteEndElement();

                    writer.WriteStartElement("INBOXNAME");
                    writer.WriteString(HEADER.INBOXNAME);
                    writer.WriteEndElement();

                    writer.WriteStartElement("LISTENER");
                    writer.WriteString(HEADER.LISTENER);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            #endregion
            #region BODY
            {
                writer.WriteStartElement("BODY");
                {
                    writer.WriteStartElement("LINENAME");
                    writer.WriteString(BODY.LINENAME);
                    writer.WriteEndElement();

                    writer.WriteStartElement("TIMESTAMP");
                    writer.WriteString(BODY.TIMESTAMP);
                    writer.WriteEndElement();

                    #region PORTLIST
                    {
                        writer.WriteStartElement("PORTLIST");
                        {
                            #region PORT
                            {
                                writer.WriteStartElement("PORT");
                                {
                                    writer.WriteStartElement("PORTNAME");
                                    writer.WriteString(BODY.PORT.PORTNAME);
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("PORTTRANSFERSTATE");
                                    writer.WriteString(BODY.PORT.PORTTRANSFERSTATE);
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("CARRIERNAME");
                                    writer.WriteString(BODY.PORT.CARRIERNAME);
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("PRODUCTNAME");
                                    writer.WriteString(BODY.PORT.PRODUCTNAME);
                                    writer.WriteEndElement();

                                    writer.WriteStartElement("PRODUCTGRADE");
                                    writer.WriteString(BODY.PORT.PRODUCTGRADE);
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();
                            }
                            #endregion
                            #region ULDPORTLIST
                            {
                                writer.WriteStartElement("ULDPORTLIST");
                                {
                                    #region PORT
                                    {
                                        writer.WriteStartElement("PORT");
                                        {
                                            writer.WriteStartElement("PORTNAME");
                                            writer.WriteString(BODY.ULDPORT.PORTNAME);
                                            writer.WriteEndElement();

                                            writer.WriteStartElement("PRODUCTNAME");
                                            writer.WriteString(BODY.ULDPORT.PRODUCTNAME);
                                            writer.WriteEndElement();
                                        }
                                        writer.WriteEndElement();
                                    }
                                    #endregion
                                }
                                writer.WriteEndElement();
                            }
                            #endregion
                        }
                        writer.WriteEndElement();
                    }
                    #endregion
                }
                writer.WriteEndElement();
            }
            #endregion
            #region RETURN
            {
                writer.WriteStartElement("RETURN");
                {
                    writer.WriteStartElement("RETURNCODE");
                    writer.WriteString(RETURN.RETURNCODE);
                    writer.WriteEndElement();

                    writer.WriteStartElement("RETURNMESSAGE");
                    writer.WriteString(RETURN.RETURNMESSAGE);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            #endregion
        }
    }
}
