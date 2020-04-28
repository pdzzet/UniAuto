using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;
using UniAuto.UniBCS.OpiSpec;


namespace UniOPI
{
    public partial class FormNodeStatus : FormBase
    {

        public FormNodeStatus()
        {
            InitializeComponent();
            lblCaption.Text = "Node Status";
        }
        private void FormNodeStatus_Load(object sender, EventArgs e)
        {
            Initial(true);
            GetData();
            this.tmrBaseRefresh.Enabled = true;
        }


        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Initial(false);
            SendReq();
        }

        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
           
            DataGridViewRow dgvRow = dgvData.Rows[e.RowIndex];
            VisibleShowInfo(dgvRow);
            ShowInfo(dgvRow);
        }

        private void tmrBaseRefresh_Tick(object sender, EventArgs e)
        {
            GetData();
        }

        private void Initial(bool ClearDataView)
        {
            if (ClearDataView) dgvData.Rows.Clear();
            ClearTextBox(flpShowInfo.Controls);
        }


        private void SendReq()
        {
            AllEquipmentStatusRequest _trx = new AllEquipmentStatusRequest();
            _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
            _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
            string _xml = _trx.WriteToXml();

            string error = string.Empty;
            FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out error, FormMainMDI.G_OPIAp.SessionID);
        }

        private void GetData()
        {
            try
            {
                eVCRMode[] _vcrStatus = { eVCRMode.DISABLE, eVCRMode.DISABLE, eVCRMode.DISABLE, eVCRMode.DISABLE, eVCRMode.DISABLE };
                int _num = 0;
                string _desc = string.Empty;

                if (dgvData.Rows.Count == 0)
                {
                    foreach (Node node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                    {
                        for (int i = 0; i < _vcrStatus.Length; i++)
                        {
                            _vcrStatus[0] = eVCRMode.DISABLE;
                        }

                        foreach (VCR _vcr in node.VCRs)
                        {
                            int.TryParse(_vcr.VCRNO, out _num);

                            _num = _num - 1;

                            if (_num < 2) _vcrStatus[_num] = _vcr.Status;
                        }
                        
                        _desc = string.Format("{0}:{1}",((int)node.HighCVMode).ToString() , Public.GetEnumDesc(node.HighCVMode)); 

                        //Local No,Local ID,CIM Mode,Equipment Alive,
                        //Upstream Inline Mode,Downstream Inline Mode,Local Alarm Status ,Auto Recipe Change Mode,
                        //Partial Full Mode,VCR#01 Enable Mode,VCR#02 Enable Mode,Bypass Mode,
                        //Bypass Inspection Equipment#01 Mode,Bypass Inspection Equipment#02 Mode,Turn Table Mode,High CV Mode,
                        //Next Line BC Status,Job Data Check Mode,COA Version Check Mode,Job Duplicate Check Mode,
                        //Recipe ID Check Mode,Group Index Check Mode,Product Type Check Mode,Porduct ID Check Mode,
                        //CV#07 Status

                        dgvData.Rows.Add(node.NodeNo, node.NodeID, node.CIMMode, node.EquipmentAlive,
                            node.UpStreamInlineMode, node.DownStreamInlineMode, node.AlarmStatus, node.AutoRecipeChange,
                            node.PartialFullMode, _vcrStatus[0].ToString(), _vcrStatus[1].ToString(), node.ByPassMode,
                            node.ByPassInsp01Mode, node.ByPassInsp02Mode, node.TurnTableMode, _desc,                            
                            node.NextLineBCStatus, node.JobDataCheckMode, node.COAVersionCheckMode, node.JobDuplicateCheckMode,
                            node.RecipeIDCheckMode, node.GroupIndexCheckMode, node.ProductTypeCheckMode, node.PorductIDCheckMode, 
                            node.CV07Status);


                    }
                    if (dgvData.Rows.Count > 0)
                    {
                        VisibleShowInfo(dgvData.Rows[0]);
                        ShowInfo(dgvData.Rows[0]);
                    }
                }
                else
                {
                    foreach (DataGridViewRow dgvRow in dgvData.Rows)
                    {
                        string strNodeNo = dgvRow.Cells[LocalNo.Name].Value.ToString();

                        if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(strNodeNo))
                        {
                            Node node = FormMainMDI.G_OPIAp.Dic_Node[strNodeNo];

                            for (int i = 0; i < _vcrStatus.Length; i++)
                            {
                                _vcrStatus[0] = eVCRMode.DISABLE;
                            }

                            foreach (VCR _vcr in node.VCRs)
                            {
                                int.TryParse(_vcr.VCRNO, out _num);

                                _num = _num - 1;

                                if (_num < 2) _vcrStatus[_num] = _vcr.Status;
                            }

                            _desc = string.Format("{0}:{1}",((int)node.HighCVMode).ToString() ,  Public.GetEnumDesc(node.HighCVMode));

                            //Local No,Local ID,CIM Mode,Equipment Alive,
                            //Upstream Inline Mode,Downstream Inline Mode,Local Alarm Status ,Auto Recipe Change Mode,
                            //Partial Full Mode,VCR#01 Enable Mode,VCR#02 Enable Mode,Bypass Mode,
                            //Bypass Inspection Equipment#01 Mode,Bypass Inspection Equipment#02 Mode,Turn Table Mode,High CV Mode,
                            //Next Line BC Status,Job Data Check Mode,COA Version Check Mode,Job Duplicate Check Mode,
                            //Recipe ID Check Mode,Group Index Check Mode,Product Type Check Mode,Porduct ID Check Mode,
                            //CV#07 Status

                            dgvRow.SetValues(node.NodeNo, node.NodeID, node.CIMMode, node.EquipmentAlive,
                                node.UpStreamInlineMode, node.DownStreamInlineMode, node.AlarmStatus,node.AutoRecipeChange,
                                node.PartialFullMode, _vcrStatus[0].ToString(), _vcrStatus[1].ToString(), node.ByPassMode,
                                node.ByPassInsp01Mode, node.ByPassInsp02Mode, node.TurnTableMode, _desc,
                                node.NextLineBCStatus, node.JobDataCheckMode, node.COAVersionCheckMode, node.JobDuplicateCheckMode,
                                node.RecipeIDCheckMode, node.GroupIndexCheckMode, node.ProductTypeCheckMode, node.PorductIDCheckMode,
                                node.CV07Status);
                        }
                    }
                    ShowInfo(dgvData.CurrentRow);
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ShowInfo(DataGridViewRow dgvRow)
        {
            try
            {
                if (dgvRow == null) return;

                txtNodeID.Text = FormatString(dgvRow.Cells[NodeID.Name].Value).ToUpper();
                txtLocalNo.Text = FormatString(dgvRow.Cells[LocalNo.Name].Value).ToUpper();
                txtCimMode.Text = FormatString(dgvRow.Cells[CimMode.Name].Value).ToUpper();
                txtEquipmentAlive.Text = FormatString(dgvRow.Cells[EquipmentAlive.Name].Value).ToUpper();
                txtUpstreamInlineMode.Text = FormatString(dgvRow.Cells[UpstreamInlineMode.Name].Value).ToUpper();
                txtDownstreamInlineMode.Text = FormatString(dgvRow.Cells[DownstreamInlineMode.Name].Value).ToUpper();
                txtLocalAlarmStatus.Text = FormatString(dgvRow.Cells[LocalAlarmStatus.Name].Value).ToUpper();
                txtAutoRecipeChangeMode.Text = FormatString(dgvRow.Cells[AutoRecipeChangeMode.Name].Value).ToUpper();
                txtPartialFullMode.Text = FormatString(dgvRow.Cells[PartialFullMode.Name].Value).ToUpper();
                txtVCR01EnableMode.Text = FormatString(dgvRow.Cells[VCR01EnableMode.Name].Value).ToUpper();
                txtVCR02EnableMode.Text = FormatString(dgvRow.Cells[VCR02EnableMode.Name].Value).ToUpper();
                txtCV07Status.Text = FormatString(dgvRow.Cells[CV07Status.Name].Value).ToUpper();
                txtBypassMode.Text = FormatString(dgvRow.Cells[BypassMode.Name].Value).ToUpper();
                txtTurnTableMode.Text = FormatString(dgvRow.Cells[TurnTableMode.Name].Value).ToUpper();
                txtBypassInsp01Mode.Text = FormatString(dgvRow.Cells[BypassInsp01Mode.Name].Value).ToUpper();
                txtBypassInsp02Mode.Text = FormatString(dgvRow.Cells[BypassInsp02Mode.Name].Value).ToUpper();
                txtNexLineBCStatus.Text = FormatString(dgvRow.Cells[NextLineBCStatus.Name].Value).ToUpper();
                txtJobDataCheckMode.Text = FormatString(dgvRow.Cells[JobDataCheckMode.Name].Value).ToUpper();
                txtCoaVersionCheckMode.Text = FormatString(dgvRow.Cells[CoaVersionCheckMode.Name].Value).ToUpper();
                txtJobDuplicateCheckMode.Text = FormatString(dgvRow.Cells[JobDuplicateCheckMode.Name].Value).ToUpper();
                txtProductIDCheckMode.Text = FormatString(dgvRow.Cells[ProductIDCheckMode.Name].Value).ToUpper();
                txtGroupIndexCheckMode.Text = FormatString(dgvRow.Cells[GroupIndexCheckMode.Name].Value).ToUpper();
                txtProductTypeCheckMode.Text = FormatString(dgvRow.Cells[ProductTypeCheckMode.Name].Value).ToUpper();
                txtRecipeIDCheckMode.Text = FormatString(dgvRow.Cells[RecipeIDCheckMode.Name].Value).ToUpper();
                txtHighCVMode.Text = FormatString(dgvRow.Cells[HighCVMode.Name].Value);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ClearTextBox(Control.ControlCollection controls)
        {
            try
            {
                foreach (Control ctl in controls)
                {
                    if (ctl is TextBox)
                        ((TextBox)ctl).Clear();

                    if (ctl.Controls.Count > 0)
                        ClearTextBox(ctl.Controls);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private string FormatString(object objValue)
        {
            string strValue = string.Empty;
            if (objValue != null)
                strValue = objValue.ToString();
            return strValue;
        }

        private void VisibleShowInfo(DataGridViewRow dgvRow)
        {
            try
            {
                foreach (Control VisibleDefault in flpShowInfo.Controls)
                {
                    if (VisibleDefault is Panel && VisibleDefault.Tag!= null)
                    {
                        VisibleDefault.Visible = true;
                    }
                    else
                        VisibleDefault.Visible = false;
                }

                var ShowItem = from sd in FormMainMDI.G_OPIAp.DBBRMCtx.SBRM_OPI_NODE_DEF where sd.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) && sd.NODENO.Equals(dgvRow.Cells[LocalNo.Name].Value) select sd.OBJECTKEY;

                foreach (var ControlName in ShowItem.ToList())
                {
                    VisibleControl(ControlName);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void VisibleControl(string ControlName)
        {
            foreach (Control _hidePanel in flpShowInfo.Controls)
            {
                if (_hidePanel is Panel && _hidePanel.Name.Equals(ControlName) == true)
                {
                    _hidePanel.Visible = true;
                    break;
                }
            }
        }
    }
}
