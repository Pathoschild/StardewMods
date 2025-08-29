using StardewValley;

namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>An item layer which can be selected by the 'auto' layer.</summary>
internal interface IAutoItemLayer : ILayer
{
    /// <summary>Get whether the layer applies to the given item.</summary>
    /// <param name="item">The item.</param>
    bool AppliesTo(Item item);
}
