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
        ** Append/Prepend/ReplaceDelimited
        ****/
        /// <summary>The operation value (e.g. the value to append or replace with).</summary>
        public string? Value { get; }

        /****
        ** RemoveDelimited/ReplaceDelimited
        ****/
        /// <summary>The value to match in the text.</summary>
        public string? Search { get; }

        /// <summary>Which delimited values should be matched.</summary>
        public string? ReplaceMode { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="operation">The text operation to perform.</param>
        /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
        /// <param name="value">The operation value (e.g. the value to append or replace with).</param>
        /// <param name="delimiter">If the target field already has a value, text to add between the previous and inserted values, if any.</param>
        /// <param name="search">The value to match in the text.</param>
        /// <param name="replaceMode">Which delimited values should be matched.</param>
        [JsonConstructor]
        public TextOperationConfig(string? operation, string?[]? target, string? value, string? delimiter, string? search, string? replaceMode)
        {
            this.Operation = operation;
            this.Target = target ?? [];
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
