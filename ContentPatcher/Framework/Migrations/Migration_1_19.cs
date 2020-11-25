using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Validates patches for format version 1.19.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_19 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_19()
            : base(new SemanticVersion(1, 19, 0))
        {
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.Time.ToString()
            };
        }

        /// <inheritdoc />
        public override bool TryMigrate(ContentConfig content, out string error)
        {
            if (!base.TryMigrate(content, out error))
                return false;

            foreach (PatchConfig patch in content.Changes)
            {
                // 1.19 adds PatchMode for maps
                if (patch.PatchMode != null && this.GetAction(patch) == PatchType.EditMap)
                {
                    error = this.GetNounPhraseError($"using {nameof(patch.PatchMode)} for a map patch");
                    return false;
                }

                // 1.19 adds OnTimeChange update rate
                if (this.GetEnum<UpdateRate>(patch.Update) == UpdateRate.OnTimeChange)
                {
                    error = this.GetNounPhraseError($"using the {nameof(UpdateRate.OnTimeChange)} update rate");
                    return false;
                }

                // 1.19 adds multiple FromFile values
                if (patch.FromFile?.Contains(",") == true)
                {
                    error = this.GetNounPhraseError($"specifying multiple {nameof(PatchConfig.FromFile)} values");
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override bool TryMigrate(Condition condition, out string error)
        {
            // 1.19 adds boolean query expressions
            bool isQuery = condition.Name?.EqualsIgnoreCase(nameof(ConditionType.Query)) == true;
            if (isQuery)
            {
                InvariantHashSet values = condition.Values?.SplitValuesUnique();
                if (values?.Any() == true && values.All(p => bool.TryParse(p, out bool _)))
                {
                    error = "using boolean query expressions";
                    return false;
                }
            }

            return base.TryMigrate(condition, out error);
        }
    }
}
