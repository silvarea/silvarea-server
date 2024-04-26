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
	}

	public class GameServerConfiguration
	{
		public int Version { get; set; }

		public string CachePath { get; set; }

		//TODO Use key instead to derive values
		public string modulus { get; set; }

		public string exponent { get; set; }
	}
}
