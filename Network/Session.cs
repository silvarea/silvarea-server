using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading;

namespace Silvarea.Network
{
	public class Session
	{
		public Socket Socket {  get; set; }

		public EndPoint EndPoint { get; set; }

		public NetworkStream Stream { get; set; }

		public readonly byte[] inBuffer = new byte[5000];

		public readonly byte[] outBuffer = new byte[5000];

		public RS2ConnectionState CurrentState { get; set; }

		// TODO: Add a reference to a player class whenever that exists.
		public Session(Socket socket)
		{
			CurrentState = RS2ConnectionState.INITIAL;
			Socket = socket;
			EndPoint = Socket.RemoteEndPoint;
			Stream = new NetworkStream(socket, ownsSocket: true);
		}

		public void Start()
		{
			Listen();
		}

		private void Listen()
		{

			try
			{
                // Todo: Get that IP and port from a config file.
                EndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 43594);
				Socket.BeginReceiveFrom(inBuffer, 0, inBuffer.Length, SocketFlags.None, ref clientEndPoint, OnDataReceive, Socket);
			} 
			catch (SocketException ex) 
			{
				Console.WriteLine(ex.ToString());
				Listen();			
			}
			
		}

		private void OnDataReceive(IAsyncResult result)
		{
			EndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 43594);

			int received = Socket.EndReceiveFrom(result, ref clientEndPoint);//TODO System.Net.Sockets.SocketException - Message = An existing connection was forcibly closed by the remote host. - Source = System.Net.Sockets; after closing client- need to handle dropped connections
 
            switch (CurrentState)
			{
				case RS2ConnectionState.INITIAL:
				case RS2ConnectionState.UPDATE:
					ProtocolDecoder.Decode(this, received);
					break;
			}
			if (result.CompletedSynchronously)
			{
				Task.Run(Listen);
			}
			else
			{
				Listen();
			}
		}
	}
}
