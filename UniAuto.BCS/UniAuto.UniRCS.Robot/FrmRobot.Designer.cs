namespace UniAuto.UniRCS.RobotService
{
    partial class FrmRobot
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
            this.panel8 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel10 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._lstBcsDLLInfo = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._lstFolderDLLInfo = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel14 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this._btnRead = new System.Windows.Forms.Button();
            this.panel20 = new System.Windows.Forms.Panel();
            this._btnConfirm = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel14.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel8
            // 
            this.panel8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(109)))), ((int)(((byte)(158)))));
            this.panel8.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel8.Location = new System.Drawing.Point(0, 642);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(1024, 12);
            this.panel8.TabIndex = 12;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(109)))), ((int)(((byte)(158)))));
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(13, 642);
            this.panel3.TabIndex = 13;
            // 
            // panel10
            // 
            this.panel10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(109)))), ((int)(((byte)(158)))));
            this.panel10.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel10.Location = new System.Drawing.Point(1011, 0);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(13, 642);
            this.panel10.TabIndex = 14;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(13, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Size = new System.Drawing.Size(998, 642);
            this.splitContainer1.SplitterDistance = 489;
            this.splitContainer1.TabIndex = 19;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._lstBcsDLLInfo);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(489, 642);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "BCS Robot DLL Information";
            // 
            // _lstBcsDLLInfo
            // 
            this._lstBcsDLLInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            this._lstBcsDLLInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lstBcsDLLInfo.FullRowSelect = true;
            this._lstBcsDLLInfo.GridLines = true;
            this._lstBcsDLLInfo.Location = new System.Drawing.Point(3, 18);
            this._lstBcsDLLInfo.Name = "_lstBcsDLLInfo";
            this._lstBcsDLLInfo.Size = new System.Drawing.Size(483, 621);
            this._lstBcsDLLInfo.TabIndex = 15;
            this._lstBcsDLLInfo.UseCompatibleStateImageBehavior = false;
            this._lstBcsDLLInfo.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Item";
            this.columnHeader3.Width = 175;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Value";
            this.columnHeader4.Width = 210;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._lstFolderDLLInfo);
            this.groupBox2.Controls.Add(this.panel2);
            this.groupBox2.Controls.Add(this.panel14);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(505, 642);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Robot DLL File Information in Folder";
            // 
            // _lstFolderDLLInfo
            // 
            this._lstFolderDLLInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this._lstFolderDLLInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lstFolderDLLInfo.FullRowSelect = true;
            this._lstFolderDLLInfo.GridLines = true;
            this._lstFolderDLLInfo.Location = new System.Drawing.Point(3, 18);
            this._lstFolderDLLInfo.Name = "_lstFolderDLLInfo";
            this._lstFolderDLLInfo.Size = new System.Drawing.Size(499, 566);
            this._lstFolderDLLInfo.TabIndex = 22;
            this._lstFolderDLLInfo.UseCompatibleStateImageBehavior = false;
            this._lstFolderDLLInfo.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Item";
            this.columnHeader1.Width = 175;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Value";
            this.columnHeader2.Width = 210;
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(3, 584);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(499, 12);
            this.panel2.TabIndex = 21;
            // 
            // panel14
            // 
            this.panel14.Controls.Add(this.panel1);
            this.panel14.Controls.Add(this._btnRead);
            this.panel14.Controls.Add(this.panel20);
            this.panel14.Controls.Add(this._btnConfirm);
            this.panel14.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel14.Location = new System.Drawing.Point(3, 596);
            this.panel14.Name = "panel14";
            this.panel14.Size = new System.Drawing.Size(499, 43);
            this.panel14.TabIndex = 19;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(156, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(12, 43);
            this.panel1.TabIndex = 4;
            // 
            // _btnRead
            // 
            this._btnRead.BackColor = System.Drawing.SystemColors.Control;
            this._btnRead.Dock = System.Windows.Forms.DockStyle.Right;
            this._btnRead.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnRead.Location = new System.Drawing.Point(168, 0);
            this._btnRead.Name = "_btnRead";
            this._btnRead.Size = new System.Drawing.Size(144, 43);
            this._btnRead.TabIndex = 3;
            this._btnRead.Text = "Read DLL Info";
            this._btnRead.UseVisualStyleBackColor = false;
            this._btnRead.Click += new System.EventHandler(this._btnRead_Click);
            // 
            // panel20
            // 
            this.panel20.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel20.Location = new System.Drawing.Point(312, 0);
            this.panel20.Name = "panel20";
            this.panel20.Size = new System.Drawing.Size(12, 43);
            this.panel20.TabIndex = 2;
            // 
            // _btnConfirm
            // 
            this._btnConfirm.BackColor = System.Drawing.SystemColors.Control;
            this._btnConfirm.Dock = System.Windows.Forms.DockStyle.Right;
            this._btnConfirm.Enabled = false;
            this._btnConfirm.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnConfirm.Location = new System.Drawing.Point(324, 0);
            this._btnConfirm.Name = "_btnConfirm";
            this._btnConfirm.Size = new System.Drawing.Size(175, 43);
            this._btnConfirm.TabIndex = 1;
            this._btnConfirm.Text = "Confirm Load DLL";
            this._btnConfirm.UseVisualStyleBackColor = false;
            this._btnConfirm.Click += new System.EventHandler(this._btnConfirm_Click);
            // 
            // FrmRobot
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(216)))), ((int)(((byte)(243)))));
            this.ClientSize = new System.Drawing.Size(1024, 654);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel10);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel8);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmRobot";
            this.Text = "Robot DLL";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.panel14.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView _lstBcsDLLInfo;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListView _lstFolderDLLInfo;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel14;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button _btnRead;
        private System.Windows.Forms.Panel panel20;
        private System.Windows.Forms.Button _btnConfirm;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}