using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>A parsed gift taste model.</summary>
    internal class GiftTasteModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The target villager.</summary>
        public NPC Villager { get; set; }

        /// <summary>A sample of the item.</summary>
        public Item Item { get; set; }

        /// <summary>How much the target villager likes this item.</summary>
        public GiftTaste Taste { get; }

        /// <summary>Whether the player has discovered this gift taste.</summary>
        public bool IsRevealed => Game1.player.hasGiftTasteBeenRevealed(this.Villager, this.Item.ParentSheetIndex);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="villager">The target villager.</param>
        /// <param name="item">A sample of the item.</param>
        /// <param name="taste">How much the target villager likes this item.</param>
        public GiftTasteModel(NPC villager, Item item, GiftTaste taste)
        {
            this.Villager = villager;
            this.Item = item;
            this.Taste = taste;
        }
    }
}
