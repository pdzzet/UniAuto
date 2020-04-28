using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormMonitorRecipe : FormBase
    {
        string MESControlMode = string.Empty;

        public Queue<string> Q_BCSReport { get; set; } //BC要OPI Pop的訊息

        DataTable dtRecipeParameter;
        BindingSource bsRecipeParameter;

        Dictionary<string, string> DicRecipe;

        bool Initial = false;

        public FormMonitorRecipe()
        {
            InitializeComponent();

            lblCaption.Text = "Recipe Parameter";

            dgvData.Rows.Clear();
            dgvDetail.Rows.Clear();
            rdoOffline.Checked = false;
            rdoOnlineLocal.Checked = false;
            rdoOnlineRemote.Checked = false;

            cboLocal.Items.Clear();
            cboRecipeName.Items.Clear();
            txtTotalReturn.Text = string.Empty;

            DicRecipe = new Dictionary<string, string>();
            Q_BCSReport = new Queue<string>();

            #region Create DataTable
            dtRecipeParameter = new DataTable();
            dtRecipeParameter.Columns.Add("EQPID", typeof(String));
            dtRecipeParameter.Columns.Add("RecipeNo", typeof(String));            
            dtRecipeParameter.Columns.Add("No", typeof(String));
            dtRecipeParameter.Columns.Add("ParameterName", typeof(String));
            dtRecipeParameter.Columns.Add("Value", typeof(String));
            dtRecipeParameter.Columns.Add("Format", typeof(String));

            bsRecipeParameter = new BindingSource();
            bsRecipeParameter.DataSource = dtRecipeParameter;
            dgvDetail.DataSource = bsRecipeParameter;
            #endregion

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

                #region Get Recipe Name
                Initial = true;
                txtTotalReturn.Text = string.Empty;
                cboRecipeName.DataSource = null;

                dtRecipeParameter.Rows.Clear();
                cboLocal.Items.Clear();
                dgvData.Rows.Clear();

                if (MESControlMode == null || MESControlMode == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text , string.Empty, "Please choose MES Control Mode", MessageBoxIcon.Error);
                    return;
                }

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                var _var = (from _recipe in FormMainMDI.G_OPIAp.DBCtx.SBRM_RECIPE
                            where _recipe.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType &&
                                  _recipe.ONLINECONTROLSTATE == MESControlMode
                            select new { RecipeName = _recipe.LINERECIPENAME, PPID = _recipe.PPID });

                List<comboxInfo> _lstRecipe = new List<comboxInfo>();
                DicRecipe = new Dictionary<string, string>();

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
                string _recipeName = cboRecipeName.Text.ToString();

                if (_recipeName == string.Empty)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Recipe Name required！", MessageBoxIcon.Warning);
                    cboRecipeName.DroppedDown = true;
                    return;
                }

                cboLocal.Items.Clear();
                dtRecipeParameter.Rows.Clear();

                #region Set Color

                if (dgvData.Rows.Count > 0) dgvData.ClearSelection();

                foreach (DataGridViewRow _row in dgvData.Rows)
                {
                    _row.DefaultCellStyle.BackColor = Color.White;

                    _row.Cells[colReturn_Return.Name].Value = string.Empty;
                    _row.Cells[colReturn_Msg.Name].Value = string.Empty;

                }
                #endregion


                #region 傳送RecipeParameterReportRequest
                RecipeParameterReportRequest _trx = new RecipeParameterReportRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.RECIPELINENAME = _recipeName;
                foreach (DataGridViewRow _row in dgvData.Rows)
                {
                    //if (_row.Cells[colReturn_CIMMode.Name].Value.ToString() == "OFF") continue;

                    RecipeParameterReportRequest.EQUIPMENTc _eq = new RecipeParameterReportRequest.EQUIPMENTc();

                    _eq.EQUIPMENTNO = _row.Cells[colReturn_LocalNo.Name].Value.ToString();
                    _eq.RECIPENO = _row.Cells[colReturn_RecipeNo.Name].Value.ToString();

                    _trx.BODY.EQUIPMENTLIST.Add(_eq);
                }                   

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp != null)
                {
                    #region RecipeParameterReportRequestReply

                    //string _respXml = _resp.Xml;

                    //RecipeParameterReportRequestReply _recipeParameterReportRequestReply = (RecipeParameterReportRequestReply)Spec.CheckXMLFormat(_respXml);

                    ShowMessage(this, lblCaption.Text, "", "Recipe Parameter Report Request Send to BC Success !", MessageBoxIcon.Information);

                    //if (FormMainMDI.SocketDriver.DicRecvMessageEvent.ContainsKey(RECIPE_PARAMETER_REPORT))
                    //    FormMainMDI.SocketDriver.DicRecvMessageEvent[RECIPE_PARAMETER_REPORT] = SocketDriver_RecvMessageEvent;
                    //else
                    //    FormMainMDI.SocketDriver.DicRecvMessageEvent.Add(RECIPE_PARAMETER_REPORT, SocketDriver_RecvMessageEvent);

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
        
      
        private void SetParamReturn(string xml)
        {
            try
            {
                string _localNo = string.Empty ;
                string _recipeNo = string.Empty;

                dtRecipeParameter.Rows.Clear();
                cboLocal.Items.Clear();
               
                if (cboRecipeName.Text.ToString() == string.Empty) return;

                if (dgvData.Rows.Count <= 0) return;

                RecipeParameterReturnReport _reply = (RecipeParameterReturnReport)Spec.CheckXMLFormat(xml);

                if (_reply.BODY.RECIPELINENAME == cboRecipeName.Text.ToString())
                {
                    foreach (RecipeParameterReturnReport.EQUIPMENTc _eq in _reply.BODY.EQUIPMENTLIST)
                    {
                        _localNo = _eq.EQUIPMENTNO;
                        _recipeNo = _eq.RECIPENO;

                        IEnumerable<DataGridViewRow> rows = dgvData.Rows
                                    .Cast<DataGridViewRow>()
                                    .Where(r => r.Cells[colReturn_LocalNo.Name].Value.ToString().Equals(_localNo) && r.Cells[colReturn_RecipeNo.Name].Value.ToString().Equals(_recipeNo));

                        if (rows.Count() == 0) 
                        {
                            ShowMessage(this, "RecipeParameterReturnReport", "", string.Format("can't find local no [{0}],recipe no [{1}]", _localNo, _recipeNo), MessageBoxIcon.Error);
                        } 
                        else 
                        {
                            DataGridViewRow _row = rows.First();

                            _row.Cells[colReturn_Return.Name].Value = _eq.RETURN.ToString();
                            _row.Cells[colReturn_Msg.Name].Value = _eq.NGMESSAGE.ToString();

                            if (_eq.DATALIST.Count > 0)
                            {
                                cboLocal.Items.Add(string.Format("{0} - {1}", _localNo, _recipeNo));

                                foreach (RecipeParameterReturnReport.DATAc _data in _eq.DATALIST)
                                {
                                    dtRecipeParameter.Rows.Add(_localNo,_recipeNo, _data.NO.PadLeft(3,'0'), _data.PARAMETERNAME, _data.VALUE, _data.EXPRESSION);         
                                }
                                dtRecipeParameter.DefaultView.Sort = "EQPID,RecipeNo,No";
                            }

                            switch (_eq.RETURN.ToString())
                            {
                                case "OK":
                                    _row.DefaultCellStyle.BackColor = Color.White;
                                    break;

                                case "NONE":
                                    _row.DefaultCellStyle.BackColor = Color.Khaki;
                                    break;

                                case "NG":
                                    _row.DefaultCellStyle.BackColor = Color.Red;
                                    break;

                                default :
                                    _row.DefaultCellStyle.BackColor = Color.Silver;
                                    break;
                            }
                        }
                    }

                    if (cboLocal.Items.Count > 0)
                    {
                        cboLocal.Items.Insert(0, "All");

                        cboLocal.SelectedItem = "All";
                    }

                    #region Check Return Result
                    string _return = string.Empty ;
                    bool _returnOK = true;
                    foreach (DataGridViewRow _row in dgvData.Rows)
                    {
                        _return = _row.Cells[colReturn_Return.Name].Value.ToString();

                        //Return : OK / NG / NONE
                        if (_return != "NONE" && _return != "OK")
                        {
                            _returnOK = false;
                            break;
                        }
                    }

                    txtTotalReturn.Text = (_returnOK ? "OK" : "NG");
                    #endregion
                }
                else
                {
                    ShowMessage(this, "RecipeParameterReturnReport", "", "Recipe Name is mismatch", MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        public void SetMesModeChoose(string MesMode,string RecipeName)
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
                    default:   break;
                }

                if (RecipeName != string.Empty)
                {
                    cboRecipeName.Text  = RecipeName;
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

                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_data[0]))
                    {
                        _node = FormMainMDI.G_OPIAp.Dic_Node[_data[0]];

                        dgvData.Rows.Add(_node.NodeID, _node.NodeNo, _data[1], _node.CIMMode.ToString(), string.Empty, string.Empty);

                    }
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

                dtRecipeParameter.Rows.Clear();
                cboLocal.Items.Clear();
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
            //按下enter鍵時處理
            if (e.KeyValue == 13)
            {
                string _ppid = string.Empty ;

                dtRecipeParameter.Rows.Clear();
                cboLocal.Items.Clear();
                dgvData.Rows.Clear();

                if (cboRecipeName.Text  == string.Empty ) return;

                if (DicRecipe.ContainsKey(cboRecipeName.Text))
                {
                    SetDataGrid_PPID(DicRecipe[cboRecipeName.Text]);
                }                
            }
        }

        private void cboLocal_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboLocal.SelectedItem == null) return;

                //local No - Recipe No
                string _data = cboLocal.SelectedItem.ToString();
                

                if (_data == "All") bsRecipeParameter.Filter = "";
                else
                {
                    string[] _split = _data.Split('-');
                    if (_split.Length != 2)
                    {
                        ShowMessage(this, lblCaption.Text , "", "ComboBox Value Format Error [LocalNo - RecipeNo]", MessageBoxIcon.Error);
                        return;
                    }
                    bsRecipeParameter.Filter = string.Format("EQPID='{0}' and RecipeNo='{1}'", _split[0].Trim(), _split[1].Trim());
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
                if (FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString())
                {
                    tmrRefresh.Enabled = false;
                    return;
                }

                RefreshData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
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

                #region Refresh Recipe Parameter Return
                if (Q_BCSReport.Count > 0)
                {
                    string _msg = Q_BCSReport.Dequeue();
                    SetParamReturn(_msg);
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

    }
}
