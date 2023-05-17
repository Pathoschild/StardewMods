using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 1.10.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_10 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_10()
            : base(new SemanticVersion(1, 10, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.HasDialogueAnswer),
                nameof(ConditionType.HavingChild),
                nameof(ConditionType.IsJojaMartComplete),
                nameof(ConditionType.Pregnant),
                nameof(ConditionType.Random)
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref patches, out error))
                return false;

            foreach (PatchConfig patch in patches)
            {
                // 1.10 allows 'FromFile' with 'EditData' patches
                if (patch.FromFile != null && this.HasAction(patch, PatchType.EditData))
                {
                    error = this.GetNounPhraseError($"using {nameof(PatchConfig.FromFile)} with action {nameof(PatchType.EditData)}");
                    return false;
                }

                // 1.10 adds MapProperties
                if (patch.MapProperties.Any())
                {
                    error = this.GetNounPhraseError($"using {nameof(PatchConfig.MapProperties)}");
                    return false;
                }
            }

            return true;
        }
    }
}
