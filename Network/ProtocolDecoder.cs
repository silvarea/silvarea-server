using System.Net.Sockets;
using System.Text;
using Silvarea.Cache;
using Silvarea.Game.IO;
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
            session.CurrentState = (RS2ConnectionState)packet.g1();
            int version = packet.g4();
            try
            {
                switch (session.CurrentState)
                {
                    case RS2ConnectionState.UPDATE:
                        if (version == ConfigurationManager.Config.GameServerConfiguration.Version)
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

        public static void Login(Session session, int size)
        {
            Console.WriteLine("We doin' the login thing: " + size);
            if (size < 2)
                return;
            Packet packet = new Packet(session.inBuffer);
            session.CurrentState = (RS2ConnectionState)packet.g1(); //try/catch here? in case the result can't resolve from mismatch -- ahh, also 18 is RECONNECTING from a dropped connection, 16 is brand new login
            int reportedSize = packet.g1();
            if (session.CurrentState == RS2ConnectionState.GAME && reportedSize == (size - 2)) //basically just verifies this login attempt isn't an accident or a fumble
            {
                Console.WriteLine("past first check");
                int version = packet.g4();
                if (version == ConfigurationManager.Config.GameServerConfiguration.Version)
				{
                    Console.WriteLine("second check - version correct");
                    Boolean isLowMemory = packet.g1() == 1;

                    for (int i = 0; i < 13; i++)//this obviously changes with revision, but can stay in engine because we can derive it from Update Server
                    {
                        packet.g4(); //something to do with cache indices, I think it's CRC32 checksum return verification?? Will read back and see.
                    }

                    /** Okay here's what's going on here
                     * first 4 ints are Isaac seed from client (woohoo!)
                     * 
                     */
                    for (int i = 0; i < 24; i++)
                    {
                        packet.g1(); //not really sure what this is, needs investigation
                    }

                    int something = packet.g1(); //dunno man, but we're about to check it
                    if (something != 10)
                        packet.g1(); //ahuh, who knows dude

                    long clientKey = packet.g8(); //client's session key?

                    long reportedServerKey = packet.g8(); //I believe this is that key we gave it earlier that we generated

                    //some logic here, duh

                    String username = TextUtils.longToPlayerName((long)packet.g8());
                    String password = TextUtils.getRS2String(packet);
                    Console.WriteLine("Username: " + username + ", Password: " + password); //this is garbled I think because of the lack of RSA encryption. Need to look at that, but otherwise it's working to here.

                    Packet loginReply = LoginHandler.Login(session, username, password);
                    session.Stream.WriteByte((byte) loginReply._opcode);
                    session.Stream.Write(loginReply.toByteArray());

                } else
                {
                    session.Stream.Write(LoginHandler.GenerateReply(session, LoginHandler.LoginReturnCode.GAME_UPDATED).toByteArray());
                }
            }
            else
            {
                Console.WriteLine("Disconnecting Socket for login packet error");
                SocketManager.Disconnect(session);
            }

        }
    }
}
