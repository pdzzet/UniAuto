using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class ForceCleanOutCommandReportRequest : Message
    {
        public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string COMMANDTYPE { get; set; }

            //public string USERID { get; set; }      //add by burce 2015/8/14 for record use opi user

			public TrxBody()
			{
				LINENAME = string.Empty;
                COMMANDTYPE = string.Empty;
                //USERID = string.Empty;  //add by burce 2015/8/14 for record use opi user
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public ForceCleanOutCommandReportRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "ForceCleanOutCommandReportReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
    }
}
