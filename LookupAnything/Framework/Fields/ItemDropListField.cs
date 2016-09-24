using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows a list of item drops.</summary>
    internal class ItemDropListField : GenericField
    {
        /*********
        ** Properties
        *********/
        /// <summary>The items to list.</summary>
        private readonly Item[] Items;

        /// <summary>The text to display if there are no items.</summary>
        private readonly string DefaultText;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="items">The items to list.</param>
        /// <param name="defaultText">The text to display if there are no items (or <c>null</c> to hide the field).</param>
        public ItemDropListField(string label, Item[] items, string defaultText = null)
            : base(label, null, hasValue: defaultText != null && items.Any())
        {
            this.Items = items;
            this.DefaultText = defaultText;
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            if (!this.Items.Any())
                return spriteBatch.DrawTextBlock(font, this.DefaultText, position, wrapWidth);

            // get icon size
            Vector2 iconSize = new Vector2(font.MeasureString("ABC").Y);

            // list items
            float height = 0;
            foreach (Item item in this.Items)
            {
                spriteBatch.DrawIcon(item, position.X, position.Y + height, iconSize);
                Vector2 textSize = spriteBatch.DrawTextBlock(font, item.Name, position + new Vector2(iconSize.X + 5, height + 5), wrapWidth);
                height += textSize.Y + 5;
            }

            // return size
            return new Vector2(wrapWidth, height);
        }
    }
}