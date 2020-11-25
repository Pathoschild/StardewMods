using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Validates patches for format version 1.16.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_16 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_16()
            : base(new SemanticVersion(1, 16, 0)) { }

        /// <inheritdoc />
        public override bool TryMigrate(ContentConfig content, out string error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            foreach (PatchConfig patch in content.Changes)
            {
                // 1.16 adds Include
                if (this.GetAction(patch) == PatchType.Include)
                {
                    error = this.GetNounPhraseError($"using action {nameof(PatchType.Include)}");
                    return false;
                }
            }

            return true;
        }
    }
}
