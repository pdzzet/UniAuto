namespace UniAuto.UniBCS.CSOT.SocketService.Test
{
    partial class FrmTest
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
            this.btnActive_RecipeIDRegisterCheckRequest = new System.Windows.Forms.Button();
            this.btnActive_RecipeParameterRequest = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnActive_RecipeIDRegisterCheckRequest
            // 
            this.btnActive_RecipeIDRegisterCheckRequest.Location = new System.Drawing.Point(95, 36);
            this.btnActive_RecipeIDRegisterCheckRequest.Name = "btnActive_RecipeIDRegisterCheckRequest";
            this.btnActive_RecipeIDRegisterCheckRequest.Size = new System.Drawing.Size(330, 39);
            this.btnActive_RecipeIDRegisterCheckRequest.TabIndex = 0;
            this.btnActive_RecipeIDRegisterCheckRequest.Text = "Active RecipeIDRegisterCheckRequest";
            this.btnActive_RecipeIDRegisterCheckRequest.UseVisualStyleBackColor = true;
            this.btnActive_RecipeIDRegisterCheckRequest.Click += new System.EventHandler(this.btnActive_RecipeIDRegisterCheckRequest_Click);
            // 
            // btnActive_RecipeParameterRequest
            // 
            this.btnActive_RecipeParameterRequest.Location = new System.Drawing.Point(95, 113);
            this.btnActive_RecipeParameterRequest.Name = "btnActive_RecipeParameterRequest";
            this.btnActive_RecipeParameterRequest.Size = new System.Drawing.Size(330, 39);
            this.btnActive_RecipeParameterRequest.TabIndex = 1;
            this.btnActive_RecipeParameterRequest.Text = "Active RecipeParameterRequest";
            this.btnActive_RecipeParameterRequest.UseVisualStyleBackColor = true;
            this.btnActive_RecipeParameterRequest.Click += new System.EventHandler(this.btnActive_RecipeParameterRequest_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(95, 197);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(330, 39);
            this.button1.TabIndex = 2;
            this.button1.Text = "ActiveJobShortCutPermit";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FrmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 348);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnActive_RecipeParameterRequest);
            this.Controls.Add(this.btnActive_RecipeIDRegisterCheckRequest);
            this.Name = "FrmTest";
            this.Text = "FrmTest";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnActive_RecipeIDRegisterCheckRequest;
        private System.Windows.Forms.Button btnActive_RecipeParameterRequest;
        private System.Windows.Forms.Button button1;
    }
}