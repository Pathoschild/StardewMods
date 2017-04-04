using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>An item stack in a chest.</summary>
    internal class ChestItem
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The chest containing the item.</summary>
        public Chest Chest { get; }

        /// <summary>The item stack.</summary>
        public Item Item { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="chest">The chest containing the item.</param>
        /// <param name="item">The item stack.</param>
        public ChestItem(Chest chest, Item item)
        {
            this.Chest = chest;
            this.Item = item;
        }
    }
}
