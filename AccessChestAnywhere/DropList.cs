using System;
using System.Collections.Generic;
using System.Linq;
using AccessChestAnywhere.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AccessChestAnywhere
{
    /// <summary>A dropdown UI component which lets the player choose from a list of values.</summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    internal class DropList<TItem> : ClickableComponent
        where TItem : class
    {
        /*********
        ** Properties
        *********/
        /// <summary>The index of the selected item.</summary>
        private int SelectedIndex;

        /// <summary>The selected item.</summary>
        private TItem SelectedItem => this.Items.First(p => p.Index == this.SelectedIndex).Value;

        /// <summary>The items in the list.</summary>
        private readonly DropListItem<TItem>[] Items;

        /// <summary>Whether the dropdown should be aligned right of the origin.</summary>
        private readonly bool ToRight;

        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font;

        /// <summary>The height of text rendered in the <see cref="Font"/>.</summary>
        private readonly int FontHeight;

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
        /// <param name="selectedItem">The selected item.</param>
        /// <param name="items">The items in the list.</param>
        /// <param name="nameSelector">A lambda which returns the display name for an item.</param>
        /// <param name="x">The X-position from which to render the list.</param>
        /// <param name="y">The Y-position from which to render the list.</param>
        /// <param name="toRight">Whether the dropdown should be aligned right of the origin.</param>
        /// <param name="font">The font with which to render text.</param>
        public DropList(TItem selectedItem, TItem[] items, Func<TItem, string> nameSelector, int x, int y, bool toRight, SpriteFont font)
            : base(new Rectangle(), nameof(DropList<TItem>))
        {
            // save values
            this.SelectedIndex = Array.IndexOf(items, selectedItem);
            this.Items = items
                .Select((item, index) => new DropListItem<TItem>(index, nameSelector(item), item))
                .ToArray();
            this.Font = font;
            this.FontHeight = (int)font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y;
            this.ToRight = toRight;

            // set dimensions
            this.bounds.Width = Game1.tileSize * 7;
            this.bounds.Height = this.FontHeight * 10 + Game1.tileSize / 16 * 9;
            this.bounds.X = toRight
                ? x
                : x - this.bounds.Width;
            this.bounds.Y = y;

            // initialise UI
            this.InitialiseComponents();
            if (this.SelectedIndex > 9)
            {
                int num = this.SelectedIndex;
                for (int i = 9; i >= 0; i--)
                {
                    this.ItemComponents[i].name = num.ToString();
                    num--;
                }
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
        /// <returns>Returns the selected item, or <c>null</c> if the coordinate wasn't found.</returns>
        public TItem Select(int x, int y)
        {
            foreach (ClickableComponent component in this.ItemComponents)
            {
                if (component.containsPoint(x, y))
                {
                    this.SelectedIndex = int.Parse(component.name);
                    return this.SelectedItem;
                }
            }
            return null;
        }

        /// <summary>Prepare the components for rendering.</summary>
        private void InitialiseComponents()
        {
            int x = this.bounds.X;
            int y = this.bounds.Y;
            int width = this.bounds.Width;
            int height = this.FontHeight;
            for (int i = 0; i < this.Items.Length && i < 10; i++)
            {
                this.ItemComponents.Add(new ClickableComponent(new Rectangle(x, y, width, height), i.ToString()));
                y += this.FontHeight;
            }
        }

        /// <summary>Render the UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        public void Draw(SpriteBatch sprites)
        {
            foreach (ClickableComponent component in this.ItemComponents)
            {
                // draw background
                if (component.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    sprites.Draw(Game1.mouseCursors, component.bounds, this.HoverBackground, Color.White);
                else if (component.name.Equals(this.SelectedIndex.ToString()))
                    sprites.Draw(Game1.mouseCursors, component.bounds, this.ActiveBackground, Color.White);
                else
                    sprites.Draw(Game1.mouseCursors, component.bounds, this.InactiveBackground, Color.White);

                // draw text
                DropListItem<TItem> item = this.Items.First(p => p.Index == int.Parse(component.name));
                Vector2 position = this.ToRight
                        ? new Vector2(component.bounds.X, component.bounds.Y + Game1.tileSize / 16)
                        : new Vector2(component.bounds.X + component.bounds.Width - this.Font.MeasureString(item.Name).X, component.bounds.Y + Game1.tileSize / 16);
                sprites.DrawString(this.Font, item.Name, position, Color.Black);
            }
        }
    }
}
