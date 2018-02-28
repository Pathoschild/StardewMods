using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the watering can.</summary>
    internal class WateringCanAttachment : BaseAttachment
    {
        /*********
        ** Properties
        *********/
        /// <summary>The config settings for the watering can attachment.</summary>
        private readonly Config.WateringCanConfig config;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The mod configuration.</param>
        public WateringCanAttachment(Config.WateringCanConfig config)
        {
            this.config = config;
        }

        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public override bool IsEnabled(SFarmer player, Tool tool, Item item, GameLocation location)
        {
            return tool is WateringCan;
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
            if (!this.config.Enable)
                return false;

            if (!(tileFeature is HoeDirt dirt) || dirt.state == HoeDirt.watered)
                return false;

            WateringCan can = (WateringCan)tool;
            int prevWater = can.WaterLeft;
            can.WaterLeft = 100;
            this.UseToolOnTile(tool, tile);
            can.WaterLeft = prevWater;

            return true;
        }
    }
}
