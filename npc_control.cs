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
						if(thismob.hate.Contains(thisuser))
						{
	                        if (MainClass.inRange(mob_x, user_x, thismob.attackrange) &&
							    MainClass.inRange(mob_y, user_y, thismob.attackrange) &&
							    MainClass.inRange(mob_z, user_z, thismob.attackrange))
	                        {
								if(thismob.nextshot <= MainClass.unixtime())
								{
									thismob.nextshot = MainClass.unixtime() + thismob.firerate;
									thisuser.write("ATTACK;"
									               + thismob.id + ";"
									               + thismob.vnum + ";"
									               + thisuser.user_id + ";"
									               + thismob.firespeed);
								}
	                        }
							else if(MainClass.inRange(mob_x,user_x,thismob.fightrange) &&
							        MainClass.inRange(mob_y,user_y,thismob.fightrange) &&
							        MainClass.inRange(mob_z,user_z,thismob.fightrange))
							{
								thismob.move(user_x,user_y,user_z);
							}
						}
                    }
                }
                Thread.Sleep(20);
            }
        }
    }
}