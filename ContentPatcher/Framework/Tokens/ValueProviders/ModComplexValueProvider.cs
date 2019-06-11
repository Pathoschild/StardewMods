using System;
using System.Collections.Generic;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider defined by a mod which specifies custom logic for readiness, context updates, and change tracking.</summary>
    internal class ModComplexValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>A function which updates the token value (if needed), and returns whether the token value changed.</summary>
        private readonly Func<bool> UpdateContextImpl;

        /// <summary>A function which returns whether the token is available for use. This should always be called after <see cref="UpdateContextImpl"/>.</summary>
        private readonly Func<bool> IsReadyImpl;

        /// <summary>A function which returns the current value for a given input argument (if any).</summary>
        private readonly Func<string, IEnumerable<string>> GetValueImpl;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>Pathoschild.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="updateContext">A function which updates the token value (if needed), and returns whether the token value changed.</param>
        /// <param name="isReady">A function which returns whether the token is available for use. This is always called after <paramref name="updateContext"/>.</param>
        /// <param name="getValue">A function which returns the current value for a given input argument (if any).</param>
        /// <param name="allowsInput">Whether the player can provide an input argument (see <paramref name="getValue"/>).</param>
        /// <param name="requiresInput">Whether the token can *only* be used with an input argument (see <paramref name="getValue"/>).</param>
        public ModComplexValueProvider(string name, Func<bool> isReady, Func<bool> updateContext, Func<string, IEnumerable<string>> getValue, bool allowsInput, bool requiresInput)
            : base(name, canHaveMultipleValuesForRoot: true)
        {
            this.UpdateContextImpl = updateContext;
            this.IsReadyImpl = isReady;
            this.GetValueImpl = getValue;
            if (allowsInput)
                this.EnableInputArguments(requiresInput, true);

            this.MarkReady(this.IsReadyImpl());
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            if (this.IsReady)
            {
                IEnumerable<string> values = this.GetValueImpl(input?.Value);
                if (values == null)
                    yield break;

                foreach (string value in values)
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
