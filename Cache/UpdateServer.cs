using System.Net.Http.Headers;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.IO.Compression;

namespace Silvarea.Cache
{
    public class UpdateServer
    {

        private static byte[] _crc = new byte[0];

        public static void init()
        {
            MemoryStream stream = new MemoryStream(4048);
            BinaryWriter buffer = new BinaryWriter(stream);
            int length = Cache.getIndex(255).getLength();
            buffer.Write((byte) 0);
            buffer.Write((uint) length);
            new CRC32();
            for (int file = 0; file < length; file++) 
            {
                uint hash = CRC32.CalculateCrc32(Cache.getIndex(255).getFile(file));
                buffer.Write((int) hash);
                MemoryStream crcDecompressed = new MemoryStream(new FileDecompressor(Cache.getIndex(255).getFile(file)).decompress());
                BinaryReader crcReader = new BinaryReader(crcDecompressed);
                int version = crcReader.Read();
                int revision = version >= 6 ? crcReader.ReadInt32() : 0;
                buffer.Write((uint) revision);
            }

        }

        public static MemoryStream getRequest(int index, int file)
        {
            var cache = getCacheFile(index, file);
            MemoryStream buffer = new MemoryStream((cache.Length - 2) + ((cache.Length - 2) / 511) + 8);
            BinaryWriter bufferWriter = new BinaryWriter(buffer);
            bufferWriter.Write((byte) index);
            bufferWriter.Write((short) file);
            int len = (((cache[1] & 0xff) << 24) + ((cache[2] & 0xff) << 16) + ((cache[3] & 0xff) << 8) + (cache[4] & 0xff)) + 9;
            if (cache[0] == 0)
            {
                len -= 4;
            }
            int c = 3;
            for (int i = 0; i < len; i++)
            {
                if (c == 512)
                {
                    bufferWriter.Write((byte) 0xFF);
                    c = 1;
                }
                bufferWriter.Write(cache[i]);
                c++;
            }
            buffer.Close();
            return buffer;
        }

        private static byte[] getCacheFile(int index, int file)
        {

            if (index == 255 && file == 255)
            {
            	return _crc;
            }

            return Cache.getCacheFile(index, file);

        }

    }
}
