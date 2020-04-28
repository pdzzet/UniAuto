﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.BcSocketSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeParameterReply : Message
	{
		public class PARAc
		{
			public string PARANAME { get; set; }

			public string VALUETYPE { get; set; }

			public string PARAVALUE { get; set; }

			public PARAc()
			{
				PARANAME = string.Empty;
				VALUETYPE = string.Empty;
				PARAVALUE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			[XmlArray("TIMEOUTEQPLIST")]
			[XmlArrayItem("MACHINENAME")]
			public List<string> TIMEOUTEQPLIST { get; set; }

			[XmlArray("CIMOFFEQPLIST")]
			[XmlArrayItem("MACHINENAME")]
			public List<string> CIMOFFEQPLIST { get; set; }

			[XmlArray("RECIPEPARALIST")]
			[XmlArrayItem("PARA")]
			public List<PARAc> RECIPEPARALIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				TIMEOUTEQPLIST = new List<string>();
				CIMOFFEQPLIST = new List<string>();
				RECIPEPARALIST = new List<PARAc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeParameterReply()
		{
			this.Direction = Spec.DirType.PASSIVE_TO_ACTIVE;
			this.WaitReply = string.Empty;
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}