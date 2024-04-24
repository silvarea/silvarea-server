﻿using Silvarea.Network;
using Silvarea.Cache;
using Silvarea.Utility;
using Newtonsoft.Json;

namespace Silvarea
{
	public class Program
	{

		public static void Main()
		{

			ConfigurationManager.Initialize();

			UpdateServer.init(ConfigurationManager.Config.GameServerConfiguration.CachePath);
			SocketManager socketManager = new SocketManager();
			socketManager.Start();
		}

		public void loadConfig()
		{


		}
	}
}


