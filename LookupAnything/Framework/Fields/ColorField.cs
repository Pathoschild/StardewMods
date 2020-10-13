using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields
{
    /// <summary>A metadata field which shows a color preview icon.</summary>
    internal class ColorField : GenericField
    {
        /*********
        ** Fields
        *********/
        /// <summary>The color to represent.</summary>
        private readonly Color Color;

        /// <summary>The dye strength.</summary>
        private readonly int Strength;

        /// <summary>Whether the field should display a shifting prismatic color scheme.</summary>
        private readonly bool IsPrismatic;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="label">A short field label.</param>
        /// <param name="item">The dye-producing item.</param>
        public ColorField(string label, Item item)
            : base(label)
        {
            // get color
            if (item.Name == "Prismatic Shard")
            {
                this.IsPrismatic = true;
                this.HasValue = true;
            }
            else
            {
                Color? color = TailoringMenu.GetDyeColor(item);
                if (color.HasValue)
                {
                    this.Color = color.Value;
                    this.HasValue = true;
                }
            }

            // get dye strength
            if (this.HasValue)
            {
                if (item.HasContextTag("dye_strong"))
                    this.Strength = 3;
                else if (item.HasContextTag("dye_medium"))
                    this.Strength = 2;
                else
                    this.Strength = 1;
            }
        }

        /// <summary>Draw the value (or return <c>null</c> to render the <see cref="GenericField.Value"/> using the default format).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="font">The recommended font.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="wrapWidth">The maximum width before which content should be wrapped.</param>
        /// <returns>Returns the drawn dimensions, or <c>null</c> to draw the <see cref="GenericField.Value"/> using the default format.</returns>
        public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
        {
            // get icon size
            float textHeight = font.MeasureString("ABC").Y;
            Vector2 iconSize = new Vector2(textHeight);

            // draw preview icon & text
            float offset = 0;
            Color color = this.IsPrismatic ? Utility.GetPrismaticColor() : this.Color;
            for (int i = 0; i < this.Strength; i++)
            {
                spriteBatch.DrawSpriteWithin(CommonHelper.Pixel, new Rectangle(0, 0, 1, 1), position.X + offset, position.Y, iconSize, color);
                offset += iconSize.X + 2;
            }

            // return size
            return new Vector2(wrapWidth, iconSize.Y + 5);
        }
    }
}
