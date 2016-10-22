using System;
using System.Text;
using StardewModdingAPI;

namespace Pathoschild.LookupAnything.Framework.Logging
{
    /// <summary>Collects log messages for a discrete task and logs them as one entry.</summary>
    internal class CumulativeLog : ICumulativeLog
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying message.</summary>
        private readonly StringBuilder Message = new StringBuilder();

        /// <summary>Whether the log has been disposed.</summary>
        private bool IsDisposed;


        /*********
        ** Public methods
        *********/
        /// <summary>Add a message to the log.</summary>
        /// <param name="message">The message to log.</param>
        public void Append(string message)
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException(nameof(CumulativeLog));

            this.Message.Append(message);
        }

        /// <summary>Add a message to the log followed by a line terminator.</summary>
        /// <param name="message">The message to log.</param>
        public void AppendLine(string message)
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException(nameof(CumulativeLog));

            this.Message.AppendLine(message);
        }

        /// <summary>Flush all messages to the log.</summary>
        public void Dispose()
        {
            if (this.IsDisposed)
                return;
            this.IsDisposed = true;

            Log.Debug(this.Message.ToString().TrimEnd('\r', '\n'));
        }
    }
}
