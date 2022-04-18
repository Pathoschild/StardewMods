#nullable disable

using System;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.ItemScanning
{
    /// <summary>An item found in the world.</summary>
    public class FoundItem
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The parent entity which contains the item (e.g. location, chest, furniture, etc).</summary>
        public object Parent { get; }

        /// <summary>The item instance.</summary>
        public Item Item { get; }

        /// <summary>Whether the item was found in the current player's inventory.</summary>
        public bool IsInInventory { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="item">The item instance.</param>
        /// <param name="parent">The parent entity which contains the item (e.g. location, chest, furniture, etc).</param>
        /// <param name="isInInventory">Whether the item was found in the current player's inventory.</param>
        public FoundItem(Item item, object parent, bool isInInventory)
        {
            this.Item = item;
            this.Parent = parent;
            this.IsInInventory = isInInventory;
        }

        /// <summary>Get the actual number of items in the stack.</summary>
        public int GetCount()
        {
            int count = Math.Max(1, this.Item.Stack);

            // special case: torch placed on a fence has a stack of 93
            if (this.Parent is Fence && this.Item is Torch && count == 93)
                count = 1;

            return count;
        }
    }
}
