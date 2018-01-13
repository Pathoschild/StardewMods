using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataMaps.DataMaps
{
    /// <summary>A data map which shows whether tiles are traversable by the player.</summary>
    internal class AccessibilityMap : IDataMap
    {
        /*********
        ** Properties
        *********/
        /// <summary>The color for a passable tile.</summary>
        private readonly Color ClearColor = Color.Green;

        /// <summary>The color for a passable but occupied tile.</summary>
        private readonly Color OccupiedColor = Color.Orange;

        /// <summary>The color for an impassable tile.</summary>
        private readonly Color ImpassableColor = Color.Red;

        /// <summary>The color for a warp tile.</summary>
        private readonly Color WarpColor = Color.Blue;

        /// <summary>The action tile property values which trigger a warp.</summary>
        /// <remarks>See remarks on <see cref="IsWarp"/>.</remarks>
        private readonly HashSet<string> WarpActions = new HashSet<string> { "EnterSewer", "LockedDoorWarp", "Warp", "WarpCommunityCenter", "WarpGreenhouse", "WarpMensLocker", "WarpWomensLocker", "WizardHatch" };

        /// <summary>The touch action tile property values which trigger a warp.</summary>
        private readonly HashSet<string> TouchWarpActions = new HashSet<string> { "Door", "MagicWarp" };

        /// <summary>The legend entries to display.</summary>
        public LegendEntry[] Legend { get; }


        /*********
        ** Accessors
        *********/
        /// <summary>The map's display name.</summary>
        public string Name { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        public AccessibilityMap(ITranslationHelper translations)
        {
            this.Name = translations.Get("maps.accessibility.name");
            this.Legend = new[]
            {
                new LegendEntry(translations.Get("maps.accessibility.clear"), this.ClearColor),
                new LegendEntry(translations.Get("maps.accessibility.occupied"), this.OccupiedColor),
                new LegendEntry(translations.Get("maps.accessibility.impassable"), this.ImpassableColor),
                new LegendEntry(translations.Get("maps.accessibility.warp"), this.WarpColor)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea, Vector2 cursorTile)
        {
            TileData[] tiles = this.GetTiles(location, visibleArea.GetTiles()).ToArray();

            // passable tiles
            TileData[] passableTiles = tiles.Where(p => p.Color == this.ClearColor).ToArray();
            yield return new TileGroup(passableTiles, outerBorderColor: this.ClearColor);

            // other tiles
            yield return new TileGroup(tiles.Except(passableTiles).ToArray());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        private IEnumerable<TileData> GetTiles(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            foreach (Vector2 tile in visibleTiles)
            {
                // get pixel coordinates
                Rectangle tilePixels = new Rectangle((int)(tile.X * Game1.tileSize), (int)(tile.Y * Game1.tileSize), Game1.tileSize, Game1.tileSize);

                // get color code
                Color color;
                if (this.IsWarp(location, tilePixels))
                    color = this.WarpColor;
                else if (this.IsPassable(location, tile, tilePixels))
                    color = this.IsOccupied(location, tile, tilePixels) ? this.OccupiedColor : this.ClearColor;
                else
                    color = this.ImpassableColor;

                // yield
                yield return new TileData(tile, color);
            }
        }

        /// <summary>Get whether there's a warp on the given tile.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tilePixels">The tile area in pixels.</param>
        /// <remarks>Derived from <see cref="GameLocation.isCollidingWithWarp"/>, <see cref="GameLocation.performAction"/>, and <see cref="GameLocation.performTouchAction"/>.</remarks>
        private bool IsWarp(GameLocation location, Rectangle tilePixels)
        {
            // check map warps
            if (location.isCollidingWithWarpOrDoor(tilePixels) != null)
                return true;

            // check tile actions
            Tile buildingTile = location.map.GetLayer("Buildings").PickTile(new Location(tilePixels.X, tilePixels.Y), Game1.viewport.Size);
            if (buildingTile != null && buildingTile.Properties.TryGetValue("Action", out PropertyValue action) && this.WarpActions.Contains(action.ToString().Split(' ')[0]))
                return true;

            // check tile touch actions
            Tile backTile = location.map.GetLayer("Back").PickTile(new Location(tilePixels.X, tilePixels.Y), Game1.viewport.Size);
            if (backTile != null && backTile.Properties.TryGetValue("TouchAction", out PropertyValue touchAction) && this.TouchWarpActions.Contains(touchAction.ToString().Split(' ')[0]))
                return true;

            return false;
        }

        /// <summary>Get whether a tile is passable.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="tilePixels">The tile area in pixels.</param>
        /// <remarks>Derived from <see cref="Farmer.MovePosition"/>, <see cref="GameLocation.isCollidingPosition(Rectangle,xTile.Dimensions.Rectangle,bool)"/>, <see cref="GameLocation.isTilePassable(Location,xTile.Dimensions.Rectangle)"/>, and <see cref="Fence"/>.</remarks>
        private bool IsPassable(GameLocation location, Vector2 tile, Rectangle tilePixels)
        {
            // check layer properties
            if (location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
                return true;

            // allow bridges
            if (location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Passable", "Buildings") != null)
            {
                Tile backTile = location.map.GetLayer("Back").PickTile(new Location(tilePixels.X, tilePixels.Y), Game1.viewport.Size);
                if (backTile == null || !backTile.TileIndexProperties.TryGetValue("Passable", out PropertyValue value) || value != "F")
                    return true;
            }

            return false;
        }

        /// <summary>Get whether a tile is blocked due to something it contains.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="tilePixels">The tile area in pixels.</param>
        /// <remarks>Derived from <see cref="GameLocation.isCollidingPosition(Rectangle,xTile.Dimensions.Rectangle,bool)"/> and <see cref="Farm.isCollidingPosition(Rectangle,xTile.Dimensions.Rectangle,bool,int,bool,Character,bool,bool,bool)"/>.</remarks>
        private bool IsOccupied(GameLocation location, Vector2 tile, Rectangle tilePixels)
        {
            // show open gate as passable
            if (location.objects.TryGetValue(tile, out Object obj) && obj is Fence fence && fence.isGate && fence.gatePosition == Fence.gateOpenedPosition)
                return false;

            // check for objects, characters, or terrain features
            if (location.isTileOccupiedIgnoreFloors(tile))
                return true;

            // buildings
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    Rectangle buildingArea = new Rectangle(building.tileX, building.tileY, building.tilesWide, building.tilesHigh);
                    if (buildingArea.Contains((int)tile.X, (int)tile.Y))
                        return true;
                }
            }

            // large terrain features
            if (location.largeTerrainFeatures != null && location.largeTerrainFeatures.Any(p => p.getBoundingBox().Intersects(tilePixels)))
                return true;

            // resource clumps
            if (location is Farm farm)
            {
                if (farm.resourceClumps.Any(p => p.getBoundingBox(p.tile).Intersects(tilePixels)))
                    return true;
            }

            return false;
        }
    }
}
