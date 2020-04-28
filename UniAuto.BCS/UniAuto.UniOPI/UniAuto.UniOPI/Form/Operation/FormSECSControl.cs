using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormSECSControl : FormBase
    {
        private string CurNode = string.Empty;

        private List<SECSInfo> CurNode_SECS;
        private List<SECSInfo> SECSInfos;

        private const string NODE_REPORTMODE_HSMS = "HSMS";
        private const string SECS_NIKON = "NIKON";
        private int GRID_COL_WIDTH = 200;

        private enum columnType
        {
            ComboBox,
            TextBox,
            Button
        }

        private class SECSInfo
        {
            public int SeqNo = 0;

            public string LocalNo = string.Empty;
            public string SECS = string.Empty;
            public string ColumnName = string.Empty;
            public string ColumnEffect = string.Empty;  //關聯性下拉選單使用(選擇後會影響的欄位)
            public string ColumnRefer = string.Empty;  //關聯性下拉選單使用(參考的欄位)
            public string RelationValue = string.Empty; //關聯性下拉選單-關聯value
            public string ItemValue = string.Empty;            
            public string Description = string.Empty; 

            public columnType ColumnType = columnType.TextBox;
            public List<comboxInfo> CboValues = new List<comboxInfo>();
        }

        public FormSECSControl()
        {
            InitializeComponent();
        }

        private void FormSECSControl_Load(object sender, EventArgs e)
        {
            gbCSOT.Visible = false;
            gbNikon.Visible = false;


            InitialForm();

            load_SECSControl();

            //取得HSMS
            InitalComboBox_HSMSNode();
        }

        private void load_SECSControl()
        {
            try
            {
                SECSInfos = new List<SECSInfo>();
                CurNode_SECS = new List<SECSInfo>();

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;

                List<SBRM_OPI_SECS_CTRL> _lstCtrl = (from _data in _ctx.SBRM_OPI_SECS_CTRL.Where(d => d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName))
                            select _data).ToList();

                foreach (SBRM_OPI_SECS_CTRL _ctrl in _lstCtrl)
                {
                    SECSInfo _info = new SECSInfo();
                    _info.LocalNo = _ctrl.NODENO;
                    _info.SECS = _ctrl.SECS;
                    _info.RelationValue = _ctrl.REFERVALUE;
                    _info.ColumnName = _ctrl.COLUMNNAME;
                    _info.SeqNo = _ctrl.SEQNO;
                    _info.Description = (_ctrl.DESCRIPTION == null ? string.Empty : _ctrl.DESCRIPTION);

                    if (_ctrl.ITEMVALUE == null || _ctrl.ITEMVALUE.ToString().Trim() == string.Empty)
                    {
                        _info.ItemValue = string.Empty ;
                        _info.ColumnType = columnType.TextBox;
                    }
                    else
                    {
                        //若ITEMVALUE=="BUTTON"，則columnType使用Button
                        if (_ctrl.ITEMVALUE == "BUTTON")
                        {
                            _info.ItemValue = string.Empty;
                            _info.ColumnType = columnType.Button;
                        }
                        else
                        {
                            _info.ColumnType = columnType.ComboBox;
                            _info.ItemValue = _ctrl.ITEMVALUE.ToString();

                            #region 取得combobox item & value
                            string[] _item = _ctrl.ITEMVALUE.ToString().Split(',');
                            foreach (string _value in _item)
                            {
                                string[] item = _value.Split(':');

                                if (item.Length != 2) continue;

                                comboxInfo _cbo = new comboxInfo();
                                _cbo.ITEM_ID = item[0].ToString();
                                _cbo.ITEM_NAME = item[1].ToString();

                                _info.CboValues.Add(_cbo);
                            }
                            #endregion
                        }
                    }

                    //關聯性下拉選單使用(選擇後會影響的column name)
                    if (_ctrl.EFFECTCOLUMN == null || _ctrl.EFFECTCOLUMN.Trim() == string.Empty) _info.ColumnEffect = string.Empty;
                    else _info.ColumnEffect = _ctrl.EFFECTCOLUMN;

                    if (_ctrl.REFERCOLUMN == null || _ctrl.REFERCOLUMN.Trim() == string.Empty) _info.ColumnRefer = string.Empty;
                    else
                    {
                        _info.ColumnRefer = _ctrl.REFERCOLUMN;

                        _info.RelationValue = (_ctrl.REFERVALUE == null?string.Empty :_ctrl.REFERVALUE);
                    }

                    SECSInfos.Add(_info);
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void InitalComboBox_HSMSNode()
        {
            try
            {
                DataTable dtNode = UniTools.InitDt(new string[] { "NODENO", "NODENAME", "NODEDESC", "REPORTMODE" });

                foreach (Node node in FormMainMDI.G_OPIAp.Dic_Node.Values.Where(d => d.ReportMode.Contains(NODE_REPORTMODE_HSMS)))
                {
                    DataRow drNew = dtNode.NewRow();
                    drNew["NODENO"] = node.NodeNo;
                    drNew["NODEDESC"] = string.Format("{0}:{1}", node.NodeNo, node.NodeName);
                    drNew["REPORTMODE"] = node.ReportMode;
                    //drNew["SECS"] = node.SECS;
                    dtNode.Rows.Add(drNew);
                }

                cboNode.DataSource = dtNode;
                cboNode.DisplayMember = "NODEDESC";
                cboNode.ValueMember = "NODENO";
                cboNode.SelectedIndex = -1;

                if (dtNode.Rows.Count > 0) cboNode.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialForm()
        {
            try
            {
                foreach (RadioButton _rdo in flpSECSCsot.Controls.OfType<RadioButton>())
                {
                    _rdo.Checked = false;
                }

                foreach (RadioButton _rdo in flpNikonSECS.Controls.OfType<RadioButton>())
                {
                    _rdo.Checked = false;
                }

                dgvSECS.Rows.Clear();

                #region 清除datagridview columns (保留SECS Name & SECS Desc欄位)
                for (int i = dgvSECS.Columns.Count - 1; i >= 0; i--)
                {
                    if (dgvSECS.Columns[i].Name.Equals(colSECSName.Name) || dgvSECS.Columns[i].Name.Equals(colSECSDesc.Name)) continue;
                    dgvSECS.Columns.RemoveAt(i);
                }
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                InitialForm();

                if (cboNode.SelectedIndex == -1)
                {
                    gbCSOT.Visible = false;
                    gbNikon.Visible = false;

                    return;
                }                

                FlowLayoutPanel _pnl = null;

                DataRow _node = ((DataRowView)cboNode.SelectedItem).Row;

                CurNode = _node["NODENO"].ToString();

                CurNode_SECS = SECSInfos.FindAll(r => r.LocalNo.Equals(CurNode));

                string _reportMode = string.Empty;

                if (!string.IsNullOrWhiteSpace(_node["REPORTMODE"].ToString()))
                    _reportMode = _node["REPORTMODE"].ToString();

                if (_reportMode.Contains(SECS_NIKON))
                {
                    gbNikon.Visible = true;
                    gbCSOT.Visible = false;
                    _pnl = flpNikonSECS;
                }
                else
                {
                    gbNikon.Visible = false;
                    gbCSOT.Visible = true;
                    _pnl = flpSECSCsot;
                }
                
                if (_pnl != null)
                {
                    foreach (RadioButton _rdo in _pnl.Controls.OfType<RadioButton>())
                    {
                        SECSInfo _secs = CurNode_SECS.Find(r=>r.SECS.Equals(_rdo.Tag.ToString()));

                        if (_secs == null) _rdo.Visible =false;
                        else _rdo .Visible =true;                       
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                if (!_rdo.Checked) return;

                Initial_GridColumns(_rdo);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Initial_GridColumns(RadioButton rdo)
        {
            try
            {
                string _secsName = string.Empty;
                string _secsDesc = rdo.Text;
                string _memo = string.Empty;
                bool _visible = true;

                if (rdo.Tag != null) _secsName = rdo.Tag.ToString();
                else
                {
                    string[] secsDescs = _secsDesc.Split(':');
                    if (secsDescs.Length > 0) _secsName = secsDescs[0].ToString();
                }

                dgvSECS.Rows.Clear();

                #region 清除datagridview columns (保留SECS Name & SECS Desc欄位)
                for (int i = dgvSECS.Columns.Count - 1; i >= 0; i--)
                {
                    if (dgvSECS.Columns[i].Name.Equals(colSECSName.Name) || dgvSECS.Columns[i].Name.Equals(colSECSDesc.Name)) continue;
                    dgvSECS.Columns.RemoveAt(i);
                }
                #endregion

                if (CurNode_SECS == null || CurNode_SECS.Count() == 0) return;

                #region 產生欄位

                var _secsColumns = from row in CurNode_SECS
                                   where row.SECS.Equals(_secsName)
                                orderby row.SeqNo
                                group row by new { row.ColumnName, row.ColumnType,row.Description } into grp
                                select grp;

                if (_secsColumns == null || _secsColumns.Count() == 0)
                {
                    dgvSECS.Rows.Add(_secsName, _secsDesc);
                    lblDesc.Text = _memo;

                    return;
                }

                foreach (var v in _secsColumns)
                {
                    _visible = true;

                    if (v.Key.ColumnName != string.Empty)
                    {
                        switch (v.Key.ColumnType)
                        {
                            case columnType.ComboBox:

                                #region ComboBoxColumn
                                var _cbo = from row in CurNode_SECS
                                           where row.ColumnName.Equals(v.Key.ColumnName) && row.SECS.Equals(_secsName)
                                           select row;

                                if (_cbo == null || _cbo.Count() == 0) continue;

                                DataGridViewComboBoxColumn _cboColumn = new DataGridViewComboBoxColumn();
                                _cboColumn.Name = v.Key.ColumnName;
                                _cboColumn.Width = GRID_COL_WIDTH;
                                _cboColumn.DisplayMember = "ITEM_DESC";
                                _cboColumn.ValueMember = "ITEM_ID";
                                _cboColumn.HeaderText = v.Key.ColumnName;

                                //若大於一筆表示有參考前欄位內容顯示
                                if (_cbo.Count() == 1)
                                {
                                    SECSInfo _curSecsInfo = _cbo.FirstOrDefault();
                                    _cboColumn.DataSource = _curSecsInfo.CboValues;
                                }
                                else
                                {
                                    List<SECSInfo> lst = _cbo.ToList();
                                    _cboColumn.Tag = lst;
                                }
                                dgvSECS.Columns.Add(_cboColumn);

                                break;

                                #endregion                               

                            case columnType.TextBox:

                                #region TextBoxColumn
                                var _txt = from row in CurNode_SECS
                                           where row.ColumnName.Equals(v.Key.ColumnName) && row.SECS.Equals(_secsName) 
                                           select row;

                                if (_txt == null || _txt.Count() == 0) continue;

                                
                                DataGridViewTextBoxColumn _txtColumn = new DataGridViewTextBoxColumn();
                                _txtColumn.Name = v.Key.ColumnName;
                                _txtColumn.HeaderText = v.Key.ColumnName;
                                _txtColumn.Width = GRID_COL_WIDTH;

                                
                                //若大於一筆表示有參考前欄位內容顯示
                                if (_txt.Count() > 1)
                                {
                                    var _cur = _txt.Where(r => r.Description.Equals(v.Key.Description)).First();

                                    _txtColumn.Name = v.Key.ColumnName + "_" + _cur.RelationValue.ToString();

                                    dgvSECS.Columns.Add(_txtColumn);

                                    dgvSECS.Columns[dgvSECS.Columns.Count - 1].Visible = false;

                                    dgvSECS.Columns[dgvSECS.Columns.Count - 1].Tag = _cur.Description;

                                    _visible = false;
                                }
                                else
                                {
                                    dgvSECS.Columns.Add(_txtColumn);

                                    dgvSECS.Columns[dgvSECS.Columns.Count - 1].Tag = _txt.First().Description;
                                }

                                break;

                                #endregion

                            case columnType.Button:

                                #region ButtonColumn

                                switch (_secsName)
                                {
                                    //case "S2F119": //S2F119 Equipment Mode Change Command Send.
	                                    
                                    //    DataGridViewButtonColumn _btn = new DataGridViewButtonColumn();
                                    //    _btn.Text = "Equipment Mode Change";
                                    //    _btn.Name = v.Key.ColumnName;
                                    //    _btn.Tag = _secsName;
                                    //    dgvSECS.Columns.Add(_btn);
                                    //    break ;

                                    default :
                                        break ;
                                }

                                break;

                                #endregion
                                
                        }
                    }

                    if (_visible == false) continue;

                    if (!string.IsNullOrWhiteSpace(v.Key.Description))
                    {
                        if (string.IsNullOrWhiteSpace(_memo))
                        {
                            if (v.Key.ColumnName == string.Empty) _memo =v.Key.Description;
                            else _memo = string.Format("{0}:{1}", v.Key.ColumnName, v.Key.Description);
                        }
                        else
                            _memo = string.Format("{0};    {1}:{2}", _memo, v.Key.ColumnName, v.Key.Description);
                    }
                }
                #endregion

                dgvSECS.Rows.Add(_secsName, _secsDesc);
                lblDesc.Text = _memo;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvSECS_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewColumn dgvCol = ((DataGridView)sender).Columns[e.ColumnIndex] as DataGridViewColumn;

                if (dgvCol.CellType == typeof(DataGridViewComboBoxCell))
                {
                    DataGridViewComboBoxColumn dgvComb = ((DataGridView)sender).Columns[e.ColumnIndex] as DataGridViewComboBoxColumn;
                    if (dgvComb.Tag == null)
                    {
                        //判斷是否有關聯其他欄位資料來源，若有將其選擇的值清掉
                        string CombColName = dgvComb.Name;
                        string SecsName = dgvSECS.CurrentRow.Cells[colSECSName.Name].Value.ToString();

                        var relCol = CurNode_SECS.Where(d => d.SECS.Equals(SecsName) && d.ColumnName.Equals(CombColName) && d.ColumnEffect != string.Empty).FirstOrDefault();
                        if (relCol == null) return;
                        dgvSECS.CurrentRow.Cells[relCol.ColumnEffect].Value = null;
                    }
                    else
                    {
                        //來源的數據需要依關聯欄位作篩選
                        List<SECSInfo> lstCmb = (List<SECSInfo>)dgvComb.Tag;

                        if (dgvSECS.CurrentRow.Cells[lstCmb[0].ColumnRefer].Value == null) return;

                        string _referValue = dgvSECS.CurrentRow.Cells[lstCmb[0].ColumnRefer].Value.ToString();

                        SECSInfo secsInfo = lstCmb.Find(d => d.RelationValue.Equals(_referValue));

                        if (secsInfo != null)
                            dgvComb.DataSource = secsInfo.CboValues;
                    }
                }
                else if (dgvCol.CellType == typeof(DataGridViewButtonCell))
                {
                    DataGridViewButtonColumn dgvButton = ((DataGridView)sender).Columns[e.ColumnIndex] as DataGridViewButtonColumn;

                    switch (dgvButton.Tag.ToString())
                    {
                        //case "S2F119":
                        //    FormSECSControl_S2F119 _frm = new FormSECSControl_S2F119(CurNode);
                        //    _frm.ShowDialog();
                        //    if (_frm != null) _frm.Dispose();
                        //    break;
                        default :
                            break;
                    }
                    //if (dgvButton.Tag != null)
                    //{
                    //    lblDesc.Text = dgvButton.Tag.ToString();
                    //}
                }
                else if (dgvCol.CellType == typeof(DataGridViewTextBoxCell))
                {
                    DataGridViewTextBoxColumn dgvTextBox = ((DataGridView)sender).Columns[e.ColumnIndex] as DataGridViewTextBoxColumn;

                    if (dgvTextBox.Tag != null)
                    {
                        lblDesc.Text = dgvTextBox.Tag.ToString(); 
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                SetSECSFunctionRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetSECSFunctionRequest()
        {
            try
            {
                if (CurNode == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose the EQP", MessageBoxIcon.Error);
                    return;
                }

                if (dgvSECS.Rows.Count == 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "No SECS can't send", MessageBoxIcon.Error);
                    return;
                }

                string secsName = dgvSECS.CurrentRow.Cells[colSECSName.Name].Value.ToString();
                string secsDesc = dgvSECS.CurrentRow.Cells[colSECSDesc.Name].Value.ToString();

                List<SECSFunctionRequest.PARAMETERc> lstParam = new List<SECSFunctionRequest.PARAMETERc>();
                foreach (DataGridViewColumn dgvc in dgvSECS.Columns)
                {
                    if (dgvc.Name.Equals(colSECSName.Name) || dgvc.Name.Equals(colSECSDesc.Name)) continue;
                    if (dgvc.Visible == false) continue;
                    SECSFunctionRequest.PARAMETERc param = new SECSFunctionRequest.PARAMETERc();

                    string[] _key = dgvc.Name.Split('_');

                    if (_key.Length == 2) param.NAME = _key[0];
                    else param.NAME = dgvc.Name;

                    string value = string.Empty;
                    if (dgvSECS.CurrentRow.Cells[dgvc.Name].Value != null) value = dgvSECS.CurrentRow.Cells[dgvc.Name].Value.ToString();

                    if (value == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Please input {0} Value", param.NAME), MessageBoxIcon.Error);
                        return;
                    }
                    param.VALUE = value;
                    
                    lstParam.Add(param);
                }

                string msg = string.Format("Please confirm whether you will process the {0} of Node:{1} ?", secsDesc, CurNode);
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                string error = string.Empty;

                SECSFunctionRequest _trx = new SECSFunctionRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurNode;
                _trx.BODY.SECSNAME = secsName;
                _trx.BODY.PARAMETERLIST = lstParam;

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), out error, FormMainMDI.G_OPIAp.SessionID);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvSECS_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

                DataGridViewColumn _col = ((DataGridView)sender).Columns[e.ColumnIndex] as DataGridViewColumn;

                if (_col.CellType == typeof(DataGridViewComboBoxCell))
                {
                    //string _value = CurNode_SECS.Where(d => d.SECS.Equals(SecsName) && d.ColumnName.Equals(CombColName) && d.ColumnEffect != string.Empty).FirstOrDefault();
                    DataGridViewComboBoxColumn dgvComb = ((DataGridView)sender).Columns[e.ColumnIndex] as DataGridViewComboBoxColumn;

                    if (dgvComb.Tag == null)
                    {
                        //判斷是否有關聯其他欄位資料來源，若有將其選擇的值清掉
                        string CombColName = dgvComb.Name;
                        string SecsName = dgvSECS.CurrentRow.Cells[colSECSName.Name].Value.ToString();
                        string _relationValue = dgvSECS.CurrentRow.Cells[CombColName].Value.ToString();

                        var relCol = CurNode_SECS.Where(d => d.SECS.Equals(SecsName) && d.ColumnName.Equals(CombColName) && d.ItemValue != string.Empty ).First();

                        if (relCol == null) return;

                        if (relCol.ColumnEffect == string.Empty) return;

                        var curCol = CurNode_SECS.Where(d => d.SECS.Equals(SecsName) && d.ColumnName.Equals(relCol.ColumnEffect) && d.ColumnRefer.Equals(CombColName) && d.RelationValue.Equals(_relationValue));

                        if (curCol == null) return;

                        if (curCol.Count()==0) return;

                        SECSInfo _secs = curCol.First();

                        string _curColName = relCol.ColumnEffect;
                        if (_secs.ColumnType== columnType.TextBox) _curColName = string.Format("{0}_{1}", _curColName, _relationValue);

                        foreach (DataGridViewColumn _colTmp in dgvSECS.Columns)
                        {
                            if (_colTmp.Name.Contains(relCol.ColumnEffect))
                            {
                                if (_colTmp.Name.Equals(_curColName))
                                {
                                    _colTmp.Visible = true;
                                    lblDesc.Text = _secs.Description.ToString(); 
                                }
                                else
                                {
                                    _colTmp.Visible = false;                                    
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

        private void dgvSECS_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            try
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, anError.Exception.ToString());
                MessageBox.Show("dgvProduct Error happened " + anError.Context.ToString());

                if (anError.Context == DataGridViewDataErrorContexts.Commit)
                {
                    MessageBox.Show("Commit error");
                }
                if (anError.Context == DataGridViewDataErrorContexts.CurrentCellChange)
                {
                    MessageBox.Show("Cell change");
                }
                if (anError.Context == DataGridViewDataErrorContexts.Parsing)
                {
                    MessageBox.Show("parsing error");
                }
                if (anError.Context == DataGridViewDataErrorContexts.LeaveControl)
                {
                    MessageBox.Show("leave control error");
                }

                if ((anError.Exception) is ConstraintException)
                {
                    DataGridView view = (DataGridView)sender;
                    view.Rows[anError.RowIndex].ErrorText = "an error";
                    view.Rows[anError.RowIndex].Cells[anError.ColumnIndex].ErrorText = "an error";

                    anError.ThrowException = false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvSECS_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvSECS.IsCurrentCellDirty)
            {
                dgvSECS.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }  
        }
    }
}
