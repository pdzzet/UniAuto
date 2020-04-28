namespace UniOPI
{
    partial class ucSemiCommand
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.grbArmCommand = new System.Windows.Forms.GroupBox();
            this.cboArmCmd = new System.Windows.Forms.ComboBox();
            this.lblArmCmd = new System.Windows.Forms.Label();
            this.cboArmSelect = new System.Windows.Forms.ComboBox();
            this.lblSlotNoDesc = new System.Windows.Forms.Label();
            this.txtTargetSlotNo = new System.Windows.Forms.TextBox();
            this.cboTargetPosition = new System.Windows.Forms.ComboBox();
            this.lblTargetSlotNo = new System.Windows.Forms.Label();
            this.lblTargetPosition = new System.Windows.Forms.Label();
            this.lnlArmSelect = new System.Windows.Forms.Label();
            this.chkChoose = new System.Windows.Forms.CheckBox();
            this.grbArmCommand.SuspendLayout();
            this.SuspendLayout();
            // 
            // grbArmCommand
            // 
            this.grbArmCommand.Controls.Add(this.cboArmCmd);
            this.grbArmCommand.Controls.Add(this.lblArmCmd);
            this.grbArmCommand.Controls.Add(this.cboArmSelect);
            this.grbArmCommand.Controls.Add(this.lblSlotNoDesc);
            this.grbArmCommand.Controls.Add(this.txtTargetSlotNo);
            this.grbArmCommand.Controls.Add(this.cboTargetPosition);
            this.grbArmCommand.Controls.Add(this.lblTargetSlotNo);
            this.grbArmCommand.Controls.Add(this.lblTargetPosition);
            this.grbArmCommand.Controls.Add(this.lnlArmSelect);
            this.grbArmCommand.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbArmCommand.Location = new System.Drawing.Point(0, 0);
            this.grbArmCommand.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.grbArmCommand.Name = "grbArmCommand";
            this.grbArmCommand.Size = new System.Drawing.Size(550, 140);
            this.grbArmCommand.TabIndex = 29;
            this.grbArmCommand.TabStop = false;
            this.grbArmCommand.Text = "  3\'st Robot Command";
            // 
            // cboArmCmd
            // 
            this.cboArmCmd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboArmCmd.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboArmCmd.FormattingEnabled = true;
            this.cboArmCmd.Location = new System.Drawing.Point(118, 17);
            this.cboArmCmd.Name = "cboArmCmd";
            this.cboArmCmd.Size = new System.Drawing.Size(405, 29);
            this.cboArmCmd.TabIndex = 92;
            // 
            // lblArmCmd
            // 
            this.lblArmCmd.BackColor = System.Drawing.Color.Black;
            this.lblArmCmd.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblArmCmd.ForeColor = System.Drawing.Color.White;
            this.lblArmCmd.Location = new System.Drawing.Point(8, 21);
            this.lblArmCmd.Name = "lblArmCmd";
            this.lblArmCmd.Size = new System.Drawing.Size(110, 25);
            this.lblArmCmd.TabIndex = 91;
            this.lblArmCmd.Text = "Command";
            this.lblArmCmd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboArmSelect
            // 
            this.cboArmSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboArmSelect.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboArmSelect.FormattingEnabled = true;
            this.cboArmSelect.Location = new System.Drawing.Point(118, 47);
            this.cboArmSelect.Name = "cboArmSelect";
            this.cboArmSelect.Size = new System.Drawing.Size(405, 29);
            this.cboArmSelect.TabIndex = 90;
            // 
            // lblSlotNoDesc
            // 
            this.lblSlotNoDesc.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSlotNoDesc.ForeColor = System.Drawing.Color.Red;
            this.lblSlotNoDesc.Location = new System.Drawing.Point(240, 115);
            this.lblSlotNoDesc.Name = "lblSlotNoDesc";
            this.lblSlotNoDesc.Size = new System.Drawing.Size(64, 21);
            this.lblSlotNoDesc.TabIndex = 89;
            this.lblSlotNoDesc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtTargetSlotNo
            // 
            this.txtTargetSlotNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTargetSlotNo.Location = new System.Drawing.Point(118, 108);
            this.txtTargetSlotNo.MaxLength = 2;
            this.txtTargetSlotNo.Name = "txtTargetSlotNo";
            this.txtTargetSlotNo.Size = new System.Drawing.Size(120, 28);
            this.txtTargetSlotNo.TabIndex = 88;
            this.txtTargetSlotNo.Tag = "0";
            this.txtTargetSlotNo.TextChanged += new System.EventHandler(this.txtTargetSlotNo_TextChanged);
            this.txtTargetSlotNo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNumber_KeyPress);
            // 
            // cboTargetPosition
            // 
            this.cboTargetPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTargetPosition.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboTargetPosition.FormattingEnabled = true;
            this.cboTargetPosition.Location = new System.Drawing.Point(118, 77);
            this.cboTargetPosition.Name = "cboTargetPosition";
            this.cboTargetPosition.Size = new System.Drawing.Size(405, 29);
            this.cboTargetPosition.TabIndex = 87;
            // 
            // lblTargetSlotNo
            // 
            this.lblTargetSlotNo.BackColor = System.Drawing.Color.Black;
            this.lblTargetSlotNo.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetSlotNo.ForeColor = System.Drawing.Color.White;
            this.lblTargetSlotNo.Location = new System.Drawing.Point(8, 109);
            this.lblTargetSlotNo.Name = "lblTargetSlotNo";
            this.lblTargetSlotNo.Size = new System.Drawing.Size(110, 25);
            this.lblTargetSlotNo.TabIndex = 84;
            this.lblTargetSlotNo.Text = "Target Slot No";
            this.lblTargetSlotNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTargetPosition
            // 
            this.lblTargetPosition.BackColor = System.Drawing.Color.Black;
            this.lblTargetPosition.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTargetPosition.ForeColor = System.Drawing.Color.White;
            this.lblTargetPosition.Location = new System.Drawing.Point(8, 79);
            this.lblTargetPosition.Name = "lblTargetPosition";
            this.lblTargetPosition.Size = new System.Drawing.Size(110, 25);
            this.lblTargetPosition.TabIndex = 83;
            this.lblTargetPosition.Text = "Target Position";
            this.lblTargetPosition.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lnlArmSelect
            // 
            this.lnlArmSelect.BackColor = System.Drawing.Color.Black;
            this.lnlArmSelect.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnlArmSelect.ForeColor = System.Drawing.Color.White;
            this.lnlArmSelect.Location = new System.Drawing.Point(8, 49);
            this.lnlArmSelect.Name = "lnlArmSelect";
            this.lnlArmSelect.Size = new System.Drawing.Size(110, 25);
            this.lnlArmSelect.TabIndex = 82;
            this.lnlArmSelect.Text = "Arm Select";
            this.lnlArmSelect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkChoose
            // 
            this.chkChoose.AutoSize = true;
            this.chkChoose.Location = new System.Drawing.Point(3, 3);
            this.chkChoose.Name = "chkChoose";
            this.chkChoose.Size = new System.Drawing.Size(15, 14);
            this.chkChoose.TabIndex = 32;
            this.chkChoose.UseVisualStyleBackColor = true;
            this.chkChoose.CheckedChanged += new System.EventHandler(this.chkChoose_CheckedChanged);
            // 
            // ucSemiCommand
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkChoose);
            this.Controls.Add(this.grbArmCommand);
            this.Name = "ucSemiCommand";
            this.Size = new System.Drawing.Size(550, 140);
            this.grbArmCommand.ResumeLayout(false);
            this.grbArmCommand.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grbArmCommand;
        private System.Windows.Forms.Label lblSlotNoDesc;
        private System.Windows.Forms.ComboBox cboTargetPosition;
        private System.Windows.Forms.Label lblTargetSlotNo;
        private System.Windows.Forms.Label lblTargetPosition;
        private System.Windows.Forms.Label lnlArmSelect;
        private System.Windows.Forms.ComboBox cboArmSelect;
        public System.Windows.Forms.CheckBox chkChoose;
        private System.Windows.Forms.Label lblArmCmd;
        public System.Windows.Forms.TextBox txtTargetSlotNo;
        private System.Windows.Forms.ComboBox cboArmCmd;
    }
}
