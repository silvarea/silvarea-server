using System.Net.Sockets;
using System.Text;
using Silvarea.Cache;

namespace Silvarea.Network
{

    public enum ProtocolOpcode
    {
        LOGIN = 14,
        UPDATE = 15
    }

    public class ProtocolDecoder
    {

        static int temp = 0;

        public static void Decode(Session session, int size)
        {
            // TODO: Fix this trash
            int opcode = -1;
            byte[] data = session.inBuffer;
            if (session.CurrentState != RS2ConnectionState.UPDATE)
            {
                opcode = session.inBuffer[0];
                data = new byte[session.inBuffer.Length - 1];
                Array.Copy(session.inBuffer, 1, data, 0, data.Length);
            }
            Packet packet = new Packet(opcode, data);

            switch (session.CurrentState)
            {
                case RS2ConnectionState.INITIAL:
                    Console.WriteLine("initial state");
                    if (opcode == (byte)ProtocolOpcode.UPDATE)
                    {
                        var versionId = packet.g4();

                        // TODO: Don't hardcode this
                        if (versionId == 410)
                        {
                            Console.WriteLine("initial state success");
                            session.Stream.Write([0]);
                            session.CurrentState = RS2ConnectionState.UPDATE;
                        }
                    }
                    break;
                case RS2ConnectionState.UPDATE:
                    int read = 0;
                    while ((size - read) > 3/* && session.Socket.Connected*/)
                    {
                        var requestType = packet.g1();
                        var indexNumber = packet.g1();
                        var fileNumber = packet.g2();
                        if (requestType < 2) //0 = non-urgent, 1 = urgent cache request
                        {
                            try
                            {
                                byte[] returnData = UpdateServer.getRequest(indexNumber, fileNumber);
                                session.Stream.Write(returnData);
                            }
                            catch (Exception e)
                            {
                                SocketManager.Disconnect(session);
                                Console.WriteLine("Some error for sure: " + e.Message);
                                if (e.InnerException != null)
                                    Console.WriteLine("Some kinda exception for sure: " + e.InnerException.Message);
                                return;
                            }
                        } else
                        {
                            Console.WriteLine("Clear queue request received from client");
                        }
                        read += 4;
                    }
                    break;
            }
        }
    }
}
