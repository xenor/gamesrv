using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace gamesrv
{

    public class user
    {
        public int user_id;
        public int logintime;
        public NetworkStream stream;
        public int lastpong = gamesrv.MainClass.unixtime();
        public user(int user_id, NetworkStream stream)
        {
            this.user_id = user_id;
            this.stream = stream;
            int time = gamesrv.MainClass.unixtime();
            gamesrv.MainClass.say("[ NEWUSER ] [ " + user_id + " ]: TIME: " + time);
        }
    }

    public class pingbot
    {
        public static void start()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(config.pingtimeout);
                int curtime = gamesrv.MainClass.unixtime();
                try
                {
                    for (int i = 0; i < gamesrv.MainClass.allusers.Count; i++ )
                    {
                        user thisuser = gamesrv.MainClass.allusers[i];
                        if (thisuser != null && thisuser.stream != null)
                        {
                            int timespan = (config.pingtimeout / 1000) - (curtime - thisuser.lastpong);
                            if (timespan < 0)
                            {
                                gamesrv.MainClass.closeConn(thisuser);
                            }
                            else
                            {
                                gamesrv.MainClass.writeToStream(thisuser.stream, "PING");
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }

    public class config
    {
        public class mysql
        {
            public const string config = "SERVER=localhost;DATABASE=lunatic_3;UID=lunatic3;PASSWORD=OPFER;";
        }
        public class locale
        {
            public const string welcome = "WELCOME";
            public const string bye = "BYE";
        }
        public class info
        {
            public static int major_version = 0;
            public static int minor_version = 1;
            public static int sub_version = 5;
            public static string version = major_version + "."
                                                + minor_version + "."
                                                + sub_version;
            public static string[] masternames = { "Anohros", "xenor" };
        }
        public static int pingtimeout = 10000;
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
        //public static user[] allusers = new user[16];
        public static List<user> allusers = new List<user>();

        public static int unixtime()
        {
            DateTime date1 = new DateTime(1970, 1, 1);
            DateTime date2 = DateTime.Now;
            TimeSpan ts = new TimeSpan(date2.Ticks - date1.Ticks);
            return (Convert.ToInt32(ts.TotalSeconds));
        }

        public static void closeConn(user user)
        {
            gamesrv.MainClass.allusers.Remove(user);
            user.stream.Close();
        }

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
                    say("[   >>>   ] [ " + thisuser.user_id + " ]: " + text);
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

            allusers.Add(new user(0, clientStream));

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
                say("[   <<<   ] [ " + thisuser.user_id + " ]: " + str);
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
                    say("[  SPRED  ] [ " + thisuser.user_id + " ]: " + str2);
                    foreach (user user in allusers)
                    {
                        if (user != null && user.stream != null)
                        {
                            writeToStream(user.stream, str2);
                        }
                    }
                }
                else if (cmd[0] == "PONG")
                {
                    thisuser.lastpong = unixtime();
                }
            }

            tcpClient.Close();
        }

        #region acceptshit

        public static void say(string str)
        {
            string curtime = DateTime.Now.ToString();
            Console.WriteLine("[ " + curtime + " ]: " + str);
        }

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
            version = config.info.version;
            Console.WriteLine("Game Server v" + version + " starting up...");
            tcpListener = new TcpListener(IPAddress.Any, 3000);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
            Thread ping_thread = new Thread(new System.Threading.ThreadStart(gamesrv.pingbot.start));
            ping_thread.Start();
        }

        #endregion
    }
}

