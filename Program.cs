using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Program{
    static int counter = 0;

    public static void Main(String[] args){
        // SocketServer.StartServer();
        TcpListener serverSocket = new TcpListener(8888);

        TcpClient clientSocket = default(TcpClient);

        serverSocket.Start();
        Console.WriteLine(" >> " + "Server Started"); 

        counter = 0;
        while (true)
        {
            counter += 1;
            clientSocket = serverSocket.AcceptTcpClient();
            Console.WriteLine(" >> " + "Client No:" + Convert.ToString(counter) + " started!");
            handleClinet client = new handleClinet();
            client.startClient(clientSocket, Convert.ToString(counter));
        }

        clientSocket.Close();
        serverSocket.Stop();
        Console.WriteLine(" >> " + "exit");
        Console.ReadLine();
    }




    

    //Class to handle each client request separatly
    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
        public void startClient(TcpClient inClientSocket, string clineNo)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }
        private void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[256];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;

            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();

                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);


                    Console.WriteLine(" >> " + "From client-" + clNo + dataFromClient);

                    rCount = Convert.ToString(requestCount);
                    serverResponse = "Server to clinet(" + clNo + ") " + rCount;
                    sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();
                    Console.WriteLine(" >> " + serverResponse);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                    counter--;
                    clientSocket.Close();
                    break;


                }
            }
        }
    
    }
}
// public class SocketServer
// {
//     private static Byte[] data;
//     private static String responseData;
//     private static TcpListener serverSocket;
//     private static NetworkStream networkStream; 
//     public static void StartServer()
//     {
//         IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 8888);
//         serverSocket = new TcpListener(ipEndPoint);
//         serverSocket.Start();
//         Console.WriteLine("Asynchonous server socket is listening at: " + ipEndPoint.Address.ToString());
//         WaitForClients();
//     }

//     private static void WaitForClients()
//     {
//         serverSocket.BeginAcceptTcpClient();
//         if(serverSocket.Connected){
//             Thread newThread = new Thread(new ThreadStart(WaitForClients));
//             newThread.Start();
//         }

//     }

//     private static void OnClientConnected(IAsyncResult asyncResult)
//     {
//         try
//         {
//             TcpClient clientSocket = serverSocket.EndAcceptTcpClient(asyncResult);
//             if (clientSocket != null)
//             Console.WriteLine("Received connection request from: " + clientSocket.Client.RemoteEndPoint.ToString());
//             networkStream = clientSocket.GetStream();
//             HandleClientRequest(clientSocket);
//         }
//         catch
//         {
//             throw;
//         }
//         WaitForClients();
//     }

//     private static async void HandleClientRequest(TcpClient clientSocket)
//     {
//         try{
//             data = new Byte[256];
//             String responseData = String.Empty;
//             Int32 bytes = networkStream.Read(data, 0, data.Length);
//             responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
//             Console.WriteLine("Received: {0}", responseData);
//             if (responseData[0] == '|'){
//                 data = System.Text.Encoding.ASCII.GetBytes("Recieved");
//                 networkStream.Write(data,0,data.Length);
//             }
//             HandleClientRequest(clientSocket);
//         }catch(Exception e){
//             Console.WriteLine("Closing connection due to: "+e.Message.ToString());
//             clientSocket.Close();
//         }

        
//     }
// }



