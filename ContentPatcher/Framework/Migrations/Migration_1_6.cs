using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
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
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.HasWalletItem.ToString(),
                ConditionType.SkillLevel.ToString()
            };
        }
    }
}
