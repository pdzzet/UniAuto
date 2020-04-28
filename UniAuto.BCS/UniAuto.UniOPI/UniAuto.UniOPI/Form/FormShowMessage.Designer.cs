namespace UniOPI
{
    partial class FormShowMessage
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormShowMessage));
            this.tlpLayout = new System.Windows.Forms.TableLayoutPanel();
            this.tlpCurMsg = new System.Windows.Forms.TableLayoutPanel();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.pnlIcon = new System.Windows.Forms.Panel();
            this.lblICON = new System.Windows.Forms.Label();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.imgICON = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpLayout.SuspendLayout();
            this.tlpCurMsg.SuspendLayout();
            this.pnlIcon.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(630, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpLayout);
            this.spcBase.Size = new System.Drawing.Size(690, 250);
            // 
            // tlpLayout
            // 
            this.tlpLayout.BackColor = System.Drawing.Color.Transparent;
            this.tlpLayout.ColumnCount = 1;
            this.tlpLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLayout.Controls.Add(this.tlpCurMsg, 0, 0);
            this.tlpLayout.Controls.Add(this.pnlButton, 0, 1);
            this.tlpLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLayout.Location = new System.Drawing.Point(0, 0);
            this.tlpLayout.Name = "tlpLayout";
            this.tlpLayout.RowCount = 2;
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 173F));
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpLayout.Size = new System.Drawing.Size(690, 219);
            this.tlpLayout.TabIndex = 2;
            // 
            // tlpCurMsg
            // 
            this.tlpCurMsg.BackColor = System.Drawing.Color.Transparent;
            this.tlpCurMsg.ColumnCount = 2;
            this.tlpCurMsg.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
            this.tlpCurMsg.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCurMsg.Controls.Add(this.txtMsg, 1, 0);
            this.tlpCurMsg.Controls.Add(this.pnlIcon, 0, 0);
            this.tlpCurMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCurMsg.Location = new System.Drawing.Point(3, 3);
            this.tlpCurMsg.Name = "tlpCurMsg";
            this.tlpCurMsg.RowCount = 1;
            this.tlpCurMsg.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCurMsg.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 167F));
            this.tlpCurMsg.Size = new System.Drawing.Size(684, 167);
            this.tlpCurMsg.TabIndex = 1;
            // 
            // txtMsg
            // 
            this.txtMsg.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.txtMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMsg.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.txtMsg.Location = new System.Drawing.Point(96, 3);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ReadOnly = true;
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMsg.Size = new System.Drawing.Size(585, 161);
            this.txtMsg.TabIndex = 0;
            // 
            // pnlIcon
            // 
            this.pnlIcon.Controls.Add(this.lblICON);
            this.pnlIcon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlIcon.Location = new System.Drawing.Point(3, 3);
            this.pnlIcon.Name = "pnlIcon";
            this.pnlIcon.Size = new System.Drawing.Size(87, 161);
            this.pnlIcon.TabIndex = 0;
            // 
            // lblICON
            // 
            this.lblICON.ImageList = this.imgICON;
            this.lblICON.Location = new System.Drawing.Point(0, 25);
            this.lblICON.Name = "lblICON";
            this.lblICON.Size = new System.Drawing.Size(87, 63);
            this.lblICON.TabIndex = 1;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnOK);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(3, 176);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(684, 40);
            this.pnlButton.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(279, 4);
            this.btnOK.Name = "btnOK";
            this.btnOK.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.TabIndex = 14;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // imgICON
            // 
            this.imgICON.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgICON.ImageStream")));
            this.imgICON.TransparentColor = System.Drawing.Color.Transparent;
            this.imgICON.Images.SetKeyName(0, "Warning-128.png");
            this.imgICON.Images.SetKeyName(1, "Error-128.png");
            this.imgICON.Images.SetKeyName(2, "Question-128.png");
            this.imgICON.Images.SetKeyName(3, "Information-128.png");
            // 
            // FormShowMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 250);
            this.Name = "FormShowMessage";
            this.Text = "  ";
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpLayout.ResumeLayout(false);
            this.tlpCurMsg.ResumeLayout(false);
            this.tlpCurMsg.PerformLayout();
            this.pnlIcon.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpLayout;
        private System.Windows.Forms.TableLayoutPanel tlpCurMsg;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Panel pnlIcon;
        private System.Windows.Forms.Label lblICON;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ImageList imgICON;
    }
}