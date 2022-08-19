using System.Collections.Generic;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations
{
    /// <summary>A text operation which parses a field's current value as a delimited list of values, and removes those matching a search value.</summary>
    internal class RemoveDelimitedTextOperation : BaseTextOperation
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The value to remove from the text.</summary>
        public ITokenString Search { get; }

        /// <summary>The text between values in a delimited string.</summary>
        public string Delimiter { get; }

        /// <summary>Which delimited values should be removed.</summary>
        public TextOperationReplaceMode ReplaceMode { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="operation">The text operation to perform.</param>
        /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
        /// <param name="search">The value to remove from the text.</param>
        /// <param name="delimiter">The text between values in a delimited string.</param>
        /// <param name="replaceMode">Which delimited values should be removed.</param>
        public RemoveDelimitedTextOperation(TextOperationType operation, ICollection<IManagedTokenString> target, IManagedTokenString search, string delimiter, TextOperationReplaceMode replaceMode)
            : base(operation, target)
        {
            this.Search = search;
            this.Delimiter = delimiter;
            this.ReplaceMode = replaceMode;

            this.Contextuals.Add(search);
        }

        /// <inheritdoc />
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
            bool replaced = false;
            {
                int prevCount = values.Count;
                switch (this.ReplaceMode)
                {
                    case TextOperationReplaceMode.First:
                        for (int i = 0; i < prevCount; i++)
                        {
                            if (values[i] == search)
                            {
                                List<string> modified = new(values);
                                modified.RemoveAt(i);
                                values = modified;

                                replaced = true;
                                break;
                            }
                        }
                        break;

                    case TextOperationReplaceMode.Last:
                        for (int i = prevCount - 1; i >= 0; i--)
                        {
                            if (values[i] == search)
                            {
                                List<string> modified = new(values);
                                modified.RemoveAt(i);
                                values = modified;

                                replaced = true;
                                break;
                            }
                        }
                        break;

                    case TextOperationReplaceMode.All:
                        {
                            List<string>? modified = null;

                            for (int i = prevCount - 1; i >= 0; i--)
                            {
                                if (values[i] == search)
                                {
                                    modified ??= new List<string>(values);
                                    modified.RemoveAt(i);

                                    replaced = true;
                                }
                            }

                            if (modified is not null)
                                values = modified;
                        }
                        break;
                }
            }

            // update field
            return replaced
                ? string.Join(this.Delimiter, values)
                : text;
        }
    }
}
