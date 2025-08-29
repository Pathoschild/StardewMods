using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.DataParsers;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
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
        var tiles = this.GetTiles(location, visibleTiles).ToLookup(p => p.Type.Id);

        return [
            new TileGroup(tiles[this.Ready.Id], outerBorderColor: this.Ready.Color),
            new TileGroup(tiles[this.NotReady.Id]),
            new TileGroup(tiles[this.NotEnoughTimeOrDead.Id], outerBorderColor: this.NotEnoughTimeOrDead.Color)
        ];
    }

    /// <inheritdoc />
    public bool AppliesTo(Item item)
    {
        return
            item is MeleeWeapon tool
            && tool.isScythe();
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get all tiles.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
    private IEnumerable<TileData> GetTiles(GameLocation location, IReadOnlySet<Vector2> visibleTiles)
    {
        foreach (Vector2 tile in visibleTiles)
        {
            // get crop
            Crop? crop = this.GetDirt(location, tile)?.crop;
            if (crop == null)
                continue;

            // special case: crop is dead
            if (crop.dead.Value)
            {
                yield return new TileData(tile, this.NotEnoughTimeOrDead);
                continue;
            }

            // yield tile
            CropDataParser data = new CropDataParser(crop, isPlanted: true);
            if (data.CropData != null)
            {
                if (data.CanHarvestNow)
                    yield return new TileData(tile, this.Ready);
                else if (!location.SeedsIgnoreSeasonsHere() && !data.Seasons.Contains(data.GetNextHarvest().Season))
                    yield return new TileData(tile, this.NotEnoughTimeOrDead);
                else
                    yield return new TileData(tile, this.NotReady);
            }
        }
    }
}
