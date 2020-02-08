using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which changes the letter case for input text.</summary>
    internal class LetterCaseValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The token type.</summary>
        private readonly ConditionType Type;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The condition type. This must be one of <see cref="ConditionType.Lowercase"/> or <see cref="ConditionType.Uppercase"/>.</param>
        public LetterCaseValueProvider(ConditionType type)
            : base(type, canHaveMultipleValuesForRoot: false)
        {
            if (type != ConditionType.Lowercase && type != ConditionType.Uppercase)
                throw new ArgumentException($"The {nameof(type)} must be one of {ConditionType.Lowercase} or {ConditionType.Uppercase}.", nameof(type));

            this.Type = type;
            this.EnableInputArguments(required: true, canHaveMultipleValues: false);
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the instance changed.</returns>
        public override bool UpdateContext(IContext context)
        {
            bool changed = !this.IsReady;
            this.MarkReady(true);
            return changed;
        }

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="IValueProvider.HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override bool HasBoundedValues(ITokenString input, out InvariantHashSet allowedValues)
        {
            allowedValues = new InvariantHashSet(this.GetValues(input));
            return true;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IValueProvider.AllowsInput"/> or <see cref="IValueProvider.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
        {
            this.AssertInputArgument(input);

            switch (this.Type)
            {
                case ConditionType.Lowercase:
                    yield return input.Value.ToLowerInvariant();
                    break;

                case ConditionType.Uppercase:
                    yield return input.Value.ToUpperInvariant();
                    break;

                default:
                    throw new NotSupportedException($"Unimplemented letter case type '{this.Type}'."); // should never happen
            }
        }
    }
}
