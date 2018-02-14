using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Attachments;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The base class for tool implementations.</summary>
    internal abstract class BaseAttachment : IAttachment
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public abstract bool IsEnabled(SFarmer player, Tool tool, Item item, GameLocation location);

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public abstract bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, SFarmer player, Tool tool, Item item, GameLocation location);


        /*********
        ** Protected methods
        *********/
        /// <summary>Use a tool on a tile.</summary>
        /// <param name="tool">The tool to use.</param>
        /// <param name="tile">The tile to affect.</param>
        /// <returns>Returns <c>true</c> for convenience when implementing tools.</returns>
        protected bool UseToolOnTile(Tool tool, Vector2 tile)
        {
            // use tool on center of tile
            Vector2 useAt = (tile * Game1.tileSize) + new Vector2(Game1.tileSize / 2f);
            tool.DoFunction(Game1.currentLocation, (int)useAt.X, (int)useAt.Y, 0, Game1.player);
            return true;
        }

        /// <summary>Trigger the player action on the given tile.</summary>
        /// <param name="location">The location for which to trigger an action.</param>
        /// <param name="tile">The tile for which to trigger an action.</param>
        /// <param name="player">The player for which to trigger an action.</param>
        protected bool CheckTileAction(GameLocation location, Vector2 tile, SFarmer player)
        {
            return location.checkAction(new Location((int)tile.X, (int)tile.Y), Game1.viewport, player);
        }

        /// <summary>Remove the specified items from the player inventory.</summary>
        /// <param name="player">The player whose inventory to edit.</param>
        /// <param name="item">The item instance to deduct.</param>
        /// <param name="count">The number to deduct.</param>
        protected void ConsumeItem(SFarmer player, Item item, int count = 1)
        {
            item.Stack -= 1;
            if (item.Stack <= 0)
                player.removeItemFromInventory(item);
        }

        /// <summary>Get a rectangle representing the tile area in absolute pixels from the map origin.</summary>
        /// <param name="tile">The tile position.</param>
        protected Rectangle GetAbsoluteTileArea(Vector2 tile)
        {
            Vector2 pos = tile * Game1.tileSize;
            return new Rectangle((int)pos.X, (int)pos.Y, Game1.tileSize, Game1.tileSize);
        }

        /// <summary>Get resource clumps in a given location.</summary>
        /// <param name="location">The location to search.</param>
        protected IEnumerable<ResourceClump> GetResourceClumps(GameLocation location)
        {
            if (location is Farm farm)
                return farm.resourceClumps;
            if (location is Woods woods)
                return woods.stumps;
            return new ResourceClump[0];
        }

        /// <summary>Get the resource clump which covers a given tile, if any.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        protected ResourceClump GetResourceClumpCoveringTile(GameLocation location, Vector2 tile)
        {
            Rectangle tileArea = this.GetAbsoluteTileArea(tile);
            foreach (ResourceClump clump in this.GetResourceClumps(location))
            {
                if (clump.getBoundingBox(clump.tile).Intersects(tileArea))
                    return clump;
            }

            return null;
        }
    }
}
