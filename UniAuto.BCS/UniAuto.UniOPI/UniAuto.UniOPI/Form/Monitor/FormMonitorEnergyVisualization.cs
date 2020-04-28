using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormMonitorEnergyVisualization : FormBase
    {
        #region Fields
        private int curRowIndex = 0, curColumnIndex = 0;
        private string preButtonName = string.Empty;
        private Button CurrNodeBtn = null;
        #endregion

        public FormMonitorEnergyVisualization()
        {
            InitializeComponent();
            lblCaption.Text = "Energy Visualization";
        }

        private void FormMonitorEnergyVisualization_Shown(object sender, EventArgs e)
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
                var selData = (from msg in ctxBRM.SBRM_ENERGYVISUALIZATIONDATA
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
                CurrNodeBtn = (Button)sender ;

                if ( !preButtonName.Equals(CurrNodeBtn.Name) )
                {
                    curColumnIndex = 0;
                    curRowIndex = 0;
                }
                preButtonName = CurrNodeBtn.Name;

                string strErrMsg = string.Empty;
                string _xml = string.Empty;

                EnergyVisualizationReportRequest request = new EnergyVisualizationReportRequest();
                request.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                request.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                request.BODY.EQUIPMENTNO = CurrNodeBtn.Name;
                _xml = request.WriteToXml();                

                MessageResponse _resp = this.SendRequestResponse(request.HEADER.TRANSACTIONID, request.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region EnergyVisualizationReportReply

                string _respXml = _resp.Xml;

                EnergyVisualizationReportReply _energyVisualizationReportReply = (EnergyVisualizationReportReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data
                SetGridViewData(_energyVisualizationReportReply);
                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetGridViewData(EnergyVisualizationReportReply reply)
        {
            try
            {

                //add by box.zhai
                //modify by yang 20161104 SECS上报的机台,需要by DB SVID（目前只有ID上报）show
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                DataTable dt = null;
               
                
                var seleqp = (from eqp in ctxBRM.SBRM_NODE
                               where eqp.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName && eqp.NODENO == reply.BODY.EQUIPMENTNO
                               select eqp).Distinct().FirstOrDefault();

                if (seleqp.REPORTMODE.Equals("HSMS_PLC"))
                {
                    //資料庫資料
                    var selData = (from msg in ctxBRM.SBRM_ENERGYVISUALIZATIONDATA
                                   where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName && msg.NODENO == reply.BODY.EQUIPMENTNO
                                   orderby msg.OBJECTKEY ascending
                                   select msg).Distinct();

                    List<SBRM_ENERGYVISUALIZATIONDATA> objTables = selData.ToList();

                    dt = UniTools.InitDt(new string[] { "NAME", "VALUE" });

                    foreach (SBRM_ENERGYVISUALIZATIONDATA para in objTables)
                    {
                        DataRow drNew = dt.NewRow();
                        drNew["NAME"] = para.PARAMETERNAME;
                        int VALUE;
                        foreach (EnergyVisualizationReportReply.DATAc data in reply.BODY.DATALIST)
                        {
                           
                            if (string.Equals(data.NAME.Trim(),para.PARAMETERNAME))
                            {
                                 drNew["VALUE"] = data.VALUE;
                                if(para.PARAMETERNAME.ToUpper().Contains("DATATYPE"))
                                {
                                   if(int.TryParse(data.VALUE,out VALUE))
                                   {
                                       if ( VALUE == 1 )  drNew["VALUE"] = "Liquid";
                                       if ( VALUE == 2 ) drNew["VALUE"] = "Electricity";
                                       if ( VALUE == 3 ) drNew["VALUE"] = "Gas";
                                       if ( VALUE == 4 ) drNew["VALUE"] = "N2";
                                       if ( VALUE == 5 ) drNew["VALUE"] = "Stripper";
                                   }                                  
                                }                               
                            }
                        }
                        dt.Rows.Add(drNew);
                    }
                    dgvData.DataSource = dt;
                    dgvData.CurrentCell = dgvData.Rows[curRowIndex].Cells[curColumnIndex];  //add by yang 20161205
                }

                else  //非SECS机台,按照存储仓库的datalist show
                {

                   // dgvData.DataSource = dt;
                    if (dgvData.DataSource == null)
                    {
                        dt = UniTools.InitDt(new string[] { "NAME", "VALUE" });
                        foreach (EnergyVisualizationReportReply.DATAc data in reply.BODY.DATALIST)
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

                            EnergyVisualizationReportReply.DATAc currDATAc = null;
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
                            foreach (EnergyVisualizationReportReply.DATAc data in reply.BODY.DATALIST)
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
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);               
            }
        }
        //获取当前click 单元格,yang 20161205
        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            curRowIndex = e.RowIndex;
            curColumnIndex = e.ColumnIndex;
        }

    }
}
