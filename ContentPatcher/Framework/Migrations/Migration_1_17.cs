using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 1.17.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_17 : BaseMigration
    {
        /*********
        ** Fields
        *********/
        /// <summary>A pattern which matches a location token name.</summary>
        private static readonly Regex LocationTokenPattern = new(@"\b(?:IsOutdoors|LocationName)\b", RegexOptions.Compiled);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_17()
            : base(new SemanticVersion(1, 17, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.TargetPathOnly)
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref patches, out error))
                return false;

            foreach (PatchConfig patch in patches)
            {
                // 1.17 adds 'Update' field
                if (patch.Update != null)
                {
                    error = this.GetNounPhraseError($"specifying the patch update rate ('{nameof(patch.Update)}' field)");
                    return false;
                }

                // pre-1.17 patches which used {{IsOutdoors}}/{{LocationName}} would update on location change
                // (Technically that applies to all references, but parsing tokens at this
                // point is difficult and a review of existing content packs shows that they
                // only used these as condition keys.)
                {
                    bool hasLocationToken = patch.When?.Keys.Any(key =>
                        !string.IsNullOrWhiteSpace(key)
                        && (key.ContainsIgnoreCase("IsOutdoors") || key.ContainsIgnoreCase("LocationName")) // quick check with false positives
                        && LocationTokenPattern.IsMatch(key) // slower but reliable check
                    ) == true;

                    if (hasLocationToken)
                        patch.Update = UpdateRate.OnLocationChange.ToString();
                }
            }

            return true;
        }
    }
}
