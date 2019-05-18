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
        /// <summary>The key which toggles the chest UI.</summary>
        public string Toggle { get; set; } = SButton.B.ToString();

        /// <summary>The key which navigates to the previous chest.</summary>
        public string PrevChest { get; set; } = $"{SButton.Left}, {SButton.LeftShoulder}";

        /// <summary>The key which navigates to the next chest.</summary>
        public string NextChest { get; set; } = $"{SButton.Right}, {SButton.RightShoulder}";

        /// <summary>The key which navigates to the previous category.</summary>
        public string PrevCategory { get; set; } = $"{SButton.Up}, {SButton.LeftTrigger}";

        /// <summary>The key which navigates to the next category.</summary>
        public string NextCategory { get; set; } = $"{SButton.Down}, {SButton.RightTrigger}";

        /// <summary>The key which edits the current chest.</summary>
        public string EditChest { get; set; } = "";

        /// <summary>The key which sorts items in the chest.</summary>
        public string SortItems { get; set; } = "";

        /// <summary>The key which, when held, enables scrolling the chest dropdown with the mouse scroll wheel.</summary>
        public string HoldToMouseWheelScrollChests { get; set; } = SButton.LeftControl.ToString();

        /// <summary>The key which, when held, enables scrolling the category dropdown with the mouse scroll wheel.</summary>
        public string HoldToMouseWheelScrollCategories { get; set; } = SButton.LeftAlt.ToString();


        /*********
        ** Public fields
        *********/
        /// <summary>Get a parsed representation of the configured controls.</summary>
        /// <param name="monitor">The monitor through which to log an error if a button value is invalid.</param>
        public ModConfigKeys ParseControls(IMonitor monitor)
        {
            return new ModConfigKeys(
                toggle: CommonHelper.ParseButtons(this.Toggle, monitor, nameof(this.Toggle)),
                prevChest: CommonHelper.ParseButtons(this.PrevChest, monitor, nameof(this.PrevChest)),
                nextChest: CommonHelper.ParseButtons(this.NextChest, monitor, nameof(this.NextChest)),
                prevCategory: CommonHelper.ParseButtons(this.PrevCategory, monitor, nameof(this.PrevCategory)),
                nextCategory: CommonHelper.ParseButtons(this.NextCategory, monitor, nameof(this.NextCategory)),
                editChest: CommonHelper.ParseButtons(this.EditChest, monitor, nameof(this.EditChest)),
                sortItems: CommonHelper.ParseButtons(this.SortItems, monitor, nameof(this.SortItems)),
                holdToMouseWheelScrollChests: CommonHelper.ParseButtons(this.HoldToMouseWheelScrollChests, monitor, nameof(this.HoldToMouseWheelScrollChests)),
                holdtoMouseWheelScrollCategories: CommonHelper.ParseButtons(this.HoldToMouseWheelScrollCategories, monitor, nameof(this.HoldToMouseWheelScrollCategories))
            );
        }
    }
}
