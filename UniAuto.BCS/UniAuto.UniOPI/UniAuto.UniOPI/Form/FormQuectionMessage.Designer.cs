namespace UniOPI
{
    partial class FormQuectionMessage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormQuectionMessage));
            this.tlpCurMsg = new System.Windows.Forms.TableLayoutPanel();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.pnlIcon = new System.Windows.Forms.Panel();
            this.lblICON = new System.Windows.Forms.Label();
            this.imgICON = new System.Windows.Forms.ImageList(this.components);
            this.pnlQuestion = new System.Windows.Forms.Panel();
            this.btnQNo = new System.Windows.Forms.Button();
            this.btnQYes = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpCurMsg.SuspendLayout();
            this.pnlIcon.SuspendLayout();
            this.pnlQuestion.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(602, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.pnlQuestion);
            this.spcBase.Panel2.Controls.Add(this.tlpCurMsg);
            this.spcBase.Size = new System.Drawing.Size(662, 181);
            // 
            // tlpCurMsg
            // 
            this.tlpCurMsg.BackColor = System.Drawing.Color.Transparent;
            this.tlpCurMsg.ColumnCount = 2;
            this.tlpCurMsg.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
            this.tlpCurMsg.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCurMsg.Controls.Add(this.txtMsg, 1, 0);
            this.tlpCurMsg.Controls.Add(this.pnlIcon, 0, 0);
            this.tlpCurMsg.Location = new System.Drawing.Point(5, 0);
            this.tlpCurMsg.Name = "tlpCurMsg";
            this.tlpCurMsg.RowCount = 1;
            this.tlpCurMsg.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpCurMsg.Size = new System.Drawing.Size(657, 106);
            this.tlpCurMsg.TabIndex = 2;
            // 
            // txtMsg
            // 
            this.txtMsg.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.txtMsg.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMsg.Location = new System.Drawing.Point(96, 3);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ReadOnly = true;
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMsg.Size = new System.Drawing.Size(557, 99);
            this.txtMsg.TabIndex = 0;
            // 
            // pnlIcon
            // 
            this.pnlIcon.Controls.Add(this.lblICON);
            this.pnlIcon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlIcon.Location = new System.Drawing.Point(3, 3);
            this.pnlIcon.Name = "pnlIcon";
            this.pnlIcon.Size = new System.Drawing.Size(87, 100);
            this.pnlIcon.TabIndex = 0;
            // 
            // lblICON
            // 
            this.lblICON.ImageList = this.imgICON;
            this.lblICON.Location = new System.Drawing.Point(0, 16);
            this.lblICON.Name = "lblICON";
            this.lblICON.Size = new System.Drawing.Size(87, 63);
            this.lblICON.TabIndex = 1;
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
            // pnlQuestion
            // 
            this.pnlQuestion.Controls.Add(this.btnQNo);
            this.pnlQuestion.Controls.Add(this.btnQYes);
            this.pnlQuestion.Location = new System.Drawing.Point(3, 109);
            this.pnlQuestion.Name = "pnlQuestion";
            this.pnlQuestion.Size = new System.Drawing.Size(656, 39);
            this.pnlQuestion.TabIndex = 16;
            // 
            // btnQNo
            // 
            this.btnQNo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQNo.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQNo.Location = new System.Drawing.Point(317, 5);
            this.btnQNo.Name = "btnQNo";
            this.btnQNo.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnQNo.Size = new System.Drawing.Size(100, 30);
            this.btnQNo.TabIndex = 15;
            this.btnQNo.Text = "No";
            this.btnQNo.UseVisualStyleBackColor = true;
            this.btnQNo.Click += new System.EventHandler(this.btnQNo_Click);
            // 
            // btnQYes
            // 
            this.btnQYes.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQYes.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQYes.Location = new System.Drawing.Point(215, 5);
            this.btnQYes.Name = "btnQYes";
            this.btnQYes.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnQYes.Size = new System.Drawing.Size(100, 30);
            this.btnQYes.TabIndex = 14;
            this.btnQYes.Text = "Yes";
            this.btnQYes.UseVisualStyleBackColor = true;
            this.btnQYes.Click += new System.EventHandler(this.btnQYes_Click);
            // 
            // FormQuectionMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 181);
            this.Name = "FormQuectionMessage";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormQuectionMessage_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpCurMsg.ResumeLayout(false);
            this.tlpCurMsg.PerformLayout();
            this.pnlIcon.ResumeLayout(false);
            this.pnlQuestion.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpCurMsg;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Panel pnlIcon;
        private System.Windows.Forms.Label lblICON;
        private System.Windows.Forms.Panel pnlQuestion;
        private System.Windows.Forms.Button btnQNo;
        private System.Windows.Forms.Button btnQYes;
        private System.Windows.Forms.ImageList imgICON;
    }
}