using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormWIPDelete : FormBase
    {
        #region Fields
        private ucCombox_Node cboLocal;
        private ucTextBox_Normal txtCstSeqNo;
        private ucTextBox_Normal txtJobSeqNo;
        private ucTextBox_Normal txtGlassID;
        private CheckBox chkRemoved;
        private CheckBox chkSelect;
        private const string JOBDATA_CATEGORY_REPLY = "JobDataCategoryReply";
        #endregion

        public FormWIPDelete()
        {
            InitializeComponent();
            this.GenerateConditionUserCtl();
        }

        private void FormWIPDelete_Load(object sender, EventArgs e)
        {
            try
            {
                //#region CBUAM line glass id改顯示 mask id
                //if (FormMainMDI.G_OPIAp.CurLine.ServerName == "CBUAM100")
                //    dgvJobData.Columns[colGlassID.Name].HeaderText = "Mask ID";
                //else
                //    dgvJobData.Columns[colGlassID.Name].HeaderText = "Glass ID";
                //#endregion

                #region 有Robot的line才可change route step
                if (FormMainMDI.G_OPIAp.Dic_Robot.Count > 0)
                {
                    dgvJobData.Columns[colChangeRoute.Name].Visible = true;
                    dgvJobData.Columns[colStopReason.Name].Visible = true;
                    dgvJobData.Columns[colCreateRobotWip.Name].Visible = true;
                }
                else
                {
                    dgvJobData.Columns[colChangeRoute.Name].Visible = false;
                    dgvJobData.Columns[colStopReason.Name].Visible = false;
                    dgvJobData.Columns[colCreateRobotWip.Name].Visible = false;
                }
                #endregion

                #region job Data - Array 不顯示job judge / CF除Sorter Line 外不顯示Job Grade

                if (FormMainMDI.G_OPIAp.CurLine.FabType.Equals("ARRAY"))
                {
                    colJobJudge.Visible = false;
                }
                else if (FormMainMDI.G_OPIAp.CurLine.FabType.Equals("CF"))
                {
                    if (FormMainMDI.G_OPIAp.CurLine.LineType != "FBSRT_TYPE1")
                        colJobGrade.Visible = false;
                }

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        #region Private Methods
        //動態產生查詢條件所需的控制項
        private void GenerateConditionUserCtl()
        {
            try
            {
                System.Drawing.Size size = new System.Drawing.Size(195, 62);
                //產生Local
                this.cboLocal = new ucCombox_Node(true, true) { Caption = "Local", Size = size };
                this.flpCondition.Controls.Add(this.cboLocal);
                //產生Cassette Seq No
                this.txtCstSeqNo = new ucTextBox_Normal(true) { Caption = "Cassette Seq No", Size = size };
                this.flpCondition.Controls.Add(this.txtCstSeqNo);
                //產生Job Seq No
                this.txtJobSeqNo = new ucTextBox_Normal(true) { Caption = "Job Seq No", Size = size };
                this.flpCondition.Controls.Add(this.txtJobSeqNo);
                //產生Glass ID
                this.txtGlassID = new ucTextBox_Normal(true) { Caption = "Glass ID", Size = size }; //{ Caption = FormMainMDI.G_OPIAp.CurLine.ServerName == "CBUAM100"?"Mask ID":"Glass ID", Size = size };
                this.flpCondition.Controls.Add(this.txtGlassID);
                //產生Removed
                this.chkRemoved = new CheckBox() { Text = "Removed Only", Anchor = (AnchorStyles.Top | AnchorStyles.Bottom) };
                this.chkRemoved.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                //this.chkRemoved.CheckedChanged += new EventHandler(chkRemoved_CheckedChanged);
                this.flpCondition.Controls.Add(this.chkRemoved);

                //產生Select All
                this.chkSelect = new CheckBox() { Text = "Select All", Anchor = (AnchorStyles.Top | AnchorStyles.Bottom) };
                this.chkSelect.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.chkSelect.CheckedChanged += new EventHandler(chkSelect_CheckedChanged);
                this.flpCondition.Controls.Add(this.chkSelect);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        void chkRemoved_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkRemoved == null) return;
            this.btnRemove.Enabled = !this.chkRemoved.Checked;
        }

        void chkSelect_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox _chk = (CheckBox)sender;

            foreach (DataGridViewRow _row in dgvJobData.Rows)
            {
                _row.Cells[colChoose.Name].Value = _chk.Checked;
            }
        }

        private MessageResponse SendtoBC_JobDataCategoryRequest()
        {
            try
            {
                JobDataCategoryRequest _trx = new JobDataCategoryRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineName;
                _trx.BODY.EQUIPMENTNO = this.cboLocal.Checked ? this.cboLocal.SelectedNodeNO : string.Empty;
                _trx.BODY.UNITNO = "00";
                _trx.BODY.PORTNO = "00";
                _trx.BODY.CASSETTESEQNO = this.txtCstSeqNo.Checked ? this.txtCstSeqNo.InputText : string.Empty;
                _trx.BODY.JOBSEQNO = this.txtJobSeqNo.Checked ? this.txtJobSeqNo.InputText : string.Empty;
                _trx.BODY.GLASSID = this.txtGlassID.Checked ? this.txtGlassID.InputText : string.Empty;
                _trx.BODY.REMOVEFLAG = this.chkRemoved.Checked ? "Y" : "N";

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);
                return _resp;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                
                return null;
            }
        }
        private void FillJobDataGrid(string xml)
        {
            try
            {
                int _slotNo = 0;
                int _jobSeqNo = 0;
                int _cstSeqNo = 0;
                int _productType = 0;
                string _nodeData = string.Empty;

                JobDataCategoryReply _JobDataCategoryReply = (JobDataCategoryReply)Spec.CheckXMLFormat(xml);

                #region Check Return Msg
                if (FormMainMDI.G_OPIAp.CurLine.ServerName != _JobDataCategoryReply.BODY.LINENAME)
                {
                    ShowMessage(this, lblCaption.Text, "", "Line Name disaccords with current Line Name", MessageBoxIcon.Error);
                    return;
                }

                if (!_JobDataCategoryReply.RETURN.RETURNCODE.Equals(FormMainMDI.G_OPIAp.ReturnCodeSuccess))
                {
                    ShowMessage(this, lblCaption.Text, _JobDataCategoryReply.RETURN, MessageBoxIcon.Error);
                    return;
                }
                #endregion

                dgvJobData.Rows.Clear();

                foreach (JobDataCategoryReply.EQUIPMENTITEMc eqpData in _JobDataCategoryReply.BODY.EQUIPMENTLIST)
                {
                    foreach (JobDataCategoryReply.JOBc jobData in eqpData.JOBDATALIST)
                    {
                        int.TryParse(jobData.SLOTNO, out _slotNo);
                        int.TryParse(jobData.JOBSEQNO, out _jobSeqNo);
                        int.TryParse(jobData.CASSETTESEQNO, out _cstSeqNo);
                        int.TryParse(jobData.PRODUCTTYPE, out _productType);

                        if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(eqpData.EQUIPMENTNO))
                        {
                            _nodeData = FormMainMDI.G_OPIAp.Dic_Node[eqpData.EQUIPMENTNO].NodeNo + "-" + FormMainMDI.G_OPIAp.Dic_Node[eqpData.EQUIPMENTNO].NodeID;
                        }
                        else _nodeData = eqpData.EQUIPMENTNO;

                        //Local No, CST Seq No, Job Seq No, GlassID, product Type, Job TYPE, Job Judge, Job Grade, PPID, TracKing Data
                        dgvJobData.Rows.Add(false, "Detail","Route","Stop Reason","Create Robot Wip",
                            _nodeData,
                            _slotNo,
                            _cstSeqNo,
                            _jobSeqNo,
                            jobData.GLASSID,
                            _productType,
                            jobData.JOBTYPE,
                            jobData.JOBJUDGE,
                            jobData.JOBGRADE,
                            jobData.PPID,
                            jobData.TRACKINGDATA
                            );
                    }
                }

                if (dgvJobData.Rows.Count > 0)
                {
                    dgvJobData.Sort(dgvJobData.Columns[colSlotNo.Name], ListSortDirection.Descending);

                    gbGlass.Text = string.Format("Total Count : {0}", dgvJobData.Rows.Count.ToString());
                }
                else
                {
                    gbGlass.Text = "Total Count : 0";
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);               
            }
        }

        private void DeleteJobRequest()
        {
            try
            {
                if (DialogResult.No == this.QuectionMessage(this, MethodBase.GetCurrentMethod().Name, "Are you sure to delete selected job?")) return;

                #region Send Job Data Delete Request
                JobDataOperationRequest _trx = new JobDataOperationRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineName;
                _trx.BODY.COMMAND = "DELETE";
                int delCount = 0;
                foreach (DataGridViewRow row in dgvJobData.Rows.Cast<DataGridViewRow>().Where(r => r.Cells["colChoose"].Value.ToString().ToUpper().Equals("TRUE")))
                {
                    JobDataOperationRequest.JOBDATAc item = new JobDataOperationRequest.JOBDATAc();
                    item.CASSETTESEQNO = row.Cells[colCstSeqNo.Name].Value.ToString();
                    item.JOBSEQNO = row.Cells[colJobSeqNo.Name].Value.ToString();
                    item.GLASSID = row.Cells[colGlassID.Name].Value.ToString();
                    _trx.BODY.JOBDATALIST.Add(item);
                    delCount += 1;
                }

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Job data delete request fial！ Please check if job exist, and retry！", MessageBoxIcon.Warning);
                    return;
                }
                #endregion

                #region Job Data Delete Reply
                string _respXml = _resp.Xml;

                JobDataOperationReply _jobDeleteReply = (JobDataOperationReply)Spec.CheckXMLFormat(_respXml);

                #region Check Return Msg
                if (FormMainMDI.G_OPIAp.CurLine.ServerName != _jobDeleteReply.BODY.LINENAME)
                {
                    ShowMessage(this, lblCaption.Text, "", "Line Name disaccords with current Line Name", MessageBoxIcon.Error);
                    return;
                }

                if (!_jobDeleteReply.RETURN.RETURNCODE.Equals(FormMainMDI.G_OPIAp.ReturnCodeSuccess))
                {
                    ShowMessage(this, lblCaption.Text, _jobDeleteReply.RETURN, MessageBoxIcon.Error);
                    return;
                }
                #endregion

                ShowMessage(this, lblCaption.Text, "", string.Format("Job data delete success！\r\nTotal deleted count[{0}]", delCount), MessageBoxIcon.Information);

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void RecoveryJobRequest()
        {
            try
            {
                if (DialogResult.No == this.QuectionMessage(this, MethodBase.GetCurrentMethod().Name, "Are you sure to recovery selected job?")) return;

                #region Send Job Data Delete Request
                JobDataOperationRequest _trx = new JobDataOperationRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineName;
                _trx.BODY.COMMAND = "RECOVERY";
                int rcyCount = 0;
                foreach (DataGridViewRow row in dgvJobData.Rows.Cast<DataGridViewRow>().Where(r => r.Cells["colChoose"].Value.ToString().ToUpper().Equals("TRUE")))
                {
                    JobDataOperationRequest.JOBDATAc item = new JobDataOperationRequest.JOBDATAc();
                    item.CASSETTESEQNO = row.Cells[colCstSeqNo.Name].Value.ToString();
                    item.JOBSEQNO = row.Cells[colJobSeqNo.Name].Value.ToString();
                    item.GLASSID = row.Cells[colGlassID.Name].Value.ToString();
                    _trx.BODY.JOBDATALIST.Add(item);
                    rcyCount += 1;
                }

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Job data recovery request fial！ Please check if job exist, and retry！", MessageBoxIcon.Warning);
                    return;
                }
                #endregion

                #region Job Data Recovery Reply
                string _respXml = _resp.Xml;

                JobDataOperationReply _jobRecoveryReply = (JobDataOperationReply)Spec.CheckXMLFormat(_respXml);

                #region Check Return Msg
                if (FormMainMDI.G_OPIAp.CurLine.ServerName != _jobRecoveryReply.BODY.LINENAME)
                {
                    ShowMessage(this, lblCaption.Text, "", "Line Name disaccords with current Line Name", MessageBoxIcon.Error);
                    return;
                }

                if (!_jobRecoveryReply.RETURN.RETURNCODE.Equals(FormMainMDI.G_OPIAp.ReturnCodeSuccess))
                {
                    ShowMessage(this, lblCaption.Text, _jobRecoveryReply.RETURN, MessageBoxIcon.Error);
                    return;
                }
                #endregion

                ShowMessage(this, lblCaption.Text, "", string.Format("Job data recovery success！\r\nRecoveried count[{0}]", rcyCount), MessageBoxIcon.Information);

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void btnQuery_Click(object sender, EventArgs e)
        {
            MessageResponse resp = SendtoBC_JobDataCategoryRequest();

            if (resp == null) return;

            FillJobDataGrid(resp.Xml);
        }

        private void dgvJobData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvJobData.CurrentRow != null)
                {
                    if (dgvJobData.Columns[e.ColumnIndex].Name != colDetail.Name &&
                        dgvJobData.Columns[e.ColumnIndex].Name != colChoose.Name &&
                        dgvJobData.Columns[e.ColumnIndex].Name != colStopReason.Name &&
                        dgvJobData.Columns[e.ColumnIndex].Name != colChangeRoute.Name&&
                        dgvJobData.Columns[e.ColumnIndex].Name!=colCreateRobotWip.Name) return;

                    string _cstSeqNo = dgvJobData.CurrentRow.Cells[colCstSeqNo.Name].Value.ToString();
                    string _jobSeqNo = dgvJobData.CurrentRow.Cells[colJobSeqNo.Name].Value.ToString();
                    string _glassID = dgvJobData.CurrentRow.Cells[colGlassID.Name].Value.ToString();

                    if (_glassID == string.Empty && _cstSeqNo == string.Empty && _jobSeqNo == string.Empty)
                    {
                        ShowMessage(this, this.lblCaption.Text, "", "Cassette Seq No、Job Seq No、Job ID must be Required！", MessageBoxIcon.Warning);
                        return;
                    }

                    if (dgvJobData.Columns[e.ColumnIndex].Name == colDetail.Name)  //點選detail
                    {
                        FormJobDataDetail _frm = new FormJobDataDetail(_glassID, _cstSeqNo, _jobSeqNo);

                        _frm.ShowDialog();

                        if (_frm != null) _frm.Dispose();
                    }
                    else if (dgvJobData.Columns[e.ColumnIndex].Name == colChoose.Name)  //點選choose
                    {
                        dgvJobData[e.ColumnIndex, e.RowIndex].Value = !(bool)dgvJobData[e.ColumnIndex, e.RowIndex].Value;
                    }
                    else if (dgvJobData.Columns[e.ColumnIndex].Name == colChangeRoute.Name)  //點選route
                    {
                        FormRobotRouteStepChange _frm = new FormRobotRouteStepChange(_cstSeqNo, _jobSeqNo);
                        _frm.ShowDialog();

                        if (_frm != null) _frm.Dispose();
                    }
                    else if (dgvJobData.Columns[e.ColumnIndex].Name == colStopReason.Name) //點選Robot Stop Reason
                    {
                        FormRobotStopReason _frm = new FormRobotStopReason(_cstSeqNo, _jobSeqNo);
                        _frm.ShowDialog();

                        if (_frm != null) _frm.Dispose();
                    }
                    else if (dgvJobData.Columns[e.ColumnIndex].Name == colCreateRobotWip.Name)
                    {
                        FormRobotWipCreate _frm = new FormRobotWipCreate(_cstSeqNo, _jobSeqNo);
                        _frm.ShowDialog();

                        if (_frm != null) _frm.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedRows = dgvJobData.Rows.Cast<DataGridViewRow>().Where(r => r.Cells["colChoose"].Value.ToString().ToUpper().Equals("TRUE"));
                if (selectedRows != null && selectedRows.Count() <= 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Please select job to delete！", MessageBoxIcon.Warning);
                    return;
                }
                DeleteJobRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRecovery_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedRows = dgvJobData.Rows.Cast<DataGridViewRow>().Where(r => r.Cells["colChoose"].Value.ToString().ToUpper().Equals("TRUE"));
                if (selectedRows != null && selectedRows.Count() <= 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Please select job to recovery！", MessageBoxIcon.Warning);
                    return;
                }
                RecoveryJobRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

    }
}
