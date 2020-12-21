using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.UI;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows a list of checkbox values.</summary>
    internal class CheckboxListField : GenericField
    {
        /*********
        ** Fields
        *********/
        /// <summary>The checkbox values to display.</summary>
        protected KeyValuePair<IFormattedText[], bool>[] Checkboxes;

        /// <summary>The intro text to show before the checkboxes.</summary>
        protected IFormattedText[] Intro;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="checkboxes">The checkbox labels and values to display.</param>
        public CheckboxListField(string label, IEnumerable<KeyValuePair<IFormattedText[], bool>> checkboxes)
            : this(label)
        {
            this.Checkboxes = checkboxes.ToArray();
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="checkboxes">The checkbox labels and values to display.</param>
        public CheckboxListField(string label, params KeyValuePair<IFormattedText[], bool>[] checkboxes)
            : this(label)
        {
            this.Checkboxes = checkboxes;
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            float topOffset = 0;
            float checkboxSize = CommonSprites.Icons.FilledCheckbox.Width * (Game1.pixelZoom / 2);
            float lineHeight = Math.Max(checkboxSize, Game1.smallFont.MeasureString("ABC").Y);
            float checkboxOffset = (lineHeight - checkboxSize) / 2;

            if (this.Intro != null)
                topOffset += spriteBatch.DrawTextBlock(font, this.Intro, position, wrapWidth).Y;

            foreach (KeyValuePair<IFormattedText[], bool> entry in this.Checkboxes)
            {
                // draw icon
                spriteBatch.Draw(
                    texture: CommonSprites.Icons.Sheet,
                    position: new Vector2(position.X, position.Y + topOffset + checkboxOffset),
                    sourceRectangle: entry.Value ? CommonSprites.Icons.FilledCheckbox : CommonSprites.Icons.EmptyCheckbox,
                    color: Color.White,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: checkboxSize / CommonSprites.Icons.FilledCheckbox.Width,
                    effects: SpriteEffects.None,
                    layerDepth: 1f
                );

                // draw text
                Vector2 textSize = spriteBatch.DrawTextBlock(Game1.smallFont, entry.Key, new Vector2(position.X + checkboxSize + 7, position.Y + topOffset), wrapWidth - checkboxSize - 7);

                // update offset
                topOffset += Math.Max(checkboxSize, textSize.Y);
            }

            return new Vector2(wrapWidth, topOffset);
        }

        /// <summary>Add intro text before the checkboxes.</summary>
        /// <param name="text">The text to show before the checkboxes.</param>
        public CheckboxListField AddIntro(params IFormattedText[] text)
        {
            this.Intro = text;
            return this;
        }

        /// <summary>Add intro text before the checkboxes.</summary>
        /// <param name="text">The text to show before the checkboxes.</param>
        public CheckboxListField AddIntro(params string[] text)
        {
            return this.AddIntro(
                text.Select(p => (IFormattedText)new FormattedText(p)).ToArray()
            );
        }

        /// <summary>Build a checkbox entry.</summary>
        /// <param name="value">Whether the value is enabled.</param>
        /// <param name="text">The checkbox text to display.</param>
        public static KeyValuePair<IFormattedText[], bool> Checkbox(bool value, params IFormattedText[] text)
        {
            return new KeyValuePair<IFormattedText[], bool>(text, value);
        }

        /// <summary>Build a checkbox entry.</summary>
        /// <param name="value">Whether the value is enabled.</param>
        /// <param name="text">The checkbox text to display.</param>
        public static KeyValuePair<IFormattedText[], bool> Checkbox(bool value, string text)
        {
            return CheckboxListField.Checkbox(value, new FormattedText(text));
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        protected CheckboxListField(string label)
            : base(label, hasValue: true)
        {
            this.Checkboxes = new KeyValuePair<IFormattedText[], bool>[0];
        }
    }
}
