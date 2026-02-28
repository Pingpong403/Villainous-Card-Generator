using System;
using System.IO;

namespace Villainous_Card_Generator.CardGeneration
{
	public static class ConfigHelper
	{
		public static string GetConfigValue(string configFile, string key)
		{
			string path = PathHelper.GetFullPath(Path.Combine("config", configFile + "-config.txt"));
			if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Config file not found: {path}");
            }

			string? line;
			try
			{
				// Pass the file path to the StreamReader constructor
				StreamReader sr = new(path);

				// Read the first line of text
				line = sr.ReadLine();

				//Continue to read until you reach end of file
				while (line != null)
				{
					// Split line into key, value pair
					string[] pair = line.Split(":");

					// If key matches given key, return the value
					if (string.Equals(pair[0], key))
					{
						return pair[1];
					}

					// Read the next line
					line = sr.ReadLine();
				}
				//close the file
				sr.Close();
			}
			catch(Exception e)
			{
				Console.WriteLine("Exception: " + e.Message);
			}
			
			return "";
		}

		public static Dictionary<string, string> GetAllValues(string configFile)
		{
			string path = PathHelper.GetFullPath(Path.Combine("config", configFile + "-config.txt"));
			if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Config file not found: {path}");
            }

			Dictionary<string, string> pairs = [];
			string? line;
			try
			{
				// Pass the file path to the StreamReader constructor
				StreamReader sr = new(path);

				// Read the first line of text
				line = sr.ReadLine();

				//Continue to read until you reach end of file
				while (line != null)
				{
					// Split line into key, value pair
					string[] pair = line.Split(":");

					// Assign to dictionary
					pairs[pair[0]] = pair[1];

					// Read the next line
					line = sr.ReadLine();
				}
				//close the file
				sr.Close();
			}
			catch(Exception e)
			{
				Console.WriteLine("Exception: " + e.Message);
			}
			
			return pairs;
		}
	}
}