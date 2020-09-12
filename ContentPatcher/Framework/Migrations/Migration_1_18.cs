using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Validates patches for format version 1.18.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_18 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_18()
            : base(new SemanticVersion(1, 18, 0))
        {
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.I18n.ToString()
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
                    // 1.18 adds 'TextOperations' field
                    if (patch.TextOperations != null)
                    {
                        error = this.GetNounPhraseError($"using {nameof(patch.TextOperations)}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
