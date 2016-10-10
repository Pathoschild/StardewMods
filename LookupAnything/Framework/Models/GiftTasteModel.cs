using Pathoschild.LookupAnything.Framework.Constants;

namespace Pathoschild.LookupAnything.Framework.Models
{
    /// <summary>A gift taste entry parsed from the game's data files.</summary>
    internal class GiftTasteModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>How much the target villager likes this item.</summary>
        public GiftTaste Taste { get; }

        /// <summary>The name of the target villager.</summary>
        public string Villager { get; }

        /// <summary>The item parent sprite index (if positive) or category (if negative).</summary>
        public int ItemID { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="taste">How much the target villager likes this item.</param>
        /// <param name="villager">The name of the target villager.</param>
        /// <param name="itemID">The item parent sprite index.</param>
        public GiftTasteModel(GiftTaste taste, string villager, int itemID)
        {
            this.Taste = taste;
            this.Villager = villager;
            this.ItemID = itemID;
        }
    }
}