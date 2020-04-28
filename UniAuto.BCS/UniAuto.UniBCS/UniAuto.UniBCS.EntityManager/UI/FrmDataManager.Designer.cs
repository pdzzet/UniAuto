namespace UniAuto.UniBCS.EntityManager.UI
{
    partial class FrmDataManager
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
            this.panel7 = new System.Windows.Forms.Panel();
            this._dgvDataGird = new System.Windows.Forms.DataGridView();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.panel9 = new System.Windows.Forms.Panel();
            this._treTreeView = new System.Windows.Forms.TreeView();
            this.panel8 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recoveryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.panel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dgvDataGird)).BeginInit();
            this.pnlMain.SuspendLayout();
            this.panel9.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.Color.Transparent;
            this.panel7.Controls.Add(this._dgvDataGird);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel7.Location = new System.Drawing.Point(283, 0);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(728, 644);
            this.panel7.TabIndex = 14;
            // 
            // _dgvDataGird
            // 
            this._dgvDataGird.AllowUserToAddRows = false;
            this._dgvDataGird.AllowUserToDeleteRows = false;
            this._dgvDataGird.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(216)))), ((int)(((byte)(243)))));
            this._dgvDataGird.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dgvDataGird.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dgvDataGird.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this._dgvDataGird.Location = new System.Drawing.Point(0, 0);
            this._dgvDataGird.Name = "_dgvDataGird";
            this._dgvDataGird.RowTemplate.Height = 27;
            this._dgvDataGird.Size = new System.Drawing.Size(728, 644);
            this._dgvDataGird.TabIndex = 0;
            this._dgvDataGird.MouseClick += new System.Windows.Forms.MouseEventHandler(this._dgvDataGird_MouseClick);
            this._dgvDataGird.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this._dgvDataGird_MouseDoubleClick);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(109)))), ((int)(((byte)(158)))));
            this.panel4.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel4.Location = new System.Drawing.Point(1011, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(13, 644);
            this.panel4.TabIndex = 12;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(109)))), ((int)(((byte)(158)))));
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(13, 644);
            this.panel3.TabIndex = 11;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(109)))), ((int)(((byte)(158)))));
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 644);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1024, 10);
            this.panel2.TabIndex = 10;
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.Transparent;
            this.pnlMain.Controls.Add(this.panel9);
            this.pnlMain.Controls.Add(this.panel8);
            this.pnlMain.Controls.Add(this.panel6);
            this.pnlMain.Controls.Add(this.panel7);
            this.pnlMain.Controls.Add(this.panel4);
            this.pnlMain.Controls.Add(this.panel3);
            this.pnlMain.Controls.Add(this.panel2);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1024, 654);
            this.pnlMain.TabIndex = 2;
            // 
            // panel9
            // 
            this.panel9.Controls.Add(this._treTreeView);
            this.panel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel9.Location = new System.Drawing.Point(13, 21);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(252, 623);
            this.panel9.TabIndex = 16;
            // 
            // _treTreeView
            // 
            this._treTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._treTreeView.FullRowSelect = true;
            this._treTreeView.HideSelection = false;
            this._treTreeView.Location = new System.Drawing.Point(0, 0);
            this._treTreeView.Name = "_treTreeView";
            this._treTreeView.Size = new System.Drawing.Size(252, 623);
            this._treTreeView.TabIndex = 0;
            this._treTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this._treTreeView_AfterSelect);
            // 
            // panel8
            // 
            this.panel8.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel8.Location = new System.Drawing.Point(13, 0);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(252, 21);
            this.panel8.TabIndex = 15;
            // 
            // panel6
            // 
            this.panel6.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel6.Location = new System.Drawing.Point(265, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(18, 644);
            this.panel6.TabIndex = 15;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.toolStripMenuItem2,
            this.removeToolStripMenuItem,
            this.recoveryToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolStripMenuItem4,
            this.toolStripMenuItem3,
            this.refreshToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 170);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(149, 6);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.removeToolStripMenuItem.Text = "Remove ...";
            this.removeToolStripMenuItem.ToolTipText = "Move in Job To Trash can";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // recoveryToolStripMenuItem
            // 
            this.recoveryToolStripMenuItem.Name = "recoveryToolStripMenuItem";
            this.recoveryToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.recoveryToolStripMenuItem.Text = "Recovery ...";
            this.recoveryToolStripMenuItem.ToolTipText = "Move out Job from Trash can";
            this.recoveryToolStripMenuItem.Click += new System.EventHandler(this.recoveryToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem1.Text = "Edit";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(149, 6);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(113, 26);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.editToolStripMenuItem.Text = "Edit...";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem4.Text = "Find";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // FrmDataManager
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(216)))), ((int)(((byte)(243)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1024, 654);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FrmDataManager";
            this.Text = "Objects";
            this.Load += new System.EventHandler(this.FrmDataManager_Load);
            this.panel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._dgvDataGird)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.panel9.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel panel9;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.TreeView _treTreeView;
        private System.Windows.Forms.DataGridView _dgvDataGird;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recoveryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;



    }
}