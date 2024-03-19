using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 2.0 and Stardew Valley 1.6.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal partial class Migration_2_0 : BaseRuntimeMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_2_0()
            : base(new SemanticVersion(2, 0, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.ModId)
            );
            this.MigrationWarnings = ["Some content packs haven't been updated for Stardew Valley 1.6.0. Content Patcher will try to auto-migrate them, but compatibility isn't guaranteed."];
            this.RuntimeEditDataMigrators = [
                new BigCraftableInformationMigrator(),
                new BlueprintsMigrator(),
                new BootsMigrator(),
                new CropsMigrator(),
                new LocationsMigrator(),
                new NpcDispositionsMigrator(),
                new ObjectContextTagsMigrator(),
                new ObjectInformationMigrator(),
                new WeaponsMigrator()
            ];
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref patches, out error))
                return false;

            // 2.0 adds Priority
            foreach (PatchConfig patch in patches)
            {
                if (patch.Priority != null)
                {
                    error = this.GetNounPhraseError($"using {nameof(patch.Priority)}");
                    return false;
                }
            }

            return true;
        }
    }
}
