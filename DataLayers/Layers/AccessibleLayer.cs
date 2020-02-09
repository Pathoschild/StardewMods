using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataLayers.Layers
{
    /// <summary>A data layer which shows whether tiles are traversable by the player.</summary>
    internal class AccessibleLayer : BaseLayer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The legend entry for passable tiles.</summary>
        private readonly LegendEntry Clear;

        /// <summary>The legend entry for passable but occupied tiles.</summary>
        private readonly LegendEntry Occupied;

        /// <summary>The legend entry for impassable tiles.</summary>
        private readonly LegendEntry Impassable;

        /// <summary>The legend entry for warp tiles.</summary>
        private readonly LegendEntry Warp;

        /// <summary>The action tile property values which trigger a warp.</summary>
        /// <remarks>See remarks on <see cref="IsWarp"/>.</remarks>
        private readonly HashSet<string> WarpActions = new HashSet<string> { "EnterSewer", "LockedDoorWarp", "Mine", "Theater_Entrance", "Warp", "WarpCommunityCenter", "WarpGreenhouse", "WarpMensLocker", "WarpWomensLocker", "WizardHatch" };

        /// <summary>The touch action tile property values which trigger a warp.</summary>
        private readonly HashSet<string> TouchWarpActions = new HashSet<string> { "Door", "MagicWarp" };


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">Provides translations in stored in the mod folder's i18n folder.</param>
        /// <param name="config">The data layer settings.</param>
        /// <param name="input">The API for checking input state.</param>
        /// <param name="monitor">Writes messages to the SMAPI log.</param>
        public AccessibleLayer(ITranslationHelper translations, LayerConfig config, IInputHelper input, IMonitor monitor)
            : base(translations.Get("accessible.name"), config, input, monitor)
        {
            this.Legend = new[]
            {
                this.Clear = new LegendEntry(translations, "accessible.clear", Color.Green),
                this.Occupied = new LegendEntry(translations, "accessible.occupied", Color.Orange),
                this.Impassable = new LegendEntry(translations, "accessible.impassable", Color.Red),
                this.Warp = new LegendEntry(translations, "accessible.warp", Color.Blue)
            };
        }

        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tile area currently visible on the screen.</param>
        /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
        /// <param name="cursorTile">The tile position under the cursor.</param>
        public override TileGroup[] Update(GameLocation location, in Rectangle visibleArea, in Vector2[] visibleTiles, in Vector2 cursorTile)
        {
            TileData[] tiles = this.GetTiles(location, visibleTiles).ToArray();
            TileData[] passableTiles = tiles.Where(p => p.Type.Id == this.Clear.Id).ToArray();

            return new[]
            {
                new TileGroup(passableTiles, outerBorderColor: this.Clear.Color),
                new TileGroup(tiles.Except(passableTiles))
            };
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the updated data layer tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
        private IEnumerable<TileData> GetTiles(GameLocation location, IEnumerable<Vector2> visibleTiles)
        {
            // get building warps
            HashSet<Vector2> buildingDoors = new HashSet<Vector2>();
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    if (building.indoors.Value == null || (building.humanDoor.X < 0 && building.humanDoor.Y < 0))
                        continue;

                    buildingDoors.Add(new Vector2(building.humanDoor.X + building.tileX.Value, building.humanDoor.Y + building.tileY.Value));
                    buildingDoors.Add(new Vector2(building.humanDoor.X + building.tileX.Value, building.humanDoor.Y + building.tileY.Value - 1));
                }
            }

            // get tile data
            foreach (Vector2 tile in visibleTiles)
            {
                // get pixel coordinates
                Rectangle tilePixels = new Rectangle((int)(tile.X * Game1.tileSize), (int)(tile.Y * Game1.tileSize), Game1.tileSize, Game1.tileSize);

                // get color
                LegendEntry type;
                if (this.IsWarp(location, tile, tilePixels, buildingDoors))
                    type = this.Warp;
                else if (this.IsPassable(location, tile, tilePixels))
                    type = this.IsOccupied(location, tile, tilePixels) ? this.Occupied : this.Clear;
                else
                    type = this.Impassable;

                // yield
                yield return new TileData(tile, type);
            }
        }

        /// <summary>Get whether there's a warp on the given tile.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <param name="tilePixels">The tile area in pixels.</param>
        /// <param name="buildingDoors">The tile positions for farm building doors in the current location.</param>
        /// <remarks>Derived from <see cref="GameLocation.isCollidingWithWarp"/>, <see cref="GameLocation.performAction"/>, and <see cref="GameLocation.performTouchAction"/>.</remarks>
        private bool IsWarp(GameLocation location, Vector2 tile, Rectangle tilePixels, HashSet<Vector2> buildingDoors)
        {
            // check farm building doors
            if (buildingDoors.Contains(tile))
                return true;

            // check tile actions
            Tile buildingTile = location.map.GetLayer("Buildings").PickTile(new Location(tilePixels.X, tilePixels.Y), Game1.viewport.Size);
            if (buildingTile != null && buildingTile.Properties.TryGetValue("Action", out PropertyValue action) && this.WarpActions.Contains(action.ToString().Split(' ')[0]))
                return true;

            // check tile touch actions
            Tile backTile = location.map.GetLayer("Back").PickTile(new Location(tilePixels.X, tilePixels.Y), Game1.viewport.Size);
            if (backTile != null && backTile.Properties.TryGetValue("TouchAction", out PropertyValue touchAction) && this.TouchWarpActions.Contains(touchAction.ToString().Split(' ')[0]))
                return true;

            // check map warps
            try
            {
                if (location.isCollidingWithWarpOrDoor(tilePixels) != null)
                    return true;
            }
            catch
            {
                // This fails in some cases like the movie theater entrance (which is checked via
                // this.WarpActions above) or TMX Loader's custom tile properties. It's safe to
                // ignore the error here, since that means it's not a valid warp.
            }

            // check mine ladders/shafts
            const int ladderID = 173, shaftID = 174;
            if (location is MineShaft && buildingTile != null && (buildingTile.TileIndex == ladderID || buildingTile.TileIndex == shaftID) && buildingTile.TileSheet.Id == "mine")
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
            if (location.objects.TryGetValue(tile, out Object obj) && obj is Fence fence && fence.isGate.Value && fence.gatePosition.Value == Fence.gateOpenedPosition)
                return false;

            // check for objects, characters, or terrain features
            if (location.isTileOccupiedIgnoreFloors(tile))
                return true;

            // buildings
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    if (building.occupiesTile(tile))
                        return true;
                }
            }

            // large terrain features
            if (location.largeTerrainFeatures.Any(p => p.getBoundingBox().Intersects(tilePixels)))
                return true;

            // resource clumps
            if (location is Farm farm)
            {
                if (farm.resourceClumps.Any(p => p.getBoundingBox(p.tile.Value).Intersects(tilePixels)))
                    return true;
            }

            return false;
        }
    }
}
