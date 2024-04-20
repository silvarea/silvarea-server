using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silvarea.Cache
{


    public class CRC32
    {
        private const uint Polynomial = 0xEDB88320;
        private static uint[] _table;

        static CRC32()
        {
            _table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                    {
                        crc = (crc >> 1) ^ Polynomial;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
                _table[i] = crc;
            }
        }

        public static uint CalculateCrc32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in data)
            {
                byte index = (byte)(((crc) & 0xff) ^ b);
                crc = ((uint)_table[index] ^ (crc >> 8));
            }
            return ~crc;
        }
    }
}
    
