namespace UniOPI
{
    partial class FormIncompleteBoxDataEdit
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
            this.flpData = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlPosition = new System.Windows.Forms.Panel();
            this.lblPosition = new System.Windows.Forms.Label();
            this.txtPosition = new System.Windows.Forms.TextBox();
            this.pnlProductName = new System.Windows.Forms.Panel();
            this.lblProductName = new System.Windows.Forms.Label();
            this.txtProductName = new System.Windows.Forms.TextBox();
            this.pnlHostProductName = new System.Windows.Forms.Panel();
            this.lblHostProductName = new System.Windows.Forms.Label();
            this.txtHostProductName = new System.Windows.Forms.TextBox();
            this.pnlShortCutFlag = new System.Windows.Forms.Panel();
            this.lblShortCutFlag = new System.Windows.Forms.Label();
            this.txtShortCutFlag = new System.Windows.Forms.TextBox();
            this.pnlBoxULDFlag = new System.Windows.Forms.Panel();
            this.lblBoxULDFlag = new System.Windows.Forms.Label();
            this.txtBoxULDFlag = new System.Windows.Forms.TextBox();
            this.pnlDPIProcessFlag = new System.Windows.Forms.Panel();
            this.lblDPIProcessFlag = new System.Windows.Forms.Label();
            this.txtDPIProcessFlag = new System.Windows.Forms.TextBox();
            this.pnlRTPFlag = new System.Windows.Forms.Panel();
            this.lblRTPFlag = new System.Windows.Forms.Label();
            this.txtRTPFlag = new System.Windows.Forms.TextBox();
            this.pnlPPID = new System.Windows.Forms.Panel();
            this.lblPPID = new System.Windows.Forms.Label();
            this.txtPPID = new System.Windows.Forms.TextBox();
            this.pnlHostPPID = new System.Windows.Forms.Panel();
            this.lblHostPPID = new System.Windows.Forms.Label();
            this.txtHostPPID = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.dgvDenseData = new System.Windows.Forms.DataGridView();
            this.colAbnormalSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAbnormalCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.flpData.SuspendLayout();
            this.pnlPosition.SuspendLayout();
            this.pnlProductName.SuspendLayout();
            this.pnlHostProductName.SuspendLayout();
            this.pnlShortCutFlag.SuspendLayout();
            this.pnlBoxULDFlag.SuspendLayout();
            this.pnlDPIProcessFlag.SuspendLayout();
            this.pnlRTPFlag.SuspendLayout();
            this.pnlPPID.SuspendLayout();
            this.pnlHostPPID.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDenseData)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(650, 30);
            this.lblCaption.Text = "Dense Product Data";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.btnDelete);
            this.spcBase.Panel2.Controls.Add(this.btnAdd);
            this.spcBase.Panel2.Controls.Add(this.dgvDenseData);
            this.spcBase.Panel2.Controls.Add(this.btnCancel);
            this.spcBase.Panel2.Controls.Add(this.btnOK);
            this.spcBase.Panel2.Controls.Add(this.flpData);
            this.spcBase.Size = new System.Drawing.Size(710, 501);
            // 
            // flpData
            // 
            this.flpData.Controls.Add(this.pnlPosition);
            this.flpData.Controls.Add(this.pnlProductName);
            this.flpData.Controls.Add(this.pnlHostProductName);
            this.flpData.Controls.Add(this.pnlShortCutFlag);
            this.flpData.Controls.Add(this.pnlBoxULDFlag);
            this.flpData.Controls.Add(this.pnlDPIProcessFlag);
            this.flpData.Controls.Add(this.pnlRTPFlag);
            this.flpData.Controls.Add(this.pnlPPID);
            this.flpData.Controls.Add(this.pnlHostPPID);
            this.flpData.Location = new System.Drawing.Point(12, 10);
            this.flpData.Name = "flpData";
            this.flpData.Size = new System.Drawing.Size(687, 182);
            this.flpData.TabIndex = 20;
            // 
            // pnlPosition
            // 
            this.pnlPosition.Controls.Add(this.lblPosition);
            this.pnlPosition.Controls.Add(this.txtPosition);
            this.pnlPosition.Location = new System.Drawing.Point(3, 3);
            this.pnlPosition.Name = "pnlPosition";
            this.pnlPosition.Size = new System.Drawing.Size(676, 23);
            this.pnlPosition.TabIndex = 18;
            this.pnlPosition.Tag = "PORTID";
            // 
            // lblPosition
            // 
            this.lblPosition.BackColor = System.Drawing.Color.Black;
            this.lblPosition.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPosition.ForeColor = System.Drawing.Color.White;
            this.lblPosition.Location = new System.Drawing.Point(1, 1);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(130, 23);
            this.lblPosition.TabIndex = 5;
            this.lblPosition.Text = "Position";
            this.lblPosition.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPosition
            // 
            this.txtPosition.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPosition.Location = new System.Drawing.Point(130, 1);
            this.txtPosition.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtPosition.Name = "txtPosition";
            this.txtPosition.Size = new System.Drawing.Size(200, 23);
            this.txtPosition.TabIndex = 0;
            this.txtPosition.Tag = "";
            this.txtPosition.Text = "01";
            // 
            // pnlProductName
            // 
            this.pnlProductName.Controls.Add(this.lblProductName);
            this.pnlProductName.Controls.Add(this.txtProductName);
            this.pnlProductName.Location = new System.Drawing.Point(3, 32);
            this.pnlProductName.Name = "pnlProductName";
            this.pnlProductName.Size = new System.Drawing.Size(335, 23);
            this.pnlProductName.TabIndex = 8;
            this.pnlProductName.Tag = "INCOMPLETEDATE";
            // 
            // lblProductName
            // 
            this.lblProductName.BackColor = System.Drawing.Color.Black;
            this.lblProductName.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblProductName.ForeColor = System.Drawing.Color.White;
            this.lblProductName.Location = new System.Drawing.Point(1, 1);
            this.lblProductName.Name = "lblProductName";
            this.lblProductName.Size = new System.Drawing.Size(130, 23);
            this.lblProductName.TabIndex = 5;
            this.lblProductName.Text = "Product Name";
            this.lblProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtProductName
            // 
            this.txtProductName.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtProductName.Location = new System.Drawing.Point(130, 1);
            this.txtProductName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtProductName.Name = "txtProductName";
            this.txtProductName.Size = new System.Drawing.Size(200, 23);
            this.txtProductName.TabIndex = 0;
            this.txtProductName.Tag = "";
            // 
            // pnlHostProductName
            // 
            this.pnlHostProductName.Controls.Add(this.lblHostProductName);
            this.pnlHostProductName.Controls.Add(this.txtHostProductName);
            this.pnlHostProductName.Location = new System.Drawing.Point(344, 32);
            this.pnlHostProductName.Name = "pnlHostProductName";
            this.pnlHostProductName.Size = new System.Drawing.Size(335, 23);
            this.pnlHostProductName.TabIndex = 26;
            this.pnlHostProductName.Tag = "CARRIERNAME";
            // 
            // lblHostProductName
            // 
            this.lblHostProductName.BackColor = System.Drawing.Color.Black;
            this.lblHostProductName.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHostProductName.ForeColor = System.Drawing.Color.White;
            this.lblHostProductName.Location = new System.Drawing.Point(1, 1);
            this.lblHostProductName.Name = "lblHostProductName";
            this.lblHostProductName.Size = new System.Drawing.Size(130, 23);
            this.lblHostProductName.TabIndex = 5;
            this.lblHostProductName.Text = "Host Product Name";
            this.lblHostProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtHostProductName
            // 
            this.txtHostProductName.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHostProductName.Location = new System.Drawing.Point(130, 1);
            this.txtHostProductName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtHostProductName.Name = "txtHostProductName";
            this.txtHostProductName.Size = new System.Drawing.Size(200, 23);
            this.txtHostProductName.TabIndex = 0;
            this.txtHostProductName.Tag = "";
            this.txtHostProductName.Text = "TBA176";
            // 
            // pnlShortCutFlag
            // 
            this.pnlShortCutFlag.Controls.Add(this.lblShortCutFlag);
            this.pnlShortCutFlag.Controls.Add(this.txtShortCutFlag);
            this.pnlShortCutFlag.Location = new System.Drawing.Point(3, 61);
            this.pnlShortCutFlag.Name = "pnlShortCutFlag";
            this.pnlShortCutFlag.Size = new System.Drawing.Size(335, 23);
            this.pnlShortCutFlag.TabIndex = 29;
            this.pnlShortCutFlag.Tag = "CARRIERNAME";
            // 
            // lblShortCutFlag
            // 
            this.lblShortCutFlag.BackColor = System.Drawing.Color.Black;
            this.lblShortCutFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShortCutFlag.ForeColor = System.Drawing.Color.White;
            this.lblShortCutFlag.Location = new System.Drawing.Point(1, 1);
            this.lblShortCutFlag.Name = "lblShortCutFlag";
            this.lblShortCutFlag.Size = new System.Drawing.Size(130, 23);
            this.lblShortCutFlag.TabIndex = 5;
            this.lblShortCutFlag.Text = "Short Cut Flag";
            this.lblShortCutFlag.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtShortCutFlag
            // 
            this.txtShortCutFlag.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtShortCutFlag.Location = new System.Drawing.Point(130, 1);
            this.txtShortCutFlag.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtShortCutFlag.Name = "txtShortCutFlag";
            this.txtShortCutFlag.Size = new System.Drawing.Size(200, 23);
            this.txtShortCutFlag.TabIndex = 0;
            this.txtShortCutFlag.Tag = "";
            this.txtShortCutFlag.Text = "TBA176";
            // 
            // pnlBoxULDFlag
            // 
            this.pnlBoxULDFlag.Controls.Add(this.lblBoxULDFlag);
            this.pnlBoxULDFlag.Controls.Add(this.txtBoxULDFlag);
            this.pnlBoxULDFlag.Location = new System.Drawing.Point(344, 61);
            this.pnlBoxULDFlag.Name = "pnlBoxULDFlag";
            this.pnlBoxULDFlag.Size = new System.Drawing.Size(335, 23);
            this.pnlBoxULDFlag.TabIndex = 30;
            this.pnlBoxULDFlag.Tag = "CARRIERNAME";
            // 
            // lblBoxULDFlag
            // 
            this.lblBoxULDFlag.BackColor = System.Drawing.Color.Black;
            this.lblBoxULDFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBoxULDFlag.ForeColor = System.Drawing.Color.White;
            this.lblBoxULDFlag.Location = new System.Drawing.Point(1, 1);
            this.lblBoxULDFlag.Name = "lblBoxULDFlag";
            this.lblBoxULDFlag.Size = new System.Drawing.Size(130, 23);
            this.lblBoxULDFlag.TabIndex = 5;
            this.lblBoxULDFlag.Text = "Box ULD Flag";
            this.lblBoxULDFlag.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBoxULDFlag
            // 
            this.txtBoxULDFlag.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBoxULDFlag.Location = new System.Drawing.Point(130, 1);
            this.txtBoxULDFlag.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtBoxULDFlag.Name = "txtBoxULDFlag";
            this.txtBoxULDFlag.Size = new System.Drawing.Size(200, 23);
            this.txtBoxULDFlag.TabIndex = 0;
            this.txtBoxULDFlag.Tag = "";
            this.txtBoxULDFlag.Text = "TBA176";
            // 
            // pnlDPIProcessFlag
            // 
            this.pnlDPIProcessFlag.Controls.Add(this.lblDPIProcessFlag);
            this.pnlDPIProcessFlag.Controls.Add(this.txtDPIProcessFlag);
            this.pnlDPIProcessFlag.Location = new System.Drawing.Point(3, 90);
            this.pnlDPIProcessFlag.Name = "pnlDPIProcessFlag";
            this.pnlDPIProcessFlag.Size = new System.Drawing.Size(335, 23);
            this.pnlDPIProcessFlag.TabIndex = 31;
            this.pnlDPIProcessFlag.Tag = "CARRIERNAME";
            // 
            // lblDPIProcessFlag
            // 
            this.lblDPIProcessFlag.BackColor = System.Drawing.Color.Black;
            this.lblDPIProcessFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDPIProcessFlag.ForeColor = System.Drawing.Color.White;
            this.lblDPIProcessFlag.Location = new System.Drawing.Point(1, 1);
            this.lblDPIProcessFlag.Name = "lblDPIProcessFlag";
            this.lblDPIProcessFlag.Size = new System.Drawing.Size(130, 23);
            this.lblDPIProcessFlag.TabIndex = 5;
            this.lblDPIProcessFlag.Text = "DPI Process Flag";
            this.lblDPIProcessFlag.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtDPIProcessFlag
            // 
            this.txtDPIProcessFlag.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDPIProcessFlag.Location = new System.Drawing.Point(130, 1);
            this.txtDPIProcessFlag.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtDPIProcessFlag.Name = "txtDPIProcessFlag";
            this.txtDPIProcessFlag.Size = new System.Drawing.Size(200, 23);
            this.txtDPIProcessFlag.TabIndex = 0;
            this.txtDPIProcessFlag.Tag = "";
            this.txtDPIProcessFlag.Text = "TBA176";
            // 
            // pnlRTPFlag
            // 
            this.pnlRTPFlag.Controls.Add(this.lblRTPFlag);
            this.pnlRTPFlag.Controls.Add(this.txtRTPFlag);
            this.pnlRTPFlag.Location = new System.Drawing.Point(344, 90);
            this.pnlRTPFlag.Name = "pnlRTPFlag";
            this.pnlRTPFlag.Size = new System.Drawing.Size(335, 23);
            this.pnlRTPFlag.TabIndex = 27;
            this.pnlRTPFlag.Tag = "CARRIERNAME";
            // 
            // lblRTPFlag
            // 
            this.lblRTPFlag.BackColor = System.Drawing.Color.Black;
            this.lblRTPFlag.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRTPFlag.ForeColor = System.Drawing.Color.White;
            this.lblRTPFlag.Location = new System.Drawing.Point(1, 1);
            this.lblRTPFlag.Name = "lblRTPFlag";
            this.lblRTPFlag.Size = new System.Drawing.Size(130, 23);
            this.lblRTPFlag.TabIndex = 5;
            this.lblRTPFlag.Text = "RTP Flag";
            this.lblRTPFlag.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtRTPFlag
            // 
            this.txtRTPFlag.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRTPFlag.Location = new System.Drawing.Point(130, 1);
            this.txtRTPFlag.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtRTPFlag.Name = "txtRTPFlag";
            this.txtRTPFlag.Size = new System.Drawing.Size(200, 23);
            this.txtRTPFlag.TabIndex = 0;
            this.txtRTPFlag.Tag = "";
            this.txtRTPFlag.Text = "TBA176";
            // 
            // pnlPPID
            // 
            this.pnlPPID.Controls.Add(this.lblPPID);
            this.pnlPPID.Controls.Add(this.txtPPID);
            this.pnlPPID.Location = new System.Drawing.Point(3, 119);
            this.pnlPPID.Name = "pnlPPID";
            this.pnlPPID.Size = new System.Drawing.Size(676, 23);
            this.pnlPPID.TabIndex = 28;
            this.pnlPPID.Tag = "CARRIERNAME";
            // 
            // lblPPID
            // 
            this.lblPPID.BackColor = System.Drawing.Color.Black;
            this.lblPPID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPPID.ForeColor = System.Drawing.Color.White;
            this.lblPPID.Location = new System.Drawing.Point(1, 1);
            this.lblPPID.Name = "lblPPID";
            this.lblPPID.Size = new System.Drawing.Size(130, 23);
            this.lblPPID.TabIndex = 5;
            this.lblPPID.Text = "PPID";
            this.lblPPID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPPID
            // 
            this.txtPPID.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPPID.Location = new System.Drawing.Point(130, 1);
            this.txtPPID.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtPPID.Name = "txtPPID";
            this.txtPPID.Size = new System.Drawing.Size(541, 23);
            this.txtPPID.TabIndex = 0;
            this.txtPPID.Tag = "";
            // 
            // pnlHostPPID
            // 
            this.pnlHostPPID.Controls.Add(this.lblHostPPID);
            this.pnlHostPPID.Controls.Add(this.txtHostPPID);
            this.pnlHostPPID.Location = new System.Drawing.Point(3, 148);
            this.pnlHostPPID.Name = "pnlHostPPID";
            this.pnlHostPPID.Size = new System.Drawing.Size(676, 23);
            this.pnlHostPPID.TabIndex = 27;
            this.pnlHostPPID.Tag = "CARRIERNAME";
            // 
            // lblHostPPID
            // 
            this.lblHostPPID.BackColor = System.Drawing.Color.Black;
            this.lblHostPPID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHostPPID.ForeColor = System.Drawing.Color.White;
            this.lblHostPPID.Location = new System.Drawing.Point(1, 1);
            this.lblHostPPID.Name = "lblHostPPID";
            this.lblHostPPID.Size = new System.Drawing.Size(130, 23);
            this.lblHostPPID.TabIndex = 5;
            this.lblHostPPID.Text = "Host PPID";
            this.lblHostPPID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtHostPPID
            // 
            this.txtHostPPID.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHostPPID.Location = new System.Drawing.Point(130, 1);
            this.txtHostPPID.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.txtHostPPID.Name = "txtHostPPID";
            this.txtHostPPID.Size = new System.Drawing.Size(541, 23);
            this.txtHostPPID.TabIndex = 0;
            this.txtHostPPID.Tag = "";
            // 
            // btnCancel
            // 
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnCancel.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnCancel.Location = new System.Drawing.Point(333, 425);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(76, 35);
            this.btnCancel.TabIndex = 22;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnOK.Location = new System.Drawing.Point(257, 425);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(76, 35);
            this.btnOK.TabIndex = 21;
            this.btnOK.Text = "OK";
            this.btnOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // dgvDenseData
            // 
            this.dgvDenseData.AllowUserToAddRows = false;
            this.dgvDenseData.AllowUserToDeleteRows = false;
            this.dgvDenseData.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvDenseData.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDenseData.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDenseData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDenseData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDenseData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAbnormalSeq,
            this.colAbnormalCode});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDenseData.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvDenseData.Location = new System.Drawing.Point(12, 198);
            this.dgvDenseData.Name = "dgvDenseData";
            this.dgvDenseData.RowHeadersVisible = false;
            this.dgvDenseData.RowTemplate.Height = 24;
            this.dgvDenseData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvDenseData.Size = new System.Drawing.Size(605, 219);
            this.dgvDenseData.TabIndex = 29;
            // 
            // colAbnormalSeq
            // 
            this.colAbnormalSeq.HeaderText = "Abnormal Seq";
            this.colAbnormalSeq.Name = "colAbnormalSeq";
            this.colAbnormalSeq.Width = 300;
            // 
            // colAbnormalCode
            // 
            this.colAbnormalCode.HeaderText = "Abnormal Code";
            this.colAbnormalCode.Name = "colAbnormalCode";
            this.colAbnormalCode.Width = 300;
            // 
            // btnAdd
            // 
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAdd.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAdd.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnAdd.Location = new System.Drawing.Point(621, 198);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(76, 35);
            this.btnAdd.TabIndex = 30;
            this.btnAdd.Tag = "ADD";
            this.btnAdd.Text = "Add";
            this.btnAdd.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnDelete.Location = new System.Drawing.Point(621, 234);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(76, 35);
            this.btnDelete.TabIndex = 32;
            this.btnDelete.Tag = "DEL";
            this.btnDelete.Text = "Delete";
            this.btnDelete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // FormIncompleteBoxDataEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(710, 501);
            this.Name = "FormIncompleteBoxDataEdit";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormIncompleteBoxDataEdit_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.flpData.ResumeLayout(false);
            this.pnlPosition.ResumeLayout(false);
            this.pnlPosition.PerformLayout();
            this.pnlProductName.ResumeLayout(false);
            this.pnlProductName.PerformLayout();
            this.pnlHostProductName.ResumeLayout(false);
            this.pnlHostProductName.PerformLayout();
            this.pnlShortCutFlag.ResumeLayout(false);
            this.pnlShortCutFlag.PerformLayout();
            this.pnlBoxULDFlag.ResumeLayout(false);
            this.pnlBoxULDFlag.PerformLayout();
            this.pnlDPIProcessFlag.ResumeLayout(false);
            this.pnlDPIProcessFlag.PerformLayout();
            this.pnlRTPFlag.ResumeLayout(false);
            this.pnlRTPFlag.PerformLayout();
            this.pnlPPID.ResumeLayout(false);
            this.pnlPPID.PerformLayout();
            this.pnlHostPPID.ResumeLayout(false);
            this.pnlHostPPID.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDenseData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpData;
        private System.Windows.Forms.Panel pnlPosition;
        private System.Windows.Forms.Label lblPosition;
        private System.Windows.Forms.TextBox txtPosition;
        private System.Windows.Forms.Panel pnlProductName;
        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.TextBox txtProductName;
        private System.Windows.Forms.Panel pnlHostProductName;
        private System.Windows.Forms.Label lblHostProductName;
        private System.Windows.Forms.TextBox txtHostProductName;
        private System.Windows.Forms.Panel pnlShortCutFlag;
        private System.Windows.Forms.Label lblShortCutFlag;
        private System.Windows.Forms.TextBox txtShortCutFlag;
        private System.Windows.Forms.Panel pnlBoxULDFlag;
        private System.Windows.Forms.Label lblBoxULDFlag;
        private System.Windows.Forms.TextBox txtBoxULDFlag;
        private System.Windows.Forms.Panel pnlDPIProcessFlag;
        private System.Windows.Forms.Label lblDPIProcessFlag;
        private System.Windows.Forms.TextBox txtDPIProcessFlag;
        private System.Windows.Forms.Panel pnlRTPFlag;
        private System.Windows.Forms.Label lblRTPFlag;
        private System.Windows.Forms.TextBox txtRTPFlag;
        private System.Windows.Forms.Panel pnlPPID;
        private System.Windows.Forms.Label lblPPID;
        private System.Windows.Forms.TextBox txtPPID;
        private System.Windows.Forms.Panel pnlHostPPID;
        private System.Windows.Forms.Label lblHostPPID;
        private System.Windows.Forms.TextBox txtHostPPID;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.DataGridView dgvDenseData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAbnormalSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAbnormalCode;
    }
}