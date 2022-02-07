using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models
{
    /// <summary>A parsed gift taste model.</summary>
    /// <param name="Villager">The target villager.</param>
    /// <param name="Item">A sample of the item.</param>
    /// <param name="Taste">How much the target villager likes this item.</param>
    internal record GiftTasteModel(NPC Villager, Item Item, GiftTaste Taste)
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the player has discovered this gift taste.</summary>
        public bool IsRevealed => Game1.player.hasGiftTasteBeenRevealed(this.Villager, this.Item.ItemId);
    }
}
