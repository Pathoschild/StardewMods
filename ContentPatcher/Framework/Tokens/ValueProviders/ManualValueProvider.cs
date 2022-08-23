using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider whose values are set manually in code.</summary>
    internal class ManualValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether the value provider can only contain those values that are explicitly added as possible values.</summary>
        private readonly bool IsBounded;

        /// <summary>The tokens which the values use.</summary>
        private readonly MutableInvariantSet TokensUsed = new();

        /// <summary>The allowed root values (or <c>null</c> if any value is allowed).</summary>
        private MutableInvariantSet? AllowedRootValues = new();

        /// <summary>The current values.</summary>
        private IInvariantSet Values = InvariantSets.Empty;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The value provider name.</param>
        /// <param name="isBounded">Whether the value provider can only contain those values that are explicitly added as possible values.</param>
        public ManualValueProvider(string name, bool isBounded)
            : base(name, mayReturnMultipleValuesForRoot: false)
        {
            this.IsBounded = isBounded;

            this.SetReady(false); // not ready until initialized
        }

        /****
        ** Management API
        ****/
        /// <summary>Add a set of possible values.</summary>
        /// <param name="possibleValues">The possible values to add.</param>
        public void AddAllowedValues(ITokenString possibleValues)
        {
            // can't reasonably generate known values if tokens are involved
            if (possibleValues.HasAnyTokens || possibleValues.IsMutable || this.AllowedRootValues == null)
            {
                this.AllowedRootValues = null;
                this.MayReturnMultipleValuesForRoot = true;
                return;
            }

            // get possible values from literal token
            IInvariantSet splitValues = possibleValues.SplitValuesUnique();
            this.AllowedRootValues.AddMany(possibleValues.SplitValuesUnique());
            if (splitValues.Count > 1)
                this.MayReturnMultipleValuesForRoot = true;
        }

        /// <summary>Add token names which this token may depend on.</summary>
        /// <param name="tokens">The token names used.</param>
        public void AddTokensUsed(IEnumerable<string> tokens)
        {
            this.TokensUsed.AddMany(tokens);
        }

        /// <summary>Set the current values.</summary>
        /// <param name="values">The values to set.</param>
        public void SetValue(ITokenString? values)
        {
            this.Values = values.SplitValuesUnique();
        }

        /// <summary>Set the current values.</summary>
        /// <param name="values">The values to set.</param>
        public void SetValue(IInvariantSet values)
        {
            this.Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        /// <summary>Set whether the token is valid for the current context.</summary>
        /// <param name="ready">The value to set.</param>
        public void SetReady(bool ready)
        {
            this.MarkReady(ready);
        }

        /****
        ** Value provider API
        ****/
        /// <inheritdoc />
        public override IInvariantSet GetTokensUsed()
        {
            return this.TokensUsed.GetImmutable();
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
        {
            allowedValues = this.IsBounded
                ? this.AllowedRootValues?.GetImmutable()
                : null;
            return allowedValues != null;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);
            return this.Values;
        }
    }
}
