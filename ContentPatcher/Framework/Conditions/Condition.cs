using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>A condition that can be checked against the token context.</summary>
    internal class Condition : IContextual
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The token name in the context.</summary>
        public string Name { get; }

        /// <summary>The token input argument, if any.</summary>
        public IManagedTokenString Input { get; }

        /// <summary>The token values for which this condition is valid.</summary>
        public IManagedTokenString Values { get; }

        /// <summary>The current values from <see cref="Values"/>.</summary>
        public InvariantHashSet CurrentValues { get; private set; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.Values.IsMutable || this.Input?.IsMutable == true;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Values.IsReady && this.Input?.IsReady != false;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name in the context.</param>
        /// <param name="input">The token input argument, if any.</param>
        /// <param name="values">The token values for which this condition is valid.</param>
        public Condition(string name, IManagedTokenString input, IManagedTokenString values)
        {
            this.Name = name;
            this.Input = input;
            this.Values = values;

            if (this.Values.IsReady)
                this.CurrentValues = this.Values.SplitValues();
        }

        /// <summary>Whether the condition matches.</summary>
        /// <param name="context">The condition context.</param>
        public bool IsMatch(IContext context)
        {
            if (!this.IsReady)
                return false;

            return context
                .GetValues(this.Name, this.Input, enforceContext: true)
                .Any(value => this.CurrentValues.Contains(value));
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            bool changed = false;

            if (this.Values.UpdateContext(context))
            {
                changed = true;
                this.CurrentValues = this.Values.SplitValues();
            }
            if (this.Input?.UpdateContext(context) == true)
                changed = true;


            return changed;
        }

        /// <summary>Get the token names used by this patch in its fields.</summary>
        public IEnumerable<string> GetTokensUsed()
        {
            // name
            yield return this.Name;

            // from input
            if (this.Input != null)
            {
                foreach (string token in this.Input.GetTokensUsed())
                    yield return token;
            }

            // from values
            foreach (string token in this.Values.GetTokensUsed())
                yield return token;
        }
    }
}
