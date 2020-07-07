using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens.Json;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations
{
    /// <summary>The base implementation for a format version migrator.</summary>
    internal abstract class BaseMigration : IMigration
    {
        /*********
        ** Private methods
        *********/
        /// <summary>The tokens added in this format version.</summary>
        protected InvariantHashSet AddedTokens { get; set; }


        /*********
        ** Accessors
        *********/
        /// <summary>The format version to which this migration applies.</summary>
        public ISemanticVersion Version { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Migrate a content pack.</summary>
        /// <param name="content">The content pack data to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed.</param>
        /// <returns>Returns whether the content pack was successfully migrated.</returns>
        public virtual bool TryMigrate(ContentConfig content, out string error)
        {
            error = null;
            return true;
        }

        /// <summary>Migrate a lexical token.</summary>
        /// <param name="lexToken">The lexical token to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        public virtual bool TryMigrate(ILexToken lexToken, out string error)
        {
            if (lexToken is LexTokenToken token)
            {
                // tokens which need a higher version
                if (this.AddedTokens?.Contains(token.Name) == true)
                {
                    error = this.GetNounPhraseError($"using token {token.Name}");
                    return false;
                }

                // check input arguments
                if (token.HasInputArgs())
                {
                    foreach (ILexToken part in token.InputArgs.Parts)
                    {
                        if (!this.TryMigrate(part, out error))
                            return false;
                    }
                }
            }

            // no issue found
            error = null;
            return true;
        }

        /// <summary>Migrate a tokenized string.</summary>
        /// <param name="tokenStr">The tokenized string to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        public virtual bool TryMigrate(IManagedTokenString tokenStr, out string error)
        {
            // no issue found
            error = null;
            return true;
        }

        /// <summary>Migrate a tokenized JSON structure.</summary>
        /// <param name="tokenStructure">The tokenized JSON structure to migrate.</param>
        /// <param name="error">An error message which indicates why migration failed (if any).</param>
        /// <returns>Returns whether migration succeeded.</returns>
        public bool TryMigrate(TokenizableJToken tokenStructure, out string error)
        {
            foreach (IManagedTokenString str in tokenStructure.GetTokenStrings())
            {
                if (!this.TryMigrate(str, out error))
                    return false;
            }

            error = null;
            return true;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="version">The version to which this migration applies.</param>
        protected BaseMigration(ISemanticVersion version)
        {
            this.Version = version;
        }

        /// <summary>Get an error message indicating an action or feature requires a newer format version.</summary>
        /// <param name="nounPhrase">The noun phrase, like "using X feature".</param>
        protected string GetNounPhraseError(string nounPhrase)
        {
            return $"{nounPhrase} requires {nameof(ContentConfig.Format)} version {this.Version} or later";
        }
    }
}
