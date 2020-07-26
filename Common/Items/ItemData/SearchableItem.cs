using System;
using StardewValley;

namespace Pathoschild.Stardew.Common.Items.ItemData
{
    /// <summary>A game item with metadata.</summary>
    /// <remarks>This is copied from the SMAPI source code and should be kept in sync with it.</remarks>
    internal class SearchableItem
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The item type.</summary>
        public ItemType Type { get; }

        /// <summary>The item instance.</summary>
        public Item Item { get; }

        /// <summary>The item's unique ID for its type.</summary>
        public int ID { get; }

        /// <summary>The item's default name.</summary>
        public string Name => this.Item.Name;

        /// <summary>The item's display name for the current language.</summary>
        public string DisplayName => this.Item.DisplayName;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The item type.</param>
        /// <param name="id">The unique ID (if different from the item's parent sheet index).</param>
        /// <param name="item">The item instance.</param>
        public SearchableItem(ItemType type, int id, Item item)
        {
            this.Type = type;
            this.ID = id;
            this.Item = item;
        }

        /// <summary>Get whether the item name contains a case-insensitive substring.</summary>
        /// <param name="substring">The substring to find.</param>
        public bool NameContains(string substring)
        {
            return
                this.Name.IndexOf(substring, StringComparison.OrdinalIgnoreCase) != -1
                || this.DisplayName.IndexOf(substring, StringComparison.OrdinalIgnoreCase) != -1;
        }

        /// <summary>Get whether the item name is exactly equal to a case-insensitive string.</summary>
        /// <param name="name">The substring to find.</param>
        public bool NameEquivalentTo(string name)
        {
            return
                this.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                || this.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
