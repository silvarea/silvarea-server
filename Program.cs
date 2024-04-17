using Silvarea.Network;

namespace Silvarea
{
	public class Program
	{

		public static void Main()
		{
			SocketManager socketManager = new SocketManager();
			socketManager.Start();
		}
	}
}


