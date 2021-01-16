using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Farmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework.Attachments
{
    /// <summary>An attachment for the watering can.</summary>
    internal class WateringCanAttachment : BaseAttachment
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
        public WateringCanAttachment(GenericAttachmentConfig config, IModRegistry modRegistry, IReflectionHelper reflection)
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
            return this.Config.Enable && tool is WateringCan;
        }

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        /// <remarks>Volcano logic derived from <see cref="VolcanoDungeon.performToolAction"/>.</remarks>
        public override bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, Farmer player, Tool tool, Item item, GameLocation location)
        {
            // water dirt
            if (this.TryGetHoeDirt(tileFeature, tileObj, out HoeDirt dirt, out _, out _) && dirt.state.Value != HoeDirt.watered)
                return this.UseWateringCanOnTile(tool, tile, player, location);

            // cool lava
            int x = (int)tile.X;
            int y = (int)tile.Y;
            if (location is VolcanoDungeon dungeon && dungeon.isTileOnMap(x, y) && dungeon.waterTiles[x, y] && !dungeon.cooledLavaTiles.ContainsKey(tile))
                return this.UseWateringCanOnTile(tool, tile, player, location);

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Use a tool on a tile.</summary>
        /// <param name="tool">The tool to use.</param>
        /// <param name="tile">The tile to affect.</param>
        /// <param name="player">The current player.</param>
        /// <param name="location">The current location.</param>
        /// <returns>Returns <c>true</c> for convenience when implementing tools.</returns>
        private bool UseWateringCanOnTile(Tool tool, Vector2 tile, Farmer player, GameLocation location)
        {
            WateringCan can = (WateringCan)tool;
            int prevWater = can.WaterLeft;
            can.WaterLeft = 100;
            try
            {
                return this.UseToolOnTile(tool, tile, player, location);
            }
            finally
            {
                can.WaterLeft = prevWater;
            }
        }
    }
}
