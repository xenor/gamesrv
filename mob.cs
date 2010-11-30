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
		
		public int id			= 0;
        public int vnum			= 0;
		public int attackrange	= 0;
		public int firerate = 0;
		public int firespeed = 0;
		public int fightrange = 0;
		public int nextshot = 0;
		public int min_damage = 0;
		public int max_damage = 0;
		
		public List<user> hate = new List<user>();
		
        public mob(int vnum, MySqlDataReader res)
        {
            this.vnum = vnum;
			MainClass.mobcount++;
			this.id = MainClass.mobcount;
			this.attackrange = Convert.ToInt32(res["attackrange"].ToString());
			this.firerate = Convert.ToInt32(res["firerate"].ToString());
			this.firespeed = Convert.ToInt32(res["firespeed"].ToString());
			this.fightrange = Convert.ToInt32(res["fightrange"].ToString());
			this.min_damage = Convert.ToInt32(res["min_damage"].ToString());
			this.max_damage = Convert.ToInt32(res["max_damage"].ToString());
        }
		
        public void warp(int x, int y, int z)
        {
			int old_x = this.position.x;
			int old_y = this.position.y;
			int old_z = this.position.z;
            this.position.x = x;
            this.position.y = y;
            this.position.z = z;
			foreach(user thisuser in gamesrv.MainClass.findUsersByPos(x,y,z))
			{
				thisuser.write("MOB;WARP;"
				               + this.id + ";"
				               + this.position.x + ";"
				               + this.position.y + ";"
				               + this.position.z);
			}
			foreach(user thisuser in gamesrv.MainClass.findUsersByPos(old_x,old_y,old_z))
			{
				thisuser.write("MOB;WARP;"
				               + this.id + ";"
				               + this.position.x + ";"
				               + this.position.y + ";"
				               + this.position.z);
			}
        }
		
		public void move(int x, int y, int z)
		{
			this.warp(x,y,z);
		}
    }
}