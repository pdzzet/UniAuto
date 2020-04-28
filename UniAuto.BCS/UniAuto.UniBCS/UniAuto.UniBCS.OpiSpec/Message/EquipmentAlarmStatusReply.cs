using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class EquipmentAlarmStatusReply : Message
	{
		public class ALARMc
		{
			public string ALARMUNIT { get; set; }

			public string ALARMID { get; set; }

			public string ALARMCODE { get; set; }

			public string ALARMLEVEL { get; set; }

			public string ALARMTEXT { get; set; }

			public ALARMc()
			{
				ALARMUNIT = string.Empty;
				ALARMID = string.Empty;
				ALARMCODE = string.Empty;
				ALARMLEVEL = string.Empty;
				ALARMTEXT = string.Empty;
			}
		}

        public class EQUIPMENTc
        {
            public string EQUIPMENTNO { get; set; }

            [XmlArray("ALARMLIST")]
            [XmlArrayItem("ALARM")]
            public List<ALARMc> ALARMLIST { get; set; }

            public EQUIPMENTc()
            {
                EQUIPMENTNO = string.Empty;
                ALARMLIST = new List<ALARMc>();
            }
        }

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            [XmlArray("EQUIPMENTLIST")]
            [XmlArrayItem("EQUIPMENT")]
            public List<EQUIPMENTc> EQUIPMENTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                EQUIPMENTLIST = new List<EQUIPMENTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public EquipmentAlarmStatusReply()
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
