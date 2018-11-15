using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using Pathoschild.Stardew.TractorMod.Framework.ModAttachments;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the hoe.</summary>
    internal class HoeAttachment : BaseAttachment
    {
        /*********
        ** Properties
        *********/
        /// <summary>The attachment settings.</summary>
        private readonly HoeConfig Config;

        /// <summary>The item ID for an artifact spot.</summary>
        private const int ArtifactSpotItemID = 590;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        public HoeAttachment(HoeConfig config)
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
            return (this.Config.TillDirt || this.Config.ClearWeeds) && tool is Hoe && tool.GetType().FullName != SeedBagAttachment.SeedBagTypeName;
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
            // clear twigs & weeds
            if (this.Config.ClearWeeds && this.IsWeed(tileObj))
                return this.UseToolOnTile(tool, tile);

            // till plain dirt
            if (this.Config.TillDirt && tileFeature == null && tileObj == null)
                return this.UseToolOnTile(tool, tile);

            // collect artifact spots
            if (this.Config.DigArtifactSpots && tileObj?.ParentSheetIndex == HoeAttachment.ArtifactSpotItemID)
                return this.UseToolOnTile(tool, tile);

            return false;
        }
    }
}
