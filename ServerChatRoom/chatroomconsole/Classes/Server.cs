using chatcommon.Classes;
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
        public static Dictionary<TcpClient, string> clientsList = new Dictionary<TcpClient, string>();
        public Server()
        {
            start();
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
            try
            {
                while ((true))
                {
                    int discussionId = 0;
                    int userId = 0;
                    int messageId = 0;
                    string dataFromClient = "";
                    List<string> composer = new List<string>();
                    handleClient client = default(handleClient);

                    try
                    {
                        clientSocket = serverSocket.AcceptTcpClient();
                        byte[] bytesFrom = new byte[(int)clientSocket.ReceiveBufferSize];
                        NetworkStream networkStream = clientSocket.GetStream();
                        networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                        dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                        dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                        // checking if user already connected (same user id) 
                        var clientsToUpdate = clientsList.Where(x => x.Value.Split('/')[1] == dataFromClient.Split('/')[1]).Select(x => x.Key).ToList();
                        if (clientsToUpdate.Count() > 0)
                            clientsList[clientsToUpdate[0]] = dataFromClient;
                        else
                            clientsList.Add(clientSocket, dataFromClient);
                    }
                    catch (Exception ex)
                    {
                        Log.error(ex.Message);
                    }

                    composer = dataFromClient.Split('/').ToList();
                    if (composer.Count > 2)
                    {
                        if (int.TryParse(composer[0], out discussionId)
                              && int.TryParse(composer[1], out userId)
                                  && int.TryParse(composer[2], out messageId)
                                      && discussionId > 0)
                        {
                            broadcast(dataFromClient);
                            Console.WriteLine("User(id = " + userId + ")" + " says: " + composer[3]);
                        }

                        // if first log in or log out.
                        else if (discussionId < 0)
                            broadcast(dataFromClient, flag: true);
                        
                        client = new handleClient();
                        client.startClient(clientSocket, dataFromClient, "User(id = " + userId + ")", clientsList);
                    }
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


        public static void broadcast(string msg, bool flag = false)
        {
            try
            {
                if (flag)
                {
                    Console.WriteLine("User(id = " + msg.Split('/')[1] + ")" + " has joined!");
                }
                var clientsByDiscussionList = clientsList
                                                    .Where(x => x.Value.Split('/')[0] == msg.Split('/')[0] || msg.Split('/')[3].Split('|').Contains(x.Value.Split('/')[1]))
                                                        .Select(x => x.Key).ToList();
                foreach (TcpClient tcpClient in clientsByDiscussionList)
                {
                    send(tcpClient, msg);
                }
            }
            catch (Exception ex)
            {
                Log.error(ex.Message);
            }
        }  //end broadcast function

        private static void send(TcpClient tcpClient, string msg)
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
}
