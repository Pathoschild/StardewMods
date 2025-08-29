namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>A building layer which can be selected by the 'auto' layer.</summary>
internal interface IAutoBuildingLayer : ILayer
{
    /// <summary>Get whether the layer applies to the given building.</summary>
    /// <param name="buildingType">The building type.</param>
    bool AppliesTo(string buildingType);
}
