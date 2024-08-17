using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to format version 1.23.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_23 : BaseMigration
    {
        /*********
        ** Fields
        *********/
        /// <summary>A pattern which matches a 'valueAt' argument.</summary>
        private static readonly Regex ValueAtPattern = new(@"\|\s*valueAt\s*=", RegexOptions.Compiled);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_23()
            : base(new SemanticVersion(1, 23, 0))
        {
            this.AddedTokens = new InvariantSet(
                nameof(ConditionType.ChildGenders),
                nameof(ConditionType.ChildNames),
                nameof(ConditionType.Count),
                nameof(ConditionType.HasCaughtFish)
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref patches, out error))
                return false;

            // 1.23 adds support for 'Fields' text operations
            foreach (PatchConfig patch in patches)
            {
                if (this.HasAction(patch, PatchType.EditData))
                {
                    foreach (TextOperationConfig? operation in patch.TextOperations)
                    {
                        if (operation?.Target.Any() == true && this.GetEnum<TextOperationTargetRoot>(operation.Target[0]) == TextOperationTargetRoot.Fields)
                        {
                            error = this.GetNounPhraseError($"using {nameof(patch.TextOperations)} with the {nameof(TextOperationTargetRoot.Fields)} target");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref lexToken, out error))
                return false;

            // 1.23 adds 'valueAt' input argument
            if (lexToken is LexTokenToken token && token.HasInputArgs())
            {
                string inputStr = token.InputArgs.ToString();
                if (inputStr.ContainsIgnoreCase("valueAt") && ValueAtPattern.IsMatch(inputStr))
                {
                    error = this.GetNounPhraseError("using the 'valueAt' argument");
                    return false;
                }
            }

            return true;
        }
    }
}
