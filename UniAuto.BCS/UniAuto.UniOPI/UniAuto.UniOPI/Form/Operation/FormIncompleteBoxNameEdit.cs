using System;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormIncompleteBoxNameEdit : FormBase
    {
        private TreeNode CurrentNode;

        public FormIncompleteBoxNameEdit(TreeNode node)
        {
            InitializeComponent();

            CurrentNode = node;
        }

        private void FormIncompleteBoxNameEdit_Load(object sender, EventArgs e)
        {
            try
            {
                txtBoxName.Text = CurrentNode.Tag.ToString();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtBoxName.Text.Trim() == string.Empty)
            {
                ShowMessage(this, lblCaption.Text, "", "Please input Dense Name", MessageBoxIcon.Warning);
                return;
            }

            CurrentNode.Text = string.Format("Box Name [ {0} ]", txtBoxName.Text);
            CurrentNode.Tag = txtBoxName.Text;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }


    }
}
