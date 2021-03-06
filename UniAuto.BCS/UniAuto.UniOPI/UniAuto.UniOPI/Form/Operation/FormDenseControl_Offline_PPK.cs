﻿using System;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormDenseControl_Offline_PPK : FormBase
    {
        public Dense CurDense = new Dense();

        public FormDenseControl_Offline_PPK()
        {
            InitializeComponent();
        }

        private void FormDenseControl_Offline_PPK_Load(object sender, EventArgs e)
        {
            try
            {
                //初始化combobox items
                InitialCombox();

                lblPortName.Text = CurDense.PortNo.ToString().PadLeft(2, '0');
                txtBoxType.Text = CurDense.BoxType.ToString();
                txtBoxID.Text = CurDense.BoxID01.ToString();                
                txtPaperBoxID.Text = CurDense.PaperBoxID.ToString();
                
                //if (CurDense.DenseDataRequest)
                //{
                //    SetDefaultData();
                //}

                txtProductType.Focus();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //private void FormDenseControl_Offline_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    try
        //    {
        //        //DenseCtrlDefaultData _default = new DenseCtrlDefaultData();

        //        //_default.ProductType = txtProductType.Text;

        //        //_default.BoxID = txtBoxID.Text;
        //        //_default.JobGrade = cboJobGrade.Text;
        //        //_default.PaperBoxID = txtPaperBoxID.Text;
        //        //_default.BoxType = cboBoxType.SelectedValue == null ? string.Empty : cboBoxType.SelectedValue.ToString();

        //        ////_default.BoxID02 = txtBoxID02.Text;
        //        ////_default.JobGrade02 = cboJobGrade02.Text;
        //        ////_default.CassetteSettingCode02 = txtCSTSettingCode02.Text;
        //        ////_default.BoxGlassCount02 = txtGlassCount02.Text;

        //        //FormMainMDI.G_OPIAp.CurLine.DenseControlDefaultData = _default;

        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
        //    }
        //}

        private void btnMapDownload_Click(object sender, System.EventArgs e)
        {
            try
            {
                string _err = string.Empty;

                #region Check Data

                #region Product Type
                if (txtProductType.Text.ToString() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text , "", "Please input product type", MessageBoxIcon.Error);

                    txtProductType.Focus();

                    return;
                }

                if (int.Parse(txtProductType.Text.ToString()) > 100 || int.Parse(txtProductType.Text.ToString()) < 1)
                {
                    ShowMessage(this, lblCaption.Text, "", " 0 < Product type < 100", MessageBoxIcon.Error);

                    txtProductType.Focus();

                    return;
                }                
                #endregion

                #endregion

                #region OfflineDenseDataSend

                PPKOfflineDenseDataSend _trx = new PPKOfflineDenseDataSend();
                
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PORTNO = CurDense.PortNo;
                _trx.BODY.EQUIPMENTNO = CurDense.NodeNo;
                _trx.BODY.PRODUCTTYPE = txtProductType.Text;
                _trx.BODY.BOXID = txtBoxID.Text;
                _trx.BODY.PAPER_BOXID = txtPaperBoxID.Text;
                _trx.BODY.JOBGRADE = cboJobGrade.Text;
                _trx.BODY.BOXTYPE = txtBoxType.Text;
                //_trx.BODY.BOXID02 = txtBoxID02.Text;
                //_trx.BODY.CASSETTESETTINGCODE01 = txtCSTSettingCode01.Text;
                //_trx.BODY.CASSETTESETTINGCODE02 = txtCSTSettingCode02.Text;
                //_trx.BODY.JOBGRADE01 = cboJobGrade01.Text;
                //_trx.BODY.JOBGRADE02 = cboJobGrade02.Text;            
                 
                string _xml = _trx.WriteToXml();

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);

                ShowMessage(this, lblCaption.Text, "", "Offline Dense Command Send to BC Success !", MessageBoxIcon.Information);

                btnMapDownload.Enabled = false;
                #endregion
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

        private void txtProductType_TextChanged(object sender, System.EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                int _num = 0;

                int.TryParse(_txt.Text.ToString(), out _num);


                if (_num > 101 || _num < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", " 0 < Product type < 100", MessageBoxIcon.Error);

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

        //private void txtGlassCount_TextChanged(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        TextBox _txt = (TextBox)sender;

        //        int _num = 0;

        //        int.TryParse(_txt.Text.ToString(), out _num);


        //        if (_num > 400 || _num < 0)
        //        {
        //            ShowMessage(this, lblCaption.Text, "", " 0 <= Glass Count <= 400", MessageBoxIcon.Error);

        //            _txt.Text = string.Empty;

        //            _txt.Focus();

        //            return;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
        //    }
        //}

        private void InitialCombox()
        {
            try
            {
                #region Job Grade

                var data = new[] 
                { 
                    new { Key = "OK", Value ="OK" } ,
                    new { Key = "NG", Value ="NG"  } ,
                    new { Key = "RP", Value ="RP" } ,
                    new { Key = "PC", Value ="PC"  } ,
                    new { Key = "MD", Value ="MD" } ,
                    new { Key = "CD", Value ="CD"  } ,
                    new { Key = "ID", Value ="ID" } ,
                    new { Key = "DR", Value ="DR"  } ,
                    new { Key = "RW", Value ="RW"  }
                };

                cboJobGrade.DataSource = data;
                cboJobGrade.DisplayMember = "Value";
                cboJobGrade.ValueMember = "Key";
                cboJobGrade.SelectedIndex = -1;

                #endregion

                //#region Box Type

                //var _boxType = new[] 
                //{ 
                //    new { Key = "1", Value ="1:InBox" } ,
                //    new { Key = "2", Value ="2:OutBox"  }
                //};

                //cboBoxType.DataSource = _boxType;
                //cboBoxType.DisplayMember = "Value";
                //cboBoxType.ValueMember = "Key";
                //cboBoxType.SelectedIndex = -1;

                //#endregion
                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        //private void SetDefaultData()
        //{
        //    try
        //    {
        //        DenseCtrlDefaultData _default = FormMainMDI.G_OPIAp.CurLine.DenseControlDefaultData;

        //        txtProductType.Text = _default.ProductType;

        //        txtBoxID.Text = _default.BoxID;
        //        cboJobGrade.Text = _default.JobGrade;
        //        txtPaperBoxID.Text = _default.PaperBoxID;
        //        cboBoxType.SelectedValue = _default.BoxType == null ? string.Empty : _default.BoxType;

        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
        //    }
        //}


    }
}
