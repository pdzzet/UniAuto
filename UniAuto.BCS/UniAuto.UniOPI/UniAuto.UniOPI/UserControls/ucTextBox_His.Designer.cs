namespace UniOPI
{
    partial class ucTextBox_His
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
            this.gbxTextBox = new System.Windows.Forms.GroupBox();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.chkUse = new System.Windows.Forms.CheckBox();
            this.gbxTextBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbxTextBox
            // 
            this.gbxTextBox.Controls.Add(this.txtInput);
            this.gbxTextBox.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbxTextBox.Location = new System.Drawing.Point(19, -5);
            this.gbxTextBox.Name = "gbxTextBox";
            this.gbxTextBox.Size = new System.Drawing.Size(185, 50);
            this.gbxTextBox.TabIndex = 9;
            this.gbxTextBox.TabStop = false;
            this.gbxTextBox.Text = "TextBox";
            // 
            // txtInput
            // 
            this.txtInput.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInput.Location = new System.Drawing.Point(6, 18);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(175, 28);
            this.txtInput.TabIndex = 0;
            // 
            // chkUse
            // 
            this.chkUse.AutoSize = true;
            this.chkUse.Location = new System.Drawing.Point(2, 20);
            this.chkUse.Name = "chkUse";
            this.chkUse.Size = new System.Drawing.Size(15, 14);
            this.chkUse.TabIndex = 8;
            this.chkUse.UseVisualStyleBackColor = true;
            // 
            // ucTextBox_His
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbxTextBox);
            this.Controls.Add(this.chkUse);
            this.Name = "ucTextBox_His";
            this.Size = new System.Drawing.Size(205, 45);
            this.gbxTextBox.ResumeLayout(false);
            this.gbxTextBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbxTextBox;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.CheckBox chkUse;
    }
}
