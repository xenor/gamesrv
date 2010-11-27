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
        public class player
        {
            public static MySqlConnection c = new MySqlConnection(config.mysql.player.config);
			public static MySqlDataReader reader;
            public static MySqlDataReader select(string query)
            {
				try{ sql.player.reader.Close(); } catch { }
                MySqlCommand cmd = c.CreateCommand();
                cmd.CommandText = query;
               	sql.player.reader = cmd.ExecuteReader();
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
			public static MySqlDataReader reader;
            public static MySqlDataReader select(string query)
            {
				try{ sql.game.reader.Close(); } catch { }
                MySqlCommand cmd = c.CreateCommand();
                cmd.CommandText = query;
                sql.game.reader = cmd.ExecuteReader();
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