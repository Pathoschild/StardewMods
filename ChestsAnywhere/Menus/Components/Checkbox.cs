using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.UI;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Components
{
    /// <summary>An input control which represents a boolean value.</summary>
    internal class Checkbox
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying value.</summary>
        public bool Value { get; set; }

        /// <summary>The X position of the rendered textbox.</summary>
        public int X { get; set; }

        /// <summary>The Y position of the rendered textbox.</summary>
        public int Y { get; set; }

        /// <summary>The width of the rendered textbox.</summary>
        public int Width { get; set; } = CommonSprites.Icons.EmptyCheckbox.Width * Game1.pixelZoom;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="value">The initial value.</param>
        public Checkbox(bool value = false)
        {
            this.Value = value;
        }

        /// <summary>Toggle the checkbox value.</summary>
        public void Toggle()
        {
            this.Value = !this.Value;
        }

        /// <summary>Get the checkbox's bounds on the screen.</summary>
        public Rectangle GetBounds()
        {
            return new Rectangle(this.X, this.Y, this.Width, this.Width);
        }

        /// <summary>Draw the checkbox to the screen.</summary>
        /// <param name="batch">The sprite batch.</param>
        public void Draw(SpriteBatch batch)
        {
            float scale = this.Width / (float)CommonSprites.Icons.FilledCheckbox.Width;
            batch.Draw(CommonSprites.Icons.Sheet, new Vector2(this.X, this.Y), this.Value ? CommonSprites.Icons.FilledCheckbox : CommonSprites.Icons.EmptyCheckbox, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1f);
        }
    }
}
