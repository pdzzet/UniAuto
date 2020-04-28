namespace UniOPI
{
    partial class ucOXRInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucOXRInfo));
            this.btnChipSet = new System.Windows.Forms.Button();
            this.txtChipData = new System.Windows.Forms.TextBox();
            this.lblChipName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnChipSet
            // 
            this.btnChipSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnChipSet.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChipSet.Image = ((System.Drawing.Image)(resources.GetObject("btnChipSet.Image")));
            this.btnChipSet.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnChipSet.Location = new System.Drawing.Point(517, 2);
            this.btnChipSet.Name = "btnChipSet";
            this.btnChipSet.Size = new System.Drawing.Size(76, 25);
            this.btnChipSet.TabIndex = 8;
            this.btnChipSet.Tag = "001";
            this.btnChipSet.Text = "Set";
            this.btnChipSet.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnChipSet.UseVisualStyleBackColor = true;
            // 
            // txtChipData
            // 
            this.txtChipData.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtChipData.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtChipData.Location = new System.Drawing.Point(116, 2);
            this.txtChipData.MaxLength = 10;
            this.txtChipData.Name = "txtChipData";
            this.txtChipData.Size = new System.Drawing.Size(398, 25);
            this.txtChipData.TabIndex = 7;
            // 
            // lblChipName
            // 
            this.lblChipName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblChipName.ForeColor = System.Drawing.Color.Black;
            this.lblChipName.Location = new System.Drawing.Point(0, 2);
            this.lblChipName.Name = "lblChipName";
            this.lblChipName.Size = new System.Drawing.Size(111, 23);
            this.lblChipName.TabIndex = 6;
            this.lblChipName.Text = "1 ~ 10";
            this.lblChipName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ucOXRInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.Controls.Add(this.btnChipSet);
            this.Controls.Add(this.txtChipData);
            this.Controls.Add(this.lblChipName);
            this.Name = "ucOXRInfo";
            this.Size = new System.Drawing.Size(605, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtChipData;
        private System.Windows.Forms.Label lblChipName;
        public System.Windows.Forms.Button btnChipSet;
    }
}
