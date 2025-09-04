using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace Pathoschild.Stardew.DataLayers.Layers.Crops;

/// <summary>A data layer which shows whether crops needs to be watered.</summary>
internal class CropWaterLayer : BaseLayer, IAutoItemLayer
{
    /*********
    ** Fields
    *********/
    /// <summary>The legend entry for dry crops.</summary>
    private readonly LegendEntry Dry;

    /// <summary>The legend entry for watered crops.</summary>
    private readonly LegendEntry Watered;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="config">The data layer settings.</param>
    /// <param name="colors">The colors to render.</param>
    public CropWaterLayer(LayerConfig config, ColorScheme colors)
        : base(I18n.CropWater_Name(), config)
    {
        const string layerId = "WateredCrops";

        this.Legend = [
           this.Watered = new LegendEntry(I18n.Keys.CropWater_Watered, colors.Get(layerId, "Watered", Color.Green)),
            this.Dry = new LegendEntry(I18n.Keys.CropWater_Dry, colors.Get(layerId, "Dry", Color.Red))
        ];
    }

    /// <inheritdoc />
    public override TileGroup[] Update(ref readonly GameLocation location, ref readonly Rectangle visibleArea, ref readonly IReadOnlySet<Vector2> visibleTiles, ref readonly Vector2 cursorTile)
    {
        return [
            this.GetGroup(location, visibleTiles, HoeDirt.watered, this.Watered),
            this.GetGroup(location, visibleTiles, HoeDirt.dry, this.Dry)
        ];
    }

    /// <inheritdoc />
    public bool AppliesTo(Item item)
    {
        return item is WateringCan;
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get a tile group.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
    /// <param name="state">The watered state to match.</param>
    /// <param name="type">The legend entry for the group.</param>
    private TileGroup GetGroup(GameLocation location, IReadOnlySet<Vector2> visibleTiles, int state, LegendEntry type)
    {
        var crops = this
            .GetCropsByStatus(location, visibleTiles, state)
            .Select(pos => new TileData(pos, type));
        return new TileGroup(crops, outerBorderColor: type.Color);
    }

    /// <summary>Get tiles containing crops not covered by a sprinkler.</summary>
    /// <param name="location">The current location.</param>
    /// <param name="visibleTiles">The tiles currently visible on the screen.</param>
    /// <param name="state">The watered state to match.</param>
    private IEnumerable<Vector2> GetCropsByStatus(GameLocation location, IReadOnlySet<Vector2> visibleTiles, int state)
    {
        foreach (Vector2 tile in visibleTiles)
        {
            HoeDirt? dirt = this.GetDirt(location, tile);
            if (dirt?.crop != null && !this.IsDeadCrop(dirt) && dirt.state.Value == state)
                yield return tile;
        }
    }
}
