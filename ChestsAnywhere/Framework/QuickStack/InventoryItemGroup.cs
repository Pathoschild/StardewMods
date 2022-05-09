using System;
using System.Collections.Generic;
using StardewValley;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.QuickStack
{
    /// <summary>
    /// Class to hold information about a group of items defined by a representative item
    /// </summary>
    internal abstract class InventoryItemGroup : InventoryGroup
    {
        protected readonly Item GroupRepresentative;

        protected InventoryItemGroup(IList<Item> refInventory, Item groupRepresentative) : base(refInventory)
        {
            if(groupRepresentative == null)
                throw new ArgumentNullException(nameof(groupRepresentative));

            this.GroupRepresentative = groupRepresentative.getOne();
        }

        /// <summary>
        /// Returns a group representative of this item group
        /// </summary>
        /// <returns></returns>
        public Item GetGroupRepresentativeItem()
        {
            return this.GroupRepresentative;
        }
    }
}
