using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewValley;
using xTile.Layers;
using xTile.Tiles;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;

/// <summary>Describes a map tile.</summary>
internal class TileSubject : BaseSubject
{
    /*********
    ** Fields
    *********/
    /// <summary>The game location.</summary>
    protected readonly GameLocation Location;

    /// <summary>The tile position.</summary>
    protected readonly Vector2 Position;

    /// <summary>Whether to show raw tile info like tilesheets and tile indexes.</summary>
    protected readonly bool ShowRawTileInfo;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
    /// <param name="location">The game location.</param>
    /// <param name="position">The tile position.</param>
    /// <param name="showRawTileInfo">Whether to show raw tile info like tilesheets and tile indexes.</param>
    public TileSubject(GameHelper gameHelper, GameLocation location, Vector2 position, bool showRawTileInfo)
        : base(gameHelper, $"({position.X}, {position.Y})", I18n.Tile_Description(), I18n.Type_MapTile())
    {
        this.Location = location;
        this.Position = position;
        this.ShowRawTileInfo = showRawTileInfo;
    }

    /// <inheritdoc />
    public override IEnumerable<ICustomField> GetData()
    {
        if (this.ShowRawTileInfo)
        {
            // yield map data
            yield return new GenericField(I18n.Tile_MapName(), this.Location.Name);

            // yield map properties
            StringBuilder summary = new();
            if (this.Location.Map.Properties.Count > 0)
            {
                foreach ((string key, string value) in this.Location.Map.Properties)
                    summary.AppendLine(I18n.Tile_MapProperties_Value(name: key, value: value));

                var field = new GenericField(I18n.Tile_MapProperties(), summary.ToString());
                if (summary.Length > 50)
                    field.CollapseByDefault(I18n.Generic_ShowXResults(this.Location.Map.Properties.Count));

                yield return field;
                summary.Clear();
            }


            // get tile on each layer
            Tile[] tiles = this.GetTiles(this.Location, this.Position).ToArray();
            if (!tiles.Any())
            {
                yield return new GenericField(I18n.Tile_LayerTileNone(), I18n.Tile_LayerTile_NoneHere());
                yield break;
            }

            // fetch tile data
            foreach (Tile tile in tiles)
            {
                summary.AppendLine(I18n.Tile_LayerTile_Appearance(index: this.Stringify(tile.TileIndex), tilesheetId: tile.TileSheet.Id, tilesheetPath: tile.TileSheet.ImageSource.Replace("\\", ": ").Replace("/", ": ")));
                summary.AppendLine();

                if (tile.BlendMode != BlendMode.Alpha)
                    summary.AppendLine(I18n.Tile_LayerTile_BlendMode(value: this.Stringify(tile.BlendMode)));

                foreach ((string name, string value) in tile.Properties)
                    summary.AppendLine(I18n.Tile_LayerTile_TileProperty(name: name, value: value));
                foreach ((string name, string value) in tile.TileIndexProperties)
                    summary.AppendLine(I18n.Tile_LayerTile_IndexProperty(name: name, value: value));

                yield return new GenericField(I18n.Tile_LayerTile(layer: tile.Layer.Id), summary.ToString().TrimEnd());
                summary.Clear();
            }
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IDebugField> GetDebugFields()
    {
        string mapTileLabel = I18n.Type_MapTile();
        string locationLabel = I18n.Tile_GameLocation();

        // tiles
        Tile[] tiles = this.GetTiles(this.Location, this.Position).ToArray();
        foreach (Tile tile in tiles)
        {
            foreach (IDebugField field in this.GetDebugFieldsFrom(tile))
                yield return new GenericDebugField($"{tile.Layer.Id}::{field.Label}", field.Value, field.HasValue) { OverrideCategory = mapTileLabel };
        }

        // location
        foreach (IDebugField field in this.GetDebugFieldsFrom(this.Location))
            yield return new GenericDebugField(field.Label, field.Value, field.HasValue, field.IsPinned) { OverrideCategory = locationLabel };
    }

    /// <inheritdoc />
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
