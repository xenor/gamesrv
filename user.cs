using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using LuaInterface;
namespace gamesrv
{
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
			public bool developer = false;
        }
        public class position_class
        {
            public int x;
            public int y;
            public int z;
        }
        public data_class data = new data_class();
        public position_class position = new position_class();
		public quest quest;

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
			this.quest = new quest(user_id,this);
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
					items.flush_items(this);
                }
                else
                {
                    this.write("LOGIN;ERROR;12");
                }
                reader.Close();
            }
        }
    }
}