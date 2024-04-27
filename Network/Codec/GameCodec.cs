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
            //in encoder, remember to opcode += session.outCipher.val(); and inverse in decoder
        }

        public static void Decode(Session session, int size)
        {
            //where the real packets get decoded :o not this pre-login nonsense
        }
    }
}
