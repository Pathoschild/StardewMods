using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations
{
    /// <summary>A text operation which parses a field's current value as a delimited list of values, and removes those matching a search value.</summary>
    internal class RemoveDelimitedTextOperation : BaseDelimitedTextOperation
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
        /// <param name="search">The value to remove from the text.</param>
        /// <param name="delimiter">The text between values in a delimited string.</param>
        /// <param name="replaceMode">Which delimited values should be removed.</param>
        public RemoveDelimitedTextOperation(ICollection<IManagedTokenString> target, IManagedTokenString search, string delimiter, TextOperationReplaceMode replaceMode)
            : base(TextOperationType.RemoveDelimited, target, search, delimiter, replaceMode) { }


        /*********
        ** Protected methods
        *********/
        /// <inheritdoc />
        protected override void ApplyToValue(IReadOnlyList<string> values, Lazy<IList<string>> mutableValues, int index)
        {
            mutableValues.Value.RemoveAt(index);
        }
    }
}
