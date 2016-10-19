using chatcommon.Classes;
using chatcommon.Entities;
using chatcommon.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace chatroomconsole.Classes
{
    class Server
    {
        IBusinessLogic BL;
        public chatcommon.Classes.NotifyTaskCompletion<List<User>> userAuthenticationTask;
        //public static Hashtable clientsList = new Hashtable();
        public static Dictionary<string, TcpClient> clientsList = new Dictionary<string, TcpClient>();
        public Server(IBusinessLogic Bl)
        {
            BL = Bl;
            start();
            //userAuthenticationTask = new NotifyTaskCompletion<List<User>>();
            //userAuthenticationTask.PropertyChanged += onUserAuthenticationTaskCompletion;
            //userAuthenticationTask.initializeNewTask(Bl.BLSecurity.AuthenticateUser("Test225", "Test", false));
            //userAuthenticationTask.initializeNewTask(Bl.BLUser.InsertUser(new List<User> { new User { LastName = "test from app", Username = "test", Password = "test" } }));
            //userAuthenticationTask.initializeNewTask(Bl.BLUser.GetUserData(999));
            //var UserFound = new Nor Bl.BLSecurity.AuthenticateUser("Test225", "Test");
        }

        private void onUserAuthenticationTaskCompletion(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsSuccessfullyCompleted"))
            {
                var userFound = userAuthenticationTask.Result;
            }
            else if (e.PropertyName.Equals("IsFaulted"))
            {

            }
        }

        private void start()
        {
            int port = 0;
            int.TryParse(ConfigurationManager.AppSettings["Port"], out port);
            System.Net.IPAddress ipAddress = default(System.Net.IPAddress);
            System.Net.IPAddress.TryParse(ConfigurationManager.AppSettings["IP"], out ipAddress);
            System.Net.IPEndPoint socket = new System.Net.IPEndPoint(ipAddress, port);
            TcpListener serverSocket = new TcpListener(socket);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine("Chat Server Started ....");
            counter = 0;
            //try
            //{
            while ((true))
            {
                int discussionId = 0;
                int userId = 0;
                int messageId = 0;
                string messageHeader = "";
                string dataFromClient = "";
                List<string> composer = new List<string>();
                handleClient client = default(handleClient);
                counter += 1;

                try
                {
                    clientSocket = serverSocket.AcceptTcpClient();
                    byte[] bytesFrom = new byte[(int)clientSocket.ReceiveBufferSize];
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                }
                catch (Exception ex)
                {
                    Log.error(ex.Message);
                }

                composer = dataFromClient.Split('/').ToList();
                if (composer.Count > 2
                        && int.TryParse(composer[0], out discussionId)
                            && int.TryParse(composer[1], out userId)
                                && int.TryParse(composer[2], out messageId))
                {
                    var messageFoundList = new NotifyTaskCompletion<List<Message>>(BL.BLMessage.GetMessageDataById(messageId)).Task.Result;
                    var userFoundList = new NotifyTaskCompletion<List<User>>(BL.BLUser.GetUserDataById(userId)).Task.Result;
                    if (messageFoundList.Count > 0 && userFoundList.Count > 0)
                    {
                        messageHeader = messageFoundList[0].DiscussionId + "/" + messageFoundList[0].UserId + "/" + messageFoundList[0].ID + "/";
                        clientsList.Add(messageHeader, clientSocket);
                        broadcast(messageHeader + " Joined ");
                        Console.WriteLine(userFoundList[0].Username + " as joined and says: " + composer[3]);
                        client = new handleClient();
                        client.startClient(clientSocket, messageHeader, userFoundList[0].Username, clientsList);
                    }
                }
            }
            /*}
            catch (Exception ex)
            {
                Log.error(ex.Message);
            }
            finally
            {
                clientSocket.Close();
                serverSocket.Stop();
                Console.WriteLine("exit");
                Console.ReadLine();
            }*/
        }

        public static void broadcast(string msg)
        {
            try
            {
                var clientsByDiscussionList = clientsList.Where(x => x.Key.Split('/')[0] == msg.Split('/')[0]).Select(x => x.Value).ToList();
                foreach (TcpClient tcpClient in clientsByDiscussionList)
                {
                    TcpClient broadcastSocket;
                    broadcastSocket = tcpClient;
                    if (broadcastSocket.Connected)
                    {
                        NetworkStream broadcastStream = broadcastSocket.GetStream();
                        Byte[] broadcastBytes = null;

                        broadcastBytes = Encoding.ASCII.GetBytes(msg + "$");

                        broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                        broadcastStream.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.error(ex.Message);
            }
        }  //end broadcast function



    }
}
