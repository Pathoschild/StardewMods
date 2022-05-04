using System;
using System.Linq;
using Newtonsoft.Json;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The input settings for a <see cref="PatchConfig.TextOperations"/> field.</summary>
    internal class TextOperationConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The text operation to perform.</summary>
        public string? Operation { get; }

        /// <summary>The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</summary>
        public string?[] Target { get; }

        /// <summary>The value to append or prepend.</summary>
        public string? Value { get; }

        /// <summary>If the target field already has a value, text to add between the previous and inserted values, if any.</summary>
        public string? Delimiter { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="operation">The text operation to perform.</param>
        /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
        /// <param name="value">The value to append or prepend.</param>
        /// <param name="delimiter">If the target field already has a value, text to add between the previous and inserted values, if any.</param>
        [JsonConstructor]
        public TextOperationConfig(string? operation, string?[]? target, string? value, string? delimiter)
        {
            this.Operation = operation;
            this.Target = target ?? Array.Empty<string>();
            this.Value = value;
            this.Delimiter = delimiter;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="other">The other instance to copy.</param>
        public TextOperationConfig(TextOperationConfig other)
            : this(
                  operation: other.Operation,
                  target: other.Target.ToArray(),
                  value: other.Value,
                  delimiter: other.Delimiter
            )
        { }
    }
}
