using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        /// <summary>The condition tokens in the string.</summary>
        public HashSet<ConditionKey> ConditionTokens { get; }

        /// <summary>The string with tokens substituted for the last context update.</summary>
        public string Value { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="raw">The raw string before token substitution.</param>
        /// <param name="conditionTokens">The condition tokens in the string.</param>
        /// <param name="tokenPattern">The regex pattern matching tokens, where the first group is the token key.</param>
        public TokenString(string raw, HashSet<ConditionKey> conditionTokens, Regex tokenPattern)
        {
            this.Raw = raw;
            this.TokenPattern = tokenPattern;
            this.ConditionTokens = conditionTokens;
        }

        /// <summary>Update the <see cref="Value"/> with the given tokens.</summary>
        /// <param name="tokenisableConditions">The conditions which can be used in tokens.</param>
        /// <returns>Returns whether the value changed.</returns>
        public bool UpdateContext(IDictionary<ConditionKey, string> tokenisableConditions)
        {
            string prevValue = this.Value;
            this.Value = this.Apply(this.Raw, tokenisableConditions);
            return this.Value != prevValue;
        }

        /// <summary>Get a copy of the tokenable string with the given tokens applied.</summary>
        /// <param name="tokens">The tokens to apply.</param>
        public string GetStringWithTokens(IDictionary<ConditionKey, string> tokens)
        {
            return this.Apply(this.Raw, tokens);
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
        private string Apply(string raw, IDictionary<ConditionKey, string> tokens)
        {
            return this.TokenPattern.Replace(raw, match =>
            {
                string rawKey = match.Groups[1].Value.Trim();
                return ConditionKey.TryParse(rawKey, out ConditionKey conditionKey) && tokens.TryGetValue(conditionKey, out string value)
                    ? value
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
