namespace UniOPI
{
    partial class FormRemoteDevice
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
            this.lblBCIp = new System.Windows.Forms.Label();
            this.lblUserId = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblMasterDevices = new System.Windows.Forms.Label();
            this.cboMasterDevice = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(426, 30);
            this.lblCaption.Text = "RemoteDevice";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.cboMasterDevice);
            this.spcBase.Panel2.Controls.Add(this.lblMasterDevices);
            this.spcBase.Panel2.Controls.Add(this.btnClose);
            this.spcBase.Panel2.Controls.Add(this.btnOpen);
            this.spcBase.Panel2.Controls.Add(this.lblPath);
            this.spcBase.Panel2.Controls.Add(this.txtPassword);
            this.spcBase.Panel2.Controls.Add(this.txtUserId);
            this.spcBase.Panel2.Controls.Add(this.lblPassword);
            this.spcBase.Panel2.Controls.Add(this.lblUserId);
            this.spcBase.Panel2.Controls.Add(this.lblBCIp);
            this.spcBase.Size = new System.Drawing.Size(486, 333);
            // 
            // lblBCIp
            // 
            this.lblBCIp.AutoSize = true;
            this.lblBCIp.Font = new System.Drawing.Font("Calibri", 12.83168F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBCIp.Location = new System.Drawing.Point(11, 17);
            this.lblBCIp.Name = "lblBCIp";
            this.lblBCIp.Size = new System.Drawing.Size(74, 22);
            this.lblBCIp.TabIndex = 0;
            this.lblBCIp.Text = "Server IP";
            // 
            // lblUserId
            // 
            this.lblUserId.AutoSize = true;
            this.lblUserId.Font = new System.Drawing.Font("Calibri", 12.83168F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserId.Location = new System.Drawing.Point(11, 48);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(64, 22);
            this.lblUserId.TabIndex = 1;
            this.lblUserId.Text = "User ID";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Calibri", 12.83168F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPassword.Location = new System.Drawing.Point(11, 79);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(80, 22);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Password";
            // 
            // txtUserId
            // 
            this.txtUserId.Font = new System.Drawing.Font("Calibri", 12F);
            this.txtUserId.Location = new System.Drawing.Point(100, 44);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(217, 27);
            this.txtUserId.TabIndex = 3;
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("Calibri", 12F);
            this.txtPassword.Location = new System.Drawing.Point(100, 79);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(217, 27);
            this.txtPassword.TabIndex = 4;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Font = new System.Drawing.Font("Calibri", 12.83168F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPath.Location = new System.Drawing.Point(11, 111);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(133, 22);
            this.lblPath.TabIndex = 5;
            this.lblPath.Text = "Original Log Path";
            // 
            // btnOpen
            // 
            this.btnOpen.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnOpen.Location = new System.Drawing.Point(132, 246);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(100, 30);
            this.btnOpen.TabIndex = 6;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnClose.Location = new System.Drawing.Point(239, 246);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblMasterDevices
            // 
            this.lblMasterDevices.AutoSize = true;
            this.lblMasterDevices.Font = new System.Drawing.Font("Calibri", 12.83168F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMasterDevices.Location = new System.Drawing.Point(11, 142);
            this.lblMasterDevices.Name = "lblMasterDevices";
            this.lblMasterDevices.Size = new System.Drawing.Size(115, 22);
            this.lblMasterDevices.TabIndex = 8;
            this.lblMasterDevices.Text = "Master Device";
            // 
            // cboMasterDevice
            // 
            this.cboMasterDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMasterDevice.Font = new System.Drawing.Font("Calibri", 12F);
            this.cboMasterDevice.FormattingEnabled = true;
            this.cboMasterDevice.Location = new System.Drawing.Point(132, 140);
            this.cboMasterDevice.Name = "cboMasterDevice";
            this.cboMasterDevice.Size = new System.Drawing.Size(63, 27);
            this.cboMasterDevice.TabIndex = 9;
            // 
            // FormRemoteDevice
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.ClientSize = new System.Drawing.Size(486, 333);
            this.Name = "FormRemoteDevice";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormRemoteDevice_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            this.spcBase.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblUserId;
        private System.Windows.Forms.Label lblBCIp;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblMasterDevices;
        private System.Windows.Forms.ComboBox cboMasterDevice;
    }
}