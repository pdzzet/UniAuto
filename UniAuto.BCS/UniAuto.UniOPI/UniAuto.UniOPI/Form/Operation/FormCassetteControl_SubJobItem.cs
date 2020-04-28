using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormCassetteControl_SubJobItem : FormBase
    {
        public string[] SubItemData;
        string OldSubJobData;
        bool SingleChoose = false; //是否只能選擇其一 (不能複選)

        public FormCassetteControl_SubJobItem(string subJobItemKey, string subJobData, bool singleChoose)
        {
            InitializeComponent();

            OldSubJobData = subJobData;

            SingleChoose = singleChoose;

            InitialObject(subJobItemKey);

            lblCaption.Text = subJobItemKey;            
        }

        private void InitialObject(string subJobItemKey)
        {
            try
            {
                #region 根據DB資料建立畫面物件
                string _value = string.Empty;

                List<SBRM_SUBJOBDATA> _lstSubJobData = FormMainMDI.G_OPIAp.DBCtx.SBRM_SUBJOBDATA.Where(d => d.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType && d.ITEMNAME == subJobItemKey).OrderBy(d => d.SUBITEMLOFFSET).ToList();

                foreach (SBRM_SUBJOBDATA data in _lstSubJobData)
                {

                    if (data.SUBITEMNAME.IndexOf("Reserved", 0) > -1) continue;

                    //RecipeGroupEndFlag 由機台給，不需提供設定-- 2015/3/27 俊成提出ㄋ
                    if (data.SUBITEMNAME == "RecipeGroupEndFlag") continue;

                    if (SubItemData == null)
                    {
                        SubItemData = new string[data.ITEMLENGTH];

                        for (int i = 0; i < SubItemData.Length; i++) SubItemData[i] = "0";

                        if (OldSubJobData.Length != data.ITEMLENGTH) OldSubJobData = string.Empty;
                    }

                    if (SingleChoose)
                    {
                        #region radio button
                        RadioButton _rdo = new RadioButton();
                        _rdo.Tag = data.SUBITEMLOFFSET;
                        _rdo.Text = data.SUBITEMDESC;
                        _rdo.Checked = false;
                        _rdo.Font = new Font("Calibri", 12, FontStyle.Regular);
                        _rdo.AutoSize = false;
                        _rdo.Width = 340;
                        flpSubJobItems.Controls.Add(_rdo);

                        if (OldSubJobData != string.Empty)
                        {
                            _value = OldSubJobData.Substring(data.SUBITEMLOFFSET, data.SUBITEMLENGTH);
                            SubItemData[data.SUBITEMLOFFSET] = _value;

                            if (_value == "1") _rdo.Checked = true;
                        }
                        #endregion
                    }
                    else
                    {
                        if (data.SUBITEMLENGTH > 1)
                        {
                            string _defaultData = string.Empty;

                            if (OldSubJobData != string.Empty)
                            {
                                _value = UniTools.ReverseStr(OldSubJobData.Substring(data.SUBITEMLOFFSET, data.SUBITEMLENGTH));
                                _defaultData = Convert.ToInt32(_value, 2).ToString();
                            }

                            #region Label
                            Label _lbl = new Label();
                            _lbl.Width = 340;
                            _lbl.Font = new Font("Calibri", 12, FontStyle.Regular);
                            _lbl.Text = data.SUBITEMDESC;
                            _lbl.TextAlign = ContentAlignment.MiddleLeft;
                            flpSubJobItems.Controls.Add(_lbl);
                            #endregion

                            #region combobox
                            ComboBox _cbo = new ComboBox();
                            _cbo.Tag = data.SUBITEMLOFFSET + "," + data.SUBITEMLENGTH;
                            _cbo.Width = 340;
                            _cbo.Font = new Font("Calibri", 12, FontStyle.Regular);
                            _cbo.DropDownStyle = ComboBoxStyle.DropDownList;
                            flpSubJobItems.Controls.Add(_cbo);

                            setComboBoxItem(_cbo, data.MEMO, _defaultData);

                            #endregion
                        }
                        else
                        {
                            #region check box
                            CheckBox _chk = new CheckBox();
                            _chk.Tag = data.SUBITEMLOFFSET;
                            _chk.Text = data.SUBITEMDESC;
                            _chk.Checked = false;
                            _chk.Font = new Font("Calibri", 12, FontStyle.Regular);
                            _chk.AutoSize = false;
                            _chk.Width = 340;
                            flpSubJobItems.Controls.Add(_chk);

                            if (OldSubJobData != string.Empty)
                            {
                                _value = OldSubJobData.Substring(data.SUBITEMLOFFSET, data.SUBITEMLENGTH);
                                SubItemData[data.SUBITEMLOFFSET] = _value;

                                if (_value == "1") _chk.Checked = true;
                            }
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
        }

        private void setComboBoxItem(ComboBox cbo, string items, string defaultData)
        {
            try
            {
                string[] _item = items.Split(',');

                List<comboxInfo> _lstCboInfo = new List<comboxInfo>();

                foreach (string _data in _item)
                {
                    string[] _detail = _data.Split(':');

                    if (_detail.Length != 2) continue;

                    _lstCboInfo.Add(new comboxInfo { ITEM_ID = _detail[0], ITEM_NAME = _detail[1] });
                }

                cbo.DataSource = _lstCboInfo.ToList();
                cbo.DisplayMember = "ITEM_DESC";
                cbo.ValueMember = "ITEM_ID";
                cbo.SelectedIndex = -1;

                if (defaultData == string.Empty) return;

                if (_lstCboInfo.Select(r => r.ITEM_ID.Equals(defaultData)).Count() > 0)
                {
                    cbo.SelectedValue = defaultData;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {            
            try
            {
                int _offset = 0;
                int _len = 0;
                int _value = 0;

                foreach (Control _ctrl in flpSubJobItems.Controls)
                {
                    if (_ctrl.Tag == null) continue;

                    if (SingleChoose)
                    {
                        RadioButton _rdo = (RadioButton)_ctrl;

                        int.TryParse(_rdo.Tag.ToString(),out _offset);

                        SubItemData[_offset] = (_rdo.Checked ? "1" : "0");
                    }
                    else
                    {
                        switch (_ctrl.GetType().Name.ToString())
                        {
                            case "CheckBox":
                                CheckBox _chk = (CheckBox)_ctrl;

                                int.TryParse(_chk.Tag.ToString(), out _offset);

                                SubItemData[_offset] = (_chk.Checked ? "1" : "0");
                                break;

                            case "ComboBox":
                                ComboBox _cbo = (ComboBox)_ctrl;

                                if (_cbo.SelectedValue == null) _value = 0;
                                else
                                {
                                    //SUBITEMLOFFSET, SUBITEMLENGTH
                                    string[] _set = _cbo.Tag.ToString().Split(',');

                                    int.TryParse(_set[0], out _offset);
                                    int.TryParse(_set[1], out _len);

                                    int.TryParse(_cbo.SelectedValue.ToString(), out _value);

                                    string _bin = UniTools.ReverseStr(Convert.ToString(_value, 2).PadLeft(_len, '0'));

                                    for (int i = 0; i < _bin.Length; i++)
                                    {
                                        SubItemData[_offset + i] = _bin.Substring(i,1);
                                    }
                                }
                                break ;
                            default :
                                break ;
                        }
                    }
                }

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
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

            this.Close();
        }
    }
}
