namespace UniOPI
{
    partial class FormStagePositionInfo
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
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.flpPosition = new System.Windows.Forms.FlowLayoutPanel();
            this.lblSendReady = new System.Windows.Forms.Label();
            this.lblReceiveReady = new System.Windows.Forms.Label();
            this.lblExchangePossible = new System.Windows.Forms.Label();
            this.lblGlassExist = new System.Windows.Forms.Label();
            this.lblDoubleGlassExist = new System.Windows.Forms.Label();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlRefresh = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.flpPosition.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.pnlRefresh.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(352, 30);
            this.lblCaption.Text = "Stage Position Info";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(412, 373);
            // 
            // tmrBaseRefresh
            // 
            this.tmrBaseRefresh.Tick += new System.EventHandler(this.tmrBaseRefresh_Tick);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 3;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpBase.Controls.Add(this.flpPosition, 1, 1);
            this.tlpBase.Controls.Add(this.pnlButton, 1, 2);
            this.tlpBase.Controls.Add(this.pnlRefresh, 1, 0);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 3;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tlpBase.Size = new System.Drawing.Size(412, 342);
            this.tlpBase.TabIndex = 3;
            // 
            // flpPosition
            // 
            this.flpPosition.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.flpPosition.Controls.Add(this.lblSendReady);
            this.flpPosition.Controls.Add(this.lblReceiveReady);
            this.flpPosition.Controls.Add(this.lblExchangePossible);
            this.flpPosition.Controls.Add(this.lblGlassExist);
            this.flpPosition.Controls.Add(this.lblDoubleGlassExist);
            this.flpPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpPosition.Location = new System.Drawing.Point(13, 48);
            this.flpPosition.Name = "flpPosition";
            this.flpPosition.Size = new System.Drawing.Size(386, 248);
            this.flpPosition.TabIndex = 25;
            // 
            // lblSendReady
            // 
            this.lblSendReady.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSendReady.Image = global::UniOPI.Properties.Resources.Bit_Sliver;
            this.lblSendReady.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSendReady.Location = new System.Drawing.Point(3, 3);
            this.lblSendReady.Margin = new System.Windows.Forms.Padding(3);
            this.lblSendReady.Name = "lblSendReady";
            this.lblSendReady.Size = new System.Drawing.Size(350, 23);
            this.lblSendReady.TabIndex = 13;
            this.lblSendReady.Tag = "SENDYREADY";
            this.lblSendReady.Text = "        Send Ready";
            this.lblSendReady.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblReceiveReady
            // 
            this.lblReceiveReady.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReceiveReady.Image = global::UniOPI.Properties.Resources.Bit_Sliver;
            this.lblReceiveReady.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblReceiveReady.Location = new System.Drawing.Point(3, 32);
            this.lblReceiveReady.Margin = new System.Windows.Forms.Padding(3);
            this.lblReceiveReady.Name = "lblReceiveReady";
            this.lblReceiveReady.Size = new System.Drawing.Size(350, 23);
            this.lblReceiveReady.TabIndex = 14;
            this.lblReceiveReady.Tag = "RECEIVEREADY";
            this.lblReceiveReady.Text = "        Receive Ready";
            this.lblReceiveReady.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblExchangePossible
            // 
            this.lblExchangePossible.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblExchangePossible.Image = global::UniOPI.Properties.Resources.Bit_Sliver;
            this.lblExchangePossible.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblExchangePossible.Location = new System.Drawing.Point(3, 61);
            this.lblExchangePossible.Margin = new System.Windows.Forms.Padding(3);
            this.lblExchangePossible.Name = "lblExchangePossible";
            this.lblExchangePossible.Size = new System.Drawing.Size(350, 23);
            this.lblExchangePossible.TabIndex = 17;
            this.lblExchangePossible.Tag = "EXCHANGEPOSSIBLE";
            this.lblExchangePossible.Text = "        Exchange Possible";
            this.lblExchangePossible.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblGlassExist
            // 
            this.lblGlassExist.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGlassExist.Image = global::UniOPI.Properties.Resources.Bit_Sliver;
            this.lblGlassExist.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblGlassExist.Location = new System.Drawing.Point(3, 90);
            this.lblGlassExist.Margin = new System.Windows.Forms.Padding(3);
            this.lblGlassExist.Name = "lblGlassExist";
            this.lblGlassExist.Size = new System.Drawing.Size(350, 23);
            this.lblGlassExist.TabIndex = 15;
            this.lblGlassExist.Tag = "GLASSEXIST";
            this.lblGlassExist.Text = "        Glass Exist";
            this.lblGlassExist.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDoubleGlassExist
            // 
            this.lblDoubleGlassExist.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDoubleGlassExist.Image = global::UniOPI.Properties.Resources.Bit_Sliver;
            this.lblDoubleGlassExist.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblDoubleGlassExist.Location = new System.Drawing.Point(3, 119);
            this.lblDoubleGlassExist.Margin = new System.Windows.Forms.Padding(3);
            this.lblDoubleGlassExist.Name = "lblDoubleGlassExist";
            this.lblDoubleGlassExist.Size = new System.Drawing.Size(350, 23);
            this.lblDoubleGlassExist.TabIndex = 16;
            this.lblDoubleGlassExist.Tag = "DOUBLEGLASSEXIST";
            this.lblDoubleGlassExist.Text = "        Double Glass Exist";
            this.lblDoubleGlassExist.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnClose);
            this.pnlButton.Location = new System.Drawing.Point(13, 302);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(386, 37);
            this.pnlButton.TabIndex = 22;
            // 
            // btnClose
            // 
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(127, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 18;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // pnlRefresh
            // 
            this.pnlRefresh.Controls.Add(this.btnRefresh);
            this.pnlRefresh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRefresh.Location = new System.Drawing.Point(13, 3);
            this.pnlRefresh.Name = "pnlRefresh";
            this.pnlRefresh.Size = new System.Drawing.Size(386, 39);
            this.pnlRefresh.TabIndex = 26;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Image = global::UniOPI.Properties.Resources.BtnRefresh;
            this.btnRefresh.Location = new System.Drawing.Point(343, 0);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(40, 40);
            this.btnRefresh.TabIndex = 18;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // FormStagePositionInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 373);
            this.Name = "FormStagePositionInfo";
            this.Text = " ";
            this.Load += new System.EventHandler(this.FormStagePositionInfo_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.flpPosition.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.pnlRefresh.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.FlowLayoutPanel flpPosition;
        private System.Windows.Forms.Label lblSendReady;
        private System.Windows.Forms.Label lblReceiveReady;
        private System.Windows.Forms.Label lblExchangePossible;
        private System.Windows.Forms.Label lblGlassExist;
        private System.Windows.Forms.Label lblDoubleGlassExist;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlRefresh;
        private System.Windows.Forms.Button btnRefresh;
    }
}