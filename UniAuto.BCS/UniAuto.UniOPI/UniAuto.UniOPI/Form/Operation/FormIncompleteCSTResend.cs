using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormIncompleteCSTResend : FormBase
    {
        enum eIncompleteType
        {
            Cassette = 0,
            AbnormalCST = 1,
            Box = 2            
        }

        eIncompleteType IncompleteType;

        TreeNode CurrentNode;

        Dictionary<string, Recipe> dicRecipe_All;

        ContextMenuStrip MemuStrip;

        string CurFileName = string.Empty;
        string CurMESTrxID = string.Empty;
        string CurCassetteID = string.Empty;

        public FormIncompleteCSTResend()
        {
            InitializeComponent();
        }

        private void FormIncompleteCSTResend_Load(object sender, EventArgs e)
        {
            try
            {
                trvData.Dock = DockStyle.Fill;

                ClrearData();

                LoadIncompleteCassette();

                dicRecipe_All = new Dictionary<string, Recipe>();

                #region Create ContextMenuStrip

                MemuStrip = new ContextMenuStrip();

                MemuStrip.Items.Add("Copy To New");
                MemuStrip.Items.Add("Modify");
                MemuStrip.Items.Add("Delete");

                MemuStrip.ItemClicked += new ToolStripItemClickedEventHandler(cmsMemu_ItemClicked);

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                  
            }
        }

        private void btnRefreshIncompleteCST_Click(object sender, EventArgs e)
        {
            try
            {
                ClrearData();

                LoadIncompleteCassette();

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvIncompeteCst_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                ClrearData();

                if (e.RowIndex < 0) return;

                if (dgvIncompeteCst.CurrentRow != null)
                {
                    
                    string _portID = dgvIncompeteCst.CurrentRow.Cells[colPortID.Name].Value.ToString();

                    var _port = FormMainMDI.G_OPIAp.Dic_Port.Values.Where(r => r.PortID.Equals(_portID));

                    if (_port.Count() == 0)
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Can't find Port ID [{0}]", _portID), MessageBoxIcon.Error);
                        return; 
                    }

                    //CurPort = _port.First();                    

                    CurFileName = dgvIncompeteCst.CurrentRow.Cells[colFileName.Name].Value.ToString();
                    CurMESTrxID = dgvIncompeteCst.CurrentRow.Cells[colMESTrxID.Name].Value.ToString();
                    CurCassetteID = dgvIncompeteCst.CurrentRow.Cells[colCassetteID.Name].Value.ToString();

                    string[] _data = CurFileName.Split('_');

                    if (_data[0] == "BOX") IncompleteType = eIncompleteType.Box;
                    else if (_data[0] == "ABN") IncompleteType = eIncompleteType.AbnormalCST;
                    else IncompleteType = eIncompleteType.Cassette;

                    ShowObjectbyType(IncompleteType);

                    if (IncompleteType == eIncompleteType.Box)
                    {
                        SendtoBC_IncompleteDenseDataRequest(dgvIncompeteCst.CurrentRow);
                    }
                    else
                    {
                        SendtoBC_IncompleteCassetteDataRequest(dgvIncompeteCst.CurrentRow);
                    }                    
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            try
            {
                //if (CurPort == null) return;

                if (CurFileName == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("File Name is Empty"), MessageBoxIcon.Error);
                    return;
                }

                if (CurMESTrxID == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("MES Trx ID is Empty"), MessageBoxIcon.Error);
                    return;
                }

                if (IncompleteType == eIncompleteType.Box)
                {
                    SendToBC_IncompleteBoxEditSave();
                }
                else
                {
                    SendToBC_IncompleteCassetteEditSave();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void CassetteCommand_Click(object sender, EventArgs e)
        {
            try
            {
                //if (CurPort == null) return;

                Button _btn = (Button)sender;

                if (CurFileName == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("File Name is Empty"), MessageBoxIcon.Error);
                    return;
                }

                if (CurMESTrxID == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("MES Trx ID is Empty"), MessageBoxIcon.Error);
                    return;
                }

                if (CurCassetteID == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("Cassette ID is Empty"), MessageBoxIcon.Error);
                    return;
                }

                SendToBc_IncompleteCassetteCommand(_btn.Tag.ToString(), CurMESTrxID, CurFileName, CurCassetteID);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SendtoBC_IncompleteCassetteDataRequest(DataGridViewRow CurRow)
        {
            try
            {
                IncompleteCassetteDataRequest _trx = new IncompleteCassetteDataRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.CASSETTEID = CurRow.Cells[colCassetteID.Name].Value.ToString();
                _trx.BODY.PORTID = CurRow.Cells[colPortID.Name].Value.ToString();
                _trx.BODY.INCOMPLETEDATE = CurRow.Cells[colUpdateTime.Name].Value.ToString();
                _trx.BODY.MESTRXID = CurRow.Cells[colMESTrxID.Name].Value.ToString();
                _trx.BODY.FILENAME = CurRow.Cells[colFileName.Name].Value.ToString();
                string _xml = _trx.WriteToXml();

                #region Send IncompleteCassetteDataReply

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region IncompleteCassetteDataReply

                string _respXml = _resp.Xml;

                //_respXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<MESSAGE>\r\n  <HEADER>\r\n    <MESSAGENAME>IncompleteCassetteDataReply</MESSAGENAME>\r\n    <TRANSACTIONID>20150422194939387</TRANSACTIONID>\r\n    <REPLYSUBJECTNAME>127.0.0.1:9000</REPLYSUBJECTNAME>\r\n    <INBOXNAME />\r\n    <LISTENER />\r\n  </HEADER>\r\n  <BODY>\r\n    <LINENAME>CBPMT100</LINENAME>\r\n    <INCOMPLETEDATE>2015-04-15 23:56:16</INCOMPLETEDATE>\r\n    <PORTID>05</PORTID>\r\n    <CASSETTEID>TBA176</CASSETTEID>\r\n    <CARRIERNAME>TBA176</CARRIERNAME>\r\n    <LINERECIPENAME>FDUMDBARAC51ZA</LINERECIPENAME>\r\n    <HOSTLINERECIPENAME>FDUMDBARAC51ZA</HOSTLINERECIPENAME>\r\n    <PPID>L2:AA;L3:ZA;L4:ZA;L5:ZA;L6:ZA;L7:ZA;L8:ZA;L9:ZA;L10:ZA;L11:ZA;L12:ZA;L13:ZA;L14:ZA;L15:ZA;L16:ZA;L17:ZA;L18:AA</PPID>\r\n    <HOSTPPID>L2:AA;L3:ZA;L4:ZA;L5:ZA;L6:ZA;L7:ZA;L8:ZA;L9:ZA;L10:ZA;L11:ZA;L12:ZA;L13:ZA;L14:ZA;L15:ZA;L16:ZA;L17:ZA;L18:AA</HOSTPPID>\r\n    <RETURNMSG>Cassette[TBA176] does not contain any Lot which is waiting to operation can be processed on Machine[FBMPH100].</RETURNMSG>\r\n    <MESTRXID>20150415235615242</MESTRXID>\r\n    <FILENAME>05_TBA176_20150415235615242</FILENAME>\r\n    <INCOMPLETECASSETTEDATALIST>\r\n      <PRODUCT>\r\n        <POSITION>1</POSITION>\r\n        <PRODUCTNAME>FB530019AZ</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530019AZ</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151173Q16240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520009N0E</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>2</POSITION>\r\n        <PRODUCTNAME>FB520016AT</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520016AT</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151163Q00140</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520009N0E</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>3</POSITION>\r\n        <PRODUCTNAME>FB530019AN</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530019AN</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151173Q16240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0V</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>4</POSITION>\r\n        <PRODUCTNAME>FB520013AA</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520013AA</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151163Q00140</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>5</POSITION>\r\n        <PRODUCTNAME>FB530028AC</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530028AC</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105152163Q02240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0V</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>6</POSITION>\r\n        <PRODUCTNAME>FB530028AF</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530028AF</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105152163Q02240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>7</POSITION>\r\n        <PRODUCTNAME>FB520009AD</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520009AD</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151163Q00140</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>8</POSITION>\r\n        <PRODUCTNAME>FB520009AM</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520009AM</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151163Q00140</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>9</POSITION>\r\n        <PRODUCTNAME>FB530014AP</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530014AP</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151173Q18240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>10</POSITION>\r\n        <PRODUCTNAME>FB530015BC</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530015BC</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151173Q18240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>11</POSITION>\r\n        <PRODUCTNAME>FB530016AY</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530016AY</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151173Q18240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>12</POSITION>\r\n        <PRODUCTNAME>FB530014AM</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530014AM</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151173Q18240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>13</POSITION>\r\n        <PRODUCTNAME>FB530019AU</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530019AU</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151173Q16240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>14</POSITION>\r\n        <PRODUCTNAME>FB530014AS</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530014AS</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151173Q18240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>15</POSITION>\r\n        <PRODUCTNAME>FB520013AB</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520013AB</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151163Q00140</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>NG</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>16</POSITION>\r\n        <PRODUCTNAME>FB530020AW</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530020AW</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151173Q16240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>17</POSITION>\r\n        <PRODUCTNAME>FB530020AZ</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530020AZ</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151173Q16240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>18</POSITION>\r\n        <PRODUCTNAME>FB530031AU</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530031AU</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105152163Q02240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>19</POSITION>\r\n        <PRODUCTNAME>FB520013AG</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520013AG</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151163Q00140</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>20</POSITION>\r\n        <PRODUCTNAME>FB530031AW</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530031AW</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105152163Q02240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>21</POSITION>\r\n        <PRODUCTNAME>FB520011AZ</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520011AZ</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151163Q00140</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>22</POSITION>\r\n        <PRODUCTNAME>FB530014AQ</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530014AQ</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151173Q18240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>23</POSITION>\r\n        <PRODUCTNAME>FB520008AX</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520008AX</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151163Q00140</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>24</POSITION>\r\n        <PRODUCTNAME>FB520017AB</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520017AB</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151163Q00140</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>25</POSITION>\r\n        <PRODUCTNAME>FB520026AD</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520026AD</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3120103151223FDX240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>26</POSITION>\r\n        <PRODUCTNAME>FB520009AG</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520009AG</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151163Q00140</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>27</POSITION>\r\n        <PRODUCTNAME>FB530031AS</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB530031AS</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105152163Q02240</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0Y</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n      <PRODUCT>\r\n        <POSITION>28</POSITION>\r\n        <PRODUCTNAME>FB520013AD</PRODUCTNAME>\r\n        <HOSTPRODUCTNAME>FB520013AD</HOSTPRODUCTNAME>\r\n        <DENSEBOXID>S3110105151163Q00140</DENSEBOXID>\r\n        <PRODUCTJUDGE />\r\n        <PRODUCTGRADE>RW</PRODUCTGRADE>\r\n        <SUBPRODUCTGRADES>OOOOOO</SUBPRODUCTGRADES>\r\n        <PAIRPRODUCTNAME />\r\n        <LOTNAME>FB520011N0U</LOTNAME>\r\n        <PRODUCTRECIPENAME>FDUMDBARAC51ZA</PRODUCTRECIPENAME>\r\n        <HOSTPRODUCTRECIPENAME>FDUMDBARAC51ZA</HOSTPRODUCTRECIPENAME>\r\n        <PRODUCTSPECNAME>FBBAR05DUM00</PRODUCTSPECNAME>\r\n        <PROCESSOPERATIONNAME>DBAR</PROCESSOPERATIONNAME>\r\n        <PRODUCTOWNER>M</PRODUCTOWNER>\r\n        <VCRREADFLAG />\r\n        <SHORTCUTFLAG />\r\n        <SAMPLEFLAG>N</SAMPLEFLAG>\r\n        <PROCESSFLAG>Y</PROCESSFLAG>\r\n        <PROCESSCOMMUNICATIONSTATE>OnLineRemote</PROCESSCOMMUNICATIONSTATE>\r\n      </PRODUCT>\r\n    </INCOMPLETECASSETTEDATALIST>\r\n  </BODY>\r\n  <RETURN>\r\n    <RETURNCODE>0000000</RETURNCODE>\r\n    <RETURNMESSAGE />\r\n  </RETURN>\r\n</MESSAGE>";

                IncompleteCassetteDataReply _incompleteCassetteDataReply = (IncompleteCassetteDataReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data
                txtIncompleteDate.Text = _incompleteCassetteDataReply.BODY.INCOMPLETEDATE.ToString();
                txtPortID.Text = _incompleteCassetteDataReply.BODY.PORTID.ToString();
                txtCassetteID.Text = _incompleteCassetteDataReply.BODY.CASSETTEID.ToString();
                txtCarrierName.Text = _incompleteCassetteDataReply.BODY.CARRIERNAME.ToString();
                txtLineRecipeName.Text = _incompleteCassetteDataReply.BODY.LINERECIPENAME.ToString();
                txtHostLineRecipeName.Text = _incompleteCassetteDataReply.BODY.HOSTLINERECIPENAME.ToString();
                txtPPID.Text = _incompleteCassetteDataReply.BODY.PPID.ToString();
                txtHostPPID.Text = _incompleteCassetteDataReply.BODY.HOSTPPID.ToString();
                txtReturnMsg.Text = _incompleteCassetteDataReply.BODY.RETURNMSG.ToString();
                txtSampleFlag.Text = string.Empty;

                foreach (IncompleteCassetteDataReply.PRODUCTc _prod in _incompleteCassetteDataReply.BODY.INCOMPLETECASSETTEDATALIST)
                {
                    TreeNode _subNode = new TreeNode();
                    _subNode.Name = "POSITION";
                    _subNode.Text = string.Format("Position [ {0} ] - [ {1} ]", _prod.POSITION, _prod.PRODUCTNAME);
                    _subNode.Tag = _prod.POSITION;

                    #region Detail
                    TreeNode _detailNode = new TreeNode();
                    _detailNode.Name = "PRODUCTNAME";
                    _detailNode.Text = string.Format("Product Name [ {0} ]", _prod.PRODUCTNAME);
                    _detailNode.Tag = _prod.PRODUCTNAME;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "HOSTPRODUCTNAME";
                    _detailNode.Text = string.Format("Host Product Name [ {0} ]", _prod.HOSTPRODUCTNAME);
                    _detailNode.Tag = _prod.HOSTPRODUCTNAME;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "DENSEBOXID";
                    _detailNode.Text = string.Format("Dense Box ID [ {0} ]", _prod.DENSEBOXID);
                    _detailNode.Tag = _prod.DENSEBOXID;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "PRODUCTJUDGE";
                    _detailNode.Text = string.Format("Product Judge [ {0} ]", _prod.PRODUCTJUDGE);
                    _detailNode.Tag = _prod.PRODUCTJUDGE;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "PRODUCTGRADE";
                    _detailNode.Text = string.Format("Product Grade [ {0} ]", _prod.PRODUCTGRADE);
                    _detailNode.Tag = _prod.PRODUCTGRADE;
                    _subNode.Nodes.Add(_detailNode);

                    //_detailNode = new TreeNode();
                    //_detailNode.Name = "SUBPRODUCTGRADES";
                    //_detailNode.Text = string.Format("Sub Product Grades [ {0} ]", _prod.SUBPRODUCTGRADES);
                    //_detailNode.Tag = _prod.SUBPRODUCTGRADES;
                    //_subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "PAIRPRODUCTNAME";
                    _detailNode.Text = string.Format("Pair Product Name [ {0} ]", _prod.PAIRPRODUCTNAME);
                    _detailNode.Tag = _prod.PAIRPRODUCTNAME;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "LOTNAME";
                    _detailNode.Text = string.Format("Lot Name [ {0} ]", _prod.LOTNAME);
                    _detailNode.Tag = _prod.LOTNAME;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "PRODUCTRECIPENAME";
                    _detailNode.Text = string.Format("Product Recipe Name [ {0} ]", _prod.PRODUCTRECIPENAME);
                    _detailNode.Tag = _prod.PRODUCTRECIPENAME;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "HOSTPRODUCTRECIPENAME";
                    _detailNode.Text = string.Format("Host Product Recipe Name [ {0} ]", _prod.HOSTPRODUCTRECIPENAME);
                    _detailNode.Tag = _prod.HOSTPRODUCTRECIPENAME;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "PRODUCTSPECNAME";
                    _detailNode.Text = string.Format("Product Spec Name [ {0} ]", _prod.PRODUCTSPECNAME);
                    _detailNode.Tag = _prod.PRODUCTSPECNAME;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "PROCESSOPERATIONNAME";
                    _detailNode.Text = string.Format("Process Operation Name [ {0} ]", _prod.PROCESSOPERATIONNAME);
                    _detailNode.Tag = _prod.PROCESSOPERATIONNAME;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "PRODUCTOWNER";
                    _detailNode.Text = string.Format("Product Owner [ {0} ]", _prod.PRODUCTOWNER);
                    _detailNode.Tag = _prod.PRODUCTOWNER;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "VCRREADFLAG";
                    _detailNode.Text = string.Format("VCR Read Flag [ {0} ]", _prod.VCRREADFLAG);
                    _detailNode.Tag = _prod.VCRREADFLAG;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "SHORTCUTFLAG";
                    _detailNode.Text = string.Format("Short Cut Flag [ {0} ]", _prod.SHORTCUTFLAG);
                    _detailNode.Tag = _prod.SHORTCUTFLAG;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "SAMPLEFLAG";
                    _detailNode.Text = string.Format("Sample Flag [ {0} ]", _prod.SAMPLEFLAG);
                    _detailNode.Tag = _prod.SAMPLEFLAG;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "PROCESSFLAG";
                    _detailNode.Text = string.Format("Process Flag [ {0} ]", _prod.SAMPLEFLAG);
                    _detailNode.Tag = _prod.SAMPLEFLAG;
                    _subNode.Nodes.Add(_detailNode);

                    _detailNode = new TreeNode();
                    _detailNode.Name = "PROCESSCOMMUNICATIONSTATE";
                    _detailNode.Text = string.Format("Process Communication State [ {0} ]", _prod.PROCESSCOMMUNICATIONSTATE);
                    _detailNode.Tag = _prod.PROCESSCOMMUNICATIONSTATE;
                    _subNode.Nodes.Add(_detailNode);

                    #endregion

                    trvData.Nodes.Add(_subNode);
                }
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SendtoBC_IncompleteDenseDataRequest(DataGridViewRow CurRow)
        {
            try
            {
                IncompleteBoxDataRequest _trx = new IncompleteBoxDataRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PORTID = CurRow.Cells[colPortID.Name].Value.ToString();
                _trx.BODY.INCOMPLETEDATE = CurRow.Cells[colUpdateTime.Name].Value.ToString();
                _trx.BODY.MESTRXID = CurRow.Cells[colMESTrxID.Name].Value.ToString();
                _trx.BODY.FILENAME = CurRow.Cells[colFileName.Name].Value.ToString();
                string _xml = _trx.WriteToXml();

                #region Send IncompleteBoxDataReply

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region IncompleteBoxDataReply

                string _respXml = _resp.Xml;

                //_respXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<MESSAGE>\r\n  <HEADER>\r\n    <MESSAGENAME>IncompleteBoxDataReply</MESSAGENAME>\r\n    <TRANSACTIONID>20150414231250017</TRANSACTIONID>\r\n    <REPLYSUBJECTNAME>127.0.0.1:5559</REPLYSUBJECTNAME>\r\n    <INBOXNAME />\r\n    <LISTENER />\r\n  </HEADER>\r\n  <BODY>\r\n  <LINENAME>FBMPH100</LINENAME>\r\n  <INCOMPLETEDATE>2015-04-22 13:10:40</INCOMPLETEDATE>\r\n  <PORTID>01</PORTID >\r\n  <MESTRXID></MESTRXID >\r\n  <FILENAME></FILENAME >\r\n  <LINERECIPENAME></LINERECIPENAME>\r\n  <HOSTLINERECIPENAME></HOSTLINERECIPENAME>\r\n  <PPID></PPID>\r\n  <HOSTPPID></HOSTPPID>\r\n  <BOXQUANTITY></BOXQUANTITY>\r\n  <SAMPLEFLAG></SAMPLEFLAG>\r\n  <RETURNMSG></RETURNMSG>\r\n  <BOXLIST>\r\n     <BOX>\r\n\t\t<BOXNAME>BOX01</BOXNAME>\r\n\t\t<PRODUCTQUANTITY>2</PRODUCTQUANTITY>\r\n        <PRODUCTLIST>\r\n\t        <PRODUCT>\r\n\t\t\t\t<POSITION>1</POSITION>\r\n      \t\t\t<PRODUCTNAME>BOX01_PRODUCTNAME01</PRODUCTNAME>\r\n\t\t\t    <HOSTPRODUCTNAME>BOX01_PRODUCTNAME01_HOST</HOSTPRODUCTNAME>\r\n\t\t\t\t<SHORTCUTFLAG>Y</SHORTCUTFLAG>\t\t\r\n                <BOXULDFLAG>Y</BOXULDFLAG>\r\n                <DPIPROCESSFLAG>Y</DPIPROCESSFLAG>\r\n                <RTPFLAG>Y</RTPFLAG>\r\n                <PPID>L2:01</PPID>\r\n                <HOSTPPID>L2:01</HOSTPPID>\r\n\t\t\t    <ABNORMALCODELIST>\r\n\t\t\t\t   <ABNORMALCODE>\r\n\t\t\t\t     <ABNORMALSEQ>1</ABNORMALSEQ>\r\n\t\t\t\t     <ABNORMALCODE>11</ABNORMALCODE>\r\n\t\t\t\t   </ABNORMALCODE>\r\n\t\t\t    </ABNORMALCODELIST>\r\n\t\t\t</PRODUCT> \r\n\t        <PRODUCT>\r\n\t\t\t\t<POSITION>2</POSITION>\r\n      \t\t\t<PRODUCTNAME>BOX01_PRODUCTNAME02</PRODUCTNAME>\r\n\t\t\t    <HOSTPRODUCTNAME>BOX01_PRODUCTNAME02_HOST</HOSTPRODUCTNAME>\r\n\t\t\t\t<SHORTCUTFLAG>Y</SHORTCUTFLAG>\t\t\r\n                <BOXULDFLAG>Y</BOXULDFLAG>\r\n                <DPIPROCESSFLAG>Y</DPIPROCESSFLAG>\r\n                <RTPFLAG>Y</RTPFLAG>\r\n                <PPID>L2:01</PPID>\r\n                <HOSTPPID>L2:01</HOSTPPID>\r\n\t\t\t    <ABNORMALCODELIST>\r\n\t\t\t\t   <ABNORMALCODE>\r\n\t\t\t\t     <ABNORMALSEQ>1</ABNORMALSEQ>\r\n\t\t\t\t     <ABNORMALCODE>11</ABNORMALCODE>\r\n\t\t\t\t   </ABNORMALCODE>\r\n\t\t\t    </ABNORMALCODELIST>\r\n\t\t\t</PRODUCT> \t\t\t\r\n\t\t</PRODUCTLIST>\r\n\t</BOX>\r\n     <BOX>\r\n\t\t<BOXNAME>BOX02</BOXNAME>\r\n\t\t<PRODUCTQUANTITY>3</PRODUCTQUANTITY>\r\n        <PRODUCTLIST>\r\n\t        <PRODUCT>\r\n\t\t\t\t<POSITION>1</POSITION>\r\n      \t\t\t<PRODUCTNAME>BOX02_PRODUCTNAME01</PRODUCTNAME>\r\n\t\t\t    <HOSTPRODUCTNAME>BOX02_PRODUCTNAME01_HOST</HOSTPRODUCTNAME>\r\n\t\t\t\t<SHORTCUTFLAG>Y</SHORTCUTFLAG>\t\t\r\n                <BOXULDFLAG>Y</BOXULDFLAG>\r\n                <DPIPROCESSFLAG>Y</DPIPROCESSFLAG>\r\n                <RTPFLAG>Y</RTPFLAG>\r\n                <PPID>L2:01</PPID>\r\n                <HOSTPPID>L2:01</HOSTPPID>\r\n\t\t\t    <ABNORMALCODELIST>\r\n\t\t\t\t   <ABNORMALCODE>\r\n\t\t\t\t     <ABNORMALSEQ>1</ABNORMALSEQ>\r\n\t\t\t\t     <ABNORMALCODE>11</ABNORMALCODE>\r\n\t\t\t\t   </ABNORMALCODE>\r\n\t\t\t    </ABNORMALCODELIST>\r\n\t\t\t</PRODUCT> \r\n\t        <PRODUCT>\r\n\t\t\t\t<POSITION>2</POSITION>\r\n      \t\t\t<PRODUCTNAME>BOX02_PRODUCTNAME02</PRODUCTNAME>\r\n\t\t\t    <HOSTPRODUCTNAME>BOX02_PRODUCTNAME02_HOST</HOSTPRODUCTNAME>\r\n\t\t\t\t<SHORTCUTFLAG>Y</SHORTCUTFLAG>\t\t\r\n                <BOXULDFLAG>Y</BOXULDFLAG>\r\n                <DPIPROCESSFLAG>Y</DPIPROCESSFLAG>\r\n                <RTPFLAG>Y</RTPFLAG>\r\n                <PPID>L2:01</PPID>\r\n                <HOSTPPID>L2:01</HOSTPPID>\r\n\t\t\t    <ABNORMALCODELIST>\r\n\t\t\t\t   <ABNORMALCODE>\r\n\t\t\t\t     <ABNORMALSEQ>1</ABNORMALSEQ>\r\n\t\t\t\t     <ABNORMALCODE>11</ABNORMALCODE>\r\n\t\t\t\t   </ABNORMALCODE>\r\n\t\t\t\t   <ABNORMALCODE>\r\n\t\t\t\t     <ABNORMALSEQ>2</ABNORMALSEQ>\r\n\t\t\t\t     <ABNORMALCODE>22</ABNORMALCODE>\r\n\t\t\t\t   </ABNORMALCODE>\r\n\t\t\t    </ABNORMALCODELIST>\r\n\t\t\t</PRODUCT> \r\n\t        <PRODUCT>\r\n\t\t\t\t<POSITION>3</POSITION>\r\n      \t\t\t<PRODUCTNAME>BOX03_PRODUCTNAME02</PRODUCTNAME>\r\n\t\t\t    <HOSTPRODUCTNAME>BOX03_PRODUCTNAME02_HOST</HOSTPRODUCTNAME>\r\n\t\t\t\t<SHORTCUTFLAG>Y</SHORTCUTFLAG>\t\t\r\n                <BOXULDFLAG>Y</BOXULDFLAG>\r\n                <DPIPROCESSFLAG>Y</DPIPROCESSFLAG>\r\n                <RTPFLAG>Y</RTPFLAG>\r\n                <PPID>L2:01</PPID>\r\n                <HOSTPPID>L2:01</HOSTPPID>\r\n\t\t\t    <ABNORMALCODELIST>\r\n\t\t\t\t   <ABNORMALCODE>\r\n\t\t\t\t     <ABNORMALSEQ>1</ABNORMALSEQ>\r\n\t\t\t\t     <ABNORMALCODE>11</ABNORMALCODE>\r\n\t\t\t\t   </ABNORMALCODE>\r\n\t\t\t    </ABNORMALCODELIST>\r\n\t\t\t</PRODUCT> \t\t\t\r\n\t\t</PRODUCTLIST>\r\n\t</BOX>\t\r\n\t</BOXLIST>\r\n  </BODY>\r\n  <RETURN>\r\n    <RETURNCODE>0000000</RETURNCODE>\r\n    <RETURNMESSAGE />\r\n  </RETURN>\r\n</MESSAGE>";

                IncompleteBoxDataReply _incompleteBoxDataReply = (IncompleteBoxDataReply)Spec.CheckXMLFormat(_respXml);
  
                #region Update Data
                txtIncompleteDate.Text = _incompleteBoxDataReply.BODY.INCOMPLETEDATE.ToString();
                txtPortID.Text = _incompleteBoxDataReply.BODY.PORTID.ToString();
                txtSampleFlag.Text = _incompleteBoxDataReply.BODY.SAMPLEFLAG.ToString();
                txtLineRecipeName.Text = _incompleteBoxDataReply.BODY.LINERECIPENAME.ToString();
                txtHostLineRecipeName.Text = _incompleteBoxDataReply.BODY.HOSTLINERECIPENAME.ToString();
                txtPPID.Text = _incompleteBoxDataReply.BODY.PPID.ToString();
                txtHostPPID.Text = _incompleteBoxDataReply.BODY.HOSTPPID.ToString();
                txtReturnMsg.Text = _incompleteBoxDataReply.BODY.RETURNMSG.ToString();

                foreach (IncompleteBoxDataReply.BOXc _box in _incompleteBoxDataReply.BODY.BOXLIST)
                {
                    TreeNode _boxNode = new TreeNode();
                    _boxNode.Name = "BOXNAME";
                    _boxNode.Text = string.Format("Box Name [ {0} ]", _box.BOXNAME);
                    _boxNode.Tag = _box.BOXNAME;

                    foreach (IncompleteBoxDataReply.PRODUCTc _prod in _box.PRODUCTLIST)
                    {
                        TreeNode _subNode = new TreeNode();
                        _subNode.Name = "POSITION";
                        _subNode.Text = string.Format("Position [ {0} ] - [ {1} ]", _prod.POSITION, _prod.PRODUCTNAME);
                        _subNode.Tag = _prod.POSITION;
                        
                        #region Detail
                        TreeNode _detailNode = new TreeNode();
                        _detailNode.Name = "PRODUCTNAME";
                        _detailNode.Text = string.Format("Product Name [ {0} ]", _prod.PRODUCTNAME);
                        _detailNode.Tag = _prod.PRODUCTNAME;
                        _subNode.Nodes.Add(_detailNode);

                        _detailNode = new TreeNode();
                        _detailNode.Name = "HOSTPRODUCTNAME";
                        _detailNode.Text = string.Format("Host Product Name [ {0} ]", _prod.HOSTPRODUCTNAME);
                        _detailNode.Tag = _prod.HOSTPRODUCTNAME;
                        _subNode.Nodes.Add(_detailNode);

                        _detailNode = new TreeNode();
                        _detailNode.Name = "SHORTCUTFLAG";
                        _detailNode.Text = string.Format("Short Cut Flag [ {0} ]", _prod.SHORTCUTFLAG);
                        _detailNode.Tag = _prod.SHORTCUTFLAG;
                        _subNode.Nodes.Add(_detailNode);

                        _detailNode = new TreeNode();
                        _detailNode.Name = "BOXULDFLAG";
                        _detailNode.Text = string.Format("Box ULD Flag [ {0} ]", _prod.BOXULDFLAG);
                        _detailNode.Tag = _prod.BOXULDFLAG;
                        _subNode.Nodes.Add(_detailNode);
                        
                        _detailNode = new TreeNode();
                        _detailNode.Name = "DPIPROCESSFLAG";
                        _detailNode.Text = string.Format("DPI Process Flag [ {0} ]", _prod.DPIPROCESSFLAG);
                        _detailNode.Tag = _prod.DPIPROCESSFLAG;
                        _subNode.Nodes.Add(_detailNode);

                        _detailNode = new TreeNode();
                        _detailNode.Name = "RTPFLAG";
                        _detailNode.Text = string.Format("RTP Flag [ {0} ]", _prod.RTPFLAG);
                        _detailNode.Tag = _prod.RTPFLAG;
                        _subNode.Nodes.Add(_detailNode);

                        _detailNode = new TreeNode();
                        _detailNode.Name = "PPID";
                        _detailNode.Text = string.Format("PPID [ {0} ]", _prod.PPID);
                        _detailNode.Tag = _prod.PPID;
                        _subNode.Nodes.Add(_detailNode);

                        _detailNode = new TreeNode();
                        _detailNode.Name = "HOSTPPID";
                        _detailNode.Text = string.Format("Host PPID [ {0} ]", _prod.HOSTPPID);
                        _detailNode.Tag = _prod.HOSTPPID;
                        _subNode.Nodes.Add(_detailNode);

                        #region Abnormal
                        TreeNode _abnormalNode = new TreeNode();
                        _abnormalNode.Name = "ABNORMALCODELIST";
                        _abnormalNode.Text = "ABNORMALCODELIST";
                        _subNode.Nodes.Add(_abnormalNode);
                        TreeNode _abnormalDetail;
                        foreach (IncompleteBoxDataReply.ABNORMALCODEc _abnormal in _prod.ABNORMALCODELIST)
                        {
                            _abnormalDetail = new TreeNode();
                            _abnormalDetail.Name = "ABNORMALCODE";
                            _abnormalDetail.Text = string.Format("{0}:{1}", _abnormal.ABNORMALSEQ, _abnormal.ABNORMALCODE);
                            _abnormalDetail.Tag = string.Format("{0}:{1}", _abnormal.ABNORMALSEQ, _abnormal.ABNORMALCODE);
                            _abnormalNode.Nodes.Add(_abnormalDetail);
                        }
                        #endregion

                        #endregion

                        _boxNode.Nodes.Add(_subNode);
                    }

                    trvData.Nodes.Add(_boxNode);
                }

                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SendToBc_IncompleteCassetteCommand(string Command, string MESTrxID, string FileName, string CassetteID)
        {
            string _err = "";
            try
            {
                IncompleteCassetteCommand _trx = new IncompleteCassetteCommand();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                _trx.BODY.INCOMPLETEDATE = txtIncompleteDate.Text.ToString();  
                _trx.BODY.PORTID = txtPortID.Text.ToString();
                _trx.BODY.CASSETTEID = CassetteID; 
                _trx.BODY.MESTRXID = MESTrxID;
                _trx.BODY.FILENAME = FileName;
                _trx.BODY.COMMAND = Command;

                string _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SendToBC_IncompleteCassetteEditSave()
        {
             try
            {
                IncompleteCassetteEditSave _trx = new IncompleteCassetteEditSave();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                _trx.BODY.INCOMPLETEDATE = txtIncompleteDate.Text.ToString(); 
                _trx.BODY.PORTID = txtPortID.Text.ToString();  
                _trx.BODY.CASSETTEID = txtCassetteID.Text.ToString();  
                _trx.BODY.CARRIERNAME = txtCarrierName.Text.ToString(); 
                _trx.BODY.LINERECIPENAME = txtLineRecipeName.Text.ToString();  
                _trx.BODY.HOSTLINERECIPENAME = txtHostLineRecipeName.Text.ToString(); 
                _trx.BODY.PPID = txtPPID.Text.ToString(); 
                _trx.BODY.HOSTPPID = txtHostPPID.Text.ToString();
                _trx.BODY.MESTRXID = CurMESTrxID;  
                _trx.BODY.FILENAME = CurFileName;

                foreach (TreeNode _cstNode in trvData.Nodes)
                {
                    IncompleteCassetteEditSave.PRODUCTc _product = new IncompleteCassetteEditSave.PRODUCTc();
                    _product.POSITION = _cstNode.Tag.ToString();

                    foreach (TreeNode _detailNode in _cstNode.Nodes)
                    {
                        switch (_detailNode.Name)
                        {
                            case "PRODUCTNAME":
                                _product.PRODUCTNAME = _detailNode.Tag.ToString();
                                break;
                            case "HOSTPRODUCTNAME":
                                _product.HOSTPRODUCTNAME = _detailNode.Tag.ToString();
                                break;
                            case "DENSEBOXID":
                                _product.DENSEBOXID = _detailNode.Tag.ToString();
                                break;
                            case "PRODUCTJUDGE":
                                _product.PRODUCTJUDGE = _detailNode.Tag.ToString();
                                break;
                            case "PRODUCTGRADE":
                                _product.PRODUCTGRADE = _detailNode.Tag.ToString();
                                break;
                            //case "SUBPRODUCTGRADES":
                            //    _product.SUBPRODUCTGRADES = _detailNode.Tag.ToString();
                            //    break;
                            case "PAIRPRODUCTNAME":
                                _product.PAIRPRODUCTNAME = _detailNode.Tag.ToString();
                                break;
                            case "LOTNAME":
                                _product.LOTNAME = _detailNode.Tag.ToString();
                                break;
                            case "PRODUCTRECIPENAME":
                                _product.PRODUCTRECIPENAME = _detailNode.Tag.ToString();
                                break;
                            case "HOSTPRODUCTRECIPENAME":
                                _product.HOSTPRODUCTRECIPENAME = _detailNode.Tag.ToString();
                                break;
                            case "PRODUCTSPECNAME":
                                _product.PRODUCTSPECNAME = _detailNode.Tag.ToString();
                                break;
                            case "PROCESSOPERATIONNAME":
                                _product.PROCESSOPERATIONNAME = _detailNode.Tag.ToString();
                                break;
                            case "PRODUCTOWNER":
                                _product.PRODUCTOWNER = _detailNode.Tag.ToString();
                                break;
                            case "VCRREADFLAG":
                                _product.VCRREADFLAG = _detailNode.Tag.ToString();
                                break;
                            case "SHORTCUTFLAG":
                                _product.SHORTCUTFLAG = _detailNode.Tag.ToString();;
                                break;
                            case "SAMPLEFLAG":
                                _product.SAMPLEFLAG = _detailNode.Tag.ToString();
                                break;
                            case "PROCESSFLAG":
                                _product.PROCESSFLAG = _detailNode.Tag.ToString();
                                break;
                            case "PROCESSCOMMUNICATIONSTATE":
                                _product.PROCESSCOMMUNICATIONSTATE = _detailNode.Tag.ToString();
                                break;
                            default:
                                break;
                        }
                    }
                    _trx.BODY.INCOMPLETECASSETTEDATALIST.Add(_product);
                }

                string _xml = _trx.WriteToXml();
                string _err = "";
                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SendToBC_IncompleteBoxEditSave()
        {
            try
            {
                IncompleteBoxEditSave _trx = new IncompleteBoxEditSave();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                _trx.BODY.INCOMPLETEDATE = txtIncompleteDate.Text.ToString(); 
                _trx.BODY.PORTID = txtPortID.Text.ToString();  
                _trx.BODY.SAMPLEFLAG = txtSampleFlag.Text.ToString();  
                _trx.BODY.LINERECIPENAME = txtLineRecipeName.Text.ToString(); 
                _trx.BODY.HOSTLINERECIPENAME = txtHostLineRecipeName.Text.ToString(); 
                _trx.BODY.PPID = txtPPID.Text.ToString();  
                _trx.BODY.HOSTPPID = txtHostPPID.Text.ToString();  
                _trx.BODY.MESTRXID = CurMESTrxID;  
                _trx.BODY.FILENAME = CurFileName;  

                foreach (TreeNode _boxNode in trvData.Nodes)
                {
                    IncompleteBoxEditSave.BOXc _box = new IncompleteBoxEditSave.BOXc();
                    _box.BOXNAME = _boxNode.Tag.ToString();
                    _box.PRODUCTQUANTITY = _boxNode.Nodes.Count.ToString();

                    foreach (TreeNode _podNode in _boxNode.Nodes)
                    {
                        IncompleteBoxEditSave.PRODUCTc _product = new IncompleteBoxEditSave.PRODUCTc();

                        _product.POSITION = _podNode.Tag.ToString();

                        foreach (TreeNode _detailNode in _podNode.Nodes)
                        {
                            switch (_detailNode.Name)
                            {
                                case "PRODUCTNAME":
                                    _product.PRODUCTNAME = _detailNode.Tag.ToString();
                                    break;
                                case "HOSTPRODUCTNAME":
                                    _product.HOSTPRODUCTNAME = _detailNode.Tag.ToString();
                                    break;
                                case "SHORTCUTFLAG":
                                    _product.SHORTCUTFLAG = _detailNode.Tag.ToString();
                                    break;
                                case "BOXULDFLAG":
                                    _product.BOXULDFLAG = _detailNode.Tag.ToString();
                                    break;
                                case "DPIPROCESSFLAG":
                                    _product.DPIPROCESSFLAG = _detailNode.Tag.ToString();
                                    break;
                                case "RTPFLAG":
                                    _product.RTPFLAG = _detailNode.Tag.ToString();
                                    break;
                                case "PPID":
                                    _product.PPID = _detailNode.Tag.ToString();
                                    break;
                                case "HOSTPPID":
                                    _product.HOSTPPID = _detailNode.Tag.ToString();
                                    break;
                                case "ABNORMALCODELIST":
                                    IncompleteBoxEditSave.ABNORMALCODEc _abnormal ;
                                    foreach (TreeNode _abNode in _detailNode.Nodes)
                                    {                                        
                                        string[] _data = _abNode.Tag.ToString().Split(':');

                                        if (_data.Length == 2)
                                        {
                                            _abnormal = new IncompleteBoxEditSave.ABNORMALCODEc();

                                            _abnormal.ABNORMALSEQ = _data[0];
                                            _abnormal.ABNORMALCODE = _data[1];

                                            _product.ABNORMALCODELIST.Add(_abnormal);
                                        }
                                    }
                                        
                                    break;
                                default: break;
                            }                            
                        }
                        _box.PRODUCTLIST.Add(_product);
                    }
                    _trx.BODY.BOXLIST.Add(_box);
                }

                string _xml = _trx.WriteToXml();
                string _err = "";
                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }     

        private void LoadIncompleteCassette()
        {
            try
            {
                var v = FormMainMDI.G_OPIAp.DBCtx.SBCS_INCOMPLETECST.Where(p => p.STATE == "NG");

                dgvIncompeteCst.Rows.Clear();

                foreach (SBCS_INCOMPLETECST cst in v)
                {
                    dgvIncompeteCst.Rows.Add(cst.UPDATETIME.ToString("yyyy-MM-dd HH:mm:ss"),
                        cst.PORTID,
                        cst.CASSETTEID,
                        cst.CASSETTESEQNO,
                        cst.MESTRXID,
                        cst.FILENAME,
                        cst.STATE,
                        cst.NGREASON);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ClrearData()
        {
            try
            {
                foreach (Panel _pnl in flpHeader.Controls.OfType<Panel>())
                {
                    foreach (TextBox _txt in _pnl.Controls.OfType<TextBox>())
                    {
                        _txt.Text = string.Empty;
                    }                    
                }

                trvData.Nodes.Clear();

                CurFileName = string.Empty;
                CurMESTrxID = string.Empty;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ShowObjectbyType(eIncompleteType FileType)
        {
            try
            {
                trvData.Nodes.Clear();

                if (FileType == eIncompleteType.Box)
                {
                    pnlCassetteID.Visible = false;
                    pnlCarrierName.Visible = false;
                    pnlSampleFlag.Visible = true; 
                }
                else
                {
                    pnlCassetteID.Visible = true;
                    pnlCarrierName.Visible = true;
                    pnlSampleFlag.Visible = false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
        
        private void trvData_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                {
                    Point ClickPoint = new Point(e.X, e.Y);
                    CurrentNode = trvData.GetNodeAt(ClickPoint);

                    if (CurrentNode == null) return;

                    if (CurrentNode.Name == "POSITION" || CurrentNode.Name == "BOXNAME")
                    {
                        CurrentNode.ContextMenuStrip = MemuStrip;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cmsMemu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                switch (e.ClickedItem.ToString())
                {
                    case "Copy To New":

                        TreeNode _node = new TreeNode(CurrentNode.Text);

                        _node.Name = CurrentNode.Name;
                        _node.Tag = CurrentNode.Tag;

                        //copy all the child nodes
                        CopyChildNodes(CurrentNode, _node);

                        //paste the node from the copied nodes
                        if (IncompleteType == eIncompleteType.Box  && CurrentNode.Name == "POSITION") CurrentNode.Parent.Nodes.Add(_node);
                        else trvData.Nodes.Add(_node);

                        break;

                    case "Modify":

                        if (IncompleteType== eIncompleteType.Box)
                        {
                            if (CurrentNode.Name == "POSITION")
                                new FormIncompleteBoxDataEdit(CurrentNode).ShowDialog();
                            else 
                                new FormIncompleteBoxNameEdit(CurrentNode).ShowDialog();
                        }
                        else
                        {
                            new FormIncompleteCassetteDataEdit (CurrentNode).ShowDialog();
                        }
                        break;

                    case "Delete":
                        CurrentNode.Remove();
                        break;

                    default :
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void CopyChildNodes(TreeNode chooseNode, TreeNode newParentNode)
        {
            try
            {
                foreach (TreeNode _node in chooseNode.Nodes)
                {
                    TreeNode tmpNode = new TreeNode(_node.Text);
                    tmpNode.Name = _node.Name;
                    tmpNode.Tag = _node.Tag;
                    tmpNode.Text = _node.Text;
                    newParentNode.Nodes.Add(tmpNode);
                    CopyChildNodes(_node, tmpNode);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void LineRecipeName_DoubleClick(object sender, EventArgs e)
        {
            try
            {

                if (dicRecipe_All.Count == 0) LoadPPIDSetting();

                FormCassetteControl_RecipeChoose frm = new FormCassetteControl_RecipeChoose(dicRecipe_All);

                if (System.Windows.Forms.DialogResult.OK == frm.ShowDialog())
                {
                    txtLineRecipeName.Text = frm.RecipeName;

                    txtPPID.Text = dicRecipe_All[frm.RecipeName].PPID;

                    if (IncompleteType == eIncompleteType.Box)
                    {
                        foreach (TreeNode _subNode in trvData.Nodes)
                        {
                            foreach (TreeNode _positionNode in _subNode.Nodes)
                            {
                                foreach (TreeNode _dataNode in _positionNode.Nodes)
                                {
                                    if (_dataNode.Name == "PPID")
                                    {
                                        _dataNode.Text = string.Format("PPID [ {0} ]", txtPPID.Text);
                                        _dataNode.Tag = txtPPID.Text;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void LoadPPIDSetting()
        {
            try
            {
                Recipe _new;

                var _var = (from _recipe in FormMainMDI.G_OPIAp.DBCtx.SBRM_RECIPE
                            where _recipe.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType 
                            select new { RecipeName = _recipe.LINERECIPENAME, PPID = _recipe.PPID });


                dicRecipe_All.Clear();

                foreach (var _recipe in _var)
                {
                    _new = new Recipe(FormMainMDI.G_OPIAp.CurLine.ServerName, _recipe.RecipeName, _recipe.PPID);
                    dicRecipe_All.Add(_recipe.RecipeName, _new);
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        
        //private void lblBox_CheckedChanged(object sender, EventArgs e)
        //{
        //    //try
        //    //{
        //    //    RadioButton _rdo = (RadioButton)sender;


        //    //    if (_rdo.Checked)
        //    //    {
        //    //        IncompleteBoxDataReply.BOXc _boxData = LstBox.Find(d => d.BOXNAME == _rdo.Name);

        //    //        //把目前已修改的資訊更新至lot data
        //    //        if (CurBoxData != null) SetDenseData();

        //    //        ShowDenseData(_boxData);

        //    //        CurBoxData = _boxData;
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //    //    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
        //    //}
        //}

        //private void btnProduct_Click(object sender, EventArgs e)
        //{
        //try
        //{
        //    Button _btn = (Button) sender ;

        //    if (CurPort == null) return;

        //    DataGridViewRow _row;
        //    FormIncompleteBoxDataEdit _frm;

        //    if (CurPort.PORTATTRIBUTE == "DENSE")
        //    {
        //        #region Dense
        //        if (dgvDenseData.CurrentRow == null)
        //        {
        //            ShowMessage(this, lblCaption.Text, "", "Please Choose Dense Product", MessageBoxIcon.Warning);
        //            return;
        //        }

        //        _row = dgvDenseData.CurrentRow;


        //        switch (_btn.Tag.ToString())
        //        {
        //            case "ADD":

        //                #region Add

        //                _frm = new FormIncompleteBoxDataEdit(_row, FormMode.AddNew);

        //                if (DialogResult.OK == _frm.ShowDialog())
        //                {

        //                }
        //                break;
        //                #endregion

        //            case "MODIFY":

        //                #region Modify


        //                _frm = new FormIncompleteBoxDataEdit(_row, FormMode.Modify);

        //                if (DialogResult.OK == _frm.ShowDialog())
        //                {

        //                }
        //                break;
        //                #endregion

        //            case "DELETE":

        //                #region Delete
        //                if (dgvDenseData.CurrentRow == null)
        //                {
        //                    ShowMessage(this, lblCaption.Text, "", "Please Choose Dense Product", MessageBoxIcon.Warning);
        //                    return;
        //                }
        //                break;
        //                #endregion

        //            default:
        //                break;
        //        }
        //        #endregion
        //    }
        //    else
        //    {
        //        #region Cassette
        //        if (dgvCassetteData.CurrentRow == null)
        //        {
        //            ShowMessage(this, lblCaption.Text, "", "Please Choose Cassette Product", MessageBoxIcon.Warning);
        //            return;
        //        }
        //        #endregion
        //    }              
        //}
        //catch (Exception ex)
        //{
        //    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
        //}
        //}

        //private void ShowDenseData(IncompleteBoxDataReply.BOXc boxData)
        //{
        //    try
        //    {
        //        #region Product
        //        dgvDenseData.Rows.Clear();

        //        foreach (IncompleteBoxDataReply.PRODUCTc product in boxData.PRODUCTLIST)
        //        {

        //            //Position,Product Name,Host Product Name,Short Cut Flag,Box ULD Flag,DPI Process Flag,RTP Flag,PPID,Host PPID 
        //            dgvDenseData.Rows.Add(
        //                product.POSITION.ToString(), product.PRODUCTNAME.ToString(), product.HOSTPRODUCTNAME.ToString(),
        //                product.SHORTCUTFLAG.ToString(), product.BOXULDFLAG.ToString(), product.DPIPROCESSFLAG.ToString(), product.RTPFLAG.ToString(), product.PPID.ToString(),
        //                product.HOSTPPID.ToString());
        //        }

        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
        //    }
        //}

        //private void SetDenseData()
        //{
        //    try
        //    {
        //        IncompleteBoxDataReply.BOXc _box = new IncompleteBoxDataReply.BOXc();

        //        //把資料回存至 LstBox 內,避免切換時資料遺失
        //        foreach (DataGridViewRow row in dgvDenseData.Rows)
        //        {
        //            IncompleteBoxDataReply.PRODUCTc _prod = new IncompleteBoxDataReply.PRODUCTc();

        //            _prod.POSITION = row.Cells[colPosition_Dense.Name].Value.ToString();
        //            _prod.PRODUCTNAME = row.Cells[colProductName_Dense.Name].Value.ToString();
        //            _prod.HOSTPRODUCTNAME = row.Cells[colHostProductName_Dense.Name].Value.ToString();

        //            _prod.SHORTCUTFLAG = row.Cells[colPosition_Dense.Name].Value.ToString();
        //            _prod.BOXULDFLAG = row.Cells[colPosition_Dense.Name].Value.ToString();
        //            _prod.DPIPROCESSFLAG = row.Cells[colPosition_Dense.Name].Value.ToString();
        //            _prod.RTPFLAG = row.Cells[colPosition_Dense.Name].Value.ToString();
        //            _prod.HOSTPPID = row.Cells[colPosition_Dense.Name].Value.ToString();

        //            _box.PRODUCTLIST.Add(_prod);
        //        }

        //        _box.PRODUCTQUANTITY = _box.PRODUCTLIST.Count().ToString();

        //        CurBoxData = _box;
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
        //    }
        //}
    }
}
