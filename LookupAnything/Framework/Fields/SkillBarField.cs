using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Components;
using Pathoschild.LookupAnything.Framework.Constants;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows experience points for a skill.</summary>
    /// <remarks>Skill calculations reverse-engineered from <see cref="Farmer.checkForLevelGain"/>.</remarks>
    public class SkillBarField : PercentageBarField
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="experience">The current progress value.</param>
        /// <param name="filledColor">The color of the filled bar.</param>
        /// <param name="emptyColor">The color of the empty bar.</param>
        public SkillBarField(string label, int experience, Color filledColor, Color emptyColor)
            : base(label, experience, Constant.MaxSkillPoints, filledColor, emptyColor, null) { }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            int[] pointsPerLevel = Constant.SkillPointsPerLevel;

            // generate text
            int nextLevelExp = pointsPerLevel.FirstOrDefault(p => p - this.CurrentValue > 0);
            int pointsForNextLevel = nextLevelExp > 0 ? nextLevelExp - this.CurrentValue : 0;
            int currentLevel = nextLevelExp > 0 ? Array.IndexOf(pointsPerLevel, nextLevelExp) : pointsPerLevel.Length;
            string text = pointsForNextLevel > 0
                ? $"level {currentLevel} ({pointsForNextLevel} exp to next)"
                : $"level {currentLevel}";

            // draw bars
            const int barWidth = 25;
            float leftOffset = 0;
            int barHeight = 0;
            foreach (int levelExp in pointsPerLevel)
            {
                float progress = Math.Min(1f, this.CurrentValue / (levelExp * 1f));
                Vector2 barSize = this.DrawBar(spriteBatch, position + new Vector2(leftOffset, 0), progress, this.FilledColor, this.EmptyColor, barWidth);
                barHeight = (int)barSize.Y;
                leftOffset += barSize.X + 2;
            }

            // draw text
            Vector2 textSize = spriteBatch.DrawStringBlock(font, text, position + new Vector2(leftOffset, 0), wrapWidth - leftOffset);
            return new Vector2(leftOffset + textSize.X, Math.Max(barHeight, textSize.Y));
        }
    }
}