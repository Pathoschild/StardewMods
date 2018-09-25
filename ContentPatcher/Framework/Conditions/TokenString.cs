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
        public string Raw { get; }

        /// <summary>The tokens used in the string.</summary>
        public HashSet<TokenName> Tokens { get; } = new HashSet<TokenName>();

        /// <summary>The unrecognised tokens in the string.</summary>
        public InvariantHashSet InvalidTokens { get; } = new InvariantHashSet();

        /// <summary>Whether the string contains any tokens (including invalid tokens).</summary>
        public bool HasAnyTokens => this.Tokens.Count > 0 || this.InvalidTokens.Count > 0;

        /// <summary>Whether the token string value may change depending on the context.</summary>
        public bool IsMutable { get; }

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
            // get raw value
            this.Raw = raw?.Trim();
            if (string.IsNullOrWhiteSpace(this.Raw))
                return;

            // extract tokens
            int tokensFound = 0;
            foreach (Match match in TokenString.TokenPattern.Matches(raw))
            {
                tokensFound++;
                string rawToken = match.Groups[1].Value.Trim();
                if (TokenName.TryParse(rawToken, out TokenName name))
                {
                    if (tokenContext.Contains(name, enforceContext: false))
                        this.Tokens.Add(name);
                    else
                        this.InvalidTokens.Add(rawToken);
                }
                else
                    this.InvalidTokens.Add(rawToken);
            }
            this.IsSingleTokenOnly = tokensFound == 1 && TokenString.TokenPattern.Replace(this.Raw, "", 1) == "";
            this.IsMutable = this.Tokens.Any();
        }

        /// <summary>Update the <see cref="Value"/> with the given tokens.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <returns>Returns whether the value changed.</returns>
        public bool UpdateContext(IContext context)
        {
            string prevValue = this.Value;
            this.Value = this.GetApplied(context);
            return this.Value != prevValue;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a new string with tokens substituted.</summary>
        /// <param name="context">Provides access to contextual tokens.</param>
        private string GetApplied(IContext context)
        {
            if (!this.IsMutable)
                return this.Raw;

            return TokenString.TokenPattern.Replace(this.Raw, match =>
            {
                TokenName name = TokenName.Parse(match.Groups[1].Value);
                IToken token = context.GetToken(name, enforceContext: true);
                return token != null
                    ? token.GetValues(name).FirstOrDefault()
                    : match.Value;
            });
        }
    }
}
