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
