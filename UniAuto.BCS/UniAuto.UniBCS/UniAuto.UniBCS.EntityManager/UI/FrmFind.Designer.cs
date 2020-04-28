namespace UniAuto.UniBCS.EntityManager.UI
{
    partial class FrmFind
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFind));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtJobID = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtJobSequence = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtCSTSequence = new System.Windows.Forms.TextBox();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.txtJobID);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.txtJobSequence);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.txtCSTSequence);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(451, 89);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "Job ID ";
            // 
            // txtJobID
            // 
            this.txtJobID.Location = new System.Drawing.Point(74, 46);
            this.txtJobID.MaxLength = 20;
            this.txtJobID.Name = "txtJobID";
            this.txtJobID.Size = new System.Drawing.Size(120, 21);
            this.txtJobID.TabIndex = 10;
            this.txtJobID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtJobID_KeyPress);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(320, 49);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 21);
            this.button2.TabIndex = 8;
            this.button2.Text = "Find";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(218, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "Job Seq";
            // 
            // txtJobSequence
            // 
            this.txtJobSequence.Location = new System.Drawing.Point(278, 13);
            this.txtJobSequence.MaxLength = 4;
            this.txtJobSequence.Name = "txtJobSequence";
            this.txtJobSequence.Size = new System.Drawing.Size(120, 21);
            this.txtJobSequence.TabIndex = 6;
            this.txtJobSequence.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtJobSequence_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "CST Seq";
            // 
            // txtCSTSequence
            // 
            this.txtCSTSequence.Location = new System.Drawing.Point(74, 14);
            this.txtCSTSequence.MaxLength = 5;
            this.txtCSTSequence.Name = "txtCSTSequence";
            this.txtCSTSequence.Size = new System.Drawing.Size(120, 21);
            this.txtCSTSequence.TabIndex = 3;
            this.txtCSTSequence.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCSTSequence_KeyPress);
            // 
            // FrmFind
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 89);
            this.Controls.Add(this.groupBox2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmFind";
            this.Text = "Find";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtJobID;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtJobSequence;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtCSTSequence;

    }
}