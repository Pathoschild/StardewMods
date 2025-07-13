using System;

namespace Pathoschild.Stardew.Common.Integrations.Profiler;

/// <summary>The API provided by the Profiler mod.</summary>
public interface IProfilerApi
{
    /// <summary>Start an operation whose timing will be tracked by Profiler.</summary>
    /// <param name="modId">The mod or content pack ID for which to time the operation.</param>
    /// <param name="eventType">A descriptive key for the operation. This only needs to be unique within the same mod's operations.</param>
    /// <param name="details">The arbitrary details to show with the operation in timing results.</param>
    /// <returns>Returns a disposable instance which can be disposed to end the operation.</returns>
    public IDisposable RecordSection(string modId, string eventType, string details);
}
