using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace gamesrv
{
	class MainClass
	{
		
		public static string version;
		public static TcpListener tcpListener;
		public static Thread listenThread;
		public static ASCIIEncoding encoder = new ASCIIEncoding();
		public static object[] users = new object[4096];
		public static int user_count = 0;
		
		public static void writeToStream(NetworkStream stream, string text)
		{	
			byte[] buffer = new byte[text.Length];
			buffer = encoder.GetBytes(text.ToString());
			stream.Write(buffer, 0, buffer.Length);
		}
		
		public static void HandleClientComm(object client)
		{
			TcpClient tcpClient = (TcpClient)client;
			NetworkStream clientStream = tcpClient.GetStream();
			users[user_count] = client;
			user_count++;
			
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
				if (str == "QUIT")
				{
					writeToStream(clientStream,"Bye =)");
					tcpClient.Close();
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
			Console.WriteLine ("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
			Console.WriteLine ();
			tcpListener = new TcpListener(IPAddress.Any, 3000);
			listenThread = new Thread(new ThreadStart(ListenForClients));
			listenThread.Start();
		}
		
		#endregion
	}
}
