using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 1.4.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_4 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_4()
            : base(new SemanticVersion(1, 4, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.DayEvent),
                nameof(ConditionType.HasFlag),
                nameof(ConditionType.HasSeenEvent),
                nameof(ConditionType.Hearts),
                nameof(ConditionType.Relationship),
                nameof(ConditionType.Spouse)
            );
        }
    }
}
