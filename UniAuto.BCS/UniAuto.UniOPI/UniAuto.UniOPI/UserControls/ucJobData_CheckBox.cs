using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class ucJobData_CheckBox : UserControl
    {
        int subItemOffset = 0;
        int subItemLength = 0;

        public ucJobData_CheckBox()
        {
            InitializeComponent();
        }

        public bool Checked
        {
            get { return this.chkItem.Checked; }
            set { this.chkItem.Checked = value; }
        }

        public string Caption
        {
            get { return this.chkItem.Text; }
            set { this.chkItem.Text = value; }
        }

        /// <summary>
        /// SUBITEMLOFFSET
        /// </summary>
        public int SubItemOffset
        {
            get { return subItemOffset; }
            set { subItemOffset = value; }
        }

        /// <summary>
        /// SUBITEMLENGTH
        /// </summary>
        public int SubItemLength
        {
            get { return subItemLength; }
            set { subItemLength = value; }
        }
    }
}
