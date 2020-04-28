namespace UniOPI
{
    partial class FormCassetteControl_JobExistenceSlot
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
            this.flpExist = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOK = new System.Windows.Forms.Button();
            this.grbExist = new System.Windows.Forms.GroupBox();
            this.grbPortData = new System.Windows.Forms.GroupBox();
            this.flpLotData = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlPortID = new System.Windows.Forms.Panel();
            this.lblPortID = new System.Windows.Forms.Label();
            this.txtPortID = new System.Windows.Forms.TextBox();
            this.pnlSlotGlassCount = new System.Windows.Forms.Panel();
            this.lblSlotGlassCount = new System.Windows.Forms.Label();
            this.txtSlotGlassCount = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblJobExistSlot = new System.Windows.Forms.Label();
            this.txtJobExistSlot = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.grbExist.SuspendLayout();
            this.grbPortData.SuspendLayout();
            this.flpLotData.SuspendLayout();
            this.pnlPortID.SuspendLayout();
            this.pnlSlotGlassCount.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(640, 30);
            this.lblCaption.Text = "Job Existence Slot";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.grbPortData);
            this.spcBase.Panel2.Controls.Add(this.grbExist);
            this.spcBase.Panel2.Controls.Add(this.btnOK);
            this.spcBase.Size = new System.Drawing.Size(700, 410);
            // 
            // flpExist
            // 
            this.flpExist.AutoScroll = true;
            this.flpExist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpExist.Location = new System.Drawing.Point(3, 22);
            this.flpExist.Name = "flpExist";
            this.flpExist.Size = new System.Drawing.Size(668, 208);
            this.flpExist.TabIndex = 1;
            // 
            // btnOK
            // 
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(279, 337);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(98, 30);
            this.btnOK.TabIndex = 34;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // grbExist
            // 
            this.grbExist.Controls.Add(this.flpExist);
            this.grbExist.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grbExist.Location = new System.Drawing.Point(12, 98);
            this.grbExist.Name = "grbExist";
            this.grbExist.Size = new System.Drawing.Size(674, 233);
            this.grbExist.TabIndex = 35;
            this.grbExist.TabStop = false;
            this.grbExist.Text = "Job existence slot";
            // 
            // grbPortData
            // 
            this.grbPortData.Controls.Add(this.flpLotData);
            this.grbPortData.Font = new System.Drawing.Font("Calibri", 12F);
            this.grbPortData.Location = new System.Drawing.Point(12, 5);
            this.grbPortData.Name = "grbPortData";
            this.grbPortData.Size = new System.Drawing.Size(674, 87);
            this.grbPortData.TabIndex = 36;
            this.grbPortData.TabStop = false;
            // 
            // flpLotData
            // 
            this.flpLotData.Controls.Add(this.pnlPortID);
            this.flpLotData.Controls.Add(this.pnlSlotGlassCount);
            this.flpLotData.Controls.Add(this.panel1);
            this.flpLotData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpLotData.Location = new System.Drawing.Point(3, 23);
            this.flpLotData.Name = "flpLotData";
            this.flpLotData.Size = new System.Drawing.Size(668, 61);
            this.flpLotData.TabIndex = 0;
            // 
            // pnlPortID
            // 
            this.pnlPortID.Controls.Add(this.lblPortID);
            this.pnlPortID.Controls.Add(this.txtPortID);
            this.pnlPortID.Location = new System.Drawing.Point(1, 1);
            this.pnlPortID.Margin = new System.Windows.Forms.Padding(1, 1, 3, 1);
            this.pnlPortID.Name = "pnlPortID";
            this.pnlPortID.Size = new System.Drawing.Size(330, 25);
            this.pnlPortID.TabIndex = 16;
            this.pnlPortID.Tag = "LotID";
            // 
            // lblPortID
            // 
            this.lblPortID.BackColor = System.Drawing.Color.Black;
            this.lblPortID.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblPortID.ForeColor = System.Drawing.Color.White;
            this.lblPortID.Location = new System.Drawing.Point(3, 1);
            this.lblPortID.Name = "lblPortID";
            this.lblPortID.Size = new System.Drawing.Size(130, 23);
            this.lblPortID.TabIndex = 5;
            this.lblPortID.Text = "Port ID";
            this.lblPortID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPortID
            // 
            this.txtPortID.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtPortID.Location = new System.Drawing.Point(135, 0);
            this.txtPortID.Name = "txtPortID";
            this.txtPortID.ReadOnly = true;
            this.txtPortID.Size = new System.Drawing.Size(190, 25);
            this.txtPortID.TabIndex = 0;
            // 
            // pnlSlotGlassCount
            // 
            this.pnlSlotGlassCount.Controls.Add(this.lblSlotGlassCount);
            this.pnlSlotGlassCount.Controls.Add(this.txtSlotGlassCount);
            this.pnlSlotGlassCount.Location = new System.Drawing.Point(335, 1);
            this.pnlSlotGlassCount.Margin = new System.Windows.Forms.Padding(1, 1, 3, 1);
            this.pnlSlotGlassCount.Name = "pnlSlotGlassCount";
            this.pnlSlotGlassCount.Size = new System.Drawing.Size(330, 25);
            this.pnlSlotGlassCount.TabIndex = 19;
            this.pnlSlotGlassCount.Tag = "PRODUCTSPECNAME";
            // 
            // lblSlotGlassCount
            // 
            this.lblSlotGlassCount.BackColor = System.Drawing.Color.Black;
            this.lblSlotGlassCount.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblSlotGlassCount.ForeColor = System.Drawing.Color.White;
            this.lblSlotGlassCount.Location = new System.Drawing.Point(3, 1);
            this.lblSlotGlassCount.Name = "lblSlotGlassCount";
            this.lblSlotGlassCount.Size = new System.Drawing.Size(130, 23);
            this.lblSlotGlassCount.TabIndex = 5;
            this.lblSlotGlassCount.Text = "Slot Glass Count";
            this.lblSlotGlassCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSlotGlassCount
            // 
            this.txtSlotGlassCount.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtSlotGlassCount.Location = new System.Drawing.Point(135, 0);
            this.txtSlotGlassCount.Name = "txtSlotGlassCount";
            this.txtSlotGlassCount.ReadOnly = true;
            this.txtSlotGlassCount.Size = new System.Drawing.Size(190, 25);
            this.txtSlotGlassCount.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblJobExistSlot);
            this.panel1.Controls.Add(this.txtJobExistSlot);
            this.panel1.Location = new System.Drawing.Point(1, 28);
            this.panel1.Margin = new System.Windows.Forms.Padding(1, 1, 3, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(664, 25);
            this.panel1.TabIndex = 20;
            this.panel1.Tag = "PRODUCTSPECNAME";
            // 
            // lblJobExistSlot
            // 
            this.lblJobExistSlot.BackColor = System.Drawing.Color.Black;
            this.lblJobExistSlot.Font = new System.Drawing.Font("Cambria", 11.25F);
            this.lblJobExistSlot.ForeColor = System.Drawing.Color.White;
            this.lblJobExistSlot.Location = new System.Drawing.Point(3, 1);
            this.lblJobExistSlot.Name = "lblJobExistSlot";
            this.lblJobExistSlot.Size = new System.Drawing.Size(130, 23);
            this.lblJobExistSlot.TabIndex = 5;
            this.lblJobExistSlot.Text = "Job Existence Slot";
            this.lblJobExistSlot.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtJobExistSlot
            // 
            this.txtJobExistSlot.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold);
            this.txtJobExistSlot.Location = new System.Drawing.Point(135, 0);
            this.txtJobExistSlot.Name = "txtJobExistSlot";
            this.txtJobExistSlot.ReadOnly = true;
            this.txtJobExistSlot.Size = new System.Drawing.Size(524, 25);
            this.txtJobExistSlot.TabIndex = 3;
            // 
            // FormCassetteControl_JobExistenceSlot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 410);
            this.Name = "FormCassetteControl_JobExistenceSlot";
            this.Text = "  ";
            this.Load += new System.EventHandler(this.FormCassetteControl_JobExistenceSlot_Load);
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.grbExist.ResumeLayout(false);
            this.grbPortData.ResumeLayout(false);
            this.flpLotData.ResumeLayout(false);
            this.pnlPortID.ResumeLayout(false);
            this.pnlPortID.PerformLayout();
            this.pnlSlotGlassCount.ResumeLayout(false);
            this.pnlSlotGlassCount.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpExist;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox grbExist;
        private System.Windows.Forms.GroupBox grbPortData;
        private System.Windows.Forms.FlowLayoutPanel flpLotData;
        private System.Windows.Forms.Panel pnlPortID;
        private System.Windows.Forms.Label lblPortID;
        private System.Windows.Forms.TextBox txtPortID;
        private System.Windows.Forms.Panel pnlSlotGlassCount;
        private System.Windows.Forms.Label lblSlotGlassCount;
        private System.Windows.Forms.TextBox txtSlotGlassCount;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblJobExistSlot;
        private System.Windows.Forms.TextBox txtJobExistSlot;
    }
}