using chatcommon.Classes;
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
        Dictionary<string, TcpClient> clientsList;
        string msgHeader;

        public handleClient()
        {
            msgHeader = "";
        }

        public void startClient(TcpClient inClientSocket, string clineNo, Dictionary<string, TcpClient> cList)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            this.clientsList = cList;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        internal void startClient(TcpClient clientSocket, string messageHeader, string username, Dictionary<string, TcpClient> clientsList)
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
                    Console.WriteLine("From client - " + clNo + " : " + dataFromClient.Split('/')[3]);

                    Server.broadcast(dataFromClient);
                }
                catch (Exception ex)
                {
                    Log.error(ex.ToString());
                    break;
                }
            }//end while
        }//end doChat
    }
}
