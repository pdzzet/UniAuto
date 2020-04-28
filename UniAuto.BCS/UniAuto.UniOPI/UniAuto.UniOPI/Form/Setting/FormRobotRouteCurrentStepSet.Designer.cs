namespace UniOPI
{
    partial class FormRobotRouteCurrentStepSet
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
            this.txtCurrentStepID = new System.Windows.Forms.TextBox();
            this.lblNewStepID = new System.Windows.Forms.Label();
            this.lblCurrentStepID = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSetOK = new System.Windows.Forms.Button();
            this.txtNewStepID = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(308, 30);
            this.lblCaption.Text = "Current Step ID Change";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.txtNewStepID);
            this.spcBase.Panel2.Controls.Add(this.txtCurrentStepID);
            this.spcBase.Panel2.Controls.Add(this.lblNewStepID);
            this.spcBase.Panel2.Controls.Add(this.lblCurrentStepID);
            this.spcBase.Panel2.Controls.Add(this.btnCancel);
            this.spcBase.Panel2.Controls.Add(this.btnSetOK);
            this.spcBase.Size = new System.Drawing.Size(368, 254);
            // 
            // txtCurrentStepID
            // 
            this.txtCurrentStepID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtCurrentStepID.Location = new System.Drawing.Point(185, 47);
            this.txtCurrentStepID.Name = "txtCurrentStepID";
            this.txtCurrentStepID.ReadOnly = true;
            this.txtCurrentStepID.Size = new System.Drawing.Size(130, 28);
            this.txtCurrentStepID.TabIndex = 38;
            // 
            // lblNewStepID
            // 
            this.lblNewStepID.BackColor = System.Drawing.Color.Black;
            this.lblNewStepID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblNewStepID.ForeColor = System.Drawing.Color.White;
            this.lblNewStepID.Location = new System.Drawing.Point(56, 86);
            this.lblNewStepID.Name = "lblNewStepID";
            this.lblNewStepID.Size = new System.Drawing.Size(130, 25);
            this.lblNewStepID.TabIndex = 37;
            this.lblNewStepID.Text = "New Step ID";
            this.lblNewStepID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCurrentStepID
            // 
            this.lblCurrentStepID.BackColor = System.Drawing.Color.Black;
            this.lblCurrentStepID.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblCurrentStepID.ForeColor = System.Drawing.Color.White;
            this.lblCurrentStepID.Location = new System.Drawing.Point(56, 48);
            this.lblCurrentStepID.Name = "lblCurrentStepID";
            this.lblCurrentStepID.Size = new System.Drawing.Size(130, 25);
            this.lblCurrentStepID.TabIndex = 36;
            this.lblCurrentStepID.Text = "Current Step ID";
            this.lblCurrentStepID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(190, 146);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 35;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSetOK
            // 
            this.btnSetOK.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSetOK.Location = new System.Drawing.Point(107, 146);
            this.btnSetOK.Name = "btnSetOK";
            this.btnSetOK.Size = new System.Drawing.Size(80, 30);
            this.btnSetOK.TabIndex = 34;
            this.btnSetOK.Text = "OK";
            this.btnSetOK.UseVisualStyleBackColor = true;
            this.btnSetOK.Click += new System.EventHandler(this.btnSetOK_Click);
            // 
            // txtNewStepID
            // 
            this.txtNewStepID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtNewStepID.Location = new System.Drawing.Point(185, 85);
            this.txtNewStepID.MaxLength = 4;
            this.txtNewStepID.Name = "txtNewStepID";
            this.txtNewStepID.Size = new System.Drawing.Size(130, 28);
            this.txtNewStepID.TabIndex = 39;
            this.txtNewStepID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNewStepID_KeyPress);
            // 
            // FormRobotRouteCurrentStepSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 254);
            this.Name = "FormRobotRouteCurrentStepSet";
            this.Text = "   ";
            this.spcBase.Panel2.ResumeLayout(false);
            this.spcBase.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtNewStepID;
        private System.Windows.Forms.TextBox txtCurrentStepID;
        private System.Windows.Forms.Label lblNewStepID;
        private System.Windows.Forms.Label lblCurrentStepID;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSetOK;
    }
}