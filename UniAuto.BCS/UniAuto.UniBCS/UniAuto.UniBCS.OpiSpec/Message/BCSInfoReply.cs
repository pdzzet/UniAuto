using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class BCSInfoReply : Message
    {
        public class RUNTIMEITEMc
        {
            public string NAME { get; set; }

            public string VAL { get; set; }

            public RUNTIMEITEMc()
            {
                NAME = string.Empty;
                VAL = string.Empty;
            }
        }

        public class KEYc
        {
            public string NAME { get; set; }

            public string VAL { get; set; }

            public KEYc()
            {
                NAME = string.Empty;
                VAL = string.Empty;
            }
        }

        public class AGENTINFOc
        {
            public string NAME { get; set; }

            public string CONNECTSTATE { get; set; }

            public string STATUS { get; set; }

            public string DLLVER { get; set; }

            public string CFGPATH { get; set; }

            public string FMTPATH { get; set; }

            public string CFGDATA { get; set; }

            [XmlArray("RUNTIMEINFOLIST")]
            [XmlArrayItem("RUNTIMEITEM")]
            public List<RUNTIMEITEMc> RUNTIMEINFOLIST { get; set; }

            public AGENTINFOc()
            {
                NAME = string.Empty;
                CONNECTSTATE = string.Empty;
                STATUS = string.Empty;
                DLLVER = string.Empty;
                CFGPATH = string.Empty;
                FMTPATH = string.Empty;
                CFGDATA = string.Empty;
                RUNTIMEINFOLIST = new List<RUNTIMEITEMc>();
            }
        }

        public class PARAMETERc
        {
            public string NAME { get; set; }

            public string VALUE { get; set; }

            public string DESCRIPTION { get; set; }

            public string TYPE { get; set; }

            public PARAMETERc()
            {
                NAME = string.Empty;
                VALUE = string.Empty;
                DESCRIPTION = string.Empty;
                TYPE = string.Empty;
            }
        }

        public class CONSTANTc
        {
            public string NAME { get; set; }

            public string DEFAULT { get; set; }

            [XmlArray("KEYLIST")]
            [XmlArrayItem("KEY")]
            public List<KEYc> KEYLIST { get; set; }

            public CONSTANTc()
            {
                NAME = string.Empty;
                DEFAULT = string.Empty;
                KEYLIST = new List<KEYc>();
            }
        }

        public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string PACKAGEVERSION { get; set; }

            public string APPSTARTUPPATH { get; set; }

            public string APPVERSION { get; set; }

            [XmlIgnore]
			public DateTime APPSTARTDATETIMEdt { get; set; }

			public string APPSTARTDATETIME
			{
                get { return this.APPSTARTDATETIMEdt.ToString("yyyyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo); }
				set
				{
					if(string.IsNullOrEmpty(value))
                        this.APPSTARTDATETIMEdt = DateTime.MinValue;
					else
					{
						string str = DateTimeFormat.Format(value);
                        this.APPSTARTDATETIMEdt = DateTime.Parse(str);
					}
				}
			}

            public string NETWORKIPADDR { get; set; }

            public string NETMACADDR { get; set; }

            public string PROCESSID { get; set; }

            public string WORKINGSET { get; set; }

            public string PEAKWORKINGSET { get; set; }

            public string PRIVATEMEMORY { get; set; }

            public string THREADCOUNT { get; set; }

            [XmlArray("AGENTINFOLIST")]
            [XmlArrayItem("ITEM")]
            public List<AGENTINFOc> AGENTINFOLIST { get; set; }

            [XmlArray("PARAMETERSLIST")]
            [XmlArrayItem("PARAMETER")]
            public List<PARAMETERc> PARAMETERSLIST { get; set; }

            [XmlArray("CONSTANTSLIST")]
            [XmlArrayItem("ITEM")]
            public List<CONSTANTc> CONSTANTSLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                PACKAGEVERSION = string.Empty;
                APPSTARTUPPATH = string.Empty;
                APPVERSION = string.Empty;
                APPSTARTDATETIMEdt = DateTime.MinValue;
                NETWORKIPADDR = string.Empty;
                NETMACADDR = string.Empty;
                PROCESSID = string.Empty;
                WORKINGSET = string.Empty;
                PEAKWORKINGSET = string.Empty;
                PRIVATEMEMORY = string.Empty;
                THREADCOUNT = string.Empty;
                AGENTINFOLIST = new List<AGENTINFOc>();
                PARAMETERSLIST = new List<PARAMETERc>();
                CONSTANTSLIST = new List<CONSTANTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public BCSInfoReply()
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
//<LINENAME></LINENAME>
//<PACKAGEVERSION></PACKAGEVERSION>
//<APPSTARTUPPATH></APPSTARTPATH>
//<APPVERSION></APPVERSION>
//<APPSTARTDATETIME></APPSTARTDATETIME>
//<NETWORKIPADDR></NETWORKIPADDR>
//<NETMACADDR></NETMACADDR>
//<AGENTINFOLIST>
//  <ITEM>
//    <NAME></NAME>
//    <CONNECTSTATE></CONNECTSTATE>
//    <STATUS></STATUS>
//    <DLLVER></DLLVER>
//    <CFGPATH></CFGPATH>
//    <FMTPATH></FMTPATH>
//    <CFGDATA></CFGDATA>
//    <RUNTIMEINFOLIST>
//      <RUNTIMEITEM>
//        <NAME></NAME>
//        <VAL></VAL>
//      </RUNTIMEITEM>
//    </RUNTIMEINFOLIST>
//  </ITEM>
//</AGENTINFOLIST>
//<PARAMETERSLIST>
//  <PARAMETER>
//    <NAME></NAME>
//    <VALUE></VALUE>
//    <DESCRIPTION></DESCRIPTION>
//    <TYPE></TYPE>
//  </PARAMETER>
//</PARAMETERSLIST>
//<CONSTANTSLIST>
//  <ITEM>
//    <NAME></NAME>
//    <DEFAULT></DEFAULT>
//    <KEYLIST>
//      <KEY>
//        <NAME></NAME>
//        <VAL></VAL>
//      </KEY>
//    </KEYLIST>
//  </ITEM>
//</CONSTANTSLIST>
