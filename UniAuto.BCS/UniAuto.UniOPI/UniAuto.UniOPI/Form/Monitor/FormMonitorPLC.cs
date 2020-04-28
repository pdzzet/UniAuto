using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormMonitorPLC : FormBase
    {
        string CurNodeNo = string.Empty;

        List<PLCTrxNameReply.PLCTRXc> PlcTrx;

        public FormMonitorPLC()
        {
            InitializeComponent();
        }

        private void FormMonitorPLC_Load(object sender, EventArgs e)
        {

            chkDetail.Checked = false;

            PlcTrx = new List<PLCTrxNameReply.PLCTRXc>();

            ColumsVisible(false);
            
            UniTools.SetComboBox_Node(cboLocalNode);

            cboLocalNode.SelectedValueChanged += new EventHandler(cboLocalNode_SelectedValueChanged);

            if (cboLocalNode.Items.Count > 0 ) cboLocalNode.SelectedIndex = 0;
            
        }

        private void ColumsVisible(bool IsVisible)
        {
            try
            {
                dgvTrxData.Columns["colGroupName"].Visible = IsVisible;
                dgvTrxData.Columns["colDir"].Visible = IsVisible;
                dgvTrxData.Columns["colEventName"].Visible = IsVisible;
                dgvTrxData.Columns["colDevcode"].Visible = IsVisible;
                dgvTrxData.Columns["colAddress"].Visible = IsVisible;
                dgvTrxData.Columns["colRAddress"].Visible = IsVisible; //add by sy.wu
                dgvTrxData.Columns["colPoints"].Visible = IsVisible;
                dgvTrxData.Columns["colSkipDecode"].Visible = IsVisible;

                dgvTrxData.Columns["colWOffset"].Visible = IsVisible;
                dgvTrxData.Columns["colWPoints"].Visible = IsVisible;
                dgvTrxData.Columns["colBOffset"].Visible = IsVisible;
                dgvTrxData.Columns["colBPoints"].Visible = IsVisible;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlcTrx.Count == 0)
                {
                    #region Send PLCTrxNameRequest

                    PLCTrxNameRequest _trx = new PLCTrxNameRequest();
                    _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                    MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                    if (_resp == null) return;

                    #endregion

                    #region PLCTrxDataReply

                    string _respXml = _resp.Xml;

                    PLCTrxNameReply _plcTrxNameReply = (PLCTrxNameReply)Spec.CheckXMLFormat(_respXml);

                    #region Update Data
                    PlcTrx = new List<PLCTrxNameReply.PLCTRXc>(_plcTrxNameReply.BODY.PLCTRXLIST.ToArray());
                    #endregion

                    #endregion
                }

                GetPLCTrxNameList();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

         private void GetPLCTrxNameList()
        {
            try
            {
                if (CurNodeNo == string.Empty) return;

                string _key = string.Format("{0}_", CurNodeNo);

                if (dgvTrxName.Rows.Count > 0)
                {
                    if (dgvTrxName.Rows[0].Cells[colPLCTrxName.Name].Value.ToString().StartsWith(_key))
                        return;
                }

                foreach (PLCTrxNameReply.PLCTRXc _plcTrx in PlcTrx)
                {
                    if (_plcTrx.PLCTRXNAME.StartsWith(_key))
                        dgvTrxName.Rows.Add(_plcTrx.PLCTRXNAME);
                }
                if (dgvTrxName.Rows.Count > 0) dgvTrxName.ClearSelection();                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

   
        private void dgvTrxName_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex == -1) return;
                if (dgvTrxName.CurrentCell == null) return;

                DataGridViewRow selectedRow = dgvTrxName.SelectedRows[0];

                #region PLCTrxDataRequest
                PLCTrxDataRequest _trx = new PLCTrxDataRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PLCTRXNAME = selectedRow.Cells[colPLCTrxName.Name].Value.ToString();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region PLCTrxDataReply

                string _respXml = _resp.Xml;

                PLCTrxDataReply _plcTrxDataReply = (PLCTrxDataReply)Spec.CheckXMLFormat(_respXml);

                //#region Check Return Msg
                //if (FormMainMDI.G_OPIAp.CurLine.ServerName != _plcTrxDataReply.BODY.LINENAME)
                //{
                //    ShowMessage(this, lblCaption.Text, "", "Line Name disaccords with current Line Name", MessageBoxIcon.Error);
                //    return;
                //}

                //if (!_plcTrxDataReply.RETURN.RETURNCODE.Equals(FormMainMDI.G_OPIAp.ReturnCodeSuccess))
                //{
                //    ShowMessage(this, lblCaption.Text, _plcTrxDataReply.RETURN, MessageBoxIcon.Error);
                //    return;
                //}
                //#endregion

                #region Update Data
                string[] columns = new string[] { "EVENT_GROUP_NAME", "DIR", "EVENT_NAME", "DEVCODE", "ADDR", "POINTS", "SKIPDECODE", "ITEM_NAME", "VAL", "WOFFSET", "WPOINTS", "BOFFSET", "BPOINTS", "EXPRESSION", "REALADDR" };

                DataTable dt = UniTools.InitDt(columns);

                DataRow drNew = null;
                foreach (PLCTrxDataReply.EVENTGROUPc grp in _plcTrxDataReply.BODY.EVENTGROUPLIST)
                {
                    foreach (PLCTrxDataReply.EVENTc evt in grp.EVENTLIST)
                    {
                        foreach (PLCTrxDataReply.ITEMc itm in evt.ITEMLIST)
                        {
                            #region 運算Real Address   add by sy.wu
                            string strA = Convert.ToInt32(evt.ADDR, 16).ToString();
                            string strB = itm.WOFFSET.ToString();
                            string sum = (int.Parse(strA) + int.Parse(strB)).ToString();
                            string strAddr = Convert.ToString(int.Parse(sum), 16);
                            string AddrNew = strAddr.PadLeft(7, '0').ToUpper();
                            #endregion

                            drNew = dt.NewRow();
                            drNew["EVENT_GROUP_NAME"] = grp.NAME;
                            drNew["DIR"] = grp.DIR;
                            drNew["EVENT_NAME"] = evt.NAME;
                            drNew["DEVCODE"] = evt.DEVCODE;
                            drNew["ADDR"] = evt.ADDR;
                            drNew["REALADDR"] = AddrNew;
                            drNew["POINTS"] = evt.POINTS;
                            drNew["SKIPDECODE"] = evt.SKIPDECODE;
                            drNew["ITEM_NAME"] = itm.NAME;
                            drNew["VAL"] = itm.VAL;
                            drNew["WOFFSET"] = itm.WOFFSET;
                            drNew["WPOINTS"] = itm.WPOINTS;
                            drNew["BOFFSET"] = itm.BOFFSET;
                            drNew["BPOINTS"] = itm.BPOINTS;
                            drNew["EXPRESSION"] = itm.EXPERESSION;

                            dt.Rows.Add(drNew);
                        }
                    }
                }

                dgvTrxData.DataSource = dt;

                if (dgvTrxData.Rows.Count > 0) dgvTrxData.ClearSelection();
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void chkDetail_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                bool IsVisible = ((CheckBox)sender).Checked;

                ColumsVisible(IsVisible);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboLocalNode_SelectedValueChanged(object sender, EventArgs e)
        {
             try
            {
                if (cboLocalNode.SelectedValue != null)
                {
                    CurNodeNo = cboLocalNode.SelectedValue.ToString();
                }
                else CurNodeNo = string.Empty;

                #region Reset Datagridview
                dgvTrxName.Rows.Clear();

                string[] columns = new string[] { "EVENT_GROUP_NAME", "DIR", "EVENT_NAME", "DEVCODE", "ADDR", "POINTS", "SKIPDECODE", "ITEM_NAME", "VAL", "WOFFSET", "WPOINTS", "BOFFSET", "BPOINTS", "EXPRESSION", "REALADDR" };

                DataTable dt = UniTools.InitDt(columns);
                
                dgvTrxData.DataSource = dt;
                #endregion
            }
             catch (Exception ex)
             {
                 NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                 ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
             }
        }
    }
}
