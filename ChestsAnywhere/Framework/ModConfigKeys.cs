using StardewModdingAPI;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The key which toggles the chest UI.</summary>
        public SButton[] Toggle { get; }

        /// <summary>The key which navigates to the previous chest.</summary>
        public SButton[] PrevChest { get; }

        /// <summary>The key which navigates to the next chest.</summary>
        public SButton[] NextChest { get; }

        /// <summary>The key which navigates to the previous category.</summary>
        public SButton[] PrevCategory { get; }

        /// <summary>The key which navigates to the next category.</summary>
        public SButton[] NextCategory { get; }

        /// <summary>The key which edits the current chest.</summary>
        public SButton[] EditChest { get; }

        /// <summary>The key which sorts items in the chest.</summary>
        public SButton[] SortItems { get; }

        /// <summary>The key which, when held, enables scrolling the chest dropdown with the mouse scroll wheel.</summary>
        public SButton[] HoldToMouseWheelScrollChests { get; }

        /// <summary>The key which, when held, enables scrolling the category dropdown with the mouse scroll wheel.</summary>
        public SButton[] HoldToMouseWheelScrollCategories { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggle">The key which toggles the chest UI.</param>
        /// <param name="prevChest">The key which navigates to the previous chest.</param>
        /// <param name="nextChest">The key which navigates to the next chest.</param>
        /// <param name="prevCategory">The key which navigates to the previous category.</param>
        /// <param name="nextCategory">The key which navigates to the next category.</param>
        /// <param name="editChest">The key which edits the current chest.</param>
        /// <param name="sortItems">The key which sorts items in the chest.</param>
        /// <param name="holdToMouseWheelScrollChests">The key which, when held, enables scrolling the chest dropdown with the mouse scroll wheel.</param>
        /// <param name="holdtoMouseWheelScrollCategories">The key which, when held, enables scrolling the category dropdown with the mouse scroll wheel.</param>
        public ModConfigKeys(SButton[] toggle, SButton[] prevChest, SButton[] nextChest, SButton[] prevCategory, SButton[] nextCategory, SButton[] editChest, SButton[] sortItems, SButton[] holdToMouseWheelScrollChests, SButton[] holdtoMouseWheelScrollCategories)
        {
            this.Toggle = toggle;
            this.PrevChest = prevChest;
            this.NextChest = nextChest;
            this.PrevCategory = prevCategory;
            this.NextCategory = nextCategory;
            this.EditChest = editChest;
            this.SortItems = sortItems;
            this.HoldToMouseWheelScrollChests = holdToMouseWheelScrollChests;
            this.HoldToMouseWheelScrollCategories = holdtoMouseWheelScrollCategories;
        }
    }
}
