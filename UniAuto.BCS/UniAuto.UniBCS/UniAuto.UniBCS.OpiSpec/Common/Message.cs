using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	//<MESSAGE>
	//    <HEADER>
	//        <MESSAGENAME>AreYouThereRequest</MESSAGENAME>
	//        <TRANSACTIONID>20101129145858687500</TRANSACTIONID>
	//        <REPLYSUBJECTNAME>COMPANY.FACTORY.MES.PRD.FAB.PEMsvr</REPLYSUBJECTNAME>
	//        <INBOXNAME>_INBOX.0A46012D.4C81ECE61413A17.764</INBOXNAME>
	//        <LISTENER>PEMListener</LISTENER>
	//    </HEADER>
	//    <BODY>
	//       <LINENAME></LINENAME>
	//    </BODY>
	//    <RETURN>
	//        <RETURNCODE>0</RETURNCODE>
	//        <RETURNMESSAGE></RETURNMESSAGE>
	//    </RETURN>
	//</MESSAGE>

	public abstract class Message
	{
        [XmlIgnore]
        public Spec.DirType Direction
        {
            get;
            set;
        }

        [XmlIgnore]
        public string WaitReply
        {
            get;
            set;
        }

        [XmlIgnore]
        protected Return _return = null;

        public Header HEADER
        {
            get;
            set;
        }

        //SPEC要求RETURN在BODY之後, 但BODY只能在子類定義
        //故父類的RETURN不可輸出XML, 由子類輸出BODY及RETURN
        [XmlIgnore]
        public Return RETURN
        {
            get { return _return; }
            set { _return = value; }
        }

        public Message()
        {
            Direction = Spec.DirType.UNKNOWN;
            WaitReply = string.Empty;
            HEADER = new Header();
            RETURN = new Return();
        }

        public string WriteToXml()
        {
            try
            {
                StringWriterUTF8 sw = new StringWriterUTF8();
                XmlSerializerNamespaces names = new XmlSerializerNamespaces();
                names.Add(string.Empty, string.Empty);//移除xmlns:xsi與xmlns:xsd
                XmlSerializer xmlwrite = new XmlSerializer(GetType());
                xmlwrite.Serialize(sw, this, names);
                return Spec.RemoveUnvisibleChar(sw.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occured in WriteToXml()", ex);
            }
        }

        public abstract Body GetBody();
	}
}
