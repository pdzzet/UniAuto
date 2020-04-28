using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;
using UNILAYOUT;
using System.Data.SqlClient;
using System.Data;

namespace UniOPI
{    
    public partial class FormLayout : FormBase
    {
        Port CurPort;
        Dense CurDense;
        Pallet CurPallet;
        Node CurNode;
        Unit CurUnit;
        VCR CurVCR;
        ToolTip Tip;

        OPIInfo OPIAp;
        int Count = 0;//add by hujunpeng 20190723

        Dictionary<string, csLabel> Dic_SECSLabel = new Dictionary<string, csLabel>();  //key: NodeNo

        public delegate void AddLogHandler(string Msg);

        private List<string> hideContrlNames;//記錄已隱藏的控制項

        public FormLayout(OPIInfo apInfo)
        {
            try
            {
                InitializeComponent();

                OPIAp = apInfo;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void FormLayout_Load(object sender, EventArgs e)
        {
            try
            {
                string strLayoutPath = string.Format("{0}{1}\\{2}.xml", OPIConst.LayoutFolder, OPIAp.CurLine.FabType,OPIAp.CurLine.ServerName);  //OPIConst.LayoutFolder + OPIAp.CurLine.FabType + string.Format(OPIAp.CurLine.ServerName + ".xml");
                if (File.Exists(strLayoutPath))
                {
                    LoadXMLFile LayoutDesign = new LoadXMLFile(strLayoutPath);
                    LayoutDesign.Create(pnlLayout);
                }

                #region 左側顯示區域設定
                flpLine.Location = new Point(0, 0);
                flpEQP.Location = new Point(0, 0);
                flpPort.Location = new Point(0, 0);
                flpUnit.Location = new Point(0, 0);
                flpVCR.Location = new Point(0, 0);
                flpDense.Location = new Point(0, 0);
                flpPallet.Location = new Point(0, 0);

                flpLine.Height = 600;
                flpEQP.Height = 600;
                flpPort.Height = 625;
                flpUnit.Height = 600;
                flpVCR.Height = 600;
                flpDense.Height = 600;
                flpPallet.Height = 600;

                if (OPIAp.CurLine.IndexerNode == null)
                {
                    lblIndexerMode.Visible = false;
                    txtIndexerMode.Visible = false;
                }
                else
                {
                    lblIndexerMode.Visible = true;
                    txtIndexerMode.Visible = true;
                }

                #region Set Line info for 左側區塊
                txtServerName.Text = OPIAp.CurLine.ServerName;
                txtLineID.Text = OPIAp.CurLine.LineID;
                txtLineID2.Text = OPIAp.CurLine.LineID2;
                txtLineType.Text = OPIAp.CurLine.LineType;
                txtFactoryType.Text = OPIAp.CurLine.FabType;

                #region 判斷是否有兩條lineid
                if (OPIAp.CurLine.LineID2 == string.Empty)
                {
                    txtLineID2.Visible = false;
                    lblLineID2.Visible = false;
                }
                else
                {
                    txtLineID2.Visible = true;
                    lblLineID2.Visible = true; 
                }
                #endregion

                #endregion

                #endregion           

                #region 判斷是否有RB 處理

                dgvJobData.Columns[colRoute.Name].Visible = OPIAp.Dic_Robot.Count > 0 ? true : false;
                dgvJobData.Columns[colStopReason.Name].Visible = OPIAp.Dic_Robot.Count > 0 ? true : false;

                #endregion

                #region Query群組不可使用下貨功能
                if (FormMainMDI.G_OPIAp.LoginGroupID == "Query")
                {
                     grbCassetteCmd.Visible=false ;
                     btnStart.Visible = false;
                }
                #endregion
                //#region CBUAM line glass id改顯示 mask id
                //if (FormMainMDI.G_OPIAp.CurLine.ServerName == "CBUAM100")
                //    dgvJobData.Columns[colGlassID.Name].HeaderText = "Mask ID";     
                //else
                //    dgvJobData.Columns[colGlassID.Name].HeaderText = "Glass ID";               
                //#endregion

                #region job Data - Array 不顯示job judge / CF除Sorter Line 外不顯示Job Grade

                if (OPIAp.CurLine.FabType.Equals("ARRAY"))
                {
                    colJobJudge.Visible = false;
                }
                else if (OPIAp.CurLine.FabType.Equals("CF"))
                {
                    if (OPIAp.CurLine.LineType != "FCSRT_TYPE1")
                        colJobGrade.Visible = false;
                }
                progressBar1.Visible = false;
                progressBar2.Visible = false;
                progressBar3.Visible = false;
                progressBar4.Visible = false;
                progressBar5.Visible = false;
                progressBar6.Visible = false;
                progressBar7.Visible = false;
                progressBar8.Visible = false;
                progressBar9.Visible = false;
                progressBar10.Visible = false;
                label10.Visible = false;
                label11.Visible = false;
                label12.Visible = false;
                label13.Visible = false;
                label14.Visible = false;
                label15.Visible = false;
                label16.Visible = false;
                label17.Visible = false;
                label8.Visible = false;
                label9.Visible = false;
                label18.Visible = false;
                label19.Visible = false;
                Reqtimer.Enabled = false;
                treeViewPIJobCount.Visible = false;
                //rectangleShape1.Visible = false;
                if ((txtLineID.Text == "FCBPH100") || (txtLineID.Text == "FCGPH100") || (txtLineID.Text == "FCMPH100") || (txtLineID.Text == "FCOPH100") || (txtLineID.Text == "FCRPH100") || (txtLineID.Text == "FCSPH100"))
                {
                    progressBar1.Visible = true;
                    progressBar2.Visible = true;
                    label8.Visible = true;
                    label9.Visible = true;
                }
                if ((txtLineID.Text == "CCPIL100") || (txtLineID.Text == "CCPIL200"))
                {
                    progressBar3.Visible = true;
                    progressBar4.Visible = true;
                    progressBar5.Visible = true;
                    progressBar6.Visible = true;
                    progressBar7.Visible = true;
                    progressBar8.Visible = true;
                    progressBar9.Visible = true;
                    progressBar10.Visible = true;
                    label10.Visible = true;
                    label11.Visible = true;
                    label12.Visible = true;
                    label13.Visible = true;
                    label14.Visible = true;
                    label15.Visible = true;
                    label16.Visible = true;
                    label17.Visible = true;
                    treeViewPIJobCount.Visible = true;
                    //rectangleShape1.Visible = true;
                }
                #endregion

                Tip = new ToolTip();

                InitialLayout();
                DataSetting_Line();
                SetPanel_Visible(flpLine);

                bgwRefresh.RunWorkerAsync();

                hideContrlNames = new List<string>();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void InitialLayout()
        {
            ToolTip tip = new ToolTip();

            try
            {
                #region Init csLabel
                string _lblName = string.Empty;
                string _lblNo = string.Empty;
                foreach (csLabel label in pnlLayout.Controls.OfType<csLabel>())
                {
                    if (label.Name.Length < 6) continue;

                    label.Tag = string.Empty;
                    label.BackColor = Color.Silver;

                    _lblName = label.Name.Substring(0, 2);
                    _lblNo = label.Name.Substring(2, 2);
                    switch (_lblName)
                    {
                        case "SC": //SECS Staus - 當SECS Disconnect時顯示通知

                            label.Visible = false;
                            Dic_SECSLabel.Add(_lblNo, label);

                            break;

                        default:
                            break;
                    }
                }
                #endregion

                #region Init Pipe
                foreach (ucPipe pipe in pnlLayout.Controls.OfType<ucPipe>())
                {
                    pipe.Tag = string.Empty;
                    pipe.pictureBox1.Click += new EventHandler(Pipe_Click);
                }
                #endregion

                #region Init Node
                int _num = 0;
                string _localNo = string.Empty ;

                foreach (ucEQ eq in pnlLayout.Controls.OfType<ucEQ>())
                {        
                    #region 取得local no
                    int.TryParse(eq.Name.Substring(1,2),out _num);

                    _localNo = string.Format("L{0}",_num.ToString()); 

                    #endregion

                    #region csPictureBox
                    List<csPictureBox > _hasParentPic = new List<csPictureBox>();
                    foreach (csPictureBox _pic in eq.Controls.OfType<csPictureBox>())
                    {
                        _pic.BackColor = _pic.BackColor = Color.DarkGray; 

                        _pic.Tag = _localNo;

                        if (_pic.Name.Length < 4) continue;

                        string _type = _pic.Name.Substring(0, 2).ToUpper();

                        int _seqNo = 0;

                        int.TryParse(_pic.Name.Substring(2, 2), out _seqNo);

                        if ( (string)_pic.PropertyData.GetPropertyVale("ParentName") != string.Empty ) _hasParentPic.Add(_pic);

                        switch (_type)
                        {
                            #region VCR
                            case "VR": //VCR
                                _pic.BackgroundImage = Properties.Resources.Layout_VCROff;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(VCR_Click);
                                break;
                            #endregion

                            #region Robot 非unit 純圖片顯示
                            case "RD": //Robot- Double Arm
                                _pic.BackgroundImage = Properties.Resources.Layout_DoubleArm;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(EQ_Click);
                                break;

                            case "RS": //Robot- single Arm
                                _pic.BackgroundImage = Properties.Resources.Layout_SingleArm;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(EQ_Click);
                                break;
                            #endregion

                            #region  Stage 非unit 純圖片顯示
                            case "TF": //Stage Turn Fix
                                _pic.BackgroundImage = Properties.Resources.Layout_StageFixed;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(EQ_Click);
                                break;

                            case "TT": //Stage Turn Table
                                _pic.BackgroundImage = Properties.Resources.Layout_StageTurn_OFF;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(EQ_Click);
                                break;

                            case "TO": //Stage Turn Over
                                _pic.BackgroundImage = Properties.Resources.Layout_StageTurnOver_OFF;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(EQ_Click);
                                break;
                            #endregion

                            #region  Conveyor
                            case "CV": //Conveyor  橫向
                                _pic.BackgroundImage = Properties.Resources.Layout_Conveyor1;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(EQ_Click);
                                break;

                            case "CB": //Conveyor  橫向加長
                                _pic.BackgroundImage = Properties.Resources.Layout_Conveyor1_1;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(EQ_Click);
                                break;

                            case "CL": //Conveyor  縱向
                                _pic.BackgroundImage = Properties.Resources.Layout_Conveyor2;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(EQ_Click);
                                break;

                            case "CM": //Conveyor  縱向加長
                                _pic.BackgroundImage = Properties.Resources.Layout_Conveyor2_1;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(EQ_Click);
                                break;
                            #endregion

                            #region PPK&QPP Dense / Pallet
                            case "PK": //PPK Dense Port
                                _pic.BackgroundImage = Properties.Resources.Dense_NONE;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(Dense_Click);
                                break;

                            case "TL": //台車 Trolley
                                _pic.BackgroundImage = Properties.Resources.Layout_Trolley;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(EQ_Click);
                                break;

                            case "TR":
                                _pic.BackgroundImage = Properties.Resources.Layout_Trolley_Track;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(EQ_Click);
                                break;

                            case "PL": //棧板 Pallet 
                                _pic.BackgroundImage = Properties.Resources.Layout_Pallet;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(Pallet_Click);
                                break;

                            #endregion

                            #region Port
                            case "CN": // Normal Cassette 
                                _pic.BackgroundImage = Properties.Resources.CSTNormal_NONE;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(Port_Click);
                                break;

                            case "CS": //Scrap
                            case "CC": // Cell Cassette
                                _pic.BackgroundImage = Properties.Resources.CSTCell_NONE;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(Port_Click);
                                break;

                            case "CW": // Wire Cassette
                                _pic.BackgroundImage = Properties.Resources.CSTWire_NONE;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(Port_Click);
                                break;

                            case "DS":  //Dense Box

                                _pic.BackgroundImage = Properties.Resources.Dense_NONE;
                                _pic.BorderStyle = BorderStyle.None;

                                if (_seqNo < 50)
                                {
                                    _pic.Click += new EventHandler(Port_Click);
                                }
                                else
                                {
                                    _pic.Click += new EventHandler(EQ_Click);
                                }
                                break;

                            case "BF": //Buffer有下貨功能
                            case "BW": //Wire Buffer 有下貨功能
                                _pic.BackgroundImage = Properties.Resources.CSTBuffer_NONE;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(Port_Click);
                                break;

                            case "MP": //M Port
                                _pic.BackgroundImage = Properties.Resources.CSTMPortr_NONE;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(Port_Click);
                                break;

                            case "TP": //Tray Port
                                _pic.BackgroundImage = Properties.Resources.CSTTray_NONE;
                                _pic.BorderStyle = BorderStyle.None;
                                _pic.Click += new EventHandler(Port_Click);
                                break;

                            #endregion

                            #region  Unit
                            case "UN":  //for unit內的物件背景顯示用 EX:UN02:RD01 Robot顏色會與unit 02相同 

                                if (_pic.Name.Length == 9)
                                {
                                    string[] _data = _pic.Name.Split(':');

                                    switch (_data[1].Substring(0, 2))
                                    {
                                        #region Robot
                                        case "RD": //Robot- Double Arm
                                            _pic.BackgroundImage = Properties.Resources.Layout_DoubleArm;
                                            _pic.BorderStyle = BorderStyle.None;
                                            _pic.Click += new EventHandler(Unit_Click);
                                            break;

                                        case "RS": //Robot- single Arm
                                            _pic.BackgroundImage = Properties.Resources.Layout_SingleArm;
                                            _pic.BorderStyle = BorderStyle.None;
                                            _pic.Click += new EventHandler(Unit_Click);
                                            break;
                                        #endregion

                                        #region  Stage

                                        case "TF": //Stage Turn Fix
                                            _pic.BackgroundImage = Properties.Resources.Layout_StageFixed;
                                            _pic.BorderStyle = BorderStyle.None;
                                            _pic.Click += new EventHandler(Unit_Click);
                                            break;

                                        case "TT": //Stage Turn Table
                                            _pic.BackgroundImage = Properties.Resources.Layout_StageTurn_OFF;
                                            _pic.BorderStyle = BorderStyle.None;
                                            _pic.Click += new EventHandler(Unit_Click);
                                            break;

                                        case "TO": //Stage Turn Over
                                            _pic.BackgroundImage = Properties.Resources.Layout_StageTurnOver_OFF;
                                            _pic.BorderStyle = BorderStyle.None;
                                            _pic.Click += new EventHandler(Unit_Click);
                                            break;

                                        #endregion

                                        #region  Conveyor
                                        case "CV": //Conveyor  橫向
                                            _pic.BackgroundImage = Properties.Resources.Layout_Conveyor1;
                                            _pic.BorderStyle = BorderStyle.None;
                                            _pic.Click += new EventHandler(Unit_Click);

                                            break;

                                        case "CB": //Conveyor  橫向加長
                                            _pic.BackgroundImage = Properties.Resources.Layout_Conveyor1_1;
                                            _pic.BorderStyle = BorderStyle.None;
                                            _pic.Click += new EventHandler(Unit_Click);

                                            break;

                                        case "CL": //Conveyor  縱向
                                            _pic.BackgroundImage = Properties.Resources.Layout_Conveyor2;
                                            _pic.BorderStyle = BorderStyle.None;
                                            _pic.Click += new EventHandler(Unit_Click);

                                            break;

                                        case "CM": //Conveyor  縱向加長
                                            _pic.BackgroundImage = Properties.Resources.Layout_Conveyor2_1;
                                            _pic.BorderStyle = BorderStyle.None;
                                            _pic.Click += new EventHandler(Unit_Click);

                                            break;
                                        #endregion

                                        #region  台車 Trolley
                                        case "TL": //台車 Trolley
                                            _pic.BackgroundImage = Properties.Resources.Layout_Trolley;
                                            _pic.BorderStyle = BorderStyle.None;
                                            _pic.Click += new EventHandler(Unit_Click);
                                            break;
                                       #endregion

                                        #region  Buffer Unit
                                        case "BF": //Buffer Unit
                                            _pic.BackgroundImage = Properties.Resources.CSTBuffer_UC;
                                            _pic.BorderStyle = BorderStyle.None;
                                            _pic.Click += new EventHandler(Unit_Click);
                                            break;
                                        #endregion

                                        default :
                                            break;
                                    }
                                }
                                break;
                            #endregion

                            default:
                                break;
                        }
                    }
                    #endregion

                    #region csLabel
                    foreach (csLabel _lbl in eq.Controls.OfType<csLabel>())
                    {                       
                        _lbl.Tag = _localNo;

                        if (_lbl.Name.Length < 4) continue;

                        int _seqNo = 0;

                        int.TryParse(_lbl.Name.Substring(2, 2), out _seqNo);

                        switch (_lbl.Name.ToUpper())
                        {
                            case "NODENAME": 
                                _lbl.Click += new EventHandler(EQ_Click);
                                _lbl.Text  = _localNo ; 
                                _lbl.BackColor = Color.Red;
                                break;

                            case "TOTALCOUNT":
                                _lbl.BackColor = Color.LawnGreen;
                                _lbl.Text = "0";

                                break;

                            default :
                                _lbl.BackColor = Color.DarkGray ;


                                if (_lbl.Name.Substring(0, 2).ToUpper() == "UN")  //Unit Object
                                {
                                    if (_seqNo < 50)
                                    {
                                        _lbl.Click += new EventHandler(Unit_Click);
                                    }
                                    else
                                    {
                                        _lbl.Click += new EventHandler(EQ_Click);
                                    }

                                }
                                else if (_lbl.Name.Substring(0, 2).ToUpper() == "PK")  //Unit Object
                                {
                                    if (_seqNo < 50)
                                    {
                                        _lbl.Click += new EventHandler(Dense_Click);
                                    }
                                    else
                                    {
                                        _lbl.Click += new EventHandler(EQ_Click);
                                    }   
                                }
                                break;
                        }
                    }
                    #endregion

                    #region csShape
                    foreach (csShape _shp in eq.Controls.OfType<csShape>())
                    {
                        switch (_shp.Name.ToUpper())
                        {
                            case "EQSTATUS":
                                _shp.Click += new EventHandler(EQ_Click);
                                _shp.Tag = _localNo;
                                _shp.BackColor = Color.DarkGray;
                                break;


                            default:
                                break;
                        }
                    }
                    #endregion

                    #region Set Parent
                    foreach (csPictureBox _pic in _hasParentPic)
                    {                        
                        string _parent = (string)_pic.PropertyData.GetPropertyVale("ParentName");

                        if (_parent != string.Empty)
                        {
                            object _parentObj = eq.Controls.Find(_parent, true).First();
                            if (_parentObj != null)
                            {
                                switch (_parentObj.GetType().Name)
                                {
                                    case "csPictureBox":
                                        csPictureBox _parentPic = (csPictureBox)_parentObj;
                                        _pic.Parent = _parentPic;
                                        _pic.Location = new Point(_pic.Location.X - _parentPic.Location.X, _pic.Location.Y - _parentPic.Location.Y);
                                        _pic.BackColor = Color.Transparent;
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }

                    }

                    #endregion
                }
                #endregion

                SetPanel_Visible(flpLine);              
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void SetPanel_Visible(FlowLayoutPanel _pnl)
        {
            foreach (Control _ctrl in pnlStatusInfo.Controls)
            {
                if (_ctrl.GetType().Name == "FlowLayoutPanel")
                {
                    if (((FlowLayoutPanel)_ctrl).Name == _pnl.Name) _ctrl.Visible = true;
                    else _ctrl.Visible = false;
                }
            }
        }

        private void Pipe_Click(object sender, EventArgs e)
        {
            try
            {
                Control pipe = ((PictureBox)sender).Parent;

                var _disVar = OPIAp.Dic_Pipe.Values.Where(r => r.IsDisplay == true);

                //I020003000101
                if (pipe.Name.Length != 13)
                {
                    ShowMessage(this, lblCaption.Text, "", "Interface key is error, Please check Layout.xml", MessageBoxIcon.Error);
                    return;
                }

                if (OPIAp.Dic_Pipe.ContainsKey(pipe.Name))
                {
                    if (OPIAp.Dic_Pipe[pipe.Name].IsDisplay == true)
                    {
                        var _var = Application.OpenForms.Cast<Form>().Where(x => x.Name == pipe.Name).FirstOrDefault();

                        if (_var == null) return;

                        FormInterface _frm = (FormInterface)_var;

                        _frm.TopMost = true;

                        return;
                    }
                    else
                    {
                        if (_disVar.Count() < 2)
                        {
                            FormInterface _interface = new FormInterface(pipe.Name);

                            _interface.Name = pipe.Name;

                            _interface.Show();

                            _interface.TopMost = true;

                            OPIAp.Dic_Pipe[pipe.Name].IsDisplay = true;
                            OPIAp.Dic_Pipe[pipe.Name].IsReply = true;
                            return;
                        }
                        else
                        {
                            ShowMessage(this, lblCaption.Text , "", "Too Much Link Signal Form", MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
                else
                {
                    if (_disVar.Count() < 2)
                    {
                        #region New Pipe
                        Interface _if = new Interface();

                        _if.PipeKey = pipe.Name;
                        _if.UpstreamNodeNo = "L" + int.Parse(pipe.Name.Substring(1, 2));
                        _if.UpstreamUnitNo = pipe.Name.Substring(3, 2);
                        _if.DownstreamNodeNo = "L" + int.Parse(pipe.Name.Substring(5, 2));
                        _if.DownstreamUnitNo = pipe.Name.Substring(7, 2);

                        _if.UpstreamSeqNo = pipe.Name.Substring(9, 2);
                        _if.DownstreamSeqNo = pipe.Name.Substring(11, 2);

                        OPIAp.Dic_Pipe.Add(pipe.Name, _if);

                        FormInterface _interface = new FormInterface(pipe.Name);

                        _interface.Name = pipe.Name;

                        _interface.Show();

                        _interface.TopMost = true;
                        #endregion
                    }
                    else
                    {
                        ShowMessage(this, lblCaption.Text , "", "Too Much Link Signal Form", MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void pnlLayout_Click(object sender, EventArgs e)
        {
            try
            {
                #region Send to BC AllDataUpdateRequest
                //AllDataUpdateRequest _trx = new AllDataUpdateRequest();
                //_trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                //_trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineID;
                //string _xml = _trx.WriteToXml();

                //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                //SendtoBC_JobDataCategoryRequest(string.Empty);
                #endregion

                DataSetting_Line();

                SetPanel_Visible(flpLine);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }
        
        private void Unit_Click(object sender, EventArgs e)
        {
            try
            {
                string _unitKey = string.Empty ;
                string _err = string.Empty;

                Control _ctrl = (Control)sender;

                if (_ctrl.GetType().Name.ToString() == "csPictureBox")
                {
                    csPictureBox _pic = (csPictureBox)_ctrl;

                    // Key: NODENO(3) + UNITNO(2)
                    _unitKey = _pic.Tag.ToString().PadRight(3, ' ') + _pic.PropertyData.myID.Substring(2,2);
                }
                else if (_ctrl.GetType().Name.ToString() == "csLabel")
                {
                    csLabel _lbl = (csLabel)_ctrl;

                    // Key: NODENO(3) + UNITNO(2)
                    _unitKey = _lbl.Tag.ToString().PadRight(3, ' ') + _lbl.PropertyData.myID.Substring(2,2);
                }

                if (OPIAp.Dic_Unit.ContainsKey(_unitKey))
                {
                    CurUnit = OPIAp.Dic_Unit[_unitKey];

                    #region Send to BC EquipmentStatusRequest

                    EquipmentStatusRequest _trx = new EquipmentStatusRequest();
                    _trx.HEADER.REPLYSUBJECTNAME = OPIAp.SessionID;
                    _trx.BODY.LINENAME = OPIAp.CurLine.ServerName;
                    _trx.BODY.EQUIPMENTNO = CurUnit.NodeNo;

                    string _xml = _trx.WriteToXml();

                    FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, OPIAp.SessionID);

                    SendtoBC_JobDataCategoryRequest(CurUnit.NodeNo, CurUnit.UnitNo.PadLeft(2, '0'), "00");

                    #endregion

                    #region Set Unit info for 左側區塊
                    txUnitID.Text = CurUnit.UnitID == null ? "" : CurUnit.UnitID;
                    txtUnitNo.Text = CurUnit.UnitNo == null ? "" : CurUnit.UnitNo;

                    #region 特殊欄位顯示

                    #region  unit run mode 
                    lblUnitRunMode.Visible = CurUnit.UseRunMode == "Y" || CurUnit.UseRunMode == "R";
                    txtUnitRunMode.Visible = CurUnit.UseRunMode == "Y" || CurUnit.UseRunMode == "R";
                    #endregion

                    #region  Cell Buffer 顯示 unit buffer info
                    if (CurUnit.UnitType == "CELL_BUFFER")
                    {
                        flpUnit_CellBF.Visible = true;
                    }
                    else
                    {
                        flpUnit_CellBF.Visible = false;
                    }
                    #endregion

                    #endregion

                    #endregion
                }
                else
                {
                    CurUnit = null;
                }

                DataSetting_Unit();

                SetPanel_Visible(flpUnit);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void EQ_Click(object sender, EventArgs e)
        {
            try
            {
                string _err = string.Empty;
                string _localNo = string.Empty ;

                switch (sender.GetType().Name)
                {
                    case "csLabel":
                        Label _lbl = (Label)sender;
                        _localNo = _lbl.Tag.ToString();//記錄選取的NodeNo
                        break ;

                    case "csShape":
                        csShape _shp = (csShape)sender;
                        _localNo = _shp.Tag.ToString();//記錄選取的NodeNo
                        break ;

                    case "csPictureBox":

                        csPictureBox _pic = (csPictureBox)sender;
                        _localNo = _pic.Tag.ToString();
                        break;

                    default :
                        break ;
                }

                
                if (_localNo != string.Empty )
                {

                    if (!OPIAp.Dic_Node.ContainsKey(_localNo))
                    {
                        CurNode = null;

                        SetPanel_Visible(flpEQP);

                        return;
                    }

                    CurNode = OPIAp.Dic_Node[_localNo];

                    #region Send to BC EquipmentStatusRequest

                    EquipmentStatusRequest _trx = new EquipmentStatusRequest();
                    _trx.HEADER.REPLYSUBJECTNAME = OPIAp.SessionID;
                    _trx.BODY.LINENAME = OPIAp.CurLine.ServerName;
                    _trx.BODY.EQUIPMENTNO = _localNo;
                   
                    string _xml = _trx.WriteToXml();

                    //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;
                    FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), out _err, OPIAp.SessionID);

                    SendtoBC_JobDataCategoryRequest(_localNo,"00","00");

                    #endregion

                    #region Set EQ info for 左側區塊

                    #region 特殊欄位顯示


                    #region  ReportMode = HSMS顯示 SECS status
                    if (OPIAp.Dic_Node[_localNo].ReportMode.Contains("HSMS"))
                    {
                        flpHSMS.Visible = true;
                    }
                    else
                    {
                        flpHSMS.Visible = false;
                    }
                    #endregion

                    #region 特殊欄位顯示 -- run mode
                    lblRunMode.Visible = OPIAp.Dic_Node[_localNo].UseRunMode == "Y" || OPIAp.Dic_Node[_localNo].UseRunMode == "R";
                    txtRunMode.Visible = OPIAp.Dic_Node[_localNo].UseRunMode == "Y" || OPIAp.Dic_Node[_localNo].UseRunMode == "R";  
                    #endregion

                    #region 特殊欄位顯示 -- indexer mode
                    lblEQIndexerMode.Visible = OPIAp.Dic_Node[_localNo].UseIndexerMode;
                    txtEQIndexerMode.Visible = OPIAp.Dic_Node[_localNo].UseIndexerMode;
                    #endregion

                    #endregion

                    #region TBBFG --L3 只有EQ Status & CIM Mode
                    if (OPIAp.CurLine.LineType == "BFG" && CurNode.NodeNo == "L3")
                    {
                        lblEQAlive.Visible = false;
                        txtEQPAlive.Visible = false;

                        lblRunMode.Visible = false;
                        txtRunMode.Visible = false;

                        lblEQIndexerMode.Visible = false;
                        txtEQIndexerMode.Visible = false;

                        lblUSInlineMode.Visible = false;
                        txtUSInlineMode.Visible = false;

                        lblDNInlineMode.Visible = false;
                        txtDNInlineMode.Visible = false;

                        lblCurrentRecipeID.Visible = false;
                        txtCurrentRecipeID.Visible = false;

                        lblTFTJobCount.Visible = false;
                        txtTFTJobCount.Visible = false;

                        lblCFJobCount.Visible = false;
                        txtCFJobCount.Visible = false;

                        lblDummyJobCount.Visible = false;
                        txtDummyJobCount.Visible = false;

                        lblEQPOperationMode.Visible = false;
                        txtEQPOperationMode.Visible = false;

                        lblAutoRecipeChange.Visible = false;
                        txtAutoRecipeChange.Visible = false;
                    }
                    #endregion

                    #region Array 顯示最後收片的glass id & receive time
                    if (FormMainMDI.G_OPIAp.CurLine.FabType == "ARRAY" && CurNode.NodeNo != "L2")
                    {
                        if (CurNode.EQPStatus == eEQPStatus.Run)
                        {
                            lblLastGlassID.Visible = true;
                            txtLastGlassID.Visible = true;
                            lblLastReceiveTime.Visible = true;
                            txtLastReceiveTime.Visible = true;
                        }
                        else
                        {
                            lblLastGlassID.Visible = false;
                            txtLastGlassID.Visible = false;
                            lblLastReceiveTime.Visible = false;
                            txtLastReceiveTime.Visible = false;
                        }
                    }
                    else
                    {
                        lblLastGlassID.Visible = false;
                        txtLastGlassID.Visible = false;
                        lblLastReceiveTime.Visible = false;
                        txtLastReceiveTime.Visible = false;
                    }
                    #endregion

                    txtEQPID.Text = CurNode.NodeID;
                    txtEQPName.Text = CurNode.NodeName;

                    #endregion

                }
                else CurNode = null;

                HandleControl(_localNo);

                DataSetting_EQP();

                SetPanel_Visible(flpEQP);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void Port_Click(object sender, EventArgs e)
        {
            try
            {
                csPictureBox _pic = (csPictureBox)sender;

                string strPortKey = _pic.PropertyData.myID;
                string strNodeNo = _pic.Tag.ToString().PadRight(3, ' ');

                if (OPIAp.Dic_Port.ContainsKey(strNodeNo + strPortKey.Substring(2, 2)))
                {
                    CurPort = OPIAp.Dic_Port[strNodeNo + strPortKey.Substring(2, 2)];

                    #region Send to BC PortStatusRequest
                    SendtoBC_PortStatusRequest(CurPort.NodeNo, CurPort.PortNo);

                    SendtoBC_JobDataCategoryRequest(CurPort.NodeNo, "00", CurPort.PortNo);
                    //PortStatusRequest _trx = new PortStatusRequest();
                    //_trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //_trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //_trx.BODY.EQUIPMENTNO = CurPort.NODENO;
                    //_trx.BODY.PORTNO = CurPort.PORTNO;

                    //string _xml = _trx.WriteToXml();

                    //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SessionID)) return;

                    #endregion

                    #region Set Port info for 左側區塊

                    txtPortEQPID.Text = CurPort.NodeID == null ? "" : CurPort.NodeID;

                    txtPortID.Text = CurPort.PortID;

                    #region 判斷有兩條lineid 時再顯示line id
                    if (OPIAp.CurLine.LineID2 == string.Empty)
                    {
                        lblPortLineID.Visible = false;
                        txtPortLineID.Visible = false;
                    }
                    else
                    {
                        lblPortLineID.Visible = true;
                        txtPortLineID.Visible = true;
                    }
                    #endregion

                    #region CCGAP Line 顯示Port Assignment
                    if (OPIAp.CurLine.LineType == "GAP" || OPIAp.CurLine.LineType == "PDR")
                    {
                        lblPortAssignment.Visible = true;
                        txtPortAssignment.Visible = true;
                    }
                    else
                    {
                        lblPortAssignment.Visible = false;
                        txtPortAssignment.Visible = false;
                    }
                    #endregion

                    #endregion
                }
                else CurPort = null;

                DataSetting_Port();

                SetPanel_Visible(flpPort);
                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void Dense_Click(object sender, EventArgs e)
        {
            try
            {
                string strDenseKey = string.Empty;
                string strNodeNo = string.Empty;

                if (sender.GetType().Name == "csLabel")
                {
                    csLabel _lbl = (csLabel)sender;

                    strDenseKey = _lbl.PropertyData.myID;
                    strNodeNo = _lbl.Tag.ToString().PadRight(3, ' ');
                }
                else
                {
                    csPictureBox _pic = (csPictureBox)sender;

                    strDenseKey = _pic.PropertyData.myID;
                    strNodeNo = _pic.Tag.ToString().PadRight(3, ' ');
                }

                if (OPIAp.Dic_Dense.ContainsKey(strNodeNo + strDenseKey.Substring(2, 2)))
                {
                    CurDense = OPIAp.Dic_Dense[strNodeNo + strDenseKey.Substring(2, 2)];

                    #region Send to BC DenseStatusRequest

                    SendtoBC_DenseStatusRequest(CurDense.NodeNo, CurDense.PortNo);

                    //DenseStatusRequest _trx = new DenseStatusRequest();
                    //_trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //_trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //_trx.BODY.EQUIPMENTNO = CurDense.NodeNo;
                    //_trx.BODY.PORTNO = CurDense.PortNo;

                    //string _xml = _trx.WriteToXml();

                    //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SessionID)) return;

                    #endregion

                    #region Set Dense info for 左側區塊

                    txtDenseEQPID.Text = CurDense.NodeID == null ? "" : CurDense.NodeID;

                    txtDensePortID.Text = CurDense.PortID;

                    txtDensePortEnable.Text = CurDense.PortEnable.ToString();

                    txtDensePackingMode.Text = CurDense.PackingMode.ToString();

                    txtDenseBoxID01.Text = CurDense.BoxID01.ToString();

                    txtDenseBoxID02.Text = CurDense.BoxID02.ToString();

                    txtDenseSource.Text = CurDense.UnpackSource.ToString();

                    txtPaperBoxID.Text = CurDense.PaperBoxID.ToString();

                    txtBoxType.Text = CurDense.BoxType.ToString();

                    txtDenseRequest.Text = CurDense.DenseDataRequest ? "ON" : "OFF";
                    #endregion


                    if (OPIAp.CurLine.LineType == "PPK")
                    {
                        #region PPK Dense 只有Port 03 ~ Port 13 僅顯示 port enable狀態
                        int _portNo = 0;
                        int.TryParse(CurDense.PortNo, out _portNo);
                        lblDenseBoxID01.Text = "Box ID";
                        lblDenseBoxID02.Visible = false;
                        txtDenseBoxID02.Visible = false;
                        lblDenseSource.Visible = false;
                        txtDenseSource.Visible = false;
                        lblDensePackingMode.Visible = false;
                        txtDensePackingMode.Visible = false;

                        if (_portNo >= 3 && _portNo <= 13)
                        {
                            lblDenseBoxID01.Visible = false;
                            txtDenseBoxID01.Visible = false;
                            lblPaperBoxID.Visible = false;
                            txtPaperBoxID.Visible = false;
                            lblBoxType.Visible = false;
                            txtBoxType.Visible = false;
                            lblDenseRequest.Visible = false;
                            txtDenseRequest.Visible = false;
                        }
                        else
                        {
                            lblDenseBoxID01.Visible = true;
                            txtDenseBoxID01.Visible = true;
                            lblPaperBoxID.Visible = true;
                            txtPaperBoxID.Visible = true;
                            lblBoxType.Visible = true;
                            txtBoxType.Visible = true;
                            lblDenseRequest.Visible = true;
                            txtDenseRequest.Visible = true;
                        }
                        #endregion

                        #region PPK Dense 只有Port 01 & Port 15 可下貨

                        //if ( CurDense.PortNo != "01" && CurDense.PortNo != "15")  
                        //{
                        //    lblDenseRequest.Visible = false;
                        //    txtDenseRequest.Visible = false;
                        //    btnDenseControl.Visible = false;
                        //}
                        //else
                        //{
                        //    lblDenseRequest.Visible = true;
                        //    txtDenseRequest.Visible = true;
                        //    btnDenseControl.Visible = true;
                        //}

                        #endregion
                    }
                    else if (OPIAp.CurLine.LineType == "QPP")
                    {
                        #region QPP Dense
                        lblPaperBoxID.Visible = false;
                        txtPaperBoxID.Visible = false;
                        lblBoxType.Visible = false;
                        txtBoxType.Visible = false;
                        #endregion
                    }                    
                }
                else CurDense = null;

                DataSetting_Dense();

                SetPanel_Visible(flpDense);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void Pallet_Click(object sender, EventArgs e)
        {
            try
            {
                csPictureBox _pic = (csPictureBox)sender;

                string strPalletKey = _pic.PropertyData.myID.Substring(2,2);

                if (OPIAp.Dic_Pallet.ContainsKey(strPalletKey))
                {
                    CurPallet = OPIAp.Dic_Pallet[strPalletKey];

                    #region Send to BC PalletStatusRequest
                    SendtoBC_PalletStatusRequest(CurPallet.PalletNo);

                    //PalletStatusRequest _trx = new PalletStatusRequest();
                    //_trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //_trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //_trx.BODY.PALLETNO = CurPallet.PalletNo;

                    //string _xml = _trx.WriteToXml();

                    //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SessionID)) return;

                    #endregion

                    #region Set Pallet info for 左側區塊

                    txtPalletNo.Text = strPalletKey;

                    txtPalletID.Text = CurPallet.PalletID;

                    txtPalletMode.Text = CurPallet.PalletMode.ToString();

                    txtPalletDataRequest.Text = CurPallet.PalletDataRequest ? "ON" : "OFF";
                    #endregion
                }
                else CurPallet = null;


                #region PPK Pallet 不提供下貨功能

                if (OPIAp.CurLine.LineType == "PPK")
                {
                    lblPalletDataRequest.Visible = false ;
                    txtPalletDataRequest.Visible = false;
                    btnPalletControl.Visible = false;
                }
                else
                {
                    lblPalletDataRequest.Visible = true;
                    txtPalletDataRequest.Visible = true;
                    btnPalletControl.Visible = true;
                }
                #endregion

                DataSetting_Pallet();

                SetPanel_Visible(flpPallet);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void VCR_Click(object sender, EventArgs e)
        {
            try
            {
                csPictureBox _pic = (csPictureBox)sender;

                if (_pic.Tag != null)
                {
                    string _localNo = _pic.Tag.ToString();//記錄選取的NodeNo

                    if (!OPIAp.Dic_Node.ContainsKey(_localNo))
                    {
                        CurNode = null;

                        SetPanel_Visible(flpVCR);

                        return;
                    }

                    CurNode = OPIAp.Dic_Node[_localNo];

                    string _vcrNo = _pic.PropertyData.myID.Substring(2,2);

                    CurVCR = CurNode.VCRs.Find(d => d.VCRNO.Equals(_vcrNo));

                    if (CurVCR != null)
                    {
                        #region Send to BC EquipmentStatusRequest
                        SendtoBC_EquipmentStatusRequest(_localNo);
                        //EquipmentStatusRequest _trx = new EquipmentStatusRequest();
                        //_trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        //_trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        //_trx.BODY.EQUIPMENTNO = _localNo;

                        //string _xml = _trx.WriteToXml();

                        //if (!this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), FormMainMDI.G_OPIAp.SessionID)) return;

                        #endregion

                        #region Set VCR info for 左側區塊
                        txtVCRNo.Text = CurVCR.VCRNO;
                        txtVCRLocalNo.Text = CurNode.NodeNo;
                        #endregion
                    }
                }
                else CurVCR = null;

                DataSetting_VCR();

                SetPanel_Visible(flpVCR);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void DataSetting_Line()
        {
            try
            {
                if (txtLineOperation.Text != OPIAp.CurLine.LineOperMode) txtLineOperation.Text = OPIAp.CurLine.LineOperMode;

                string _indexerDesc = UniTools.GetEquipmentIndexerModeDesc(OPIAp.CurLine.IndexerMode.ToString());

                if (txtIndexerMode.Text != _indexerDesc) txtIndexerMode.Text = _indexerDesc;

                //IndexerMode = 3:changer mode || Line Operation Mode = EXCHANGE
                if (OPIAp.CurLine.IndexerMode == 3 || OPIAp.CurLine.LineOperMode == "EXCHANGE")
                {
                    if (txtPlanStatus.Text != OPIAp.CurLine.ChangerPlanStatus.ToString()) txtPlanStatus.Text = OPIAp.CurLine.ChangerPlanStatus.ToString();
                    if (txtPlanID.Text != OPIAp.CurLine.ChangerPlanID) txtPlanID.Text = OPIAp.CurLine.ChangerPlanID;
                    if (txtStandByPlanID.Text != OPIAp.CurLine.StandByChangerPlanID) txtStandByPlanID.Text = OPIAp.CurLine.StandByChangerPlanID;
                    //if (txtEQPRunMode.Text != OPIAp.CurLine.EquipmentRunMode) txtEQPRunMode.Text = OPIAp.CurLine.EquipmentRunMode;
                    flpChangerPlan.Visible = true;
                }
                else
                {
                    //txtEQPRunMode.Text = string.Empty;
                    txtPlanStatus.Text = string.Empty;
                    txtPlanID.Text = string.Empty;
                    txtStandByPlanID.Text = string.Empty;
                    flpChangerPlan.Visible = false;
                }

                if (OPIAp.CurLine.HistoryType == "FCPHO")
                {
                    lblShortCutMode.Visible = true;
                    txtShortCutMode.Visible = true;
                    if (txtShortCutMode.Text != OPIAp.CurLine.ShortCutMode)
                    {
                        txtShortCutMode.Text = OPIAp.CurLine.ShortCutMode;
                    }
                }
                else
                {
                    lblShortCutMode.Visible = false;
                    txtShortCutMode.Visible = false;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void DataSetting_EQP()
        {
            try
            {
                if (CurNode != null)
                {

                    #region Set node real data

                    //Equipment Operation Mode
                    if (txtEQPOperationMode.Text != CurNode.OperationMode.ToString()) txtEQPOperationMode.Text = CurNode.OperationMode.ToString();                    

                    //Auto Recipe Change
                    if (txtAutoRecipeChange.Text != CurNode.AutoRecipeChange) txtAutoRecipeChange.Text = CurNode.AutoRecipeChange;

                    //EQP Status
                    if (txtCurrentStatus.Text != CurNode.EQPStatus.ToString()) txtCurrentStatus.Text = CurNode.EQPStatus.ToString();

                    //CIM Mode
                    if (txtCimMode.Text != CurNode.CIMMode.ToString()) txtCimMode.Text = CurNode.CIMMode.ToString();

                    //TFT Job Count
                    if (txtTFTJobCount.Text != CurNode.TFTJobCount.ToString()) txtTFTJobCount.Text = CurNode.TFTJobCount.ToString();

                    //CF Job Count
                    if (txtCFJobCount.Text != CurNode.CFJobCount.ToString()) txtCFJobCount.Text = CurNode.CFJobCount.ToString();

                    //Dummy Job Count
                    if (txtDummyJobCount.Text != CurNode.DummyJobCount.ToString()) txtDummyJobCount.Text = CurNode.DummyJobCount.ToString();

                    #region Run Mode
                    if (CurNode.EQPRunMode == null)
                    {
                        if (txtRunMode.Text != string.Empty) txtRunMode.Text = string.Empty;
                    }
                    else
                    {
                        if (txtRunMode.Text != CurNode.EQPRunMode) txtRunMode.Text = CurNode.EQPRunMode;
                    }

                    #endregion

                    #region EQP Indexer Mode

                    if (OPIAp.CurLine.IndexerNode != null)
                    {
                        if (CurNode.NodeNo == OPIAp.CurLine.IndexerNode.NodeNo)
                        {
                            string _indexerDesc = UniTools.GetEquipmentIndexerModeDesc(OPIAp.CurLine.IndexerMode.ToString());

                            if (txtEQIndexerMode.Text != _indexerDesc) txtEQIndexerMode.Text = _indexerDesc;
                        }
                    }
                    #endregion
                    
                    #region Operation Mode

                    if (txtEQPOperationMode.Text != CurNode.OperationMode.ToString()) txtEQPOperationMode.Text = CurNode.OperationMode.ToString();
                    
                    #endregion

                    #region InlineMode
                    if (CurNode.UpStreamInlineMode == null)
                    {
                        if (txtUSInlineMode.Text != string.Empty) txtUSInlineMode.Text = string.Empty;
                    }
                    else
                    {
                        if (txtUSInlineMode.Text != CurNode.UpStreamInlineMode) txtUSInlineMode.Text = CurNode.UpStreamInlineMode;
                    }
                    #endregion

                    #region DownStreamlineMode
                    if (CurNode.DownStreamInlineMode == null)
                    {
                        txtDNInlineMode.Text = string.Empty ;
                    }
                    else
                    {
                        if (txtDNInlineMode.Text != CurNode.DownStreamInlineMode) txtDNInlineMode.Text = CurNode.DownStreamInlineMode;
                    }
                    
                    #endregion

                    #region Current Recipe ID
                    if (CurNode.RecipeName == null) 
                    {
                        if (txtCurrentRecipeID.Text !=string.Empty ) txtCurrentRecipeID.Text = string.Empty ;
                    }
                    else
                    {
                        if (txtCurrentRecipeID.Text != CurNode.RecipeName) txtCurrentRecipeID.Text = CurNode.RecipeName;
                    }
                    #endregion

                    #region EQP Alive
                    if (CurNode.EquipmentAlive == null)
                    {
                        if (txtEQPAlive.Text != string.Empty ) txtEQPAlive.Text = string.Empty ;
                    }
                    else 
                    {
                        if (txtEQPAlive.Text != CurNode.EquipmentAlive) txtEQPAlive.Text = CurNode.EquipmentAlive;
                    }
                    
                    #endregion

                    #region HSMS Control Mode
                    if (CurNode.HSMSControlMode == null)
                    {
                        if (txtCtrlMode.Text != string.Empty) txtCtrlMode.Text = string.Empty;
                    }
                    else
                    {
                        if (txtCtrlMode.Text != CurNode.HSMSControlMode) txtCtrlMode.Text =  CurNode.HSMSControlMode;
                    }
                    
                    #endregion

                    #region HSMS Status
                    if (CurNode.HSMSStatus == null)
                    {
                        if (txtHSMSStatus.Text != string.Empty ) txtHSMSStatus.Text = string.Empty ;
                    }
                    else
                    {
                        if (txtHSMSStatus.Text !=  CurNode.HSMSStatus) txtHSMSStatus.Text =  CurNode.HSMSStatus;
                    }
                    
                    #endregion

                    #region 最後receive glass id & datetime - 若超過tack time則textbox顯示紅色
                    //Last Receive Glass ID
                    if (txtLastGlassID.Text != CurNode.LastReceiveGlassID.ToString()) txtLastGlassID.Text = CurNode.LastReceiveGlassID.ToString();

                    //Last Receive Glass ID
                    if (txtLastReceiveTime.Text != CurNode.LastReceiveDateTime.ToString()) txtLastReceiveTime.Text = CurNode.LastReceiveDateTime.ToString();

                    if (CurNode.IsOverTackTime)
                    {
                        txtLastGlassID.BackColor = Color.Red;
                        txtLastReceiveTime.BackColor = Color.Red;
                    }
                    else
                    {
                        txtLastGlassID.BackColor = SystemColors.Control;
                        txtLastReceiveTime.BackColor = SystemColors.Control;
                    }
                    #endregion    
      
                    #region Array 顯示最後收片的glass id & receive time
                    if (FormMainMDI.G_OPIAp.CurLine.FabType == "ARRAY" && CurNode.NodeNo != "L2")
                    {

                        if (CurNode.EQPStatus == eEQPStatus.Run)
                        {
                            lblLastGlassID.Visible = true;
                            txtLastGlassID.Visible = true;
                            lblLastReceiveTime.Visible = true;
                            txtLastReceiveTime.Visible = true;
                        }
                        else
                        {
                            lblLastGlassID.Visible = false;
                            txtLastGlassID.Visible = false;
                            lblLastReceiveTime.Visible = false;
                            txtLastReceiveTime.Visible = false;
                        }
                    }
                    #endregion

                    #endregion
                }
                else
                {
                    #region Set empty
                    txtEQPID.Text = string.Empty ;
                    txtEQPName.Text = string.Empty;
                    txtCurrentStatus.Text = string.Empty;
                    txtCimMode.Text = string.Empty;
                    txtRunMode.Text = string.Empty;
                    txtUSInlineMode.Text = string.Empty;
                    txtDNInlineMode.Text = string.Empty;
                    txtCurrentRecipeID.Text = string.Empty;
                    txtTFTJobCount.Text = "0";
                    txtCFJobCount.Text = "0";
                    txtDummyJobCount.Text = "0";
                    txtEQPAlive.Text = string.Empty;
                    txtEQPOperationMode.Text = string.Empty;
                    #endregion
                }                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void DataSetting_Port()
        {
            try
            {
                if (CurPort != null)
                {
                    //GAP Port Assignment
                    if (txtPortAssignment.Text != CurPort.PortAssignment.ToString()) txtPortAssignment.Text = CurPort.PortAssignment.ToString();

                    //Line id
                    if (txtPortLineID.Text != CurPort.LineID.ToString()) txtPortLineID.Text = CurPort.LineID.ToString();
                    
                    // Port Status
                    if (txtPortCount.Text != CurPort.PortGlassCount.ToString()) txtPortCount.Text = CurPort.PortGlassCount.ToString();

                    // Port Status
                    if (txtPortStatus.Text != CurPort.PortStatus.ToString()) txtPortStatus.Text = CurPort.PortStatus.ToString();

                    //Port Down
                    if (txtPortDown.Text != CurPort.PortDown.ToString()) txtPortDown.Text = CurPort.PortDown.ToString();

                    //Cassette Status
                    if (txtCassetteStatus.Text != CurPort.CassetteStatus.ToString()) txtCassetteStatus.Text = CurPort.CassetteStatus.ToString();

                    //Port Type
                    if (txtPortType.Text != CurPort.PortType.ToString()) txtPortType.Text = CurPort.PortType.ToString();

                    #region Port Mode
                    //if (CurPort.PortMode == null)
                    //{
                    //    if (txtPortMode.Text != string.Empty) txtPortMode.Text = string.Empty ;
                    //}
                    //else
                    //{
                        if (txtPortMode.Text != CurPort.PortMode.ToString()) txtPortMode.Text = CurPort.PortMode.ToString();
                    //}                        
                    #endregion

                    #region Port Enable
                    //if (CurPort.PortEnable == null)
                    //{
                    //    if (txtPortEnable.Text != string.Empty ) txtPortEnable.Text = string.Empty ;
                    //}
                    //else
                    //{
                        if (txtPortEnable.Text != CurPort.PortEnable.ToString()) txtPortEnable.Text = CurPort.PortEnable.ToString();
                    //}
                    #endregion
                                           
                    #region Cassette ID
                        if (CurPort.CassetteID == null)
                    {
                        if (txtCassetteID.Text != string.Empty) txtCassetteID.Text = string.Empty ;
                    }
                    else
                    {
                        if (txtCassetteID.Text != CurPort.CassetteID) txtCassetteID.Text = CurPort.CassetteID;
                    }
                    #endregion
                   
                    #region Cassette Seq No
                    if (CurPort.CassetteSeqNo == null)
                    {
                        if (CurPort.CassetteSeqNo != string.Empty) CurPort.CassetteSeqNo = string.Empty;
                    }
                    else
                    {
                        if (txtCassetteSeqNo.Text != CurPort.CassetteSeqNo) txtCassetteSeqNo.Text = CurPort.CassetteSeqNo;
                    }
                    #endregion

                    #region Loading CST Type

                    if (txtLoadCSTType.Text != CurPort.LoadingCassetteType.ToString()) txtLoadCSTType.Text = CurPort.LoadingCassetteType.ToString();
                    
                    #endregion

                    #region Port Transfer Mode
                    //if (CurPort.PortTransfer == null)
                    //{
                    //    if (txtPortTransfer.Text != string.Empty ) txtPortTransfer.Text =string.Empty ;
                    //}
                    //else 
                    //{
                        if (txtPortTransfer.Text != CurPort.PortTransfer.ToString()) txtPortTransfer.Text = CurPort.PortTransfer.ToString();
                    //}
                    #endregion

                    #region Port Partial Full Mode
                    txtPartialFullMode.Text = CurPort.PartialFullMode ? "ON" : "OFF";
                    #endregion

                    #region Port Grade   -add by sy.wu-
                    if (OPIAp.CurLine.FabType == "CF" && OPIAp.CurLine.IndexerMode == 2)
                    {
                        lblPortGrade.Visible = true;
                        txtPortGrade.Visible = true;
                        if (txtPortGrade.Text != CurPort.PortGrade.ToString())
                        {
                            txtPortGrade.Text = CurPort.PortGrade.ToString();
                        }
                    }
                    else
                    {
                        lblPortGrade.Visible = false;
                        txtPortGrade.Visible = false;
                    }
                    //txtPortGrade.Text = CurPort.PortGrade;
                    #endregion 
                    //#region Port Grade
                    //txtPortGrade.Text = CurPort.PortGrade;
                    //#endregion

                    //#region Port Product Type
                    //txtPortProductType.Text = CurPort.ProductType;
                    //#endregion

                    //#region PortProcess Type
                    //txtPortProcessType.Text = GetPortProcrssTypeContent(CurPort.ProcessType_Array);
                    //#endregion

                    #region Cassette Command下拉選單設定 -- cassette status & port status變更時重新設定下拉選單
                    if ((cboCassetteCmd.Tag.ToString() != CurPort.CassetteStatus.ToString() + CurPort.PortStatus.ToString()) ||
                        (OPIAp.CurLine.LineType == "FCUPK_TYPE1" && OPIAp.RunModeHaveChange) ||
                        (OPIAp.CurLine.LineType == "FCREW_TYPE1" && OPIAp.IndexerModeHaveChange))
                    {

                        OPIAp.RunModeHaveChange = false;
                        OPIAp.IndexerModeHaveChange = false;

                        cboCassetteCmd.Tag = CurPort.CassetteStatus.ToString() + CurPort.PortStatus.ToString();
                        //‘1’：Cassette Process Start
                        //‘2’：Cassette Process Start By Count
                        //‘3’：Cassette Process Pause
                        //‘4’：Cassette Process Resume
                        //‘5’：Cassette Process Abort
                        //‘6’：Cassette Process Cancel 
                        //‘7’：Cassette Reload
                        //‘8’：Cassette Load
                        //‘9’：Cassette Re-Map
                        //‘11’：Cassette Map Download
                        //‘12’：Cassette Aborting
                        List<comboxInfo> _lstCSTCmd = new List<comboxInfo>();

                        #region Cassettee status -- 顯示rule by 登京
                        switch (CurPort.CassetteStatus)
                        {
                            case eCassetteStatus.UnKnown: break;

                            case eCassetteStatus.WaitingforCassetteData:

                                #region WaitingforCassetteData
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "6", ITEM_NAME = "Cancel" });
                                break;
                                #endregion

                            case eCassetteStatus.WaitingforStartCommand:

                                #region WaitingforStartCommand

                                switch (OPIAp.CurLine.LineType)
                                {
                                    case "FCUPK_TYPE1":

                                        #region CF UPK Rule - 當Unloader的EQP Run Mode為3:Normal下，Cassette Status:”3:Wait For Start Command”時，OPI Offline下拉選單只能選擇”Start By Count”．
                                        //1. 當Unloader的EQP Run Mode為3:Normal下，Cassette Status:”3:Wait For Start Command”時，OPI Offline下拉選單只能選擇”Start By Count”．
                                        //2. 當Unloader的EQP Run Mode為4:Re-Clean下，Cassette Status:”3:Wait For Start Command”時，OPI Offline下拉選單只能選擇”Start”．

                                        Node _nodeL5 = FormMainMDI.G_OPIAp.Dic_Node["L5"];

                                        if (_nodeL5.EQPRunMode == "NORMAL")
                                            _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "2", ITEM_NAME = "Start By Count" });
                                        else if (_nodeL5.EQPRunMode == "RE-CLEAN")
                                            _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "1", ITEM_NAME = "Start" });

                                        break;

                                        #endregion

                                    case "FCREW_TYPE1":

                                        #region CF REW Rule 
                                        //Loader Port當Indexer Mode為 11:Normal下，Cassette Status:”3:Wait For Start Command”時，OPI Offline下拉選單只能選擇”Start By Count”． 其餘 僅能挑選Start

                                        if (OPIAp.CurLine.IndexerMode == 11 && CurPort.PortType== ePortType.LoadingPort)
                                            _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "2", ITEM_NAME = "Start By Count" });
                                        else 
                                            _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "1", ITEM_NAME = "Start" });
                                        
                                        break;

                                        #endregion
                                        
                                    default :
                                        
                                        //CF僅提供Start
                                        if (OPIAp.CurLine.FabType == "CF")
                                        {
                                            _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "1", ITEM_NAME = "Start" });
                                        }
                                        else
                                        {
                                            _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "1", ITEM_NAME = "Start" });
                                            _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "2", ITEM_NAME = "Start By Count" });
                                        }
                                        break;
                                }

                                //if (OPIAp.CurLine.LineType != "FCUPK_TYPE1")
                                //{
                                //    _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "1", ITEM_NAME = "Start" });
                                //    _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "2", ITEM_NAME = "Start By Count" });
                                //}
                                //else
                                //{
                                //    #region CF UPK Rule
                                //    //1. 當Unloader的EQP Run Mode為3:Normal下，Cassette Status:”3:Wait For Start Command”時，OPI Offline下拉選單只能選擇”Start By Count”．
                                //    //2. 當Unloader的EQP Run Mode為4:Re-Clean下，Cassette Status:”3:Wait For Start Command”時，OPI Offline下拉選單只能選擇”Start”．

                                //    Node _nodeL5 = FormMainMDI.G_OPIAp.Dic_Node["L5"];

                                //    if (_nodeL5.EQPRunMode=="NORMAL")
                                //        _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "2", ITEM_NAME = "Start By Count" });
                                //    else if (_nodeL5.EQPRunMode=="RE-CLEAN")
                                //        _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "1", ITEM_NAME = "Start" });

                                //    #endregion
                                //}
                                
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "3", ITEM_NAME = "Pause" });
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "6", ITEM_NAME = "Cancel" });
                                break;
                                #endregion

                            case eCassetteStatus.WaitingforProcessing:

                                #region WaitingforProcessing
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "3", ITEM_NAME = "Pause" });
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "6", ITEM_NAME = "Cancel" });
                                break;
                                #endregion

                            case eCassetteStatus.ProcessPaused:

                                 #region ProcessPaused
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "4", ITEM_NAME = "Resume" });
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "5", ITEM_NAME = "Abort" });
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "6", ITEM_NAME = "Cancel" });
                                break;
                                #endregion

                            case eCassetteStatus.InProcessing:

                                #region InProcessing
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "3", ITEM_NAME = "Pause" });
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "5", ITEM_NAME = "Abort" });

                                //如果Indexer Operation Mode是3:Change Mode時不能下 Remap功能 --俊成
                                if (OPIAp.CurLine.IndexerMode != 3)
                                {
                                    _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "9", ITEM_NAME = "CST Re-Map" });
                                }

                                //如果Port Type = Both Used
                                if (CurPort.PortType == ePortType.BothPort)
                                {
                                    _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "12", ITEM_NAME = "CST Aborting" });
                                }
                                break;
                                #endregion

                            case eCassetteStatus.InAborting:

                                #region InAborting
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "5", ITEM_NAME = "Abort" });

                                break;
                                #endregion

                            case eCassetteStatus.ProcessCompleted:

                                #region ProcessCompleted
                                if (CurPort.PortStatus == ePortStatus.UnloadRequest)
                                {
                                    _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "7", ITEM_NAME = "Cassette Reload" });
                                }
                                break;
                                #endregion

                            case eCassetteStatus.CassetteReMap:

                                #region CassetteReMap
                                _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "5", ITEM_NAME = "Abort" });
                                //_lstCSTCmd.Add(new comboxInfo { ITEM_ID = "1", ITEM_NAME = "Start" });
                                //_lstCSTCmd.Add(new comboxInfo { ITEM_ID = "2", ITEM_NAME = "Start By Count" });
                                //_lstCSTCmd.Add(new comboxInfo { ITEM_ID = "3", ITEM_NAME = "Pause" });
                                //_lstCSTCmd.Add(new comboxInfo { ITEM_ID = "6", ITEM_NAME = "Cancel" });

                                break;
                                #endregion

                            case eCassetteStatus.NoCassetteExist:

                                #region NoCassetteExist
                                //LDCM時 , NO CST 的狀態 可送cancel  ---2015/4/2 俊成提出
                                if (CurPort.PortStatus == ePortStatus.LoadComplete)
                                {
                                    _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "6", ITEM_NAME = "Cancel" });
                                }
                                else if (CurPort.PortStatus == ePortStatus.LoadRequest)
                                {
                                    _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "8", ITEM_NAME = "Cassette Load" });
                                }
                                break;
                                #endregion

                            default: break;
                        }
                        #endregion

                        //#region port status -- 顯示rule by 登京
                        ////Load Request - Cassette load
                        ////Unload Request- Cassette Reload
                        //switch (CurPort.PortStatus)
                        //{
                        //    case ePortStatus.LoadRequest:
                        //    case ePortStatus.UnloadComplete:
                        //        _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "8", ITEM_NAME = "Cassette Load" });
                        //        break;

                        //    case ePortStatus.UnloadRequest:
                        //        _lstCSTCmd.Add(new comboxInfo { ITEM_ID = "7", ITEM_NAME = "Cassette Reload" });
                        //        break;

                        //    default: break;
                        //}
                        //#endregion

                        cboCassetteCmd.Enabled = false; 
                        cboCassetteCmd.DataSource = _lstCSTCmd.ToList();
                        cboCassetteCmd.DisplayMember = "ITEM_NAME";
                        cboCassetteCmd.ValueMember = "ITEM_ID";
                        cboCassetteCmd.SelectedIndex = -1;
                        cboCassetteCmd.Enabled = true;
                    }
                    #endregion

                    //CurPort.SubCassetteStatus = "WACSTEDIT";
                    //FormMainMDI.G_OPIAp.CurLine.MesControlMode = "OFFLINE";
                    //CurPort.JobExistenceSlot = "1".PadLeft(CurPort.MaxCount, '1');
                    //CurPort.CassetteID = "CST0001";
                    //CurPort.PortGlassCount = CurPort.MaxCount;

                    #region Port Start Button -- 只有wait edit狀態才能點選button
                    if (CurPort.SubCassetteStatus != null)
                    {
                        //‘WACSTEDIT’: Wait for OPI Edit Cassette Data (for online local/Offline)
                        //‘WAREMAPEDIT’: Wait for OPI remap edit Cassette Data.
                        //‘WASTART’: Wait for OPI Start command
                        switch (CurPort.SubCassetteStatus)
                        {
                            case "WACSTEDIT":
                            case "WAREMAPEDIT":
                                if (!btnStart.Enabled) btnStart.Enabled = true;
                                btnStart.BackColor = (btnStart.BackColor == Color.Green ? Color.DimGray : Color.Green);
                                break;

                            default:
                                btnStart.BackColor = Color.DimGray;
                                if (btnStart.Enabled) btnStart.Enabled = false;
                                break;
                        }

                        if (btnStart.Tag.ToString() != CurPort.SubCassetteStatus)
                        {
                            btnStart.Tag = CurPort.SubCassetteStatus;
                            ToolTip _tip = new ToolTip();
                            _tip.SetToolTip(btnStart, btnStart.Tag.ToString());
                        }
                    }
                    else
                    {
                        btnStart.BackColor = Color.DimGray;
                        btnStart.Enabled = false;
                    }
                    #endregion

                    #region Cassette Status為 Waiting for Start Command時，send 按鈕閃爍
                    if (CurPort.CassetteStatus == eCassetteStatus.WaitingforStartCommand)
                    {
                        btnCassetteCmd.BackColor = (btnCassetteCmd.BackColor == Color.Green ? Color.DimGray : Color.Green);
                    }
                    else 
                    {
                        btnCassetteCmd.BackColor = Color.DimGray;
                    }
                    #endregion
                }
                else
                {
                    #region Set empty
                    txtPortEQPID.Text = string.Empty;
                    txtPortID.Text = string.Empty;
                    txtCassetteID.Text = string.Empty;
                    txtCassetteSeqNo.Text = string.Empty;
                    txtPortType.Text = string.Empty;
                    txtPortStatus.Text = string.Empty;
                    txtCassetteStatus.Text = string.Empty;
                    txtPortTransfer.Text = string.Empty;
                    txtPortMode.Text = string.Empty;
                    txtPortEnable.Text = string.Empty;
                    txtPortDown.Text = string.Empty;
                    btnStart.Enabled = false;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void DataSetting_Dense()
        {
            try
            {
                if (CurDense == null)
                {
                    #region Set empty
                    txtDensePortEnable.Text = string.Empty;
                    txtDensePackingMode.Text = string.Empty;
                    txtDenseBoxID01.Text = string.Empty;
                    txtDenseBoxID02.Text = string.Empty;
                    txtDenseSource.Text = string.Empty;
                    txtPaperBoxID.Text = string.Empty;
                    txtBoxType.Text = string.Empty;
                    txtDenseRequest.Text = string.Empty;
                    btnDenseControl.Enabled = false;
                    #endregion 

                    return;
                }

                 #region Update Dense Info
                // Port Enable
                if (txtDensePortEnable.Text != CurDense.PortEnable.ToString()) txtDensePortEnable.Text = CurDense.PortEnable.ToString();

                // Dense Packing Mode
                if (txtDensePackingMode.Text != CurDense.PackingMode.ToString()) txtDensePackingMode.Text = CurDense.PackingMode.ToString();

                //Dense Box #01
                if (txtDenseBoxID01.Text != CurDense.BoxID01.ToString()) txtDenseBoxID01.Text = CurDense.BoxID01.ToString();

                //Dense Box #02
                if (txtDenseBoxID02.Text != CurDense.BoxID02.ToString()) txtDenseBoxID02.Text = CurDense.BoxID02.ToString();

                //Dense Source
                if (txtDenseSource.Text != CurDense.UnpackSource.ToString()) txtDenseSource.Text = CurDense.UnpackSource.ToString();

                //Paper Box ID
                if (txtPaperBoxID.Text != CurDense.PaperBoxID.ToString()) txtPaperBoxID.Text = CurDense.PaperBoxID.ToString();

                //Box Type
                if (txtBoxType.Text != CurDense.BoxType.ToString()) txtBoxType.Text = CurDense.BoxType.ToString();

                //Dense Request
                if (txtDenseRequest.Text != CurDense.DenseDataRequest.ToString()) txtDenseRequest.Text = CurDense.DenseDataRequest ? "ON" : "OFF";    
                #endregion

                if (CurDense.DenseDataRequest) 
                {
                    btnDenseControl.BackColor = (btnDenseControl.BackColor == Color.Green ? Color.DimGray : Color.Green);
                    if (!btnDenseControl.Enabled) btnDenseControl.Enabled = true;
                }
                else
                {
                    btnDenseControl.BackColor = Color.DimGray;
                    if (btnDenseControl.Enabled) btnDenseControl.Enabled = false;
                }
 
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void DataSetting_Pallet()
        {
            try
            {
                if (CurPallet == null)
                {
                    #region Set empty
                    txtPalletNo.Text = string.Empty;
                    txtPalletID.Text = string.Empty;
                    txtPalletMode.Text = string.Empty;
                    txtPalletDataRequest.Text = string.Empty;

                    btnPalletControl.Enabled = false;
                    #endregion

                    return;
                }

                #region Update Pallet Info
                // Pallet ID
                if (txtPalletID.Text != CurPallet.PalletID.ToString()) txtPalletID.Text = CurPallet.PalletID.ToString();

                // Pallet Mode
                if (txtPalletMode.Text != CurPallet.PalletMode.ToString()) txtPalletMode.Text = CurPallet.PalletMode.ToString();

                //Pallet Request
                if (txtPalletDataRequest.Text != CurPallet.PalletDataRequest.ToString()) txtPalletDataRequest.Text = CurPallet.PalletDataRequest ? "ON" : "OFF";
                #endregion

                if (CurPallet.PalletDataRequest)
                {
                    btnPalletControl.BackColor = (btnPalletControl.BackColor == Color.Green ? Color.DimGray : Color.Green);
                    if (!btnPalletControl.Enabled) btnPalletControl.Enabled = true;
                }
                else
                {
                    btnPalletControl.BackColor = Color.DimGray;
                    if (btnPalletControl.Enabled) btnPalletControl.Enabled = false;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void DataSetting_Unit()
        {
            try
            {
                if (CurUnit != null)
                {
                    if (txtUnitStatus.Text != CurUnit.UnitStatus.ToString()) txtUnitStatus.Text = CurUnit.UnitStatus.ToString();

                    if (txtUnitCFCount.Text != CurUnit.CFJobCount.ToString()) txtUnitCFCount.Text = CurUnit.CFJobCount.ToString();

                    if (txtUnitTFTCount.Text != CurUnit.TFTJobCount.ToString()) txtUnitTFTCount.Text = CurUnit.TFTJobCount.ToString();

                    if (txtUnitRunMode.Text != CurUnit.UnitRunMode.ToString()) txtUnitRunMode.Text = CurUnit.UnitRunMode.ToString();

                    #region Cell Buffer
                    if (CurUnit.UnitType == "CELL_BUFFER")
                    {
                        if (txtBF_WarningCount.Text != CurUnit.BufferWarningCount.ToString()) txtBF_WarningCount.Text = CurUnit.BufferWarningCount.ToString();

                        if (txtBF_CurrentCount.Text != CurUnit.BufferCurrentCount.ToString()) txtBF_CurrentCount.Text = CurUnit.BufferCurrentCount.ToString();

                        if (txtBF_SlotCount.Text != CurUnit.BufferTotalSlotCount.ToString()) txtBF_SlotCount.Text = CurUnit.BufferTotalSlotCount.ToString();

                        //1：Set ; 2：Reset
                        if (CurUnit.BufferWarningStatus == 1) txtBF_WarnStatus.Text = "1:Set";
                        else txtBF_WarnStatus.Text = "2:ReSet";

                        //1：Set ; 2：Reset
                        if (CurUnit.BufferStoreOverAlive == 1) txtBF_OverAlive.Text = "1:Set";
                        else txtBF_OverAlive.Text = "2:ReSet";
                    }
                    #endregion
                }
                else
                {
                    #region Set empty
                    txUnitID.Text = "";
                    txtUnitNo.Text = "";
                    txtUnitCFCount.Text = "0";                    
                    txtUnitStatus.Text = "";
                    txtUnitTFTCount.Text = "0";
                    txtUnitRunMode.Text = string.Empty;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void DataSetting_VCR()
        {
            try
            {
                if (CurVCR != null)
                {
                    if (txtVCRStatus.Text != CurVCR.Status.ToString()) txtVCRStatus.Text = CurVCR.Status.ToString();
                }
                else
                {
                    #region Set empty
                    txtVCRStatus.Text = string.Empty;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void HandleControl(string NodeNo)
        {
            if (hideContrlNames != null && hideContrlNames.Count > 0)//先將前次隱藏的顯示
            {
                foreach (string ctrlName in hideContrlNames)
                {
                    Control[] ctrls = this.Controls.Find(ctrlName, true);
                    if (ctrls.Length > 0) ctrls[0].Visible = true;
                }
                hideContrlNames.Clear();
            }
            //List<SBRM_OPI_OBJECT_DEF> hideCtrls = OPIAp.Dic_Node[NodeNo].HideContrls;
            //foreach (SBRM_OPI_OBJECT_DEF hideCtrl in hideCtrls)
            //{
            //    hideContrlNames.Add(hideCtrl.OBJECTNAME);//將此次要隱藏的記錄下來，以便下次顯示
            //    Control[] ctrls = this.Controls.Find(hideCtrl.OBJECTNAME, true);
            //    if (ctrls.Length > 0) ctrls[0].Visible = false;
            //}
        }

        private void Refresh_LayoutInfo()
        {
            int _num = 0;
            string _localNo = string.Empty;
            string _unitNo = string.Empty;
            string _itemType = string.Empty;
            string _itemNo = string.Empty;
            string _itemKey = string.Empty;
            string materialstatus1 = string.Empty; //Abb By Hujunpeng 20180904
            string materialvalue1 = string.Empty;
            string materialstatus2 = string.Empty;
            string materialvalue2 = string.Empty;
            Node _node = null;
            Unit _unit = null;
            Control _ctrl = null;
            int _totalCnt = 0;
            int count = 0;//add by hujunpeng 20190723
            Count++;

            try
            {
                foreach (ucEQ _eq in pnlLayout.Controls.OfType<ucEQ>())
                {
                    int.TryParse(_eq.Name.Substring(1, 2), out _num);

                    _localNo = string.Format("L{0}", _num.ToString());

                    if (OPIAp.Dic_Node.ContainsKey(_localNo) == false) continue;

                    _node = OPIAp.Dic_Node[_localNo];


                    #region EQP Information
                    #region EQP Status

                    _ctrl = _eq.Controls["EQSTATUS"];

                    if (_ctrl != null)
                    {
                        EQPStatusColor(_ctrl, _node.EQPStatus);

                        // 學名+俗名+Status(Run，Idle，STOP，Pause)
                        Tip.SetToolTip(_ctrl, _node.NodeName + "-" + _node.NodeID + "-" + _node.EQPStatus.ToString());
                    }

                    #region 非unit 的picture背景與EQP Status相同
                    foreach (csPictureBox _picObj in _eq.Controls.OfType<csPictureBox>())
                    {
                        //若為Unit 編碼為九碼 EX:UNXX:RDXX
                        if (_picObj.Name.Length < 9)
                        {
                            EQPStatusColor(_picObj, _node.EQPStatus);
                        }
                    }
                    #endregion

                    #endregion

                    #region TotalCount
                    _totalCnt = (_node.TFTJobCount + _node.CFJobCount + _node.DummyJobCount + _node.ThroughDummyJobCount + _node.ThicknessDummyJobCount + _node.UVMASKJobCount +
                       _node.UnassembledTFTJobCount + _node.ITODummyJobCount + _node.NIPDummyJobCount + _node.MetalOneDummyJobCount);//sy add 20160826
                    foreach (Port _port in OPIAp.Dic_Port.Values)
                    {
                        if (_port.NodeNo == _localNo) _totalCnt = _totalCnt + _port.PortGlassCount;
                    }

                    _node.TotalCount = _totalCnt;

                    _ctrl = _eq.Controls["TotalCount"];

                    if (_ctrl != null) ((csLabel)_ctrl).Text = _node.TotalCount.ToString();

                    #endregion

                    #region[Add By Yangzhenteng For OPI Display 20180904]
                    #region[For PI Material Real Report] Add By Yangzhenteng For PI OPI Display 20180904
                    if ((txtLineID.Text == "CCPIL100") || (txtLineID.Text == "CCPIL200"))
                    {
                        progressBar3.Value = (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK01Weight.ToString())) > 3500 ? 3500 : (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK01Weight.ToString())); //Yangzhenteng
                        progressBar4.Value = (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK02Weight.ToString())) > 3500 ? 3500 : (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK02Weight.ToString())); //Yangzhenteng
                        progressBar5.Value = (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK03Weight.ToString())) > 3500 ? 3500 : (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK03Weight.ToString())); //Yangzhenteng
                        progressBar6.Value = (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK04Weight.ToString())) > 3500 ? 3500 : (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK04Weight.ToString())); //Yangzhenteng
                        progressBar7.Value = (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK05Weight.ToString())) > 3500 ? 3500 : (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK05Weight.ToString())); //Yangzhenteng
                        progressBar8.Value = (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK06Weight.ToString())) > 3500 ? 3500 : (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK06Weight.ToString())); //Yangzhenteng
                        progressBar9.Value = (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK07Weight.ToString())) > 3500 ? 3500 : (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK07Weight.ToString())); //Yangzhenteng
                        progressBar10.Value = (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK08Weight.ToString())) > 3500 ? 3500 : (int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK08Weight.ToString()));//Yangzhenteng
                        //ToolTip Tip = new ToolTip();
                        Tip.SetToolTip(progressBar3, string.Format("MaterialID:{0},MaterialWeight:{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK01ID.ToString(), int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK01Weight.ToString())));
                        Tip.SetToolTip(progressBar4, string.Format("MaterialID:{0},MaterialWeight:{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK02ID.ToString(), int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK02Weight.ToString())));
                        Tip.SetToolTip(progressBar5, string.Format("MaterialID:{0},MaterialWeight:{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK03ID.ToString(), int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK03Weight.ToString())));
                        Tip.SetToolTip(progressBar6, string.Format("MaterialID:{0},MaterialWeight:{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK04ID.ToString(), int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK04Weight.ToString())));
                        Tip.SetToolTip(progressBar7, string.Format("MaterialID:{0},MaterialWeight:{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK05ID.ToString(), int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK05Weight.ToString())));
                        Tip.SetToolTip(progressBar8, string.Format("MaterialID:{0},MaterialWeight:{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK06ID.ToString(), int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK06Weight.ToString())));
                        Tip.SetToolTip(progressBar9, string.Format("MaterialID:{0},MaterialWeight:{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK07ID.ToString(), int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK07Weight.ToString())));
                        Tip.SetToolTip(progressBar10, string.Format("MaterialID:{0},MaterialWeight:{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK08ID.ToString(),int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialTK08Weight.ToString())));
                    }
                    #endregion

                    #region CF Materialinfo //Add By Hujunpeng 20180904
                    if ((txtLineID.Text == "FCBPH100") || (txtLineID.Text == "FCGPH100") || (txtLineID.Text == "FCMPH100") || (txtLineID.Text == "FCOPH100") || (txtLineID.Text == "FCRPH100") || (txtLineID.Text == "FCSPH100"))
                    {
                        int num = 0;
                        if (!string.IsNullOrEmpty(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF01Weight))
                        {
                            if (int.TryParse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF01Weight, out num))
                            {
                                if (progressBar1.Value != int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF01Weight))
                                {
                                    progressBar1.Value = int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF01Weight);
                                }
                                if (label8.Text != string.Format("{0},{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF01Status, progressBar1.Value.ToString()))
                                {
                                    label8.Text = string.Format("{0},{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF01Status, progressBar1.Value.ToString());
                                }

                                if (label18.Text != FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF01ID)
                                {
                                    label18.Text = FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF01ID;
                                }
                            }                           
                        }
                        if (!string.IsNullOrEmpty(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF02Weight))
                        {
                            if (int.TryParse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF02Weight, out num))
                            {
                                if (progressBar2.Value != int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF02Weight))
                                {
                                    progressBar2.Value = int.Parse(FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF02Weight);
                                }
                                if (label9.Text != string.Format("{0},{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF02Status, progressBar2.Value.ToString()))
                                {
                                    label9.Text = string.Format("{0},{1}", FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF02Status, progressBar2.Value.ToString());
                                }

                                if (label19.Text != FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF02ID)
                                {
                                    label19.Text = FormMainMDI.G_OPIAp.CurMaterialRealWeight.MaterialForCF02ID;
                                }
                            }                           
                        }
                        string[] str = label8.Text.Split(',');
                        if (str.Length > 1)
                        {
                            foreach (string item in str)
                            {
                                materialstatus1 = str[0];
                                materialvalue1 = str[1];
                            }
                        }

                        string[] str1 = label9.Text.Split(',');
                        if (str1.Length > 1)
                        {
                            foreach (string item in str)
                            {
                                materialstatus2 = str1[0];
                                materialvalue2 = str1[1];
                            }
                        }
                        Tip.SetToolTip(progressBar1, string.Format("MaterialID:{0},MaterialWeight:{1},MaterialStatus:{2}", label18.Text, materialvalue1, materialstatus1));
                        Tip.SetToolTip(progressBar2, string.Format("MaterialID:{0},MaterialWeight:{1},MaterialStatus:{2}", label19.Text, materialvalue2, materialstatus2));

                    }
                    #endregion

                    #region For PI Job Count Monitor
                    //add by hujunpeng 20190723
                    if ((txtLineID.Text == "CCPIL100") || (txtLineID.Text == "CCPIL200"))
                    {
                        if (Count == 10)
                        {
                            count++;
                            if (count == 24)
                            {
                                #region count显示
                                if (!string.IsNullOrEmpty(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner03))
                                {
                                    treeViewPIJobCount.BeginUpdate();
                                    treeViewPIJobCount.Nodes.Clear();
                                    TreeNode tn1 = treeViewPIJobCount.Nodes.Add(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner01);
                                    TreeNode tn2 = treeViewPIJobCount.Nodes.Add(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner02);
                                    TreeNode tn3 = treeViewPIJobCount.Nodes.Add(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner03);
                                    TreeNode Ntn1 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.TFTCount01.ToString());
                                    TreeNode Ntn2 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.TFTCount02.ToString());
                                    TreeNode Ntn3 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.TFTCount03.ToString());
                                    TreeNode Ntn4 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.CFCount01.ToString());
                                    TreeNode Ntn5 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.CFCount02.ToString());
                                    TreeNode Ntn6 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.CFCount03.ToString());
                                    tn1.Nodes.Add(Ntn1);
                                    tn1.Nodes.Add(Ntn4);
                                    tn2.Nodes.Add(Ntn2);
                                    tn2.Nodes.Add(Ntn5);
                                    tn3.Nodes.Add(Ntn3);
                                    tn3.Nodes.Add(Ntn6);
                                    treeViewPIJobCount.EndUpdate();
                                }
                                else if (!string.IsNullOrEmpty(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner02) && string.IsNullOrEmpty(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner03))
                                {
                                    treeViewPIJobCount.BeginUpdate();
                                    treeViewPIJobCount.Nodes.Clear();
                                    TreeNode tn1 = treeViewPIJobCount.Nodes.Add(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner01);
                                    TreeNode tn2 = treeViewPIJobCount.Nodes.Add(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner02);
                                    TreeNode Ntn1 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.TFTCount01.ToString());
                                    TreeNode Ntn2 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.TFTCount02.ToString());
                                    TreeNode Ntn4 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.CFCount01.ToString());
                                    TreeNode Ntn5 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.CFCount02.ToString());
                                    tn1.Nodes.Add(Ntn1);
                                    tn1.Nodes.Add(Ntn4);
                                    tn2.Nodes.Add(Ntn2);
                                    tn2.Nodes.Add(Ntn5);
                                    treeViewPIJobCount.EndUpdate();
                                }
                                else if (!string.IsNullOrEmpty(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner01) && string.IsNullOrEmpty(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner02) && string.IsNullOrEmpty(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner03))
                                {
                                    treeViewPIJobCount.BeginUpdate();
                                    treeViewPIJobCount.Nodes.Clear();
                                    TreeNode tn1 = treeViewPIJobCount.Nodes.Add(FormMainMDI.G_OPIAp.PIJobCount.ProductGroupOwner01);
                                    TreeNode Ntn1 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.TFTCount01.ToString());
                                    TreeNode Ntn4 = new TreeNode(FormMainMDI.G_OPIAp.PIJobCount.CFCount01.ToString());
                                    tn1.Nodes.Add(Ntn1);
                                    tn1.Nodes.Add(Ntn4);
                                    treeViewPIJobCount.EndUpdate();
                                }
                                else
                                {
                                    treeViewPIJobCount.Nodes.Clear();
                                }
                                for (int i = treeViewPIJobCount.GetNodeCount(false) - 1; i > -1; i--)
                                {
                                    treeViewPIJobCount.SelectedNode = treeViewPIJobCount.Nodes[i];
                                    treeViewPIJobCount.SelectedNode.ExpandAll();
                                }
                                count = 0;
                                Count = 0;
                                #endregion
                            }
                        }


                    }
                    #endregion

                    #endregion
                    #region CIM Mode

                    _ctrl = _eq.Controls["NodeName"];

                    if (_ctrl != null)
                    {
                        _ctrl.BackColor = (_node.CIMMode == eCIMMode.ON ? Color.LawnGreen : Color.Red);
                    }
                    #endregion

                    #region Alarm

                    if (_node.AlarmStatus)
                    {
                        _eq.BackColor = _eq.BackColor == Color.Red ? Color.DarkGray : Color.Red;
                        _ctrl = _eq.Controls["EQSTATUS"];//Modified By zhangwei CF 厂增加Warning状态显示
                        if ((OPIAp.CurLine.FabType == "CF")||
                            (OPIAp.CurLine.LineType == "PIL" || OPIAp.CurLine.LineType == "PIL_2" || OPIAp.CurLine.LineType == "ODF" ||
                             OPIAp.CurLine.LineType == "ODF_2" || OPIAp.CurLine.LineType == "PCS" || OPIAp.CurLine.LineType == "GAP" ||
                             OPIAp.CurLine.LineType == "PTH" || OPIAp.CurLine.LineType == "PDR" || OPIAp.CurLine.LineType == "TAM")                                                                                 
                            ) //Modify By yangzhenteng For Cell FEOL,增加Warning状态显示;
                        {
                            if (_node.EQPStatus == eEQPStatus.Idle || _node.EQPStatus == eEQPStatus.Run)
                                _ctrl.BackColor = Color.Fuchsia;
                        }
                    }
                    else
                    {
                        if (_eq.BackColor != Color.DarkGray) _eq.BackColor = Color.DarkGray;
                    }

                    #endregion

                        #region Port Type
                        foreach (Port _port in FormMainMDI.G_OPIAp.Dic_Port.Values)
                        {
                            string _ctrlName = "PT" + _port.PortNo.PadLeft(2, '0');
                            _ctrl = _eq.Controls[_ctrlName];
                            if (_ctrl != null)
                            {
                                PortType(_ctrl, _port.PortType);
                                EQPStatusColor(_ctrl, _node.EQPStatus);
                            }
                        }
                        #endregion

                    #endregion

                        #region ucEQP 內的物件處理  - Unit & Port & VCR
                        foreach (Control _subCtrl in _eq.Controls)
                        {
                            switch (_subCtrl.GetType().Name.ToString())
                            {
                                case "csPictureBox":

                                    #region csPictureBox
                                    csPictureBox _pic = (csPictureBox)_subCtrl;

                                    SetPictureImage(_node, _pic);

                                    foreach (csPictureBox _subPic in _pic.Controls.OfType<csPictureBox>())
                                    {
                                        SetPictureImage(_node, _subPic);
                                    }
                                    #endregion
                                    break;

                                case "csLabel":

                                    #region Unit
                                    csLabel _lbl = (csLabel)_subCtrl;

                                    if (_lbl.PropertyData.myID.Length < 4) continue;

                                    _itemType = _lbl.PropertyData.myID.Substring(0, 2);  //UN...
                                    _itemNo = _lbl.PropertyData.myID.Substring(2, 2);

                                    int.TryParse(_itemNo, out _num);

                                    if (_itemType == "UN")
                                    {
                                        #region UNXX
                                        if (_num < 50)
                                        {
                                            #region unit 的csLabel背景顯示
                                            _itemKey = _node.NodeNo.PadRight(3, ' ') + _itemNo;
                                            if (OPIAp.Dic_Unit.ContainsKey(_itemKey))
                                            {
                                                _unit = OPIAp.Dic_Unit[_itemKey];

                                                if (_unit.UnitType == "CELL_BUFFER")
                                                {
                                                    if (_unit.BufferWarningStatus == 1 || _unit.BufferStoreOverAlive == 1)
                                                    {
                                                        Label _lblTmp = new Label();
                                                        EQPStatusColor(_lblTmp, _unit.UnitStatus);
                                                        Color _color = _lblTmp.BackColor;

                                                        _lbl.BackColor = _lbl.BackColor == Color.Silver ? _color : Color.Silver;
                                                    }
                                                    else EQPStatusColor(_lbl, _unit.UnitStatus);
                                                }
                                                else
                                                {
                                                    EQPStatusColor(_lbl, _unit.UnitStatus);
                                                }

                                                Tip.SetToolTip(_lbl, string.Format("U{0}-{1}", _unit.UnitNo.PadLeft(2, '0'), _unit.UnitStatus.ToString()));
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region 非unit 的csLabel背景與EQP Status相同
                                            EQPStatusColor(_lbl, _node.EQPStatus);

                                            // 學名+俗名+Status(Run，Idle，STOP，Pause)
                                            Tip.SetToolTip(_lbl, _node.NodeName + "-" + _node.NodeID + "-" + _node.EQPStatus.ToString());
                                            #endregion
                                        }
                                        #endregion
                                    }
                                    else if (_itemType == "MS")
                                    {
                                        #region Material Status
                                        if (_node.MaterialStatus)
                                        {
                                            EQPStatusColor(_lbl, _node.EQPStatus);
                                            _lbl.ForeColor = _lbl.ForeColor == Color.Red ? Color.Black : Color.Red;
                                            _lbl.Visible = true;
                                        }
                                        else _lbl.Visible = false;
                                        #endregion
                                    }
                                    #endregion

                                    break;

                                default: break;
                            }
                        }
                        #endregion
                    }

                    foreach (csLabel _lbl in pnlLayout.Controls.OfType<csLabel>())
                    {
                        // EX: SC0200
                        //OPI Layout畫面上顯示SECS的連線狀態
                        _localNo = _lbl.Name.Substring(2, 2);
                        _unitNo = _lbl.Name.Substring(4, 2);

                        int.TryParse(_localNo, out _num);

                        if (OPIAp.Dic_Node.ContainsKey(string.Format("L{0}", _num.ToString())) == false) continue;

                        _node = OPIAp.Dic_Node[string.Format("L{0}", _num.ToString())];
                        _unit = null;

                        if (_unitNo != "00")
                        {
                            _itemKey = _node.NodeNo.PadRight(3, ' ') + _unitNo;

                            if (OPIAp.Dic_Unit.ContainsKey(_itemKey))
                            {
                                _unit = OPIAp.Dic_Unit[_itemKey];
                            }
                            else continue;
                        }

                        switch (_lbl.Name.Substring(0, 2))
                        {
                            case "SC":

                                #region SECS Connection Status
                                if (_unitNo == "00")
                                {
                                    if (_node.HSMSStatus != null && _node.HSMSStatus == "CONNECTED")
                                    {
                                        _lbl.Visible = false;
                                    }
                                    else
                                    {
                                        _lbl.BackColor = _lbl.BackColor == Color.Red ? Color.Silver : Color.Red;
                                        _lbl.Visible = true;
                                    }
                                }
                                else
                                {
                                    if (_unit.HSMSStatus != null && _unit.HSMSStatus == "CONNECTED")
                                    {
                                        _lbl.Visible = false;
                                    }
                                    else
                                    {
                                        _lbl.BackColor = _lbl.BackColor == Color.Red ? Color.Silver : Color.Red;
                                        _lbl.Visible = true;
                                    }
                                }
                                break;
                                #endregion

                            default:
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

        private void SetPictureImage(Node _node,csPictureBox _pic)
        {
            try
            {
                #region csPictureBox

                string _itemType = _pic.PropertyData.myID.Substring(0, 2);  //UN,RD,TO...
                string _itemNo = _pic.PropertyData.myID.Substring(2, 2);
                string _itemKey = string.Empty;

                if (_pic.PropertyData.myID.Trim().Length == 4)
                {
                    switch (_itemType)
                    {
                        case "VR":

                            #region VCR
                            VCR _vcr = _node.VCRs.Find(d => d.VCRNO.Equals(_itemNo));

                            if (_vcr != null)
                            {
                                if (_vcr.Status == eVCRMode.ENABLE)
                                {
                                    if (_pic.BackgroundImage != UniOPI.Properties.Resources.Layout_VCROn) _pic.BackgroundImage = UniOPI.Properties.Resources.Layout_VCROn;
                                }
                                else
                                {
                                    if (_pic.BackgroundImage != UniOPI.Properties.Resources.Layout_VCROff)  _pic.BackgroundImage = UniOPI.Properties.Resources.Layout_VCROff;
                                }
                            }

                            break;

                            #endregion
                            
                        case "TF":

                            #region  Stage Turn Fix
                            //if (_node.TURNTABLEMODE != null && _node.TURNTABLEMODE == "1")
                            //{
                            //    if (_pic.BackgroundImage != UniOPI.Properties.Resources.Layout_StageFixed)
                            //        _pic.BackgroundImage = UniOPI.Properties.Resources.Layout_StageFixed;
                            //}
                            //else
                            //{
                            //    if (_pic.BackgroundImage != UniOPI.Properties.Resources.Layout_StageFixed)
                            //        _pic.BackgroundImage = UniOPI.Properties.Resources.Layout_StageFixed;
                            //}

                            break;

                            #endregion
                            
                        case "TT":

                            #region  Stage Turn Table
                            if ( _node.TurnTableMode)
                            {
                                if (_pic.BackgroundImage != UniOPI.Properties.Resources.Layout_StageTurn_ON)
                                    _pic.BackgroundImage = UniOPI.Properties.Resources.Layout_StageTurn_ON;
                            }
                            else
                            {
                                if (_pic.BackgroundImage != UniOPI.Properties.Resources.Layout_StageTurn_OFF)
                                    _pic.BackgroundImage = UniOPI.Properties.Resources.Layout_StageTurn_OFF;
                            }
                            break;

                            #endregion
                            
                        case "TO":

                            #region  Stage Turn Over
                            if ( _node.TurnTableMode)
                            {
                                if (_pic.BackgroundImage != UniOPI.Properties.Resources.Layout_StageTurnOver_ON)
                                    _pic.BackgroundImage = UniOPI.Properties.Resources.Layout_StageTurnOver_ON;
                            }
                            else
                            {
                                if (_pic.BackgroundImage != UniOPI.Properties.Resources.Layout_StageTurnOver_OFF)
                                    _pic.BackgroundImage = UniOPI.Properties.Resources.Layout_StageTurnOver_OFF;
                            }

                            break;

                            #endregion
                            
                        case "CN":

                            #region Normal Cassette

                            _itemKey = _node.NodeNo.PadRight(3, ' ') + _itemNo;

                            if (OPIAp.Dic_Port.ContainsKey(_itemKey))
                            {
                                Port _port = OPIAp.Dic_Port[_itemKey];

                                switch (_port.PortStatus)
                                {
                                    case ePortStatus.LoadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTNormal_LR;
                                        break;

                                    case ePortStatus.LoadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTNormal_LC;
                                        break;

                                    case ePortStatus.UnloadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTNormal_UR;
                                        break;

                                    case ePortStatus.UnloadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTNormal_UC;
                                        break;

                                    default:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTNormal_NONE;
                                        break;
                                }
                            }

                            break;

                            #endregion
                           
                        case "CS": //Scrap
                        case "CC": // Cell Cassette

                            #region Cell Cassette & Scrap Cassette
                            _itemKey = _node.NodeNo.PadRight(3, ' ') + _itemNo;

                            if (OPIAp.Dic_Port.ContainsKey(_itemKey))
                            {
                                Port _port = OPIAp.Dic_Port[_itemKey];

                                switch (_port.PortStatus)
                                {
                                    case ePortStatus.LoadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTCell_LR;
                                        break;

                                    case ePortStatus.LoadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTCell_LC;
                                        break;

                                    case ePortStatus.UnloadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTCell_UR;
                                        break;

                                    case ePortStatus.UnloadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTCell_UC;
                                        break;

                                    default:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTCell_NONE;
                                        break;
                                }
                            }

                            break;

                            #endregion                           

                        case "CW": // Wire Cassette

                            #region Wire Cassette
                            _itemKey = _node.NodeNo.PadRight(3, ' ') + _itemNo;

                            if (OPIAp.Dic_Port.ContainsKey(_itemKey))
                            {
                                Port _port = OPIAp.Dic_Port[_itemKey];

                                switch (_port.PortStatus)
                                {
                                    case ePortStatus.LoadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTWire_LR;
                                        break;

                                    case ePortStatus.LoadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTWire_LC;
                                        break;

                                    case ePortStatus.UnloadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTWire_UR;
                                        break;

                                    case ePortStatus.UnloadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTWire_UC;
                                        break;

                                    default:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTWire_NONE;
                                        break;
                                }
                            }

                            break;

                            #endregion
                           
                        case "DS":  //Dense Box

                            #region Dense Box
                            _itemKey = _node.NodeNo.PadRight(3, ' ') + _itemNo;

                            if (OPIAp.Dic_Port.ContainsKey(_itemKey))
                            {
                                Port _port = OPIAp.Dic_Port[_itemKey];

                                switch (_port.PortStatus)
                                {
                                    case ePortStatus.LoadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.Dense_LR;
                                        break;

                                    case ePortStatus.LoadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.Dense_LC;
                                        break;

                                    case ePortStatus.UnloadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.Dense_UR;
                                        break;

                                    case ePortStatus.UnloadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.Dense_UC;
                                        break;

                                    default:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.Dense_NONE;
                                        break;
                                }
                            }

                            break;

                            #endregion
                          
                        case "BF": //Buffer有下貨功能
                        case "BW": //Wire Buffer 有下貨功能

                            #region Buffer Cassette
                            _itemKey = _node.NodeNo.PadRight(3, ' ') + _itemNo;

                            if (OPIAp.Dic_Port.ContainsKey(_itemKey))
                            {
                                Port _port = OPIAp.Dic_Port[_itemKey];

                                switch (_port.PortStatus)
                                {
                                    case ePortStatus.LoadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTBuffer_LR;
                                        break;

                                    case ePortStatus.LoadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTBuffer_LC;
                                        break;

                                    case ePortStatus.UnloadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTBuffer_UR;
                                        break;

                                    case ePortStatus.UnloadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTBuffer_UC;
                                        break;

                                    default:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTBuffer_NONE;
                                        break;
                                }

                            }

                            break;

                            #endregion                            

                        case "MP": //M Port

                            #region M Port

                            _itemKey = _node.NodeNo.PadRight(3, ' ') + _itemNo;

                            if (OPIAp.Dic_Port.ContainsKey(_itemKey))
                            {
                                Port _port = OPIAp.Dic_Port[_itemKey];

                                switch (_port.PortStatus)
                                {
                                    case ePortStatus.LoadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTMPortr_LR;
                                        break;

                                    case ePortStatus.LoadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTMPortr_LC;
                                        break;

                                    case ePortStatus.UnloadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTMPortr_UR;
                                        break;

                                    case ePortStatus.UnloadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTMPortr_UC;
                                        break;

                                    default:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTMPortr_NONE;
                                        break;
                                }
                            }

                            break;

                            #endregion

                        case "TP": //Tray Port

                            #region Tray Port

                            _itemKey = _node.NodeNo.PadRight(3, ' ') + _itemNo;

                            if (OPIAp.Dic_Port.ContainsKey(_itemKey))
                            {
                                Port _port = OPIAp.Dic_Port[_itemKey];

                                switch (_port.PortStatus)
                                {
                                    case ePortStatus.LoadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTTray_LR;
                                        break;

                                    case ePortStatus.LoadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTTray_LC;
                                        break;

                                    case ePortStatus.UnloadRequest:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTTray_UR;
                                        break;

                                    case ePortStatus.UnloadComplete:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTTray_UC;
                                        break;

                                    default:
                                        _pic.BackgroundImage = UniOPI.Properties.Resources.CSTTray_NONE;
                                        break;
                                }
                            }

                            break;

                            #endregion

                        default:
                            break;
                    }
                }
                else if (_pic.PropertyData.myID.Trim().Length == 9)
                {
                    #region Unit
                    if (_itemType == "UN")
                    {
                        _itemKey = _node.NodeNo.PadRight(3, ' ') + _itemNo;
                        if (OPIAp.Dic_Unit.ContainsKey(_itemKey))
                        {
                            Unit _unit = OPIAp.Dic_Unit[_itemKey];

                            EQPStatusColor(_pic, _unit.UnitStatus);

                            Tip.SetToolTip(_pic, string.Format("U{0}-{1}", _unit.UnitNo.PadLeft(2, '0'), _unit.UnitStatus.ToString()));
                        }
                    }
                    #endregion
                }

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void PortType(Control ctrObject, ePortType PortType)
        {
            try
            {
                switch (PortType)
                {
                    case ePortType.LoadingPort:

                        if (ctrObject != null) ctrObject.Text = "LD";

                        break;

                    case ePortType.UnloadingPort:

                        if (ctrObject != null) ctrObject.Text = "UD";

                        break;

                    case ePortType.BothPort:

                        if (ctrObject != null) ctrObject.Text = "BU";

                        break;

                    case ePortType.BufferType:
                    case ePortType.LoaderinBufferType:
                    case ePortType.UnloaderinBufferType:

                        if (ctrObject != null) ctrObject.Text = "BF";

                        break;

                    default:

                        if (ctrObject != null) ctrObject.Text = "UN";
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void EQPStatusColor(Control ctrObject, eEQPStatus CurrentStatus)
        {
            try
            {
                switch (CurrentStatus)
                {
                    case eEQPStatus.Idle:

                        if (ctrObject.BackColor != Color.Gold) ctrObject.BackColor = Color.Gold;

                        break;

                    case eEQPStatus.Pause:

                        if (ctrObject.BackColor != Color.Cyan) ctrObject.BackColor = Color.Cyan;
                        
                        break;

                    case eEQPStatus.Run:

                        if (ctrObject.BackColor != Color.YellowGreen) ctrObject.BackColor = Color.YellowGreen;
                        
                        break;

                    case eEQPStatus.Setup:

                        if (ctrObject.BackColor != Color.MediumPurple) ctrObject.BackColor = Color.MediumPurple;
                        
                        break;

                    case eEQPStatus.Stop:

                        if (ctrObject.BackColor != Color.IndianRed) ctrObject.BackColor = Color.IndianRed;
                        
                        break;

                    default:

                        if (ctrObject.BackColor != Color.Gray) ctrObject.BackColor = Color.Gray;
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex); 
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void dgvJobData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvJobData.CurrentRow != null)
                {
                    if (dgvJobData.Columns[e.ColumnIndex].Name == "colDetail")
                    {
                        #region FormJobDataDetail
                        string _cstSeqNo = dgvJobData.CurrentRow.Cells[colCstSeqNo.Name].Value.ToString();
                        string _jobSeqNo = dgvJobData.CurrentRow.Cells[colJobSeqNo.Name].Value.ToString();
                        string _glassID = dgvJobData.CurrentRow.Cells[colGlassID.Name].Value.ToString();

                        if (_glassID == string.Empty && _cstSeqNo == string.Empty && _jobSeqNo == string.Empty)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Cassette Seq No、Job Seq No、Job ID must be Required！", MessageBoxIcon.Warning);
                            return;
                        }

                        FormJobDataDetail _frm = new FormJobDataDetail(_glassID, _cstSeqNo, _jobSeqNo);
                        _frm.TopMost = true;
                        _frm.ShowDialog(this);

                        _frm.Dispose();
                        #endregion
                    }
                    else if (dgvJobData.Columns[e.ColumnIndex].Name == "colRoute")
                    {
                        #region FormRBRouteInfo
                        string _cstSeqNo = dgvJobData.CurrentRow.Cells[colCstSeqNo.Name].Value.ToString();
                        string _jobSeqNo = dgvJobData.CurrentRow.Cells[colJobSeqNo.Name].Value.ToString();

                        if (_cstSeqNo == string.Empty || _jobSeqNo == string.Empty)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Cassette Seq No and Job Seq No must be Required！", MessageBoxIcon.Warning);
                            return;
                        }

                        FormRBRouteInfo _frm = new FormRBRouteInfo(_cstSeqNo, _jobSeqNo){ TopMost = true } ;
                        _frm.ShowDialog();
                        if (_frm != null) _frm.Dispose();
                        #endregion
                    }
                    else if (dgvJobData.Columns[e.ColumnIndex].Name == "colStopReason")
                    {
                        #region FormRobotStopReason
                        string _cstSeqNo = dgvJobData.CurrentRow.Cells[colCstSeqNo.Name].Value.ToString();
                        string _jobSeqNo = dgvJobData.CurrentRow.Cells[colJobSeqNo.Name].Value.ToString();

                        if (_cstSeqNo == string.Empty || _jobSeqNo == string.Empty)
                        {
                            ShowMessage(this, this.lblCaption.Text, "", "Cassette Seq No and Job Seq No must be Required！", MessageBoxIcon.Warning);
                            return;
                        }

                        FormRobotStopReason _frm = new FormRobotStopReason(_cstSeqNo, _jobSeqNo) { TopMost = true };
                        _frm.ShowDialog();
                        if (_frm != null) _frm.Dispose();
                        #endregion
                    }

                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                string CurPortMESMode = string.Empty;

                #region Check Data
                if (CurPort.PortGlassCount > CurPort.MaxCount)
                {
                    ShowMessage(this, lblCaption.Text , "", string.Format("Port Glass Count [{0}] > Port Max Count [{1}]",CurPort.PortGlassCount.ToString(), CurPort.MaxCount.ToString()), MessageBoxIcon.Error);
                    return;
                }

                if (CurPort.LineID == OPIAp.CurLine.LineID)
                {
                    if (OPIAp.CurLine.MesControlMode == null)
                    {
                        ShowMessage(this,lblCaption.Text , "", "MES Control is null", MessageBoxIcon.Error);
                        return;
                    }

                    CurPortMESMode = OPIAp.CurLine.MesControlMode;
                }
                else if (CurPort.LineID == OPIAp.CurLine.LineID2)
                {
                    if (OPIAp.CurLine.MesControlMode2 == null)
                    {
                        ShowMessage(this, lblCaption.Text, "", "MES Control is null", MessageBoxIcon.Error);
                        return;
                    }

                    CurPortMESMode = OPIAp.CurLine.MesControlMode2;
                }
                else
                {
                    CurPortMESMode = OPIAp.CurLine.MesControlMode;
                }
                #endregion

                if (CurPortMESMode.ToUpper() == "OFFLINE")
                {
                    //if (OPIAp.CurLine.LineType == "CBDPI")
                    //{
                    //    FormMainMDI.FrmCassetteControl_Offline_DPI = new FormCassetteControl_Offline_DPI();
                    //    FormMainMDI.FrmCassetteControl_Offline_DPI.MesControlMode = CurPortMESMode.ToUpper();
                    //    FormMainMDI.FrmCassetteControl_Offline_DPI.curPort = CurPort;

                    //    if (OPIAp.Dic_Port.ContainsKey(CurPort.NodeNo.PadRight(3, ' ') + CurPort.RelationPortNo) == false)
                    //    {
                    //        ShowMessage(this, lblCaption.Text , "", string.Format("Can't find Relation Port[{0}]",CurPort.RelationPortNo ), MessageBoxIcon.Error);
                    //        return;
                    //    }

                    //    FormMainMDI.FrmCassetteControl_Offline_DPI.curPort02 = OPIAp.Dic_Port[CurPort.NodeNo.PadRight(3, ' ') + CurPort.RelationPortNo];
                    //    FormMainMDI.FrmCassetteControl_Offline_DPI.ShowDialog();
                    //}
                    //else if (OPIAp.CurLine.LineType == "CBPRM" && CurPort.PortAttribute == "VIRTUAL")
                    //{
                    //    FormMainMDI.FrmCassetteControl_Offline_PRM = new FormCassetteControl_Offline_PRM();
                    //    FormMainMDI.FrmCassetteControl_Offline_PRM.curPort = CurPort;
                    //    FormMainMDI.FrmCassetteControl_Offline_PRM.MesControlMode = CurPortMESMode.ToUpper();
                    //    FormMainMDI.FrmCassetteControl_Offline_PRM.ShowDialog();
                    //}
                    //else
                    //{
                        FormMainMDI.FrmCassetteOperation_Offline = new FormCassetteControl_Offline();
                        FormMainMDI.FrmCassetteOperation_Offline.curPort = CurPort;
                        FormMainMDI.FrmCassetteOperation_Offline.MesControlMode = CurPortMESMode.ToUpper();
                        FormMainMDI.FrmCassetteOperation_Offline.ShowDialog();
                    //}
                }
                else if (CurPortMESMode.ToUpper() == "LOCAL")
                {
                    //if (OPIAp.CurLine.LineType == "CBDPI")
                    //{
                    //    FormMainMDI.FrmCassetteControl_Local_DPI = new FormCassetteControl_Local_DPI();
                    //    FormMainMDI.FrmCassetteControl_Local_DPI.MesControlMode = CurPortMESMode.ToUpper();
                    //    FormMainMDI.FrmCassetteControl_Local_DPI.curPort = CurPort;

                    //    if (OPIAp.Dic_Port.ContainsKey(CurPort.NodeNo.PadRight(3, ' ') + CurPort.RelationPortNo) == false)
                    //    {
                    //        ShowMessage(this, lblCaption.Text, "", string.Format("Can't find Relation Port[{0}]", CurPort.RelationPortNo), MessageBoxIcon.Error);
                    //        return;
                    //    }

                    //    FormMainMDI.FrmCassetteControl_Local_DPI.curPort02 = OPIAp.Dic_Port[CurPort.NodeNo.PadRight(3, ' ') + CurPort.RelationPortNo];
                    //    FormMainMDI.FrmCassetteControl_Local_DPI.ShowDialog();
                    //}
                    //else
                    //{
                        FormMainMDI.FrmCassetteOperation_Local = new FormCassetteControl_Local();
                        FormMainMDI.FrmCassetteOperation_Local.curPort = CurPort;
                        FormMainMDI.FrmCassetteOperation_Local.MesControlMode = CurPortMESMode.ToUpper();
                        FormMainMDI.FrmCassetteOperation_Local.ShowDialog();
                    //}
                }
                else
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("MES Mode [{0}] is error", CurPortMESMode), MessageBoxIcon.Error);
                    return;
                }
          
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void btnDenseControl_Click(object sender, EventArgs e)
        {
            try
            {
                if (OPIAp.CurLine.LineType == "PPK")
                {
                    #region PPK Line Check Box Type & Box ID & Paper Box ID
                    //in box => boix id 必填 
                    //out box => paper box id 必填 
                    if (CurDense.BoxType == eBoxType.Unknown)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Box Type is Unknown", MessageBoxIcon.Error);
                        return;
                    }
                    else if (CurDense.BoxType == eBoxType.InBox)
                    {
                        if (CurDense.BoxID01 == string.Empty)
                        {
                            ShowMessage(this, lblCaption.Text, "", "Box ID can't empty when Box Type is InBox", MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else if (CurDense.BoxType == eBoxType.OutBox)
                    {
                        if (CurDense.PaperBoxID == string.Empty)
                        {
                            ShowMessage(this, lblCaption.Text, "", "Paper Box ID can't empty when Box Type is OutBox", MessageBoxIcon.Error);
                            return;
                        }
                    }

                    #endregion

                    #region PPK Line map download

                    if (OPIAp.CurLine.MesControlMode.ToUpper() == "OFFLINE")
                    {
                        FormMainMDI.FrmDenseControl_Offline_PPK = new FormDenseControl_Offline_PPK();
                        FormMainMDI.FrmDenseControl_Offline_PPK.CurDense = CurDense;
                        FormMainMDI.FrmDenseControl_Offline_PPK.ShowDialog();

                    }
                    else if (OPIAp.CurLine.MesControlMode.ToUpper() == "LOCAL")
                    {
                        FormMainMDI.FrmDenseControl_Local_PPK = new FormDenseControl_Local_PPK();
                        FormMainMDI.FrmDenseControl_Local_PPK.CurDense = CurDense;
                        FormMainMDI.FrmDenseControl_Local_PPK.ShowDialog();
                    }
                    #endregion
                }
                else
                {
                    if (OPIAp.CurLine.MesControlMode.ToUpper() == "OFFLINE")
                    {
                        FormMainMDI.FrmDenseControl_Offline = new FormDenseControl_Offline();
                        FormMainMDI.FrmDenseControl_Offline.CurDense = CurDense;
                        FormMainMDI.FrmDenseControl_Offline.ShowDialog();

                    }
                    else if (OPIAp.CurLine.MesControlMode.ToUpper() == "LOCAL")
                    {
                        FormMainMDI.FrmDenseControl_Local = new FormDenseControl_Local();
                        FormMainMDI.FrmDenseControl_Local.CurDense = CurDense;
                        FormMainMDI.FrmDenseControl_Local.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void btnPalletControl_Click(object sender, EventArgs e)
        {
            try
            {
                if (OPIAp.CurLine.MesControlMode.ToUpper() == "OFFLINE")
                {
                    FormMainMDI.FrmPalletControl_Offline = new FormPalletControl_Offline();
                    FormMainMDI.FrmPalletControl_Offline.CurPallet = CurPallet;
                    FormMainMDI.FrmPalletControl_Offline.ShowDialog();

                }
                else if (OPIAp.CurLine.MesControlMode.ToUpper() == "LOCAL")
                {
                    FormMainMDI.FrmPalletControl_Local = new FormPalletControl_Local();
                    FormMainMDI.FrmPalletControl_Local.CurPallet = CurPallet;
                    FormMainMDI.FrmPalletControl_Local.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void btnSlot_Click(object sender, EventArgs e)
        {
            //FormMainMDI.FrmSlotInformation.Visible = true;
            //FormMainMDI.FrmSlotInformation.tmrRefresh.Enabled = true;
            //FormMainMDI.CurForm = FormMainMDI.FrmSlotInformation;
            //FormMainMDI.FrmSlotInformation.BringToFront();
            //FormMainMDI.FrmSlotInformation.SetSelectedNodePort(txtPortEQPID.Text.Trim(), txtPortID.Text.Trim());
            FormMainMDI.FrmPortStatus.Visible = true;
            FormMainMDI.FrmPortStatus.tmrBaseRefresh.Enabled = true;
            FormMainMDI.CurForm = FormMainMDI.FrmPortStatus;
            FormMainMDI.FrmPortStatus.BringToFront();
            FormMainMDI.FrmPortStatus.SetSelectedNodePort(txtPortEQPID.Text.Trim(), txtPortID.Text.Trim());
        }

        private void btnPlan_Click(object sender, EventArgs e)
        {
            new FormChangerPlanDetail().ShowDialog();
        }

        private void btnStandbyPlan_Click(object sender, EventArgs e)
        {
            new FormStandByChangerPlanDetail().ShowDialog();
        }

        private void bgwRefresh_DoWork(object sender, DoWorkEventArgs e)
        {
            while (FormMainMDI.IsRun)
            {
                try
                {
                    if (tlpBase.InvokeRequired)
                    {
                        #region InvokeRequired
                        this.BeginInvoke(new MethodInvoker(
                           delegate
                           {
                               //更新畫面
                               Refresh_LayoutInfo();

                               //更新JobData to DataGrid
                               if (flpLine.Visible)
                               {
                                   DataSetting_Line();
                               }
                               else
                               {
                                   if (flpEQP.Visible)
                                       DataSetting_EQP();
                                   else if (flpPort.Visible)
                                       DataSetting_Port();
                                   else if (flpUnit.Visible)
                                       DataSetting_Unit();
                                   else if (flpVCR.Visible)
                                       DataSetting_VCR();
                                   else if (flpDense.Visible)
                                       DataSetting_Dense();
                                   else if (flpPallet.Visible)
                                       DataSetting_Pallet();
                               }
                           }));
                        #endregion
                    }
                    else
                    {
                        #region 
                        //更新畫面
                        Refresh_LayoutInfo();

                        //更新JobData to DataGrid
                        if (flpLine.Visible)
                        {
                            DataSetting_Line();
                        }
                        else
                        {
                            if (flpEQP.Visible)
                                DataSetting_EQP();
                            else if (flpPort.Visible)
                                DataSetting_Port();
                            else if (flpUnit.Visible)
                                DataSetting_Unit();
                            else if (flpVCR.Visible)
                                DataSetting_VCR();
                            else if (flpDense.Visible)
                                DataSetting_Dense();
                            else if (flpPallet.Visible)
                                DataSetting_Pallet();
                        }
                        #endregion
                    }

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                    
                }
            }
        }

        private void btnCassetteCmd_Click(object sender, EventArgs e)
        {
            try
            {
                string msg = string.Empty;
                string _err = string.Empty;
                Button _btn = (Button)sender;
                string _processCount = "0";
                if (cboCassetteCmd.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text , "", "Please choose Cassette Command", MessageBoxIcon.Warning);
                    cboCassetteCmd.Focus();
                    return;
                }
                
                string _cmd = (cboCassetteCmd.SelectedValue == null ? string.Empty : cboCassetteCmd.SelectedValue.ToString());

                if (_cmd == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Cassette Command", MessageBoxIcon.Warning);
                    cboCassetteCmd.Focus();
                    return;
                }

                //‘1’：Cassette Process Start
                //‘2’：Cassette Process Start By Count
                //‘3’：Cassette Process Pause
                //‘4’：Cassette Process Resume
                //‘5’：Cassette Process Abort
                //‘6’：Cassette Process Cancel 
                //‘7’：Cassette Reload
                //‘8’：Cassette Load
                //‘9’：Cassette Re-Map
                //‘11’：Cassette Map Download
                //‘12’：Cassette Aborting

                #region 2：Cassette Process Start By Count輸入count
                if (_cmd == "2")
                {
                    FormCassetteCommand_PortCount _frm = new FormCassetteCommand_PortCount(CurPort);
                    if (_frm.ShowDialog() == DialogResult.OK)
                    {
                        int _temp =  _frm.StartByCount;
                        _processCount = _temp.ToString();
                    }
                    else return;
                }
                #endregion


                #region 5：Cassette Process Abort && Port Type = Both 時，跳出訊息提示視窗
                if (_cmd == "5")
                {
                    if (CurPort.PortType == ePortType.BothPort)
                    {
                        msg = string.Format("Cassette Will Abnormal Complete and Unload, Glass Will Not Store/Fetch With This Cassette! \r\n  卡匣将会异常结束及此卡匣不再收送基板!", cboCassetteCmd.Text.ToString());
                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                    }
                }
                #endregion

                #region 9：CST Re-Map , 如果Indexer Operation Mode是3:Change Mode時不能下 Remap功能 --俊成
                if (_cmd == "9")
                {
                    if (OPIAp.CurLine.IndexerMode == 3)
                    {
                        ShowMessage(this, lblCaption.Text , "", "Indexer Operation Mode can't use Remap command", MessageBoxIcon.Warning);
                        return;
                    }
                }
                #endregion

                msg = string.Format("Please confirm whether you will process the Cassette Command [{0}] ?", cboCassetteCmd.Text.ToString());
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, MethodBase.GetCurrentMethod().Name)) return;
                
                SendtoBC_CassetteCommandRequest(_cmd, _processCount);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void tmrInterface_Tick(object sender, EventArgs e)
        {
            try
            {
                if (FormMainMDI.CurForm == null) { tmrInterface.Enabled = false; return; }
                if (FormMainMDI.CurForm.Tag == null) { tmrInterface.Enabled = false; return; }
                if (this.Tag == null) { tmrInterface.Enabled = false; return; }
                if (OPIAp.Dic_Pipe == null) { tmrInterface.Enabled = false; return; }

                if (FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString()) { tmrInterface.Enabled = false; return; }


                var _var = OPIAp.Dic_Pipe.Values.Where(r => r.IsDisplay == true && r.IsReply == true);

                foreach (Interface _interface in _var)
                {
                    // 傳送Socket給BC要求 LinkSignal
                    LinkSignalDataRequest _trx = new LinkSignalDataRequest();
                    _trx.HEADER.REPLYSUBJECTNAME = OPIAp.SessionID;
                    _trx.BODY.LINENAME = OPIAp.CurLine.ServerName;
                    _trx.BODY.DOWNSTREAMEQUIPMENTNO = _interface.DownstreamNodeNo;
                    _trx.BODY.DOWNSTREAMEQUIPMENTUNITNO = _interface.DownstreamUnitNo;
                    _trx.BODY.UPSTREAMEQUIPMENTNO = _interface.UpstreamNodeNo;
                    _trx.BODY.UPSTREAMEQUIPMENTUNITNO = _interface.UpstreamUnitNo;
                    _trx.BODY.UPSTREAMSEQUENCENO_BIT = _interface.UpstreamSeqNo;
                    _trx.BODY.DOWNSTREAMSEQUENCENO_BIT = _interface.DownstreamSeqNo;
                    
                    if (OPIAp.Dic_LinkSignal_JobDataPathNoU.ContainsKey(_interface.PipeKey))
                    {
                        string[] _seqNo = OPIAp.Dic_LinkSignal_JobDataPathNoU[_interface.PipeKey].Split(',');

                        foreach (string _no in _seqNo)
                        {
                            LinkSignalDataRequest.SEQUENCENO_WORDc _wordSeqNo = new LinkSignalDataRequest.SEQUENCENO_WORDc();
                            _wordSeqNo.SEQUENCENO_WORD = _no;
                            _trx.BODY.UPSTREAMSEQUENCENOLIST.Add(_wordSeqNo);
                        }
                    }
                    else
                    {
                        LinkSignalDataRequest.SEQUENCENO_WORDc _wordSeqNo = new LinkSignalDataRequest.SEQUENCENO_WORDc();
                        _wordSeqNo.SEQUENCENO_WORD = _interface.UpstreamSeqNo;
                        _trx.BODY.UPSTREAMSEQUENCENOLIST.Add(_wordSeqNo);
                    }


                    if (OPIAp.Dic_LinkSignal_JobDataPathNoD.ContainsKey(_interface.PipeKey))
                    {
                        string[] _seqNo = OPIAp.Dic_LinkSignal_JobDataPathNoD[_interface.PipeKey].Split(',');

                        foreach (string _no in _seqNo)
                        {
                            LinkSignalDataRequest.SEQUENCENO_WORDc _wordSeqNo = new LinkSignalDataRequest.SEQUENCENO_WORDc();
                            _wordSeqNo.SEQUENCENO_WORD = _no;
                            _trx.BODY.DOWNSTREAMSEQUENCENOLIST.Add(_wordSeqNo);
                        }
                    }
                    else
                    {
                        LinkSignalDataRequest.SEQUENCENO_WORDc _wordSeqNo = new LinkSignalDataRequest.SEQUENCENO_WORDc();
                        _wordSeqNo.SEQUENCENO_WORD = _interface.DownstreamSeqNo;
                        _trx.BODY.DOWNSTREAMSEQUENCENOLIST.Add(_wordSeqNo);
                    }

                    string _xml = _trx.WriteToXml();

                    string _err = string.Empty;

                    FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, OPIAp.SessionID);
                    OPIAp.Dic_Pipe[_interface.PipeKey].IsReply = false;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);               
            }
        }

        #region 同步 socket

        private void SendtoBC_JobDataCategoryRequest(string LocalNo,string UnitNo,string PortNo)
        {
            try
            {
                int _slotNo = 0;
                int _jobSeqNo = 0;
                int _cstSeqNo = 0;
                int _productType = 0;
                string _nodeData = string.Empty ;

                dgvJobData.Rows.Clear();

                #region Send JobDataRequest

                JobDataCategoryRequest _trx = new JobDataCategoryRequest();
                _trx.HEADER.REPLYSUBJECTNAME = OPIAp.SessionID;
                _trx.BODY.LINENAME = OPIAp.CurLine.ServerName;

                _trx.BODY.EQUIPMENTNO = LocalNo;
                _trx.BODY.UNITNO = UnitNo;
                _trx.BODY.PORTNO = PortNo;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region JobDataReply

                string _respXml = _resp.Xml;

                JobDataCategoryReply _JobDataCategoryReply = (JobDataCategoryReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data

                foreach (JobDataCategoryReply.EQUIPMENTITEMc eqpData in _JobDataCategoryReply.BODY.EQUIPMENTLIST)
                {
                    foreach (JobDataCategoryReply.JOBc jobData in eqpData.JOBDATALIST)
                    {
                        int.TryParse(jobData.SLOTNO,out _slotNo);
                        int.TryParse(jobData.JOBSEQNO,out _jobSeqNo);
                        int.TryParse(jobData.CASSETTESEQNO,out _cstSeqNo);
                        int.TryParse(jobData.PRODUCTTYPE,out _productType);

                        if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(eqpData.EQUIPMENTNO))
                        {
                            _nodeData = FormMainMDI.G_OPIAp.Dic_Node[eqpData.EQUIPMENTNO].NodeNo + "-" + FormMainMDI.G_OPIAp.Dic_Node[eqpData.EQUIPMENTNO].NodeID;
                        } else _nodeData = eqpData.EQUIPMENTNO;

                        //Local No, CST Seq No, Job Seq No, GlassID, product Type, Job TYPE, Job Judge, Job Grade, PPID, TracKing Data
                        //20150312 cy:增加一個Sort Slot欄位, 以做排序
                        dgvJobData.Rows.Add("Detail","Route","Stop Reason",
                            _nodeData,
                            _slotNo,
                            _cstSeqNo,
                            _jobSeqNo,
                            jobData.GLASSID,
                            _productType,
                            jobData.JOBTYPE,
                            jobData.JOBJUDGE,
                            jobData.JOBGRADE,
                            jobData.PPID,
                            jobData.LINERECIPENAME,
                            jobData.PRODUCTSPECVER, 
                            jobData.OWNERID, 
                            jobData.PROCESSOPERATIONNAME,
                            jobData.SAMPLINGFLAG,
                            jobData.TRACKINGDATA                  
                            );                        
                    }
                }

                if (dgvJobData.Rows.Count > 0)
                {
                    dgvJobData.Sort(dgvJobData.Columns[colSlotNo.Name], ListSortDirection.Descending);
                }
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void SendtoBC_EquipmentStatusRequest(string LocalNo)
        {
            try
            {
                #region Send to BC EquipmentStatusRequest

                EquipmentStatusRequest _trx = new EquipmentStatusRequest();
                _trx.HEADER.REPLYSUBJECTNAME = OPIAp.SessionID;
                _trx.BODY.LINENAME = OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = LocalNo;
                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp != null)
                {
                    #region EquipmentStatusReply

                    string _respXml = _resp.Xml;

                    EquipmentStatusReply _equipmentStatusReply = (EquipmentStatusReply)Spec.CheckXMLFormat(_respXml);

                    //#region Check Return Msg
                    //if (FormMainMDI.G_OPIAp.CurLine.ServerName != _equipmentStatusReply.BODY.LINENAME)
                    //{
                    //    ShowMessage(this, lblCaption.Text, "", "Line Name disaccords with current Line Name", MessageBoxIcon.Error);
                    //    return;
                    //}

                    //if (!_equipmentStatusReply.RETURN.RETURNCODE.Equals(FormMainMDI.G_OPIAp.ReturnCodeSuccess))
                    //{
                    //    ShowMessage(this, lblCaption.Text, _equipmentStatusReply.RETURN, MessageBoxIcon.Error);
                    //    return;
                    //}
                    //#endregion

                    #region Update Data
                    string _key = _equipmentStatusReply.BODY.EQUIPMENTNO;
                    if (!OPIAp.Dic_Node.ContainsKey(_key))
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Can't find Equipment No[{0}]", _equipmentStatusReply.BODY.EQUIPMENTNO), MessageBoxIcon.Error);
                        return;
                    }
                    OPIAp.Dic_Node[_key].SetNodeInfo(_equipmentStatusReply);
                    #endregion

                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void SendtoBC_PortStatusRequest(string LocalNo, string PortNo)
        {
            try
            {
                #region Send to BC PortStatusRequest
                PortStatusRequest _trx = new PortStatusRequest();
                _trx.HEADER.REPLYSUBJECTNAME = OPIAp.SessionID;
                _trx.BODY.LINENAME = OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = LocalNo;
                _trx.BODY.PORTNO = PortNo;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp != null)
                {
                    #region PortStatusReply

                    string _respXml = _resp.Xml;

                    PortStatusReply _portStatusReply = (PortStatusReply)Spec.CheckXMLFormat(_respXml);

                    //#region Check Return Msg
                    //if (FormMainMDI.G_OPIAp.CurLine.ServerName != _portStatusReply.BODY.LINENAME)
                    //{
                    //    ShowMessage(this, lblCaption.Text, "", "Line Name disaccords with current Line Name", MessageBoxIcon.Error);
                    //    return;
                    //}

                    //if (!_portStatusReply.RETURN.RETURNCODE.Equals(FormMainMDI.G_OPIAp.ReturnCodeSuccess))
                    //{
                    //    ShowMessage(this, lblCaption.Text, _portStatusReply.RETURN, MessageBoxIcon.Error);
                    //    return;
                    //}
                    //#endregion

                    #region Update Data
                    string _key = _portStatusReply.BODY.EQUIPMENTNO.PadRight(3, ' ') + _portStatusReply.BODY.PORTNO.PadRight(2, ' ');
                    if (!OPIAp.Dic_Port.ContainsKey(_key))
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Can't find Equipement No[{0}],Port No [{1}]", _portStatusReply.BODY.EQUIPMENTNO, _portStatusReply.BODY.PORTNO), MessageBoxIcon.Error);
                        return;
                    }
                    OPIAp.Dic_Port[_key].SetPortInfo(_portStatusReply);
                    #endregion

                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void SendtoBC_DenseStatusRequest(string LocalNo, string PortNo)
        {
            try
            {
                #region Send to BC DenseStatusRequest

                DenseStatusRequest _trx = new DenseStatusRequest();
                _trx.HEADER.REPLYSUBJECTNAME = OPIAp.SessionID;
                _trx.BODY.LINENAME = OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = LocalNo;
                _trx.BODY.PORTNO = PortNo;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp != null)
                {
                    #region DenseStatusReply

                    string _respXml = _resp.Xml;

                    DenseStatusReply _denseStatusReply = (DenseStatusReply)Spec.CheckXMLFormat(_respXml);

                    //#region Check Return Msg
                    //if (FormMainMDI.G_OPIAp.CurLine.ServerName != _denseStatusReply.BODY.LINENAME)
                    //{
                    //    ShowMessage(this, lblCaption.Text, "", "Line Name disaccords with current Line Name", MessageBoxIcon.Error);
                    //    return;
                    //}

                    //if (!_denseStatusReply.RETURN.RETURNCODE.Equals(FormMainMDI.G_OPIAp.ReturnCodeSuccess))
                    //{
                    //    ShowMessage(this, lblCaption.Text, _denseStatusReply.RETURN, MessageBoxIcon.Error);
                    //    return;
                    //}
                    //#endregion

                    #region Update Data
                    string _key = _denseStatusReply.BODY.EQUIPMENTNO.PadRight(3, ' ') + _denseStatusReply.BODY.PORTNO.PadRight(2, ' ');
                    if (!OPIAp.Dic_Dense.ContainsKey(_key))
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Can't find Equipement No[{0}],Port No [{1}]", _denseStatusReply.BODY.EQUIPMENTNO, _denseStatusReply.BODY.PORTNO), MessageBoxIcon.Error);
                        return;
                    }
                    OPIAp.Dic_Dense[_key].SetDenseInfo(_denseStatusReply);
                    #endregion

                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void SendtoBC_PalletStatusRequest(string PalletNo)
        {
            try
            {
                #region Send to BC DenseStatusRequest

                PalletStatusRequest _trx = new PalletStatusRequest();
                _trx.HEADER.REPLYSUBJECTNAME = OPIAp.SessionID;
                _trx.BODY.LINENAME = OPIAp.CurLine.ServerName;
                _trx.BODY.PALLETNO = PalletNo;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp != null)
                {
                    #region DenseStatusReply

                    string _respXml = _resp.Xml;

                    PalletStatusReply _palletStatusReply = (PalletStatusReply)Spec.CheckXMLFormat(_respXml);

                    //#region Check Return Msg
                    //if (FormMainMDI.G_OPIAp.CurLine.ServerName != _palletStatusReply.BODY.LINENAME)
                    //{
                    //    ShowMessage(this, lblCaption.Text, "", "Line Name disaccords with current Line Name", MessageBoxIcon.Error);
                    //    return;
                    //}

                    //if (!_palletStatusReply.RETURN.RETURNCODE.Equals(FormMainMDI.G_OPIAp.ReturnCodeSuccess))
                    //{
                    //    ShowMessage(this, lblCaption.Text, _palletStatusReply.RETURN, MessageBoxIcon.Error);
                    //    return;
                    //}
                    //#endregion

                    #region Update Data
                    string _key = _palletStatusReply.BODY.PALLETNO;
                    if (!OPIAp.Dic_Pallet.ContainsKey(_key))
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Can't find Pallet No[{0}]", _palletStatusReply.BODY.PALLETNO), MessageBoxIcon.Error);
                        return;
                    }
                    OPIAp.Dic_Pallet[_key].SetPalletInfo(_palletStatusReply);
                    #endregion

                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void SendtoBC_CassetteCommandRequest(string CassetteCmd,string ProcessCount)
        {
            try
            {
                #region Send to BC DenseStatusRequest

                CassetteCommandRequest _trx = new CassetteCommandRequest();

                _trx.HEADER.REPLYSUBJECTNAME = OPIAp.SessionID;
                _trx.BODY.LINENAME = OPIAp.CurLine.ServerName;
                _trx.BODY.CASSETTEID = txtCassetteID.Text;
                _trx.BODY.PORTID = CurPort.PortID;
                _trx.BODY.PORTNO = CurPort.PortNo;
                _trx.BODY.EQUIPMENTNO = CurPort.NodeNo;
                _trx.BODY.OPERATORID = OPIAp.LoginUserID;
                _trx.BODY.PROCESSCOUNT = ProcessCount;
                _trx.BODY.CASSETTECOMMAND = CassetteCmd;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp != null)
                {
                    #region CassetteCommandReply

                    //string _respXml = _resp.Xml;

                    //CassetteCommandReply _cassetteCommandReply = (CassetteCommandReply)Spec.CheckXMLFormat(_respXml);

                    //#region Check Return Msg
                    //if (FormMainMDI.G_OPIAp.CurLine.ServerName != _cassetteCommandReply.BODY.LINENAME)
                    //{
                    //    ShowMessage(this, lblCaption.Text, "", "Line Name disaccords with current Line Name", MessageBoxIcon.Error);
                    //    return;
                    //}

                    //if (!_cassetteCommandReply.RETURN.RETURNCODE.Equals(FormMainMDI.G_OPIAp.ReturnCodeSuccess))
                    //{
                    //    ShowMessage(this, lblCaption.Text, _cassetteCommandReply.RETURN, MessageBoxIcon.Error);
                    //    return;
                    //}
                    //#endregion

                    ShowMessage(this, lblCaption.Text,"", string.Format("Cassette Command Send to BC Success"), MessageBoxIcon.Information);

                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }
               
        #endregion

      
        private string GetPortProcrssTypeContent(string index)
        {
            int result = 0;
            if (int.TryParse(index, out result) == true)
            {
                foreach(ProcessType_Array processTypeArray in OPIAp.CurLine.Lst_ProcessType_Array)
                {
                    if (processTypeArray.ProcessTypeNo == int.Parse(index))
                    {
                        return processTypeArray.ProcessTypeDesc;
                    }
                }
            }
            return index;
        }

        private void Reqtimer_Tick(object sender, EventArgs e)//20180523 add by hujunpeng
        {
            string mystr = "Server=.;user=sa;pwd=itc123!@#;database=UNIBCS_t3";
            SqlConnection mycon = new SqlConnection(mystr);
            try
            {
                mycon.Open();
                string sql = "select [NODENAME] from SBRM_NODE where [SERVERNAME]='" + FormMainMDI.G_OPIAp.CurLine.ServerName + "'and [NODENO]='L6'";
                string sql1 = "select [NODENAME] from SBRM_NODE where [SERVERNAME]='" + FormMainMDI.G_OPIAp.CurLine.ServerName + "'and [NODENO]='L4'";
                //SqlCommand mycom=new SqlCommand (sql,mycon);
                //SqlDataReader mydr = mycom.ExecuteReader();
                //if (mydr.Read())
                //{
                //    label8.Text = (mydr.GetValue(11)).ToString();
                //}
                SqlDataAdapter myda = new SqlDataAdapter(sql, mycon);
                SqlDataAdapter myda1 = new SqlDataAdapter(sql1, mycon);
                DataSet myds = new DataSet();
                DataSet myds1 = new DataSet();
                myda.Fill(myds, "L6");
                myda1.Fill(myds1, "L4");

                DataRow row = myds.Tables[0].Rows[0];
                DataRow row1 = myds1.Tables[0].Rows[0];
                if (row[0] != null || row1[0] != null)
                {
                    if (row[0].ToString().Count() != 30)
                    {
                        NLogManager.Logger.LogErrorWrite("FormLayout", "Reqtimer_Tick", "PRID1长度不是30码");
                        row[0] = "000000000000000000000000000000";
                    }
                    if (row1[0].ToString().Count() != 30)
                    {
                        NLogManager.Logger.LogErrorWrite("FormLayout", "Reqtimer_Tick", "PRID1长度不是30码");
                        row1[0] = "000000000000000000000000000000";
                    }
                    progressBar1.Value = int.Parse(row[0].ToString().Substring(25, 5));
                    label8.Text = string.Format("{0}，{1}", row[0].ToString().Substring(24, 1), row[0].ToString().Substring(25, 5));

                    if (progressBar1.Value < 5000)
                    {
                        progressBar1.ForeColor = Color.Red;
                    }
                    else if (progressBar1.Value > 10000)
                    {
                        progressBar1.ForeColor = Color.Green;
                    }
                    else
                    {
                        progressBar1.ForeColor = Color.Yellow;
                    }
                    progressBar2.Value = int.Parse(row1[0].ToString().Substring(25, 5));
                    label9.Text = string.Format("{0}，{1}", row1[0].ToString().Substring(24, 1), row1[0].ToString().Substring(25, 5));
                    if (progressBar2.Value < 5000)
                    {
                        progressBar2.ForeColor = Color.Red;
                    }
                    else if (progressBar2.Value > 10000)
                    {
                        progressBar2.ForeColor = Color.Green;
                    }
                    else
                    {
                        progressBar2.ForeColor = Color.Yellow;
                    }

                }
                ToolTip _tip = new ToolTip();

                _tip.SetToolTip(progressBar1, string.Format("MaterialID:{0},MaterialWeight:{1},MaterialStatus:{2}", row[0].ToString().Substring(0, 23), row[0].ToString().Substring(25, 5), row[0].ToString().Substring(24, 1)));

                _tip.SetToolTip(progressBar2, string.Format("MaterialID:{0},MaterialWeight:{1},MaterialStatus:{2}", row1[0].ToString().Substring(0, 23), row1[0].ToString().Substring(25, 5), row1[0].ToString().Substring(24, 1)));
            }
            catch (Exception ex)
            { NLogManager.Logger.LogErrorWrite("FormLayout", "Reqtimer_Tick", ex); }
            finally
            { mycon.Close(); }

            MaterialStatusRequest _trx = new MaterialStatusRequest();
            _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
            _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
            _trx.BODY.EQUIPMENTNO = "L6";
            _trx.BODY.COMMAND = "MaterialStatusRequest";

            MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

            if (_resp == null) return;

            string _respXml = _resp.Xml;
            MaterialStatusReply _MaterialStatusReply = (MaterialStatusReply)Spec.CheckXMLFormat(_respXml);
            if (_MaterialStatusReply.BODY.SLOTNO == "1")
            {
                progressBar1.Value = int.Parse(_MaterialStatusReply.BODY.MATERIALVALUE);
                label8.Text = string.Format("{0}，{1}", _MaterialStatusReply.BODY.MATERIALSTATUS, _MaterialStatusReply.BODY.MATERIALVALUE);
                

                if (progressBar1.Value < 5000)
                {
                    progressBar1.ForeColor = Color.Red;
                }
                else if (progressBar1.Value > 10000)
                {
                    progressBar1.ForeColor = Color.Green;
                }
                else
                {
                    progressBar1.ForeColor = Color.Yellow;
                }
            }
            if (_MaterialStatusReply.BODY.SLOTNO == "2")
            {
                progressBar2.Value = int.Parse(_MaterialStatusReply.BODY.MATERIALVALUE);
                label9.Text = string.Format("{0}，{1}", _MaterialStatusReply.BODY.MATERIALSTATUS, _MaterialStatusReply.BODY.MATERIALVALUE);


                if (progressBar2.Value < 5000)
                {
                    progressBar2.ForeColor = Color.Red;
                }
                else if (progressBar2.Value > 10000)
                {
                    progressBar2.ForeColor = Color.Green;
                }
                else
                {
                    progressBar2.ForeColor = Color.Yellow;
                }
            }
            if (_MaterialStatusReply.BODY.SLOTNO == "0")
            {
                NLogManager.Logger.LogErrorWrite("FormLayout", "Reqtimer_Tick", "机台回复超时");
            }
        }
    }
}
