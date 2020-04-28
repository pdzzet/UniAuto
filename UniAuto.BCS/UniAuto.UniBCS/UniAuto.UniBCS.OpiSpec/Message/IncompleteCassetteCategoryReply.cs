using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class IncompleteCassetteCategoryReply : Message
	{
		public class INCOMPLETEDATEITEMc
		{
			public string INCOMPLETEDATE { get; set; }

			[XmlArray("CASSETTELIST")]
			[XmlArrayItem("CST")]
			public List<CSTc> CASSETTELIST { get; set; }

			public INCOMPLETEDATEITEMc()
			{
				INCOMPLETEDATE = string.Empty;
				CASSETTELIST = new List<CSTc>();
			}
		}

		public class CSTc
		{
			public string PORTID { get; set; }

			public string CASSETTEID { get; set; }

			public CSTc()
			{
				PORTID = string.Empty;
				CASSETTEID = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			[XmlArray("INCOMPLETEDATELIST")]
			[XmlArrayItem("INCOMPLETEDATEITEM")]
			public List<INCOMPLETEDATEITEMc> INCOMPLETEDATELIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				INCOMPLETEDATELIST = new List<INCOMPLETEDATEITEMc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public IncompleteCassetteCategoryReply()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
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
