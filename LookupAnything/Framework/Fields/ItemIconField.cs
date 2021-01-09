using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows an item icon.</summary>
    internal class ItemIconField : GenericField, ILinkField
    {
        /*********
        ** Fields
        *********/
        /// <summary>The item icon to draw.</summary>
        private readonly SpriteInfo Sprite;

        /// <summary>Gets the subject the link points to, if applicable.</summary>
        private readonly ISubject LinkSubject;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="label">A short field label.</param>
        /// <param name="item">The item for which to display an icon.</param>
        /// <param name="codex">Provides subject entries to create a link, if applicable.</param>
        /// <param name="text">The text to display (if not the item name).</param>
        public ItemIconField(GameHelper gameHelper, string label, Item item, ISubjectRegistry codex, string text = null)
            : base(label, hasValue: item != null)
        {
            this.Sprite = gameHelper.GetSprite(item);

            if (item != null)
            {
                this.LinkSubject = codex?.GetByEntity(item);

                text = !string.IsNullOrWhiteSpace(text) ? text : item.DisplayName;
                Color? color = this.LinkSubject != null ? Color.Blue : null;
                this.Value = new IFormattedText[] { new FormattedText(text, color) };
            }
        }

        /// <inheritdoc />
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            // get icon size
            float textHeight = font.MeasureString("ABC").Y;
            Vector2 iconSize = new Vector2(textHeight);

            // draw icon & text
            spriteBatch.DrawSpriteWithin(this.Sprite, position.X, position.Y, iconSize);
            Vector2 textSize = spriteBatch.DrawTextBlock(font, this.Value, position + new Vector2(iconSize.X + 5, 5), wrapWidth);

            // return size
            return new Vector2(wrapWidth, textSize.Y + 5);
        }

        /// <inheritdoc />
        public ISubject GetLinkSubject()
        {
            return this.LinkSubject;
        }
    }
}
