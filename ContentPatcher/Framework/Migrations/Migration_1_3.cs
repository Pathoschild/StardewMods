using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrate patches to format version 1.3.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_3 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_3()
            : base(new SemanticVersion(1, 3, 0)) { }

        /// <inheritdoc />
        public override bool TryMigrate(ContentConfig content, out string error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            // 1.3 adds config.json
            if (content.ConfigSchema.Any())
            {
                error = this.GetNounPhraseError($"using the {nameof(ContentConfig.ConfigSchema)} field");
                return false;
            }

            // check patch format
            foreach (PatchConfig patch in content.Changes)
            {
                // 1.3 adds tokens in FromFile
                if (patch.FromFile != null && patch.FromFile.Contains("{{"))
                {
                    error = this.GetNounPhraseError($"using the {{{{token}}}} feature in {nameof(PatchConfig.FromFile)} fields");
                    return false;
                }

                // 1.3 adds When
                if (patch.When.Any())
                {
                    error = this.GetNounPhraseError($"using the condition feature ({nameof(ContentConfig.Changes)}.{nameof(PatchConfig.When)} field)");
                    return false;
                }
            }

            return true;
        }
    }
}
