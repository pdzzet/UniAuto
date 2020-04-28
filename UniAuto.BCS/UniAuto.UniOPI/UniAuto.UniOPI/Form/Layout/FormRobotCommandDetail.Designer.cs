namespace UniOPI
{
    partial class FormRobotCommandDetail
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
            this.richTxt = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).BeginInit();
            this.spcBase.Panel2.SuspendLayout();
            this.spcBase.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCaption
            // 
            this.lblCaption.Size = new System.Drawing.Size(559, 30);
            this.lblCaption.Text = "Robot Command Detail";
            // 
            // spcBase
            // 
            // 
            // spcBase.Panel2
            // 
            this.spcBase.Panel2.Controls.Add(this.richTxt);
            this.spcBase.Size = new System.Drawing.Size(619, 277);
            // 
            // richTxt
            // 
            this.richTxt.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.richTxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTxt.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTxt.ForeColor = System.Drawing.Color.Black;
            this.richTxt.Location = new System.Drawing.Point(0, 0);
            this.richTxt.Margin = new System.Windows.Forms.Padding(4);
            this.richTxt.Name = "richTxt";
            this.richTxt.Size = new System.Drawing.Size(619, 246);
            this.richTxt.TabIndex = 1;
            this.richTxt.Text = "";
            // 
            // FormRobotCommandDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 277);
            this.ControlBox = true;
            this.Name = "FormRobotCommandDetail";
            this.Text = "   ";
            this.spcBase.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcBase)).EndInit();
            this.spcBase.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTxt;
    }
}