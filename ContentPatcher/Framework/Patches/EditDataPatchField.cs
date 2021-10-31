using System.Collections.Generic;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>A specific field in a data file to change.</summary>
    internal class EditDataPatchField : IContextual
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying contextual values.</summary>
        private readonly AggregateContextual Contextuals;


        /*********
        ** Accessors
        *********/
        /// <summary>The unique key for the entry in the data file.</summary>
        public ITokenString EntryKey { get; }

        /// <summary>The field index to change.</summary>
        public ITokenString FieldKey { get; }

        /// <summary>The entry value to set.</summary>
        public TokenizableJToken Value { get; }

        /// <inheritdoc />
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <inheritdoc />
        public bool IsReady => this.Contextuals.IsReady;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="entryKey">The unique key for the entry in the data file.</param>
        /// <param name="fieldKey">The field number to change.</param>
        /// <param name="value">The entry value to set.</param>
        public EditDataPatchField(IManagedTokenString entryKey, IManagedTokenString fieldKey, TokenizableJToken value)
        {
            this.EntryKey = entryKey;
            this.FieldKey = fieldKey;
            this.Value = value;

            this.Contextuals = new AggregateContextual()
                .Add(entryKey)
                .Add(fieldKey)
                .Add(value);
        }

        /// <inheritdoc />
        public bool UpdateContext(IContext context)
        {
            return this.Contextuals.UpdateContext(context);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetTokensUsed()
        {
            return this.Contextuals.GetTokensUsed();
        }

        /// <inheritdoc />
        public IContextualState GetDiagnosticState()
        {
            return this.Contextuals.GetDiagnosticState();
        }
    }
}
