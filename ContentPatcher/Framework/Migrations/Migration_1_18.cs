using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 1.18.</summary>
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
            this.AddedTokens = new InvariantSet(
                ConditionType.I18n.ToString()
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ContentConfig content, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            foreach (PatchConfig patch in content.Changes.WhereNotNull())
            {
                // 1.18 adds 'TextOperations' field
                if (patch.TextOperations.Any())
                {
                    error = this.GetNounPhraseError($"using {nameof(patch.TextOperations)}");
                    return false;
                }
            }

            return true;
        }
    }
}
