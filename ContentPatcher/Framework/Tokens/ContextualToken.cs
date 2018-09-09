using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token for a built-in condition whose value may change with the context.</summary>
    internal class ContextualToken : BaseToken
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
        /// <param name="getValues">Get the current token values.</param>
        /// <param name="needsLoadedSave">Whether a save must be loaded for the token to be available.</param>
        public ContextualToken(string name, Func<IEnumerable<string>> getValues, bool needsLoadedSave)
            : base(name, canHaveMultipleValues: true, requiresSubkeys: false)
        {
            this.NeedsLoadedSave = needsLoadedSave;
            this.FetchValues = () => new InvariantHashSet(getValues());
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token values.</param>
        /// <param name="getValue">Get the current token value.</param>
        /// <param name="needsLoadedSave">Whether a save must be loaded for the token to be available.</param>
        public ContextualToken(string name, Func<string> getValue, bool needsLoadedSave)
            : base(name, canHaveMultipleValues: false, requiresSubkeys: false)
        {
            this.NeedsLoadedSave = needsLoadedSave;
            this.FetchValues = () => new InvariantHashSet { getValue() };
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
        public override IEnumerable<string> GetValues()
        {
            if (this.Values != null)
                return this.Values;
            return new string[0];
        }
    }
}
