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
            //where the real packets get decoded :o not this pre-login nonsense
        }
    }
}
