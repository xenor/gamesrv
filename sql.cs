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
    class sql
    {
        public static MySqlDataReader reader;
        public class player
        {
            public static MySqlConnection c = new MySqlConnection(config.mysql.player.config);
            public static MySqlDataReader select(string query)
            {
				try { sql.reader.Close(); }
                catch { }
                MySqlCommand cmd = c.CreateCommand();
                cmd.CommandText = query;
                reader = cmd.ExecuteReader();
                return reader;
            }
			public static int insert_id()
			{
				MySqlDataReader res = select("SELECT LAST_INSERT_ID()");
				res.Read();
				return res.GetInt32(0);
			}
        }
        public class game
        {
            public static MySqlConnection c = new MySqlConnection(config.mysql.game.config);
            public static MySqlDataReader select(string query)
            {
				try { sql.reader.Close(); }
                catch { }
                MySqlCommand cmd = c.CreateCommand();
                cmd.CommandText = query;
                reader = cmd.ExecuteReader();
                return reader;
            }
			public static int insert_id()
			{
				MySqlDataReader res = select("SELECT LAST_INSERT_ID()");
				return (int)res.GetUInt32(0);
			}
        }
    }
}