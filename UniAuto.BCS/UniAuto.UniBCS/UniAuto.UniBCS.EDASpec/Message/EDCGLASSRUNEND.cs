using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.EDASpec
{
	[XmlRoot("MESSAGE")]
	public class EDCGLASSRUNEND : Message
	{
		public class sub_eqpc
		{
			public string sub_eqp_id { get; set; }

			public string sub_eqp_slot_id { get; set; }

			public string sub_eqp_status { get; set; }

			public string recipe_id { get; set; }

			public string start_time { get; set; }

			public string end_time { get; set; }

			public sub_eqpc()
			{
				sub_eqp_id = string.Empty;
				sub_eqp_slot_id = string.Empty;
				sub_eqp_status = string.Empty;
				recipe_id = string.Empty;
				start_time = string.Empty;
				end_time = string.Empty;
			}
		}

		public class eqpc
		{
			public string eqp_id { get; set; }

			public string eqp_status { get; set; }

			public string recipe_id { get; set; }

			public string eqp_slot_id { get; set; }

			public string start_time { get; set; }

			public string end_time { get; set; }

			[XmlArray("sub_eqp_list")]
			[XmlArrayItem("sub_eqp")]
			public List<sub_eqpc> sub_eqp_list { get; set; }

			public eqpc()
			{
				eqp_id = string.Empty;
				eqp_status = string.Empty;
				recipe_id = string.Empty;
				eqp_slot_id = string.Empty;
				start_time = string.Empty;
				end_time = string.Empty;
				sub_eqp_list = new List<sub_eqpc>();
			}
		}

		public class message_idc
		{
			public string id { get; set; }

			public message_idc()
			{
				id = string.Empty;
			}
		}

		public class messagec
		{
			public message_idc message_id { get; set; }

			public string systembyte { get; set; }

			public string timestamp { get; set; }

			public string oriented_site { get; set; }

			public string oriented_factory_name { get; set; }

			public string current_site { get; set; }

			public string current_factory_name { get; set; }

			public string line_id { get; set; }

			public string glass_id { get; set; }

			public string pfcd { get; set; }

			public string product_group { get; set; }

			public string process_id { get; set; }

			public string lot_id { get; set; }

			public string start_time { get; set; }

			public string end_time { get; set; }

			public string cst_id { get; set; }

			public string slot_id { get; set; }

			[XmlArray("eqp_list")]
			[XmlArrayItem("eqp")]
			public List<eqpc> eqp_list { get; set; }

			public messagec()
			{
				message_id = new message_idc();
				systembyte = string.Empty;
				timestamp = string.Empty;
				oriented_site = string.Empty;
				oriented_factory_name = string.Empty;
				current_site = string.Empty;
				current_factory_name = string.Empty;
				line_id = string.Empty;
				glass_id = string.Empty;
				pfcd = string.Empty;
				product_group = string.Empty;
				process_id = string.Empty;
				lot_id = string.Empty;
				start_time = string.Empty;
				end_time = string.Empty;
				cst_id = string.Empty;
				slot_id = string.Empty;
				eqp_list = new List<eqpc>();
			}
		}

		public class TrxBody : Body
		{
			public messagec message { get; set; }

			public TrxBody()
			{
				message = new messagec();
			}
		}

		public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public EDCGLASSRUNEND()
		{
			this.Direction = Spec.DirType.BC_TO_EDA;
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
