using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silvarea.Cache
{
    public class CacheFile
    {
        private Compression _compression { get; set;}

        private MemoryStream _data { get; set;}

        private int _uncompressedSize { get; set;}

        private int _cacheSize { get; set;}

        public enum Compression
        {
            NONE = 0,

            BZIP = 1,

            GZIP = 2

        }

        public CacheFile(MemoryStream data, int compression, int uncompressedSize, int cacheSize)
        {
            _data = data;
            _compression = ConfigureCompression(compression);
            _uncompressedSize = uncompressedSize;
            _cacheSize = cacheSize;
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
            return _data.ToArray();
        }

    }
}
