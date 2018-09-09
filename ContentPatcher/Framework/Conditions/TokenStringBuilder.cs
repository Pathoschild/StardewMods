using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Conditions
{
    internal class TokenStringBuilder
    {
        /*********
        ** Properties
        *********/
        /// <summary>The regex pattern matching a string token.</summary>
        /// <remarks>This field is only internal to support unit tests and shouldn't be used directly.</remarks>
        internal static readonly Regex TokenPattern = new Regex(@"{{([ \w\.\-]+)}}", RegexOptions.Compiled);

        /// <summary>The configuration to apply.</summary>
        private readonly InvariantDictionary<ConfigField> Config;


        /*********
        ** Accessors
        *********/
        /// <summary>The raw string containing tokens.</summary>
        public string RawValue { get; }

        /// <summary>The tokens used in the string.</summary>
        public HashSet<TokenKey> Tokens { get; } = new HashSet<TokenKey>();

        /// <summary>The config tokens in the string.</summary>
        public InvariantHashSet ConfigTokens { get; } = new InvariantHashSet();

        /// <summary>The unrecognised tokens in the string.</summary>
        public InvariantHashSet InvalidTokens { get; } = new InvariantHashSet();

        /// <summary>Whether the string contains any tokens (including invalid tokens).</summary>
        public bool HasAnyTokens => this.Tokens.Count > 0 || this.ConfigTokens.Count > 0 || this.InvalidTokens.Count > 0;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="rawValue">The raw string which may contain tokens.</param>
        /// <param name="config">The player configuration.</param>
        /// <param name="tokens">The tokens to apply.</param>
        public TokenStringBuilder(string rawValue, InvariantDictionary<ConfigField> config, InvariantDictionary<IToken> tokens)
        {
            this.RawValue = rawValue;
            this.Config = config;

            foreach (Match match in TokenStringBuilder.TokenPattern.Matches(rawValue))
            {
                string rawToken = match.Groups[1].Value.Trim();
                if (config.ContainsKey(rawToken))
                    this.ConfigTokens.Add(rawToken);
                else if (TokenKey.TryParse(rawToken, out TokenKey key) && tokens.ContainsKey(key.Key))
                    this.Tokens.Add(key);
                else
                    this.InvalidTokens.Add(rawToken);
            }
        }

        /// <summary>Construct a token string and permanently apply config values.</summary>
        public TokenString Build()
        {
            TokenString tokenString = new TokenString(this.RawValue, this.Tokens, TokenStringBuilder.TokenPattern);
            InvariantDictionary<string> configValues = new InvariantDictionary<string>(this.Config.ToDictionary(p => p.Key, p => p.Value.Value.FirstOrDefault()));
            tokenString.ApplyPermanently(configValues);
            return tokenString;
        }
    }
}
