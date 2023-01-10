using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Program{
    public static void Main(String[] args){
        SocketServer.StartServer();
        Console.Read();
    }

}


public class SocketServer
{
    private static Byte[] data;
    private static String responseData;
    private static TcpListener serverSocket;
    private static NetworkStream networkStream; 
    public static void StartServer()
    {
        IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostEntry.AddressList[0];
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 8888);
        serverSocket = new TcpListener(ipEndPoint);
        serverSocket.Start();
        Console.WriteLine("Asynchonous server socket is listening at: " + ipEndPoint.Address.ToString());
        WaitForClients();
    }

    private static void WaitForClients()
    {
        serverSocket.BeginAcceptTcpClient(new System.AsyncCallback(OnClientConnected), null);
    }

    private static void OnClientConnected(IAsyncResult asyncResult)
    {
        try
        {
            TcpClient clientSocket = serverSocket.EndAcceptTcpClient(asyncResult);
            if (clientSocket != null)
            Console.WriteLine("Received connection request from: " + clientSocket.Client.RemoteEndPoint.ToString());
            networkStream = clientSocket.GetStream();
            HandleClientRequest(clientSocket);
        }
        catch
        {
            throw;
        }
        WaitForClients();
    }

    private static void HandleClientRequest(TcpClient clientSocket)
    {
        try{
            data = new Byte[256];
            String responseData = String.Empty;
            Int32 bytes = networkStream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine("Received: {0}", responseData);
            //Write your code here to process the data
            HandleClientRequest(clientSocket);
        }catch(Exception e){
            Console.WriteLine("Closing connection due to: "+e.Message.ToString());
            clientSocket.Close();
        }


    }
}