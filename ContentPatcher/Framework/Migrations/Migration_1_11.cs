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
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.Lowercase.ToString(),
                ConditionType.Uppercase.ToString()
            };
        }

        /// <summary>Migrate a lexical token.</summary>
        /// <param name="lexToken">The lexical token to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        public override bool TryMigrate(ILexToken lexToken, out string error)
        {
            if (!base.TryMigrate(lexToken, out error))
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
