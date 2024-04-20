using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.BZip2;
using Silvarea.Network;

namespace Silvarea.Cache
{
    internal class FileDecompressor
    {

        private readonly CacheFile _file;

        public FileDecompressor(byte[] data)
        {
            Packet packet = new Packet(data);
            MemoryStream buffer = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(buffer);
            int compression = reader.ReadByte();// packet.g1();
            //TODO Aw fuckin' christ. The client does networking in Big Endian and BinaryReader reads in Little Endian... Either find a better Reader or it's time to make a Packet/Stream Class!
            byte[] uncompressedData = reader.ReadBytes(4);
            Array.Reverse(uncompressedData);
            int uncompressedSize = BitConverter.ToInt32(uncompressedData);//packet.g4();
            int cacheSize;
            if (compression != 0)
            {
                byte[] cacheSizeData = reader.ReadBytes(4);
                Array.Reverse(cacheSizeData);
                cacheSize = BitConverter.ToInt32(cacheSizeData);//packet.g4();
            } else
            {
                cacheSize = uncompressedSize;
            }
            Console.WriteLine("Decompressing file - size = " + cacheSize + ", uncompressed = " + uncompressedSize + ", compression type = " + compression);
            _file = new CacheFile(buffer, compression, uncompressedSize, cacheSize);
        }

        public byte[] decompress()
        {
            Console.WriteLine("Trying to create byte array of size: " + _file.CacheSize);
            byte[] newData = new byte[_file.CacheSize];
            switch (_file._Compression)
            {
                case CacheFile.Compression.NONE:
                    Console.WriteLine("Compression type: NONE");
                    Array.Copy(_file.toByteArray(), 5, newData, 0, _file.CacheSize);
                    break;
                case CacheFile.Compression.BZIP: //Jagex uses a headerless BZip2 compression, so every BZip2 package I tried gave Invalid Header errors. Forced to implement custom decompression. :( I guess at least it keeps 3rd party packages out of engine so far.
                    Console.WriteLine("Compression type: BZIP");
                    byte[] bzipHeader = new byte[] {(byte)'B', (byte)'Z', (byte)'h', (byte)'1'};
                    byte[] data = _file.toByteArray().Skip(5).ToArray();//maybe 9?


                    MemoryStream stream = new MemoryStream(data);
                    Console.WriteLine("streamlen: " + stream.Length);
                    stream.Write(bzipHeader, 0, bzipHeader.Length);
                    stream.Seek(0, SeekOrigin.Begin);
                    BZip2InputStream bz2decompress = new BZip2InputStream(stream);
                    //Console.WriteLine("bzipd len = " + bz2decompress.);
                    bz2decompress.Read(newData);
					//finish this, idiot
					break;
                case CacheFile.Compression.GZIP:
                    Console.WriteLine("Compression type: GZIP");
                    byte[] gzipData = _file.toByteArray().Skip(9).ToArray();
                    MemoryStream gzipStream = new MemoryStream(gzipData);
                    GZipStream gzdecompress = new GZipStream(gzipStream, CompressionMode.Decompress);
                    gzdecompress.Read(newData, 0, newData.Length);
                    break;
            }


            var directory = Path.GetDirectoryName($"C:\\Users\\Brian\\Testing\\{_file.UncompressedSize}.txt");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            //File.WriteAllBytes(directory, newData);

            FileStream fs = new FileStream(directory, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(newData);
            }
            fs.Close();

			//Console.WriteLine("decomp'd data: " + (sbyte)newData[10] + ", " + (sbyte)newData[11] + ", " + (sbyte)newData[12] + ", " + (sbyte)newData[13] + ", " + (sbyte)newData[14]);
			return newData;
        }

    }
}
