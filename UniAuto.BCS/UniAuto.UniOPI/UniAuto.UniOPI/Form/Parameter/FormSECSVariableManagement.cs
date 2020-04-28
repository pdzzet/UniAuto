using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormSECSVariableManagement : FormBase
    {
        private const string NODE_REPORTMODE_HSMS = "HSMS";

        private CmbItem[] DataTypeItems = new[]             
        { 
            new CmbItem{ ID = "APC_IMPORTANT", NAME = "APC Important" } ,
            new CmbItem{ ID = "APC_NORMAL", NAME = "APC Normal" } ,
            new CmbItem{ ID = "SPECIAL_DATA", NAME = "Special Data" },
            new CmbItem{ ID = "UTILITY", NAME = "Utility" }
        };

        private CmbItem[] TrIDItems = new[]             
        { 
            new CmbItem{ ID = "S1F5_02", NAME = "S1F5 APC Important for name" },
            new CmbItem{ ID = "S1F5_04", NAME = "S1F5 APC Normal for name" },
            new CmbItem{ ID = "S1F5_06", NAME = "S1F5 Special Data for name" },
            new CmbItem{ ID = "S1F5_07", NAME = "S1F5 APC Important for ID" },
            new CmbItem{ ID = "S1F5_08", NAME = "S1F5 APC Normal for ID" },
            new CmbItem{ ID = "S1F5_10", NAME = "S1F5 Special Data for ID" },
            new CmbItem{ ID = "S6F3_04", NAME = "S6F3 APC Important for name" },
            new CmbItem{ ID = "S6F3_05", NAME = "S6F3 APC Normal for name" },
            new CmbItem{ ID = "S6F3_08", NAME = "S6F3 Special Data for name" },
            new CmbItem{ ID = "S6F3_09", NAME = "S6F3 APC Important for ID" },
            new CmbItem{ ID = "S6F3_10", NAME = "S6F3 APC Normal for ID" },
            new CmbItem{ ID = "S6F3_11", NAME = "S6F3 Special Data for ID" },
            new CmbItem{ ID = "APCIM", NAME = "Nikon APC Important" } ,
            new CmbItem{ ID = "APCNO", NAME = "Nikon APC Normal" } ,
            new CmbItem{ ID = "SPCAL", NAME = "Nikon Special Data" },
            new CmbItem{ ID = "UTILY", NAME = "Nikon Utility" }
        };

        public FormSECSVariableManagement()
        {
            InitializeComponent();
            lblCaption.Text = "SECS Variable Management";
        }

        private void FormSECSVariableManagement_Load(object sender, EventArgs e)
        {
            this.InitialCombox();
            dgvData.AutoGenerateColumns = false;
        }

        private void btnQuerySource_Click(object sender, EventArgs e)
        {
            try
            {
                string source = cmbSource.SelectedValue.ToString();
                UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBCtx;
                switch (source)
                {
                    case "SBRM_RECIPEPARAMETER":
                        #region SBRM_RECIPEPARAMETER
                        var recipeData = from msg in ctx.SBRM_RECIPEPARAMETER
                                         where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                                         select msg;

                        List<SBRM_RECIPEPARAMETER> objRecipeData = recipeData.ToList();
                        if (cmbNode.SelectedIndex != -1)
                            objRecipeData = objRecipeData.Where(msg => msg.NODENO == (cmbNode.SelectedItem as dynamic).NodeNo).ToList();

                        dgvSourceData.AutoGenerateColumns = false;
                        dgvSourceData.DataSource = objRecipeData;

                        foreach (DataGridViewRow dr in dgvSourceData.Rows)
                        {
                            if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(dr.Cells[colSourceNodeNo.Name].Value.ToString()))
                                dr.Cells[colSourceNodeID.Name].Value = FormMainMDI.G_OPIAp.Dic_Node[dr.Cells[colSourceNodeNo.Name].Value.ToString()].NodeID.ToString();
                        }

                        break;
                        #endregion
                    case "SBRM_DAILYCHECKDATA":
                        #region SBRM_DAILYCHECKDATA
                        var dailycheckData = from msg in ctx.SBRM_DAILYCHECKDATA
                                             where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                                             select msg;

                        List<SBRM_DAILYCHECKDATA> objDailycheck = dailycheckData.ToList();
                        if (cmbNode.SelectedIndex != -1)
                            objDailycheck = objDailycheck.Where(msg => msg.NODENO == (cmbNode.SelectedItem as dynamic).NodeNo).ToList();

                        dgvSourceData.AutoGenerateColumns = false;
                        dgvSourceData.DataSource = objDailycheck;

                        foreach (DataGridViewRow dr in dgvSourceData.Rows)
                        {
                            if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(dr.Cells[colSourceNodeNo.Name].Value.ToString()))
                                dr.Cells[colSourceNodeID.Name].Value = FormMainMDI.G_OPIAp.Dic_Node[dr.Cells[colSourceNodeNo.Name].Value.ToString()].NodeID.ToString();
                        }

                        break;
                        #endregion
                    case "SBRM_PROCESSDATA":
                        #region SBRM_PROCESSDATA
                        var processData = from msg in ctx.SBRM_PROCESSDATA
                                          where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                                          select msg;

                        List<SBRM_PROCESSDATA> objProcess = processData.ToList();
                        if (cmbNode.SelectedIndex != -1)
                            objProcess = objProcess.Where(msg => msg.NODENO == (cmbNode.SelectedItem as dynamic).NodeNo).ToList();

                        dgvSourceData.AutoGenerateColumns = false;
                        dgvSourceData.DataSource = objProcess;

                        foreach (DataGridViewRow dr in dgvSourceData.Rows)
                        {
                            if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(dr.Cells[colSourceNodeNo.Name].Value.ToString()))
                                dr.Cells[colSourceNodeID.Name].Value = FormMainMDI.G_OPIAp.Dic_Node[dr.Cells[colSourceNodeNo.Name].Value.ToString()].NodeID.ToString();
                        }

                        break;
                        #endregion
                    case "SBRM_ENERGYVISUALIZATIONDATA":
                        #region SBRM_ENERGYVISUALIZATIONDATA
                        var energyvisualizationData = from msg in ctx.SBRM_ENERGYVISUALIZATIONDATA where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName select msg;

                        List<SBRM_ENERGYVISUALIZATIONDATA> objenergyvisualizationData = energyvisualizationData.ToList();
                        if (cmbNode.SelectedIndex != -1)
                            objenergyvisualizationData = objenergyvisualizationData.Where(msg => msg.NODENO == (cmbNode.SelectedItem as dynamic).NodeNo).ToList();

                        dgvSourceData.AutoGenerateColumns = false;
                        dgvSourceData.DataSource = objenergyvisualizationData;

                        foreach (DataGridViewRow dr in dgvSourceData.Rows)
                        {
                            if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(dr.Cells[colSourceNodeNo.Name].Value.ToString()))
                            {
                                dr.Cells[colSourceNodeID.Name].Value = FormMainMDI.G_OPIAp.Dic_Node[dr.Cells[colSourceNodeNo.Name].Value.ToString()].NodeID.ToString();
                            }
                        }
                        break;
                        #endregion
                    case "SBRM_APCDATAREPORT":
                        #region SBRM_APCDATAREPORT
                        var apcDataReport = from msg in ctx.SBRM_APCDATAREPORT where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName select msg;

                        List<SBRM_APCDATAREPORT> objAPCDataReport = apcDataReport.ToList();
                        if (cmbNode.SelectedIndex != -1)
                            objAPCDataReport = objAPCDataReport.Where(msg => msg.NODENO == (cmbNode.SelectedItem as dynamic).NodeNo).ToList();

                        dgvSourceData.AutoGenerateColumns = false;
                        dgvSourceData.DataSource = objAPCDataReport;

                        foreach (DataGridViewRow dr in dgvSourceData.Rows)
                        {
                            if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(dr.Cells[colSourceNodeNo.Name].Value.ToString()))
                            {
                                dr.Cells[colSourceNodeID.Name].Value = FormMainMDI.G_OPIAp.Dic_Node[dr.Cells[colSourceNodeNo.Name].Value.ToString()].NodeID.ToString();
                            }
                        }
                        break;
                        #endregion
                }

                if (dgvSourceData.RowCount <= 0)
                {
                    this.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "No matching data for your query！", MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            CheckChangeSave();
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvData.SelectedRows.Count != 1)
                    return;

                SBRM_SECS_VARIABLEDATA objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as SBRM_SECS_VARIABLEDATA;
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_SECS_VARIABLEDATA objAlarm = null;

                if (objSelModify.OBJECTKEY == 0)
                {
                    // 修改的是尚未Submit的新增
                    var objAddModify = (from secs in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SECS_VARIABLEDATA>()
                                        where secs.LINEID == objSelModify.LINEID && secs.LINETYPE == objSelModify.LINETYPE &&
                                        secs.NODENO == objSelModify.NODENO && secs.DATATYPE == objSelModify.DATATYPE &&
                                        secs.TRID == objSelModify.TRID && secs.ITEM_ID == objSelModify.ITEM_ID
                                        select secs).FirstOrDefault();
                    if (objAddModify != null)
                    {
                        objAlarm = objAddModify;
                    }
                    else return;
                }
                else
                {
                    objAlarm = objSelModify;
                }

                FormSECSVariableManagementEdit frmModify = new FormSECSVariableManagementEdit(objSelModify);
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
            if (dgvData.SelectedRows.Count == 0)
                return;

            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, "Are you sure to delete selected records?")) return;

            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                
                SBRM_SECS_VARIABLEDATA objEach = null, objToDelete = null;

                foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                {
                    objEach = selectedRow.DataBoundItem as SBRM_SECS_VARIABLEDATA;
                    if (objEach.OBJECTKEY > 0)
                    {
                        ctxBRM.SBRM_SECS_VARIABLEDATA.DeleteOnSubmit(objEach);
                    }
                    else
                    {
                        objToDelete = (from secs in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SECS_VARIABLEDATA>()
                                       where secs.LINEID == objEach.LINEID && secs.LINETYPE == objEach.LINETYPE &&
                                    secs.NODENO == objEach.NODENO && secs.DATATYPE == objEach.DATATYPE &&
                                    secs.TRID == objEach.TRID && secs.ITEM_NAME == objEach.ITEM_NAME
                                       select secs).FirstOrDefault();

                        if (objToDelete != null)
                            ctxBRM.SBRM_SECS_VARIABLEDATA.DeleteOnSubmit(objToDelete);
                    }

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
           // SendToSECS();  //yang 让Save帮忙判断
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            CheckChangeSave();
        }

        private void btnSetToRow_Click(object sender, EventArgs e)
        {
            try
            {
                #region CheckData
                if (cmbDataType.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Data Type required！", MessageBoxIcon.Warning);
                    cmbDataType.Focus();
                    cmbDataType.DroppedDown = true;
                    return;
                }

                if (cmbTrID.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Transfer ID required！", MessageBoxIcon.Warning);
                    cmbTrID.Focus();
                    cmbTrID.DroppedDown = true;
                    return;
                }
                #endregion

                string source = cmbSource.SelectedValue.ToString();
                foreach (DataGridViewRow selectedRow in dgvSourceData.SelectedRows)
                {
                    if (selectedRow == null) continue;

                    //20150415 cy:修改Nikon的ItemID來源為SVID
                    string strItemName = "";
                    string strDesc = "";
                    string strItemID = "";
                    switch (source)
                    {
                        case "SBRM_RECIPEPARAMETER":
                            strItemName = (selectedRow.DataBoundItem as SBRM_RECIPEPARAMETER).PARAMETERNAME;
                            strDesc = (selectedRow.DataBoundItem as SBRM_RECIPEPARAMETER).DESCRIPTION;
                            strItemID = (selectedRow.DataBoundItem as SBRM_RECIPEPARAMETER).SVID;
                            break;
                        case "SBRM_DAILYCHECKDATA":
                            strItemName = (selectedRow.DataBoundItem as SBRM_DAILYCHECKDATA).PARAMETERNAME;
                            strDesc = (selectedRow.DataBoundItem as SBRM_DAILYCHECKDATA).DESCRIPTION;
                            strItemID = (selectedRow.DataBoundItem as SBRM_DAILYCHECKDATA).SVID;

                            break;
                        case "SBRM_PROCESSDATA":
                            strItemName = (selectedRow.DataBoundItem as SBRM_PROCESSDATA).PARAMETERNAME;
                            strDesc = (selectedRow.DataBoundItem as SBRM_PROCESSDATA).DESCRIPTION;
                            strItemID = (selectedRow.DataBoundItem as SBRM_PROCESSDATA).SVID;
                            break;

                        case "SBRM_ENERGYVISUALIZATIONDATA":
                            strItemName = (selectedRow.DataBoundItem as SBRM_ENERGYVISUALIZATIONDATA).PARAMETERNAME;
                            strDesc = (selectedRow.DataBoundItem as SBRM_ENERGYVISUALIZATIONDATA).DESCRIPTION;
                            strItemID = (selectedRow.DataBoundItem as SBRM_ENERGYVISUALIZATIONDATA).SVID;
                            break;

                        case "SBRM_APCDATAREPORT":
                            strItemName = (selectedRow.DataBoundItem as SBRM_APCDATAREPORT).PARAMETERNAME;
                            strDesc = (selectedRow.DataBoundItem as SBRM_APCDATAREPORT).DESCRIPTION;
                            strItemID = (selectedRow.DataBoundItem as SBRM_APCDATAREPORT).SVID;
                            break;
                    
                    }

                    string strLineType = FormMainMDI.G_OPIAp.CurLine.LineType;
                    string strLineID = FormMainMDI.G_OPIAp.CurLine.LineID;
                    string strServerName = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    string strNodeNo = ((dynamic)this.cmbNode.SelectedItem).NodeNo;
                    string strDataType = this.cmbDataType.SelectedValue.ToString();
                    string strTrID = this.cmbTrID.SelectedValue.ToString();
                    //string strItemID = "";
                    string strItemType = "";
                    char strItemSet = '1';
                    string strSp1 = "";
                    string strSp2 = "";
                    string strSp3 = "";

                    UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                    SBRM_SECS_VARIABLEDATA objAddModify = null;
                    objAddModify = (from secs in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SECS_VARIABLEDATA>()
                                    where secs.LINEID == strLineID && secs.LINETYPE == strLineType &&
                                    secs.NODENO == strNodeNo && secs.DATATYPE == strDataType &&
                                    secs.TRID == strTrID && secs.ITEM_NAME == strItemName
                                    select secs).FirstOrDefault();

                    if (objAddModify == null &&
                        CheckUniqueKeyOK(strLineType, strLineID, strNodeNo, strDataType, strTrID, strItemName) == false)
                    {
                        continue;
                    }

                    SBRM_SECS_VARIABLEDATA objSecsVariable = null;
                    #region AddNew
                    if (objAddModify != null)
                    {
                        // 新增的如果是尚未Submit的資料(已存在ChangeSet().Inserts)，直接取出物件來編輯
                        objSecsVariable = objAddModify;
                    }
                    else
                    {
                        objSecsVariable = new SBRM_SECS_VARIABLEDATA();
                        objSecsVariable.LINETYPE = strLineType;
                        objSecsVariable.LINEID = strLineID;
                        objSecsVariable.SERVERNAME = strServerName;
                        objSecsVariable.NODENO = strNodeNo;
                        objSecsVariable.DATATYPE = strDataType;
                        objSecsVariable.TRID = strTrID;
                        objSecsVariable.ITEM_NAME = strItemName;

                        ctxBRM.SBRM_SECS_VARIABLEDATA.InsertOnSubmit(objSecsVariable);
                    }
                    #endregion

                    if (objSecsVariable == null) continue;
                    objSecsVariable.ITEM_ID = strItemID;
                    objSecsVariable.ITEM_TYPE = strItemType;
                    objSecsVariable.ITEM_SET = strItemSet;
                    objSecsVariable.SP1 = strSp1;
                    objSecsVariable.SP2 = strSp2;
                    objSecsVariable.SP3 = strSp3;
                    objSecsVariable.DESCRIPTION = strDesc;
                }

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
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
                    SBRM_SECS_VARIABLEDATA objData = dr.DataBoundItem as SBRM_SECS_VARIABLEDATA;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SECS_VARIABLEDATA>().Any(
                        msg => msg.LINEID == objData.LINEID && msg.LINETYPE == objData.LINETYPE
                                   && msg.NODENO == objData.NODENO && msg.DATATYPE == objData.DATATYPE
                                   && msg.TRID == objData.TRID && msg.ITEM_NAME == objData.ITEM_NAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_SECS_VARIABLEDATA>().Any(
                        msg => msg.LINEID == objData.LINEID && msg.LINETYPE == objData.LINETYPE
                                   && msg.NODENO == objData.NODENO && msg.DATATYPE == objData.DATATYPE
                                   && msg.TRID == objData.TRID && msg.ITEM_NAME == objData.ITEM_NAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_SECS_VARIABLEDATA>().Any(
                        msg => msg.LINEID == objData.LINEID && msg.LINETYPE == objData.LINETYPE
                                   && msg.NODENO == objData.NODENO && msg.DATATYPE == objData.DATATYPE
                                   && msg.TRID == objData.TRID && msg.ITEM_NAME == objData.ITEM_NAME))
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

        private void cmbDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbDataType.SelectedIndex < 0) return;

                string dataTypeName = (cmbDataType.SelectedItem as CmbItem).NAME;
                var filter = from item in TrIDItems
                             where item.NAME.IndexOf(dataTypeName) > -1
                             select item;

                #region By node篩選
                if (cmbNode.SelectedIndex != -1)
                {
                    string _nodeNo = (cmbNode.SelectedItem as dynamic).NodeNo;

                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_nodeNo))
                    {
                        if (FormMainMDI.G_OPIAp.Dic_Node[_nodeNo].NodeAttribute == "NIKON")
                        {
                            filter = filter.Where(r => r.NAME.IndexOf("Nikon") > -1);
                        }
                        else
                        {
                            filter = filter.Where(r => r.NAME.IndexOf("Nikon") == -1);

                            #region By Line Type篩選 --- 2015-10-15 CY 提出 (just for T3 Rule)
                            // line type == DRY_ICD => 僅顯示for ID item
                            // line type != DRY_ICD => 僅顯示 for Name item
                            // modify by yang 2017/6/5  DRY_ICD,DRY_TEL,PHL都是report by ID
                            // CVD_AKT是name
                            // if (FormMainMDI.G_OPIAp.CurLine.LineType == "CVD_AKT")  modify by qiumin 20180409 DRY_TELreport by name
                            if (FormMainMDI.G_OPIAp.CurLine.LineType == "CVD_AKT" || FormMainMDI.G_OPIAp.CurLine.LineType == "DRY_TEL")
                            {
                                
                                filter = filter.Where(r => r.NAME.IndexOf("for name") > -1);
                            }
                            else
                            {
                                filter = filter.Where(r => r.NAME.IndexOf("for ID") > -1);
                            }
                            #endregion
                        }
                    }                    
                }                
                #endregion


                List<CmbItem> lstData = filter.ToList<CmbItem>();
                if (lstData.Count > 0)
                    this.cmbTrID.DataSource = lstData;
                else
                    this.cmbTrID.DataSource = TrIDItems;

                this.cmbTrID.DisplayMember = "NAME";
                this.cmbTrID.ValueMember = "ID";
                this.cmbTrID.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cmbNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbNode.SelectedIndex < 0) return;

                string _nodeNo = (cmbNode.SelectedItem as dynamic).NodeNo;

                Node _node = null;

                cmbDataType.SelectedIndexChanged -= new EventHandler(cmbDataType_SelectedIndexChanged);
                if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_nodeNo))
                {
                    _node = FormMainMDI.G_OPIAp.Dic_Node[_nodeNo];

                    if (_node.NodeAttribute == "NIKON")
                        this.cmbDataType.DataSource = DataTypeItems;
                    else
                    {
                        var _filter = DataTypeItems.Where(r => r.NAME != "Utility");
                        List<CmbItem> _lstData = _filter.ToList<CmbItem>();
                        this.cmbDataType.DataSource = _lstData;
                    }

                    this.cmbDataType.DisplayMember = "NAME";
                    this.cmbDataType.ValueMember = "ID";
                    this.cmbDataType.SelectedIndex = -1;
                    this.cmbTrID.SelectedIndex = -1;

                    cmbDataType.SelectedIndexChanged += new EventHandler(cmbDataType_SelectedIndexChanged);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void GetGridViewData(bool showMessage = false)
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                var selData = from msg in ctxBRM.SBRM_SECS_VARIABLEDATA
                              where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                              select msg;

                //已修改未更新物件
                var addData = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SECS_VARIABLEDATA>().Where(
                    msg => msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName);

                List<SBRM_SECS_VARIABLEDATA> objTables = selData.ToList();
                objTables.AddRange(addData.ToList());

                if (cmbNode.SelectedIndex != -1)
                    objTables = objTables.Where(msg => msg.NODENO == ((dynamic)this.cmbNode.SelectedItem).NodeNo).ToList();
                if (cmbDataType.SelectedIndex != -1)
                    objTables = objTables.Where(msg => msg.DATATYPE == cmbDataType.SelectedValue.ToString()).ToList();
                if (this.cmbTrID.SelectedIndex != -1)
                    objTables = objTables.Where(msg => msg.TRID == cmbTrID.SelectedValue.ToString()).ToList();

                dgvData.AutoGenerateColumns = false;
                dgvData.DataSource = objTables;

                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(dr.Cells[colNODENO.Name].Value.ToString()))
                        dr.Cells[colNodeID.Name].Value = FormMainMDI.G_OPIAp.Dic_Node[dr.Cells[colNODENO.Name].Value.ToString()].NodeID.ToString();
                }
                if (objTables.Count == 0 && showMessage == true)
                    ShowMessage(this, "Query Reuslt", "", "No matching data for your query！", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialCombox()
        {
            SetNode();
            SetSource();
            SetDataType();
            SetTrID();
        }

        private void SetNode()
        {
            var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                     where node.ReportMode.Contains(NODE_REPORTMODE_HSMS)
                     select new
                     {
                         IDNAME = string.Format("{0}-{1}-{2}", node.NodeNo, node.NodeID, node.NodeName),
                         node.NodeNo,
                         node.NodeID
                     }).ToList();

            if (q == null || q.Count == 0) return;

            cmbNode.SelectedIndexChanged -= new EventHandler(cmbNode_SelectedIndexChanged);
            cmbNode.DataSource = q;
            cmbNode.DisplayMember = "IDNAME";
            cmbNode.ValueMember = "NODEID";
            cmbNode.SelectedIndex = -1;
            cmbNode.SelectedIndexChanged += new EventHandler(cmbNode_SelectedIndexChanged);
        }

        private void SetSource()
        {
            var data = new[]             
            { 
                new { ID = "SBRM_RECIPEPARAMETER", NAME = "RecipeParameter" } ,
                new { ID = "SBRM_DAILYCHECKDATA", NAME = "DailycheckData" } ,
                new { ID = "SBRM_PROCESSDATA", NAME = "ProcessData" },
                new { ID = "SBRM_ENERGYVISUALIZATIONDATA",NAME = "EnergyvisualizationData"},
                new { ID = "SBRM_APCDATAREPORT",NAME = "APC Data Report"}
            };
            this.cmbSource.DataSource = data;
            this.cmbSource.DisplayMember = "NAME";
            this.cmbSource.ValueMember = "ID";
            this.cmbSource.SelectedIndex = 0;
        }

        private void SetDataType()
        {
            cmbDataType.SelectedIndexChanged -= new EventHandler(cmbDataType_SelectedIndexChanged);
            this.cmbDataType.DataSource = DataTypeItems;
            this.cmbDataType.DisplayMember = "NAME";
            this.cmbDataType.ValueMember = "ID";
            this.cmbDataType.SelectedIndex = -1;
            cmbDataType.SelectedIndexChanged += new EventHandler(cmbDataType_SelectedIndexChanged);
        }

        private void SetTrID()
        {
            this.cmbTrID.DataSource = TrIDItems; ;
            this.cmbTrID.DisplayMember = "NAME";
            this.cmbTrID.ValueMember = "ID";
            this.cmbTrID.SelectedIndex = -1;
        }

        private bool CheckUniqueKeyOK(string LineType, string LineID, string NodeNo, string DataType, string TrID, string strItemName)
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                int rowCnt = (from msg in ctxBRM.SBRM_SECS_VARIABLEDATA
                              where msg.LINEID == LineID && msg.LINETYPE == LineType && msg.NODENO == NodeNo && msg.DATATYPE == DataType
                              && msg.TRID == TrID && msg.ITEM_NAME == strItemName
                              select msg).Count();
                if (rowCnt > 0)
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1}, {2}, {3}, {4}, {5})！",
                        LineID, LineType, NodeNo, DataType, TrID, strItemName);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return false;
                }

                //已修改未更新物件
                if (FormMode == UniOPI.FormMode.AddNew)
                {
                    var add = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SECS_VARIABLEDATA>().Where(
                        msg => msg.LINEID == LineID && msg.LINETYPE == LineType && msg.NODENO == NodeNo && msg.DATATYPE == DataType
                              && msg.TRID == TrID && msg.ITEM_NAME == strItemName);
                    if (add.Count() > 0)
                    {
                        string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1}, {2}, {3}, {4}, {5})！",
                        LineID, LineType, NodeNo, DataType, TrID, strItemName);
                        ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                        return false;
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

        private void Save()
        {
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

            try
            {
                string _sqlDesc = string.Empty;

                string _sqlErr = string.Empty;

                try
                {
                    #region 取得更新的data

                    //新增
                    foreach (object objToInsert in _ctxBRM.GetChangeSet().Inserts)
                    {
                        if (objToInsert.GetType().Name != "SBRM_SECS_VARIABLEDATA") continue;

                        SBRM_SECS_VARIABLEDATA _updateData = (SBRM_SECS_VARIABLEDATA)objToInsert;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.TRID, _updateData.ITEM_NAME);
                    }

                    //delete
                    foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                    {
                        if (objToDelete.GetType().Name != "SBRM_SECS_VARIABLEDATA") continue;

                        SBRM_SECS_VARIABLEDATA _updateData = (SBRM_SECS_VARIABLEDATA)objToDelete;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.TRID, _updateData.ITEM_NAME);
                    }

                    //modify
                    foreach (object objToUpdate in _ctxBRM.GetChangeSet().Updates)
                    {
                        if (objToUpdate.GetType().Name != "SBRM_SECS_VARIABLEDATA") continue;

                        SBRM_SECS_VARIABLEDATA _updateData = (SBRM_SECS_VARIABLEDATA)objToUpdate;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.TRID, _updateData.ITEM_NAME);
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
                string _err = UniTools.InsertOPIHistory_DB("SBRM_SECS_VARIABLEDATA", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                Public.SendDatabaseReloadRequest("SBRM_SECS_VARIABLEDATA");

                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "SECS Variable Data Save Success！", MessageBoxIcon.Information);

                    SendToSECS(); //yang
                }

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //add by yang 2016/11/28 save后就让SECS处理
        private void SendToSECS()
        {
            try
            {
                if(!cmbSource.SelectedValue.ToString().Equals("SBRM_ENERGYVISUALIZATIONDATA")&&!cmbSource.SelectedValue.ToString().Equals("SBRM_APCDATAREPORT")
                                   &&!cmbSource.SelectedValue.ToString().Equals("SBRM_DAILYCHECKDATA"))
                    return;
                string secsName = string.Empty;

                string trID = (cmbTrID.SelectedItem as CmbItem).NAME;

                if (trID.ToUpper().Contains("FOR NAME"))
                    secsName = "S2F19";
                else if (trID.ToUpper().Contains("FOR ID"))
                    secsName = "S2F21";
                else
                    secsName = "S2F23";  //NIKON

                if (secsName.Length == 0)
                    return;

                List<SECSFunctionRequest.PARAMETERc> lstParam = new List<SECSFunctionRequest.PARAMETERc>();
         
                    SECSFunctionRequest.PARAMETERc param = new SECSFunctionRequest.PARAMETERc();

                    param.NAME = cmbDataType.SelectedValue.ToString();
                    param.VALUE = cmbTrID.SelectedValue.ToString();
                
                    lstParam.Add(param);

                SECSFunctionRequest _trx = new SECSFunctionRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = (cmbNode.SelectedItem as dynamic).NodeNo;
                _trx.BODY.SECSNAME = secsName;
                _trx.BODY.PARAMETERLIST = lstParam;

                string error = string.Empty;

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), out error, FormMainMDI.G_OPIAp.SessionID);

                ShowMessage(this, lblCaption.Text, "", "SECS Data Auto Send & Set Success！", MessageBoxIcon.Information);
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

    class CmbItem
    {
        public string ID { get; set; }
        public string NAME { get; set; }
    }
}
