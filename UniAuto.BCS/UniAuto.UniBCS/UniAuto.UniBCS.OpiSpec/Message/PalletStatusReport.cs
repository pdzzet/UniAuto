﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class PalletStatusReport : Message
    {
        public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string PALLETNO { get; set; }

            public string PALLETID { get; set; }

            public string PALLETMODE { get; set; }

            public string PALLETDATAREQUEST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                PALLETNO = string.Empty;
                PALLETID = string.Empty;
                PALLETMODE = string.Empty;
                PALLETDATAREQUEST = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public PalletStatusReport()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = ""; // "PalletStatusReportReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
    }
}