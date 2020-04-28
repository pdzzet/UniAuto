using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;


namespace UniOPI
{
    public partial class FormRecipeRegister : FormBase
    {
        string MESControlMode = string.Empty;

        Dictionary<string, string> DicRecipe;
        Dictionary<string, Node> Dic_UseNode { get; set; } // Key: NODENO

        bool Initial = false;

        public Queue<string> Q_BCSReport { get; set; } //BC要OPI Pop的訊息


        public FormRecipeRegister()
        {
            InitializeComponent();

            Dic_UseNode = FormMainMDI.G_OPIAp.Dic_Node;
        }

        private void FormRecipeRegister_Load(object sender, System.EventArgs e)
        {
            Q_BCSReport = new Queue<string>();
           
            SetPort();

            #region TBBFG機台沒有REMOTE MODE的模式
            if (FormMainMDI.G_OPIAp.CurLine.LineType == "BFG")
            {
                rdoOnlineRemote.Visible = false;
            }
            #endregion
        }

        private void rdo_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                if (!_rdo.Checked) return;

                MESControlMode = _rdo.Tag.ToString();

                ReloadRecipeName(MESControlMode);            
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void ReloadRecipeName(string mesMode)
        {
            try
            {
                #region Get Recipe Name
                Initial = true;
                txtTotalReturn.Text = string.Empty;
                cboRecipeName.DataSource = null;

                dgvData.Rows.Clear();

                if (mesMode == null || mesMode == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, string.Empty, "Please choose MES Control Mode", MessageBoxIcon.Error);
                    return;
                }

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                List<comboxInfo> _lstRecipe = new List<comboxInfo>();
                DicRecipe = new Dictionary<string, string>();

                var _var = (from _recipe in FormMainMDI.G_OPIAp.DBCtx.SBRM_RECIPE
                            where _recipe.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType &&
                                    _recipe.ONLINECONTROLSTATE == MESControlMode
                            select new { RecipeName = _recipe.LINERECIPENAME, PPID = _recipe.PPID });

                foreach (var _recipe in _var)
                {
                    _lstRecipe.Add(new comboxInfo { ITEM_NAME = _recipe.RecipeName, ITEM_ID = _recipe.PPID });

                    DicRecipe.Add(_recipe.RecipeName, _recipe.PPID);
                }                

                cboRecipeName.DataSource = _lstRecipe.ToList();
                cboRecipeName.DisplayMember = "ITEM_NAME";
                cboRecipeName.ValueMember = "ITEM_ID";
                cboRecipeName.SelectedIndex = -1;
                Initial = false;
                #endregion
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
                string _portNo = "0";
                string _recipeName = cboRecipeName.Text.ToString();

                if (_recipeName == string.Empty)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Recipe Name required！", MessageBoxIcon.Warning);
                    cboRecipeName.DroppedDown = true;
                    return;
                }

                if (dgvData.Rows.Count <= 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "No Recipe Data in DataGridView ！", MessageBoxIcon.Warning);
                    return;
                }

                if (cboPortNo.SelectedValue != null)
                {
                    _portNo = cboPortNo.SelectedValue.ToString();
                }

                #region 清除gridview return data

                txtTotalReturn.Text = string.Empty;

                #region Set Color
                if (dgvData.Rows.Count > 0) dgvData.ClearSelection();
                #endregion

                foreach (DataGridViewRow _row in dgvData.Rows)
                {
                    _row.DefaultCellStyle.BackColor = Color.White;

                    _row.Cells[colReturn_Return.Name].Value = string.Empty;
                    _row.Cells[colReturn_Msg.Name].Value = string.Empty;
                }
                #endregion

                #region 傳送RecipeRegisterValidationCommandRequest
                RecipeRegisterValidationCommandRequest _trx = new RecipeRegisterValidationCommandRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PORTNO = _portNo;

                RecipeRegisterValidationCommandRequest.LINEc _line = new RecipeRegisterValidationCommandRequest.LINEc();
                _line.RECIPELINENAME =  FormMainMDI.G_OPIAp.CurLine.ServerName;

                RecipeRegisterValidationCommandRequest.RECIPECHECKc _recipe = new RecipeRegisterValidationCommandRequest.RECIPECHECKc();
                _recipe.RECIPENAME = _recipeName;

                foreach (DataGridViewRow _row in dgvData.Rows)
                {
                    if (_row.Cells[colReturn_LocalNo.Name].Value.ToString() == string.Empty) continue;
                    if (_row.Cells[colReturn_RecipeNo.Name].Value.ToString() == string.Empty) continue;

                    RecipeRegisterValidationCommandRequest.EQUIPMENTc _eq = new RecipeRegisterValidationCommandRequest.EQUIPMENTc();
                    _eq.EQUIPMENTNO = _row.Cells[colReturn_LocalNo.Name].Value.ToString();
                    _eq.RECIPENO = _row.Cells[colReturn_RecipeNo.Name].Value.ToString();

                    //2015-03-17 由BC處理---昌爺
                    //#region 當Recipe No為0和CIM Mode為OFF時，按下Send要Pass掉不處理 --登京
                    //if (FormMainMDI.G_OPIAp.Dic_Node[_eq.EQUIPMENTNO].CIMMode == eCIMMode.OFF) continue;
                    //if (_eq.RECIPENO == "0" || (_eq.RECIPENO == "0".PadLeft(_eq.RECIPENO.Length, '0'))) continue;
                    //#endregion

                    _recipe.EQUIPMENTLIST.Add(_eq);
                }

                _line.RECIPECHECKLIST.Add(_recipe);

                _trx.BODY.LINELIST.Add(_line);


                string _xml = _trx.WriteToXml();
                string _err = string.Empty;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp != null)
                {
                    #region RecipeRegisterValidationCommandReply

                    string _respXml = _resp.Xml;

                    RecipeRegisterValidationCommandReply _recipeRegisterValidationCommandReply = (RecipeRegisterValidationCommandReply)Spec.CheckXMLFormat(_respXml);

                    ShowMessage(this, lblCaption.Text, "", "Recipe Register Request Send to BC Success !", MessageBoxIcon.Information);

                    #endregion
                }

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetPort()
        {
            try
            {
                var _q = (from port in FormMainMDI.G_OPIAp.Dic_Port.Values
                          where (port.PortType == ePortType.LoadingPort || port.PortType == ePortType.LoaderinBufferType || port.PortType == ePortType.BothPort) // "LoadingPort" || port.PORTTYPE == "BufferPort_LoaderInBufferType") 
                          select port
                    ).ToList();

                if (_q == null || _q.Count == 0)
                {
                    cboPortNo.DataSource = null;
                    return;
                }

                Port _p = new Port();
                _q.Insert(0, _p);

                cboPortNo.DataSource = _q;
                cboPortNo.DisplayMember = "PORTNO";
                cboPortNo.ValueMember = "PORTNO";
                cboPortNo.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetRegisterReturn(string xml)
        {
            try
            {
                string _localNo = string.Empty;
                string _recipeNo = string.Empty;
                string _useServerName = string.Empty;

                bool _findData = false;  //回覆的recipe name是否包含畫面顯示的recipe name

                if (cboRecipeName.Text.ToString() == string.Empty) return;

                if (dgvData.Rows.Count <= 0) return;

                _useServerName = FormMainMDI.G_OPIAp.CurLine.ServerName;

                RecipeRegisterValidationReturnReport _reply = (RecipeRegisterValidationReturnReport)Spec.CheckXMLFormat(xml);

                foreach (RecipeRegisterValidationReturnReport.LINEc _line in _reply.BODY.LINELIST)
                {
                    if (_useServerName == _line.RECIPELINENAME)
                    {
                        #region find recpe name
                        foreach (RecipeRegisterValidationReturnReport.RECIPECHECKc _recipeCheck in _line.RECIPECHECKLIST)
                        {                            
                            if (_recipeCheck.RECIPENAME == cboRecipeName.Text.ToString())
                            {
                                _findData = true;

                                #region Get Register return
                                foreach (RecipeRegisterValidationReturnReport.EQUIPMENTc _eq in _recipeCheck.EQUIPMENTLIST)
                                {
                                    _localNo = _eq.EQUIPMENTNO;
                                    _recipeNo = _eq.RECIPENO.ToString();

                                    IEnumerable<DataGridViewRow> rows = dgvData.Rows
                                                .Cast<DataGridViewRow>()
                                                .Where(r => r.Cells[colReturn_LocalNo.Name].Value.ToString().Equals(_localNo) && r.Cells[colReturn_RecipeNo.Name].Value.ToString().Equals(_recipeNo));

                                    if (rows.Count() == 0)
                                    {
                                        ShowMessage(this, lblCaption.Text , "", string.Format("can't find local no [{0}],recipe no [{1}]", _localNo, _recipeNo), MessageBoxIcon.Error);
                                    }
                                    else
                                    {
                                        DataGridViewRow _row = rows.First();

                                        _row.Cells[colReturn_Return.Name].Value = _eq.RETURN.ToString();
                                        _row.Cells[colReturn_Msg.Name].Value = _eq.NGMESSAGE.ToString();

                                        //RecipeRegisterValidationReturnReport
                                        //1. OK:
                                        //2. NG: NG, MACHINENG, NOMACHINE, RECIPELENNG, TIMEOUT
                                        //3. NONE: ZERO, CIMOFF, NOCHECK
                                        switch (_eq.RETURN.ToString())
                                        {
                                            case "OK":
                                                _row.DefaultCellStyle.BackColor = Color.White;
                                                break;

                                            case "NONE":
                                                _row.DefaultCellStyle.BackColor = Color.Gold;
                                                break;

                                            case "NG":
                                                if (_eq.NGMESSAGE == "TIMEOUT") _row.DefaultCellStyle.BackColor = Color.Pink;
                                                else _row.DefaultCellStyle.BackColor = Color.Red; 

                                                //_row.DefaultCellStyle.BackColor = Color.Red;
                                                break;

                                            default:
                                                _row.DefaultCellStyle.BackColor = Color.Silver;
                                                break;
                                        }
                                    }
                                }
                                #endregion
                            }
                        }

                        #region Check Return Result
                        if (_findData)
                        {
                            string _return = string.Empty;
                            bool _returnOK = true;
                            foreach (DataGridViewRow _row in dgvData.Rows)
                            {
                                _return = _row.Cells[colReturn_Return.Name].Value.ToString();

                                //OK , NG, NONE
                                if (_return != "OK" && _return != "NONE")
                                {
                                    _returnOK = false;
                                    break;
                                }
                            }

                            txtTotalReturn.Text = (_returnOK ? "OK" : "NG");
                        }
                        #endregion

                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public void SetMesModeChoose(string MesMode, string RecipeName)
        {
            try
            {
                MESControlMode = MesMode;

                rdoOffline.Checked = false;
                rdoOnlineLocal.Checked = false;
                rdoOnlineRemote.Checked = false;

                if (MESControlMode == null) return;

                ///LOCAL,OFFLINE,REMOTE
                switch (MESControlMode.ToUpper())
                {
                    case "OFFLINE": rdoOffline.Checked = true; break;
                    case "LOCAL": rdoOnlineLocal.Checked = true; break;
                    case "REMOTE": rdoOnlineRemote.Checked = true; break;
                    default: break;
                }

                if (RecipeName != string.Empty)
                {
                    cboRecipeName.Text = RecipeName;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetDataGrid_PPID(string PPID)
        {
            try
            {
                #region Show Recipe No Detail

                // PPID 儲存格式為 L2:RecipeNo;L3:RecipeNo;L4:RecipeNo...
                string[] _recipeNo = PPID.Split(';');

                Node _node;

                foreach (string _Nodedata in _recipeNo)
                {
                    string[] _data = _Nodedata.Split(':');

                    if (Dic_UseNode.ContainsKey(_data[0]))
                    {
                        _node = Dic_UseNode[_data[0]];

                        dgvData.Rows.Add(_node.NodeID, _node.NodeNo, _data[1], _node.CIMMode.ToString(), string.Empty, string.Empty);
                    }

                    //if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_data[0]))
                    //{
                    //    _node = FormMainMDI.G_OPIAp.Dic_Node[_data[0]];

                    //    dgvData.Rows.Add(_node.NODEID, _node.NODENO, _data[1], _node.CIMMode.ToString(), string.Empty, string.Empty);
                    //}
                }

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboRecipeName_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (Initial) return;

                dgvData.Rows.Clear();

                if (cboRecipeName.SelectedValue == null) return;

                string _ppid = cboRecipeName.SelectedValue.ToString();

                SetDataGrid_PPID(_ppid);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboRecipeName_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                //按下enter鍵時處理
                if (e.KeyValue == 13)
                {
                    string _ppid = string.Empty;

                    dgvData.Rows.Clear();

                    if (cboRecipeName.Text == string.Empty) return;

                    if (DicRecipe == null) return;

                    if (DicRecipe.ContainsKey(cboRecipeName.Text))
                    {
                        SetDataGrid_PPID(DicRecipe[cboRecipeName.Text]);
                    }
                }
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
                //timer不停止，需收到BC回覆後顯示在畫面上
                //if (FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString())
                //{
                //    tmrRefresh.Enabled = false;
                //    return;
                //}

                RefreshData();

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                tmrRefresh.Enabled = false;
            } 
        }

        public void RefreshData()
        {
            try
            {
                #region Refresh CIM Mode
                string _nodeNo = string.Empty;
                foreach (DataGridViewRow _row in dgvData.Rows)
                {
                    _nodeNo = _row.Cells[colReturn_LocalNo.Name].Value.ToString();

                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_nodeNo))
                    {
                        _row.Cells[colReturn_CIMMode.Name].Value = FormMainMDI.G_OPIAp.Dic_Node[_nodeNo].CIMMode.ToString();
                    }
                }                
                #endregion

                #region Refresh Register Return
                if (Q_BCSReport.Count > 0)
                {
                    string _msg = Q_BCSReport.Dequeue();
                    SetRegisterReturn(_msg);
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                tmrRefresh.Enabled = false;
            } 
        }

        private void chkCrossRecipe_CheckedChanged(object sender, EventArgs e)
        {            
            try
            {

                Dic_UseNode = FormMainMDI.G_OPIAp.Dic_Node;
                dgvData.Columns[colReturn_CIMMode.Name].Visible = true;
                
                ReloadRecipeName(MESControlMode);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
