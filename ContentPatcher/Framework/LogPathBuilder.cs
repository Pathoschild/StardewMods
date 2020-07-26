using System;
using System.Collections.Generic;
using System.Linq;

namespace ContentPatcher.Framework
{
    /// <summary>Encapsulates building a breadcrumb path for log messages.</summary>
    internal class LogPathBuilder
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The path values to combine into a breadcrumb path.</summary>
        public string[] Segments { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="segments">The initial path values.</param>
        public LogPathBuilder(params string[] segments)
        {
            this.Segments = segments;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="segments">The initial path values.</param>
        public LogPathBuilder(IEnumerable<string> segments)
            : this(segments.ToArray()) { }

        /// <summary>Get a new instance with the given path values appended.</summary>
        /// <param name="segments">The path values to append.</param>
        public LogPathBuilder With(params string[] segments)
        {
            return new LogPathBuilder(this.Segments.Concat(segments));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" > ", this.Segments);
        }
    }
}
