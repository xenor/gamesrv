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
}