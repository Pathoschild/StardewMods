using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models
{
    /// <summary>An item that can be produced by a fish pond, with extra info used for drawing.</summary>
    internal class FishPondDrop : FishPondDropData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>An instance of the produced item.</summary>
        public Object SampleItem { get; }

        /// <summary>The sprite icon to draw.</summary>
        public SpriteInfo Sprite { get; }

        /// <summary>Whether the item has been unlocked for the current fish pond.</summary>
        public bool IsUnlocked { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="data">The underlying drop data.</param>
        /// <param name="sampleItem">An instance of the produced item.</param>
        /// <param name="sprite">The sprite icon to draw.</param>
        /// <param name="isUnlocked">Whether the item has been unlocked for the current fish pond.</param>
        public FishPondDrop(FishPondDropData data, Object sampleItem, SpriteInfo sprite, bool isUnlocked)
            : base(data.MinPopulation, data.ItemID, data.MinDrop, data.MaxDrop, data.Probability)
        {
            this.SampleItem = sampleItem;
            this.Sprite = sprite;
            this.IsUnlocked = isUnlocked;
        }
    }
}
