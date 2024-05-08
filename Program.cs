using Silvarea.Network;
using Silvarea.Cache;
using Silvarea.Utility;

namespace Silvarea
{
	public class Program
	{

		public static void Main()
		{
			ConfigurationManager.Initialize();

			TestingShit();

			UpdateServer.init(ConfigurationManager.Config.GameServerConfiguration.CachePath);

			SocketManager socketManager = new SocketManager();

			socketManager.Start();

		}

		public static void TestingShit()
		{
			Packet bitBlock = new Packet();

			bitBlock.openBitBuffer();

			//bitBlock.pBits(1, 0);
			//bitBlock.pBits(4, 3);
			bitBlock.pBits(7, 13);

			bitBlock.closeBitBuffer();

			Packet bitsTime = new Packet(bitBlock);

			//Console.WriteLine($"GBits is {bitsTime.gBits(1)}");
			//Console.WriteLine($"GBits is {bitsTime.gBits(4)}");
			Console.WriteLine($"GBits is {bitsTime.gBits(7)}");

			Packet packet = new Packet();

			packet.p1(1);
			packet.p2(50);

			packet.Position = 0;

			Console.WriteLine($"G1 is {packet.g1()}");
			Console.WriteLine($"G2 is {packet.g2()}");
		}
	}
}


