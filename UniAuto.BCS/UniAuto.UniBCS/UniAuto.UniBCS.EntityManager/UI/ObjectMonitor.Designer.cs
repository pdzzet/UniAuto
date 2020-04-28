namespace UniAuto.UniBCS.EntityManager.UI
{
    partial class ObjectMonitor
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
            this.treeViewMonitor = new System.Windows.Forms.TreeView();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // treeViewMonitor
            // 
            this.treeViewMonitor.Dock = System.Windows.Forms.DockStyle.Top;
            this.treeViewMonitor.Font = new System.Drawing.Font("PMingLiU", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.treeViewMonitor.Location = new System.Drawing.Point(0, 0);
            this.treeViewMonitor.Margin = new System.Windows.Forms.Padding(0);
            this.treeViewMonitor.Name = "treeViewMonitor";
            this.treeViewMonitor.Size = new System.Drawing.Size(534, 578);
            this.treeViewMonitor.TabIndex = 3;
            this.treeViewMonitor.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewMonitor_ItemDrag);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(447, 585);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // ObjectMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 612);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.treeViewMonitor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ObjectMonitor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ObjectMonitor";
            this.Load += new System.EventHandler(this.ObjectMonitor_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewMonitor;
        private System.Windows.Forms.Button btnRefresh;
    }
}