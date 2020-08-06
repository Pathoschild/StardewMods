using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token wrapped by Content Patcher to handle global input arguments.</summary>
    internal class HigherLevelTokenWrapper : DelegatingToken, IHigherLevelToken
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="token">The wrapped token instance.</param>
        public HigherLevelTokenWrapper(IToken token)
            : base(token) { }

        /// <summary>Construct an instance.</summary>
        /// <param name="provider">The underlying value provider.</param>
        /// <param name="scope">The mod namespace in which the token is accessible, or <c>null</c> for any namespace.</param>
        public HigherLevelTokenWrapper(IValueProvider provider, string scope = null)
            : base(new GenericToken(provider, scope)) { }

        /// <inheritdoc />
        public override bool CanHaveMultipleValues(IInputArguments input)
        {
            // 'contains' limited to true/false
            if (input.ReservedArgs.ContainsKey(InputArguments.ContainsKey))
                return false;

            // default logic
            return base.CanHaveMultipleValues(input);
        }

        /// <inheritdoc />
        public override bool TryValidateValues(IInputArguments input, InvariantHashSet values, IContext context, out string error)
        {
            // 'contains' limited to true/false
            if (input.ReservedArgs.ContainsKey(InputArguments.ContainsKey))
            {
                if (!base.TryValidateInput(input, out error))
                    return false;

                string[] invalidValues = values
                    .Where(p => !bool.TryParse(p, out bool _))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                if (invalidValues.Any())
                {
                    error = $"invalid values ({string.Join(", ", invalidValues)}); expected 'true' or 'false' when used with 'contains'.";
                    return false;
                }

                error = null;
                return true;
            }

            return base.TryValidateValues(input, values, context, out error);
        }

        /// <inheritdoc />
        public override bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
        {
            // 'contains' limited to true/false
            if (input.ReservedArgs.ContainsKey(InputArguments.ContainsKey))
            {
                min = -1;
                max = -1;
                return false;
            }

            // default logic
            return base.HasBoundedRangeValues(input, out min, out max);
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            // 'contains' limited to true/false
            if (input.ReservedArgs.ContainsKey(InputArguments.ContainsKey))
            {
                allowedValues = InvariantHashSet.Boolean();
                return true;
            }

            // default logic
            return base.HasBoundedValues(input, out allowedValues);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            var values = base.GetValues(input);

            // contains
            if (input.ReservedArgs.TryGetValue(InputArguments.ContainsKey, out IInputArgumentValue rawSearch))
            {
                InvariantHashSet search = new InvariantHashSet(rawSearch.Parsed);
                bool match = search.Any() && values.Any(value => search.Contains(value));
                return new[] { match.ToString() };
            }

            return values;
        }
    }
}
