namespace Client
{
    partial class LogInForm
    {
        private System.Windows.Forms.TextBox userNameBox;
        private System.Windows.Forms.TextBox passwordBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button logInButton;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.ListBox logBox;

        private void InitializeComponent()
        {
            this.userNameBox = new System.Windows.Forms.TextBox();
            this.passwordBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.logInButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.logBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // userNameBox
            // 
            this.userNameBox.Location = new System.Drawing.Point(12, 37);
            this.userNameBox.Name = "userNameBox";
            this.userNameBox.Size = new System.Drawing.Size(119, 20);
            this.userNameBox.TabIndex = 0;
            this.userNameBox.Text = "user1";
            // 
            // passwordBox
            // 
            this.passwordBox.Location = new System.Drawing.Point(148, 38);
            this.passwordBox.Name = "passwordBox";
            this.passwordBox.PasswordChar = '*';
            this.passwordBox.Size = new System.Drawing.Size(119, 20);
            this.passwordBox.TabIndex = 1;
            this.passwordBox.Text = "pas123";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "User Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(145, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password";
            // 
            // logInButton
            // 
            this.logInButton.Location = new System.Drawing.Point(331, 35);
            this.logInButton.Name = "logInButton";
            this.logInButton.Size = new System.Drawing.Size(75, 23);
            this.logInButton.TabIndex = 4;
            this.logInButton.Text = "Log In";
            this.logInButton.UseVisualStyleBackColor = true;
            this.logInButton.Click += new System.EventHandler(this.logInClick);
            // 
            // exitButton
            // 
            this.exitButton.Location = new System.Drawing.Point(435, 35);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 23);
            this.exitButton.TabIndex = 5;
            this.exitButton.Text = "Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // logBox
            // 
            this.logBox.FormattingEnabled = true;
            this.logBox.Items.AddRange(new object[] {
            "Connected to authentication server successfully.",
            "Please enter user name and password."});
            this.logBox.Location = new System.Drawing.Point(12, 63);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(498, 277);
            this.logBox.TabIndex = 6;
            // 
            // LogInForm
            // 
            this.ClientSize = new System.Drawing.Size(522, 349);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.logInButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.passwordBox);
            this.Controls.Add(this.userNameBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LogInForm";
            this.Text = "Log In Screen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}