using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.TextOperations
{
    /// <summary>An entry in an edit patch to perform a text operation over an existing value.</summary>
    internal class TextOperation : ITextOperation
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying contextual values.</summary>
        private readonly AggregateContextual Contextuals;


        /*********
        ** Accessors
        *********/
        /// <summary>The text operation to perform.</summary>
        public TextOperationType Operation { get; set; }

        /// <summary>The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</summary>
        public ITokenString[] Target { get; set; }

        /// <summary>The value to append or prepend.</summary>
        public ITokenString Value { get; set; }

        /// <summary>If the target field already has a value, text to add between the previous and inserted values, if any.</summary>
        public string Delimiter { get; set; }

        /// <inheritdoc />
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <inheritdoc />
        public bool IsReady => this.Contextuals.IsReady;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="operation">The text operation to perform.</param>
        /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
        /// <param name="value">The value to append or prepend.</param>
        /// <param name="delimiter">If the target field already has a value, text to add between the previous and inserted values, if any.</param>
        public TextOperation(TextOperationType operation, ICollection<IManagedTokenString> target, IManagedTokenString value, string? delimiter)
        {
            this.Operation = operation;
            this.Target = target.ToArray<ITokenString>();
            this.Value = value;
            this.Delimiter = delimiter ?? string.Empty;

            this.Contextuals = new AggregateContextual()
                .Add(target)
                .Add(value);
        }

        /// <inheritdoc />
        public bool UpdateContext(IContext context)
        {
            return this.Contextuals.UpdateContext(context);
        }

        /// <inheritdoc />
        public IInvariantSet GetTokensUsed()
        {
            return this.Contextuals.GetTokensUsed();
        }

        /// <inheritdoc />
        public IContextualState GetDiagnosticState()
        {
            return this.Contextuals.GetDiagnosticState();
        }

        /// <summary>Get the first entry in the <see cref="Target"/> as an enum value.</summary>
        public TextOperationTargetRoot? GetTargetRoot()
        {
            if (this.Target.Length == 0)
                return null;

            if (!Enum.TryParse(this.Target[0].Value, true, out TextOperationTargetRoot root))
                return null;

            return root;
        }

        /// <summary>Get a copy of the input with the text operation applied.</summary>
        /// <param name="text">The input to modify.</param>
        public string Apply(string? text)
        {
            string? value = this.Value.Value;
            if (value is null)
                return text ?? "";

            string delimiter = string.IsNullOrEmpty(text)
                ? ""
                : this.Delimiter;

            return this.Operation switch
            {
                TextOperationType.Append => this.Append(value, text, delimiter),
                TextOperationType.Prepend => this.Prepend(value, text, delimiter),
                TextOperationType.RemoveFirstOccurrence => this.RemoveFirstOccurrence(value, text, delimiter),
                TextOperationType.RemoveLastOccurrence => this.RemoveLastOccurrence(value, text, delimiter),
                TextOperationType.RemoveAllOccurrences => this.RemoveAllOccurrences(value, text, delimiter),
                _ => throw new InvalidOperationException($"Unknown text operation type '{this.Operation}'.")
            };
        }

        private string Append(string value, string? text, string delimiter)
        {
            return text + delimiter + value;
        }

        private string Prepend(string value, string? text, string delimiter)
        {
            return value + delimiter + text;
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
