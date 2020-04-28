using System;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormIncompleteBoxDataEdit : FormBase
    {
        private TreeNode CurrentNode;

        public FormIncompleteBoxDataEdit(TreeNode node)
        {
            InitializeComponent();

            CurrentNode = node;
        }

        private void FormIncompleteBoxDataEdit_Load(object sender, EventArgs e)
        {
            try
            {
                txtPosition.Enabled = true;
                txtProductName.Enabled = true;
                txtShortCutFlag.Enabled = true;
                txtDPIProcessFlag.Enabled = true;

                txtHostProductName.Enabled = true;
                txtBoxULDFlag.Enabled = true;
                txtRTPFlag.Enabled = true;

                txtPPID.Enabled = false;
                txtHostPPID.Enabled = false;

                #region Show Data

                txtPosition.Text = CurrentNode.Tag.ToString();

                foreach (TreeNode _tN in CurrentNode.Nodes)
                {
                    switch (_tN.Name)
                    {
                        case "PRODUCTNAME":
                            txtProductName.Text = _tN.Tag.ToString();
                            break ;
                        case "HOSTPRODUCTNAME":
                            txtHostProductName.Text = _tN.Tag.ToString();
                            break ;
                        case "SHORTCUTFLAG":
                            txtShortCutFlag.Text = _tN.Tag.ToString();
                            break ;
                        case "BOXULDFLAG":
                            txtBoxULDFlag.Text = _tN.Tag.ToString();
                            break ;
                        case "DPIPROCESSFLAG":
                            txtDPIProcessFlag.Text = _tN.Tag.ToString();
                            break ;
                        case "RTPFLAG":
                            txtRTPFlag.Text = _tN.Tag.ToString();
                            break ;
                        case "PPID":
                            txtPPID.Text = _tN.Tag.ToString();
                            break;
                        case "HOSTPPID":
                            txtHostPPID.Text = _tN.Tag.ToString();
                            break;
                        case "ABNORMALCODELIST":
                            dgvDenseData.Rows.Clear();
                            foreach (TreeNode _tn1 in _tN.Nodes)
                            {
                                string[] _data = _tn1.Tag.ToString().Split(':');
                                if (_data.Length == 2)
                                {
                                    dgvDenseData.Rows.Add(_data[0], _data[1]);
                                }
                            }
                            break;
                        default :
                            break ;
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (CheckData() == false) return;

                CurrentNode.Text = string.Format("Position [ {0} ] - [ {1} ]", txtPosition.Text, txtProductName.Text);
                CurrentNode.Tag = txtPosition.Text;

                foreach (TreeNode _tN in CurrentNode.Nodes)
                {
                    switch (_tN.Name)
                    {
                        case "PRODUCTNAME":
                            _tN.Text = string.Format("Product Name [ {0} ]", txtProductName.Text);
                            _tN.Tag = txtProductName.Text;
                            break;
                        case "HOSTPRODUCTNAME":
                            _tN.Text = string.Format("Host Product Name [ {0} ]", txtHostProductName.Text);
                            _tN.Tag = txtHostProductName.Text;
                            break;
                        case "SHORTCUTFLAG":
                            _tN.Text = string.Format("Short Cut Flag [ {0} ]", txtShortCutFlag.Text);
                            _tN.Tag = txtShortCutFlag.Text;
                            break;
                        case "BOXULDFLAG":
                            _tN.Text = string.Format("Box ULD Flag [ {0} ]", txtBoxULDFlag.Text);
                            _tN.Tag = txtBoxULDFlag.Text;
                            break;
                        case "DPIPROCESSFLAG":
                            _tN.Text = string.Format("DPI Process Flag [ {0} ]", txtDPIProcessFlag.Text);
                            _tN.Tag = txtDPIProcessFlag.Text;
                            break;
                        case "RTPFLAG":
                            _tN.Text = string.Format("RTP Flag [ {0} ]", txtRTPFlag.Text);
                            _tN.Tag = txtRTPFlag.Text;
                            break;
                        case "ABNORMALCODELIST":
                            _tN.Nodes.Clear();

                            TreeNode _abnormalDetail;

                            foreach (DataGridViewRow _row in dgvDenseData.Rows)
                            {
                                _abnormalDetail = new TreeNode();
                                _abnormalDetail.Name = "ABNORMALCODE";
                                _abnormalDetail.Text = _row.Cells[0].Value +":"+ _row.Cells[1].Value;
                                _abnormalDetail.Tag = _row.Cells[0].Value +":"+ _row.Cells[1].Value;

                                _tN.Nodes.Add(_abnormalDetail);
                            }
                            break;

                        default:
                            break;
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            dgvDenseData.Rows.Add();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDenseData.CurrentRow == null)
            {
                ShowMessage(this, lblCaption.Text, "", "Please Choose Dense Product", MessageBoxIcon.Warning);
                return;
            }

            dgvDenseData.Rows.Remove(dgvDenseData.CurrentRow);
        }

        private bool CheckData()
        {
            try
            {
                if (txtPosition.Text.Trim() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please input Position", MessageBoxIcon.Warning);
                    return false;
                }

                if (txtProductName.Text.Trim() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please input Product Name", MessageBoxIcon.Warning);
                    return false;
                }

                foreach (DataGridViewRow _row in dgvDenseData.Rows)
                {
                    if (_row.Cells[0].Value == null || _row.Cells[0].Value.ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please input Abnormal Seq No", MessageBoxIcon.Warning);
                        return false;
                    }

                    if (_row.Cells[1].Value == null || _row.Cells[1].Value.ToString() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please input Abnormal Code", MessageBoxIcon.Warning);
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
    }
}
