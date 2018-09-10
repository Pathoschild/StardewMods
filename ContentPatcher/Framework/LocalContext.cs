using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework
{
    /// <summary>Manages the token context for a specific content pack.</summary>
    internal class LocalContext : IContext
    {
        /*********
        ** Properties
        *********/
        /// <summary>Manages the available global tokens.</summary>
        private readonly TokenManager TokenManager;


        /*********
        ** Accessors
        *********/
        /// <summary>The managed local tokens.</summary>
        public InvariantDictionary<IToken> LocalTokens { get; } = new InvariantDictionary<IToken>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tokenManager">Manages the available global tokens.</param>
        public LocalContext(TokenManager tokenManager)
        {
            this.TokenManager = tokenManager;
        }

        /// <summary>Get the underlying token which handles a key.</summary>
        /// <param name="key">The token key.</param>
        /// <returns>Returns the matching token, or <c>null</c> if none was found.</returns>
        public IToken GetToken(TokenKey key)
        {
            IToken globalToken = this.TokenManager.GetToken(key);
            if (globalToken != null)
                return globalToken;

            return this.LocalTokens.TryGetValue(key.Key, out IToken localToken)
                ? localToken
                : null;
        }

        /// <summary>Get the underlying tokens.</summary>
        public IEnumerable<IToken> GetTokens()
        {
            return this.GetTokens(localOnly: false);
        }

        /// <summary>Get the underlying tokens.</summary>
        /// <param name="localOnly">Whether to only return local tokens.</param>
        public IEnumerable<IToken> GetTokens(bool localOnly)
        {
            if (!localOnly)
            {
                foreach (IToken token in this.TokenManager.GetTokens())
                    yield return token;
            }

            foreach (IToken token in this.LocalTokens.Values)
                yield return token;
        }

        /// <summary>Get the current value of the given token for comparison. This is only valid for tokens where <see cref="IToken.CanHaveMultipleValues"/> is false; see <see cref="IContext.GetValues"/> otherwise.</summary>
        /// <param name="key">The token key.</param>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        /// <exception cref="ArgumentException">The specified key does includes or doesn't include a subkey, depending on <see cref="IToken.RequiresSubkeys"/>.</exception>
        /// <exception cref="InvalidOperationException">The specified token allows multiple values; see <see cref="IContext.GetValues"/> instead.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        public string GetValue(TokenKey key)
        {
            IToken token = this.GetToken(key);
            this.AssertToken(key.Key, key.Subkey, token);

            if (token.CanHaveMultipleValues)
                throw new InvalidOperationException($"The {key} token allows multiple values, so {nameof(this.GetValue)} is not valid.");

            return key.Subkey != null
                ? token.GetValues(key.Subkey).FirstOrDefault()
                : token.GetValues().FirstOrDefault();
        }

        /// <summary>Get the current values of the given token for comparison.</summary>
        /// <param name="key">The token key.</param>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        public IEnumerable<string> GetValues(TokenKey key)
        {
            IToken token = this.GetToken(key);
            this.AssertToken(key.Key, key.Subkey, token);
            return key.Subkey != null
                ? token.GetValues(key.Subkey)
                : token.GetValues();
        }

        /// <summary>Get the tokens that can only contain one value.</summary>
        public InvariantDictionary<IToken> GetSingleValues()
        {
            InvariantDictionary<IToken> values = new InvariantDictionary<IToken>();

            foreach (IToken token in this.GetTokens())
            {
                if (token.CanHaveMultipleValues)
                    continue;

                values[token.Name] = token;
            }

            return values;
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Assert that a token is valid and matches the key.</summary>
        /// <param name="tokenKey">The token key.</param>
        /// <param name="subkey">The token subkey (if any).</param>
        /// <param name="token">The resolved token.</param>
        /// <exception cref="ArgumentException">The specified key does includes or doesn't include a subkey, depending on <see cref="IToken.RequiresSubkeys"/>.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        /// <remarks>This implementation is duplicated by <see cref="TokenManager"/>.</remarks>
        private void AssertToken(string tokenKey, string subkey, IToken token)
        {
            if (token == null)
                throw new KeyNotFoundException($"There's no token with key {tokenKey}.");
            if (token.RequiresSubkeys && subkey == null)
                throw new InvalidOperationException($"The {tokenKey} token requires a subkey, but none was provided.");
            if (!token.RequiresSubkeys && subkey != null)
                throw new InvalidOperationException($"The {tokenKey} token doesn't allow a subkey, but a '{subkey}' subkey was provided.");
        }
    }
}
