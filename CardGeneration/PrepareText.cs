using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;

namespace Villainous_Card_Generator.CardGeneration
{
	public static class PrepareText
	{
		/// <summary>
		/// Creates an image in TextIntermediary that contains the title.
		/// </summary>
		/// <param name="text">title text</param>
		/// <param name="font">font to draw the title in</param>
		/// <param name="textColor">color to draw the title in</param>
		/// <param name="maxWidth">maximum width the title is allowed to take up</param>
		/// <param name="maxHeight">maximum height the title is allowed to take up</param>
		public static void DrawTitle(string text, Font font, Color textColor, int maxWidth, int maxHeight)
		{
			// Remove duplicate designation
			if (text[^1] == ')' && text[^3] == '(')
			{
				if (text[^4] == ' ') text = text[0..^4];
				else text = text[0..^3];
			}

			// For titles, capitalize the text
			text = text.ToUpper();

			// Set the textformatflags for center alignment and no trimming
			TextFormatFlags tf = TextFormatFlags.VerticalCenter|
				TextFormatFlags.HorizontalCenter|
				TextFormatFlags.NoPadding;

			// Create a new image of the maximum size for our graphics
			Image img = new Bitmap(maxWidth, maxHeight);
			Graphics drawing = Graphics.FromImage(img);

			// Use high quality everything
			drawing.CompositingQuality = CompositingQuality.HighQuality;
			drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
			drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
			drawing.SmoothingMode = SmoothingMode.HighQuality;
			drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			
			// Paint a transparent background
			drawing.Clear(Color.Transparent);

			// One line maximum
			float textHeight = TextRenderer.MeasureText(text, font, new Size(1000, 1000), tf).Height;

			// Find the proper squish ratio for given title
			float textFullWidth = TextRenderer.MeasureText(text, font, new Size(1000, 1000), tf).Width;
			if (textFullWidth > maxWidth)
			{
				float horizontalSquish = maxWidth / textFullWidth;
				drawing.ScaleTransform(horizontalSquish, 1.0F);
				maxWidth = (int)textFullWidth;
			}

			// Get all the words
			List<CardWord> words = GetCardWords(text, textColor, font, null);

			// Set up variables
			float startY = (maxHeight - textHeight) / 2;

			// Draw title word by word
			DrawWordByWord(words, drawing, tf, maxWidth, textHeight, maxWidth / 2, startY);

			drawing.Save();
			drawing.Dispose();

			// Ensure output directory exists and save per-element PNG
			var relativeOutDir = Path.Combine("temp", "TextIntermediary");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, "Title.png");
			img.Save(outpath, ImageFormat.Png);
			img.Dispose();
		}

		/// <summary>
		/// Creates an image in TextIntermediary that contains all of the text/assets needed for an ability.
		/// </summary>
		/// <param name="ability">ability of the card</param>
		/// <param name="activateAbility">ability on activate of the card</param>
		/// <param name="activateCost">cost to activate the card</param>
		/// <param name="gainsAction">symbol for the action gained</param>
		/// <param name="font">font to draw the text with</param>
		/// <param name="textColor">base color to draw the text with</param>
		/// <param name="maxWidth">maximum width this text can take</param>
		/// <param name="maxHeight">maximum height this text can take</param>
		/// <param name="keywordsAndColors">a dictionary to compare every word to to determine bolding and coloring</param>
		public static void DrawAbility(string ability, string activateAbility, string activateCost, string gainsAction, Font font, Color textColor, int maxWidth, int maxHeight, Dictionary<string, string> keywordsAndColors)
		{
			// Set up variables we'll potentially need
			float granularity = float.Parse(ConfigHelper.GetConfigValue("text", "fontDecreaseGranularity"));
			float paddingLines = float.Parse(ConfigHelper.GetConfigValue("text", "abilityPaddingLines"));
			float minFontSize = float.Parse(ConfigHelper.GetConfigValue("text", "abilityMinFontSize"));
			float dividingLineLines = float.Parse(ConfigHelper.GetConfigValue("asset", "dividingLineLines"));
			float actionSymbolLines = float.Parse(ConfigHelper.GetConfigValue("asset", "actionSymbolLines"));
			float lineSpacing = float.Parse(ConfigHelper.GetConfigValue("text", "lineSpacingFactor"));
			int abilityBottomPadding = int.Parse(ConfigHelper.GetConfigValue("card", "abilityBottomPadding"));
			int sideAAMaxW = int.Parse(ConfigHelper.GetConfigValue("card", "sideActivateAbilityMaxWidth"));
			int sideAACenterX = int.Parse(ConfigHelper.GetConfigValue("card", "sideActivateAbilityCenterX"));

			// Set the textformatflags for center alignment and no trimming
			TextFormatFlags tf = TextFormatFlags.VerticalCenter|
				TextFormatFlags.HorizontalCenter|
				TextFormatFlags.NoPadding;

			// Create a new image of the maximum size for our graphics
			Image img = new Bitmap(maxWidth, maxHeight + abilityBottomPadding);
			Graphics drawing = Graphics.FromImage(img);

			// Use high quality everything
			drawing.CompositingQuality = CompositingQuality.HighQuality;
			drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
			drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
			drawing.SmoothingMode = SmoothingMode.HighQuality;
			drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			
			// Paint a transparent background
			drawing.Clear(Color.Transparent);

			// We need this for symbol resizing
			float originalLineHeight = TextRenderer.MeasureText("Tq", font, new Size(1000, 1000), tf).Height * lineSpacing;

			// Naively find the maximum allowable font size by measuring everything
			float lineHeight;
			float abilityHeight;
			float activateAbilityHeight;
			bool activateAbilityTextTaller = true;
			float gainsActionHeight;
			float paddingHeight;
			float textHeight;
			do
			{
				// Combine every given ability into one metric
				lineHeight = TextRenderer.MeasureText("TjJ", font, new Size(1000, 1000), tf).Height * lineSpacing;
				abilityHeight = ability == "" ? 0 : MeasureWordByWord(GetCardWords(ability, textColor, font, keywordsAndColors), tf, maxWidth, lineHeight);
				activateAbilityHeight = 0;
				if (activateAbility != "" || activateCost != "")
				{
					if (ability == "" || activateCost != "") // If there is no ability or there is an activate cost, measure normally
					{
						activateAbilityHeight += actionSymbolLines * lineHeight + MeasureWordByWord(GetCardWords(activateAbility, textColor, font, keywordsAndColors), tf, maxWidth, lineHeight);
					}
					else
					{
						float aaTextHeight = MeasureWordByWord(GetCardWords(activateAbility, textColor, font, keywordsAndColors), tf, sideAAMaxW, lineHeight);
						float aaSymbolHeight = actionSymbolLines * lineHeight;
						activateAbilityTextTaller = aaTextHeight > aaSymbolHeight;
						activateAbilityHeight += Math.Max(aaSymbolHeight, aaTextHeight);
					}
				}
				gainsActionHeight = 0;
				if (gainsAction != "")
				{
					gainsActionHeight = actionSymbolLines * lineHeight;
					if (activateAbility != "" && ability != "" && activateCost == "") // Special case where we need to draw extra text
					{
						gainsActionHeight += lineHeight;
					}
				}
				int numPadding = (abilityHeight > 0 ? 1 : 0) + (activateAbilityHeight > 0 ? 1 : 0) + (gainsActionHeight > 0 ? 1 : 0) - 1;
				if (numPadding < 0) numPadding = 0;
				paddingHeight = numPadding * lineHeight * paddingLines;
				textHeight = abilityHeight + activateAbilityHeight + gainsActionHeight + paddingHeight;

				if (textHeight > maxHeight - (activateAbility != "" && ability == "" ? textHeight * 0.1 : 0)) font = new Font(font.Name, font.Size - granularity, font.Style, font.Unit);
			} while (textHeight > maxHeight);

			// Check if font size went below minimum and notify the user
			if (font.Size < minFontSize)
			{
				Console.WriteLine($"The following card's Ability went below the minimum {minFontSize}px:");
			}

			List<CardWord> colon = GetCardWords(":", textColor, font, keywordsAndColors);
			List<CardWord> locationGainsText = GetCardWords("\\This \\location \\gains\\:", textColor, font, keywordsAndColors);

			// For resizing symbols with more complexity
			float lineHeightRatio = lineHeight / originalLineHeight;

			// Drawing variables
			float currentY = (maxHeight - textHeight) / 2;
			List<CardWord> words;

			// Draw the ability first
			if (ability != "")
			{
				words = GetCardWords(ability, textColor, font, keywordsAndColors);
				currentY = DrawWordByWord(words, drawing, tf, maxWidth, lineHeight, maxWidth / 2, currentY);
				currentY += lineHeight * paddingLines;
			}

			// Then draw the activate symbol, cost, and ability
			if (activateAbility != "" || activateCost != "")
			{
				// Symbol
				string activateSymbolPath = PathHelper.GetFullPath(Path.Combine("assets", "Activate.png"));
				Image activateSymbol = Image.FromFile(activateSymbolPath);
				float resizing = actionSymbolLines * lineHeight / activateSymbol.Height;
				float symbolCenterX = maxWidth / 2;
				if (ability == "" || activateCost != "") // If there is no ability or there is an activate cost, draw normally
				{
					int colonCenterX = int.Parse(ConfigHelper.GetConfigValue("card", "colonCenterX"));
					int colonPadding = int.Parse(ConfigHelper.GetConfigValue("card", "colonPadding"));
					bool drawColon = activateCost != "" && activateCost[0..4] == "Pay " && (activateCost[^6..^0] == " Power" || activateCost[^7..^0] == " Power.");
					float symbolW = activateSymbol.Width * resizing;
					if (activateCost != "")
					{
						symbolCenterX = colonCenterX - colonPadding - symbolW / 2;
					}
					if (ability == "" && activateAbility != "")
					{
						currentY = lineHeight * 0.1F;
					}
					DrawSymbol(activateSymbol, drawing, textColor, symbolCenterX, currentY + actionSymbolLines * lineHeight / 2, resizing);
					
					// Cost, if any
					if (activateCost != "")
					{
						float costLeftX = colonCenterX + colonPadding;
						float activateCostWidth = maxWidth / 2;
						float activateCostHeight = MeasureWordByWord(GetCardWords(activateCost, textColor, font, keywordsAndColors), tf, activateCostWidth, lineHeight);
						float activateCostY = currentY + (2 * lineHeight - activateCostHeight) / 2; // maximum of 3 lines for clarity
						if (drawColon)
						{
							Font acFont = new Font(font, FontStyle.Bold);
							float costCenterX = costLeftX + TextRenderer.MeasureText(activateCost, acFont, new Size(1000, 1000), tf).Width / 2;
							DrawWordByWord(colon, drawing, tf, maxWidth, lineHeight, colonCenterX, currentY + lineHeight / 2);
							words = GetCardWords(activateCost, textColor, acFont, keywordsAndColors);
							DrawWordByWord(words, drawing, tf, activateCostWidth, lineHeight, costCenterX, activateCostY);
						}
						else
						{
							float costCenterX = costLeftX + TextRenderer.MeasureText(activateCost, font, new Size(1000, 1000), tf).Width / 2;
							words = GetCardWords(activateCost, textColor, font, keywordsAndColors);
							DrawWordByWord(words, drawing, tf, activateCostWidth, lineHeight, maxWidth / 2 + costCenterX, activateCostY);
						}
					}
					currentY += actionSymbolLines * lineHeight;

					// Ability, if any
					if (activateAbility != "")
					{
						words = GetCardWords(activateAbility, textColor, font, keywordsAndColors);
						currentY = DrawWordByWord(words, drawing, tf, maxWidth, lineHeight, maxWidth / 2, currentY);
					}
				}
				else // Otherwise, the activate ability is to the right of the symbol
				{
					// Symbol
					symbolCenterX = sideAACenterX - sideAAMaxW / 2 - 100 - activateSymbol.Width * resizing / 2;
					DrawSymbol(activateSymbol, drawing, textColor, symbolCenterX, currentY + activateAbilityHeight / 2, resizing);
					
					// Activate ability
					words = GetCardWords(activateAbility, textColor, font, keywordsAndColors);
					float drawY = currentY;
					if (!activateAbilityTextTaller)
					{
						drawY += (activateAbilityHeight - MeasureWordByWord(words, tf, sideAAMaxW, lineHeight)) / 2;
					}
					DrawWordByWord(words, drawing, tf, sideAAMaxW, lineHeight, maxWidth - sideAAMaxW / 2 - 30, drawY);
					currentY += activateAbilityHeight;
				}
				currentY += lineHeight * paddingLines;
			}

			// Finally, draw the gained action
			if (gainsAction != "" && AssetHelper.AssetExists(gainsAction, true))
			{
				if (ability != "" && activateAbility != "" && activateCost == "")
				{
					currentY = DrawWordByWord(locationGainsText, drawing, tf, maxWidth, lineHeight, maxWidth / 2, currentY);
				}
				string assetName = AssetHelper.GetAssetName(gainsAction, true);
				string gainPowerAmt = AssetHelper.GainPowerAmount(assetName);
				if (gainPowerAmt != "")
				{
					assetName = "GainPower";
				}
				string gainsSymbolPath = PathHelper.GetFullPath(Path.Combine("assets", assetName + ".png"));
				Image gainsSymbol = Image.FromFile(gainsSymbolPath);
				float resizing = actionSymbolLines * lineHeight / gainsSymbol.Height;
				DrawSymbol(gainsSymbol, drawing, textColor, maxWidth / 2, currentY + actionSymbolLines * lineHeight / 2, resizing);

				// If this was a Gain Power action, draw the amount to be gained
				if (gainPowerAmt != "")
				{
					Font gainPowerFont = FontLoader.GetFont(
						ConfigHelper.GetConfigValue("text", "elementFont"),
						float.Parse(ConfigHelper.GetConfigValue("text", "costFontSize")) * resizing
					);
					Point gainPowerPos = new(
						(int)(maxWidth / 2),
						(int)(currentY + actionSymbolLines * lineHeight / 2)
					);
					TextRenderer.DrawText(drawing, gainPowerAmt, gainPowerFont, gainPowerPos, textColor, tf);
				}
			}

			drawing.Save();

			drawing.Dispose();

			// Ensure output directory exists and save per-element PNG
			var relativeOutDir = Path.Combine("temp", "TextIntermediary");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, "Ability.png");
			img.Save(outpath, ImageFormat.Png);
			img.Dispose();
		}

		/// <summary>
		/// Creates an image in TextIntermediary that contains the specified type.
		/// </summary>
		/// <param name="text">text to be drawn</param>
		/// <param name="font">font to draw the text in</param>
		/// <param name="textColor">color to draw the text in</param>
		/// <param name="maxWidth">maximum width the text is allowed to take up</param>
		/// <param name="maxHeight">maximum height the text is allowed to take up</param>
		/// <param name="keywordsAndColors">a dictionary containing all the possible keywords and their colors</param>
		public static void DrawType(string text, Font font, Color textColor, int maxWidth, int maxHeight, Dictionary<string, string> keywordsAndColors)
		{
			// Set the textformatflags for center alignment and no trimming
			TextFormatFlags tf = TextFormatFlags.VerticalCenter|
				TextFormatFlags.HorizontalCenter|
				TextFormatFlags.NoPadding;

			// Create a new image of the maximum size for our graphics
			Image img = new Bitmap(maxWidth, maxHeight);
			Graphics drawing = Graphics.FromImage(img);

			// Use high quality everything
			drawing.CompositingQuality = CompositingQuality.HighQuality;
			drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
			drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
			drawing.SmoothingMode = SmoothingMode.HighQuality;
			drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			
			// Paint a transparent background
			drawing.Clear(Color.Transparent);

			// One line maximum
			int textHeight = TextRenderer.MeasureText(text, font, new Size(1000, 1000), tf).Height;

			// Find the proper squish ratio for given title
			float textFullWidth = TextRenderer.MeasureText(text, font, new Size(1000, 1000), tf).Width;
			if (textFullWidth > maxWidth)
			{
				float horizontalSquish = maxWidth / textFullWidth;
				drawing.ScaleTransform(horizontalSquish, 1.0F);
				maxWidth = (int)textFullWidth;
			}

			// Get all the words
			List<CardWord> words = GetCardWords(text, textColor, font, keywordsAndColors, true);

			// Set up variables
			float startY = (maxHeight - textHeight) / 2;

			// Draw type word by word
			DrawWordByWord(words, drawing, tf, maxWidth, textHeight, maxWidth / 2, startY);

			drawing.Save();
			drawing.Dispose();

			// Ensure output directory exists and save per-element PNG
			var relativeOutDir = Path.Combine("temp", "TextIntermediary");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, "Type.png");
			img.Save(outpath, ImageFormat.Png);
			img.Dispose();
		}

		/// <summary>
		/// Creates an image in TextIntermediary that contains the specified corner element.
		/// </summary>
		/// <param name="text">text to include in the element</param>
		/// <param name="font">font to draw the text with</param>
		/// <param name="textColor">color to draw the text in</param>
		/// <param name="element">which corner element this is (e.g. "Cost", "Strength", "TopRight", or "BottomRight")</param>
		/// <param name="maxWidth">maximum width this element can take up</param>
		/// <param name="maxHeight">maximum height this element can take up</param>
		public static void DrawCornerElement(string text, Font font, Color textColor, string element, int maxWidth, int maxHeight)
		{
			// Set the textformatflags for center alignment and no trimming
			TextFormatFlags tf = TextFormatFlags.VerticalCenter|
				TextFormatFlags.HorizontalCenter|
				TextFormatFlags.NoPadding;

			// Create a new image of the maximum size for our graphics
			Image img = new Bitmap(maxWidth, maxHeight);
			Graphics drawing = Graphics.FromImage(img);

			// Use high quality everything
			drawing.CompositingQuality = CompositingQuality.HighQuality;
			drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
			drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
			drawing.SmoothingMode = SmoothingMode.HighQuality;
			drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			
			// Paint a transparent background
			drawing.Clear(Color.Transparent);

			// One line maximum
			int textHeight = TextRenderer.MeasureText(text, font, new Size(1000, 1000), tf).Height;

			// Find the proper squish ratio for given corner element
			int textFullWidth = TextRenderer.MeasureText(text, font, new Size(1000, 1000), tf).Width;
			if (textFullWidth > maxWidth)
			{
				float horizontalSquish = (float)maxWidth / textFullWidth;
				drawing.ScaleTransform(horizontalSquish, 1.0F);
				maxWidth = textFullWidth;
			}

			// Get all the words
			List<CardWord> words = GetCardWords(text, textColor, font, null);

			// Set up variables
			int startY = (maxHeight - textHeight) / 2;

			// Draw corner element word by word
			DrawWordByWord(words, drawing, tf, maxWidth, textHeight, maxWidth / 2, startY);

			drawing.Save();
			drawing.Dispose();

			// Ensure output directory exists and save per-element PNG
			var relativeOutDir = Path.Combine("temp", "TextIntermediary");
            var outDir = PathHelper.GetFullPath(relativeOutDir);
			Directory.CreateDirectory(outDir);
			var outpath = Path.Combine(outDir, element + ".png");
			img.Save(outpath, ImageFormat.Png);
			img.Dispose();
		}

		/// <summary>
		/// Goes letter by letter to build words based on formatting rules.
		/// </summary>
		/// <param name="text">text to be converted</param>
		/// <param name="defaultBrush">default text brush</param>
		/// <param name="defaultFont">default text font</param>
		/// <param name="keywordData">a dictionary of keywords and their associated colors</param>
		/// <param name="isType">whether or not this is the type element</param>
		/// <returns>a list of all words as CardWord objects</returns>
		public static List<CardWord> GetCardWords(string text, Color defaultColor, Font defaultFont, Dictionary<string, string>? keywordData, bool isType = false)
		{
			bool typeIsCaps = ConfigHelper.GetConfigValue("text", "typeIsCaps") == "true";
			
			char italicSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "italicCharacter"));
			char boldSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "boldCharacter"));
			char escapeSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "escapeCharacter"));
			char newlineSymbol = Convert.ToChar(ConfigHelper.GetConfigValue("text", "newlineCharacter"));

			Font italicFont = new Font(defaultFont, FontStyle.Italic);

			Font boldFont;
			string regularVariant = ConfigHelper.GetConfigValue("text", "abilityFont");
			string boldVariant = regularVariant[0..^4] + "Bold" + regularVariant[^4..^0];
			string boldPath = PathHelper.GetFullPath(Path.Combine("fonts", boldVariant));
			boldFont = File.Exists(boldPath) ?
				FontLoader.GetFont(boldPath, defaultFont.Size, FontStyle.Bold) :
				new(defaultFont, FontStyle.Bold);

			Font boldItalicFont = new Font(boldFont, FontStyle.Bold | FontStyle.Italic);

			List<CardWord> cardWords = [];

			bool italicsOpen = false;
			bool boldOpen = false;
			bool escapeNext = false;
			bool ignoreFormatting = false;
			string builtWord = "";
			foreach (char letter in text)
			{
				if (letter == ' ')
				{
					// End of word
					if (builtWord != "")
					{
						bool isKeyword = keywordData != null && keywordData.TryGetValue(builtWord, out string? value);
						bool boldWord = isKeyword && !boldOpen || !isKeyword && boldOpen;
						CardWord word = new(
							builtWord,
							isKeyword && !ignoreFormatting ? Color.FromArgb(Convert.ToInt32("ff" + keywordData[builtWord], 16)) : defaultColor,
							boldWord && !ignoreFormatting ? (italicsOpen ? boldItalicFont : boldFont) : italicsOpen ? italicFont : defaultFont
						);
						word.SetType(isType);
						if (isType && typeIsCaps) word.SetText(word.GetText().ToUpper());
						cardWords.Add(word);
					}
					cardWords.Add(new CardWord(" ", defaultColor, defaultFont));
					builtWord = "";
					ignoreFormatting = false;
				}
				else if (escapeNext)
				{
					// If the next was a symbol, then we escape it
					if (letter == italicSymbol ||
						letter == boldSymbol   ||
						letter == escapeSymbol ||
						letter == newlineSymbol
						)
					{
						cardWords.Add(new CardWord(Convert.ToString(letter), defaultColor, defaultFont));
					}

					// Otherwise, this means the next word should not be formatted
					else
					{
						if (builtWord == "") ignoreFormatting = true;
						builtWord += letter;
					}
					escapeNext = false;
				}
				else
				{
					if (letter == italicSymbol)
					{
						italicsOpen = !italicsOpen;
						if (builtWord != "")
						{
							bool isKeyword = keywordData != null && keywordData.TryGetValue(builtWord, out string? value);
							bool boldWord = isKeyword && !boldOpen || !isKeyword && boldOpen;
							CardWord word = new(
								builtWord,
								isKeyword && !ignoreFormatting ? Color.FromArgb(Convert.ToInt32("ff" + keywordData[builtWord], 16)) : defaultColor,
								boldWord && !ignoreFormatting ? (!italicsOpen ? boldItalicFont : boldFont) : !italicsOpen ? italicFont : defaultFont
							);
							word.SetType(isType);
							if (isType && typeIsCaps) word.SetText(word.GetText().ToUpper());
							cardWords.Add(word);
							builtWord = "";
						}
					}
					else if (letter == boldSymbol)
					{
						boldOpen = !boldOpen;
						if (builtWord != "")
						{
							bool isKeyword = keywordData != null && keywordData.TryGetValue(builtWord, out string? value);
							bool boldWord = isKeyword && boldOpen || !isKeyword && !boldOpen;
							CardWord word = new(
								builtWord,
								isKeyword && !ignoreFormatting ? Color.FromArgb(Convert.ToInt32("ff" + keywordData[builtWord], 16)) : defaultColor,
								boldWord && !ignoreFormatting ? (italicsOpen ? boldItalicFont : boldFont) : italicsOpen ? italicFont : defaultFont
							);
							word.SetType(isType);
							if (isType && typeIsCaps) word.SetText(word.GetText().ToUpper());
							cardWords.Add(word);
							builtWord = "";
						}
					}
					else if (letter == escapeSymbol)
					{
						escapeNext = true;
					}
					else if (letter == newlineSymbol || letter == '\n')
					{
						if (builtWord != "")
						{
							bool isKeyword = keywordData != null && keywordData.TryGetValue(builtWord, out string? value);
							bool boldWord = isKeyword && !boldOpen || !isKeyword && boldOpen;
							CardWord word = new(
								builtWord,
								isKeyword && !ignoreFormatting ? Color.FromArgb(Convert.ToInt32("ff" + keywordData[builtWord], 16)) : defaultColor,
								boldWord && !ignoreFormatting ? (italicsOpen ? boldItalicFont : boldFont) : italicsOpen ? italicFont : defaultFont
							);
							word.SetType(isType);
							if (isType && typeIsCaps) word.SetText(word.GetText().ToUpper());
							cardWords.Add(word);
							builtWord = "";
						}
						cardWords.Add(new CardWord("\n", defaultColor, defaultFont));
						ignoreFormatting = false;
					}
					else
					{
						if (MiscHelper.IsPunctuation(Convert.ToString(letter)) && builtWord != "")
						{
							bool isKeyword = keywordData != null && keywordData.TryGetValue(builtWord, out string? value);
							bool boldWord = isKeyword && !boldOpen || !isKeyword && boldOpen;
							CardWord word = new(
								builtWord,
								isKeyword && !ignoreFormatting ? Color.FromArgb(Convert.ToInt32("ff" + keywordData[builtWord], 16)) : defaultColor,
								boldWord && !ignoreFormatting ? (italicsOpen ? boldItalicFont : boldFont) : italicsOpen ? italicFont : defaultFont
							);
							word.SetType(isType);
							if (isType && typeIsCaps) word.SetText(word.GetText().ToUpper());
							cardWords.Add(word);
							cardWords.Add(new CardWord(Convert.ToString(letter), defaultColor, defaultFont));
							builtWord = "";
							ignoreFormatting = false;
						}
						// Most generic case - add a letter to builtWord
						else
						{
							builtWord += letter;
						}
					}
				}
			}
			if (builtWord != "")
			{
				bool isKeyword = keywordData != null && keywordData.TryGetValue(builtWord, out string? value);
				bool boldWord = isKeyword && !boldOpen || !isKeyword && boldOpen;
				CardWord word = new(
					builtWord,
					isKeyword && !ignoreFormatting ? Color.FromArgb(Convert.ToInt32("ff" + keywordData[builtWord], 16)) : defaultColor,
					boldWord && !ignoreFormatting ? (italicsOpen ? boldItalicFont : boldFont) : italicsOpen ? italicFont : defaultFont
				);
				word.SetType(isType);
				if (isType && typeIsCaps) word.SetText(word.GetText().ToUpper());
				cardWords.Add(word);
			}

			return cardWords;
		}

		/// <summary>
		/// Takes in a list of words and measures how much vertical space they will need.
		/// </summary>
		/// <param name="words">the list of CardWord objects to measure</param>
		/// <param name="g">a Graphics object to measure with</param>
		/// <param name="sf">the StringFormat used to draw the words</param>
		/// <param name="maxW">the maximum width the words can take up</param>
		/// <param name="lineHeight">the height of each line</param>
		/// <returns>the vertical space needed by these words</returns>
		public static float MeasureWordByWord(List<CardWord> words, TextFormatFlags tf, float maxW, float lineHeight)
		{
			float actionSymbolLines = float.Parse(ConfigHelper.GetConfigValue("asset", "actionSymbolLines"));
			float dividingLineLines = float.Parse(ConfigHelper.GetConfigValue("asset", "dividingLineLines"));

			float textHeight = 0;
			float lineWidth = 0;
			bool space = false;
			float spaceWidth = 0;
			foreach (CardWord word in words)
			{
				// Keywords: add the amount of vertical space they take up
				if (AssetHelper.AssetExists(word.GetText()))
				{
					if (lineWidth > 0) // Check if some words have already been added to line
					{
						textHeight += lineHeight;
						lineWidth = 0;
						space = false;
					}
					if (word.GetText() == "DividingLine_") textHeight += dividingLineLines * lineHeight;
					else textHeight += actionSymbolLines * lineHeight;
				}
				// Spaces: set flag
				else if (word.GetText() == " ")
				{
					space = true;
					spaceWidth = word.GetSizeF((int)maxW, tf).Width;
				}
				// Newlines: add a line
				else if (word.GetText() == "\n")
				{
					textHeight += lineHeight;
					lineWidth = 0.001F; // Completely ignore the text that was already built up
				}
				// Generic case: add word's width (+ space), check if over
				else
				{
					lineWidth += word.GetSizeF((int)maxW, tf).Width + (space ? spaceWidth : 0);
					if (lineWidth > maxW)
					{
						textHeight += lineHeight;
						lineWidth = word.GetSizeF((int)maxW, tf).Width;
					}
					space = false;
				}
			}
			if (lineWidth > 0) textHeight += lineHeight;
			return textHeight;
		}

		/// <summary>
		/// For use when words of varying styles need to be drawn in a cohesive paragraph.
		/// Meant to be used in a chain with other text that will share the same Graphics object.
		/// </summary>
		/// <param name="words">each word to be drawn</param>
		/// <param name="g">the Graphics object used to draw</param>
		/// <param name="sf">the StringFormat used to draw the words</param>
		/// <param name="maxW">max width the words are allowed to take up</param>
		/// <param name="lineH">height each line takes up</param>
		/// <param name="startY">the y-coordinate to start drawing at</param>
		/// <returns>the y-coordinate drawing ended at</returns>
		private static float DrawWordByWord(List<CardWord> words, Graphics g, TextFormatFlags tf, float maxW, float lineH, float centerX, float startY)
		{
			// Set up variables we'll potentially need
			float dlLines = float.Parse(ConfigHelper.GetConfigValue("asset", "dividingLineLines"));
			float asLines = float.Parse(ConfigHelper.GetConfigValue("asset", "actionSymbolLines"));
			int horizontalPadding = int.Parse(ConfigHelper.GetConfigValue("text", "wordHorizontalPadding"));
			Color color = ColorTranslator.FromHtml("#" + ConfigHelper.GetConfigValue("color", "fontColor"));

			// Draw text word by word
			float currentY = startY;
			int iCheck = 0;
			int iDraw = 0;
			float lineLength;
			bool drawAsset = false;
			bool endOfText = false;
			while (!endOfText)
			{
				// First, find the length of this line
				lineLength = 0;
				try
				{
					bool endOfLine = false;
					float currentWordWidth;
					bool space = false;
					float spaceWidth = 0;
					while (!endOfLine)
					{
						// Measure each word
						currentWordWidth = words[iCheck].GetSizeF(maxW, tf).Width;

						if (words[iCheck].GetText() == " " && lineLength > 0)
						{
							if (lineLength == 0) iDraw++;
							space = true;
							spaceWidth = words[iCheck].GetSizeF(maxW, tf).Width;
							iCheck++;
						}
						else if (AssetHelper.AssetExists(words[iCheck].GetText()))
						{
							endOfLine = true;
							drawAsset = true;
						}
						else if (words[iCheck].GetText() == "\n")
						{
							endOfLine = true;
							iCheck++;
						}
						else if (lineLength + currentWordWidth + (space ? spaceWidth : 0) > maxW)
						{
							endOfLine = true;
						}
						else
						{
							lineLength += currentWordWidth + (space ? spaceWidth : 0);
							space = false;
							iCheck++;
						}
					}
				}
				catch (Exception ex)            
				{                
					if (ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
					{
						endOfText = true;
					}
					else
						throw;
				}
				// Draw each word in the line
				float currentX = centerX - lineLength / 2;
				for (int i = iDraw; i < iCheck; i++)
				{
					CardWord word = words[i];
					if (word.GetText() != "\n")
					{
						float wordWidth = words[iDraw].GetSizeF((int)maxW, tf).Width;
						float wordHeight = words[iDraw].GetSizeF((int)maxW, tf).Height;
						Bitmap textB = new((int)wordWidth + horizontalPadding, (int)wordHeight);
						Graphics textG = Graphics.FromImage(textB);
						textG.CompositingQuality = CompositingQuality.HighQuality;
						textG.InterpolationMode = InterpolationMode.HighQualityBilinear;
						textG.PixelOffsetMode = PixelOffsetMode.HighQuality;
						textG.SmoothingMode = SmoothingMode.HighQuality;
						textG.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
						Color bgColor = Color.Black;
						textG.Clear(bgColor);
						TextRenderer.DrawText(textG, word.GetText(), word.GetTextFont(), new Rectangle(0, 0, (int)wordWidth + horizontalPadding, (int)wordHeight), word.GetTextColor(), bgColor, tf);
						MiscHelper.FixTransparency(textB, word.GetTextColor(), bgColor);
						g.DrawImage(textB, new Point((int)currentX - horizontalPadding / 2, (int)currentY));
						currentX += wordWidth;
					}
					iDraw++;
				}
				// Move the register down
				currentY += lineH;
				// Draw the asset that is up to draw
				if (drawAsset)
				{
					string assetName = AssetHelper.GetAssetName(words[iDraw].GetText());
					string gainPowerAmt = AssetHelper.GainPowerAmount(assetName);
					if (gainPowerAmt != "")
					{
						assetName = "GainPower";
					}
					string gainsSymbolPath = PathHelper.GetFullPath(Path.Combine("assets", assetName + MiscHelper.FindExtension("assets", assetName)));
					Image asset = Image.FromFile(gainsSymbolPath);
					float resizing = string.Equals(assetName, "DividingLine") ? 1.0F : asLines * lineH / asset.Height;
					float yOffset = string.Equals(assetName, "DividingLine") ? dlLines * lineH / 2 : asLines * lineH / 2;
					DrawSymbol(asset, g, color, maxW / 2, currentY + yOffset, resizing);

					// If this was a Gain Power action, draw the amount to be gained
					if (gainPowerAmt != "")
					{
						Font gainPowerFont = FontLoader.GetFont(
							ConfigHelper.GetConfigValue("text", "elementFont"),
							float.Parse(ConfigHelper.GetConfigValue("text", "costFontSize")) * resizing
						);
						Point gainPowerPos = new(
							(int)(maxW / 2),
							(int)(currentY + asLines * lineH / 2)
						);
						TextRenderer.DrawText(g, gainPowerAmt, gainPowerFont, gainPowerPos, color, tf);
					}
					currentY += lineH * (string.Equals(assetName, "DividingLine") ? dlLines : asLines);
					iCheck++;
					iDraw++;
					drawAsset = false;
				}
				// Eat through whitespace
				bool ignoreSpaces = true;
				while (ignoreSpaces)
				{
					if (iDraw < words.Count)
					{
						if (words[iDraw].GetText() == " ") iDraw++;
						else ignoreSpaces = false;
					}
					else ignoreSpaces = false;
				}
			}
			return currentY;
		}

		/// <summary>
		/// Draws the given square symbol at the given coordinates.
		/// </summary>
		/// <param name="symbol">the symbol to draw</param>
		/// <param name="g">the Graphics object this will use to draw</param>
		/// <param name="centerX">the x-coordinate of the center of the symbol</param>
		/// <param name="centerY">the y-coordinate of the center of the symbol</param>
		/// <param name="resizing">optional resizing factor</param>
		private static void DrawSymbol(Image symbol, Graphics g, Color color, float centerX, float centerY, float resizing = 1.0F)
		{
			Bitmap b = new(symbol, new Size((int)(symbol.Width * resizing), (int)(symbol.Height * resizing)));
			MiscHelper.ColorSymbol(b, color);
			float x = centerX - resizing * symbol.Width / 2;
			float y = centerY - resizing * symbol.Height / 2;
			g.DrawImage(b, new PointF(x, y));
		}

		public class CardWord
		{
			private string text;
			private Color textColor;
			private Font textFont;
			private bool isType = false;

			public CardWord()
			{
				text = "";
				textColor = Color.Black;
				textFont = new Font(FontLoader.GetFont("roboto.ttf", 1), new FontStyle());
			}

			public CardWord(string text)
			{
				this.text = text;
				textColor = Color.Black;
				textFont = new Font(FontLoader.GetFont("roboto.ttf", 1), new FontStyle());
			}

			public CardWord(string text, Color textColor, Font textFont)
			{
				this.text = text;
				this.textColor = textColor;
				this.textFont = textFont;
			}

			public CardWord(CardWord other)
			{
				text = other.GetText();
				textColor = other.GetTextColor();
				textFont = other.GetTextFont();
			}

			public string GetText() { return text; }
			public Color GetTextColor() { return textColor; }
			public Font GetTextFont() { return textFont; }
			public bool IsType() { return isType; }

			public void SetText(string text) { this.text = text; }
			public void SetTextBrush(Color textColor) { this.textColor = textColor; }
			public void SetTextFont(Font textFont) { this.textFont = textFont; }
			public void SetType(bool isType) { this.isType = isType; }

			public SizeF GetSizeF(float maxWidth, TextFormatFlags tf)
			{
				if (text == " ")
				{
					return TextRenderer.MeasureText(text, textFont, new Size((int)maxWidth, 1000), tf) * float.Parse(ConfigHelper.GetConfigValue("text", "spaceShrink"));
				}
				return TextRenderer.MeasureText(text, textFont, new Size((int)maxWidth, 1000), tf);
			}
		}
	}
}