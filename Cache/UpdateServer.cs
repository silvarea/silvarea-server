using System.Net.Http.Headers;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.IO.Compression;
using System.Threading.Tasks.Dataflow;
using System.Buffers.Binary;
using Silvarea.Network;

namespace Silvarea.Cache
{
    public class UpdateServer
    {

        private static byte[] _crc = new byte[0];

        public static void init(string path)
        {
            Console.WriteLine("Loading cache...");
            int total = 0;
            foreach (string filePath in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"\" + path))
            {
                if (filePath.Contains("main_file_cache.idx"))
                {
                    int idx = int.Parse(filePath.Split(".idx")[1]);
                    if (idx != 255 && idx > total)
                    {
                        total = idx;
                    }
                }
            }
            Cache.Index255 = new CacheIndex(path, 255, null);
            Cache.Indices = new CacheIndex[total + 1];
            for (int file = 0; file < Cache.Indices.Length; file++)
            {
                Cache.Indices[file] = new CacheIndex(path, file, Cache.Index255);
            }

            byte[] data = new byte[4048];
            Packet packet = new Packet((byte) 0, data);

            int length = Cache.getIndex(255).getLength() / 6;
            packet.p1(0);
            packet.p4(length * 4); //multiply by 8 for 460+
            new CRC32();
            for (int file = 0; file < length; file++) 
            {
                int hash = (int)CRC32.CalculateCrc32(Cache.getIndex(255).getFile(file));
                packet.p4(hash);
                Packet crcDecompressed = new Packet(new FileDecompressor(Cache.getIndex(255).getFile(file)).decompress());
                int version = crcDecompressed.g1();
                int revision = version >= 6 ? crcDecompressed.g4() : 0;
                //packet.p4(revision);//Only send in 460+
            }
            _crc = packet.toByteArray();
            //Array.Resize(ref _crc, 4048);
        }

        public static byte[] getRequest(int index, int file)
        {
            var cache = getCacheFile(index, file);
            byte[] data = new byte[(cache.Length - 2) + ((cache.Length - 2) / 511) + 8];
            Packet packet = new Packet(0, data);
            packet.p1((byte) index);
            packet.p2((short) file);
            int len = 
                     (((cache[1] & 0xff) << 24) +
                     ((cache[2] & 0xff) << 16) +
                     ((cache[3] & 0xff) << 8) +
                     (cache[4] & 0xff)) + 9;
            
            if (cache[0] == 0)
            {
                len -= 4;
            }
            int c = 3;
            for (int i = 0; i < len; i++)
            {
                if (c == 512)
                {
                    packet.p1(0xFF);
                    c = 1;
                }
                packet.p1(cache[i]);
                c++;
            }
            return packet.toByteArray();
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
