using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.TextOperations
{
    /// <summary>The base implementation for a text operation.</summary>
    internal abstract class BaseTextOperation : ITextOperation
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying contextual values.</summary>
        protected readonly AggregateContextual Contextuals;


        /*********
        ** Accessors
        *********/
        /// <summary>The text operation to perform.</summary>
        public TextOperationType Operation { get; }

        /// <summary>The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</summary>
        public ITokenString[] Target { get; }

        /// <inheritdoc />
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <inheritdoc />
        public bool IsReady => this.Contextuals.IsReady;


        /*********
        ** Public methods
        *********/
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
        public abstract string Apply(string? text);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="operation">The text operation to perform.</param>
        /// <param name="target">The specific text field to change as a breadcrumb path. Each value in the list represents a field to navigate into.</param>
        protected BaseTextOperation(TextOperationType operation, ICollection<IManagedTokenString> target)
        {
            this.Operation = operation;
            this.Target = target.ToArray<ITokenString>();

            this.Contextuals = new AggregateContextual()
                .Add(target);
        }
    }
}
