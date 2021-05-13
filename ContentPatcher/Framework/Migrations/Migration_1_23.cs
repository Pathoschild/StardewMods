using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Validates patches for format version 1.23.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_23 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_23()
            : base(new SemanticVersion(1, 23, 0))
        {
            this.AddedTokens.AddMany(
                ConditionType.HasCaughtFish.ToString()
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ContentConfig content, out string error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            // 1.23 adds support for 'Fields' text operations
            foreach (PatchConfig patch in content.Changes)
            {
                if (this.GetAction(patch) == PatchType.EditData)
                {
                    foreach (var operation in patch.TextOperations)
                    {
                        if (operation.Target.Any() && this.GetEnum<TextOperationTargetRoot>(operation.Target[0]) == TextOperationTargetRoot.Fields)
                        {
                            error = this.GetNounPhraseError($"using {nameof(patch.TextOperations)} with the {nameof(TextOperationTargetRoot.Fields)} target");
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}