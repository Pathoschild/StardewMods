using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>Wraps registered tokens to handle reserved arguments.</summary>
    internal class HigherLevelTokenWrapper : DelegatingToken
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="token">The wrapped token instance.</param>
        public HigherLevelTokenWrapper(IToken token)
            : base(token) { }

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
