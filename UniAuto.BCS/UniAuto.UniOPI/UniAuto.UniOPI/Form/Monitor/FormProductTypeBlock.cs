using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormProductTypeBlock : FormBase
    {
        public FormProductTypeBlock()
        {
            InitializeComponent();

            lblCaption.Text = "Product Type Block";
        }

        private void FormProductTypeBlock_Load(object sender, EventArgs e)
        {
            try
            {
                dgvData.Rows.Clear();

                #region Load Node Information

                foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                {
                    dgvData.Rows.Add(_node.NodeNo, string.Format("{0}-{1}", _node.NodeNo, _node.NodeID), 0, 0, 0, 0, 0, 0, 0, 0, 0);
                }

                #endregion
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
                if (FormMainMDI.CurForm.Tag.ToString() != this.Tag.ToString())
                {
                    tmrBaseRefresh.Enabled = false;
                    return;
                }

                #region Send Request

                BCS_ProductTypeInfoRequestReply _product = FormMainMDI.G_OPIAp.CurLine.BC_ProductTypeInfoRequestReply;
 
                DateTime _now = DateTime.Now;

                TimeSpan _ts = _now.Subtract(_product.LastRequestDate).Duration();

                if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                {
                    if (_product.IsReply)
                    {
                        Send_ProductTypeInfoRequest();
                    }
                }

                #endregion

                DisplayProductType();

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

            }
        }

        private void DisplayProductType()
        {
            try
            {
                BCS_ProductTypeInfoRequestReply _product = FormMainMDI.G_OPIAp.CurLine.BC_ProductTypeInfoRequestReply;

                foreach (ProductTypeInfo _prod in _product.Dic_ProductTypeInfo.Values)
                {
                    DataGridViewRow _row = dgvData.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[colLocalNo.Name].Value.ToString().Equals(_prod.NodeNo)).FirstOrDefault();

                    _row.Cells[colProductType.Name].Value = _prod.ProductType;
                    _row.Cells[colUnit01.Name].Value = _prod.ProductType_Unit01;
                    _row.Cells[colUnit02.Name].Value = _prod.ProductType_Unit02;
                    _row.Cells[colUnit03.Name].Value = _prod.ProductType_Unit03;
                    _row.Cells[colUnit04.Name].Value = _prod.ProductType_Unit04;
                    _row.Cells[colUnit05.Name].Value = _prod.ProductType_Unit05;
                    _row.Cells[colUnit06.Name].Value = _prod.ProductType_Unit06;
                    _row.Cells[colUnit07.Name].Value = _prod.ProductType_Unit07;
                    _row.Cells[colUnit08.Name].Value = _prod.ProductType_Unit08;
                }             
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public void Send_ProductTypeInfoRequest()
        {
            try
            {
                string _err = string.Empty;
                ProductTypeInfoRequest _trx = new ProductTypeInfoRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), out _err, FormMainMDI.G_OPIAp.SessionID);

                FormMainMDI.G_OPIAp.CurLine.BC_ProductTypeInfoRequestReply.IsReply = false;

                FormMainMDI.G_OPIAp.CurLine.BC_ProductTypeInfoRequestReply.LastRequestDate = DateTime.Now;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Send_ProductTypeInfoRequest();
        }

    }
}
