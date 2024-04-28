using Org.BouncyCastle.Math;
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
                        Console.WriteLine("Login... what is this? " + packet.g1());
                        Random rand = new Random();
                        session.serverKey = ((long)(rand.NextDouble() * 99999999D) << 32) + ((long)(rand.NextDouble() * 99999999D));//might not need to save this, actually. Not here, anyway. Further down.
                        Packet loginReply = new Packet(-1, new byte[9]);
                        loginReply.p1(0);
                        loginReply.p8(session.serverKey);
                        Console.WriteLine("Server key is: " + session.serverKey);
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
            if (size < 2)
                return;
            Packet packet = new Packet(session.inBuffer);
            session.CurrentState = (RS2ConnectionState)packet.g1(); //try/catch here? in case the result can't resolve from mismatch -- ahh, also 18 is RECONNECTING from a dropped connection, 16 is brand new login
            int reportedSize = packet.g1();
            if ((session.CurrentState == RS2ConnectionState.GAME || session.CurrentState == RS2ConnectionState.RECONNECT) && reportedSize == (size - 2)) //basically just verifies this login attempt isn't an accident or a fumble, think we need support for 18 here as well?
            {
                int version = packet.g4();
                if (version == ConfigurationManager.Config.GameServerConfiguration.Version)
                {
                    Boolean isLowMemory = packet.g1() == 1;
                    for (int i = 0; i < UpdateServer.hashes.Length; i++)
                    {
                        if (packet.g4() != UpdateServer.hashes[i])
                        {
                            session.Stream.Write(LoginHandler.GenerateReply(session, LoginHandler.LoginReturnCode.GAME_UPDATED).toByteArray());
                            SocketManager.Disconnect(session);
                        }
                    }

                    int encryptedSize = packet.g1();

                    byte[] encryptedData = new byte[encryptedSize];
                    packet.Read(encryptedData, 0, encryptedSize);
                    Packet decryptedPacket = new Packet(new BigInteger(encryptedData).ModPow(new BigInteger(ConfigurationManager.Config.GameServerConfiguration.Exponent), new BigInteger(ConfigurationManager.Config.GameServerConfiguration.Modulus)).ToByteArray());


                    int blockOpcode = decryptedPacket.g1();
                    if (blockOpcode != 10)
                    {
                        session.Stream.Write(LoginHandler.GenerateReply(null, LoginHandler.LoginReturnCode.UNABLE_TO_COMPLETE).toByteArray());
                        SocketManager.Disconnect(session);
                    }

                    int clientKey1 = decryptedPacket.g4();
                    int clientKey2 = decryptedPacket.g4();
                    long incomingServerKey = decryptedPacket.g8();

                    if (session.serverKey != incomingServerKey)
                    {
                        session.Stream.Write(LoginHandler.GenerateReply(null, LoginHandler.LoginReturnCode.BAD_SESSION_ID).toByteArray());
                        SocketManager.Disconnect(session);
                    }

                    int[] cipherKey = { clientKey1, clientKey2, (int)(incomingServerKey >> 32), (int)incomingServerKey };

                    session.inCipher = new Isaac(cipherKey);

                    for (int i = 0; i < cipherKey.Length; i++)
                        cipherKey[i] += 50;

                    session.outCipher = new Isaac(cipherKey);

                    int uid = decryptedPacket.g4();

                    string username = TextUtils.longToPlayerName((long)decryptedPacket.g8());
                    string password = TextUtils.getRS2String(decryptedPacket);
                    Console.WriteLine("Username: " + username + ", Password: " + password);

                    Packet loginReply = LoginHandler.Login(session, username, password, isLowMemory);
                    session.Stream.Write(loginReply.toByteArray());

                    //in encoder, remember to opcode += session.outCipher.val(); and inverse in decoder

                }
                else
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
