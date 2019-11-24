using Pathoschild.Stardew.LookupAnything.Framework.Constants;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>A raw gift taste entry parsed from the game data.</summary>
    internal class GiftTasteEntry
    {
        /*********
        ** Accessors
        *********/
        /// <summary>How much the target villager likes this item.</summary>
        public GiftTaste Taste { get; }

        /// <summary>The name of the target villager.</summary>
        public string VillagerName { get; }

        /// <summary>The item parent sprite index (if positive) or category (if negative).</summary>
        public int RefID { get; set; }

        /// <summary>Whether this gift taste applies to all villagers unless otherwise excepted.</summary>
        public bool IsUniversal { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="taste">How much the target villager likes this item.</param>
        /// <param name="villagerName">The name of the target villager.</param>
        /// <param name="refID">The item parent sprite index (if positive) or category (if negative).</param>
        /// <param name="isUniversal">Whether this gift taste applies to all villagers unless otherwise excepted.</param>
        public GiftTasteEntry(GiftTaste taste, string villagerName, int refID, bool isUniversal = false)
        {
            this.Taste = taste;
            this.VillagerName = villagerName;
            this.RefID = refID;
            this.IsUniversal = isUniversal;
        }
    }
}
