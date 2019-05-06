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
        public ITokenString Input { get; }

        /// <summary>The token values for which this condition is valid.</summary>
        public InvariantHashSet Values { get; }

        /// <summary>Whether the instance may change depending on the context.</summary>
        public bool IsMutable => this.Input?.IsMutable ?? true;

        /// <summary>Whether the instance is valid for the current context.</summary>
        public bool IsReady => this.Input?.IsReady ?? true;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name in the context.</param>
        /// <param name="input">The token input argument, if any.</param>
        /// <param name="values">The token values for which this condition is valid.</param>
        public Condition(string name, ITokenString input, InvariantHashSet values)
        {
            this.Name = name;
            this.Input = input;
            this.Values = values;
        }

        /// <summary>Whether the condition matches.</summary>
        /// <param name="context">The condition context.</param>
        public bool IsMatch(IContext context)
        {
            if (!this.IsReady)
                return false;

            return context
                .GetValues(this.Name, this.Input, enforceContext: true)
                .Any(value => this.Values.Contains(value));
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext(IContext context)
        {
            return this.Input?.UpdateContext(context) ?? false;
        }
    }
}
