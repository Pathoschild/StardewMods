using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders
{
    /// <summary>A value provider which normalizes an asset name into the form expected by the game.</summary>
    internal class FormatAssetNameValueProvider : BaseValueProvider
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public FormatAssetNameValueProvider()
            : base(ConditionType.FormatAssetName, mayReturnMultipleValuesForRoot: false)
        {
            this.EnableInputArguments(required: true, mayReturnMultipleValues: false, maxPositionalArgs: null);
            this.ValidNamedArguments.Add("separator");
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

            string path = input.GetPositionalSegment();

            if (!string.IsNullOrWhiteSpace(path))
            {
                path = PathUtilities.NormalizeAssetName(path);

                if (input.NamedArgs.TryGetValue("separator", out IInputArgumentValue separator))
                    path = path.Replace(PathUtilities.PreferredAssetSeparator.ToString(), separator.Parsed.FirstOrDefault());

                yield return path;
            }
        }
    }
}
