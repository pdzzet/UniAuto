using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ValidateBoxReply : Message
	{
		public class BOXc
		{
			public string BOXNAME { get; set; }

			public string PRODUCTSPECNAME { get; set; }

			public string PRODUCTOWNER { get; set; }

			public string BOXGRADE { get; set; }

			public string PROCESSOPERATIONNAME { get; set; }

			public string PRDCARRIERSETCODE { get; set; }

			public string PRODUCTQUANTITY { get; set; }

			public string NODESTACK { get; set; }

			public string PRODUCTSIZETYPE { get; set; }

			public string PRODUCTSIZE { get; set; }

			public string PRODUCTSPECGROUP { get; set; }

			public string PRODUCTGCPTYPE { get; set; }

			public string ORIENTEDSITE { get; set; }

			public string ORIENTEDFACTORYNAME { get; set; }

			public string CURRENTSITE { get; set; }

			public string CURRENTFACTORYNAME { get; set; }

			public string BCPRODUCTTYPE { get; set; }

			public string BCPRODUCTID { get; set; }

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCT")]
			public List<PRODUCTc> PRODUCTLIST { get; set; }

			public BOXc()
			{
				BOXNAME = string.Empty;
				PRODUCTSPECNAME = string.Empty;
				PRODUCTOWNER = string.Empty;
				BOXGRADE = string.Empty;
				PROCESSOPERATIONNAME = string.Empty;
				PRDCARRIERSETCODE = string.Empty;
				PRODUCTQUANTITY = string.Empty;
				NODESTACK = string.Empty;
				PRODUCTSIZETYPE = string.Empty;
				PRODUCTSIZE = string.Empty;
				PRODUCTSPECGROUP = string.Empty;
				PRODUCTGCPTYPE = string.Empty;
				ORIENTEDSITE = string.Empty;
				ORIENTEDFACTORYNAME = string.Empty;
				CURRENTSITE = string.Empty;
				CURRENTFACTORYNAME = string.Empty;
				BCPRODUCTTYPE = string.Empty;
				BCPRODUCTID = string.Empty;
				PRODUCTLIST = new List<PRODUCTc>();
			}
		}

		public class PRODUCTc
		{
			public string POSITION { get; set; }

			public string PRODUCTNAME { get; set; }

			public string PRODUCTJUDGE { get; set; }

			public string PRODUCTGRADE { get; set; }

			public string GROUPID { get; set; }

			public string PRODUCTTYPE { get; set; }

			public string OWNERID { get; set; }

			public string OWNERTYPE { get; set; }

			public string DUMUSEDCOUNT { get; set; }

			[XmlArray("ABNORMALCODELIST")]
			[XmlArrayItem("CODE")]
			public List<CODEc> ABNORMALCODELIST { get; set; }

			public string FMAFLAG { get; set; }

			public string MHUFLAG { get; set; }

			public string BOXULDFLAG { get; set; }

			public string PPID { get; set; }

			[XmlIgnore]
			public bool PROCESSFLAGbool { get; set; }

			public string PROCESSFLAG
			{
				get{ return PROCESSFLAGbool ? "Y" : "N"; }
				set{ PROCESSFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public PRODUCTc()
			{
				POSITION = string.Empty;
				PRODUCTNAME = string.Empty;
				PRODUCTJUDGE = string.Empty;
				PRODUCTGRADE = string.Empty;
				GROUPID = string.Empty;
				PRODUCTTYPE = string.Empty;
				OWNERID = string.Empty;
				OWNERTYPE = string.Empty;
				DUMUSEDCOUNT = string.Empty;
				ABNORMALCODELIST = new List<CODEc>();
				FMAFLAG = string.Empty;
				MHUFLAG = string.Empty;
				BOXULDFLAG = string.Empty;
				PPID = string.Empty;
				PROCESSFLAGbool = false;
			}
		}

		public class CODEc
		{
			public string ABNORMALSEQ { get; set; }
			
			public string ABNORMALCODE { get; set; }

			public CODEc()
			{
                ABNORMALSEQ = string.Empty;
				ABNORMALCODE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PORTNAME { get; set; }

			public string LINERECIPENAME { get; set; }

			[XmlIgnore]
			public bool RECIPEPARAVALIDATIONFLAGbool { get; set; }

			public string RECIPEPARAVALIDATIONFLAG
			{
				get{ return RECIPEPARAVALIDATIONFLAGbool ? "Y" : "N"; }
				set{ RECIPEPARAVALIDATIONFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			[XmlArray("RECIPEPARANOCHECKLIST")]
			[XmlArrayItem("MACHINENAME")]
			public List<string> RECIPEPARANOCHECKLIST { get; set; }

			public string CARRIERSETCODE { get; set; }

			public string BOXQUANTITY { get; set; }

			[XmlIgnore]
			public bool VALIRESULTbool { get; set; }

			public string VALIRESULT
			{
				get{ return VALIRESULTbool ? "Y" : "N"; }
				set{ VALIRESULTbool = (string.Compare(value, "Y", true) == 0); }
			}

			[XmlArray("BOXLIST")]
			[XmlArrayItem("BOX")]
			public List<BOXc> BOXLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PORTNAME = string.Empty;
				LINERECIPENAME = string.Empty;
				RECIPEPARAVALIDATIONFLAGbool = false;
				RECIPEPARANOCHECKLIST = new List<string>();
				CARRIERSETCODE = string.Empty;
				BOXQUANTITY = string.Empty;
				VALIRESULTbool = false;
				BOXLIST = new List<BOXc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ValidateBoxReply()
		{
			this.Direction = Spec.DirType.MES_TO_BC;
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
