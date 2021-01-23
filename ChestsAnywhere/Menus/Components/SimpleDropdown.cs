using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere.Menus.Components
{
    /// <summary>An input control which represents a dropdown of values.</summary>
    /// <typeparam name="TKey">The input key type.</typeparam>
    internal class SimpleDropdown<TKey>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying dropdown component.</summary>
        private readonly OptionsDropDown Dropdown;

        /// <summary>Whether the dropdown is currently expanded.</summary>
        private readonly IReflectedField<bool> IsExpandedField;

        /// <summary>The options in the dropdown list.</summary>
        private readonly List<Tuple<string, TKey, string>> Options;


        /*********
        ** Accessors
        *********/
        /// <summary>The selected key.</summary>
        public TKey SelectedKey => this.Options[this.Dropdown.selectedOption].Item2;

        /// <summary>The current bounds.</summary>
        public Rectangle Bounds => this.Dropdown.bounds;

        /// <summary>Whether the dropdown is currently expanded.</summary>
        public bool IsExpanded => this.IsExpandedField.GetValue();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="options">The available dropdown options.</param>
        /// <param name="selected">The selected key, if any.</param>
        /// <param name="getKeyString">The logic to get the </param>
        public SimpleDropdown(IReflectionHelper reflection, IEnumerable<KeyValuePair<TKey, string>> options, TKey selected = default, Func<TKey, string> getKeyString = null)
        {
            getKeyString ??= raw => raw?.ToString();

            // parse options
            var optionKeys = new List<string>();
            var optionLabels = new List<string>();
            var lookup = new List<Tuple<string, TKey, string>>();
            foreach (var option in options)
            {
                string keyStr = getKeyString(option.Key);

                optionKeys.Add(keyStr);
                optionLabels.Add(option.Value);
                lookup.Add(Tuple.Create(keyStr, option.Key, option.Value));
            }

            // build dropdown
            this.Options = lookup;
            this.Dropdown = new OptionsDropDown(null, -int.MaxValue)
            {
                dropDownOptions = optionKeys,
                dropDownDisplayOptions = optionLabels
            };
            this.IsExpandedField = reflection.GetField<bool>(this.Dropdown, "clicked");

            // select element
            this.TrySelect(selected);
        }

        /// <summary>Try to select the given key in the dropdown.</summary>
        /// <param name="key">The key to select.</param>
        /// <returns>Returns whether the value was selected.</returns>
        public bool TrySelect(TKey key)
        {
            int selectedIndex = this.Options.FindIndex(p => p.Item2.Equals(key));
            if (selectedIndex >= 0)
            {
                this.Dropdown.selectedOption = selectedIndex;
                return true;
            }

            return false;
        }

        /// <summary>Handle a click on the dropdown, if applicable.</summary>
        /// <param name="x">The cursor's X position.</param>
        /// <param name="y">The cursor's Y position.</param>
        /// <returns>Returns whether the click was handled.</returns>
        public bool TryClick(int x, int y)
        {
            // expand dropdown
            if (!this.IsExpanded)
            {
                if (this.Bounds.Contains(x, y))
                {
                    this.Dropdown.receiveLeftClick(x, y);
                    return true;
                }
                return false;
            }

            // select item in dropdown or close
            this.Dropdown.leftClickReleased(x, y);
            return true;
        }

        /// <summary>Handle the cursor hovering on the dropdown, if applicable.</summary>
        /// <param name="x">The cursor's X position.</param>
        /// <param name="y">The cursor's Y position.</param>
        /// <returns>Returns whether the hover was handled.</returns>
        public bool TryHover(int x, int y)
        {
            if (!this.IsExpanded)
                return false;

            this.Dropdown.leftClickHeld(x, y);
            return true;
        }

        /// <summary>Draw the checkbox to the screen.</summary>
        /// <param name="batch">The sprite batch.</param>
        /// <param name="x">The X position at which to draw.</param>
        /// <param name="y">The Y position at which to draw.</param>
        public void Draw(SpriteBatch batch, int x, int y)
        {
            var dropdown = this.Dropdown;

            if (x != dropdown.bounds.X || y != dropdown.bounds.Y)
            {
                dropdown.bounds = new Rectangle(x, y, dropdown.bounds.Width, dropdown.bounds.Height);
                dropdown.RecalculateBounds();
            }

            dropdown.draw(batch, 0, 0);
        }
    }
}
