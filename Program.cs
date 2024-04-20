using Silvarea.Network;
using Silvarea.Cache;

namespace Silvarea
{
	public class Program
	{

		public static void Main()
		{
			UpdateServer.init("../../../data/cache");
			SocketManager socketManager = new SocketManager();
			socketManager.Start();
		}
	}
}


