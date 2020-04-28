namespace UniOPI
{
    partial class FormCassetteControl_ProductType_Array
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblProductType = new System.Windows.Forms.Label();
            this.txtProductType = new System.Windows.Forms.TextBox();
            this.grbOwnerType = new System.Windows.Forms.GroupBox();
            this.rdo_Product = new System.Windows.Forms.RadioButton();
            this.rdo_Dummy = new System.Windows.Forms.RadioButton();
            this.rdo_Engineer = new System.Windows.Forms.RadioButton();
            this.grbNum = new System.Windows.Forms.GroupBox();
            this.rdo_0 = new System.Windows.Forms.RadioButton();
            this.rdo_7 = new System.Windows.Forms.RadioButton();
            this.rdo_4 = new System.Windows.Forms.RadioButton();
            this.rdo_8 = new System.Windows.Forms.RadioButton();
            this.rdo_5 = new System.Windows.Forms.RadioButton();
            this.rdo_6 = new System.Windows.Forms.RadioButton();
            this.rdo_9 = new System.Windows.Forms.RadioButton();
            this.rdo_3 = new System.Windows.Forms.RadioButton();
            this.rdo_2 = new System.Windows.Forms.RadioButton();
            this.rdo_1 = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.panel2.SuspendLayout();
            this.grbOwnerType.SuspendLayout();
            this.grbNum.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(414, 30);
            this.lblCaption.Text = "Array Product Type";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.grbNum);
            this.spcBase.Panel2.Controls.Add(this.grbOwnerType);
            this.spcBase.Panel2.Controls.Add(this.panel2);
            this.spcBase.Panel2.Controls.Add(this.btnCancel);
            this.spcBase.Panel2.Controls.Add(this.btnOK);
            this.spcBase.Size = new System.Drawing.Size(474, 356);
            // 
            // btnCancel
            // 
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(226, 265);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnCancel.Size = new System.Drawing.Size(117, 31);
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(98, 265);
            this.btnOK.Name = "btnOK";
            this.btnOK.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnOK.Size = new System.Drawing.Size(122, 31);
            this.btnOK.TabIndex = 20;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblProductType);
            this.panel2.Controls.Add(this.txtProductType);
            this.panel2.Location = new System.Drawing.Point(30, 17);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(394, 30);
            this.panel2.TabIndex = 21;
            this.panel2.Tag = "PRODUCTSPECVER";
            // 
            // lblProductType
            // 
            this.lblProductType.BackColor = System.Drawing.Color.Black;
            this.lblProductType.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblProductType.ForeColor = System.Drawing.Color.White;
            this.lblProductType.Location = new System.Drawing.Point(3, 0);
            this.lblProductType.Name = "lblProductType";
            this.lblProductType.Size = new System.Drawing.Size(130, 30);
            this.lblProductType.TabIndex = 5;
            this.lblProductType.Text = "Product Type";
            this.lblProductType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtProductType
            // 
            this.txtProductType.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtProductType.Location = new System.Drawing.Point(135, 2);
            this.txtProductType.MaxLength = 3;
            this.txtProductType.Name = "txtProductType";
            this.txtProductType.ReadOnly = true;
            this.txtProductType.Size = new System.Drawing.Size(256, 25);
            this.txtProductType.TabIndex = 4;
            // 
            // grbOwnerType
            // 
            this.grbOwnerType.Controls.Add(this.rdo_Product);
            this.grbOwnerType.Controls.Add(this.rdo_Dummy);
            this.grbOwnerType.Controls.Add(this.rdo_Engineer);
            this.grbOwnerType.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.grbOwnerType.Location = new System.Drawing.Point(36, 153);
            this.grbOwnerType.Name = "grbOwnerType";
            this.grbOwnerType.Size = new System.Drawing.Size(388, 83);
            this.grbOwnerType.TabIndex = 22;
            this.grbOwnerType.TabStop = false;
            this.grbOwnerType.Text = "Owner Type";
            // 
            // rdo_Product
            // 
            this.rdo_Product.AutoSize = true;
            this.rdo_Product.Location = new System.Drawing.Point(246, 41);
            this.rdo_Product.Name = "rdo_Product";
            this.rdo_Product.Size = new System.Drawing.Size(77, 21);
            this.rdo_Product.TabIndex = 2;
            this.rdo_Product.Tag = "3";
            this.rdo_Product.Text = "Product";
            this.rdo_Product.UseVisualStyleBackColor = true;
            this.rdo_Product.CheckedChanged += new System.EventHandler(this.rdoType_CheckedChanged);
            // 
            // rdo_Dummy
            // 
            this.rdo_Dummy.AutoSize = true;
            this.rdo_Dummy.Location = new System.Drawing.Point(135, 41);
            this.rdo_Dummy.Name = "rdo_Dummy";
            this.rdo_Dummy.Size = new System.Drawing.Size(76, 21);
            this.rdo_Dummy.TabIndex = 1;
            this.rdo_Dummy.Tag = "2";
            this.rdo_Dummy.Text = "Dummy";
            this.rdo_Dummy.UseVisualStyleBackColor = true;
            this.rdo_Dummy.CheckedChanged += new System.EventHandler(this.rdoType_CheckedChanged);
            // 
            // rdo_Engineer
            // 
            this.rdo_Engineer.AutoSize = true;
            this.rdo_Engineer.Checked = true;
            this.rdo_Engineer.Location = new System.Drawing.Point(14, 41);
            this.rdo_Engineer.Name = "rdo_Engineer";
            this.rdo_Engineer.Size = new System.Drawing.Size(82, 21);
            this.rdo_Engineer.TabIndex = 0;
            this.rdo_Engineer.TabStop = true;
            this.rdo_Engineer.Tag = "1";
            this.rdo_Engineer.Text = "Engineer";
            this.rdo_Engineer.UseVisualStyleBackColor = true;
            this.rdo_Engineer.CheckedChanged += new System.EventHandler(this.rdoType_CheckedChanged);
            // 
            // grbNum
            // 
            this.grbNum.Controls.Add(this.rdo_0);
            this.grbNum.Controls.Add(this.rdo_7);
            this.grbNum.Controls.Add(this.rdo_4);
            this.grbNum.Controls.Add(this.rdo_8);
            this.grbNum.Controls.Add(this.rdo_5);
            this.grbNum.Controls.Add(this.rdo_6);
            this.grbNum.Controls.Add(this.rdo_9);
            this.grbNum.Controls.Add(this.rdo_3);
            this.grbNum.Controls.Add(this.rdo_2);
            this.grbNum.Controls.Add(this.rdo_1);
            this.grbNum.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.grbNum.Location = new System.Drawing.Point(36, 68);
            this.grbNum.Name = "grbNum";
            this.grbNum.Size = new System.Drawing.Size(408, 64);
            this.grbNum.TabIndex = 23;
            this.grbNum.TabStop = false;
            // 
            // rdo_0
            // 
            this.rdo_0.AutoSize = true;
            this.rdo_0.Checked = true;
            this.rdo_0.Location = new System.Drawing.Point(6, 24);
            this.rdo_0.Name = "rdo_0";
            this.rdo_0.Size = new System.Drawing.Size(34, 21);
            this.rdo_0.TabIndex = 10;
            this.rdo_0.TabStop = true;
            this.rdo_0.Text = "0";
            this.rdo_0.UseVisualStyleBackColor = true;
            this.rdo_0.CheckedChanged += new System.EventHandler(this.rdoNum_CheckedChanged);
            // 
            // rdo_7
            // 
            this.rdo_7.AutoSize = true;
            this.rdo_7.Location = new System.Drawing.Point(277, 24);
            this.rdo_7.Name = "rdo_7";
            this.rdo_7.Size = new System.Drawing.Size(34, 21);
            this.rdo_7.TabIndex = 9;
            this.rdo_7.Text = "7";
            this.rdo_7.UseVisualStyleBackColor = true;
            this.rdo_7.CheckedChanged += new System.EventHandler(this.rdoNum_CheckedChanged);
            // 
            // rdo_4
            // 
            this.rdo_4.AutoSize = true;
            this.rdo_4.Location = new System.Drawing.Point(158, 24);
            this.rdo_4.Name = "rdo_4";
            this.rdo_4.Size = new System.Drawing.Size(34, 21);
            this.rdo_4.TabIndex = 8;
            this.rdo_4.Text = "4";
            this.rdo_4.UseVisualStyleBackColor = true;
            this.rdo_4.CheckedChanged += new System.EventHandler(this.rdoNum_CheckedChanged);
            // 
            // rdo_8
            // 
            this.rdo_8.AutoSize = true;
            this.rdo_8.Location = new System.Drawing.Point(317, 24);
            this.rdo_8.Name = "rdo_8";
            this.rdo_8.Size = new System.Drawing.Size(34, 21);
            this.rdo_8.TabIndex = 7;
            this.rdo_8.Text = "8";
            this.rdo_8.UseVisualStyleBackColor = true;
            this.rdo_8.CheckedChanged += new System.EventHandler(this.rdoNum_CheckedChanged);
            // 
            // rdo_5
            // 
            this.rdo_5.AutoSize = true;
            this.rdo_5.Location = new System.Drawing.Point(198, 24);
            this.rdo_5.Name = "rdo_5";
            this.rdo_5.Size = new System.Drawing.Size(34, 21);
            this.rdo_5.TabIndex = 6;
            this.rdo_5.Text = "5";
            this.rdo_5.UseVisualStyleBackColor = true;
            this.rdo_5.CheckedChanged += new System.EventHandler(this.rdoNum_CheckedChanged);
            // 
            // rdo_6
            // 
            this.rdo_6.AutoSize = true;
            this.rdo_6.Location = new System.Drawing.Point(238, 24);
            this.rdo_6.Name = "rdo_6";
            this.rdo_6.Size = new System.Drawing.Size(34, 21);
            this.rdo_6.TabIndex = 5;
            this.rdo_6.Text = "6";
            this.rdo_6.UseVisualStyleBackColor = true;
            this.rdo_6.CheckedChanged += new System.EventHandler(this.rdoNum_CheckedChanged);
            // 
            // rdo_9
            // 
            this.rdo_9.AutoSize = true;
            this.rdo_9.Location = new System.Drawing.Point(357, 24);
            this.rdo_9.Name = "rdo_9";
            this.rdo_9.Size = new System.Drawing.Size(34, 21);
            this.rdo_9.TabIndex = 4;
            this.rdo_9.Text = "9";
            this.rdo_9.UseVisualStyleBackColor = true;
            this.rdo_9.CheckedChanged += new System.EventHandler(this.rdoNum_CheckedChanged);
            // 
            // rdo_3
            // 
            this.rdo_3.AutoSize = true;
            this.rdo_3.Location = new System.Drawing.Point(118, 24);
            this.rdo_3.Name = "rdo_3";
            this.rdo_3.Size = new System.Drawing.Size(34, 21);
            this.rdo_3.TabIndex = 3;
            this.rdo_3.Text = "3";
            this.rdo_3.UseVisualStyleBackColor = true;
            this.rdo_3.CheckedChanged += new System.EventHandler(this.rdoNum_CheckedChanged);
            // 
            // rdo_2
            // 
            this.rdo_2.AutoSize = true;
            this.rdo_2.Location = new System.Drawing.Point(78, 24);
            this.rdo_2.Name = "rdo_2";
            this.rdo_2.Size = new System.Drawing.Size(34, 21);
            this.rdo_2.TabIndex = 2;
            this.rdo_2.Text = "2";
            this.rdo_2.UseVisualStyleBackColor = true;
            this.rdo_2.CheckedChanged += new System.EventHandler(this.rdoNum_CheckedChanged);
            // 
            // rdo_1
            // 
            this.rdo_1.AutoSize = true;
            this.rdo_1.Checked = true;
            this.rdo_1.Location = new System.Drawing.Point(42, 24);
            this.rdo_1.Name = "rdo_1";
            this.rdo_1.Size = new System.Drawing.Size(34, 21);
            this.rdo_1.TabIndex = 1;
            this.rdo_1.TabStop = true;
            this.rdo_1.Text = "1";
            this.rdo_1.UseVisualStyleBackColor = true;
            this.rdo_1.CheckedChanged += new System.EventHandler(this.rdoNum_CheckedChanged);
            // 
            // FormCassetteControl_ProductType_Array
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 356);
            this.Name = "FormCassetteControl_ProductType_Array";
            this.Text = "  ";
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.grbOwnerType.ResumeLayout(false);
            this.grbOwnerType.PerformLayout();
            this.grbNum.ResumeLayout(false);
            this.grbNum.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox grbNum;
        private System.Windows.Forms.GroupBox grbOwnerType;
        private System.Windows.Forms.RadioButton rdo_Product;
        private System.Windows.Forms.RadioButton rdo_Dummy;
        private System.Windows.Forms.RadioButton rdo_Engineer;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblProductType;
        private System.Windows.Forms.TextBox txtProductType;
        private System.Windows.Forms.RadioButton rdo_7;
        private System.Windows.Forms.RadioButton rdo_4;
        private System.Windows.Forms.RadioButton rdo_8;
        private System.Windows.Forms.RadioButton rdo_5;
        private System.Windows.Forms.RadioButton rdo_6;
        private System.Windows.Forms.RadioButton rdo_9;
        private System.Windows.Forms.RadioButton rdo_3;
        private System.Windows.Forms.RadioButton rdo_2;
        private System.Windows.Forms.RadioButton rdo_1;
        private System.Windows.Forms.RadioButton rdo_0;
    }
}