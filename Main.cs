using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace gamesrv
{
	
	public class user
	{
		public int user_id;
		public int logintime;
		public NetworkStream stream;
		public user (int user_id, NetworkStream stream)
		{
			this.user_id = user_id;
			this.stream = stream;
			long time = DateTime.Now.ToFileTime();
			Console.WriteLine("User with ID " + user_id + " connected at " + time + ".");
		}
	}
	
	class MainClass
	{
		
		public static string version;
		public static TcpListener tcpListener;
		public static Thread listenThread;
		public static ASCIIEncoding encoder = new ASCIIEncoding();
		public static NetworkStream[] users = new NetworkStream[4096];
		public static int user_count = 0;
		public static user[] allusers = new user[16];
		
		public static user findUserByStream(NetworkStream stream)
		{
			foreach(user user in allusers)
			{
				if(user.stream == stream)
				{
					return user;
				}
			}
			return new user(0,null);
		}
		
		public static void writeToStream(NetworkStream stream, string text)
		{
			if (stream != null)
			{
				byte[] buffer = new byte[text.Length];
				buffer = encoder.GetBytes(text.ToString() + "\r\n");
				stream.Write(buffer, 0, buffer.Length);
			}
		}
		
		public static void HandleClientComm(object client)
		{
			TcpClient tcpClient = (TcpClient)client;
			NetworkStream clientStream = tcpClient.GetStream();
			users[user_count] = clientStream;
			user_count++;
			
			allusers[user_count] = new user(0,clientStream);
			
			writeToStream(clientStream,"WELCOME");
			
			byte[] message = new byte[4096];
			int bytesRead;
			
			while (true)
			{
				bytesRead = 0;
				
				try
				{
					//blocks until a client sends a message
					bytesRead = clientStream.Read(message, 0, 4096);
				}
				catch
				{
					//a socket error has occured
					break;
				}
				
				if (bytesRead == 0)
				{
					//the client has disconnected from the server
					break;
				}
			
				//message has successfully been received
				string str = encoder.GetString(message, 0, bytesRead).Trim();
				Console.WriteLine("Incoming string from " + client.ToString() + ": " + str);
				Console.WriteLine("SUBSTR:" + str.Substring(0,1));
				if (str == "QUIT")
				{
					writeToStream(clientStream,"Bye =)");
					tcpClient.Close();
				}
				else if (str.Substring(0,1) == "N")
				{
					string str2 = str.Substring(2);
					Console.WriteLine("str2: " + str2);
					foreach(user user in allusers)
					{
						if(user.stream != null)
						{
    						writeToStream(user.stream,str2);
						}
					}
				}
			}
			
			tcpClient.Close();
		}
		
		#region acceptshit
		
		public static void ListenForClients()
		{
			tcpListener.Start();
			
			while (true)
			{
				TcpClient client = tcpListener.AcceptTcpClient();
				Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
				clientThread.Start(client);
			}
		}
		
		public static void Main (string[] args)
		{
			version = "0.1.1";
			Console.WriteLine ("Game Server v" + version + " starting up...");
			tcpListener = new TcpListener(IPAddress.Any, 3000);
			listenThread = new Thread(new ThreadStart(ListenForClients));
			listenThread.Start();
		}
		
		#endregion
	}
}

