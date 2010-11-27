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
	class items
	{
		public static void flush_items(user thisuser)
		{
			foreach(item thisitem in MainClass.items)
			{
				if(MainClass.inRange(thisitem.position.x,thisuser.position.x) && 
				   MainClass.inRange(thisitem.position.y,thisuser.position.y) && 
				   MainClass.inRange(thisitem.position.z,thisuser.position.z))
				{
					thisuser.write("ITEM;OK;" + thisitem.item_id + ";" + thisitem.vnum);
				}
			}
		}
	}
	class item
	{
		public class position_class
		{
			public int x;
			public int y;
			public int z;
		}
		public position_class position = new position_class();
		
		public int item_id;
		public int vnum;
		public int owner_id;
		public user owner;
		
		public item (int item_id, int vnum, int x, int y, int z)
		{
			this.vnum = vnum;
			this.item_id = item_id;
			this.position.x = x;
			this.position.y = y;
			this.position.z = z;
			gamesrv.MainClass.say("ADDED ITEM: " + this.item_id + "; " + this.vnum);
		}
		
		public item (int item_id, int vnum, user thisuser)
		{
			this.vnum = vnum;
			this.item_id = item_id;
			this.owner = thisuser;
			this.owner_id = thisuser.user_id;
			gamesrv.MainClass.say("ADDED ITEM: " + this.item_id + "; " + this.vnum);
		}
	}
}
