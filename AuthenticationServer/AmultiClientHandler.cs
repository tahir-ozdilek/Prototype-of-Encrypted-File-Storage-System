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
