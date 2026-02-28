using System;

namespace Villainous_Card_Generator
{
	public static class PathHelper
	{
		public static string GetFullPath(string relativePath)
		{
			var baseDir = AppContext.BaseDirectory;
			var fullPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", relativePath));
			if (!Directory.Exists(Path.GetDirectoryName(fullPath)) && !File.Exists(fullPath))
			{
				fullPath = Path.GetFullPath(Path.Combine(baseDir, relativePath));
			}
			
			return fullPath;
		}
	}
}