namespace UniOPI
{
    partial class FormProductTypeBlock
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
            this.gbxGridData = new System.Windows.Forms.GroupBox();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLocalID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProductType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnit01 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnit02 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnit03 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnit04 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnit05 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnit06 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnit07 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnit08 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpBottom = new System.Windows.Forms.TableLayoutPanel();
            this.btnRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            this.gbxGridData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.tlpBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(1200, 30);
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(1260, 522);
            // 
            // tmrBaseRefresh
            // 
            this.tmrBaseRefresh.Tick += new System.EventHandler(this.tmrBaseRefresh_Tick);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBase.Controls.Add(this.gbxGridData, 0, 0);
            this.tlpBase.Controls.Add(this.tlpBottom, 0, 1);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBase.Size = new System.Drawing.Size(1260, 491);
            this.tlpBase.TabIndex = 1;
            // 
            // gbxGridData
            // 
            this.gbxGridData.Controls.Add(this.dgvData);
            this.gbxGridData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxGridData.Location = new System.Drawing.Point(3, 3);
            this.gbxGridData.Name = "gbxGridData";
            this.gbxGridData.Size = new System.Drawing.Size(1254, 440);
            this.gbxGridData.TabIndex = 15;
            this.gbxGridData.TabStop = false;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvData.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLocalNo,
            this.colLocalID,
            this.colProductType,
            this.colUnit01,
            this.colUnit02,
            this.colUnit03,
            this.colUnit04,
            this.colUnit05,
            this.colUnit06,
            this.colUnit07,
            this.colUnit08});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 18);
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(1248, 419);
            this.dgvData.TabIndex = 11;
            // 
            // colLocalNo
            // 
            this.colLocalNo.HeaderText = "Local No";
            this.colLocalNo.Name = "colLocalNo";
            this.colLocalNo.ReadOnly = true;
            this.colLocalNo.Visible = false;
            this.colLocalNo.Width = 79;
            // 
            // colLocalID
            // 
            this.colLocalID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colLocalID.HeaderText = "Local Name";
            this.colLocalID.Name = "colLocalID";
            this.colLocalID.ReadOnly = true;
            // 
            // colProductType
            // 
            this.colProductType.HeaderText = "Product Type";
            this.colProductType.Name = "colProductType";
            this.colProductType.ReadOnly = true;
            this.colProductType.Width = 127;
            // 
            // colUnit01
            // 
            this.colUnit01.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUnit01.HeaderText = "Unit #01";
            this.colUnit01.Name = "colUnit01";
            this.colUnit01.ReadOnly = true;
            this.colUnit01.Width = 110;
            // 
            // colUnit02
            // 
            this.colUnit02.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUnit02.HeaderText = "Unit #02";
            this.colUnit02.Name = "colUnit02";
            this.colUnit02.ReadOnly = true;
            this.colUnit02.Width = 110;
            // 
            // colUnit03
            // 
            this.colUnit03.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUnit03.HeaderText = "Unit #03";
            this.colUnit03.Name = "colUnit03";
            this.colUnit03.ReadOnly = true;
            this.colUnit03.Width = 110;
            // 
            // colUnit04
            // 
            this.colUnit04.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUnit04.HeaderText = "Unit #04";
            this.colUnit04.Name = "colUnit04";
            this.colUnit04.ReadOnly = true;
            this.colUnit04.Width = 110;
            // 
            // colUnit05
            // 
            this.colUnit05.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUnit05.HeaderText = "Unit #05";
            this.colUnit05.Name = "colUnit05";
            this.colUnit05.ReadOnly = true;
            this.colUnit05.Width = 110;
            // 
            // colUnit06
            // 
            this.colUnit06.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUnit06.HeaderText = "Unit #06";
            this.colUnit06.Name = "colUnit06";
            this.colUnit06.ReadOnly = true;
            this.colUnit06.Width = 110;
            // 
            // colUnit07
            // 
            this.colUnit07.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUnit07.HeaderText = "Unit #07";
            this.colUnit07.Name = "colUnit07";
            this.colUnit07.ReadOnly = true;
            this.colUnit07.Width = 110;
            // 
            // colUnit08
            // 
            this.colUnit08.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUnit08.HeaderText = "Unit #08";
            this.colUnit08.Name = "colUnit08";
            this.colUnit08.ReadOnly = true;
            this.colUnit08.Width = 110;
            // 
            // tlpBottom
            // 
            this.tlpBottom.ColumnCount = 3;
            this.tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpBottom.Controls.Add(this.btnRefresh, 1, 0);
            this.tlpBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tlpBottom.Location = new System.Drawing.Point(3, 449);
            this.tlpBottom.Name = "tlpBottom";
            this.tlpBottom.RowCount = 1;
            this.tlpBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBottom.Size = new System.Drawing.Size(1254, 39);
            this.tlpBottom.TabIndex = 14;
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnRefresh.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(1136, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(95, 33);
            this.btnRefresh.TabIndex = 25;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // FormProductTypeBlock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1260, 522);
            this.Name = "FormProductTypeBlock";
            this.Text = "FormProductTypeBlock";
            this.Load += new System.EventHandler(this.FormProductTypeBlock_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            this.gbxGridData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.tlpBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.GroupBox gbxGridData;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.TableLayoutPanel tlpBottom;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnit01;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnit02;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnit03;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnit04;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnit05;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnit06;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnit07;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnit08;
    }
}