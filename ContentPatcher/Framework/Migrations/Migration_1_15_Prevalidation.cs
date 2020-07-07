using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Validates patches for format version 1.15.</summary>
    /// <remarks>This needs to be run before <see cref="Migration_1_15_Rewrites"/> to avoid rewritten token arguments triggering validation errors.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_15_Prevalidation : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_15_Prevalidation()
            : base(new SemanticVersion(1, 15, 0))
        {
            this.AddedTokens = new InvariantHashSet
            {
                ConditionType.HasConversationTopic.ToString()
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

            // 1.15 adds named arguments
            if (lexToken is LexTokenToken token && token.HasInputArgs() && token.InputArgs.ToString().Contains("|") && token.InputArgs.ToString().Contains("="))
            {
                error = this.GetNounPhraseError("using named arguments");
                return false;
            }

            return true;
        }
    }
}
