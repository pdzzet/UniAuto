using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	//<RETURN>
	//    <RETURNCODE>0</RETURNCODE>
	//    <RETURNMESSAGE></RETURNMESSAGE>
	//</RETURN>

	public class Return
	{
		public string RETURNCODE
		{
			get;
			set;
		}

		public string RETURNMESSAGE
		{
			get;
			set;
		}

		public Return()
		{
			RETURNCODE = "0";
			RETURNMESSAGE = string.Empty;
		}

		public Return(string ReturnCode, string ReturnMessage)
		{
			RETURNCODE = ReturnCode;
			RETURNMESSAGE = ReturnMessage;
		}
	}
}
