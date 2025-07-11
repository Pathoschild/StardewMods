using System;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.Profiler;

/// <summary>Handles the logic for integrating with the Profiler mod.</summary>
internal class ProfilerIntegration : BaseIntegration<IProfilerApi>
{
    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
    /// <param name="monitor">Encapsulates monitoring and logging.</param>
    public ProfilerIntegration(IModRegistry modRegistry, IMonitor monitor)
        : base("Profiler", "SinZ.Profiler", "2.0.0", modRegistry, monitor) { }

    /// <returns>If Profiler is installed, returns a disposable instance which can be disposed to end the operation; else returns <c>null</c>.</returns>
    /// <inheritdoc cref="IProfilerApi.RecordSection"/>
    public IDisposable? RecordSection(string modId, string eventType, string details)
    {
        return this.ModApi?.RecordSection(modId, eventType, details);
    }
}
