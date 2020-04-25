using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.IO;

namespace Client
{

    public partial class Client : Form
    {

        static AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        public byte incomingData;           //incoming data from server application
        private int tokenID;
        Socket clientSocketForFileServer;
        IPAddress ipAddress;
        IPEndPoint remoteApplication;
        string browsedFileName;

        public Client(int tokenID)
        {
            InitializeComponent();
            this.tokenID = tokenID;
            setInterfaceToDisconnected();
        }

        private void IpBox_Text_Enter(object sender, EventArgs e)
        {
            ipBox.ForeColor = Color.Black;
            if (ipBox.Text == "Server IP")
            {
                ipBox.Clear();   
            }
        }

        private void IpBox_Text_Leave(object sender, EventArgs e)
        {
            if (ipBox.Text == "")
            {
                ipBox.ForeColor = Color.Gray;
                ipBox.Text = "Server IP";
            }
        }

        private void portBox_Text_Enter(object sender, EventArgs e)
        {
            portBox.ForeColor = Color.Black;
            if (portBox.Text == "Port")
            {
                portBox.Clear();    
            }
        }

        private void portBox_Text_Leave(object sender, EventArgs e)
        {
            if (portBox.Text == "")
            {
                portBox.ForeColor = Color.Gray;
                portBox.Text = "Port";
            }
        }

        private void EncryptAndUploadClick(object sender, EventArgs e)
        {
            if (File.Exists(pathBox.Text))
            {
                try
                {
                    clientSocketForFileServer.Send(Encoding.UTF8.GetBytes("upload" + browsedFileName));

                    FileStream readFileToBeEncrypted = File.OpenRead(pathBox.Text);    //Open the file by flow
                                                                                       //byte[] inputByteArray = new byte[readFileToBeEncrypted.Length];    //Get file binary characters
                    // Create a TcpClient. A new tcp connection is used for transferring big files.
                    TcpClient clientEndPoint = new TcpClient("127.0.0.1", 9990);
                    NetworkStream networkStream = clientEndPoint.GetStream();

                    //very secure encryption (:
                    //aes.GenerateKey();
                    aes.Key = Encoding.UTF8.GetBytes("hWmZq4t6w9z$C&F)");
                    //aes.GenerateIV();
                    aes.IV = Encoding.UTF8.GetBytes("12345678");
                    // Then each key must be held in a secure place in client side. That is a different large topic.
                                  
                    CryptoStream cryptoStream = new CryptoStream(networkStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    //cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
                    readFileToBeEncrypted.CopyTo(cryptoStream);

                    cryptoStream.Dispose();
                    readFileToBeEncrypted.Dispose(); ;
                    networkStream.Dispose();// Close();
                    clientEndPoint.Dispose();// Close();

                    byte[] receivedByteArray = new byte[1024];
                    clientSocketForFileServer.Receive(receivedByteArray);
                    string receivedResponse = Encoding.UTF8.GetString(receivedByteArray).Substring(0, Encoding.UTF8.GetString(receivedByteArray).IndexOf("\0"));
                    if (receivedResponse == "Upload completed")
                    {
                        serverBox.Items.Add(browsedFileName);
                        logBox.Items.Add("File uploaded: " + browsedFileName);
                    }
                    else
                    {
                        logBox.Items.Add("File couldnt be uploaded: " + browsedFileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (serverBox.SelectedIndex != -1)
                {
                    clientSocketForFileServer.Send(Encoding.UTF8.GetBytes("dwload" + serverBox.SelectedItem.ToString()));
                
                    IPAddress localAddr = IPAddress.Parse(ipBox.Text);
                    TcpListener tcpListener = new TcpListener(localAddr, 9991);

                    tcpListener.Start();
                    TcpClient clientEntPoint = tcpListener.AcceptTcpClient();
                    NetworkStream networkStream = clientEntPoint.GetStream();

                    //aes.Key = Encoding.UTF8.GetBytes("hWmZq4t6w9z$C&F)");
                    
                    CryptoStream cryptoStream = new CryptoStream(networkStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

                    FileStream fileStream = File.OpenWrite("d:\\client\\" + serverBox.SelectedItem.ToString()); 
                    cryptoStream.CopyTo(fileStream);

                    cryptoStream.Dispose();
                    fileStream.Dispose();
                    networkStream.Dispose();
                    clientEntPoint.Dispose();
                    tcpListener.Stop();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }     
        }
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (serverBox.SelectedIndex != -1)
            {
                string nameToBeDeleted;
                nameToBeDeleted = serverBox.SelectedItem.ToString();

                clientSocketForFileServer.Send(Encoding.UTF8.GetBytes("delete" + nameToBeDeleted));
                logBox.Items.Add("Deletion request sent.");
                logBox.SelectedIndex = logBox.Items.Count - 1;

                byte[] receivedByteArray = new byte[1024];
                clientSocketForFileServer.Receive(receivedByteArray);
                string receivedResponse = Encoding.UTF8.GetString(receivedByteArray).Substring(0, Encoding.UTF8.GetString(receivedByteArray).IndexOf("\0"));
                if (receivedResponse == "File deletion completed")
                {
                    serverBox.Items.RemoveAt(serverBox.SelectedIndex);
                    logBox.Items.Add("File deleted: " + nameToBeDeleted);
                }
                else
                {
                    logBox.Items.Add("File couldnt be deleted: " + nameToBeDeleted);
                }
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.ShowDialog();
                pathBox.Text = openFileDialog1.FileName;
                browsedFileName = openFileDialog1.SafeFileName;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            int headerLenght = 18;
            try
            {
                clientSocketForFileServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //tcp socket initializing
                ipAddress = IPAddress.Parse(ipBox.Text);
                remoteApplication = new IPEndPoint(ipAddress, Convert.ToInt32(portBox.Text));
                clientSocketForFileServer.Connect(remoteApplication); //client socket will connect to server socket 
                logBox.Items.Add("Connected to file server.");

                clientSocketForFileServer.Send(BitConverter.GetBytes(tokenID));
                logBox.Items.Add("Token Id recieved from authentication server,");
                logBox.Items.Add("sent to file server. Waiting for validation.");
                logBox.SelectedIndex = logBox.Items.Count - 1;

                byte[] byteMessage = new Byte[1024];

                clientSocketForFileServer.Receive(byteMessage);
                string stringMessage = Encoding.UTF8.GetString(byteMessage);
                logBox.Items.Add("Received response: " + stringMessage);
                logBox.SelectedIndex = logBox.Items.Count - 1;
                
                if (stringMessage.Substring(0, headerLenght) == "Token Id Validated")
                {
                    logBox.Items.Add("Token Id has been validated, now you have access to file server.");
                    parseNamesOfFilesStoredInServerAndUpdateUI(stringMessage, headerLenght);
                    setInterfaceToConnected();
                }
                else
                {
                    logBox.Items.Add(stringMessage);
                    MessageBox.Show("Seems Token Id rejected. Application is terminated!");
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            clientSocketForFileServer.Send(Encoding.UTF8.GetBytes("discon"));
            clientSocketForFileServer.Disconnect(false);
            setInterfaceToDisconnected();
        }

        //Parses the string contains names of files in the server and updates the files list in the UI.
        //That string was produced from data previously recived from server.
        public void parseNamesOfFilesStoredInServerAndUpdateUI(string stringMessage, int headerLenght)
        {
            serverBox.Items.Clear();
            int i = headerLenght;
            int previousBlock = headerLenght;
            while (stringMessage[i] != '\0')
            {
                if (stringMessage[i] == '#')
                {
                    serverBox.Items.Add(stringMessage.Substring(previousBlock, i - previousBlock));
                    previousBlock = i + 1;
                }
                i++;
            }
        }

        public void setInterfaceToConnected()
        {
            disconnectButton.Enabled = true;
            encryptUploadButton.Enabled = true;
            browseButton.Enabled = true;
            downloadButton.Enabled = true;
            deleteButton.Enabled = true;
            connectButton.Enabled = false;
            connectLabel.Text = "Connected";
            connectLabel.ForeColor = Color.Green;
        }

        public void setInterfaceToDisconnected()
        {
            serverBox.Items.Clear();
            disconnectButton.Enabled = false;
            encryptUploadButton.Enabled = false;
            browseButton.Enabled = false;
            downloadButton.Enabled = false;
            deleteButton.Enabled = false;
            connectButton.Enabled = true;
            connectLabel.Text = "Disconnected";
            connectLabel.ForeColor = Color.Red;
        }


    }
}
