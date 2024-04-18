using System.Net.Http.Headers;

namespace Silvarea.Cache
{
    public class UpdateServer
    {

        private static byte[] _crc = new byte[0];

        public static void init()
        {
            //BufferedStream data = new BufferedStream();
            //BinaryWriter buffer = new BinaryWriter(data);
            //buffer.(Cache.getIndex(255).getLength() * 8);

        }

        public static MemoryStream getRequest(int index, int file)
        {

            var cache = getCacheFile(index, file);

            return new MemoryStream();
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
