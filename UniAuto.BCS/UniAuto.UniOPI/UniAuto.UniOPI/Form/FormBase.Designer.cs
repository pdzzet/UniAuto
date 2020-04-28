namespace UniOPI
{
    partial class FormBase
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
            this.spcBase = new System.Windows.Forms.SplitContainer();
            this.tlpTitle = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTitleBg = new System.Windows.Forms.Panel();
            this.lblCaption = new System.Windows.Forms.Label();
            this.pnlTopBack = new System.Windows.Forms.Panel();
            this.pnlTopBack2 = new System.Windows.Forms.Panel();
            this.tmrBaseRefresh = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel1.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpTitle.SuspendLayout();
            this.pnlTitleBg.SuspendLayout();
            this.pnlTopBack.SuspendLayout();
            this.SuspendLayout();
            // 
            // spcBase
            // 
            this.spcBase.BackColor = System.Drawing.Color.Transparent;
            this.spcBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcBase.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spcBase.IsSplitterFixed = true;
            this.spcBase.Location = new System.Drawing.Point(0, 0);
            this.spcBase.Name = "spcBase";
            this.spcBase.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcBase.Panel1
            // 
            this.spcBase.Panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.spcBase.Panel1.Controls.Add(this.tlpTitle);
            this.spcBase.Panel1MinSize = 30;
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.spcBase.Panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.spcBase.Size = new System.Drawing.Size(1264, 548);
            this.spcBase.SplitterDistance = 30;
            this.spcBase.SplitterWidth = 1;
            this.spcBase.TabIndex = 0;
            // 
            // tlpTitle
            // 
            this.tlpTitle.ColumnCount = 3;
            this.tlpTitle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpTitle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTitle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlpTitle.Controls.Add(this.pnlTitleBg, 1, 0);
            this.tlpTitle.Controls.Add(this.pnlTopBack, 2, 0);
            this.tlpTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTitle.Location = new System.Drawing.Point(0, 0);
            this.tlpTitle.Name = "tlpTitle";
            this.tlpTitle.RowCount = 1;
            this.tlpTitle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTitle.Size = new System.Drawing.Size(1264, 30);
            this.tlpTitle.TabIndex = 0;
            // 
            // pnlTitleBg
            // 
            this.pnlTitleBg.BackgroundImage = global::UniOPI.Properties.Resources.Bg_BaseCaption;
            this.pnlTitleBg.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlTitleBg.Controls.Add(this.lblCaption);
            this.pnlTitleBg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTitleBg.Location = new System.Drawing.Point(30, 0);
            this.pnlTitleBg.Margin = new System.Windows.Forms.Padding(0);
            this.pnlTitleBg.Name = "pnlTitleBg";
            this.pnlTitleBg.Size = new System.Drawing.Size(1204, 30);
            this.pnlTitleBg.TabIndex = 0;
            // 
            // lblCaption
            // 
            this.lblCaption.BackColor = System.Drawing.Color.Transparent;
            this.lblCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCaption.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblCaption.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCaption.ForeColor = System.Drawing.Color.White;
            this.lblCaption.Location = new System.Drawing.Point(0, 0);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(1204, 30);
            this.lblCaption.TabIndex = 1;
            this.lblCaption.Text = "Caption";
            this.lblCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlTopBack
            // 
            this.pnlTopBack.Controls.Add(this.pnlTopBack2);
            this.pnlTopBack.Location = new System.Drawing.Point(1234, 0);
            this.pnlTopBack.Margin = new System.Windows.Forms.Padding(0);
            this.pnlTopBack.Name = "pnlTopBack";
            this.pnlTopBack.Size = new System.Drawing.Size(30, 30);
            this.pnlTopBack.TabIndex = 1;
            // 
            // pnlTopBack2
            // 
            this.pnlTopBack2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTopBack2.Location = new System.Drawing.Point(0, 0);
            this.pnlTopBack2.Margin = new System.Windows.Forms.Padding(0);
            this.pnlTopBack2.Name = "pnlTopBack2";
            this.pnlTopBack2.Size = new System.Drawing.Size(30, 30);
            this.pnlTopBack2.TabIndex = 0;
            // 
            // tmrBaseRefresh
            // 
            this.tmrBaseRefresh.Interval = 1000;
            // 
            // FormBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1264, 548);
            this.ControlBox = false;
            this.Controls.Add(this.spcBase);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormBase";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FormBase";
            this.spcBase.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpTitle.ResumeLayout(false);
            this.pnlTitleBg.ResumeLayout(false);
            this.pnlTopBack.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lblCaption;
        public System.Windows.Forms.SplitContainer spcBase;
        private System.Windows.Forms.TableLayoutPanel tlpTitle;
        private System.Windows.Forms.Panel pnlTitleBg;
        public System.Windows.Forms.Timer tmrBaseRefresh;
        private System.Windows.Forms.Panel pnlTopBack;
        public System.Windows.Forms.Panel pnlTopBack2;
    }
}