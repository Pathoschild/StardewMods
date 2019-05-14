using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A conditional value for a dynamic token.</summary>
    internal class DynamicTokenValue : IContextual
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The name of the token whose value to set.</summary>
        public string Name { get; }

        /// <summary>The token value to set.</summary>
        public IManagedTokenString Value { get; }

        /// <summary>The conditions that must match to set this value.</summary>
        public Condition[] Conditions { get; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.Value.IsMutable || this.Conditions.Any(p => p.IsMutable);

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Value.IsReady && this.Conditions.All(p => p.IsReady);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The name of the token whose value to set.</param>
        /// <param name="value">The token value to set.</param>
        /// <param name="conditions">The conditions that must match to set this value.</param>
        public DynamicTokenValue(string name, IManagedTokenString value, IEnumerable<Condition> conditions)
        {
            this.Name = name;
            this.Value = value;
            this.Conditions = conditions.ToArray();
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            bool changed = false;

            foreach (IContextual value in this.Conditions)
            {
                if (value.UpdateContext(context))
                    changed = true;
            }

            if (this.Value.UpdateContext(context))
                changed = true;

            return changed;
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            // value
            foreach (string token in this.Value.GetTokensUsed())
                yield return token;

            // conditions
            foreach (string token in this.Conditions.SelectMany(p => p.GetTokensUsed()))
                yield return token;
        }
    }
}
