using System.Data;
using System.Net;
using System.Net.Sockets;

namespace Silvarea.Network
{
	public class Session
	{
		public Socket Socket {  get; set; }

		public NetworkStream Stream { get; set; }

		public readonly byte[] inBuffer = new byte[5000];

		public readonly byte[] outBuffer = new byte[5000];

		public RS2ConnectionState CurrentState { get; set; }

		// TODO: Add a reference to a player class whenever that exists.
		public Session(Socket socket)
		{
			CurrentState = RS2ConnectionState.INITIAL;
			Socket = socket;
			Stream = new NetworkStream(socket, ownsSocket: true);
		}

		public void Start()
		{
			Listen();
		}

		private void Listen()
		{
			// Todo: Get that IP and port from a config file.
			EndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 43594);
			Socket.BeginReceiveFrom(inBuffer, 0, inBuffer.Length, SocketFlags.None, ref clientEndPoint, OnDataReceive, Socket);
		}

		private void OnDataReceive(IAsyncResult result)
		{
			EndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 43594);
			int received = Socket.EndReceiveFrom(result, ref clientEndPoint);

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
