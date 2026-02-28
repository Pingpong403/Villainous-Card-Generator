using System;

namespace Villainous_Card_Generator.CardGeneration
{
	public static class AssetHelper
	{
		public static bool AssetExists(string assetCode, bool lenient = false)
		{
			string assetName;
			if (lenient)
			{
				string assetCharacter = ConfigHelper.GetConfigValue("text", "assetCharacter");
				if (assetCode[^1].ToString() != assetCharacter)
				{
					assetCode += '_';
				}
			}
			assetName = GetAssetName(assetCode);
			if (assetName == "") return false;
			if (GainPowerAmount(assetName) != "")
			{
				assetName = "GainPower";
			}
			string pathNoExt = Path.Combine("assets", assetName);
			string relativePath = pathNoExt + MiscHelper.FindExtension("assets", assetName);
			return File.Exists(PathHelper.GetFullPath(relativePath));
		}

		public static string GetAssetName(string assetCode, bool lenient = false)
		{
			if (assetCode == "") return "";
			string assetCharacter = ConfigHelper.GetConfigValue("text", "assetCharacter");
			if (assetCode[^1].ToString() != assetCharacter) return lenient ? assetCode : "";
			char[] letters = new char[assetCode.Length - 1];
			for (int i = 0; i < assetCode.Length - 1; i++)
			{
				letters[i] = assetCode[i];
			}

			return new string(letters);
		}

		public static string GainPowerAmount(string gainsActionCode)
		{
			char[] letters = gainsActionCode.ToCharArray();
			if (letters.Length >= 10)
			{
				// GainXPower
				string gain = new(letters[0..4]);
				string power = new(letters[(letters.Length - 5)..letters.Length]);
				if (Equals(gain, "Gain") && Equals(power, "Power"))
				{
					return new string(letters[4..(letters.Length - 5)]);
				}
			}
			return "";
		}
	}
}