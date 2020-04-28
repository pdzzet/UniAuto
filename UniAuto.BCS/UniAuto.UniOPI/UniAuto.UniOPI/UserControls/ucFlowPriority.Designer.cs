namespace UniOPI
{
    partial class ucFlowPriority
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
            this.cboFlowPriority = new System.Windows.Forms.ComboBox();
            this.lblFlowPriority = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cboFlowPriority
            // 
            this.cboFlowPriority.Dock = System.Windows.Forms.DockStyle.Left;
            this.cboFlowPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFlowPriority.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboFlowPriority.FormattingEnabled = true;
            this.cboFlowPriority.Location = new System.Drawing.Point(208, 0);
            this.cboFlowPriority.Name = "cboFlowPriority";
            this.cboFlowPriority.Size = new System.Drawing.Size(279, 29);
            this.cboFlowPriority.TabIndex = 14;
            // 
            // lblFlowPriority
            // 
            this.lblFlowPriority.BackColor = System.Drawing.Color.Black;
            this.lblFlowPriority.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblFlowPriority.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFlowPriority.ForeColor = System.Drawing.Color.White;
            this.lblFlowPriority.Location = new System.Drawing.Point(0, 0);
            this.lblFlowPriority.Name = "lblFlowPriority";
            this.lblFlowPriority.Size = new System.Drawing.Size(208, 30);
            this.lblFlowPriority.TabIndex = 13;
            this.lblFlowPriority.Text = "1\'st Priority of Local No [  00  ]";
            this.lblFlowPriority.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ucFlowPriority
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cboFlowPriority);
            this.Controls.Add(this.lblFlowPriority);
            this.Name = "ucFlowPriority";
            this.Size = new System.Drawing.Size(490, 30);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cboFlowPriority;
        private System.Windows.Forms.Label lblFlowPriority;
    }
}
