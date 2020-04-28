namespace UniOPI
{
    partial class FormPalletControl_Local
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.dgvBox = new System.Windows.Forms.DataGridView();
            this.colSlotNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGlassID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnMapDownload = new System.Windows.Forms.Button();
            this.grbLot = new System.Windows.Forms.GroupBox();
            this.flpPalletInfo = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlPalletNo = new System.Windows.Forms.Panel();
            this.lblPalletNo = new System.Windows.Forms.Label();
            this.txtPalletNo = new System.Windows.Forms.TextBox();
            this.pnlPalletID = new System.Windows.Forms.Panel();
            this.lblPalletID = new System.Windows.Forms.Label();
            this.txtPalletID = new System.Windows.Forms.TextBox();
            this.pnlDenseBoxCount = new System.Windows.Forms.Panel();
            this.txtDenseBoxCount = new System.Windows.Forms.TextBox();
            this.lblDenseBoxCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBox)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.grbLot.SuspendLayout();
            this.flpPalletInfo.SuspendLayout();
            this.pnlPalletNo.SuspendLayout();
            this.pnlPalletID.SuspendLayout();
            this.pnlDenseBoxCount.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(584, 30);
            this.lblCaption.Text = "Local Pallet Control";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(644, 522);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 3;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tlpBase.Controls.Add(this.dgvBox, 1, 2);
            this.tlpBase.Controls.Add(this.panel2, 1, 1);
            this.tlpBase.Controls.Add(this.pnlButtons, 1, 3);
            this.tlpBase.Controls.Add(this.grbLot, 1, 0);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 4;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Size = new System.Drawing.Size(644, 491);
            this.tlpBase.TabIndex = 5;
            // 
            // dgvBox
            // 
            this.dgvBox.AllowUserToAddRows = false;
            this.dgvBox.AllowUserToDeleteRows = false;
            this.dgvBox.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvBox.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvBox.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvBox.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvBox.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBox.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSlotNo,
            this.colGlassID});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvBox.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBox.Location = new System.Drawing.Point(13, 118);
            this.dgvBox.Name = "dgvBox";
            this.dgvBox.RowHeadersVisible = false;
            this.dgvBox.RowTemplate.Height = 24;
            this.dgvBox.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvBox.Size = new System.Drawing.Size(618, 327);
            this.dgvBox.TabIndex = 23;
            // 
            // colSlotNo
            // 
            this.colSlotNo.HeaderText = "No";
            this.colSlotNo.Name = "colSlotNo";
            this.colSlotNo.ReadOnly = true;
            this.colSlotNo.Width = 40;
            // 
            // colGlassID
            // 
            this.colGlassID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colGlassID.HeaderText = "Glass ID";
            this.colGlassID.MinimumWidth = 150;
            this.colGlassID.Name = "colGlassID";
            this.colGlassID.ReadOnly = true;
            // 
            // panel2
            // 
            this.panel2.BackgroundImage = global::UniOPI.Properties.Resources.line2;
            this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel2.Location = new System.Drawing.Point(13, 113);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(618, 1);
            this.panel2.TabIndex = 21;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnMapDownload);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButtons.Location = new System.Drawing.Point(13, 451);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(618, 37);
            this.pnlButtons.TabIndex = 16;
            // 
            // btnMapDownload
            // 
            this.btnMapDownload.BackColor = System.Drawing.SystemColors.Control;
            this.btnMapDownload.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnMapDownload.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.btnMapDownload.Location = new System.Drawing.Point(463, 4);
            this.btnMapDownload.Name = "btnMapDownload";
            this.btnMapDownload.Size = new System.Drawing.Size(150, 30);
            this.btnMapDownload.TabIndex = 17;
            this.btnMapDownload.Text = "Map Download";
            this.btnMapDownload.UseVisualStyleBackColor = false;
            this.btnMapDownload.Click += new System.EventHandler(this.btnMapDownload_Click);
            // 
            // grbLot
            // 
            this.grbLot.AutoSize = true;
            this.grbLot.Controls.Add(this.flpPalletInfo);
            this.grbLot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbLot.Location = new System.Drawing.Point(13, 0);
            this.grbLot.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.grbLot.Name = "grbLot";
            this.grbLot.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.grbLot.Size = new System.Drawing.Size(618, 107);
            this.grbLot.TabIndex = 22;
            this.grbLot.TabStop = false;
            // 
            // flpPalletInfo
            // 
            this.flpPalletInfo.AutoScroll = true;
            this.flpPalletInfo.Controls.Add(this.pnlPalletNo);
            this.flpPalletInfo.Controls.Add(this.pnlPalletID);
            this.flpPalletInfo.Controls.Add(this.pnlDenseBoxCount);
            this.flpPalletInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpPalletInfo.Location = new System.Drawing.Point(3, 15);
            this.flpPalletInfo.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.flpPalletInfo.Name = "flpPalletInfo";
            this.flpPalletInfo.Size = new System.Drawing.Size(612, 89);
            this.flpPalletInfo.TabIndex = 15;
            // 
            // pnlPalletNo
            // 
            this.pnlPalletNo.Controls.Add(this.lblPalletNo);
            this.pnlPalletNo.Controls.Add(this.txtPalletNo);
            this.pnlPalletNo.Location = new System.Drawing.Point(1, 1);
            this.pnlPalletNo.Margin = new System.Windows.Forms.Padding(1, 1, 3, 1);
            this.pnlPalletNo.Name = "pnlPalletNo";
            this.pnlPalletNo.Size = new System.Drawing.Size(608, 25);
            this.pnlPalletNo.TabIndex = 48;
            this.pnlPalletNo.Tag = "LotID";
            // 
            // lblPalletNo
            // 
            this.lblPalletNo.BackColor = System.Drawing.Color.Black;
            this.lblPalletNo.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblPalletNo.ForeColor = System.Drawing.Color.White;
            this.lblPalletNo.Location = new System.Drawing.Point(3, 1);
            this.lblPalletNo.Name = "lblPalletNo";
            this.lblPalletNo.Size = new System.Drawing.Size(170, 23);
            this.lblPalletNo.TabIndex = 5;
            this.lblPalletNo.Text = "Pallet No";
            this.lblPalletNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPalletNo
            // 
            this.txtPalletNo.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtPalletNo.Location = new System.Drawing.Point(175, 0);
            this.txtPalletNo.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.txtPalletNo.Name = "txtPalletNo";
            this.txtPalletNo.ReadOnly = true;
            this.txtPalletNo.Size = new System.Drawing.Size(210, 25);
            this.txtPalletNo.TabIndex = 0;
            // 
            // pnlPalletID
            // 
            this.pnlPalletID.Controls.Add(this.lblPalletID);
            this.pnlPalletID.Controls.Add(this.txtPalletID);
            this.pnlPalletID.Location = new System.Drawing.Point(1, 28);
            this.pnlPalletID.Margin = new System.Windows.Forms.Padding(1, 1, 3, 1);
            this.pnlPalletID.Name = "pnlPalletID";
            this.pnlPalletID.Size = new System.Drawing.Size(608, 25);
            this.pnlPalletID.TabIndex = 50;
            this.pnlPalletID.Tag = "PRODUCTOWNER";
            // 
            // lblPalletID
            // 
            this.lblPalletID.BackColor = System.Drawing.Color.Black;
            this.lblPalletID.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblPalletID.ForeColor = System.Drawing.Color.White;
            this.lblPalletID.Location = new System.Drawing.Point(3, 1);
            this.lblPalletID.Name = "lblPalletID";
            this.lblPalletID.Size = new System.Drawing.Size(170, 23);
            this.lblPalletID.TabIndex = 5;
            this.lblPalletID.Text = "Pallet ID";
            this.lblPalletID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPalletID
            // 
            this.txtPalletID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtPalletID.Location = new System.Drawing.Point(175, 0);
            this.txtPalletID.Name = "txtPalletID";
            this.txtPalletID.ReadOnly = true;
            this.txtPalletID.Size = new System.Drawing.Size(210, 25);
            this.txtPalletID.TabIndex = 2;
            // 
            // pnlDenseBoxCount
            // 
            this.pnlDenseBoxCount.Controls.Add(this.txtDenseBoxCount);
            this.pnlDenseBoxCount.Controls.Add(this.lblDenseBoxCount);
            this.pnlDenseBoxCount.Location = new System.Drawing.Point(1, 55);
            this.pnlDenseBoxCount.Margin = new System.Windows.Forms.Padding(1, 1, 3, 1);
            this.pnlDenseBoxCount.Name = "pnlDenseBoxCount";
            this.pnlDenseBoxCount.Size = new System.Drawing.Size(608, 25);
            this.pnlDenseBoxCount.TabIndex = 57;
            this.pnlDenseBoxCount.Tag = "PRODUCTOWNER";
            // 
            // txtDenseBoxCount
            // 
            this.txtDenseBoxCount.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtDenseBoxCount.Location = new System.Drawing.Point(175, 0);
            this.txtDenseBoxCount.MaxLength = 2;
            this.txtDenseBoxCount.Name = "txtDenseBoxCount";
            this.txtDenseBoxCount.ReadOnly = true;
            this.txtDenseBoxCount.Size = new System.Drawing.Size(210, 25);
            this.txtDenseBoxCount.TabIndex = 2;
            // 
            // lblDenseBoxCount
            // 
            this.lblDenseBoxCount.BackColor = System.Drawing.Color.Black;
            this.lblDenseBoxCount.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblDenseBoxCount.ForeColor = System.Drawing.Color.White;
            this.lblDenseBoxCount.Location = new System.Drawing.Point(3, 1);
            this.lblDenseBoxCount.Name = "lblDenseBoxCount";
            this.lblDenseBoxCount.Size = new System.Drawing.Size(170, 23);
            this.lblDenseBoxCount.TabIndex = 5;
            this.lblDenseBoxCount.Text = "Dense Box Count";
            this.lblDenseBoxCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FormPalletControl_Local
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 522);
            this.ControlBox = true;
            this.Name = "FormPalletControl_Local";
            this.Text = "FormPalletControl_Local";
            this.Load += new System.EventHandler(this.FormPalletControl_Local_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.tlpBase.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBox)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.grbLot.ResumeLayout(false);
            this.flpPalletInfo.ResumeLayout(false);
            this.pnlPalletNo.ResumeLayout(false);
            this.pnlPalletNo.PerformLayout();
            this.pnlPalletID.ResumeLayout(false);
            this.pnlPalletID.PerformLayout();
            this.pnlDenseBoxCount.ResumeLayout(false);
            this.pnlDenseBoxCount.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnMapDownload;
        private System.Windows.Forms.GroupBox grbLot;
        private System.Windows.Forms.DataGridView dgvBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGlassID;
        private System.Windows.Forms.FlowLayoutPanel flpPalletInfo;
        private System.Windows.Forms.Panel pnlPalletNo;
        private System.Windows.Forms.Label lblPalletNo;
        private System.Windows.Forms.TextBox txtPalletNo;
        private System.Windows.Forms.Panel pnlPalletID;
        private System.Windows.Forms.Label lblPalletID;
        private System.Windows.Forms.TextBox txtPalletID;
        private System.Windows.Forms.Panel pnlDenseBoxCount;
        private System.Windows.Forms.TextBox txtDenseBoxCount;
        private System.Windows.Forms.Label lblDenseBoxCount;
    }
}