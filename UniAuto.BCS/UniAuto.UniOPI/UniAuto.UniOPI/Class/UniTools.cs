using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public static class UniTools
    {
        #region 匯出Excel
        public static bool ExportToExcel(string[] lstSheetName, string[] lstTitleName, DataTable[] lstSourceDt, out string errMessage)
        {
            errMessage = string.Empty;
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel files (*.xls)|*.xls";
                saveDialog.Title = "Save a Excel File";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string xlsFilePath = saveDialog.FileName;
                    WorkbookMgr CurWbMgr = new WorkbookMgr();
                    CurWbMgr.WriteWorkbook(xlsFilePath, lstSheetName, lstTitleName, lstSourceDt, new string[0], new string[0]);
                }

                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 匯出Excel
        /// </summary>
        /// <param name="title">標題名稱</param>
        /// <param name="dgv">DataGridView Control</param>
        /// <param name="maxRecord">最大資料筆數,0為全部</param>
        /// <param name="errMessage">錯誤訊息</param>
        /// <returns></returns>
        public static bool ExportToExcel(string title, DataGridView dgv, int maxRecord, out string errMessage)
        {
            errMessage = string.Empty;
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel files (*.xls)|*.xls";
                saveDialog.Title = "Save a Excel File";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string xlsFilePath = saveDialog.FileName;

                    WorkbookMgr CurWbMgr = new WorkbookMgr();
                    DataTable CurXLSData = new DataTable();

                    CurXLSData = dgvToDt(dgv, maxRecord);
                    CurWbMgr.WriteWorkbook(xlsFilePath, "Report", title, CurXLSData);

                    return true;
                }
                else
                {
                    errMessage = "Cancel Export";
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 匯出Excel
        /// </summary>
        /// <param name="title">標題名稱</param>
        /// <param name="dgv">DataGridView Control</param>
        /// <param name="maxRecord">最大資料筆數,0為全部</param>
        /// <param name="errMessage">錯誤訊息</param>
        /// <returns></returns>
        public static bool ExportToExcel(string title, DataGridView dgv, int maxRecord,List<string> passColumnName, out string errMessage)
        {
            errMessage = string.Empty;
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel files (*.xls)|*.xls";
                saveDialog.Title = "Save a Excel File";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string xlsFilePath = saveDialog.FileName;

                    WorkbookMgr CurWbMgr = new WorkbookMgr();
                    DataTable CurXLSData = new DataTable();

                    CurXLSData = dgvToDt(dgv, passColumnName,maxRecord);
                    CurWbMgr.WriteWorkbook(xlsFilePath, "Report", title, CurXLSData);

                    return true;
                }
                else
                {
                    errMessage = "Cancel Export";
                    return false;
                }

            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 匯出Excel
        /// </summary>
        /// <param name="title">標題名稱</param>
        /// <param name="pdgv">DataGridView Control</param>
        /// <param name="exportAll">true:匯出全部資料, false:匯出當前分頁資料</param>
        /// <param name="errMessage">錯誤訊息</param>
        /// <returns></returns>
        public static bool ExportToExcel(string title, PagedGridView pdgv, bool exportAll, out string errMessage)
        {
            errMessage = string.Empty;
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel files (*.xls)|*.xls";
                saveDialog.Title = "Save a Excel File";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string xlsFilePath = saveDialog.FileName;

                    WorkbookMgr CurWbMgr = new WorkbookMgr();
                    DataTable CurXLSData = new DataTable();

                    CurXLSData = dgvToDt(pdgv, exportAll);
                    CurWbMgr.WriteWorkbook(xlsFilePath, "Report", title, CurXLSData);

                    return true;
                }
                else
                {
                    errMessage = "Cancel Export";
                    return false;
                }

                
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }

        public static DataTable dgvToDt(DataGridView dgv, int maxRecord)
        {
            DataTable dt = new DataTable();
            DataColumn dc;

            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                dc = new DataColumn();
                dc.ColumnName = dgv.Columns[i].HeaderText.ToString();
                dt.Columns.Add(dc);
            }

            //MaxRecord為0則取全部
            int rowCount = (maxRecord == 0 || maxRecord > dgv.Rows.Count) ? dgv.Rows.Count : maxRecord;

            for (int j = 0; j < rowCount; j++)
            {
                DataRow dr = dt.NewRow();
                for (int x = 0; x < dgv.Columns.Count; x++)
                {
                    dr[x] = dgv.Rows[j].Cells[x].Value;
                }
                dt.Rows.Add(dr);
            }

            //Remove unvisible column
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (dgv.Columns[i].Visible == false)
                    dt.Columns.Remove(dgv.Columns[i].HeaderText);
            }
            return dt;
        }

        public static DataTable dgvToDt(DataGridView dgv, List<string> passColumnName,int maxRecord)
        {
            DataTable dt = new DataTable();
            DataColumn dc;

            for (int i = 0; i < dgv.Columns.Count; i++)
            {                
                dc = new DataColumn();
                dc.ColumnName = dgv.Columns[i].HeaderText.ToString();
                dt.Columns.Add(dc);
            }

            //MaxRecord為0則取全部
            int rowCount = (maxRecord == 0 || maxRecord > dgv.Rows.Count) ? dgv.Rows.Count : maxRecord;

            for (int j = 0; j < rowCount; j++)
            {
                DataRow dr = dt.NewRow();
                for (int x = 0; x < dgv.Columns.Count; x++)
                {                    
                    dr[x] = dgv.Rows[j].Cells[x].Value;
                }
                dt.Rows.Add(dr);
            }

            //Remove unvisible column
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (dgv.Columns[i].Visible == false || passColumnName.Contains(dgv.Columns[i].Name))
                {
                    dt.Columns.Remove(dgv.Columns[i].HeaderText);
                }
            }
            return dt;
        }

        public static DataTable dgvToDt(PagedGridView pdgv, bool exportAll = true)
        {
            DataTable dt = new DataTable();
            DataColumn dc;

            for (int i = 0; i < pdgv.Columns.Count; i++)
            {
                dc = new DataColumn();
                dc.ColumnName = pdgv.Columns[i].HeaderText.ToString();
                dt.Columns.Add(dc);
            }

            // 匯出全部或當前分頁
            DataTable dtExport = exportAll ? pdgv.Source : (pdgv.DataSource as DataTable);

            string columnName = string.Empty;
            for (int j = 0; j < dtExport.Rows.Count; j++)
            {
                DataRow dr = dt.NewRow();
                for (int x = 0; x < pdgv.Columns.Count; x++)
                {
                    columnName = pdgv.Columns[x].DataPropertyName;
                    if (!dtExport.Columns.Contains(columnName))
                        continue;
                    dr[x] = dtExport.Rows[j][columnName].ToString();
                }
                dt.Rows.Add(dr);
            }

            //Remove unvisible column
            for (int i = 0; i < pdgv.Columns.Count; i++)
            {
                if (pdgv.Columns[i].Visible == false || pdgv.Columns[i] is DataGridViewButtonColumn)
                    dt.Columns.Remove(pdgv.Columns[i].HeaderText);
            }
            return dt;
        }
        #endregion

        public static DataTable InitDt(string[] colList)
        {
            DataTable dt = new DataTable();
            foreach (string col in colList)
            {
                dt.Columns.Add(col);
            }
            return dt;
        }

        // 判斷填入TextBox填入的數值是否為整數
        public static bool CheckTextBoxKeyPressIsInteger(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (Char)48 || e.KeyChar == (Char)49 ||
               e.KeyChar == (Char)50 || e.KeyChar == (Char)51 ||
               e.KeyChar == (Char)52 || e.KeyChar == (Char)53 ||
               e.KeyChar == (Char)54 || e.KeyChar == (Char)55 ||
               e.KeyChar == (Char)56 || e.KeyChar == (Char)57 ||
               e.KeyChar == (Char)13 || e.KeyChar == (Char)8)
            {
                e.Handled = false;
                return true;
            }
            else
            {
                e.Handled = true;
                return false;
            }
        }

        public static string GetGlassID(int slotNo)
        {
            string _glassID = string.Empty;


            if (FormMainMDI.G_OPIAp.CurLine.LineType == "FCUPK_TYPE1")
            {
                //UPK 要自動產生glass id 不提供給人員修改 : 1 + yymmdd + CSTSeqNo*5 + JobSeqNo*4
                _glassID = slotNo.ToString().PadLeft(4, '0');
            }
            else
            {
                //AA,AB,AC.....跳過 I O兩字母
                int _quotient = slotNo / 24; //商數決定第一碼
                int _remainder = slotNo % 24; //餘數決定第二碼

                int _data1 = 0; //第一碼
                int _data2 = 0; //第二碼
                int _data3 = 0; //第三碼

                #region 第一碼
                if (_remainder == 0)
                {
                    _data1 = 90;
                    _quotient = _quotient - 1;
                }
                else
                {
                    _data1 = 65 + (_remainder - 1);

                    if (_data1 >= 73) _data1 = _data1 + 1;
                    if (_data1 >= 79) _data1 = _data1 + 1;
                }
                #endregion

                #region 第二碼 & 第三碼
                if (_quotient > 23)
                {
                    int _quotient2 = _quotient / 24;
                    int _remainder2 = _quotient % 24;

                    #region 第二碼

                    _data2 = 65 + _remainder2;

                    if (_data2 >= 73) _data2 = _data2 + 1;
                    if (_data2 >= 79) _data2 = _data2 + 1;

                    #endregion


                    _data3 = 65 + _quotient2;

                    if (_data3 >= 73) _data3 = _data3 + 1;
                    if (_data3 >= 79) _data3 = _data3 + 1;
                }
                else
                {
                    _data3 = 65;

                    _data2 = 65 + _quotient;
                    if (_data2 >= 73) _data2 = _data2 + 1;
                    if (_data2 >= 79) _data2 = _data2 + 1;
                }
                #endregion

                _glassID = Convert.ToChar(_data3).ToString() + Convert.ToChar(_data2).ToString() + Convert.ToChar(_data1).ToString();

            }

            return _glassID;
        }

        
        //1. 前9码 + 序列号（1~Z, i,I 与o,O不使用）* 1 碼 +“B”;
        //2. 前9码  = 人员输入长度，最大9码,可輸入英數
        //3. 全部字母大写，无小写,排除I O
        //4. 序列号1码 (1-9, A-Z,排除I O)
        //5. 总长度11码
        public static string GetGlassID_Array(int slotNo)
        {
            string _glassID = string.Empty;

            int _data1 = 0; //第一碼

            if (slotNo >= 1 && slotNo <= 9)
            {
                _data1 = 48 + slotNo;
            }
            else
            {
                //AA,AB,AC.....跳過 I O兩字母
                int _quotient = (slotNo - 9) / 24; //商數決定第一碼
                int _remainder = (slotNo - 9) % 24; //餘數決定第二碼

                if (_remainder == 0)
                {
                    _data1 = 90;
                    _quotient = _quotient - 1;
                }
                else
                {
                    _data1 = 65 + (_remainder - 1);

                    if (_data1 >= 73) _data1 = _data1 + 1;  //跳過 I 
                    if (_data1 >= 79) _data1 = _data1 + 1;  //跳過 O
                }
            }

            _glassID =  Convert.ToChar(_data1).ToString() + "B";

            return _glassID;
        }

        public static bool GetVirualGlassIDSerialNo(ref string glassSerialNo, ref string msg)
        {
            int serialNo = 0;
            UpdateResult updateRst = UpdateResult.None;
            while (updateRst == UpdateResult.None || updateRst == UpdateResult.UpdateFail)
            {
                updateRst = TryGetVirualGlassIDSerialNo(ref serialNo, ref msg);
            }
            if (updateRst == UpdateResult.CheckFail) return false;
            glassSerialNo = serialNo.ToString().PadLeft(3, '0');
            return true;
        }

        public static List<string> GetNodeRecipeID(string NodeNo)
        {
            try
            {
                List<string> lstNodeRecipeID = new List<string>();

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var c = from msg in ctxBRM.SBRM_RECIPE
                        where msg.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType
                        select msg.PPID;

                DataTable dtRecipe = DBConnect.GetDataTable(ctxBRM, "", c);

                foreach (DataRow drRecipe in dtRecipe.Rows)
                {
                    string strPPID = drRecipe["PPID"].ToString();

                    List<string> _ppid = strPPID.Split(';').ToList();

                    string result = _ppid.FirstOrDefault(s => s.Contains(NodeNo + ":"));

                    if (result == null) continue;

                    string[] _data = result.Split(':');

                    if (_data.Length < 2) continue;

                    if (_data[1] == FormMainMDI.G_OPIAp.Dic_Node[NodeNo].DefaultRecipeNo) continue;

                    if (lstNodeRecipeID.Contains(_data[1])) continue;

                    lstNodeRecipeID.Add(_data[1]);
                }

                if (lstNodeRecipeID.Count > 0) lstNodeRecipeID.Sort();

                return lstNodeRecipeID;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite("UniTools", MethodBase.GetCurrentMethod().Name, ex);
                return null;
            }
        }

        private static UpdateResult TryGetVirualGlassIDSerialNo(ref int serialNo, ref string errMsg)
        {
            UniBCSDataContext dbCtx = new UniBCSDataContext(FormMainMDI.G_OPIAp.DBCtx.Connection);
            try
            {

                //判斷SBRM_OPI_PARAMETER 是否有KEYWORD為VirualGlassIDSerialNo的參數
                var opiParam = from d in dbCtx.SBRM_OPI_PARAMETER
                               where d.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) && d.KEYWORD.Equals("VirualGlassIDSerialNo")
                               select d;
                if (opiParam.Count() == 0)
                {
                    errMsg = "Can't find the KEYWORD is equal VirualGlassIDSerialNo in SBRM_OPI_PARAMETER.";
                    return UpdateResult.CheckFail;
                }
                //判斷SBRM_OPI_PARAMETER 的KEYWORD為VirualGlassIDSerialNo的參數是否為數字 
                string curItemValue = opiParam.FirstOrDefault().ITEMVALUE;
                int curSerialNo = 0;
                if (!int.TryParse(curItemValue, out curSerialNo))
                {
                    errMsg = "The ItemValue of VirualGlassIDSerialNo must be  integer in  SBRM_OPI_PARAMETER";
                    return UpdateResult.CheckFail;
                }
                //取得並更新序號
                var opiParamItemValue = from d in dbCtx.SBRM_OPI_PARAMETER
                                        where d.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType && d.KEYWORD == "VirualGlassIDSerialNo" && d.ITEMVALUE == curItemValue
                                        select d;
                SBRM_OPI_PARAMETER clsOpiParamItemValue = opiParamItemValue.ToList()[0];
                serialNo = curSerialNo + 1;
                clsOpiParamItemValue.ITEMVALUE = serialNo.ToString();
                dbCtx.SubmitChanges();
                return UpdateResult.UpdateSuccess;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite("UniTools", MethodBase.GetCurrentMethod().Name, ex);
                return UpdateResult.UpdateFail;
            }
            finally
            {
                dbCtx.Dispose();
            }

        }

        public static string InsertOPIHistory_DB(string TableName, string CommandData, string ErrorMsg)
        {
            try
            {
                #region 紀錄opi history
                SBCS_OPIHISTORY_TRX _opiHistory = new SBCS_OPIHISTORY_TRX();

                _opiHistory.SESSECTIONID = FormMainMDI.G_OPIAp.SessionID == null ? "" : FormMainMDI.G_OPIAp.SessionID; 
                _opiHistory.OPDATETIME = DateTime.Now;
                _opiHistory.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                _opiHistory.DIRECTION = "OPI-> DB";
                _opiHistory.TRANSACTIONID = string.Empty;
                _opiHistory.MESSAGENAME = TableName;
                _opiHistory.MESSAGETRX = string.Empty;
                _opiHistory.RETURNCODE = string.Empty;
                _opiHistory.RETURNMESSAGE = string.Empty;
                _opiHistory.COMMANDKEY = "DBModify";
                _opiHistory.COMMANDDATA = CommandData;
                _opiHistory.COMMANDTYPE = "DB";
                _opiHistory.PROCESSRESULT = ErrorMsg==string.Empty ? "Success":"NG";
                _opiHistory.PROCESSNGMESSAGE = ErrorMsg;

                FormMainMDI.G_OPIAp.DBCtx.SBCS_OPIHISTORY_TRX.InsertOnSubmit(_opiHistory);
                FormMainMDI.G_OPIAp.DBCtx.SubmitChanges();
                #endregion

                return string.Empty;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite("UniTools", MethodBase.GetCurrentMethod().Name, ex);
                return ex.Message;
            }
        }

        public static string InsertOPIHistory_Trx_Send(DateTime TimeStamp, string MsgName, string TrxID, string MsgXml, string SessionId)
        {
            try
            {
                string _cmdKey = string.Empty;
                string _cmdData = string.Empty;
                string _desc1 = string.Empty;
                string _desc2 = string.Empty;
                int _num=0;

                #region Traction Decode
                switch (MsgName)
                {                    
                    case "DatabaseReloadRequest":

                        #region DatabaseReloadRequest
                        _cmdKey = "DBModify";

                        DatabaseReloadRequest _databaseReload = (DatabaseReloadRequest)Spec.CheckXMLFormat(MsgXml);
                        _cmdData = string.Format("Reload Table [{0}]", _databaseReload.BODY.TABLENAME);
                        break;
                       #endregion

                    case "LineModeChangeRequest":

                        #region LineModeChangeRequest
                        _cmdKey = "LineModeChange";

                        LineModeChangeRequest _lineModeChange = (LineModeChangeRequest)Spec.CheckXMLFormat(MsgXml);
                        _cmdData = string.Format("Line Mode Change to [{0}]", _lineModeChange.BODY.LINEMODE);
                        break;
                       #endregion

                    case "DateTimeCalibrationRequest":

                        #region DateTimeCalibrationRequest
                        _cmdKey = "DateTimeCalibration";

                        DateTimeCalibrationRequest _dateTimeCalibration = (DateTimeCalibrationRequest)Spec.CheckXMLFormat(MsgXml);

                        foreach (DateTimeCalibrationRequest.EQUIPMENTc _dateTimeCalibrationEq in _dateTimeCalibration.BODY.EQUIPMENTLIST)
                        {
                            _desc1 = _desc1 + (_desc1 == string.Empty ? string.Empty : ",") + _dateTimeCalibrationEq.EQUIPMENTNO;
                        }

                        _cmdData = string.Format("DateTime Calibration for Local No [{0}]", _desc1);
                        break;
                        #endregion

                    case "LocalModeCassetteDataSend":

                        #region LocalModeCassetteDataSend

                        _cmdKey = "LocalModeMapDownload";

                        LocalModeCassetteDataSend _localModeMapDownload = (LocalModeCassetteDataSend)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}],Port No [{1}],Port ID [{2}],Cassette ID [{3}]", _localModeMapDownload.BODY.EQUIPMENTNO,_localModeMapDownload.BODY.PORTNO,_localModeMapDownload.BODY.PORTID,_localModeMapDownload.BODY.CASSETTEID);
                        break;
                        #endregion


                    case "OfflineModeCassetteDataSend":

                        #region OfflineModeCassetteDataSend

                        _cmdKey = "OfflineMapDownload";

                        OfflineModeCassetteDataSend _offlineModeMapDownload = (OfflineModeCassetteDataSend)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}] ,Port No [{1}],Port ID [{2}],Cassette ID [{3}]", _offlineModeMapDownload.BODY.EQUIPMENTNO, _offlineModeMapDownload.BODY.PORTNO, _offlineModeMapDownload.BODY.PORTID, _offlineModeMapDownload.BODY.CASSETTEID);
                        break;
                        #endregion

                    case "CassetteCommandRequest":

                        #region CassetteCommandRequest
                        _cmdKey = "CassetteCommand";

                        CassetteCommandRequest _cassetteCommand = (CassetteCommandRequest)Spec.CheckXMLFormat(MsgXml);

                        #region Get Description
                        _desc2 = GetCassetteCommandDesc(_cassetteCommand.BODY.CASSETTECOMMAND);

                        if (_cassetteCommand.BODY.CASSETTECOMMAND == "2")
                            _desc2 = string.Format("{0}, Process Count{1}", _desc2, _cassetteCommand.BODY.PROCESSCOUNT);
                        #endregion

                        _cmdData = string.Format("Local No [{0}], Port No [{1}],Port ID[{2}], Cassette ID[{3}], Cassette Command [{4}]",
                            _cassetteCommand.BODY.EQUIPMENTNO, _cassetteCommand.BODY.PORTNO, _cassetteCommand.BODY.PORTID, _cassetteCommand.BODY.CASSETTEID, _desc2);

                        break;
                        #endregion

                    case "PortCommandRequest":

                        #region PortCommandRequest
                        _cmdKey = "PortCommand";

                        PortCommandRequest _portCommand = (PortCommandRequest)Spec.CheckXMLFormat(MsgXml);

                        #region Get Description
                        int.TryParse(_portCommand.BODY.PORTCOMMAND,out _num);

                        switch (_portCommand.BODY.COMMANDTYPE)
                        {
                            case "TYPE":
                                _desc1 = "Port Type";
                                _desc2 = GetEnumDescription<ePortType>(_num);
                                break;
                            case "MODE":
                                _desc1 = "Port Mode";
                                _desc2 = GetEnumDescription<ePortMode>(_num);
                                break;
                            case "ENABLED":
                                _desc1 = "Port Enable";
                                _desc2 = GetEnumDescription<ePortEnable>(_num);
                                break;
                            case "TRANSFER":
                                _desc1 = "Port Transfer";
                                _desc2 = GetEnumDescription<ePortTransfer>(_num);
                                break;
                        }
                        #endregion

                        _cmdData = string.Format("Local No [{0}], Port No [{1}],Port ID[{2}], Command Type [{3}], Port Command [{4}]",
                            _portCommand.BODY.EQUIPMENTNO, _portCommand.BODY.PORTNO, _portCommand.BODY.PORTID, _desc1, _desc2);

                        break;
                        #endregion

                    case "PortAssignmentCommandRequest":

                        #region PortAssignmentCommandRequest
                        _cmdKey = "PortAssignmentChange";

                        PortAssignmentCommandRequest _portAssignmentCommand = (PortAssignmentCommandRequest)Spec.CheckXMLFormat(MsgXml);

                        #region Get Description
                        foreach (PortAssignmentCommandRequest.ASSIGNMENTc _assignment in _portAssignmentCommand.BODY.ASSIGNMENTLIST)
                        {
                            _cmdData = _cmdData + (_cmdData == string.Empty ? "" : "\r\n") + string.Format("Local No [{0}], Port No [{1}],Port ID[{2}], Assignment [{3}] ",
                               _assignment.EQUIPMENTNO, _assignment.PORTNO, _assignment.PORTID, _assignment.ASSIGNMENT);
                        }

                        #endregion

                        break;
                        #endregion

                    case "CIMMessageCommandRequest":

                        #region CIMMessageCommandRequest
                        _cmdKey = "CIMMessageCommand";

                        CIMMessageCommandRequest _cimMessageCommand = (CIMMessageCommandRequest)Spec.CheckXMLFormat(MsgXml);

                        if (FormMainMDI.G_OPIAp.CurLine.FabType == "CELL")
                        {
                            _cmdData = string.Format("Local No [{0}], Touch Panel No [{1}],Message ID[{2}], Command[{3}], Message Text [{4}]",
                                _cimMessageCommand.BODY.EQUIPMENTNO, _cimMessageCommand.BODY.TOUCHPANELNO, _cimMessageCommand.BODY.MESSAGEID, _cimMessageCommand.BODY.COMMAND, _cimMessageCommand.BODY.MESSAGETEXT);
                        }
                        else
                        {
                            _cmdData = string.Format("Local No [{0}], Message ID[{1}], Command[{2}], Message Text [{3}]",
                                _cimMessageCommand.BODY.EQUIPMENTNO, _cimMessageCommand.BODY.MESSAGEID, _cimMessageCommand.BODY.COMMAND, _cimMessageCommand.BODY.MESSAGETEXT); 
                        }

                        break;
                        #endregion

                    case "CIMModeChangeRequest":

                        #region CIMModeChangeRequest
                        _cmdKey = "CIMModeChange";

                        CIMModeChangeRequest _cimModeChange = (CIMModeChangeRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], CIM Mode[{1}]",
                            _cimModeChange.BODY.EQUIPMENTNO, _cimModeChange.BODY.CIMMODE);
                        
                        break;
                        #endregion

                    case "IncompleteCassetteCommand":

                        #region IncompleteCassetteCommand
                        _cmdKey = "IncompleteCassette";

                        IncompleteCassetteCommand _incompleteCassette = (IncompleteCassetteCommand)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("[{5}] Incomplete Date [{0}], Port ID[{1}], Cassette ID[{2}], MES Trx ID [{3}], File Name [{4}] ",
                            _incompleteCassette.BODY.INCOMPLETEDATE, _incompleteCassette.BODY.PORTID, _incompleteCassette.BODY.CASSETTEID, _incompleteCassette.BODY.MESTRXID,_incompleteCassette.BODY.FILENAME,_incompleteCassette.BODY.COMMAND);

                        break;
                        #endregion

                    case "SamplingRuleChangeCommand":

                        #region SamplingRuleChangeCommand
                        _cmdKey = "SamplingRuleChange";

                        SamplingRuleChangeCommand _samplingRuleChange = (SamplingRuleChangeCommand)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Sampling Rule [{1}], Sampling Unit [{2}], Side Information [{3}]",
                            _samplingRuleChange.BODY.EQUIPMENTNO, GetSamplingRuleDesc(_samplingRuleChange.BODY.SAMPLINGRULE), _samplingRuleChange.BODY.SAMPLINGUNIT, _samplingRuleChange.BODY.SIDEINFORMATION);

                        break;
                        #endregion

                    case "EquipmentRunModeSetCommand":

                        #region EquipmentRunModeSetCommand
                        _cmdKey = "EquipmentRunModeSet";

                        EquipmentRunModeSetCommand _equipmentRunModeSet = (EquipmentRunModeSetCommand)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Run Mode Change[{1}]",
                            _equipmentRunModeSet.BODY.EQUIPMENTNO, GetEquipmentRunModeDesc(_equipmentRunModeSet.BODY.EQUIPMENTNO,_equipmentRunModeSet.BODY.COMMAND));

                        break;
                        #endregion

                    case "BCControlCommand":

                        #region BCControlCommand
                        _cmdKey = "BCControlCommand";

                        BCControlCommand _bcControlCommand = (BCControlCommand)Spec.CheckXMLFormat(MsgXml);

                        switch (_bcControlCommand.BODY.COMMANDTYPE)
                        {
                            case "PROCESSPAUSE":
                                _desc1 = "Process Pause Command";

                                if (_bcControlCommand.BODY.COMMAND == "1") _desc2 = "1:Pause";
                                else if (_bcControlCommand.BODY.COMMAND == "2") _desc2 = "2:Resume";
                                else _desc2 = _bcControlCommand.BODY.COMMAND;
                                break ;

                            case "TRANSFERSTOP":
                                _desc1 = "Transfer Stop Command";

                                if (_bcControlCommand.BODY.COMMAND == "1") _desc2 = "1:Stop";
                                else if (_bcControlCommand.BODY.COMMAND == "2") _desc2 = "2:Resume";
                                else _desc2 = _bcControlCommand.BODY.COMMAND;
                                break ;

                            case "PROCESSSTOP":
                                _desc1 = "Process Stop Command";

                                if (_bcControlCommand.BODY.COMMAND == "1") _desc2 = "1:Stop";
                                else if (_bcControlCommand.BODY.COMMAND == "2") _desc2 = "2:Run";
                                else _desc2 = _bcControlCommand.BODY.COMMAND;
                                break ;

                            default :
                                _desc1 = _bcControlCommand.BODY.COMMANDTYPE;
                                _desc2 = _bcControlCommand.BODY.COMMAND;
                                break ;
                        }

                        _cmdData = string.Format("[{2}] - Set [{3}] to Local No [{0}], Unit No [{1}]",
                            _bcControlCommand.BODY.EQUIPMENTNO, _bcControlCommand.BODY.UNITNO, _desc1, _desc2);

                        break;
                        #endregion

                    case "ForceCleanOutCommand":

                        #region ForceCleanOutCommand
                        _cmdKey = "ForceCleanOutCommand";

                        ForceCleanOutCommand _forceCleanOutCommand = (ForceCleanOutCommand)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("{0} Set Status[{1}]",
                            _forceCleanOutCommand.BODY.COMMAND, _forceCleanOutCommand.BODY.STATUS);

                        break;
                        #endregion

                    case "RobotOperationModeCommand":

                        #region RobotOperationModeCommand
                        _cmdKey = "RobotOperationMode";

                        RobotOperationModeCommand _robotOperationMode = (RobotOperationModeCommand)Spec.CheckXMLFormat(MsgXml);

                        switch (_robotOperationMode.BODY.OPERATIONMODE)
                        {
                            case "1": _desc1 = "1:Normal Mode"; break ;
                            case "2": _desc1 = "2:D(Dual) Mode"; break ;
                            case "3": _desc1 = "3:S(Single) Mode"; break ;
                            default : _desc1 =_robotOperationMode.BODY.OPERATIONMODE; break ;
                        }

                        _cmdData = string.Format("Local No {0}, Robot Position No [{1}] Set Operation Mode[{2}]",
                            _robotOperationMode.BODY.EQUIPMENTNO, _robotOperationMode.BODY.ROBOTPOSITIONNO, _desc1);

                        break;
                        #endregion

                    case "EquipmentFetchGlassCommand":

                        #region EquipmentFetchGlassCommand
                        _cmdKey = "EquipmentFetchGlass";

                        EquipmentFetchGlassCommand _equipmentFetchGlass = (EquipmentFetchGlassCommand)Spec.CheckXMLFormat(MsgXml);
                        
                        _cmdData = string.Format("Local No {0} Set Rule Name1 : {1} - [{2}] ; Rule Name2 : {3} - [{4}]",
                            _equipmentFetchGlass.BODY.EQUIPMENTNO, GetProportionalNameDesc(_equipmentFetchGlass.BODY.RULENAME1),_equipmentFetchGlass.BODY.RULEVALUE1,
                            GetProportionalNameDesc(_equipmentFetchGlass.BODY.RULENAME2),_equipmentFetchGlass.BODY.RULEVALUE2);

                        break;
                        #endregion

                    case "RobotModeChangeRequest":

                        #region RobotModeChangeRequest
                        _cmdKey = "RobotModeChange";

                        RobotModeChangeRequest _robotModeChange = (RobotModeChangeRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Robot Name [{1}] Change to [{2}]",
                            _robotModeChange.BODY.EQUIPMENTNO, _robotModeChange.BODY.ROBOTNAME, _robotModeChange.BODY.ROBOTMODE);

                        break;
                        #endregion

                    case "RobotCommandRequest":

                        #region RobotCommandRequest
                        _cmdKey = "RobotCommand";

                        RobotCommandRequest _robotSemiCommand = (RobotCommandRequest)Spec.CheckXMLFormat(MsgXml);
                       
                        foreach (RobotCommandRequest.COMMANDc _cmd in _robotSemiCommand.BODY.COMMANDLIST)
                        {
                            _cmdData = _cmdData + (_cmdData == string.Empty ? string.Empty : ";") + string.Format(" {0} Set Command [{1}] Arm[{2}] Position [{3}],Slot No [{4}] ", _cmd.COMMAND_SEQ, _cmd.ROBOT_COMMAND, _cmd.ARM_SELECT, _cmd.TARGETPOSITION, _cmd.TARGETSLOTNO);
                        }

                        _cmdData = string.Format("Local No [{0}], Robot Name [{1}] Set ", _robotSemiCommand.BODY.EQUIPMENTNO, _robotSemiCommand.BODY.ROBOTNAME) + _cmdData;

                        break;
                        #endregion

                    case "IndexerOperationModeChangeRequest":

                        #region IndexerOperationModeChangeRequest
                        _cmdKey = "IndexerOperationMode";

                        IndexerOperationModeChangeRequest _indexerOperationMode = (IndexerOperationModeChangeRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Indexer Operation Mode Change[{1}]",
                            _indexerOperationMode.BODY.EQUIPMENTNO, GetEquipmentIndexerModeDesc(_indexerOperationMode.BODY.INDEXEROPERATIONMODE));

                        break;
                        #endregion

                    case "CoolRunSetRequest":

                        #region CoolRunSetRequest
                        _cmdKey = "CoolRunSet";

                        CoolRunSetRequest _coolRunSet = (CoolRunSetRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Cool Run Set [{0}]",
                            _coolRunSet.BODY.COOLRUNCOUNT);

                        break;
                        #endregion

                    case "VCRStatusChangeRequest":

                        #region VCRStatusChangeRequest
                        _cmdKey = "VCRStatusChange";

                        VCRStatusChangeRequest _vcrStatusChange = (VCRStatusChangeRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], VCR No [{1}] Change to [{2}]",
                            _vcrStatusChange.BODY.EQUIPMENTNO, _vcrStatusChange.BODY.VCRNO, _vcrStatusChange.BODY.VCRMODE);

                        break;
                        #endregion

                    case "MPLCInterlockChangeRequest":

                        #region MPLCInterlockChangeRequest
                        _cmdKey = "MPLCInterlockChange";

                        MPLCInterlockChangeRequest _mplcInterlockChange = (MPLCInterlockChangeRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], MPLC Interlock No [{1}] Change to [{2}]",
                            _mplcInterlockChange.BODY.EQUIPMENTNO, _mplcInterlockChange.BODY.MPLCINTERLOCKNO, _mplcInterlockChange.BODY.MPLCINTERLOCK);

                        break;
                        #endregion

                    case "SECSFunctionRequest":

                        #region SECSFunctionRequest
                        _cmdKey = "SECSFunction";

                        SECSFunctionRequest _secsFunction = (SECSFunctionRequest)Spec.CheckXMLFormat(MsgXml);

                        foreach (SECSFunctionRequest.PARAMETERc _param in _secsFunction.BODY.PARAMETERLIST)
                        {
                            _desc1 = _desc1 + (_desc1 == string.Empty ? string.Empty : ";") + string.Format("{0} - {1}",_param.NAME,_param.VALUE);                            
                        }

                        if (_desc1 == string.Empty )
                            _cmdData = string.Format("Local No [{0}], SENS Name [{1}] ",
                                _secsFunction.BODY.EQUIPMENTNO, _secsFunction.BODY.SECSNAME);
                        else
                            _cmdData = string.Format("Local No [{0}], SENS Name [{1}] Set {2}",
                                _secsFunction.BODY.EQUIPMENTNO, _secsFunction.BODY.SECSNAME, _desc1);

                        break;
                        #endregion

                    case "CassetteOnPortQTimeRequest":

                        #region CassetteOnPortQTimeRequest
                        _cmdKey = "CassetteOnPortQTime";

                        CassetteOnPortQTimeRequest _cassetteOnPortQTime = (CassetteOnPortQTimeRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}] Cassette On Port QTime Change to [{1}]",
                            _cassetteOnPortQTime.BODY.EQUIPMENTNO, _cassetteOnPortQTime.BODY.QTIME);

                        break;
                        #endregion

                    case "InspectionIdleTimeRequest":

                        #region InspectionIdleTimeRequest
                        _cmdKey = "InspectionIdleTime";

                        InspectionIdleTimeRequest _inspectionIdleTime = (InspectionIdleTimeRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Inspection Idle Time Change to [{1}]",
                            _inspectionIdleTime.BODY.EQUIPMENTNO, _inspectionIdleTime.BODY.IDLETIME);

                        break;
                        #endregion

                    case "CreateInspFileRequest":

                        #region CreateInspFileRequest
                        _cmdKey = "CreateInspFile";

                        CreateInspFileRequest _createInspFile = (CreateInspFileRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Cassette Seq No [{0}], Job Seq No [{1}] Create File",
                            _createInspFile.BODY.CASSETTESEQNO, _createInspFile.BODY.JOBSEQNO);

                        break;
                        #endregion

                    case "DisconnectUsersRequest":

                        #region DisconnectUsersRequest
                        _cmdKey = "DisconnectUsers";

                        DisconnectUsersRequest _disconnectUsers = (DisconnectUsersRequest)Spec.CheckXMLFormat(MsgXml);

                        foreach (DisconnectUsersRequest.USERc _user in _disconnectUsers.BODY.USERLIST)
                        {
                            _desc1 = _desc1 + (_desc1 == string.Empty ? string.Empty : ";") + string.Format("Group [{0}] - {1} in {2} Login at {3}", _user.USERGROUP, _user.USERID, _user.LOGINSERVERIP, _user.LOGINTIME);
                        }

                        _cmdData = string.Format("Disconnection User for {0}",
                           _desc1);

                        break;
                        #endregion

                    case "ClientDisconnectReply":

                        #region ClientDisconnectReply
                        _cmdKey = "ClientDisconnect";

                        ClientDisconnectReply _clientDisconnect = (ClientDisconnectReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("User ID [{0}], Login Server IP [{1}]",
                            _clientDisconnect.BODY.USERID, _clientDisconnect.BODY.LOGINSERVERIP);

                        break;
                        #endregion

                    case "EquipmentReportSettingRequest":

                        #region EquipmentReportSettingRequest
                        _cmdKey = "EQReportSetting";

                        EquipmentReportSettingRequest _eqReportSetting = (EquipmentReportSettingRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Report Type [{1}] Change to [{2}] - [{3}]",
                            _eqReportSetting.BODY.EQUIPMENTNO, _eqReportSetting.BODY.REPORTTYPE, _eqReportSetting.BODY.REPORTENABLE, _eqReportSetting.BODY.REPORTTIME);

                        break;
                        #endregion

                    case "PPKLocalModeDenseDataSend":

                        #region LocalModeDenseDataSend
                        _cmdKey = "PPK DenseMapDownload";

                        PPKLocalModeDenseDataSend _PPKlocalDSMapDownload = (PPKLocalModeDenseDataSend)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Port No [{1}]",
                            _PPKlocalDSMapDownload.BODY.EQUIPMENTNO, _PPKlocalDSMapDownload.BODY.PORTNO);

                        break;
                        #endregion

                    case "PPKOfflineDenseDataSend":

                        #region PPKOfflineDenseDataSend
                        _cmdKey = "PPK DenseMapDownload";

                        PPKOfflineDenseDataSend _PPKofflineDSMapDownload = (PPKOfflineDenseDataSend)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Port No [{1}] - Box ID [{2}]",
                            _PPKofflineDSMapDownload.BODY.EQUIPMENTNO, _PPKofflineDSMapDownload.BODY.PORTNO, _PPKofflineDSMapDownload.BODY.BOXID);

                        break;
                        #endregion

                    case "LocalModeDenseDataSend":

                        #region LocalModeDenseDataSend
                        _cmdKey = "DenseMapDownload";

                        LocalModeDenseDataSend _localDSMapDownload = (LocalModeDenseDataSend)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Port No [{1}]",
                            _localDSMapDownload.BODY.EQUIPMENTNO, _localDSMapDownload.BODY.PORTNO);

                        break;
                        #endregion

                    case "OfflineDenseDataSend":

                        #region OfflineDenseDataSend
                        _cmdKey = "DenseMapDownload";

                        OfflineDenseDataSend _offlineDSMapDownload = (OfflineDenseDataSend)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Port No [{1}] - Box ID01 [{2}], Box ID02 [{3}]",
                            _offlineDSMapDownload.BODY.EQUIPMENTNO, _offlineDSMapDownload.BODY.PORTNO, _offlineDSMapDownload.BODY.BOXID01, _offlineDSMapDownload.BODY.BOXID02);

                        break;
                        #endregion

                    case "LocalModePalletDataSend":

                        #region LocalModePalletDataSend
                        _cmdKey = "PalletMapDownload";

                        LocalModePalletDataSend _localPalletMapDownload = (LocalModePalletDataSend)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Pallet No [{0}], Pallet ID [{1}]",
                            _localPalletMapDownload.BODY.PALLETNO, _localPalletMapDownload.BODY.PALLETID);

                        break;
                        #endregion

                    case "OfflinePalletDataSend":

                        #region OfflinePalletDataSend
                        _cmdKey = "PalletMapDownload";

                        OfflinePalletDataSend _offlinePalletMapDownload = (OfflinePalletDataSend)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Pallet No [{0}], Pallet ID [{1}]",
                            _offlinePalletMapDownload.BODY.PALLETNO, _offlinePalletMapDownload.BODY.PALLETID);

                        break;
                        #endregion

                    case "JobDataOperationRequest":

                        #region JobDataOperationRequest
                        _cmdKey = "JobDataModify";

                        JobDataOperationRequest _jobDataDelete = (JobDataOperationRequest)Spec.CheckXMLFormat(MsgXml);

                        foreach (JobDataOperationRequest.JOBDATAc _job in _jobDataDelete.BODY.JOBDATALIST)
                        {
                            _desc1 = _desc1 + (_desc1 == string.Empty ? string.Empty : " ; ") +
                                string.Format("Cassette Seq No [{0}], Job Seq No [{1}], Glass ID [{2}]", _job.CASSETTESEQNO, _job.JOBSEQNO, _job.GLASSID);      
                        }

                        _cmdData = string.Format("[{0}] - {1}",
                            _jobDataDelete.BODY.COMMAND, _desc1);

                        break;
                        #endregion

                    case "ChangerPlanDownloadSetCommandRequest":

                        #region ChangerPlanDownloadSetCommandRequest
                        _cmdKey = "ChangerPlanDownload";

                        ChangerPlanDownloadSetCommandRequest _changerPlanDownloadSetCommand = (ChangerPlanDownloadSetCommandRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Return Code [{0}], Plan ID [{1}]",
                            _changerPlanDownloadSetCommand.BODY.RETURNCODE, _changerPlanDownloadSetCommand.BODY.PLANID);

                        break;
                        #endregion

                    case "ODFTrackTimeSettingChangeRequest":

                        #region ODFTrackTimeSettingChangeRequest
                        _cmdKey = "ODFTrackTimeSetting";

                        ODFTrackTimeSettingChangeRequest _odfTrackTimeSetting = (ODFTrackTimeSettingChangeRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}] Modiyf",
                            _odfTrackTimeSetting.BODY.EQUIPMENTNO);

                        break;
                        #endregion

                    case "UnitRunModeChangeRequest":

                        #region UnitRunModeChangeRequest
                        _cmdKey = "UnitRunModeChange";

                        UnitRunModeChangeRequest _unitRunModeChangeRequest = (UnitRunModeChangeRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}] - [{1}]", _unitRunModeChangeRequest.BODY.EQUIPMENTNO, _unitRunModeChangeRequest.BODY.NEW_RUNMODE);

                        foreach (UnitRunModeChangeRequest.UNITc _unit in _unitRunModeChangeRequest.BODY.UNITLIST)
                        {
                            _cmdData = _cmdData + string.Format("{0}[{1}] ", _unit.UNITNO, _unit.NEW_RUNMODE);
                        }
                        break;
                        #endregion

                    case "InspectionFlowPriorityChangeRequest":

                        #region InspectionFlowPriorityChangeRequest
                        _cmdKey = "InspectionFlowPriorityChange";

                        InspectionFlowPriorityChangeRequest _inspectionFlowPriorityChangeRequest = (InspectionFlowPriorityChangeRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Port No[{1}] Flow Priority [{3}] ",
                            _inspectionFlowPriorityChangeRequest.BODY.EQUIPMENTNO, _inspectionFlowPriorityChangeRequest.BODY.PORTNO, _inspectionFlowPriorityChangeRequest.BODY.PRIORITY);

                        break;
                        #endregion

                    default :
                        return string.Empty;
                }
                #endregion

                #region 紀錄opi history
                SBCS_OPIHISTORY_TRX _opiHistory = new SBCS_OPIHISTORY_TRX();
                _opiHistory.SESSECTIONID = SessionId == null ? "" : SessionId;
                _opiHistory.OPDATETIME = TimeStamp;
                _opiHistory.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                _opiHistory.DIRECTION = "OPI->BCS";
                _opiHistory.TRANSACTIONID = TrxID;
                _opiHistory.MESSAGENAME = MsgName;
                _opiHistory.MESSAGETRX = MsgXml;
                _opiHistory.RETURNCODE = string.Empty;
                _opiHistory.RETURNMESSAGE = string.Empty;
                _opiHistory.COMMANDKEY = _cmdKey;
                _opiHistory.COMMANDDATA = _cmdData;
                _opiHistory.COMMANDTYPE = "TRX";
                _opiHistory.PROCESSRESULT = string.Empty;
                _opiHistory.PROCESSNGMESSAGE = string.Empty;

                FormMainMDI.G_OPIAp.DBCtx.SBCS_OPIHISTORY_TRX.InsertOnSubmit(_opiHistory);
                FormMainMDI.G_OPIAp.DBCtx.SubmitChanges();

                #endregion

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string InsertOPIHistory_Trx_Receive(DateTime TimeStamp, string MsgName, string TrxID,  string MsgXml, string SessionId)
        {
            try
            {
                string _cmdKey = string.Empty;
                string _cmdData = string.Empty;
                string _desc1 = string.Empty;
                string _desc2 = string.Empty;

                #region Traction Decode
                switch (MsgName)
                {
                    case "DatabaseReloadReply":

                        #region DatabaseReloadReply
                        _cmdKey = "DBModify";
                        DatabaseReloadReply _databaseReload = (DatabaseReloadReply)Spec.CheckXMLFormat(MsgXml);
                        _cmdData = string.Format("Reload Table [{0}]", _databaseReload.BODY.TABLENAME);
                        break;
                        #endregion

                    case "LineModeChangeReply":

                        #region LineModeChangeReply
                        _cmdKey = "LineModeChange";

                        LineModeChangeReply _lineModeChange = (LineModeChangeReply)Spec.CheckXMLFormat(MsgXml);
                        _cmdData = string.Format("Line Mode Change Reply");
                        break;
                        #endregion

                    case "DateTimeCalibrationReply":

                        #region DateTimeCalibrationReply
                        _cmdKey = "DateTimeCalibration";

                        DateTimeCalibrationReply _dateTimeCalibration = (DateTimeCalibrationReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("DateTime Calibration Reply");
                        break;
                        #endregion

                    case "LocalModeCassetteDataSendReply":

                        #region LocalModeCassetteDataSendReply

                        _cmdKey = "LocalModeMapDownload";

                        LocalModeCassetteDataSendReply _localModeMapDownload = (LocalModeCassetteDataSendReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}],Port No [{1}],Port ID [{2}],Cassette ID [{3}]", _localModeMapDownload.BODY.EQUIPMENTNO, _localModeMapDownload.BODY.PORTNO, _localModeMapDownload.BODY.PORTID, _localModeMapDownload.BODY.CASSETTEID);
                        break;
                        #endregion

                    case "OfflineModeCassetteDataSendReply":

                        #region OfflineModeCassetteDataSendReply

                        _cmdKey = "OfflineMapDownload";

                        OfflineModeCassetteDataSendReply _offlineModeMapDownload = (OfflineModeCassetteDataSendReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}] ,Port No [{1}],Port ID [{2}],Cassette ID [{3}]", _offlineModeMapDownload.BODY.EQUIPMENTNO, _offlineModeMapDownload.BODY.PORTNO, _offlineModeMapDownload.BODY.PORTID, _offlineModeMapDownload.BODY.CASSETTEID);
                        break;
                        #endregion

                    case "CassetteMapDownloadResultReport":

                        #region CassetteMapDownloadResultReport

                        _cmdKey = "MapDownloadResult";

                        CassetteMapDownloadResultReport _mapDownloadResult = (CassetteMapDownloadResultReport)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}] ,Port No [{1}],Port ID [{2}],Cassette ID [{3}]", _mapDownloadResult.BODY.EQUIPMENTNO, _mapDownloadResult.BODY.PORTNO, _mapDownloadResult.BODY.PORTID, _mapDownloadResult.BODY.CASSETTEID);
                        break;
                        #endregion

                    case "CassetteCommandReply":

                        #region CassetteCommandReply
                        _cmdKey = "CassetteCommand";

                        CassetteCommandReply _cassetteCommand = (CassetteCommandReply)Spec.CheckXMLFormat(MsgXml);

                        #region Get Description
                        _desc2 = GetCassetteCommandDesc(_cassetteCommand.BODY.CASSETTECOMMAND);

                        if (_cassetteCommand.BODY.CASSETTECOMMAND =="2")
                            _desc2 = string.Format("{0}, Process Count{1}", _desc2, _cassetteCommand.BODY.PROCESSCOUNT);
                        #endregion

                        _cmdData = string.Format("Local No [{0}], Port No [{1}],Port ID[{2}], Cassette ID[{3}], Cassette Command [{4}]",
                            _cassetteCommand.BODY.EQUIPMENTNO, _cassetteCommand.BODY.PORTNO, _cassetteCommand.BODY.PORTID, _cassetteCommand.BODY.CASSETTEID, _desc2);

                        break;
                        #endregion

                    case "PortCommandReply":

                        #region PortCommandReply
                        _cmdKey = "PortCommand";

                        PortCommandReply _portCommand = (PortCommandReply)Spec.CheckXMLFormat(MsgXml);


                        _cmdData = string.Format("Local No [{0}], Port No [{1}],Port ID[{2}], Port Command [{3}]",
                            _portCommand.BODY.EQUIPMENTNO, _portCommand.BODY.PORTNO, _portCommand.BODY.PORTID, _portCommand.BODY.PORTCOMMAND);


                        break;
                        #endregion

                    case "PortAssignmentCommandReply":

                        #region PortAssignmentCommandReply
                        _cmdKey = "PortAssignmentChange";

                        PortAssignmentCommandReply _portAssignmentCommand = (PortAssignmentCommandReply)Spec.CheckXMLFormat(MsgXml);


                        _cmdData = string.Format("Server Name [{0}]", _portAssignmentCommand.BODY.LINENAME);


                        break;
                        #endregion

                    case "CIMMessageCommandReply":

                        #region CIMMessageCommandReply
                        _cmdKey = "CIMMessageCommand";

                        CIMMessageCommandReply _cimMessageCommand = (CIMMessageCommandReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Message ID[{1}]",
                            _cimMessageCommand.BODY.EQUIPMENTNO, _cimMessageCommand.BODY.MESSAGEID);

                        break;
                        #endregion

                    case "CIMModeChangeReply":

                        #region CIMModeChangeReply
                        _cmdKey = "CIMModeChange";

                        CIMModeChangeReply _cimModeChange = (CIMModeChangeReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], CIM Mode[{1}]",
                            _cimModeChange.BODY.EQUIPMENTNO, _cimModeChange.BODY.CIMMODE);

                        break;
                        #endregion

                    case "IncompleteCassetteCommandReply":

                        #region IncompleteCassetteCommandReply
                        _cmdKey = "IncompleteCassette";

                        IncompleteCassetteCommandReply _incompleteCassette = (IncompleteCassetteCommandReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Incomplete Date [{0}], Port ID[{1}], Cassette ID[{2}]",
                            _incompleteCassette.BODY.INCOMPLETEDATE, _incompleteCassette.BODY.PORTID, _incompleteCassette.BODY.CASSETTEID);

                        break;
                        #endregion

                    case "SamplingRuleChangeCommandReply":

                        #region SamplingRuleChangeCommandReply
                        _cmdKey = "SamplingRuleChange";

                        SamplingRuleChangeCommandReply _samplingRuleChange = (SamplingRuleChangeCommandReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}]",_samplingRuleChange.BODY.EQUIPMENTNO);

                        break;
                        #endregion

                    case "EquipmentRunModeSetCommandReply":

                        #region EquipmentRunModeSetCommandReply
                        _cmdKey = "EquipmentRunModeSet";

                        EquipmentRunModeSetCommandReply _equipmentRunModeSet = (EquipmentRunModeSetCommandReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}]",
                            _equipmentRunModeSet.BODY.EQUIPMENTNO);

                        break;
                        #endregion

                    case "BCControlCommandReply":

                        #region BCControlCommandReply
                        _cmdKey = "BCControlCommand";

                        BCControlCommandReply _bcControlCommand = (BCControlCommandReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Unit No [{1}]",
                            _bcControlCommand.BODY.EQUIPMENTNO, _bcControlCommand.BODY.UNITNO);

                        break;
                        #endregion

                    case "ForceCleanOutCommandReply":

                        #region ForceCleanOutCommandReply
                        _cmdKey = "ForceCleanOutCommand";

                        //ForceCleanOutCommandReply _forceCleanOutCommand = (ForceCleanOutCommandReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Empty ;

                        break;
                        #endregion

                    case "RobotOperationModeCommandReply":

                        #region RobotOperationModeCommandReply
                        _cmdKey = "RobotOperationMode";

                        RobotOperationModeCommandReply _robotOperationMode = (RobotOperationModeCommandReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No {0} Reply",
                            _robotOperationMode.BODY.EQUIPMENTNO);

                        break;
                        #endregion

                    //case "GlassGradeMappingCommandReply":

                    //    #region GlassGradeMappingCommandReply
                    //    _cmdKey = "GlassGradeMapping";

                    //    GlassGradeMappingCommandReply _glassGradeMapping = (GlassGradeMappingCommandReply)Spec.CheckXMLFormat(MsgXml);

                    //    _cmdData = string.Format("Local No [{0}] Reply",
                    //        _glassGradeMapping.BODY.EQUIPMENTNO);

                    //    break;
                    //    #endregion

                    case "RobotModeChangeReply":

                        #region RobotModeChangeReply
                        _cmdKey = "RobotModeChange";

                        RobotModeChangeReply _robotModeChange = (RobotModeChangeReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Robot Name [{1}] Change to [{2}]",
                            _robotModeChange.BODY.EQUIPMENTNO, _robotModeChange.BODY.ROBOTNAME, _robotModeChange.BODY.ROBOTMODE);

                        break;
                        #endregion

                    case "RobotCommandReply":

                        #region RobotCommandReply
                        _cmdKey = "RobotCommand";

                        RobotCommandReply _robotSemiCommand = (RobotCommandReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Robot Name [{1}] Reply",
                            _robotSemiCommand.BODY.EQUIPMENTNO, _robotSemiCommand.BODY.ROBOTNAME);

                        break;
                        #endregion

                    case "IndexerOperationModeChangeReply":

                        #region IndexerOperationModeChangeReply
                        _cmdKey = "IndexerOperationMode";

                        IndexerOperationModeChangeReply _indexerOperationMode = (IndexerOperationModeChangeReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}] Reply",
                            _indexerOperationMode.BODY.EQUIPMENTNO);

                        break;
                        #endregion

                    case "CoolRunSetReply":

                        #region CoolRunSetReply
                        _cmdKey = "CoolRunSet";

                        //CoolRunSetReply _coolRunSet = (CoolRunSetReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Cool Run Set Reply");

                        break;
                        #endregion

                    case "VCRStatusChangeReply":

                        #region VCRStatusChangeReply
                        _cmdKey = "VCRStatusChange";

                        VCRStatusChangeReply _vcrStatusChange = (VCRStatusChangeReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], VCR No [{1}] Change to [{2}]",
                            _vcrStatusChange.BODY.EQUIPMENTNO, _vcrStatusChange.BODY.VCRNO, _vcrStatusChange.BODY.VCRMODE);

                        break;
                        #endregion

                    case "MPLCInterlockChangeReply":

                        #region MPLCInterlockChangeReply
                        _cmdKey = "MPLCInterlockChange";

                        MPLCInterlockChangeReply _mplcInterlockChange = (MPLCInterlockChangeReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], MPLC Interlock No [{1}] Change to [{2}]",
                            _mplcInterlockChange.BODY.EQUIPMENTNO, _mplcInterlockChange.BODY.MPLCINTERLOCKNO, _mplcInterlockChange.BODY.MPLCINTERLOCK);

                        break;
                        #endregion

                    case "SECSFunctionReply":

                        #region SECSFunctionReply
                        _cmdKey = "SECSFunction";

                        SECSFunctionReply _secsFunction = (SECSFunctionReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], SENS Name [{1}] ",
                            _secsFunction.BODY.EQUIPMENTNO, _secsFunction.BODY.SECSNAME);

                        break;
                        #endregion

                    case "CassetteOnPortQTimeReply":

                        #region CassetteOnPortQTimeReply
                        _cmdKey = "CassetteOnPortQTime";

                        CassetteOnPortQTimeReply _cassetteOnPortQTime = (CassetteOnPortQTimeReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}] Cassette On Port QTime Reply",
                            _cassetteOnPortQTime.BODY.EQUIPMENTNO);

                        break;
                        #endregion

                    case "InspectionIdleTimeReply":

                        #region InspectionIdleTimeReply
                        _cmdKey = "InspectionIdleTime";

                        InspectionIdleTimeReply _inspectionIdleTime = (InspectionIdleTimeReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Inspection Idle Time Reply",
                            _inspectionIdleTime.BODY.EQUIPMENTNO);

                        break;
                        #endregion

                    case "CreateInspFileReply":

                        #region CreateInspFileReply
                        _cmdKey = "CreateInspFile";

                        CreateInspFileReply _createInspFile = (CreateInspFileReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Cassette Seq No [{0}], Job Seq No [{1}] Create File",
                            _createInspFile.BODY.CASSETTESEQNO, _createInspFile.BODY.JOBSEQNO);

                        break;
                        #endregion

                    case "DisconnectUsersReply":

                        #region DisconnectUsersReply
                        _cmdKey = "DisconnectUsers";

                        //DisconnectUsersReply _disconnectUsers = (DisconnectUsersReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Disconnection User Reply");

                        break;
                        #endregion

                    case "ClientDisconnectRequest":

                        #region ClientDisconnectRequest
                        _cmdKey = "ClientDisconnect";

                        ClientDisconnectRequest _clientDisconnect = (ClientDisconnectRequest)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("User ID [{0}], Login Server IP [{1}], User Group [{2}], Login Time [{3}]",
                            _clientDisconnect.BODY.USERID, _clientDisconnect.BODY.LOGINSERVERIP,_clientDisconnect.BODY.USERGROUP,_clientDisconnect.BODY.LOGINTIME);

                        break;
                        #endregion

                    case "EquipmentReportSettingRequestReply":

                        #region EquipmentReportSettingRequestReply
                        _cmdKey = "EQReportSetting";

                        EquipmentReportSettingRequestReply _eqReportSetting = (EquipmentReportSettingRequestReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}] Reply",
                            _eqReportSetting.BODY.EQUIPMENTNO);

                        break;
                        #endregion

                    case "PPKLocalModeDenseDataSendReply":

                        #region PPKLocalModeDenseDataSendReply
                        _cmdKey = "PPK DenseMapDownload";

                        PPKLocalModeDenseDataSendReply _PPKlocalDSMapDownload = (PPKLocalModeDenseDataSendReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Port No [{1}]",
                            _PPKlocalDSMapDownload.BODY.EQUIPMENTNO, _PPKlocalDSMapDownload.BODY.PORTNO);

                        break;
                        #endregion

                    case "PPKOfflineDenseDataSendReply":

                        #region PPKOfflineDenseDataSendReply
                        _cmdKey = "PPK DenseMapDownload";

                        PPKOfflineDenseDataSendReply _PPKofflineDSMapDownload = (PPKOfflineDenseDataSendReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Port No [{1}]",
                            _PPKofflineDSMapDownload.BODY.EQUIPMENTNO, _PPKofflineDSMapDownload.BODY.PORTNO);

                        break;
                        #endregion

                    case "LocalModeDenseDataSendReply":

                        #region LocalModeDenseDataSendReply
                        _cmdKey = "DenseMapDownload";

                        LocalModeDenseDataSendReply _localDSMapDownload = (LocalModeDenseDataSendReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Port No [{1}]",
                            _localDSMapDownload.BODY.EQUIPMENTNO, _localDSMapDownload.BODY.PORTNO);

                        break;
                        #endregion

                    case "OfflineDenseDataSendReply":

                        #region OfflineDenseDataSendReply
                        _cmdKey = "DenseMapDownload";

                        OfflineDenseDataSendReply _offlineDSMapDownload = (OfflineDenseDataSendReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}], Port No [{1}]",
                            _offlineDSMapDownload.BODY.EQUIPMENTNO, _offlineDSMapDownload.BODY.PORTNO);

                        break;
                        #endregion

                    case "LocalModePalletDataSendReply":

                        #region LocalModePalletDataSendReply
                        _cmdKey = "PalletMapDownload";

                        LocalModePalletDataSendReply _localPalletMapDownload = (LocalModePalletDataSendReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Pallet No [{0}]",
                            _localPalletMapDownload.BODY.PALLETNO);

                        break;
                        #endregion

                    case "OfflinePalletDataSendReply":

                        #region OfflinePalletDataSendReply
                        _cmdKey = "PalletMapDownload";

                        OfflinePalletDataSendReply _offlinePalletMapDownload = (OfflinePalletDataSendReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Pallet No [{0}]",
                            _offlinePalletMapDownload.BODY.PALLETNO);

                        break;
                        #endregion

                    //case "OperationPermissionReply":

                    //    #region OperationPermissionReply
                    //    _cmdKey = "ATSOperationRunMode";

                    //    OperationPermissionReply _operationPermissionRequest = (OperationPermissionReply)Spec.CheckXMLFormat(MsgXml);

                    //    _cmdData = string.Format("Local No [{0}] Operator Permission Command Reply ",
                    //        _operationPermissionRequest.BODY.EQUIPMENTNO);

                    //    break;
                    //    #endregion

                    //case "OperationPermissionResultReport":

                    //    #region OperationPermissionResultReport
                    //    _cmdKey = "ATSOperationRunMode";

                    //    OperationPermissionResultReport _operationPermissionResult = (OperationPermissionResultReport)Spec.CheckXMLFormat(MsgXml);

                    //    if (_operationPermissionResult.BODY.OPERATORPERMISSION == "1") _desc1 = "1：Request";
                    //    else if (_operationPermissionResult.BODY.OPERATORPERMISSION == "2") _desc1 = "2：Complete";
                    //    else _desc1 = _operationPermissionResult.BODY.OPERATORPERMISSION;

                    //    if (_operationPermissionResult.BODY.RETURNCODE == "1") _desc2 = "1：OK";
                    //    else if (_operationPermissionResult.BODY.RETURNCODE == "2") _desc2 = "2：NG";
                    //    else _desc2 = _operationPermissionResult.BODY.RETURNCODE;

                    //    _cmdData = string.Format("Local No [{0}] Operator Permission Command [{1}] Return [{2}] ",
                    //        _operationPermissionResult.BODY.EQUIPMENTNO, _desc1, _desc2);

                    //    break;
                    //    #endregion

                    //case "OperationRunModeChangeReply":

                    //    #region OperationRunModeChangeReply
                    //    _cmdKey = "ATSOperationRunMode";

                    //    OperationRunModeChangeReply _operationRunModeChangeRequest = (OperationRunModeChangeReply)Spec.CheckXMLFormat(MsgXml);

                    //    if (_operationRunModeChangeRequest.BODY.COMMANDTYPE == "RUNMODE")
                    //    {
                    //        _desc1 = "Run Mode";
                    //    }
                    //    else
                    //    {
                    //        _desc1 = "Loader Operation Mode";
                    //    }

                    //    _cmdData = string.Format("Local No [{0}] Set {1} Reply", _operationRunModeChangeRequest.BODY.EQUIPMENTNO, _desc1);

                    //    break;
                    //    #endregion

                    //case "OperationRunModeChangeResultReport":

                    //    #region OperationRunModeChangeResultReport
                    //    _cmdKey = "ATSOperationRunMode";

                    //    OperationRunModeChangeResultReport _operationRunModeChangeResult = (OperationRunModeChangeResultReport)Spec.CheckXMLFormat(MsgXml);

                    //    if (_operationRunModeChangeResult.BODY.COMMANDTYPE == "RUNMODE")
                    //    {
                    //        _desc1 = "Run Mode";
                    //    }
                    //    else if (_operationRunModeChangeResult.BODY.COMMANDTYPE == "LOADEROPERATIONMODE")
                    //    {
                    //        _desc1 = "Loader Operation Mode";
                    //    }

                    //    if (_operationRunModeChangeResult.BODY.RETURNCODE == "1") _desc2 = "1：OK";
                    //    else if (_operationRunModeChangeResult.BODY.RETURNCODE == "2") _desc2 = "2：NG";
                    //    else _desc2 = _operationRunModeChangeResult.BODY.RETURNCODE;

                    //    _cmdData = string.Format("Local No [{0}] Set {1}  Return [{2}]", _operationRunModeChangeResult.BODY.EQUIPMENTNO, _desc1, _desc2);

                    //    break;
                    //    #endregion

                    case "JobDataOperationReply":

                        #region JobDataOperationReply
                        _cmdKey = "JobDataModify";

                        JobDataOperationReply _jobDataDelete = (JobDataOperationReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("[{0}] Reply",
                            _jobDataDelete.BODY.COMMAND);

                        break;
                        #endregion

                    case "ChangerPlanDownloadSetCommandReply":

                        #region ChangerPlanDownloadSetCommandReply
                        _cmdKey = "ChangerPlanDownload";

                        //ChangerPlanDownloadSetCommandReply _changerPlanDownloadSetCommand = (ChangerPlanDownloadSetCommandReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Empty;

                        break;
                        #endregion

                    case "ODFTrackTimeSettingChangeReply":

                        #region ODFTrackTimeSettingChangeReply
                        _cmdKey = "ODFTrackTimeSetting";

                        ODFTrackTimeSettingChangeReply _odfTrackTimeSetting = (ODFTrackTimeSettingChangeReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("Local No [{0}] Modiyf Reply",
                            _odfTrackTimeSetting.BODY.EQUIPMENTNO);

                        break;
                        #endregion

                    case "UnitRunModeChangeReply":

                        #region UnitRunModeChangeReply
                        _cmdKey = "UnitRunModeChange";

                        UnitRunModeChangeReply _unitRunModeChangeReply = (UnitRunModeChangeReply)Spec.CheckXMLFormat(MsgXml);

                         _cmdData = string.Format("Local No [{0}] Modiyf Reply", _unitRunModeChangeReply.BODY.EQUIPMENTNO);

                        break;
                        #endregion

                    case "InspectionFlowPriorityChangeReply":

                        #region InspectionFlowPriorityChangeReply
                        _cmdKey = "InspectionFlowPriorityChange";

                        InspectionFlowPriorityChangeReply _inspectionFlowPriorityChangeReply = (InspectionFlowPriorityChangeReply)Spec.CheckXMLFormat(MsgXml);

                        _cmdData = string.Format("InspectionFlowPriorityChange Reply");

                        break;
                        #endregion

                    default:
                        return string.Empty;
                }
                #endregion

                UniAuto.UniBCS.OpiSpec.Message _msg = Spec.CheckXMLFormat(MsgXml);

                #region 紀錄opi history

                SBCS_OPIHISTORY_TRX _opiHistory = new SBCS_OPIHISTORY_TRX();
                _opiHistory.SESSECTIONID = SessionId == null ? "" : SessionId;
                _opiHistory.OPDATETIME = TimeStamp;
                _opiHistory.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
                _opiHistory.DIRECTION = "BCS->OPI";
                _opiHistory.TRANSACTIONID = TrxID;
                _opiHistory.MESSAGENAME = MsgName;
                _opiHistory.MESSAGETRX = MsgXml;
                _opiHistory.RETURNCODE = _msg.RETURN.RETURNCODE;
                _opiHistory.RETURNMESSAGE = _msg.RETURN.RETURNMESSAGE;
                _opiHistory.COMMANDKEY = _cmdKey;
                _opiHistory.COMMANDDATA = _cmdData;
                _opiHistory.COMMANDTYPE = "TRX";
                _opiHistory.PROCESSRESULT = string.Empty;
                _opiHistory.PROCESSNGMESSAGE = string.Empty;

                FormMainMDI.G_OPIAp.DBCtx.SBCS_OPIHISTORY_TRX.InsertOnSubmit(_opiHistory);
                FormMainMDI.G_OPIAp.DBCtx.SubmitChanges();
                //Thread.Sleep(500);
                #endregion

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static string GetCassetteCommandDesc(string CommandValue)
        {
            #region Get Description
            switch (CommandValue)
            {
                case "1":  return  "1:Cassette Process Start";
                case "2":  return "2:Cassette Process Start By Count";              
                case "3":  return "3:Cassette Process Pause";
                case "4":  return "4:Cassette Process Resume";
                case "5":  return "5:Cassette Process Abort";
                case "6":  return "6:Cassette Process Cancel";
                case "7":  return "7:Cassette Reload";
                case "8":  return "8:Cassette Load";                    
                case "9":  return "9:Cassette Re-Map";
                case "11": return "11:Cassette Map Download";
                case "12": return "12:Cassette Aborting";
                default: return CommandValue;
            }
            #endregion
        }

        private static string GetProportionalNameDesc(string RuleName)
        {
            int _num = 0;

            if (int.TryParse(RuleName, out _num))
            {
                FetchGlassProportionalName _ftechName = FormMainMDI.G_OPIAp.CurLine.FetchGlassProportionalNames.Find(r => r.ProportionalNameNo.Equals(_num));

                if (_ftechName == null) return string.Format("{0}-UnKnown", RuleName);

                return _ftechName.ProportionalNameDesc;

            }
            else
            {
                return RuleName;
            }

        }

        public static string GetEquipmentIndexerModeDesc(string IndexerModeNo)
        {
            int _num = 0;

            if (int.TryParse(IndexerModeNo, out _num))
            {
                LineIndexerMode _indexer = FormMainMDI.G_OPIAp.CurLine.LineIndexerModes.Find(r => r.IndexerModeNo.Equals(_num));

                if (_indexer == null) return string.Format("{0}-UnKnown", IndexerModeNo);

                return _indexer.IndexerModeDesc;

            }
            else
            {
                return IndexerModeNo;
            }

        }

        private static string GetEquipmentRunModeDesc(string NodeNo,string RunModeValue)
        {
            if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(NodeNo) == false) return RunModeValue;


            int _no = 0;

            int.TryParse(RunModeValue, out _no);

            List<LineRunMode> _lstRunMode = FormMainMDI.G_OPIAp.Dic_Node[NodeNo].LineRunModes;

            if (_lstRunMode != null && _lstRunMode.Count > 0)
            {
                LineRunMode _runMode = _lstRunMode.Where(r => r.RunModeNo.Equals(_no)).FirstOrDefault();

                return _runMode.RunModeDesc;
            }
            else return RunModeValue ;
        }

        private static string GetSamplingRuleDesc(string RuleValue)
        {
            switch (RuleValue)
            {
                case "1": return "1:By Count" ;
                case "2": return "2:By Unit" ;
                case "3": return "3:By Slot" ;
                case "4": return "4:By ID" ;
                case "5": return "5:Full Inspection" ;
                case "6": return "6:Inspection Skip" ;
                case "7": return "7:Normal Inspection" ;
                default: return RuleValue;
            }
        }

        private static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static string GetEnumDescription<TEnum>(int value)
        {
            return value.ToString() + ":" + GetEnumDescription((Enum)(object)((TEnum)(object)value)); 
        }

        public static void Reload_RecipeSetting() 
        {
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

            var _check = (from _d in _ctxBRM.SBRM_NODE
                        where _d.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                         select new { NodeNo = _d.NODENO, RecipeCheck = _d.RECIPEREGVALIDATIONENABLED, ParameterCheck = _d.RECIPEPARAVALIDATIONENABLED });

            foreach (var _data in _check)
            {
                if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_data.NodeNo))
                {
                    FormMainMDI.G_OPIAp.Dic_Node[_data.NodeNo].RecipeRegisterCheck = (_data.RecipeCheck == "Y" ? true : false);
                    FormMainMDI.G_OPIAp.Dic_Node[_data.NodeNo].RecipeParameterCheck = (_data.ParameterCheck == "Y" ? true : false);
                }
            }            
        }

        public static void SetComboBox_Node(ComboBox cmbNode)
        {
            var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                     select new
                     {
                         IDNAME = string.Format("{0} - {1} - {2}", node.NodeNo, node.NodeID, node.NodeName),
                         node.NodeNo,
                         node.NodeID
                     }).ToList();

            if (q == null || q.Count == 0) return;

            cmbNode.DataSource = q;
            cmbNode.DisplayMember = "IDNAME";
            cmbNode.ValueMember = "NODENO";
            cmbNode.SelectedIndex = -1;
        }

        public static string ReverseStr(string strSource)
        {
            if (!string.IsNullOrEmpty(strSource))
            {
                try
                {
                    char[] charArray = strSource.ToCharArray();

                    Array.Reverse(charArray);

                    return new string(charArray);
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite("UniTools", MethodBase.GetCurrentMethod().Name, ex);
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }

        }

        //檢查OPI Message Body的LineName
        public static string CheckBodyLineName(UniAuto.UniBCS.OpiSpec.Body body)
        {
            string _msg = string.Empty;
            //bool ret = false;
            PropertyInfo[] properties = body.GetType().GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name == "LINENAME")
                {
                    string line_name = prop.GetValue(body, null) as string;
                    if (line_name == FormMainMDI.G_OPIAp.CurLine.ServerName)
                    {
                        //ret = true;
                        _msg = string.Empty;
                    }
                    else
                    {
                        _msg = string.Format("BCS Line Name [{0}] disaccords with OPI Line Name [{1}]", line_name, FormMainMDI.G_OPIAp.CurLine.ServerName);
                    }
                    break;
                }
            }
            return _msg;
        }

        public static void addRecipeTableHistory(string operation, SBRM_RECIPE recipe, UniBCSDataContext ctxBRM, string conStr)
        {
            try
            {
                if (CheckTableExist("SBCS_RECIPETABLE_TRX", conStr) == false) return;

                SBCS_RECIPETABLE_TRX _recipeHis = new SBCS_RECIPETABLE_TRX();

                //_recipeHis.ISCROSS = isCross ? "Y" : "N";
                _recipeHis.MODIFYTYPE = operation;

                _recipeHis.LINETYPE = recipe.LINETYPE;
                _recipeHis.ONLINECONTROLSTATE = recipe.ONLINECONTROLSTATE;
                _recipeHis.LINERECIPENAME = recipe.LINERECIPENAME;
                _recipeHis.PPID = recipe.PPID;
                _recipeHis.UPDATETIME = operation == "Delete" ? System.DateTime.Now : recipe.LASTUPDATEDT;
                _recipeHis.OPERATORID = recipe.UPDATEOPERATOR;
                _recipeHis.UPDATEPCIP = recipe.UPDATEPCIP;
                _recipeHis.NODEID = string.Empty;
                _recipeHis.REMARK = recipe.REMARK;

                ctxBRM.SBCS_RECIPETABLE_TRX.InsertOnSubmit(_recipeHis);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite("UniTools", MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        //Check Table 是否存在於DB內
        public static bool CheckTableExist(string tableName, string connStr)
        {
            try
            {
                bool _exists;
                csDBConfigXML _dbConfigXml = FormMainMDI.G_OPIAp.DBConfigXml;
                string _sql = string.Format("IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='{0}') SELECT 1 ELSE SELECT 0", tableName);

                if (connStr == string.Empty) connStr = FormMainMDI.G_OPIAp.DBConnStr;

                using (SqlConnection connection = new SqlConnection(connStr))
                {
                    using (SqlCommand command = new SqlCommand(_sql))
                    {
                        connection.Open();
                        command.Connection = connection;
                        _exists = command.ExecuteScalar().ToString() == "1" ? true : false;
                        connection.Close();
                    }
                }

                return _exists;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite("UniTools", MethodBase.GetCurrentMethod().Name, ex);
                return false;
            }
        }
    }

    public class comboxInfo
    {
        private string itemID = string.Empty;
        private string itemName = string.Empty;

        public string ITEM_ID
        {
            get { return itemID; }
            set { itemID = value; }
        }
        public string ITEM_NAME
        {
            get { return itemName; }
            set { itemName = value; }
        }
        public string ITEM_DESC
        {
            get { return string.Format("{0}:{1}", itemID, itemName); }
        }
    }

    public class WorkbookMgr
    {
        public class SWorkbook
        {
            public HSSFWorkbook XLSData { get; set; }

            public Dictionary<string, HSSFSheet> DicSheetData { get; set; }

            public int TotalSheetCount { get; set; }

            public List<string> LstSheetName { get; set; }

            public SWorkbook(Stream stream)
            {
                try
                {
                    TotalSheetCount = 0;
                    if (XLSData != null) XLSData = null;
                    XLSData = new HSSFWorkbook(stream);
                    LstSheetName = new List<string>();

                    if (DicSheetData != null) DicSheetData.Clear();
                    DicSheetData = null;
                    DicSheetData = new Dictionary<string, HSSFSheet>();

                    TotalSheetCount = XLSData.NumberOfSheets;

                    for (int i = 0; i < XLSData.NumberOfSheets; ++i)
                    {
                        HSSFSheet sheet = (HSSFSheet)(XLSData.GetSheetAt(i));
                        DicSheetData.Add(sheet.SheetName, sheet);
                        LstSheetName.Add(sheet.SheetName);
                        sheet = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            /// <summary>
            /// 轉換成DataTable進行資料操作
            /// </summary>
            /// <param name="sheetName">工作表名稱</param>
            /// <param name="dt">欲存放的DataTable</param>
            /// <param name="ColumnIndex">Column的起始位置</param>                
            public void ExcelSheet2DataTable(string sheetName, DataTable dt, int ColumnIndex)
            {
                if (XLSData == null && !DicSheetData.ContainsKey(sheetName)) return;

                if (dt == null) return;

                int rowCount = DicSheetData[sheetName].LastRowNum;
                int colCount = DicSheetData[sheetName].GetRow(ColumnIndex).LastCellNum;


                int i = 0;
                for (i = 0; i < colCount; ++i) dt.Columns.Add(i.ToString());

                HSSFRow row = DicSheetData[sheetName].GetRow(ColumnIndex) as HSSFRow;
                for (int j = 0; j < colCount; ++j)
                {
                    HSSFCell cell = row.GetCell(j) as HSSFCell;
                    try
                    {
                        dt.Columns[j].ColumnName = cell.StringCellValue;
                    }
                    catch
                    {
                        dt.Columns[j].ColumnName = cell.NumericCellValue.ToString();
                    }
                }
                Transform(DicSheetData[sheetName], dt, ColumnIndex + 1, rowCount, colCount);
            }

            /// <summary>
            /// 進行轉換sheet 2 dataTable
            /// </summary>
            /// <param name="sheet">欲轉換的工作表名稱</param>
            /// <param name="dt">存放的DataTable</param>
            /// <param name="rowStartIndex">指定開始工作表中的開始列</param>
            /// <param name="rowLen">長度(多少列)</param>
            /// <param name="colLen">每列有多少欄位</param>
            /// <param name="isCheckRegions">是否檢查合併儲存格</param>
            public void Transform(HSSFSheet sheet, DataTable dt, int rowStartIndex, int rowLen, int colLen)
            {
                for (int i = rowStartIndex; i <= rowLen; ++i)
                {
                    HSSFRow row = sheet.GetRow(i) as HSSFRow;
                    if (row == null) continue;
                    DataRow dr = dt.NewRow();

                    for (int j = 0; j < colLen; ++j)
                    {
                        HSSFCell cell = row.GetCell(j) as HSSFCell;
                        if (cell == null) break;

                        if (!cell.Sheet.IsColumnHidden(j))
                        {
                            try
                            {
                                dr[j] = cell.StringCellValue;
                            }
                            catch
                            {
                                dr[j] = cell.NumericCellValue;
                            }
                        }
                        else { ; }
                    }
                    dt.Rows.Add(dr);
                }
            }
        }

        public Dictionary<string, SWorkbook> DicAllOpenWorkbook { get; set; }

        public bool IsMakeGroupRow { get; set; }

        public WorkbookMgr()
        {
            DicAllOpenWorkbook = new Dictionary<string, SWorkbook>();
            DicAllOpenWorkbook.Clear();
            IsMakeGroupRow = false;
        }

        public List<string> GetAllSheetName(string fileKey)
        {
            return DicAllOpenWorkbook[fileKey].LstSheetName;
        }

        public int GetTotalMASheetMaxCount()
        {
            int result = 0;
            foreach (KeyValuePair<string, SWorkbook> item in DicAllOpenWorkbook)
            {
                result += item.Value.TotalSheetCount;
            }
            return result;
        }

        public bool OpenWorkbook(string fileAddr)
        {
            bool result = false;
            try
            {
                if (!DicAllOpenWorkbook.ContainsKey(fileAddr))
                {
                    using (FileStream file = new FileStream(fileAddr, FileMode.Open, FileAccess.Read))
                    {
                        DicAllOpenWorkbook.Add(fileAddr, new SWorkbook(file));
                        result = true;
                    }
                }
                else { ; }
            }
            catch (Exception ex)
            {
                result = false;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //GC.Collect();
            }

            return result;
        }

        public bool WriteWorkbook(string fileName, string sheetName, string titleName, DataTable sourceDt)
        {
            bool result = false;
            try
            {
                MemoryStream ms = DataTableToExcelStream(sourceDt, sheetName, titleName) as MemoryStream;

                //如果沒有資料則不寫入
                if (ms.Length < 1) return false;

                FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                byte[] data = ms.ToArray();

                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();

                data = null;
                ms = null;
                fs = null;

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //GC.Collect();
            }
            return result;
        }

        public bool WriteWorkbook(string fileName, string[] sheetName, string[] titleName, DataTable[] sourceDt, string[] yellowColumn, string[] redColumn)
        {
            bool result = false;
            try
            {
                MemoryStream ms = DataTableToExcelStream(sourceDt, sheetName, titleName, yellowColumn, redColumn) as MemoryStream;

                //如果沒有資料則不寫入
                if (ms.Length < 1) return false;

                FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                byte[] data = ms.ToArray();

                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();

                data = null;
                ms = null;
                fs = null;

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //GC.Collect();
            }
            return result;
        }

        public bool WriteWorkbook(string fileName, string sheetName, string titleName, string[] header, DataTable sourceDt)
        {
            bool result = false;
            try
            {
                MemoryStream ms = DataTableToExcelStream(sourceDt, sheetName, titleName, header) as MemoryStream;

                //如果沒有資料則不寫入
                if (ms.Length < 1) return false;

                FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                byte[] data = ms.ToArray();

                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();

                data = null;
                ms = null;
                fs = null;

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //GC.Collect();
            }
            return result;
        }

        private Stream DataTableToExcelStream(DataTable sourceDt, string sheetName, string titleName)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();
            MemoryStream ms = new MemoryStream();
            const int everySheetMaxRow = 65000;         //每頁Sheet的資料筆數上限設定為65000筆
            int sheetCount;                             //會產生的Sheet數
            HSSFSheet sheet;

            //如果可以整除，就不需要多加1頁Sheet，如果無法整除，就必須再加一頁Sheet
            sheetCount = sourceDt.Rows.Count % everySheetMaxRow > 0 ? sourceDt.Rows.Count / everySheetMaxRow + 1 : sourceDt.Rows.Count / everySheetMaxRow;

            for (int p = 0; p < sheetCount; p++)
            {
                sheet = (HSSFSheet)(workbook.CreateSheet(sheetName + "_" + (p + 1).ToString()));
                HSSFFont titleFont = workbook.CreateFont() as HSSFFont;
                titleFont.FontName = "標楷體";
                titleFont.FontHeightInPoints = 24;

                HSSFFont dataFont = workbook.CreateFont() as HSSFFont;
                dataFont.FontName = "標楷體";
                dataFont.FontHeightInPoints = 16;

                HSSFCellStyle titleFormat = workbook.CreateCellStyle() as HSSFCellStyle;
                titleFormat.Alignment = NPOI.SS.UserModel.HorizontalAlignment.LEFT;
                titleFormat.VerticalAlignment = VerticalAlignment.CENTER;
                titleFormat.SetFont(titleFont);

                HSSFCellStyle subTitleFormat = workbook.CreateCellStyle() as HSSFCellStyle;
                subTitleFormat.BorderBottom = CellBorderType.THIN;
                subTitleFormat.Alignment = NPOI.SS.UserModel.HorizontalAlignment.CENTER;
                subTitleFormat.SetFont(dataFont);

                HSSFCellStyle dataFormat = workbook.CreateCellStyle() as HSSFCellStyle;
                dataFormat.SetFont(dataFont);

                HSSFRow headerRow = (HSSFRow)(sheet.CreateRow(0));
                headerRow.CreateCell(1).SetCellValue(titleName);
                headerRow.GetCell(1).CellStyle = titleFormat;
                headerRow.HeightInPoints = 32;
                HSSFRow columnNameRow = (HSSFRow)(sheet.CreateRow(1));
                foreach (DataColumn column in sourceDt.Columns)
                {
                    columnNameRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
                    columnNameRow.GetCell(column.Ordinal).CellStyle = subTitleFormat;
                }
                columnNameRow.HeightInPoints = 24;
                int rowIndex = 2;                   //前兩行是標題和Header
                int dataCount = 0;                  //表示現在是在做DataTable裡面的第幾筆
                int writeCount = 0;                 //已經寫入該Sheet頁的筆數，當每個Sheet滿足限制筆術後，可以進行跳離，可以增加效率
                foreach (DataRow row in sourceDt.Rows)
                {
                    HSSFRow dataRow;
                    if (dataCount < (p + 1) * everySheetMaxRow && dataCount >= p * everySheetMaxRow)
                    {
                        dataRow = (HSSFRow)(sheet.CreateRow(rowIndex));
                        foreach (DataColumn column in sourceDt.Columns)
                        {
                            dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                            dataRow.GetCell(column.Ordinal).CellStyle = dataFormat;
                        }
                        dataRow.HeightInPoints = 24;
                        rowIndex++;
                        writeCount++;
                    }
                    dataCount++;

                    if (writeCount >= everySheetMaxRow) break;  //如果該Sheet頁已經滿足筆數，則跳出迴圈，降低程式執行Loading
                }
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 1, sourceDt.Columns.Count - 1));
                for (int i = 0; i < sourceDt.Columns.Count; ++i)
                {
                    sheet.SetColumnWidth(i, 32 * 256);
                }
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
            }

            sheet = null;
            workbook = null;

            return ms;
        }

        private Stream DataTableToExcelStream(DataTable[] sourceDt, string[] sheetName, string[] titleName, string[] yellowColumn, string[] redColumn)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();
            MemoryStream ms = new MemoryStream();
            int sheetCount = sourceDt.Count();          //會產生的Sheet數 -- 一個data table存一個sheet
            HSSFSheet sheet;
            int p = 0;
            foreach (DataTable _dt in sourceDt)
            {

                sheet = (HSSFSheet)(workbook.CreateSheet(sheetName[p]));

                #region 定義字型格式
                HSSFFont titleFont = workbook.CreateFont() as HSSFFont;
                titleFont.FontName = "標楷體";
                titleFont.FontHeightInPoints = 24;

                HSSFFont dataFont = workbook.CreateFont() as HSSFFont;
                dataFont.FontName = "標楷體";
                dataFont.FontHeightInPoints = 16;

                HSSFCellStyle titleFormat = workbook.CreateCellStyle() as HSSFCellStyle;
                titleFormat.Alignment = NPOI.SS.UserModel.HorizontalAlignment.LEFT;
                titleFormat.VerticalAlignment = VerticalAlignment.CENTER;
                titleFormat.SetFont(titleFont);

                HSSFCellStyle subTitleFormat = workbook.CreateCellStyle() as HSSFCellStyle;
                subTitleFormat.BorderBottom = CellBorderType.THIN;
                subTitleFormat.Alignment = NPOI.SS.UserModel.HorizontalAlignment.CENTER;
                subTitleFormat.SetFont(dataFont);

                HSSFCellStyle dataFormat = workbook.CreateCellStyle() as HSSFCellStyle;
                dataFormat.SetFont(dataFont);

                //顯示黃底欄位
                HSSFCellStyle dataFormat_Y = workbook.CreateCellStyle() as HSSFCellStyle;
                dataFormat_Y.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.YELLOW.index;
                dataFormat_Y.FillPattern = FillPatternType.SOLID_FOREGROUND;
                dataFormat_Y.SetFont(dataFont);

                //顯示紅底欄位
                HSSFCellStyle dataFormat_R = workbook.CreateCellStyle() as HSSFCellStyle;
                dataFormat_R.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.RED.index;
                dataFormat_R.FillPattern = FillPatternType.SOLID_FOREGROUND;
                dataFormat_R.SetFont(dataFont);

                #endregion

                int rowIndex = 0;

                #region 判斷是否要有標題
                if (titleName != null)
                {
                    HSSFRow headerRow = (HSSFRow)(sheet.CreateRow(rowIndex));
                    headerRow.CreateCell(1).SetCellValue(titleName[p]);
                    headerRow.GetCell(1).CellStyle = titleFormat;
                    headerRow.HeightInPoints = 32;
                    rowIndex = rowIndex + 1;
                }
                #endregion

                #region 產生欄位名稱
                HSSFRow columnNameRow = (HSSFRow)(sheet.CreateRow(rowIndex));
                foreach (DataColumn column in _dt.Columns)
                {
                    columnNameRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
                    columnNameRow.GetCell(column.Ordinal).CellStyle = subTitleFormat;
                }
                columnNameRow.HeightInPoints = 24;
                rowIndex = rowIndex + 1;
                #endregion

                int dataCount = 0;                  //表示現在是在做DataTable裡面的第幾筆
                int writeCount = 0;                 //已經寫入該Sheet頁的筆數，當每個Sheet滿足限制筆術後，可以進行跳離，可以增加效率
                foreach (DataRow row in _dt.Rows)
                {
                    HSSFRow dataRow;

                    dataRow = (HSSFRow)(sheet.CreateRow(rowIndex));
                    foreach (DataColumn column in _dt.Columns)
                    {
                        dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());


                        if (Array.IndexOf(yellowColumn, column.ColumnName) >= 0)
                            dataRow.GetCell(column.Ordinal).CellStyle = dataFormat_Y;
                        else if (Array.IndexOf(redColumn, column.ColumnName) >= 0)
                            dataRow.GetCell(column.Ordinal).CellStyle = dataFormat_R;
                        else dataRow.GetCell(column.Ordinal).CellStyle = dataFormat;

                    }
                    dataRow.HeightInPoints = 24;
                    rowIndex++;
                    writeCount++;
                }
                dataCount++;

                if (titleName != null) sheet.AddMergedRegion(new CellRangeAddress(0, 0, 1, _dt.Columns.Count - 1)); //若無標題 則不需合併儲存格

                for (int i = 0; i < _dt.Columns.Count; ++i)
                {
                    sheet.SetColumnWidth(i, 32 * 256);
                }
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
                p = p + 1;
            }

            sheet = null;
            workbook = null;

            return ms;
        }

        private Stream DataTableToExcelStream(DataTable sourceDt, string sheetName, string titleName, string[] header)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();
            MemoryStream ms = new MemoryStream();
            const int everySheetMaxRow = 65000;         //每頁Sheet的資料筆數上限設定為65000筆
            int sheetCount;                             //會產生的Sheet數
            HSSFSheet sheet;

            //如果可以整除，就不需要多加1頁Sheet，如果無法整除，就必須再加一頁Sheet
            sheetCount = sourceDt.Rows.Count % everySheetMaxRow > 0 ? sourceDt.Rows.Count / everySheetMaxRow + 1 : sourceDt.Rows.Count / everySheetMaxRow;

            for (int p = 0; p < sheetCount; p++)
            {
                sheet = (HSSFSheet)(workbook.CreateSheet(sheetName + "_" + (p + 1).ToString()));
                HSSFFont titleFont = workbook.CreateFont() as HSSFFont;
                titleFont.FontName = "標楷體";
                titleFont.FontHeightInPoints = 24;

                HSSFFont dataFont = workbook.CreateFont() as HSSFFont;
                dataFont.FontName = "標楷體";
                dataFont.FontHeightInPoints = 16;

                HSSFCellStyle titleFormat = workbook.CreateCellStyle() as HSSFCellStyle;
                titleFormat.Alignment = NPOI.SS.UserModel.HorizontalAlignment.LEFT;
                titleFormat.VerticalAlignment = VerticalAlignment.CENTER;
                titleFormat.SetFont(titleFont);

                HSSFCellStyle subTitleFormat = workbook.CreateCellStyle() as HSSFCellStyle;
                subTitleFormat.BorderBottom = CellBorderType.THIN;
                subTitleFormat.Alignment = NPOI.SS.UserModel.HorizontalAlignment.CENTER;
                subTitleFormat.SetFont(dataFont);

                HSSFCellStyle subTitleFormat2 = workbook.CreateCellStyle() as HSSFCellStyle;
                subTitleFormat2.BorderBottom = CellBorderType.THIN;
                subTitleFormat2.Alignment = NPOI.SS.UserModel.HorizontalAlignment.LEFT;
                subTitleFormat2.SetFont(dataFont);

                HSSFCellStyle dataFormat = workbook.CreateCellStyle() as HSSFCellStyle;
                dataFormat.SetFont(dataFont);

                int rowIndex = 2;

                //header
                if (header != null)
                {
                    HSSFRow titleRow = (HSSFRow)(sheet.CreateRow(0));
                    titleRow.CreateCell(1).SetCellValue(titleName);
                    titleRow.GetCell(1).CellStyle = titleFormat;
                    titleRow.HeightInPoints = 32;

                    for (int i = 0; i < header.Length; i++)
                    {
                        HSSFRow headerRow = (HSSFRow)(sheet.CreateRow(i + 1));
                        headerRow.CreateCell(0).SetCellValue(header[i]);
                        headerRow.GetCell(0).CellStyle = subTitleFormat2;
                        headerRow.HeightInPoints = 24;
                    }
                    rowIndex = header.Length + 1;

                    HSSFRow columnNameRow = (HSSFRow)(sheet.CreateRow(rowIndex));
                    foreach (DataColumn column in sourceDt.Columns)
                    {
                        columnNameRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
                        columnNameRow.GetCell(column.Ordinal).CellStyle = subTitleFormat;
                    }
                    columnNameRow.HeightInPoints = 24;

                    rowIndex = rowIndex + 1;
                }
                else
                {
                    HSSFRow headerRow = (HSSFRow)(sheet.CreateRow(0));
                    headerRow.CreateCell(1).SetCellValue(titleName);
                    headerRow.GetCell(1).CellStyle = titleFormat;
                    headerRow.HeightInPoints = 32;
                    HSSFRow columnNameRow = (HSSFRow)(sheet.CreateRow(1));
                    foreach (DataColumn column in sourceDt.Columns)
                    {
                        columnNameRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
                        columnNameRow.GetCell(column.Ordinal).CellStyle = subTitleFormat;
                    }
                    columnNameRow.HeightInPoints = 24;
                }

                //int rowIndex = 2;                   //前兩行是標題和Header
                int dataCount = 0;                  //表示現在是在做DataTable裡面的第幾筆
                int writeCount = 0;                 //已經寫入該Sheet頁的筆數，當每個Sheet滿足限制筆術後，可以進行跳離，可以增加效率
                foreach (DataRow row in sourceDt.Rows)
                {
                    HSSFRow dataRow;
                    if (dataCount < (p + 1) * everySheetMaxRow && dataCount >= p * everySheetMaxRow)
                    {
                        dataRow = (HSSFRow)(sheet.CreateRow(rowIndex));
                        foreach (DataColumn column in sourceDt.Columns)
                        {
                            dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                            dataRow.GetCell(column.Ordinal).CellStyle = dataFormat;
                        }
                        dataRow.HeightInPoints = 24;
                        rowIndex++;
                        writeCount++;
                    }
                    dataCount++;

                    if (writeCount >= everySheetMaxRow) break;  //如果該Sheet頁已經滿足筆數，則跳出迴圈，降低程式執行Loading
                }
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 1, sourceDt.Columns.Count - 1));
                for (int i = 0; i < sourceDt.Columns.Count; ++i)
                {
                    sheet.SetColumnWidth(i, 32 * 256);
                }
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
            }

            sheet = null;
            workbook = null;

            return ms;
        }
    }

    public class DBConnect
    {
        public static DataTable GetDataTable(UniBCSDataContext db, string TableName, System.Linq.IQueryable query)
        {
            DataTable dt = null;
            DataSet ds = GetDataSet(db, TableName, query);
            if (ds.Tables.Count > 0) dt = ds.Tables[0];
            return dt;
        }
        public static DataSet GetDataSet(UniBCSDataContext db, string TableName, System.Linq.IQueryable query)
        {
            if (query == null) throw new ArgumentNullException("query");

            if (string.IsNullOrEmpty(TableName)) TableName = "table";

            DataSet ds = new DataSet();

            System.Data.SqlClient.SqlCommand cmd = db.GetCommand(query) as System.Data.SqlClient.SqlCommand;
            DataTable dt = new DataTable(TableName);
            System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(cmd);
            try
            {
                db.Connection.Open();
                adapter.Fill(ds, TableName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                db.Connection.Close();
            }

            return ds;
        }

        public static DataTable GetDataTable(UniBCSDataContext db, string TableName, string Sql)
        {
            DataTable dt = null;
            DataSet ds = GetDataSet(db, TableName, Sql);
            if (ds.Tables.Count > 0) dt = ds.Tables[0];
            return dt;
        }
        public static DataSet GetDataSet(UniBCSDataContext db, string TableName, string Sql)
        {
            if (string.IsNullOrEmpty(TableName)) TableName = "table";

            DataSet ds = new DataSet();

            System.Data.SqlClient.SqlDataAdapter adapter =
                new System.Data.SqlClient.SqlDataAdapter(Sql, db.Connection.ConnectionString);
            try
            {
                db.Connection.Open();
                adapter.Fill(ds, TableName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                db.Connection.Close();
            }

            return ds;
        }

        public static DataTable ToDataTable(IEnumerable query)
        {
            try
            {
                DataTable dt = new DataTable();
                foreach (object obj in query)
                {
                    Type t = obj.GetType();
                    PropertyInfo[] pis = t.GetProperties();
                    if (dt.Columns.Count == 0)
                    {
                        foreach (PropertyInfo pi in pis)
                        {
                            Type propType = pi.PropertyType;
                            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                propType = Nullable.GetUnderlyingType(propType);
                            }
                            dt.Columns.Add(pi.Name, propType);
                        }
                    }
                    DataRow dr = dt.NewRow();
                    foreach (PropertyInfo pi in pis)
                    {
                        object value = pi.GetValue(obj, null);

                        if (value != null) dr[pi.Name] = value;                      
                                                
                    }
                    dt.Rows.Add(dr);
                }
                return dt;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite("UniTools", MethodBase.GetCurrentMethod().Name, ex);
                return null;
            }
        }
         
    }

    public class RecipeInfo
    {
        public string NodeNo { get; set; }
        public string NodeName { get; set; }
        public int RecipeIndex { get; set; }
        public int RecipeLength { get; set; }
    }

    public static class Public
    {

        public static void SendDatabaseReloadRequest(string TableName)
        {
            string _xml = string.Empty;
            DatabaseReloadRequest _trx = new DatabaseReloadRequest();
            _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
            _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
            _trx.BODY.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;
            _trx.BODY.TABLENAME = TableName;
            _xml = _trx.WriteToXml();

            string error = string.Empty;

            //傳送trx資訊
            FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out error, FormMainMDI.G_OPIAp.SessionID);
        }

        public static string GetEnumDesc(Enum en)
        {
            string desc = string.Empty;
            FieldInfo fi = en.GetType().GetField(en.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
                desc = attributes[0].Description;
            else
                desc = en.ToString();
            return desc;
        }
    }

    public static class DataContextExtensions
    {
        /// <summary>
        ///     Discard all pending changes of current DataContext.
        ///     All un-submitted changes, including insert/delete/modify will lost.
        /// </summary>
        /// <param name="context"></param>
        public static void DiscardPendingChanges(this DataContext context)
        {
            context.RefreshPendingChanges(RefreshMode.OverwriteCurrentValues);
            ChangeSet changeSet = context.GetChangeSet();
            if (changeSet != null)
            {
                //Undo inserts
                foreach (object objToInsert in changeSet.Inserts)
                {
                    context.GetTable(objToInsert.GetType()).DeleteOnSubmit(objToInsert);
                }
                //Undo deletes
                foreach (object objToDelete in changeSet.Deletes)
                {
                    context.GetTable(objToDelete.GetType()).InsertOnSubmit(objToDelete);
                }
            }
        }

        /// <summary>
        ///     Refreshes all pending Delete/Update entity objects of current DataContext according to the specified mode.
        ///     Nothing will do on Pending Insert entity objects.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="refreshMode">A value that specifies how optimistic concurrency conflicts are handled.</param>
        public static void RefreshPendingChanges(this DataContext context, RefreshMode refreshMode)
        {
            ChangeSet changeSet = context.GetChangeSet();
            if (changeSet != null)
            {
                context.Refresh(refreshMode, changeSet.Deletes);
                context.Refresh(refreshMode, changeSet.Updates);
            }
        }
    }
}
