using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework;
using StardewValley;
using StardewValley.Extensions;

namespace Pathoschild.Stardew.LookupAnything
{
    /// <summary>Provides utility methods for drawing to the screen.</summary>
    internal static class DrawTextHelper
    {
        /*********
        ** Fields
        *********/
        /// <summary>The last language for which the helper was initialized.</summary>
        private static string? LastLanguage;

        /// <summary>The characters after which we can line-wrap text, but which are still included in the string.</summary>
        private static readonly HashSet<char> SoftBreakCharacters = new();


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
        public static Vector2 DrawTextBlock(this SpriteBatch batch, SpriteFont font, string? text, Vector2 position, float wrapWidth, Color? color = null, bool bold = false, float scale = 1)
        {
            return batch.DrawTextBlock(font, [new FormattedText(text, color, bold)], position, wrapWidth, scale);
        }

        /// <summary>Draw a block of text to the screen with the specified wrap width.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="font">The sprite font.</param>
        /// <param name="text">The block of text to write.</param>
        /// <param name="position">The position at which to draw the text.</param>
        /// <param name="wrapWidth">The width at which to wrap the text.</param>
        /// <param name="scale">The font scale.</param>
        /// <returns>Returns the text dimensions.</returns>
        public static Vector2 DrawTextBlock(this SpriteBatch batch, SpriteFont font, IEnumerable<IFormattedText?>? text, Vector2 position, float wrapWidth, float scale = 1)
        {
            if (text == null)
                return new Vector2(0, 0);

            DrawTextHelper.InitIfNeeded();

            // track draw values
            float xOffset = 0;
            float yOffset = 0;
            float lineHeight = font.MeasureString("ABC").Y * scale;
            float spaceWidth = DrawHelper.GetSpaceWidth(font) * scale;
            float blockWidth = 0;
            float blockHeight = lineHeight;

            // draw text snippets
            DrawTextHelper.InitIfNeeded();
            foreach (IFormattedText? snippet in text)
            {
                if (snippet?.Text == null)
                    continue;

                // build word list
                string[] rawWords = snippet.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (rawWords.Length > 0)
                {
                    // keep surrounding spaces, since this snippet may be drawn before or after another
                    if (snippet.Text.StartsWith(" "))
                        rawWords[0] = $" {rawWords[0]}";
                    if (snippet.Text.EndsWith(" "))
                        rawWords[^1] += " ";
                }
                //for (int i = 0; i < rawWords.Length; i++) rawWords[i] = '[' + rawWords[i] + ']';

                // draw words
                bool isFirstOfLine = true;
                foreach (string rawWord in rawWords)
                {
                    // split within words if needed (e.g. list separators)
                    bool isStartOfWord = true;
                    foreach (string wordPart in DrawTextHelper.SplitWithinWordForLineWrapping(rawWord))
                    {
                        // check wrap width
                        float wordWidth = font.MeasureString(wordPart).X * scale;
                        float prependSpace = isStartOfWord && !isFirstOfLine
                            ? spaceWidth
                            : 0; // no space around soft breaks or start of line

                        if (wordPart == Environment.NewLine || ((wordWidth + xOffset + prependSpace) > wrapWidth && (int)xOffset != 0))
                        {
                            xOffset = 0;
                            yOffset += lineHeight;
                            blockHeight += lineHeight;
                            isFirstOfLine = true;
                        }
                        if (wordPart == Environment.NewLine)
                            continue;

                        // draw text
                        Vector2 wordPosition = new Vector2(position.X + xOffset + prependSpace, position.Y + yOffset);
                        if (snippet.Bold)
                            Utility.drawBoldText(batch, wordPart, font, wordPosition, snippet.Color ?? Color.Black, scale);
                        else
                            batch.DrawString(font, wordPart, wordPosition, snippet.Color ?? Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 1);

                        // update draw values
                        if (xOffset + wordWidth + prependSpace > blockWidth)
                            blockWidth = xOffset + wordWidth + prependSpace;
                        xOffset += wordWidth + prependSpace;

                        isFirstOfLine = false;
                        isStartOfWord = false;
                    }
                }
            }

            // return text position & dimensions
            return new Vector2(blockWidth, blockHeight);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Initialize for the current language if needed.</summary>
        public static void InitIfNeeded()
        {
            string language = LocalizedContentManager.CurrentLanguageString;

            if (DrawTextHelper.LastLanguage != language)
            {
                string characters = I18n.GetByKey(I18n.Keys.Generic_LineWrapOn).UsePlaceholder(false);

                DrawTextHelper.SoftBreakCharacters.Clear();
                if (!string.IsNullOrEmpty(characters))
                    DrawTextHelper.SoftBreakCharacters.AddRange(characters);

                DrawTextHelper.LastLanguage = language;
            }
        }

        /// <summary>Split a word into segments based on newlines and soft-break characters.</summary>
        /// <param name="text">The text to split.</param>
        private static IList<string> SplitWithinWordForLineWrapping(string text)
        {
            HashSet<char> splitChars = DrawTextHelper.SoftBreakCharacters;
            string newLine = Environment.NewLine;

            // handle soft breaks within word
            List<string> words = new List<string>();
            int start = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                // newline marker
                if (ch == newLine[0] && DrawTextHelper.IsNewlineAt(text, i))
                {
                    if (i > start)
                        words.Add(text.Substring(start, i - start));
                    words.Add(newLine);

                    i += newLine.Length;
                    start = i;
                }

                // soft break character
                else if (splitChars.Contains(ch))
                {
                    words.Add(text.Substring(start, i - start + 1));
                    start = i + 1;
                }
            }

            // add any remainder
            if (start == 0)
                words.Add(text);
            else if (start < text.Length - 1)
                words.Add(text.Substring(start));

            return words;
        }

        /// <summary>Get whether there's a newline sequence at a given text position.</summary>
        /// <param name="text">The text to search.</param>
        /// <param name="index">The index to check.</param>
        private static bool IsNewlineAt(string text, int index)
        {
            string newline = Environment.NewLine;

            for (int i = index, n = 0; i < text.Length && n < newline.Length; i++, n++)
            {
                if (text[i] != newline[n])
                    return false;
            }

            return true;
        }
    }
}
