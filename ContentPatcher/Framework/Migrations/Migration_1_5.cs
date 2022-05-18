using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrate patches to format version 1.5.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_5 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_5()
            : base(new SemanticVersion(1, 5, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.FarmCave),
                nameof(ConditionType.FarmhouseUpgrade),
                nameof(ConditionType.FarmName),
                nameof(ConditionType.HasFile),
                nameof(ConditionType.HasProfession),
                nameof(ConditionType.PlayerGender),
                nameof(ConditionType.PlayerName),
                nameof(ConditionType.PreferredPet),
                nameof(ConditionType.Year)
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ContentConfig content, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            // 1.5 adds dynamic tokens
            if (content.DynamicTokens.Any())
            {
                error = this.GetNounPhraseError($"using the {nameof(ContentConfig.DynamicTokens)} field");
                return false;
            }

            // check patch format
            foreach (PatchConfig? patch in content.Changes)
            {
                // 1.5 adds multiple Target values
                if (patch?.Target?.Contains(",") == true)
                {
                    error = this.GetNounPhraseError($"specifying multiple {nameof(PatchConfig.Target)} values");
                    return false;
                }
            }

            return true;
        }
    }
}
