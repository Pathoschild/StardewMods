using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.TractorMod.Framework.Attachments;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The base class for tool implementations.</summary>
    internal abstract class BaseAttachment : IAttachment
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to private code.</summary>
        protected IReflectionHelper Reflection { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>The minimum number of ticks between each update.</summary>
        public int RateLimit { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the tool is currently enabled.</summary>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public abstract bool IsEnabled(Farmer player, Tool tool, Item item, GameLocation location);

        /// <summary>Apply the tool to the given tile.</summary>
        /// <param name="tile">The tile to modify.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="player">The current player.</param>
        /// <param name="tool">The tool selected by the player (if any).</param>
        /// <param name="item">The item selected by the player (if any).</param>
        /// <param name="location">The current location.</param>
        public abstract bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, Farmer player, Tool tool, Item item, GameLocation location);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="rateLimit">The minimum number of ticks between each update.</param>
        protected BaseAttachment(IReflectionHelper reflection, int rateLimit = 0)
        {
            this.Reflection = reflection;
            this.RateLimit = rateLimit;
        }

        /// <summary>Use a tool on a tile.</summary>
        /// <param name="tool">The tool to use.</param>
        /// <param name="tile">The tile to affect.</param>
        /// <returns>Returns <c>true</c> for convenience when implementing tools.</returns>
        protected bool UseToolOnTile(Tool tool, Vector2 tile)
        {
            // use tool on center of tile
            Vector2 useAt = (tile * Game1.tileSize) + new Vector2(Game1.tileSize / 2f);
            Game1.player.lastClick = useAt;
            tool.DoFunction(Game1.currentLocation, (int)useAt.X, (int)useAt.Y, 0, Game1.player);
            return true;
        }

        /// <summary>Trigger the player action on the given tile.</summary>
        /// <param name="location">The location for which to trigger an action.</param>
        /// <param name="tile">The tile for which to trigger an action.</param>
        /// <param name="player">The player for which to trigger an action.</param>
        protected bool CheckTileAction(GameLocation location, Vector2 tile, Farmer player)
        {
            return location.checkAction(new Location((int)tile.X, (int)tile.Y), Game1.viewport, player);
        }

        /// <summary>Get whether a given object is a weed.</summary>
        /// <param name="obj">The world object.</param>
        protected bool IsTwig(SObject obj)
        {
            return obj?.ParentSheetIndex == 294 || obj?.ParentSheetIndex == 295;
        }

        /// <summary>Get whether a given object is a weed.</summary>
        /// <param name="obj">The world object.</param>
        protected bool IsWeed(SObject obj)
        {
            return !(obj is Chest) && obj?.Name == "Weeds";
        }

        /// <summary>Remove the specified items from the player inventory.</summary>
        /// <param name="player">The player whose inventory to edit.</param>
        /// <param name="item">The item instance to deduct.</param>
        /// <param name="count">The number to deduct.</param>
        protected void ConsumeItem(Farmer player, Item item, int count = 1)
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
            switch (location)
            {
                case Farm farm:
                    return farm.resourceClumps;

                case Forest forest:
                    return forest.log != null
                        ? new[] { forest.log }
                        : new ResourceClump[0];

                case Woods woods:
                    return woods.stumps;

                case MineShaft mineshaft:
                    return mineshaft.resourceClumps;

                default:
                    if (location.Name == "DeepWoods" || location.Name.StartsWith("DeepWoods_"))
                        return this.Reflection.GetField<IList<ResourceClump>>(location, "resourceClumps", required: false)?.GetValue() ?? new ResourceClump[0];
                    return new ResourceClump[0];
            }
        }

        /// <summary>Get the resource clump which covers a given tile, if any.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        protected ResourceClump GetResourceClumpCoveringTile(GameLocation location, Vector2 tile)
        {
            Rectangle tileArea = this.GetAbsoluteTileArea(tile);
            foreach (ResourceClump clump in this.GetResourceClumps(location))
            {
                if (clump.getBoundingBox(clump.tile.Value).Intersects(tileArea))
                    return clump;
            }

            return null;
        }

        /// <summary>Get the tilled dirt for a tile, if any.</summary>
        /// <param name="tileFeature">The feature on the tile.</param>
        /// <param name="tileObj">The object on the tile.</param>
        /// <param name="dirt">The tilled dirt found, if any.</param>
        /// <param name="isCoveredByObj">Whether there's an object placed over the tilled dirt.</param>
        /// <returns>Returns whether tilled dirt was found.</returns>
        protected bool TryGetHoeDirt(TerrainFeature tileFeature, SObject tileObj, out HoeDirt dirt, out bool isCoveredByObj)
        {
            // garden pot
            if (tileObj is IndoorPot pot)
            {
                dirt = pot.hoeDirt.Value;
                isCoveredByObj = false;
                return true;
            }

            // regular dirt
            if ((dirt = tileFeature as HoeDirt) != null)
            {
                isCoveredByObj = tileObj != null;
                return true;
            }

            // none found
            dirt = null;
            isCoveredByObj = false;
            return false;
        }
    }
}
