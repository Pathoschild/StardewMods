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
        /****
        ** Common fields
        ****/
        /// <summary>The text operation to perform.</summary>
        public string? Operation { get; }

        /// <summary>The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</summary>
        public string?[] Target { get; }

        /// <summary>The text between values in a delimited string.</summary>
        public string? Delimiter { get; }

        /****
        ** Append/Prepend
        ****/
        /// <summary>The value to append or prepend.</summary>
        public string? Value { get; }

        /****
        ** RemoveDelimited
        ****/
        /// <summary>The value to remove from the text.</summary>
        public string? Search { get; }

        /// <summary>Which delimited values should be removed.</summary>
        public string? ReplaceMode { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="operation">The text operation to perform.</param>
        /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
        /// <param name="value">The value to append or prepend.</param>
        /// <param name="delimiter">If the target field already has a value, text to add between the previous and inserted values, if any.</param>
        /// <param name="search">The value to remove from the text.</param>
        /// <param name="replaceMode">Which delimited values should be removed.</param>
        [JsonConstructor]
        public TextOperationConfig(string? operation, string?[]? target, string? value, string? delimiter, string? search, string? replaceMode)
        {
            this.Operation = operation;
            this.Target = target ?? Array.Empty<string>();
            this.Value = value;
            this.Delimiter = delimiter;
            this.Search = search;
            this.ReplaceMode = replaceMode;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="other">The other instance to copy.</param>
        public TextOperationConfig(TextOperationConfig other)
            : this(
                  operation: other.Operation,
                  target: other.Target.ToArray(),
                  value: other.Value,
                  delimiter: other.Delimiter,
                  search: other.Search,
                  replaceMode: other.ReplaceMode
            )
        { }
    }
}
