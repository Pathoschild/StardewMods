using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations
{
    /// <summary>A text operation which parses a field's current value as a delimited list of values, and removes those matching a search value.</summary>
    internal class RemoveDelimitedTextOperation : BaseTextOperation
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The value to append or prepend.</summary>
        public ITokenString Value { get; set; }

        /// <summary>If the target field already has a value, text to add between the previous and inserted values, if any.</summary>
        public string Delimiter { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="operation">The text operation to perform.</param>
        /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
        /// <param name="value">The value to append or prepend.</param>
        /// <param name="delimiter">If the target field already has a value, text to add between the previous and inserted values, if any.</param>
        public RemoveDelimitedTextOperation(TextOperationType operation, ICollection<IManagedTokenString> target, IManagedTokenString value, string? delimiter)
            : base(operation, target)
        {
            this.Value = value;
            this.Delimiter = delimiter ?? string.Empty;

            this.Contextuals.Add(value);
        }

        /// <inheritdoc />
        public override string Apply(string? text)
        {
            string? value = this.Value.Value;
            if (value is null)
                return text ?? "";

            string delimiter = string.IsNullOrEmpty(text)
                ? ""
                : this.Delimiter;

            return this.Operation switch
            {
                TextOperationType.RemoveFirstOccurrence => this.RemoveFirstOccurrence(value, text, delimiter),
                TextOperationType.RemoveLastOccurrence => this.RemoveLastOccurrence(value, text, delimiter),
                TextOperationType.RemoveAllOccurrences => this.RemoveAllOccurrences(value, text, delimiter),
                _ => throw new InvalidOperationException($"Unknown text operation type '{this.Operation}'.")
            };
        }

        private string RemoveFirstOccurrence(string value, string? text, string delimiter)
        {
            if (text is null)
                return "";
            if (delimiter == "")
                return text ?? "";
            List<string> split = text.Split(delimiter).ToList();
            for (int i = 0; i < split.Count; i++)
            {
                if (split[i] == value)
                {
                    split.RemoveAt(i);
                    break;
                }
            }
            return string.Join(delimiter, split);
        }

        private string RemoveLastOccurrence(string value, string? text, string delimiter)
        {
            if (text is null)
                return "";
            if (delimiter == "")
                return text ?? "";
            List<string> split = text.Split(delimiter).ToList();
            for (int i = split.Count - 1; i >= 0; i--)
            {
                if (split[i] == value)
                {
                    split.RemoveAt(i);
                    break;
                }
            }
            return string.Join(delimiter, split);
        }

        private string RemoveAllOccurrences(string value, string? text, string delimiter)
        {
            if (text is null)
                return "";
            if (delimiter == "")
                return text ?? "";
            List<string> split = text.Split(delimiter).ToList();
            for (int i = split.Count - 1; i >= 0; i--)
            {
                if (split[i] == value)
                    split.RemoveAt(i);
            }
            return string.Join(delimiter, split);
        }
    }
}
