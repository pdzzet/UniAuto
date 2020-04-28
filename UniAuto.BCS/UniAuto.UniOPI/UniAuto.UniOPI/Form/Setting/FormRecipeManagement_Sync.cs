using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRecipeSync : FormBase
    {
        string MESMode = "OFFLINE";

        public FormRecipeSync()
        {
            InitializeComponent();
        }

        private void FormRecipeSync_Load(object sender, System.EventArgs e)
        {
            try
            {
                string _serverName = FormMainMDI.G_OPIAp.CurLine.ServerName;
                string _lineType = FormMainMDI.G_OPIAp.CurLine.LineType;
                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;

                List<Line> _lstLine = new List<Line>();

                var _x = (from line in _ctx.SBRM_LINE
                          where line.SERVERNAME != _serverName && line.LINETYPE == _lineType  
                          select new LineSync
                          {
                              Checked = true,
                              ServerName = line.SERVERNAME,
                              LineName = line.LINENAME,
                              FabType = line.FABTYPE,
                              LineType = line.LINETYPE
                          });

                dgvList.DataSource = _x.ToList();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btn_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender;
                string _errMsg = string.Empty;

                switch (_btn.Tag.ToString())
                {
                    case "Sync":

                        string _msg = string.Format("Please confirm whether you will Sync. Recipe Data ?");

                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text,  _msg)) return;

                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text )) return;

                        #region Sync
                        List<LineSync> _lstLine = dgvList.DataSource as List<LineSync>;
                        if (_lstLine.Count == 0)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "No line of same type to sync！", MessageBoxIcon.Information);
                            return;
                        }

                        #region 取得來源資料
                        UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;
                        List<SBRM_RECIPE> _lstSource = (from r in _ctx.SBRM_RECIPE
                                                        where (r.ONLINECONTROLSTATE == MESMode)
                                                       select r).ToList<SBRM_RECIPE>();
                        #endregion
                        
                        Dictionary<string, string> _dicLineMsg = new Dictionary<string, string>();

                        foreach (LineSync lineToSync in _lstLine)
                        {
                            if (lineToSync.Checked)
                            {
                                _errMsg = this.SyncRemoteLine(_lstSource, lineToSync);

                                if (!"".Equals(_errMsg))
                                {
                                    _dicLineMsg.Add(lineToSync.ServerName, _errMsg);                                    
                                }

                                //NLogManager.Logger.LogInfoWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("Sync. Recipe for ServerName [{0}]",lineToSync.ServerName));
                            }
                        }
                        
                        StringBuilder _sb = new StringBuilder();

                        foreach (KeyValuePair<string, string> kvp in _dicLineMsg)
                        {
                            _sb.AppendFormat("{0}Line ID: {1}, Message: {2}", Environment.NewLine, kvp.Key, kvp.Value);
                        }

                        if (_dicLineMsg.Count > 0)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", string.Format("Sync complete with errors!\n{0}", _sb.ToString()), MessageBoxIcon.Error);
                        }
                        else
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Sync success!", MessageBoxIcon.Information);
                        }                        
                        #endregion

                        this.DialogResult = System.Windows.Forms.DialogResult.OK;

                        break;

                    case "Close":

                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

                        break;

                    default:
                        break;

                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private string SyncRemoteLine(List<SBRM_RECIPE> LstSource, LineSync LineToSync)
        {
            try
            {
                string _sqlDesc = string.Empty;
                string _errMsg = string.Empty;
                string _onlineCtrlState = string.Empty;
                string _lineRecipeName = string.Empty;

                string _fabType = LineToSync.FabType;
                string _lineType = LineToSync.LineType;
                string _serverName = LineToSync.ServerName;
                string _conStr = string.Empty;
                DateTime _syncTime = System.DateTime.Now;

                SBRM_RECIPE _objRecipe = null;
                UniBCSDataContext _ctxBRM = null;
                SqlConnection _loginDBConn = null;                
                List<SBRM_RECIPE> _lstRemote = null;
                csDBConfigXML _dbConfigXml = FormMainMDI.G_OPIAp.DBConfigXml;

                #region connect remote servername DB
                try
                {
                    _conStr = _dbConfigXml.dic_Setting[_fabType].dic_LineType[_lineType].dic_Line[_serverName].DBConn.ToString();
                    _loginDBConn = new SqlConnection(_conStr);
                }
                catch (Exception knfe)
                {
                    _errMsg = "Cannot find database connection information.";
                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, knfe);
                    return _errMsg;
                }

                try
                {
                    _ctxBRM = new UniBCSDataContext(_loginDBConn);
                    _lstRemote = (from recipe in _ctxBRM.SBRM_RECIPE
                                 select recipe).ToList<SBRM_RECIPE>();
                }
                catch (Exception sqlex)
                {
                    _errMsg = sqlex.Message;
                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, sqlex);
                    return _errMsg;
                }

                NLogManager.Logger.LogInfoWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("Connect to Server Name [{0}].", _serverName));

                #endregion

                #region Sync Recipe Table

                foreach (SBRM_RECIPE src in LstSource)
                {
                    _fabType = src.FABTYPE;
                    _lineType = src.LINETYPE;
                    _onlineCtrlState = src.ONLINECONTROLSTATE;
                    _lineRecipeName = src.LINERECIPENAME;

                    SBRM_RECIPE existRecipe = _lstRemote.FirstOrDefault<SBRM_RECIPE>(
                        r => r.FABTYPE == _fabType &&
                            r.LINETYPE == _lineType &&
                            r.ONLINECONTROLSTATE == _onlineCtrlState &&
                            r.LINERECIPENAME == _lineRecipeName
                            );

                    if (existRecipe != null)    // Key值相同者略過
                    {
                        NLogManager.Logger.LogWarnWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                            string.Format("Sync skipped on ({0}, {1}, {2}, {3}).", _fabType, _lineType, _onlineCtrlState, _lineRecipeName));
                        continue;
                    }

                    #region 建立SBRM_RECIPE 物件
                    _objRecipe = new SBRM_RECIPE();

                    _objRecipe.FABTYPE = _fabType;
                    _objRecipe.LINETYPE = _lineType;
                    _objRecipe.ONLINECONTROLSTATE = _onlineCtrlState;
                    _objRecipe.LINERECIPENAME = _lineRecipeName;
                    _objRecipe.PPID = src.PPID;

                    _objRecipe.LASTUPDATEDT = _syncTime;
                    _objRecipe.UPDATEOPERATOR = FormMainMDI.G_OPIAp.LoginUserID;
                    _objRecipe.UPDATELINEID = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    _objRecipe.UPDATEPCIP = FormMainMDI.G_OPIAp.LocalIPAddress;
                    _objRecipe.REMARK = src.REMARK;
                    #endregion

                    _ctxBRM.SBRM_RECIPE.InsertOnSubmit(_objRecipe);

                    try
                    {
                        UniTools.addRecipeTableHistory( "Create", (SBRM_RECIPE)_objRecipe, _ctxBRM, _conStr);

                        _sqlDesc = _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") +
                            string.Format("ADD [ {0} , {1} , {2} ] ", _objRecipe.ONLINECONTROLSTATE, _objRecipe.LINERECIPENAME, _objRecipe.PPID);

                        _ctxBRM.SubmitChanges();
                    }
                    catch (System.Data.Linq.ChangeConflictException ex)
                    {
                        _errMsg += string.Format("Sync Failed on ({0}, {1}, {2}, {3}).", _fabType, _lineType, _onlineCtrlState, _lineRecipeName);

                        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);

                        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                        continue;
                    }

                    NLogManager.Logger.LogInfoWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                        string.Format("Sync Succeeded on ({0}, {1}, {2}, {3}).", _fabType, _lineType, _onlineCtrlState, _lineRecipeName));
                }

                #endregion

                #region 建立SBCS_OPIHISTORY_TRX 物件
                SBCS_OPIHISTORY_TRX _opiHistory = new SBCS_OPIHISTORY_TRX();

                _opiHistory.SESSECTIONID = FormMainMDI.G_OPIAp.SessionID == null ? "" : FormMainMDI.G_OPIAp.SessionID;
                _opiHistory.OPDATETIME = DateTime.Now;
                _opiHistory.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                _opiHistory.DIRECTION = "OPI-> DB";
                _opiHistory.TRANSACTIONID = string.Empty;
                _opiHistory.MESSAGENAME = "SBRM_RECIPE";
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

                return _errMsg;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                
                return ex.ToString();
            }
        }

        private void rdo_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                if (_rdo.Checked) MESMode = _rdo.Tag.ToString();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private class LineSync
        {
            public bool Checked { get; set; }
            public string ServerName { get; set; }
            public string LineName { get; set; }
            public string FabType { get; set; }
            public string LineType { get; set; }
        }
    }
}
