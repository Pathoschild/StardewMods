using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 1.28.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_28 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_28()
            : base(new SemanticVersion(1, 28, 0)) { }

        /// <inheritdoc />
        public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref patches, out error))
                return false;

            // 1.28 adds the RemoveDelimited text operation
            foreach (PatchConfig patch in patches)
            {
                if (this.HasAction(patch, PatchType.EditData))
                {
                    foreach (TextOperationConfig? operation in patch.TextOperations)
                    {
                        TextOperationType? operationType = this.GetEnum<TextOperationType>(operation?.Operation);
                        if (operationType is TextOperationType.RemoveDelimited)
                        {
                            error = this.GetNounPhraseError($"using {nameof(patch.TextOperations)} of type {operationType.Value}");
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
