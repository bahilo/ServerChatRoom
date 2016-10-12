using chatcommon.Classes;
using chatcommon.Entities;
using chatcommon.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static Hashtable clientsList = new Hashtable();
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
            System.Net.IPAddress ipAddress = default(System.Net.IPAddress);
            System.Net.IPAddress.TryParse("127.0.0.1", out ipAddress);
            System.Net.IPEndPoint socket = new System.Net.IPEndPoint(ipAddress, 8888);
            TcpListener serverSocket = new TcpListener(socket);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine("Chat Server Started ....");
            counter = 0;
            try
            {
                while ((true))
                {
                    counter += 1;
                    clientSocket = serverSocket.AcceptTcpClient();

                    byte[] bytesFrom = new byte[(int)clientSocket.ReceiveBufferSize];
                    string dataFromClient = null;

                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                    if (clientsList.ContainsKey(dataFromClient))
                        clientsList.Remove(dataFromClient);
                    
                        clientsList.Add(dataFromClient, clientSocket);

                    broadcast(dataFromClient + " Joined ", dataFromClient, false);

                    Console.WriteLine(dataFromClient + " Joined chat room ");
                    handleClient client = new handleClient();
                    client.startClient(clientSocket, dataFromClient, clientsList);
                }
            }
            finally
            {
                clientSocket.Close();
                serverSocket.Stop();
                Console.WriteLine("exit");
                Console.ReadLine();
            }
        }

        public static void broadcast(string msg, string uName, bool flag)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                TcpClient broadcastSocket;
                broadcastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
                Byte[] broadcastBytes = null;

                if (flag == true)
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(uName + " says : " + msg + "$");
                }
                else
                {
                    broadcastBytes = Encoding.ASCII.GetBytes(msg + "$");
                }

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }  //end broadcast function
               


    }
}
