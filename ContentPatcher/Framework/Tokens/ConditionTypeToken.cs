using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token for a built-in condition whose value may change with the context.</summary>
    internal class ConditionTypeToken : BaseToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>Get the current token values.</summary>
        private readonly Func<InvariantHashSet> FetchValues;

        /// <summary>Whether a save must be loaded for the token to be available.</summary>
        private readonly bool NeedsLoadedSave;

        /// <summary>The token values as of the last context update.</summary>
        private InvariantHashSet Values;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token values.</param>
        /// <param name="values">Get the current token values.</param>
        /// <param name="needsLoadedSave">Whether a save must be loaded for the token to be available.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        public ConditionTypeToken(ConditionType name, Func<IEnumerable<string>> values, bool needsLoadedSave, IEnumerable<string> allowedValues = null)
            : base(name.ToString(), canHaveMultipleValues: true, requiresSubkeys: false, allowedValues: allowedValues)
        {
            this.NeedsLoadedSave = needsLoadedSave;
            this.FetchValues = () => new InvariantHashSet(values());
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token values.</param>
        /// <param name="value">Get the current token value.</param>
        /// <param name="needsLoadedSave">Whether a save must be loaded for the token to be available.</param>
        /// <param name="allowedValues">The allowed values (or <c>null</c> if any value is allowed).</param>
        public ConditionTypeToken(ConditionType name, Func<string> value, bool needsLoadedSave, IEnumerable<string> allowedValues = null)
            : base(name.ToString(), canHaveMultipleValues: false, requiresSubkeys: false, allowedValues: allowedValues)
        {
            this.NeedsLoadedSave = needsLoadedSave;
            this.FetchValues = () => new InvariantHashSet { value() };
        }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        public override void UpdateContext(IContext context)
        {
            this.IsValidInContext = this.NeedsLoadedSave && Context.IsWorldReady;
            this.Values = this.IsValidInContext ? this.FetchValues() : null;
        }

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check, if applicable.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.RequiresSubkeys"/>.</exception>
        public override IEnumerable<string> GetValues(TokenName? name = null)
        {
            this.AssertTokenName(name);

            if (this.Values != null)
                return this.Values;
            return Enumerable.Empty<string>();
        }
    }
}
