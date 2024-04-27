using Silvarea.Cache;
using Silvarea.Utility;

namespace Silvarea.Network.Codec
{

    public class ProtocolCodec
    {

        public static void Handshake(Session session, int size)
        {
            Packet packet = new Packet(session.inBuffer);
            session.CurrentState = (RS2ConnectionState)packet.g1();
            try
            {
                switch (session.CurrentState)
                {
                    case RS2ConnectionState.UPDATE:
                        int version = packet.g4();
                        if (version == ConfigurationManager.Config.GameServerConfiguration.Version)
                            session.Stream.Write([0]);
                        else
                        {
                            session.Stream.Write([6]);
                            SocketManager.Disconnect(session);
                        }
                        break;
                    case RS2ConnectionState.LOGIN:
                        Console.WriteLine("Login... what is this... client state? " + packet.g1());
                        Random rand = new Random();
                        session.serverKey = ((long)(rand.NextDouble() * 99999999D) << 32) + (long)(rand.NextDouble() * 99999999D);
                        Packet loginReply = new Packet(-1, new byte[9]);
                        loginReply.p1(0);
                        loginReply.p8(session.serverKey);
                        session.Stream.Write(loginReply.toByteArray());
                        break;
                    default:
                        Console.WriteLine("Disconnecting Session - 1");
                        SocketManager.Disconnect(session);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                SocketManager.Disconnect(session);
            }
        }

        public static void Update(Session session, int size)
        {
            int read = 0;
            //check if not divisible by 2, all requests here should be an even number.
            Packet packet = new Packet(session.inBuffer);
            while (size - read > 3 && session.Socket.Connected)
            {
                var requestType = packet.g1();
                var indexNumber = packet.g1();
                var fileNumber = packet.g2();
                read += 4;
                try
                {
                    switch (requestType)
                    {
                        case 0: //TODO non-urgent request
                        case 1: //urgent request
                            session.Stream.Write(UpdateServer.getRequest(indexNumber, fileNumber));
                            break;
                        case 2: //non-urgent clear request queue
                        case 3: //urgent clear requests queue
                            Console.WriteLine("clear queue request received from client");
                            break;
                        case 4: //client error
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    SocketManager.Disconnect(session);
                }
            }
        }
    }
}
