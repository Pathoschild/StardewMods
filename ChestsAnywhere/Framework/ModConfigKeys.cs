using Pathoschild.Stardew.Common.Input;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>A set of parsed key bindings.</summary>
    internal class ModConfigKeys
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The keys which toggle the chest UI.</summary>
        public KeyBinding Toggle { get; }

        /// <summary>The keys which navigate to the previous chest.</summary>
        public KeyBinding PrevChest { get; }

        /// <summary>The keys which navigate to the next chest.</summary>
        public KeyBinding NextChest { get; }

        /// <summary>The keys which navigate to the previous category.</summary>
        public KeyBinding PrevCategory { get; }

        /// <summary>The keys which navigate to the next category.</summary>
        public KeyBinding NextCategory { get; }

        /// <summary>The keys which edit the current chest.</summary>
        public KeyBinding EditChest { get; }

        /// <summary>The keys which sort items in the chest.</summary>
        public KeyBinding SortItems { get; }

        /// <summary>The keys which, when held, enable scrolling the chest dropdown with the mouse scroll wheel.</summary>
        public KeyBinding HoldToMouseWheelScrollChests { get; }

        /// <summary>The keys which, when held, enable scrolling the category dropdown with the mouse scroll wheel.</summary>
        public KeyBinding HoldToMouseWheelScrollCategories { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="toggle">The keys which toggle the chest UI.</param>
        /// <param name="prevChest">The keys which navigate to the previous chest.</param>
        /// <param name="nextChest">The keys which navigate to the next chest.</param>
        /// <param name="prevCategory">The keys which navigate to the previous category.</param>
        /// <param name="nextCategory">The keys which navigate to the next category.</param>
        /// <param name="editChest">The keys which edit the current chest.</param>
        /// <param name="sortItems">The keys which sort items in the chest.</param>
        /// <param name="holdToMouseWheelScrollChests">The keys which, when held, enable scrolling the chest dropdown with the mouse scroll wheel.</param>
        /// <param name="holdToMouseWheelScrollCategories">The keys which, when held, enable scrolling the category dropdown with the mouse scroll wheel.</param>
        public ModConfigKeys(KeyBinding toggle, KeyBinding prevChest, KeyBinding nextChest, KeyBinding prevCategory, KeyBinding nextCategory, KeyBinding editChest, KeyBinding sortItems, KeyBinding holdToMouseWheelScrollChests, KeyBinding holdToMouseWheelScrollCategories)
        {
            this.Toggle = toggle;
            this.PrevChest = prevChest;
            this.NextChest = nextChest;
            this.PrevCategory = prevCategory;
            this.NextCategory = nextCategory;
            this.EditChest = editChest;
            this.SortItems = sortItems;
            this.HoldToMouseWheelScrollChests = holdToMouseWheelScrollChests;
            this.HoldToMouseWheelScrollCategories = holdToMouseWheelScrollCategories;
        }
    }
}
