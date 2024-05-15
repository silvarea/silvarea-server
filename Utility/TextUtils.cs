using Silvarea.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silvarea.Utility
{
    public class TextUtils
    {

        private static readonly char[] VALID_CHARS = {
        '_', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i',
        'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's',
        't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2',
        '3', '4', '5', '6', '7', '8', '9'
    };

        public static String longToPlayerName(long l)
        {
            if (l <= 0L || l >= 0x5b5b57f8a98a5dd1L)
            {
                return null;
            }
            if (l % 37L == 0L)
            {
                return null;
            }
            int i = 0;
            char[] ac = new char[12];
            while (l != 0L)
            {
                long l1 = l;
                l /= 37L;
                ac[11 - i++] = VALID_CHARS[(int)(l1 - l * 37L)];
            }
            return new String(ac, 12 - i, i);
        }

        public static long playerNameToLong(String s)
        {
            var name = s.ToCharArray();
            long l = 0L;
            for (int i = 0; i < s.Length && i < 12; i++)
            {
                var c = name[i];

                l *= 37L;
                if (c >= 65 && c <= 90)
                {
                    l += (1 + c) - 65;
                }
                else if (c >= 97 && c <= 122)
                {
                    l += c - 96;
                }
                else if (c >= 48 && c <= 57)
                {
                    l += (27 + c) - 48;
                }
            }
            while (l % 37L == 0L && l != 0L)
            {
                l /= 37L;
            }
            return l;
        }

        public static String getRS2String(Packet packet)
        {
            StringBuilder s = new StringBuilder();
            int c;
            while (packet.Position != packet.Length && (c = packet.g1()) != 0) {
                s.Append((char)c);
            }
            return s.ToString();
        }

    }
}
