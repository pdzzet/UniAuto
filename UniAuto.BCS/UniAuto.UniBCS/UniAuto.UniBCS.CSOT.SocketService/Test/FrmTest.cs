using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using UniAuto.UniBCS.Entity;

namespace UniAuto.UniBCS.CSOT.SocketService.Test
{
    public partial class FrmTest : Form
    {
        public FrmTest()
        {
            InitializeComponent();
        }

        public void Init()
        {
        }

        public ActiveSocketService ActiveSocketService { get; set; }
        public PassiveSocketService PassiveSocketService { get; set; }

        private void btnActive_RecipeIDRegisterCheckRequest_Click(object sender, EventArgs e)
        {
            try
            {
                string trxid = "trxid";
                string line_name = "linename";
                List<RecipeCheckInfo> recipeCheckInfos = new List<RecipeCheckInfo>();
                recipeCheckInfos.Add(new RecipeCheckInfo("eqpNo", 1, 2, "recipeID", "lineRecipeName", true, true));
                recipeCheckInfos.Add(new RecipeCheckInfo("eqpNo", 1, 2, "recipeID", "lineRecipeName", true, true));

                List<string> noCheckList = new List<string>();
                noCheckList.Add("123");
                noCheckList.Add("456");
                ActiveSocketService.RecipeIDRegisterCheckRequest(trxid, line_name, recipeCheckInfos, noCheckList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnActive_RecipeParameterRequest_Click(object sender, EventArgs e)
        {
            try
            {
                string trxid = "trxid";
                string line_name = "linename";
                List<RecipeCheckInfo> recipeCheckInfos = new List<RecipeCheckInfo>();
                recipeCheckInfos.Add(new RecipeCheckInfo("eqpNo", 1, 2, "recipeID", "lineRecipeName", true, true));
                recipeCheckInfos.Add(new RecipeCheckInfo("eqpNo", 1, 2, "recipeID", "lineRecipeName", true, true));

                List<string> noCheckList = new List<string>();
                noCheckList.Add("123");
                noCheckList.Add("456");
                ActiveSocketService.RecipeParameterRequest(trxid, line_name, recipeCheckInfos, noCheckList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Job job = new Job();
                job.CassetteSequenceNo = "1";
                job.JobSequenceNo = "2";
                job.MesCstBody.LOTLIST.Add(new LOTc());
                job.MesCstBody.LOTLIST[0].PROCESSLINELIST.Add(new PROCESSLINEc());
                job.MesCstBody.LOTLIST[0].PROCESSLINELIST.Add(new PROCESSLINEc());
                job.MesCstBody.LOTLIST[0].PROCESSLINELIST.Add(new PROCESSLINEc());
                job.MesCstBody.LOTLIST[0].PROCESSLINELIST[0].BCPRODUCTID = "BCPRODUCTID_1";
                job.MesCstBody.LOTLIST[0].PROCESSLINELIST[1].BCPRODUCTID = "BCPRODUCTID_2";
                job.MesCstBody.LOTLIST[0].PROCESSLINELIST[2].BCPRODUCTID = "BCPRODUCTID_3";

                XmlDocument xml_doc = new XmlDocument();
#region XML
                string xml = @"<MESSAGE>
<HEADER>
	<MESSAGENAME>CFShortcutPermitReply</MESSAGENAME>
	<TRANSACTIONID>20101129145858687500</TRANSACTIONID>
	<REPLYSUBJECTNAME>COMPANY.FACTORY.MES.PRD.FAB.PEMsvr</REPLYSUBJECTNAME>
	<INBOXNAME>_INBOX.0A46012D.4C81ECE61413A17.764</INBOXNAME>
	<LISTENER>PEMListener</LISTENER>
</HEADER>
<BODY>
    <LINENAME>HOLA</LINENAME>
    <LINERECIPENAME></LINERECIPENAME>
    <SELECTEDPOSITIONMAP></SELECTEDPOSITIONMAP>
    <PERMITFLAG></PERMITFLAG>
    <LOTLIST>
        <LOT>
            <LOTNAME></LOTNAME>
            <PRODUCTSPECNAME></PRODUCTSPECNAME>
            <PRODUCTSPECVER></PRODUCTSPECVER>
            <PROCESSFLOWNAME></PROCESSFLOWNAME>
            <PROCESSOPERATIONNAME></PROCESSOPERATIONNAME>
            <PRODUCTOWNER></PRODUCTOWNER>
            <PRDCARRIERSETCODE></PRDCARRIERSETCODE>
            <SALEORDER></SALEORDER>
            <PRODUCTSIZETYPE></PRODUCTSIZETYPE>
            <PRODUCTSIZE></PRODUCTSIZE>
            <BCPRODUCTTYPE></BCPRODUCTTYPE>
            <BCPRODUCTID></BCPRODUCTID>
            <PRODUCTPROCESSTYPE></PRODUCTPROCESSTYPE>
            <LINERECIPENAME></LINERECIPENAME>
            <PPID></PPID>
            <SUBPRODUCTSPECS></SUBPRODUCTSPECS>
            <SUBPRODUCTNAMES></SUBPRODUCTNAMES>
            <SUBPRODUCTLINES></SUBPRODUCTLINES>
            <SUBPRODUCTSIZETYPES></SUBPRODUCTSIZETYPES>
            <SUBPRODUCTSIZES></SUBPRODUCTSIZES>
            <ORIENTEDSITE></ORIENTEDSITE>
            <ORIENTEDFACTORYNAME></ORIENTEDFACTORYNAME>
            <CURRENTSITE></CURRENTSITE>
            <CURRENTFACTORYNAME></CURRENTFACTORYNAME>
            <PRODUCTTHICKNESS></PRODUCTTHICKNESS>
            <PRODUCTLIST>
                <PRODUCT>
                    <POSITION></POSITION>
                    <PRODUCTNAME></PRODUCTNAME>
                    <ARRAYPRODUCTNAME></ARRAYPRODUCTNAME>
                    <CFPRODUCTNAME></CFPRODUCTNAME>
                    <ARRAYPRODUCTSPECNAME></ARRAYPRODUCTSPECNAME>
                    <ARRAYLOTNAME></ARRAYLOTNAME>
                    <DENSEBOXID></DENSEBOXID>
                    <PRODUCTJUDGE></PRODUCTJUDGE>
                    <PRODUCTGRADE></PRODUCTGRADE>
                    <SOURCEPART></SOURCEPART>
                    <PRODUCTRECIPENAME></PRODUCTRECIPENAME>
                    <SUBPRODUCTGRADES></SUBPRODUCTGRADES>
                    <SUBPRODUCTDEFECTCODE></SUBPRODUCTDEFECTCODE>
                    <SUBPRODUCTJPSGRADE></SUBPRODUCTJPSGRADE>
                    <SUBPRODUCTJPSCODE></SUBPRODUCTJPSCODE>
                    <SUBPRODUCTJPSFLAG></SUBPRODUCTJPSFLAG>
                    <ARRAYSUBPRODUCTGRADE></ARRAYSUBPRODUCTGRADE>
                    <CFSUBPRODUCTGRADE></CFSUBPRODUCTGRADE>
                    <ABNORMALCODELIST>
                        <CODE>
                            <ABNORMALSEQ></ABNORMALSEQ>
                            <ABNORMALCODE></ABNORMALCODE>
                        </CODE>
                    </ABNORMALCODELIST>
                    <GROUPID></GROUPID>
                    <PRODUCTTYPE></PRODUCTTYPE>
                    <LCDROPLIST>
                        <LCDROPAMOUNT></LCDROPAMOUNT>
                    </LCDROPLIST>
                    <DUMUSEDCOUNT></DUMUSEDCOUNT>
                    <CFTYPE1REPAIRCOUNT></CFTYPE1REPAIRCOUNT>
                    <CFTYPE2REPAIRCOUNT></CFTYPE2REPAIRCOUNT>
                    <CARBONREPAIRCOUNT></CARBONREPAIRCOUNT>
                    <LASERREPAIRCOUNT></LASERREPAIRCOUNT>
                    <ITOSIDEFLAG></ITOSIDEFLAG>
                    <REWORKCOUNT></REWORKCOUNT>
                    <REWORKLIST>
                        <REWORK>
                            <REWORKFLOWNAME></REWORKFLOWNAME>
                            <REWORKCOUNT></REWORKCOUNT>
                        </REWORK>
                    </REWORKLIST>
                    <SHORTCUTFLAG></SHORTCUTFLAG>
                    <OWNERTYPE></OWNERTYPE>
                    <OWNERID></OWNERID>
                    <REVPROCESSOPERATIONNAME></REVPROCESSOPERATIONNAME>
                    <TARGETPORTNAME></TARGETPORTNAME>
                    <SPUTTEREQPMAKER></SPUTTEREQPMAKER>
                    <CHAMBERRUNMODE></CHAMBERRUNMODE>
                    <TEMPERATUREFLAG></TEMPERATUREFLAG>
                    <MACHINEPROCESSSEQ></MACHINEPROCESSSEQ>
                    <SCRAPCUTFLAG></SCRAPCUTFLAG>
                    <PPID></PPID>
                    <FMAFLAG></FMAFLAG>
                    <MHUFLAG></MHUFLAG>
                    <DEFECTLIST>
                      <DEFECT>
                        <SUBPRODUCTNAME></SUBPRODUCTNAME>
                        <ARRAYDEFECTCODES></ ARRAYDEFECTCODES >
                        <ARRAYDEFECTADDRESS></ARRAYDEFECTADDRESS>
                        <CFDEFECTCODES></CFDEFECTCODES>
                        <CFDEFECTADDRESS></CFDEFECTADDRESS>
                        <PIDEFECTCODES></PIDEFECTCODES>
                        <PIDEFECTADDRESS></PIDEFECTADDRESS>
                        <ODFDEFECTCODES></ODFDEFECTCODES>
                        <ODFDEFECTADDRESS></ODFDEFECTADDRESS>
                      </DEFECT>
                    </DEFECTLIST>
                    <ARRAYPRODUCTSPECVER></ARRAYPRODUCTSPECVER>
                    <AGINGENABLE></AGINGENABLE>
                    <PROCESSFLAG></PROCESSFLAG>
                </PRODUCT>
            </PRODUCTLIST>
        </LOT>
    </LOTLIST>
</BODY>
<RETURN>
    <RETURNCODE></RETURNCODE>
    <RETURNMESSAGE></RETURNMESSAGE>
</RETURN>
</MESSAGE>";
                xml_doc.LoadXml(xml);
#endregion
                XmlDocument ret = ActiveSocketService.JobShortCutPermit(job, xml);

                PassiveSocketService.PASSIVE_JOBShortCutPermit(ret);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
