using System;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
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
        protected InvariantHashSet AddedTokens { get; } = new();


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public ISemanticVersion Version { get; }


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public virtual bool TryMigrate(ContentConfig content, [NotNullWhen(false)] out string? error)
        {
            error = null;
            return true;
        }

        /// <inheritdoc />
        public virtual bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
        {
            if (lexToken is LexTokenToken token)
            {
                // tokens which need a higher version
                if (this.AddedTokens.Contains(token.Name))
                {
                    error = this.GetNounPhraseError($"using token {token.Name}");
                    return false;
                }

                // check input arguments
                if (token.HasInputArgs())
                {
                    ILexToken[] parts = token.InputArgs.Parts;
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (!this.TryMigrate(ref parts[i], out error))
                            return false;
                    }
                }
            }

            // no issue found
            error = null;
            return true;
        }

        /// <inheritdoc />
        public virtual bool TryMigrate(IManagedTokenString tokenStr, [NotNullWhen(false)] out string? error)
        {
            // no issue found
            error = null;
            return true;
        }

        /// <inheritdoc />
        public bool TryMigrate(TokenizableJToken tokenStructure, [NotNullWhen(false)] out string? error)
        {
            foreach (IManagedTokenString str in tokenStructure.GetTokenStrings())
            {
                if (!this.TryMigrate(str, out error))
                    return false;
            }

            error = null;
            return true;
        }

        /// <inheritdoc />
        public virtual bool TryMigrate(Condition condition, [NotNullWhen(false)] out string? error)
        {
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

        /// <summary>Get the action type for a patch.</summary>
        /// <param name="patch">The patch to parse.</param>
        protected PatchType? GetAction(PatchConfig? patch)
        {
            if (patch is null)
                return null;

            return this.GetEnum<PatchType>(patch.Action);
        }

        /// <summary>Get whether the patch is of the given type.</summary>
        /// <param name="patch">The patch to check.</param>
        /// <param name="type">The expected patch type.</param>
        protected bool HasAction([NotNullWhen(true)] PatchConfig? patch, PatchType type)
        {
            return this.GetAction(patch) == type;
        }

        /// <summary>Get a parsed enum value, or <c>null</c> if it's not valid.</summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="raw">The raw value.</param>
        protected TEnum? GetEnum<TEnum>(string? raw)
            where TEnum : struct
        {
            return Enum.TryParse(raw, ignoreCase: true, out TEnum parsed)
                ? parsed
                : null;
        }
    }
}
