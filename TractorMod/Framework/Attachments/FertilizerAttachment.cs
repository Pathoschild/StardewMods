using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewValley;
using StardewValley.TerrainFeatures;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for fertiliser or speed-gro.</summary>
    internal class FertilizerAttachment : BaseAttachment
    {
        /*********
        ** Properties
        *********/
        /// <summary>The attachment settings.</summary>
        private readonly FertilizerConfig Config;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The attachment settings.</param>
        public FertilizerAttachment(FertilizerConfig config)
        {
            this.Config = config;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(SFarmer player, Tool tool, Item item, GameLocation location)
        {
            return this.Config.Enable && item?.Category == SObject.fertilizerCategory;
        }

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, SFarmer player, Tool tool, Item item, GameLocation location)
        {
            if (item == null || item.Stack <= 0)
                return false;

            // get dirt
            if (!(tileFeature is HoeDirt dirt) || dirt.fertilizer.Value != HoeDirt.noFertilizer)
                return false;

            // ignore if there's a giant crop, meteorite, etc covering the tile
            if (this.GetResourceClumpCoveringTile(location, tile) != null)
                return false;

            // apply fertiliser
            dirt.fertilizer.Value = item.ParentSheetIndex;
            this.ConsumeItem(player, item);
            return true;
        }
    }
}
