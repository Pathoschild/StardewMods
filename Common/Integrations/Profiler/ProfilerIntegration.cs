using System;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.Profiler;

internal static class ProfilerIntegration
{
    private static IProfilerApi? ProfilerApi;
    internal static void Initialize(IModRegistry registry)
    {
        ProfilerApi = registry.GetApi<IProfilerApi>("SinZ.Profiler");
    }

    public static IDisposable? RecordSection(string modId, string eventType, string details)
    {
        if (ProfilerApi == null)
        {
            return null;
        }

        return ProfilerApi.RecordSection(modId, eventType, details);
    }
}
