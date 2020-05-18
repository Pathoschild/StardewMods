using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for fertilizer or speed-gro.</summary>
    internal class FertilizerAttachment : BaseAttachment
    {
        /*********
        ** Fields
        *********/
        /// <summary>The attachment settings.</summary>
        private readonly GenericAttachmentConfig Config;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The attachment settings.</param>
        /// <param name="modRegistry">Fetches metadata about loaded mods.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public FertilizerAttachment(GenericAttachmentConfig config, IModRegistry modRegistry, IReflectionHelper reflection)
            : base(modRegistry, reflection)
        {
            this.Config = config;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(Farmer player, Tool tool, Item item, GameLocation location)
        {
            return this.Config.Enable && item?.Category == SObject.fertilizerCategory && item.Stack > 0;
        }

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, Farmer player, Tool tool, Item item, GameLocation location)
        {
            if (item == null || item.Stack <= 0)
                return false;

            switch (item.ParentSheetIndex)
            {
                // tree fertilizer
                case 805:
                    if (tileFeature is Tree tree && !tree.fertilized.Value && tree.growthStage.Value < Tree.treeStage && tree.fertilize(location))
                    {
                        this.ConsumeItem(player, item);
                        return true;
                    }
                    return false;

                // crop fertilizer
                default:
                    // get unfertilised dirt
                    if (!this.TryGetHoeDirt(tileFeature, tileObj, out HoeDirt dirt, out bool dirtCoveredByObj, out _) || dirt.fertilizer.Value != HoeDirt.noFertilizer)
                        return false;

                    // ignore if there's a giant crop, meteorite, etc covering the tile
                    if (dirtCoveredByObj || this.HasResourceClumpCoveringTile(location, tile))
                        return false;

                    // apply fertilizer
                    bool fertilized = dirt.plant(item.ParentSheetIndex, (int)tile.X, (int)tile.Y, player, isFertilizer: true, location);
                    if (fertilized)
                        this.ConsumeItem(player, item);
                    return fertilized;
            }
        }
    }
}
