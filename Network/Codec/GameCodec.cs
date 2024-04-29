using Silvarea.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silvarea.Network.Codec
{
    internal class GameCodec
    {

        public static void Encode(Session session, Packet packet)
        {
            Packet encodedPacket = new Packet();
            if (packet.Opcode == -1)
            {
                session.Stream.Write(packet.toByteArray());
                return;
            }

            int size = (int)ConfigurationManager.Config.PacketSizes.OutgoingPackets[packet.Opcode];

            encodedPacket.p1((byte)(packet.Opcode += session.outCipher.val()));

            if (size == -1) //VAR_BYTE
            {
                encodedPacket.p1((byte)packet.Length);
            }
            else if (size == -2) //VAR_SHORT
            {
                encodedPacket.p2((short)packet.Length);
            }
            encodedPacket.pdata(packet.toByteArray(), (int)packet.Length);
            session.Stream.Write(encodedPacket.toByteArray());
        }

        public static void Decode(Session session, Packet packet, int size)
        {
            if (size < 1)
                return;

            int readableBytes = size;

            int opcode = (packet.g1() - session.inCipher.val()) & 0xff;
            readableBytes -= 1;

            int expectedSize = ConfigurationManager.Config.PacketSizes.IncomingPackets[opcode];//load in packet size array thru config load (io.json). Unfortunately, I don't think this one is in the client, but this will try to handle unknowns. The risk is if the client sends more than one packet in a stream, which I don't think it will do.

            if (expectedSize == -1)
            {
                expectedSize = packet.g1();
                readableBytes -= 1;
            }
            else if (expectedSize == -3)
            {
                expectedSize = (size - 1);//If more than one packet is received inbetween reads and one is unhandled (-3 length) we will lose all of the packets in the read, throwing the Isaac cipher out of sync and ruining the connection.
            }
            readableBytes -= (expectedSize);

            byte[] decodedData = new byte[expectedSize];
            packet.Read(decodedData);
            Packet decodedPacket = new Packet(opcode, decodedData);

            if (readableBytes > 0)
            {
                byte[] data = new byte[readableBytes];
                packet.Read(data);
                Decode(session, new Packet(data), readableBytes);
            }
            //from here we can send the packet thru our PacketManager. Since the position information travels with the packet, it will be read correctly even though it still contains the opcode and potentially size information.
            //PacketManager.handle(decodedPacket);

        }
    }
}
