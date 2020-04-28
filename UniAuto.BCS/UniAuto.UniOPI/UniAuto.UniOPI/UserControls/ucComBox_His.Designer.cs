namespace UniOPI
{
    partial class ucComBox_His
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
            this.chkUse = new System.Windows.Forms.CheckBox();
            this.gbxCombox = new System.Windows.Forms.GroupBox();
            this.cmbItem = new System.Windows.Forms.ComboBox();
            this.gbxCombox.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkUse
            // 
            this.chkUse.AutoSize = true;
            this.chkUse.Location = new System.Drawing.Point(2, 20);
            this.chkUse.Name = "chkUse";
            this.chkUse.Size = new System.Drawing.Size(15, 14);
            this.chkUse.TabIndex = 7;
            this.chkUse.UseVisualStyleBackColor = true;
            // 
            // gbxCombox
            // 
            this.gbxCombox.Controls.Add(this.cmbItem);
            this.gbxCombox.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbxCombox.Location = new System.Drawing.Point(19, -5);
            this.gbxCombox.Name = "gbxCombox";
            this.gbxCombox.Size = new System.Drawing.Size(185, 50);
            this.gbxCombox.TabIndex = 6;
            this.gbxCombox.TabStop = false;
            this.gbxCombox.Text = "Combox";
            // 
            // cmbItem
            // 
            this.cmbItem.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbItem.FormattingEnabled = true;
            this.cmbItem.Items.AddRange(new object[] {
            "Run",
            "Down",
            "Idle"});
            this.cmbItem.Location = new System.Drawing.Point(6, 19);
            this.cmbItem.Name = "cmbItem";
            this.cmbItem.Size = new System.Drawing.Size(175, 26);
            this.cmbItem.TabIndex = 3;
            // 
            // ucComBox_His
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkUse);
            this.Controls.Add(this.gbxCombox);
            this.Name = "ucComBox_His";
            this.Size = new System.Drawing.Size(205, 45);
            this.gbxCombox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbxCombox;
        public System.Windows.Forms.ComboBox cmbItem;
        public System.Windows.Forms.CheckBox chkUse;

    }
}
