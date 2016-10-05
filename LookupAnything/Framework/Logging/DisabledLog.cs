namespace Pathoschild.LookupAnything.Framework.Logging
{
    /// <summary>A logger that deliberately does nothing.</summary>
    internal class DisabledLog : ICumulativeLog
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Add a message to the log.</summary>
        /// <param name="message">The message to log.</param>
        public void Append(string message) { }

        /// <summary>Add a message to the log followed by a line terminator.</summary>
        /// <param name="message">The message to log.</param>
        public void AppendLine(string message) { }

        /// <summary>Flush all messages to the log.</summary>
        public void Dispose() { }
    }
}