using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens.Json;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>Migrates patches to a given format version.</summary>
    internal interface IMigration
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The format version to which this migration applies.</summary>
        ISemanticVersion Version { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Migrate a content pack.</summary>
        /// <param name="content">The content pack data to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed.</param>
        /// <returns>Returns whether migration succeeded.</returns>
        bool TryMigrate(ContentConfig content, out string error);

        /// <summary>Migrate a lexical token.</summary>
        /// <param name="lexToken">The lexical token to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        bool TryMigrate(ILexToken lexToken, out string error);

        /// <summary>Migrate a tokenized string.</summary>
        /// <param name="tokenStr">The tokenized string to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        bool TryMigrate(IManagedTokenString tokenStr, out string error);

        /// <summary>Migrate a tokenized JSON structure.</summary>
        /// <param name="tokenStructure">The tokenized JSON structure to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        bool TryMigrate(TokenizableJToken tokenStructure, out string error);
    }
}
