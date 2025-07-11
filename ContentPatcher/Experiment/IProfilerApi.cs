using System;
using System.Collections.Generic;
using System.Text;

namespace ContentPatcher.Experiment
{
    public interface IProfilerApi
    {
        public IDisposable RecordSection(string ModId, string EventType, string Details);
    }
}
