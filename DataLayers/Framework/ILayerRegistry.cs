namespace Pathoschild.Stardew.DataLayers.Framework;

/// <summary>The data for a data layer registered through the API.</summary>
/// <param name="UniqueId">The unique ID for this layer, including the mod ID prefix.</param>
/// <param name="LocalId">The unique ID for this layer within the mod, used in color schemes.</param>
/// <param name="Layer">The layer implementation provided by the mod.</param>
internal record LayerRegistration(string UniqueId, string LocalId, IDataLayer Layer);
