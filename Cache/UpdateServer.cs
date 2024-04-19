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

            return new MemoryStream(cache);
        }

        private static byte[] getCacheFile(int index, int file)
        {

            if (index == 255 && file == 255)
            {
            	return _crc;
            }

            return null;

        }

    }
}
