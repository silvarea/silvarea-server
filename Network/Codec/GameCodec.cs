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
            if (packet._opcode == -1)
            {
                session.Stream.Write(packet.toByteArray());
                return;
            }

            encodedPacket.p1((byte) (packet._opcode += session.outCipher.val()));

            int size = (int) packet.Length;//load in packet size array from client thru config loader (io.json) and check sizes here.

            if (size == -1) //VAR_BYTE
            {
                encodedPacket.p1((byte) packet.Length);
            } 
            else if (size == -2) //VAR_SHORT
            {
                encodedPacket.p2((short) packet.Length);
            }
            encodedPacket.pdata(packet.toByteArray(), (int) packet.Length);
            session.Stream.Write(encodedPacket.toByteArray());
        }

        public static void Decode(Session session, int size)
        {
            if (size < 1)
                return;

            Packet packet = new Packet(session.inBuffer);
            int opcode = (packet.g1() - session.inCipher.val()) & 0xff;
            //opcode -= session.inCipher.val();

            int expectedSize = size - 1;//load in packet size array thru config load (io.json). Unfortunately, I don't think this one is in the client, but this will try to handle unknowns. The risk is if the client sends more than one packet in a stream, which I don't think it will do.

            if (size == -1)
            {
                Console.WriteLine("We shouldn't be here");
                expectedSize = packet.g1();
            }
            //TODO Don't hardcode this
            if (opcode == 153)
            {
                int known = packet.g4();//has set value in client... 1057001181
                int checkVal = packet.g1() - session.inCipher.val();//Should be 91. This was throwing me off, advancing Isaac Cipher by 1... this is absolutely necessary to handle when using Isaac!
                Console.WriteLine($"Packet 153 (Isaac Verify?) handled, known = {known}, checkVal = {checkVal}");
                return;
            }

            //from here we can send the packet thru our PacketManager. Since the position information travels with the packet, it will be read correctly even though it still contains the opcode and potentially size information.
            //PacketManager.handle(opcode, packet, expectedSize);

            Console.WriteLine($"Decoding packet - opcode = {opcode}, size = {expectedSize}");
        }
    }
}
