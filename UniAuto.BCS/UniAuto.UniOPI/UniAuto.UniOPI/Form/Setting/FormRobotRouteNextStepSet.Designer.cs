namespace UniOPI
{
    partial class FormRobotRouteNextStepSet
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtCurrentNextStepID = new System.Windows.Forms.TextBox();
            this.lblNewNextStepID = new System.Windows.Forms.Label();
            this.lblCurrentNextStepID = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSetOK = new System.Windows.Forms.Button();
            this.cboNextStep = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(308, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.panel1);
            this.spcBase.Size = new System.Drawing.Size(368, 254);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtCurrentNextStepID);
            this.panel1.Controls.Add(this.lblNewNextStepID);
            this.panel1.Controls.Add(this.lblCurrentNextStepID);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnSetOK);
            this.panel1.Controls.Add(this.cboNextStep);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(368, 223);
            this.panel1.TabIndex = 0;
            // 
            // txtCurrentNextStepID
            // 
            this.txtCurrentNextStepID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtCurrentNextStepID.Location = new System.Drawing.Point(185, 42);
            this.txtCurrentNextStepID.Name = "txtCurrentNextStepID";
            this.txtCurrentNextStepID.ReadOnly = true;
            this.txtCurrentNextStepID.Size = new System.Drawing.Size(160, 28);
            this.txtCurrentNextStepID.TabIndex = 32;
            // 
            // lblNewNextStepID
            // 
            this.lblNewNextStepID.BackColor = System.Drawing.Color.Black;
            this.lblNewNextStepID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblNewNextStepID.ForeColor = System.Drawing.Color.White;
            this.lblNewNextStepID.Location = new System.Drawing.Point(25, 81);
            this.lblNewNextStepID.Name = "lblNewNextStepID";
            this.lblNewNextStepID.Size = new System.Drawing.Size(160, 25);
            this.lblNewNextStepID.TabIndex = 22;
            this.lblNewNextStepID.Text = "New Next Step ID";
            this.lblNewNextStepID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCurrentNextStepID
            // 
            this.lblCurrentNextStepID.BackColor = System.Drawing.Color.Black;
            this.lblCurrentNextStepID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblCurrentNextStepID.ForeColor = System.Drawing.Color.White;
            this.lblCurrentNextStepID.Location = new System.Drawing.Point(25, 43);
            this.lblCurrentNextStepID.Name = "lblCurrentNextStepID";
            this.lblCurrentNextStepID.Size = new System.Drawing.Size(160, 25);
            this.lblCurrentNextStepID.TabIndex = 21;
            this.lblCurrentNextStepID.Text = "Current Next Step ID";
            this.lblCurrentNextStepID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(190, 141);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSetOK
            // 
            this.btnSetOK.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetOK.Location = new System.Drawing.Point(107, 141);
            this.btnSetOK.Name = "btnSetOK";
            this.btnSetOK.Size = new System.Drawing.Size(80, 30);
            this.btnSetOK.TabIndex = 2;
            this.btnSetOK.Text = "OK";
            this.btnSetOK.UseVisualStyleBackColor = true;
            this.btnSetOK.Click += new System.EventHandler(this.btnSetOK_Click);
            // 
            // cboNextStep
            // 
            this.cboNextStep.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNextStep.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboNextStep.FormattingEnabled = true;
            this.cboNextStep.Location = new System.Drawing.Point(183, 78);
            this.cboNextStep.Name = "cboNextStep";
            this.cboNextStep.Size = new System.Drawing.Size(160, 31);
            this.cboNextStep.TabIndex = 0;
            // 
            // FormRobotRouteNextStepSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 254);
            this.Name = "FormRobotRouteNextStepSet";
            this.Text = " ";
            this.Load += new System.EventHandler(this.FormRobotRouteNextStepSet_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSetOK;
        private System.Windows.Forms.ComboBox cboNextStep;
        private System.Windows.Forms.Label lblNewNextStepID;
        private System.Windows.Forms.Label lblCurrentNextStepID;
        private System.Windows.Forms.TextBox txtCurrentNextStepID;
    }
}