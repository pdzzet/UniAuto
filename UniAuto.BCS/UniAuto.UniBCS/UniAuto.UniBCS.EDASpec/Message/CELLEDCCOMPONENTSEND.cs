using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.EDASpec
{
	[XmlRoot("MESSAGE")]
	public class CELLEDCCOMPONENTSEND : Message
	{
		public class paramc
		{
			public string param_group { get; set; }

			public string param_name { get; set; }

			public string param_value { get; set; }

			public paramc()
			{
				param_group = string.Empty;
				param_name = string.Empty;
				param_value = string.Empty;
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

			public string component_id { get; set; }

			public string process_id { get; set; }

			public string pfcd { get; set; }

			public string product_group { get; set; }

			public string line_id { get; set; }

			public string eqp_id { get; set; }

			public string cst_id { get; set; }

			public string slot_no { get; set; }

			public string productspectype { get; set; }

			public string substrate_type { get; set; }

			public string operator_id { get; set; }

			public string recipe { get; set; }

			public string fab_area { get; set; }

			public string eqp_start_time { get; set; }

			public string eqp_end_time { get; set; }

			public string track_out_time { get; set; }

			[XmlArray("param_list")]
			[XmlArrayItem("param")]
			public List<paramc> param_list { get; set; }

			public messagec()
			{
				message_id = new message_idc();
				systembyte = string.Empty;
				timestamp = string.Empty;
				oriented_site = string.Empty;
				oriented_factory_name = string.Empty;
				current_site = string.Empty;
				current_factory_name = string.Empty;
				component_id = string.Empty;
				process_id = string.Empty;
				pfcd = string.Empty;
				product_group = string.Empty;
				line_id = string.Empty;
				eqp_id = string.Empty;
				cst_id = string.Empty;
				slot_no = string.Empty;
				productspectype = string.Empty;
				substrate_type = string.Empty;
				operator_id = string.Empty;
				recipe = string.Empty;
				fab_area = string.Empty;
				eqp_start_time = string.Empty;
				eqp_end_time = string.Empty;
				track_out_time = string.Empty;
				param_list = new List<paramc>();
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

		public CELLEDCCOMPONENTSEND()
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
