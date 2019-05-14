using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    internal class ModValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The function providing values.</summary>
        private readonly Func<ITokenString, string[]> ValueFunction;

        /// <summary>The possible resulting values for this token.</summary>
        private HashSet<string> Values = new HashSet<string>();

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name"> The value provider name.</param>
        /// <param name="valueFunction">The function providing values.</param>
        public ModValueProvider(string name, Func<ITokenString, string[]> valueFunction)
        :   base(name, true)
        {
            this.ValueFunction = valueFunction;
            this.EnableInputArguments(true, true);
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(IManagedTokenString input)
        {
            this.AssertInputArgument(input);

            foreach (string value in this.ValueFunction(input))
                yield return value;
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(this.Values, () =>
            {
                this.Values = new HashSet<string>(this.ValueFunction(null));
                this.IsReady = this.Values.Count > 0;
            });
        }
    }
}
