using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;

namespace UniOPI
{
    public partial class ucCombox_Node : UserControl
    {
        #region Fields
        private string _selectedNodeFullName;
        private string _selectedNodeNO;
        private string _selectedNodeID;
        #endregion

        #region Property
        public bool Checked
        {
            get { return this.chkUse.Checked; }
            set { this.chkUse.Checked = value; }
        }

        public string Caption
        {
            get { return this.gbxCombox.Text; }
            set { this.gbxCombox.Text = value; }
        }

        public string SelectedNodeFullName
        {
            get { return _selectedNodeFullName; }
        }

        public string SelectedNodeNO
        {
            get { return _selectedNodeNO; }
        }

        public string SelectedNodeID
        {
            get { return _selectedNodeID; }
        }

        public object SelectedItem
        {
            get { return cmbItem.SelectedItem; }
        }
        #endregion

        public ucCombox_Node(bool selectAll, bool chkUse)
        {
            InitializeComponent();
            BindDataSource(selectAll);
            this.chkUse.Click += chkUse_Click;
            if (!chkUse)
            {
                this.chkUse.Visible = false;
                this.cmbItem.Dock = DockStyle.Fill;
            }
        }
                
        #region Events
        private void cmbItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            this._selectedNodeFullName = string.Empty;
            this._selectedNodeID = string.Empty;
            this._selectedNodeNO = string.Empty;

            if (cmbItem.SelectedIndex < 0)
                return;

            this._selectedNodeFullName = ((dynamic)cmbItem.SelectedItem).IDNAME;
            this._selectedNodeID = ((dynamic)cmbItem.SelectedItem).NodeID;
            this._selectedNodeNO = ((dynamic)cmbItem.SelectedItem).NodeNo;
        }

        private void chkUse_Click(object sender, EventArgs e)
        {
            CheckBox objChk = (CheckBox)sender;
            if (objChk.Checked)
            {
                cmbItem.Focus();
                cmbItem.DroppedDown = true;
            }
        }
        #endregion

        #region Private Methods
        private bool BindDataSource(bool all)
        {
            var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                     select new
                     {
                         IDNAME = string.Format("{0}-{1}-{2}", node.NodeNo, node.NodeID, node.NodeName),
                         node.NodeNo,
                         node.NodeID
                     }).ToList();

            if (q == null || q.Count == 0)
                return false;

            if (all)
                q.Insert(0, new { IDNAME = "00-ALL", NodeNo = "00", NodeID = "" }); //增加All的選項，此時的NodeNo及NodeID都給0
            
            cmbItem.SelectedIndexChanged -= cmbItem_SelectedIndexChanged;
            cmbItem.DataSource = q;
            cmbItem.DisplayMember = "IDNAME";
            cmbItem.ValueMember = "NODENO";
            cmbItem.SelectedIndex = -1;
            cmbItem.SelectedIndexChanged += cmbItem_SelectedIndexChanged;
            return true;
        }

        #endregion
    }
}
