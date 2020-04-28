namespace UniOPI
{
    partial class FormRBRouteInfo
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
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.dgvcName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvcValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.grbCassette = new System.Windows.Forms.GroupBox();
            this.flpCassette = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlCstSeqNo = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCstSeqNo = new System.Windows.Forms.TextBox();
            this.pnlJobSeqNo = new System.Windows.Forms.Panel();
            this.lblJobSeqNo = new System.Windows.Forms.Label();
            this.txtJobSeqNo = new System.Windows.Forms.TextBox();
            this.pnlGlassID = new System.Windows.Forms.Panel();
            this.lblGlassID = new System.Windows.Forms.Label();
            this.txtGlassID = new System.Windows.Forms.TextBox();
            this.pnlRealStepID = new System.Windows.Forms.Panel();
            this.lblRealStepID = new System.Windows.Forms.Label();
            this.txtRealStepID = new System.Windows.Forms.TextBox();
            this.pnlRealNextStepID = new System.Windows.Forms.Panel();
            this.lblRealNextStepID = new System.Windows.Forms.Label();
            this.txtRealNextStepID = new System.Windows.Forms.TextBox();
            this.pnlRouteID = new System.Windows.Forms.Panel();
            this.lblRouteID = new System.Windows.Forms.Label();
            this.txtRouteID = new System.Windows.Forms.TextBox();
            this.pnlDescription = new System.Windows.Forms.Panel();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.panel2.SuspendLayout();
            this.grbCassette.SuspendLayout();
            this.flpCassette.SuspendLayout();
            this.pnlCstSeqNo.SuspendLayout();
            this.pnlJobSeqNo.SuspendLayout();
            this.pnlGlassID.SuspendLayout();
            this.pnlRealStepID.SuspendLayout();
            this.pnlRealNextStepID.SuspendLayout();
            this.pnlRouteID.SuspendLayout();
            this.pnlDescription.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(566, 30);
            this.lblCaption.Text = "Robot Route Step Information";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(626, 529);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.Controls.Add(this.dgvData, 0, 1);
            this.tlpBase.Controls.Add(this.panel2, 0, 2);
            this.tlpBase.Controls.Add(this.grbCassette, 0, 0);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 3;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 171F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tlpBase.Size = new System.Drawing.Size(626, 498);
            this.tlpBase.TabIndex = 1;
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
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvcName,
            this.dgvcValue});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(3, 174);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.RowTemplate.Height = 24;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.Size = new System.Drawing.Size(620, 282);
            this.dgvData.TabIndex = 15;
            // 
            // dgvcName
            // 
            this.dgvcName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dgvcName.HeaderText = "Item Name";
            this.dgvcName.Name = "dgvcName";
            this.dgvcName.ReadOnly = true;
            this.dgvcName.Width = 250;
            // 
            // dgvcValue
            // 
            this.dgvcValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvcValue.HeaderText = "Item Value";
            this.dgvcValue.Name = "dgvcValue";
            this.dgvcValue.ReadOnly = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnClose);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 462);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(620, 33);
            this.panel2.TabIndex = 21;
            // 
            // btnClose
            // 
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClose.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(250, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 12;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // grbCassette
            // 
            this.grbCassette.Controls.Add(this.flpCassette);
            this.grbCassette.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbCassette.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbCassette.Location = new System.Drawing.Point(3, 3);
            this.grbCassette.Name = "grbCassette";
            this.grbCassette.Size = new System.Drawing.Size(620, 165);
            this.grbCassette.TabIndex = 23;
            this.grbCassette.TabStop = false;
            // 
            // flpCassette
            // 
            this.flpCassette.Controls.Add(this.pnlCstSeqNo);
            this.flpCassette.Controls.Add(this.pnlJobSeqNo);
            this.flpCassette.Controls.Add(this.pnlGlassID);
            this.flpCassette.Controls.Add(this.pnlRealStepID);
            this.flpCassette.Controls.Add(this.pnlRealNextStepID);
            this.flpCassette.Controls.Add(this.pnlRouteID);
            this.flpCassette.Controls.Add(this.pnlDescription);
            this.flpCassette.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpCassette.Location = new System.Drawing.Point(3, 21);
            this.flpCassette.Name = "flpCassette";
            this.flpCassette.Size = new System.Drawing.Size(614, 141);
            this.flpCassette.TabIndex = 21;
            // 
            // pnlCstSeqNo
            // 
            this.pnlCstSeqNo.Controls.Add(this.label1);
            this.pnlCstSeqNo.Controls.Add(this.txtCstSeqNo);
            this.pnlCstSeqNo.Location = new System.Drawing.Point(3, 1);
            this.pnlCstSeqNo.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlCstSeqNo.Name = "pnlCstSeqNo";
            this.pnlCstSeqNo.Size = new System.Drawing.Size(300, 25);
            this.pnlCstSeqNo.TabIndex = 8;
            this.pnlCstSeqNo.Tag = "CASSETTESEQUENCENO";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.MediumBlue;
            this.label1.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 25);
            this.label1.TabIndex = 5;
            this.label1.Text = "Cassette Sequence No";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCstSeqNo
            // 
            this.txtCstSeqNo.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtCstSeqNo.Location = new System.Drawing.Point(145, 0);
            this.txtCstSeqNo.Name = "txtCstSeqNo";
            this.txtCstSeqNo.ReadOnly = true;
            this.txtCstSeqNo.Size = new System.Drawing.Size(150, 25);
            this.txtCstSeqNo.TabIndex = 0;
            // 
            // pnlJobSeqNo
            // 
            this.pnlJobSeqNo.Controls.Add(this.lblJobSeqNo);
            this.pnlJobSeqNo.Controls.Add(this.txtJobSeqNo);
            this.pnlJobSeqNo.Location = new System.Drawing.Point(309, 1);
            this.pnlJobSeqNo.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlJobSeqNo.Name = "pnlJobSeqNo";
            this.pnlJobSeqNo.Size = new System.Drawing.Size(300, 25);
            this.pnlJobSeqNo.TabIndex = 9;
            this.pnlJobSeqNo.Tag = "JOBSEQUENCENO";
            // 
            // lblJobSeqNo
            // 
            this.lblJobSeqNo.BackColor = System.Drawing.Color.MediumBlue;
            this.lblJobSeqNo.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblJobSeqNo.ForeColor = System.Drawing.Color.White;
            this.lblJobSeqNo.Location = new System.Drawing.Point(0, 0);
            this.lblJobSeqNo.Name = "lblJobSeqNo";
            this.lblJobSeqNo.Size = new System.Drawing.Size(145, 25);
            this.lblJobSeqNo.TabIndex = 5;
            this.lblJobSeqNo.Text = "Job Sequence No";
            this.lblJobSeqNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtJobSeqNo
            // 
            this.txtJobSeqNo.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtJobSeqNo.Location = new System.Drawing.Point(145, 0);
            this.txtJobSeqNo.Name = "txtJobSeqNo";
            this.txtJobSeqNo.ReadOnly = true;
            this.txtJobSeqNo.Size = new System.Drawing.Size(150, 25);
            this.txtJobSeqNo.TabIndex = 0;
            // 
            // pnlGlassID
            // 
            this.pnlGlassID.Controls.Add(this.lblGlassID);
            this.pnlGlassID.Controls.Add(this.txtGlassID);
            this.pnlGlassID.Location = new System.Drawing.Point(3, 28);
            this.pnlGlassID.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlGlassID.Name = "pnlGlassID";
            this.pnlGlassID.Size = new System.Drawing.Size(606, 25);
            this.pnlGlassID.TabIndex = 22;
            this.pnlGlassID.Tag = "PPID";
            // 
            // lblGlassID
            // 
            this.lblGlassID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblGlassID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGlassID.ForeColor = System.Drawing.Color.White;
            this.lblGlassID.Location = new System.Drawing.Point(0, 0);
            this.lblGlassID.Name = "lblGlassID";
            this.lblGlassID.Size = new System.Drawing.Size(145, 25);
            this.lblGlassID.TabIndex = 5;
            this.lblGlassID.Text = "Glass ID";
            this.lblGlassID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtGlassID
            // 
            this.txtGlassID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtGlassID.Location = new System.Drawing.Point(145, 0);
            this.txtGlassID.Name = "txtGlassID";
            this.txtGlassID.ReadOnly = true;
            this.txtGlassID.Size = new System.Drawing.Size(456, 25);
            this.txtGlassID.TabIndex = 0;
            // 
            // pnlRealStepID
            // 
            this.pnlRealStepID.Controls.Add(this.lblRealStepID);
            this.pnlRealStepID.Controls.Add(this.txtRealStepID);
            this.pnlRealStepID.Location = new System.Drawing.Point(3, 55);
            this.pnlRealStepID.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlRealStepID.Name = "pnlRealStepID";
            this.pnlRealStepID.Size = new System.Drawing.Size(300, 25);
            this.pnlRealStepID.TabIndex = 23;
            this.pnlRealStepID.Tag = "PPID";
            // 
            // lblRealStepID
            // 
            this.lblRealStepID.BackColor = System.Drawing.Color.MediumBlue;
            this.lblRealStepID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRealStepID.ForeColor = System.Drawing.Color.White;
            this.lblRealStepID.Location = new System.Drawing.Point(0, 0);
            this.lblRealStepID.Name = "lblRealStepID";
            this.lblRealStepID.Size = new System.Drawing.Size(145, 25);
            this.lblRealStepID.TabIndex = 5;
            this.lblRealStepID.Text = "Real Time Step ID";
            this.lblRealStepID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtRealStepID
            // 
            this.txtRealStepID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtRealStepID.Location = new System.Drawing.Point(145, 0);
            this.txtRealStepID.Name = "txtRealStepID";
            this.txtRealStepID.ReadOnly = true;
            this.txtRealStepID.Size = new System.Drawing.Size(150, 25);
            this.txtRealStepID.TabIndex = 0;
            // 
            // pnlRealNextStepID
            // 
            this.pnlRealNextStepID.Controls.Add(this.lblRealNextStepID);
            this.pnlRealNextStepID.Controls.Add(this.txtRealNextStepID);
            this.pnlRealNextStepID.Location = new System.Drawing.Point(309, 55);
            this.pnlRealNextStepID.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlRealNextStepID.Name = "pnlRealNextStepID";
            this.pnlRealNextStepID.Size = new System.Drawing.Size(300, 25);
            this.pnlRealNextStepID.TabIndex = 24;
            this.pnlRealNextStepID.Tag = "PPID";
            // 
            // lblRealNextStepID
            // 
            this.lblRealNextStepID.BackColor = System.Drawing.Color.MediumBlue;
            this.lblRealNextStepID.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRealNextStepID.ForeColor = System.Drawing.Color.White;
            this.lblRealNextStepID.Location = new System.Drawing.Point(0, 0);
            this.lblRealNextStepID.Name = "lblRealNextStepID";
            this.lblRealNextStepID.Size = new System.Drawing.Size(145, 25);
            this.lblRealNextStepID.TabIndex = 5;
            this.lblRealNextStepID.Text = "Real Time Next Step ID";
            this.lblRealNextStepID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtRealNextStepID
            // 
            this.txtRealNextStepID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtRealNextStepID.Location = new System.Drawing.Point(145, 0);
            this.txtRealNextStepID.Name = "txtRealNextStepID";
            this.txtRealNextStepID.ReadOnly = true;
            this.txtRealNextStepID.Size = new System.Drawing.Size(150, 25);
            this.txtRealNextStepID.TabIndex = 0;
            // 
            // pnlRouteID
            // 
            this.pnlRouteID.Controls.Add(this.lblRouteID);
            this.pnlRouteID.Controls.Add(this.txtRouteID);
            this.pnlRouteID.Location = new System.Drawing.Point(3, 82);
            this.pnlRouteID.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlRouteID.Name = "pnlRouteID";
            this.pnlRouteID.Size = new System.Drawing.Size(300, 25);
            this.pnlRouteID.TabIndex = 25;
            this.pnlRouteID.Tag = "PPID";
            // 
            // lblRouteID
            // 
            this.lblRouteID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblRouteID.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRouteID.ForeColor = System.Drawing.Color.White;
            this.lblRouteID.Location = new System.Drawing.Point(0, 0);
            this.lblRouteID.Name = "lblRouteID";
            this.lblRouteID.Size = new System.Drawing.Size(145, 25);
            this.lblRouteID.TabIndex = 5;
            this.lblRouteID.Text = "Route ID";
            this.lblRouteID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtRouteID
            // 
            this.txtRouteID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtRouteID.Location = new System.Drawing.Point(145, 0);
            this.txtRouteID.Name = "txtRouteID";
            this.txtRouteID.ReadOnly = true;
            this.txtRouteID.Size = new System.Drawing.Size(150, 25);
            this.txtRouteID.TabIndex = 0;
            // 
            // pnlDescription
            // 
            this.pnlDescription.Controls.Add(this.lblDescription);
            this.pnlDescription.Controls.Add(this.txtDescription);
            this.pnlDescription.Location = new System.Drawing.Point(3, 109);
            this.pnlDescription.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.pnlDescription.Name = "pnlDescription";
            this.pnlDescription.Size = new System.Drawing.Size(606, 25);
            this.pnlDescription.TabIndex = 26;
            this.pnlDescription.Tag = "PPID";
            // 
            // lblDescription
            // 
            this.lblDescription.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblDescription.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescription.ForeColor = System.Drawing.Color.White;
            this.lblDescription.Location = new System.Drawing.Point(0, 0);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(145, 25);
            this.lblDescription.TabIndex = 5;
            this.lblDescription.Text = "Description";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtDescription.Location = new System.Drawing.Point(145, 0);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(456, 25);
            this.txtDescription.TabIndex = 0;
            // 
            // FormRBRouteInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 529);
            this.Name = "FormRBRouteInfo";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormRBRouteInfo_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.panel2.ResumeLayout(false);
            this.grbCassette.ResumeLayout(false);
            this.flpCassette.ResumeLayout(false);
            this.pnlCstSeqNo.ResumeLayout(false);
            this.pnlCstSeqNo.PerformLayout();
            this.pnlJobSeqNo.ResumeLayout(false);
            this.pnlJobSeqNo.PerformLayout();
            this.pnlGlassID.ResumeLayout(false);
            this.pnlGlassID.PerformLayout();
            this.pnlRealStepID.ResumeLayout(false);
            this.pnlRealStepID.PerformLayout();
            this.pnlRealNextStepID.ResumeLayout(false);
            this.pnlRealNextStepID.PerformLayout();
            this.pnlRouteID.ResumeLayout(false);
            this.pnlRouteID.PerformLayout();
            this.pnlDescription.ResumeLayout(false);
            this.pnlDescription.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Panel pnlGlassID;
        private System.Windows.Forms.Label lblGlassID;
        private System.Windows.Forms.TextBox txtGlassID;
        private System.Windows.Forms.Panel pnlCstSeqNo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCstSeqNo;
        private System.Windows.Forms.Panel pnlJobSeqNo;
        private System.Windows.Forms.Label lblJobSeqNo;
        private System.Windows.Forms.TextBox txtJobSeqNo;
        private System.Windows.Forms.GroupBox grbCassette;
        private System.Windows.Forms.FlowLayoutPanel flpCassette;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvcName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvcValue;
        private System.Windows.Forms.Panel pnlRealStepID;
        private System.Windows.Forms.Label lblRealStepID;
        private System.Windows.Forms.TextBox txtRealStepID;
        private System.Windows.Forms.Panel pnlRealNextStepID;
        private System.Windows.Forms.Label lblRealNextStepID;
        private System.Windows.Forms.TextBox txtRealNextStepID;
        private System.Windows.Forms.Panel pnlRouteID;
        private System.Windows.Forms.Label lblRouteID;
        private System.Windows.Forms.TextBox txtRouteID;
        private System.Windows.Forms.Panel pnlDescription;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
    }
}