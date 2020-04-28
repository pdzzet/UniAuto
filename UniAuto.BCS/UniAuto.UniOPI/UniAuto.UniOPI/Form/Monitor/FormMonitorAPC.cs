using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormMonitorAPC : FormBase
    {
        #region Fields
        private int curRowIndex=0, curColumnIndex=0;
        private string preButtonName=string.Empty;
        private Button CurrNodeBtn = null;
        #endregion

        public FormMonitorAPC()
        {
            InitializeComponent();
            lblCaption.Text = "APC Data";
        }

        private void FormMonitorAPC_Shown(object sender, EventArgs e)
        {
            this.Initial();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (CurrNodeBtn != null)
                btn_Click(CurrNodeBtn, e);
        }

        private void Initial()
        {
            dgvData.Rows.Clear();
            this.InitialButton();
        }

        private void InitialButton()
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                var selData = (from msg in ctxBRM.SBRM_APCDATAREPORT
                               where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                               select msg.NODENO).Distinct();
                var selData1 = (from msg in ctxBRM.SBRM_SECS_VARIABLEDATA
                               where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                               select msg.NODENO).Distinct();

                List<string> objTables = selData.ToList();
                List<string> objTables1 = selData1.ToList();
                objTables.AddRange(objTables1);

                //foreach (string key in FormMainMDI.G_OPIAp.Dic_Node.Keys)
                //{
                //    string reportMode = FormMainMDI.G_OPIAp.Dic_Node[key].ReportMode;
                //    if (reportMode.Contains("HSMS"))
                //    {
                //        objTables.Add(FormMainMDI.G_OPIAp.Dic_Node[key].NodeNo);
                //    }
                //}
                List<UcAutoButtons.ButtonInfo> lstButtonInfo = new List<UcAutoButtons.ButtonInfo>();
                foreach (KeyValuePair<string, Node> kvpNode in FormMainMDI.G_OPIAp.Dic_Node)
                {
                    bool find = false;
                    foreach (string node_no in objTables)
                    {
                        if (kvpNode.Key == node_no)
                        {
                            find = true;
                            break;
                        }
                    }
                    if (find)
                    {
                        string strButtonText = string.Format("{0}-{1}", kvpNode.Value.NodeNo, kvpNode.Value.NodeID);
                        lstButtonInfo.Add(new UcAutoButtons.ButtonInfo(kvpNode.Key, strButtonText, btn_Click));
                    }
                }

                ucAutoBtnEqp.CreateButton(lstButtonInfo);
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
                CurrNodeBtn = (Button)sender;
                if (preButtonName != string.Empty && preButtonName != CurrNodeBtn.Name)
                {
                    curColumnIndex = 0;
                    curRowIndex = 0;
                }
                preButtonName = CurrNodeBtn.Name;
                //add by box.zhai 20161103
                string requestType = string.Empty;
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var selData = (from msg in ctxBRM.SBRM_OPI_SECS_CTRL
                                   where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName & msg.NODENO == CurrNodeBtn.Name 
                                   & msg.SECS=="S1F5" &msg.SECSTYPE=="CSOT"&msg.COLUMNNAME=="SFCD"
                               select msg.ITEMVALUE).Distinct();

                List<string> listSelData = selData.ToList();
                if (FormMainMDI.G_OPIAp.Dic_Node[CurrNodeBtn.Name].ReportMode == "HSMS_PLC")
                {
                    if (listSelData.Count == 0)
                    {
                        string msg = string.Format("SBRM_SECS_VARIABLEDATA 表中未维护 LineID=[{0}],NodeNO=[{1}],SECS Type=[CSOT],SECS=[S1F5]，ColumnName=[SFCD]！",
                                FormMainMDI.G_OPIAp.CurLine.LineName, CurrNodeBtn.Name);
                        MessageBox.Show(this, msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        foreach (string str in listSelData)
                        {
                            if (str.Contains("02") || str.Contains("04"))
                            {
                                requestType = "Name";
                            }
                            else if (str.Contains("07") || str.Contains("08"))
                            {
                                requestType = "ID";
                            }
                            else
                            {
                                string msg = string.Format("SBRM_SECS_VARIABLEDATA中LineID=[{0}],NodeNO=[{1}],SECS Type=[CSOT],SECS=[S1F5]，ColumnName=[SFCD]的Item Value不包含APC Data！",
                                    FormMainMDI.G_OPIAp.CurLine.LineName, CurrNodeBtn.Name);
                                MessageBox.Show(this, msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }

                string strErrMsg = string.Empty;
                string _xml = string.Empty;

                APCDataReportRequest request = new APCDataReportRequest();
                request.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                request.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                request.BODY.EQUIPMENTNO = CurrNodeBtn.Name;
                request.BODY.SECSREQUESTBYIDORNAME=requestType;
                _xml = request.WriteToXml();                

                MessageResponse _resp = this.SendRequestResponse(request.HEADER.TRANSACTIONID, request.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region APCDataRequestReply

                string _respXml = _resp.Xml;

                APCDataReportReply _apcDataReply = (APCDataReportReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data
                SetGridViewData(_apcDataReply);
                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }


        private void SetGridViewData(APCDataReportReply reply)
        {
            try
            {
                #region //20161205 box.zhai modify 勝杰 需求 refresh 表格不要跳動
                DataTable dt = null;

                //add by box.zhai
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                var selData = (from msg in ctxBRM.SBRM_APCDATAREPORT
                                where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName && msg.NODENO == reply.BODY.EQUIPMENTNO
                                orderby msg.OBJECTKEY ascending
                                select msg).Distinct();

                List<SBRM_APCDATAREPORT> objTables = selData.ToList();

                dt = UniTools.InitDt(new string[] { "NAME", "VALUE" });
                foreach (SBRM_APCDATAREPORT para in objTables)
                {
                    DataRow drNew = dt.NewRow();
                    drNew["NAME"] = para.PARAMETERNAME;
                    foreach (APCDataReportReply.DATAc data in reply.BODY.DATALIST)
                    {
                        if (string.Equals(data.NAME.Trim(), para.PARAMETERNAME))
                        {
                            drNew["VALUE"] = data.VALUE;
                            break;
                        }
                    }
                    dt.Rows.Add(drNew);
                }
                dgvData.DataSource = dt;
                dgvData.CurrentCell=dgvData.Rows[curRowIndex].Cells[curColumnIndex];
                #endregion

                //DataTable dt = null;
                //if (dgvData.DataSource == null)
                //{
                //    dt = UniTools.InitDt(new string[] { "NAME", "VALUE" });
                //    foreach (APCDataReportReply.DATAc data in reply.BODY.DATALIST)
                //    {
                //        DataRow drNew = dt.NewRow();
                //        drNew["NAME"] = data.NAME;
                //        drNew["VALUE"] = data.VALUE;
                //        dt.Rows.Add(drNew);
                //    }

                //    dgvData.DataSource = dt;
                //}
                //else
                //{
                //    dt = dgvData.DataSource as DataTable;

                //    int rowIndex = 0;
                //    if (reply.BODY.DATALIST.Count < dt.Rows.Count)
                //    {
                //        // 新取得的資料筆數比原來資料少時，將原來DataTable多出來的筆數刪掉
                //        for (int x = dt.Rows.Count - 1; x >= reply.BODY.DATALIST.Count; x--)
                //        {
                //            dt.Rows[x].Delete();
                //        }

                //        APCDataReportReply.DATAc currDATAc = null;
                //        foreach (DataRow dr in dt.Rows)
                //        {
                //            if (rowIndex < reply.BODY.DATALIST.Count)
                //            {
                //                currDATAc = reply.BODY.DATALIST[rowIndex];
                //                if (!currDATAc.NAME.Equals(dr["NAME"].ToString()))
                //                {
                //                    dr["NAME"] = currDATAc.NAME;
                //                    dr["VALUE"] = currDATAc.VALUE;
                //                }
                //                else
                //                {
                //                    if (!currDATAc.Equals(dr["VALUE"].ToString()))
                //                        dr["VALUE"] = currDATAc.VALUE;
                //                }
                //            }

                //            rowIndex++;
                //        }
                //    }
                //    else
                //    {
                //        DataRow drCurr = null;
                //        foreach (APCDataReportReply.DATAc data in reply.BODY.DATALIST)
                //        {
                //            if (rowIndex < dt.Rows.Count)
                //            {
                //                drCurr = dt.Rows[rowIndex];
                //                if (!drCurr["NAME"].ToString().Equals(data.NAME))
                //                {
                //                    drCurr["NAME"] = data.NAME;
                //                    drCurr["VALUE"] = data.VALUE;
                //                }
                //                else
                //                {
                //                    if (!drCurr["VALUE"].ToString().Equals(data.VALUE))
                //                        drCurr["VALUE"] = data.VALUE;
                //                }
                //            }
                //            else
                //            {
                //                // 新取得的資料筆數比原來資料多時，將多出的資料加入DataTable
                //                DataRow drNew = dt.NewRow();
                //                drNew["NAME"] = data.NAME;
                //                drNew["VALUE"] = data.VALUE;
                //                dt.Rows.Add(drNew);
                //            }

                //            rowIndex++;
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);               
            }
        }

        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            curRowIndex = e.RowIndex;
            curColumnIndex = e.ColumnIndex;
        }

    }
}
