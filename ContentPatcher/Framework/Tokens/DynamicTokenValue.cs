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
        /// <summary>The name of the token whose value to set.</summary>
        public string Name { get; }

        /// <summary>The token value to set.</summary>
        public ITokenString Value { get; }

        /// <summary>The conditions that must match to set this value.</summary>
        public Condition[] Conditions { get; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Contextuals.IsReady;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The name of the token whose value to set.</param>
        /// <param name="value">The token value to set.</param>
        /// <param name="conditions">The conditions that must match to set this value.</param>
        public DynamicTokenValue(string name, ITokenString value, IEnumerable<Condition> conditions)
        {
            this.Name = name;
            this.Value = value;
            this.Conditions = conditions.ToArray();
            this.Contextuals = new AggregateContextual()
                .Add(this.Value)
                .Add(this.Conditions);
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
    }
}
