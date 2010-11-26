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
}