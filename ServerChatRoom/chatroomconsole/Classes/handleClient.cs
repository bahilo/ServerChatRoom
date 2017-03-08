using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace chatroomconsole.Classes
{
    class handleClient
    {
        TcpClient clientSocket;
        string clNo;
        Dictionary<TcpClient, string> clientsList;
        string msgHeader;

        public handleClient()
        {
            msgHeader = "";
        }

        public void startClient(TcpClient inClientSocket, string clineNo, Dictionary<TcpClient, string> cList)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            this.clientsList = cList;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        internal void startClient(TcpClient clientSocket, string messageHeader, string username, Dictionary<TcpClient, string> clientsList)
        {
            msgHeader = messageHeader;
            this.startClient(clientSocket, username, clientsList);
        }

        private void doChat()
        {
            byte[] bytesFrom = new byte[(int)clientSocket.ReceiveBufferSize];
            string dataFromClient = null;

            while ((true))
            {
                try
                {
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    if (dataFromClient.Split('/')[0] == "-1")
                        throw new ApplicationException("Exit application.");
                    var clientsToUpdate = clientsList.Where(x => x.Value.Split('/')[1] == dataFromClient.Split('/')[1]).Select(x => x.Key).ToList();
                    if (clientsToUpdate.Count() > 0)
                        clientsList[clientsToUpdate[0]] = dataFromClient;
                    Console.WriteLine("From client - " + clNo + " : " + dataFromClient.Split('/')[4]);

                    Server.broadcast(dataFromClient);
                }
                catch (Exception)
                {
                    if (clientsList.Keys.Contains(clientSocket))
                    {
                        Console.WriteLine("User(id = " + clientsList[clientSocket].Split('/')[1] + ")" + " has exited!");
                        Server.broadcast(dataFromClient);
                        clientsList.Remove(clientSocket);
                        break;
                    }                    
                }
            }//end while
        }//end doChat
    }
}
