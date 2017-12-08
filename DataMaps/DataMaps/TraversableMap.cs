using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataMaps.Framework;
using StardewValley;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.DataMaps.DataMaps
{
    /// <summary>A data map which shows whether tiles are traversable by the player.</summary>
    internal class TraversableMap : IDataMap
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


        /*********
        ** Accessors
        *********/
        /// <summary>The map's display name.</summary>
        public string Name { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public TraversableMap()
        {
            this.Name = "Accessibility";
        }

        /// <summary>Get the legend entries to display.</summary>
        public IEnumerable<LegendEntry> GetLegendEntries()
        {
            return new[]
            {
                new LegendEntry("Clear", this.ClearColor),
                new LegendEntry("Occupied", this.OccupiedColor),
                new LegendEntry("Impassable", this.ImpassableColor),
                new LegendEntry("Warp", this.WarpColor)
            };
        }

        /// <summary>Get the updated data map tiles.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="visibleArea">The tiles currently visible on the screen.</param>
        public IEnumerable<TileGroup> Update(GameLocation location, Rectangle visibleArea)
        {
            TileData[] tiles = this.GetTiles(location, visibleArea.GetTiles()).ToArray();

            // passable tiles
            TileData[] passableTiles = tiles.Where(p => p.Color == this.ClearColor).ToArray();
            yield return new TileGroup(passableTiles, outerBorders: true);

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
                Color color;
                if (this.IsWarp(location, tile))
                    color = this.WarpColor;
                else if (this.IsPassable(location, tile))
                    color = this.IsOccupied(location, tile) ? this.OccupiedColor : this.ClearColor;
                else
                    color = this.ImpassableColor;

                yield return new TileData(tile, color);
            }
        }

        /// <summary>Get whether there's a warp on the given tile.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <remarks>Derived from <see cref="GameLocation.isCollidingWithWarp"/> and <see cref="GameLocation.performAction"/>.</remarks>
        private bool IsWarp(GameLocation location, Vector2 tile)
        {
            // check map warps
            {
                Rectangle area = new Rectangle((int)(tile.X * Game1.tileSize), (int)(tile.Y * Game1.tileSize), Game1.tileSize, Game1.tileSize);
                if (location.isCollidingWithWarpOrDoor(area) != null)
                    return true;
            }

            // check tile action
            {
                Tile mapTile = location.map.GetLayer("Buildings").PickTile(new Location((int)(tile.X * Game1.tileSize), (int)(tile.Y * Game1.tileSize)), Game1.viewport.Size);
                if (mapTile != null && mapTile.Properties.TryGetValue("Action", out PropertyValue action) && this.WarpActions.Contains(action))
                    return true;
            }

            return false;
        }

        /// <summary>Get whether a tile is passable.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        /// <remarks>Derived from <see cref="Farmer.MovePosition"/> and <see cref="Fence"/>.</remarks>
        private bool IsPassable(GameLocation location, Vector2 tile)
        {
            // tile passable
            if (location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
                return true;

            return false;
        }

        /// <summary>Get whether a tile is blocked due to something it contains.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to check.</param>
        private bool IsOccupied(GameLocation location, Vector2 tile)
        {
            if (location.isTileOccupiedIgnoreFloors(tile))
                return true;

            return false;
        }
    }
}
