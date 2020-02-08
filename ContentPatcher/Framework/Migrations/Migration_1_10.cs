using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrate patches to format version 1.10.</summary>
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
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.HasDialogueAnswer.ToString(),
                ConditionType.HavingChild.ToString(),
                ConditionType.IsJojaMartComplete.ToString(),
                ConditionType.Pregnant.ToString(),
                ConditionType.Random.ToString()
            };
        }

        /// <summary>Migrate a content pack.</summary>
        /// <param name="content">The content pack data to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed.</param>
        /// <returns>Returns whether the content pack was successfully migrated.</returns>
        public override bool TryMigrate(ContentConfig content, out string error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            if (content.Changes?.Any() == true)
            {
                foreach (PatchConfig patch in content.Changes)
                {
                    // 1.10 allows 'FromFile' with 'EditData' patches
                    if (patch.FromFile != null && Enum.TryParse(patch.Action, true, out PatchType action) && action == PatchType.EditData)
                    {
                        error = this.GetNounPhraseError($"using {nameof(PatchConfig.FromFile)} with action {nameof(PatchType.EditData)}");
                        return false;
                    }

                    // 1.10 adds MapProperties
                    if (patch.MapProperties != null)
                    {
                        error = this.GetNounPhraseError($"using {nameof(PatchConfig.MapProperties)}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
