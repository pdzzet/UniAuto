using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormMonitorEDC : FormBase
    {
        private Button CurrNodeBtn = null;

        public FormMonitorEDC()
        {
            InitializeComponent();
            lblCaption.Text = "Process Data";
        }

        private void FormMonitorEDC_Shown(object sender, EventArgs e)
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
                var selData = (from msg in ctxBRM.SBRM_PROCESSDATA
                              where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                              select msg.NODENO).Distinct();

                List<string> objTables = selData.ToList();
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
                CurrNodeBtn = sender as System.Windows.Forms.Button;

                string strErrMsg = string.Empty;
                string _xml = string.Empty;

                ProcessDataReportRequest request = new ProcessDataReportRequest();
                request.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                request.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                request.BODY.EQUIPMENTNO = CurrNodeBtn.Name;
                _xml = request.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(request.HEADER.TRANSACTIONID, request.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region ProcessDataReportReply

                string _respXml = _resp.Xml;

                ProcessDataReportReply _processDataReportReply = (ProcessDataReportReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data
                this.SetGridViewData(_processDataReportReply);
                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }


        private void SetGridViewData(ProcessDataReportReply reply)
        {
            try
            {
                DataTable dt = null;
                if (dgvData.DataSource == null)
                {
                    dt = UniTools.InitDt(new string[] { "NAME", "VALUE" });
                    foreach (ProcessDataReportReply.DATAc data in reply.BODY.DATALIST)
                    {
                        DataRow drNew = dt.NewRow();
                        drNew["NAME"] = data.NAME;
                        drNew["VALUE"] = data.VALUE;
                        dt.Rows.Add(drNew);
                    }

                    dgvData.DataSource = dt;
                }
                else
                {
                    dt = dgvData.DataSource as DataTable;

                    int rowIndex = 0;
                    if (reply.BODY.DATALIST.Count < dt.Rows.Count)
                    {
                        // 新取得的資料筆數比原來資料少時，將原來DataTable多出來的筆數刪掉
                        for (int x = dt.Rows.Count - 1; x >= reply.BODY.DATALIST.Count; x--)
                        {
                            dt.Rows[x].Delete();
                        }

                        ProcessDataReportReply.DATAc currDATAc = null;
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (rowIndex < reply.BODY.DATALIST.Count)
                            {
                                currDATAc = reply.BODY.DATALIST[rowIndex];
                                if (!currDATAc.NAME.Equals(dr["NAME"].ToString()))
                                {
                                    dr["NAME"] = currDATAc.NAME;
                                    dr["VALUE"] = currDATAc.VALUE;
                                }
                                else
                                {
                                    if (!currDATAc.Equals(dr["VALUE"].ToString()))
                                        dr["VALUE"] = currDATAc.VALUE;
                                }
                            }

                            rowIndex++;
                        }
                    }
                    else
                    {
                        DataRow drCurr = null;
                        foreach (ProcessDataReportReply.DATAc data in reply.BODY.DATALIST)
                        {
                            if (rowIndex < dt.Rows.Count)
                            {
                                drCurr = dt.Rows[rowIndex];
                                if (!drCurr["NAME"].ToString().Equals(data.NAME))
                                {
                                    drCurr["NAME"] = data.NAME;
                                    drCurr["VALUE"] = data.VALUE;
                                }
                                else
                                {
                                    if (!drCurr["VALUE"].ToString().Equals(data.VALUE))
                                        drCurr["VALUE"] = data.VALUE;
                                }
                            }
                            else
                            {
                                // 新取得的資料筆數比原來資料多時，將多出的資料加入DataTable
                                DataRow drNew = dt.NewRow();
                                drNew["NAME"] = data.NAME;
                                drNew["VALUE"] = data.VALUE;
                                dt.Rows.Add(drNew);
                            }

                            rowIndex++;
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

    }
}
