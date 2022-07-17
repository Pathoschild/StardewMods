using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations
{
    /// <summary>A text operation which appends or prepends text to a field's current value.</summary>
    internal class AppendOrPrependTextOperation : BaseTextOperation
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
        public AppendOrPrependTextOperation(TextOperationType operation, ICollection<IManagedTokenString> target, IManagedTokenString value, string? delimiter)
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
                TextOperationType.Append => text + delimiter + value,
                TextOperationType.Prepend => value + delimiter + text,
                _ => throw new InvalidOperationException($"Unknown text operation type '{this.Operation}'.")
            };
        }
    }
}
