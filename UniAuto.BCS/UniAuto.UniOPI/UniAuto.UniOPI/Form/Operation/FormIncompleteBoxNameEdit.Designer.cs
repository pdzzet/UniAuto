namespace UniOPI
{
    partial class FormIncompleteBoxNameEdit
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
            this.flpData = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlBoxName = new System.Windows.Forms.Panel();
            this.lblBoxName = new System.Windows.Forms.Label();
            this.txtBoxName = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.flpData.SuspendLayout();
            this.pnlBoxName.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(367, 30);
            this.lblCaption.Text = "Box Name Data";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.btnCancel);
            this.spcBase.Panel2.Controls.Add(this.btnOK);
            this.spcBase.Panel2.Controls.Add(this.flpData);
            this.spcBase.Size = new System.Drawing.Size(427, 260);
            // 
            // flpData
            // 
            this.flpData.Controls.Add(this.pnlBoxName);
            this.flpData.Location = new System.Drawing.Point(30, 31);
            this.flpData.Name = "flpData";
            this.flpData.Size = new System.Drawing.Size(355, 43);
            this.flpData.TabIndex = 21;
            // 
            // pnlBoxName
            // 
            this.pnlBoxName.Controls.Add(this.lblBoxName);
            this.pnlBoxName.Controls.Add(this.txtBoxName);
            this.pnlBoxName.Location = new System.Drawing.Point(3, 3);
            this.pnlBoxName.Name = "pnlBoxName";
            this.pnlBoxName.Size = new System.Drawing.Size(335, 23);
            this.pnlBoxName.TabIndex = 8;
            this.pnlBoxName.Tag = "INCOMPLETEDATE";
            // 
            // lblBoxName
            // 
            this.lblBoxName.BackColor = System.Drawing.Color.Black;
            this.lblBoxName.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblBoxName.ForeColor = System.Drawing.Color.White;
            this.lblBoxName.Location = new System.Drawing.Point(1, 1);
            this.lblBoxName.Name = "lblBoxName";
            this.lblBoxName.Size = new System.Drawing.Size(130, 23);
            this.lblBoxName.TabIndex = 5;
            this.lblBoxName.Text = "Box Name";
            this.lblBoxName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBoxName
            // 
            this.txtBoxName.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBoxName.Location = new System.Drawing.Point(130, 1);
            this.txtBoxName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtBoxName.Name = "txtBoxName";
            this.txtBoxName.Size = new System.Drawing.Size(200, 23);
            this.txtBoxName.TabIndex = 0;
            this.txtBoxName.Tag = "";
            // 
            // btnCancel
            // 
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnCancel.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnCancel.Location = new System.Drawing.Point(214, 122);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(76, 35);
            this.btnCancel.TabIndex = 24;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnOK.Location = new System.Drawing.Point(138, 122);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(76, 35);
            this.btnOK.TabIndex = 23;
            this.btnOK.Text = "OK";
            this.btnOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // FormIncompleteBoxNameEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 260);
            this.Name = "FormIncompleteBoxNameEdit";
            this.Text = "FormIncompleteBoxNameEdit";
            this.Load += new System.EventHandler(this.FormIncompleteBoxNameEdit_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.flpData.ResumeLayout(false);
            this.pnlBoxName.ResumeLayout(false);
            this.pnlBoxName.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpData;
        private System.Windows.Forms.Panel pnlBoxName;
        private System.Windows.Forms.Label lblBoxName;
        private System.Windows.Forms.TextBox txtBoxName;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
    }
}