using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	//<HEADER>
	//    <MESSAGENAME>AreYouThereRequest</MESSAGENAME>
	//    <TRANSACTIONID>20101129145858687500</TRANSACTIONID>
	//    <REPLYSUBJECTNAME>COMPANY.FACTORY.MES.PRD.FAB.PEMsvr</REPLYSUBJECTNAME>
	//    <INBOXNAME>_INBOX.0A46012D.4C81ECE61413A17.764</INBOXNAME>
	//    <LISTENER>PEMListener</LISTENER>
	//</HEADER>

	public class Header
	{
		public string MESSAGENAME
		{
			get;
			set;
		}

		public string TRANSACTIONID
		{
			get;
			set;
		}

		public string REPLYSUBJECTNAME
		{
			get;
			set;
		}

		public string INBOXNAME
		{
			get;
			set;
		}

		public string LISTENER
		{
			get;
			set;
		}

		public Header()
		{
			MESSAGENAME = string.Empty;
			TRANSACTIONID = Spec.GetTransactionID();
			REPLYSUBJECTNAME = string.Empty;
			INBOXNAME = string.Empty;
			LISTENER = string.Empty;
		}
	}
}
