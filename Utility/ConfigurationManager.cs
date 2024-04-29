using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silvarea.Utility
{
	public static class ConfigurationManager
	{

		public static Configuration Config { get; private set; }

		// Absolutely, 100% lifted this code from ACE and updated it slightly. 
		public static void Initialize()
		{
			Console.WriteLine("Importing server configuration...");
			var configFile = @"config.json";

			var directoryName = Path.GetDirectoryName(configFile);
			var fileName = Path.GetFileName(configFile) ?? "config.json";

			string pathToUse;

			if (string.IsNullOrWhiteSpace(directoryName))
			{

				directoryName = Environment.CurrentDirectory;

				pathToUse = Path.Combine(directoryName, fileName);

				if (!File.Exists(pathToUse))
				{
					var executingAssemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

					directoryName = Path.GetDirectoryName(executingAssemblyLocation);

					if (directoryName != null) pathToUse = Path.Combine(directoryName, fileName);
				}
			}
			else
			{
				pathToUse = configFile;
			}

			try
			{
				if (!File.Exists(pathToUse))
				{
					Console.WriteLine("File does not exist. Please copy the config.json file from the Data folder to the ");
					throw new Exception("Missing configuration file!");
				}

				var fileText = File.ReadAllText(pathToUse);

				Config = JsonConvert.DeserializeObject<Configuration>(fileText);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An exception occured while loading the configuration file!");
				Console.WriteLine($"Exception: {ex.Message}");
			}
            Console.WriteLine("Server configuration loaded.");
        }
	}
}
