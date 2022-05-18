using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which gets the internal asset key for a content pack file, to allow loading it directly through a content manager.</summary>
    internal class InternalAssetKeyValueProvider : BaseValueProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get the internal asset key for a relative path.</summary>
        private readonly Func<string, IAssetName> GetInternalAssetKey;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="getInternalAssetKey">Get the internal asset key for a relative path.</param>
        public InternalAssetKeyValueProvider(Func<string, IAssetName> getInternalAssetKey)
            : base(ConditionType.InternalAssetKey, mayReturnMultipleValuesForRoot: false)
        {
            this.GetInternalAssetKey = getInternalAssetKey;

            this.EnableInputArguments(required: true, mayReturnMultipleValues: false, maxPositionalArgs: null);
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            bool changed = !this.IsReady;
            this.MarkReady(true);
            return changed;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            this.AssertInput(input);

            string? path = input.GetPositionalSegment();

            return !string.IsNullOrWhiteSpace(path)
                ? InvariantSets.FromValue(this.GetInternalAssetKey(path).Name)
                : InvariantSets.Empty;
        }
    }
}
