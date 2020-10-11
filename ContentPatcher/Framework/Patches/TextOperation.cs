using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>An entry in an edit patch to perform a text operation over an existing value.</summary>
    internal class TextOperation : IContextual
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

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Contextuals.IsReady;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="operation">The text operation to perform.</param>
        /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
        /// <param name="value">The value to append or prepend.</param>
        /// <param name="delimiter">If the target field already has a value, text to add between the previous and inserted values, if any.</param>
        public TextOperation(TextOperationType operation, IManagedTokenString[] target, IManagedTokenString value, string delimiter)
        {
            this.Operation = operation;
            this.Target = target.Cast<ITokenString>().ToArray();
            this.Value = value;
            this.Delimiter = delimiter;

            this.Contextuals = new AggregateContextual()
                .Add(target)
                .Add(value);
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            return this.Contextuals.UpdateContext(context);
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            return this.Contextuals.GetTokensUsed();
        }

        /// <summary>Get diagnostic info about the contextual instance.</summary>
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
        public string Apply(string text)
        {
            string delimiter = string.IsNullOrEmpty(text)
                ? ""
                : this.Delimiter;

            return this.Operation switch
            {
                TextOperationType.Append => text + delimiter + this.Value.Value,
                TextOperationType.Prepend => this.Value.Value + delimiter + text,
                _ => throw new InvalidOperationException($"Unknown text operation type '{this.Operation}'.")
            };
        }
    }
}
