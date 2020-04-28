namespace UniOPI
{
    partial class FormTrxTest
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
            this.txtTrxName = new System.Windows.Forms.TextBox();
            this.lblTrxName = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(565, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.lblMessage);
            this.spcBase.Panel2.Controls.Add(this.btnCancel);
            this.spcBase.Panel2.Controls.Add(this.btnOK);
            this.spcBase.Panel2.Controls.Add(this.txtMessage);
            this.spcBase.Panel2.Controls.Add(this.txtTrxName);
            this.spcBase.Panel2.Controls.Add(this.lblTrxName);
            this.spcBase.Size = new System.Drawing.Size(625, 534);
            // 
            // txtTrxName
            // 
            this.txtTrxName.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTrxName.Location = new System.Drawing.Point(85, 5);
            this.txtTrxName.Name = "txtTrxName";
            this.txtTrxName.Size = new System.Drawing.Size(528, 26);
            this.txtTrxName.TabIndex = 6;
            // 
            // lblTrxName
            // 
            this.lblTrxName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.lblTrxName.ForeColor = System.Drawing.Color.Black;
            this.lblTrxName.Location = new System.Drawing.Point(9, 1);
            this.lblTrxName.Name = "lblTrxName";
            this.lblTrxName.Size = new System.Drawing.Size(80, 30);
            this.lblTrxName.TabIndex = 5;
            this.lblTrxName.Text = "Trx Name";
            this.lblTrxName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMessage
            // 
            this.txtMessage.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessage.Location = new System.Drawing.Point(85, 37);
            this.txtMessage.MaxLength = 65535;
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(528, 423);
            this.txtMessage.TabIndex = 7;
            // 
            // btnOK
            // 
            this.btnOK.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(243, 466);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 25);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(324, 466);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.lblMessage.ForeColor = System.Drawing.Color.Black;
            this.lblMessage.Location = new System.Drawing.Point(12, 37);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(67, 30);
            this.lblMessage.TabIndex = 10;
            this.lblMessage.Text = "Message";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormTrxTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(625, 534);
            this.Name = "FormTrxTest";
            this.Text = "FormTrxTest";
            this.spcBase.Panel2.ResumeLayout(false);
            this.spcBase.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.TextBox txtTrxName;
        private System.Windows.Forms.Label lblTrxName;

    }
}