using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations
{
    /// <summary>A text operation which parses a field's current value as a delimited list of values, and replaces those matching a search value with a provided value.</summary>
    internal class ReplaceDelimitedTextOperation : BaseDelimitedTextOperation
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The text with which to replace the matched value.</summary>
        public ITokenString ReplaceWith { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
        /// <param name="search">The value to match.</param>
        /// <param name="replaceWith">The text with which to replace the matched value.</param>
        /// <param name="delimiter">The text between values in a delimited string.</param>
        /// <param name="replaceMode">Which delimited values should be removed.</param>
        public ReplaceDelimitedTextOperation(ICollection<IManagedTokenString> target, IManagedTokenString search, IManagedTokenString replaceWith, string delimiter, TextOperationReplaceMode replaceMode)
            : base(TextOperationType.ReplaceDelimited, target, search, delimiter, replaceMode)
        {
            this.ReplaceWith = replaceWith;

            this.Contextuals.Add(replaceWith);
        }


        /*********
        ** Protected methods
        *********/
        /// <inheritdoc />
        protected override void ApplyToValue(IReadOnlyList<string> values, Lazy<IList<string>> mutableValues, int index)
        {
            mutableValues.Value[index] = this.ReplaceWith.Value ?? "";
        }
    }
}
