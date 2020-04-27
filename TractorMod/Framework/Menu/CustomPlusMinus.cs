using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.TractorMod.Framework.Menu
{
    class CustomPlusMinus : OptionsPlusMinus
    {
        public string Name { get; set; }
        /// <summary>
        /// Custom Plus and Minus that does not affect screen zoom
        /// </summary>
        /// <param name="name">Name of the component</param>
        /// <param name="label">Label to be displayed next to the PlusMinus</param>
        /// <param name="whichoption">WhichOption render option</param>
        /// <param name="options">The actual value of the component</param>
        /// <param name="displayOptions">The displayed value of the Component</param>
        /// <param name="initialValue">Where the component should start</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        public CustomPlusMinus(string name,string label,int whichoption, List<string> options, List<string> displayOptions,int initialValue, int x1 = -1, int y1 = -1)
            : base(label,whichoption,options,displayOptions,x1,y1)
        {
            this.selected = options.IndexOf(initialValue.ToString());
            this.Name = name;
        }

        /// <summary>
        /// When the component is clicked
        /// </summary>
        /// <param name="x">X of the mouse</param>
        /// <param name="y">Y of the mouse</param>
        public override void receiveLeftClick(int x, int y)
        {
            if (this.greyedOut || this.options.Count <= 0)
                return;
            int selected1 = this.selected;
            if (this.minusButton.Contains(x, y) && this.selected != 0)
            {
                this.selected -= 1;
                Game1.playSound("drumkit6");
            }
            else if (this.plusButton.Contains(x, y) && this.selected != this.options.Count - 1)
            {
                this.selected += 1;
                Game1.playSound("drumkit6");
            }
            if (this.selected < 0)
                this.selected = 0;
            else if (this.selected >= this.options.Count)
                this.selected = this.options.Count - 1;
            int selected2 = this.selected;
            if (selected1 == selected2)
                return;
        }

        /// <summary>
        /// Gets the current selected value
        /// </summary>
        /// <returns>Current value held be the component</returns>
        public int GetCurrentOption()
        {
            return Convert.ToInt32(this.options[this.selected]);
        }


    }
}
