using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrate patches to format version 1.13.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_13 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_13()
            : base(new SemanticVersion(1, 13, 0))
        {
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.Query.ToString()
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
                    // 1.13 adds map tile patches
                    if (patch.MapTiles != null)
                    {
                        error = this.GetNounPhraseError($"using {nameof(PatchConfig.MapTiles)}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
