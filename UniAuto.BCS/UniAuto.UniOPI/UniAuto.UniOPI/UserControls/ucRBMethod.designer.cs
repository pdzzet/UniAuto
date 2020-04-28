namespace UniOPI
{
    partial class ucRBMethod
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
            this.cboMethodName = new System.Windows.Forms.ComboBox();
            this.cboObjectName = new System.Windows.Forms.ComboBox();
            this.lblObjectName = new System.Windows.Forms.Label();
            this.lblMethodName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cboMethodName
            // 
            this.cboMethodName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMethodName.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.cboMethodName.FormattingEnabled = true;
            this.cboMethodName.Location = new System.Drawing.Point(183, 31);
            this.cboMethodName.Name = "cboMethodName";
            this.cboMethodName.Size = new System.Drawing.Size(637, 29);
            this.cboMethodName.TabIndex = 24;
            this.cboMethodName.Tag = "Method Name";
            this.cboMethodName.SelectedIndexChanged += new System.EventHandler(this.cboMethodName_SelectedIndexChanged);
            // 
            // cboObjectName
            // 
            this.cboObjectName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboObjectName.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.cboObjectName.FormattingEnabled = true;
            this.cboObjectName.Location = new System.Drawing.Point(183, 0);
            this.cboObjectName.Name = "cboObjectName";
            this.cboObjectName.Size = new System.Drawing.Size(637, 29);
            this.cboObjectName.TabIndex = 23;
            this.cboObjectName.Tag = "Object Name";
            this.cboObjectName.SelectedIndexChanged += new System.EventHandler(this.cboObjectName_SelectedIndexChanged);
            // 
            // lblObjectName
            // 
            this.lblObjectName.BackColor = System.Drawing.Color.Black;
            this.lblObjectName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblObjectName.ForeColor = System.Drawing.Color.White;
            this.lblObjectName.Location = new System.Drawing.Point(3, 1);
            this.lblObjectName.Name = "lblObjectName";
            this.lblObjectName.Size = new System.Drawing.Size(180, 25);
            this.lblObjectName.TabIndex = 26;
            this.lblObjectName.Text = "Object Name";
            this.lblObjectName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblMethodName
            // 
            this.lblMethodName.BackColor = System.Drawing.Color.Black;
            this.lblMethodName.Font = new System.Drawing.Font("Calibri", 13F);
            this.lblMethodName.ForeColor = System.Drawing.Color.White;
            this.lblMethodName.Location = new System.Drawing.Point(3, 32);
            this.lblMethodName.Name = "lblMethodName";
            this.lblMethodName.Size = new System.Drawing.Size(180, 25);
            this.lblMethodName.TabIndex = 25;
            this.lblMethodName.Text = "Method Name";
            this.lblMethodName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ucRBMethod
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cboMethodName);
            this.Controls.Add(this.cboObjectName);
            this.Controls.Add(this.lblObjectName);
            this.Controls.Add(this.lblMethodName);
            this.Name = "ucRBMethod";
            this.Size = new System.Drawing.Size(821, 63);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cboMethodName;
        private System.Windows.Forms.ComboBox cboObjectName;
        private System.Windows.Forms.Label lblObjectName;
        private System.Windows.Forms.Label lblMethodName;
    }
}
