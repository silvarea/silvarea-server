using System.Net.Sockets;
using System.Text;
using Silvarea.Cache;
using Silvarea.Utility;

namespace Silvarea.Network
{

    public class ProtocolDecoder
    {

        public static void Decode(Session session, int size)
        {
            //where the real packets get decoded :o not this pre-login nonsense
        }

        public static void Handshake(Session session, int size)
        {
            Packet packet = new Packet(session.inBuffer);
            session.CurrentState = (RS2ConnectionState) packet.g1();
            int version = packet.g4();
            try
            {
                switch (session.CurrentState)
                {
                    case RS2ConnectionState.UPDATE:
                        if (version == 410) //TODO Don't hardcode this
                            session.Stream.Write([0]);
                        else
                        {
                            session.Stream.Write([6]);
                            SocketManager.Disconnect(session);
                        }
                        break;
                    case RS2ConnectionState.LOGIN:
                        Random rand = new Random();
                        long key = ((long)(rand.Next(1) * 99999999D) << 32) + ((long)(rand.Next(1) * 99999999D));
                        Packet loginReply = new Packet(-1, new byte[9]);
                        loginReply.p1(0);
                        loginReply.p8(key);
                        session.Stream.Write(loginReply.toByteArray());
                        break;
                    default:
                        Console.WriteLine("Disconnecting Session - 1");
                        SocketManager.Disconnect(session);
                        break;
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                SocketManager.Disconnect(session);
            }
        }

        public static void Update(Session session, int size)
        {
            int read = 0;
            //check if not divisible by 2 e.e somethin' funky goin' on
            Packet packet = new Packet(session.inBuffer);
            while ((size - read) > 3 && session.Socket.Connected)
            {
                var requestType = packet.g1();
                var indexNumber = packet.g1();
                var fileNumber = packet.g2();
                read += 4;
                try
                {
                    switch (requestType)
                    {
                        case 0: //non-urgent request
                        case 1: //urgent request
                            session.Stream.Write(UpdateServer.getRequest(indexNumber, fileNumber));
                            break;
                        case 2: //clear request queue plz :3
                        case 3: //I SAID CLEAR IT, MOTHERFUCKER
                            Console.WriteLine("clear queue request received from client");
                            break;
                        case 4: //client error
                            break;
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    SocketManager.Disconnect(session);
                }
            }
        }

        public static void Login(Session session, int size)
        {
            Console.WriteLine("We doin' the login thing: " + size);
            if (size < 2)
                return;
            Packet packet = new Packet(session.inBuffer);
            session.CurrentState = (RS2ConnectionState) packet.g1(); //try/catch here? in case the result can't resolve from mismatch
            int reportedSize = packet.g1();
            if (session.CurrentState == RS2ConnectionState.GAME && reportedSize == (size - 2)) //basically just verifies this login attempt isn't an accident or a fumble
            {
                Console.WriteLine("past first check");
                int version = packet.g4();
                if (version == 410) //TODO Don't hardcode this
                {
                    Console.WriteLine("second check - version correct");
                    Boolean isLowMemory = packet.g1() == 1;
                    for (int i = 0; i < 24; i++) 
                    {
                        packet.g1(); //not really sure what this is, needs investigation
                    }

                    for (int i = 0;i < 13; i++)
                    {
                        packet.g4(); //something to do with cache indices, I think it's CRC32 checksum return verification?? Will read back and see.
                    }

                    int something = packet.g1(); //dunno man, but we're about to check it
                    if (something != 10)
                        packet.g1(); //ahuh, who knows dude

                    long clientKey = packet.g8(); //client's session key?

                    long reportedServerKey = packet.g8(); //I believe this is that key we gave it earlier that we generated

                    //some logic here, duh

                    String username = TextUtils.longToPlayerName((long) packet.g8());
                    String password = TextUtils.getRS2String(packet);
                    Console.WriteLine("Username: " + username + ", Password: " + password); //this shit is garbled I think because of the lack of RSA encryption. Need to look at that, but otherwise it's working to here.

                }
            } else
            {
                Console.WriteLine("Disconnecting Socket for login packet error");
                SocketManager.Disconnect(session);
            }

        }
    }
}
