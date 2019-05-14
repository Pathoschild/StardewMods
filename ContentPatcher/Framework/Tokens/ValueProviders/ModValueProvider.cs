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
    }
}
