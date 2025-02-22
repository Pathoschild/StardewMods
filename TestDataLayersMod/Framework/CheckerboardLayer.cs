using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.DataLayers;
using StardewValley;

namespace Pathoschild.Stardew.TestDataLayersMod.Framework;

/// <summary>A data layer which shows a checkerboard tile pattern.</summary>
internal class CheckerboardLayer : IDataLayer
{
    /*********
    ** Accessors
    *********/
    /// <inheritdoc />
    public string Name => I18n.Example_Layer_Title();


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public void Configure(ILegendBuilder legendBuilder)
    {
        legendBuilder
            .Add("example.layer.even", I18n.Example_Layer_Even(), Color.Green)
            .Add("example.layer.odd", I18n.Example_Layer_Odd(), Color.Red);
    }

    /// <inheritdoc />
    public void Update(ILayerBuilder builder, GameLocation location, Rectangle visibleArea, IReadOnlySet<Vector2> visibleTiles, Vector2 cursorTile)
    {
        builder.AddTileGroup(
            "",
            group => group.AddTiles(
                visibleTiles,
                coords => coords.X % 2 == 0 ^ coords.Y % 2 == 0
                    ? "example.layer.even"
                    : "example.layer.odd"
            )
        );
    }
}
