using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.DataLayers.Layers.Coverage;

/// <summary>A data layer which shows sprinkler coverage.</summary>
internal class SprinklerLayer : BaseLayer, IAutoItemLayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The legend entry for sprinkled tiles.</summary>
    private readonly LegendEntry Wet;

    /// <summary>The legend entry for unsprinkled tiles.</summary>
    private readonly LegendEntry Dry;

    /// <summary>The border color for the sprinkler under the cursor.</summary>
    private readonly Color SelectedColor;

    /// <summary>The maximum number of tiles outside the visible screen area to search for sprinklers.</summary>
    private readonly int SearchRadius;

    /// <summary>Handles access to the supported mod integrations.</summary>
    private readonly ModIntegrations Mods;

    /// <summary>Whether the player has any legacy sprinkler mods which don't override methods like <see cref="SObject.IsSprinkler"/> instead.</summary>
    private readonly bool HasLegacySprinklerMods;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The data layer settings.</param>
    /// <param name="colors">The colors to render.</param>
    /// <param name="mods">Handles access to the supported mod integrations.</param>
    public SprinklerLayer(LayerConfig config, ColorScheme colors, ModIntegrations mods)
        : base(I18n.Sprinklers_Name(), config)
    {
        const string layerId = "SprinklerCoverage";

        // init
        this.Mods = mods;
        this.SelectedColor = colors.Get(layerId, "Selected", Color.Blue);
        this.Legend = [
            this.Wet = new LegendEntry(I18n.Keys.Sprinklers_Covered, colors.Get(layerId, "Covered", Color.Green)),
            this.Dry = new LegendEntry(I18n.Keys.Sprinklers_DryCrops, colors.Get(layerId, "NotCovered", Color.Red))
        ];
        this.HasLegacySprinklerMods = mods.BetterSprinklers.IsLoaded || mods.BetterSprinklersPlus.IsLoaded || mods.LineSprinklers.IsLoaded || mods.SimpleSprinkler.IsLoaded;

        // get search radius
        this.SearchRadius = 10;
        if (this.HasLegacySprinklerMods)
        {
            if (mods.BetterSprinklers.IsLoaded)
                this.SearchRadius = Math.Max(this.SearchRadius, mods.BetterSprinklers.MaxRadius);
            if (mods.BetterSprinklersPlus.IsLoaded)
                this.SearchRadius = Math.Max(this.SearchRadius, mods.BetterSprinklersPlus.MaxRadius);
            if (mods.LineSprinklers.IsLoaded)
                this.SearchRadius = Math.Max(this.SearchRadius, mods.LineSprinklers.MaxRadius);
        }
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        // get coverage
        Dictionary<string, Vector2[]>? legacyCustomCoverageBySprinklerId = this.GetLegacyCustomSprinklerTiles();

        // yield sprinkler coverage
        var covered = new HashSet<Vector2>();
        var groups = new List<TileGroup>();
        foreach (Vector2 origin in visibleArea.Expand(this.SearchRadius).GetTiles())
        {
            if (!location.objects.TryGetValue(origin, out SObject sprinkler) || !this.IsSprinkler(sprinkler, legacyCustomCoverageBySprinklerId))
                continue;

            TileData[] tiles = this
                .GetCoverage(sprinkler, sprinkler.TileLocation, legacyCustomCoverageBySprinklerId, isHeld: false, visibleTiles)
                .Select(pos => new TileData(pos, this.Wet))
                .ToArray();

            foreach (TileData tile in tiles)
                covered.Add(tile.TilePosition);

            groups.Add(new TileGroup(tiles, outerBorderColor: sprinkler.TileLocation == cursorTile ? this.SelectedColor : this.Wet.Color));
        }

        // yield dry crops
        var dryCrops = this
            .GetDryCrops(location, visibleTiles, covered)
            .Select(pos => new TileData(pos, this.Dry));
        groups.Add(new TileGroup(dryCrops, outerBorderColor: this.Dry.Color));

        // yield sprinkler being placed
        SObject heldObj = Game1.player.ActiveObject;
        if (this.IsSprinkler(heldObj, legacyCustomCoverageBySprinklerId))
        {
            var tiles = this
                .GetCoverage(heldObj, cursorTile, legacyCustomCoverageBySprinklerId, isHeld: true, visibleTiles)
                .Select(pos => new TileData(pos, this.Wet, this.Wet.Color * 0.75f));
            groups.Add(new TileGroup(tiles, outerBorderColor: this.SelectedColor, shouldExport: false));
        }

        return groups.ToArray();
    }

    /// <inheritdoc />
    public bool AppliesTo(Item item)
    {
        return
            item is SObject obj
            && this.IsSprinkler(obj, this.GetLegacyCustomSprinklerTiles());
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get whether a map terrain feature is a crop.</summary>
    /// <param name="terrain">The map terrain feature.</param>
    private bool IsCrop(TerrainFeature terrain)
    {
        return terrain is HoeDirt { crop: not null };
    }

    /// <summary>Get whether an object is a sprinkler.</summary>
    /// <param name="sprinkler">The object to check.</param>
    /// <param name="legacyCustomCoverageBySprinklerId">The relative sprinkler coverage by qualified item ID for legacy custom sprinklers which don't override methods like <see cref="SObject.GetSprinklerTiles"/> instead, if any.</param>
    private bool IsSprinkler(SObject? sprinkler, Dictionary<string, Vector2[]>? legacyCustomCoverageBySprinklerId)
    {
        return
            sprinkler != null
            && (
                sprinkler.IsSprinkler()

                // older custom sprinklers
                || (
                    legacyCustomCoverageBySprinklerId != null
                    && sprinkler.bigCraftable.Value
                    && legacyCustomCoverageBySprinklerId.ContainsKey(sprinkler.QualifiedItemId)
                )
            );
    }

    /// <summary>Get the relative sprinkler coverage by qualified item ID for legacy custom sprinklers which don't override methods like <see cref="SObject.GetSprinklerTiles"/> instead, if any.</summary>
    private Dictionary<string, Vector2[]>? GetLegacyCustomSprinklerTiles()
    {
        if (!this.HasLegacySprinklerMods)
            return null;

        Dictionary<string, Vector2[]> tilesBySprinklerId = [];

        // Better Sprinklers
        if (this.Mods.BetterSprinklers.IsLoaded)
        {
            foreach ((int id, Vector2[] range) in this.Mods.BetterSprinklers.GetSprinklerTiles())
                tilesBySprinklerId[$"{ItemRegistry.type_object}{id}"] = range;
        }

        // Better Sprinklers Plus
        if (this.Mods.BetterSprinklersPlus.IsLoaded)
        {
            foreach ((int id, Vector2[] range) in this.Mods.BetterSprinklersPlus.GetSprinklerTiles())
                tilesBySprinklerId[$"{ItemRegistry.type_object}{id}"] = range;
        }

        // Line Sprinklers
        if (this.Mods.LineSprinklers.IsLoaded)
        {
            foreach ((int id, Vector2[] range) in this.Mods.LineSprinklers.GetSprinklerTiles())
                tilesBySprinklerId[$"{ItemRegistry.type_object}{id}"] = range;
        }

        // Simple Sprinkler
        if (this.Mods.SimpleSprinkler.IsLoaded)
        {
            foreach ((int id, Vector2[] range) in this.Mods.SimpleSprinkler.GetNewSprinklerTiles())
                tilesBySprinklerId[$"{ItemRegistry.type_object}{id}"] = range;
        }

        return tilesBySprinklerId;
    }

    /// <summary>Get a sprinkler tile radius.</summary>
    /// <param name="sprinkler">The sprinkler whose radius to get.</param>
    /// <param name="origin">The sprinkler's tile.</param>
    /// <param name="legacyCustomSprinklerRanges">The relative sprinkler coverage by qualified item ID for legacy custom sprinklers which don't override methods like <see cref="SObject.GetSprinklerTiles"/> instead, if any.</param>
    /// <param name="isHeld">Whether the player is holding the sprinkler.</param>
    /// <param name="visibleTiles">The tile positions currently visible on the screen.</param>
    /// <remarks>Derived from <see cref="SObject.DayUpdate"/>.</remarks>
    private IEnumerable<Vector2> GetCoverage(SObject sprinkler, Vector2 origin, Dictionary<string, Vector2[]>? legacyCustomSprinklerRanges, bool isHeld, IReadOnlySet<Vector2> visibleTiles)
    {
        // get vanilla tiles
        IEnumerable<Vector2> tiles = sprinkler.GetSprinklerTiles();
        if (isHeld && sprinkler.TileLocation == Vector2.Zero)
            tiles = tiles.Select(tile => tile + origin);

        // add custom tiles
        if (legacyCustomSprinklerRanges != null && legacyCustomSprinklerRanges.TryGetValue(sprinkler.QualifiedItemId, out Vector2[]? customTiles))
            tiles = new HashSet<Vector2>(tiles.Concat(customTiles.Select(tile => tile + origin)));

        // filter to visible tiles
        return tiles.Where(visibleTiles.Contains);
    }

    /// <summary>Get tiles containing crops not covered by a sprinkler.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
    /// <param name="coveredTiles">The tiles covered by a sprinkler.</param>
    private IEnumerable<Vector2> GetDryCrops(GameLocation location, IReadOnlySet<Vector2> visibleTiles, HashSet<Vector2> coveredTiles)
    {
        foreach (Vector2 tile in visibleTiles)
        {
            if (coveredTiles.Contains(tile))
                continue;

            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain) && this.IsCrop(terrain))
                yield return tile;
        }
    }
}
