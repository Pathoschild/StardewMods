using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Validates patches for format version 1.17.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_17 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_17()
            : base(new SemanticVersion(1, 17, 0))
        {
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.TargetPathOnly.ToString()
            };
        }

        /// <summary>Migrate a content pack.</summary>
        /// <param name="content">The content pack data to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed.</param>
        /// <returns>Returns whether the content pack was successfully migrated.</returns>
        public override bool TryMigrate(ContentConfig content, out string error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            if (content.Changes?.Any() == true)
            {
                foreach (PatchConfig patch in content.Changes)
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
                    if (patch.When?.Any() == true)
                    {
                        bool hasLocationToken = patch.When.Keys.Any(key =>
                            !string.IsNullOrWhiteSpace(key)
                            && (key.ContainsIgnoreCase("IsOutdoors") || key.ContainsIgnoreCase("LocationName")) // quick check with false positives
                            && Regex.IsMatch(key, @"\b(?:IsOutdoors|LocationName)\b") // slower but reliable check
                        );

                        if (hasLocationToken)
                            patch.Update = UpdateRate.OnLocationChange.ToString();
                    }
                }
            }

            return true;
        }
    }
}
