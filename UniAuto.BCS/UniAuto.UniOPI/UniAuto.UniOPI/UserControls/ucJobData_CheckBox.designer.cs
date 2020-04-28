namespace UniOPI
{
    partial class ucJobData_CheckBox
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
            this.chkItem = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkItem
            // 
            this.chkItem.Font = new System.Drawing.Font("Calibri", 12F);
            this.chkItem.Location = new System.Drawing.Point(12, 5);
            this.chkItem.Name = "chkItem";
            this.chkItem.Size = new System.Drawing.Size(258, 23);
            this.chkItem.TabIndex = 7;
            this.chkItem.Text = "Job Data Sub Item";
            this.chkItem.UseVisualStyleBackColor = true;
            // 
            // ucJobData_CheckBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.chkItem);
            this.Name = "ucJobData_CheckBox";
            this.Size = new System.Drawing.Size(329, 33);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkItem;
    }
}
