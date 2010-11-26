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
	public class quest
    {
		
		public int user_id;
		public user user;
		
		public class pc
		{
			public void get_name()
			{
				
			}
		}
		
        public quest(int user_id, user user)
        {
			this.user_id = user_id;
			this.user = user;
        }
	}
}
