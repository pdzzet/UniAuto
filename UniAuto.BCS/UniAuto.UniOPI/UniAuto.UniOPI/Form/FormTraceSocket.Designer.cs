namespace UniOPI
{
    partial class FormTraceSocket
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
            this.txtReport = new System.Windows.Forms.TextBox();
            this.txtReportReply = new System.Windows.Forms.TextBox();
            this.txtRequest = new System.Windows.Forms.TextBox();
            this.txtReply = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtInQueue = new System.Windows.Forms.TextBox();
            this.txtOutQueue = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.txtUIThreadID = new System.Windows.Forms.TextBox();
            this.txtPollThreadID = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtReport
            // 
            this.txtReport.Location = new System.Drawing.Point(248, 24);
            this.txtReport.Name = "txtReport";
            this.txtReport.ReadOnly = true;
            this.txtReport.Size = new System.Drawing.Size(100, 25);
            this.txtReport.TabIndex = 0;
            // 
            // txtReportReply
            // 
            this.txtReportReply.Location = new System.Drawing.Point(248, 55);
            this.txtReportReply.Name = "txtReportReply";
            this.txtReportReply.ReadOnly = true;
            this.txtReportReply.Size = new System.Drawing.Size(100, 25);
            this.txtReportReply.TabIndex = 1;
            // 
            // txtRequest
            // 
            this.txtRequest.Location = new System.Drawing.Point(248, 24);
            this.txtRequest.Name = "txtRequest";
            this.txtRequest.ReadOnly = true;
            this.txtRequest.Size = new System.Drawing.Size(100, 25);
            this.txtRequest.TabIndex = 2;
            // 
            // txtReply
            // 
            this.txtReply.Location = new System.Drawing.Point(248, 55);
            this.txtReply.Name = "txtReply";
            this.txtReply.ReadOnly = true;
            this.txtReply.Size = new System.Drawing.Size(100, 25);
            this.txtReply.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "EQ Status Report";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(41, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "EQ Status Reply";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(41, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "EQ Status Request";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(41, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(143, 15);
            this.label4.TabIndex = 7;
            this.label4.Text = "EQ Status Report Reply";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtReport);
            this.groupBox1.Controls.Add(this.txtReportReply);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(419, 91);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtRequest);
            this.groupBox2.Controls.Add(this.txtReply);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 91);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(419, 90);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtInQueue);
            this.groupBox3.Controls.Add(this.txtOutQueue);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(0, 181);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(419, 92);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox3";
            // 
            // txtInQueue
            // 
            this.txtInQueue.Location = new System.Drawing.Point(248, 24);
            this.txtInQueue.Name = "txtInQueue";
            this.txtInQueue.ReadOnly = true;
            this.txtInQueue.Size = new System.Drawing.Size(100, 25);
            this.txtInQueue.TabIndex = 7;
            // 
            // txtOutQueue
            // 
            this.txtOutQueue.Location = new System.Drawing.Point(248, 55);
            this.txtOutQueue.Name = "txtOutQueue";
            this.txtOutQueue.ReadOnly = true;
            this.txtOutQueue.Size = new System.Drawing.Size(100, 25);
            this.txtOutQueue.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(41, 27);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 15);
            this.label5.TabIndex = 10;
            this.label5.Text = "Socket In Queue";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(41, 58);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(110, 15);
            this.label6.TabIndex = 9;
            this.label6.Text = "Socket Out Queue";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtUIThreadID);
            this.groupBox4.Controls.Add(this.txtPollThreadID);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(0, 273);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(419, 187);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "groupBox4";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // txtUIThreadID
            // 
            this.txtUIThreadID.Location = new System.Drawing.Point(248, 24);
            this.txtUIThreadID.Name = "txtUIThreadID";
            this.txtUIThreadID.ReadOnly = true;
            this.txtUIThreadID.Size = new System.Drawing.Size(100, 25);
            this.txtUIThreadID.TabIndex = 11;
            // 
            // txtPollThreadID
            // 
            this.txtPollThreadID.Location = new System.Drawing.Point(248, 55);
            this.txtPollThreadID.Name = "txtPollThreadID";
            this.txtPollThreadID.ReadOnly = true;
            this.txtPollThreadID.Size = new System.Drawing.Size(100, 25);
            this.txtPollThreadID.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(41, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(85, 15);
            this.label7.TabIndex = 14;
            this.label7.Text = "UI Thread ID";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(41, 58);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(93, 15);
            this.label8.TabIndex = 13;
            this.label8.Text = "Poll Thread ID";
            // 
            // FormTraceSocket
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(419, 460);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormTraceSocket";
            this.Text = "FormTraceSocket";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormTraceSocket_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtReport;
        private System.Windows.Forms.TextBox txtReportReply;
        private System.Windows.Forms.TextBox txtRequest;
        private System.Windows.Forms.TextBox txtReply;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtInQueue;
        private System.Windows.Forms.TextBox txtOutQueue;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox txtUIThreadID;
        private System.Windows.Forms.TextBox txtPollThreadID;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
    }
}