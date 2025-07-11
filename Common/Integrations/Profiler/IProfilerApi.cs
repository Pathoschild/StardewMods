using System;

namespace Pathoschild.Stardew.Common.Integrations.Profiler;

public interface IProfilerApi
{
    public IDisposable RecordSection(string ModId, string EventType, string Details);
}
