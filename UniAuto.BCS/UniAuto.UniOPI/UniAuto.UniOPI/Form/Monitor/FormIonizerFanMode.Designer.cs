namespace UniOPI
{
    partial class FormIonizerFanMode
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
            this.spcAlarm = new System.Windows.Forms.SplitContainer();
            this.pnlCombox = new System.Windows.Forms.Panel();
            this.cmbNode = new System.Windows.Forms.ComboBox();
            this.lblNode = new System.Windows.Forms.Label();
            this.btnQuery = new System.Windows.Forms.Button();
            this.gbxContent = new System.Windows.Forms.GroupBox();
            this.tlpFan = new System.Windows.Forms.TableLayoutPanel();
            this.lblFanNo01 = new System.Windows.Forms.Label();
            this.lblFanNo02 = new System.Windows.Forms.Label();
            this.lblFanNo03 = new System.Windows.Forms.Label();
            this.lblFanNo04 = new System.Windows.Forms.Label();
            this.lblFanNo05 = new System.Windows.Forms.Label();
            this.lblFanNo06 = new System.Windows.Forms.Label();
            this.lblFanNo07 = new System.Windows.Forms.Label();
            this.lblFanNo08 = new System.Windows.Forms.Label();
            this.lblFanNo09 = new System.Windows.Forms.Label();
            this.lblFanNo10 = new System.Windows.Forms.Label();
            this.lblFanNo11 = new System.Windows.Forms.Label();
            this.lblFanNo12 = new System.Windows.Forms.Label();
            this.lblFanNo13 = new System.Windows.Forms.Label();
            this.lblFanNo14 = new System.Windows.Forms.Label();
            this.lblFanNo15 = new System.Windows.Forms.Label();
            this.lblFanNo16 = new System.Windows.Forms.Label();
            this.lblFanNo17 = new System.Windows.Forms.Label();
            this.lblFanNo18 = new System.Windows.Forms.Label();
            this.lblFanNo19 = new System.Windows.Forms.Label();
            this.lblFanNo20 = new System.Windows.Forms.Label();
            this.lblFanNo21 = new System.Windows.Forms.Label();
            this.lblFanNo22 = new System.Windows.Forms.Label();
            this.lblFanNo23 = new System.Windows.Forms.Label();
            this.lblFanNo24 = new System.Windows.Forms.Label();
            this.lblFanNo25 = new System.Windows.Forms.Label();
            this.lblFanNo26 = new System.Windows.Forms.Label();
            this.lblFanNo27 = new System.Windows.Forms.Label();
            this.lblFanNo28 = new System.Windows.Forms.Label();
            this.lblFanNo29 = new System.Windows.Forms.Label();
            this.lblFanNo30 = new System.Windows.Forms.Label();
            this.lblFanNo31 = new System.Windows.Forms.Label();
            this.lblFanNo32 = new System.Windows.Forms.Label();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcAlarm)).BeginInit();
            this.spcAlarm.Panel1.SuspendLayout();
            this.spcAlarm.Panel2.SuspendLayout();
            this.spcAlarm.SuspendLayout();
            this.pnlCombox.SuspendLayout();
            this.gbxContent.SuspendLayout();
            this.tlpFan.SuspendLayout();
            this.SuspendLayout();
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.spcAlarm);
            // 
            // spcAlarm
            // 
            this.spcAlarm.BackColor = System.Drawing.Color.Transparent;
            this.spcAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcAlarm.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spcAlarm.Location = new System.Drawing.Point(0, 0);
            this.spcAlarm.Name = "spcAlarm";
            this.spcAlarm.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcAlarm.Panel1
            // 
            this.spcAlarm.Panel1.Controls.Add(this.pnlCombox);
            // 
            // spcAlarm.Panel2
            // 
            this.spcAlarm.Panel2.Controls.Add(this.gbxContent);
            this.spcAlarm.Size = new System.Drawing.Size(1264, 517);
            this.spcAlarm.SplitterDistance = 70;
            this.spcAlarm.TabIndex = 1;
            // 
            // pnlCombox
            // 
            this.pnlCombox.Controls.Add(this.cmbNode);
            this.pnlCombox.Controls.Add(this.lblNode);
            this.pnlCombox.Controls.Add(this.btnQuery);
            this.pnlCombox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCombox.Location = new System.Drawing.Point(0, 0);
            this.pnlCombox.Name = "pnlCombox";
            this.pnlCombox.Size = new System.Drawing.Size(1264, 70);
            this.pnlCombox.TabIndex = 26;
            // 
            // cmbNode
            // 
            this.cmbNode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbNode.FormattingEnabled = true;
            this.cmbNode.Location = new System.Drawing.Point(59, 22);
            this.cmbNode.Name = "cmbNode";
            this.cmbNode.Size = new System.Drawing.Size(240, 29);
            this.cmbNode.TabIndex = 4;
            this.cmbNode.SelectedIndexChanged += new System.EventHandler(this.cmbNode_SelectedIndexChanged);
            // 
            // lblNode
            // 
            this.lblNode.AutoSize = true;
            this.lblNode.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNode.Location = new System.Drawing.Point(6, 26);
            this.lblNode.Name = "lblNode";
            this.lblNode.Size = new System.Drawing.Size(45, 21);
            this.lblNode.TabIndex = 5;
            this.lblNode.Text = "Local";
            // 
            // btnQuery
            // 
            this.btnQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuery.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuery.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuery.Location = new System.Drawing.Point(1143, 22);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(110, 29);
            this.btnQuery.TabIndex = 9;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // gbxContent
            // 
            this.gbxContent.Controls.Add(this.tlpFan);
            this.gbxContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxContent.Location = new System.Drawing.Point(0, 0);
            this.gbxContent.Name = "gbxContent";
            this.gbxContent.Size = new System.Drawing.Size(1264, 443);
            this.gbxContent.TabIndex = 16;
            this.gbxContent.TabStop = false;
            // 
            // tlpFan
            // 
            this.tlpFan.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.InsetDouble;
            this.tlpFan.ColumnCount = 8;
            this.tlpFan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpFan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpFan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpFan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpFan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpFan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpFan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpFan.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tlpFan.Controls.Add(this.lblFanNo01, 0, 0);
            this.tlpFan.Controls.Add(this.lblFanNo02, 1, 0);
            this.tlpFan.Controls.Add(this.lblFanNo03, 2, 0);
            this.tlpFan.Controls.Add(this.lblFanNo04, 3, 0);
            this.tlpFan.Controls.Add(this.lblFanNo05, 4, 0);
            this.tlpFan.Controls.Add(this.lblFanNo06, 5, 0);
            this.tlpFan.Controls.Add(this.lblFanNo07, 6, 0);
            this.tlpFan.Controls.Add(this.lblFanNo08, 7, 0);
            this.tlpFan.Controls.Add(this.lblFanNo09, 0, 1);
            this.tlpFan.Controls.Add(this.lblFanNo10, 1, 1);
            this.tlpFan.Controls.Add(this.lblFanNo11, 2, 1);
            this.tlpFan.Controls.Add(this.lblFanNo12, 3, 1);
            this.tlpFan.Controls.Add(this.lblFanNo13, 4, 1);
            this.tlpFan.Controls.Add(this.lblFanNo14, 5, 1);
            this.tlpFan.Controls.Add(this.lblFanNo15, 6, 1);
            this.tlpFan.Controls.Add(this.lblFanNo16, 7, 1);
            this.tlpFan.Controls.Add(this.lblFanNo17, 0, 2);
            this.tlpFan.Controls.Add(this.lblFanNo18, 1, 2);
            this.tlpFan.Controls.Add(this.lblFanNo19, 2, 2);
            this.tlpFan.Controls.Add(this.lblFanNo20, 3, 2);
            this.tlpFan.Controls.Add(this.lblFanNo21, 4, 2);
            this.tlpFan.Controls.Add(this.lblFanNo22, 5, 2);
            this.tlpFan.Controls.Add(this.lblFanNo23, 6, 2);
            this.tlpFan.Controls.Add(this.lblFanNo24, 7, 2);
            this.tlpFan.Controls.Add(this.lblFanNo25, 0, 3);
            this.tlpFan.Controls.Add(this.lblFanNo26, 1, 3);
            this.tlpFan.Controls.Add(this.lblFanNo27, 2, 3);
            this.tlpFan.Controls.Add(this.lblFanNo28, 3, 3);
            this.tlpFan.Controls.Add(this.lblFanNo29, 4, 3);
            this.tlpFan.Controls.Add(this.lblFanNo30, 5, 3);
            this.tlpFan.Controls.Add(this.lblFanNo31, 6, 3);
            this.tlpFan.Controls.Add(this.lblFanNo32, 7, 3);
            this.tlpFan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpFan.Location = new System.Drawing.Point(3, 18);
            this.tlpFan.Name = "tlpFan";
            this.tlpFan.Padding = new System.Windows.Forms.Padding(2);
            this.tlpFan.RowCount = 4;
            this.tlpFan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpFan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpFan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpFan.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tlpFan.Size = new System.Drawing.Size(1258, 422);
            this.tlpFan.TabIndex = 0;
            // 
            // lblFanNo01
            // 
            this.lblFanNo01.AutoSize = true;
            this.lblFanNo01.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo01.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo01.Location = new System.Drawing.Point(8, 5);
            this.lblFanNo01.Name = "lblFanNo01";
            this.lblFanNo01.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo01.TabIndex = 0;
            this.lblFanNo01.Text = "Fan No#01";
            this.lblFanNo01.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo02
            // 
            this.lblFanNo02.AutoSize = true;
            this.lblFanNo02.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo02.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo02.Location = new System.Drawing.Point(164, 5);
            this.lblFanNo02.Name = "lblFanNo02";
            this.lblFanNo02.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo02.TabIndex = 1;
            this.lblFanNo02.Text = "Fan No#02";
            this.lblFanNo02.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo03
            // 
            this.lblFanNo03.AutoSize = true;
            this.lblFanNo03.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo03.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo03.Location = new System.Drawing.Point(320, 5);
            this.lblFanNo03.Name = "lblFanNo03";
            this.lblFanNo03.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo03.TabIndex = 2;
            this.lblFanNo03.Text = "Fan No#03";
            this.lblFanNo03.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo04
            // 
            this.lblFanNo04.AutoSize = true;
            this.lblFanNo04.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo04.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo04.Location = new System.Drawing.Point(476, 5);
            this.lblFanNo04.Name = "lblFanNo04";
            this.lblFanNo04.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo04.TabIndex = 3;
            this.lblFanNo04.Text = "Fan No#04";
            this.lblFanNo04.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo05
            // 
            this.lblFanNo05.AutoSize = true;
            this.lblFanNo05.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo05.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo05.Location = new System.Drawing.Point(632, 5);
            this.lblFanNo05.Name = "lblFanNo05";
            this.lblFanNo05.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo05.TabIndex = 4;
            this.lblFanNo05.Text = "Fan No#05";
            this.lblFanNo05.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo06
            // 
            this.lblFanNo06.AutoSize = true;
            this.lblFanNo06.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo06.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo06.Location = new System.Drawing.Point(788, 5);
            this.lblFanNo06.Name = "lblFanNo06";
            this.lblFanNo06.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo06.TabIndex = 5;
            this.lblFanNo06.Text = "Fan No#06";
            this.lblFanNo06.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo07
            // 
            this.lblFanNo07.AutoSize = true;
            this.lblFanNo07.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo07.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo07.Location = new System.Drawing.Point(944, 5);
            this.lblFanNo07.Name = "lblFanNo07";
            this.lblFanNo07.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo07.TabIndex = 6;
            this.lblFanNo07.Text = "Fan No#07";
            this.lblFanNo07.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo08
            // 
            this.lblFanNo08.AutoSize = true;
            this.lblFanNo08.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo08.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo08.Location = new System.Drawing.Point(1100, 5);
            this.lblFanNo08.Name = "lblFanNo08";
            this.lblFanNo08.Size = new System.Drawing.Size(150, 100);
            this.lblFanNo08.TabIndex = 7;
            this.lblFanNo08.Text = "Fan No#08";
            this.lblFanNo08.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo09
            // 
            this.lblFanNo09.AutoSize = true;
            this.lblFanNo09.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo09.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo09.Location = new System.Drawing.Point(8, 108);
            this.lblFanNo09.Name = "lblFanNo09";
            this.lblFanNo09.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo09.TabIndex = 8;
            this.lblFanNo09.Text = "Fan No#09";
            this.lblFanNo09.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo10
            // 
            this.lblFanNo10.AutoSize = true;
            this.lblFanNo10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo10.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo10.Location = new System.Drawing.Point(164, 108);
            this.lblFanNo10.Name = "lblFanNo10";
            this.lblFanNo10.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo10.TabIndex = 9;
            this.lblFanNo10.Text = "Fan No#10";
            this.lblFanNo10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo11
            // 
            this.lblFanNo11.AutoSize = true;
            this.lblFanNo11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo11.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo11.Location = new System.Drawing.Point(320, 108);
            this.lblFanNo11.Name = "lblFanNo11";
            this.lblFanNo11.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo11.TabIndex = 10;
            this.lblFanNo11.Text = "Fan No#11";
            this.lblFanNo11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo12
            // 
            this.lblFanNo12.AutoSize = true;
            this.lblFanNo12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo12.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo12.Location = new System.Drawing.Point(476, 108);
            this.lblFanNo12.Name = "lblFanNo12";
            this.lblFanNo12.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo12.TabIndex = 11;
            this.lblFanNo12.Text = "Fan No#12";
            this.lblFanNo12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo13
            // 
            this.lblFanNo13.AutoSize = true;
            this.lblFanNo13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo13.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo13.Location = new System.Drawing.Point(632, 108);
            this.lblFanNo13.Name = "lblFanNo13";
            this.lblFanNo13.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo13.TabIndex = 12;
            this.lblFanNo13.Text = "Fan No#13";
            this.lblFanNo13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo14
            // 
            this.lblFanNo14.AutoSize = true;
            this.lblFanNo14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo14.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo14.Location = new System.Drawing.Point(788, 108);
            this.lblFanNo14.Name = "lblFanNo14";
            this.lblFanNo14.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo14.TabIndex = 13;
            this.lblFanNo14.Text = "Fan No#14";
            this.lblFanNo14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo15
            // 
            this.lblFanNo15.AutoSize = true;
            this.lblFanNo15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo15.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo15.Location = new System.Drawing.Point(944, 108);
            this.lblFanNo15.Name = "lblFanNo15";
            this.lblFanNo15.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo15.TabIndex = 14;
            this.lblFanNo15.Text = "Fan No#15";
            this.lblFanNo15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo16
            // 
            this.lblFanNo16.AutoSize = true;
            this.lblFanNo16.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo16.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo16.Location = new System.Drawing.Point(1100, 108);
            this.lblFanNo16.Name = "lblFanNo16";
            this.lblFanNo16.Size = new System.Drawing.Size(150, 100);
            this.lblFanNo16.TabIndex = 15;
            this.lblFanNo16.Text = "Fan No#16";
            this.lblFanNo16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo17
            // 
            this.lblFanNo17.AutoSize = true;
            this.lblFanNo17.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo17.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo17.Location = new System.Drawing.Point(8, 211);
            this.lblFanNo17.Name = "lblFanNo17";
            this.lblFanNo17.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo17.TabIndex = 16;
            this.lblFanNo17.Text = "Fan No#17";
            this.lblFanNo17.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo18
            // 
            this.lblFanNo18.AutoSize = true;
            this.lblFanNo18.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo18.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo18.Location = new System.Drawing.Point(164, 211);
            this.lblFanNo18.Name = "lblFanNo18";
            this.lblFanNo18.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo18.TabIndex = 17;
            this.lblFanNo18.Text = "Fan No#18";
            this.lblFanNo18.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo19
            // 
            this.lblFanNo19.AutoSize = true;
            this.lblFanNo19.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo19.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo19.Location = new System.Drawing.Point(320, 211);
            this.lblFanNo19.Name = "lblFanNo19";
            this.lblFanNo19.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo19.TabIndex = 18;
            this.lblFanNo19.Text = "Fan No#19";
            this.lblFanNo19.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo20
            // 
            this.lblFanNo20.AutoSize = true;
            this.lblFanNo20.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo20.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo20.Location = new System.Drawing.Point(476, 211);
            this.lblFanNo20.Name = "lblFanNo20";
            this.lblFanNo20.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo20.TabIndex = 19;
            this.lblFanNo20.Text = "Fan No#20";
            this.lblFanNo20.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo21
            // 
            this.lblFanNo21.AutoSize = true;
            this.lblFanNo21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo21.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo21.Location = new System.Drawing.Point(632, 211);
            this.lblFanNo21.Name = "lblFanNo21";
            this.lblFanNo21.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo21.TabIndex = 20;
            this.lblFanNo21.Text = "Fan No#21";
            this.lblFanNo21.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo22
            // 
            this.lblFanNo22.AutoSize = true;
            this.lblFanNo22.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo22.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo22.Location = new System.Drawing.Point(788, 211);
            this.lblFanNo22.Name = "lblFanNo22";
            this.lblFanNo22.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo22.TabIndex = 21;
            this.lblFanNo22.Text = "Fan No#22";
            this.lblFanNo22.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo23
            // 
            this.lblFanNo23.AutoSize = true;
            this.lblFanNo23.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo23.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo23.Location = new System.Drawing.Point(944, 211);
            this.lblFanNo23.Name = "lblFanNo23";
            this.lblFanNo23.Size = new System.Drawing.Size(147, 100);
            this.lblFanNo23.TabIndex = 22;
            this.lblFanNo23.Text = "Fan No#23";
            this.lblFanNo23.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo24
            // 
            this.lblFanNo24.AutoSize = true;
            this.lblFanNo24.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo24.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo24.Location = new System.Drawing.Point(1100, 211);
            this.lblFanNo24.Name = "lblFanNo24";
            this.lblFanNo24.Size = new System.Drawing.Size(150, 100);
            this.lblFanNo24.TabIndex = 23;
            this.lblFanNo24.Text = "Fan No#24";
            this.lblFanNo24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo25
            // 
            this.lblFanNo25.AutoSize = true;
            this.lblFanNo25.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo25.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo25.Location = new System.Drawing.Point(8, 314);
            this.lblFanNo25.Name = "lblFanNo25";
            this.lblFanNo25.Size = new System.Drawing.Size(147, 103);
            this.lblFanNo25.TabIndex = 24;
            this.lblFanNo25.Text = "Fan No#25";
            this.lblFanNo25.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo26
            // 
            this.lblFanNo26.AutoSize = true;
            this.lblFanNo26.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo26.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo26.Location = new System.Drawing.Point(164, 314);
            this.lblFanNo26.Name = "lblFanNo26";
            this.lblFanNo26.Size = new System.Drawing.Size(147, 103);
            this.lblFanNo26.TabIndex = 25;
            this.lblFanNo26.Text = "Fan No#26";
            this.lblFanNo26.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo27
            // 
            this.lblFanNo27.AutoSize = true;
            this.lblFanNo27.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo27.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo27.Location = new System.Drawing.Point(320, 314);
            this.lblFanNo27.Name = "lblFanNo27";
            this.lblFanNo27.Size = new System.Drawing.Size(147, 103);
            this.lblFanNo27.TabIndex = 26;
            this.lblFanNo27.Text = "Fan No#27";
            this.lblFanNo27.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo28
            // 
            this.lblFanNo28.AutoSize = true;
            this.lblFanNo28.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo28.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo28.Location = new System.Drawing.Point(476, 314);
            this.lblFanNo28.Name = "lblFanNo28";
            this.lblFanNo28.Size = new System.Drawing.Size(147, 103);
            this.lblFanNo28.TabIndex = 27;
            this.lblFanNo28.Text = "Fan No#28";
            this.lblFanNo28.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo29
            // 
            this.lblFanNo29.AutoSize = true;
            this.lblFanNo29.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo29.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo29.Location = new System.Drawing.Point(632, 314);
            this.lblFanNo29.Name = "lblFanNo29";
            this.lblFanNo29.Size = new System.Drawing.Size(147, 103);
            this.lblFanNo29.TabIndex = 28;
            this.lblFanNo29.Text = "Fan No#29";
            this.lblFanNo29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo30
            // 
            this.lblFanNo30.AutoSize = true;
            this.lblFanNo30.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo30.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo30.Location = new System.Drawing.Point(788, 314);
            this.lblFanNo30.Name = "lblFanNo30";
            this.lblFanNo30.Size = new System.Drawing.Size(147, 103);
            this.lblFanNo30.TabIndex = 29;
            this.lblFanNo30.Text = "Fan No#30";
            this.lblFanNo30.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo31
            // 
            this.lblFanNo31.AutoSize = true;
            this.lblFanNo31.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo31.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo31.Location = new System.Drawing.Point(944, 314);
            this.lblFanNo31.Name = "lblFanNo31";
            this.lblFanNo31.Size = new System.Drawing.Size(147, 103);
            this.lblFanNo31.TabIndex = 30;
            this.lblFanNo31.Text = "Fan No#31";
            this.lblFanNo31.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanNo32
            // 
            this.lblFanNo32.AutoSize = true;
            this.lblFanNo32.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFanNo32.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanNo32.Location = new System.Drawing.Point(1100, 314);
            this.lblFanNo32.Name = "lblFanNo32";
            this.lblFanNo32.Size = new System.Drawing.Size(150, 103);
            this.lblFanNo32.TabIndex = 31;
            this.lblFanNo32.Text = "Fan No#32";
            this.lblFanNo32.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 1000;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // FormIonizerFanMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.ClientSize = new System.Drawing.Size(1264, 548);
            this.Name = "FormIonizerFanMode";
            this.Load += new System.EventHandler(this.FormIonizerFanMode_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.spcAlarm.Panel1.ResumeLayout(false);
            this.spcAlarm.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcAlarm)).EndInit();
            this.spcAlarm.ResumeLayout(false);
            this.pnlCombox.ResumeLayout(false);
            this.pnlCombox.PerformLayout();
            this.gbxContent.ResumeLayout(false);
            this.tlpFan.ResumeLayout(false);
            this.tlpFan.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer spcAlarm;
        private System.Windows.Forms.GroupBox gbxContent;
        private System.Windows.Forms.Panel pnlCombox;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Label lblNode;
        private System.Windows.Forms.ComboBox cmbNode;
        private System.Windows.Forms.TableLayoutPanel tlpFan;
        private System.Windows.Forms.Label lblFanNo01;
        private System.Windows.Forms.Label lblFanNo02;
        private System.Windows.Forms.Label lblFanNo03;
        private System.Windows.Forms.Label lblFanNo04;
        private System.Windows.Forms.Label lblFanNo05;
        private System.Windows.Forms.Label lblFanNo06;
        private System.Windows.Forms.Label lblFanNo07;
        private System.Windows.Forms.Label lblFanNo08;
        private System.Windows.Forms.Label lblFanNo09;
        private System.Windows.Forms.Label lblFanNo10;
        private System.Windows.Forms.Label lblFanNo11;
        private System.Windows.Forms.Label lblFanNo12;
        private System.Windows.Forms.Label lblFanNo13;
        private System.Windows.Forms.Label lblFanNo14;
        private System.Windows.Forms.Label lblFanNo15;
        private System.Windows.Forms.Label lblFanNo16;
        private System.Windows.Forms.Label lblFanNo17;
        private System.Windows.Forms.Label lblFanNo18;
        private System.Windows.Forms.Label lblFanNo19;
        private System.Windows.Forms.Label lblFanNo20;
        private System.Windows.Forms.Label lblFanNo21;
        private System.Windows.Forms.Label lblFanNo22;
        private System.Windows.Forms.Label lblFanNo23;
        private System.Windows.Forms.Label lblFanNo24;
        private System.Windows.Forms.Label lblFanNo25;
        private System.Windows.Forms.Label lblFanNo26;
        private System.Windows.Forms.Label lblFanNo27;
        private System.Windows.Forms.Label lblFanNo28;
        private System.Windows.Forms.Label lblFanNo29;
        private System.Windows.Forms.Label lblFanNo30;
        private System.Windows.Forms.Label lblFanNo31;
        private System.Windows.Forms.Label lblFanNo32;
        public System.Windows.Forms.Timer tmrRefresh;
    }
}