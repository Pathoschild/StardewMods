using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.DataParsers;
using Pathoschild.Stardew.DataLayers.Framework;
using Pathoschild.Stardew.DataLayers.Framework.ConfigModels;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace Pathoschild.Stardew.DataLayers.Layers.Crops;

/// <summary>A data layer which shows whether crops are ready to be harvested.</summary>
internal class CropHarvestLayer : BaseLayer, IAutoItemLayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The legend entry for crops which are ready.</summary>
    private readonly LegendEntry Ready;

    /// <summary>The legend entry for crops which are not ready.</summary>
    private readonly LegendEntry NotReady;

    /// <summary>The legend entry for crops which won't be ready to harvest before the season change (or are dead).</summary>
    private readonly LegendEntry NotEnoughTimeOrDead;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The data layer settings.</param>
    /// <param name="colors">The colors to render.</param>
    public CropHarvestLayer(LayerConfig config, ColorScheme colors)
        : base(I18n.CropHarvest_Name(), config)
    {
        const string layerId = "CropsReadyToHarvest";

        this.Legend = [
            this.Ready = new LegendEntry(I18n.Keys.CropHarvest_Ready, colors.Get(layerId, "Ready", Color.Green)),
            this.NotReady = new LegendEntry(I18n.Keys.CropHarvest_NotReady, colors.Get(layerId, "NotReady", Color.Black)),
            this.NotEnoughTimeOrDead = new LegendEntry(I18n.Keys.CropHarvest_NotEnoughTimeOrDead, colors.Get(layerId, "NotEnoughTimeOrDead", Color.Red))
        ];
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        ILookup<string, TileData> tiles = this.GetTiles(location, visibleTiles).ToLookup(p => p.Type.Id);

        return [
            new TileGroup(tiles[this.Ready.Id], outerBorderColor: this.Ready.Color),
            new TileGroup(tiles[this.NotReady.Id]),
            new TileGroup(tiles[this.NotEnoughTimeOrDead.Id], outerBorderColor: this.NotEnoughTimeOrDead.Color)
        ];
    }

    /// <inheritdoc />
    public bool AppliesTo(Item item)
    {
        switch (item)
        {
            // scythe
            case MeleeWeapon tool:
                return tool.isScythe();

            // seeds
            case Object when item.HasTypeObject():
                return
                    item.ItemId is "MixedFlowerSeeds" or Crop.mixedSeedsId or "251"/* Tea Sapling */
                    || Game1.cropData.ContainsKey(item.ItemId);

            default:
                return false;
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get all tiles.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
    private IEnumerable<TileData> GetTiles(GameLocation location, IReadOnlySet<Vector2> visibleTiles)
    {
        // check tiles
        foreach (Vector2 tile in visibleTiles)
        {
            if (this.TryCheckTile(location, tile, out TileData? tileData, out int tileWidth))
            {
                yield return tileData;

                for (int xOffset = 1; xOffset < tileWidth; xOffset++)
                    yield return new TileData(new Vector2(tileData.TilePosition.X + xOffset, tileData.TilePosition.Y), tileData.Type, tileData.Color, tileData.DrawOffset);
            }
        }

        // check large terrain features
        foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
        {
            if (visibleTiles.Contains(feature.Tile) && this.TryCheckLargeTerrainFeature(feature, out TileData? tileData, out int tileWidth))
            {
                yield return tileData;

                for (int xOffset = 1; xOffset < tileWidth; xOffset++)
                    yield return new TileData(new Vector2(feature.Tile.X + xOffset, feature.Tile.Y), tileData.Type, tileData.Color, tileData.DrawOffset);
            }
        }
    }

    /// <summary>Get the tile data for a tile.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="tile">The tile to check.</param>
    /// <param name="tileData">The tile data if valid.</param>
    /// <param name="tileWidth">The bush's tile width.</param>
    /// <returns>Returns whether the tile contains a harvestable type.</returns>
    private bool TryCheckTile(GameLocation location, Vector2 tile, [NotNullWhen(true)] out TileData? tileData, out int tileWidth)
    {
        // crop
        if (this.GetDirt(location, tile)?.crop is { } crop && this.TryCheckCrop(location, tile, crop, out tileData))
        {
            tileWidth = 1;
            return true;
        }

        // object
        if (location.objects.TryGetValue(tile, out Object obj) && obj is not null)
        {
            if (obj is IndoorPot pot)
            {
                if (pot.bush.Value is { } bush && this.TryCheckBush(tile, bush, out tileData, out tileWidth))
                    return true;

                tileData = null;
                tileWidth = 0;
                return false;
            }

            if (obj.isForage() || obj is { IsSpawnedObject: true, CanBeGrabbed: true })
            {
                tileData = new TileData(tile, this.Ready);
                tileWidth = 1;
                return true;
            }
        }

        // terrain feature
        if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature? terrainFeature) && this.TryCheckTerrainFeature(tile, terrainFeature, out tileData, out tileWidth))
            return true;

        // none found
        tileData = null;
        tileWidth = 0;
        return false;
    }

    /// <summary>Get the tile data for a bush.</summary>
    /// <param name="tile">The tile containing the bush.</param>
    /// <param name="bush">The bush instance.</param>
    /// <param name="tileData">The tile data if valid.</param>
    /// <param name="tileWidth">The bush's tile width.</param>
    /// <returns>Returns whether the bush is a harvestable type.</returns>
    private bool TryCheckBush(Vector2 tile, Bush bush, [NotNullWhen(true)] out TileData? tileData, out int tileWidth)
    {
        if (
            !(bush.size.Value == Bush.mediumBush && !bush.townBush.Value) // berry bush
            && bush.size.Value != Bush.greenTeaBush // tea bush
        )
        {
            tileData = null;
            tileWidth = 0;
            return false;
        }

        bool ready = bush.tileSheetOffset.Value == 1;
        tileData = new TileData(tile, ready ? this.Ready : this.NotReady);
        tileWidth = bush.size.Value switch
        {
            Bush.walnutBush or Bush.mediumBush => 2,
            Bush.largeBush => 3,
            _ => 1
        };
        return true;
    }

    /// <summary>Get the tile data for a crop.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="tile">The tile containing the crop.</param>
    /// <param name="crop">The crop instance.</param>
    /// <param name="tileData">The tile data if valid.</param>
    /// <returns>Returns whether the crop is a harvestable type.</returns>
    private bool TryCheckCrop(GameLocation location, Vector2 tile, Crop crop, [NotNullWhen(true)] out TileData? tileData)
    {
        // get data
        CropDataParser data = new CropDataParser(crop, isPlanted: true);
        if (data.CropData == null)
        {
            tileData = null;
            return false;
        }

        // get harvest time
        if (crop.dead.Value)
            tileData = new TileData(tile, this.NotEnoughTimeOrDead);
        else if (data.CanHarvestNow)
            tileData = new TileData(tile, this.Ready);
        else if (!location.SeedsIgnoreSeasonsHere() && !data.Seasons.Contains(data.GetNextHarvest().Season))
            tileData = new TileData(tile, this.NotEnoughTimeOrDead);
        else
            tileData = new TileData(tile, this.NotReady);
        return true;
    }

    /// <summary>Get the tile data for a terrain feature.</summary>
    /// <param name="tile">The tile containing the terrain feature.</param>
    /// <param name="terrainFeature">The terrain feature instance.</param>
    /// <param name="tileData">The tile data if valid.</param>
    /// <param name="tileWidth">The terrain feature's tile width.</param>
    /// <returns>Returns whether the crop is a harvestable type.</returns>
    private bool TryCheckTerrainFeature(Vector2 tile, TerrainFeature terrainFeature, [NotNullWhen(true)] out TileData? tileData, out int tileWidth)
    {
        switch (terrainFeature)
        {
            case Bush bush:
                if (this.TryCheckBush(tile, bush, out tileData, out tileWidth))
                    return true;
                break;

            case FruitTree fruitTree:
                if (fruitTree.growthStage.Value >= FruitTree.treeStage)
                {
                    bool ready = fruitTree.fruit.Count > 0;
                    tileData = new TileData(tile, ready ? this.Ready : this.NotReady);
                    tileWidth = 1;
                    return true;
                }
                break;

            case Tree tree:
                if (!tree.stump.Value && tree.growthStage.Value >= Tree.treeStage)
                {
                    bool ready =
                        tree.hasMoss.Value
                        || (tree.hasSeed.Value && (Game1.IsMultiplayer || Game1.player.ForagingLevel >= 1));
                    tileData = new TileData(tile, ready ? this.Ready : this.NotReady);
                    tileWidth = 1;
                    return true;
                }
                break;
        }

        tileData = null;
        tileWidth = 0;
        return false;
    }

    /// <summary>Get the tile data for a large terrain feature.</summary>
    /// <param name="terrainFeature">The terrain feature instance.</param>
    /// <param name="tileData">The tile data if valid.</param>
    /// <param name="tileWidth">The terrain feature's tile width.</param>
    /// <returns>Returns whether the crop is a harvestable type.</returns>
    private bool TryCheckLargeTerrainFeature(LargeTerrainFeature terrainFeature, [NotNullWhen(true)] out TileData? tileData, out int tileWidth)
    {
        switch (terrainFeature)
        {
            case Bush bush:
                if (this.TryCheckBush(terrainFeature.Tile, bush, out tileData, out tileWidth))
                    return true;

                break;
        }

        tileData = null;
        tileWidth = 0;
        return false;
    }
}
