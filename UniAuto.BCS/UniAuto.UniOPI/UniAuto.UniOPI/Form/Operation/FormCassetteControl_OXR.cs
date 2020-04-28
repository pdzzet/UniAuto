using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormCassetteControl_OXR : FormBase
    {
        private const string OXR = "OXR";
        public string OXRInfo = string.Empty;
        private int ChipCount = 400;
        List<comboxInfo> _lstOXR = null;

        public FormCassetteControl_OXR(string Oxr,string OxrItem)
        {
            InitializeComponent();

            if (FormMainMDI.G_OPIAp.CurLine.FabType == "CF")
            {
                ChipCount = 1200;
                txtChipsCount.MaxLength = 4;
            }

            #region 建立OXR對應item
            _lstOXR = new List<comboxInfo>();

            string[] _items = OxrItem.Split(',');

            foreach (string _item in _items)
            {
                string[] _det = _item.Split(':');

                if (_det.Count() < 1) continue;

                if (_det.Count() == 1)
                {
                    comboxInfo _jobItem = new comboxInfo();

                    _jobItem.ITEM_ID = _det[0];
                    _jobItem.ITEM_NAME = _det[0];
                    _lstOXR.Add(_jobItem);
                }
                else
                {
                    comboxInfo _jobItem = new comboxInfo();

                    _jobItem.ITEM_ID = _det[0];
                    _jobItem.ITEM_NAME = _det[1];
                    _lstOXR.Add(_jobItem);
                }
            }

            //建立OXR Item
            if (_lstOXR != null && _lstOXR.Count > 0)
            {
                InitialObject();
            }
            #endregion

            #region Initial OXR Info
            if (Oxr.Trim() == string.Empty)
            {
                txtChipsCount.Text = "9"; // "56";

                SetChipCount(string.Empty );

                OXRInfo = GetOXRInfo();
            }
            else
            {
                txtChipsCount.Text = Oxr.Trim().Length.ToString();

                SetChipCount(Oxr.Trim());
            }
            #endregion
        }

        private void InitialObject()
        {
            try
            {
                #region 根據DB資料建立畫面物件-- OXR Information

                foreach (comboxInfo _data in _lstOXR)
                {
                    RadioButton _rdo = new RadioButton();
                    _rdo.Name = _data.ITEM_ID;
                    _rdo.Text = (_data.ITEM_ID == _data.ITEM_NAME) ? _data.ITEM_NAME : _data.ITEM_DESC;
                    _rdo.Tag = _data.ITEM_ID;
                    _rdo.Size = new System.Drawing.Size(150, 25);
                    _rdo.AutoSize = false;
                    flpOXR.Controls.Add(_rdo);

                    if (flpOXR.Controls.Count == 1) _rdo.Checked = true;
                }

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private char GetOXRFlag()
        {
            try
            {
                char _rdoData = ' ';

                #region Get Radion Data
                var _var = from obj in flpOXR.Controls.OfType<RadioButton>()
                           where obj.Checked == true
                           select obj;

                if (_var.Count() > 0)
                {
                    RadioButton _rdo = (RadioButton)_var.First();

                    _rdoData = _rdo.Tag.ToString()[0];
                }

                return _rdoData;

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                return ' ';
            }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender ;

                char _rdoData = GetOXRFlag();

                if (_btn.Tag.ToString() == "All")
                {
                    foreach (ucOXRInfo _ucOXR in flpChips.Controls.OfType<ucOXRInfo>())
                    {
                        _ucOXR.InitialOXR(string.Empty, _rdoData);
                    }

                    //foreach (Panel _pnl in flpChips.Controls.OfType<Panel>())
                    //{
                    //    if (_pnl.Visible == false) break;

                    //    foreach (TextBox _txt in _pnl.Controls.OfType<TextBox>())
                    //    {
                    //        _txt.Text = string.Empty.PadLeft(_txt.MaxLength, _rdoData);
                    //    }
                    //}
                }
                //else
                //{
                //    string _pnlName = "pnlChip" + _btn.Tag.ToString();
                //    string _txtName = "txtChip" + _btn.Tag.ToString();

                //    TextBox _txt = flpChips.Controls.Find(_txtName, true).OfType<TextBox>().First();

                //    _txt.Text =  string.Empty.PadLeft(_txt.MaxLength,_rdoData);
                //}
               
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            OXRInfo = GetOXRInfo();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
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

        private void txtChips_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                int _num = 0;

                int.TryParse(_txt.Text.ToString(), out _num);


                if (_num > ChipCount || _num < 1)
                {
                    ShowMessage(this, lblCaption.Text , "", string.Format(" 1 <= Chips Count <= {0}",ChipCount.ToString()), MessageBoxIcon.Error);

                    txtChipsCount.Text = string.Empty;

                    txtChipsCount.Focus();

                    return;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void btnSetCount_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                int _cnt = 0;

                int.TryParse(txtChipsCount.Text, out _cnt);

                if (_cnt > ChipCount || _cnt < 1)
                {
                    ShowMessage(this, lblCaption.Text, "", string.Format(" 1 <= Chips Count <= {0}",ChipCount.ToString()), MessageBoxIcon.Error);

                    txtChipsCount.Text = string.Empty;

                    txtChipsCount.Focus();

                    return;
                }

                SetChipCount(string.Empty);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnChipSet_Click(object sender, EventArgs e)
        {
            char _rdoData = GetOXRFlag();

            Button _bnt = (Button)sender;

           ucOXRInfo _ucOXR = (ucOXRInfo)(_bnt.Parent);

           _ucOXR.InitialOXR(string.Empty, _rdoData);
        }
        
        private void SetChipCount(string oxrInfo)
        {
            try
            {
                int _chipCnt = 0;
                int _len = 0;
                //int _seq = 0;
                string _temp = string.Empty ;
                //TextBox _txt = null;
                char _flag = GetOXRFlag();

                int.TryParse(txtChipsCount.Text.ToString(), out _chipCnt);

                int _ucCount = _chipCnt / 10 ;
                
                if (_chipCnt - (_ucCount * 10) > 0 ) _ucCount = _ucCount + 1;

                flpChips.Controls.Clear();

                for (int i = 0; i < _ucCount; i++)
                {
                    if (i * 10 + 10 > _chipCnt) _len = _chipCnt - (i * 10);
                    else _len = 10;

                    ucOXRInfo _ucOXR = new ucOXRInfo(i, _len);

                    _ucOXR.btnChipSet.Click +=new EventHandler(btnChipSet_Click);

                    _ucOXR.InitialOXR(oxrInfo, _flag);

                    flpChips.Controls.Add(_ucOXR);
                }


                //foreach (Panel _pnl in flpChips.Controls.OfType<Panel>())
                //{
                //    //pnlChip001
                //    _temp = _pnl.Name.Substring(7,3);
                //    int.TryParse(_temp,out _seq);

                //    _txt = _pnl.Controls.Find("txtChip"+_temp,false).OfType<TextBox>().First();

                //    if (_seq > _cnt)
                //    {                        
                //        _pnl.Visible = false;

                //        _txt.Text  = string.Empty;
                //    }
                //    else
                //    {
                //        _pnl.Visible = true;

                //        if (_seq + 9 > _cnt)
                //        {
                //            _txt.MaxLength = (_cnt % 10);

                //            if (oxrInfo == string.Empty)
                //                _txt.Text = string.Empty.PadLeft(_cnt % 10, _flag);
                //            else
                //                _txt.Text = oxrInfo.Substring(_seq-1);
                //        }
                //        else
                //        {
                //            _txt.MaxLength = 10;

                //            if (oxrInfo == string.Empty)
                //                _txt.Text = string.Empty.PadLeft(10, _flag);
                //            else
                //               _txt.Text = oxrInfo.Substring(_seq-1, 10);
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

        private string GetOXRInfo()
        {            
            try
            {
                //int _cnt = 0;
                //int.TryParse(txtChipsCount.Text.ToString(), out _cnt);

                string _info = string.Empty ;
                string _no = string.Empty ;
                string _tmp = string.Empty ;
                //TextBox _txt = null;

                foreach (ucOXRInfo _ucOXR in flpChips.Controls.OfType<ucOXRInfo>())
                {
                    _tmp = _ucOXR.GetOXR();

                    _info = _info + _tmp;
                }

                //foreach (Panel _pnl in flpChips.Controls.OfType<Panel>())
                //{
                //    if (_pnl.Visible == false) break;

                //    _no = _pnl.Name.Substring(7,3);

                //    _txt = _pnl.Controls.Find("txtChip"+_no,false).OfType<TextBox>().First();

                //    _tmp =_txt.Text.ToString().Trim() ;
                //    if (_tmp.Length != _txt.MaxLength)                    
                //    {
                //        _txt.Text = _tmp.PadLeft(_txt.MaxLength, '0');
                //    }

                //    _info = _info + _txt.Text;
                //}

                return _info;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                
                return string.Empty;
            }
        }
    }
}
