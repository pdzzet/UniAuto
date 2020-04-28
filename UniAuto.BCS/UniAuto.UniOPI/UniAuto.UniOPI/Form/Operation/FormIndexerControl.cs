using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormIndexerControl : FormBase
    {
        Node IndexerNode = null;

        public FormIndexerControl()
        {
            InitializeComponent();
        }

        private void FormIndexerControl_Load(object sender, EventArgs e)
        {
            try
            {
                #region Get Indexer Node 

                if (FormMainMDI.G_OPIAp.CurLine.IndexerNode == null)
                {
                    tmrRefresh.Enabled = false;
                    ShowMessage(this, lblCaption.Text, "", "Can't find indexer equipment", MessageBoxIcon.Error);
                    return;
                }

                IndexerNode = FormMainMDI.G_OPIAp.CurLine.IndexerNode;

                if (FormMainMDI.G_OPIAp.CurLine.LineType.CompareTo("MSP_ULVAC") == 0 || FormMainMDI.G_OPIAp.CurLine.LineType.CompareTo("ITO_ULVAC") == 0)
                {
                    lbl1stName.Text = "Load Lock 01 Rule";
                    lbl2stName.Text = "Load Lock 02 Rule";
                    lbl1stcboName.Text = "Load Lock 01 Name";
                    lbl2stcboName.Text = "Load Lock 02 Value";
                    lbl1sttxtName.Text = "Load Lock 01 Name";
                    lbl2sttxtName.Text = "Load Lock 02 Value";
                }
                else
                {
                    lbl1stName.Text = "1st Proportional Rule";
                    lbl2stName.Text = "2st Proportional Rule";
                    lbl1stcboName.Text = "1st Proportional Rule Name";
                    lbl1sttxtName.Text = "1nd Proportional Rule Value";
                    lbl2stcboName.Text = "2st Proportional Rule Name";
                    lbl2sttxtName.Text = "2nd Proportional Rule Value";
                }

                #endregion

                #region initial Robot Operation Mode
                //List<IndexerRobotStage> _lstStages = FormMainMDI.G_OPIAp.CurLine.BC_RobotOperationModeReply.IndexerRobotStages;

                //if (_lstStages.Count == 0) gbRobotOperMode.Visible = false;
                //else
                //{
                //    foreach (IndexerRobotStage _stage in _lstStages)
                //    {
                //        dgvRobotOperMode.Rows.Add(_stage.LocalNo, _stage.LocalID, _stage.RobotPosNo, _stage.RobotPosNo + ":" + _stage.Description, _stage.Direction, _stage.OperationMode.ToString(), string.Empty, "Send");
                //    }
                //}
                #endregion

                #region initial Equipment Fetch Glass Proportional --just for CVD & DRY
                ////List<string> _lstFetchLine = new List<string>(new string[] { "CVD", "DRY_ADP", "DRY_TEL" });
                ////if (_lstFetchLine.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                ////{
                //    gbEqpFetchGlassPL.Visible = true;

                //    DataTable _fetchItems = UniTools.InitDt(new string[] { "NEW_FETCH_MODE", "NEW_FETCH_MODE_DESC" });
                //    DataTable _fetchItems2 = UniTools.InitDt(new string[] { "NEW_FETCH_MODE", "NEW_FETCH_MODE_DESC" });
                //    List<FetchGlassProportionalName> _lstFetchName = FormMainMDI.G_OPIAp.CurLine.FetchGlassProportionalNames;
                //    if (_lstFetchName != null && _lstFetchName.Count > 0)
                //    {
                //        foreach (FetchGlassProportionalName _name in _lstFetchName)
                //        {
                //            DataRow drNew = _fetchItems.NewRow();

                //            drNew["NEW_FETCH_MODE"] = _name.ProportionalNameNo;
                //            drNew["NEW_FETCH_MODE_DESC"] = _name.ProportionalNameDesc;

                //            _fetchItems.Rows.Add(drNew);


                //            DataRow drNew2 = _fetchItems2.NewRow();

                //            drNew2["NEW_FETCH_MODE"] = _name.ProportionalNameNo;
                //            drNew2["NEW_FETCH_MODE_DESC"] = _name.ProportionalNameDesc;

                //            _fetchItems2.Rows.Add(drNew2);
                //        }
                //    }

                //    cbo1stPlRuleName.DataSource = _fetchItems;
                //    cbo1stPlRuleName.DisplayMember = "NEW_FETCH_MODE_DESC";
                //    cbo1stPlRuleName.ValueMember = "NEW_FETCH_MODE";
                //    cbo1stPlRuleName.SelectedIndex = -1;

                //    cbo2ndPlRuleName.DataSource = _fetchItems2;
                //    cbo2ndPlRuleName.DisplayMember = "NEW_FETCH_MODE_DESC";
                //    cbo2ndPlRuleName.ValueMember = "NEW_FETCH_MODE";
                //    cbo2ndPlRuleName.SelectedIndex = -1;
                ////}
                ////else gbEqpFetchGlassPL.Visible = false;

                //#region EquipmentFetchGlassProportional
                ////List<comboxInfo> _lstRuleName = new List<comboxInfo>();
                ////_lstRuleName.Add(new comboxInfo { ITEM_ID = "1", ITEM_NAME = "NORMAL" });
                ////_lstRuleName.Add(new comboxInfo { ITEM_ID = "2", ITEM_NAME = "IGZO" });
                ////_lstRuleName.Add(new comboxInfo { ITEM_ID = "3", ITEM_NAME = "MQC" });
                ////_lstRuleName.Add(new comboxInfo { ITEM_ID = "4", ITEM_NAME = "ENG" });
                ////_lstRuleName.Add(new comboxInfo { ITEM_ID = "5", ITEM_NAME = "A (Spare)" });
                ////_lstRuleName.Add(new comboxInfo { ITEM_ID = "6", ITEM_NAME = "B (Spare)" });
                ////_lstRuleName.Add(new comboxInfo { ITEM_ID = "7", ITEM_NAME = "HT" });
                ////_lstRuleName.Add(new comboxInfo { ITEM_ID = "8", ITEM_NAME = "LT" });

                ////cbo1stPlRuleName.DataSource = _lstRuleName.ToList();
                ////cbo1stPlRuleName.DisplayMember = "ITEM_DESC";
                ////cbo1stPlRuleName.ValueMember = "ITEM_ID";
                ////cbo1stPlRuleName.SelectedIndex = -1;

                ////cbo2ndPlRuleName.DataSource = _lstRuleName.ToList();
                ////cbo2ndPlRuleName.DisplayMember = "ITEM_DESC";
                ////cbo2ndPlRuleName.ValueMember = "ITEM_ID";
                ////cbo2ndPlRuleName.SelectedIndex = -1;
                //#endregion

                #endregion

                #region Indexer Mode                
                DataTable _indexerItems = UniTools.InitDt(new string[] { "NEW_INDEXER_MODE", "NEW_INDEXER_MODE_DESC" });
                List<LineIndexerMode> _lstIndexerModes = FormMainMDI.G_OPIAp.CurLine.LineIndexerModes;
                if (_lstIndexerModes != null && _lstIndexerModes.Count > 0)
                {

                    foreach (LineIndexerMode _mode in _lstIndexerModes)
                    {
                        DataRow drNew = _indexerItems.NewRow();
                        drNew["NEW_INDEXER_MODE"] = _mode.IndexerModeNo;
                        drNew["NEW_INDEXER_MODE_DESC"] = _mode.IndexerModeDesc;
                        _indexerItems.Rows.Add(drNew);
                    }
                }
                cboIndexerMode_New.DataSource = _indexerItems;
                cboIndexerMode_New.DisplayMember = "NEW_INDEXER_MODE_DESC";
                cboIndexerMode_New.ValueMember = "NEW_INDEXER_MODE";
                cboIndexerMode_New.SelectedIndex = -1;
                #endregion

                #region RobotOpeMode
                //DataTable _dtRobotOpeMode = UniTools.InitDt(new string[] { "NEW_MODE", "NEW_MODE_DESC" });

                //foreach (string _operMode in Enum.GetNames(typeof(eRobotOperMode)))
                //{
                //    int _value = (int)Enum.Parse(typeof(eRobotOperMode), _operMode);

                //    if (_value == 0) continue;

                //    eRobotOperMode robotOpeMode = (eRobotOperMode)Enum.Parse(typeof(eRobotOperMode), _operMode);
                //    string desc = Public.GetEnumDesc(robotOpeMode);
                //    DataRow drNew = _dtRobotOpeMode.NewRow();
                //    drNew["NEW_MODE"] = _value;
                //    drNew["NEW_MODE_DESC"] = string.Format("{0}:{1}", _value, desc);
                //    _dtRobotOpeMode.Rows.Add(drNew);
                //}
                //colNewMode.DataSource = _dtRobotOpeMode;
                //colNewMode.DataPropertyName = "NEW_MODE";
                //colNewMode.DisplayMember = "NEW_MODE_DESC";
                //colNewMode.ValueMember = "NEW_MODE";
                #endregion

                
                #region Indexer Mode                
                DataTable _dtRobotFetch = UniTools.InitDt(new string[] { "NEW_ROBOT_FETCH", "NEW_ROBOT_FETCH_DESC" });

                foreach (string _fetchMode in Enum.GetNames(typeof(eRobotFetchSequenceMode)))
                {
                    int _value = (int)Enum.Parse(typeof(eRobotFetchSequenceMode), _fetchMode);

                    if (_value == 0) continue;

                    eRobotFetchSequenceMode robotOpeMode = (eRobotFetchSequenceMode)Enum.Parse(typeof(eRobotFetchSequenceMode), _fetchMode);
                    string desc = Public.GetEnumDesc(robotOpeMode);
                    DataRow drNew = _dtRobotFetch.NewRow();
                    drNew["NEW_ROBOT_FETCH"] = _value;
                    drNew["NEW_ROBOT_FETCH_DESC"] = string.Format("{0}:{1}", _value, desc);
                    _dtRobotFetch.Rows.Add(drNew);
                }
                cboRobotFetchSeqMode_New.DataSource = _dtRobotFetch;
                cboRobotFetchSeqMode_New.DisplayMember = "NEW_ROBOT_FETCH_DESC";
                cboRobotFetchSeqMode_New.ValueMember = "NEW_ROBOT_FETCH";
                cboRobotFetchSeqMode_New.SelectedIndex = -1;
                #endregion

                txtLocalNo.Text = IndexerNode.NodeNo;
                txtLocalID.Text = IndexerNode.NodeID;
                RefreshData();
                cboNextPlanID.Visible = false;
                rdoNG.Checked = true;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void RefreshData()
        {
            try
            {
                txtIndexerMode.Text = UniTools.GetEquipmentIndexerModeDesc(FormMainMDI.G_OPIAp.CurLine.IndexerMode.ToString());
                txtCoolRunSetCount.Text = FormMainMDI.G_OPIAp.CurLine.CoolRunSetCount.ToString();
                txtCoolRunRemainCnt.Text = FormMainMDI.G_OPIAp.CurLine.CoolRunRemainCount.ToString();
                txtRobotFetchSeqMode.Text = FormMainMDI.G_OPIAp.CurLine.RobotFetchSequenceMode.ToString();
                //if (gbRobotOperMode.Visible)
                //{
                //    List<IndexerRobotStage> _lstStages = FormMainMDI.G_OPIAp.CurLine.BC_RobotOperationModeReply.IndexerRobotStages;

                //    foreach (DataGridViewRow _row in dgvRobotOperMode.Rows)
                //    {
                //        string _positionNo = _row.Cells[colPositionNo.Name].Value.ToString();

                //        IndexerRobotStage _stage = _lstStages.Find(s => s.RobotPosNo == _positionNo);

                //        if (_stage == null) continue;

                //        if (_stage.OperationMode.ToString() == _row.Cells[colCurMode.Name].Value.ToString()) continue;

                //        _row.Cells[colCurMode.Name].Value = _stage.OperationMode.ToString();
                //    }
                //}

                //if (gbEqpFetchGlassPL.Visible)
                //{
                //    txtEQRuleName_1.Text = FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.GetRuleName_1();
                //    txtEQRuleValue_1.Text = FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleValue_1;
                //    txtEQRuleName_2.Text = FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.GetRuleName_2();
                //    txtEQRuleValue_2.Text = FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleValue_2;
                //}
                

                //3:Changer Mode
                if (FormMainMDI.G_OPIAp.CurLine.IndexerMode == 3)
                {
                    if (txtCurrentPlanID.Text != FormMainMDI.G_OPIAp.CurLine.ChangerPlanID) txtCurrentPlanID.Text = FormMainMDI.G_OPIAp.CurLine.ChangerPlanID;
                    pnlCurrentPlanID.Visible = true;
                    grbChangerPlanDownload.Visible = true;
                }
                else
                {
                    txtCurrentPlanID.Text = string.Empty;
                    pnlCurrentPlanID.Visible = false;
                    grbChangerPlanDownload.Visible = false;

                    rdoOK.Checked = false;
                    rdoNG.Checked = false;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, System.EventArgs e)
        {
            try
            {
                Send_LineStatusRequest();

                Send_CurrentChangerPlanRequest();
                //if (gbRobotOperMode.Visible)
                //{
                //    Send_RobotOperationModeRequest();
                //}

                //if (gbEqpFetchGlassPL.Visible)
                //{
                //    Send_EquipmentFetchGlassRuleRequest();
                //}
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvRobotOperMode_CellClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            try
            {
                if (!dgvRobotOperMode.Columns[e.ColumnIndex].Name.Equals(colSend.Name)) return;

                string _positionNo = dgvRobotOperMode.CurrentRow.Cells[colPositionNo.Name].Value.ToString();
                string _posDesc = dgvRobotOperMode.CurrentRow.Cells[colDescription.Name].Value.ToString();
                string _curMode = string.Empty;

                if (dgvRobotOperMode.CurrentRow.Cells[colCurMode.Name].Value != null)
                    _curMode = dgvRobotOperMode.CurrentRow.Cells[colCurMode.Name].Value.ToString();

                if (dgvRobotOperMode.CurrentRow.Cells[colNewMode.Name].Value == null || dgvRobotOperMode.CurrentRow.Cells[colNewMode.Name].Value.ToString() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text , "", "Please choose the New Mode", MessageBoxIcon.Error);
                    return;
                }
                string _newMode = dgvRobotOperMode.CurrentRow.Cells[colNewMode.Name].Value.ToString();

                string _newModeDesc = ((eRobotOperationMode)(int.Parse(_newMode))).ToString();

                if (_curMode.Equals(_newModeDesc))
                {
                    ShowMessage(this,lblCaption.Text , "", "The Current Mode and New Mode must be different!!", MessageBoxIcon.Error);
                    return;
                }

                string msg = string.Format("Please confirm whether you will change the Robot operation mode of positon : {0} into [{1}]?", _posDesc, _newModeDesc);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text )) return;

                #region send RobotOperationModeCommand

                RobotOperationModeCommand _trx = new RobotOperationModeCommand();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = IndexerNode.NodeNo;
                _trx.BODY.ROBOTPOSITIONNO = _positionNo;
                _trx.BODY.OPERATIONMODE = _newMode;

                //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;
                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                ShowMessage(this, lblCaption.Text, "", "Robot Operation Mod eCommand Send to BC Success !", MessageBoxIcon.Information);

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }      

        private void btnFetchGlass_Click(object sender, System.EventArgs e)
        {
            try
            {
                #region Check
                if (cbo1stPlRuleName.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose 1'st Proportional Rule Name", MessageBoxIcon.Warning);
                    cbo1stPlRuleName.Focus();
                    return;
                }
                if (string.IsNullOrWhiteSpace(txt1stPlRuleName.Text))
                {
                    ShowMessage(this, lblCaption.Text, "", "Please fill in 1'st Proportional Rule Value", MessageBoxIcon.Warning);
                    txt1stPlRuleName.Focus();
                    return;
                }
                if (cbo2ndPlRuleName.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose 2'nd Proportional Rule Name", MessageBoxIcon.Warning);
                    cbo2ndPlRuleName.Focus();
                    return;
                }
                if (string.IsNullOrWhiteSpace(txt2ndPlRuleName.Text))
                {
                    ShowMessage(this, lblCaption.Text, "", "Please fill in 2'nd Proportional Rule Value", MessageBoxIcon.Warning);
                    txt2ndPlRuleName.Focus();
                    return;
                }
                #endregion

                string msg = string.Format("Please confirm whether you will process the Equipment Fetch Glass Proportional ?");
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region EquipmentFetchGlassCommand
                EquipmentFetchGlassCommand _trx = new EquipmentFetchGlassCommand();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = IndexerNode.NodeNo;
                _trx.BODY.RULENAME1 = cbo1stPlRuleName.SelectedValue.ToString();
                _trx.BODY.RULEVALUE1 = txt1stPlRuleName.Text.Trim();
                _trx.BODY.RULENAME2 = cbo2ndPlRuleName.SelectedValue.ToString();
                _trx.BODY.RULEVALUE2 = txt2ndPlRuleName.Text.Trim();               

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region EquipmentFetchGlassCommandReply

                string _respXml = _resp.Xml;

                EquipmentFetchGlassCommandReply _equipmentFetchGlassCommandReply = (EquipmentFetchGlassCommandReply)Spec.CheckXMLFormat(_respXml);

                ShowMessage(this, lblCaption.Text, "", string.Format("Equipment Fetch Glass Command Send to BC Success"), MessageBoxIcon.Information);

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnIndexerMode_New_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (cboIndexerMode_New.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose the New Indexer Mode", MessageBoxIcon.Error);
                    cboIndexerMode_New.Focus();
                    return; 
                }

                string _newMode = cboIndexerMode_New.SelectedValue.ToString();
                string _newModeDesc = UniTools.GetEquipmentIndexerModeDesc(_newMode); // ((eIndexerMode)(int.Parse(_newMode))).ToString();

                //if (_newModeDesc == txtIndexerMode.Text)
                //{
                //    ShowMessage(this, lblCaption.Text, "", "The Current Indexer Mode and New Indexer Mode must be different!!", MessageBoxIcon.Error);
                //    cboIndexerMode_New.Focus();
                //    return;
                //}

                string msg = string.Format("Please confirm whether you will change the Indexer Operation Mode of Node:{0}  into [{1}] ?", IndexerNode.NodeNo, _newModeDesc);

                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;

                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region IndexerOperationModeChangeRequest
                IndexerOperationModeChangeRequest _trx = new IndexerOperationModeChangeRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = IndexerNode.NodeNo;
                _trx.BODY.INDEXEROPERATIONMODE = _newMode;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region IndexerOperationModeChangeReply

                string _respXml = _resp.Xml;

                IndexerOperationModeChangeReply _indexerOperationModeChangeReply = (IndexerOperationModeChangeReply)Spec.CheckXMLFormat(_respXml);

                ShowMessage(this, lblCaption.Text, "", string.Format("Indexer Operation Mode Change Request Send to BC Success"), MessageBoxIcon.Information);

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnCoolRunCount_New_Click(object sender, System.EventArgs e)
        {
            try
            {
                #region Check

                //判斷是否有填入New Cool Run Count
                if (string.IsNullOrWhiteSpace(txtCoolRunCount_New.Text.ToString().Trim()))
                {
                    ShowMessage(this, lblCaption.Text, "", "Please fill in New Cool Run Count", MessageBoxIcon.Warning);
                    txtCoolRunCount_New.Focus();
                    return;
                }

                int _coolRunCount = 0;
                int.TryParse(txtCoolRunCount_New.Text.ToString(), out _coolRunCount);

                if (_coolRunCount > 65535)
                {
                    ShowMessage(this, lblCaption.Text, "", "The New Cool Run Count Can't be  more than 65535", MessageBoxIcon.Warning);
                    txtCoolRunCount_New.Focus();
                    return;
                }

                if (_coolRunCount < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "The New Cool Run Count Can't be  less  than 0", MessageBoxIcon.Warning);
                    txtCoolRunCount_New.Focus();
                    return;
                }

                if (_coolRunCount.ToString() == txtCoolRunSetCount.Text.ToString())
                {
                    ShowMessage(this, lblCaption.Text, "", "The New Cool Run Count and Cool Run Set Count must be different!!", MessageBoxIcon.Error);
                    txtCoolRunCount_New.Focus();
                    return;
                }
                #endregion

                string msg = string.Format("Please confirm whether you will process the Cool Run Set Count into [{0}] ?", _coolRunCount.ToString());
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region 組command
                CoolRunSetRequest _trx = new CoolRunSetRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.COOLRUNCOUNT = _coolRunCount.ToString();               

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region CoolRunSetReply

                ShowMessage(this, lblCaption.Text, "", string.Format("Cool Run Set Request Send to BC Success"), MessageBoxIcon.Information);

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRobotFetchSeqMode_New_Click(object sender, EventArgs e)
        {
            try
            {
                #region Check

                if (cboRobotFetchSeqMode_New.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Robot Fetch Sequence Mode", MessageBoxIcon.Error);
                    cboRobotFetchSeqMode_New.Focus();
                    return;
                }
                #endregion

                string _newMode = cboRobotFetchSeqMode_New.SelectedValue.ToString();
                string _newModeDesc = ((eRobotFetchSequenceMode)(int.Parse(_newMode))).ToString();

                if (_newModeDesc == txtIndexerMode.Text)
                {
                    ShowMessage(this, lblCaption.Text, "", "The Current Indexer Mode and New Indexer Mode must be different!!", MessageBoxIcon.Error);
                    cboIndexerMode_New.Focus();
                    return;
                }

                string msg = string.Format("Please confirm whether you will change the  Robot Fetch Sequence Mode into [{0}] ?", _newModeDesc);

                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region RobotFetchSequenceModeChangeRequest
                RobotFetchSequenceModeChangeRequest _trx = new RobotFetchSequenceModeChangeRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.ROBOT_FETCH_SEQUENCE_MODE = _newMode;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region RobotFetchSequenceModeChangeReply

                ShowMessage(this, lblCaption.Text, "", string.Format("Robot Fetch Sequence Mode Change Change Request Send to BC Success"), MessageBoxIcon.Information);

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtCoolRunCount_New_TextChanged(object sender, System.EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                if (txtCoolRunCount_New.Text == string.Empty) return;

                int _num = 0;

                int.TryParse(_txt.Text.ToString(), out _num);


                if (_num > 65535 || _num < 0)
                {
                    ShowMessage(this, lblCaption.Text , "", " 0 <= Cool Run Count <= 65535", MessageBoxIcon.Error);

                    txtCoolRunCount_New.Text = string.Empty;

                    txtCoolRunCount_New.Focus();

                    return;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                asciiCode == 8)   // Backspace
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void btnChangerPlanCommand_Click(object sender, EventArgs e)
        {
            try
            {
                string _retrunCode = string.Empty;
                string _nextPlanID = string.Empty;

                if (rdoOK.Checked) _retrunCode = "OK";
                else _retrunCode = "NG"; 

                #region Check

                #region offline mode方可下達 OK / remote,local,offline mode可下達NG
                if (_retrunCode == "OK")
                {
                    if (FormMainMDI.G_OPIAp.CurLine.MesControlMode != "OFFLINE")
                    {
                        ShowMessage(this, lblCaption.Text, "", "MES Mode is not offline mode, can't set OK Command", MessageBoxIcon.Warning);
                        return;
                    }

                    if (cboNextPlanID.SelectedIndex == -1 || cboNextPlanID.SelectedValue == null)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please choose Next Plan ID", MessageBoxIcon.Warning);
                        return;
                    }

                    _nextPlanID = cboNextPlanID.SelectedValue.ToString();

                    if (_nextPlanID == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Current Plan ID is Empty", MessageBoxIcon.Error);
                        return;
                    }
                }
                else if (_retrunCode == "NG")
                {
                    if (cmbCancelPlan.SelectedIndex == 0) _nextPlanID = FormMainMDI.G_OPIAp.CurLine.ChangerPlanID;
                    if (cmbCancelPlan.SelectedIndex == 1) _nextPlanID = FormMainMDI.G_OPIAp.CurLine.StandByChangerPlanID;
                    if (cmbCancelPlan.SelectedIndex == 2) _nextPlanID = "ALLPLAN";
                    cmbCancelPlan.SelectedIndex = -1;
                    txtCancelPlan.Clear();
                }
                #endregion
               

                #endregion

                string msg = string.Format("Please confirm whether you will process the Changer Plan Download Set Command Request ?");
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                #region Send ChangerPlanDownloadSetCommandRequest
                ChangerPlanDownloadSetCommandRequest _trx = new ChangerPlanDownloadSetCommandRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.RETURNCODE = _retrunCode;
                _trx.BODY.PLANID = _nextPlanID;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region ChangerPlanDownloadSetCommandReply

                ShowMessage(this, lblCaption.Text, "", string.Format("Changer Plan Download Set Command Request Send to BC Success"), MessageBoxIcon.Information);

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString())
                {
                    tmrRefresh.Enabled = false;
                    return;
                }

                #region Send Request
                if (FormMainMDI.G_OPIAp.CurLine.IndexerNode == null)
                {
                    tmrRefresh.Enabled = false;

                    return;
                }

                #region RobotOperationModeReply
                //if (gbRobotOperMode.Visible)
                //{
                //    BCS_RobotOperationModeReply _reply = FormMainMDI.G_OPIAp.CurLine.BC_RobotOperationModeReply;

                //    if (_reply.IsReply)
                //    {
                //        DateTime _now = DateTime.Now;
                //        TimeSpan _ts = _now.Subtract(_reply.LastRequestDate).Duration();

                //        if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                //        {
                //            Send_RobotOperationModeRequest();
                //        }
                //    }
                //}
                #endregion

                #region EquipmentFetchGlassRuleReply
                //if (gbEqpFetchGlassPL.Visible)
                //{
                //    BCS_EquipmentFetchGlassRuleReply _reply = FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply;

                //    if (_reply.IsReply)
                //    {
                //        DateTime _now = DateTime.Now;
                //        TimeSpan _ts = _now.Subtract(_reply.LastRequestDate).Duration();

                //        if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                //        {
                //            Send_EquipmentFetchGlassRuleRequest();
                //        }
                //    }
                //}
                #endregion

                #endregion

                RefreshData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_RobotOperationModeRequest()
        {
            try
            {
                string _err = string.Empty;
                string _xml = string.Empty;

                RobotOperationModeRequest _trx = new RobotOperationModeRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                if (FormMainMDI.G_OPIAp.CurLine.IndexerNode != null)
                _trx.BODY.EQUIPMENTNO = FormMainMDI.G_OPIAp.CurLine.IndexerNode.NodeNo;
                else
                    return;
                _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                FormMainMDI.G_OPIAp.CurLine.BC_RobotOperationModeReply.LastRequestDate = DateTime.Now;

                FormMainMDI.G_OPIAp.CurLine.BC_RobotOperationModeReply.IsReply = false;
            }
            catch (Exception ex)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void Send_EquipmentFetchGlassRuleRequest()
        {
            try
            {
                string _err = string.Empty;
                string _xml = string.Empty;

                EquipmentFetchGlassRuleRequest _trx = new EquipmentFetchGlassRuleRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                if (FormMainMDI.G_OPIAp.CurLine.IndexerNode != null)
                    _trx.BODY.EQUIPMENTNO = FormMainMDI.G_OPIAp.CurLine.IndexerNode.NodeNo;
                else
                    return;
                _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.LastRequestDate = DateTime.Now;

                FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.IsReply = false;
            }
            catch (Exception ex)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void Send_LineStatusRequest()
        {
            try
            {
                string _err = string.Empty;
                string _xml = string.Empty;

                LineStatusRequest _trx = new LineStatusRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
            }
            catch (Exception ex)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void Send_CurrentChangerPlanRequest()
        {
            try
            {
                string _err = string.Empty;
                string _xml = string.Empty;

                CurrentChangerPlanRequest _trx = new CurrentChangerPlanRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
            }
            catch (Exception ex)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void rdoChangerPlan_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                if (_rdo.Checked == false) return;                

                string _code = _rdo.Tag.ToString();

                string _currentPlanID = txtCurrentPlanID.Text.ToString();

                if (_code == "OK")
                {
                    #region 提供user選擇一組非current plan id -- Get Next Plan ID ComboBox

                    UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                    var _lstPlanID = (from p in ctxBRM.SBRM_CHANGEPLAN
                                      where p.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && p.PLANID != _currentPlanID
                                      select new
                                      {
                                          p.PLANID
                                      }).Distinct().ToList();

                    if (_lstPlanID != null && _lstPlanID.Count > 0)
                    {
                        cboNextPlanID.DataSource = _lstPlanID;
                        cboNextPlanID.DisplayMember = "PLANID";
                        cboNextPlanID.ValueMember = "PLANID";
                        cboNextPlanID.SelectedIndex = -1;
                    }
                    #endregion

                    lblNextPlanID.Text = "Choose New Plan ID";
                    cboNextPlanID.Visible = true;
                    cmbCancelPlan.Visible = false;
                    txtCancelPlan.Visible = false;
                }
                else if (_code == "NG")
                {

                    lblNextPlanID.Text = "Cancel Plan ID";

                    cboNextPlanID.DataSource = null;
                    cboNextPlanID.Visible = false;

                    cmbCancelPlan.Visible = true;
                    txtCancelPlan.Visible = true;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cmbCancelPlan_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCancelPlan.SelectedIndex == 0)
            {
                txtCancelPlan.Visible = true;
                txtCancelPlan.Text = FormMainMDI.G_OPIAp.CurLine.ChangerPlanID;
            }
            if (cmbCancelPlan.SelectedIndex == 1)
            {
                txtCancelPlan.Visible = true;
                txtCancelPlan.Text = FormMainMDI.G_OPIAp.CurLine.StandByChangerPlanID;
            }
            if (cmbCancelPlan.SelectedIndex == 2)
            {
                txtCancelPlan.Visible = false;
            }
        }
    }
}
