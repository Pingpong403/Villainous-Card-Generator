namespace Villainous_Card_Generator.CardGeneration
{
	/// <summary>
	/// Static methods used to manipulate images.
	/// </summary>
	public static class ImageManipulation
	{
		/// <summary>
		/// Converts a background color to transparency. Only works if the image is composed of a solid color anti-aliased with a different solid color.
		/// </summary>
		/// <param name="b">the Bitmap containing the text</param>
		/// <param name="correctColor">the Color that represents the foreground color</param>
		/// <param name="backgroundColor">the Color that represents the background color</param>
		public static void Mask(Bitmap b, Color correctColor, Color backgroundColor)
		{
			float totalDistance = (float)Math.Sqrt((correctColor.R - backgroundColor.R) * (correctColor.R - backgroundColor.R) + 
							  (correctColor.G - backgroundColor.G) * (correctColor.G - backgroundColor.G) + 
							  (correctColor.B - backgroundColor.B) * (correctColor.B - backgroundColor.B));
			for (int x = 0; x < b.Width; x++)
			{
				for (int y = 0; y < b.Height; y++)
				{
					// Distance is greater the further the color is from the background color
					float currentDistance = (float)Math.Sqrt((b.GetPixel(x, y).R - backgroundColor.R) * (b.GetPixel(x, y).R - backgroundColor.R) + 
							 		 (b.GetPixel(x, y).G - backgroundColor.G) * (b.GetPixel(x, y).G - backgroundColor.G) + 
							 		 (b.GetPixel(x, y).B - backgroundColor.B) * (b.GetPixel(x, y).B - backgroundColor.B));
					int newA = (int)(currentDistance / totalDistance * 255);
					b.SetPixel(x, y, Color.FromArgb(newA, correctColor));
				}
			}
		}

		/// <summary>
		/// Turns all colors in a symbol to the same color. Preserves the alpha channel.
		/// </summary>
		/// <param name="symbol">the Bitmap containing the symbol</param>
		/// <param name="color">the color to convert the existing symbol into</param>
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
	}
}