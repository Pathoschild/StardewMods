using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewValley;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace Pathoschild.Stardew.LookupAnything.Framework.Subjects
{
    /// <summary>Describes a map tile.</summary>
    internal class TileSubject : BaseSubject
    {
        /*********
        ** Fields
        *********/
        /// <summary>The game location.</summary>
        private readonly GameLocation Location;

        /// <summary>The tile position.</summary>
        private readonly Vector2 Position;

        /// <summary>Whether to show raw tile info like tilesheets and tile indexes.</summary>
        private readonly bool ShowRawTileInfo;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="codex">Provides subject entries for target values.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="location">The game location.</param>
        /// <param name="position">The tile position.</param>
        /// <param name="showRawTileInfo">Whether to show raw tile info like tilesheets and tile indexes.</param>
        public TileSubject(SubjectFactory codex, GameHelper gameHelper, GameLocation location, Vector2 position, bool showRawTileInfo)
            : base(codex, gameHelper, $"({position.X}, {position.Y})", I18n.Tile_Description(), I18n.Type_MapTile())
        {
            this.Location = location;
            this.Position = position;
            this.ShowRawTileInfo = showRawTileInfo;
        }

        /// <summary>Get the data to display for this subject.</summary>
        public override IEnumerable<ICustomField> GetData()
        {
            // raw map data
            if (this.ShowRawTileInfo)
            {
                // yield map data
                yield return new GenericField(this.GameHelper, I18n.Tile_MapName(), this.Location.Name);

                // get tiles
                Tile[] tiles = this.GetTiles(this.Location, this.Position).ToArray();
                if (!tiles.Any())
                {
                    yield return new GenericField(this.GameHelper, I18n.Tile_Tile(), I18n.Tile_Tile_NoneHere());
                    yield break;
                }

                // fetch tile data
                foreach (Tile tile in tiles)
                {
                    string layerName = tile.Layer.Id;
                    yield return new GenericField(this.GameHelper, I18n.Tile_TileIndex(layerName: layerName), this.Stringify(tile.TileIndex));
                    yield return new GenericField(this.GameHelper, I18n.Tile_Tilesheet(layerName: layerName), tile.TileSheet.ImageSource.Replace("\\", ": ").Replace("/", ": "));
                    yield return new GenericField(this.GameHelper, I18n.Tile_BlendMode(layerName: layerName), this.Stringify(tile.BlendMode));
                    foreach (KeyValuePair<string, PropertyValue> property in tile.TileIndexProperties)
                        yield return new GenericField(this.GameHelper, I18n.Tile_IndexProperty(layerName: layerName, propertyName: property.Key), property.Value);
                    foreach (KeyValuePair<string, PropertyValue> property in tile.Properties)
                        yield return new GenericField(this.GameHelper, I18n.Tile_TileProperty(layerName: layerName, propertyName: property.Key), property.Value);
                }
            }
        }

        /// <summary>Get raw debug data to display for this subject.</summary>
        public override IEnumerable<IDebugField> GetDebugFields()
        {
            Tile[] tiles = this.GetTiles(this.Location, this.Position).ToArray();
            foreach (Tile tile in tiles)
            {
                foreach (IDebugField field in this.GetDebugFieldsFrom(tile))
                    yield return new GenericDebugField($"{tile.Layer.Id}::{field.Label}", field.Value, field.HasValue, field.IsPinned);
            }
        }

        /// <summary>Draw the subject portrait (if available).</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        /// <param name="position">The position at which to draw.</param>
        /// <param name="size">The size of the portrait to draw.</param>
        /// <returns>Returns <c>true</c> if a portrait was drawn, else <c>false</c>.</returns>
        public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
        {
            return false;
        }

        /// <summary>Whether a tile lookup should be enabled.</summary>
        /// <param name="location">The game location being searched.</param>
        /// <param name="tile">The tile position.</param>
        /// <param name="enableTileMetadata">Whether map tile metadata</param>
        public static bool EnableLookup(GameLocation location, Vector2 tile, bool enableTileMetadata)
        {
            return
                enableTileMetadata;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the tiles at the specified tile position.</summary>
        /// <param name="location">The game location.</param>
        /// <param name="position">The tile position.</param>
        private IEnumerable<Tile> GetTiles(GameLocation location, Vector2 position)
        {
            if (position.X < 0 || position.Y < 0)
                yield break;

            foreach (Layer layer in location.map.Layers)
            {
                if (position.X > layer.LayerWidth || position.Y > layer.LayerHeight)
                    continue;

                Tile tile = layer.Tiles[(int)position.X, (int)position.Y];
                if (tile != null)
                    yield return tile;
            }
        }

        /// <summary>Get whether a tile property is defined.</summary>
        /// <param name="location">The game location.</param>
        /// <param name="tile">The tile position.</param>
        /// <param name="name">The property name.</param>
        /// <param name="layer">The map layer name to check.</param>
        /// <param name="arguments">The space-separated property values, if any.</param>
        private static bool HasTileProperty(GameLocation location, Vector2 tile, string name, string layer, out string[] arguments)
        {
            string property = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, name, layer);
            arguments = property?.Split(' ').ToArray() ?? new string[0];
            return property != null;
        }
    }
}
