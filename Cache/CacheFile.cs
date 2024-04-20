using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silvarea.Network;

namespace Silvarea.Cache
{
    public class CacheFile
    {
        public Compression _Compression { get; set;}

        public MemoryStream Data { get; set;}

        public int UncompressedSize { get; set;}

        public int CacheSize { get; set;}

        public enum Compression
        {
            NONE = 0,

            BZIP = 1,

            GZIP = 2

        }

        public CacheFile(MemoryStream data, int compression, int uncompressedSize, int cacheSize)
        {
            Data = data;
            _Compression = ConfigureCompression(compression);
            UncompressedSize = uncompressedSize;
            CacheSize = cacheSize;
        }

        public Compression ConfigureCompression(int compression)
        {
            if (compression == 0)
            {
                return Compression.NONE;
            } else if (compression == 1)
            {
                return Compression.BZIP;
            } else
            {
                return Compression.GZIP;
            }
        }

        public byte[] toByteArray()
        {
            return Data.ToArray();//Data.toByteArray();
        }

    }
}
