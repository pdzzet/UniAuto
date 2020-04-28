using System;
using System.Linq;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class ucSemiCommand : UserControl
    {
        public int CommandSeqNo { get; set; }
        public Robot CurRobot;

        public ucSemiCommand()
        {
            InitializeComponent();
        }

        public void InitialCommand(int seqNo, Robot robot)
        {
            try
            {
                CurRobot = robot;
                CommandSeqNo = seqNo;

                grbArmCommand.Text = string.Format("  {0}'st Robot Command", seqNo.ToString());

                if (FormMainMDI.G_OPIAp.CurLine.FabType == "CELL") txtTargetSlotNo.MaxLength = 3;
                else txtTargetSlotNo.MaxLength = 2;

                #region initial Arm Command

                var _cmdVar = new[] 
                { 
                    new { Key = "1:Put", Value = "1" } ,
                    new { Key = "2:Get", Value = "2" } ,
                    new { Key = "4:Exchange", Value = "4" } ,
                    new { Key = "8:Put Ready", Value = "8" } ,
                    new { Key = "16:Get Ready", Value = "16" } ,
                    new { Key = "32:Get/Put", Value = "32" } ,
                    new { Key = "64:Multi-Put", Value = "64" } ,
                    new { Key = "128:Multi-Get", Value = "128" },
                    new { Key = "256:RTC-Put", Value = "256" }
                    
                };

                cboArmCmd.DataSource = _cmdVar;

                cboArmCmd.DisplayMember = "Key";
                cboArmCmd.ValueMember = "Value";
                cboArmCmd.SelectedIndex = -1;
                #endregion

                #region initial Arm Select
                if (CurRobot.RobotArmCount == 4)
                {                    
                    var data = new[] 
                    { 
                        new { Key = "1:Upper/Left Arm", Value = "1" } ,
                        new { Key = "2:Lower/Left Arm", Value = "2" } ,
                        new { Key = "3:Left Both Arm", Value = "3" } ,
                        new { Key = "4:Upper/Right Arm", Value = "4" }, 
                        new { Key = "5:Upper Both Arm", Value = "5" }, 
                        new { Key = "8:Lower/Right Arm", Value = "8" } ,
                        new { Key = "10:Lower Both Arm", Value = "10" } ,
                        new { Key = "12:Right Both Arm", Value = "12" } 
                    };

                    cboArmSelect.DataSource = data;
                }
                else
                {
                    var data = new[] 
                    { 
                        new { Key = "1:Upper Arm", Value = "1" } ,
                        new { Key = "2:Lower Arm", Value = "2" } ,
                        new { Key = "3:Both Arm", Value = "3" } 
                    };

                    cboArmSelect.DataSource = data;
                }

                cboArmSelect.DisplayMember = "Key";
                cboArmSelect.ValueMember = "Value";
                cboArmSelect.SelectedIndex = -1;
                #endregion

                #region initial Target Position
                var s = FormMainMDI.G_OPIAp.Lst_RobotStage.Where(r => r.RobotName.Equals(CurRobot.RobotName) ).
                    Select(r=> new comboxInfo()
                    {
                        ITEM_ID = r.StageID,
                        ITEM_NAME= r.StageName
                    } ) ;

                if (s == null || s.Count() == 0) return;

                cboTargetPosition.SelectedIndexChanged -= new EventHandler(cboTargetPosition_SelectedIndexChanged);
                cboTargetPosition.DataSource = s.ToList();
                cboTargetPosition.DisplayMember = "ITEM_DESC";
                cboTargetPosition.ValueMember = "ITEM_ID";
                cboTargetPosition.SelectedIndex = -1;
                cboTargetPosition.SelectedIndexChanged +=new EventHandler(cboTargetPosition_SelectedIndexChanged);

                #endregion

                EnableObject(false);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void EnableObject(bool enable)
        {
            //chkChoose.Enabled = enable;
            cboArmCmd.Enabled = enable;
            cboArmSelect.Enabled = enable;
            cboTargetPosition.Enabled = enable;
            txtTargetSlotNo.Enabled = enable;            
        }

        public string GetArmCommand()
        {
            try
            {
                if (cboArmCmd.SelectedValue == null) return string.Empty;

                return cboArmCmd.SelectedValue.ToString();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            //try
            //{
            //    var _var = flpArmCmd.Controls.OfType<RadioButton>().Where(r => r.Checked.Equals(true));

            //    if (_var.Count() == 0) return string.Empty;

            //    RadioButton _rdo = _var.First();

            //    if (_rdo.Tag == null) return string.Empty;

            //    return _rdo.Tag.ToString();
            //}
            //catch (Exception ex)
            //{
            //    throw (ex);                
            //}
        }

        public string GetArmSelect()
        {
            try
            {
                if (cboArmSelect.SelectedValue == null) return string.Empty;

                return cboArmSelect.SelectedValue.ToString();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public string GetTargetPosition()
        {
            try
            {
                if (cboTargetPosition.SelectedValue == null) return string.Empty;

                return cboTargetPosition.SelectedValue.ToString();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public int GetTargetSlotNo()
        {
            try
            {
                int _cnt = 0;

                int.TryParse(txtTargetSlotNo.Text.ToString(), out _cnt);

                return _cnt;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public int GetTargetSlotMaxCount()
        {
            int _cnt = 0;

            if (txtTargetSlotNo.Tag == null) return 0;

            int.TryParse(txtTargetSlotNo.Tag.ToString(), out _cnt);

            return _cnt;
        }

        private void cboTargetPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboTargetPosition.SelectedValue == null) return;

                string _stageID = cboTargetPosition.SelectedValue.ToString();

                RobotStage _stage = FormMainMDI.G_OPIAp.Lst_RobotStage.Where(r=>r.StageID.Equals(_stageID)).FirstOrDefault();

                lblSlotNoDesc.Text = string.Format("{0}~{1}", 1, _stage.SlotMaxCount);

                txtTargetSlotNo.Text = string.Empty;
                txtTargetSlotNo.Tag = _stage.SlotMaxCount;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            UniTools.CheckTextBoxKeyPressIsInteger(sender, e);
        }

        private void chkChoose_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox _chk = (CheckBox)sender;

                EnableObject(_chk.Checked);

                //foreach (RadioButton _rdo in flpArmCmd.Controls.OfType<RadioButton>())
                //{
                //    if (_rdo.Name == "rdoPut") _rdo.Checked = true;
                //    else _rdo.Checked = false;
                //}
                cboArmCmd.SelectedIndex = -1;
                cboArmSelect.SelectedIndex = -1;
                cboTargetPosition.SelectedIndex = -1;

                txtTargetSlotNo.TextChanged -=new EventHandler(txtTargetSlotNo_TextChanged);
                txtTargetSlotNo.Text = string.Empty;
                txtTargetSlotNo.TextChanged += new EventHandler(txtTargetSlotNo_TextChanged);

                lblSlotNoDesc.Text = string.Empty;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void txtTargetSlotNo_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int _maxCnt = 0;
                int _curCnt = 0;

                TextBox _txt = (TextBox)sender;

                int.TryParse(_txt.Tag.ToString(), out _maxCnt);
                int.TryParse(_txt.Text.ToString(), out _curCnt);

                if (_curCnt > _maxCnt || _curCnt < 1)
                {
                    FormMainMDI.FrmOPIMessage.ShowMessage(new Form(), "Robot Command", "", string.Format("{0}'st Robot Command Target Slot is not Between 1~{1}！", CommandSeqNo.ToString(), _maxCnt), MessageBoxIcon.Error);                 
                    txtTargetSlotNo.Focus();
                    txtTargetSlotNo.SelectAll();
                    return;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }
}
