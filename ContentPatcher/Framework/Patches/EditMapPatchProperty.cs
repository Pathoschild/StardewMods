using System.Collections.Generic;
using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>A map property to change when editing a map.</summary>
    internal class EditMapPatchProperty : IContextual
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying contextual values.</summary>
        private readonly AggregateContextual Contextuals;


        /*********
        ** Accessors
        *********/
        /// <summary>The map property name.</summary>
        public ITokenString Key { get; }

        /// <summary>The map property value.</summary>
        public ITokenString Value { get; }

        /// <inheritdoc />
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <inheritdoc />
        public bool IsReady => this.Contextuals.IsReady;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="key">The map property name.</param>
        /// <param name="value">The map property value.</param>
        public EditMapPatchProperty(IManagedTokenString key, IManagedTokenString value)
        {
            this.Key = key;
            this.Value = value;

            this.Contextuals = new AggregateContextual()
                .Add(key)
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
