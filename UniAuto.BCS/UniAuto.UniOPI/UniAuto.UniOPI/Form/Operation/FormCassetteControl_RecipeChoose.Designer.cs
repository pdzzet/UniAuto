namespace UniOPI
{
    partial class FormCassetteControl_RecipeChoose
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
            this.dgvRecipe = new System.Windows.Forms.DataGridView();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.colRecipeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.tlpBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipe)).BeginInit();
            this.pnlButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(452, 30);
            this.lblCaption.Text = "Choose Recipe";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.tlpBase);
            this.spcBase.Size = new System.Drawing.Size(512, 390);
            // 
            // tlpBase
            // 
            this.tlpBase.ColumnCount = 1;
            this.tlpBase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBase.Controls.Add(this.dgvRecipe, 0, 0);
            this.tlpBase.Controls.Add(this.pnlButton, 0, 1);
            this.tlpBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpBase.Location = new System.Drawing.Point(0, 0);
            this.tlpBase.Name = "tlpBase";
            this.tlpBase.RowCount = 2;
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpBase.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tlpBase.Size = new System.Drawing.Size(512, 359);
            this.tlpBase.TabIndex = 0;
            // 
            // dgvRecipe
            // 
            this.dgvRecipe.AllowUserToAddRows = false;
            this.dgvRecipe.AllowUserToDeleteRows = false;
            this.dgvRecipe.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvRecipe.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvRecipe.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRecipe.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvRecipe.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecipe.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRecipeName,
            this.colPPID});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvRecipe.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvRecipe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRecipe.Location = new System.Drawing.Point(3, 3);
            this.dgvRecipe.Name = "dgvRecipe";
            this.dgvRecipe.ReadOnly = true;
            this.dgvRecipe.RowHeadersVisible = false;
            this.dgvRecipe.RowTemplate.Height = 24;
            this.dgvRecipe.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecipe.Size = new System.Drawing.Size(506, 307);
            this.dgvRecipe.TabIndex = 18;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnCancel);
            this.pnlButton.Controls.Add(this.btnOK);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButton.Location = new System.Drawing.Point(3, 316);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(506, 40);
            this.pnlButton.TabIndex = 19;
            // 
            // btnCancel
            // 
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(261, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 35);
            this.btnCancel.TabIndex = 26;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.Button_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(156, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 35);
            this.btnOK.TabIndex = 25;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.Button_Click);
            // 
            // colRecipeName
            // 
            this.colRecipeName.HeaderText = "RecipeName";
            this.colRecipeName.Name = "colRecipeName";
            this.colRecipeName.ReadOnly = true;
            this.colRecipeName.Width = 200;
            // 
            // colPPID
            // 
            this.colPPID.HeaderText = "PPID";
            this.colPPID.Name = "colPPID";
            this.colPPID.ReadOnly = true;
            this.colPPID.Width = 300;
            // 
            // FormCassetteControl_RecipeChoose
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 390);
            this.Name = "FormCassetteControl_RecipeChoose";
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.tlpBase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipe)).EndInit();
            this.pnlButton.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpBase;
        private System.Windows.Forms.DataGridView dgvRecipe;
        private System.Windows.Forms.Panel pnlButton;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecipeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPPID;
    }
}