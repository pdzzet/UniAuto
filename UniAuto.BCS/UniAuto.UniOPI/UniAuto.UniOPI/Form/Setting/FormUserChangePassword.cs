using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormUserChangePassword : FormBase
    {
        private Dictionary<string, List<Line>> _dicLineType = null;

        public Dictionary<string, List<Line>> LineTypeCollection
        {
            get
            {
                if (_dicLineType == null)
                {
                    _dicLineType = new Dictionary<string, List<Line>>();

                    foreach (Line eachLine in FormMainMDI.G_OPIAp.Dic_Line.Values)
                    {
                        // 排除local line
                        if (eachLine.LineID.Equals(FormMainMDI.G_OPIAp.CurLine.LineID))
                            continue;

                        if (!_dicLineType.ContainsKey(eachLine.LineType))
                        {
                            List<Line> lstLine = new List<Line>();
                            lstLine.Add(eachLine);
                            _dicLineType.Add(eachLine.LineType, lstLine);
                        }
                        else
                            _dicLineType[eachLine.LineType].Add(eachLine);
                    }
                }

                return _dicLineType;
            }
        }

        public FormUserChangePassword()
        {
            InitializeComponent();
            lblCaption.Text = "User Change Password";
        }

        #region Events
        private void FormUserChangePassword_Load(object sender, EventArgs e)
        {
            gbxPassword.Text = string.Format("Current User : {0}", FormMainMDI.G_OPIAp.LoginUserID);
            this.GetLineTypeList();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                dgvList.DataSource = null;

                #region CheckData
                if ("".Equals(txtOldPassword.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Old password required！", MessageBoxIcon.Information);
                    txtOldPassword.Focus();
                    return;
                }

                if (!FormMainMDI.G_OPIAp.LoginPassword.Equals(txtOldPassword.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Old password is not correct！", MessageBoxIcon.Information);
                    txtOldPassword.Focus();
                    return;
                }

                if ("".Equals(txtNewPassword.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "New password required！", MessageBoxIcon.Information);
                    txtNewPassword.Focus();
                    return;
                }

                if (txtOldPassword.Text.Trim().Equals(txtNewPassword.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "New password must be different from Old password！", MessageBoxIcon.Information);
                    txtNewPassword.Focus();
                    return;
                }
                #endregion

                List<SyncResult> lstChangeResult = new List<SyncResult>();
                List<LineTypeClass> lstSelcted = null;

                lstSelcted = chklsbLineType.CheckedItems.OfType<LineTypeClass>().ToList();

                // 沒有勾選要同步的LineType時，預設抓出和CurLine同LineType的Line出來同步
                if (lstSelcted.Count == 0)
                {
                    var query = (from c in LineTypeCollection
                                 where c.Key == FormMainMDI.G_OPIAp.CurLine.LineType
                                 select c);

                    List<KeyValuePair<string, List<Line>>> keyValuePairs = query.ToList()
                                                                        .Select(t => new KeyValuePair<string, List<Line>>(t.Key, t.Value))
                                                                        .ToList();
                    if (keyValuePairs.Count > 0)
                    {
                        List<string> lstLineStr = new List<string>();
                        foreach (Line line in keyValuePairs[0].Value)
                            lstLineStr.Add(line.LineID);

                        lstSelcted.Add(new LineTypeClass()
                            {
                                LineType = FormMainMDI.G_OPIAp.CurLine.LineType,
                                LineIDStr = string.Join(",", lstLineStr),
                                LineCollection = keyValuePairs[0].Value
                            });
                    }
                }

                try
                {
                    lstChangeResult.AddRange(this.ChangeLocalLine());
                    lstChangeResult.AddRange(this.SyncRemoteLine(lstSelcted));
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                }

                txtOldPassword.Clear();
                txtNewPassword.Clear();

                if (lstChangeResult.Count > 0)
                {
                    // Show Result
                    dgvList.DataSource = lstChangeResult;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Private Methods
        private void GetLineTypeList()
        {
            try
            {
                chklsbLineType.Items.Clear();

                List<string> lstLineStr = new List<string>();
                LineTypeClass objLineType = null;
                foreach (string lineType in LineTypeCollection.Keys)
                {
                    if (LineTypeCollection[lineType].Count == 0)
                        continue;

                    lstLineStr.Clear();
                    foreach (Line line in LineTypeCollection[lineType])
                        lstLineStr.Add(line.LineID);

                    objLineType = new LineTypeClass()
                    {
                        LineType = lineType,
                        LineIDStr = string.Join(",", lstLineStr),
                        LineCollection = LineTypeCollection[lineType]
                    };

                    chklsbLineType.Items.Add(objLineType);
                }

                ((ListBox)chklsbLineType).DisplayMember = "Description";
                ((ListBox)chklsbLineType).ValueMember = "LineType";
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private List<SyncResult> ChangeLocalLine()
        {
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

            SBRM_OPI_USER_ACCOUNT _userToSync = null;

            string _errMsg = string.Empty;

            List<SyncResult> _lstResult = new List<SyncResult>();

            try
            {
                #region check data from DB
                _userToSync = (from user in _ctxBRM.SBRM_OPI_USER_ACCOUNT
                               where user.USER_ID == FormMainMDI.G_OPIAp.LoginUserID
                               select user).FirstOrDefault();

                if (_userToSync == null)
                {
                    _errMsg = string.Format("User: [{0}] not exist.", FormMainMDI.G_OPIAp.LoginUserID);

                    NLogManager.Logger.LogWarnWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, _errMsg);

                    _lstResult.Add(new SyncResult()
                    {
                        LineType = FormMainMDI.G_OPIAp.CurLine.LineType,
                        LineID = FormMainMDI.G_OPIAp.CurLine.LineID,
                        ServerName = FormMainMDI.G_OPIAp.CurLine.ServerName,
                        Result = "Fail",
                        ResultDesc = _errMsg
                    });

                    return _lstResult;
                }
                #endregion

                _userToSync.PASSWORD = txtNewPassword.Text.Trim();

                _userToSync.TRX_DATETIME = System.DateTime.Now;

                string _sqlDesc = string.Format("User ID[{0} Change Password [{1}]]", FormMainMDI.G_OPIAp.LoginUserID, _userToSync.PASSWORD);

                try
                {
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
                        _errMsg = ex.ToString();

                        foreach (System.Data.Linq.MemberChangeConflict _data in _ctxBRM.ChangeConflicts[0].MemberConflicts)
                        {
                            if (_data.DatabaseValue != _data.OriginalValue)
                            {
                                _errMsg = _errMsg + string.Format("\r\n Change Conflicts : Property '{0}': Database value: {1}, Original value {2}, Current Value:{3}", _data.Member.Name, _data.DatabaseValue, _data.OriginalValue, _data.CurrentValue);
                            }
                        }

                        _lstResult.Add(new SyncResult()
                        {
                            LineType = FormMainMDI.G_OPIAp.CurLine.LineType,
                            LineID = FormMainMDI.G_OPIAp.CurLine.LineID,
                            ServerName = FormMainMDI.G_OPIAp.CurLine.ServerName,
                            Result = "Fail",
                            ResultDesc = _errMsg
                        });

                        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                    }
                }

                #region 紀錄opi history
                string _err = UniTools.InsertOPIHistory_DB("SBRM_OPI_USER_ACCOUNT", _sqlDesc, _errMsg);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                if (_lstResult.Count > 0) return _lstResult;                

                _lstResult.Add(new SyncResult()
                {
                    LineType = FormMainMDI.G_OPIAp.CurLine.LineType,
                    LineID = FormMainMDI.G_OPIAp.CurLine.LineID,
                    ServerName = FormMainMDI.G_OPIAp.CurLine.ServerName,
                    Result = "Success",
                    ResultDesc = ""
                });

                // Update user password
                FormMainMDI.G_OPIAp.LoginPassword = txtNewPassword.Text.Trim();

                NLogManager.Logger.LogInfoWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, "Change password succeeded.");

                return _lstResult;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                _lstResult.Add(new SyncResult()
                {
                    LineType = FormMainMDI.G_OPIAp.CurLine.LineType,
                    LineID = FormMainMDI.G_OPIAp.CurLine.LineID,
                    ServerName = FormMainMDI.G_OPIAp.CurLine.ServerName,
                    Result = "Fail",
                    ResultDesc = ex.ToString()
                });

                return _lstResult;
            }
        }

        private List<SyncResult> SyncRemoteLine(List<LineTypeClass> LineTypeToSync)
        {
            string _errMsg = string.Empty;
            string _fabType = string.Empty;
            string _lineType = string.Empty;
            string _serverName = string.Empty;
            string _sqlDesc = string.Format("User ID[{0} Change Password [{1}]]", FormMainMDI.G_OPIAp.LoginUserID, txtNewPassword.Text.Trim());

            UniBCSDataContext _ctxBRM = null;            
            
            List<SyncResult> _lstResult = new List<SyncResult>();

            SqlConnection _remoteDBConn = null;

            csDBConfigXML _dbConfigXml = FormMainMDI.G_OPIAp.DBConfigXml;

            List<SBRM_OPI_USER_ACCOUNT> _lstRemote = null;

            SBRM_OPI_USER_ACCOUNT _userToSync = null;

            try
            {
                foreach (LineTypeClass objLineType in LineTypeToSync)
                {
                    foreach (Line lineToSync in objLineType.LineCollection)
                    {
                        _errMsg = string.Empty;
                        _fabType = lineToSync.FabType;
                        _lineType = lineToSync.LineType;
                        _serverName = lineToSync.ServerName;

                        #region  connect remote line DB
                        try
                        {
                            _remoteDBConn = new SqlConnection(_dbConfigXml.dic_Setting[_fabType].dic_LineType[_lineType].dic_Line[_serverName].DBConn.ToString());
                        }
                        catch (Exception ex)
                        {
                            _errMsg = "Cannot find database connection information.";

                            NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                            
                            _lstResult.Add(new SyncResult()
                            {
                                LineType = _lineType,
                                LineID = lineToSync.LineID,
                                ServerName = lineToSync.ServerName,
                                Result = "Fail",
                                ResultDesc = _errMsg
                            });
                            continue;
                        }
                         #endregion

                        #region Get User account from remote line
                        try
                        {
                            _ctxBRM = new UniBCSDataContext(_remoteDBConn);

                            _lstRemote = (from user in _ctxBRM.SBRM_OPI_USER_ACCOUNT
                                         select user).ToList<SBRM_OPI_USER_ACCOUNT>();

                            _userToSync = _lstRemote.FirstOrDefault<SBRM_OPI_USER_ACCOUNT>(u => u.USER_ID == FormMainMDI.G_OPIAp.LoginUserID);

                            if (_userToSync == null)
                            {
                                _errMsg = string.Format("User: [{0}] not exist.", FormMainMDI.G_OPIAp.LoginUserID);

                                NLogManager.Logger.LogWarnWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, _errMsg);
 
                                _lstResult.Add(new SyncResult()
                                {
                                    LineType = _lineType,
                                    LineID = lineToSync.LineID,
                                    ServerName = lineToSync.ServerName,
                                    Result = "Fail",
                                    ResultDesc = _errMsg
                                });

                                continue;
                            }
                        }
                        catch (Exception sqlex)
                        {
                            _errMsg = sqlex.Message;

                            NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, sqlex);
                            
                            _lstResult.Add(new SyncResult()
                            {
                                LineType = _lineType,
                                LineID = lineToSync.LineID,
                                ServerName = lineToSync.ServerName,
                                Result = "Fail",
                                ResultDesc = _errMsg
                            });
                            continue;
                        }
                        #endregion

                        _userToSync.PASSWORD = txtNewPassword.Text.Trim();
                        _userToSync.TRX_DATETIME = System.DateTime.Now;
                       
                        try
                        {
                            _ctxBRM.SubmitChanges();
                        }
                        catch (Exception ex)
                        {
                            _errMsg = ex.Message;

                            NLogManager.Logger.LogInfoWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, _errMsg);
                            
                            _lstResult.Add(new SyncResult()
                            {
                                LineType = _lineType,
                                LineID = lineToSync.LineID,
                                ServerName = lineToSync.ServerName,
                                Result = "Fail",
                                ResultDesc = _errMsg
                            });
                            continue;
                        }

                        #region 建立SBCS_OPIHISTORY_TRX 物件
                        SBCS_OPIHISTORY_TRX _opiHistory = new SBCS_OPIHISTORY_TRX();

                        _opiHistory.SESSECTIONID = FormMainMDI.G_OPIAp.SessionID == null ? "" : FormMainMDI.G_OPIAp.SessionID;
                        _opiHistory.OPDATETIME = DateTime.Now;
                        _opiHistory.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                        _opiHistory.DIRECTION = "OPI-> DB";
                        _opiHistory.TRANSACTIONID = string.Empty;
                        _opiHistory.MESSAGENAME = "SBRM_OPI_USER_ACCOUNT";
                        _opiHistory.MESSAGETRX = "Sync Data From " + FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _opiHistory.RETURNCODE = string.Empty;
                        _opiHistory.RETURNMESSAGE = string.Empty;
                        _opiHistory.COMMANDKEY = "DBModify_Sync";
                        _opiHistory.COMMANDDATA = _sqlDesc;
                        _opiHistory.COMMANDTYPE = "DB";
                        _opiHistory.PROCESSRESULT = _errMsg == string.Empty ? "Success" : "NG";
                        _opiHistory.PROCESSNGMESSAGE = _errMsg;

                        _ctxBRM.SBCS_OPIHISTORY_TRX.InsertOnSubmit(_opiHistory);

                        try
                        {
                            _ctxBRM.SubmitChanges();
                        }
                        catch (System.Data.Linq.ChangeConflictException ex)
                        {
                            NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);

                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                        }

                        #endregion

                        _lstResult.Add(new SyncResult()
                        {
                            LineType = _lineType,
                            LineID = lineToSync.LineID,
                            ServerName = lineToSync.ServerName,
                            Result = "Success",
                            ResultDesc = ""
                        });

                        NLogManager.Logger.LogInfoWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("Sync ServerName [{0}] succeeded.", lineToSync.ServerName));
                    }
                }

                return _lstResult;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                _lstResult.Add(new SyncResult()
                {
                    LineType = FormMainMDI.G_OPIAp.CurLine.LineType,
                    LineID = FormMainMDI.G_OPIAp.CurLine.LineID,
                    ServerName = FormMainMDI.G_OPIAp.CurLine.ServerName,
                    Result = "Fail",
                    ResultDesc = ex.ToString()
                });

                return _lstResult;
            }
        }
        #endregion
    }

    class LineTypeClass
    {
        public string LineType { get; set; }
        public string LineIDStr { get; set; }
        public List<UniOPI.Line> LineCollection { get; set; }
        public string Description
        {
            get
            {
                return string.Format("{0} ({1})", this.LineType, this.LineIDStr);
            }
        }
    }

    class SyncResult
    {
        public string LineType { get; set; }
        public string LineID { get; set; }
        public string ServerName { get; set; }
        public string Result { get; set; }
        public string ResultDesc { get; set; }
    }
}