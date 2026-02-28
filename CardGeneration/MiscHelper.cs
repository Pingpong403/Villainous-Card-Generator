using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Villainous_Card_Generator.CardGeneration
{
	public static class MiscHelper
	{
		public static bool CheckStructure()
		{
			// 1, 2, 4, 8, 16, 32 - higher number, more extreme
			int infractions = 0;
			if (!Directory.Exists(PathHelper.GetFullPath("config\\"))) infractions += 32;
			if (!Directory.Exists(PathHelper.GetFullPath("assets\\"))) infractions += 16;
			if (!Directory.Exists(PathHelper.GetFullPath("fonts\\"))) infractions += 8;
			if (!Directory.Exists(PathHelper.GetFullPath("Card Data\\"))) infractions += 7;
			else
			{
				if (!Directory.Exists(PathHelper.GetFullPath(Path.Combine("Card Data", "-TextFiles\\")))) infractions += 4;
				if (!Directory.Exists(PathHelper.GetFullPath(Path.Combine("Card Data", "-Layout\\")))) infractions += 2;
				if (!Directory.Exists(PathHelper.GetFullPath(Path.Combine("Card Data", "-Images\\")))) infractions += 1;
			}
			switch (infractions)
			{
				case 63:
					Console.WriteLine("Executable is not in project root. Please relocate to Villainous-Card-Generator folder.");
					return false;
				case int n when n < 63 && n >= 8:
					Console.WriteLine("Missing one or more vital configuration folders. Please redownload or relocate missing folders to the Villainous-Card-Generator folder.");
					return false;
				case 7:
					Console.WriteLine("Missing Card Data folder. Please redownload or relocate the folder to the Villainous-Card-Generator folder.");
					return false;
				case int n when n < 7 && n >= 1:
					Console.WriteLine("Missing one or more vital Card Data folders. Please ensure -TextFiles, -Layout, and -Images are placed in Card Data.");
					return false;
				case 0:
					return true;
				default:
					return false;
			}
		}

		public static List<string> GetTextFilesLines(string file)
		{
			List<string> lines = [];
			string path = PathHelper.GetFullPath(Path.Combine("Card Data", "-TextFiles", file + ".txt"));
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Text file not found: {path}");
            }

			string? line;
			try
			{
				// Pass the file path to the StreamReader constructor
				StreamReader sr = new(path);

				// Read the first line of text
				line = sr.ReadLine();

				// Continue to read until you reach end of file
				while (line != null)
				{
					// Skip empty lines
					if (line != "")
					{
						// '#' denotes a comment line
						if (line[0] != '#')
						{
							// Add the line to the list
							lines.Add(line);
						}
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

			return lines;
		}

		public static string CleanTitle(string title)
		{
			string cleanTitle = "";

			bool alreadyAddedSpace = false;
			foreach (char letter in title)
			{
				// For spaces, only add one between words
				if (letter == ' ')
				{
					if (!alreadyAddedSpace)
					{
						cleanTitle += ' ';
						alreadyAddedSpace = true;
					}
				}

				// For newline characters, remove them and add a space
				else if (letter == '%' || letter == '\n')
				{
					if (!alreadyAddedSpace)
					{
						cleanTitle += ' ';
						alreadyAddedSpace = true;
					}
				}

				// For everything else, only add the letter
				else
				{
					cleanTitle += letter;
					alreadyAddedSpace = false;
				}
			}

			return cleanTitle;
		}

		public static Point GetElementPos(string element)
		{
			int elementCenterX = int.Parse(ConfigHelper.GetConfigValue("layout", element + "CenterX"));
			int elementCenterY = int.Parse(ConfigHelper.GetConfigValue("layout", element + "CenterY"));
			return new Point(elementCenterX, elementCenterY);
		}

		public static bool ElementExists(string deck, string element)
		{
			string relativePath = Path.Combine("Card Data", "-Layout", deck + element + ".png");
			if (!File.Exists(PathHelper.GetFullPath(relativePath)))
			{
				relativePath = deck + element + ".jpg";
				if (!File.Exists(PathHelper.GetFullPath(relativePath)))
				{
					relativePath = deck + element + ".jpeg";
					return File.Exists(PathHelper.GetFullPath(relativePath));
				}
				return true;
			}
			return true;
		}

		public static string FindExtension(string dir, string fileName)
		{
			string pathNoExt = Path.Combine(dir, fileName);
			string ext = ".png";
			string relativePath = pathNoExt + ext;
			if (!File.Exists(PathHelper.GetFullPath(relativePath)))
			{
				ext = ".jpg";
				relativePath = pathNoExt + ext;
				if (!File.Exists(PathHelper.GetFullPath(relativePath)))
				{
					ext = ".jpeg";
					relativePath = pathNoExt + ext;
					if (!File.Exists(PathHelper.GetFullPath(relativePath)))
					{
						return "";
					}
				}
			}
			return ext;
		}

		public static void FixTransparency(Bitmap b, Color correctC, Color bgC)
		{
			float totalDiff = Math.Abs(correctC.R - bgC.R) +
							  Math.Abs(correctC.G - bgC.G) + 
							  Math.Abs(correctC.B - bgC.B);
			for (int x = 0; x < b.Width; x++)
			{
				for (int y = 0; y < b.Height; y++)
				{
					float currDiff = Math.Abs(b.GetPixel(x, y).R - bgC.R) +
									 Math.Abs(b.GetPixel(x, y).G - bgC.G) + 
									 Math.Abs(b.GetPixel(x, y).B - bgC.B);
					int newA = (int)(currDiff / totalDiff * 255);
					b.SetPixel(x, y, Color.FromArgb(newA, correctC));
				}
			}
		}

		public static void ColorSymbol(Bitmap symbol, Color color)
		{
			for (int x = 0; x < symbol.Width; x++)
			{
				for (int y = 0; y < symbol.Height; y++)
				{
					int alpha = symbol.GetPixel(x, y).A;
					symbol.SetPixel(x, y, Color.FromArgb(alpha, color));
				}
			}
		}

		/// <meta>Original code from https://stackoverflow.com/a/4405876</meta>
		public static string Capitalize(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };

		public static bool IsPunctuation(string text)
		{
			if (".?!,;:/-".Contains(text))
			{
				return true;
			}
			return false;
		}
	}
}