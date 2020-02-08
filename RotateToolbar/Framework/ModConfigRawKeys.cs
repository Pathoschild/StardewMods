using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.RotateToolbar.Framework
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigRawKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which rotates the toolbar up (i.e. show the previous inventory row).</summary>
        public string ShiftToPrevious { get; set; } = "";

        /// <summary>The key which rotates the toolbar up (i.e. show the next inventory row).</summary>
        public string ShiftToNext { get; set; } = SButton.Tab.ToString();


        /*********
        ** Public fields
        *********/
        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public ModConfigKeys ParseControls(IMonitor monitor)
        {
            return new ModConfigKeys(
                shiftToPrevious: CommonHelper.ParseButtons(this.ShiftToPrevious, monitor, nameof(this.ShiftToPrevious)),
                shiftToNext: CommonHelper.ParseButtons(this.ShiftToNext, monitor, nameof(this.ShiftToNext))
            );
        }
    }
}
