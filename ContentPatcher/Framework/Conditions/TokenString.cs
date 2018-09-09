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
        private readonly Regex TokenPattern;


        /*********
        ** Accessors
        *********/
        /// <summary>The raw string without token substitution.</summary>
        public string Raw { get; private set; }

        /// <summary>The tokens used in the string.</summary>
        public HashSet<TokenKey> Tokens { get; }

        /// <summary>The string with tokens substituted for the last context update.</summary>
        public string Value { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="raw">The raw string before token substitution.</param>
        /// <param name="tokens">The tokens used in the string.</param>
        /// <param name="tokenPattern">The regex pattern matching tokens, where the first group is the token key.</param>
        public TokenString(string raw, HashSet<TokenKey> tokens, Regex tokenPattern)
        {
            this.Raw = raw;
            this.TokenPattern = tokenPattern;
            this.Tokens = tokens;
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
            return this.TokenPattern.Replace(raw, match =>
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
            return this.TokenPattern.Replace(raw, match =>
            {
                string key = match.Groups[1].Value.Trim();
                return tokens.TryGetValue(key, out string value)
                    ? value
                    : match.Value;
            });
        }
    }
}
