namespace AuthenticationServer
{
    partial class AuthenticationServer
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
        public void InitializeComponent()
        {
            this.logBox = new System.Windows.Forms.ListBox();
            this.runButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // logBox
            // 
            this.logBox.FormattingEnabled = true;
            this.logBox.Location = new System.Drawing.Point(12, 32);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(460, 316);
            this.logBox.TabIndex = 0;
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(150, 5);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(200, 20);
            this.runButton.TabIndex = 1;
            this.runButton.Text = "RUN AUTHENTICATION SERVER";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // AuthenticationServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.logBox);
            this.Name = "AuthenticationServer";
            this.Text = "Authentication Server";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ListBox logBox;
        private System.Windows.Forms.Button runButton;
    }
}

