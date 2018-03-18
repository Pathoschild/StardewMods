using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.ConfigModels;
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

        /// <summary>The condition tokens in the string.</summary>
        public HashSet<ConditionKey> TokenKeys { get; }

        /// <summary>The string with tokens substituted for the last context update.</summary>
        public string Value { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="raw">The raw string before token substitution.</param>
        public TokenString(string raw)
        {
            this.Raw = raw;
            this.TokenKeys = new HashSet<ConditionKey>(this.GetTokenKeys(raw));
        }

        /// <summary>Update the <see cref="Value"/> with the given tokens.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the value changed.</returns>
        public bool UpdateContext(ConditionContext context)
        {
            string prevValue = this.Value;
            this.Value = this.Apply(this.Raw, context.Values);
            return this.Value != prevValue;
        }

        /// <summary>Get a copy of the tokenable string with the given tokens applied.</summary>
        /// <param name="tokens">The tokens to apply.</param>
        public string GetStringWithTokens(IDictionary<ConditionKey, string> tokens)
        {
            return this.Apply(this.Raw, tokens);
        }

        /// <summary>Apply permanent tokens to a string and ensure that all remaining tokens match a <see cref="ConditionKey"/>.</summary>
        /// <param name="str">The raw string before token substitution.</param>
        /// <param name="configValues">The config tokens to apply immediately.</param>
        /// <param name="parsed">The preparsed string.</param>
        /// <param name="error">An error indicating why the string is invalid (if applicable).</param>
        /// <returns>Returns whether the string was successfully preparsed.</returns>
        public static bool TryParse(string str, IDictionary<string, ConfigField> configValues, out TokenString parsed, out string error)
        {
            // apply permanent tokens
            try
            {
                str = TokenString.TokenPattern.Replace(str, match =>
                {
                    // get key
                    string key = match.Groups[1].Value.Trim();

                    // ignore conditions
                    if (Enum.TryParse(key, true, out ConditionKey _))
                        return match.Value;

                    // validate config field
                    if (!configValues.TryGetValue(key, out ConfigField field))
                        throw new InvalidOperationException($"token {{{{{key}}}}} doesn't match any condition or config field.");
                    if (field.AllowMultiple)
                        throw new InvalidOperationException($"token {{{{{key}}}}} can't be used, because that config field allows multiple values.");

                    // apply
                    return field.Value.FirstOrDefault() ?? "";
                });
            }
            catch (InvalidOperationException ex)
            {
                parsed = null;
                error = ex.Message;
                return false;
            }

            // detect invalid tokens
            InvariantHashSet invalidTokens = new InvariantHashSet(
                from Match match in TokenString.TokenPattern.Matches(str)
                let rawToken = match.Groups[1].Value.Trim()
                where !Enum.TryParse(rawToken, true, out ConditionKey _)
                select rawToken
            );
            if (invalidTokens.Any())
            {
                parsed = null;
                error = $"found invalid tokens: {string.Join(", ", invalidTokens.OrderBy(p => p))}";
                return false;
            }

            // preparse OK
            parsed = new TokenString(str);
            error = null;
            return true;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Get all condition tokens present in a string.</summary>
        /// <param name="raw">The raw string.</param>
        private IEnumerable<ConditionKey> GetTokenKeys(string raw)
        {
            foreach (Match match in TokenString.TokenPattern.Matches(raw))
            {
                string rawKey = match.Groups[1].Value.Trim();
                if (!Enum.TryParse(rawKey, true, out ConditionKey conditionKey))
                    throw new FormatException($"The token {{{{{rawKey}}}}} does not match a known condition.");

                yield return conditionKey;
            }
        }

        /// <summary>Get a new string with tokens substituted.</summary>
        /// <param name="raw">The raw string before token substitution.</param>
        /// <param name="tokens">The token values to apply.</param>
        private string Apply(string raw, IDictionary<ConditionKey, string> tokens)
        {
            return TokenString.TokenPattern.Replace(raw, match =>
            {
                string key = match.Groups[1].Value.Trim();
                return Enum.TryParse(key, true, out ConditionKey conditionKey) && tokens.TryGetValue(conditionKey, out string value)
                    ? value
                    : match.Value;
            });
        }
    }
}
