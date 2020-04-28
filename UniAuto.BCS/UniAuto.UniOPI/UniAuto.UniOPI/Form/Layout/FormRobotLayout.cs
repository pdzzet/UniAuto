using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;


namespace UniOPI
{
    public partial class FormRobotLayout : FormBase
    {
        OPIInfo OPIAp;

        public FormRobotLayout(OPIInfo apInfo)
        {
            InitializeComponent();

            OPIAp = apInfo;

            tmrBaseRefresh.Enabled = false;
            tmrRefresh.Enabled = true;

            lsvRobotCommand.DoubleClick += new EventHandler(lsvRobotCommand_DoubleClick);
            tmrBaseRefresh.Tick +=new EventHandler(tmrBaseRefresh_Tick);
            tmrRefresh.Tick += new EventHandler(tmrRefresh_Tick);
        }

        private void FormRobotLayout_Load(object sender, EventArgs e)
        {
            try
            {
                int _portSlotCnt = 0;
                string _name = string.Empty;
                string _display = string.Empty;

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;

                #region Load Robot Image
                string _robotPath =string.Format("{0}{1}.png", OPIConst.RobotFolder ,FormMainMDI.G_OPIAp.CurLine.ServerName );

                if (File.Exists(_robotPath))
                {
                    pnlRobotPic.BackgroundImage = new Bitmap(_robotPath);
                }
                #endregion

                if (FormMainMDI.G_OPIAp.CurLine.FabType == "CELL")
                {
                    flpRobot.Height = 250;
                    flpStage.Height = 250;
                    flpPortStage.Height = 250;

                    tlpBase.RowStyles[0].Height = 280;

                    #region robot unloader dispatch
                    dgvDispatch.Rows.Clear();
                    foreach (string _key in OPIAp.Dic_Port.Keys)
                    {
                        Port _port = OPIAp.Dic_Port[_key];

                        dgvDispatch.Rows.Add(_key, _port.PortID);
                    }
                    #endregion
                }
                else
                {
                    flpRobot.Height = 210;
                    flpStage.Height = 210;
                    flpPortStage.Height = 210;  
                    tlpBase.RowStyles[0].Height = 240;

                    tabInfo.TabPages.Remove(tabInfo.TabPages["tpUnloaderDispatch"]);
                }
                
                #region initial TextBox & Label
                foreach (Robot _rb in OPIAp.Dic_Robot.Values)
                {
                    #region 判斷是否有雙手臂 -- initial label & TextBox寬度用 (ArmMaxJobCount=2表示雙手臂,4Arm 8片)
                    int _lblRbCaptionWidth = 0;  //Robot Info Caption Width
                    int _lblArmCaptionWidth = 0; //Arm 01 -04 Caption Width
                    int _lblArmWidth = 0; //Arm label (front,Back) caption width
                    int _txtArmWidth = 0; //Arm txt width
                    int _lblStageWidth = 0;
                    int _txtStageWidth = 0;  //最少須給105才能顯是完整

                    #region 依照手臂&Arm上job count設定lable & textbox寬度
                    if (_rb.RobotArmCount == 4)
                    {                        
                        _lblRbCaptionWidth = 285;
                        _lblStageWidth = 140;
                        _txtStageWidth = 143;

                        _lblArmCaptionWidth = 140;
                        _lblArmWidth = 45;
                        _txtArmWidth = 95;

                        flpRobot.Width = 289;
                        flpStage.Width = 289;
                        flpPortStage.Width = 289;

                        tlpBase.RowStyles[1].Height = 100;
                    }
                    else
                    {
                        _lblRbCaptionWidth = 195;
                        _lblStageWidth = 90;
                        _txtStageWidth = 105;

                        if (_rb.ArmMaxJobCount == 2)
                        {
                            _lblArmCaptionWidth = 195;
                            _lblArmWidth = 89;
                            _txtArmWidth = 105;
                        }
                        else
                        {
                            _lblArmCaptionWidth = 0;
                            _lblArmWidth = 89;
                            _txtArmWidth = 105;
                        }

                        flpRobot.Width = 200;
                        flpStage.Width = 200;
                        flpPortStage.Width = 200;

                        tlpBase.RowStyles[1].Height = 65;
                    }
                    #endregion

                    #endregion

                    #region Initial Robot Command Area
                    for (int i = _rb.RobotArmCount; i >= 1 ; i--)
                    {
                        lstRobotCommand.Items.Add(string.Empty );
                    //    Label _lblRC = new Label();
                    //    _lblRC.Name = i.ToString() + "RobotCommand";
                    //    _lblRC.AutoSize = false;
                    //    _lblRC.Size = new System.Drawing.Size(1000, 23);
                    //    _lblRC.Padding = new Padding(5, 0, 0, 0);
                    //    _lblRC.Font = new Font("Cambria", 11);
                    //    _lblRC.TextAlign = ContentAlignment.MiddleLeft;
                    //    _lblRC.ForeColor = Color.Black;
                    //    _lblRC.BackColor = Color.Transparent;
                    //    //_lbl.ReadOnly = true;

                    //    grbRobotCommand.Controls.Add(_lblRC);
                    //    _lblRC.Dock = DockStyle.Top;
                    }
                    #endregion

                    #region Arm
                    #region robot Arm Header
                    Label  _lbl = new Label();

                    _lbl.Name = string.Format("lbl{0}", _rb.RobotName);
                    _lbl.Text = string.Format("[{0}] - {1} Arm Info", _rb.RobotName, _rb.NodeNo); 

                    _lbl.Font = new Font("Calibri", 11);
                    _lbl.TextAlign = ContentAlignment.MiddleCenter;
                    _lbl.BackColor = Color.Black;
                    _lbl.ForeColor = Color.White;
                    _lbl.Size = new Size(_lblRbCaptionWidth, 25); //new Size(200, 28);
                    _lbl.Margin = new System.Windows.Forms.Padding(1, 3, 3, 1);
                    _lbl.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
                    flpRobot.Controls.Add(_lbl);
                    #endregion

                    #region robot Arm
                    if (_rb.RobotArmCount == 4)
                    {
                        #region robot Arm == 4 (initial顯示順序 Arm03->Arm01->Arm04->Arm02)
                        //Lable: lbl+RobotName+"_Arm03", TextBox : txt+RobotName+"_Arm03_Front"
                        InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm03", _rb.RobotName), "Arm03", _lblArmCaptionWidth, false);
                        InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm01", _rb.RobotName), "Arm01", _lblArmCaptionWidth, false);
                        InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm03_Front", _rb.RobotName), "Front", _lblArmWidth, false);
                        InitialArmTxt(flpRobot, string.Format("txt{0}_Arm03_Front", _rb.RobotName), _txtArmWidth);
                        InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm01_Front", _rb.RobotName), "Front", _lblArmWidth, false);
                        InitialArmTxt(flpRobot, string.Format("txt{0}_Arm01_Front", _rb.RobotName), _txtArmWidth);

                        if (_rb.ArmMaxJobCount == 2)
                        {
                            InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm03_Back", _rb.RobotName), "Back", _lblArmWidth, false);
                            InitialArmTxt(flpRobot, string.Format("txt{0}_Arm03_Back", _rb.RobotName), _txtArmWidth);
                            InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm01_Back", _rb.RobotName), "Back", _lblArmWidth, false);
                            InitialArmTxt(flpRobot, string.Format("txt{0}_Arm01_Back", _rb.RobotName), _txtArmWidth);
                        }

                        InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm04", _rb.RobotName), "Arm04", _lblArmCaptionWidth, false);
                        InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm02", _rb.RobotName), "Arm02", _lblArmCaptionWidth, false);
                        InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm04_Front", _rb.RobotName), "Front", _lblArmWidth, false);
                        InitialArmTxt(flpRobot, string.Format("txt{0}_Arm04_Front", _rb.RobotName), _txtArmWidth);
                        InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm02_Front", _rb.RobotName), "Front", _lblArmWidth, false);
                        InitialArmTxt(flpRobot, string.Format("txt{0}_Arm02_Front", _rb.RobotName), _txtArmWidth);

                        if (_rb.ArmMaxJobCount == 2)
                        {
                            InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm04_Back", _rb.RobotName), "Back", _lblArmWidth, false);
                            InitialArmTxt(flpRobot, string.Format("txt{0}_Arm04_Back", _rb.RobotName), _txtArmWidth);
                            InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm02_Back", _rb.RobotName), "Back", _lblArmWidth, false);
                            InitialArmTxt(flpRobot, string.Format("txt{0}_Arm02_Back", _rb.RobotName), _txtArmWidth);
                        }
                        #endregion
                    }
                    else
                    {
                        #region Robot Arm = 2
                        if (_rb.ArmMaxJobCount == 2)
                        {
                            InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm01", _rb.RobotName), "Arm01", _lblArmCaptionWidth, false);
                        }

                        InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm01_Front", _rb.RobotName), _rb.ArmMaxJobCount == 1 ? "Arm01" : "Front", _lblArmWidth, false);
                        InitialArmTxt(flpRobot, string.Format("txt{0}_Arm01_Front", _rb.RobotName), _txtArmWidth);

                        if (_rb.ArmMaxJobCount == 2)
                        {
                            InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm01_Back", _rb.RobotName), "Back", _lblArmWidth, false);
                            InitialArmTxt(flpRobot, string.Format("txt{0}_Arm01_Back", _rb.RobotName), _txtArmWidth);

                            InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm02", _rb.RobotName), "Arm02", _lblArmCaptionWidth, false);
                        }

                        InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm02_Front", _rb.RobotName), _rb.ArmMaxJobCount == 1 ? "Arm02" : "Front", _lblArmWidth, false);
                        InitialArmTxt(flpRobot, string.Format("txt{0}_Arm02_Front", _rb.RobotName), _txtArmWidth);

                        if (_rb.ArmMaxJobCount == 2)
                        {
                            InitialArmLabel(flpRobot, string.Format("lbl{0}_Arm02_Back", _rb.RobotName), "Back", _lblArmWidth, false);
                            InitialArmTxt(flpRobot, string.Format("txt{0}_Arm02_Back", _rb.RobotName), _txtArmWidth);
                        }
                        #endregion
                    }
                    #endregion

                    #endregion

                    #region Stage - not include port
                    
                    var _lstStageNode = (from row in _ctx.SBRM_ROBOT_STAGE.Where(d =>
                        d.ROBOTNAME.Equals(_rb.RobotName) && (d.STAGETYPE != "PORT")).OrderBy(r=>r.NODENO)
                                                                 select row.NODENO).Distinct().ToList();
                    foreach (string _nodeNo in _lstStageNode)
                    {
                        #region robo Stage Header
                        _lbl = new Label();

                        _lbl.Name = "lblStage";
                        _lbl.Text = string.Format("[{0}] - {1} Stage Info",_rb.RobotName,_nodeNo);

                        _lbl.Font = new Font("Calibri", 12);
                        _lbl.TextAlign = ContentAlignment.MiddleCenter;
                        _lbl.BackColor = Color.Black;
                        _lbl.ForeColor = Color.White;
                        _lbl.Size = new Size(_lblRbCaptionWidth, 25); 
                        _lbl.Margin = new System.Windows.Forms.Padding(1, 3, 3, 1);
                        _lbl.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
                        flpStage.Controls.Add(_lbl);
                        #endregion

                        #region robo Stage - not include port
                        foreach (RobotStage _stage in OPIAp.Lst_RobotStage.Where(r => r.RobotName.Equals(_rb.RobotName) && r.NodeNo.Equals(_nodeNo) && r.StageType != "PORT"))
                        {
                            if (_portSlotCnt < _stage.SlotMaxCount) _portSlotCnt = _stage.SlotMaxCount;

                            for (int i = 1; i <= _stage.SlotMaxCount; i++)
                            {
                                _name = string.Format("{0}_{1}_{2}", _stage.RobotName, _stage.StageID.PadLeft(2, '0'), i.ToString().PadLeft(3, '0'));

                                if (_rb.RobotArmCount == 4 && _rb.ArmMaxJobCount == 2)
                                {
                                    //Slot No = 01 : Left Front
                                    //Slot No = 02 : Left  Back
                                    //Slot No = 03 : Right Front
                                    //Slot No = 04 : Right Back
                                    if (i == 1) _display = string.Format("{0}-{1}-{2}", _stage.StageID, _stage.StageName, "Left Front");
                                    else if (i == 2) _display = string.Format("{0}-{1}-{2}", _stage.StageID, _stage.StageName, "Left Back");
                                    else if (i == 3) _display = string.Format("{0}-{1}-{2}", _stage.StageID, _stage.StageName, "Right Front");
                                    else if (i == 4) _display = string.Format("{0}-{1}-{2}", _stage.StageID, _stage.StageName, "Right Back");
                                    else _display = string.Format("{0}-{1}-{2}", _stage.StageID, _stage.StageName, i.ToString().PadLeft(3, '0'));
                                }
                                else
                                {
                                    _display = string.Format("{0}-{1}-{2}", _stage.StageID, _stage.StageName, i.ToString().PadLeft(3, '0'));
                                }
                                InitialArmLabel(flpStage, "lbl" + _name, _display, _lblStageWidth, _stage.StageType == "STAGE");
                                InitialArmTxt(flpStage, "txt" + _name, _txtStageWidth);
                            }
                        }
                        #endregion
                    }

                    #endregion

                    #region Port Stage
                    var _lstPortNode = (from row in _ctx.SBRM_ROBOT_STAGE.Where(d =>
                        d.ROBOTNAME.Equals(_rb.RobotName) && (d.STAGETYPE == "PORT")).OrderBy(r=>r.NODENO)
                                                                 select row.NODENO).Distinct().ToList();
                    foreach (string _nodeNo in _lstPortNode)
                    {

                        #region robo Port Stage Header
                        _lbl = new Label();

                        _lbl.Name = "lblPort";
                        _lbl.Text = string.Format("[{0}] - {1} Port Info", _rb.RobotName, _nodeNo); 

                        _lbl.Font = new Font("Calibri", 12);
                        _lbl.TextAlign = ContentAlignment.MiddleCenter;
                        _lbl.BackColor = Color.Black;
                        _lbl.ForeColor = Color.White;
                        _lbl.Size = new Size(_lblRbCaptionWidth, 25); //new Size(200, 28);
                        _lbl.Margin = new System.Windows.Forms.Padding(1, 3, 3, 1);
                        _lbl.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
                        flpPortStage.Controls.Add(_lbl);
                        #endregion

                        #region robo port Stage
                        foreach (RobotStage _stage in OPIAp.Lst_RobotStage.Where(r => r.RobotName.Equals(_rb.RobotName) && r.NodeNo.Equals(_nodeNo) && r.StageType == "PORT"))
                        {
                            for (int i = 1; i <= _portSlotCnt; i++)
                            {
                                _name = string.Format("{0}_{1}_{2}", _stage.RobotName, _stage.StageID.PadLeft(2, '0'), i.ToString().PadLeft(3, '0'));

                                _display = string.Format("{0}-{1}-000", _stage.StageID, _stage.StageName);

                                InitialArmLabel(flpPortStage, "lbl" + _name, _display, _lblStageWidth, _stage.StageType == "STAGE");
                                InitialArmTxt(flpPortStage, "txt" + _name, _txtStageWidth);
                            }
                        }

                        #endregion
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

        private void RobotStage_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                Label _lbl = (Label)sender;
                //"lbl" + _stage.RobotName + "_" + _stage.StageNo.PadLeft(2, '0');
                string[] _data = _lbl.Name.Split('_');
                string _rbName = _data[0].Substring(3);
                string _stageID = _data[1];

                var _var = OPIAp.Lst_RobotStage.Where(r => r.RobotName.Equals(_rbName) && r.StageID.Equals(_stageID));

                if (_var.Count() == 0)
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", string.Format("Can't find Robot Name [{0}],Stage ID [{1}]", _rbName, _stageID), MessageBoxIcon.Error);
                    return;
                }

                RobotStage _stage = (RobotStage)_var.First();

                if (_stage.BC_StagePositionInfoReply == null)
                {
                    BCS_StagePositionInfoReply _reply = new BCS_StagePositionInfoReply();

                    _reply.NodeNo = _stage.NodeNo;

                    _stage.BC_StagePositionInfoReply = _reply;
                }

                _stage.BC_StagePositionInfoReply.IsReply = true;
                //_stage.BC_StagePositionInfoReply.IsDisplay = true;

                FormStagePositionInfo _frm = new FormStagePositionInfo(_stage) { TopMost = true };
                _frm.ShowDialog();
                if (_frm != null) _frm.Dispose();
                //_stage.BC_StagePositionInfoReply.IsDisplay = false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void JobDataQuery_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                //(CstSeqNo,JobSeqNo)
                string _tmp = _txt.Text.ToString();

                //去除前後( )
                if (_tmp.Length >= 3)
                {
                    _tmp = _tmp.Substring(1, _tmp.Length - 2);

                    string[] _data = _tmp.ToString().Split(',');

                    if (_data.Length != 2)
                    {
                        ShowMessage(this, lblCaption.Text, "", "No Cst Seq No or Job Seq No Data", MessageBoxIcon.Warning);
                        return;
                    }

                    if (_data[0] == string.Empty || _data[1] == string.Empty)
                    {
                        ShowMessage(this, this.lblCaption.Text, "", "Cassette Seq No、Job Seq No must be Required！", MessageBoxIcon.Warning);
                        return;
                    }

                    FormJobDataDetail _frm = new FormJobDataDetail(string.Empty, _data[0], _data[1]);
                    _frm.TopMost = true;
                    _frm.ShowDialog();

                    _frm.Dispose();
                }
                else
                {
                    ShowMessage(this, lblCaption.Text, "", "Cst Seq No and Job Seq No Fromat Error (CstSeqNo,JobSeqNo)", MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void lsvRobotCommand_DoubleClick(object sender, EventArgs e)
        {
            try
            {                
                if (lsvRobotCommand.SelectedItems.Count > 0)
                {
                    string _type = this.lsvRobotCommand.SelectedItems[0].SubItems[0].Text.ToString() as string;
                    string _detail = this.lsvRobotCommand.SelectedItems[0].SubItems[1].Text.ToString() as string;
                    FormRobotCommandDetail _frm = new FormRobotCommandDetail(_type,_detail);
                    _frm.ShowDialog();

                    if (_frm != null)  _frm.Dispose();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnClearCommand_Click(object sender, EventArgs e)
        {
            try
            {
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, "Please confirm whether you will clear the robot Command Information?")) return;
                lsvRobotCommand.Items.Clear();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void tmrBaseRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (FormMainMDI.CurForm == null || FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString())
                {
                    tmrBaseRefresh.Enabled = false;
                    return;
                }

                TextBox _txt = null;
                Label _lbl = null;
                string _objectName = string.Empty;
                string _objectValue = string.Empty;
                string _cstSeqNo = string.Empty;
                string _jobSeqNo = string.Empty;
                string _key = string.Empty;
                string _rbName = string.Empty;
                string _stageID = string.Empty;
                string _display=string.Empty ;
                int _seqNo = 0;
                int _armJobQty = 0;
                Port _port = null;

                #region 顯示Robot Message -- 改至 tmrRefresh處理，不會因為切換視窗而停止更新
               
                //string msg = string.Empty;


                //#region Test
                ////string _dtTest = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fffff");
                ////string _type = _dtTest.Substring(24,1)=="0" ? "Error" : (_dtTest.Substring(24,1)== "5" ? "Warn":"");
                ////string _test = string.Format("[{0}] {1}^{2}", _dtTest, " Robot Command Test Data", _type);
                ////OPIInfo.Q_RobotCommand.Enqueue(_test);
                //#endregion

                //while (OPIInfo.Q_RobotMessage.Count > 0)
                //{

                //    lock (OPIInfo.Q_RobotMessage)
                //    {
                //        msg = OPIInfo.Q_RobotMessage.Dequeue();
                //    }

                //    string[] _msgItem = msg.Split('^');

                //    if (_msgItem.Count() < 2) continue;

                //    //NLogManager.Logger.LogInfoWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("RobotCommandReport Dequeue [{0}]", msg));

                //    lsvRobotCommand.Items.Insert(0, new ListViewItem(new string[] { _msgItem[1],_msgItem[0]}));

                //    if (_msgItem[1].ToUpper() == "ERROR") lsvRobotCommand.Items[0].ForeColor = Color.Red;
                //    else if (_msgItem[1].ToUpper() == "WARN")  lsvRobotCommand.Items[0].ForeColor = Color.Blue;

                //    //NLogManager.Logger.LogInfoWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("RobotCommandReport Count [{0}] : {1} ", lsvRobotCommand.Items.Count.ToString(), msg));

                //    //保留50筆最新的紀錄
                //    if (lsvRobotCommand.Items.Count > OPIAp.RBCmdDisplayCnt)
                //        lsvRobotCommand.Items.RemoveAt(lsvRobotCommand.Items.Count - 1);
                //}

                #endregion

                #region 顯示Robot / Stage Status

                #region Robot
                foreach (Robot _rb in OPIAp.Dic_Robot.Values)
                {
                    if (_rb.ArmMaxJobCount > _armJobQty) _armJobQty = _rb.ArmMaxJobCount;

                    foreach (ArmInfo _arm in _rb.LstArms)
                    {
                        #region Arm Front TextBox 
                        //txt + RobotName + "_Arm03_Front"
                        _objectName = string.Format("txt{0}_{1}_Front", _rb.RobotName, _arm.ArmName);

                        _txt = (TextBox)flpRobot.Controls.Find(_objectName, false).FirstOrDefault();
                        if (_txt != null)
                        {
                            if (_txt.Tag.ToString() != _arm.TrackingData_Front.ToString())
                            {
                                _txt.Tag = _arm.TrackingData_Front.ToString();

                                TrackingDataColor(_txt, _arm.TrackingData_Front);
                            }

                            _cstSeqNo = _arm.CstSeqNo_Front == null || _arm.CstSeqNo_Front == string.Empty ? "0" : _arm.CstSeqNo_Front;
                            _jobSeqNo = _arm.JobSeqNo_Front == null || _arm.JobSeqNo_Front == string.Empty ? "0" : _arm.JobSeqNo_Front;

                            //_objectValue = (_rb.RobotArmCount == 4 ? "Front " : string.Empty) + string.Format("({0},{1})", _cstSeqNo, _jobSeqNo);
                            _objectValue =  string.Format("({0},{1})", _cstSeqNo, _jobSeqNo);

                            if (_txt.Text.ToString() != _objectValue)
                            {
                                _txt.Text = _objectValue;
                            }
                        }
                        #endregion

                        #region Arm Front Label - 顯示 job exist status
                        //lbl + RobotName + "_Arm03_Front"
                        _objectName = string.Format("lbl{0}_{1}_Front", _rb.RobotName, _arm.ArmName);

                        _lbl = (Label)flpRobot.Controls.Find(_objectName, false).FirstOrDefault();
                        if (_lbl != null)
                        {
                            if (_lbl.Tag.ToString() != _arm.JobExist_Front.ToString())
                            {
                                _lbl.Tag = _arm.JobExist_Front.ToString();

                                RobotJobStatusColor(_lbl, _arm.JobExist_Front);
                            }
                        }
                        #endregion

                        #region Arm Back
                        //txt + RobotName + "_Arm03_Back"
                        _objectName = string.Format("txt{0}_{1}_Back", _rb.RobotName, _arm.ArmName);
                        _txt = (TextBox)flpRobot.Controls.Find(_objectName, false).FirstOrDefault();
                        if (_txt != null)
                        {
                            if (_txt.Tag.ToString() != _arm.TrackingData_Back.ToString())
                            {
                                _txt.Tag = _arm.TrackingData_Back.ToString();

                                TrackingDataColor(_txt, _arm.TrackingData_Back);
                            }

                            _cstSeqNo = _arm.CstSeqNo_Back == null || _arm.CstSeqNo_Back == string.Empty ? "0" : _arm.CstSeqNo_Back;
                            _jobSeqNo = _arm.JobSeqNo_Back == null || _arm.JobSeqNo_Back == string.Empty ? "0" : _arm.JobSeqNo_Back;

                            //_objectValue = (_rb.RobotArmCount == 4 ? "Back  " : string.Empty) + string.Format("({0},{1})", _cstSeqNo, _jobSeqNo);
                            _objectValue =  string.Format("({0},{1})", _cstSeqNo, _jobSeqNo);

                            if (_txt.Text.ToString() != _objectValue)
                            {
                                _txt.Text = _objectValue;
                            }
                        }
                        #endregion

                        #region Arm Back Label - 顯示 job exist status
                        //lbl + RobotName + "_Arm03_Front"
                        _objectName = string.Format("lbl{0}_{1}_Back", _rb.RobotName, _arm.ArmName);

                        _lbl = (Label)flpRobot.Controls.Find(_objectName, false).FirstOrDefault();
                        if (_lbl != null)
                        {
                            if (_lbl.Tag.ToString() != _arm.JobExist_Back.ToString())
                            {
                                _lbl.Tag = _arm.JobExist_Back.ToString();

                                RobotJobStatusColor(_lbl, _arm.JobExist_Back);
                            }
                        }
                        #endregion
                    }
                }
                #endregion

                #region Stage (not include port)
                foreach (Label _stageLabel in flpStage.Controls.OfType<Label>())
                {
                    //_stage.RobotName, _stage.StageID.PadLeft(2, '0'), i.ToString().PadLeft(2,'0')
                    string[] _items = _stageLabel.Name.Substring(3).Split('_');

                    if (_items.Count() != 3) continue ;

                    RobotStage _stage = OPIAp.Lst_RobotStage.Where(r => r.StageID.Equals(_items[1])).FirstOrDefault();

                    if (_stage == null) { ResetStageObject(_stageLabel.Name.Substring(3), flpStage,eRobotStageStatus.UnKnown); continue; }

                    StageJobData _job = _stage.Lst_JobData.Where(r => r.SlotNo.Equals(_items[2])).FirstOrDefault();

                    if (_job == null) { ResetStageObject(_stageLabel.Name.Substring(3), flpStage, _stage.StageStatus); continue; }

                    #region Stage Label - Stage Status
                    if (_stageLabel.Tag.ToString() != _stage.StageStatus.ToString())
                    {
                        _stageLabel.Tag = _stage.StageStatus.ToString();

                        RobotStageStatusColor(_stageLabel, _stage.StageStatus);
                    }
                    #endregion

                    #region Stage TextBox
                    _objectName = string.Format("txt{0}_{1}_{2}", _items[0], _items[1], _items[2]);

                    _txt = (TextBox)flpStage.Controls.Find(_objectName, false).FirstOrDefault();

                    if (_txt != null)
                    {
                        if (_txt.Tag.ToString() != _job.TrackingData.ToString())
                        {
                            _txt.Tag = _job.TrackingData.ToString();

                            TrackingDataColor(_txt, _job.TrackingData);
                        }

                        _cstSeqNo = _job.CstSeqNo == null || _job.CstSeqNo == string.Empty ? "0" : _job.CstSeqNo;
                        _jobSeqNo = _job.JobSeqNo == null || _job.JobSeqNo == string.Empty ? "0" : _job.JobSeqNo;

                        _objectValue = string.Format("({0},{1})", _cstSeqNo, _jobSeqNo);

                        if (_txt.Text.ToString() != _objectValue)
                        {
                            _txt.Text = _objectValue;
                        }
                    }

                    #endregion
                }

                //foreach (RobotStage _stage in OPIAp.Lst_RobotStage.Where(r=>r.StageType!="PORT"))
                //{
                //    foreach (StageJobData _job in _stage.Lst_JobData)
                //    {
                //        #region Stage Label - Stage Status
                //        _objectName = string.Format("lbl{0}_{1}_{2}", _stage.RobotName, _stage.StageID.PadLeft(2, '0'), _job.SlotNo.PadLeft(2, '0'));

                //        _lbl = (Label)flpStage.Controls.Find(_objectName, false).FirstOrDefault();

                //        if (_lbl != null)
                //        {
                //            if (_lbl.Tag.ToString() != _stage.StageStatus.ToString())
                //            {
                //                _lbl.Tag = _stage.StageStatus.ToString();

                //                RobotStageStatusColor(_lbl, _stage.StageStatus);
                //            }
                //        }
                //        //else ResetStageObject(
                //        #endregion

                //        #region Stage TextBox
                //        _objectName = string.Format("txt{0}_{1}_{2}", _stage.RobotName, _stage.StageID.PadLeft(2, '0'), _job.SlotNo.PadLeft(2, '0'));

                //        _txt = (TextBox)flpStage.Controls.Find(_objectName, false).FirstOrDefault();

                //        if (_txt != null)
                //        {
                //            if (_txt.Tag.ToString() != _job.TrackingData.ToString())
                //            {
                //                _txt.Tag = _job.TrackingData.ToString();

                //                TrackingDataColor(_txt, _job.TrackingData);
                //            }

                //            _cstSeqNo = _job.CstSeqNo == null || _job.CstSeqNo == string.Empty ? "0" : _job.CstSeqNo;
                //            _jobSeqNo = _job.JobSeqNo == null || _job.JobSeqNo == string.Empty ? "0" : _job.JobSeqNo;

                //            _objectValue = string.Format("({0},{1})", _cstSeqNo, _jobSeqNo);

                //            if (_txt.Text.ToString() != _objectValue)
                //            {
                //                _txt.Text = _objectValue;
                //            }
                //        }

                //        #endregion
                //    }
                //}
                #endregion

                #region Port Stage
                
                foreach (Label _stageLabel in flpPortStage.Controls.OfType<Label>())
                {
                    //_stage.RobotName, _stage.StageID.PadLeft(2, '0'), i.ToString().PadLeft(2,'0')
                    string[] _items = _stageLabel.Name.Substring(3).Split('_');

                    if (_items.Count() != 3) continue;

                    RobotStage _stage = OPIAp.Lst_RobotStage.Where(r => r.StageID.Equals(_items[1])).FirstOrDefault();

                    if (_stage == null) { ResetStageObject(_stageLabel.Name.Substring(3), flpPortStage, eRobotStageStatus.UnKnown); continue; }

                    int.TryParse(_items[2],out _seqNo );

                    if (_stage.Lst_JobData.Count() < (_seqNo)) { ResetStageObject(_stageLabel.Name.Substring(3), flpPortStage, _stage.StageStatus); continue; }

                    StageJobData _job = _stage.Lst_JobData[_seqNo-1];

                    #region Stage Label - Stage Status & Label Caption
                    if (_stageLabel.Tag.ToString() != _stage.StageStatus.ToString())
                    {
                        _stageLabel.Tag = _stage.StageStatus.ToString();

                        RobotStageStatusColor(_stageLabel, _stage.StageStatus);
                    }

                    _display = string.Format("{0}-{1}-{2}", _stage.StageID, _stage.StageName, _job.SlotNo.ToString().PadLeft(3, '0'));

                    if (_stageLabel.Text.ToString() != _display)
                    {
                        _stageLabel.Text = _display;
                    }

                    #endregion

                    #region Stage TextBox
                    _objectName = string.Format("txt{0}_{1}_{2}", _items[0], _items[1], _items[2]);

                    _txt = (TextBox)flpPortStage.Controls.Find(_objectName, false).FirstOrDefault();

                    if (_txt != null)
                    {
                        if (_txt.Tag.ToString() != _job.TrackingData.ToString())
                        {
                            _txt.Tag = _job.TrackingData.ToString();

                            TrackingDataColor(_txt, _job.TrackingData);
                        }

                        _cstSeqNo = _job.CstSeqNo == null || _job.CstSeqNo == string.Empty ? "0" : _job.CstSeqNo;
                        _jobSeqNo = _job.JobSeqNo == null || _job.JobSeqNo == string.Empty ? "0" : _job.JobSeqNo;

                        _objectValue = string.Format("({0},{1})", _cstSeqNo, _jobSeqNo);

                        if (_txt.Text.ToString() != _objectValue)
                        {
                            _txt.Text = _objectValue;
                        }
                    }

                    #endregion                     
                }

                #endregion

                #endregion

                #region Robot command 
                lock (OPIInfo.Lst_RobotCommand)
                {
                    foreach (RobotCommandReport.COMMANDc _cmd in OPIInfo.Lst_RobotCommand)
                    {
                        int.TryParse( _cmd.COMMAND_SEQ , out _seqNo);
                        _objectName = _cmd.COMMAND_SEQ + "RobotCommand";

                        if (_cmd.ROBOT_COMMAND == string.Empty)
                            _display = string.Empty;
                        else
                        {
                            if (_armJobQty == 1)
                            {
                                _display = string.Format("{0}. {1} Cassette Seq No [{2}], Job Seq No [{3}], Robot Command [{4}], Arm [{5}], Position [{6}], Slot No [{7}]",
                                   _cmd.COMMAND_SEQ, _cmd.COMMAND_DATETIME, _cmd.CASSETTESEQNO, _cmd.JOBSEQNO, _cmd.ROBOT_COMMAND, _cmd.ARM_SELECT, _cmd.TARGETPOSITION, _cmd.TARGETSLOTNO);
                            }
                            else
                            {
                                //_display = string.Format("{0}. {1} Front Cassette Seq No [{2}], Front Job Seq No [{3}], Back Cassette Seq No [{8}], Back Job Seq No [{9}],Robot Command [{4}], Arm [{5}], Position [{6}], Slot No [{7}]",
                                //  _cmd.COMMAND_SEQ, _cmd.COMMAND_DATETIME, _cmd.CASSETTESEQNO, _cmd.JOBSEQNO, _cmd.ROBOT_COMMAND, _cmd.ARM_SELECT, _cmd.TARGETPOSITION, _cmd.TARGETSLOTNO, _cmd.CASSETTESEQNO_BACK, _cmd.JOBSEQNO_BACK);
                                _display = string.Format("{0}. {1} Front Job ({2},{3}), Back Job ({8},{9}),Robot Command [{4}], Arm [{5}], Position [{6}], Slot No [{7}]",
                                  _cmd.COMMAND_SEQ, _cmd.COMMAND_DATETIME, _cmd.CASSETTESEQNO, _cmd.JOBSEQNO, _cmd.ROBOT_COMMAND, _cmd.ARM_SELECT, _cmd.TARGETPOSITION, _cmd.TARGETSLOTNO, _cmd.CASSETTESEQNO_BACK, _cmd.JOBSEQNO_BACK);
                            }
                        }

                        if (lstRobotCommand.Items.Count < (_seqNo - 1)) continue;
                        else lstRobotCommand.Items[_seqNo-1] = _display;
                    }
                }

                #endregion

                #region 顯示Robot Unloader Dispatch
                foreach (DataGridViewRow _row in dgvDispatch.Rows)
                {
                    _key = _row.Cells[colDispatch_PortKey.Name].Value.ToString();
                    _port = OPIAp.Dic_Port[_key];

                    _row.Cells[colDispatch_Grade01.Name].Value = _port.RobotUnloaderDispatch.Grade01.ToString();
                    _row.Cells[colDispatch_Grade02.Name].Value = _port.RobotUnloaderDispatch.Grade02.ToString();
                    _row.Cells[colDispatch_Grade03.Name].Value = _port.RobotUnloaderDispatch.Grade03.ToString();
                    //_row.Cells[colDispatch_Grade04.Name].Value = _port.RobotUnloaderDispatch.Grade04.ToString();

                    _row.Cells[colDispatch_OperatorID.Name].Value = _port.RobotUnloaderDispatch.OperatorID.ToString();
                }
                #endregion
            }
            catch (Exception ex)
            {
                tmrBaseRefresh.Enabled = false;

                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                string _objectName = string.Empty;
                string _objectValue = string.Empty;
                string _cstSeqNo = string.Empty;
                string _jobSeqNo = string.Empty;
                string _key = string.Empty;
                string _rbName = string.Empty;
                string _stageID = string.Empty;
                string _display = string.Empty;

                #region 顯示Robot Message

                string msg = string.Empty;


                #region Test
                //string _dtTest = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fffff");
                //string _type = _dtTest.Substring(24,1)=="0" ? "Error" : (_dtTest.Substring(24,1)== "5" ? "Warn":"");
                //string _test = string.Format("[{0}] {1}^{2}", _dtTest, " Robot Command Test Data", _type);
                //OPIInfo.Q_RobotCommand.Enqueue(_test);
                #endregion

                while (OPIInfo.Q_RobotMessage.Count > 0)
                {

                    lock (OPIInfo.Q_RobotMessage)
                    {
                        msg = OPIInfo.Q_RobotMessage.Dequeue();
                    }

                    string[] _msgItem = msg.Split('^');

                    if (_msgItem.Count() < 2) continue;

                    lsvRobotCommand.Items.Insert(0, new ListViewItem(new string[] { _msgItem[1], _msgItem[0] }));

                    if (_msgItem[1].ToUpper() == "ERROR") lsvRobotCommand.Items[0].ForeColor = Color.Blue;
                    else if (_msgItem[1].ToUpper() == "WARN") lsvRobotCommand.Items[0].ForeColor = Color.Black;

                    //保留50筆最新的紀錄
                    if (lsvRobotCommand.Items.Count > OPIAp.RBCmdDisplayCnt)
                        lsvRobotCommand.Items.RemoveAt(lsvRobotCommand.Items.Count - 1);
                }

                #endregion
            }
            catch (Exception ex)
            {
                tmrRefresh.Enabled = false;

                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ResetStageObject(string objectName,FlowLayoutPanel flowPanel,eRobotStageStatus stageStatus)
        {
            try
            {
                Label _lbl = (Label)flowPanel.Controls.Find("lbl"+objectName, false).FirstOrDefault();

                if (_lbl != null)
                {
                    if (_lbl.Tag.ToString() != stageStatus.ToString())
                    {
                        _lbl.Tag = stageStatus.ToString();

                        RobotStageStatusColor(_lbl, stageStatus);
                    }
                }

                TextBox _txt = (TextBox)flowPanel.Controls.Find("txt" + objectName, false).FirstOrDefault();

                if (_txt != null)
                {
                    if (_txt.Tag.ToString() != string.Empty)
                    {
                        _txt.Tag = string.Empty;

                        TrackingDataColor(_txt, string.Empty);
                    }
                    _txt.Text = "(0,0)";
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialArmLabel(FlowLayoutPanel flp,string name, string caption, int lblWidth, bool isStage)
        {
            try
            {
                //Lable: 
                //     Robot => lbl + RobotName + "_" + Arm03" 
                //     Stage => lbl + RobotName + "_" + _stage.StageID.PadLeft(2, '0')
                Label _lbl = new Label();
                _lbl.Name = name;
                _lbl.Text = caption;
                _lbl.Font = new Font("Calibri", 12);
                _lbl.TextAlign = ContentAlignment.MiddleLeft;
                _lbl.BackColor = Color.Black; //Color.DimGray;
                _lbl.ForeColor = Color.White;
                _lbl.Size = new Size(lblWidth, 25);
                _lbl.Tag = "";
                _lbl.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
                _lbl.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
                flp.Controls.Add(_lbl);

                if (isStage)
                {
                    _lbl.DoubleClick += new EventHandler(RobotStage_DoubleClick);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialArmTxt(FlowLayoutPanel flp,string name, int txtWidth)
        {
            try
            {
                //TextBox: 
                //     Robot => txt + RobotName + "_" + Arm03" 
                //     Stage => txt + RobotName + "_" + _stage.StageID.PadLeft(2, '0')
                TextBox _txt = new TextBox();
                _txt.Name = name;
                _txt.Font = new Font("Calibri", 12);
                _txt.TextAlign = HorizontalAlignment.Center;
                _txt.Size = new Size(txtWidth, 25);
                _txt.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);  //new System.Windows.Forms.Padding(1);
                _txt.Padding = new System.Windows.Forms.Padding(0, 0, 0, 0);
                _txt.ReadOnly = true;
                _txt.Tag = string.Empty;
                _txt.DoubleClick += new EventHandler(JobDataQuery_DoubleClick);
                flp.Controls.Add(_txt);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void RobotStageStatusColor(Label lbl, eRobotStageStatus Status)
        {
            try
            {
                #region 顯示Robot / Stage Status

                switch (Status)
                {
                    case eRobotStageStatus.UnKnown:
                        lbl.BackColor = Color.Firebrick;
                        break;
                    case eRobotStageStatus.NoRquest:  //無RB服務
                        lbl.BackColor = Color.MediumTurquoise;
                        break;
                    case eRobotStageStatus.LDRQ:  //可收片
                        lbl.BackColor = Color.DimGray;
                        break;
                    case eRobotStageStatus.UDRQ: //有片
                        lbl.BackColor = Color.Green;
                        break;
                    case eRobotStageStatus.LDRQ_UDRQ: //LDRQ_UDRQ
                        lbl.BackColor = Color.MediumBlue;
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void RobotJobStatusColor(Label lbl, eRobotJobStatus Status)
        {
            try
            {
                #region 顯示Robot / Stage Status

                switch (Status)
                {
                    case eRobotJobStatus.UnKnown: //0：Unknown
                        lbl.BackColor = Color.Firebrick;
                        break;
                    case eRobotJobStatus.NoExist: //1：No Exist(bit0)
                        lbl.BackColor = Color.DimGray;
                        break;
                    case eRobotJobStatus.Exist: //2：Exist(bit1)
                        lbl.BackColor = Color.Green;
                        break;
                    case eRobotJobStatus.ArmDisabled: //4：Arm Disabled(bit2)
                        lbl.BackColor = Color.Blue;                        
                        break ;
                    case eRobotJobStatus.ArmDisabledNoExist: //5：Arm Disabled & No Exist Job
                        lbl.BackColor = Color.Orange;    
                        break ;
                    case eRobotJobStatus.ArmDisableExist: //6：Arm Disable & Exist Job
                        lbl.BackColor = Color.Fuchsia;    
                        break ;
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void TrackingDataColor(TextBox txt, string trackingData)
        {
            try
            {
                #region 顯示Tracking Data Color
                //0：Not Processed (Not In EQ) => Yellow
                //1：Normal Processed (In EQ) =>Green
                //2：Abnormal Processed (In EQ) => Orange
                //3：Process Skip (In EQ) => Gray

                if (trackingData.Contains("3"))
                {
                    txt.BackColor = Color.Gray;
                }
                else if (trackingData.Contains("2"))
                {
                    txt.BackColor = Color.Orange;
                }
                else if (trackingData.Equals("1"))
                {
                    txt.BackColor = Color.Green;
                }
                else if (trackingData.Equals("0"))
                {
                    txt.BackColor = Color.Yellow;
                }
                else
                    txt.BackColor = Color.White;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnQueryCommand_Click(object sender, EventArgs e)
        {
            try
            {
                FormRobotCommandHis _frm = new FormRobotCommandHis(OPIAp);
                _frm.ShowDialog();

                if (_frm != null) _frm.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRefreshStage_Click(object sender, EventArgs e)
        {
            try
            {
                string _xml = string.Empty;
                string _err = string.Empty;

                #region Send RobotStageInfoRequest
                RobotStageInfoRequest _robotStageInfoRequest = new RobotStageInfoRequest();
                _robotStageInfoRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _robotStageInfoRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _xml = _robotStageInfoRequest.WriteToXml();
                
                if (FormMainMDI.SocketDriver.SendMessage(_robotStageInfoRequest.HEADER.TRANSACTIONID, _robotStageInfoRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID) == false)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format("Robot Stage Info Request Error [{0}]", _err), MessageBoxIcon.Warning);
                }

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
