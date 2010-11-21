#region includes
using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
#endregion

namespace gamesrv
{
    #region config
    public class config
    {
        public class mysql
        {
            public class player
            {
                public const string config = "SERVER=192.168.56.101;" +
                                                "DATABASE=gamesrv_player;" +
                                                "UID=lunatic3;" +
                                                "PASSWORD=lalaftw#!;";
                public const string dbname = "gamesrv_player";
            }
            public class game
            {
                public const string config = "SERVER=192.168.56.101;" +
                                                "DATABASE=gamesrv_game;" +
                                                "UID=lunatic3;" +
                                                "PASSWORD=lalaftw#!;";
                public const string dbname = "gamesrv_game";
            }
        }
        public class locale
        {
            public const string welcome = "WELCOME";
            public const string bye = "BYE";
        }
        public class info			// JEAH LINE 42 <3
        {
            public static int major_version = 0;
            public static int minor_version = 2;
            public static int sub_version = 1;
            public static string version = major_version + "."
                                                + minor_version + "."
                                                + sub_version;
            public static string[] masternames = { "Anohros", "xenor" };
        }
        public static int pingtimeout = 0;
        public static int logintimeout = -1;
        public static bool debug = true;
        public static bool testserver = true;
    }
    #endregion

    #region sql
    class sql
    {
        public static MySqlCommand cmd = new MySqlCommand();
        public static MySqlDataReader reader;
        public class player
        {
            public static MySqlConnection c = new MySqlConnection(config.mysql.player.config);
            public static MySqlDataReader select(string query)
            {
                MySqlCommand cmd = c.CreateCommand();
                cmd.CommandText = query;
                MySqlDataReader reader = cmd.ExecuteReader();
                return reader;
            }
        }
        public class game
        {
            public static MySqlConnection c = new MySqlConnection(config.mysql.game.config);
            public static MySqlDataReader select(string query)
            {
                try { sql.reader.Close(); }
                catch { }
                sql.cmd = c.CreateCommand();
                sql.cmd.CommandText = query;
                sql.reader = sql.cmd.ExecuteReader();
                return reader;
            }
        }
    }
    #endregion

    #region game
    public class game
    {
        public class proto
        {
            public static List<string> ship = new List<string>();
        }
    }
    #endregion

    #region quest
    class quest
    {
        public quest()
        {
            /*Lua lua = new Lua();
            lua.DoFile("test.quest");
                */
        }
    }
    #endregion

    #region user
    public class user
    {
        public int user_id;
        public int logintime;
        public NetworkStream stream;
        public NetworkStream logstream;
        public int lastpong = gamesrv.MainClass.unixtime();
        public bool identified = false;
        public bool identping = false;
        public class data_class
        {
            public string nick = "";
            public int adminlevel = 0;
        }
        public class position_class
        {
            public int x;
            public int y;
            public int z;
        }
        public data_class data = new data_class();
        public position_class position = new position_class();

        public void write(string str)
        {
            byte[] buffer = new byte[str.Length];
            buffer = gamesrv.MainClass.encoder.GetBytes(str.ToString() + "\r\n");
            try
            {
                this.stream.Write(buffer, 0, buffer.Length);
                gamesrv.MainClass.say("[   >>>   ] [ " + this.user_id + " " + this.data.nick + " (" + this.data.adminlevel + ") ]: " + str);
                this.log("[   >>>   ] [ " + this.user_id + " " + this.data.nick + " (" + this.data.adminlevel + ") ]: " + str);
            }
            catch
            {
                Console.WriteLine("ERROR WHILE WRITING TO " + this.user_id);
                MainClass.closeConn(this);
            }
        }

        public void log(string str)
        {
            gamesrv.MainClass.writeToStream(this.logstream, str);
        }

        public user(int user_id, NetworkStream stream)
        {
            this.user_id = user_id;
            this.stream = stream;
            int time = gamesrv.MainClass.unixtime();
            gamesrv.MainClass.say("[ NEWUSER ] [ " + user_id + " ]: TIME: " + time);
        }

        public void identAs(string login, string passwd)
        {
            if (this.user_id != 0)
            {
                this.write("LOGIN;ERROR;10");
            }
            else
            {
                string qry = "SELECT * FROM " + config.mysql.player.dbname + ".accounts WHERE login = '" + login + "' AND passwd = '" + passwd + "'";
                MySqlDataReader reader = gamesrv.sql.player.select(qry);
                if (reader.Read())
                {
                    this.identified = true;
                    this.user_id = Convert.ToInt32(reader["account_id"].ToString());
                    this.data.nick = reader["nick"].ToString();
                    this.data.adminlevel = Convert.ToInt32(reader["adminlevel"].ToString());
                    this.write("LOGIN;OK");
                }
                else
                {
                    this.write("LOGIN;ERROR;12");
                }
                reader.Close();
            }
        }
    }
    #endregion

    #region ping/pong
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
                    for (int i = 0; i < gamesrv.MainClass.allusers.Count; i++)
                    {
                        user thisuser = gamesrv.MainClass.allusers[i];
                        if (thisuser != null && thisuser.stream != null)
                        {
                            int timespan = (config.pingtimeout / 1000) - (curtime - thisuser.lastpong);
                            if (timespan < 0)
                            {
                                thisuser.write("ERROR;21");
                                gamesrv.MainClass.closeConn(thisuser);
                            }
                            else
                            {
                                thisuser.write("PING");
                            }
                            if (thisuser.identified == false && thisuser.identping == true)
                            {
                                thisuser.write("ERROR;13");
                                gamesrv.MainClass.closeConn(thisuser);
                            }
                            else if (thisuser.identified == false && thisuser.identping == false)
                            {
                                thisuser.identping = true;
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
    #endregion

    #region Monster Class
    class mob
    {
        public class position_class
        {
            public int x = 0;
            public int y = 0;
            public int z = 0;
        }
        public position_class position = new position_class();
		
		public int id = 0;
        public int vnum = 0;
		public int attackrange = 0;
		public int firerate = 0;
		public int firespeed = 0;
		public int nextshot = 0;
		
		public List<user> hate = new List<user>();
		
        public mob(int vnum, MySqlDataReader res)
        {
            this.vnum = vnum;
			MainClass.mobcount++;
			this.id = MainClass.mobcount;
			this.attackrange = Convert.ToInt32(res["attackrange"].ToString());
			this.firerate = Convert.ToInt32(res["firerate"].ToString());
			this.firespeed = Convert.ToInt32(res["firespeed"].ToString());
        }
		
        public void warp(int x, int y, int z)
        {
            this.position.x = x;
            this.position.y = y;
            this.position.z = z;
        }
    }
    #endregion

	class attack
	{
		public void start(int user_id, int mob_id)
		{
			
		}
	}
	
    #region NPC Control
    class npc_control
    {
        public void start()
        {
            while (true)
            {
                foreach (mob thismob in MainClass.mobs)
                {
                    int mob_x = thismob.position.x;
                    int mob_y = thismob.position.y;
                    int mob_z = thismob.position.z;
                    foreach (user thisuser in MainClass.allusers)
                    {
                        int user_x = thisuser.position.x;
                        int user_y = thisuser.position.y;
                        int user_z = thisuser.position.z;
                        if (MainClass.inRange(mob_x, user_x,thismob.attackrange) && MainClass.inRange(mob_y, user_y, thismob.attackrange) && MainClass.inRange(mob_z, user_z, thismob.attackrange))
                        {
                            //thisuser.write("MOB " + thismob.vnum + " IN RANGE.");
							if(thismob.hate.Contains(thisuser))
							{
								if(thismob.nextshot < MainClass.unixtime())
								{
									thismob.nextshot = MainClass.unixtime() + thismob.firerate;
									thisuser.write("ATTACK;"
									               + thismob.id + ";"
									               + thismob.vnum + ";"
									               + thisuser.user_id + ";"
									               + thismob.firespeed
									               );
								}
							}
                        }
                    }
                }
                Thread.Sleep(5);
            }
        }
    }
    #endregion

    class MainClass
    {
        #region sinnlos
        public static string version;
        public static Random rand = new Random();

        public static volatile System.IO.FileStream logstream;

        public static TcpListener tcpListener;
        public static volatile Thread listenThread;
        public static volatile ASCIIEncoding encoder = new ASCIIEncoding();
        public static volatile sql db = new sql();

        public static NetworkStream[] users = new NetworkStream[4096];
        public static int user_count = 0;
        public static volatile List<user> allusers = new List<user>();

        public static volatile List<mob> mobs = new List<mob>();
		public static volatile int mobcount = 0;
		
		public static volatile bool online = true;

        public static int unixtime()
        {
            DateTime date1 = new DateTime(1970, 1, 1);
            DateTime date2 = DateTime.Now;
            TimeSpan ts = new TimeSpan(date2.Ticks - date1.Ticks);
            return (Convert.ToInt32(ts.TotalSeconds));
        }

        public static void closeConn(user user)
        {
            gamesrv.MainClass.say("[  KILLD  ] [ " + user.user_id + " ]: TIME: " + unixtime());
            gamesrv.MainClass.allusers.Remove(user);
            user.stream.Close();
			List<user> list = allusers;
			allusers = new List<user>{};
			foreach(user thisuser in list)
			{
				allusers.Add(thisuser);
			}
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

        public static user findUserByNick(string nick)
        {
            foreach (user cur_user in allusers)
            {
                if (cur_user != null && cur_user.data.nick == nick)
                {
                    return cur_user;
                }
            }
            return new user(0, null);
        }

        public static List<mob> findMobsByPos(int x, int y, int z)
        {
            List<mob> moblist = new List<mob>();
            foreach (mob thismob in mobs)
            {
                if (inRange(x, thismob.position.x) && inRange(y, thismob.position.y) && inRange(z, thismob.position.z))
                {
                    moblist.Add(thismob);
                }
            }
            return moblist;
        }

        public static bool inRange(int i_1, int i_2, int range)
        {
            if (((i_1 + range) >= i_2) && (i_1 - range) <= i_2) return true; else return false;
        }

        public static bool inRange(int i1, int i2)
        {
            return inRange(i1, i2, 10);
        }
		
		public static void fixUsers()
		{
			user[] list = allusers.ToArray();
			allusers = null;
			allusers = new List<user>();
			foreach(user thisuser in list)
			{
				if(thisuser.stream != null)
				{
					allusers.Add(thisuser);
				}
			}
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
                    gamesrv.MainClass.say("[   >>>   ] [ " + thisuser.user_id + " " + thisuser.data.nick + " ]: " + text);
                    thisuser.log("[   >>>   ] [ " + thisuser.user_id + " " + thisuser.data.nick + " ]: " + text);
                }
                catch
                {
                    Console.WriteLine("ERROR WHILE WRITING TO " + thisuser.user_id);
                }
            }
        }

        #endregion

        public static void HandleClientComm(object client)
        {
            #region accept user
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            users[user_count] = clientStream;
            user_count++;

            allusers.Add(new user(0, clientStream));

            writeToStream(clientStream, config.locale.welcome);

            byte[] message = new byte[4096];
            int bytesRead;
            #endregion
            while (true)
            {

                #region read
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
                string fullstr = encoder.GetString(message, 0, bytesRead).Trim();
                user thisuser = findUserByStream(clientStream);
                char[] split = "\r\n".ToCharArray();
                string[] strs = fullstr.Split(split);
                foreach (string str in strs)
                {
                    if (str.Trim() != "")
                    {
                        say("[   <<<   ] [ " + thisuser.user_id + " " + thisuser.data.nick + " (" + thisuser.data.adminlevel + ") ]: " + str);
                        thisuser.log("[   <<<   ] [ " + thisuser.user_id + " " + thisuser.data.nick + " (" + thisuser.data.adminlevel + ") ]: " + str);
                        string[] cmd = str.Split(';');
                        cmd[0] = cmd[0].ToUpper();
                #endregion

                        #region base (QUIT/NOTICE)

                        if (cmd[0] == "QUIT")
                        {
                            writeToStream(clientStream, config.locale.bye);
                            closeConn(thisuser);
                        }

                        else if (cmd[0] == "NOTICE")
                        {
                            string str2 = str.Substring(7);
                            say("[    N    ] [ " + thisuser.user_id + " ]: " + str2);
							fixUsers();
							bool fail = true;
							while(fail == true)
							{
								try
								{
									fail = false;
		                            foreach (user user in allusers)
		                            {
		                                if (user != null && user.stream != null)
		                                {
											try
											{
		                                    	user.write("NOTICE;" + str2);
											}
											catch {}
		                                }
										else
										{
											closeConn(user);
										}
		                            }
								}
								catch
								{
									fail = true;
								}
							}
                        }

                        #endregion

                        #region ping/pong
                        else if (cmd[0] == "PING")
                        {
                            thisuser.write("PONG");
                        }
                        else if (cmd[0] == "PONG")
                        {
                            thisuser.lastpong = unixtime();
                        }
                        #endregion

                        #region login
                        else if (cmd[0] == "LOGIN")
                        {
                            if (cmd.Length > 3)
                            {
                                string username = cmd[1];
                                char[] pwlength = cmd[2].ToCharArray();
                                string password = cmd[3];
                                thisuser.identAs(username, password);
                            }
                            else
                            {
                                thisuser.write("LOGIN;ERROR;11");
                            }
                        }
                        #endregion

                        #region message
                        else if (cmd[0] == "MESSAGE")
                        {
                            bool sent = false;
                            foreach (user cuser in allusers)
                            {
                                if (cuser.data.nick == cmd[1])
                                {
                                    string msg = str.Substring(9 + cmd[1].Length);
                                    cuser.write("MESSAGE;" + thisuser.data.nick + ";" + msg);
                                    sent = true;
                                }
                            }
                            if (sent == false)
                            {
                                thisuser.write("MESSAGE;ERROR;30");
                            }
                        }
                        #endregion

                        #region mobs
                        else if (cmd[0] == "FLUSH")
                        {
                            if (cmd.Length > 1)
                            {
                                if (cmd[1] == "MOBS")
                                {
                                    List<mob> list = findMobsByPos(thisuser.position.x, thisuser.position.y, thisuser.position.z);
                                    foreach (mob thismob in list)
                                    {
                                        thisuser.write("SPAWN;OK;"
										               + thismob.id + ";"
                                                       + thismob.vnum + ";"
                                                       + thismob.position.x + ";"
                                                       + thismob.position.y + ";"
                                                       + thismob.position.z);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region admin level 2+
                        else if ((config.testserver == true || thisuser.data.adminlevel > 1) && cmd[0] == "WARP")
                        {
                            if (cmd.Length > 0)
                            {
								try
								{
	                                thisuser.position.x = Convert.ToInt32(cmd[1]);
	                                thisuser.position.y = Convert.ToInt32(cmd[2]);
	                                thisuser.position.z = Convert.ToInt32(cmd[3]);
	                                thisuser.write("WARP;OK;" + thisuser.position.x + ";" + thisuser.position.y + ";" + thisuser.position.z);
								}
								catch
								{
									thisuser.write("WARP;ERROR;22;WARP <X> <Y> [<Z>]");
								}
                            }
                            else
                            {
                                thisuser.write("WARP;ERROR;22;WARP <X> <Y> [<Z>]");
                            }
                        }

                        else if ((config.testserver == true || thisuser.data.adminlevel > 1) && cmd[0] == "SPAWN")
                        {
                            if (cmd.Length > 1)
                            {
								int count = 1;
								int vnum = 0;
                                if (cmd.Length > 2)
                                {
                                    count = Convert.ToInt32(cmd[2]);
                                }
                                try
                                {
                                    vnum = Convert.ToInt32(cmd[1]);
                                }
                                catch
                                {
                                }
                                MySqlDataReader res = sql.game.select("SELECT * FROM ship_proto WHERE vnum = '" + vnum + "'");
                                if (res.Read())
                                {
									for(int i = 0;i < count;i++)
									{
	                                    int x = rand.Next(thisuser.position.x - 10, thisuser.position.x + 10);
	                                    int y = rand.Next(thisuser.position.y - 10, thisuser.position.y + 10);
	                                    int z = rand.Next(thisuser.position.z - 10, thisuser.position.z + 10);
	                                    mob thismob = new mob(Convert.ToInt32(res["vnum"]),res);
	                                    thismob.warp(x, y, z);
	                                    gamesrv.MainClass.mobs.Add(thismob);
										int id = thismob.id;
	                                    thisuser.write("SPAWN;OK;" + id + ";" + res["vnum"] + ";" + x + ";" + y + ";" + z);
									}
                                }
                                else
                                {
                                    thisuser.write("SPAWN;ERROR;40");
                                }
                            }
                            else
                            {
                                thisuser.write("SPAWN;ERROR;22");
                            }
                        }
						
						else if((config.testserver == true || thisuser.data.adminlevel > 1) && cmd[0] == "AGGR")
						{
							if(cmd.Length < 1)
							{
								List<mob> list = findMobsByPos(thisuser.position.x, thisuser.position.y, thisuser.position.z);
								foreach(mob curmob in list)
								{
									curmob.hate.Add(thisuser);
								}
							}
							else
							{
								int id = Convert.ToInt32(cmd[1]);
								foreach(mob curmob in mobs)
								{
									if(curmob.id == id)
									{
										curmob.hate.Add(thisuser);
									}
								}
							}
						}
                        #endregion
						
						#region admin level 3+
						else if((config.testserver == true || thisuser.data.adminlevel > 2) && cmd[0] == "SHUTDOWN")
						{
							MainClass.online = false;
						}
						#endregion

                        #region debug
                        else if (config.debug == true)
                        {
                            if (cmd[0] == "USER_ID")
                            {
                                thisuser.user_id = Convert.ToInt32(cmd[1]);
                                thisuser.write("USER_ID;OK");
                            }

                            else if (cmd[0] == "NICK")
                            {
                                thisuser.data.nick = cmd[1];
                                thisuser.write("NICK;OK");
                            }

                            else if (cmd[0] == "POS")
                            {
                                int x = thisuser.position.x;
                                int y = thisuser.position.y;
                                int z = thisuser.position.z;
                                thisuser.write("[ " + x + " | " + y + " | " + z + " ]");
                            }

                            else if (cmd[0] == "LOG")
                            {
                                thisuser.identified = true;
                                user user = findUserByNick(cmd[1]);
                                user.logstream = thisuser.stream;
                            }

                            else if (cmd[0] == "MAP_PIC")
                            {
                                for (int x = -10; x < 10; x++)
                                {
                                    for (int y = -10; y < 10; y++)
                                    {
                                        int mob_count = 0;
                                        foreach (mob thismob in mobs)
                                        {
                                            if (thismob.position.x == x && thismob.position.y == y)
                                            {
                                                mob_count++;
                                            }
                                        }
                                        Console.Write(mob_count);
                                    }
                                    Console.Write("\r\n");
                                }
                            }

                            else
                            {
                                thisuser.write("ERROR;20");
                            }
                        }
                        else
                        {
                            thisuser.write("ERROR;20");
                        }
                    }
                }
                        #endregion
            }

            tcpClient.Close();
        }

        #region acceptshit

        public static void log(string str)
        {
            byte[] bytes = encoder.GetBytes(str);
            logstream.Write(bytes, 0, bytes.Length);
            logstream.Flush();
        }

        public static void say(string str)
        {
            string curtime = DateTime.Now.ToString();
            string writestring = "[ " + curtime + " ]: " + str + "\r\n";
            if (config.debug == true)
            {
                Console.WriteLine(writestring.Trim());
            }
            log(writestring);
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

        public static void cacheGameDB()
        {
            MySqlDataReader reader = gamesrv.sql.game.select("SELECT * FROM ship_proto");
            while (reader.Read())
            {
                gamesrv.game.proto.ship[Convert.ToInt32(reader["vnum"].ToString())] = reader.ToString();
            }
        }

        public static void Main(string[] args)
        {
            version = config.info.version;
            int curtime = unixtime();
            if (!System.IO.Directory.Exists("./syslog/")) System.IO.Directory.CreateDirectory("./syslog/");
            logstream = System.IO.File.Create("./syslog/" + curtime + ".log");
            say("Using Syslog: ./syslog/" + curtime + ".log");
            say("Game Server v" + version + " starting up...");
            try
            {
                gamesrv.sql.player.c.Open();
            }
            catch (Exception e)
            {
                say("FAILED TO CONNECT PLAYER DATABASE: " + e.Message);
                return;
            }
            say("PLAYER DB\t\t\tREADY");
            try
            {
                gamesrv.sql.game.c.Open();
            }
            catch (Exception e)
            {
                say("FAILED TO CONNECT GAME DATABASE: " + e.Message);
                return;
            }
            say("GAME DB\t\t\tREADY");
            tcpListener = new TcpListener(IPAddress.Any, 3000);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
            say("LISTEN THREAD\t\t\tREADY");
            Thread npc_thread = new Thread(new ThreadStart(new npc_control().start));
            npc_thread.Start();
            say("NPC CONTROL\t\t\tREADY");
			Thread ping_thread;
            if (config.pingtimeout > 0)
            {
                ping_thread = new Thread(new System.Threading.ThreadStart(gamesrv.pingbot.start));
                ping_thread.Start();
                say("PING THREAD\t\t\tREADY");
            }
            say("GAME SERVER READY\t\tREADY");
			while(true)
			{
				if(online == false)
				{
					for(int i = 10;i > 0; i--)
					{
						fixUsers();
						bool fail = true;
						while(fail == true)
						{
							try
							{
								fail = false;
								foreach(user thisuser in allusers)
								{
									thisuser.write("SHUTDOWN;50;" + i);
								}
								Thread.Sleep(1000);
							}
							catch
							{
								fail = true;
							}
						}
					}
					foreach(user thisuser in allusers)
					{
						thisuser.write("SHUTDOWN;51");
					}
					listenThread.Abort();
					listenThread.Interrupt();
					npc_thread.Abort();
					npc_thread.Interrupt();
					try
					{
						ping_thread.Abort();
						ping_thread.Interrupt();
					}
					catch{}
					say("SHUTDOWN COMPLETE AT " + unixtime());
					Thread.CurrentThread.Abort();
					Thread.CurrentThread.Interrupt();
				}
			}
        }

        #endregion
    }
}