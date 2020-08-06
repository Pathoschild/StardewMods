using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A conditional value for a dynamic token.</summary>
    internal class DynamicTokenValue : IContextual
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying contextual values.</summary>
        private readonly AggregateContextual Contextuals;


        /*********
        ** Accessors
        *********/
        /// <summary>The token for which this value is registered.</summary>
        public ManagedManualToken ParentToken { get; }

        /// <summary>The name of the token whose value to set.</summary>
        public string Name => this.ParentToken.ValueProvider.Name;

        /// <summary>The token value to set.</summary>
        public ITokenString Value { get; }

        /// <summary>The conditions that must match to set this value.</summary>
        public Condition[] Conditions { get; }

        /// <inheritdoc />
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <inheritdoc />
        public bool IsReady => this.Contextuals.IsReady;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="parentToken">The token whose value to set.</param>
        /// <param name="value">The token value to set.</param>
        /// <param name="conditions">The conditions that must match to set this value.</param>
        public DynamicTokenValue(ManagedManualToken parentToken, IManagedTokenString value, IEnumerable<Condition> conditions)
        {
            this.ParentToken = parentToken;
            this.Value = value;
            this.Conditions = conditions.ToArray();
            this.Contextuals = new AggregateContextual()
                .Add(value)
                .Add(this.Conditions);
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
