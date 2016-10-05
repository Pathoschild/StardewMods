using System;

namespace Pathoschild.LookupAnything.Framework.Logging
{
    /// <summary>Collects log messages for a discrete task and logs them as one entry when disposed.</summary>
    internal interface ICumulativeLog : IDisposable
    {
        /// <summary>Add a message to the log.</summary>
        /// <param name="message">The message to log.</param>
        void Append(string message);

        /// <summary>Add a message to the log followed by a line terminator.</summary>
        /// <param name="message">The message to log.</param>
        void AppendLine(string message);
    }
}