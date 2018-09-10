using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Conditions
{
    /// <summary>A string value which can contain condition tokens.</summary>
    internal class TokenString
    {
        /*********
        ** Properties
        *********/
        /// <summary>The regex pattern matching a string token.</summary>
        private static readonly Regex TokenPattern = new Regex(@"{{([ \w\.\-]+)}}", RegexOptions.Compiled);


        /*********
        ** Accessors
        *********/
        /// <summary>The raw string without token substitution.</summary>
        public string Raw { get; private set; }

        /// <summary>The tokens used in the string.</summary>
        public HashSet<IToken> Tokens { get; } = new HashSet<IToken>();

        /// <summary>The unrecognised tokens in the string.</summary>
        public InvariantHashSet InvalidTokens { get; } = new InvariantHashSet();

        /// <summary>Whether the string contains any tokens (including invalid tokens).</summary>
        public bool HasAnyTokens => this.Tokens.Count > 0 || this.InvalidTokens.Count > 0;

        /// <summary>Whether the token string consists of a single token with no surrounding text.</summary>
        public bool IsSingleTokenOnly { get; }

        /// <summary>The string with tokens substituted for the last context update.</summary>
        public string Value { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="raw">The raw string before token substitution.</param>
        /// <param name="tokenContext">The available token context.</param>
        public TokenString(string raw, IContext tokenContext)
        {
            this.Raw = raw.Trim();

            int tokensFound = 0;
            foreach (Match match in TokenString.TokenPattern.Matches(raw))
            {
                tokensFound++;
                string rawToken = match.Groups[1].Value.Trim();
                if (TokenKey.TryParse(rawToken, out TokenKey key))
                {
                    IToken token = tokenContext.GetToken(key);
                    if (token != null)
                        this.Tokens.Add(token);
                    else
                        this.InvalidTokens.Add(rawToken);
                }
                else
                    this.InvalidTokens.Add(rawToken);
            }

            this.IsSingleTokenOnly = tokensFound == 1 && TokenString.TokenPattern.Replace(this.Raw, "", 1) == "";
        }

        /// <summary>Update the <see cref="Value"/> with the given tokens.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <param name="singleValueTokens">The tokens that can only contain one value.</param>
        /// <returns>Returns whether the value changed.</returns>
        public bool UpdateContext(IContext context, InvariantDictionary<IToken> singleValueTokens)
        {
            string prevValue = this.Value;
            this.Value = this.Apply(this.Raw, singleValueTokens);
            return this.Value != prevValue;
        }

        /// <summary>Permanently apply the given token values to the string.</summary>
        /// <param name="values">The token values to apply.</param>
        public void ApplyPermanently(InvariantDictionary<string> values)
        {
            this.Raw = this.Apply(this.Raw, values);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a new string with tokens substituted.</summary>
        /// <param name="raw">The raw string before token substitution.</param>
        /// <param name="tokens">The token values to apply.</param>
        private string Apply(string raw, InvariantDictionary<IToken> tokens)
        {
            return TokenString.TokenPattern.Replace(raw, match =>
            {
                string key = match.Groups[1].Value.Trim();
                return tokens.TryGetValue(key, out IToken token)
                    ? token.GetValues().FirstOrDefault()
                    : match.Value;
            });
        }

        /// <summary>Get a new string with tokens substituted.</summary>
        /// <param name="raw">The raw string before token substitution.</param>
        /// <param name="tokens">The token values to apply.</param>
        private string Apply(string raw, InvariantDictionary<string> tokens)
        {
            return TokenString.TokenPattern.Replace(raw, match =>
            {
                string key = match.Groups[1].Value.Trim();
                return tokens.TryGetValue(key, out string value)
                    ? value
                    : match.Value;
            });
        }
    }
}
