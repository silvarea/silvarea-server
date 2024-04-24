using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading;

namespace Silvarea.Network
{
    /*
	 * So this is pretty neat, I've learned some things while writing this code that I never realized was going on behind the scenes.
	 * 
	 * So a client will actually keep 2 sockets open with the server, the first one for updateserver communication, the second for game communication. (I've also made some changes in the client to force a connection to a specific address, and there is a packet that the client can receive that changes a socket address, but I'm not sure what it's used for. Investigation required.)
	 * 
	 * When a client starts, it makes an initial connection to the server for a handshake, where the client provides it's state request (15 in this case for update) and version number, and the server responds with a return code (0 for success, 6 for version mismatch, others exist and will need further investigation)
	 * 
	 * This initial data stream also has a request for the update server attached to the end for the CRC checksum list. No special handling is required for this because we pass off decoding responsibility to the update server at this point, so that request will go there immediately if we're processing data correctly.
	 * 
	 * That socket will then remain connected to the update server, exchanging requests for cache data.
	 * 
	 * When the client attempts to log in, it will open a second socket that will do the same handshake, but provide 14 as a state request for login
	 * 
	 * The server sends back a return code (0 for success) and a 64-bit (8 byte) key (RSA? Investigate) and route all of the next incoming data through the login decoder
	 * 
	 * The client will then send the login packet which has quite a bit of data I won't get into here
	 * 
	 * Handshake:
	 * > Connect
	 * > Read State Request (14 or 15)
	 * > if (15)
	 *	> Read version#
	 *	> Send return code 0
	 *	> Route all data through update server
	 * >if (14)
	 *	> Route next data through login decoder
	 *	> Login decoder will change route to main decoder after success
	 * 
	 * These notes are somewhat speculatory, see parentheticals.
	 */
    public class Session
    {
        public Socket Socket { get; set; }

        public EndPoint EndPoint { get; set; }

        public NetworkStream Stream { get; set; }

        public readonly byte[] inBuffer = new byte[5000];

        public readonly byte[] outBuffer = new byte[5000];

        public RS2ConnectionState CurrentState { get; set; }

        // TODO: Add a reference to a player class whenever that exists.
        public Session(Socket socket)
        {
            CurrentState = RS2ConnectionState.HANDSHAKE;
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
                if (!Socket.Connected)
                    return;
                // Todo: Get that IP and port from a config file.
                EndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 43594);//TODO Don't hardcode. I think this will cause only local clients to be able to connect? Should refuse to read bytes if from outside connection. We'll see when we try to connect from another PC.
                Socket.BeginReceiveFrom(inBuffer, 0, inBuffer.Length, SocketFlags.None, ref clientEndPoint, OnDataReceive, Socket);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
                SocketManager.Disconnect(this);
            }

        }

        private void OnDataReceive(IAsyncResult result)
        {
            //EndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 43594);
            EndPoint clientEndPoint = Socket.RemoteEndPoint;

            if (clientEndPoint == null)
                return;

            int received = 0;
            try
            {

                received = Socket.EndReceiveFrom(result, ref clientEndPoint);//TODO System.Net.Sockets.SocketException - Message = An existing connection was forcibly closed by the remote host. - Source = System.Net.Sockets; after closing client- need to handle dropped connections
            }
            catch (Exception ex)
            {
                Console.WriteLine("Some error: " + ex.Message);
                SocketManager.Disconnect(this);
                return;
            }
            if (received == 0)
            {
                return;
            }

            switch (CurrentState)
            {
                case RS2ConnectionState.HANDSHAKE:
                    ProtocolDecoder.Handshake(this, received);
                    break;
                case RS2ConnectionState.UPDATE:
                    ProtocolDecoder.Update(this, received);
                    break;
                case RS2ConnectionState.LOGIN:
                    ProtocolDecoder.Login(this, received);
                    break;
                case RS2ConnectionState.GAME:
                    ProtocolDecoder.Decode(this, received);
                    break;
                default:
                    SocketManager.Disconnect(this);
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
            //return;
        }
    }
}
