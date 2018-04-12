using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewValley;
using StardewValley.TerrainFeatures;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the grass starter item.</summary>
    internal class GrassStarterAttachment : BaseAttachment
    {
        /*********
        ** Properties
        *********/
        /// <summary>The attachment settings.</summary>
        private readonly GrassStarterConfig Config;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="config">The attachment settings.</param>
        public GrassStarterAttachment(GrassStarterConfig config)
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
            return this.Config.Enable && item?.ParentSheetIndex == 297;
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
            if (!(item is SObject obj) || obj.Stack <= 0)
                return false;

            if (obj.canBePlacedHere(location, tile) && obj.placementAction(location, (int)(tile.X * Game1.tileSize), (int)(tile.Y * Game1.tileSize), player))
            {
                this.ConsumeItem(player, item);
                return true;
            }

            return false;
        }
    }
}
