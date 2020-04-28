﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class IncompleteCassetteCategoryRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public IncompleteCassetteCategoryRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "IncompleteCassetteCategoryReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}