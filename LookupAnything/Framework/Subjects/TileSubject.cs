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
        ** Properties
        *********/
        /// <summary>The game location.</summary>
        private readonly GameLocation Location;

        /// <summary>The tile position.</summary>
        private readonly Vector2 Position;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="location">The game location.</param>
        /// <param name="position">The tile position.</param>
        public TileSubject(GameLocation location, Vector2 position)
            : base($"({position.X}, {position.Y})", "A tile position on the map. This is displayed because you enabled tile lookups in the configuration.", "Map tile")
        {
            this.Location = location;
            this.Position = position;
        }

        /// <summary>Get the data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<ICustomField> GetData(Metadata metadata)
        {
            // yield map data
            yield return new GenericField("Map name", this.Location.Name);

            // get tiles
            Tile[] tiles = this.GetTiles(this.Location, this.Position).ToArray();
            if (!tiles.Any())
            {
                yield return new GenericField("Tile", "no tile here");
                yield break;
            }

            // fetch tile data
            foreach (Tile tile in tiles)
            {
                string layerName = tile.Layer.Id;
                yield return new GenericField($"{layerName}: tile index", tile.TileIndex);
                yield return new GenericField($"{layerName}: tile sheet", tile.TileSheet.ImageSource.Replace("\\", ": ").Replace("/", ": "));
                yield return new GenericField($"{layerName}: blend mode", tile.BlendMode);
                foreach (KeyValuePair<string, PropertyValue> property in tile.TileIndexProperties)
                    yield return new GenericField($"{layerName}: ix props: {property.Key}", property.Value);
                foreach (KeyValuePair<string, PropertyValue> property in tile.Properties)
                    yield return new GenericField($"{layerName}: props: {property.Key}", property.Value);
            }
        }

        /// <summary>Get raw debug data to display for this subject.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public override IEnumerable<IDebugField> GetDebugFields(Metadata metadata)
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
    }
}
