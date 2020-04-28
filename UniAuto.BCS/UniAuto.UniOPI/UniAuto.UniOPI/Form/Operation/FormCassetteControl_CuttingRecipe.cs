using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    //               LOT PPID	                Process Line	                                   STB	 
    //-----------------------------------------------------------------------------------------------------------------------------
    //CBCUT100~300	 L2,L3,0000(補滿至L8)	    0000,L4~L8	                                        X	
    //-----------------------------------------------------------------------------------------------------------------------------
    //CBCUT400	     L2,L3,0000(補滿至L8)	    CUT400 : 0000,L4~L8
    //                                          CUT500 : 0000,L4~L10,0000(補滿至L15)	        CUT500:0000(補滿至L10),L11~L15
    //-----------------------------------------------------------------------------------------------------------------------------	
    //CBCUT500	     L2,L3,0000(補滿至L15)	    0000,L4~L10,0000(補滿至L15)	                    0000(補滿至L10),L11~L15
    //-----------------------------------------------------------------------------------------------------------------------------	  


    //                 Current Line PPID	                                             Cross Line PPID
    //-----------------------------------------------------------------------------------------------------------------------------	  
    //CBCUT100~300	   LOT PPID (L2,L3) + Process Line (L4~L8)	                              X
    //-----------------------------------------------------------------------------------------------------------------------------	  
    //CBCUT400	       LOT PPID (L2,L3) + Process Line (CUT400: L4~L8)	                      Process Line (CUT500: L2~L9) + STB (CUT500 L10~L15) -- L2,L3為0
    //-----------------------------------------------------------------------------------------------------------------------------	  
    //CBCUT500	       LOT PPID (L2,L3) + Process Line (L4~L19) + STB ( L10~ L15)	          X
    //-----------------------------------------------------------------------------------------------------------------------------	 
   
    public partial class FormCassetteControl_CuttingRecipe : FormBase
    {
        public string CurrentLinePPID = string.Empty;
        public string CrossLinePPID = string.Empty;

        public CUT_CSTSettingCodeData LotCstSettingCode;
        public CUT_CSTSettingCodeData ProcCstSettingCode;
        public CUT_CSTSettingCodeData STBCstSettingCode;
        public CUT_CSTSettingCodeData CrossProcCstSettingCode;
        public CUT_CSTSettingCodeData CrossSTBCstSettingCode;

        public CUT_CSTSettingCodeData LotProductType;
        public CUT_CSTSettingCodeData ProcProductType;
        public CUT_CSTSettingCodeData STBProductType;
        public CUT_CSTSettingCodeData CrossProcProductType;
        public CUT_CSTSettingCodeData CrossSTBProductType;

        public CUT_CSTSettingCodeData LotProductID;
        public CUT_CSTSettingCodeData ProcProductID;
        public CUT_CSTSettingCodeData STBProductID;
        public CUT_CSTSettingCodeData CrossProcProductID;
        public CUT_CSTSettingCodeData CrossSTBProductID;

        public Recipe Recipe_Lot_Cutting;   //Cutting Line Lot Recipe Name
        public Recipe Recipe_Proc_Cutting;  //Process Line Recipe Name
        public Recipe Recipe_STB_Cutting;   //STB Prod Recipe Name
        public Recipe Recipe_CrossProc_Cutting;  //Process Line Recipe Name - for cross line
        public Recipe Recipe_CrossSTB_Cutting;  //STB Prod Recipe Name - for cross line
        public Dictionary<string, Recipe> dicRecipe_Cross;

        private string MesControlMode = string.Empty;
        private string CrossServerName = string.Empty;
        private string CrossLineType = string.Empty;
        private Port CurPort;
        private Dictionary<string, Recipe> dicRecipe_All;

        public FormCassetteControl_CuttingRecipe(string mesMode, Dictionary<string, Recipe> dicRecipe,Port port)
        {
            InitializeComponent();

            LotCstSettingCode = new CUT_CSTSettingCodeData();
            ProcCstSettingCode = new CUT_CSTSettingCodeData();
            STBCstSettingCode = new CUT_CSTSettingCodeData();
            CrossProcCstSettingCode = new CUT_CSTSettingCodeData();
            CrossSTBCstSettingCode = new CUT_CSTSettingCodeData();

            LotProductType = new CUT_CSTSettingCodeData();
            ProcProductType = new CUT_CSTSettingCodeData();
            STBProductType = new CUT_CSTSettingCodeData();
            CrossProcProductType = new CUT_CSTSettingCodeData();
            CrossSTBProductType = new CUT_CSTSettingCodeData();

            LotProductID = new CUT_CSTSettingCodeData();
            ProcProductID = new CUT_CSTSettingCodeData();
            STBProductID = new CUT_CSTSettingCodeData();
            CrossProcProductID = new CUT_CSTSettingCodeData();
            CrossSTBProductID = new CUT_CSTSettingCodeData();

            MesControlMode = mesMode;
            CurPort=port;
            dicRecipe_All = dicRecipe;
        }

        private void FormCassetteControl_CuttingRecipe_Load(object sender, EventArgs e)
        {
            try
            {
                string _ppid = string.Empty;

                #region 清除資料
                txtCurrentLinePPID.Text =string.Empty ;
                txtLotLinePPID.Text =string.Empty ;
                txtProcessLinePPID.Text =string.Empty ;
                txtSTBLinePPID.Text =string.Empty ;
                txtLotCassetteSettingCode.Text =string.Empty ;
                txtProcCassetteSettingCode.Text =string.Empty ;
                txtSTBCassetteSettingCode.Text =string.Empty ;

                //txtCrossProcCassetteSettingCode.Text =string.Empty ;
                //txtCrossSTBCassetteSettingCode.Text =string.Empty ;
                //txtCrossLinePPID.Text =string.Empty ;
                //txtProcessLinePPID_Cross.Text =string.Empty ;
                //txtSTBLinePPID_Cross.Text = string.Empty;

                //txtCrossLinePPID.Text = string.Empty;
                #endregion


                #region 本line
                foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                {
                    _ppid = _ppid + (_ppid == string.Empty ? "" : ";") + _node.NodeNo + ":" + _node.DefaultRecipeNo;
                }
                txtCurrentLinePPID.Tag = _ppid;

                #region 物件顯示
                int _num = 0;
                int.TryParse(CurPort.NodeNo.Substring(1), out _num);

                if (_num == 2)  //L2
                {
                    pnlProcessLinePPID.Visible = true;
                    pnlProcCassetteSettingCode.Visible = true;

                    txtLotLinePPID.Tag = "LOT";
                    btnLotLinePPID.Tag = FormMainMDI.G_OPIAp.CurLine.ServerName + ",LOT";
                    btnProcessLinePPID.Tag = FormMainMDI.G_OPIAp.CurLine.ServerName + ",PROCESSLINE";
                    btnSTBLinePPID.Tag = FormMainMDI.G_OPIAp.CurLine.ServerName + ",STB";

                    #region CUT500
                    if (FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_3")
                    {
                        pnlSTBLinePPID.Visible = true;

                        pnlSTBCassetteSettingCode.Visible = true;

                        //grbLocalLine.Size = new Size(884, 250);
                    }
                    else
                    {
                        pnlSTBLinePPID.Visible = false;

                        pnlSTBCassetteSettingCode.Visible = false;

                        //grbLocalLine.Size = new Size(884, 190);
                    }
                    #endregion
                }
                if (_num >= 4 && _num <= 9) //L4-L9
                {
                    pnlProcessLinePPID.Visible = false;
                    pnlProcCassetteSettingCode.Visible = false;

                    txtLotLinePPID.Tag = "PROCESSLINE";
                    btnLotLinePPID.Tag = FormMainMDI.G_OPIAp.CurLine.ServerName + ",PROCESSLINE";
                    btnProcessLinePPID.Tag = FormMainMDI.G_OPIAp.CurLine.ServerName + ",";
                    btnSTBLinePPID.Tag = FormMainMDI.G_OPIAp.CurLine.ServerName + ",STB";

                    #region CUT500
                    if (FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_3")
                    {
                        pnlSTBLinePPID.Visible = true;

                        pnlSTBCassetteSettingCode.Visible = true;

                        //grbLocalLine.Size = new Size(884, 250);
                    }
                    else
                    {
                        pnlSTBLinePPID.Visible = false;

                        pnlSTBCassetteSettingCode.Visible = false;

                        //grbLocalLine.Size = new Size(884, 190);
                    }
                    #endregion
                }
                if (_num >= 10 && _num <= 15) //L10-L15
                {
                    pnlProcessLinePPID.Visible = false;
                    pnlProcCassetteSettingCode.Visible = false;

                    pnlSTBLinePPID.Visible = false;
                    pnlSTBCassetteSettingCode.Visible = false;

                    txtLotLinePPID.Tag = "STB";
                    btnLotLinePPID.Tag = FormMainMDI.G_OPIAp.CurLine.ServerName + ",STB";
                    btnProcessLinePPID.Tag = FormMainMDI.G_OPIAp.CurLine.ServerName + ",";
                    btnSTBLinePPID.Tag = FormMainMDI.G_OPIAp.CurLine.ServerName + ",";
                }

                #endregion

                this.Size = new Size(900, 400);
                #endregion

                #region 跨line
                //if (CrossLineType != string.Empty && CurPort.NodeNo == "L2")
                //{
                //    #region 跨line畫面設定
                //    grbCrossLine.Visible = true;

                //    btnProcessLinePPID_Cross.Tag = CrossServerName + ",PROCESSLINE";

                //    txtCrossLinePPID.Text = string.Empty;
                    
                //    _ppid = string.Empty;

                //    foreach (Node _node in FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.Values)
                //    {
                //        _ppid = _ppid + (_ppid == string.Empty ? "" : ";") + _node.NodeNo + ":" + _node.DefaultRecipeNo;
                //    }
                    
                //    txtCrossLinePPID.Tag = _ppid;

                //    btnProcessLinePPID_Cross.Tag = CrossServerName + ",PROCESSLINE";
                //    btnSTBLinePPID_Cross.Tag = CrossServerName + ",STB";

                //    //CUT400 -> CUT500
                //    if (FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_2")
                //    {
                //        pnlSTBLinePPID_Cross.Visible = true;
                //        pnlCrossSTBCassetteSettingCode.Visible = true;

                //        grbCrossLine.Size = new Size(884, 190);

                //        this.Size = new Size(900, 560);
                //    }
                //    else
                //    {
                //        pnlSTBLinePPID_Cross.Visible = false;
                //        pnlCrossSTBCassetteSettingCode.Visible = false;

                //        grbCrossLine.Size = new Size(884, 130);

                //        this.Size = new Size(900, 520);
                //    }
                //    #endregion

                //    #region 取得跨Line資訊 --- CBCUT400跨500, CBCUT100 / CBCUT200 Recipe (CBCUT100 & CBCUT200互跨) 資訊
                //    Recipe _new;
                //    dicRecipe_Cross = new Dictionary<string, Recipe>();

                //    var _var = (from recipe in FormMainMDI.G_OPIAp.DBCtx.SBRM_RECIPE_CROSS
                //                where recipe.LINETYPE == CrossLineType &&
                //                      recipe.ONLINECONTROLSTATE == FormMainMDI.G_OPIAp.CurLine.MesControlMode
                //                select new { RecipeName = recipe.LINERECIPENAME, PPID = recipe.PPID, RecipeType = recipe.RECIPETYPE });

                //    foreach (var _recipe in _var)
                //    {
                //        _new = new Recipe(CrossServerName, _recipe.RecipeName, _recipe.PPID, _recipe.RecipeType);
                //        dicRecipe_Cross.Add(_recipe.RecipeName, _new);
                //    }
                //    #endregion
                //}
                //else
                //{
                //    grbCrossLine.Visible = false;

                //    txtCrossLinePPID.Tag = string.Empty;
                //}
                #endregion

                #region 顯示資料 
                if (CurrentLinePPID != string.Empty)
                {
                    if (pnlCurrentLinePPID.Visible ) txtCurrentLinePPID.Text = CurrentLinePPID;
                    if (pnlLotLinePPID.Visible ) txtLotLinePPID.Text =Recipe_Lot_Cutting == null? string.Empty : Recipe_Lot_Cutting.PPID;
                    if (pnlProcessLinePPID.Visible) txtProcessLinePPID.Text = Recipe_Proc_Cutting == null ? string.Empty : Recipe_Proc_Cutting.PPID;
                    if (pnlSTBLinePPID.Visible ) txtSTBLinePPID.Text = Recipe_STB_Cutting == null ? string.Empty : Recipe_STB_Cutting.PPID;

                    #region cassette setting code / product id / Product type
                    if (pnlLotCassetteSettingCode.Visible)
                    {
                        txtLotCassetteSettingCode.Text = LotCstSettingCode.Data;
                        txtLotCassetteSettingCode.ReadOnly = LotCstSettingCode.ReadOnly;

                        txtProductType.Text = LotProductType.Data;
                        txtProductType.ReadOnly = LotProductType.ReadOnly;

                        txtProductID.Text = LotProductID.Data;
                        txtProductID.ReadOnly = LotProductID.ReadOnly;
                    }

                    if (pnlProcCassetteSettingCode.Visible)
                    {
                        txtProcCassetteSettingCode.Text = ProcCstSettingCode.Data;
                        txtProcCassetteSettingCode.ReadOnly = ProcCstSettingCode.ReadOnly;

                        txtProcProductType.Text = ProcProductType.Data;
                        txtProcProductType.ReadOnly = ProcProductType.ReadOnly;

                        txtProcProductID.Text = ProcProductID.Data;
                        txtProcProductID.ReadOnly = ProcProductID.ReadOnly;
                    }

                    if (pnlSTBCassetteSettingCode.Visible)
                    {
                        txtSTBCassetteSettingCode.Text = STBCstSettingCode.Data;
                        txtSTBCassetteSettingCode.ReadOnly = STBCstSettingCode.ReadOnly;

                        txtSTBProductType.Text = STBProductType.Data;
                        txtSTBProductType.ReadOnly = STBProductType.ReadOnly;

                        txtSTBProductID.Text = STBProductID.Data;
                        txtSTBProductID.ReadOnly = STBProductID.ReadOnly;
                    }                                                           
                    #endregion

                }

                //if (CrossLinePPID !=string.Empty )
                //{
                //    if (pnlCrossLinePPID.Visible ) txtCrossLinePPID.Text = CrossLinePPID;
                //    if (pnlProcessLinePPID_Cross.Visible ) txtProcessLinePPID_Cross.Text = Recipe_CrossProc_Cutting == null ? string.Empty : Recipe_CrossProc_Cutting.PPID;
                //    if (pnlSTBLinePPID_Cross.Visible) txtSTBLinePPID_Cross.Text = Recipe_CrossSTB_Cutting == null ? string.Empty : Recipe_CrossSTB_Cutting.PPID;

                //    #region cassette setting code / product id / Product type
                //    if (pnlCrossProcCassetteSettingCode.Visible)
                //    {
                //        txtCrossProcCassetteSettingCode.Text = CrossProcCstSettingCode.Data;
                //        txtCrossProcCassetteSettingCode.ReadOnly = CrossProcCstSettingCode.ReadOnly;

                //        txtCrossProcProductType.Text = CrossProcProductType.Data;
                //        txtCrossProcProductType.ReadOnly = CrossProcProductType.ReadOnly;

                //        txtCrossProcProductD.Text = CrossProcProductID.Data;
                //        txtCrossProcProductD.ReadOnly = CrossProcProductID.ReadOnly;
                //    }

                //    if (pnlCrossSTBCassetteSettingCode.Visible)
                //    {
                //        txtCrossSTBCassetteSettingCode.Text = CrossSTBCstSettingCode.Data;
                //        txtCrossSTBCassetteSettingCode.ReadOnly = CrossSTBCstSettingCode.ReadOnly;

                //        txtCrossSTBProductType.Text = CrossSTBProductType.Data;
                //        txtCrossSTBProductType.ReadOnly = CrossSTBProductType.ReadOnly;

                //        txtCrossSTBProductID.Text = CrossSTBProductID.Data;
                //        txtCrossSTBProductID.ReadOnly = CrossSTBProductID.ReadOnly;

                //    }
                //    #endregion
                //}
                #endregion
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
                Button _btn = (Button)sender;

                string[] _tag = _btn.Tag.ToString().Split(',');

                if (_tag[0] == FormMainMDI.G_OPIAp.CurLine.ServerName)
                {
                    if (dicRecipe_All == null) return;

                    #region Local Line
                    FormCassetteControl_RecipeChoose frm = new FormCassetteControl_RecipeChoose(_tag[1], dicRecipe_All);

                    if (System.Windows.Forms.DialogResult.OK == frm.ShowDialog())
                    {
                        if (_btn.Name == "btnLotLinePPID")
                        {
                            txtLotLinePPID.Text = dicRecipe_All[frm.RecipeName].PPID;

                            Recipe_Lot_Cutting = dicRecipe_All[frm.RecipeName];
                        }
                        else if (_btn.Name == "btnProcessLinePPID")
                        {
                            txtProcessLinePPID.Text = dicRecipe_All[frm.RecipeName].PPID;

                            Recipe_Proc_Cutting = dicRecipe_All[frm.RecipeName];
                        }
                        else if (_btn.Name == "btnSTBLinePPID")
                        {
                            txtSTBLinePPID.Text = dicRecipe_All[frm.RecipeName].PPID;

                            Recipe_STB_Cutting = dicRecipe_All[frm.RecipeName];
                        }
                    }
                    #endregion
                }
                else
                {
                    if (dicRecipe_Cross == null) return;

                    #region Cross Line
                    //FormCassetteControl_RecipeChoose frm = new FormCassetteControl_RecipeChoose(_tag[1], dicRecipe_Cross);

                    //if (System.Windows.Forms.DialogResult.OK == frm.ShowDialog())
                    //{
                    //    if (_btn.Name == "btnProcessLinePPID_Cross")
                    //    {
                    //        txtProcessLinePPID_Cross.Text = dicRecipe_Cross[frm.RecipeName].PPID;

                    //        Recipe_CrossProc_Cutting = dicRecipe_Cross[frm.RecipeName];
                    //        Recipe_CrossProc_Cutting.ServerName = _tag[0];
                    //    }
                    //    else if (_btn.Name == "btnSTBLinePPID_Cross")
                    //    {
                    //        txtSTBLinePPID_Cross.Text = dicRecipe_Cross[frm.RecipeName].PPID;

                    //        Recipe_CrossSTB_Cutting=dicRecipe_Cross[frm.RecipeName];
                    //        Recipe_CrossSTB_Cutting.ServerName = _tag[0];
                    //    }
                    //}
                    #endregion
                }                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtRecipe_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                if (_txt.Text.ToString() == string.Empty) return;

                string[] _data = _txt.Text.ToString().Split(';');
                string[] _currentPPID =  (txtCurrentLinePPID.Text.ToString() == string.Empty ? txtCurrentLinePPID.Tag.ToString() : txtCurrentLinePPID.Text.ToString()).Split(';');
                //string[] _crossPPID = (txtCrossLinePPID.Text.ToString() == string.Empty ? txtCrossLinePPID.Tag.ToString() : txtCrossLinePPID.Text.ToString()).Split(';'); 

                switch (_txt.Tag.ToString())
                {
                    case "LOT":

                        #region 取 L2 & L3 
                        
                        //CBCUT100~300	L2,L3,0000(補滿至L8)
                        //CBCUT400	L2,L3,0000(補滿至L8)
                        //CBCUT500	L2,L3,0000(補滿至L15)
                        for (int i = 0; i < _data.Length; i++)
                        {
                            if (_data[i].Contains("L2")) { if (_currentPPID.Length  > i) _currentPPID[0] = _data[i]; continue; }
                            if (_data[i].Contains("L3")) { if (_currentPPID.Length  > i) _currentPPID[1] = _data[i]; continue; }
                        }

                        #region L2 & L3 若沒有Recipe No,則給預設值 00
       
                        for (int i = 0; i < _currentPPID.Length ; i++)
                        {
                            if (_currentPPID[i].Trim() != string.Empty) continue;

                            Node _node = null;

                            switch (i)
                            {
                                case 0:  //L2
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L2"))  _node = FormMainMDI.G_OPIAp.Dic_Node["L2"];
                                    break ;
                                case 1 : //L3
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L3")) _node = FormMainMDI.G_OPIAp.Dic_Node["L3"];
                                    break ;
                                default :
                                    break ;
                            }

                            if (_node != null) _currentPPID[i] = _node.NodeNo + ":" + _node.DefaultRecipeNo; //string.Format("{0}:{1}", _node.NODENO, "0".PadLeft(_node.RecipeLen, '0')); 
                        }
                        #endregion

                        txtCurrentLinePPID.Text = string.Join(";", _currentPPID);
                        #endregion

                        break;

                    case "PROCESSLINE":

                        #region 取 L4 ~ L9
                        //CBCUT100~300	0000,L4~L8
                        //CBCUT400	    0000,L4~L8
                        //CBCUT500	    0000,L4~L10,0000(補滿至L15)                        
                        
                        for (int i = 0; i < _data.Length; i++)
                        {
                            if (_data[i].Contains("L4")) { if (_currentPPID.Length  > i) _currentPPID[2] = _data[i]; continue ; }
                            if (_data[i].Contains("L5")) { if (_currentPPID.Length  > i) _currentPPID[3] = _data[i]; continue; }
                            if (_data[i].Contains("L6")) { if (_currentPPID.Length  > i) _currentPPID[4] = _data[i]; continue; }
                            if (_data[i].Contains("L7")) { if (_currentPPID.Length  > i) _currentPPID[5] = _data[i]; continue; }
                            if (_data[i].Contains("L8")) { if (_currentPPID.Length  > i) _currentPPID[6] = _data[i]; continue; }
                            if (_data[i].Contains("L9")) { if (_currentPPID.Length  > i) _currentPPID[7] = _data[i]; continue; }
                            //if (_data[i].Contains("L10")) { if (_currentPPID.Length  > i) _currentPPID[8] = _data[i]; continue; }
                        }

                        #region L4 ~ L9 若沒有Recipe No,則給預設值 00
                 
                        for (int i = 2; i < _currentPPID.Length; i++)
                        {
                            if (_currentPPID[i].Trim() != string.Empty) continue;

                            Node _node = null;

                            switch (i)
                            {
                                case 2:  //L4
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L4")) _node = FormMainMDI.G_OPIAp.Dic_Node["L4"];
                                    break;
                                case 3: //L5
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L5")) _node = FormMainMDI.G_OPIAp.Dic_Node["L5"];
                                    break;
                                case 4:  //L6
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L6")) _node = FormMainMDI.G_OPIAp.Dic_Node["L6"];
                                    break;
                                case 5: //L7
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L7")) _node = FormMainMDI.G_OPIAp.Dic_Node["L7"];
                                    break;
                                case 6:  //L8
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L8")) _node = FormMainMDI.G_OPIAp.Dic_Node["L8"];
                                    break;
                                case 7:  //L9
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L9")) _node = FormMainMDI.G_OPIAp.Dic_Node["L9"];
                                    break;
                                //case 8: //L10
                                //    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L10")) _node = FormMainMDI.G_OPIAp.Dic_Node["L10"];
                                //    break;
                                default:
                                    break;
                            }

                            if (_node != null) _currentPPID[i] = _node.NodeNo + ":" + _node.DefaultRecipeNo; // string.Format("{0}:{1}", _node.NODENO, "0".PadLeft(_node.RecipeLen, '0'));
                        }

                        #endregion

                        txtCurrentLinePPID.Text = string.Join(";", _currentPPID);
                        #endregion

                        break;

                    case "STB":

                        #region 取 L10 ~ L15 -- CBCUT_3
                        //CBCUT100~300	 -------------------
                        //CBCUT400	     -------------------
                        //CBCUT500	     0000(補滿至L10),L11~L15

                        for (int i = 0; i < _data.Length ; i++)
                        {
                            if (_data[i].Contains("L10")) { if (_currentPPID.Length > i) _currentPPID[8] = _data[i]; continue; }
                            if (_data[i].Contains("L11")) { if (_currentPPID.Length  > i) _currentPPID[9] = _data[i]; continue; }
                            if (_data[i].Contains("L12")) { if (_currentPPID.Length  > i) _currentPPID[10] = _data[i]; continue; }
                            if (_data[i].Contains("L13")) { if (_currentPPID.Length  > i) _currentPPID[11] = _data[i]; continue; }
                            if (_data[i].Contains("L14")) { if (_currentPPID.Length  > i) _currentPPID[12] = _data[i]; continue; }
                            if (_data[i].Contains("L15")) { if (_currentPPID.Length  > i) _currentPPID[13] = _data[i]; continue; }
                        }

                        #region L10 ~ L15 若沒有Recipe No,則給預設值 00
                        for (int i = 8; i < _currentPPID.Length; i++)
                        {
                            if (_currentPPID[i].Trim() != string.Empty) continue;

                            Node _node = null;

                            switch (i)
                            {
                                case 8:  //L10
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L10")) _node = FormMainMDI.G_OPIAp.Dic_Node["L10"];
                                    break;
                                case 9:  //L11
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L11")) _node = FormMainMDI.G_OPIAp.Dic_Node["L11"];
                                    break;
                                case 10: //L12
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L12")) _node = FormMainMDI.G_OPIAp.Dic_Node["L12"];
                                    break;
                                case 11:  //L13
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L13")) _node = FormMainMDI.G_OPIAp.Dic_Node["L13"];
                                    break;
                                case 12: //L14
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L14")) _node = FormMainMDI.G_OPIAp.Dic_Node["L14"];
                                    break;
                                case 13:  //L15
                                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey("L15")) _node = FormMainMDI.G_OPIAp.Dic_Node["L15"];
                                    break;
                                default:
                                    break;
                            }

                            if (_node != null) _currentPPID[i] = _node.NodeNo + ":" + _node.DefaultRecipeNo; //string.Format("{0}:{1}", _node.NODENO, "0".PadLeft(_node.RecipeLen, '0'));
                        }
                        #endregion

                        txtCurrentLinePPID.Text = string.Join(";", _currentPPID);
                           
                        #endregion

                        break;

                    //case "PROCESSLINE_CROSS":

                        //#region 跨line  L2 ~ L9

                        //for (int i = 0; i < _data.Length; i++)
                        //{
                        //    if (_data[i].Contains("L2")) { if (_crossPPID.Length  > i) _crossPPID[0] = _data[i]; continue; }
                        //    if (_data[i].Contains("L3")) { if (_crossPPID.Length  > i) _crossPPID[1] = _data[i]; continue; }
                        //    if (_data[i].Contains("L4")) { if (_crossPPID.Length  > i) _crossPPID[2] = _data[i]; continue; }
                        //    if (_data[i].Contains("L5")) { if (_crossPPID.Length  > i) _crossPPID[3] = _data[i]; continue; }
                        //    if (_data[i].Contains("L6")) { if (_crossPPID.Length  > i) _crossPPID[4] = _data[i]; continue; }
                        //    if (_data[i].Contains("L7")) { if (_crossPPID.Length  > i) _crossPPID[5] = _data[i]; continue; }
                        //    if (_data[i].Contains("L8")) { if (_crossPPID.Length  > i) _crossPPID[6] = _data[i]; continue; }
                        //    if (_data[i].Contains("L9")) { if (_crossPPID.Length  > i) _crossPPID[7] = _data[i]; continue; }
                        //    //if (_data[i].Contains("L10")) { if (_crossPPID.Length  > i) _crossPPID[8] = _data[i]; continue; }
                        //}

                        //#region L2 ~ L10 若沒有Recipe No,則給預設值 00

                        //for (int i = 0; i < _crossPPID.Length; i++)
                        //{
                        //    if (_crossPPID[i].Trim() != string.Empty) continue;

                        //    Node _node = null;

                        //    switch (i)
                        //    {
                        //        case 0:  //L2
                        //            if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L2")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L2"];
                        //            break;
                        //        case 1: //L3
                        //            if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L3")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L3"];
                        //            break;
                        //        case 2:  //L4
                        //            if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L4")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L4"];
                        //            break;
                        //        case 3: //L5
                        //            if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L5")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L5"];
                        //            break;
                        //        case 4:  //L6
                        //            if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L6")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L6"];
                        //            break;
                        //        case 5: //L7
                        //            if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L7")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L7"];
                        //            break;
                        //        case 6:  //L8
                        //            if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L8")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L8"];
                        //            break;
                        //        case 7: //L9
                        //            if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L9")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L9"];
                        //            break;
                        //        //case 8:  //L10
                        //        //    if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L10")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L10"];
                        //        //    break;
                        //        default:
                        //            break;
                        //    }

                        //    if (_node != null) _crossPPID[i] = _node.NodeNo + ":" + _node.DefaultRecipeNo; //string.Format("{0}:{1}", _node.NODENO, "0".PadLeft(_node.RecipeLen, '0'));
                        //}
                        //#endregion

                        //txtCrossLinePPID.Text = string.Join(";", _crossPPID);

                        //#endregion

                        //break;

                    //case "STB_CROSS":

                    //    #region CBCUT400 跨line CBCUT500 L10 ~ L15

                    //    for (int i = 0; i < _data.Length; i++)
                    //    {
                    //        if (_data[i].Contains("L10")) { if (_crossPPID.Length > i) _crossPPID[8] = _data[i]; continue; }
                    //        if (_data[i].Contains("L11")) { if (_crossPPID.Length  > i) _crossPPID[9] = _data[i]; continue; }
                    //        if (_data[i].Contains("L12")) { if (_crossPPID.Length  > i) _crossPPID[10] = _data[i]; continue; }
                    //        if (_data[i].Contains("L13")) { if (_crossPPID.Length  > i) _crossPPID[11] = _data[i]; continue; }
                    //        if (_data[i].Contains("L14")) { if (_crossPPID.Length  > i) _crossPPID[12] = _data[i]; continue; }
                    //        if (_data[i].Contains("L15")) { if (_crossPPID.Length  > i) _crossPPID[13] = _data[i]; continue; }
                    //    }

                    //    #region L10 ~ L15 若沒有Recipe No,則給預設值 00

                    //    for (int i = 8; i < _crossPPID.Length ; i++)
                    //    {
                    //        if (_crossPPID[i].Trim() != string.Empty) continue;

                    //        Node _node = null;

                    //        switch (i)
                    //        {
                    //            case 8:  //L10
                    //                if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L10")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L10"];
                    //                break;
                    //            case 9:  //L11
                    //                if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L11")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L11"];
                    //                break;
                    //            case 10: //L12
                    //                if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L12")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L12"];
                    //                break;
                    //            case 11:  //L13
                    //                if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L13")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L13"];
                    //                break;
                    //            case 12: //L14
                    //                if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L14")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L14"];
                    //                break;
                    //            case 13:  //L15
                    //                if (FormMainMDI.G_OPIAp.Dic_CrossNode_CUT.ContainsKey("L15")) _node = FormMainMDI.G_OPIAp.Dic_CrossNode_CUT["L15"];
                    //                break;
                    //            default:
                    //                break;
                    //        }

                    //        if (_node != null) _crossPPID[i] = _node.NodeNo + ":" + _node.DefaultRecipeNo; //string.Format("{0}:{1}", _node.NODENO, "0".PadLeft(_node.RecipeLen, '0'));
                    //    }
                    //    #endregion

                    //    txtCrossLinePPID.Text = string.Join(";", _crossPPID);

                    //    #endregion

                    //    break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private bool CheckData()
        {
            try
            {
                #region Check Data

                if (txtCurrentLinePPID.Text.ToString().Replace(";", "").Trim() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose recipe id", MessageBoxIcon.Error);

                    return false;
                }

                if (txtLotLinePPID.Text.ToString().Replace(";", "").Trim() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Lot Recipe Name", MessageBoxIcon.Error);

                    return false;
                }

                if (txtLotCassetteSettingCode.Text.ToString() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please inpute Cassette Setting Code", MessageBoxIcon.Error);

                    txtLotCassetteSettingCode.Focus();

                    return false;
                }

                if (txtProductType.Text.ToString() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please inpute Product Type", MessageBoxIcon.Error);

                    txtProductType.Focus();

                    return false;
                }

                if (txtProductID.Text.ToString() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please inpute Product ID", MessageBoxIcon.Error);

                    txtProductID.Focus();

                    return false;
                }

                if (txtProcessLinePPID.Text.ToString().Replace(";", "").Trim() != string.Empty)
                {
                    if (txtProcCassetteSettingCode.Text.ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please inpute Process Cassette Setting Code", MessageBoxIcon.Error);

                        txtProcCassetteSettingCode.Focus();

                        return false;
                    }

                    if (txtProcProductType.Text.ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please inpute Process Product Type", MessageBoxIcon.Error);

                        txtProcProductType.Focus();

                        return false;
                    }

                    if (txtProcProductID.Text.ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please inpute Process Product ID", MessageBoxIcon.Error);

                        txtProcProductID.Focus();

                        return false;
                    }

                }

                if (txtSTBLinePPID.Text.ToString().Replace(";", "").Trim() != string.Empty)
                {
                    if (txtSTBCassetteSettingCode.Text.ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please inpute STB Cassette Setting Code", MessageBoxIcon.Error);

                        txtSTBCassetteSettingCode.Focus();

                        return false;
                    }

                    if (txtSTBProductType.Text.ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please inpute STB Product Type", MessageBoxIcon.Error);

                        txtSTBProductType.Focus();

                        return false;
                    }

                    if (txtSTBProductID.Text.ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please inpute STB Product ID", MessageBoxIcon.Error);

                        txtSTBProductID.Focus();

                        return false;
                    }
                }

                //if (txtProcessLinePPID_Cross.Text.ToString().Replace(";", "").Trim() != string.Empty)
                //{
                //    if (txtCrossProcCassetteSettingCode.Text.ToString() == string.Empty)
                //    {
                //        ShowMessage(this, lblCaption.Text, "", "Please inpute Cross Process Cassette Setting Code", MessageBoxIcon.Error);

                //        txtCrossProcCassetteSettingCode.Focus();

                //        return false ;
                //    }

                //    if (txtCrossProcProductType.Text.ToString() == string.Empty)
                //    {
                //        ShowMessage(this, lblCaption.Text, "", "Please inpute Cross Process Product Type", MessageBoxIcon.Error);

                //        txtCrossProcProductType.Focus();

                //        return false;
                //    }

                //    if (txtCrossProcProductD.Text.ToString() == string.Empty)
                //    {
                //        ShowMessage(this, lblCaption.Text, "", "Please inpute Cross Process Product ID", MessageBoxIcon.Error);

                //        txtCrossProcProductD.Focus();

                //        return false;
                //    }
                //}

                //if (txtSTBLinePPID_Cross.Text.ToString().Replace(";", "").Trim() != string.Empty)
                //{
                //    if (txtCrossSTBCassetteSettingCode.Text.ToString() == string.Empty)
                //    {
                //        ShowMessage(this, lblCaption.Text, "", "Please inpute Cross Process Cassette Setting Code", MessageBoxIcon.Error);

                //        txtCrossSTBCassetteSettingCode.Focus();

                //        return false;
                //    }

                //    if (txtCrossSTBProductType.Text.ToString() == string.Empty)
                //    {
                //        ShowMessage(this, lblCaption.Text, "", "Please inpute STB Product Type", MessageBoxIcon.Error);

                //        txtCrossSTBProductType.Focus();

                //        return false;
                //    }

                //    if (txtCrossSTBProductID.Text.ToString() == string.Empty)
                //    {
                //        ShowMessage(this, lblCaption.Text, "", "Please inpute STB Product ID", MessageBoxIcon.Error);

                //        txtCrossSTBProductID.Focus();

                //        return false;
                //    }
                //}

                #region STB有填值，則Process line也一定要填值
                if (pnlSTBLinePPID.Visible == true)
                {
                    if (txtSTBLinePPID.Text.ToString().Replace(";", "").Trim() != string.Empty)
                    {
                        if (pnlProcessLinePPID.Visible)
                        {
                            if (txtProcessLinePPID.Text.ToString().Replace(";", "").Trim() == string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "", "Please choose Process Line Recipe Name", MessageBoxIcon.Error);

                                return false;
                            }
                        }
                    }
                }

                //if (pnlSTBLinePPID_Cross.Visible == true)
                //{
                //    if (txtSTBLinePPID_Cross.Text.ToString().Replace(";", "").Trim() != string.Empty)
                //    {
                //        if (txtProcessLinePPID_Cross.Text.ToString().Replace(";", "").Trim() == string.Empty)
                //        {
                //            ShowMessage(this, lblCaption.Text, "", "Please choose Cross Process Line Recipe Name", MessageBoxIcon.Error);

                //            return false;
                //        }
                //    }
                //}
                #endregion

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

        private void Button_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender;

                switch (_btn.Text.ToUpper())
                {
                    case "OK":

                        if (CheckData() == false) return;

                        CurrentLinePPID = txtCurrentLinePPID.Text.ToString();
                        //CrossLinePPID = txtCrossLinePPID.Text.ToString(); 
            
                        LotCstSettingCode.Data = txtLotCassetteSettingCode.Text.ToString();
                        ProcCstSettingCode.Data = txtProcCassetteSettingCode.Text.ToString ();
                        STBCstSettingCode.Data  = txtSTBCassetteSettingCode.Text.ToString ();
                        //CrossProcCstSettingCode.Data  = txtCrossProcCassetteSettingCode.Text.ToString ();
                        //CrossSTBCstSettingCode.Data  = txtCrossSTBCassetteSettingCode.Text.ToString ();

                        LotProductType.Data  = txtProductType.Text.ToString();
                        ProcProductType.Data = txtProcProductType.Text.ToString();
                        STBProductType.Data = txtSTBProductType.Text .ToString();
                        //CrossProcProductType.Data = txtCrossProcProductType.Text.ToString();
                        //CrossSTBProductType.Data = txtCrossSTBProductType.Text.ToString();

                        LotProductID.Data =txtProductID.Text.ToString();                        
                        ProcProductID.Data = txtProcProductID.Text.ToString();
                        STBProductID.Data = txtSTBProductID.Text.ToString();
                        //CrossProcProductID.Data =txtCrossProcProductD.Text.ToString();
                        //CrossSTBProductID.Data = txtCrossSTBProductID.Text.ToString();


                        this.DialogResult = System.Windows.Forms.DialogResult.OK;

                        break;

                    case "CANCEL":

                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                        break;

                    default:
                        break;
                }

                this.Close();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtProductType_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                if (MesControlMode.ToUpper() == "LOCAL") return;

                if (_txt.Text == string.Empty) return;

                int _num = 0;

                int.TryParse(_txt.Text.ToString(), out _num);

                if (_num > 100 || _num < 0)
                {
                    ShowMessage(this, lblCaption.Text, "","0 <= Product Type <= 100", MessageBoxIcon.Error);

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

        private void txtProductID_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                if (_txt.Text == string.Empty) return;

                int _num = 0;

                int.TryParse(_txt.Text.ToString(), out _num);

                if (_num > 65535 || _num < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "0 <= Product ID <= 65535", MessageBoxIcon.Error);

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

    }

    public class CUT_CSTSettingCodeData
    {
        public string Data { get; set; }
        public bool ReadOnly { get; set; }

        public CUT_CSTSettingCodeData()
        {
            Data = string.Empty;
            ReadOnly = false;
        }
    }
}
