using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which exists only to satisfy token validation.</summary>
    internal class ForValidationValueProvider : BaseValueProvider
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="condType">The condition type this value provider is for.</param>
        public ForValidationValueProvider(ConditionType condType)
            : base(condType, canHaveMultipleValuesForRoot: false)
        {
            this.IsReady = true;
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            return false;
        }

        /// <summary>Get the allowed values for an input argument (or <c>null</c> if any value is allowed).</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override InvariantHashSet GetAllowedValues(ITokenString input)
        {
            return new InvariantHashSet();
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            yield break;
        }
    }
}
