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
        public class info
        {
            public static int major_version = 0;
            public static int minor_version = 2;
            public static int sub_version = 1;
            public static string version = major_version + "."
                                                + minor_version + "."
                                                + sub_version;
            public static List<string> masternames = new List<string>{"Anohros", "xenor"};
        }
        public static int pingtimeout = 0;
        public static int logintimeout = -1;
        public static bool debug = true;
        public static bool testserver = true;
    }
}
