using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Components;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows a progress bar UI.</summary>
    public class PercentageBarField : GenericField
    {
        /*********
        ** Properties
        *********/
        /// <summary>The current progress value.</summary>
        private readonly int CurrentValue;

        /// <summary>The maximum progress value.</summary>
        private readonly int MaxValue;

        /// <summary>The text to show next to the progress bar (if any).</summary>
        private readonly string Text;

        /// <summary>The color of the filled bar.</summary>
        private readonly Color FilledColor;

        /// <summary>The color of the empty bar.</summary>
        private readonly Color EmptyColor;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="currentValue">The current progress value.</param>
        /// <param name="maxValue">The maximum progress value.</param>
        /// <param name="filledColor">The color of the filled bar.</param>
        /// <param name="emptyColor">The color of the empty bar.</param>
        /// <param name="text">The text to show next to the progress bar (if any).</param>
        public PercentageBarField(string label, int currentValue, int maxValue, Color filledColor, Color emptyColor, string text)
            : base(label, null, hasValue: true)
        {
            this.CurrentValue = currentValue;
            this.MaxValue = maxValue;
            this.FilledColor = filledColor;
            this.EmptyColor = emptyColor;
            this.Text = text;
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="sprites">The sprite batch in which to draw.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped..</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch sprites, SpriteFont font, Vector2 position, float wrapWidth)
        {
            // calculate bars
            int barHeight = 22;
            float ratio = this.CurrentValue / (this.MaxValue * 1f);
            float maxWidth = Math.Min(100, wrapWidth);
            float filledWidth = maxWidth * ratio;
            float emptyWidth = maxWidth - filledWidth;

            // draw
            if (filledWidth > 0)
                sprites.Draw(Sprites.GetPixel(), new Rectangle((int)position.X, (int)position.Y, (int)filledWidth, barHeight), this.FilledColor);
            if (emptyWidth > 0)
                sprites.Draw(Sprites.GetPixel(), new Rectangle((int)(position.X + filledWidth), (int)position.Y, (int)emptyWidth, barHeight), this.EmptyColor);
            Vector2 textSize = !string.IsNullOrWhiteSpace(this.Text)
                ? sprites.DrawStringBlock(font, this.Text, new Vector2(position.X + maxWidth + 3, position.Y), wrapWidth)
                : Vector2.Zero;

            return new Vector2(maxWidth + 3 + textSize.X, Math.Max(barHeight, textSize.Y));
        }
    }
}