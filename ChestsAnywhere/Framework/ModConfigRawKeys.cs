using Pathoschild.Stardew.Common;
using StardewModdingAPI;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>A set of raw key bindings.</summary>
    internal class ModConfigRawKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the chest UI.</summary>
        public string Toggle { get; set; } = SButton.B.ToString();

        /// <summary>The keys which navigate to the previous chest.</summary>
        public string PrevChest { get; set; } = $"{SButton.Left}, {SButton.LeftShoulder}";

        /// <summary>The keys which navigate to the next chest.</summary>
        public string NextChest { get; set; } = $"{SButton.Right}, {SButton.RightShoulder}";

        /// <summary>The keys which navigate to the previous category.</summary>
        public string PrevCategory { get; set; } = $"{SButton.Up}, {SButton.LeftTrigger}";

        /// <summary>The keys which navigate to the next category.</summary>
        public string NextCategory { get; set; } = $"{SButton.Down}, {SButton.RightTrigger}";

        /// <summary>The keys which edit the current chest.</summary>
        public string EditChest { get; set; } = "";

        /// <summary>The keys which sort items in the chest.</summary>
        public string SortItems { get; set; } = "";

        /// <summary>The keys which, when held, enable scrolling the chest dropdown with the mouse scroll wheel.</summary>
        public string HoldToMouseWheelScrollChests { get; set; } = SButton.LeftControl.ToString();

        /// <summary>The keys which, when held, enable scrolling the category dropdown with the mouse scroll wheel.</summary>
        public string HoldToMouseWheelScrollCategories { get; set; } = SButton.LeftAlt.ToString();


        /*********
        ** Public fields
        *********/
        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="input">The API for checking input state.</param>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public ModConfigKeys ParseControls(IInputHelper input, IMonitor monitor)
        {
            return new ModConfigKeys(
                toggle: CommonHelper.ParseButtons(this.Toggle, input, monitor, nameof(this.Toggle)),
                prevChest: CommonHelper.ParseButtons(this.PrevChest, input, monitor, nameof(this.PrevChest)),
                nextChest: CommonHelper.ParseButtons(this.NextChest, input, monitor, nameof(this.NextChest)),
                prevCategory: CommonHelper.ParseButtons(this.PrevCategory, input, monitor, nameof(this.PrevCategory)),
                nextCategory: CommonHelper.ParseButtons(this.NextCategory, input, monitor, nameof(this.NextCategory)),
                editChest: CommonHelper.ParseButtons(this.EditChest, input, monitor, nameof(this.EditChest)),
                sortItems: CommonHelper.ParseButtons(this.SortItems, input, monitor, nameof(this.SortItems)),
                holdToMouseWheelScrollChests: CommonHelper.ParseButtons(this.HoldToMouseWheelScrollChests, input, monitor, nameof(this.HoldToMouseWheelScrollChests)),
                holdToMouseWheelScrollCategories: CommonHelper.ParseButtons(this.HoldToMouseWheelScrollCategories, input, monitor, nameof(this.HoldToMouseWheelScrollCategories))
            );
        }
    }
}
