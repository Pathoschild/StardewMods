using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AccessChestAnywhere
{
    /// <summary>A dropdown UI component which lets the player choose from a list of values.</summary>
    internal class DropList : ClickableComponent
    {
        /*********
        ** Properties
        *********/
        /// <summary>The index of the selected item.</summary>
        private int SelectedIndex;

        /// <summary>The items in the list.</summary>
        private readonly string[] Items;

        /// <summary>Whether the dropdown should be aligned right of the origin.</summary>
        private readonly bool ToRight;

        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font;

        /// <summary>The background for the selected item.</summary>
        private readonly Rectangle ActiveBackground = new Rectangle(258, 258, 4, 4);

        /// <summary>The background for a non-selected, non-hovered item.</summary>
        private readonly Rectangle InactiveBackground = new Rectangle(269, 258, 4, 4);

        /// <summary>The background for an item under the cursor.</summary>
        private readonly Rectangle HoverBackground = new Rectangle(161, 340, 4, 4);

        /// <summary>The clickable components representing the list items.</summary>
        private readonly List<ClickableComponent> ItemComponents = new List<ClickableComponent>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="selectedIndex">The index of the selected item.</param>
        /// <param name="items">The items in the list.</param>
        /// <param name="x">The X-position from which to render the list.</param>
        /// <param name="y">The Y-position from which to render the list.</param>
        /// <param name="toRight">Whether the dropdown should be aligned right of the origin.</param>
        /// <param name="font">The font with which to render text.</param>
        public DropList(int selectedIndex, string[] items, int x, int y, bool toRight, SpriteFont font)
            : base(new Rectangle(), selectedIndex.ToString())
        {
            this.SelectedIndex = selectedIndex;
            this.Items = items;
            this.Font = font;
            this.ToRight = toRight;
            this.bounds.Width = Game1.tileSize * 7;
            this.bounds.Height = (int)font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y * 10 + Game1.tileSize / 16 * 9;
            this.bounds.X = toRight
                ? x
                : x - this.bounds.Width;
            this.bounds.Y = y;

            this.InitialiseComponents();
            if (selectedIndex <= 9)
                return;

            int num = selectedIndex;
            for (int i = 9; i >= 0; i--)
            {
                this.ItemComponents[i].name = num.ToString();
                num--;
            }
        }

        /// <summary>A method invoked when the player scrolls the dropdown using the mouse wheel.</summary>
        /// <param name="direction">The scroll direction.</param>
        public void ReceiveScrollWheelAction(int direction)
        {
            if (direction > 0)
            {
                if (this.ItemComponents.Count != 10 || int.Parse(this.ItemComponents[9].name) <= 9)
                    return;
                foreach (var current in this.ItemComponents)
                    current.name = (int.Parse(current.name) - 1).ToString();
            }
            else if (this.ItemComponents.Count == 10 && int.Parse(this.ItemComponents[9].name) + 1 < this.Items.Length)
            {
                foreach (var current in this.ItemComponents)
                    current.name = (int.Parse(current.name) + 1).ToString();
            }
        }

        /// <summary>Select an item in the list.</summary>
        /// <param name="x">The X-position of the item in the UI.</param>
        /// <param name="y">The Y-position of the item in the UI.</param>
        /// <returns>Returns the selected index, or -1 if the coordinate wasn't found.</returns>
        public int Select(int x, int y)
        {
            foreach (ClickableComponent component in this.ItemComponents)
            {
                if (component.containsPoint(x, y))
                {
                    this.SelectedIndex = int.Parse(component.name);
                    return this.SelectedIndex;
                }
            }
            return -1;
        }

        /// <summary>Prepare the components for rendering.</summary>
        private void InitialiseComponents()
        {
            int x = this.bounds.X;
            int y = this.bounds.Y;
            int width = this.bounds.Width;
            int height = (int)this.Font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y;
            for (int i = 0; i < this.Items.Length && i < 10; i++)
            {
                this.ItemComponents.Add(new ClickableComponent(new Rectangle(x, y, width, height), i.ToString()));
                y += (int)this.Font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y + Game1.tileSize / 16;
            }
        }

        /// <summary>Render the UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        public void Draw(SpriteBatch sprites)
        {
            foreach (ClickableComponent component in this.ItemComponents)
            {
                if (component.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    sprites.Draw(Game1.mouseCursors, component.bounds, this.HoverBackground, Color.White);
                else if (component.name.Equals(this.SelectedIndex.ToString()))
                    sprites.Draw(Game1.mouseCursors, component.bounds, this.ActiveBackground, Color.White);
                else
                    sprites.Draw(Game1.mouseCursors, component.bounds, this.InactiveBackground, Color.White);

                Vector2 position = this.ToRight
                        ? new Vector2(component.bounds.X, component.bounds.Y + Game1.tileSize / 16)
                        : new Vector2(component.bounds.X + component.bounds.Width - this.Font.MeasureString(this.Items[int.Parse(component.name)]).X, component.bounds.Y + Game1.tileSize / 16);
                sprites.DrawString(this.Font, this.Items[int.Parse(component.name)], position, Color.Black);
            }
        }
    }
}
