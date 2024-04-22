﻿using System.Net.Sockets;
using System.Net;

namespace Silvarea.Network
{
	public class SocketManager
	{

		public static List<Session> sessions = new List<Session>();

		private IPAddress _address;
		private IPEndPoint _endpoint;
		private Socket _socket;

		public SocketManager() {
			_address = new IPAddress([127, 0, 0, 1]);
			_endpoint = new IPEndPoint(_address, 43594);
			_socket = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		}

		public void Start() 
		{
			_socket.Bind(_endpoint);
			_socket.Listen(2000);

			while (true)
			{
				Listen();
			}
		}

		public void Listen() 
		{
			Socket clientSocket = _socket.Accept();
			Console.WriteLine("New connection type: " + clientSocket.SocketType);
			//var session = sessions.Find(s => clientSocket == s.Socket);
			var session = sessions.Find(s => clientSocket.RemoteEndPoint.Equals(s.EndPoint));
			if (session == null)
			{
				session = new Session(clientSocket);
				sessions.Add(session);
				session.Start();
			} else
			{
				session.Socket = clientSocket;
				session.Start();
				Console.WriteLine("Continued Connection!");//notice how this never happens? lol.
			}
		}

		public static void Disconnect(Session s)
		{
            s.Socket.Disconnect(false);
            sessions.Remove(s);
		}

		public void Close() 
		{
		
		}
	}
}
