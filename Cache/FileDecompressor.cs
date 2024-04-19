using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.BZip2;

namespace Silvarea.Cache
{
    internal class FileDecompressor
    {

        private readonly CacheFile _file;

        public FileDecompressor(byte[] data)
        {
            MemoryStream buffer = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(buffer);
            int compression = reader.ReadByte();
            int uncompressedSize = reader.ReadInt32();
            int cacheSize = compression != 0 ? reader.ReadInt32() : uncompressedSize;
            _file = new CacheFile(buffer, compression, uncompressedSize, cacheSize);
        }

        public byte[] decompress()
        {
            byte[] newData = new byte[_file.CacheSize];
            switch (_file._Compression)
            {
                case CacheFile.Compression.NONE:
                    Array.Copy(_file.toByteArray(), 5, newData, 0, _file.CacheSize);
                    break;
                case CacheFile.Compression.BZIP:
                    BZip2InputStream bz2decompress = new BZip2InputStream(_file.Data);
                    bz2decompress.Read(newData, 9, newData.Length);
                    break;
                case CacheFile.Compression.GZIP:
                    GZipStream gzdecompress = new GZipStream(_file.Data, CompressionMode.Decompress);
                    gzdecompress.Read(newData, 9, newData.Length);
                    break;
            }
            return newData;
        }

    }
}
