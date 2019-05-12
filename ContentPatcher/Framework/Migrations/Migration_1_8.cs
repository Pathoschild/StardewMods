using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrate patches to format version 1.8.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_8 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_8()
            : base(new SemanticVersion(1, 8, 0)) { }

        /// <summary>Migrate a content pack.</summary>
        /// <param name="content">The content pack data to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed.</param>
        /// <returns>Returns whether the content pack was successfully migrated.</returns>
        public override bool TryMigrate(ContentConfig content, out string error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            // 1.8 adds MoveEntries
            if (content.Changes != null)
            {
                foreach (PatchConfig patch in content.Changes)
                {
                    if (patch.MoveEntries?.Any() == true)
                    {
                        error = this.GetNounPhraseError($"using {nameof(PatchConfig.MoveEntries)}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
