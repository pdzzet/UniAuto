namespace UniOPI
{
    partial class ucCombox_Node
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.gbxCombox = new System.Windows.Forms.GroupBox();
            this.cmbItem = new System.Windows.Forms.ComboBox();
            this.chkUse = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.gbxCombox.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.gbxCombox);
            this.panel1.Controls.Add(this.chkUse);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(220, 57);
            this.panel1.TabIndex = 8;
            // 
            // gbxCombox
            // 
            this.gbxCombox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbxCombox.Controls.Add(this.cmbItem);
            this.gbxCombox.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbxCombox.Location = new System.Drawing.Point(19, 2);
            this.gbxCombox.Name = "gbxCombox";
            this.gbxCombox.Size = new System.Drawing.Size(200, 53);
            this.gbxCombox.TabIndex = 10;
            this.gbxCombox.TabStop = false;
            this.gbxCombox.Text = "Combox";
            // 
            // cmbItem
            // 
            this.cmbItem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbItem.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbItem.FormattingEnabled = true;
            this.cmbItem.Location = new System.Drawing.Point(6, 20);
            this.cmbItem.Name = "cmbItem";
            this.cmbItem.Size = new System.Drawing.Size(188, 26);
            this.cmbItem.TabIndex = 3;
            // 
            // chkUse
            // 
            this.chkUse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUse.AutoSize = true;
            this.chkUse.Location = new System.Drawing.Point(1, 24);
            this.chkUse.Name = "chkUse";
            this.chkUse.Size = new System.Drawing.Size(15, 14);
            this.chkUse.TabIndex = 9;
            this.chkUse.UseVisualStyleBackColor = true;
            // 
            // ucCombox_Node
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "ucCombox_Node";
            this.Size = new System.Drawing.Size(225, 62);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.gbxCombox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox chkUse;
        private System.Windows.Forms.GroupBox gbxCombox;
        private System.Windows.Forms.ComboBox cmbItem;
    }
}
