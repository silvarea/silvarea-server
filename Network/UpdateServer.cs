using System.Net.Http.Headers;

namespace Silvarea.Network
{
	public class UpdateServer
	{

		private byte[] _crc;

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
