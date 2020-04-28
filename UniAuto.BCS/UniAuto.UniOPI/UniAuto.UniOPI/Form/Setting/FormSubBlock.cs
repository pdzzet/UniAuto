using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UNILAYOUT;

namespace UniOPI
{
    public partial class FormSubBlock : FormBase
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr GetFocus();

        private List<string> lstTextbox = new List<string>() { "txtStartEQP", "txtControlEQP", "txtNextSubBlockEQP", "txtNextSubBlockEQPList" };

        //設定SubBlock顏色為15種
        Color[] SubBlock_Color = new Color[] { 
                Color.Brown, Color.CadetBlue, Color.CornflowerBlue, Color.DarkBlue,Color.ForestGreen, 
                Color.GreenYellow , Color.LightBlue , Color.LimeGreen, Color.Moccasin,Color.Olive,
                Color.Pink ,Color.SandyBrown,Color.Tan,Color.Tomato,Color.Aquamarine};

        ToolTip Tip;

        public FormSubBlock()
        {
            InitializeComponent();
            lblCaption.Text = "Sub Block";
        }
        
        private void FormSubBlock_Load(object sender, EventArgs e)
        {
            InitialCombox();

            try
            {
                Load_LayoutDesign();
                GetGridViewData();
                tlpEdit.Enabled = false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void EQ_Click(object sender, EventArgs e)
        {
            try
            {
                Control currFocus = this.GetFocusedControl();
                if (!lstTextbox.Contains(currFocus.Name))
                    return;

                Control ctr = (Control)sender;
                Control ctrParent = ctr.Parent;

                string nodeName = string.Format("L{0}", Convert.ToInt16(ctrParent.Name.Substring(1, 2)));
                if ("txtNextSubBlockEQPList".Equals(currFocus.Name))
                {
                    List<string> lstContent = null;
                    if ("".Equals(currFocus.Text.Trim()))
                    {
                        currFocus.Text = nodeName;
                    }
                    else
                    {
                        lstContent = currFocus.Text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (!lstContent.Contains(nodeName))
                        {
                            lstContent.Add(nodeName);
                            currFocus.Text = string.Join(";", lstContent.ToArray());
                        }
                    }
                }
                else
                {
                    currFocus.Text = nodeName;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Unit_Click(object sender, EventArgs e)
        {
            try
            {
                Control currFocus = this.GetFocusedControl();
                if (!lstTextbox.Contains(currFocus.Name))
                    return;

                Control ctr = (Control)sender;
                Control ctrParent = ctr.Parent;
                
                string nodeName = string.Format("L{0}:{1}", 
                    Convert.ToInt16(ctrParent.Name.Substring(1, 2)), 
                    Convert.ToInt16(ctr.Name.Substring(2, 2)));
                if ("txtNextSubBlockEQPList".Equals(currFocus.Name))
                {
                    List<string> lstContent = null;
                    if ("".Equals(currFocus.Text.Trim()))
                    {
                        currFocus.Text = nodeName;
                    }
                    else
                    {
                        lstContent = currFocus.Text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (!lstContent.Contains(nodeName))
                        {
                            lstContent.Add(nodeName);
                            currFocus.Text = string.Join(";", lstContent.ToArray());
                        }
                    }
                }
                else
                {
                    currFocus.Text = nodeName;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            CheckChangeSave();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                FormSubBlockEdit frmAdd = new FormSubBlockEdit(null);

                frmAdd.StartPosition = FormStartPosition.CenterScreen;
                frmAdd.ShowDialog();
                frmAdd.Dispose();

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvData.SelectedRows.Count != 1)
                    return;

                SBRM_SUBBLOCK_CTRL objData = dgvData.SelectedRows[0].DataBoundItem as SBRM_SUBBLOCK_CTRL;
                long objectKey = objData.OBJECTKEY;
                string strServerName = objData.SERVERNAME;
                string strSubBlockID = objData.SUBBLOCKID;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                SBRM_SUBBLOCK_CTRL objAlarm = null;

                var objSelModify = from msg in ctxBRM.SBRM_SUBBLOCK_CTRL
                                   where msg.OBJECTKEY == objectKey
                                   select msg;
                if (objSelModify.Count() > 0)
                {
                    objAlarm = objSelModify.ToList()[0];
                }

                var objAddModify = from msg in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SUBBLOCK_CTRL>()
                                   where msg.SERVERNAME == strServerName && msg.SUBBLOCKID == strSubBlockID
                                   select msg;
                if (objAddModify.Count() > 0)
                {
                    objAlarm = objAddModify.ToList()[0];
                }

                FormSubBlockEdit frmModify = new FormSubBlockEdit(objAlarm);
                frmModify.StartPosition = FormStartPosition.CenterScreen;
                frmModify.ShowDialog();
                frmModify.Dispose();

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
         }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvData.SelectedRows.Count == 0)
                    return;

                DialogResult result = this.QuectionMessage(this, this.lblCaption.Text,"Are you sure to delete selected records?");
                if (result == System.Windows.Forms.DialogResult.No)
                    return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                SBRM_SUBBLOCK_CTRL objData = dgvData.SelectedRows[0].DataBoundItem as SBRM_SUBBLOCK_CTRL;
                long objectKey = objData.OBJECTKEY;
                string strLineID = objData.LINEID;
                string strSubBlockID = objData.SUBBLOCKID;

                var objSelDelete = from msg in ctxBRM.SBRM_SUBBLOCK_CTRL
                                   where msg.OBJECTKEY == objectKey
                                   select msg;
                if (objSelDelete.Count() > 0)
                {
                    foreach (SBRM_SUBBLOCK_CTRL obj in objSelDelete)
                        ctxBRM.SBRM_SUBBLOCK_CTRL.DeleteOnSubmit(obj);
                }

                var objAddDelete = from msg in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SUBBLOCK_CTRL>()
                                   where msg.LINEID == strLineID && msg.SUBBLOCKID == strSubBlockID
                                   select msg;
                if (objAddDelete.Count() > 0)
                {
                    foreach (SBRM_SUBBLOCK_CTRL obj in objAddDelete)
                        ctxBRM.SBRM_SUBBLOCK_CTRL.DeleteOnSubmit(obj);
                }

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            } 
        }

        //20150318 cy:增加Enable/Disable All的功能
        private void btnEnableAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvData.RowCount <= 0) return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var objSelEnable = from msg in ctxBRM.SBRM_SUBBLOCK_CTRL
                                   where msg.LINEID == FormMainMDI.G_OPIAp.CurLine.LineID && msg.ENABLED == "N"
                                   select msg;
                if (objSelEnable.Count() > 0)
                {
                    foreach (SBRM_SUBBLOCK_CTRL obj in objSelEnable)
                        obj.ENABLED = "Y";
                }

                var objAddEnable = from msg in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SUBBLOCK_CTRL>()
                                   where msg.LINEID == FormMainMDI.G_OPIAp.CurLine.LineID && msg.ENABLED == "N"
                                   select msg;
                if (objAddEnable.Count() > 0)
                {
                    foreach (SBRM_SUBBLOCK_CTRL obj in objAddEnable)
                        obj.ENABLED = "Y";
                }

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            } 
        }

        private void btnDisableAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvData.RowCount <= 0) return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var objSelDisable = from msg in ctxBRM.SBRM_SUBBLOCK_CTRL
                                   where msg.LINEID == FormMainMDI.G_OPIAp.CurLine.LineID && msg.ENABLED == "Y"
                                   select msg;
                if (objSelDisable.Count() > 0)
                {
                    foreach (SBRM_SUBBLOCK_CTRL obj in objSelDisable)
                        obj.ENABLED = "N";
                }

                var objAddDisable = from msg in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SUBBLOCK_CTRL>()
                                   where msg.LINEID == FormMainMDI.G_OPIAp.CurLine.LineID && msg.ENABLED == "Y"
                                   select msg;
                if (objAddDisable.Count() > 0)
                {
                    foreach (SBRM_SUBBLOCK_CTRL obj in objAddDisable)
                        obj.ENABLED = "N";
                }

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            } 
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }
        
        private void dgvData_DataSourceChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvData.DataSource == null || dgvData.Rows.Count == 0) return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                ctxBRM.GetChangeSet();
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    SBRM_SUBBLOCK_CTRL obj = dr.DataBoundItem as SBRM_SUBBLOCK_CTRL;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SUBBLOCK_CTRL>().Any(
                        msg => msg.SERVERNAME == obj.SERVERNAME && msg.SUBBLOCKID == obj.SUBBLOCKID))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_SUBBLOCK_CTRL>().Any(
                        msg => msg.SERVERNAME == obj.SERVERNAME && msg.SUBBLOCKID == obj.SUBBLOCKID))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_SUBBLOCK_CTRL>().Any(
                        msg => msg.SERVERNAME == obj.SERVERNAME && msg.SUBBLOCKID == obj.SUBBLOCKID))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 195);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtEQP_Enter(object sender, EventArgs e)
        {
            try
            {
                TextBox txt = sender as TextBox;

                FormSubBlockEQSelect frmSel = new FormSubBlockEQSelect();
                frmSel.SelectedValue = txt.Text.Trim();
                frmSel.StartPosition = FormStartPosition.CenterScreen;
                if ("txtNextSubBlockEQPList".Equals(txt.Name))
                {
                    frmSel.MultipleSelect = true;
                }

                var result = frmSel.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string val = frmSel.ReturnValue;
                    txt.Text = val;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int idx = e.RowIndex;
            if (idx >= 0 && dgvData.Rows[idx].DataBoundItem != null)
            {
                this.FillTlpEditData(dgvData.Rows[idx].DataBoundItem as SBRM_SUBBLOCK_CTRL);
            }
        }

        private void GetGridViewData()
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                var selData = from msg in ctxBRM.SBRM_SUBBLOCK_CTRL
                              where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                              select msg;

                //已修改未更新物件
                var addData = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SUBBLOCK_CTRL>().Where(
                    msg => msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName);

                List<SBRM_SUBBLOCK_CTRL> objTables = selData.ToList();
                objTables.AddRange(addData.ToList());

                dgvData.AutoGenerateColumns = false;
                dgvData.DataSource = objTables;

                if (dgvData.Rows.Count > 0)
                {
                    DataGridViewCellEventArgs e = new DataGridViewCellEventArgs(0, 0);
                    dgvList_CellClick(dgvData, e);
                }

                Refresh_BlockColor();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Load_LayoutDesign()
        {
            try
            {
                pnlLayout.AutoScroll = true;

                Tip = new ToolTip();

                //Load Layout
                string strLayoutPath = OPIConst.LayoutFolder +string.Format(FormMainMDI.G_OPIAp.CurLine.FabType+"\\")+ string.Format(FormMainMDI.G_OPIAp.CurLine.ServerName + ".xml");
                if (File.Exists(strLayoutPath))
                {
                    LoadXMLFile LayoutDesign = new LoadXMLFile(strLayoutPath);
                    LayoutDesign.Create(pnlLayout);
                }

                #region 去除非Unit & port物件
                RemoveLayoutObject();
                #endregion
                
                #region Init Node
                int _num = 0;
                string _localNo = string.Empty;
                Node _node = null;

                foreach (ucEQ eq in pnlLayout.Controls.OfType<ucEQ>())
                {
                    #region 取得local no
                    int.TryParse(eq.Name.Substring(1, 2), out _num);

                    _localNo = string.Format("L{0}", _num.ToString()); 

                    eq.BackColor = Color.DarkGray;

                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_localNo))
                    {
                        _node = FormMainMDI.G_OPIAp.Dic_Node[_localNo];

                        // 學名+俗名
                        Tip.SetToolTip(eq, _node.NodeNo + "-" + _node.NodeName + "-" + _node.NodeID);
                    }
                    #endregion

                    #region csLabel
                    foreach (csLabel _lbl in eq.Controls.OfType<csLabel>())
                    {
                        switch (_lbl.Name.ToUpper())
                        {
                            case "NODENAME":
                                _lbl.Text = _localNo;

                                _lbl.BackColor = Color.Black;
                                _lbl.ForeColor = Color.Yellow;
                                break;

                            default:
                                break;
                        }
                    }
                    #endregion

                    #region csPictureBox
                    foreach (csPictureBox _pic in eq.Controls.OfType<csPictureBox>())
                    {
                        _pic.BackColor = Color.DarkGray; 

                        string _type = _pic.Name.Substring(0, 2).ToUpper();

                        int _seqNo = 0;

                        int.TryParse(_pic.Name.Substring(2, 2), out _seqNo);

                        switch (_type)
                        {
                            case "CN": // Normal Cassette 
                                _pic.BackgroundImage = Properties.Resources.CSTNormal_LC;
                                _pic.BorderStyle = BorderStyle.None;

                                break;

                            case "UN":  //for unit內的物件背景顯示用 EX:UN02:RD01 Robot顏色會與unit 02相同 -- TBPHL

                                if (_pic.Name.Length == 9)
                                {
                                    string[] _data = _pic.Name.Split(':');

                                    switch (_data[1].Substring(0, 2))
                                    {
                                        #region Robot
                                        case "RD": //Robot- Double Arm
                                            _pic.BackgroundImage = Properties.Resources.Layout_DoubleArm;
                                            _pic.BorderStyle = BorderStyle.None;
                                            break;

                                        case "RS": //Robot- single Arm
                                            _pic.BackgroundImage = Properties.Resources.Layout_SingleArm;
                                            _pic.BorderStyle = BorderStyle.None;
                                            break;
                                        #endregion

                                        #region  Stage

                                        case "TF": //Stage Turn Fix
                                            _pic.BackgroundImage = Properties.Resources.Layout_StageFixed;
                                            _pic.BorderStyle = BorderStyle.None;
                                            break;

                                        case "TT": //Stage Turn Table
                                            _pic.BackgroundImage = Properties.Resources.Layout_StageTurn_ON;
                                            _pic.BorderStyle = BorderStyle.None;
                                            break;

                                        case "TO": //Stage Turn Over
                                            _pic.BackgroundImage = Properties.Resources.Layout_StageTurnOver_ON;
                                            _pic.BorderStyle = BorderStyle.None;
                                            break;

                                        #endregion

                                        #region  Conveyor
                                        case "CV": //Conveyor  橫向
                                            _pic.BackgroundImage = Properties.Resources.Layout_Conveyor1;
                                            _pic.BorderStyle = BorderStyle.None;

                                            break;

                                        case "CL": //Conveyor  縱向
                                            _pic.BackgroundImage = Properties.Resources.Layout_Conveyor2;
                                            _pic.BorderStyle = BorderStyle.None;

                                            break;
                                        #endregion
                                    }
                                }
                                break;

                            default:

                                break;
                        }
                    }
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

        private void SetTlpEditCtl(bool enable, bool clear = false)
        {
            try
            {
                if (clear)
                {
                    txtSubBlockID.Clear();
                    txtStartEQP.Clear();
                    txtControlEQP.Clear();
                    cmbStartEventMsg.SelectedIndex = -1;
                    txtInterLockNo.Clear();
                    txtNextSubBlockEQP.Clear();
                    txtNextSubBlockEQPList.Clear();
                    chkEnabled.Checked = true;
                    txtInterLockReplyNo.Clear();
                    txtRemark.Clear();
                }

                tlpEdit.Enabled = enable;
                if (enable)
                    txtSubBlockID.Focus();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void FillTlpEditData(SBRM_SUBBLOCK_CTRL ctlSubBlock)
        {
            try
            {
                txtSubBlockID.Text = ctlSubBlock.SUBBLOCKID;
                txtStartEQP.Text = ctlSubBlock.STARTEQP;
                txtControlEQP.Text = ctlSubBlock.CONTROLEQP;
                cmbStartEventMsg.SelectedValue = ctlSubBlock.STARTEVENTMSG;
                txtInterLockNo.Text = ctlSubBlock.INTERLOCKNO;
                txtNextSubBlockEQP.Text = ctlSubBlock.NEXTSUBBLOCKEQP;
                txtNextSubBlockEQPList.Text = ctlSubBlock.NEXTSUBBLOCKEQPLIST;
                chkEnabled.Checked = ("Y".Equals(ctlSubBlock.ENABLED)) ? true : false;
                txtInterLockReplyNo.Text = ctlSubBlock.INTERLOCKREPLYNO;
                txtRemark.Text = ctlSubBlock.REMARK;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    
        private Control GetFocusedControl()
        {
            Control focusedControl = null;
            // To get hold of the focused control:
            IntPtr focusedHandle = GetFocus();
            if (focusedHandle != IntPtr.Zero)
                // Note that if the focused Control is not a .Net control, then this will return null.
                focusedControl = Control.FromHandle(focusedHandle);
            return focusedControl;
        }

        private void InitialCombox()
        {
            SetEventMsg();
        }

        private void SetEventMsg()
        {
            try
            {
                var data = new[] 
                { 
                    new { Key = "RECEIVE", Value = "Receive" } ,
                    new { Key = "SEND", Value = "Send" } ,
                    new { Key = "STORE", Value = "Store" } ,
                    new { Key = "FETCH", Value = "Fetch" } ,
                    new { Key = "PROCCOMP", Value = "Process Compplete" }
                };

                var s = data.ToList();
                this.cmbStartEventMsg.DataSource = s;
                this.cmbStartEventMsg.DisplayMember = "Value";
                this.cmbStartEventMsg.ValueMember = "Key";
                this.cmbStartEventMsg.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvData_CellErrorTextChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        //隱藏BlockControl用不到Layout物件
        private void RemoveLayoutObject()
        {
            ArrayList aryDisable = new ArrayList();
            try
            {
                #region 去除水管
                ArrayList aryPipe = new ArrayList();
                foreach (ucPipe pipe in pnlLayout.Controls.OfType<ucPipe>())
                {
                    aryPipe.Add(pipe);
                }
                for (int i = 0; i < aryPipe.Count; i++)
                {
                    Control ctr = (Control)aryPipe[i];
                    pnlLayout.Controls.Remove(ctr);
                }
                #endregion

                #region 去除非unit物件
                foreach (ucEQ eq in pnlLayout.Controls.OfType<ucEQ>())
                {
                    aryDisable.Clear();
                    foreach (Control ctr in eq.Controls)
                    {
                        //畫面保留NodeName
                        if (ctr.Name.ToUpper() == "NODENAME")
                        {
                            continue;
                        }

                        //保留UN開頭但unitNo<90
                        if (ctr.Name.Length >= 4 && ctr.Name.Substring(0, 2) == "UN" && int.Parse(ctr.Name.Substring(2, 2)) < 90)
                            continue;

                        //保留Port
                        if (ctr.Name.Length >= 2 && (ctr.Name.Substring(0, 2) == "CN" 
                            || ctr.Name.Substring(0, 2) == "CS" || ctr.Name.Substring(0, 2) == "CC"
                            || ctr.Name.Substring(0, 2) == "CW" || ctr.Name.Substring(0, 2) == "DS"
                            || ctr.Name.Substring(0, 2) == "BF" || ctr.Name.Substring(0, 2) == "BW" 
                            ))
                        {
                            continue;
                        }                        

                        aryDisable.Add(ctr);
                    }

                    for (int i = 0; i < aryDisable.Count; i++)
                    {
                        Control ctr = (Control)aryDisable[i];
                        eq.Controls.Remove(ctr);
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

        // 更新BlockColor
        private void Refresh_BlockColor()
        {            
            string strENABLED = "";
            string[] strEQList;
            string strKey = "";
            int unitNo = 0;

            try
            {
                int SubIndex = 0;//計算Block Color顏色陣列的Index

                #region 清除backcolor
                foreach (ucEQ eq in pnlLayout.Controls.OfType<ucEQ>())
                {
                    eq.BackColor = Color.DarkGray;

                    foreach (Control ctr in eq.Controls)
                    {
                        if (ctr.Name.ToUpper() == "NODENAME") continue;

                        ctr.BackColor = Color.DarkGray;
                    }
                }
                #endregion

                #region 依照設定給color
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    strENABLED = dr.Cells["dgvchkENABLED"].Value.ToString();
                    strEQList = dr.Cells["dgvtxtNEXTSUBBLOCKEQPLIST"].Value.ToString().Split(';');
                    
                    if (strENABLED.ToUpper() != "Y") continue;
                    
                    foreach (string strEQNo in strEQList)
                    {
                        if (strEQNo.Contains(":"))
                        {
                            string strTmp = strEQNo.Split(':')[0];// ':' 前為NodeNo
                            string strNodeNo = strTmp.Substring(1, strTmp.Length - 1);
                            ucEQ uceq = (ucEQ)this.pnlLayout.Controls["N" + int.Parse(strNodeNo).ToString("00") + "00"];
                            if (uceq != null)
                            {
                                if (strEQNo.Split(':')[1].Substring(0, 1) != "P")
                                {

                                    int.TryParse(strEQNo.Split(':')[1], out unitNo);

                                    strKey = "UN" + unitNo.ToString("00");//':'後為UnitNo

                                    //strKey = "UN" + int.Parse(strEQNo.Split(':')[1]).ToString("00");//':'後為UnitNo

                                    foreach (Control ctr in uceq.Controls)
                                    {
                                        if (ctr.Name.Length >= 4 && ctr.Name.Substring(0, 4) == strKey)
                                        {
                                            if (SubBlock_Color.Length > SubIndex) //判別是否超出顏色數量
                                                ctr.BackColor = SubBlock_Color[SubIndex];
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //去除Name的頭(L),再加成 N+02(NodeNo) +00
                            string strNodeNo = strEQNo.Substring(1, strEQNo.Length - 1);
                            ucEQ uceq = (ucEQ)this.pnlLayout.Controls["N" + int.Parse(strNodeNo).ToString("00") + "00"];
                            if (uceq != null)
                            {
                                if (SubBlock_Color.Length > SubIndex)　//判別是否超出顏色數量
                                    uceq.BackColor = SubBlock_Color[SubIndex];                                
                            }
                        }
                    }
                    SubIndex++;
                }
                #endregion

                #region 判斷node下的unit若沒有被設定block，則顏色改為跟node一致
                foreach (ucEQ eq in pnlLayout.Controls.OfType<ucEQ>())
                {
                    foreach (Control ctr in eq.Controls)
                    {
                        if (ctr.Name.ToUpper() == "NODENAME") continue;

                        if (ctr.BackColor == Color.DarkGray)
                            ctr.BackColor = eq.BackColor;
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

        private void Save()
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                string _sqlDesc = string.Empty;

                string _sqlErr = string.Empty;

                try
                {
                    #region 取得更新的sub Block

                    //新增
                    foreach (object objToInsert in _ctxBRM.GetChangeSet().Inserts)
                    {
                        if (objToInsert.GetType().Name != "SBRM_SUBBLOCK_CTRL") continue;

                        SBRM_SUBBLOCK_CTRL _subBlock = (SBRM_SUBBLOCK_CTRL)objToInsert;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [ {0} , {1} , {2} ] ", _subBlock.SUBBLOCKID, _subBlock.STARTEQP, _subBlock.NEXTSUBBLOCKEQPLIST);
                    }

                    //delete
                    foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                    {
                        if (objToDelete.GetType().Name != "SBRM_SUBBLOCK_CTRL") continue;

                        SBRM_SUBBLOCK_CTRL _subBlock = (SBRM_SUBBLOCK_CTRL)objToDelete;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [ {0} , {1} , {2} ] ", _subBlock.SUBBLOCKID, _subBlock.STARTEQP, _subBlock.NEXTSUBBLOCKEQPLIST);
                    }

                    //modify
                    foreach (object objToUpdate in _ctxBRM.GetChangeSet().Updates)
                    {
                        if (objToUpdate.GetType().Name != "SBRM_SUBBLOCK_CTRL") continue;

                        SBRM_SUBBLOCK_CTRL _subBlock = (SBRM_SUBBLOCK_CTRL)objToUpdate;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [ {0} , {1} , {2} ] ", _subBlock.SUBBLOCKID, _subBlock.STARTEQP, _subBlock.NEXTSUBBLOCKEQPLIST);
                    }

                    #endregion

                    if (_sqlDesc == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, string.Empty, "No Data for Update！", MessageBoxIcon.Warning);
                        return;
                    }

                    _ctxBRM.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                }
                catch (System.Data.Linq.ChangeConflictException err)
                {
                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, err);

                    foreach (System.Data.Linq.ObjectChangeConflict occ in _ctxBRM.ChangeConflicts)
                    {
                        // 將變更的欄位寫入資料庫（合併更新）
                        occ.Resolve(System.Data.Linq.RefreshMode.KeepChanges);
                    }

                    try
                    {
                        _ctxBRM.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        _sqlErr = ex.ToString();

                        foreach (System.Data.Linq.MemberChangeConflict _data in _ctxBRM.ChangeConflicts[0].MemberConflicts)
                        {
                            if (_data.DatabaseValue != _data.OriginalValue)
                            {
                                _sqlErr = _sqlErr + string.Format("\r\n Change Conflicts : Property '{0}': Database value: {1}, Original value {2}, Current Value:{3}", _data.Member.Name, _data.DatabaseValue, _data.OriginalValue, _data.CurrentValue);
                            }
                        }

                        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                    }
                }

                #region 紀錄opi history
                string _err = UniTools.InsertOPIHistory_DB("SBRM_SUBBLOCK_CTRL", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                Public.SendDatabaseReloadRequest("SBRM_SUBBLOCK_CTRL");

                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Sub Block Save Success！", MessageBoxIcon.Information);
                }

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }    
        }

        public override void CheckChangeSave()
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                if (_ctxBRM.GetChangeSet().Inserts.Count() > 0 ||
                     _ctxBRM.GetChangeSet().Deletes.Count() > 0 ||
                     _ctxBRM.GetChangeSet().Updates.Count() > 0)
                {
                    string _msg = string.Format("Please confirm whether you will save data before change layout ?");
                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg))
                    {
                        RefreshData();
                        return;
                    }
                    else
                    {
                        Save();
                    }
                }
                else
                {
                    RefreshData();
                }
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
                #region Refresh

                FormMainMDI.G_OPIAp.RefreshDBBRMCtx();

                GetGridViewData();

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
