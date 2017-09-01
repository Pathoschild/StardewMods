using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the pickaxe.</summary>
    internal class PickaxeAttachment : BaseAttachment
    {
        /*********
        ** Properties
        *********/
        /// <summary>Whether to destroy flooring and paths.</summary>
        private readonly bool BreakFlooring;

        /// <summary>Whether to break rocks.</summary>
        private readonly bool BreakRocks;

        /// <summary>Whether to clear tilled dirt.</summary>
        private readonly bool ClearDirt;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        public PickaxeAttachment(ModConfig config)
        {
            this.BreakFlooring = config.PickaxeBreaksFlooring;
            this.BreakRocks = config.PickaxeBreaksRocks;
            this.ClearDirt = config.PickaxeClearsDirt;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(SFarmer player, Tool tool, Item item, GameLocation location)
        {
            return tool is Pickaxe;
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
            // break stones
            if (tileObj?.Name == "Stone")
                return this.BreakRocks && this.UseToolOnTile(tool, tile);

            // break flooring & paths
            if (tileFeature is Flooring)
                return this.BreakFlooring && this.UseToolOnTile(tool, tile);

            // handle dirt
            if (tileFeature is HoeDirt dirt)
            {
                // clear tilled dirt
                if (dirt.crop == null)
                    return this.ClearDirt && this.UseToolOnTile(tool, tile);

                // clear dead crops
                if (dirt.crop?.dead == true)
                    return this.UseToolOnTile(tool, tile);
            }

            return false;
        }
    }
}
