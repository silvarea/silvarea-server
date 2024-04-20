using System.Net.Http.Headers;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.IO.Compression;
using System.Threading.Tasks.Dataflow;
using System.Buffers.Binary;

namespace Silvarea.Cache
{
    public class UpdateServer
    {

        private static byte[] _crc = new byte[0];

        public static void init(string path)
        {
            Console.Write("Loading cache...");
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

            MemoryStream stream = new MemoryStream(4048);
            BinaryWriter buffer = new BinaryWriter(stream);
            int length = Cache.getIndex(255).getLength() / 6; //wasn't dividing this by 6
            buffer.Write((byte) 0);
            buffer.Write((uint) length * 8); //wasn't multiplying this by 8
            new CRC32();
            for (int file = 0; file < length; file++) 
            {
                int hash = (int)CRC32.CalculateCrc32(Cache.getIndex(255).getFile(file));
                Console.WriteLine("CRC32 hash = " + hash);
                buffer.Write((int) hash);
                MemoryStream crcDecompressed = new MemoryStream(new FileDecompressor(Cache.getIndex(255).getFile(file)).decompress());
                BinaryReader crcReader = new BinaryReader(crcDecompressed);
                int version = crcReader.ReadByte();
                Console.WriteLine("file: " + file + ", version: " + version);
                int revision = version >= 6 ? crcReader.ReadInt32() : 0;
                buffer.Write((uint) revision);
            }
            //stream.Write(new byte[4048 - stream.Length], (stream.Length - 1), (4048 - stream.Length));
            Console.WriteLine("should be 4048: " + stream.Capacity);
            _crc = stream.ToArray();
            Array.Resize(ref _crc, 4048);
            Console.WriteLine("should also be 4048: " + _crc.Length);
        }

        public static MemoryStream getRequest(int index, int file)
        {
            var cache = getCacheFile(index, file);
            Console.WriteLine("Reading file " + file + " index " + index + " now, size = " + cache.Length);
            MemoryStream buffer = new MemoryStream((cache.Length - 2) + ((cache.Length - 2) / 511) + 8);
            BinaryWriter bufferWriter = new BinaryWriter(buffer);
            Console.WriteLine("bufferlen = " + buffer.Capacity);
            bufferWriter.Write((byte) index);
            bufferWriter.Write((short) file);
            Console.WriteLine(cache[1] + ", " + cache[2] + ", " + cache[3] + ", " + cache[4]);
            int len = 
                     (((cache[4] & 0xff) << 24) +
                     ((cache[3] & 0xff) << 16) +
                     ((cache[2] & 0xff) << 8) +
                     (cache[1] & 0xff)) + 9;//Why the fuck do I have to reverse this for it to work? There's gotta be something happening before this to screw that up. Makes no sense.
            
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
                    bufferWriter.Write((byte) 0xFF);
                    c = 1;
                }
                bufferWriter.Write((byte)cache[i]);
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
