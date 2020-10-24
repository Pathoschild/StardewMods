using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.UI
{
    /// <summary>A button UI component which lets the player trigger a dropdown list.</summary>
    internal class Dropdown<TItem> : ClickableComponent
    {
        /*********
        ** Fields
        *********/
        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font;

        /// <summary>The dropdown list.</summary>
        private readonly DropdownList<TItem> List;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the dropdown list is expanded.</summary>
        public bool IsExpanded { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The displayed tab text.</param>
        /// <param name="x">The X-position at which to draw the tab.</param>
        /// <param name="y">The Y-position at which to draw the tab.</param>
        /// <param name="font">The font with which to render text.</param>
        /// <param name="selectedItem">The selected item.</param>
        /// <param name="items">The items in the list.</param>
        /// <param name="nameSelector">A lambda which returns the display name for an item.</param>
        /// <param name="rightAlign">Whether the button should be right-aligned, so it's left of the <paramref name="x"/> and <paramref name="y"/> position.</param>
        public Dropdown(string name, int x, int y, SpriteFont font, TItem selectedItem, TItem[] items, Func<TItem, string> nameSelector, bool rightAlign = false)
            : base(Rectangle.Empty, name)
        {
            // save values
            this.Font = font;

            // set bounds
            Vector2 size = Dropdown<TItem>.GetTabSize(font, name);
            this.bounds.Width = (int)size.X;
            this.bounds.Height = (int)size.Y;
            this.bounds.X = x;
            if (rightAlign)
                this.bounds.X -= this.bounds.Width;
            this.bounds.Y = y;

            // create dropdown
            this.List = new DropdownList<TItem>(selectedItem, items, nameSelector, this.bounds.X, this.bounds.Bottom, font);
        }

        /// <summary>Get whether the dropdown contains the given UI pixel position.</summary>
        /// <param name="x">The UI X position.</param>
        /// <param name="y">The UI Y position.</param>
        public override bool containsPoint(int x, int y)
        {
            return
                base.containsPoint(x, y)
                || (this.IsExpanded && this.List.containsPoint(x, y));
        }

        /// <summary>Select an item in the list if it's under the cursor.</summary>
        /// <param name="x">The X-position of the item in the UI.</param>
        /// <param name="y">The Y-position of the item in the UI.</param>
        /// <param name="selected">The selected item, if found.</param>
        /// <returns>Returns whether an item was selected.</returns>
        public bool TrySelect(int x, int y, out TItem selected)
        {
            if (this.IsExpanded)
                return this.List.TrySelect(x, y, out selected);

            selected = default;
            return false;
        }

        /// <summary>A method invoked when the player scrolls the dropdown using the mouse wheel.</summary>
        /// <param name="direction">The scroll direction.</param>
        public void ReceiveScrollWheelAction(int direction)
        {
            if (this.IsExpanded)
                this.List.ReceiveScrollWheelAction(direction);
        }

        /// <summary>Render the tab UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        /// <param name="opacity">The opacity at which to draw.</param>
        public void Draw(SpriteBatch sprites, float opacity = 1)
        {
            // calculate
            int x = this.bounds.X;
            int y = this.bounds.Y;
            int width = this.bounds.Width;
            int height = this.bounds.Height;
            int zoom = Game1.pixelZoom;
            Color color = Color.White * opacity;
            int borderWidth = CommonSprites.Tab.Left.Width * zoom;
            int cornerWidth = CommonSprites.Tab.TopLeft.Width * zoom;

            // draw tab
            var sheet = CommonSprites.Tab.Sheet;
            sprites.Draw(sheet, CommonSprites.Tab.Background, x + borderWidth, y + borderWidth, width - borderWidth * 2, height - borderWidth * 2, color);
            sprites.Draw(sheet, CommonSprites.Tab.Top, x + cornerWidth, y, width - borderWidth * 2, borderWidth, color);
            sprites.Draw(sheet, CommonSprites.Tab.Left, x, y + cornerWidth, borderWidth, height - borderWidth * 2, color);
            sprites.Draw(sheet, CommonSprites.Tab.Right, x + width - borderWidth, y + cornerWidth, borderWidth, height - cornerWidth * 2, color);
            sprites.Draw(sheet, CommonSprites.Tab.Bottom, x + cornerWidth, y + height - borderWidth, width - borderWidth * 2, borderWidth, color);
            sprites.Draw(sheet, CommonSprites.Tab.TopLeft, x, y, cornerWidth, cornerWidth, color);
            sprites.Draw(sheet, CommonSprites.Tab.TopRight, x + width - cornerWidth, y, cornerWidth, cornerWidth, color);
            sprites.Draw(sheet, CommonSprites.Tab.BottomLeft, x, y + height - cornerWidth, cornerWidth, cornerWidth, color);
            sprites.Draw(sheet, CommonSprites.Tab.BottomRight, x + width - cornerWidth, y + height - cornerWidth, cornerWidth, cornerWidth, color);
            sprites.DrawString(this.Font, this.name, new Vector2(x + cornerWidth, y + cornerWidth), Color.Black * opacity);

            // draw dropdown
            if (this.IsExpanded)
                this.List.Draw(sprites, opacity);
        }

        /// <summary>Get the size of the tab rendered for a given label.</summary>
        /// <param name="font">The font with which to render text.</param>
        /// <param name="name">The displayed tab text.</param>
        public static Vector2 GetTabSize(SpriteFont font, string name)
        {
            return
                font.MeasureString(name) // get font size
                - new Vector2(0, 10) // adjust for font's broken measurement
                + new Vector2(CommonSprites.Tab.TopLeft.Width * 2 * Game1.pixelZoom); // add space for borders
        }
    }
}
