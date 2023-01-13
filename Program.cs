using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
// database
using MySql.Data;
using MySql.Data.MySqlClient;
// JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class Program{
    static int counter = 0;    
    public class DBConnection
    {
        private DBConnection()
        {
        }

        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public MySqlConnection Connection { get; set;}

        public static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
           return _instance;
        }
    
        public bool IsConnect()
        {
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(DatabaseName))
                    return false;
                string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", Server, DatabaseName, UserName, Password);
                Connection = new MySqlConnection(connstring);
                Connection.Open();
            }
    
            return true;
        }
    
        public void Close()
        {
            Connection.Close();
        }        
    }
    

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
    /////////////////////////////        DATABASE CONNECTION CLASS            ////////////////////////////////////////////////////////
    public class DatabaseControl{
        private static DBConnection dbCon;

        public DatabaseControl(){
            dbCon = DBConnection.Instance();
            dbCon.Server = "localhost";
            dbCon.DatabaseName = "Share_and_Spread";
            dbCon.UserName = "root";
            dbCon.Password = "";
            if (dbCon.IsConnect()){
                Console.WriteLine(">> database connected");
            } else{
                Console.WriteLine(">> Failed to connect to database");
            }
        }

        public List<List<String>> SendQuery(string query){
            var cmd = new MySqlCommand(query, dbCon.Connection);
            List<List<String>> FullMessage = new List<List<String>>();
            
            int count = 0;
            try{
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        FullMessage.Add(new List<String>());
                        string someStringFromColumnZero = reader.GetString(0);
                        Console.WriteLine("|||"+someStringFromColumnZero);
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.WriteLine("----"+reader.GetValue(i).ToString());
                            FullMessage[count].Add(reader.GetValue(i).ToString());

                        }

                        count++;
                    }
                }
                
            }catch(Exception e){
                FullMessage.Clear();
                FullMessage.Add(new List<String>());

                Console.WriteLine(e.Message);
                FullMessage[0].Add("Failed-Exception");

            }

            try{
                FullMessage[0][0].Count();
            }
            catch(Exception){
                FullMessage.Add(new List<String>());
                FullMessage[0].Add("");
            }
            
            return FullMessage;

        }
        public void Close(){
            dbCon.Close();
        }
        public string FormatData(List<List<String>> DeList){
            String Response_Data = "";
            for (int i = 0 ; i< DeList.Count; i++){
                // Console.WriteLine("First Loop-" + i);

                for (int a = 0 ; a< DeList[0].Count; a++){
                    // Console.WriteLine("Second Loop-" + a);

                    Response_Data += DeList[i][a];
                    if(a != DeList[0].Count-1){
                        Response_Data+=";;;";
                    }
                }
                if(i != DeList.Count-1){
                        Response_Data+="|BRK|";
                }
            }
            return Response_Data;
        }

    } 
    /////////////////////////////////////////////////////////////////////////
    public static class StaticRandom
    {
        private static int seed;

        private static ThreadLocal<Random> threadLocal = new ThreadLocal<Random>
            (() => new Random(Interlocked.Increment(ref seed)));

        static StaticRandom()
        {
            seed = Environment.TickCount;
        }

        public static Random Instance { get { return threadLocal.Value; } }
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

            JObject json;
            string Database_Response_Data = null;

            while ((true))
            {
                DatabaseControl DB = new DatabaseControl();
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    bytesFrom = new byte[256];
                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    Database_Response_Data = "";
                    Console.WriteLine(" >> " + "From client-" + clNo + dataFromClient);
                    Console.WriteLine(" >> SPACER");
                    serverResponse = "";

                    // me stafff Innit yeeee :D

                    // json.RemoveAll();
                    if (dataFromClient.Split('|')[0] == "Send"){
                        Random rnd = new Random();
                        
                        int rnd_value = StaticRandom.Instance.Next(1, 90000);

                        int rnd_value_2 = StaticRandom.Instance.Next(1, 90000);
                        string query = "SELECT UserID FROM users WHERE UserID = "+rnd_value;
                        // CHECK FOR DUPLICATES
                        try{
                            while(DB.SendQuery(query)[0][0] != ""){
                                Console.WriteLine(" >>Remaking user ID");
                                rnd_value = StaticRandom.Instance.Next(1, 90000);
                            }
                        }catch(Exception){}

                        query = "SELECT ComplaintID FROM complaints WHERE ComplaintID = "+rnd_value_2;
                        try{
                            while(DB.SendQuery(query)[0][0] != ""){
                                Console.WriteLine(" >>Remaking complaint ID");
                                rnd_value_2 = StaticRandom.Instance.Next(1, 90000);
                            }
                        }catch(Exception){}

                        // END


                        json = JObject.Parse(dataFromClient.Split('|')[1]);

                        query = "INSERT INTO users VALUES ("+rnd_value.ToString() +","+'\u0022'+json["Fullname"]+'\u0022'+","+'\u0022'+json["Email"]+'\u0022'+","+'\u0022'+json["Password"]+'\u0022'+","+'\u0022'+json["Phonenumber"]+'\u0022'+","+'\u0022'+json["Address"]+'\u0022'+")";
                        try{
                            Database_Response_Data = DB.SendQuery(query)[0][0];
                        }catch(Exception){Database_Response_Data="";}

                        serverResponse = Database_Response_Data;
                        query = "INSERT INTO complaints VALUES ("+rnd_value_2.ToString() +","+'\u0022'+rnd_value.ToString()+'\u0022'+","+'\u0022'+json["Title"]+'\u0022'+','+'\u0022'+json["Complaint"]+'\u0022'+","+'\u0022'+'\u0022'+")";
                        try{
                            Database_Response_Data = DB.SendQuery(query)[0][0];
                        }catch(Exception){Database_Response_Data="";}

                        serverResponse += "|"+Database_Response_Data;
                        if((serverResponse.Split('|')[0] != "Failed-Exception") || (serverResponse.Split('|')[1] != "Failed-Exception")){
                            serverResponse = "Complaint has been processed!";
                        }else{
                            serverResponse = "Complaint process was unsuccesfull and thrown an error (╥︣﹏᷅╥)!";
                        }
                        sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);
                        networkStream.Flush();
                        Console.WriteLine(" >> " + serverResponse);

                    }else if(dataFromClient.Split('|')[0] == "Login"){
                        ////////////////////// LOGIN ///////////////////////////////////////////////////////////////////////////////////////
                        json = JObject.Parse(dataFromClient.Split('|')[1]);

                        string query = "SELECT UserID FROM users WHERE Email = "+'\u0022'+json["Email"]+'\u0022'+" AND Password = "+'\u0022'+json["Password"]+'\u0022';
                        List<List<String>> TEMPList = DB.SendQuery(query);
                        Console.WriteLine(TEMPList[0].Count);
                        if(TEMPList[0].Count == 1){
                            Console.WriteLine(Database_Response_Data);
                            sendBytes = Encoding.ASCII.GetBytes("Logged in!|"+TEMPList[0][0]);
                            networkStream.Write(sendBytes, 0, sendBytes.Length);
                            networkStream.Flush();
                            Console.WriteLine(" >> " + Database_Response_Data);
                        }else{
                            sendBytes = Encoding.ASCII.GetBytes("No user found!");
                            networkStream.Write(sendBytes, 0, sendBytes.Length);
                            networkStream.Flush();
                            Console.WriteLine(" >> " + "No user found!");
                        }
                    }else if (dataFromClient.Split('|')[0] == "Retrieve"){
                        ////////////////////// RETRIEVING COMPLAINTS ///////////////////////////////////////////////////////////////////////////////////////
                        string query = "SELECT IsAdmin FROM users WHERE UserID = "+'\u0022'+dataFromClient.Split('|')[1]+'\u0022';
                        if(DB.SendQuery(query)[0][0] == "True"){
                            query = "SELECT Title, ComplaintID FROM complaints";
                        }else{
                            query = "SELECT Title, ComplaintID FROM complaints WHERE UserID = "+'\u0022'+dataFromClient.Split('|')[1]+'\u0022';
                        }

                        Console.WriteLine(" -- query - "+query);
                        // Console.WriteLine(Database_Response_Data);
                        Database_Response_Data = DB.FormatData(DB.SendQuery(query));

                        
                        if(Database_Response_Data == "" || Database_Response_Data ==  null){
                            Database_Response_Data = "Found nothing!";
                        }
                        sendBytes = Encoding.ASCII.GetBytes(Database_Response_Data);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);
                        networkStream.Flush();
                        Console.WriteLine(" >> " + Database_Response_Data);



                    }else if (dataFromClient.Split('|')[0] == "ViewComplaint"){
                        string query = "SELECT Messages FROM complaints WHERE ComplaintID = "+'\u0022'+dataFromClient.Split('|')[1]+'\u0022';
                        Console.WriteLine(" -- query - "+query);
                        Database_Response_Data = DB.SendQuery(query)[0][0];

                        foreach(var item in Database_Response_Data.Split('|')){
                            serverResponse+= item;

                            if(Database_Response_Data.Split('|')[Database_Response_Data.Split('|').Length-1] != item){
                                serverResponse+= "|BRK|";
                            }
                        }
                        
                        if(serverResponse == "" || serverResponse ==  null){
                            serverResponse = "No messages!";
                        }
                        sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);
                        networkStream.Flush();
                        Console.WriteLine(" >> " + serverResponse);

                    }else if (dataFromClient.Split('|')[0] == "SendMessage"){
                        string query = "SELECT Messages FROM complaints WHERE ComplaintID = "+'\u0022'+dataFromClient.Split('|')[1]+'\u0022';
                        Console.WriteLine(" -- query - "+query);
                        Database_Response_Data = DB.SendQuery(query)[0][0];
                        Database_Response_Data+= "|"+dataFromClient.Split('|')[2];


                        query = "UPDATE complaints SET Messages="+'\u0022'+Database_Response_Data+'\u0022'+" WHERE ComplaintID = "+'\u0022'+dataFromClient.Split('|')[1]+'\u0022';
                        Console.WriteLine(" -- query - "+query);
                        Database_Response_Data = DB.SendQuery(query)[0][0];
                        Console.WriteLine(Database_Response_Data);
  
                        if(Database_Response_Data == "" || Database_Response_Data ==  null){
                            Database_Response_Data = "Succesful!";
                        }else{
                            Database_Response_Data = "Failed!";

                        }
                        sendBytes = Encoding.ASCII.GetBytes(Database_Response_Data);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);
                        networkStream.Flush();
                        Console.WriteLine(" >> " + serverResponse);


                    }
                    else{
                        rCount = Convert.ToString(requestCount);
                        serverResponse = "Server to clinet(" + clNo + ") " + rCount;
                        sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);
                        networkStream.Flush();
                        Console.WriteLine(" >> " + serverResponse);
                    }

                    // me stuff done innit fam xDDD

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
