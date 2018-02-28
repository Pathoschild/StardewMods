using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.ModAttachments
{
    /// <summary>An attachment for the Seed Bag mod.</summary>
    internal class SeedBagAttachment : BaseAttachment
    {
        /*********
        ** Properties
        *********/
        /// <summary>Whether to cut down non-fruit trees.</summary>
        private readonly bool Enable;

        /*********
        ** Accessors
        *********/
        /// <summary>The <see cref="System.Type.FullName"/> value for the Seed Bag mod's seed bag.</summary>
        internal const string SeedBagTypeName = "SeedBag.SeedBagTool";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        public SeedBagAttachment(ModConfig config)
        {
            this.Enable = config.StandardAttachments.SeedBag.Enable;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(SFarmer player, Tool tool, Item item, GameLocation location)
        {
            return tool?.GetType().FullName == SeedBagAttachment.SeedBagTypeName;
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
            if (this.Enable)
                return false;

            // till plain dirt
            if (tileFeature is HoeDirt)
                return this.UseToolOnTile(tool, tile);

            return false;
        }
    }
}
