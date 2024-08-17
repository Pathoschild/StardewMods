using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 1.19.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_19 : BaseMigration
    {
        /*********
        ** Fields
        *********/
        /// <summary>Handles parsing raw strings into tokens.</summary>
        private readonly Lexer Lexer = Lexer.Instance;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_19()
            : base(new SemanticVersion(1, 19, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.Time)
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref patches, out error))
                return false;

            foreach (PatchConfig patch in patches)
            {
                // 1.19 adds PatchMode for maps
                if (patch.PatchMode != null && this.HasAction(patch, PatchType.EditMap))
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
                if (this.HasMultipleValues(patch.FromFile))
                {
                    error = this.GetNounPhraseError($"specifying multiple {nameof(PatchConfig.FromFile)} values");
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override bool TryMigrate(Condition condition, [NotNullWhen(false)] out string? error)
        {
            // 1.19 adds boolean query expressions
            bool isQuery = condition.Name?.EqualsIgnoreCase(nameof(ConditionType.Query)) == true;
            if (isQuery)
            {
                IInvariantSet? values = condition.Values?.SplitValuesUnique();
                if (values?.Any() == true && values.All(p => bool.TryParse(p, out bool _)))
                {
                    error = "using boolean query expressions";
                    return false;
                }
            }

            return base.TryMigrate(condition, out error);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether a value has multiple top-level lexical values.</summary>
        /// <param name="raw">The raw unparsed value.</param>
        private bool HasMultipleValues(string? raw)
        {
            // quick check
            if (raw?.Contains(",") != true)
                return false;

            // lexical check (this ensures a value like '{{Random: a, b}}' isn't incorrectly treated as two values)
            return this.Lexer
                .SplitLexically(raw)
                .Take(2)
                .Count() > 1;
        }
    }
}
