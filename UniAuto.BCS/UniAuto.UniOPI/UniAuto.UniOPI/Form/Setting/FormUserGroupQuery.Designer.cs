namespace UniOPI
{
    partial class FormUserGroupQuery
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
            this.tlpUserGroupQuery = new System.Windows.Forms.TableLayoutPanel();
            this.pnlGroupQuery = new System.Windows.Forms.Panel();
            this.btnHide = new System.Windows.Forms.Button();
            this.btnExpand = new System.Windows.Forms.Button();
            this.cboGroupID = new System.Windows.Forms.ComboBox();
            this.btnQuery = new System.Windows.Forms.Button();
            this.lblGroupID = new System.Windows.Forms.Label();
            this.grbUserGroupQuery = new System.Windows.Forms.GroupBox();
            this.dgvGroup = new System.Windows.Forms.DataGridView();
            this.colGroupID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGroupName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colButtonKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExpand = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMainButton = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSubButton = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFunButton = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.chkVisible = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.chkEnable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colButtonDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpUserGroupQuery.SuspendLayout();
            this.pnlGroupQuery.SuspendLayout();
            this.grbUserGroupQuery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGroup)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1041, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpUserGroupQuery);
            this.spcBase.Size = new System.Drawing.Size(1101, 522);
            // 
            // tlpUserGroupQuery
            // 
            this.tlpUserGroupQuery.ColumnCount = 1;
            this.tlpUserGroupQuery.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpUserGroupQuery.Controls.Add(this.pnlGroupQuery, 0, 0);
            this.tlpUserGroupQuery.Controls.Add(this.grbUserGroupQuery, 0, 1);
            this.tlpUserGroupQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpUserGroupQuery.Location = new System.Drawing.Point(0, 0);
            this.tlpUserGroupQuery.Name = "tlpUserGroupQuery";
            this.tlpUserGroupQuery.RowCount = 2;
            this.tlpUserGroupQuery.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tlpUserGroupQuery.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpUserGroupQuery.Size = new System.Drawing.Size(1101, 491);
            this.tlpUserGroupQuery.TabIndex = 0;
            // 
            // pnlGroupQuery
            // 
            this.pnlGroupQuery.Controls.Add(this.btnHide);
            this.pnlGroupQuery.Controls.Add(this.btnExpand);
            this.pnlGroupQuery.Controls.Add(this.cboGroupID);
            this.pnlGroupQuery.Controls.Add(this.btnQuery);
            this.pnlGroupQuery.Controls.Add(this.lblGroupID);
            this.pnlGroupQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGroupQuery.Location = new System.Drawing.Point(3, 3);
            this.pnlGroupQuery.Name = "pnlGroupQuery";
            this.pnlGroupQuery.Size = new System.Drawing.Size(1095, 54);
            this.pnlGroupQuery.TabIndex = 1;
            // 
            // btnHide
            // 
            this.btnHide.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnHide.Location = new System.Drawing.Point(976, 9);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(110, 29);
            this.btnHide.TabIndex = 51;
            this.btnHide.Text = "Hide All";
            this.btnHide.UseVisualStyleBackColor = true;
            this.btnHide.Click += new System.EventHandler(this.btnHide_Click);
            // 
            // btnExpand
            // 
            this.btnExpand.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnExpand.Location = new System.Drawing.Point(860, 9);
            this.btnExpand.Name = "btnExpand";
            this.btnExpand.Size = new System.Drawing.Size(110, 29);
            this.btnExpand.TabIndex = 50;
            this.btnExpand.Text = "Expand All";
            this.btnExpand.UseVisualStyleBackColor = true;
            this.btnExpand.Click += new System.EventHandler(this.btnExpand_Click);
            // 
            // cboGroupID
            // 
            this.cboGroupID.Font = new System.Drawing.Font("Calibri", 11.25F);
            this.cboGroupID.FormattingEnabled = true;
            this.cboGroupID.Location = new System.Drawing.Point(93, 12);
            this.cboGroupID.Name = "cboGroupID";
            this.cboGroupID.Size = new System.Drawing.Size(220, 26);
            this.cboGroupID.TabIndex = 49;
            // 
            // btnQuery
            // 
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.btnQuery.Location = new System.Drawing.Point(323, 10);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(110, 29);
            this.btnQuery.TabIndex = 2;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // lblGroupID
            // 
            this.lblGroupID.AutoSize = true;
            this.lblGroupID.Font = new System.Drawing.Font("Calibri", 12.75F);
            this.lblGroupID.Location = new System.Drawing.Point(22, 14);
            this.lblGroupID.Name = "lblGroupID";
            this.lblGroupID.Size = new System.Drawing.Size(72, 21);
            this.lblGroupID.TabIndex = 0;
            this.lblGroupID.Text = "Group ID";
            // 
            // grbUserGroupQuery
            // 
            this.grbUserGroupQuery.Controls.Add(this.dgvGroup);
            this.grbUserGroupQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbUserGroupQuery.Location = new System.Drawing.Point(3, 63);
            this.grbUserGroupQuery.Name = "grbUserGroupQuery";
            this.grbUserGroupQuery.Size = new System.Drawing.Size(1095, 425);
            this.grbUserGroupQuery.TabIndex = 0;
            this.grbUserGroupQuery.TabStop = false;
            // 
            // dgvGroup
            // 
            this.dgvGroup.AllowUserToAddRows = false;
            this.dgvGroup.AllowUserToDeleteRows = false;
            this.dgvGroup.AllowUserToOrderColumns = true;
            this.dgvGroup.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvGroup.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvGroup.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvGroup.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvGroup.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvGroup.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvGroup.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colGroupID,
            this.colGroupName,
            this.colButtonKey,
            this.colExpand,
            this.colMainButton,
            this.colSubButton,
            this.colFunButton,
            this.chkVisible,
            this.chkEnable,
            this.colButtonDesc});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvGroup.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvGroup.Location = new System.Drawing.Point(3, 18);
            this.dgvGroup.Name = "dgvGroup";
            this.dgvGroup.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvGroup.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvGroup.RowHeadersVisible = false;
            this.dgvGroup.RowTemplate.Height = 24;
            this.dgvGroup.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvGroup.Size = new System.Drawing.Size(1089, 404);
            this.dgvGroup.TabIndex = 0;
            this.dgvGroup.DataSourceChanged += new System.EventHandler(this.dgvGroup_DataSourceChanged);
            this.dgvGroup.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvGroup_CellClick);
            // 
            // colGroupID
            // 
            this.colGroupID.DataPropertyName = "GroupID";
            this.colGroupID.FillWeight = 75F;
            this.colGroupID.HeaderText = "Group ID";
            this.colGroupID.Name = "colGroupID";
            this.colGroupID.ReadOnly = true;
            // 
            // colGroupName
            // 
            this.colGroupName.DataPropertyName = "GroupName";
            this.colGroupName.FillWeight = 75F;
            this.colGroupName.HeaderText = "Group Name";
            this.colGroupName.Name = "colGroupName";
            this.colGroupName.ReadOnly = true;
            // 
            // colButtonKey
            // 
            this.colButtonKey.DataPropertyName = "ButtonKey";
            this.colButtonKey.HeaderText = "Button Key";
            this.colButtonKey.Name = "colButtonKey";
            this.colButtonKey.ReadOnly = true;
            this.colButtonKey.Visible = false;
            // 
            // colExpand
            // 
            this.colExpand.DataPropertyName = "EXPAND";
            this.colExpand.FillWeight = 15F;
            this.colExpand.HeaderText = "";
            this.colExpand.Name = "colExpand";
            this.colExpand.ReadOnly = true;
            // 
            // colMainButton
            // 
            this.colMainButton.DataPropertyName = "ButtonMain";
            this.colMainButton.FillWeight = 85.27919F;
            this.colMainButton.HeaderText = "Button Main layout";
            this.colMainButton.Name = "colMainButton";
            this.colMainButton.ReadOnly = true;
            // 
            // colSubButton
            // 
            this.colSubButton.DataPropertyName = "ButtonSub";
            this.colSubButton.FillWeight = 85.27919F;
            this.colSubButton.HeaderText = "Button Sub layout";
            this.colSubButton.Name = "colSubButton";
            this.colSubButton.ReadOnly = true;
            // 
            // colFunButton
            // 
            this.colFunButton.DataPropertyName = "ButtonFun";
            this.colFunButton.FillWeight = 85.27919F;
            this.colFunButton.HeaderText = "Button Side layout";
            this.colFunButton.Name = "colFunButton";
            this.colFunButton.ReadOnly = true;
            // 
            // chkVisible
            // 
            this.chkVisible.DataPropertyName = "VISIBLE";
            this.chkVisible.FalseValue = "N";
            this.chkVisible.FillWeight = 35F;
            this.chkVisible.HeaderText = "Visible";
            this.chkVisible.Name = "chkVisible";
            this.chkVisible.ReadOnly = true;
            this.chkVisible.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.chkVisible.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.chkVisible.TrueValue = "Y";
            // 
            // chkEnable
            // 
            this.chkEnable.DataPropertyName = "ENABLE";
            this.chkEnable.FalseValue = "N";
            this.chkEnable.FillWeight = 35F;
            this.chkEnable.HeaderText = "Enable";
            this.chkEnable.Name = "chkEnable";
            this.chkEnable.ReadOnly = true;
            this.chkEnable.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.chkEnable.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.chkEnable.TrueValue = "Y";
            // 
            // colButtonDesc
            // 
            this.colButtonDesc.DataPropertyName = "ButtonDesc";
            this.colButtonDesc.HeaderText = "ButtonDesc";
            this.colButtonDesc.Name = "colButtonDesc";
            this.colButtonDesc.ReadOnly = true;
            this.colButtonDesc.Visible = false;
            // 
            // FormUserGroupQuery
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1101, 522);
            this.Name = "FormUserGroupQuery";
            this.Text = "FormUserGroupQuery";
            this.Load += new System.EventHandler(this.FormUserGroupQuery_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpUserGroupQuery.ResumeLayout(false);
            this.pnlGroupQuery.ResumeLayout(false);
            this.pnlGroupQuery.PerformLayout();
            this.grbUserGroupQuery.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvGroup)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpUserGroupQuery;
        private System.Windows.Forms.GroupBox grbUserGroupQuery;
        private System.Windows.Forms.DataGridView dgvGroup;
        private System.Windows.Forms.Panel pnlGroupQuery;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Label lblGroupID;
        private System.Windows.Forms.ComboBox cboGroupID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGroupID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGroupName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colButtonKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExpand;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMainButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSubButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFunButton;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chkVisible;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chkEnable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colButtonDesc;
        private System.Windows.Forms.Button btnExpand;
        private System.Windows.Forms.Button btnHide;
    }
}