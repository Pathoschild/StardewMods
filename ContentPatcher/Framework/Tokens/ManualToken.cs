using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A tokens whose values must be changed manually.</summary>
    internal class ManualToken : GenericToken
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="values">Get the current token values.</param>
        /// <param name="scope">The mod namespace in which the token is accessible, or <c>null</c> for any namespace.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        /// <param name="canHaveMultipleValues">Whether the root may contain multiple values (or <c>null</c> to set it based on the given values).</param>
        public ManualToken(string name, InvariantHashSet values, string scope = null, InvariantHashSet allowedValues = null, bool? canHaveMultipleValues = null)
            : base(new ManualValueProvider(name, values, allowedValues, canHaveMultipleValues), scope) { }

        /// <summary>Update the values of the token.</summary>
        /// <param name="values">The new values of the token.</param>
        public void UpdateValues(InvariantHashSet values)
        {
            (this.Values as ManualValueProvider).UpdateValues(values);
        }
    }
}
