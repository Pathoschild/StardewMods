using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrate patches to format version 1.6.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_6 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_6()
            : base(new SemanticVersion(1, 6, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.HasWalletItem),
                nameof(ConditionType.SkillLevel)
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ContentConfig content, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            // before 1.6, the 'sun' weather included 'wind'
            foreach (PatchConfig patch in content.Changes.WhereNotNull())
            {
                if (patch.When.TryGetValue(nameof(ConditionType.Weather), out string? value) && value?.Contains("Sun") == true)
                    patch.When[nameof(ConditionType.Weather)] = $"{value}, Wind";
            }

            return true;
        }
    }
}
