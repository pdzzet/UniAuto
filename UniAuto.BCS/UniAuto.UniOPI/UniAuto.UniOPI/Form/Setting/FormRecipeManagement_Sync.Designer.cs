namespace UniOPI
{
    partial class FormRecipeSync
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.dgvList = new System.Windows.Forms.DataGridView();
            this.colChkChecked = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFabType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLineType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.rdoOnlineRemote = new System.Windows.Forms.RadioButton();
            this.lblMESMode = new System.Windows.Forms.Label();
            this.rdoOnlineLocal = new System.Windows.Forms.RadioButton();
            this.rdoOffline = new System.Windows.Forms.RadioButton();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnSync = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvList)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(788, 30);
            this.lblCaption.Text = "Recipe Sync";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(848, 562);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.dgvList, 0, 1);
            this.tlpBase.Controls.Add(this.pnlHeader, 0, 0);
            this.tlpBase.Controls.Add(this.pnlButton, 0, 2);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 3;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F));
            this.tlpBase.Size = new System.Drawing.Size(848, 531);
            this.tlpBase.TabIndex = 0;
            // 
            // dgvList
            // 
            this.dgvList.AllowUserToAddRows = false;
            this.dgvList.AllowUserToDeleteRows = false;
            this.dgvList.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvList.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvList.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colChkChecked,
            this.colServerName,
            this.colLineName,
            this.colFabType,
            this.colLineType});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvList.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvList.Location = new System.Drawing.Point(3, 53);
            this.dgvList.Name = "dgvList";
            this.dgvList.RowHeadersVisible = false;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvList.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvList.RowTemplate.Height = 24;
            this.dgvList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvList.Size = new System.Drawing.Size(842, 418);
            this.dgvList.TabIndex = 28;
            // 
            // colChkChecked
            // 
            this.colChkChecked.DataPropertyName = "Checked";
            this.colChkChecked.HeaderText = "Checked";
            this.colChkChecked.Name = "colChkChecked";
            this.colChkChecked.Width = 80;
            // 
            // colServerName
            // 
            this.colServerName.DataPropertyName = "LineID";
            this.colServerName.HeaderText = "Line ID";
            this.colServerName.Name = "colServerName";
            this.colServerName.ReadOnly = true;
            this.colServerName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colServerName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colServerName.Width = 150;
            // 
            // colLineName
            // 
            this.colLineName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colLineName.DataPropertyName = "LineName";
            this.colLineName.HeaderText = "LineName";
            this.colLineName.Name = "colLineName";
            this.colLineName.ReadOnly = true;
            // 
            // colFabType
            // 
            this.colFabType.DataPropertyName = "FabType";
            this.colFabType.HeaderText = "Fab Type";
            this.colFabType.Name = "colFabType";
            this.colFabType.ReadOnly = true;
            this.colFabType.Visible = false;
            // 
            // colLineType
            // 
            this.colLineType.DataPropertyName = "LineType";
            this.colLineType.HeaderText = "Line Type";
            this.colLineType.Name = "colLineType";
            this.colLineType.Visible = false;
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.rdoOnlineRemote);
            this.pnlHeader.Controls.Add(this.lblMESMode);
            this.pnlHeader.Controls.Add(this.rdoOnlineLocal);
            this.pnlHeader.Controls.Add(this.rdoOffline);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Location = new System.Drawing.Point(3, 3);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(842, 44);
            this.pnlHeader.TabIndex = 16;
            // 
            // rdoOnlineRemote
            // 
            this.rdoOnlineRemote.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.rdoOnlineRemote.Location = new System.Drawing.Point(309, 11);
            this.rdoOnlineRemote.Name = "rdoOnlineRemote";
            this.rdoOnlineRemote.Size = new System.Drawing.Size(131, 25);
            this.rdoOnlineRemote.TabIndex = 38;
            this.rdoOnlineRemote.Tag = "REMOTE";
            this.rdoOnlineRemote.Text = "Online Remote";
            this.rdoOnlineRemote.UseVisualStyleBackColor = true;
            this.rdoOnlineRemote.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
            // 
            // lblMESMode
            // 
            this.lblMESMode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMESMode.Location = new System.Drawing.Point(7, 14);
            this.lblMESMode.Name = "lblMESMode";
            this.lblMESMode.Size = new System.Drawing.Size(105, 21);
            this.lblMESMode.TabIndex = 37;
            this.lblMESMode.Text = "MES Mode";
            this.lblMESMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // rdoOnlineLocal
            // 
            this.rdoOnlineLocal.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.rdoOnlineLocal.Location = new System.Drawing.Point(194, 12);
            this.rdoOnlineLocal.Name = "rdoOnlineLocal";
            this.rdoOnlineLocal.Size = new System.Drawing.Size(112, 25);
            this.rdoOnlineLocal.TabIndex = 36;
            this.rdoOnlineLocal.Tag = "LOCAL";
            this.rdoOnlineLocal.Text = "Online Local";
            this.rdoOnlineLocal.UseVisualStyleBackColor = true;
            this.rdoOnlineLocal.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
            // 
            // rdoOffline
            // 
            this.rdoOffline.Checked = true;
            this.rdoOffline.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.rdoOffline.Location = new System.Drawing.Point(117, 12);
            this.rdoOffline.Name = "rdoOffline";
            this.rdoOffline.Size = new System.Drawing.Size(74, 25);
            this.rdoOffline.TabIndex = 35;
            this.rdoOffline.TabStop = true;
            this.rdoOffline.Tag = "OFFLINE";
            this.rdoOffline.Text = "Offline";
            this.rdoOffline.UseVisualStyleBackColor = true;
            this.rdoOffline.CheckedChanged += new System.EventHandler(this.rdo_CheckedChanged);
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnSync);
            this.pnlButton.Controls.Add(this.btnClose);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(3, 477);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(842, 51);
            this.pnlButton.TabIndex = 15;
            // 
            // btnSync
            // 
            this.btnSync.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSync.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSync.Location = new System.Drawing.Point(324, 5);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(95, 35);
            this.btnSync.TabIndex = 27;
            this.btnSync.Tag = "Sync";
            this.btnSync.Text = "Sync";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.btn_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(419, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(95, 35);
            this.btnClose.TabIndex = 26;
            this.btnClose.Tag = "Close";
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btn_Click);
            // 
            // FormRecipeSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(848, 562);
            this.Name = "FormRecipeSync";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormRecipeSync_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvList)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Button btnSync;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.DataGridView dgvList;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.RadioButton rdoOnlineRemote;
        private System.Windows.Forms.Label lblMESMode;
        private System.Windows.Forms.RadioButton rdoOnlineLocal;
        private System.Windows.Forms.RadioButton rdoOffline;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colChkChecked;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFabType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLineType;


    }
}