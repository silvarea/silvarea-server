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
            _file = new CacheFile(buffer, compression, uncompressedSize, cacheSize);
        }

        public byte[] decompress()
        {
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
                    byte[] data = _file.toByteArray().Skip(5).ToArray();
                    MemoryStream stream = new MemoryStream(data);
                    stream.Write(bzipHeader, 0, bzipHeader.Length);
                    stream.Seek(0, SeekOrigin.Begin);
                    BZip2InputStream bz2decompress = new BZip2InputStream(stream);
                    bz2decompress.Read(newData);
                    bz2decompress.Close();
					break;
                case CacheFile.Compression.GZIP:
                    Console.WriteLine("Compression type: GZIP");
                    byte[] gzipData = _file.toByteArray().Skip(9).ToArray();
                    MemoryStream gzipStream = new MemoryStream(gzipData);
                    try
                    {
                        GZipStream gzdecompress = new GZipStream(gzipStream, CompressionMode.Decompress);
                        int totalRead = 0, bytesRead;
                        while ((bytesRead = gzdecompress.Read(newData, totalRead, newData.Length - totalRead)) > 0)
                        {
                            totalRead += bytesRead;
                        }
                        gzdecompress.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }
                    break;
            }
			return newData;
        }

    }
}
