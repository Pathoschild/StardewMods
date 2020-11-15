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
    /// <typeparam name="TValue">The item value type.</typeparam>
    internal class DropdownList<TValue> : ClickableComponent
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
        /// <summary>The selected option.</summary>
        private DropListOption SelectedOption;

        /// <summary>The options in the list.</summary>
        private readonly DropListOption[] Options;

        /// <summary>The item index shown at the top of the list.</summary>
        private int FirstVisibleIndex;

        /// <summary>The maximum items to display.</summary>
        private int MaxItems;

        /// <summary>The item index shown at the bottom of the list.</summary>
        private int LastVisibleIndex => this.FirstVisibleIndex + this.MaxItems - 1;

        /// <summary>The maximum index that can be shown at the top of the list.</summary>
        private int MaxFirstVisibleIndex => this.Options.Length - this.MaxItems;


        /****
        ** Rendering
        ****/
        /// <summary>The font with which to render text.</summary>
        private readonly SpriteFont Font;


        /*********
        ** Accessors
        *********/
        /// <summary>The selected value.</summary>
        public TValue SelectedValue => this.SelectedOption.Value;

        /// <summary>The display label for the selected value.</summary>
        public string SelectedLabel => this.SelectedOption.label;

        /// <summary>The maximum height for the possible labels.</summary>
        public int MaxLabelHeight { get; }

        /// <summary>The maximum width for the possible labels.</summary>
        public int MaxLabelWidth { get; private set; }

        /// <summary>The <see cref="ClickableComponent.myID"/> value for the top entry in the dropdown.</summary>
        public int TopComponentId => this.Options.First(p => p.visible).myID;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="selectedValue">The selected value.</param>
        /// <param name="items">The items in the list.</param>
        /// <param name="getLabel">Get the display label for an item.</param>
        /// <param name="x">The X-position from which to render the list.</param>
        /// <param name="y">The Y-position from which to render the list.</param>
        /// <param name="font">The font with which to render text.</param>
        public DropdownList(TValue selectedValue, TValue[] items, Func<TValue, string> getLabel, int x, int y, SpriteFont font)
            : base(new Rectangle(), nameof(DropdownList<TValue>))
        {
            // save values
            this.Options = items
                .Select((item, index) => new DropListOption(Rectangle.Empty, index, getLabel(item), item, font))
                .ToArray();
            this.Font = font;
            this.MaxLabelHeight = this.Options.Max(p => p.LabelHeight);

            // set initial selection
            int selectedIndex = Array.IndexOf(items, selectedValue);
            this.SelectedOption = selectedIndex >= 0
                ? this.Options[selectedIndex]
                : this.Options.First();

            // initialize UI
            this.bounds.X = x;
            this.bounds.Y = y;
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
        /// <param name="selected">The selected value, if found.</param>
        /// <returns>Returns whether an item was selected.</returns>
        public bool TrySelect(int x, int y, out TValue selected)
        {
            var option = this.Options.FirstOrDefault(p => p.visible && p.containsPoint(x, y));
            if (option != null)
            {
                this.SelectedOption = option;
                selected = option.Value;
                return true;
            }

            selected = default;
            return false;
        }

        /// <summary>Select an item in the list matching the given value.</summary>
        /// <param name="value">The value to search.</param>
        /// <returns>Returns whether an item was selected.</returns>
        public bool TrySelect(TValue value)
        {
            var entry = this.Options.FirstOrDefault(p =>
                (p.Value == null && value == null)
                || p.Value?.Equals(value) == true
            );

            if (entry == null)
                return false;

            this.SelectedOption = entry;
            return true;
        }

        /// <summary>Render the UI.</summary>
        /// <param name="sprites">The sprites to render.</param>
        /// <param name="opacity">The opacity at which to draw.</param>
        public void Draw(SpriteBatch sprites, float opacity = 1)
        {
            // draw dropdown items
            foreach (DropListOption option in this.Options)
            {
                if (!option.visible)
                    continue;

                // draw background
                if (option.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    sprites.Draw(CommonSprites.DropDown.Sheet, option.bounds, CommonSprites.DropDown.HoverBackground, Color.White * opacity);
                else if (option.Index == this.SelectedOption.Index)
                    sprites.Draw(CommonSprites.DropDown.Sheet, option.bounds, CommonSprites.DropDown.ActiveBackground, Color.White * opacity);
                else
                    sprites.Draw(CommonSprites.DropDown.Sheet, option.bounds, CommonSprites.DropDown.InactiveBackground, Color.White * opacity);

                // draw text
                Vector2 position = new Vector2(option.bounds.X + DropdownList<TValue>.DropdownPadding, option.bounds.Y + Game1.tileSize / 16);
                sprites.DrawString(this.Font, option.label, position, Color.Black * opacity);
            }

            // draw up/down arrows
            if (this.FirstVisibleIndex > 0)
                sprites.Draw(CommonSprites.Icons.Sheet, new Vector2(this.bounds.X - CommonSprites.Icons.UpArrow.Width, this.bounds.Y), CommonSprites.Icons.UpArrow, Color.White * opacity);
            if (this.FirstVisibleIndex < this.MaxFirstVisibleIndex)
                sprites.Draw(CommonSprites.Icons.Sheet, new Vector2(this.bounds.X - CommonSprites.Icons.UpArrow.Width, this.bounds.Y + this.bounds.Height - CommonSprites.Icons.DownArrow.Height), CommonSprites.Icons.DownArrow, Color.White * opacity);
        }

        /// <summary>Recalculate dimensions and components for rendering.</summary>
        public void ReinitializeComponents()
        {
            // get item size
            int itemWidth = this.MaxLabelWidth = Math.Max(this.Options.Max(p => p.LabelWidth), Game1.tileSize * 2) + DropdownList<TValue>.DropdownPadding * 2;
            int itemHeight = this.MaxLabelHeight;

            // get pagination
            this.MaxItems = Math.Min((Game1.viewport.Height - this.bounds.Y) / itemHeight, this.Options.Length);
            this.FirstVisibleIndex = this.GetValidFirstItem(this.FirstVisibleIndex, this.MaxFirstVisibleIndex);

            // get dropdown size
            this.bounds.Width = itemWidth;
            this.bounds.Height = itemHeight * this.MaxItems;

            // update components
            int x = this.bounds.X;
            int y = this.bounds.Y;
            foreach (var option in this.Options)
            {
                option.visible = option.Index >= this.FirstVisibleIndex && option.Index <= this.LastVisibleIndex;
                if (option.visible)
                {
                    option.bounds = new Rectangle(x, y, itemWidth, itemHeight);
                    y += itemHeight;
                }
            }

            // update controller flow
            this.ReinitializeControllerFlow();
        }

        /// <summary>Set the fields to support controller snapping.</summary>
        public void ReinitializeControllerFlow()
        {
            int firstIndex = this.FirstVisibleIndex;
            int lastIndex = this.LastVisibleIndex;

            int initialId = 1_100_000;
            foreach (var option in this.Options)
            {
                int index = option.Index;
                int id = initialId + index;

                option.myID = id;
                option.upNeighborID = index > firstIndex
                    ? id - 1
                    : -99999;
                option.downNeighborID = index < lastIndex
                    ? id + 1
                    : -1;
            }
        }

        /// <summary>Get the nested components for controller snapping.</summary>
        public IEnumerable<ClickableComponent> GetChildComponents()
        {
            return this.Options;
        }


        /*********
        ** Private methods
        *********/
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
            this.ReinitializeComponents();
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
        /// <summary>A clickable option in the dropdown.</summary>
        private class DropListOption : ClickableComponent
        {
            /*********
            ** Accessors
            *********/
            /// <summary>The option's index in the list.</summary>
            public int Index { get; }

            /// <summary>The option value.</summary>
            public TValue Value { get; }

            /// <summary>The label text width in pixels.</summary>
            public int LabelWidth { get; }

            /// <summary>The label text height in pixels.</summary>
            public int LabelHeight { get; }


            /*********
            ** Public methods
            *********/
            /// <summary>Construct an instance.</summary>
            /// <param name="bounds">The pixel bounds on screen.</param>
            /// <param name="index">The option's index in the list.</param>
            /// <param name="label">The display text.</param>
            /// <param name="value">The option value.</param>
            /// <param name="font">The font with which to measure the label.</param>
            public DropListOption(Rectangle bounds, int index, string label, TValue value, SpriteFont font)
                : base(bounds: bounds, name: index.ToString(), label: label)
            {
                this.Index = index;
                this.Value = value;

                Vector2 labelSize = font.MeasureString(label);
                this.LabelWidth = (int)labelSize.X;
                this.LabelHeight = (int)labelSize.Y;
            }
        }
    }
}
