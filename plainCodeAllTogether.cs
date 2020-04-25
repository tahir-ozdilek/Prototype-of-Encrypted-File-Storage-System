using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{    public partial class LogInForm : Form
    {
        Socket clientSocketForAuthenticationServer;// = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //tcp socket initializing
        IPAddress ipAddress;// = IPAddress.Parse("127.0.0.1");
        IPEndPoint remoteApplication;// = new IPEndPoint(ipAddress, Convert.ToInt32("9999"));

        public LogInForm()
        {
            InitializeComponent();
        }
        
        // Makes borderless form movable!!!
        private const int WM_NCHITTEST = 0x84;
        private const int HT_CAPTION = 0x2;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void logInClick(object sender, EventArgs e)
        {
            logInButton.Enabled = false;
            // gui wont freeze when attempting to connect
            Thread newThread = new Thread(new ThreadStart(logIn));
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();
        }

        //LogIn is being done in a new thread, therefore INVOKEs are used to accessing main form thread.
        private void logIn()
        {
            clientSocketForAuthenticationServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //tcp socket initializing
            ipAddress = IPAddress.Parse("127.0.0.1");
            remoteApplication = new IPEndPoint(ipAddress, Convert.ToInt32("9999"));
            try
            {
                clientSocketForAuthenticationServer.Connect(remoteApplication); //client socket will connect to server socket 
            }
            catch (Exception ex)
            {
                MessageBox.Show("socket.Connect(remoteApplication). Details:\n" + ex.ToString());
            }

            try
            {
                //Assuming neither username will be allowed to end with space nor password will be allowed to start with space
                byte[] authenticationInfo = Encoding.UTF8.GetBytes("clientRequest" + userNameBox.Text + " " + passwordBox.Text);
                clientSocketForAuthenticationServer.Send(authenticationInfo);
                Invoke(new Action(() =>
                {
                    logBox.Items.Add("Authentication request sent.");
                    logBox.SelectedIndex = logBox.Items.Count - 1;
                }));

                byte[] messageReceivedByte = new Byte[1024];
                clientSocketForAuthenticationServer.Receive(messageReceivedByte);

                string messageReceivedString = Encoding.UTF8.GetString(messageReceivedByte).Substring(0, Encoding.UTF8.GetString(messageReceivedByte).IndexOf("\0"));
                if (messageReceivedString == "rejected")
                {
                    Invoke(new Action(() =>
                    {
                        logBox.Items.Add("Authentication rejected, you may try again.");
                        logInButton.Enabled = true;
                        logBox.SelectedIndex = logBox.Items.Count - 1;
                    }));
                }
                else
                {
                    Invoke(new Action(() =>
                    {
                        logBox.Items.Add("You are authenticated with token: " + BitConverter.ToInt32(messageReceivedByte, 0));
                        logBox.Items.Add("You are being forwarded to File Storage Client UI in a few seconds.");
                        logBox.SelectedIndex = logBox.Items.Count - 1;
                    }));
                   // backgroundWorkerForLogInConnection.CancelAsync();

                    //Thread.Sleep(5000);
                    
                    Invoke(new Action(() =>
                    {
                        this.Hide();
                    }));
                    Application.Run(new Client(BitConverter.ToInt32(messageReceivedByte, 0)));
                    Invoke(new Action(() =>
                    {
                        Dispose();
                    }));
                }
                clientSocketForAuthenticationServer.Disconnect(false);
                clientSocketForAuthenticationServer.Close();
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    logBox.Items.Add(ex.ToString());
                }));
                Invoke(new Action(() =>
                {
                    logInButton.Enabled = true;
                }));
            }
        }
    }
}
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AuthenticationServer
{
    public partial class AuthenticationServer : Form
    {   
        //Tuple <tokenID, userName>
        public static LinkedList<Tuple<int, string>> authenticatedUsers = new LinkedList<Tuple<int, string>>();
        static IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 9999);
        Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Socket clientSocket = default(Socket);
        BackgroundWorker backgroundWorker = new BackgroundWorker(); // gui wont freeze with this worker 

        public AuthenticationServer()
        {
            InitializeComponent();
        }

        public void startListening(object sender, DoWorkEventArgs e)
        {
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);        //Supportss 10 different clients
                Invoke(new Action(() =>
                {
                    logBox.Items.Add("Listening all IP's at 9999 port");
                    logBox.SelectedIndex = logBox.Items.Count - 1;
                }));

                while (true)   //we wait for a connection
                {
                    clientSocket = listener.Accept();

                    new AmultiClientHandler(clientSocket, this);
                } 
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("Exception when trying to start listening:");
                    MessageBox.Show(ex.ToString());
                }));
            }
            clientSocket.Close();
            listener.Close();
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            backgroundWorker.DoWork += new DoWorkEventHandler(startListening);
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.RunWorkerAsync();
            runButton.Enabled = false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthenticationServer
{
    public class AmultiClientHandler
    {
        BackgroundWorker backgroundWorker = new BackgroundWorker(); // gui wont freeze with this worker 
        AuthenticationServer authenticationInstance;
        Socket seperateSocketForEachRequest;
        public AmultiClientHandler(Socket clientSocket, AuthenticationServer authentication)
        {
            this.seperateSocketForEachRequest = clientSocket;
            this.authenticationInstance = authentication;
            backgroundWorker.DoWork += new DoWorkEventHandler(multipleAnswer);   //backgroundworker will use listensocket method
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.RunWorkerAsync();
        }

        public void multipleAnswer(object sender, DoWorkEventArgs e)
        {
            byte[] byteMessage = new Byte[1024];

            try
            {
                seperateSocketForEachRequest.Receive(byteMessage);
            }
            catch (Exception ex)
            {
                insertTextToLogBox("Error when receiving data. Exception details:" + ex.ToString());
            }

            string stringMessage = Encoding.UTF8.GetString(byteMessage);
            insertTextToLogBox("Received: " + stringMessage);
            int messageHeaderEnd = 13;
            string messageHeader = stringMessage.Substring(0, messageHeaderEnd);

            switch (messageHeader)
            {
                case "clientRequest":
                    int userNameEndIndex = stringMessage.IndexOf(" ");

                    string requestedUserName = stringMessage.Substring(messageHeaderEnd, userNameEndIndex - messageHeaderEnd);
                    string requestedPassword = stringMessage.Substring(userNameEndIndex + 1, stringMessage.IndexOf("\0") - userNameEndIndex - 1);

                    StreamReader file = new StreamReader("database.txt");
                    string randomDataLine = "";
                    bool isAuthenticated = false;
                    try
                    {
                        while (randomDataLine != null)
                        {
                            randomDataLine = file.ReadLine();
                            HashAlgorithm hashAlgorithm = SHA256.Create();
                            byte[] userNameHash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(randomDataLine + requestedUserName));

                            string readUserNameHashLine = file.ReadLine();
                            byte[] storedUserNameHash = Encoding.UTF8.GetBytes(readUserNameHashLine);

                            //Converts Binary Hash to String
                            StringBuilder buildStringUserName = new StringBuilder();
                            foreach (Byte b in userNameHash)
                                buildStringUserName.Append(b.ToString("x2"));
                            string userNameHashString = buildStringUserName.ToString();

                            if (userNameHashString == readUserNameHashLine)
                            {
                                byte[] passwordHash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(randomDataLine + requestedPassword));

                                StringBuilder buildStringPass = new StringBuilder();
                                foreach (Byte b in passwordHash)
                                    buildStringPass.Append(b.ToString("x2"));
                                string passHashString = buildStringPass.ToString();
                                
                                if (file.ReadLine() == passHashString)
                                {
                                    isAuthenticated = true;
                                    insertTextToLogBox("Username and password confirmed for: " + requestedUserName);

                                    Random randomGenerator = new Random();
                                    int tokenID = randomGenerator.Next(100000, 999999);

                                    AuthenticationServer.authenticatedUsers.AddLast(new Tuple<int, string>(tokenID, requestedUserName));
                                    insertTextToLogBox("Token ID ' " + tokenID + " ' stored");

                                    seperateSocketForEachRequest.Send(BitConverter.GetBytes(tokenID));
                                    insertTextToLogBox("Token ID ' " + tokenID + " ' sent to client.");
                                    break;
                                }
                            }
                            file.ReadLine();
                        }
                        file.Close();
                    }
                    catch (Exception er)
                    {
                        insertTextToLogBox("Error" + er.Message);
                    }

                    if (!isAuthenticated)
                    {
                        insertTextToLogBox("Authentication rejected for: " + requestedUserName);

                        seperateSocketForEachRequest.Send(Encoding.UTF8.GetBytes("rejected"));
                        insertTextToLogBox("Authentication rejection for " + requestedUserName + " user replied.");
                    }
                    break;
                case "serverRequest":
                    bool isAuthenticatedServer = false;
                    string userNameToSentToServer="";
                    foreach (var token in AuthenticationServer.authenticatedUsers)
                    {
                        if (token.Item1.Equals(BitConverter.ToInt32((byteMessage.Skip(messageHeaderEnd).Take(17).ToArray()), 0)))
                        {
                            isAuthenticatedServer = true;
                            userNameToSentToServer = token.Item2;
                        }
                    }
                    if (isAuthenticatedServer) //AuthenticationServer.authenticatedUsers.Contains(new Tuple<int, string>(, 0), ))) 
                    {
                        seperateSocketForEachRequest.Send(Encoding.UTF8.GetBytes("Validated" + userNameToSentToServer));
                        insertTextToLogBox("Validation message sent ot File Server for: " + (BitConverter.ToInt32((byteMessage.Skip(messageHeaderEnd).Take(17).ToArray()), 0)).ToString() + "  Token Id");
                    }
                    else
                    {
                        seperateSocketForEachRequest.Send(Encoding.UTF8.GetBytes("Rejected"));
                        insertTextToLogBox("Rejection message sent ot File Server for:" + (BitConverter.ToInt32((byteMessage.Skip(messageHeaderEnd).Take(17).ToArray()), 0)).ToString() + "  Token Id");
                    }

                    break;
                default:
                    insertTextToLogBox("Invalid request. WHO THE HELL ARE YOU? Rejected!");
                    break;
            }

            seperateSocketForEachRequest.Disconnect(true);
            seperateSocketForEachRequest.Close();
        }

        public void insertTextToLogBox(string message)
        {
            authenticationInstance.Invoke(new Action(() =>
            {
                authenticationInstance.logBox.Items.Add(message);
                authenticationInstance.logBox.SelectedIndex = authenticationInstance.logBox.Items.Count - 1;
            }));
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileServer
{
    public partial class FileServer : Form
    {
        public LinkedList<int> authenticatedUsers = new LinkedList<int>();
        static IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 9998);
        Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Socket clientSocket = default(Socket);
        BackgroundWorker backgroundWorker = new BackgroundWorker(); // gui wont freeze with this worker 
        public FileServer()
        {
            InitializeComponent();
        }

        public void runFileServer(object sender, DoWorkEventArgs e)
        {
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);        //Supportss 10 different clients
                Invoke(new Action(() =>
                {
                    logBox.Items.Add("Listening all IP's at 9998 port");
                    logBox.SelectedIndex = logBox.Items.Count - 1;
                }));

                while (true)   //we wait for a connection
                {
                    clientSocket = listener.Accept();
                    new FmultiClientHandler(clientSocket, this);
                }
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("Exception when trying to start listening:");
                    MessageBox.Show(ex.ToString());
                }));
            }
            clientSocket.Close();
            listener.Close();
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            backgroundWorker.DoWork += new DoWorkEventHandler(runFileServer);
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.RunWorkerAsync();
            runButton.Enabled = false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileServer
{
    public class FmultiClientHandler
    {
        BackgroundWorker backgroundWorker = new BackgroundWorker(); // gui wont freeze with this worker 
        FileServer fileHandlerInstance;
        Socket seperateSocketForEachRequest;
        string curretClientUserName;

        public FmultiClientHandler(Socket clientSocket, FileServer fileHandler)
        {
            this.seperateSocketForEachRequest = clientSocket;
            this.fileHandlerInstance = fileHandler;
            backgroundWorker.DoWork += new DoWorkEventHandler(multipleAnswer);   //backgroundworker will use listensocket method
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.RunWorkerAsync();
        }

        public void multipleAnswer(object sender, DoWorkEventArgs e)
        {
            byte[] byteMessage = new Byte[1024];

            try
            {
                seperateSocketForEachRequest.Receive(byteMessage);
            }
            catch (Exception ex)
            {
                insertTextToLogBox("Error when receiving data. Exception details:" + ex.ToString());
            }

            insertTextToLogBox("Received: " + (BitConverter.ToInt32(byteMessage, 0)).ToString());

            bool isAuthenticated = validateRequestedTokenFromAuthenticationServer(byteMessage);

            if (isAuthenticated)
            {
                try
                {                   
                    seperateSocketForEachRequest.Send(Encoding.UTF8.GetBytes("Token Id Validated" + getNameOfFilesStoredInServer())); //string.Join("#", Directory.GetFiles(curretClientUserName)) + "#"));
                    
                    insertTextToLogBox("Validation confirmation sent to client and he can see his library now.");
                }
                catch(Exception ex)
                {
                        MessageBox.Show(ex.ToString());
                }

                bool disconnectRequest = false;
                //Reply client's requests until it disconnects.
                while (!disconnectRequest)
                {
                    byte[] loopRequest = new byte[1024];
                    try
                    {
                        seperateSocketForEachRequest.Receive(loopRequest);
                    }
                    catch (Exception ex)
                    {
                        insertTextToLogBox("Error when receiving data. Exception details:" + ex.ToString());
                    }

                    int requestMessageHeaderLenght = 6;
                    string userRequest = Encoding.UTF8.GetString(loopRequest).Substring(0, requestMessageHeaderLenght);

                    switch (userRequest)
                    {
                        case "discon":
                            disconnectRequest = true;
                            break;
                        case "upload":
                            try
                            {
                                string nameItemToBeUploaded = Encoding.UTF8.GetString(loopRequest).Substring(requestMessageHeaderLenght, Encoding.UTF8.GetString(loopRequest).IndexOf("\0") - requestMessageHeaderLenght);
                                
                                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                                TcpListener tcpListener = new TcpListener(localAddr, 9990);

                                tcpListener.Start();
                                TcpClient clientEntPoint = tcpListener.AcceptTcpClient();
                                NetworkStream networkStreaam = clientEntPoint.GetStream();

                                FileStream streamSavesEncryptedFile = File.OpenWrite(curretClientUserName + "\\" + nameItemToBeUploaded);

                                networkStreaam.CopyTo(streamSavesEncryptedFile);
                                streamSavesEncryptedFile.Dispose();
                                tcpListener.Stop();

                                seperateSocketForEachRequest.Send(Encoding.UTF8.GetBytes("Upload completed"));
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                            break;
                        case "dwload":
                            try
                            {
                                string nameItemToBeDownloaded= Encoding.UTF8.GetString(loopRequest).Substring(requestMessageHeaderLenght, Encoding.UTF8.GetString(loopRequest).IndexOf("\0") - requestMessageHeaderLenght);

                                TcpClient clientEndPoint = new TcpClient("127.0.0.1", 9991);
                                NetworkStream networkStream = clientEndPoint.GetStream();

                                FileStream streamSavesEncryptedFile = File.OpenRead(curretClientUserName + "\\" + nameItemToBeDownloaded);

                                streamSavesEncryptedFile.CopyTo(networkStream);
                                streamSavesEncryptedFile.Dispose();
                                networkStream.Dispose();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                            break;

                        case "delete":
                            string nameItemToBeDeleted = Encoding.UTF8.GetString(loopRequest).Substring(requestMessageHeaderLenght, Encoding.UTF8.GetString(loopRequest).IndexOf("\0") - requestMessageHeaderLenght); //(Encoding.UTF8.GetString(itemToBeDeleted)).Substring(0, Encoding.UTF8.GetString(itemToBeDeleted).IndexOf("\0"));
                            loopRequest.Initialize();
                            if (File.Exists(Path.Combine(@curretClientUserName, nameItemToBeDeleted)))
                            {
                                // If file found, delete it    
                                try
                                {
                                    File.Delete(Path.Combine(@curretClientUserName, nameItemToBeDeleted));
                                    insertTextToLogBox("File has been deleted.");

                                }
                                catch (Exception ex)
                                {
                                    insertTextToLogBox("File couldnt be deleted." + ex.ToString());
                                }
                                seperateSocketForEachRequest.Send(Encoding.UTF8.GetBytes("File deletion completed"));
                                insertTextToLogBox("File deletion confirmation sent to client.");
                            }
                            else
                            {
                                seperateSocketForEachRequest.Send(Encoding.UTF8.GetBytes("File couldnt be deleted."));
                                insertTextToLogBox("File couldnt be deleted.");
                            }
                            break;
                        default:
                            insertTextToLogBox("Invalid request?");
                            break;
                    }
                }
            }
            else
            {
                seperateSocketForEachRequest.Send(Encoding.UTF8.GetBytes("Token Id Rejected. Connection is terminated."));
            }

            seperateSocketForEachRequest.Disconnect(true);
            seperateSocketForEachRequest.Dispose();
        }

        public bool validateRequestedTokenFromAuthenticationServer(byte[] byteMessage)
        {
            try
            {
                Socket clientTypeSocketForTokenIdValidation = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //tcp socket initializing
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint remoteApplication = new IPEndPoint(ipAddress, 9999);
                clientTypeSocketForTokenIdValidation.Connect(remoteApplication); //client socket will connect to server socket 
                int packetHeaderLength = 9;
                insertTextToLogBox("Connected to authentication server.");

                clientTypeSocketForTokenIdValidation.Send((Encoding.UTF8.GetBytes("serverRequest")).Concat(byteMessage.Take(4)).ToArray());
                insertTextToLogBox("Recieved Token Id sent to file server. Waiting for validation.");

                byte[] recievedMessage = new Byte[1024];
                clientTypeSocketForTokenIdValidation.Receive(recievedMessage);
                string stringMessage = Encoding.UTF8.GetString(recievedMessage);
                insertTextToLogBox("Recieved response from authentication server: " + stringMessage);

                if (stringMessage.Substring(0, packetHeaderLength)  == "Validated")
                {
                    curretClientUserName = stringMessage.Substring(packetHeaderLength, stringMessage.IndexOf("\0") - packetHeaderLength);
                    return true;
                }     
            }
            catch (Exception ex)
            {
                insertTextToLogBox(ex.ToString());
            }
            return false;
        }

        public string getNameOfFilesStoredInServer()
        {
            var fileNamesEIString = Directory.EnumerateFiles(curretClientUserName, "*", SearchOption.AllDirectories).Select(Path.GetFileName);
            List<string> fileNamesList = new List<string>();
            fileNamesList = fileNamesEIString.ToList();
            string concatenatedFilesNames = "";

            foreach (string item in fileNamesList)
            {
                concatenatedFilesNames += (item + "#");
            }

            return concatenatedFilesNames;
        }
        
        public void insertTextToLogBox(string message)
        {
            fileHandlerInstance.Invoke(new Action(() =>
            {
                fileHandlerInstance.logBox.Items.Add(message);
                fileHandlerInstance.logBox.SelectedIndex = fileHandlerInstance.logBox.Items.Count - 1;
            }));
        }
    }
}
