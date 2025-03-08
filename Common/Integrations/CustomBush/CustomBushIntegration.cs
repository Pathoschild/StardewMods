using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.Common.Integrations.CustomBush;

/// <summary>Handles the logic for integrating with the Custom Bush mod.</summary>
internal class CustomBushIntegration : BaseIntegration<ICustomBushApi>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public CustomBushIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("CustomBush", "furyx639.CustomBush", "2.0.0-beta.1", modRegistry, monitor) { }

    /// <summary>Try to get the custom bush model associated with the given bush.</summary>
    /// <param name="bush">The bush to check.</param>
    /// <param name="customBush">The resulting custom bush, if applicable.</param>
    /// <returns>True if the custom bush associated with the given bush is found.</returns>
    public bool TryGetBush(Bush bush, [NotNullWhen(true)] out ICustomBush? customBush)
    {
        customBush = null;

        return this.IsLoaded && this.ModApi.TryGetBush(bush, out customBush);
    }
}
