using System;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using Pathoschild.Stardew.TractorMod.Framework.ModAttachments;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the hoe.</summary>
    internal class HoeAttachment : BaseAttachment
    {
        /*********
        ** Fields
        *********/
        /// <summary>The attachment settings.</summary>
        private readonly HoeConfig Config;

        /// <summary>The item ID for an artifact spot.</summary>
        private const int ArtifactSpotItemID = 590;

        /// <summary>The minimum delay before attempting to re-till the same empty dirt tile.</summary>
        private readonly TimeSpan TillDirtDelay = TimeSpan.FromSeconds(1);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="modRegistry">Fetches metadata about loaded mods.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public HoeAttachment(HoeConfig config, IModRegistry modRegistry, IReflectionHelper reflection)
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
        public override bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, Farmer player, Tool tool, Item item, GameLocation location)
        {
            // clear weeds
            if (this.Config.ClearWeeds && this.IsWeed(tileObj))
                return this.UseToolOnTile(tool, tile, player, location);

            // collect artifact spots
            if (this.Config.DigArtifactSpots && tileObj?.ParentSheetIndex == HoeAttachment.ArtifactSpotItemID)
                return this.UseToolOnTile(tool, tile, player, location);

            // till plain dirt
            if (this.Config.TillDirt && tileFeature == null && tileObj == null && this.TryStartCooldown(tile.ToString(), this.TillDirtDelay))
                return this.UseToolOnTile(tool, tile, player, location);

            return false;
        }
    }
}
