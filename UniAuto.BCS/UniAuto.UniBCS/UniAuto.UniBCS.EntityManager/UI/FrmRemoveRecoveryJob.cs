using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UniAuto.UniBCS.Entity;

namespace UniAuto.UniBCS.EntityManager.UI
{
    public partial class FrmRemoveRecoveryJob : Form
    {
        private string _equipmentNo;
        private string _cstSeq;
        private string _slotNo;

       
        public string EquipmentNo
        {
            get { return _equipmentNo; }
            set { _equipmentNo = value; }
        }
        
        public string CstSeq
        {
            get { return _cstSeq; }
            set { _cstSeq = value; }
        }

        public string SlotNo
        {
            get { return _slotNo; }
            set { _slotNo = value; }
        }

        public FrmRemoveRecoveryJob(REMOVERECOVERY mode)
        {
            InitializeComponent();
            if (mode == REMOVERECOVERY.REMOVE)
            {
                this.Text += "_ Remove"; 
            }
            else
            {
                this.Text += "_ Recovery";
            }
        }

        private void FrmRemoveRecoveryJob_Load(object sender, EventArgs e)
        {
            List<Equipment> equipments = ObjectManager.EquipmentManager.GetEQPs();
            if (equipments != null)
            {
                cmbEquipment.Items.Clear();
                foreach (Equipment eq in equipments)
                {
                    cmbEquipment.Items.Add(eq.Data.NODENO);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cmbEquipment.Text))
            {
                EquipmentNo = "";
            }
            else
                EquipmentNo=cmbEquipment.Text.Trim();
            if (string.IsNullOrEmpty(txtCstSeq.Text))
                CstSeq = "";
            else
                CstSeq = txtCstSeq.Text.Trim();
            if (string.IsNullOrEmpty(txtSlotNo.Text))
                SlotNo = "";
            else
                SlotNo = txtSlotNo.Text.Trim();
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void txtCstSeq_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
            else
                e.Handled = false;
        }

        private void txtSlotNo_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void txtSlotNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
            else
                e.Handled = false;
        }
    }
}
