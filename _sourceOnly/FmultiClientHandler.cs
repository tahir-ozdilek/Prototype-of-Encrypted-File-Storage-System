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
