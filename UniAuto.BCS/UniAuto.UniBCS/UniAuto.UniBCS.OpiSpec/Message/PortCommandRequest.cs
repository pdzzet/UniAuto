﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class PortCommandRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string OPERATORID { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string PORTNO { get; set; }

			public string PORTID { get; set; }

			public string COMMANDTYPE { get; set; }

			public string PORTCOMMAND { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				OPERATORID = string.Empty;
				EQUIPMENTNO = string.Empty;
				PORTNO = string.Empty;
				PORTID = string.Empty;
				COMMANDTYPE = string.Empty;
				PORTCOMMAND = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public PortCommandRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "PortCommandReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
