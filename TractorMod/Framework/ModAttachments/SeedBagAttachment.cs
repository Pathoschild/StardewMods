using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.ModAttachments
{
    /// <summary>An attachment for the Seed Bag mod.</summary>
    internal class SeedBagAttachment : ExtendedDistanceAttachment<ExtendedDistanceConfig>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique ID for the Seed Bag mod.</summary>
        internal const string ModId = "Platonymous.SeedBag";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The attachment settings.</param>
        /// <param name="modRegistry">Fetches metadata about loaded mods.</param>
        public SeedBagAttachment(ExtendedDistanceConfig config, IModRegistry modRegistry)
            : base(config, modRegistry)
        {
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(Farmer player, Tool? tool, Item? item, GameLocation location)
        {
            return
                this.Config.Enable
                && tool is { Name: "Seed Bag" };
        }

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool Apply(Vector2 tile, SObject? tileObj, TerrainFeature? tileFeature, Farmer player, Tool? tool, Item? item, GameLocation location)
        {
            tool = tool.AssertNotNull();

            // apply to plain dirt
            if (tileFeature is HoeDirt { crop: null })
                return this.UseToolOnTile(tool, tile, player, location);

            return false;
        }
    }
}
