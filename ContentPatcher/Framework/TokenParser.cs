using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Utilities;
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

        /// <summary>The mod IDs which are currently installed.</summary>
        public InvariantHashSet InstalledMods { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tokenContext">The tokens available for this content pack.</param>
        /// <param name="forMod">The manifest for the content pack being parsed.</param>
        /// <param name="migrator">The migrator which validates and migrates content pack data.</param>
        /// <param name="installedMods">The mod IDs which are currently installed.</param>
        public TokenParser(IContext tokenContext, IManifest forMod, IMigration migrator, InvariantHashSet installedMods)
        {
            this.Context = tokenContext;
            this.ForMod = forMod;
            this.Migrator = migrator;
            this.InstalledMods = installedMods;
        }

        /// <summary>Parse a string which can contain tokens, and validate that it's valid.</summary>
        /// <param name="rawValue">The raw string which may contain tokens.</param>
        /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value.</param>
        public bool TryParseStringTokens(string rawValue, InvariantHashSet assumeModIds, out string error, out IParsedTokenString parsed)
        {
            // parse
            parsed = new TokenString(rawValue, this.Context);
            if (!this.Migrator.TryMigrate(parsed, out error))
                return false;

            // validate tokens
            foreach (LexTokenToken lexToken in parsed.GetTokenPlaceholders(recursive: false))
            {
                if (!this.TryValidateToken(lexToken, assumeModIds, out error))
                    return false;
            }

            // looks OK
            error = null;
            return true;
        }


        /// <summary>Parse a JSON structure which can contain tokens, and validate that it's valid.</summary>
        /// <param name="rawJson">The raw JSON structure which may contain tokens.</param>
        /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value, which may be legitimately <c>null</c> even if successful.</param>
        public bool TryParseJsonTokens(JToken rawJson, InvariantHashSet assumeModIds, out string error, out TokenizableJToken parsed)
        {
            if (rawJson == null || rawJson.Type == JTokenType.Null)
            {
                error = null;
                parsed = null;
                return true;
            }

            // parse
            parsed = new TokenizableJToken(rawJson, this.Context);
            if (!this.Migrator.TryMigrate(parsed, out error))
                return false;

            // validate tokens
            foreach (LexTokenToken lexToken in parsed.GetTokenStrings().SelectMany(p => p.GetTokenPlaceholders(recursive: false)))
            {
                if (!this.TryValidateToken(lexToken, assumeModIds, out error))
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
        /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
        /// <param name="error">An error phrase indicating why validation failed (if applicable).</param>
        public bool TryValidateToken(LexTokenToken lexToken, InvariantHashSet assumeModIds, out string error)
        {
            // token doesn't exist
            IToken token = this.Context.GetToken(lexToken.Name, enforceContext: false);
            if (token == null)
            {
                // special case: requires a missing mod that's checked via HasMod
                string requiredModId = lexToken.GetProviderModId();
                if (assumeModIds?.Contains(requiredModId) == true && !this.InstalledMods.Contains(requiredModId))
                {
                    error = null;
                    return true;
                }

                // else error
                error = $"'{lexToken}' can't be used as a token because that token could not be found.";
                return false;
            }

            // token requires dependency
            if (token is ModProvidedToken modToken && !this.ForMod.HasDependency(modToken.Mod.UniqueID, canBeOptional: false))
            {
                if (assumeModIds == null || !assumeModIds.Contains(modToken.Mod.UniqueID))
                {
                    error = $"'{lexToken}' can't be used because it's provided by {modToken.Mod.Name} (ID: {modToken.Mod.UniqueID}), but {this.ForMod.Name} doesn't list it as a dependency and the patch doesn't have an immutable {ConditionType.HasMod} condition for that mod.";
                    return false;
                }
            }

            // no error found
            error = null;
            return true;
        }
    }
}
