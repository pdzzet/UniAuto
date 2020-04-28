using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormCassetteControl_Offline : FormBase
    {
        public Port curPort = new Port();
        public string MesControlMode = string.Empty;

        private bool IsShowDefault = false;
        private bool IsEmptyCST = false;  //是否為空CST - 空CST可不輸入 Port & Cassette 層資料 
        private bool IsRemap = false;
        private bool IsUseSetButton = false;

        private bool IsSetting = false; //是否使用Set button 設定資料中

        private Dictionary<string, Recipe> dicRecipe_All;      //目前mes mode下可使用的recipe     
        public Dictionary<string, Recipe> dicRecipeUse;        //被使用的recipe -- for recipe check use (local line)

        public Queue<string> Q_BCSReport { get; set; } //BC要OPI Pop的訊息

        private List<OfflineCassetteDataReply.LOTDATAc> LstLots;
        private OfflineCassetteDataReply.LOTDATAc CurLotData;
     
        private List<RangeCondition> LstRangeCondition;  //判斷TextBox輸入值是否符合VALUERAGNE欄位設定-- txtRange_TextChanged觸發判斷用
        private List<CheckEmpty> LstCheckEmpty;         //依據CHECKEMPTY欄位設定，為'Y'者則於mapdownload時判斷，不允許空值 --for Port & Cassette層用
        private List<string> LstCheckEmpty_Slot;         //依據CHECKEMPTY欄位設定，為'Y'者則於mapdownload時判斷，不允許空值 --for Slot層用

        #region for cutting line
        public Recipe Recipe_Lot_Cutting;        //Cutting Line Lot Recipe Name
        public Recipe Recipe_Proc_Cutting;       //Process Line Recipe Name
        public Recipe Recipe_STB_Cutting;        //STB Prod Recipe Name
        #endregion

        public FormCassetteControl_Offline()
        {
            InitializeComponent();

            lblCaption.Text = "Offline Cassette Control";

            #region 初始化
            LstRangeCondition = new List<RangeCondition>();
            LstCheckEmpty = new List<CheckEmpty>();
            LstCheckEmpty_Slot = new List<string>();
            Q_BCSReport = new Queue<string>();
            dicRecipe_All = new Dictionary<string, Recipe>();
            dicRecipeUse = new Dictionary<string, Recipe>();
            #endregion
        }

        public class chplan
        {
            public string glassID { get; set; }
            public bool processFlag { get; set; }
        }

        List<chplan> cplan = new List<chplan>();

        private void FormCassetteControl_Offline_Load(object sender, EventArgs e)
        {
            try
            {
                bool _processFlag = false;
                string _slotNo = string.Empty;
                string _glassID = string.Empty;
                string _TargetCstID = string.Empty;
                string _msg = string.Empty;

                #region Glass ID 長度限制 -- Array & CF：最大8 码,CELL： 最大7码
                txtGlassID.MaxLength = FormMainMDI.G_OPIAp.GlassIDMaxLength;
                #endregion

                #region Check
                if (curPort.CassetteID == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Cassette ID is empty", MessageBoxIcon.Error);
                    return;
                }

                if (MesControlMode == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Mes Control Mode is unknown", MessageBoxIcon.Error);
                    return;
                }

                if (curPort.PortID == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Port ID is unknown", MessageBoxIcon.Error);
                    return;
                }
                #endregion

                //初始化Form上面的資訊
                InitialFormInfo();

                //取得畫面顯示物件
                GetCSTControlDef();

                //重新取得recipe check 設定~ 避免被修改過
                UniTools.Reload_RecipeSetting();
                
                //讀取 DB PPID 資料
                LoadPPIDSetting();

                //需要在InitialFormInfo()後面
                lblPortName.Text = curPort.PortID.ToString().PadLeft(2, '0');
                txtCassetteID.Text = curPort.CassetteID;
                txtSlotGlassCount.Text = curPort.PortGlassCount.ToString();

                IsRemap = false;
                switch (curPort.SubCassetteStatus)
                {
                    case "WACSTEDIT":

                        #region WACSTEDIT : Wait for OPI Edit Cassette Data (for online local/Offline)

                        #region by port type 判斷是否為空CST

                        switch (curPort.PortType)
                        {
                            case ePortType.BothPort:

                                #region Both Port -- 若機台會上報JobExistenceSlot，則判斷 port count = 0,則視為空CST下貨，否則需產生資料下貨 ; 若機台不會上報JobExistenceSlot，則當為非空CST由USER自行輸入
                                if (curPort.MapplingEnable)
                                {
                                    if (curPort.PortGlassCount == 0) IsEmptyCST = true;
                                    else IsEmptyCST = false;                                    
                                }
                                else IsEmptyCST = false; 
                                #endregion

                                break;

                            case ePortType.UnloadingPort:

                                #region Parital Full Mode 是針對 CF及CELL, ARRAY不用Parital Full Mode 都可輸入資料 ---- 2015.01.27 俊成

                                if (FormMainMDI.G_OPIAp.CurLine.FabType == "ARRAY")
                                {
                                    if (curPort.PortGlassCount == 0) IsEmptyCST = true;
                                }
                                else
                                {
                                    if (curPort.PartialFullMode == false) 
                                    {
                                        IsEmptyCST = true;
                                    }
                                    else
                                    {
                                        if (curPort.PortGlassCount == 0) IsEmptyCST = true;
                                    }                                    
                                }
                                #endregion

                                break;

                            case ePortType.LoadingPort:

                                IsEmptyCST = false;

                                //ARRAY CAC Loader可上空CST, CELL CLN Loader可上空CST
                                if (FormMainMDI.G_OPIAp.CurLine.LineType == "CAC_MYTEK" || FormMainMDI.G_OPIAp.CurLine.LineType == "CLN")
                                {
                                    if (curPort.PortGlassCount == 0) IsEmptyCST = true;
                                }

                                break;

                            default :
                                break;
                        }
                        #endregion

                        
                        #region 依據[SBRM_PORT]->MAPPINGENABLE判斷機台是否會上報JobExistenceSlot, 有上報JobExistenceSlot port判斷是否與port max count長度一致 & JobExistenceSlot count 是否與 port glass count數量是否一致
                        if (curPort.MapplingEnable)
                        {
                            //JobExistenceSlot port判斷是否與port max count長度一致
                            if (curPort.JobExistenceSlot.Length != curPort.MaxCount)
                            {
                                ShowMessage(this, lblCaption.Text, "", string.Format("JobExistenceSlot length [{0}] != Port Max Count [{1}]", curPort.JobExistenceSlot.Length.ToString(), curPort.MaxCount.ToString()), MessageBoxIcon.Error);

                                return;
                            }

                            //JobExistenceSlot count 是否與 port glass count數量是否一致
                            int _cnt = 0 ;
                            for (int i = 0; i < curPort.JobExistenceSlot.Length; i++)
                            {
                                if (curPort.JobExistenceSlot.Substring(i, 1) == "1") _cnt = _cnt + 1;
                            }
                                   
                            if (_cnt != curPort.PortGlassCount)
                            {
                                _msg = string.Format("Job Existence Slot Count [{0}] is different from  Port Glass Count [{1}]", (_cnt).ToString(), curPort.PortGlassCount.ToString());
                                ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);

                                return;
                            }
                        }                                
                        #endregion
                                                
                        if (IsEmptyCST)
                        {
                            #region Port type == UD && (不為partial full or partial full mode 但 port count = 0) 時，鎖定畫面，僅提供map download button
                            EnableGlassData(false);
                            btnRecipeCheck.Enabled = false;
                            btnSet.Enabled = false;
                            btnRecipe.Enabled = false;
                            btnLotRecipeID.Enabled = false;
                            btnMapDownload.Enabled = true;
                            #endregion
                        }
                        else
                        {                            
                            EnableGlassData(true);

                            IsShowDefault = true;
                            SetDefaultData();

                            #region DataGridView產生slot , 3:changer mode
                            if (FormMainMDI.G_OPIAp.CurLine.IndexerMode == 3 && (curPort.PortType != ePortType.UnloadingPort ))
                            {
                                if (FormMainMDI.G_OPIAp.CurLine.ChangerPlanID == string.Empty)
                                {
                                    ShowMessage(this, lblCaption.Text , "", "Plan ID is empty", MessageBoxIcon.Error);

                                    return;
                                }

                                #region changer mode時，讀取changer plan設定
                                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                                var selData = from d in ctxBRM.SBRM_CHANGEPLAN
                                              where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) &&                                                 
                                                  d.SOURCECASSETTEID.Equals(curPort.CassetteID)
                                              select d;

                                List<SBRM_CHANGEPLAN> _lstChangePlan = selData.ToList();

                                if (_lstChangePlan.Count == 0)
                                {
                                    _msg = string.Format("Can't find Plan Data for plan id [{0}], Casseet ID [{1}]!", FormMainMDI.G_OPIAp.CurLine.ChangerPlanID, curPort.CassetteID);

                                    ShowMessage(this, lblCaption.Text , "", _msg, MessageBoxIcon.Error);

                                    return;
                                }

                                dgvProduct.Rows.Clear();

                                for (int i = curPort.MaxCount; i > 0 ; i--)
                                {
                                    _slotNo = i.ToString().PadLeft(3, '0');

                                    if (curPort.MapplingEnable == false) _processFlag = false;
                                    else _processFlag = curPort.JobExistenceSlot.Substring(i - 1, 1) == "1" ? true : false;
                                    
                                    SBRM_CHANGEPLAN _plan = _lstChangePlan.Find(r => r.SLOTNO == _slotNo);
                                    if (_plan == null)
                                    {
                                        _glassID = string.Empty;
                                    }
                                    else
                                    {
                                        _glassID = _plan.JOBID;
                                        _TargetCstID = _plan.TARGETASSETTEID;
                                        cplan.Add(new chplan() { glassID = _glassID, processFlag = _processFlag });
                                    }

                                    //colChoose,     colSlotNo,      colGlassID,         colProcessFlag,    colRecipeID,        colPPID,
                                    //colJobType,    colProcessType, colJobGrade,        colJobJudge,       colGroupIndex,      colSubstrateType,
                                    //colTargetCSTID,colCOAVersion,  colInspReservations,colPreInlineID,    colFlowPriorityInfo,colNetworkNo 
                                    //colEQPFlag,    colOwnerID,     colOwnerType,       colRevProcOperName,colPanelSize,       colTurnAngleFlag
                                    //colBlockSize,  colTargetSlotNo,colScrapCutFlag,    colInlineReworkCount,colOXR    ,       colVendorName
                                    //colRejudgeCount,colBURCheckCount,colDotRepairCount,colLineRepairCount  , colMaxReworkCount,colCurrentReworkCount
                                    //colOQCBank
                                    dgvProduct.Rows.Add(
                                            false,        _slotNo,      _glassID,     _processFlag, string.Empty, string.Empty,
                                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                            _TargetCstID, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                            string.Empty);
                                }
                                chkGlassID.Checked = false;
                                chkGlassID.ForeColor = Color.Black;
                                chkGlassID.BackColor = Color.Transparent;
                                #endregion
                            }
                            else
                            {
                                #region 產生空的slot

                                dgvProduct.Rows.Clear();

                                for (int i = curPort.MaxCount; i > 0; i--)
                                {
                                    _slotNo = i.ToString().PadLeft(3, '0');

                                    if (curPort.MapplingEnable == false) _processFlag = false;
                                    else _processFlag = curPort.JobExistenceSlot.Substring(i - 1, 1) == "1" ? true : false;


                                    //colChoose,     colSlotNo,      colGlassID,         colProcessFlag,    colRecipeID,        colPPID,
                                    //colJobType,    colProcessType, colJobGrade,        colJobJudge,       colGroupIndex,      colSubstrateType,
                                    //colTargetCSTID,colCOAVersion,  colInspReservations,colPreInlineID,    colFlowPriorityInfo,colNetworkNo 
                                    //colEQPFlag,    colOwnerID,     colOwnerType,       colRevProcOperName,colPanelSize,       colTurnAngleFlag
                                    //colBlockSize,  colTargetSlotNo,colScrapCutFlag,    colInlineReworkCount,colOXR ,       colVendorName
                                    // colRejudgeCount,colBURCheckCount,colDotRepairCount,colLineRepairCount , colMaxReworkCount,colCurrentReworkCount
                                    //colOQCBank
                                    dgvProduct.Rows.Add(
                                            false,        _slotNo,      string.Empty, _processFlag, string.Empty, string.Empty,
                                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                            _TargetCstID, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                            string.Empty, "0", string.Empty, string.Empty, string.Empty, string.Empty,
                                            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                            string.Empty);

                                }

                                #endregion
                            }
                            #endregion

                            #region 取得起始&結束對應的slot no，若不會上報則預設從01開始
                            if (curPort.MapplingEnable ==false )
                            {
                                #region 從 slot01開始建帳
                                chkJobExist.Checked = false;
                                txtSelFrSlotNo.Text = "1";
                                txtSelToSlotNo.Text = curPort.MaxCount.ToString();
                                txtSelFrSlotNo.Enabled = false;
                                #endregion
                            }
                            else
                            {
                                #region 取得JobExistenceSlot起始&結束對應的slot no
                                int _start = curPort.JobExistenceSlot.IndexOf('1', 0);
                                int _end = curPort.JobExistenceSlot.LastIndexOf("1");

                                txtSelFrSlotNo.Text = (_start + 1).ToString();
                                txtSelToSlotNo.Text = (_end + 1).ToString();

                                if (_end + 1  > curPort.MaxCount)
                                {
                                    _msg = string.Format("End Slot No [{0}] >  Port Max Count [{1}]", (_end + 1).ToString(), curPort.MaxCount.ToString());
                                    ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);

                                    return;
                                }
                                #endregion

                                #region Dense須從slot 01開始
                                if (curPort.PortAttribute == "DENSE")
                                {
                                    if (_start > 0)
                                    {
                                        ShowMessage(this, lblCaption.Text, "", "Start Slot No >  1", MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                                #endregion
                            }
                            #endregion

                            btnRecipeCheck.Enabled = true;
                            btnSet.Enabled = true;
                            btnRecipe.Enabled = true;
                            btnLotRecipeID.Enabled = true;
                            btnMapDownload.Enabled = true;
                        }
                        break;
                        #endregion

                    case "WAREMAPEDIT":

                        #region WAREMAPEDIT : Wait for OPI remap edit Cassette Data.
                        EnableGlassData(false);

                        IsRemap = true;
                        IsShowDefault = true;
                        SetDefaultData();

                        btnRecipeCheck.Enabled = false;
                        btnMapDownload.Enabled = false;

                        #region 產生空的slot

                        dgvProduct.Rows.Clear();

                        for (int i = curPort.MaxCount; i > 0; i--)
                        {
                            _slotNo = i.ToString().PadLeft(3, '0');

                            //colChoose,     colSlotNo,      colGlassID,         colProcessFlag,    colRecipeID,        colPPID,
                            //colJobType,    colProcessType, colJobGrade,        colJobJudge,       colGroupIndex,      colSubstrateType,
                            //colTargetCSTID,colCOAVersion,  colInspReservations,colPreInlineID,    colFlowPriorityInfo,colNetworkNo 
                            //colEQPFlag,    colOwnerID,     colOwnerType,       colRevProcOperName,colPanelSize,       colTurnAngleFlag
                            //colBlockSize,  colTargetSlotNo,colScrapCutFlag,    colInlineReworkCount,colOXR,       colVendorName
                            // colRejudgeCount,colBURCheckCount,colDotRepairCount,colLineRepairCount , colMaxReworkCount,colCurrentReworkCount
                            //colOQCBank
                            dgvProduct.Rows.Add(
                                    false,       _slotNo,       string.Empty, false,        string.Empty, string.Empty,
                                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                    _TargetCstID, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                    string.Empty, "0",          string.Empty, string.Empty, string.Empty, string.Empty,
                                    string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                    string.Empty);

                        }

                        #endregion

                        #region 取得JobExistenceSlot起始&結束對應的slot no
                        int _startInx = curPort.JobExistenceSlot.IndexOf('1', 0);
                        int _endInx = curPort.JobExistenceSlot.LastIndexOf("1");

                        txtSelFrSlotNo.Text = (_startInx + 1).ToString();
                        txtSelToSlotNo.Text = (_endInx + 1).ToString();

                        if (_endInx + 1 > curPort.MaxCount)
                        {
                            ShowMessage(this, lblCaption.Text, "", "End Slot No >  Port Max Count", MessageBoxIcon.Error);

                            return;
                        }
                        #endregion

                        SendtoBC_CassetteDataRequest();//傳送和BC要 CST Data  
                        break;
                        #endregion

                    default:
                        
                        EnableGlassData(false);

                        btnRecipeCheck.Enabled = false;
                        btnSet.Enabled = false;
                        btnRecipe.Enabled = false;
                        btnLotRecipeID.Enabled = false;
                        btnMapDownload.Enabled = false;
                        break;
                }

                tmrRefresh.Enabled = true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void FormCassetteControl_Offline_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                tmrRefresh.Enabled = false;

                if (IsShowDefault)
                {
                    #region 存入CSTCtrlDefaultData --畫面開啟時給的defalue
                    CSTCtrlDefaultData _def = null;
                    FlowLayoutPanel _flowPanel = null;

                    string _chkName = string.Empty;
                    string _objName = string.Empty;
                    eObjectType _objType = eObjectType.TextBox;
                    bool _chkValue = false;
                    string _objValue = string.Empty;

                    #region Product層item
                    for (int i = 0; i < 2; i++)
                    {
                        if (i == 0) _flowPanel = flpSlotData;
                        else _flowPanel = flpSlotData_Special;

                        foreach (Panel _pnl in _flowPanel.Controls.OfType<Panel>())
                        {
                            _def = FormMainMDI.G_OPIAp.CurLine.LstCstControlDefaultData.Find(r => r.PanelName.Equals(_pnl.Name));

                            if (_pnl.Visible == false)
                            {
                                if (_def != null) FormMainMDI.G_OPIAp.CurLine.LstCstControlDefaultData.Remove(_def);
                                continue;
                            }
                            else
                            {
                                if (_pnl.Name == "pnlProcessFlag")
                                {
                                    _chkName = "chkProcess";
                                    _objName = "chkProcessFlag";

                                    _objType = eObjectType.CheckBox;

                                    _chkValue = chkProcess.Checked;
                                    _objValue = chkProcessFlag.Checked ? "Y" : "N";
                                }
                                else if (_pnl.Name == "pnlTargetCSTID")
                                {
                                    continue;
                                }
                                else
                                {
                                    foreach (Control _ctrl in _pnl.Controls)
                                    {
                                        switch (_ctrl.GetType().Name.ToString())
                                        {
                                            case "TextBox":
                                                _objType = eObjectType.TextBox;
                                                _objName = _ctrl.Name;
                                                _objValue = _ctrl.Text;
                                                break;

                                            case "ComboBox":
                                                _objType = eObjectType.ComboBox;
                                                _objName = _ctrl.Name;

                                                if (_objName == "cboJobGrade") _objValue = ((ComboBox)_ctrl).Text.ToString();
                                                else
                                                {
                                                    if (((ComboBox)_ctrl).SelectedValue == null) _objValue = string.Empty;
                                                    else _objValue = ((ComboBox)_ctrl).SelectedValue.ToString();
                                                }
                                                break;

                                            case "CheckBox":
                                                _chkName = _ctrl.Name;
                                                _chkValue = (_chkName== "chkGlassID" ? true : ((CheckBox)_ctrl).Checked);
                                                break;

                                            default:
                                                break;
                                        }
                                    }
                                }

                                if (_def == null)
                                {
                                    _def = new CSTCtrlDefaultData();
                                    _def.PanelName = _pnl.Name;
                                    _def.CheckBoxName = _chkName;
                                    _def.CheckBoxValue = _chkValue;
                                    _def.ObjectName = _objName;
                                    _def.ObjectType = _objType;
                                    _def.ObjectValue = _objValue;

                                    FormMainMDI.G_OPIAp.CurLine.LstCstControlDefaultData.Add(_def);
                                }
                                else
                                {
                                    _def.PanelName = _pnl.Name;
                                    _def.CheckBoxName = _chkName;
                                    _def.CheckBoxValue = _chkValue;
                                    _def.ObjectName = _objName;
                                    _def.ObjectType = _objType;
                                    _def.ObjectValue = _objValue;
                                }
                            }
                        }
                    }
                    #endregion

                    #region Lot層item

                    if (flpLotRecipeID.Visible)
                    {
                        _def = FormMainMDI.G_OPIAp.CurLine.LstCstControlDefaultData.Find(r => r.PanelName.Equals("flpLotRecipeID"));

                        if (_def == null)
                        {
                            _def = new CSTCtrlDefaultData();

                            FormMainMDI.G_OPIAp.CurLine.LstCstControlDefaultData.Add(_def);
                        }

                        _def.PanelName = "flpLotRecipeID";
                        _def.CheckBoxName = string.Empty;
                        _def.CheckBoxValue = true;
                        _def.ObjectName = string.Empty;
                        _def.ObjectType = eObjectType.TextBox;
                        _def.ObjectValue = string.Empty;

                        _def.LotRecipeName = txtLotRecipeID.Tag == null ? string.Empty : txtLotRecipeID.Tag.ToString();
                        _def.LotRecipeID = txtLotRecipeID.Text.ToString();
                    }

                    //bool _add = false;
                    //if (OPIConst.LstLineType_Cutting.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                    //{
                    //    _def = FormMainMDI.G_OPIAp.CurLine.LstCstControlDefaultData.Find(r => r.PanelName.Equals("flpCuttingRecipeID"));

                    //    if (_def == null)
                    //    {
                    //        _add = true;
                    //        _def = new CSTCtrlDefaultData();
                    //    }

                    //    _def.PanelName = "flpCuttingRecipeID";
                    //    _def.CheckBoxName = string.Empty;
                    //    _def.CheckBoxValue = true;
                    //    _def.ObjectName = string.Empty;
                    //    _def.ObjectType = eObjectType.TextBox;
                    //    _def.ObjectValue = string.Empty;

                    //    _def.CurrentLinePPID = txtLotRecipeID.Text;
                    //    _def.Recipe_Lot_Cutting = Recipe_Lot_Cutting;
                    //    _def.Recipe_Proc_Cutting = Recipe_Proc_Cutting;
                    //    _def.Recipe_STB_Cutting = Recipe_STB_Cutting;

                    //    if (_add) FormMainMDI.G_OPIAp.CurLine.LstCstControlDefaultData.Add(_def);
                    //}
                    
                    #endregion

                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvProduct_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            try
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, anError.Exception.ToString());
                MessageBox.Show("dgvProduct Error happened " + anError.Context.ToString());

                if (anError.Context == DataGridViewDataErrorContexts.Commit)
                {
                    MessageBox.Show("Commit error");
                }
                if (anError.Context == DataGridViewDataErrorContexts.CurrentCellChange)
                {
                    MessageBox.Show("Cell change");
                }
                if (anError.Context == DataGridViewDataErrorContexts.Parsing)
                {
                    MessageBox.Show("parsing error");
                }
                if (anError.Context == DataGridViewDataErrorContexts.LeaveControl)
                {
                    MessageBox.Show("leave control error");
                }

                if ((anError.Exception) is ConstraintException)
                {
                    DataGridView view = (DataGridView)sender;
                    view.Rows[anError.RowIndex].ErrorText = "an error";
                    view.Rows[anError.RowIndex].Cells[anError.ColumnIndex].ErrorText = "an error";

                    anError.ThrowException = false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvProduct_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvProduct.IsCurrentCellDirty)
            {
                dgvProduct.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }    
        }

        private void dgvProduct_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            try
            {
                string _colName = dgvProduct.Columns[e.ColumnIndex].Name;
                string _errMsg = string.Empty;

                switch (_colName)
                {
                    case "colProcessFlag":

                        #region 跳片判斷 --DENSE 從slot01開始建帳，不可跳片 EX:產生了15片帳 slot01-15~ 我只要抽10片，就要從slot01-05取消
                        if (IsUseSetButton) break;
                        if (curPort.PortAttribute == "DENSE")
                        {
                            if ((bool)dgvProduct["colProcessFlag", e.RowIndex].Value == true)
                            {                                
                                if (e.RowIndex > 0)
                                {
                                    //選取須由上往下勾選
                                    if ((bool)dgvProduct["colProcessFlag", e.RowIndex - 1].Value == false && dgvProduct["colGlassID", e.RowIndex - 1].Value.ToString() != string.Empty)
                                    {
                                        dgvProduct["colProcessFlag", e.RowIndex].Value = false;
                                    }
                                }
                            }
                            else if ((bool)dgvProduct["colProcessFlag", e.RowIndex].Value == false) 
                            {
                                //若要取消flag,下面一片process flag不可以被勾選(跳片)--由下往上取消勾選
                                if (e.RowIndex < dgvProduct.Rows.Count - 1)
                                {
                                    if ((bool)dgvProduct["colProcessFlag", e.RowIndex + 1].Value == true)
                                    {
                                        dgvProduct["colProcessFlag", e.RowIndex].Value = true;
                                    }
                                }
                            }

                        }
                        #endregion

                        break;

                    case "colGlassID":

                        #region 不可輸入字母I O i o

                        if (dgvProduct[colGlassID.Name, e.RowIndex].Value == null ||
                            dgvProduct[colGlassID.Name, e.RowIndex].Value.ToString() == string.Empty) return;

                        string _glassID = dgvProduct[colGlassID.Name, e.RowIndex].Value.ToString().ToUpper();

                        foreach (Char _chr in _glassID)
                        {
                            if (_chr == 'I' || _chr == 'O')
                            {
                                ShowMessage(this, lblCaption.Text, "", string.Format("I and O are invalid data "), MessageBoxIcon.Error);
                                dgvProduct[colGlassID.Name, e.RowIndex].Value = string.Empty;
                                return;
                            }
                        }

                        dgvProduct[colGlassID.Name, e.RowIndex].Value = _glassID;

                        #endregion

                        break;

                    default: break;

                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvProduct_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            try
            {
                string _colName = dgvProduct.Columns[e.ColumnIndex].Name;
                string _errMsg = string.Empty;

                switch (_colName)
                {
                    case "colRecipeID":
                        #region 判斷recipe 是否存在
                        if (dgvProduct[colRecipeID.Name, e.RowIndex].Value == null)
                        {
                            dgvProduct[colPPID.Name, e.RowIndex].Value = string.Empty;
                            return;
                        }

                        if (dgvProduct[colRecipeID.Name, e.RowIndex].Value.ToString() == string.Empty)
                        {
                            dgvProduct[colPPID.Name, e.RowIndex].Value = string.Empty;
                            return;
                        }

                        string _recipe = dgvProduct[colRecipeID.Name, e.RowIndex].Value.ToString();

                        if (dicRecipe_All.ContainsKey(_recipe))
                        {
                            dgvProduct[colPPID.Name, e.RowIndex].Value = dicRecipe_All[_recipe].PPID;
                        }
                        else
                        {
                            dgvProduct[colRecipeID.Name, e.RowIndex].Value = txtRecipeID.Text.ToString();
                        }
                        #endregion
                        break;

                    case "colProcessFlag":

                        #region 跳片判斷 -- DENSE 從slot01開始建帳，不可跳片 EX:產生了15片帳 slot01-15~ 我只要抽10片，就要從slot01-05取消
                        if (IsUseSetButton) break;
                        if (curPort.PortAttribute == "DENSE")
                        {
                            if ((bool)dgvProduct[colProcessFlag.Name, e.RowIndex].Value == true)
                            {
                                if (e.RowIndex > 0)
                                {
                                    //選取須由上往下勾選
                                    if ((bool)dgvProduct[colProcessFlag.Name, e.RowIndex - 1].Value == false && dgvProduct[colGlassID.Name, e.RowIndex - 1].Value.ToString() != string.Empty)
                                    {
                                        ShowMessage(this, lblCaption.Text, "", string.Format("sampling flag of slot {0} must be choose", dgvProduct.Rows[e.RowIndex - 1].Cells[colSlotNo.Name].Value), MessageBoxIcon.Error);
                                    }
                                }
                            }
                            else if ((bool)dgvProduct[colProcessFlag.Name, e.RowIndex].Value == false)
                            {
                                //若要取消flag,下面一片process flag不可以被勾選(跳片)--由下往上取消勾選
                                if (e.RowIndex < dgvProduct.Rows.Count - 1)
                                {
                                    if ((bool)dgvProduct[colProcessFlag.Name, e.RowIndex + 1].Value == true)
                                    {
                                        ShowMessage(this, lblCaption.Text, "", string.Format("sampling flag of slot {0} is choose", dgvProduct.Rows[e.RowIndex + 1].Cells[colSlotNo.Name].Value), MessageBoxIcon.Error);

                                    }
                                }
                            }
                        }
                        #endregion

                        break;

                    case "colChoose":

                        if (IsSetting) break;

                        #region Choose
                        if (((bool)dgvProduct.Rows[e.RowIndex].Cells[colChoose.Name].Value) == true)
                        {
                            #region 將預設值帶入勾選的slot
                            int _slotNo = 0;

                            int.TryParse(dgvProduct.Rows[e.RowIndex].Cells[colSlotNo.Name].Value.ToString(), out _slotNo);

                            SetGlassData(_slotNo, _slotNo,false);

                            #endregion
                        }
                        else
                        {
                            #region 清除data
                            foreach (DataGridViewColumn _col in dgvProduct.Columns)
                            {
                                if (_col.Name == "colProcessFlag" || _col.Name == "colChoose")
                                    dgvProduct.Rows[e.RowIndex].Cells[_col.Name].Value = false;
                                else if (_col.Name == "colSlotNo")
                                    continue;
                                else
                                    dgvProduct.Rows[e.RowIndex].Cells[_col.Name].Value = string.Empty;
                            }

                            #endregion
                        }
                        #endregion

                        break;

                    default: break;

                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRecipe_Click(object sender, EventArgs e)
        {
            try
            {
                //讀取 DB PPID 資料
                if (dicRecipe_All.Count == 0) LoadPPIDSetting();

                FormCassetteControl_RecipeChange frm = new FormCassetteControl_RecipeChange(dicRecipe_All, MesControlMode);
                frm.DgvRows = dgvProduct.Rows;
                frm.ShowDialog();

                if (frm != null) frm.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnMapDownload_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender;
                int _num = 0;
                string _msg = string.Empty;
                string _recipeMsg = string.Empty;

                string _recipeName = string.Empty;
                string _ppid = string.Empty;
                string _slotNo = string.Empty;
                string _glassID = string.Empty;
                string _data = string.Empty;
                string _curPPID = string.Empty;  //用來判斷slot 內的PPID是否一致(for CVD & DRY 在mix run mode時判斷用，同CST的slot內的PPID都要相同)
                string _acJobType = string.Empty; // 用來判斷 job type是否都一致 
                bool IsProcess = false;  //用來判斷是否至少有一筆sampling flag = "Y" 

                txtReturnCode.Text = string.Empty;

                //讀取 DB PPID 資料
                if (dicRecipe_All.Count == 0) LoadPPIDSetting();

                #region Check offline下貨需卡產生glass id要與JobExistenceSlot 一致 --會上報JobExistenceSlot者才須check

                if (curPort.MapplingEnable)
                {
                    foreach (DataGridViewRow _row in dgvProduct.Rows)
                    {
                        _slotNo = _row.Cells[colSlotNo.Name].Value.ToString();
                        _glassID = (_row.Cells[colGlassID.Name].Value==null ? string.Empty  :_row.Cells[colGlassID.Name].Value.ToString());

                        int.TryParse(_slotNo,out _num);

                        if (curPort.JobExistenceSlot.Substring(_num-1, 1) == "0" && _glassID != string.Empty)
                        {
                            _msg = string.Format("Glass ID in Slot No [{0}] must be empty", _slotNo);
                            ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);
                            return;
                        }

                        if (curPort.JobExistenceSlot.Substring(_num-1, 1) == "1" && _glassID == string.Empty)
                        {
                            _msg = string.Format("can't find Slot No [{0}]", _slotNo);
                            ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                #endregion

                #region[PCS CHECK PCSCSTSETTINGCODELIST 是否为1001形式:20170717huangjiayin]
                if (FormMainMDI.G_OPIAp.CurLine.LineType == "PCS")
                {
                    foreach (char c in txtPCSCassetteSettingCodeList.Text)
                    {
                        if (c != '1' && c != '0')
                        {
                            ShowMessage(this, lblCaption.Text, "", "PCSCSTSETTINGCODE LIST Must be like [01010101...]!", MessageBoxIcon.Error);
                            return;
                        }
                    }

                    foreach (char c in txtPCSBlockSizeList.Text )
                    {
                        if (c != '1' && c != '0')
                        {
                            ShowMessage(this, lblCaption.Text, "", "PCSBLOCKSIZE LIST Must be like [01010101...]!", MessageBoxIcon.Error);
                            return;
                        }
                    }

                    //Code2 不为空的情况下，CodeList必须包含1；反之，不可包含1
                    if (string.IsNullOrEmpty(txtCassetteSettingCode_PCS2.Text) && txtPCSCassetteSettingCodeList.Text.Contains("1"))
                    {
                        ShowMessage(this, lblCaption.Text, "", "PCSCassetteSettingCode LIST has [1], But PCSCassetteSettingCode2 is Null!", MessageBoxIcon.Error);
                        return;
                    }
                    if (!string.IsNullOrEmpty(txtCassetteSettingCode_PCS2.Text) && !txtPCSCassetteSettingCodeList.Text.Contains("1"))
                    {
                        ShowMessage(this, lblCaption.Text, "", "PCSCassetteSettingCode LIST has no [1], But PCSCassetteSettingCode2 exists!", MessageBoxIcon.Error);
                        return;
                    }

                    //BlockSize2 不为空的情况下，CodeList必须包含1；反之，不可包含1
                    if (txtBlockSize2.Text == "0" && txtPCSBlockSizeList.Text.Contains("1"))
                    {
                        ShowMessage(this, lblCaption.Text, "", "PCSBLOCKSIZE LIST has [1], But BlockSize2 is 0!", MessageBoxIcon.Error);
                        return;
                    }
                    if (txtBlockSize2.Text != "0" && !txtPCSBlockSizeList.Text.Contains("1"))
                    {
                        ShowMessage(this, lblCaption.Text, "", "PCSBLOCKSIZE LIST has no [1], But BlockSize2 is not 0!", MessageBoxIcon.Error);
                        return;
                    }






                }
                #endregion

                if (!IsEmptyCST)
                {
                    #region Port & Cassette 層TextBox 依照DB設定 (SBRM_CST_CONTROL_DEF->CHECKEMPTY)判斷是否為必填欄位 -- 非空CST才需要判斷
                    foreach (CheckEmpty _chk in LstCheckEmpty)
                    {
                        if (_chk.TxtObject != null)
                        {
                            if (_chk.TxtObject.Text == string.Empty)
                            {
                                if (_chk.TxtObject.Name == "txtReworkMaxCount")
                                {
                                    #region FCREW_TYPE1 的 Port 是 LD 且 Product Type 非 Dummy，Rework Max Count 必填且需大於零 -- 其他情況非必填但需自動填零，避免字串轉整數時出錯
                                    if (FormMainMDI.G_OPIAp.CurLine.LineType == "FCREW_TYPE1")
                                    {
                                        string _jobType = string.Empty;

                                        //取得job type
                                        foreach (DataGridViewRow _row in dgvProduct.Rows.Cast<DataGridViewRow>().Where(r => r.Cells["colProcessFlag"].Value.ToString().ToUpper().Equals("TRUE")))
                                        {
                                            _jobType = _row.Cells[colJobType.Name].Value == null ? "0" : _row.Cells[colJobType.Name].Value.ToString();
                                            break;
                                        }

                                        if (curPort.PortType == ePortType.LoadingPort && _jobType != "3")
                                        {
                                            ShowMessage(this, lblCaption.Text, "", _chk.RangeText, MessageBoxIcon.Error);
                                            _chk.TxtObject.Focus();
                                            return;
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    ShowMessage(this, lblCaption.Text, "", _chk.RangeText, MessageBoxIcon.Error);
                                    _chk.TxtObject.Focus();
                                    return;
                                }
                            }
                        }
                        else if (_chk.CboObject != null)
                        {
                            if (_chk.CboObject.SelectedValue == null || _chk.CboObject.SelectedValue.ToString() == string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "", _chk.RangeText, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Port 層Cassette Setting Code不管是否空CST / LD UD都需要填 -- CELL
                    if (pnlCassetteSettingCode_Port.Visible)
                    {
                        if (txtCassetteSettingCode_Port.Text.ToString().Trim() == string.Empty)
                        {
                            ShowMessage(this, lblCaption.Text, "", "Please input Cassette Setting Code", MessageBoxIcon.Error);

                            txtCassetteSettingCode_Port.Focus();

                            return;
                        }
                    }
                    #endregion   
                }


                #region 判斷sampling flag = "Y" 的slot是否都有glass id & recipe id,且至少要有一筆sampling flag = "Y" , AC廠的job type 都要一樣
                foreach (DataGridViewRow _row in dgvProduct.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[colProcessFlag.Name].Value.ToString().ToUpper().Equals("TRUE")))
                {
                    IsProcess = true;

                    _slotNo = _row.Cells[colSlotNo.Name].Value.ToString();

                    #region 判斷sampling flag = "Y" 的slot是否都有glass id
                    if (_row.Cells[colGlassID.Name].Value==null || _row.Cells[colGlassID.Name].Value.ToString().Trim() == string.Empty)
                    {                        
                        _msg = string.Format("No Glass ID in Slot No [{0}]", _slotNo);
                        ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);

                        return;
                    }
                    #endregion

                    _glassID = _row.Cells[colGlassID.Name].Value.ToString();

                    #region 判斷colGlassID是否有重複
                    if (colGlassID.ReadOnly ==false)
                    {
                        List < DataGridViewRow> _lstGlassID = dgvProduct.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[colProcessFlag.Name].Value.ToString().ToUpper().Equals("TRUE") &&
                                                                            r.Cells[colGlassID.Name].Value.ToString().Equals(_glassID)).ToList();

                        if (_lstGlassID.Count() > 1)
                        {

                            foreach (DataGridViewRow _dupRow in _lstGlassID)
                            {
                                _msg = _msg + string.Format("Slot [{0}]  \r\n", _dupRow.Cells[colSlotNo.Name].Value.ToString());
                            }

                            _msg = string.Format("Glass ID [{0}] is duplicate in  \r\n", _glassID) + _msg ;

                            ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);

                            return;
                        }
                    }
                    #endregion

                    #region 判斷sampling flag = "Y" 的slot是否都有recipe name & ppid 
                    //if (OPIConst.LstLineType_Cutting.Contains(FormMainMDI.G_OPIAp.CurLine.LineType) == false)
                    if (pnlRecipeID.Visible )
                    {
                        if (_row.Cells[colRecipeID.Name].Value == null || _row.Cells[colRecipeID.Name].Value.ToString().Trim() == string.Empty)
                        {
                            _msg = string.Format("No Recipe ID in Slot No [{0}]", _slotNo);
                            ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);

                            return;
                        }

                        if (_row.Cells[colPPID.Name].Value==null || _row.Cells[colPPID.Name].Value.ToString().Trim() == string.Empty)
                        {
                            _msg = string.Format("No PPID in Slot No [{0}]", _slotNo);
                            ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);

                            return;
                        }
                    }
                     #endregion

                    #region AC廠的job type 都要一樣
                    if (FormMainMDI.G_OPIAp.CurLine.FabType != "CELL")
                    {
                        if (_acJobType == string.Empty) _acJobType = _row.Cells[colJobType.Name].Value.ToString();
                        else
                        {
                            if (_acJobType != _row.Cells[colJobType.Name].Value.ToString())
                            {
                                _msg = string.Format("Job Type in  Slot No [{0}] must be same with other slot", _slotNo);
                                ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);

                                return;
                            }
                        }
                    }
                    #endregion

                    #region Slot 層依照DB設定 (SBRM_CST_CONTROL_DEF->CHECKEMPTY)判斷是否為必填欄位
                    foreach (string _colName in LstCheckEmpty_Slot)
                    {
                        if (_row.Cells[_colName].Value == null || _row.Cells[_colName].Value.ToString() == string.Empty)
                        {
                            _msg = string.Format("No {0} in Slot No [{1}]",dgvProduct.Columns[_colName].HeaderText, _slotNo);
                            ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);

                            return;
                        }
                    }
                    #endregion
                }

                #region 會上報 不為空CST時，至少要勾選一片玻璃處理 (排除UD)               
                if (IsProcess == false && IsEmptyCST == false && curPort.MapplingEnable && curPort.PortType != ePortType.UnloadingPort)
                {
                    //Array BFG 不需卡至少要勾選一片玻璃處理
                    if (FormMainMDI.G_OPIAp.CurLine.LineType != "BFG_SHUZTUNG") 
                    {
                        ShowMessage(this, lblCaption.Text, "", "Sampling Flag can't be false for all slot", MessageBoxIcon.Error);

                        return;
                    }
                }
                #endregion

                #endregion

                if (flpLotRecipeID.Visible )
                {
                    #region Lot Recipe ID Check

                    if (!IsEmptyCST)
                    {
                        #region 判斷是否做過recipe check 且都回覆OK

                        #region Cutting Line 判斷是否有設定recipe
                        if (txtLotRecipeID.Text.Trim() == string.Empty || txtLotRecipeID.Tag == null)
                        {
                            ShowMessage(this, lblCaption.Text, "", "Please Choose Current Line PPID ", MessageBoxIcon.Error);
                            return;
                        }
                        _recipeName = txtLotRecipeID.Tag.ToString();
                        #endregion

                        #region Recipe check OK ?
                        if (dicRecipeUse.ContainsKey(_recipeName))
                        {
                            if (dicRecipeUse[_recipeName].AllEQ_RecipeCheck_OK == false)
                            {
                                _recipeMsg = _recipeMsg + string.Format("Lot Recipe Name [{0}] \r\n", _recipeName);
                            }
                        }
                        else
                        {
                            _recipeMsg = _recipeMsg + string.Format("Lot Recipe Name [{0}] \r\n", _recipeName);
                        }
                        #endregion

                        #endregion
                    }

                    #endregion
                }

                if (pnlRecipeID.Visible)
                {
                    #region Product Recipe ID Check
                    List<string> _useRecipe = new List<string>();

                    foreach (DataGridViewRow row in dgvProduct.Rows)
                    {
                        _slotNo = row.Cells[colSlotNo.Name].Value.ToString();
                        _glassID = (row.Cells[colGlassID.Name].Value == null ? string.Empty : row.Cells[colGlassID.Name].Value.ToString());

                        if (row.Cells[colProcessFlag.Name].Value.ToString().ToUpper().Equals("TRUE"))
                        {
                            #region Normal line -- Check recipe name是否存在 & 取得 PPID (分號隔開)

                            _recipeName = row.Cells[colRecipeID.Name].Value.ToString();
                            
                            #region 取得 PPID (分號隔開)
                            if (dicRecipe_All.ContainsKey(_recipeName))
                            {
                                _ppid = dicRecipe_All[_recipeName].PPID;
                            }
                            else
                            {
                                _msg = string.Format("{0} is not exist", _recipeName.ToString());

                                ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);

                                return;
                            }
                            row.Cells[colPPID.Name].Value = _ppid;
                            #endregion

                            #region 判斷是否做過recipe check 且都回覆OK
                            if (dicRecipeUse.ContainsKey(_recipeName))
                            {
                                if (dicRecipeUse[_recipeName].AllEQ_RecipeCheck_OK == false)
                                {
                                    _recipeMsg = _recipeMsg + string.Format("Slot [{0}], Recipe Name [{1}] \r\n", _slotNo, _recipeName);
                                }
                            }
                            else
                            {
                                _recipeMsg = _recipeMsg + string.Format("Slot [{0}], Recipe Name [{1}] \r\n", _slotNo, _recipeName);
                            }
                            #endregion

                            #endregion 
                        }                     
                    }

                    #endregion
                }

                if (_recipeMsg != string.Empty)
                {
                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, string.Format("{0} \r\n Recipe Check NG ! Do you want to continue ?", _recipeMsg))) return;
                }

                _msg = string.Format("Please confirm whether you will process the Cassette Command [{0}] ?", _btn.Text.ToString());
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text,  _msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                SendtoBC_OfflineModeCassetteDataSend();

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            try
            {
                int _intFrSlotNo = 0;
                int _intToSlotNo = 0;

                #region 取得slot no
                if (string.IsNullOrWhiteSpace(txtSelFrSlotNo.Text.ToString()))
                {
                    ShowMessage(this, lblCaption.Text, "", "Please key in Start Slot No", MessageBoxIcon.Error);
                    txtSelFrSlotNo.Focus();
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtSelToSlotNo.Text.ToString()))
                {
                    ShowMessage(this, lblCaption.Text, "", "Please key in End Slot No", MessageBoxIcon.Error);
                    txtSelToSlotNo.Focus();
                    return;
                }

                int.TryParse(txtSelFrSlotNo.Text.ToString(), out _intFrSlotNo);
                int.TryParse(txtSelToSlotNo.Text.ToString(), out _intToSlotNo);
                if (_intToSlotNo < _intFrSlotNo)
                {
                    ShowMessage(this, lblCaption.Text, "", "The start Slot No must be less than the end Slot No!! ", MessageBoxIcon.Error);
                    return;
                }
                #endregion

                IsSetting = true;
                SetGlassData(_intFrSlotNo, _intToSlotNo, chkJobExist.Checked);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            finally
            {
                IsSetting = false;
            }
        }

        private void btnSlotCntDetail_Click(object sender, EventArgs e)
        {
            try
            {
                if (curPort.JobExistenceSlot == null)
                {
                    ShowMessage(this, lblCaption.Text, string.Empty, "Job Existence Slot is null", MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    FormCassetteControl_JobExistenceSlot _frm =  new FormCassetteControl_JobExistenceSlot(curPort);
                    _frm.ShowDialog();
                    if (_frm != null) _frm.Dispose();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRecipeCheck_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvProduct.Rows.Count <= 0) return;

                string _key = string.Empty;
                string _err = string.Empty;

                dicRecipeUse.Clear();

                //if (OPIConst.LstLineType_Cutting.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                if (flpLotRecipeID.Visible)
                {
                    if (txtLotRecipeID.Tag!= null)
                    {
                        _key = txtLotRecipeID.Tag.ToString().Trim();
                    }

                    if (!dicRecipe_All.ContainsKey(_key))
                    {
                        _err = string.Format("{0} is not exist", _key);

                        ShowMessage(this, lblCaption.Text, "", _err, MessageBoxIcon.Error);

                        return;
                    }

                    dicRecipeUse.Add(_key, dicRecipe_All[_key]);

                    #region Cutting Recipe Chech
                    //if (Recipe_Lot_Cutting != null)
                    //{
                    //    _recipeId = Recipe_Lot_Cutting.RecipeName;
                    //    if (!dicRecipeUse.ContainsKey(_recipeId))
                    //    {
                    //        dicRecipeUse.Add(_recipeId, dicRecipe_All[_recipeId]);
                    //    }
                    //}

                    //if (Recipe_Proc_Cutting != null)
                    //{
                    //    _recipeId = Recipe_Proc_Cutting.RecipeName;
                    //    if (!dicRecipeUse.ContainsKey(_recipeId))
                    //    {
                    //        dicRecipeUse.Add(_recipeId, dicRecipe_All[_recipeId]);
                    //    }
                    //}

                    //if (Recipe_STB_Cutting != null)
                    //{
                    //    _recipeId = Recipe_STB_Cutting.RecipeName;
                    //    if (!dicRecipeUse.ContainsKey(_recipeId))
                    //    {
                    //        dicRecipeUse.Add(_recipeId, dicRecipe_All[_recipeId]);
                    //    }
                    //}
                    #endregion
                }
                else
                {
                    
                    #region 取得有使用的recipe
                    foreach (DataGridViewRow _row in dgvProduct.Rows)
                    {
                        if (!(bool)_row.Cells[colProcessFlag.Name].Value) continue;

                        _key = _row.Cells[colRecipeID.Name].Value.ToString();

                        if (!dicRecipe_All.ContainsKey(_key))
                        {
                            _err = string.Format("{0} is not exist", _key);

                            ShowMessage(this, lblCaption.Text, "", _err, MessageBoxIcon.Error);

                            return;
                        }

                        //已經存在於被使用的recipe check dic內，不需要再新增
                        if (dicRecipeUse.ContainsKey(_key))
                        {
                            continue;
                        }

                        dicRecipeUse.Add(_key, dicRecipe_All[_key]);
                    }
                    #endregion
                }

                #region Recipe Check
                FormCassetteControl_RecipeCheck _frm = new FormCassetteControl_RecipeCheck(dicRecipeUse);

                if (DialogResult.OK == _frm.ShowDialog())
                {

                    RecipeRegisterValidationCommandRequest _trx = new RecipeRegisterValidationCommandRequest();
                    _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    _trx.BODY.PORTNO = curPort.PortNo;

                    #region 本line recipe

                    List<string> _choose = _frm.chooseRecipe;

                    if (_choose.Count > 0)
                    {
                        RecipeRegisterValidationCommandRequest.LINEc _line = new RecipeRegisterValidationCommandRequest.LINEc();
                        _line.RECIPELINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                        foreach (string _recipeName in _choose)
                        {
                            RecipeRegisterValidationCommandRequest.RECIPECHECKc _recipeCheck = new RecipeRegisterValidationCommandRequest.RECIPECHECKc();
                            _recipeCheck.RECIPENAME = _recipeName;

                            foreach (RecipeCheckEQ _checkEQ in dicRecipeUse[_recipeName].LocalRecipeCheck)
                            {
                                RecipeRegisterValidationCommandRequest.EQUIPMENTc _eqc = new RecipeRegisterValidationCommandRequest.EQUIPMENTc();
                                _eqc.EQUIPMENTNO = _checkEQ.LocalNo;
                                _eqc.RECIPENO = _checkEQ.RecipeNo;

                                _recipeCheck.EQUIPMENTLIST.Add(_eqc);
                            }

                            _line.RECIPECHECKLIST.Add(_recipeCheck);
                        }

                        _trx.BODY.LINELIST.Add(_line);
                    }
                    #endregion

                    if (_trx.BODY.LINELIST.Count <= 0) return;

                    string _xml = _trx.WriteToXml();

                    MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SocketResponseTime_Query);

                    if (_resp != null)
                    {
                        #region RecipeRegisterValidationCommandReply
                        ShowMessage(this, lblCaption.Text, "", "Send Recipe Register Validation Command to BC Sucess", MessageBoxIcon.Information);
                        #endregion
                    }                    
                }

                if (_frm != null) _frm.Dispose();
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnLotRecipeID_Click(object sender, EventArgs e)
        {
            try
            {
                FormCassetteControl_RecipeChoose frm = new FormCassetteControl_RecipeChoose(dicRecipe_All);

                if (System.Windows.Forms.DialogResult.OK == frm.ShowDialog())
                {
                    txtLotRecipeID.Tag = frm.RecipeName;
                    txtLotRecipeID.Text = frm.RecipeID;
                }

                if (frm != null) frm.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }

            //try
            //{
                //if (dicRecipe_All.Count == 0) LoadPPIDSetting();

                //FormCassetteControl_CuttingRecipe frm = new FormCassetteControl_CuttingRecipe(MesControlMode,dicRecipe_All, curPort);

                //frm.CurrentLinePPID = txtCurrentLinePPID.Text.ToString();
                ////frm.CrossLinePPID = txtCrossLinePPID.Text.ToString();

                //frm.LotCstSettingCode.Data = txtCassetteSettingCode.Text.ToString();
                //frm.ProcCstSettingCode.Data = txtCassetteSettingCode_CUT.Text.ToString();
                ////frm.STBCstSettingCode.Data = txtSTBCassetteSettingCode.Text.ToString();
                ////frm.CrossProcCstSettingCode.Data = txtCrossCassetteSettingCode.Text.ToString();
                ////frm.CrossSTBCstSettingCode.Data = txtSTBCassetteSettingCode.Text.ToString();

                //frm.LotProductID.Data = txtProductID.Text.ToString();
                //frm.ProcProductID.Data = txtProductID_CUT.Text.ToString();
                ////frm.STBProductID.Data = txtProductID_STB.Text.ToString();
                ////frm.CrossProcProductID.Data = txtProductID_Cross.Text.ToString();
                ////frm.CrossSTBProductID.Data = txtProductID_STB.Text.ToString();

                //frm.LotProductType.Data = txtProductType.Text.ToString();
                //frm.ProcProductType.Data = txtProductType_CUT.Text.ToString();
                ////frm.STBProductType.Data = txtProductType_STB.Text.ToString();
                ////frm.CrossProcProductType.Data = txtProductType_Cross.Text.ToString();
                ////frm.CrossSTBProductType.Data = txtProductType_STB.Text.ToString();

                //frm.Recipe_Lot_Cutting = Recipe_Lot_Cutting;
                //frm.Recipe_Proc_Cutting = Recipe_Proc_Cutting;
                //frm.Recipe_STB_Cutting = Recipe_STB_Cutting;
                ////frm.Recipe_CrossProc_Cutting = Recipe_CrossProc_Cutting;
                ////frm.Recipe_CrossSTB_Cutting = Recipe_CrossSTB_Cutting;

                //if (DialogResult.OK == frm.ShowDialog())
                //{
                //    //dicRecipe_Cross = frm.dicRecipe_Cross;

                //    txtCurrentLinePPID.Text = frm.CurrentLinePPID;
                //    //txtCrossLinePPID.Text = frm.CrossLinePPID;

                //    txtCassetteSettingCode.Text = frm.LotCstSettingCode.Data;
                //    txtCassetteSettingCode_CUT.Text = frm.ProcCstSettingCode.Data;
                //    //txtSTBCassetteSettingCode.Text = FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_3" ? frm.STBCstSettingCode.Data : frm.CrossSTBCstSettingCode.Data;
                //    //txtCrossCassetteSettingCode.Text = frm.CrossProcCstSettingCode.Data;

                //    txtProductID.Text = frm.LotProductID.Data;
                //    txtProductID_CUT.Text = frm.ProcProductID.Data;
                //    //txtProductID_STB.Text =  FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_3" ? frm.STBProductID.Data : frm.CrossSTBProductID.Data;
                //    //txtProductID_Cross.Text = frm.CrossProcProductID.Data;

                //    txtProductType.Text = frm.LotProductType.Data;
                //    txtProductType_CUT.Text = frm.ProcProductType.Data;
                //    //txtProductType_STB.Text = FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_3" ? frm.STBProductType.Data : frm.CrossSTBProductType.Data; 
                //    //txtProductType_Cross.Text = frm.CrossProcProductType.Data ;

                //    Recipe_Lot_Cutting = frm.Recipe_Lot_Cutting;
                //    Recipe_Proc_Cutting = frm.Recipe_Proc_Cutting;
                //    Recipe_STB_Cutting = frm.Recipe_STB_Cutting;
                //    //Recipe_CrossProc_Cutting = frm.Recipe_CrossProc_Cutting;
                //    //Recipe_CrossSTB_Cutting = frm.Recipe_CrossSTB_Cutting;
                //}

                //if (frm != null) frm.Dispose();
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            //    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            //}
        }

        private void btnProductType_Click(object sender, EventArgs e)
        {
            try
            {
                FormCassetteControl_ProductType_Array _frm = new FormCassetteControl_ProductType_Array();

                if (_frm.ShowDialog() == DialogResult.OK)
                {
                    txtProductType.Text = _frm.PorductType;
                }

                if (_frm != null) _frm.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnTargetSlotNo_Click(object sender, EventArgs e)
        {
            try
            {
                FormCassetteControl_Offline_TargetSlotNo _frm = new FormCassetteControl_Offline_TargetSlotNo(dgvProduct.Rows);

                _frm.ShowDialog();

                if (_frm != null) _frm.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtFlowPriorityInfo_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                FormCassetteeControl_FlowPriorityInfo _frm = new FormCassetteeControl_FlowPriorityInfo(txtFlowPriorityInfo.Text);

                if (_frm.ShowDialog() == DialogResult.OK)
                {
                    txtFlowPriorityInfo.Text = _frm.FlowPriorityInfo;
                }

                if (_frm != null) _frm.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtRange_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                if (_txt.Text == string.Empty) return;

                RangeCondition _range = LstRangeCondition.Find(r => r.ObjectName.Equals(_txt.Name));

                if (_range == null) return;

                int _num = 0;

                int.TryParse(_txt.Text.ToString(), out _num);

                if (_num > _range.Max || _num < _range.Min)
                {
                    ShowMessage(this, lblCaption.Text, "", _range.RangeText, MessageBoxIcon.Error);

                    _txt.Text = string.Empty;

                    _txt.Focus();

                    return;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                asciiCode == 8)   // Backspace
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void txtSlotNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            UniTools.CheckTextBoxKeyPressIsInteger(sender, e);
        }

        private void txtGlassID_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                (asciiCode >= 65 & asciiCode <= 90 & asciiCode != 79 & asciiCode != 73) |   // A-Z except I、O
                (asciiCode >= 97 & asciiCode <= 122 & asciiCode != 111 & asciiCode != 105) |   // a-z except i、o
                asciiCode == 8)    // Backspace
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void txtRecipeID_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                FormCassetteControl_RecipeChoose frm = new FormCassetteControl_RecipeChoose(dicRecipe_All);

                if (System.Windows.Forms.DialogResult.OK == frm.ShowDialog())
                {
                    txtRecipeID.Text = frm.RecipeName;
                }

                if (frm != null) frm.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtOXR_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                FormCassetteControl_OXR _frm = new FormCassetteControl_OXR(_txt.Text.ToString().Trim(), _txt.Tag.ToString());

                if (_frm.ShowDialog() == DialogResult.OK)
                {
                    _txt.Text = _frm.OXRInfo;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void JobSubItem_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                if (_txt.Tag == null) return;

                string _subItemKey = _txt.Tag.ToString();
                string _subItemValue = string.Empty;

                bool _isSingleChoose = false;//判斷是否只能勾選一個

                switch (_subItemKey)
                {
                    case "EQPFlag":
                        _txt = txtEQPFlag;

                        //FCREP_TYPE1 & FCREP_TYPE2 & FCREP_TYPE3 & FCMAC_TYPE1 只能勾選一個flag
                        if (FormMainMDI.G_OPIAp.CurLine.LineType == "FCREP_TYPE1" || FormMainMDI.G_OPIAp.CurLine.LineType == "FCREP_TYPE2" ||
                            FormMainMDI.G_OPIAp.CurLine.LineType == "FCREP_TYPE3" || FormMainMDI.G_OPIAp.CurLine.LineType == "FCMAC_TYPE1") _isSingleChoose = true;

                        break;

                    case "INSPReservations":
                        _txt = txtInspReservations;
                        break;

                    default:
                        break;
                }
                
                FormCassetteControl_SubJobItem _subJobItem = new FormCassetteControl_SubJobItem(_subItemKey, _txt.Text.ToString(), _isSingleChoose);

                if (_subJobItem.ShowDialog() == DialogResult.OK)
                {
                    _txt.Text = _subJobItem.SubItemData == null ? string.Empty : string.Join("", _subJobItem.SubItemData);
                }

                if (_subJobItem != null) _subJobItem.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void chkSelect_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox _chkSelect = (CheckBox)sender;

                CheckBox _chk = null;

                foreach (GroupBox _grb in pnlSlotInfo.Controls.OfType<GroupBox>())
                {
                    foreach (FlowLayoutPanel _flp in _grb.Controls.OfType<FlowLayoutPanel>())
                    {
                        foreach (Panel _panel in _flp.Controls.OfType<Panel>())
                        {

                            if (_panel.Name == "pnlSlotNo" || _panel.Name == "pnlTargetCSTID") continue;

                            if (_panel.Name == "pnlProcessFlag")
                            {
                                #region pnlProcessFlag

                                _chk = _panel.Controls.Find("chkProcess", false).OfType<CheckBox>().First();

                                _chk.Checked = _chkSelect.Checked;
                                #endregion
                            }
                            else
                            {
                                #region 其他控制項設定 -- 取得設定值
                                _chk = _panel.Controls.OfType<CheckBox>().First();

                                _chk.Checked = _chkSelect.Checked;
                            }
                                #endregion
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
        
        private void chkChoose_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox _chk = (CheckBox)sender;

                switch (_chk.Name)
                {
                    case "chkProcessFlag":
                        if (_chk.Checked)
                        {
                            _chk.BackColor = Color.Lime;
                            _chk.Text = "Process";
                        }
                        else
                        {
                            _chk.BackColor = Color.Silver;
                            _chk.Text = "Not Process";
                        }
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

        private void chkLot_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                if (_rdo.Checked)
                {
                    OfflineCassetteDataReply.LOTDATAc _lotData = LstLots.Find(d => d.LOTNAME == _rdo.Name);

                    ShowLotData(_lotData);

                    CurLotData = _lotData;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetDefaultData()
        {
            //try
            //{
            //    Panel _pnl = null;
            //    CheckBox _chk = null;
            //    TextBox _txt = null;
            //    ComboBox _cbo = null;

            //    foreach (CSTCtrlDefaultData _default in FormMainMDI.G_OPIAp.CurLine.LstCstControlDefaultData)
            //    {
            //        #region Lot層item -- cutting recipe
            //        if (_default.PanelName == "flpLotRecipeID")
            //        {
            //            txtLotRecipeID.Tag = _default.LotRecipeName;
            //            txtLotRecipeID.Text = _default.LotRecipeID;

            //            continue;
            //        }
            //        #endregion

            //        #region product 層item 

            //        if (pnlSlotInfo.Controls.Find(_default.PanelName, true).OfType<Panel>().Count() > 0)
            //        {
            //            _pnl = (Panel)pnlSlotInfo.Controls.Find(_default.PanelName, true).First();

            //            if (_pnl == null) FormMainMDI.G_OPIAp.CurLine.LstCstControlDefaultData.Remove(_default);

            //            _chk = (CheckBox)_pnl.Controls.Find(_default.CheckBoxName, false).First();

            //            if (_chk != null) _chk.Checked = _default.CheckBoxValue;

            //            switch (_default.ObjectType)
            //            {
            //                case eObjectType.CheckBox:

            //                    _chk = (CheckBox)_pnl.Controls.Find(_default.ObjectName, false).First();
            //                    if (_chk != null) _chk.Checked = (_default.ObjectValue == "Y" ? true : false);

            //                    break;

            //                case eObjectType.ComboBox:

            //                    _cbo = (ComboBox)_pnl.Controls.Find(_default.ObjectName, false).First();
            //                    if (_cbo != null)
            //                    {
            //                        if (_cbo.Name == "cboJobGrade") _cbo.Text = _default.ObjectValue;
            //                        else _cbo.SelectedValue = _default.ObjectValue;
            //                    }

            //                    break;

            //                case eObjectType.TextBox:

            //                    _txt = (TextBox)_pnl.Controls.Find(_default.ObjectName, false).First();
            //                    if (_txt != null) _txt.Text = _default.ObjectValue;

            //                    break;

            //                default:

            //                    break;
            //            }
            //        }
            //        #endregion                    
            //    }
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            //    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            //}
        }

        private void ShowLotData(OfflineCassetteDataReply.LOTDATAc lotData)
        {
            try
            {
                bool _processFlag = false;
                string _slotNo = string.Empty;

                #region Lot
                txtLotID.Text = lotData.LOTNAME;
                txtProcOperName.Text = lotData.PROCESSOPERATIONNAME;
                //txtProdOwner.Text = lotData.PRODUCTOWNER;
                cboProductOwner.SelectedValue = lotData.PRODUCTOWNER;
                if (cboProductOwner.SelectedValue == null) cboProductOwner.Text = lotData.PRODUCTOWNER;

                txtProdSpecName.Text = lotData.PRODUCTSPECNAME;

                txtProductType.Text = lotData.BCPRODUCTTYPE;
                txtProductType_CUT.Text= lotData.BCPRODUCTTYPE_CUT;

                txtProductID.Text = lotData.PRODUCTID;
                txtProductID_CUT.Text = lotData.PRODUCTID_CUT;

                txtReworkMaxCount.Text = lotData.CFREWORKCOUNT;
                //txtInlineReworkCount.Text = lotData.INLINE_REWORK_MAX_COUNT;
                txtTargetCSTID_CF.Text = lotData.TARGETCSTID_CF;
                txtCassetteSettingCode.Text = lotData.CSTSETTINGCODE;
                txtCassetteSettingCode_CUT.Text = lotData.CSTSETTINGCODE_CUT;
                txtCassetteSettingCode_PCS2.Text = lotData.CSTSETTINGCODE2;
                txtLotRecipeID.Tag = lotData.LINERECIPENAME;
                txtLotRecipeID.Text = lotData.PPID;

                #region CUT PPID
                //if (OPIConst.LstLineType_Cutting.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                //{
                //    #region 取得BC傳過來的TRX設定資料

                //    #region Lot Recipe
                //    Recipe_Lot_Cutting = new Recipe(FormMainMDI.G_OPIAp.CurLine.ServerName, lotData.LINERECIPENAME, lotData.PPID);
                //    #endregion

                //    #region Process Line List --- 最多兩組,一組本Line & 被跨Line的
                //    foreach (OfflineCassetteDataReply.PROCESSLINEc _product in lotData.PROCESSLINELIST)
                //    {
                //        if (_product.LINENAME == FormMainMDI.G_OPIAp.CurLine.ServerName)
                //        {
                //            //本line ppid
                //            Recipe_Proc_Cutting = new Recipe(FormMainMDI.G_OPIAp.CurLine.ServerName, _product.LINERECIPENAME, _product.PPID);
                //            txtCassetteSettingCode_CUT.Text = _product.CSTSETTINGCODE;
                //        }
                //        else 
                //        {
                //            //跨line ppid
                //            //Recipe_CrossProc_Cutting = new Recipe(_product.LINENAME, _product.LINERECIPENAME, _product.PPID, "PROCESSLINE");
                //            //txtCrossCassetteSettingCode.Text = _product.CSTSETTINGCODE;
                //        }
                //    }
                //    #endregion

                //    #region STB Prod Spec List
                //    //foreach (OfflineCassetteDataReply.STBPRODUCTSPECc _stb in lotData.STBPRODUCTSPECLIST)
                //    //{
                //    //    if ( _stb.LINENAME == FormMainMDI.G_OPIAp.CurLine.LineID2)
                //    //    {
                //    //        //本line ppid
                //    //        Recipe_STB_Cutting = new Recipe(_stb.LINENAME, _stb.LINERECIPENAME, _stb.PPID, "STB");
                //    //        txtSTBCassetteSettingCode.Text = _stb.CSTSETTINGCODE;
                //    //    }
                //    //    else
                //    //    {
                //    //        //跨line ppid
                //    //        Recipe_CrossSTB_Cutting = new Recipe(_stb.LINENAME, _stb.LINERECIPENAME, _stb.PPID, "STB");
                //    //        txtSTBCassetteSettingCode.Text = _stb.CSTSETTINGCODE;
                //    //    }
                //    //}
                //    #endregion

                //    #endregion

                //    #region 拆解並重新組合Cuttin PPID並顯示於畫面上

                //    #region Current Line PPID

                //    List<string> _recipeData = new List<string>();
                //    RecipeCheckEQ _recipe = null;
                //    string _recipeNo = string.Empty;

                //    foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                //    {

                //        if (_node.NodeNo == "L2" || _node.NodeNo == "L3")
                //        {
                //            #region 取 LOT PPID : L2,L3

                //            _recipe = Recipe_Lot_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L2") || r.LocalNo.Equals("L3"));
                            
                //            if (_recipe != null) _recipeNo = _recipe.RecipeNo;
                            
                //            else _recipeNo = _node.DefaultRecipeNo;                            
                            
                //            #endregion
                //        }
                //        else if (_node.NodeNo == "L4" || _node.NodeNo == "L5" || _node.NodeNo == "L6" || _node.NodeNo == "L7" || _node.NodeNo == "L8" || _node.NodeNo == "L9" || _node.NodeNo == "L10")
                //        {
                //            #region 取Process Line :L4~L10

                //            _recipe = Recipe_Proc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L4") || r.LocalNo.Equals("L5") || r.LocalNo.Equals("L6") || r.LocalNo.Equals("L7")
                //                 || r.LocalNo.Equals("L8") || r.LocalNo.Equals("L9") || r.LocalNo.Equals("L10") );

                //            if (_recipe != null) _recipeNo = _recipe.RecipeNo;

                //            else _recipeNo = _node.DefaultRecipeNo;
                            
                //            #endregion
                //        }
                //        else if (_node.NodeNo == "L11" || _node.NodeNo == "L12" || _node.NodeNo == "L13" || _node.NodeNo == "L14" || _node.NodeNo == "L15")
                //        {
                //            #region 取 STB : L11~L15

                //            _recipe = Recipe_STB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L11") || r.LocalNo.Equals("L12") || r.LocalNo.Equals("L13") || 
                //                r.LocalNo.Equals("L14") || r.LocalNo.Equals("L15"));

                //            if (_recipe != null) _recipeNo = _recipe.RecipeNo;
                            
                //            else _recipeNo = _node.DefaultRecipeNo;

                //            #endregion
                //        }
                //        else
                //        {
                //            _recipeNo = _node.DefaultRecipeNo;
                //        }

                //        _recipeData.Add(_node.NodeNo + ":" + _recipeNo);
                //    }

                //    txtLotRecipeID.Text = string.Join(";", _recipeData.ToArray());

                //    #endregion
                   
                //    #region Cross Line PPID

                //    //_recipeData = new List<string>();

                //    //#region 取 Process Line Cross PPID : L2 ~ L10
                //    //if (Recipe_CrossProc_Cutting != null)
                //    //{
                //    //    foreach (Node _node in FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.Values)
                //    //    {
                //    //        if (_node.NodeNo == "L2" || _node.NodeNo == "L3" || _node.NodeNo == "L4" || _node.NodeNo == "L5" || _node.NodeNo == "L6" ||
                //    //            _node.NodeNo == "L7" || _node.NodeNo == "L8" || _node.NodeNo == "L9" || _node.NodeNo == "L10")
                //    //        {
                //    //            _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L2") || r.LocalNo.Equals("L3") || r.LocalNo.Equals("L4") ||
                //    //                r.LocalNo.Equals("L5") || r.LocalNo.Equals("L6") || r.LocalNo.Equals("L7") || r.LocalNo.Equals("L8") || r.LocalNo.Equals("L9") || r.LocalNo.Equals("L10"));
                //    //            {
                //    //                if (_recipe != null) _recipeNo = _recipe.RecipeNo;

                //    //                else _recipeNo = _node.DefaultRecipeNo;
                //    //            }

                //    //            _recipeData.Add(_node.NodeNo + ":" + _recipeNo);
                //    //        }                                                        
                //    //    }
                //    //}
                //    //#endregion

                //    //#region 取 STB Line Cross PPID : L11 ~ L15
                //    //if (Recipe_CrossSTB_Cutting != null)
                //    //{
                //    //    foreach (Node _node in FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.Values)
                //    //    {
                //    //        if (_node.NodeNo == "L11" || _node.NodeNo == "L12" || _node.NodeNo == "L13" || _node.NodeNo == "L14" || _node.NodeNo == "L15")
                //    //        {

                //    //            _recipe = Recipe_CrossSTB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L11") || r.LocalNo.Equals("L12") || r.LocalNo.Equals("L13") ||
                //    //                r.LocalNo.Equals("L14") || r.LocalNo.Equals("L15"));

                //    //            if (_recipe != null) _recipeNo = _recipe.RecipeNo;

                //    //            else _recipeNo = _node.DefaultRecipeNo;

                //    //            _recipeData.Add(_node.NodeNo + ":" + _recipeNo);
                //    //        }
                //    //    }
                //    //}
                //    //#endregion

                //    //#region mark
                //    ////if (FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_2")
                //    ////{
                //    ////    #region Get Recipe No
                //    ////    _L2RecipeNo = "00";
                //    ////    _L3RecipeNo = "00";
                //    ////    _L4RecipeNo = "00";
                //    ////    _L5RecipeNo = "00";
                //    ////    _L6RecipeNo = "00";
                //    ////    _L7RecipeNo = "00";
                //    ////    _L8RecipeNo = "00";
                //    ////    _L9RecipeNo = "00";
                //    ////    _L10RecipeNo = "00";
                //    ////    _L11RecipeNo = "00";
                //    ////    _L12RecipeNo = "00";
                //    ////    _L13RecipeNo = "00";
                //    ////    _L14RecipeNo = "00";
                //    ////    _L15RecipeNo = "00";

                //    ////    _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L2"));
                //    ////    if (_recipe != null) _L2RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L3"));
                //    ////    if (_recipe != null) _L3RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L4"));
                //    ////    if (_recipe != null) _L4RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L5"));
                //    ////    if (_recipe != null) _L5RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L6"));
                //    ////    if (_recipe != null) _L6RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L7"));
                //    ////    if (_recipe != null) _L7RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L8"));
                //    ////    if (_recipe != null) _L8RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L9"));
                //    ////    if (_recipe != null) _L9RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L10"));
                //    ////    if (_recipe != null) _L10RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossSTB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L11"));
                //    ////    if (_recipe != null) _L11RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossSTB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L12"));
                //    ////    if (_recipe != null) _L12RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossSTB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L13"));
                //    ////    if (_recipe != null) _L13RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossSTB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L14"));
                //    ////    if (_recipe != null) _L14RecipeNo = _recipe.RecipeNo;

                //    ////    _recipe = Recipe_CrossSTB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L15"));
                //    ////    if (_recipe != null) _L15RecipeNo = _recipe.RecipeNo;

                //    ////    _crossPPID = string.Format("L2:{0};L3:{1};L4:{2};L5:{3};L6:{4};L7:{5};L8:{6};L9:{7};L10:{8};L11:{9};L12:{10};L13:{11};L14:{12};L15:{13}",
                //    ////                                _L2RecipeNo, _L3RecipeNo, _L4RecipeNo, _L5RecipeNo, _L6RecipeNo, _L7RecipeNo, _L8RecipeNo, _L9RecipeNo, _L10RecipeNo,
                //    ////                                _L11RecipeNo, _L12RecipeNo, _L13RecipeNo, _L14RecipeNo, _L15RecipeNo);

                //    ////    #endregion

                //    ////    txtCrossLinePPID.Text = _crossPPID;

                //    ////}
                //    //#endregion
                   
                //    ////txtCrossLinePPID.Text = string.Join(";", _recipeData.ToArray());

                //    #endregion

                //    #endregion
                //}
                #endregion

                #endregion

                #region Product

                DataGridViewRow _row = null;

                foreach (OfflineCassetteDataReply.PRODUCTDATAc product in lotData.PRODUCTLIST)
                {
                    _processFlag = product.PROCESSFLAG == "Y" ? true : false;

                    var _var = dgvProduct.Rows
                           .Cast<DataGridViewRow>()
                           .Where(r => r.Cells[colSlotNo.Name].Value.ToString().Equals(product.SLOTNO.ToString()));

                    if (_var.Count() > 0)
                    {
                        _row = _var.First();

                        _row.Cells[colGlassID.Name].Value = product.PRODUCTNAME.ToString();
                        _row.Cells[colProcessFlag.Name].Value = _processFlag;
                        _row.Cells[colRecipeID.Name].Value = product.PRODUCTRECIPENAME.ToString();
                        _row.Cells[colPPID.Name].Value = product.PPID.ToString();
                        _row.Cells[colJobType.Name].Value = product.PRODUCTTYPE.ToString();
                        _row.Cells[colProcessType.Name].Value = product.PROCESSTYPE.ToString();
                        _row.Cells[colJobGrade.Name].Value = product.PRODUCTGRADE.ToString();
                        _row.Cells[colJobJudge.Name].Value = product.PRODUCTJUDGE.ToString();
                        _row.Cells[colGroupIndex.Name].Value = product.GROUPID.ToString();
                        _row.Cells[colSubstrateType.Name].Value = product.SUBSTRATETYPE.ToString();
                        _row.Cells[colTargetCSTID.Name].Value = product.TARGETCSTID.ToString();
                        _row.Cells[colCOAVersion.Name].Value = product.COAVERSION.ToString();
                        _row.Cells[colInspReservations.Name].Value = product.INSPRESERVATION.ToString();
                        _row.Cells[colPreInlineID.Name].Value = product.PREINLINEID.ToString();
                        _row.Cells[colFlowPriorityInfo.Name].Value = product.FLOWPRIORITY.ToString();
                        _row.Cells[colNetworkNo.Name].Value = product.NETWORKNO.ToString();
                        _row.Cells[colEQPFlag.Name].Value = product.EQPFLAG.ToString();
                        _row.Cells[colOwnerID.Name].Value = product.OWNERID.ToString();
                        _row.Cells[colOwnerType.Name].Value = product.OWNERTYPE.ToString();
                        _row.Cells[colRevProcOperName.Name].Value = product.REVPROCESSOPERATIONNAME.ToString();
                        _row.Cells[colPanelSize.Name].Value = product.PANELSIZE.ToString();
                        _row.Cells[colTurnAngleFlag.Name].Value = product.TRUNANGLEFLAG.ToString();
                        _row.Cells[colBlockSize.Name].Value = product.BLOCK_SIZE.ToString();
                        _row.Cells[colTargetSlotNo.Name].Value = product.TARGET_SLOTNO.ToString();
                        _row.Cells[colInlineReworkCount.Name].Value = product.CFINLINEREWORKMAXCOUNT.ToString();
                        _row.Cells[colUVMaskUseCount.Name].Value = product.UV_MASK_USE_COUNT.ToString();
                        _row.Cells[colAssembleSeqNo.Name].Value = product.ASSEMBLE_SEQNO.ToString();
                        _row.Cells[colBlockOX.Name].Value = product.BLOCK_OX_INFO.ToString();
                        _row.Cells[colGlassThickness.Name].Value = product.GLASS_THICKNESS.ToString();
                        _row.Cells[colOperationID.Name].Value = product.OPERATION_ID.ToString();
                        _row.Cells[colPILiquidType.Name].Value = product.PI_LIQUID_TYPE.ToString();
                        _row.Cells[colOXR.Name].Value = product.OXR.ToString().ToString();
                        _row.Cells[colVendorName.Name].Value = product.VENDER_NAME.ToString().ToString();
                        _row.Cells[colRejudgeCount.Name].Value = product.REJUDGE_COUNT.ToString().ToString();
                        _row.Cells[colBURCheckCount.Name].Value = product.BUR_CHECK_COUNT.ToString().ToString();
                        _row.Cells[colDotRepairCount.Name].Value = product.DOT_REPAIR_COUNT.ToString().ToString();
                        _row.Cells[colLineRepairCount.Name].Value = product.LINE_REPAIR_COUNT.ToString().ToString();
                        _row.Cells[colMaxReworkCount.Name].Value = product.MAX_REWORK_COUNT.ToString().ToString();
                        _row.Cells[colCurrentReworkCount.Name].Value = product.CURRENT_REWORK_COUNT.ToString().ToString();
                        _row.Cells[colOQCBank.Name].Value = product.OQC_BANK.ToString().ToString();
                    }
                }

                if (dgvProduct.Rows.Count > 0) dgvProduct.Sort(dgvProduct.Columns[colSlotNo.Name], ListSortDirection.Descending);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetGlassData(int startSlotNo, int endSlotNo, bool checkJobExist)
        {
            try
            {
                CheckBox _chk;
                TextBox _txt;
                ComboBox _cbo;
                CheckBox _chkProcFlag = null;

                int _slotNo = 0;

                string _itemValue = string.Empty;
                string _colName = string.Empty;                


                #region Check 必填寫的item是否都有設定值
                
                foreach (GroupBox _grb in pnlSlotInfo.Controls.OfType<GroupBox>())
                {
                    foreach (FlowLayoutPanel _flp in _grb.Controls.OfType<FlowLayoutPanel>())
                    {
                        foreach (Panel _panel in _flp.Controls.OfType<Panel>())
                        {
                            if (_panel.Visible == false) continue;

                            //AbnormalCode允許空白
                            if (_panel.Name == "pnlSlotNo" || _panel.Name == "pnlTargetCSTID" || _panel.Name == "pnlProcessFlag" || _panel.Name == "pnlAbnormalCode") continue;

                            #region 其他控制項判斷是否有勾選，若有勾選 則須有輸入值
                            _itemValue = string.Empty;
                            _chk = _panel.Controls.OfType<CheckBox>().First();

                            if (!_chk.Checked) continue;

                            if (_panel.Controls.OfType<TextBox>().Count() == 0)
                            {
                                if (_panel.Controls.OfType<ComboBox>().Count() == 0) return;
                                else
                                {
                                    #region ComboBox
                                    _cbo = _panel.Controls.OfType<ComboBox>().First();

                                    if (_cbo.Name == "cboJobGrade")
                                    {
                                        _itemValue = _cbo.Text.ToString();
                                    }
                                    else
                                    {
                                        if (_cbo.SelectedValue != null)
                                            _itemValue = _cbo.SelectedValue.ToString();
                                    }

                                    if (_itemValue.Trim() == string.Empty)
                                    {
                                        ShowMessage(this, lblCaption.Text, string.Empty, string.Format("{0} is empty !", _chk.Text), MessageBoxIcon.Warning);
                                        _cbo.Focus();
                                        return;
                                    }
                                    #endregion
                                }
                            }
                            else
                            {
                                #region TextBox
                                _txt = _panel.Controls.OfType<TextBox>().First();

                                _itemValue = _txt.Text.ToString();
                                
                                if (_itemValue.Trim() == string.Empty)
                                {
                                    ShowMessage(this, lblCaption.Text, string.Empty, string.Format("{0} is empty !", _chk.Text), MessageBoxIcon.Warning);
                                    _txt.Focus();
                                    return;
                                }                                

                                if (_txt.Name == "txtRecipeID")
                                {
                                    if (!dicRecipe_All.ContainsKey(_itemValue))
                                    {
                                        ShowMessage(this, lblCaption.Text, string.Empty, string.Format("Recipe Name [{0}] is not exist !", _itemValue), MessageBoxIcon.Warning);
                                        txtRecipeID.Focus();
                                        return;
                                    }
                                }
                                #endregion

                            #endregion
                           }
                        }
                    }
                }
                #endregion

                #region 更新至DataGridView
                foreach (GroupBox _grb in pnlSlotInfo.Controls.OfType<GroupBox>())
                {
                    foreach (FlowLayoutPanel _flp in _grb.Controls.OfType<FlowLayoutPanel>())
                    {
                        foreach (Panel _panel in _flp.Controls.OfType<Panel>())
                        {
                            if (_panel.Visible == false) continue;

                            _itemValue = string.Empty;

                            _colName = "col" + _panel.Name.Substring(3);

                            if (_panel.Name == "pnlSlotNo" ) continue;

                            if (_panel.Name == "pnlProcessFlag")
                            {
                                #region pnlProcessFlag
                                _chk = _panel.Controls.Find("chkProcess", false).OfType<CheckBox>().First();

                                if (!_chk.Checked) continue;

                                _chkProcFlag = _panel.Controls.Find("chkProcessFlag", false).OfType<CheckBox>().First();

                                _itemValue = _chkProcFlag.Checked ? "true" : "false";
                                #endregion
                            }
                            else
                            {
                                #region 其他控制項設定 -- 取得設定值
                                _chk = _panel.Controls.OfType<CheckBox>().First();

                                if (!_chk.Checked) continue;

                                if (_panel.Controls.OfType<TextBox>().Count() == 0)
                                {
                                    if (_panel.Controls.OfType<ComboBox>().Count() == 0)
                                    {
                                        //非combobox & textbox
                                        _itemValue = string.Empty;
                                        _colName = string.Empty;
                                    }
                                    else
                                    {
                                        #region ComboBox
                                        _cbo = _panel.Controls.OfType<ComboBox>().First();

                                        if (_cbo.Name == "cboJobGrade")
                                        {
                                            _itemValue = _cbo.Text.ToString();
                                        }
                                        else
                                        {
                                            if (_cbo.SelectedValue != null)
                                                _itemValue = _cbo.SelectedValue.ToString();
                                            else _itemValue = string.Empty;
                                        }

                                        #endregion
                                    }
                                }
                                else
                                {
                                    #region TextBox
                                    _txt = _panel.Controls.OfType<TextBox>().First();

                                    _itemValue = _txt.Text.ToString();

                                    if (_txt.Name == "txtGroupIndex")
                                    {
                                        int _num = 0;

                                        int.TryParse(_txt.Text.ToString(), out _num);

                                        if (_num > 65535 || _num < 1)
                                        {
                                            ShowMessage(this, lblCaption.Text, "", " 1 <= Group Index <= 65535", MessageBoxIcon.Error);

                                            txtGroupIndex.Text = "1";

                                            txtGroupIndex.Focus();

                                            return;
                                        }
                                    }
                                    #endregion
                                }
                                #endregion
                            }

                            #region 更新至dataGridView
                            IsUseSetButton = true;
                            for (int i = dgvProduct.Rows.Count - 1; i >= 0; i--)
                            {
                                DataGridViewRow dr = dgvProduct.Rows[i];

                                _slotNo = 0;

                                int.TryParse(dr.Cells["colSlotNo"].Value.ToString(), out _slotNo);

                                if (_slotNo < startSlotNo) continue;

                                if (_slotNo > endSlotNo) break;

                                if (checkJobExist && curPort.JobExistenceSlot.Substring(_slotNo - 1, 1) != "1") continue;

                                #region update to datagridview

                                #region ChangerPlan的Glass不能取消勾選 add by sy.wu
                                if (FormMainMDI.G_OPIAp.CurLine.IndexerMode == 3 && (curPort.PortType != ePortType.UnloadingPort))
                                {
                                    dr.Cells["colChoose"].Value = true;
                                    foreach (chplan cp in cplan)
                                    {
                                        if (dr.Cells["colGlassID"].Value.ToString() == cp.glassID)
                                        {
                                            dr.Cells["colChoose"].ReadOnly = true;
                                        }
                                    }
                                }
                                else
                                {
                                    dr.Cells["colChoose"].Value = true;
                                }
                                #endregion

                                switch (_colName)
                                {
                                    case "colProcessFlag":
                                        #region ChangerPlan的Glass才勾選 ProcessFlag  add by sy.wu
                                        if (FormMainMDI.G_OPIAp.CurLine.IndexerMode == 3 && (curPort.PortType != ePortType.UnloadingPort))
                                        {
                                            //dr.Cells[_colName].Value = null;

                                            foreach (chplan cp in cplan)
                                            {
                                                if (dr.Cells["colGlassID"].Value.ToString() == cp.glassID)
                                                {
                                                    dr.Cells[_colName].Value = _chkProcFlag.Checked;
                                                }
                                            }
                                        }
                                        #endregion
                                        else
                                        {
                                            dr.Cells[_colName].Value = _chkProcFlag.Checked;
                                        }
                                        break;

                                    case "colGlassID":

                                        #region ChangerPlan的Glass不要再給GlassID  add by sy.wu
                                        if (FormMainMDI.G_OPIAp.CurLine.IndexerMode == 3 && (curPort.PortType != ePortType.UnloadingPort))
                                        {
                                            if (dr.Cells[_colName].Value.ToString() == "")
                                            {
                                                if (FormMainMDI.G_OPIAp.CurLine.FabType == "ARRAY")
                                                    dr.Cells[_colName].Value = _itemValue.Trim() + UniTools.GetGlassID_Array(_slotNo);
                                                else
                                                    dr.Cells[_colName].Value = _itemValue.Trim() + UniTools.GetGlassID(_slotNo);
                                            }
                                        }
                                        #endregion
                                        else
                                        {
                                            if (FormMainMDI.G_OPIAp.CurLine.FabType == "ARRAY")
                                                dr.Cells[_colName].Value = _itemValue.Trim() + UniTools.GetGlassID_Array(_slotNo);
                                            else
                                                dr.Cells[_colName].Value = _itemValue.Trim() + UniTools.GetGlassID(_slotNo);
                                        }

                                        break;

                                    case "colGroupIndex":
                                    case "colJobGrade":
                                    case "colRecipeID":
                                    case "colJobType":
                                    case "colJobJudge":                                    
                                    case "colProcessType":
                                    case "colCOAVersion":
                                    case "colInspReservations":
                                    case "colPreInlineID":
                                    case "colFlowPriorityInfo":
                                    case "colNetworkNo":
                                    case "colEQPFlag":
                                    case "colOwnerID":
                                    case "colOwnerType":
                                    case "colRevProcOperName":
                                    case "colSubstrateType":
                                    case "colPanelSize":
                                    case "colTurnAngleFlag":
                                    case "colBlockSize":
                                    case "colTargetSlotNo":
                                    case "colScrapCutFlag":
                                    case "colInlineReworkCount":
                                    case "colUVMaskUseCount":
                                    case "colAssembleSeqNo":
                                    case "colOXR":  //Panel OX Information
                                    case "colBlockOX":  //Block OX Information
                                    case "colGlassThickness":
                                    case "colOperationID":
                                    case "colPILiquidType":
                                    case "colVendorName":
                                    case "colRejudgeCount":
                                    case "colBURCheckCount":
                                    case "colDotRepairCount":
                                    case "colLineRepairCount":
                                    case "colMaxReworkCount":
                                    case "colCurrentReworkCount":
                                    case "colOQCBank":
                                    case "colPCSCassetteSettingCodeList":
                                    case "colPCSBlockSizeList":
                                    case "colBlockSize1":
                                    case "colBlockSize2":
                                        dr.Cells[_colName].Value = _itemValue;
                                        break;

                                    default:
                                        break;
                                }
                                #endregion
                            }
                            IsUseSetButton = false;

                            #endregion
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            finally
            {
                IsUseSetButton = false;
            }
        }

        private void LoadPPIDSetting()
        {
            try
            {
                Recipe _new;

                var _var = (from _recipe in FormMainMDI.G_OPIAp.DBCtx.SBRM_RECIPE
                            where _recipe.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType &&
                                  _recipe.ONLINECONTROLSTATE == MesControlMode
                            select new { RecipeName = _recipe.LINERECIPENAME, PPID = _recipe.PPID });


                dicRecipe_All.Clear();

                foreach (var _recipe in _var)
                {
                    _new = new Recipe(FormMainMDI.G_OPIAp.CurLine.ServerName, _recipe.RecipeName, _recipe.PPID);
                    dicRecipe_All.Add(_recipe.RecipeName, _new);
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //private void LoadCrossPPIDSetting()
        //{
            //try
            //{
            //    #region 取得跨line Recipe 資訊
            //    Recipe _new;
            //    dicRecipe_Cross = new Dictionary<string, Recipe>();

            //    string _serverNameCross = FormMainMDI.G_OPIAp.CrossServerName_CUT;

            //    if (_serverNameCross == string.Empty) return;

            //    var _var = (from recipe in FormMainMDI.G_OPIAp.DBCtx.SBRM_RECIPE_CROSS
            //                where recipe.LINETYPE == FormMainMDI.G_OPIAp.Dic_Line[_serverNameCross].LineType &&
            //                         recipe.ONLINECONTROLSTATE == MesControlMode
            //                select new { RecipeName = recipe.LINERECIPENAME, PPID = recipe.PPID, RecipeType = recipe.RECIPETYPE });

            //    foreach (var _recipe in _var)
            //    {
            //        _new = new Recipe(_serverNameCross, _recipe.RecipeName, _recipe.PPID, _recipe.RecipeType);
            //        dicRecipe_Cross.Add(_recipe.RecipeName, _new);
            //    }

            //    #endregion
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            //    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            //}
        //}

        private void GetCSTControlDef()
        {
            try
            {
                string _colName = string.Empty;
                string _objName = string.Empty;

                #region 取得下貨畫面要顯示的物件資訊 (SBRM_CST_CONTROL_DEF)
                UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBCtx;
                List<SBRM_CST_CONTROL_DEF> _lstCstControl = (from row in ctx.SBRM_CST_CONTROL_DEF.Where(d =>
                    d.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) &&
                    (d.MESCONTROLMODE.Contains(MesControlMode)) &&
                    (d.PORTIDLIST.Contains(curPort.PortID) || d.PORTIDLIST.Equals("ALL")))
                                                             select row).ToList();
                
                #region 設定Port層物件
                var _portObj = _lstCstControl.FindAll(r => r.OBJECTLEVEL.Equals("Port"));
                foreach (Control _ctrl in flpCassetteInfo.Controls)
                {
                    SBRM_CST_CONTROL_DEF _portUse = _portObj.Find(r => r.OBJECTNAME.Equals(_ctrl.Name));

                    if (_portUse != null) _ctrl.Visible = true;
                    else
                    {
                        _ctrl.Visible = false;
                        continue;
                    }

                    #region ComboBox 設定combobox item
                    _objName = string.Format("cbo{0}", _ctrl.Name.Substring(3));

                    if (_ctrl.Controls.Find(_objName, false).OfType<ComboBox>().Count() > 0)
                    {
                        ComboBox _cbo = _ctrl.Controls.Find(_objName, false).OfType<ComboBox>().First();

                        if (_portUse.COMBOBOXITEMS != string.Empty)
                        {
                            InitialCombox(_cbo, null, _portUse.COMBOBOXITEMS, _portUse.DEFAULTVALUE);
                        }

                        #region 設定Empty Check --用來判斷不可為空值物件
                        if (_portUse.CHECKEMPTY != null && _portUse.CHECKEMPTY == "Y")
                        {
                            SetEmptyCheck(null, _cbo, _portUse.RANGETEXT);
                        }
                        #endregion

                        continue;
                    }
                    #endregion

                    #region TextBox
                    _objName = string.Format("txt{0}", _ctrl.Name.Substring(3));

                    if (_ctrl.Controls.Find(_objName, false).OfType<TextBox>().Count() > 0)
                    {
                        TextBox _txt = _ctrl.Controls.Find(_objName, false).OfType<TextBox>().First();

                        #region 設定Range Condition
                        if (_portUse.RANGE != null && _portUse.RANGE != string.Empty)
                        {
                            SetRangeCondition(_txt, _portUse.RANGE, _portUse.RANGETEXT);
                        }
                        #endregion

                        #region 設定Empty Check --用來判斷不可為空值物件
                        if (_portUse.CHECKEMPTY != null && _portUse.CHECKEMPTY == "Y")
                        {
                            SetEmptyCheck(_txt,null, _portUse.RANGETEXT);
                        }
                        #endregion

                        #region 設定default value
                        if (_portUse.DEFAULTVALUE != null && _portUse.DEFAULTVALUE != string.Empty)
                        {
                            _txt.Text = _portUse.DEFAULTVALUE;
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion

                #region 設定Cassette層物件
                var _cassetteObj = _lstCstControl.FindAll(r => r.OBJECTLEVEL.Equals("Cassette"));
                foreach (Control _ctrl in flpLotInfo.Controls)
                {
                    if (_ctrl.Name == "flpLotList") continue;

                    SBRM_CST_CONTROL_DEF _cassetteUse = _cassetteObj.Find(r => r.OBJECTNAME.Equals(_ctrl.Name));

                    if (_cassetteUse != null) _ctrl.Visible = true;
                    else
                    {
                        _ctrl.Visible = false;
                        continue;
                    }

                    #region Only For CST Operation Mode=2 (Lot to Lot) && 只有在changer mode時才需要顯示設定
                    if (_ctrl.Name == "pnlTargetCSTID_CF")
                    {
                        if (FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode != eCSTOperationMode.LotToLot ||
                            FormMainMDI.G_OPIAp.CurLine.IndexerMode != 3)
                        {
                            _ctrl.Visible = false;
                            continue;
                        }
                    }
                    #endregion

                    #region ComboBox 設定combobox item
                    _objName = string.Format("cbo{0}", _ctrl.Name.Substring(3));
                    if (_ctrl.Controls.Find(_objName, false).OfType<ComboBox>().Count() > 0)
                    {
                        ComboBox _cbo = _ctrl.Controls.Find(_objName, false).OfType<ComboBox>().First();

                        if (_cassetteUse.COMBOBOXITEMS != string.Empty)
                        {
                            InitialCombox(_cbo, null, _cassetteUse.COMBOBOXITEMS, _cassetteUse.DEFAULTVALUE);
                        }

                        #region 設定Empty Check --用來判斷不可為空值物件
                        if (_cassetteUse.CHECKEMPTY != null && _cassetteUse.CHECKEMPTY == "Y")
                        {
                            SetEmptyCheck(null, _cbo, _cassetteUse.RANGETEXT);
                        }
                        #endregion

                        continue;
                    }
                    #endregion

                    #region TextBox
                    _objName = string.Format("txt{0}", _ctrl.Name.Substring(3));
                    if (_ctrl.Controls.Find(_objName, false).OfType<TextBox>().Count() > 0)
                    {
                        TextBox _txt = _ctrl.Controls.Find(_objName, false).OfType<TextBox>().First();

                        #region 設定Range Condition
                        if (_cassetteUse.RANGE != null && _cassetteUse.RANGE != string.Empty)
                        {
                            SetRangeCondition(_txt, _cassetteUse.RANGE, _cassetteUse.RANGETEXT);
                        }
                        #endregion

                        #region 設定Empty Check --用來判斷不可為空值物件
                        if (_cassetteUse.CHECKEMPTY != null && _cassetteUse.CHECKEMPTY != string.Empty)
                        {
                            SetEmptyCheck(_txt,null, _cassetteUse.RANGETEXT);
                        }
                        #endregion

                        #region 設定default value
                        if (!string.IsNullOrEmpty(_cassetteUse.DEFAULTVALUE ))
                        {
                            _txt.Text = _cassetteUse.DEFAULTVALUE;
                        }
                        #endregion
                    }
                    #endregion

                }
                #endregion

                #region 設定Product Common層物件 --需同步設定dataGridView
                var _commonObj = _lstCstControl.FindAll(r => r.OBJECTLEVEL.Equals("Common"));
                
                foreach (Control _ctrl in flpSlotData.Controls)
                {
                    SBRM_CST_CONTROL_DEF _commonUse = _commonObj.Find(r => r.OBJECTNAME.Equals(_ctrl.Name));

                    if (_commonUse != null) _ctrl.Visible = true;
                    else
                    {
                        _ctrl.Visible = false;
                    }
                    #region DataGridView 
                    _colName = string.Format("col{0}", _ctrl.Name.Substring(3));

                    if (dgvProduct.Columns.Contains(_colName))
                    {
                        dgvProduct.Columns[_colName].Visible = _ctrl.Visible;
                    }
                    #endregion

                    if (_commonUse == null) continue;

                    #region 設定Empty Check --用來判斷不可為空值物件
                    if (!string.IsNullOrEmpty(_commonUse.CHECKEMPTY))
                    {
                        _objName = string.Format("chk{0}", _ctrl.Name.Substring(3));

                        if (_objName == "chkProcessFlag") _objName = "chkProcess";

                        CheckBox _chk = _ctrl.Controls.Find(_objName, false).OfType<CheckBox>().FirstOrDefault();
                        if (_chk != null)
                        {
                            _chk.BackColor = Color.Blue;
                            _chk.Checked = true;
                            //_chk.Enabled = false;
                            LstCheckEmpty_Slot.Add(_colName);
                        }
                    }
                    #endregion

                    #region ComboBox 設定combobox item
                    _objName = string.Format("cbo{0}", _ctrl.Name.Substring(3));
                    if (_ctrl.Controls.Find(_objName, false).OfType<ComboBox>().Count() > 0)
                    {
                        ComboBox _cbo = _ctrl.Controls.Find(_objName, false).OfType<ComboBox>().First();

                        if (_commonUse.COMBOBOXITEMS != string.Empty)
                        {
                            DataGridViewComboBoxColumn _colCbo = null;

                            if (dgvProduct.Columns.Contains(_colName))
                            {
                                if (dgvProduct.Columns[_colName].CellType.Name == "DataGridViewComboBoxCell")
                                {
                                    _colCbo = (DataGridViewComboBoxColumn)dgvProduct.Columns[_colName];
                                }
                            }
                            InitialCombox(_cbo, _colCbo, _commonUse.COMBOBOXITEMS, _commonUse.DEFAULTVALUE);
                        }
                        continue;
                    }
                    #endregion

                    #region TextBox
                    _objName = string.Format("txt{0}", _ctrl.Name.Substring(3));
                    if (_ctrl.Controls.Find(_objName, false).OfType<TextBox>().Count() > 0)
                    {
                        TextBox _txt = _ctrl.Controls.Find(_objName, false).OfType<TextBox>().First();

                        #region 設定Object Tag
                        if (_commonUse.OBJECTTAG != null && _commonUse.OBJECTTAG != string.Empty)
                        {
                            _txt.Tag = _commonUse.OBJECTTAG;
                        }
                        #endregion

                        #region 設定Range Condition
                        if (_commonUse.RANGE != null && _commonUse.RANGE != string.Empty)
                        {
                            SetRangeCondition(_txt, _commonUse.RANGE, _commonUse.RANGETEXT);
                        }
                        #endregion

                        #region UPK 要自動產生glass id 不提供給人員修改 : 1+yymmdd+CSTSeqNo*5+JobSeqNo*4
                        if (FormMainMDI.G_OPIAp.CurLine.LineType == "FCUPK_TYPE1" && _txt.Name == "txtGlassID")
                        {                            
                            _txt.ReadOnly = true;
                        }
                        #endregion

                        #region 設定default value
                        if (_commonUse.DEFAULTVALUE != null && _commonUse.DEFAULTVALUE != string.Empty)
                        {
                            _txt.Text = _commonUse.DEFAULTVALUE;
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion

                #region 設定Product Special層物件 --需同步設定dataGridView
                var _sepcialObj = _lstCstControl.FindAll(r => r.OBJECTLEVEL.Equals("Special"));
                foreach (Control _ctrl in flpSlotData_Special.Controls)
                {
                    SBRM_CST_CONTROL_DEF _specialUse = _sepcialObj.Find(r => r.OBJECTNAME.Equals(_ctrl.Name));

                    if (_specialUse != null) _ctrl.Visible = true;  
                    else _ctrl.Visible = false;

                    #region DataGridView
                    _colName = string.Format("col{0}", _ctrl.Name.Substring(3));

                    if (dgvProduct.Columns.Contains(_colName))
                    {
                        dgvProduct.Columns[_colName].Visible = _ctrl.Visible;
                    }
                    #endregion

                    if (_specialUse == null) continue;

                    #region 設定Empty Check --用來判斷不可為空值物件
                    if (_specialUse.CHECKEMPTY != null && _specialUse.CHECKEMPTY == "Y")
                    {
                        _objName = string.Format("chk{0}", _ctrl.Name.Substring(3));
                        CheckBox _chk = _ctrl.Controls.Find(_objName, false).OfType<CheckBox>().FirstOrDefault();
                        if (_chk != null)
                        {
                            _chk.BackColor = Color.Blue;
                            _chk.Checked = true;
                            //_chk.Enabled = false;
                            LstCheckEmpty_Slot.Add(_colName);
                        }
                    }
                    #endregion

                    #region ComboBox 設定combobox item
                    _objName = string.Format("cbo{0}", _ctrl.Name.Substring(3));

                    if (_ctrl.Controls.Find(_objName, false).OfType<ComboBox>().Count() > 0)
                    {
                        ComboBox _cbo = _ctrl.Controls.Find(_objName, false).OfType<ComboBox>().First();

                        if (_specialUse.COMBOBOXITEMS != string.Empty)
                        {
                            DataGridViewComboBoxColumn _colCbo = null;

                            if (dgvProduct.Columns.Contains(_colName))
                            {
                                if (dgvProduct.Columns[_colName].CellType.Name == "DataGridViewComboBoxCell")
                                {
                                    _colCbo = (DataGridViewComboBoxColumn)dgvProduct.Columns[_colName];
                                }
                            }

                            InitialCombox(_cbo, _colCbo, _specialUse.COMBOBOXITEMS, _specialUse.DEFAULTVALUE);
                        }
                        continue;
                    }

                    #endregion

                    #region TextBox
                    _objName = string.Format("txt{0}", _ctrl.Name.Substring(3));

                    if (_ctrl.Controls.Find(_objName, false).OfType<TextBox>().Count() > 0)
                    {
                        TextBox _txt = _ctrl.Controls.Find(_objName, false).OfType<TextBox>().First();

                        #region 設定Object Tag
                        if (_specialUse.OBJECTTAG != null && _specialUse.OBJECTTAG != string.Empty)
                        {
                            _txt.Tag = _specialUse.OBJECTTAG;
                        }
                        #endregion

                        #region 設定Range Condition
                        if (_specialUse.RANGE != null && _specialUse.RANGE != string.Empty)
                        {
                            SetRangeCondition(_txt, _specialUse.RANGE, _specialUse.RANGETEXT);
                        }
                        #endregion

                        #region 設定default value
                        if (_specialUse.DEFAULTVALUE != null && _specialUse.DEFAULTVALUE != string.Empty)
                        {
                            _txt.Text = _specialUse.DEFAULTVALUE;
                        }
                        #endregion
                    }    

                    #endregion
                }
                #endregion

                #region 設定DataGridView 欄位顯示處理(不存在special層物件卻需要by line顯示設定)
                List<string> _specialColumns = new List<string> { "colChoose", "colPPID", "colTargetCSTID", "colTargetSlotNo" };
                var _slottObj = _lstCstControl.FindAll(r => r.OBJECTLEVEL.Equals("Slot"));

                foreach (string _col in _specialColumns)
                {
                    if (_slottObj.Find(r => r.OBJECTNAME.Equals(_col)) != null)
                        dgvProduct.Columns[_col].Visible = true;
                    else
                        dgvProduct.Columns[_col].Visible = false;


                    
                    #region Only For CST Operation Mode=2 (Lot to Lot)
                    if (dgvProduct.Columns[_col].Name == "colTargetSlotNo") 
                    {
                        if (FormMainMDI.G_OPIAp.CurLine.CassetteOperationMode != eCSTOperationMode.LotToLot)
                        {
                            dgvProduct.Columns[_col].Visible = false;
                            btnTargetSlotNo.Visible = false;
                        }
                    }
                    #endregion
                }
                #endregion

                btnRecipeCheck.Visible = true;

                #endregion

                #region cboJobType  -- array 需要出視窗選擇
                if (FormMainMDI.G_OPIAp.CurLine.FabType == "ARRAY")
                {
                    btnProductType.Visible = true;
                    txtProductType.ReadOnly = true;
                }
                else
                {
                    btnProductType.Visible = false;
                    txtProductType.ReadOnly = false;
                }
                #endregion

                #region tlpBase 判斷是否有ppid,調整長度
                if (flpLotRecipeID.Visible)
                {
                    tlpBase.RowStyles[2].Height = 140;
                }
                else
                {
                    tlpBase.RowStyles[2].Height = 110;
                }
                #endregion

                #region 判斷是否需要slot recipe setting button 設定按鈕
                if (colRecipeID.Visible ==false )
                {
                    btnRecipe.Visible = false;
                }
                else
                {
                    btnRecipe.Visible = true;
                }
                #endregion

                #region 判斷是否有special item~調整group box大小
                if (flpSlotData_Special.Controls.OfType<Panel>().Where(r=>r.Visible.Equals(true)).Count() > 0)
                {
                    grbSlotCommon.Width = 705;
                    grbSlotSpecial.Width = 375;
                    grbSlotCommon.Height = 200;
                    tlpBase.RowStyles[3].Height = 210;
                    grbSlotSpecial.Visible = true;
                }
                else
                {
                    grbSlotCommon.Width = 1084;
                    grbSlotCommon.Height = 150;
                    tlpBase.RowStyles[3].Height = 160;
                    grbSlotSpecial.Visible = false;
                }
                #endregion

                #region 三個shop提供dgvProduct 可修改 colGlassID
                colGlassID.ReadOnly = false;
                if (FormMainMDI.G_OPIAp.CurLine.FabType == "CF")
                {
                    colGlassID.MaxInputLength = FormMainMDI.G_OPIAp.GlassIDMaxLength + 3;
                }
                else if (FormMainMDI.G_OPIAp.CurLine.FabType == "ARRAY")
                {
                    colGlassID.MaxInputLength = FormMainMDI.G_OPIAp.GlassIDMaxLength + 2;
                }
                else if (FormMainMDI.G_OPIAp.CurLine.FabType == "CELL")
                {
                    colGlassID.MaxInputLength = FormMainMDI.G_OPIAp.GlassIDMaxLength + 3;
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void SetEmptyCheck(TextBox Txt,ComboBox Cbo, string Description)
        {
            try
            {
                string _lblName = string.Empty;

                #region 變更Label顏色
                if (Txt != null) _lblName = string.Format("lbl{0}", Txt.Name.Substring(3));
                else if (Cbo != null) _lblName = string.Format("lbl{0}", Cbo.Name.Substring(3));

                Label _lbl = tlpBase.Controls.Find(_lblName,true).OfType<Label>().First();

                _lbl.BackColor = Color.Blue;
                _lbl.ForeColor = Color.White;
                #endregion

                CheckEmpty _check = new CheckEmpty();
                _check.TxtObject = Txt;
                _check.CboObject = Cbo;
                _check.RangeText = Description;

                LstCheckEmpty.Add(_check);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetRangeCondition(TextBox  Obj, string Range, string RangeText)
        {
            try
            {
                int _num = 0;

                RangeCondition _range = new RangeCondition();
                _range.ObjectName = Obj.Name;
                _range.Range = Range;
                _range.RangeText = RangeText;

                string[] _data = _range.Range.Split(',');

                if (_data.Length != 2) return ;

                int.TryParse(_data[0], out _num);
                _range.Min = _num;

                int.TryParse(_data[1], out _num);
                _range.Max = _num;

                if (_range.Min > _range.Max) return;

                LstRangeCondition.Add(_range);

                Obj.TextChanged += new EventHandler(txtRange_TextChanged);                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialCombox(ComboBox Cbo,DataGridViewComboBoxColumn ColumnCbo, string Items,string DefaultValue)
        {
            try
            {
                bool _useDesc = true; //判斷id & name相同，則不使用desc顯示

                string[] _items = Items.Split(',');
                List<comboxInfo> _lst = new List<comboxInfo>();

                foreach (string _item in _items)
                {
                    string[] _det = _item.Split(':');
                    if (_det.Count() < 2)
                    {
                        _useDesc = false;
                        comboxInfo _jobItem = new comboxInfo();

                        _jobItem.ITEM_ID = _det[0];
                        _jobItem.ITEM_NAME = _det[0];
                        _lst.Add(_jobItem);
                    }
                    else
                    {
                        _useDesc = true;
                        comboxInfo _jobItem = new comboxInfo();

                        _jobItem.ITEM_ID = _det[0];
                        _jobItem.ITEM_NAME = _det[1];
                        _lst.Add(_jobItem);
                    }
                }

                if (_lst != null && _lst.Count > 0)
                {
                    if (Cbo != null)
                    {
                        Cbo.DataSource = _lst.ToList();
                        Cbo.DisplayMember = _useDesc ? "ITEM_DESC" : "ITEM_ID";
                        Cbo.ValueMember = "ITEM_ID";
                        Cbo.SelectedIndex = -1;
                    }

                    if (ColumnCbo != null)
                    {
                        ColumnCbo.DataSource = _lst.ToList();
                        ColumnCbo.DisplayMember = _useDesc ? "ITEM_DESC" : "ITEM_ID";
                        ColumnCbo.ValueMember = "ITEM_ID";
                    }
                }

                if (DefaultValue != null && DefaultValue.Trim() != string.Empty)
                {
                    if (Cbo.Name == "cboJobGrade")
                    {
                        Cbo.Text = DefaultValue;
                    }
                    else
                    {
                        Cbo.SelectedValue = DefaultValue;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialFormInfo()
        {
            try
            {
                #region 清除基本畫面

                #region CASSETTE
                txtCassetteID.Text = string.Empty;
                txtSlotGlassCount.Text = "0";
                txtCassetteSettingCode_Port.Text = string.Empty;
                #endregion

                #region LOT
                foreach (Panel _pnl in flpLotInfo.Controls.OfType<Panel>())
                {
                    foreach (TextBox _txt in _pnl.Controls.OfType<TextBox>())
                    {
                        _txt.Text = string.Empty;
                    }
                }

                txtLotRecipeID.ReadOnly = true;
                #endregion

                #region Common & Special slot item
                txtSelFrSlotNo.Text = string.Empty;
                txtSelToSlotNo.Text = string.Empty;
                dgvProduct.Rows.Clear();

                foreach (Panel _pnl in flpSlotData.Controls.OfType<Panel>())
                {
                    foreach (Control _ctrl in _pnl.Controls)
                    {
                        switch (_ctrl.GetType().Name)
                        {
                            case "CheckBox":
                                if (_ctrl.Name == "chkProcessFlag") ((CheckBox)_ctrl).Checked = true;
                                else ((CheckBox)_ctrl).Checked = false;
                                break;
                            case "TextBox":
                                ((TextBox)_ctrl).Text = string.Empty;
                                break;
                            case "ComboBox":
                                ((ComboBox)_ctrl).SelectedValue = string.Empty;
                                break;
                        }
                    }
                }
                foreach (Panel _pnl in flpSlotData_Special.Controls.OfType<Panel>())
                {
                    foreach (Control _ctrl in _pnl.Controls)
                    {
                        switch (_ctrl.GetType().Name)
                        {
                            case "CheckBox":
                                ((CheckBox)_ctrl).Checked = false;
                                break;
                            case "TextBox":
                                ((TextBox)_ctrl).Text = string.Empty;
                                break;
                            case "ComboBox":
                                ((ComboBox)_ctrl).SelectedValue = string.Empty;
                                break;
                        }
                    }
                }
                //chkProcessFlag.Checked = true;              
                //txtGlassID.Text = string.Empty;
                //txtRecipeID.Text = string.Empty;

                //txtGroupIndex.Text = string.Empty;
                //txtOwnerID.Text = string.Empty;                
                //txtRevProcOperName.Text = string.Empty;
                //txtEQPFlag.Text = string.Empty;
                //txtFlowPriorityInfo.Text = string.Empty;
                //txtInspReservations.Text = string.Empty;                
                //txtCOAVersion.Text = string.Empty;
                //txtNetworkNo.Text = string.Empty;

                //cboSubstrateType.SelectedValue = string.Empty;
                //cboJobType.SelectedValue = string.Empty;
                //cboJobJudge.SelectedValue = string.Empty;
                //cboOwnerType.SelectedValue = string.Empty;
                //cboProcessType.SelectedValue = string.Empty;
                //cboPreInlineID.SelectedValue = string.Empty;
                //cboJobGrade.SelectedValue = string.Empty;

                #region UPK 要自動產生glass id 不提供給人員修改 : 1+yymmdd+CSTSeqNo*5+JobSeqNo*4
                if (FormMainMDI.G_OPIAp.CurLine.LineType == "FCUPK_TYPE1")
                {
                    txtGlassID.Text = string.Format("1{0}{1}", DateTime.Now.ToString("yyyyMMdd"), curPort.CassetteSeqNo.PadLeft(5, '0'));
                }
                #endregion

                #endregion

                btnMapDownload.Enabled = false;

                #endregion

                #region gridview欄位修改設定
                //1. CELL 對應datagridview內的recipe id不可被修改
                //2. AC (CF & ARRAY) datagridview內的job type不可被修改
                if (FormMainMDI.G_OPIAp.CurLine.FabType == "CELL")
                {
                    dgvProduct.Columns["colRecipeID"].ReadOnly = true;
                    dgvProduct.Columns[colJobType.Name].ReadOnly = false;
                }
                else
                {
                    dgvProduct.Columns["colRecipeID"].ReadOnly = false;
                    dgvProduct.Columns[colJobType.Name].ReadOnly = true;
                }
                #endregion

                #region by line 修改顯示名稱

                if (FormMainMDI.G_OPIAp.CurLine.LineType.Equals("PCS"))
                {
                    lblProductType_CUT.Text = "PCS Product Type";
                    lblProductID_CUT.Text = "PCS Product ID";
                    lblCassetteSettingCode_CUT.Text = "PCS CST Setting Code";
                }

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void EnableGlassData(bool IsEnable)
        {
            try
            {
                #region 設定物件是否可被修改

                #region CASSETTE
                txtCassetteID.ReadOnly = true;  //Cassette ID不可被修改
                txtSlotGlassCount.ReadOnly = true;

                //cboLineOperMode.Enabled = IsEnable;
                
                //PORT 層Cassette Setting Code不管是否空CST / LD UD都需要填 
                txtCassetteSettingCode_Port.Enabled = true;
                #endregion

                #region LOT
                foreach (Panel _pnl in flpLotInfo.Controls.OfType<Panel>())
                {
                    foreach (TextBox _txt in _pnl.Controls.OfType<TextBox>())
                    {
                        _txt.Enabled = IsEnable;
                    }
                }

                cboProductOwner.Enabled = IsEnable;

                btnProductType.Enabled = IsEnable;

                btnLotRecipeID.Enabled = IsEnable;

                #endregion

                #region Slot
                chkProcessFlag.Enabled = IsEnable;

                foreach (Panel _pnl in flpSlotData.Controls.OfType<Panel>())
                {
                    foreach (Control _ctrl in _pnl.Controls)
                    {
                        _ctrl.Enabled = IsEnable;
                    }
                }

                foreach (Panel _pnl in flpSlotData_Special.Controls.OfType<Panel>())
                {
                    foreach (Control _ctrl in _pnl.Controls)
                    {
                        _ctrl.Enabled = IsEnable;
                    }
                }


                //txtGlassID.Enabled = IsEnable;
                //txtRecipeID.Enabled = IsEnable;                
                //txtGroupIndex.Enabled = IsEnable;
                ////txtOXR.Enabled = IsEnable;
                //txtOwnerID.Enabled = IsEnable;                
                //txtRevProcOperName.Enabled = IsEnable;
                //txtEQPFlag.Enabled = IsEnable;
                //txtFlowPriorityInfo.Enabled = IsEnable;
                //txtInspReservations.Enabled = IsEnable;                
                //txtCOAVersion.Enabled = IsEnable;
                //txtNetworkNo.Enabled = IsEnable;
                ////txtRepairCount.Enabled = IsEnable;
                ////txtAbnormalCode.Enabled = IsEnable;
                //txtGroupIndex.Enabled = IsEnable;
                //txtPanelSize.Enabled = IsEnable;
                ////txtPanelSize_Cross.Enabled = IsEnable;
                ////txtReworkCount.Enabled = IsEnable;
                ////txtNodeStack.Enabled = IsEnable;
                //txtBlockSize.Enabled = IsEnable;

                //cboJobType.Enabled = IsEnable;
                //cboJobGrade.Enabled = IsEnable;
                //cboOwnerType.Enabled = IsEnable;
                ////cboTempFlag.Enabled = IsEnable;
                //cboJobJudge.Enabled = IsEnable;
                //cboProcessType.Enabled = IsEnable;
                //cboPreInlineID.Enabled = IsEnable;
                ////cboAgingEnable.Enabled = IsEnable;
                ////cboOXRFlag.Enabled = IsEnable;
                ////cboScrapCutFlag.Enabled = IsEnable;
                //cboTurnAngleFlag.Enabled = IsEnable;
                
                ////cboPanelSizeFlag_Cross.Enabled = IsEnable;
                ////cboArrayTTPEQVersion.Enabled = IsEnable;
                ////cboRepairResult.Enabled = IsEnable;
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        #region Socket Process

        private void SendtoBC_OfflineModeCassetteDataSend()
        {
            try
            {
                string _err = string.Empty;
                string _ppid = string.Empty;

                OfflineModeCassetteDataSend trx = new OfflineModeCassetteDataSend();
                trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                trx.BODY.EQUIPMENTNO = curPort.NodeNo;
                trx.BODY.PORTNO = curPort.PortNo;
                trx.BODY.PORTID = curPort.PortID;
                trx.BODY.CASSETTEID = curPort.CassetteID;
                trx.BODY.PRODUCTQUANTITY = txtSlotGlassCount.Text.ToString();
                trx.BODY.REMAPFLAG = IsRemap ? "Y" : "N";
                trx.BODY.CSTSETTINGCODE = txtCassetteSettingCode_Port.Text.ToString().PadRight(4,' ');

                OfflineModeCassetteDataSend.LOTDATAc _lot = new OfflineModeCassetteDataSend.LOTDATAc();
                _lot.LOTNAME = txtLotID.Text.ToString();
                _lot.PROCESSOPERATIONNAME = txtProcOperName.Text.ToString();
                _lot.PRODUCTOWNER = cboProductOwner.SelectedValue == null ? cboProductOwner.Text : cboProductOwner.SelectedValue.ToString(); //txtProdOwner.Text.ToString();
                _lot.PRODUCTSPECNAME = txtProdSpecName.Text.ToString();

                _lot.BCPRODUCTTYPE = txtProductType.Text.ToString().Trim() ==string.Empty ? "0": txtProductType.Text.ToString();
                _lot.BCPRODUCTTYPE_CUT = txtProductType_CUT.Text.ToString().Trim() == string.Empty ? "0" : txtProductType_CUT.Text.ToString();

                _lot.PRODUCTID = txtProductID.Text.ToString().Trim() == string.Empty ? "0" : txtProductID.Text.ToString();
                _lot.PRODUCTID_CUT = txtProductID_CUT.Text.ToString().Trim() == string.Empty ? "0" : txtProductID_CUT.Text.ToString();

                _lot.CFREWORKCOUNT = txtReworkMaxCount.Text.ToString() == string.Empty ? "0" : txtReworkMaxCount.Text.ToString();
                _lot.TARGETCSTID_CF = txtTargetCSTID_CF.Text.ToString();

                _lot.LINERECIPENAME = txtLotRecipeID.Tag == null ? string.Empty : txtLotRecipeID.Tag.ToString();
                _lot.PPID = txtLotRecipeID.Text.ToString();
                _lot.CSTSETTINGCODE = txtCassetteSettingCode.Text.ToString().PadRight(4, ' '); ;
                _lot.CSTSETTINGCODE_CUT = txtCassetteSettingCode_CUT.Text.ToString().PadRight(4, ' '); ;
                _lot.CSTSETTINGCODE2 = txtCassetteSettingCode_PCS2.Text.ToString().PadRight(4,' ');
                _lot.PCSCSTSETTINGCODELIST = txtPCSCassetteSettingCodeList.Text.ToString().PadRight(16, '0');
                _lot.PCSBLOCKSIZELIST = txtPCSBlockSizeList.Text.ToString().PadRight(16, '0');
                _lot.BLOCKSIZE1 = txtBlockSize1.Text.ToString();
                _lot.BLOCKSIZE2 = txtBlockSize2.Text.ToString();
                trx.BODY.LOTLIST.Add(_lot);

                DataGridViewRow row = null;
                for (int i = dgvProduct.Rows.Count - 1; i >= 0; i--)
                {
                    row = dgvProduct.Rows[i];

                    if (row.Cells[colGlassID.Name].Value == null) continue;
                    if (row.Cells[colGlassID.Name].Value.ToString() == string.Empty) continue;

                    OfflineModeCassetteDataSend.PRODUCTDATAc _product = new OfflineModeCassetteDataSend.PRODUCTDATAc();

                    _product.SLOTNO = (row.Cells[colSlotNo.Name].Value == null ? string.Empty : row.Cells[colSlotNo.Name].Value.ToString());
                    _product.PROCESSFLAG = (bool)row.Cells[colProcessFlag.Name].Value ? "Y" : "N";
                    _product.PRODUCTNAME = (row.Cells[colGlassID.Name].Value == null ? string.Empty : row.Cells[colGlassID.Name].Value.ToString());
                    _product.PRODUCTRECIPENAME = (row.Cells[colRecipeID.Name].Value == null ? string.Empty : row.Cells[colRecipeID.Name].Value.ToString());
                    _product.PRODUCTTYPE = (row.Cells[colJobType.Name].Value == null ? "0" : row.Cells[colJobType.Name].Value.ToString());
                    _product.SUBSTRATETYPE = (row.Cells[colSubstrateType.Name].Value == null ? "0" : row.Cells[colSubstrateType.Name].Value.ToString());
                    _product.PRODUCTJUDGE = (row.Cells[colJobJudge.Name].Value == null ? "0" : row.Cells[colJobJudge.Name].Value.ToString());
                    _product.PRODUCTGRADE = (row.Cells[colJobGrade.Name].Value == null ? "0" : row.Cells[colJobGrade.Name].Value.ToString());
                    _product.GROUPID = (row.Cells[colGroupIndex.Name].Value == null ? string.Empty : row.Cells[colGroupIndex.Name].Value.ToString());
                    _product.PROCESSTYPE = (row.Cells[colProcessType.Name].Value == null ? "0" : row.Cells[colProcessType.Name].Value.ToString()); ;
                    _product.TARGETCSTID = (row.Cells[colTargetCSTID.Name].Value == null ? string.Empty : row.Cells[colTargetCSTID.Name].Value.ToString()); 
                    _product.COAVERSION = (row.Cells[colCOAVersion.Name].Value == null ? string.Empty : row.Cells[colCOAVersion.Name].Value.ToString());
                    _product.INSPRESERVATION = (row.Cells[colInspReservations.Name].Value == null ? string.Empty : row.Cells[colInspReservations.Name].Value.ToString());
                    _product.PREINLINEID = (row.Cells[colPreInlineID.Name].Value == null ? "0" : row.Cells[colPreInlineID.Name].Value.ToString());
                    _product.FLOWPRIORITY = (row.Cells[colFlowPriorityInfo.Name].Value == null ? string.Empty : row.Cells[colFlowPriorityInfo.Name].Value.ToString());
                    _product.NETWORKNO = (row.Cells[colNetworkNo.Name].Value == null ? string.Empty : row.Cells[colNetworkNo.Name].Value.ToString());
                    _product.OWNERTYPE = (row.Cells[colOwnerType.Name].Value == null ? string.Empty : row.Cells[colOwnerType.Name].Value.ToString());
                    _product.OWNERID = (row.Cells[colOwnerID.Name].Value == null ? string.Empty : row.Cells[colOwnerID.Name].Value.ToString());
                    _product.REVPROCESSOPERATIONNAME = (row.Cells[colRevProcOperName.Name].Value == null ? string.Empty : row.Cells[colRevProcOperName.Name].Value.ToString());
                    _product.EQPFLAG = (row.Cells[colEQPFlag.Name].Value == null ? string.Empty : row.Cells[colEQPFlag.Name].Value.ToString());
                    _product.OPI_PPID = row.Cells[colPPID.Name].Value.ToString();
                    _product.SCRAPCUTFLAG = (row.Cells[colScrapCutFlag.Name].Value == null ? "" : row.Cells[colScrapCutFlag.Name].Value.ToString());
                    _product.PANELSIZE = (row.Cells[colPanelSize.Name].Value == null ? "" : row.Cells[colPanelSize.Name].Value.ToString());
                    _product.TRUNANGLEFLAG = (row.Cells[colTurnAngleFlag.Name].Value == null ? "" : row.Cells[colTurnAngleFlag.Name].Value.ToString());
                    _product.BLOCK_SIZE = (row.Cells[colBlockSize.Name].Value == null ? "" : row.Cells[colBlockSize.Name].Value.ToString());
                    _product.CFINLINEREWORKMAXCOUNT = (row.Cells[colInlineReworkCount.Name].Value == null ? "0" : row.Cells[colInlineReworkCount.Name].Value.ToString());
                    _product.UV_MASK_USE_COUNT = (row.Cells[colUVMaskUseCount.Name].Value == null ? "0" : row.Cells[colUVMaskUseCount.Name].Value.ToString());
                    _product.ASSEMBLE_SEQNO = (row.Cells[colAssembleSeqNo.Name].Value == null ? "0" : row.Cells[colAssembleSeqNo.Name].Value.ToString());
                    _product.BLOCK_OX_INFO = (row.Cells[colBlockOX.Name].Value == null ? string.Empty : row.Cells[colBlockOX.Name].Value.ToString());
                    _product.GLASS_THICKNESS = (row.Cells[colGlassThickness.Name].Value == null ? "0" : row.Cells[colGlassThickness.Name].Value.ToString());
                    _product.OPERATION_ID = (row.Cells[colOperationID.Name].Value == null ? string.Empty : row.Cells[colOperationID.Name].Value.ToString());
                    _product.PI_LIQUID_TYPE = (row.Cells[colPILiquidType.Name].Value == null ? string.Empty  : row.Cells[colPILiquidType.Name].Value.ToString());
                    _product.TARGET_SLOTNO = (row.Cells[colTargetSlotNo.Name].Value == null ? "" : row.Cells[colTargetSlotNo.Name].Value.ToString());
                    _product.OXR = (row.Cells[colOXR.Name].Value == null ? string.Empty : row.Cells[colOXR.Name].Value.ToString());
                    _product.VENDER_NAME = (row.Cells[colVendorName.Name].Value == null ? string.Empty : row.Cells[colVendorName.Name].Value.ToString());
                    _product.REJUDGE_COUNT = (row.Cells[colRejudgeCount.Name].Value == null ? "0" : row.Cells[colRejudgeCount.Name].Value.ToString());
                    _product.BUR_CHECK_COUNT = (row.Cells[colBURCheckCount.Name].Value == null ? "0" : row.Cells[colBURCheckCount.Name].Value.ToString());
                    _product.DOT_REPAIR_COUNT = (row.Cells[colDotRepairCount.Name].Value == null ? "0" : row.Cells[colDotRepairCount.Name].Value.ToString());
                    _product.LINE_REPAIR_COUNT = (row.Cells[colLineRepairCount.Name].Value == null ? "0" : row.Cells[colLineRepairCount.Name].Value.ToString());
                    _product.MAX_REWORK_COUNT = (row.Cells[colMaxReworkCount.Name].Value == null ? "0" : row.Cells[colMaxReworkCount.Name].Value.ToString());
                    _product.CURRENT_REWORK_COUNT = (row.Cells[colCurrentReworkCount.Name].Value == null ? "0" : row.Cells[colCurrentReworkCount.Name].Value.ToString());
                    _product.OQC_BANK = (row.Cells[colOQCBank.Name].Value == null ? "0" : row.Cells[colOQCBank.Name].Value.ToString());
                    trx.BODY.LOTLIST[0].PRODUCTLIST.Add(_product);
                }

                string _xml = trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(trx.HEADER.TRANSACTIONID, trx.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SocketResponseTime_MapDownload);

                if (_resp != null)
                {
                    #region OfflineModeCassetteDataSendReply
                    ShowMessage(this, lblCaption.Text, "", "Offline Cassette Command Send to BC Success !", MessageBoxIcon.Information);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SendtoBC_CassetteDataRequest()
        {
            try
            {
                OfflineCassetteDataRequest _trx = new OfflineCassetteDataRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PORTNO = curPort.PortNo;
                _trx.BODY.EQUIPMENTNO = curPort.NodeNo;
                _trx.BODY.PORTID = curPort.PortID;
                _trx.BODY.CASSETTEID = curPort.CassetteID;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;
               
                #region OfflineCassetteDataReply

                string _respXml = _resp.Xml;
                
                RecvMessage_CassetteDataReply(_respXml); //Refresh Data

                EnableGlassData(true);

                btnRecipeCheck.Enabled = true;
                btnMapDownload.Enabled = true;

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void RecvMessage_CassetteMapDownloadResultReport(string xml)
        {
            try
            {
                CassetteMapDownloadResultReport trxData = (CassetteMapDownloadResultReport)Spec.CheckXMLFormat(xml);

                if (trxData.BODY.EQUIPMENTNO == curPort.NodeNo && trxData.BODY.PORTID == curPort.PortID && trxData.BODY.CASSETTEID.Trim() == curPort.CassetteID)
                {
                    if (tlpBase.InvokeRequired)
                    {
                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            switch (trxData.BODY.RESULT)
                            {
                                case "0": txtReturnCode.Text = "0:Unknown"; break;
                                case "1": txtReturnCode.Text = "1:Command OK"; btnMapDownload.Enabled = false; break;
                                case "2": txtReturnCode.Text = "2:Command Error"; break;
                                case "3": txtReturnCode.Text = "3:CST Id Is Invalid"; break;
                                case "4": txtReturnCode.Text = "4:Recipe Mismatch"; break;
                                case "5": txtReturnCode.Text = "5:Slot Information Mismatch"; break;
                                case "6": txtReturnCode.Text = "6:Job Type Mismatch"; break;
                                case "7": txtReturnCode.Text = "7:Cassette Setting Code Mismatch"; break;
                                case "8": txtReturnCode.Text = "8:Alaready Received"; break;
                                case "9": txtReturnCode.Text = "9:Other Error"; break;
                                case "10": txtReturnCode.Text = "10:Product Type Mismatch"; break;
                                case "11": txtReturnCode.Text = "11:Group Index Mismatch"; break;
                                case "12": txtReturnCode.Text = "12:Product ID Mismatch"; break;
                                case "99": txtReturnCode.Text = "99:Equipment response timeout"; break;
                                default: txtReturnCode.Text = string.Empty; break;
                            }

                            //Cassette Control 功能理的Map Download成功的狀況下，自動關閉此畫面，若失敗請告知原因 -- 2014-12-23 by登京
                            if (trxData.BODY.RESULT == "1")
                            {
                                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                                this.Close();
                            }
                        }));
                    }
                    else
                    {
                        switch (trxData.BODY.RESULT)
                        {
                            case "0": txtReturnCode.Text = "0:Unknown"; break;
                            case "1": txtReturnCode.Text = "1:Command OK"; btnMapDownload.Enabled = false; break;
                            case "2": txtReturnCode.Text = "2:Command Error"; break;
                            case "3": txtReturnCode.Text = "3:CST Id Is Invalid"; break;
                            case "4": txtReturnCode.Text = "4:Recipe Mismatch"; break;
                            case "5": txtReturnCode.Text = "5:Slot Information Mismatch"; break;
                            case "6": txtReturnCode.Text = "6:Job Type Mismatch"; break;
                            case "7": txtReturnCode.Text = "7:Cassette Setting Code Mismatch"; break;
                            case "8": txtReturnCode.Text = "8:Alaready Received"; break;
                            case "9": txtReturnCode.Text = "9:Other Error"; break;
                            case "10": txtReturnCode.Text = "10:Product Type Mismatch"; break;
                            case "11": txtReturnCode.Text = "11:Group Index Mismatch"; break;
                            case "12": txtReturnCode.Text = "12:Product ID Mismatch"; break;
                            case "99": txtReturnCode.Text = "99:Equipment response timeout"; break;
                            default: txtReturnCode.Text = string.Empty; break;
                        }

                        //Cassette Control 功能理的Map Download成功的狀況下，自動關閉此畫面，若失敗請告知原因 -- 2014-12-23 by登京
                        if (trxData.BODY.RESULT == "1")
                        {
                            this.DialogResult = System.Windows.Forms.DialogResult.OK;
                            this.Close();
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

        private void RecvMessage_RecipeRegisterValidationReturnReport( string xml)
        {
            try
            {
                string _recipeName = string.Empty;
                string _jobType_Desc = string.Empty;
                string _jobJudge_Desc = string.Empty;
                string _processType_Desc = string.Empty;
                string _preInlineID_Desc = string.Empty;
                string _cuttingFlag_Desc = string.Empty;

                string _msgNG = string.Empty;

                RecipeRegisterValidationReturnReport trxData = (RecipeRegisterValidationReturnReport)Spec.CheckXMLFormat(xml);

                foreach (RecipeRegisterValidationReturnReport.LINEc _line in trxData.BODY.LINELIST)
                {
                    if (FormMainMDI.G_OPIAp.CurLine.ServerName == _line.RECIPELINENAME)
                    {
                        foreach (RecipeRegisterValidationReturnReport.RECIPECHECKc _recipeCheck in _line.RECIPECHECKLIST)
                        {
                            _recipeName = _recipeCheck.RECIPENAME;

                            if (dicRecipeUse.ContainsKey(_recipeName))
                            {
                                foreach (RecipeRegisterValidationReturnReport.EQUIPMENTc _eq in _recipeCheck.EQUIPMENTLIST)
                                {
                                    dicRecipeUse[_recipeName].SetRecipeCheckResult(_eq.EQUIPMENTNO, _eq.RECIPENO, _eq.RETURN, _eq.NGMESSAGE);

                                    if (_eq.RETURN != "OK")
                                    {
                                        _msgNG = _msgNG + string.Format("Local No [{0}], Recipe Name [{1}], Recipe No [{2}]  [{3}] \r\n", _eq.EQUIPMENTNO, _recipeName, _eq.RECIPENO, _eq.NGMESSAGE);

                                    }
                                }
                            }
                        }
                    }
                    //else
                    //{
                    //    foreach (RecipeRegisterValidationReturnReport.RECIPECHECKc _recipeCheck in _line.RECIPECHECKLIST)
                    //    {
                    //        _recipeName = _recipeCheck.RECIPENAME;

                            //if (dicRecipeUse_Cross.ContainsKey(_recipeName))
                            //{
                            //    foreach (RecipeRegisterValidationReturnReport.EQUIPMENTc _eq in _recipeCheck.EQUIPMENTLIST)
                            //    {
                            //        dicRecipeUse_Cross[_recipeName].SetRecipeCheckResult(_eq.EQUIPMENTNO, _eq.RECIPENO, _eq.RETURN, _eq.NGMESSAGE);
                            //    }
                            //}
                    //    }
                    //}
                }

                if (_msgNG != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", _msgNG, MessageBoxIcon.Warning);
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void RecvMessage_CassetteDataReply(string xml)
        {
            try
            {
                RadioButton _firstLot = null;

                string _slotNo = string.Empty;
                string _jobType_Desc = string.Empty;
                string _jobJudge_Desc = string.Empty;
                string _processType_Desc = string.Empty;
                string _preInlineID_Desc = string.Empty;
                string _cuttingFlag_Desc = string.Empty;

                OfflineCassetteDataReply trxData = (OfflineCassetteDataReply)Spec.CheckXMLFormat(xml);

                #region Cassette
                txtCassetteID.Text = trxData.BODY.CASSETTEID;
                txtSlotGlassCount.Text = trxData.BODY.PRODUCTQUANTITY;
                txtCassetteSettingCode_Port.Text = trxData.BODY.CSTSETTINGCODE;
                #endregion


                LstLots = new List<OfflineCassetteDataReply.LOTDATAc>();

                foreach (OfflineCassetteDataReply.LOTDATAc lot in trxData.BODY.LOTLIST)
                {
                    LstLots.Add(lot);

                    RadioButton _lot = new RadioButton();
                    _lot.Name = lot.LOTNAME;
                    _lot.Font = new Font("Calibri", 10.75f, FontStyle.Regular);
                    _lot.Text = lot.LOTNAME;
                    _lot.CheckedChanged += new EventHandler(chkLot_CheckedChanged);
                    _lot.Visible = true;
                    _lot.Checked = false;
                    _lot.Appearance = Appearance.Button;
                    _lot.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
                    flpLotList.Controls.Add(_lot);

                    if (_firstLot == null) _firstLot = _lot;

                }

                if (LstLots.Count > 0)
                {
                    if (LstLots.Count == 1) flpLotList.Visible = false;

                    _firstLot.Checked = true;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        #endregion

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Q_BCSReport.Count > 0)
                {
                    string _msg = Q_BCSReport.Dequeue();
                    string[] _msgItem = _msg.Split('^');

                    if (_msgItem.Length != 2) return ;

                    switch (_msgItem[0])
                    {
                        case "CassetteMapDownloadResultReport":

                            #region CassetteMapDownloadResultReport
                            RecvMessage_CassetteMapDownloadResultReport(_msgItem[1]);
                            #endregion

                            break;

                        case "RecipeRegisterValidationReturnReport":

                            #region RecipeRegisterValidationReturnReport
                            RecvMessage_RecipeRegisterValidationReturnReport(_msgItem[1]);
                            #endregion

                            break;

                        default :
                            break;
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

    class RangeCondition
    {
        public string ObjectName { get; set; }
        public string Range { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public string RangeText { get; set; }
    }

    class CheckEmpty
    {
        public TextBox TxtObject { get; set; }
        public ComboBox CboObject { get; set; }
        public string RangeText { get; set; }
    }
}
