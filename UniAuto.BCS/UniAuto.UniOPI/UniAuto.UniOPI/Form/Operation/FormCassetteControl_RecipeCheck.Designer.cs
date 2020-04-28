namespace UniOPI
{
    partial class FormCassetteControl_RecipeCheck
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.dgvRecipe = new System.Windows.Forms.DataGridView();
            this.dgvResult = new System.Windows.Forms.DataGridView();
            this.colLocalNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecipeNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.corReturn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReturnMsg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCIMMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pnlNG = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.colCheck = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colRecipeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecipeCheckResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipe)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResult)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(950, 30);
            this.lblCaption.Text = "Recipe Check";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.label4);
            this.spcBase.Panel2.Controls.Add(this.label3);
            this.spcBase.Panel2.Controls.Add(this.label2);
            this.spcBase.Panel2.Controls.Add(this.label1);
            this.spcBase.Panel2.Controls.Add(this.panel3);
            this.spcBase.Panel2.Controls.Add(this.panel2);
            this.spcBase.Panel2.Controls.Add(this.panel1);
            this.spcBase.Panel2.Controls.Add(this.pnlNG);
            this.spcBase.Panel2.Controls.Add(this.groupBox1);
            this.spcBase.Panel2.Controls.Add(this.btnCancel);
            this.spcBase.Panel2.Controls.Add(this.btnOK);
            this.spcBase.Size = new System.Drawing.Size(1010, 479);
            // 
            // btnCancel
            // 
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(902, 410);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(796, 410);
            this.btnOK.Name = "btnOK";
            this.btnOK.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.TabIndex = 20;
            this.btnOK.Text = "Send";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // dgvRecipe
            // 
            this.dgvRecipe.AllowUserToAddRows = false;
            this.dgvRecipe.AllowUserToDeleteRows = false;
            this.dgvRecipe.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvRecipe.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvRecipe.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvRecipe.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRecipe.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvRecipe.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecipe.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCheck,
            this.colRecipeName,
            this.colServerName,
            this.colRecipeCheckResult});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvRecipe.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvRecipe.Location = new System.Drawing.Point(6, 12);
            this.dgvRecipe.Name = "dgvRecipe";
            this.dgvRecipe.RowHeadersVisible = false;
            this.dgvRecipe.RowTemplate.Height = 24;
            this.dgvRecipe.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecipe.Size = new System.Drawing.Size(465, 361);
            this.dgvRecipe.TabIndex = 22;
            this.dgvRecipe.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRecipe_CellClick);
            // 
            // dgvResult
            // 
            this.dgvResult.AllowUserToAddRows = false;
            this.dgvResult.AllowUserToDeleteRows = false;
            this.dgvResult.AllowUserToResizeRows = false;
            this.dgvResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvResult.BackgroundColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12.75F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLocalNo,
            this.colRecipeNo,
            this.corReturn,
            this.colReturnMsg,
            this.colCIMMode});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvResult.DefaultCellStyle = dataGridViewCellStyle5;
            this.dgvResult.Location = new System.Drawing.Point(474, 12);
            this.dgvResult.Name = "dgvResult";
            this.dgvResult.ReadOnly = true;
            this.dgvResult.RowHeadersVisible = false;
            this.dgvResult.RowTemplate.Height = 24;
            this.dgvResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvResult.Size = new System.Drawing.Size(510, 361);
            this.dgvResult.TabIndex = 23;
            // 
            // colLocalNo
            // 
            this.colLocalNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colLocalNo.HeaderText = "Local No";
            this.colLocalNo.Name = "colLocalNo";
            this.colLocalNo.ReadOnly = true;
            this.colLocalNo.Width = 95;
            // 
            // colRecipeNo
            // 
            this.colRecipeNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colRecipeNo.HeaderText = "Recipe No";
            this.colRecipeNo.MinimumWidth = 100;
            this.colRecipeNo.Name = "colRecipeNo";
            this.colRecipeNo.ReadOnly = true;
            this.colRecipeNo.Width = 104;
            // 
            // corReturn
            // 
            this.corReturn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.corReturn.HeaderText = "Return";
            this.corReturn.Name = "corReturn";
            this.corReturn.ReadOnly = true;
            this.corReturn.Width = 55;
            // 
            // colReturnMsg
            // 
            this.colReturnMsg.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colReturnMsg.HeaderText = "Return Message";
            this.colReturnMsg.Name = "colReturnMsg";
            this.colReturnMsg.ReadOnly = true;
            this.colReturnMsg.Width = 145;
            // 
            // colCIMMode
            // 
            this.colCIMMode.HeaderText = "CIM Mode";
            this.colCIMMode.Name = "colCIMMode";
            this.colCIMMode.ReadOnly = true;
            this.colCIMMode.Width = 108;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dgvRecipe);
            this.groupBox1.Controls.Add(this.dgvResult);
            this.groupBox1.Location = new System.Drawing.Point(12, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(990, 379);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            // 
            // pnlNG
            // 
            this.pnlNG.BackColor = System.Drawing.Color.Red;
            this.pnlNG.Location = new System.Drawing.Point(12, 390);
            this.pnlNG.Name = "pnlNG";
            this.pnlNG.Size = new System.Drawing.Size(19, 19);
            this.pnlNG.TabIndex = 25;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Silver;
            this.panel1.Location = new System.Drawing.Point(12, 415);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(19, 19);
            this.panel1.TabIndex = 26;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Gold;
            this.panel2.Location = new System.Drawing.Point(176, 390);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(19, 19);
            this.panel2.TabIndex = 27;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Pink;
            this.panel3.Location = new System.Drawing.Point(176, 415);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(19, 19);
            this.panel3.TabIndex = 28;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(31, 390);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 19);
            this.label1.TabIndex = 29;
            this.label1.Text = "Register NG";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(32, 415);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 19);
            this.label2.TabIndex = 30;
            this.label2.Text = "Unknown";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(197, 390);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(235, 19);
            this.label3.TabIndex = 31;
            this.label3.Text = "By Pass (CIM OFF / No Check / Zero)";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(197, 415);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 19);
            this.label4.TabIndex = 32;
            this.label4.Text = "Timeout";
            // 
            // colCheck
            // 
            this.colCheck.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colCheck.HeaderText = "Check";
            this.colCheck.Name = "colCheck";
            this.colCheck.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colCheck.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colCheck.Width = 50;
            // 
            // colRecipeName
            // 
            this.colRecipeName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colRecipeName.HeaderText = "Recipe Name";
            this.colRecipeName.Name = "colRecipeName";
            this.colRecipeName.ReadOnly = true;
            // 
            // colServerName
            // 
            this.colServerName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colServerName.HeaderText = "Line ID";
            this.colServerName.Name = "colServerName";
            // 
            // colRecipeCheckResult
            // 
            this.colRecipeCheckResult.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colRecipeCheckResult.HeaderText = "Result";
            this.colRecipeCheckResult.Name = "colRecipeCheckResult";
            this.colRecipeCheckResult.ReadOnly = true;
            this.colRecipeCheckResult.Width = 50;
            // 
            // FormCassetteControl_RecipeCheck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1010, 479);
            this.Name = "FormCassetteControl_RecipeCheck";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormCassetteControl_RecipeCheck_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecipe)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResult)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridView dgvRecipe;
        private System.Windows.Forms.DataGridView dgvResult;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel pnlNG;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLocalNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecipeNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn corReturn;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReturnMsg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCIMMode;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecipeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecipeCheckResult;
    }
}