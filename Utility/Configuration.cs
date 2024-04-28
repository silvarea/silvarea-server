using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Silvarea.Utility
{
	public class Configuration
	{
		public GameServerConfiguration GameServerConfiguration { get; set; }

		public PacketSizes PacketSizes { get; set; }
	}

	public class GameServerConfiguration
	{
		public int Version { get; set; }

		public string CachePath { get; set; }

		//TODO Use key instead to derive values
		public string Modulus { get; set; }

		public string Exponent { get; set; }
	}

	public class PacketSizes
	{
		public Dictionary<int, int> IncomingPackets { get; set; }

		public Dictionary<int, int> OutgoingPackets { get; set; }

	}
}
