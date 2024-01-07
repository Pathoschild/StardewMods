using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 2.0.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_2_0 : BaseMigration
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
