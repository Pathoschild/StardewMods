using Pathoschild.LookupAnything.Framework.Constants;

namespace Pathoschild.LookupAnything.Framework.Models
{
    /// <summary>A raw gift taste entry parsed from the game's data files.</summary>
    internal class RawGiftTasteModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>How much the target villager likes this item.</summary>
        public GiftTaste Taste { get; }

        /// <summary>The name of the target villager.</summary>
        public string Villager { get; }

        /// <summary>The item parent sprite index (if positive) or category (if negative).</summary>
        public int RefID { get; set; }

        /// <summary>Whether this gift taste applies to all villagers unless otherwise excepted.</summary>
        public bool IsUniversal { get; }

        /// <summary>Whether the <see cref="RefID"/> refers to a category of items, instead of a specific item ID.</summary>
        public bool IsCategory => this.RefID < 0;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="taste">How much the target villager likes this item.</param>
        /// <param name="villager">The name of the target villager.</param>
        /// <param name="refID">The item parent sprite index (if positive) or category (if negative).</param>
        /// <param name="isUniversal">Whether this gift taste applies to all villagers unless otherwise excepted.</param>
        public RawGiftTasteModel(GiftTaste taste, string villager, int refID, bool isUniversal = false)
        {
            this.Taste = taste;
            this.Villager = villager;
            this.RefID = refID;
            this.IsUniversal = isUniversal;
        }
    }
}