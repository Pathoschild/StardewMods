using System;
using System.Linq;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>An ingredient requirement for a recipe.</summary>
    internal class Requirement
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The matching items available to consume.</summary>
        public ChestItem[] Consumables { get; }

        /// <summary>The number of items needed for the recipe.</summary>
        public int CountNeeded { get; }

        /// <summary>Whether the consumables needed for this requirement are ready.</summary>
        public bool IsMet { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="consumables">The matching items available to consume.</param>
        /// <param name="countNeeded">The number of items needed for the recipe.</param>
        public Requirement(ChestItem[] consumables, int countNeeded)
        {
            this.Consumables = consumables;
            this.CountNeeded = countNeeded;
            this.IsMet = consumables.Sum(p => p.Item.Stack) >= countNeeded;
        }

        /// <summary>Consume the required items.</summary>
        public void Consume()
        {
            int countNeeded = this.CountNeeded;
            foreach (ChestItem chestItem in this.Consumables)
            {
                Chest chest = chestItem.Chest;
                Item item = chestItem.Item;

                // reduce stack
                int reduceBy = Math.Min(countNeeded, item.Stack);
                countNeeded -= reduceBy;
                item.Stack -= reduceBy;

                // remove stack if empty
                if (item.Stack <= 0)
                {
                    chest.items.Remove(item);
                    chest.clearNulls();
                }

                // exit if done
                if (countNeeded <= 0)
                    return;
            }
        }

        /// <summary>Get the first matching consumable item.</summary>
        public Item GetOne()
        {
            return this.Consumables.FirstOrDefault()?.Item.getOne();
        }
    }
}
