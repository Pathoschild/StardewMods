using System.Text.RegularExpressions;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.TractorMod.Framework.Menu
{
    public class NamedOptionsCheckbox : OptionsCheckbox
    {
        public string Name { get; set; }
        /// <summary>
        /// Create a NamedOptionsCheckbox with a name field
        /// </summary>
        /// <param name="label">The label that will be displayed next to the checkbox</param>
        /// <param name="whichOption">Which option (Render type)</param>
        /// <param name="name">To store the name of the component</param>
        public NamedOptionsCheckbox(string label, int whichOption,string name)
            :base (label,whichOption)
        {
            this.Name = name;
            Regex r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])");
            base.label = r.Replace(label, " ");
        }

        /// <summary>
        /// Handle a left click on the checkbox
        /// </summary>
        /// <param name="x">x pos of mouse</param>
        /// <param name="y">y pos of mouse</param>
        public override void receiveLeftClick(int x, int y)
        {
            if (this.greyedOut)
                return;
            Game1.playSound("drumkit6");
            this.isChecked = !this.isChecked;
        }
    }
}
