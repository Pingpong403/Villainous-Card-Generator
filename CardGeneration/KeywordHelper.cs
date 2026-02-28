using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Villainous_Card_Generator.CardGeneration
{
	public static class KeywordHelper
	{
		public static HashSet<string> GetKeywords()
		{
			HashSet<string> keywords = [];

			foreach (string line in MiscHelper.GetTextFilesLines("Keywords"))
			{
				// Split line individual keywords
				string[] variations = line.Split("|");

				// Add each to hashset
				foreach (string variation in variations) keywords.Add(variation);
			}

			return keywords;
		}

		public static Dictionary<string, string> GetColorMapping()
		{
			Dictionary<string, string> baseKeywordColors = [];
			Dictionary<string, string> keywordsAndColors = [];

			// Populate dictionary to hold each singular keyword and its corresponding color
			foreach (string line in MiscHelper.GetTextFilesLines("Colors"))
			{
				string[] lineSplit = line.Split("|");
				// If color exists in -TextFiles\Colors.txt, use that color.
				// If not, use color found in config\color-config.txt
				if (lineSplit.Length == 1)
				{
					// Search through color-config.txt for correct color
					string searchParam = lineSplit[0].ToLower() + "Color";
					baseKeywordColors[lineSplit[0]] = ConfigHelper.GetConfigValue("color", searchParam);
				}
				else
				{
					// First part is the base keyword, second part is its color
					baseKeywordColors[lineSplit[0]] = lineSplit[1];
				}
			}

			// Link each keyword variant to its singular form and therefore its correct color
			foreach (string line in MiscHelper.GetTextFilesLines("Keywords"))
			{
				string[] lineSplit = line.Split("|");
				foreach (string variant in lineSplit)
				{
					if (variant != "")
					{
						if (!baseKeywordColors.TryGetValue(lineSplit[0], out string? value))
						{
							keywordsAndColors[variant] = ConfigHelper.GetConfigValue("color", "fontColor");
						}
						else
						{
							keywordsAndColors[variant] = value == "" ? ConfigHelper.GetConfigValue("color", "fontColor") : value;
						}
					}
				}
			}

			return keywordsAndColors;
		}
	}
}