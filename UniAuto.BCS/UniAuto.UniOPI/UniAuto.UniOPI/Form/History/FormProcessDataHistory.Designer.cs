namespace UniOPI
{
    partial class FormProcessDataHistory
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProcessDataHistory));
            this.tlpBase = new System.Windows.Forms.TableLayoutPanel();
            this.tlpHeader = new System.Windows.Forms.TableLayoutPanel();
            this.flpnlCondition = new System.Windows.Forms.FlowLayoutPanel();
            this.grbDateTime = new System.Windows.Forms.GroupBox();
            this.lblTo = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.lblFrom = new System.Windows.Forms.Label();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.grbLatest = new System.Windows.Forms.GroupBox();
            this.rdoMonth = new System.Windows.Forms.RadioButton();
            this.rdoWeek = new System.Windows.Forms.RadioButton();
            this.rdoDay = new System.Windows.Forms.RadioButton();
            this.rdoHour = new System.Windows.Forms.RadioButton();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnQuery = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.gbHistory = new System.Windows.Forms.GroupBox();
            this.dgvData = new UniOPI.PagedGridView();
            this.bnavRecord = new System.Windows.Forms.BindingNavigator(this.components);
            this.bindingNavigatorCountItem = new System.Windows.Forms.ToolStripLabel();
            this.bindingNavigatorMoveFirstItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMovePreviousItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorPositionItem = new System.Windows.Forms.ToolStripTextBox();
            this.bindingNavigatorSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.bindingNavigatorMoveNextItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorMoveLastItem = new System.Windows.Forms.ToolStripButton();
            this.bindingNavigatorSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.tlpHeader.SuspendLayout();
            this.grbDateTime.SuspendLayout();
            this.grbLatest.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.gbHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bnavRecord)).BeginInit();
            this.bnavRecord.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1080, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(1140, 594);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 3;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.tlpBase.Controls.Add(this.tlpHeader, 1, 0);
            this.tlpBase.Controls.Add(this.gbHistory, 1, 1);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 128F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Size = new System.Drawing.Size(1140, 563);
            this.tlpBase.TabIndex = 4;
            // 
            // tlpHeader
            // 
            this.tlpHeader.ColumnCount = 4;
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 156F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 214F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 99F));
            this.tlpHeader.Controls.Add(this.flpnlCondition, 0, 0);
            this.tlpHeader.Controls.Add(this.grbDateTime, 0, 0);
            this.tlpHeader.Controls.Add(this.grbLatest, 0, 0);
            this.tlpHeader.Controls.Add(this.pnlButton, 3, 0);
            this.tlpHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpHeader.Location = new System.Drawing.Point(8, 3);
            this.tlpHeader.Name = "tlpHeader";
            this.tlpHeader.RowCount = 1;
            this.tlpHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpHeader.Size = new System.Drawing.Size(1124, 122);
            this.tlpHeader.TabIndex = 10;
            // 
            // flpnlCondition
            // 
            this.flpnlCondition.Location = new System.Drawing.Point(373, 3);
            this.flpnlCondition.Name = "flpnlCondition";
            this.flpnlCondition.Size = new System.Drawing.Size(649, 115);
            this.flpnlCondition.TabIndex = 9;
            // 
            // grbDateTime
            // 
            this.grbDateTime.Controls.Add(this.lblTo);
            this.grbDateTime.Controls.Add(this.dtpStartDate);
            this.grbDateTime.Controls.Add(this.lblFrom);
            this.grbDateTime.Controls.Add(this.dtpEndDate);
            this.grbDateTime.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.grbDateTime.Location = new System.Drawing.Point(159, 3);
            this.grbDateTime.Name = "grbDateTime";
            this.grbDateTime.Size = new System.Drawing.Size(208, 108);
            this.grbDateTime.TabIndex = 8;
            this.grbDateTime.TabStop = false;
            this.grbDateTime.Text = "Duration";
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTo.Location = new System.Drawing.Point(7, 69);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(21, 15);
            this.lblTo.TabIndex = 8;
            this.lblTo.Text = "To";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.CustomFormat = "yyyy / MM / dd HH";
            this.dtpStartDate.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartDate.Location = new System.Drawing.Point(45, 23);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(159, 23);
            this.dtpStartDate.TabIndex = 5;
            this.dtpStartDate.Value = new System.DateTime(2014, 12, 2, 0, 0, 0, 0);
            this.dtpStartDate.ValueChanged += new System.EventHandler(this.dtpDateTime_ValueChanged);
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFrom.Location = new System.Drawing.Point(5, 26);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(37, 15);
            this.lblFrom.TabIndex = 7;
            this.lblFrom.Text = "From";
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.CustomFormat = "yyyy / MM / dd HH";
            this.dtpEndDate.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndDate.Location = new System.Drawing.Point(45, 65);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(159, 23);
            this.dtpEndDate.TabIndex = 6;
            this.dtpEndDate.Value = new System.DateTime(2014, 12, 9, 0, 0, 0, 0);
            this.dtpEndDate.ValueChanged += new System.EventHandler(this.dtpDateTime_ValueChanged);
            // 
            // grbLatest
            // 
            this.grbLatest.Controls.Add(this.rdoMonth);
            this.grbLatest.Controls.Add(this.rdoWeek);
            this.grbLatest.Controls.Add(this.rdoDay);
            this.grbLatest.Controls.Add(this.rdoHour);
            this.grbLatest.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbLatest.Location = new System.Drawing.Point(3, 3);
            this.grbLatest.Name = "grbLatest";
            this.grbLatest.Size = new System.Drawing.Size(150, 108);
            this.grbLatest.TabIndex = 7;
            this.grbLatest.TabStop = false;
            this.grbLatest.Text = "The Latest";
            // 
            // rdoMonth
            // 
            this.rdoMonth.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoMonth.Location = new System.Drawing.Point(75, 65);
            this.rdoMonth.Name = "rdoMonth";
            this.rdoMonth.Size = new System.Drawing.Size(80, 27);
            this.rdoMonth.TabIndex = 3;
            this.rdoMonth.TabStop = true;
            this.rdoMonth.Tag = "M";
            this.rdoMonth.Text = "1 Month";
            this.rdoMonth.UseVisualStyleBackColor = true;
            this.rdoMonth.CheckedChanged += new System.EventHandler(this.rdoLatest_CheckedChanged);
            // 
            // rdoWeek
            // 
            this.rdoWeek.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoWeek.Location = new System.Drawing.Point(5, 65);
            this.rdoWeek.Name = "rdoWeek";
            this.rdoWeek.Size = new System.Drawing.Size(72, 27);
            this.rdoWeek.TabIndex = 2;
            this.rdoWeek.TabStop = true;
            this.rdoWeek.Tag = "W";
            this.rdoWeek.Text = "1 Week";
            this.rdoWeek.UseVisualStyleBackColor = true;
            this.rdoWeek.CheckedChanged += new System.EventHandler(this.rdoLatest_CheckedChanged);
            // 
            // rdoDay
            // 
            this.rdoDay.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoDay.Location = new System.Drawing.Point(75, 26);
            this.rdoDay.Name = "rdoDay";
            this.rdoDay.Size = new System.Drawing.Size(70, 27);
            this.rdoDay.TabIndex = 1;
            this.rdoDay.TabStop = true;
            this.rdoDay.Tag = "D";
            this.rdoDay.Text = "1 Day";
            this.rdoDay.UseVisualStyleBackColor = true;
            this.rdoDay.CheckedChanged += new System.EventHandler(this.rdoLatest_CheckedChanged);
            // 
            // rdoHour
            // 
            this.rdoHour.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdoHour.Location = new System.Drawing.Point(5, 26);
            this.rdoHour.Name = "rdoHour";
            this.rdoHour.Size = new System.Drawing.Size(70, 27);
            this.rdoHour.TabIndex = 0;
            this.rdoHour.TabStop = true;
            this.rdoHour.Tag = "H";
            this.rdoHour.Text = "1 Hour";
            this.rdoHour.UseVisualStyleBackColor = true;
            this.rdoHour.CheckedChanged += new System.EventHandler(this.rdoLatest_CheckedChanged);
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnQuery);
            this.pnlButton.Controls.Add(this.btnExport);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(1028, 3);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(93, 116);
            this.pnlButton.TabIndex = 4;
            // 
            // btnQuery
            // 
            this.btnQuery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.Location = new System.Drawing.Point(3, 27);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(88, 39);
            this.btnQuery.TabIndex = 20;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // btnExport
            // 
            this.btnExport.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnExport.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExport.Location = new System.Drawing.Point(3, 73);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(88, 39);
            this.btnExport.TabIndex = 1;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // gbHistory
            // 
            this.gbHistory.BackColor = System.Drawing.Color.Transparent;
            this.gbHistory.Controls.Add(this.dgvData);
            this.gbHistory.Controls.Add(this.bnavRecord);
            this.gbHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbHistory.Font = new System.Drawing.Font("Georgia", 11.25F);
            this.gbHistory.ForeColor = System.Drawing.Color.Black;
            this.gbHistory.Location = new System.Drawing.Point(5, 128);
            this.gbHistory.Margin = new System.Windows.Forms.Padding(0);
            this.gbHistory.Name = "gbHistory";
            this.gbHistory.Padding = new System.Windows.Forms.Padding(0);
            this.gbHistory.Size = new System.Drawing.Size(1130, 435);
            this.gbHistory.TabIndex = 9;
            this.gbHistory.TabStop = false;
            this.gbHistory.Text = "Total Count: 0";
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Georgia", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(0, 18);
            this.dgvData.Name = "dgvData";
            this.dgvData.PageSize = 18;
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersWidth = 5;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvData.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1130, 392);
            this.dgvData.TabIndex = 12;
            this.dgvData.Tables = ((System.ComponentModel.BindingList<System.Data.DataTable>)(resources.GetObject("dgvData.Tables")));
            this.dgvData.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvData_CellContentClick);
            // 
            // bnavRecord
            // 
            this.bnavRecord.AddNewItem = null;
            this.bnavRecord.CountItem = this.bindingNavigatorCountItem;
            this.bnavRecord.DeleteItem = null;
            this.bnavRecord.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bnavRecord.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bnavRecord.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bindingNavigatorMoveFirstItem,
            this.bindingNavigatorMovePreviousItem,
            this.bindingNavigatorSeparator,
            this.bindingNavigatorPositionItem,
            this.bindingNavigatorCountItem,
            this.bindingNavigatorSeparator1,
            this.bindingNavigatorMoveNextItem,
            this.bindingNavigatorMoveLastItem,
            this.bindingNavigatorSeparator2});
            this.bnavRecord.Location = new System.Drawing.Point(0, 410);
            this.bnavRecord.MoveFirstItem = this.bindingNavigatorMoveFirstItem;
            this.bnavRecord.MoveLastItem = this.bindingNavigatorMoveLastItem;
            this.bnavRecord.MoveNextItem = this.bindingNavigatorMoveNextItem;
            this.bnavRecord.MovePreviousItem = this.bindingNavigatorMovePreviousItem;
            this.bnavRecord.Name = "bnavRecord";
            this.bnavRecord.PositionItem = this.bindingNavigatorPositionItem;
            this.bnavRecord.Size = new System.Drawing.Size(1130, 25);
            this.bnavRecord.Stretch = true;
            this.bnavRecord.TabIndex = 11;
            // 
            // bindingNavigatorCountItem
            // 
            this.bindingNavigatorCountItem.Name = "bindingNavigatorCountItem";
            this.bindingNavigatorCountItem.Size = new System.Drawing.Size(40, 22);
            this.bindingNavigatorCountItem.Text = "/ {0}";
            this.bindingNavigatorCountItem.ToolTipText = "項目總數";
            // 
            // bindingNavigatorMoveFirstItem
            // 
            this.bindingNavigatorMoveFirstItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveFirstItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveFirstItem.Image")));
            this.bindingNavigatorMoveFirstItem.Name = "bindingNavigatorMoveFirstItem";
            this.bindingNavigatorMoveFirstItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveFirstItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveFirstItem.Text = "移到最前面";
            // 
            // bindingNavigatorMovePreviousItem
            // 
            this.bindingNavigatorMovePreviousItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMovePreviousItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMovePreviousItem.Image")));
            this.bindingNavigatorMovePreviousItem.Name = "bindingNavigatorMovePreviousItem";
            this.bindingNavigatorMovePreviousItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMovePreviousItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMovePreviousItem.Text = "移到上一個";
            // 
            // bindingNavigatorSeparator
            // 
            this.bindingNavigatorSeparator.Name = "bindingNavigatorSeparator";
            this.bindingNavigatorSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorPositionItem
            // 
            this.bindingNavigatorPositionItem.AccessibleName = "位置";
            this.bindingNavigatorPositionItem.AutoSize = false;
            this.bindingNavigatorPositionItem.Name = "bindingNavigatorPositionItem";
            this.bindingNavigatorPositionItem.Size = new System.Drawing.Size(50, 23);
            this.bindingNavigatorPositionItem.Text = "0";
            this.bindingNavigatorPositionItem.ToolTipText = "目前的位置";
            // 
            // bindingNavigatorSeparator1
            // 
            this.bindingNavigatorSeparator1.Name = "bindingNavigatorSeparator1";
            this.bindingNavigatorSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // bindingNavigatorMoveNextItem
            // 
            this.bindingNavigatorMoveNextItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveNextItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveNextItem.Image")));
            this.bindingNavigatorMoveNextItem.Name = "bindingNavigatorMoveNextItem";
            this.bindingNavigatorMoveNextItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveNextItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveNextItem.Text = "移到下一個";
            // 
            // bindingNavigatorMoveLastItem
            // 
            this.bindingNavigatorMoveLastItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveLastItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveLastItem.Image")));
            this.bindingNavigatorMoveLastItem.Name = "bindingNavigatorMoveLastItem";
            this.bindingNavigatorMoveLastItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveLastItem.Size = new System.Drawing.Size(23, 22);
            this.bindingNavigatorMoveLastItem.Text = "移到最後面";
            // 
            // bindingNavigatorSeparator2
            // 
            this.bindingNavigatorSeparator2.Name = "bindingNavigatorSeparator2";
            this.bindingNavigatorSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // FormProcessDataHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 594);
            this.Name = "FormProcessDataHistory";
            this.Text = "FormProcessDataHistory";
            this.Load += new System.EventHandler(this.FormProcessDataHistory_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.tlpHeader.ResumeLayout(false);
            this.grbDateTime.ResumeLayout(false);
            this.grbDateTime.PerformLayout();
            this.grbLatest.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.gbHistory.ResumeLayout(false);
            this.gbHistory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bnavRecord)).EndInit();
            this.bnavRecord.ResumeLayout(false);
            this.bnavRecord.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.TableLayoutPanel tlpHeader;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Button btnExport;
        //private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.GroupBox gbHistory;
        private System.Windows.Forms.BindingNavigator bnavRecord;
        private System.Windows.Forms.ToolStripLabel bindingNavigatorCountItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveFirstItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMovePreviousItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator;
        private System.Windows.Forms.ToolStripTextBox bindingNavigatorPositionItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator1;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveNextItem;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveLastItem;
        private System.Windows.Forms.ToolStripSeparator bindingNavigatorSeparator2;
        private System.Windows.Forms.GroupBox grbLatest;
        private System.Windows.Forms.RadioButton rdoMonth;
        private System.Windows.Forms.RadioButton rdoWeek;
        private System.Windows.Forms.RadioButton rdoDay;
        private System.Windows.Forms.RadioButton rdoHour;
        private System.Windows.Forms.GroupBox grbDateTime;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private PagedGridView dgvData;
        private System.Windows.Forms.FlowLayoutPanel flpnlCondition;
    }
}