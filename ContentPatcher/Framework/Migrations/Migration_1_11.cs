using System;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrate patches to format version 1.11.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_11 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_11()
            : base(new SemanticVersion(1, 11, 0))
        {
            this.AddedTokens.AddMany(
                ConditionType.Lowercase.ToString(),
                ConditionType.Uppercase.ToString()
            );
        }

        /// <inheritdoc />
        public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
        {
            if (!base.TryMigrate(ref lexToken, out error))
                return false;

            // 1.11 adds pinned keys
            if (lexToken is LexTokenToken token && token.Name.Equals("Random", StringComparison.OrdinalIgnoreCase) && token.HasInputArgs() && token.InputArgs.ToString().Contains("|"))
            {
                error = this.GetNounPhraseError("using pinned keys with the Random token");
                return false;
            }

            return true;
        }
    }
}
