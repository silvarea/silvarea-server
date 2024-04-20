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
            foreach (string filePath in Directory.GetFiles(path))
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

            int length = Cache.getIndex(255).getLength() / 6; //wasn't dividing this by 6
            packet.p1(0);//this is creating the crc file we read back for 255/255 request, this is writing in little endian. Make big endian all the way down to ensure data integrity
            packet.p4(length * 4); //multiply by 8 for 460+
            new CRC32();
            for (int file = 0; file < length; file++) 
            {
                int hash = (int)CRC32.CalculateCrc32(Cache.getIndex(255).getFile(file));
                Console.WriteLine("CRC32 hash = " + hash);
                packet.p4(hash);
                Packet crcDecompressed = new Packet(new FileDecompressor(Cache.getIndex(255).getFile(file)).decompress());
                int version = crcDecompressed.g1();
                Console.WriteLine("file: " + file + ", version: " + version);
                int revision = version >= 6 ? crcDecompressed.g4() : 0;//might be the change we have to make for 410
                //packet.p4(revision);//Only send in 460+
            }
            //stream.Write(new byte[4048 - stream.Length], (stream.Length - 1), (4048 - stream.Length));
            _crc = packet.toByteArray();
            Array.Resize(ref _crc, 4048);
            Console.WriteLine("should also be 4048: " + _crc.Length);
        }

        public static MemoryStream getRequest(int index, int file)
        {
            var cache = getCacheFile(index, file);
            Console.WriteLine("Reading file " + file + " index " + index + " now, size = " + cache.Length);
            byte[] data = new byte[(cache.Length - 2) + ((cache.Length - 2) / 511) + 8];
            Packet packet = new Packet(0, data);
            //MemoryStream buffer = new MemoryStream((cache.Length - 2) + ((cache.Length - 2) / 511) + 8);
            //BinaryWriter bufferWriter = new BinaryWriter(buffer);
            //Console.WriteLine("bufferlen = " + packet.Capacity);
            packet.p1((byte) index);
            packet.p2((short) file);
            Console.WriteLine(cache[1] + ", " + cache[2] + ", " + cache[3] + ", " + cache[4]);
            int len = 
                     (((cache[1] & 0xff) << 24) +
                     ((cache[2] & 0xff) << 16) +
                     ((cache[3] & 0xff) << 8) +
                     (cache[4] & 0xff)) + 9;//Why the fuck do I have to reverse this for it to work? There's gotta be something happening before this to screw that up. Makes no sense.
            
            if (cache[0] == 0)
            {
                len -= 4;
            }
            int c = 3;
            Console.WriteLine("len = " + len);
            for (int i = 0; i < len; i++)
            //for (int i = len; i > -1; i++)
            {
                if (c == 512)
                {
                    packet.p1((byte) 0xFF);
                    c = 1;
                }
                //Console.WriteLine("testcrc: " + (sbyte)cache[i]);
                packet.p1((sbyte) cache[i]);
                c++;
            }
            return new MemoryStream(packet.toByteArray());
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
