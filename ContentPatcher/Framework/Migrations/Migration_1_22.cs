using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Validates patches for format version 1.22.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_22 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_22()
            : base(new SemanticVersion(1, 22, 0))
        {
            this.AddedTokens = new InvariantSet(
                ConditionType.FirstValidFile.ToString(),
                ConditionType.HasActiveQuest.ToString()
            );
        }
    }
}
