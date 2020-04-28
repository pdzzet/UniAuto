namespace UniOPI
{
    partial class FormCassetteControl_RecipeChange
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
            this.tplBase = new System.Windows.Forms.TableLayoutPanel();
            this.grbSlot = new System.Windows.Forms.GroupBox();
            this.dgvCassette = new System.Windows.Forms.DataGridView();
            this.colLotName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProcessFlag = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colSlotNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOwnerID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOwnerType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOldRecipeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNewRecip = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNewPPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlSlot = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.chk_SelectAll = new System.Windows.Forms.CheckBox();
            this.btnSet = new System.Windows.Forms.Button();
            this.txtSelToSlotNo = new System.Windows.Forms.TextBox();
            this.lblSign = new System.Windows.Forms.Label();
            this.txtSelFrSlotNo = new System.Windows.Forms.TextBox();
            this.lblSelSlotNo = new System.Windows.Forms.Label();
            this.gbRecipe = new System.Windows.Forms.GroupBox();
            this.dgvRecipe = new System.Windows.Forms.DataGridView();
            this.colRecipeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlSetting = new System.Windows.Forms.Panel();
            this.btnSetToRows = new System.Windows.Forms.Button();
            this.btnSetToRow = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tplBase.SuspendLayout();
            this.grbSlot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCassette)).BeginInit();
            this.pnlSlot.SuspendLayout();
            this.gbRecipe.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipe)).BeginInit();
            this.pnlSetting.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(930, 30);
            this.lblCaption.Text = "Recipe Change";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tplBase);
            this.spcBase.Size = new System.Drawing.Size(990, 618);
            // 
            // tplBase
            // 
            this.tplBase.ColumnCount = 3;
            this.tplBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.tplBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tplBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tplBase.Controls.Add(this.grbSlot, 2, 1);
            this.tplBase.Controls.Add(this.pnlSlot, 0, 0);
            this.tplBase.Controls.Add(this.gbRecipe, 0, 1);
            this.tplBase.Controls.Add(this.pnlSetting, 1, 1);
            this.tplBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tplBase.Location = new System.Drawing.Point(0, 0);
            this.tplBase.Name = "tplBase";
            this.tplBase.RowCount = 3;
            this.tplBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tplBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 519F));
            this.tplBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tplBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tplBase.Size = new System.Drawing.Size(990, 587);
            this.tplBase.TabIndex = 1;
            // 
            // grbSlot
            // 
            this.grbSlot.Controls.Add(this.dgvCassette);
            this.grbSlot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbSlot.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbSlot.Location = new System.Drawing.Point(440, 56);
            this.grbSlot.Name = "grbSlot";
            this.grbSlot.Size = new System.Drawing.Size(547, 513);
            this.grbSlot.TabIndex = 3;
            this.grbSlot.TabStop = false;
            // 
            // dgvCassette
            // 
            this.dgvCassette.AllowUserToAddRows = false;
            this.dgvCassette.AllowUserToDeleteRows = false;
            this.dgvCassette.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvCassette.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvCassette.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCassette.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvCassette.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCassette.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLotName,
            this.colProcessFlag,
            this.colSlotNo,
            this.colOwnerID,
            this.colOwnerType,
            this.colOldRecipeName,
            this.colNewRecip,
            this.colNewPPID});
            this.dgvCassette.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCassette.Location = new System.Drawing.Point(3, 22);
            this.dgvCassette.Name = "dgvCassette";
            this.dgvCassette.ReadOnly = true;
            this.dgvCassette.RowHeadersVisible = false;
            this.dgvCassette.RowTemplate.Height = 24;
            this.dgvCassette.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCassette.Size = new System.Drawing.Size(541, 488);
            this.dgvCassette.TabIndex = 17;
            // 
            // colLotName
            // 
            this.colLotName.HeaderText = "Lot Name";
            this.colLotName.Name = "colLotName";
            this.colLotName.ReadOnly = true;
            this.colLotName.Visible = false;
            // 
            // colProcessFlag
            // 
            this.colProcessFlag.HeaderText = "Process";
            this.colProcessFlag.Name = "colProcessFlag";
            this.colProcessFlag.ReadOnly = true;
            this.colProcessFlag.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colProcessFlag.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colProcessFlag.Visible = false;
            this.colProcessFlag.Width = 90;
            // 
            // colSlotNo
            // 
            this.colSlotNo.HeaderText = "Slot No";
            this.colSlotNo.Name = "colSlotNo";
            this.colSlotNo.ReadOnly = true;
            this.colSlotNo.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colSlotNo.Width = 80;
            // 
            // colOwnerID
            // 
            this.colOwnerID.HeaderText = "OwnerID";
            this.colOwnerID.Name = "colOwnerID";
            this.colOwnerID.ReadOnly = true;
            this.colOwnerID.Width = 65;
            // 
            // colOwnerType
            // 
            this.colOwnerType.HeaderText = "OwnerType";
            this.colOwnerType.Name = "colOwnerType";
            this.colOwnerType.ReadOnly = true;
            this.colOwnerType.Width = 90;
            // 
            // colOldRecipeName
            // 
            this.colOldRecipeName.HeaderText = "Old Recipe Name";
            this.colOldRecipeName.Name = "colOldRecipeName";
            this.colOldRecipeName.ReadOnly = true;
            this.colOldRecipeName.Visible = false;
            this.colOldRecipeName.Width = 140;
            // 
            // colNewRecip
            // 
            this.colNewRecip.HeaderText = "Recipe Name";
            this.colNewRecip.Name = "colNewRecip";
            this.colNewRecip.ReadOnly = true;
            this.colNewRecip.Width = 173;
            // 
            // colNewPPID
            // 
            this.colNewPPID.HeaderText = "PPID";
            this.colNewPPID.Name = "colNewPPID";
            this.colNewPPID.ReadOnly = true;
            this.colNewPPID.Width = 350;
            // 
            // pnlSlot
            // 
            this.tplBase.SetColumnSpan(this.pnlSlot, 3);
            this.pnlSlot.Controls.Add(this.btnCancel);
            this.pnlSlot.Controls.Add(this.btnOK);
            this.pnlSlot.Controls.Add(this.chk_SelectAll);
            this.pnlSlot.Controls.Add(this.btnSet);
            this.pnlSlot.Controls.Add(this.txtSelToSlotNo);
            this.pnlSlot.Controls.Add(this.lblSign);
            this.pnlSlot.Controls.Add(this.txtSelFrSlotNo);
            this.pnlSlot.Controls.Add(this.lblSelSlotNo);
            this.pnlSlot.Location = new System.Drawing.Point(3, 3);
            this.pnlSlot.Name = "pnlSlot";
            this.pnlSlot.Size = new System.Drawing.Size(984, 47);
            this.pnlSlot.TabIndex = 4;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(905, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(70, 30);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(834, 10);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(70, 30);
            this.btnOK.TabIndex = 22;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // chk_SelectAll
            // 
            this.chk_SelectAll.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk_SelectAll.ForeColor = System.Drawing.Color.Black;
            this.chk_SelectAll.Location = new System.Drawing.Point(6, 10);
            this.chk_SelectAll.Name = "chk_SelectAll";
            this.chk_SelectAll.Size = new System.Drawing.Size(85, 29);
            this.chk_SelectAll.TabIndex = 24;
            this.chk_SelectAll.Text = "Select All";
            this.chk_SelectAll.UseVisualStyleBackColor = true;
            this.chk_SelectAll.CheckedChanged += new System.EventHandler(this.chk_SelectAll_CheckedChanged);
            // 
            // btnSet
            // 
            this.btnSet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSet.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSet.Location = new System.Drawing.Point(306, 12);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(63, 27);
            this.btnSet.TabIndex = 23;
            this.btnSet.Text = "Set";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // txtSelToSlotNo
            // 
            this.txtSelToSlotNo.Location = new System.Drawing.Point(237, 13);
            this.txtSelToSlotNo.Name = "txtSelToSlotNo";
            this.txtSelToSlotNo.Size = new System.Drawing.Size(63, 22);
            this.txtSelToSlotNo.TabIndex = 13;
            // 
            // lblSign
            // 
            this.lblSign.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSign.ForeColor = System.Drawing.Color.Black;
            this.lblSign.Location = new System.Drawing.Point(217, 13);
            this.lblSign.Name = "lblSign";
            this.lblSign.Size = new System.Drawing.Size(20, 27);
            this.lblSign.TabIndex = 12;
            this.lblSign.Text = "~";
            this.lblSign.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtSelFrSlotNo
            // 
            this.txtSelFrSlotNo.Location = new System.Drawing.Point(153, 13);
            this.txtSelFrSlotNo.Name = "txtSelFrSlotNo";
            this.txtSelFrSlotNo.Size = new System.Drawing.Size(63, 22);
            this.txtSelFrSlotNo.TabIndex = 11;
            this.txtSelFrSlotNo.Tag = "001";
            this.txtSelFrSlotNo.Text = "001";
            // 
            // lblSelSlotNo
            // 
            this.lblSelSlotNo.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelSlotNo.ForeColor = System.Drawing.Color.Blue;
            this.lblSelSlotNo.Location = new System.Drawing.Point(100, 10);
            this.lblSelSlotNo.Name = "lblSelSlotNo";
            this.lblSelSlotNo.Size = new System.Drawing.Size(55, 27);
            this.lblSelSlotNo.TabIndex = 10;
            this.lblSelSlotNo.Text = "Slot No";
            this.lblSelSlotNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gbRecipe
            // 
            this.gbRecipe.Controls.Add(this.dgvRecipe);
            this.gbRecipe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbRecipe.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbRecipe.Location = new System.Drawing.Point(3, 56);
            this.gbRecipe.Name = "gbRecipe";
            this.gbRecipe.Size = new System.Drawing.Size(394, 513);
            this.gbRecipe.TabIndex = 2;
            this.gbRecipe.TabStop = false;
            this.gbRecipe.Text = "Recipe";
            this.gbRecipe.Visible = false;
            // 
            // dgvRecipe
            // 
            this.dgvRecipe.AllowUserToAddRows = false;
            this.dgvRecipe.AllowUserToDeleteRows = false;
            this.dgvRecipe.AllowUserToResizeRows = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvRecipe.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvRecipe.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRecipe.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvRecipe.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecipe.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRecipeName,
            this.colPPID});
            this.dgvRecipe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRecipe.Location = new System.Drawing.Point(3, 22);
            this.dgvRecipe.MultiSelect = false;
            this.dgvRecipe.Name = "dgvRecipe";
            this.dgvRecipe.ReadOnly = true;
            this.dgvRecipe.RowHeadersVisible = false;
            this.dgvRecipe.RowTemplate.Height = 24;
            this.dgvRecipe.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecipe.Size = new System.Drawing.Size(388, 488);
            this.dgvRecipe.TabIndex = 17;
            // 
            // colRecipeName
            // 
            this.colRecipeName.HeaderText = "RecipeName";
            this.colRecipeName.Name = "colRecipeName";
            this.colRecipeName.ReadOnly = true;
            // 
            // colPPID
            // 
            this.colPPID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colPPID.HeaderText = "PPID";
            this.colPPID.Name = "colPPID";
            this.colPPID.ReadOnly = true;
            // 
            // pnlSetting
            // 
            this.pnlSetting.Controls.Add(this.btnSetToRows);
            this.pnlSetting.Controls.Add(this.btnSetToRow);
            this.pnlSetting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSetting.Location = new System.Drawing.Point(403, 56);
            this.pnlSetting.Name = "pnlSetting";
            this.pnlSetting.Size = new System.Drawing.Size(31, 513);
            this.pnlSetting.TabIndex = 5;
            // 
            // btnSetToRows
            // 
            this.btnSetToRows.BackgroundImage = global::UniOPI.Properties.Resources.SetAll;
            this.btnSetToRows.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSetToRows.Location = new System.Drawing.Point(1, 197);
            this.btnSetToRows.Name = "btnSetToRows";
            this.btnSetToRows.Size = new System.Drawing.Size(30, 30);
            this.btnSetToRows.TabIndex = 6;
            this.btnSetToRows.UseVisualStyleBackColor = true;
            this.btnSetToRows.Click += new System.EventHandler(this.btnSetToRows_Click);
            // 
            // btnSetToRow
            // 
            this.btnSetToRow.BackgroundImage = global::UniOPI.Properties.Resources.SetOne;
            this.btnSetToRow.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSetToRow.Location = new System.Drawing.Point(1, 144);
            this.btnSetToRow.Name = "btnSetToRow";
            this.btnSetToRow.Size = new System.Drawing.Size(30, 30);
            this.btnSetToRow.TabIndex = 5;
            this.btnSetToRow.UseVisualStyleBackColor = true;
            this.btnSetToRow.Click += new System.EventHandler(this.btnSetToRow_Click);
            // 
            // FormCassetteControl_RecipeChange
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(990, 618);
            this.Name = "FormCassetteControl_RecipeChange";
            this.Text = "   ";
            this.Load += new System.EventHandler(this.FormCassetteControl_RecipeChange_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tplBase.ResumeLayout(false);
            this.grbSlot.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCassette)).EndInit();
            this.pnlSlot.ResumeLayout(false);
            this.pnlSlot.PerformLayout();
            this.gbRecipe.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipe)).EndInit();
            this.pnlSetting.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tplBase;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox gbRecipe;
        private System.Windows.Forms.DataGridView dgvRecipe;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecipeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPPID;
        private System.Windows.Forms.GroupBox grbSlot;
        private System.Windows.Forms.DataGridView dgvCassette;
        private System.Windows.Forms.Panel pnlSlot;
        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.TextBox txtSelToSlotNo;
        private System.Windows.Forms.Label lblSign;
        private System.Windows.Forms.TextBox txtSelFrSlotNo;
        private System.Windows.Forms.Label lblSelSlotNo;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chk_SelectAll;
        private System.Windows.Forms.Panel pnlSetting;
        private System.Windows.Forms.Button btnSetToRow;
        private System.Windows.Forms.Button btnSetToRows;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLotName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colProcessFlag;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSlotNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOwnerID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOwnerType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOldRecipeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNewRecip;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNewPPID;
    }
}