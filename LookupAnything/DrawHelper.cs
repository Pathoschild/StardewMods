using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything
{
    /// <summary>Provides utility methods for drawing to the screen.</summary>
    internal static class DrawTextHelper
    {
        /*********
        ** Public methods
        *********/
        /****
        ** Drawing
        ****/
        /// <summary>Draw a block of text to the screen with the specified wrap width.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="font">The sprite font.</param>
        /// <param name="text">The block of text to write.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="wrapWidth">The width at which to wrap the text.</param>
        /// <param name="color">The text color.</param>
        /// <param name="bold">Whether to draw bold text.</param>
        /// <param name="scale">The font scale.</param>
        /// <returns>Returns the text dimensions.</returns>
        public static Vector2 DrawTextBlock(this SpriteBatch batch, SpriteFont font, string text, Vector2 position, float wrapWidth, Color? color = null, bool bold = false, float scale = 1)
        {
            return batch.DrawTextBlock(font, new IFormattedText[] { new FormattedText(text, color, bold) }, position, wrapWidth, scale);
        }

        /// <summary>Draw a block of text to the screen with the specified wrap width.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="font">The sprite font.</param>
        /// <param name="text">The block of text to write.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="wrapWidth">The width at which to wrap the text.</param>
        /// <param name="scale">The font scale.</param>
        /// <returns>Returns the text dimensions.</returns>
        public static Vector2 DrawTextBlock(this SpriteBatch batch, SpriteFont font, IEnumerable<IFormattedText> text, Vector2 position, float wrapWidth, float scale = 1)
        {
            if (text == null)
                return new Vector2(0, 0);

            // track draw values
            float xOffset = 0;
            float yOffset = 0;
            float lineHeight = font.MeasureString("ABC").Y * scale;
            float spaceWidth = DrawHelper.GetSpaceWidth(font) * scale;
            float blockWidth = 0;
            float blockHeight = lineHeight;

            // draw text snippets
            foreach (IFormattedText snippet in text)
            {
                if (snippet?.Text == null)
                    continue;

                // track surrounding spaces for combined translations
                bool startSpace = snippet.Text.StartsWith(" ");
                bool endSpace = snippet.Text.EndsWith(" ");

                // get word list
                IList<string> words = new List<string>();
                string[] rawWords = snippet.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0, last = rawWords.Length - 1; i <= last; i++)
                {
                    // get word
                    string word = rawWords[i];
                    if (startSpace && i == 0)
                        word = $" {word}";
                    if (endSpace && i == last)
                        word += " ";

                    // split on newlines
                    string wordPart = word;
                    int newlineIndex;
                    while ((newlineIndex = wordPart.IndexOf(Environment.NewLine, StringComparison.Ordinal)) >= 0)
                    {
                        if (newlineIndex == 0)
                        {
                            words.Add(Environment.NewLine);
                            wordPart = wordPart.Substring(Environment.NewLine.Length);
                        }
                        else if (newlineIndex > 0)
                        {
                            words.Add(wordPart.Substring(0, newlineIndex));
                            words.Add(Environment.NewLine);
                            wordPart = wordPart.Substring(newlineIndex + Environment.NewLine.Length);
                        }
                    }

                    // add remaining word (after newline split)
                    if (wordPart.Length > 0)
                        words.Add(wordPart);
                }

                // draw words to screen
                bool isFirstOfLine = true;
                foreach (string word in words)
                {
                    // check wrap width
                    float wordWidth = font.MeasureString(word).X * scale;
                    float prependSpace = isFirstOfLine ? 0 : spaceWidth;
                    if (word == Environment.NewLine || ((wordWidth + xOffset + prependSpace) > wrapWidth && (int)xOffset != 0))
                    {
                        xOffset = 0;
                        yOffset += lineHeight;
                        blockHeight += lineHeight;
                        isFirstOfLine = true;
                    }
                    if (word == Environment.NewLine)
                        continue;

                    // draw text
                    Vector2 wordPosition = new Vector2(position.X + xOffset + prependSpace, position.Y + yOffset);
                    if (snippet.Bold)
                        Utility.drawBoldText(batch, word, font, wordPosition, snippet.Color ?? Color.Black, scale);
                    else
                        batch.DrawString(font, word, wordPosition, snippet.Color ?? Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 1);

                    // update draw values
                    if (xOffset + wordWidth + prependSpace > blockWidth)
                        blockWidth = xOffset + wordWidth + prependSpace;
                    xOffset += wordWidth + prependSpace;
                    isFirstOfLine = false;
                }
            }

            // return text position & dimensions
            return new Vector2(blockWidth, blockHeight);
        }
    }
}
