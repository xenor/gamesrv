using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Data;
using MySql.Data.MySqlClient;

namespace gamesrv
{

    public class user
    {
        public int user_id;
        public int logintime;
        public NetworkStream stream;
        public user(int user_id, NetworkStream stream)
        {
            this.user_id = user_id;
            this.stream = stream;
            long time = DateTime.Now.ToFileTime();
            Console.WriteLine("[ NEWUSER ] [ " + user_id + " ]: TIME: " + time);
        }
    }

    public class config
    {
        public class mysql
        {
            public static string config = "SERVER=localhost;DATABASE=lunatic_3;UID=lunatic3;PASSWORD=OPFER;";
        }
        public class locale
        {
            public static string welcome = "WELCOME";
            public static string bye = "BYE";
        }
    }

    class MainClass
    {

        public static string version;
        public static TcpListener tcpListener;
        public static Thread listenThread;
        public static ASCIIEncoding encoder = new ASCIIEncoding();
        public static MySqlConnection SQL = new MySqlConnection(config.mysql.config);

        public static NetworkStream[] users = new NetworkStream[4096];
        public static int user_count = 0;
        public static user[] allusers = new user[16];

        public static user findUserByStream(NetworkStream stream)
        {
            foreach (user cur_user in allusers)
            {
                if (cur_user != null && cur_user.stream == stream)
                {
                    return cur_user;
                }
            }
            return new user(0, null);
        }

        public static void writeToStream(NetworkStream stream, string text)
        {
            if (stream != null)
            {
                byte[] buffer = new byte[text.Length];
                buffer = encoder.GetBytes(text.ToString() + "\r\n");
                user thisuser = findUserByStream(stream);
                try
                {
                    stream.Write(buffer, 0, buffer.Length);
                    Console.WriteLine("[   >>>   ] [ " + thisuser.user_id + " ]: " + text);
                }
                catch
                {
                    Console.WriteLine("ERROR WHILE WRITING TO " + thisuser.user_id);
                }
            }
        }

        public static void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            users[user_count] = clientStream;
            user_count++;

            allusers[user_count] = new user(0, clientStream);

            writeToStream(clientStream, config.locale.welcome);

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                string str = encoder.GetString(message, 0, bytesRead).Trim();
                user thisuser = findUserByStream(clientStream);
                Console.WriteLine("[   <<<   ] [ " + thisuser.user_id + " ]: " + str);
                string[] cmd = str.Split(' ');
                cmd[0] = cmd[0].ToUpper();
                if (cmd[0] == "QUIT")
                {
                    writeToStream(clientStream, config.locale.bye);
                    tcpClient.Close();
                }
                else if (cmd[0] == "NOTICE")
                {
                    string str2 = str.Substring(7);
                    Console.WriteLine("[  SPRED  ] [ " + thisuser.user_id + " ]: " + str2);
                    foreach (user user in allusers)
                    {
                        if (user != null && user.stream != null)
                        {
                            writeToStream(user.stream, str2);
                        }
                    }
                }
            }

            tcpClient.Close();
        }

        #region acceptshit

        public static void ListenForClients()
        {
            tcpListener.Start();

            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        public static void Main(string[] args)
        {
            version = "0.1.1";
            Console.WriteLine("Game Server v" + version + " starting up...");
            tcpListener = new TcpListener(IPAddress.Any, 3000);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
        }

        #endregion
    }
}

