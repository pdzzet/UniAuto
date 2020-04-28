using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UniAuto.UniBCS.EntityManager.UI
{
    public partial class FrmFind : Form
    {
        private string _jobSequence;

        public string JobSequence {
            get { return _jobSequence; }
            set { _jobSequence = value; }
        }
        private string _cstSequence;

        public string CstSequence {
            get { return _cstSequence; }
            set { _cstSequence = value; }
        }
        private string _jobID;

        public string JobID {
            get { return _jobID; }
            set { _jobID = value; }
        }
        public FrmFind() {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }

        private void txtCSTSequence_KeyPress(object sender, KeyPressEventArgs e) {
            if (!(char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8) {
                e.Handled = true;
            } else
                e.Handled = false;
        }

        private void txtJobSequence_KeyPress(object sender, KeyPressEventArgs e) {
            if (!(char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8) {
                e.Handled = true;
            } else
                e.Handled = false;
        }

        private void txtJobID_KeyPress(object sender, KeyPressEventArgs e) {
            if (char.IsLower(e.KeyChar)) {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }
            //e.Handled = false;
        }

        private void button2_Click(object sender, EventArgs e) {
            this.CstSequence = txtCSTSequence.Text.Trim();
            this.JobSequence = txtJobSequence.Text.Trim();
            this.JobID = txtJobID.Text.Trim();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
