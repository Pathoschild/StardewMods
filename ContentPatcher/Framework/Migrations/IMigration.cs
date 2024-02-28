using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
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

        /// <summary>If set, content packs for which this migration applies will be listed in an <c>INFO</c> log with these messages.</summary>
        public string[] MigrationWarnings { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Migrate a content pack.</summary>
        /// <param name="content">The content pack data to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed.</param>
        /// <returns>Returns whether migration succeeded.</returns>
        /// <remarks>This only runs for the main <c>content.json</c>; most migrations should use <see cref="TryMigrate(ref PatchConfig[],out string?)"/> instead, which also runs for <c>Include</c> patches.</remarks>
        bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error);

        /// <summary>Migrate a list of patches.</summary>
        /// <param name="patches">The patch data to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed.</param>
        /// <returns>Returns whether migration succeeded.</returns>
        bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error);

        /// <summary>Migrate a lexical token.</summary>
        /// <param name="lexToken">The lexical token to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error);

        /// <summary>Migrate a tokenized string.</summary>
        /// <param name="tokenStr">The tokenized string to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        bool TryMigrate(IManagedTokenString tokenStr, [NotNullWhen(false)] out string? error);

        /// <summary>Migrate a tokenized JSON structure.</summary>
        /// <param name="tokenStructure">The tokenized JSON structure to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        bool TryMigrate(TokenizableJToken tokenStructure, [NotNullWhen(false)] out string? error);

        /// <summary>Migrate a condition.</summary>
        /// <param name="condition">The condition to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        bool TryMigrate(Condition condition, [NotNullWhen(false)] out string? error);
    }
}
