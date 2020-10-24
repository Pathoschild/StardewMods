using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.UI
{
    /// <summary>A dropdown UI component which lets the player choose from a list of values.</summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    internal class DropdownList<TItem> : ClickableComponent
    {
        /*********
        ** Fields
        *********/
        /****
        ** Constants
        ****/
        /// <summary>The padding applied to dropdown lists.</summary>
        private const int DropdownPadding = 5;

        /****
        ** Items
        ****/
        /// <summary>The index of the selected item.</summary>
        private int SelectedIndex;

        /// <summary>The selected item.</summary>
        private TItem SelectedItem => this.Items.First(p => p.Index == this.SelectedIndex).Value;

        /// <summary>The items in the list.</summary>
        private readonly DropListItem[] Items;

        /// <summary>The item index shown at the top of the list.</summary>
        private int FirstVisibleIndex;

        /// <summary>The maximum items to display.</summary>
        private int MaxItems;

        /// <summary>The maximum index for the first item.</summary>
        private int MaxFirstVisibleIndex;

        /****
        ** Rendering
        ****/
        /// <summary>The dropdown's origin position.</summary>
        private readonly Vector2 Origin;

        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font;

        /// <summary>The height of text rendered in the <see cref="Font"/>.</summary>
        private readonly int FontHeight;

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
        /// <param name="font">The font with which to render text.</param>
        public DropdownList(TItem selectedItem, TItem[] items, Func<TItem, string> nameSelector, int x, int y, SpriteFont font)
            : base(new Rectangle(), nameof(DropdownList<TItem>))
        {
            // save values
            this.SelectedIndex = Array.IndexOf(items, selectedItem);
            this.Items = items
                .Select((item, index) => new DropListItem(index, nameSelector(item), item))
                .ToArray();
            this.Font = font;
            this.FontHeight = (int)font.MeasureString("abcdefghijklmnopqrstuvwxyz").Y;

            // initialize UI
            this.Origin = new Vector2(x, y);
            this.ReinitializeComponents();
        }

        /// <summary>A method invoked when the player scrolls the dropdown using the mouse wheel.</summary>
        /// <param name="direction">The scroll direction.</param>
        public void ReceiveScrollWheelAction(int direction)
        {
            this.Scroll(direction > 0 ? -1 : 1); // scrolling down moves first item up
        }

        /// <summary>Select an item in the list if it's under the cursor.</summary>
        /// <param name="x">The X-position of the item in the UI.</param>
        /// <param name="y">The Y-position of the item in the UI.</param>
        /// <param name="selected">The selected item, if found.</param>
        /// <returns>Returns whether an item was selected.</returns>
        public bool TrySelect(int x, int y, out TItem selected)
        {
            foreach (ClickableComponent component in this.ItemComponents)
            {
                if (component.containsPoint(x, y))
                {
                    this.SelectedIndex = int.Parse(component.name);
                    selected = this.SelectedItem;
                    return true;
                }
            }

            selected = default;
            return false;
        }

        /// <summary>Render the UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        /// <param name="opacity">The opacity at which to draw.</param>
        public void Draw(SpriteBatch sprites, float opacity = 1)
        {
            // draw dropdown items
            foreach (ClickableComponent component in this.ItemComponents)
            {
                // draw background
                if (component.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    sprites.Draw(CommonSprites.DropDown.Sheet, component.bounds, CommonSprites.DropDown.HoverBackground, Color.White * opacity);
                else if (component.name.Equals(this.SelectedIndex.ToString()))
                    sprites.Draw(CommonSprites.DropDown.Sheet, component.bounds, CommonSprites.DropDown.ActiveBackground, Color.White * opacity);
                else
                    sprites.Draw(CommonSprites.DropDown.Sheet, component.bounds, CommonSprites.DropDown.InactiveBackground, Color.White * opacity);

                // draw text
                DropListItem item = this.Items.First(p => p.Index == int.Parse(component.name));
                Vector2 position = new Vector2(component.bounds.X + DropdownList<TItem>.DropdownPadding, component.bounds.Y + Game1.tileSize / 16);
                sprites.DrawString(this.Font, item.Name, position, Color.Black * opacity);
            }

            // draw up/down arrows
            if (this.FirstVisibleIndex > 0)
                sprites.Draw(CommonSprites.Icons.Sheet, new Vector2(this.bounds.X - CommonSprites.Icons.UpArrow.Width, this.bounds.Y), CommonSprites.Icons.UpArrow, Color.White * opacity);
            if (this.FirstVisibleIndex < this.MaxFirstVisibleIndex)
                sprites.Draw(CommonSprites.Icons.Sheet, new Vector2(this.bounds.X - CommonSprites.Icons.UpArrow.Width, this.bounds.Y + this.bounds.Height - CommonSprites.Icons.DownArrow.Height), CommonSprites.Icons.DownArrow, Color.White * opacity);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Recalculate dimensions and components for rendering.</summary>
        private void ReinitializeComponents()
        {
            // get item size
            int minItemWidth = Game1.tileSize * 2;
            int itemWidth = Math.Max((int)this.Items.Max(p => this.Font.MeasureString(p.Name).X), minItemWidth) + DropdownList<TItem>.DropdownPadding * 2;
            int itemHeight = this.FontHeight;

            // get pagination
            int itemCount = this.Items.Length;
            this.MaxItems = Math.Min((Game1.viewport.Height - (int)this.Origin.Y) / itemHeight, itemCount);
            this.MaxFirstVisibleIndex = this.Items.Length - this.MaxItems;
            this.FirstVisibleIndex = this.GetValidFirstItem(this.FirstVisibleIndex, this.MaxFirstVisibleIndex);

            // get dropdown size
            this.bounds.Width = itemWidth;
            this.bounds.Height = itemHeight * this.MaxItems;
            this.bounds.X = (int)this.Origin.X;
            this.bounds.Y = (int)this.Origin.Y;

            // generate components
            this.ItemComponents.Clear();
            int x = this.bounds.X;
            int y = this.bounds.Y;
            for (int i = this.FirstVisibleIndex; i < this.MaxItems; i++)
            {
                this.ItemComponents.Add(new ClickableComponent(new Rectangle(x, y, itemWidth, itemHeight), i.ToString()));
                y += this.FontHeight;
            }
        }

        /// <summary>Scroll the dropdown by the specified amount.</summary>
        /// <param name="amount">The number of items to scroll.</param>
        private void Scroll(int amount)
        {
            // recalculate first item
            int firstItem = this.GetValidFirstItem(this.FirstVisibleIndex + amount, this.MaxFirstVisibleIndex);
            if (firstItem == this.FirstVisibleIndex)
                return;
            this.FirstVisibleIndex = firstItem;

            // update displayed items
            int itemIndex = firstItem;
            foreach (ClickableComponent current in this.ItemComponents)
            {
                current.name = itemIndex.ToString();
                itemIndex++;
            }
        }

        /// <summary>Calculate a valid index for the first displayed item in the list.</summary>
        /// <param name="value">The initial value, which may not be valid.</param>
        /// <param name="maxIndex">The maximum first index.</param>
        private int GetValidFirstItem(int value, int maxIndex)
        {
            return Math.Max(Math.Min(value, maxIndex), 0);
        }


        /*********
        ** Private models
        *********/
        /// <summary>An item in a drop list.</summary>
        private class DropListItem
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The item's index in the list.</summary>
            public int Index { get; }

            /// <summary>The display name.</summary>
            public string Name { get; }

            /// <summary>The item value.</summary>
            public TItem Value { get; }


            /*********
            ** Public methods
            *********/
            /// <summary>Construct an instance.</summary>
            /// <param name="index">The item's index in the list.</param>
            /// <param name="name">The display name.</param>
            /// <param name="value">The item value.</param>
            public DropListItem(int index, string name, TItem value)
            {
                this.Index = index;
                this.Name = name;
                this.Value = value;
            }
        }
    }
}
