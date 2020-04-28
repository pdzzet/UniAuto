namespace UniAuto.UniBCS.EntityManager.UI
{
    partial class FrmRemoveRecoveryJob
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRemoveRecoveryJob));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbEquipment = new System.Windows.Forms.ComboBox();
            this.txtCstSeq = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSlotNo = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnCancel);
            this.groupBox1.Controls.Add(this.btnOK);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtSlotNo);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtCstSeq);
            this.groupBox1.Controls.Add(this.cmbEquipment);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(420, 126);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // cmbEquipment
            // 
            this.cmbEquipment.FormattingEnabled = true;
            this.cmbEquipment.Location = new System.Drawing.Point(78, 19);
            this.cmbEquipment.Name = "cmbEquipment";
            this.cmbEquipment.Size = new System.Drawing.Size(121, 21);
            this.cmbEquipment.TabIndex = 1;
            // 
            // txtCstSeq
            // 
            this.txtCstSeq.Location = new System.Drawing.Point(79, 62);
            this.txtCstSeq.MaxLength = 5;
            this.txtCstSeq.Name = "txtCstSeq";
            this.txtCstSeq.Size = new System.Drawing.Size(120, 20);
            this.txtCstSeq.TabIndex = 3;
            this.txtCstSeq.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCstSeq_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Equipment";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "CST Seq";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(221, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Slot No";
            // 
            // txtSlotNo
            // 
            this.txtSlotNo.Location = new System.Drawing.Point(278, 62);
            this.txtSlotNo.MaxLength = 4;
            this.txtSlotNo.Name = "txtSlotNo";
            this.txtSlotNo.Size = new System.Drawing.Size(120, 20);
            this.txtSlotNo.TabIndex = 6;
            this.txtSlotNo.TextChanged += new System.EventHandler(this.txtSlotNo_TextChanged);
            this.txtSlotNo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSlotNo_KeyPress);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(323, 97);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(224, 97);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FrmRemoveRecoveryJob
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 126);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmRemoveRecoveryJob";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Condition";
            this.Load += new System.EventHandler(this.FrmRemoveRecoveryJob_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSlotNo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCstSeq;
        private System.Windows.Forms.ComboBox cmbEquipment;
    }
}