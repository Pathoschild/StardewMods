using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Lexing.LexTokens;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrate patches to format version 1.9.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Named for clarity.")]
    internal class Migration_1_9 : BaseMigration
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public Migration_1_9()
            : base(new SemanticVersion(1, 9, 0)) { }

        /// <summary>Migrate a lexical token.</summary>
        /// <param name="lexToken">The lexical token to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        public override bool TryMigrate(ILexToken lexToken, out string error)
        {
            if (!base.TryMigrate(lexToken, out error))
                return false;

            // 1.9 adds mod tokens
            if (lexToken is LexTokenToken token && token.Name.Contains(InternalConstants.ModTokenSeparator))
            {
                error = this.GetNounPhraseError($"using custom mod-provided tokens like '{lexToken}'");
                return false;
            }

            return true;
        }
    }
}
