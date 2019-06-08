using System;
using System.Collections.Generic;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider defined by a mod.</summary>
    internal class ModValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get whether the token is valid in the current context.</summary>
        private readonly Func<bool> IsReadyImpl;

        /// <summary>Update the token if needed, and return <c>true</c> if the token changed (so any tokens using it should be rechecked).</summary>
        private readonly Func<bool> UpdateContextImpl;

        /// <summary>Get the current token value for a given input argument. If this returns <c>null</c> (not an empty string), the token will be marked unavailable in the current context.</summary>
        private readonly Func<string, IEnumerable<string>> GetValueImpl;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name"> The value provider name.</param>
        /// <param name="isReady">Get whether the token is valid in the current context.</param>
        /// <param name="updateContext">Update the token if needed, and return <c>true</c> if the token changed (so any tokens using it should be rechecked).</param>
        /// <param name="getValue">Get the current token value for a given input argument. If this returns <c>null</c> (not an empty string), the token will be marked unavailable in the current context.</param>
        /// <param name="allowsInput">Whether the value provider allows an input argument.</param>
        /// <param name="requiresInput">Whether an input argument is required when using this value provider.</param>
        public ModValueProvider(string name, Func<bool> isReady, Func<bool> updateContext, Func<string, IEnumerable<string>> getValue, bool allowsInput, bool requiresInput)
            : base(name, canHaveMultipleValuesForRoot: true)
        {
            this.IsReadyImpl = isReady;
            this.UpdateContextImpl = updateContext;
            this.GetValueImpl = getValue;
            if (allowsInput)
                this.EnableInputArguments(requiresInput, true);
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            if (this.IsReady)
            {
                foreach (string value in this.GetValueImpl(input.Value))
                    yield return value;
            }
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            return this.IsChanged(() =>
            {
                bool changed = this.UpdateContextImpl();
                this.MarkReady(this.IsReadyImpl());
                return changed;
            });
        }
    }
}
