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
