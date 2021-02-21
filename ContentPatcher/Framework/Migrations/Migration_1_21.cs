using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Validates patches for format version 1.21.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_21 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_21()
            : base(new SemanticVersion(1, 21, 0))
        {
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.Render.ToString()
            };
        }

        /// <inheritdoc />
        public override bool TryMigrate(ContentConfig content, out string error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            // 1.21 adds CustomLocations
            if (content.CustomLocations.Any())
            {
                error = this.GetNounPhraseError($"using the {nameof(content.CustomLocations)} field");
                return false;
            }

            // 1.21 adds AddWarps
            foreach (PatchConfig patch in content.Changes)
            {
                if (patch.AddWarps.Any())
                {
                    error = this.GetNounPhraseError($"using the {nameof(patch.AddWarps)} field");
                    return false;
                }
            }

            return true;
        }
    }
}
