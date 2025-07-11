using System;
using System.Collections.Generic;
using System.Text;
using StardewModdingAPI;

namespace ContentPatcher.Experiment
{
    internal static class ProfilerIntegration
    {
        class PlaceholderDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private static IProfilerApi? ProfilerApi;
        internal static void Initialize(IModRegistry registry)
        {
            ProfilerApi = registry.GetApi<IProfilerApi>("SinZ.Profiler");
        }

        public static IDisposable RecordSection(string ModId, string EventType, string Details)
        {
            if (ProfilerApi == null)
            {
                return new PlaceholderDisposable();
            }

            return ProfilerApi.RecordSection(ModId, EventType, Details);
        }
    }
}
