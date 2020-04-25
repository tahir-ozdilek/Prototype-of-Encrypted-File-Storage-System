namespace Client
{

    partial class Client
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Client));
            this.browseButton = new System.Windows.Forms.Button();
            this.encryptUploadButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.downloadButton = new System.Windows.Forms.Button();
            this.serverBox = new System.Windows.Forms.ListBox();
            this.pathBox = new System.Windows.Forms.TextBox();
            this.logBox = new System.Windows.Forms.ListBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.ipBox = new System.Windows.Forms.TextBox();
            this.portBox = new System.Windows.Forms.TextBox();
            this.connectLabel = new System.Windows.Forms.Label();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.serverPanelLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // browseButton
            // 
            resources.ApplyResources(this.browseButton, "browseButton");
            this.browseButton.Name = "browseButton";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // encryptUploadButton
            // 
            resources.ApplyResources(this.encryptUploadButton, "encryptUploadButton");
            this.encryptUploadButton.Name = "encryptUploadButton";
            this.encryptUploadButton.UseVisualStyleBackColor = true;
            this.encryptUploadButton.Click += new System.EventHandler(this.EncryptAndUploadClick);
            // 
            // deleteButton
            // 
            resources.ApplyResources(this.deleteButton, "deleteButton");
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // downloadButton
            // 
            resources.ApplyResources(this.downloadButton, "downloadButton");
            this.downloadButton.Name = "downloadButton";
            this.downloadButton.UseVisualStyleBackColor = true;
            this.downloadButton.Click += new System.EventHandler(this.DownloadButton_Click);
            // 
            // serverBox
            // 
            resources.ApplyResources(this.serverBox, "serverBox");
            this.serverBox.FormattingEnabled = true;
            this.serverBox.Name = "serverBox";
            // 
            // pathBox
            // 
            resources.ApplyResources(this.pathBox, "pathBox");
            this.pathBox.Name = "pathBox";
            this.pathBox.ReadOnly = true;
            // 
            // logBox
            // 
            this.logBox.FormattingEnabled = true;
            this.logBox.Items.AddRange(new object[] {
            resources.GetString("logBox.Items"),
            resources.GetString("logBox.Items1"),
            resources.GetString("logBox.Items2"),
            resources.GetString("logBox.Items3"),
            resources.GetString("logBox.Items4")});
            resources.ApplyResources(this.logBox, "logBox");
            this.logBox.Name = "logBox";
            // 
            // connectButton
            // 
            resources.ApplyResources(this.connectButton, "connectButton");
            this.connectButton.Name = "connectButton";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // ipBox
            // 
            this.ipBox.ForeColor = System.Drawing.SystemColors.ControlDark;
            resources.ApplyResources(this.ipBox, "ipBox");
            this.ipBox.Name = "ipBox";
            this.ipBox.Enter += new System.EventHandler(this.IpBox_Text_Enter);
            this.ipBox.Leave += new System.EventHandler(this.IpBox_Text_Leave);
            // 
            // portBox
            // 
            this.portBox.ForeColor = System.Drawing.SystemColors.ControlDark;
            resources.ApplyResources(this.portBox, "portBox");
            this.portBox.Name = "portBox";
            this.portBox.Enter += new System.EventHandler(this.portBox_Text_Enter);
            this.portBox.Leave += new System.EventHandler(this.portBox_Text_Leave);
            // 
            // connectLabel
            // 
            resources.ApplyResources(this.connectLabel, "connectLabel");
            this.connectLabel.ForeColor = System.Drawing.Color.Red;
            this.connectLabel.Name = "connectLabel";
            // 
            // disconnectButton
            // 
            resources.ApplyResources(this.disconnectButton, "disconnectButton");
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.DisconnectButton_Click);
            // 
            // serverPanelLabel
            // 
            resources.ApplyResources(this.serverPanelLabel, "serverPanelLabel");
            this.serverPanelLabel.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.serverPanelLabel.Name = "serverPanelLabel";
            // 
            // Client
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.serverPanelLabel);
            this.Controls.Add(this.disconnectButton);
            this.Controls.Add(this.connectLabel);
            this.Controls.Add(this.portBox);
            this.Controls.Add(this.ipBox);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.pathBox);
            this.Controls.Add(this.serverBox);
            this.Controls.Add(this.downloadButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.encryptUploadButton);
            this.Controls.Add(this.browseButton);
            this.Name = "Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button encryptUploadButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button downloadButton;
        private System.Windows.Forms.ListBox serverBox;
        private System.Windows.Forms.TextBox pathBox;
        private System.Windows.Forms.ListBox logBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TextBox ipBox;
        private System.Windows.Forms.TextBox portBox;
        private System.Windows.Forms.Label connectLabel;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.Label serverPanelLabel;
    }
}

