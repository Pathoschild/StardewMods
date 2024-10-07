using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations
{
    /// <summary>A text operation which parses a field's current value as a delimited list of values, and performs an action for those matching a search value.</summary>
    internal abstract class BaseDelimitedTextOperation : BaseTextOperation
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The value to match.</summary>
        public ITokenString Search { get; }

        /// <summary>The text between values in a delimited string.</summary>
        public string Delimiter { get; }

        /// <summary>Which delimited values the action should be applied to.</summary>
        public TextOperationReplaceMode ReplaceMode { get; }


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        [return: NotNullIfNotNull(nameof(text))]
        public override string? Apply(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // get search
            string? search = this.Search.Value;
            if (search is null)
                return text;

            // apply
            IReadOnlyList<string> values = text.Split(this.Delimiter);
            var mutableValues = new Lazy<IList<string>>(() => new List<string>(values));
            {
                int prevCount = values.Count;
                switch (this.ReplaceMode)
                {
                    case TextOperationReplaceMode.First:
                        for (int i = 0; i < prevCount; i++)
                        {
                            if (values[i] == search)
                            {
                                this.ApplyToValue(values, mutableValues, i);
                                break;
                            }
                        }
                        break;

                    case TextOperationReplaceMode.Last:
                        for (int i = prevCount - 1; i >= 0; i--)
                        {
                            if (values[i] == search)
                            {
                                this.ApplyToValue(values, mutableValues, i);
                                break;
                            }
                        }
                        break;

                    case TextOperationReplaceMode.All:
                        for (int i = prevCount - 1; i >= 0; i--)
                        {
                            if (values[i] == search)
                                this.ApplyToValue(values, mutableValues, i);
                        }
                        break;
                }
            }

            // update field
            return mutableValues.IsValueCreated
                ? string.Join(this.Delimiter, mutableValues.Value)
                : text;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="operation">The text operation to perform.</param>
        /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
        /// <param name="search">The value to match.</param>
        /// <param name="delimiter">The text between values in a delimited string.</param>
        /// <param name="replaceMode">Which delimited values the action should be applied to.</param>
        protected BaseDelimitedTextOperation(TextOperationType operation, ICollection<IManagedTokenString> target, IManagedTokenString search, string delimiter, TextOperationReplaceMode replaceMode)
            : base(operation, target)
        {
            this.Search = search;
            this.Delimiter = delimiter;
            this.ReplaceMode = replaceMode;

            this.Contextuals.Add(search);
        }

        /// <summary>Apply the operation to a matched delimited value.</summary>
        /// <param name="values">The original values before this operation was applied.</param>
        /// <param name="mutableValues">The modified values. Accessing the value will mark it modified.</param>
        /// <param name="index">The index of the delimited value that was matched in <paramref name="values"/>. When applying a <see cref="TextOperationReplaceMode.All"/> operation, the list is iterated in reverse order, so you can safely remove values without offsetting the index.</param>
        protected abstract void ApplyToValue(IReadOnlyList<string> values, Lazy<IList<string>> mutableValues, int index);
    }
}
