using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormChangerPlanEdit : FormBase
    {
        private string glassIDSerialNo;

        public string PlanID { get; set; }

        public FormChangerPlanEdit()
        {
            InitializeComponent();
            FormMode = UniOPI.FormMode.AddNew;
            gbEditMode.Text = "Add";
            glassIDSerialNo = string.Empty;

            rdoAuto.Checked = true;
        }

        public FormChangerPlanEdit(string planID)
        {
            InitializeComponent();
            FormMode = UniOPI.FormMode.Modify;
            gbEditMode.Text = "Modify";
            txtPlanID.Text = planID;
            txtPlanID.Enabled = false;
            glassIDSerialNo = string.Empty;
            GetChangePlan(planID);
        }

        private void FormChangerPlanEdit_Load(object sender, EventArgs e)
        {
            try
            {
                #region Glass ID 長度限制 
                txtGlassID.MaxLength = FormMainMDI.G_OPIAp.GlassIDMaxLength;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private DataTable InitChangerPlan()
        {
            return UniTools.InitDt(new string[] { "OBJECTKEY", "SERVERNAME", "LINEID", "PLANID", "SOURCECASSETTEID", "TARGETASSETTEID", "SLOTNO", "JOBID", "TARGETSLOTNO", "TARGETSLOTSELECT" });
        }

        private void GetChangePlan(string planID)
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                List<SBRM_CHANGEPLAN> lstDBChgPlan = (from d in _ctxBRM.SBRM_CHANGEPLAN
                                                      where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.PLANID.Equals(planID)
                                                      select d).ToList();

                //已修改未更新物件
                List<SBRM_CHANGEPLAN> lstINSChgPlan = (from d in _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_CHANGEPLAN>()
                                                       where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.PLANID.Equals(planID)
                                                       select d).ToList();
                //已刪除未更新的物件
                List<SBRM_CHANGEPLAN> lstDelChgPlan = (from d in _ctxBRM.GetChangeSet().Deletes.OfType<SBRM_CHANGEPLAN>()
                                                       where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.PLANID.Equals(planID)
                                                       select d).ToList();

                lstDBChgPlan.AddRange(lstINSChgPlan);
                DataTable dtChagerPlan = InitChangerPlan();
                //lstDBChgPlan.Sort((d1, d2) => d1.SLOTNO.CompareTo(d2.SLOTNO));//lstDBChgPlan不排序, 改用dtChagerPlan.DefaultView.Sort

                foreach (SBRM_CHANGEPLAN chgPlan in lstDBChgPlan)
                {
                    DataRow drNew = dtChagerPlan.NewRow();
                    if (lstDelChgPlan.Contains(chgPlan)) continue;
                    drNew["OBJECTKEY"] = chgPlan.OBJECTKEY;
                    drNew["LINEID"] = chgPlan.LINEID;
                    drNew["SERVERNAME"] = chgPlan.SERVERNAME;
                    drNew["PLANID"] = chgPlan.PLANID;
                    drNew["SOURCECASSETTEID"] = chgPlan.SOURCECASSETTEID;
                    drNew["TARGETASSETTEID"] = chgPlan.TARGETASSETTEID;
                    drNew["SLOTNO"] = chgPlan.SLOTNO;
                    drNew["JOBID"] = chgPlan.JOBID;
                    drNew["TARGETSLOTNO"] = chgPlan.TARGETSLOTNO;
                    drNew["TARGETSLOTSELECT"] = chgPlan.TARGETSLOTNO == string.Empty ? "Auto" : "Manual";
                    dtChagerPlan.Rows.Add(drNew);
                }
                dtChagerPlan.DefaultView.Sort = "SOURCECASSETTEID, SLOTNO";
                dgvChgPlan.DataSource = dtChagerPlan;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //Check 資料是否一樣，若有修改才需要更新
        private bool chkSame(DataRow drCur, SBRM_CHANGEPLAN chkChgPlan)
        {
            try
            {
                if (!drCur["SLOTNO"].Equals(chkChgPlan.SLOTNO)) return false;
                if (!drCur["SOURCECASSETTEID"].Equals(chkChgPlan.SOURCECASSETTEID)) return false;
                if (!drCur["TARGETASSETTEID"].Equals(chkChgPlan.TARGETASSETTEID)) return false;
                if (!drCur["JOBID"].Equals(chkChgPlan.JOBID)) return false;
                if (!drCur["TARGETSLOTNO"].Equals(chkChgPlan.TARGETSLOTNO)) return false;
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return false;
            }
        }

        private void Clear()
        {
            txtPlanID.Enabled = true;
            txtPlanID.Clear();
            DataTable dt = (DataTable)dgvChgPlan.DataSource;
            dt.Rows.Clear();
        }

        //Check Plan id 是否已存在
        private bool chkAllowAddPlan(string plan)
        {
            UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
            List<SBRM_CHANGEPLAN> lstDBChgPlan = (from d in ctxBRM.SBRM_CHANGEPLAN
                                                  where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.PLANID.Equals(plan)
                                                  select d).ToList();
            if (lstDBChgPlan.Count > 0) return false;
            List<SBRM_CHANGEPLAN> lstAddChgPlan = (from d in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_CHANGEPLAN>()
                                                   where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.PLANID.Equals(plan)
                                                   select d).ToList();
            if (lstAddChgPlan.Count > 0) return false;
            return true;
        }

        //Check Cassette id 是否已存在
        private string chkSourceCSTID(string CstID)
        {
            UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
            List<SBRM_CHANGEPLAN> _lstDB = (from d in ctxBRM.SBRM_CHANGEPLAN
                                                  where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.SOURCECASSETTEID.Equals(CstID)
                                                  select d).ToList();
            if (_lstDB.Count > 0) return _lstDB.First().PLANID;

            List<SBRM_CHANGEPLAN> _lsInsert = (from d in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_CHANGEPLAN>()
                                                   where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.SOURCECASSETTEID.Equals(CstID)
                                                   select d).ToList();

            if (_lsInsert.Count > 0) return _lsInsert.First().PLANID;

            return string.Empty ;
        }

        private bool CheckUniqueKeyOK(DataTable dtAllChgPlan, ref List<SBRM_CHANGEPLAN> lstDBChgPlan, ref List<SBRM_CHANGEPLAN> lstAddChgPlan)
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                List<SBRM_CHANGEPLAN> lstAllChkChgPlan = new List<SBRM_CHANGEPLAN>();
                lstDBChgPlan = (from d in ctxBRM.SBRM_CHANGEPLAN
                                where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.PLANID.Equals(txtPlanID.Text.Trim().ToString())
                                select d).ToList();
                lstAllChkChgPlan.AddRange(lstDBChgPlan);
                //已修改未更新物件
                lstAddChgPlan = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_CHANGEPLAN>().Where(
                        d => d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.PLANID.Equals(txtPlanID.Text.Trim().ToString())).ToList();
                lstAllChkChgPlan.AddRange(lstAddChgPlan);

                if (lstDBChgPlan.Count == 0) return true;

                foreach (DataRow drChgPlan in dtAllChgPlan.Rows)
                {
                    //UK :SERVERNAME,PLANID,SOURCECASSETTEID,SLOTNO
                    SBRM_CHANGEPLAN chkdbChager = lstAllChkChgPlan.Find(d => d.SERVERNAME.Equals(drChgPlan["SERVERNAME"].ToString()) && d.PLANID.Equals(drChgPlan["PLANID"].ToString()) &&
                        d.SOURCECASSETTEID.Equals(drChgPlan["SOURCECASSETTEID"].ToString()) && d.SLOTNO.Equals(drChgPlan["SLOTNO"].ToString()));
                    if (chkdbChager != null)
                    {
                        long objKey = 0;
                        long.TryParse(drChgPlan["OBJECTKEY"].ToString(), out objKey);
                        if (!chkdbChager.OBJECTKEY.Equals(objKey))
                        {
                            string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1}, {2}, {3})！", drChgPlan["SERVERNAME"].ToString(), drChgPlan["PLANID"].ToString(), drChgPlan["SOURCECASSETTEID"].ToString(), drChgPlan["SLOTNO"].ToString());
                            ShowMessage(this, lblCaption.Text , "", errMsg, MessageBoxIcon.Warning);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool CheckData()
        {
            try
            {
                bool _isAllTargetIDEmpty = true;
                int _num = 0;
                DataTable dtAllChgPlan = (DataTable)dgvChgPlan.DataSource;
                List<string> _useSourceCstID = new List<string>();
                List<string> lstSlotNo = new List<string>(); // 做為暫存 Target SlotNo判斷是否重覆
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                #region 判斷Source Cassette ID & Target Cassette ID
                foreach (DataRow drChgPlan in dtAllChgPlan.Rows)
                {
                    #region 檢查 SourceCassetteID 是否為空
                    if (drChgPlan["SOURCECASSETTEID"].ToString() == string.Empty) 
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Slot [{0}] Source Cassette ID is empty!", drChgPlan["SLOTNO"].ToString()), MessageBoxIcon.Error);
                        return false ;
                    }
                    #endregion

                    #region Traget CST ID可以為空的,但不可全為空
                    if (drChgPlan["TARGETASSETTEID"].ToString() != string.Empty)
                    {
                        _isAllTargetIDEmpty = false;
                    }
                    #endregion

                    #region 檢查Jobid是否為空
                    if (drChgPlan["JOBID"].ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Slot [{0}] Glass ID is empty!", drChgPlan["SLOTNO"].ToString()), MessageBoxIcon.Error);
                        return false ;
                    }
                    #endregion

                    #region 檢查CassetteID的來源和目標是否一樣
                    if (drChgPlan["SOURCECASSETTEID"].ToString() == drChgPlan["TARGETASSETTEID"].ToString()) 
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Slot [{0}] Source Cassette ID and Target Cassette ID must be different!", drChgPlan["SLOTNO"].ToString()), MessageBoxIcon.Error);
                        return false ;
                    }
                    #endregion

                    #region  Target Slot Select 為 Manual Mode 時判斷
                    if (drChgPlan["TARGETSLOTSELECT"].ToString().Trim() == "Manual")
                    {
                        #region Target Slot Select 為 Manual Mode 時，需設定target slot no
                        if (drChgPlan["TARGETSLOTNO"].ToString().Trim() == "")
                        {
                            ShowMessage(this, lblCaption.Text, "", string.Format(" Traget CST ID[{0}] Target SlotNo is Null",
                                drChgPlan["TARGETASSETTEID"].ToString()), MessageBoxIcon.Error);
                            return false;
                        }
                        #endregion

                        #region  Target Slot No必須為數值
                        if (int.TryParse(drChgPlan["TARGETSLOTNO"].ToString(), out _num) == false)
                        {
                            ShowMessage(this, lblCaption.Text, "", string.Format(" Traget CST ID[{0}] Target SlotNo [{1}] must be number ", drChgPlan["TARGETASSETTEID"].ToString(), drChgPlan["TARGETSLOTNO"].ToString()), MessageBoxIcon.Error);
                            return false;
                        }
                        #endregion

                        drChgPlan["TARGETSLOTNO"] = int.Parse(drChgPlan["TARGETSLOTNO"].ToString()).ToString("000"); //將 Target SlotNo 轉成為三碼數字如(001,002);

                        #region 檢查Target CST ID & TargetSlotNo是否重覆 ,本Plan -- 同一Target CST ID + Target Slot No不能assign 兩次
                        if (lstSlotNo.Contains(drChgPlan["TARGETASSETTEID"].ToString() + "," + drChgPlan["TARGETSLOTNO"].ToString()))//判斷Target SlotNo是否重覆
                        {
                            ShowMessage(this, lblCaption.Text, "", string.Format(" Traget CST ID[{0}] Target SlotNo [{1}] is Exists", drChgPlan["TARGETASSETTEID"].ToString(), drChgPlan["TARGETSLOTNO"].ToString()), MessageBoxIcon.Error);
                            return false;
                        }
                        else
                        {
                            lstSlotNo.Add(drChgPlan["TARGETASSETTEID"].ToString() + "," + drChgPlan["TARGETSLOTNO"].ToString());//未重覆,加入
                        }
                        #endregion

                        #region 檢查Target CST ID & TargetSlotNo是否重覆 ,比對其它Plan
                        var _useVar = from d in _ctxBRM.SBRM_CHANGEPLAN
                                      where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) &&
                                      d.TARGETASSETTEID.Equals(drChgPlan["TARGETASSETTEID"].ToString()) &&
                                      (drChgPlan["TARGETSLOTNO"].ToString() == d.TARGETSLOTNO) &&
                                      d.PLANID != txtPlanID.Text.ToString()
                                      select d;

                        if (_useVar.Count() > 0)
                        {
                            SBRM_CHANGEPLAN _use = _useVar.First();
                            ShowMessage(this, lblCaption.Text, "",
                                string.Format(" Target CST ID[{1}] Target SlotNo [{2}] is be used in Plan [{0}]!",
                                _use.PLANID, drChgPlan["TARGETASSETTEID"].ToString(), drChgPlan["TARGETSLOTNO"].ToString()), MessageBoxIcon.Error);

                            return false;
                        }

                        //比對每一筆的TargetCSTID,TargetSlotNo是否存在 未儲存的資料中
                        List<SBRM_CHANGEPLAN> _lsInsert = (from d in _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_CHANGEPLAN>()
                                                           where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) &&
                                                           d.TARGETASSETTEID.Equals(drChgPlan["TARGETASSETTEID"].ToString()) &&
                                                           (drChgPlan["TARGETSLOTNO"].ToString() == d.TARGETSLOTNO) &&
                                                           d.PLANID != txtPlanID.Text.ToString()
                                                           select d).ToList();

                        if (_lsInsert.Count() > 0)
                        {
                            ShowMessage(this, lblCaption.Text, "", string.Format(" Target CST ID[{0}] Target SlotNo [{1}] is be used in Plan]!",
                                drChgPlan["TARGETASSETTEID"].ToString(), drChgPlan["TARGETSLOTNO"].ToString()), MessageBoxIcon.Error);
                            return false;
                        }
                        #endregion
                    }
                    
                    #endregion

                    //將來源的CassetID記錄起來
                    if (!_useSourceCstID.Contains(drChgPlan["SOURCECASSETTEID"].ToString()))  _useSourceCstID.Add(drChgPlan["SOURCECASSETTEID"].ToString()); 
                }

                if (_isAllTargetIDEmpty)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("Target Cassette ID can't be empty for all slot"), MessageBoxIcon.Error);
                    return false ;
                }
                #endregion


                #region 判斷source cassette id是否已經被使用過
                foreach (string _sourceCstId in _useSourceCstID)
                {
                    //UK :SERVERNAME,PLANID,SOURCECASSETTEID,SLOTNO
                    var _useVar = from d in _ctxBRM.SBRM_CHANGEPLAN
                                  where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.SOURCECASSETTEID.Equals(_sourceCstId) && d.PLANID != txtPlanID.Text.ToString()
                                  select d;

                    if (_useVar.Count() > 0)
                    {
                        SBRM_CHANGEPLAN _use = _useVar.First();

                        ShowMessage(this, lblCaption.Text, "", string.Format("Source Cassette ID is be used in Plan [{0}]!", _use.PLANID), MessageBoxIcon.Error);
                        return false ;
                    }
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return false;
            }
        }       

        #region Key Press
        private void txtSlotNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            UniTools.CheckTextBoxKeyPressIsInteger(sender, e);
        }

        private void txtGlassID_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                (asciiCode >= 65 & asciiCode <= 90) |   // A-Z except I、O
                (asciiCode >= 97 & asciiCode <= 122) |   // a-z except i、o
                asciiCode == 8 |    // Backspace
                asciiCode == 45 |   // -
                asciiCode == 95)    // _
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        //大寫 , 底線 數字跟字母 , 八碼
        private void txtPlanID_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                (asciiCode >= 65 & asciiCode <= 90) |   // A-Z except I、O
                (asciiCode >= 97 & asciiCode <= 122) |   // a-z except i、o
                asciiCode == 8 |    // Backspace
                asciiCode == 95)    // _
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }
        #endregion

        #region DataGridView

        private void dgDepotInMx_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {

            if (e.Control is DataGridViewTextBoxEditingControl)//检测是被表示的控件还是DataGridViewTextBoxEditingControl 
            {
                DataGridView dgv = (DataGridView)sender;
                DataGridViewTextBoxEditingControl tb = (DataGridViewTextBoxEditingControl)e.Control;//取得被表示的控件 
                tb.KeyPress -= new KeyPressEventHandler(dataGridViewTextBox_KeyPress);//事件处理器删除                 
                if (dgv.CurrentCell.OwningColumn.Name == colTargetSlotNo.Name)//检测对应列 
                {                    
                    tb.KeyPress += new KeyPressEventHandler(dataGridViewTextBox_KeyPress);// KeyPress事件处理器追加 
                }
            }
        }

        private void dataGridViewTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            UniTools.CheckTextBoxKeyPressIsInteger(sender, e);
        }

        private void dgvChgPlan_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

                string _select = string.Empty;
                DataGridView _dgv = (DataGridView)sender;


                if (_dgv.CurrentCell.OwningColumn.Name == colTargetSlotNo.Name)
                {
                    _select = _dgv.Rows[_dgv.CurrentCell.RowIndex].Cells[colTargetSlotSelect.Name].Value.ToString();
                    if (_select == "Auto")
                    {
                        if (_dgv.CurrentCell.Value.ToString() != string.Empty)
                        {
                            ShowMessage(this, lblCaption.Text, "", "Target Slot No must be empty in Auto Select!", MessageBoxIcon.Warning);
                            _dgv.CurrentCell.Value = string.Empty;
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

        #endregion

        #region Button Click

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvChgPlan.Rows.Count == 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "No Plan Data!", MessageBoxIcon.Warning);
                    return;
                }

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                DataTable dtAllChgPlan = (DataTable)dgvChgPlan.DataSource;
                List<SBRM_CHANGEPLAN> lstDBChgPlan = null;
                List<SBRM_CHANGEPLAN> lstAddChgPlan = null;
                //List<string> _useSourceCstID = new List<string>();

                DateTime _dateTime = DateTime.Now;

                //判斷UniqueKey
                if (!CheckUniqueKeyOK(dtAllChgPlan, ref lstDBChgPlan, ref lstAddChgPlan)) return;

                if (!CheckData()) return;


                #region 處理新增、修改的
                foreach (DataRow drChgPlan in dtAllChgPlan.Rows)
                {
                    bool addNew = false;
                    SBRM_CHANGEPLAN chgPlan = null;
                    long objKey = 0;
                    long.TryParse(drChgPlan["OBJECTKEY"].ToString(), out objKey);
                    var chkDBChgPlan = lstDBChgPlan.Find(d => d.OBJECTKEY.Equals(objKey));
                    if (chkDBChgPlan != null)
                    {
                        //比較是否不同
                        if (!chkSame(drChgPlan, chkDBChgPlan)) chgPlan = chkDBChgPlan;

                    }
                    else
                    {
                        //UK :SERVERNAME,PLANID,SOURCECASSETTEID,SLOTNO
                        var chkAddChgPlan = lstAddChgPlan.Find(d => d.SERVERNAME.Equals(drChgPlan["SERVERNAME"].ToString()) && d.PLANID.Equals(drChgPlan["PLANID"].ToString()) && d.SOURCECASSETTEID.Equals(drChgPlan["SOURCECASSETTEID"].ToString()) && d.SLOTNO.Equals(drChgPlan["SLOTNO"].ToString()));
                        if (chkAddChgPlan != null)
                        {
                            if (!chkSame(drChgPlan, chkAddChgPlan)) chgPlan = chkAddChgPlan;
                        }
                        else
                        {
                            chgPlan = new SBRM_CHANGEPLAN();
                            addNew = true;
                        }
                    }
                    if (chgPlan != null)
                    {
                        chgPlan.JOBID = drChgPlan["JOBID"].ToString();
                        chgPlan.LINEID = drChgPlan["LINEID"].ToString();
                        chgPlan.SERVERNAME = drChgPlan["SERVERNAME"].ToString();
                        chgPlan.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                        chgPlan.PLANID = txtPlanID.Text.ToString();
                        chgPlan.SLOTNO = drChgPlan["SLOTNO"].ToString();
                        chgPlan.SOURCECASSETTEID = drChgPlan["SOURCECASSETTEID"].ToString();
                        chgPlan.TARGETASSETTEID = drChgPlan["TARGETASSETTEID"].ToString();
                        chgPlan.TARGETSLOTNO = drChgPlan["TARGETSLOTNO"].ToString(); // int.Parse().ToString("000"); //將 Target SlotNo 轉成為三碼數字如(001,002);
                        chgPlan.UPDATETIME = _dateTime;
                        if (addNew) FormMainMDI.G_OPIAp.DBBRMCtx.SBRM_CHANGEPLAN.InsertOnSubmit(chgPlan);
                    }
                }
                #endregion

                #region 處理刪除的
                foreach (SBRM_CHANGEPLAN chgPlan in lstDBChgPlan)
                {
                    //當畫面找不到時即刪除了
                    DataRow[] drChkExist = dtAllChgPlan.Select(string.Format("OBJECTKEY='{0}'", chgPlan.OBJECTKEY));
                    if (drChkExist.Length > 0) continue;
                    ctxBRM.SBRM_CHANGEPLAN.DeleteOnSubmit(chgPlan);
                }
                foreach (SBRM_CHANGEPLAN chgPlan in lstAddChgPlan)
                {
                    //當畫面找不到時即刪除了
                    //UK :SERVERNAME,PLANID,SOURCECASSETTEID,SLOTNO
                    DataRow[] drChkExist = dtAllChgPlan.Select(string.Format("SERVERNAME='{0}' AND PLANID='{1}' AND SOURCECASSETTEID='{2}' AND SLOTNO='{3}'", chgPlan.SERVERNAME, chgPlan.PLANID, chgPlan.SOURCECASSETTEID, chgPlan.SLOTNO));
                    if (drChkExist.Length > 0) continue;
                    ctxBRM.SBRM_CHANGEPLAN.DeleteOnSubmit(chgPlan);
                }
                #endregion

                this.PlanID = txtPlanID.Text.ToString();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            try
            {
                #region Check 必要欄位是否有填入
                if (string.IsNullOrWhiteSpace(txtPlanID.Text.ToString()))
                {
                    ShowMessage(this, lblCaption.Text, "", "Plan ID is required", MessageBoxIcon.Warning);
                    txtPlanID.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtStartSlotNo.Text.ToString()))
                {
                    ShowMessage(this, lblCaption.Text, "", "Start Slot No is required", MessageBoxIcon.Warning);
                    txtStartSlotNo.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEndSlotNo.Text.ToString()))
                {
                    ShowMessage(this, lblCaption.Text, "", "End Slot No is required", MessageBoxIcon.Warning);
                    txtEndSlotNo.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtSourceCstID.Text.ToString()))
                {
                    ShowMessage(this, lblCaption.Text, "", "Source CST ID is required", MessageBoxIcon.Warning);
                    txtSourceCstID.Focus();
                    return;
                }

                if (txtSourceCstID.Text.ToString().Equals(txtTargetCstID.Text.ToString()))
                {
                    ShowMessage(this, lblCaption.Text, "", "The Source and Target CST ID must be different!!", MessageBoxIcon.Warning);
                    txtSourceCstID.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtGlassID.Text.ToString()))
                {
                    ShowMessage(this, lblCaption.Text, "", "Glass ID is required", MessageBoxIcon.Warning);
                    txtGlassID.Focus();
                    return;
                }

                #endregion

                #region Get 輸入資料
                int intStartSlotNo = 0;
                int intEndSlotNo = 0;

                int.TryParse(txtStartSlotNo.Text.ToString(), out intStartSlotNo);
                int.TryParse(txtEndSlotNo.Text.ToString(), out intEndSlotNo);

                if (intStartSlotNo == 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "The Start Slot No must be greater than 0", MessageBoxIcon.Warning);
                    txtSourceCstID.Focus();
                    return;
                }

                if (intEndSlotNo == 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "The End Slot No must be greater than 0", MessageBoxIcon.Warning);
                    txtEndSlotNo.Focus();
                    return;
                }

                if (intStartSlotNo > intEndSlotNo)
                {
                    ShowMessage(this, lblCaption.Text, "", "Start Slot No must be less than End Slot No", MessageBoxIcon.Warning);
                    txtStartSlotNo.Focus();
                    return;
                }

                string planID = txtPlanID.Text.ToString();
                string _souceCSTID = txtSourceCstID.Text.ToString();
                string _targetCSTID = txtTargetCstID.Text.ToString().Trim();
                string _glassIDHeader = txtGlassID.Text.ToString().Trim();

                #endregion

                #region 新增模式資料判斷
                if (FormMode == FormMode.AddNew)
                {
                    #region 若新增模式則判斷是否已新增過Plan ID，若有則跳出請使用者使用修改模式來新增
                    if (!chkAllowAddPlan(planID))
                    {
                        ShowMessage(this, lblCaption.Text, "", "The Plan id has been added.Please use modify!!", MessageBoxIcon.Warning);
                        txtPlanID.Focus();
                        return;
                    }
                    #endregion

                    #region 若新增模式則判斷是否已新增過Source Cassette ID (不同的plan 不能指定相同的Source CST)
                    string _usePlanID = chkSourceCSTID(txtSourceCstID.Text.ToString());
                    if (_usePlanID != string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Source Cassette ID is be used in Plan [{0}]!", _usePlanID), MessageBoxIcon.Error);
                        txtSourceCstID.Focus();
                        return;
                    }
                    #endregion

                }
                #endregion

                DataTable dtAllChgPlan = (DataTable)dgvChgPlan.DataSource;

                if (dtAllChgPlan == null) dtAllChgPlan = InitChangerPlan();

                #region Check Slot是否已存在 -- source cst + source slot也不能assign兩次
                if (dtAllChgPlan.Rows.Count > 0)
                {
                    for (int slotNo = intStartSlotNo; slotNo <= intEndSlotNo; slotNo++)
                    {
                        string chkSlotNo = slotNo.ToString().PadLeft(3, '0');

                        DataRow[] drChkChgPlan = dtAllChgPlan.Select(string.Format("SLOTNO='{0}' and SOURCECASSETTEID='{1}'", chkSlotNo, _souceCSTID));

                        if (drChkChgPlan.Length > 0)
                        {
                            ShowMessage(this, lblCaption.Text, "", string.Format("Slot No{0} is exists!!", drChkChgPlan[0]["SLOTNO"].ToString()), MessageBoxIcon.Warning);
                            return;
                        }
                    }

                }
                #endregion

                #region 計算目前要新增的Target CST數量--到同一個Target CST 不能超過28
                int _targetCstCount = intEndSlotNo - intStartSlotNo + 1;
                foreach (DataRow _row in dtAllChgPlan.Rows)
                {
                    if (_row["TARGETASSETTEID"].ToString() == _targetCSTID) _targetCstCount = _targetCstCount + 1;
                }

                if (_targetCstCount > 28)
                {
                    ShowMessage(this, lblCaption.Text, "", "Same Target Cassette ID must be less than or equal to 28", MessageBoxIcon.Warning);
                    txtTargetCstID.Focus();
                    return;
                }
                #endregion

                #region 加入DataTable
                for (int slotNo = intStartSlotNo; slotNo <= intEndSlotNo; slotNo++)
                {
                    string glassID = string.Empty;
                    
                    if (FormMainMDI.G_OPIAp.CurLine.FabType=="ARRAY")
                        glassID = _glassIDHeader + UniTools.GetGlassID_Array(slotNo); 
                    else
                        glassID = _glassIDHeader + UniTools.GetGlassID(slotNo); 

                    DataRow drNew = dtAllChgPlan.NewRow();
                    drNew["SLOTNO"] = slotNo.ToString().PadLeft(3, '0');
                    drNew["JOBID"] = glassID;
                    drNew["PLANID"] = txtPlanID.Text.Trim().ToString();
                    drNew["LINEID"] = FormMainMDI.G_OPIAp.CurLine.LineID;
                    drNew["SERVERNAME"] = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    drNew["TARGETASSETTEID"] = _targetCSTID; // txtTargetCstID.Text.ToString().Trim();
                    drNew["SOURCECASSETTEID"] = _souceCSTID; // txtSourceCstID.Text.ToString().Trim();
                    drNew["TARGETSLOTNO"] = "";
                    drNew["TARGETSLOTSELECT"] = rdoAuto.Checked ? "Auto" : "Manual";
                    dtAllChgPlan.Rows.Add(drNew);

                }
                dtAllChgPlan.DefaultView.Sort = "SOURCECASSETTEID, SLOTNO";
                dgvChgPlan.DataSource = dtAllChgPlan.DefaultView.ToTable();
                #endregion

                #region Clear Layout
                if (txtPlanID.Enabled == true) txtPlanID.Enabled = false;
                txtStartSlotNo.Clear();
                txtEndSlotNo.Clear();
                txtSourceCstID.Clear();
                txtTargetCstID.Clear();
                txtStartSlotNo.Focus();
                #endregion
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
                if (dgvChgPlan.Rows.Count == 0) return;

                DataTable dtAllChgPlan = (DataTable)dgvChgPlan.DataSource;
                DataRow[] drDelChgPlan = dtAllChgPlan.Select(string.Format("SLOTNO='{0}'", dgvChgPlan.CurrentRow.Cells[colSlotNo.Name].Value));
                
                if (drDelChgPlan.Length > 0)
                {
                    dtAllChgPlan.Rows.Remove(drDelChgPlan[0]);
                }

                if (FormMode.Equals(UniOPI.FormMode.AddNew) && dgvChgPlan.Rows.Count == 0 && txtPlanID.Enabled == false)
                    txtPlanID.Enabled = true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        #endregion       

    }
}
