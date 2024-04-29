using System.Net.Sockets;
using System.Net;

namespace Silvarea.Network
{
    public class SocketManager
    {

        public static List<Session> sessions = new List<Session>();

        private IPAddress _address;
        private IPEndPoint _endpoint;
        private Socket _socket;

        public SocketManager()
        {
            Console.WriteLine("Opening socket...");
            _address = new IPAddress([127, 0, 0, 1]);
            _endpoint = new IPEndPoint(_address, 43594);
            _socket = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            _socket.Bind(_endpoint);
            _socket.Listen(2000);

            Console.WriteLine("Silvarea is online!");

            while (true)
            {
                Listen();
            }
        }

        public void Listen()
        {
            Socket clientSocket = _socket.Accept();
            Console.WriteLine("New connection from IP: " + clientSocket.RemoteEndPoint.ToString().Split(":")[0] + ". Number of live connections: " + sessions.Count);
            var session = sessions.Find(s => clientSocket.RemoteEndPoint.ToString().Split(":")[0].Equals(s.Socket.RemoteEndPoint.ToString().Split(":")[0])); //instead of checking the sessions list, check the player list for connected sockets 
            if (session == null || session.CurrentState != RS2ConnectionState.GAME)
            {
                session = new Session(clientSocket);
                sessions.Add(session);
                session.Start();
            }
            else
            {
                session.Socket = clientSocket;
                session.Start();
            }
        }

        public static void Disconnect(Session s)
        {
            if (s.Socket.Connected)
            {
                s.Socket.Disconnect(false);
                s.Socket.Dispose();
            }
            if (sessions.Contains(s))
                sessions.Remove(s);
        }
    }
}
