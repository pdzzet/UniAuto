using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class AssembleComplete : Message
	{
		public class MAINPRODUCTc
		{
			public string CARRIERNAME { get; set; }

			public string PRODUCTNAME { get; set; }

			public MAINPRODUCTc()
			{
				CARRIERNAME = string.Empty;
				PRODUCTNAME = string.Empty;
			}
		}

		public class PAIRPRODUCTc
		{
			public string CARRIERNAME { get; set; }

			public string PRODUCTNAME { get; set; }

			public PAIRPRODUCTc()
			{
				CARRIERNAME = string.Empty;
				PRODUCTNAME = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public MAINPRODUCTc MAINPRODUCT { get; set; }

			public PAIRPRODUCTc PAIRPRODUCT { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				MAINPRODUCT = new MAINPRODUCTc();
				PAIRPRODUCT = new PAIRPRODUCTc();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public AssembleComplete()
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
	}
}
