using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace ContentPatcher.Framework
{
    /// <summary>Handles low-level parsing and validation for tokens.</summary>
    internal class TokenParser
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The manifest for the content pack being parsed.</summary>
        public IManifest ForMod { get; }

        /// <summary>The tokens available for this content pack.</summary>
        public IContext Context { get; }

        /// <summary>The migrator which validates and migrates content pack data.</summary>
        public IMigration Migrator { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tokenContext">The tokens available for this content pack.</param>
        /// <param name="forMod">The manifest for the content pack being parsed.</param>
        /// <param name="migrator">The migrator which validates and migrates content pack data.</param>
        public TokenParser(IContext tokenContext, IManifest forMod, IMigration migrator)
        {
            this.Context = tokenContext;
            this.ForMod = forMod;
            this.Migrator = migrator;
        }

        /// <summary>Parse a string which can contain tokens, and validate that it's valid.</summary>
        /// <param name="rawValue">The raw string which may contain tokens.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value.</param>
        public bool TryParseStringTokens(string rawValue, out string error, out IParsedTokenString parsed)
        {
            // parse
            parsed = new TokenString(rawValue, this.Context);
            if (!this.Migrator.TryMigrate(parsed, out error))
                return false;

            // validate unknown tokens
            IContextualState state = parsed.GetDiagnosticState();
            if (state.InvalidTokens.Any())
            {
                error = $"found unknown tokens ({string.Join(", ", state.InvalidTokens.OrderBy(p => p))})";
                parsed = null;
                return false;
            }

            // validate tokens
            foreach (LexTokenToken lexToken in parsed.GetTokenPlaceholders(recursive: false))
            {
                if (!this.TryValidateToken(lexToken, out error))
                    return false;
            }

            // looks OK
            error = null;
            return true;
        }


        /// <summary>Parse a JSON structure which can contain tokens, and validate that it's valid.</summary>
        /// <param name="rawJson">The raw JSON structure which may contain tokens.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value, which may be legitimately <c>null</c> even if successful.</param>
        public bool TryParseJsonTokens(JToken rawJson, out string error, out TokenisableJToken parsed)
        {
            if (rawJson == null || rawJson.Type == JTokenType.Null)
            {
                error = null;
                parsed = null;
                return true;
            }

            // parse
            parsed = new TokenisableJToken(rawJson, this.Context);
            if (!this.Migrator.TryMigrate(parsed, out error))
                return false;

            // validate tokens
            foreach (LexTokenToken lexToken in parsed.GetTokenStrings().SelectMany(p => p.GetTokenPlaceholders(recursive: false)))
            {
                if (!this.TryValidateToken(lexToken, out error))
                    return false;
            }

            // looks OK
            if (parsed.Value.Type == JTokenType.Null)
                parsed = null;
            error = null;
            return true;
        }

        /// <summary>Validate whether a token referenced in a content pack field is valid.</summary>
        /// <param name="lexToken">The lexical token reference to check.</param>
        /// <param name="error">An error phrase indicating why validation failed (if applicable).</param>
        public bool TryValidateToken(LexTokenToken lexToken, out string error)
        {
            // token doesn't exist
            IToken token = this.Context.GetToken(lexToken.Name, enforceContext: false);
            if (token == null)
            {
                error = $"'{lexToken}' can't be used as a token because that token could not be found.";
                return false;
            }

            // token requires dependency
            if (token is ModProvidedToken modToken && !this.ForMod.HasDependency(modToken.Mod.UniqueID, canBeOptional: false))
            {
                error = $"'{lexToken}' can't be used because it's provided by {modToken.Mod.Name} (ID: {modToken.Mod.UniqueID}), but {this.ForMod.Name} doesn't list it as a dependency.";
                return false;
            }

            // no error found
            error = null;
            return true;
        }
    }
}
