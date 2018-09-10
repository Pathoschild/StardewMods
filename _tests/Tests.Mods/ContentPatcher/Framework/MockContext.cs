using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace Pathoschild.Stardew.Tests.Mods.ContentPatcher.Framework
{
    /// <summary>Wraps a set of predefined tokens with the interface accepted by Content Patcher types.</summary>
    internal class MockContext : IContext
    {
        /*********
        ** Properties
        *********/
        /// <summary>The underlying token values.</summary>
        private readonly InvariantDictionary<IToken> Tokens = new InvariantDictionary<IToken>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tokens">The tokens to provide.</param>
        public MockContext(params IToken[] tokens)
        {
            foreach (IToken token in tokens)
                this.Tokens[token.Name] = token;
        }

        /// <summary>Get the underlying token which handles a key.</summary>
        /// <param name="key">The token key.</param>
        /// <returns>Returns the matching token, or <c>null</c> if none was found.</returns>
        public IToken GetToken(TokenKey key)
        {
            return this.Tokens.TryGetValue(key.Key, out IToken token)
                ? token
                : null;
        }

        /// <summary>Get the underlying tokens.</summary>
        public IEnumerable<IToken> GetTokens()
        {
            return this.Tokens.Values;
        }

        /// <summary>Get the current value of the given token for comparison. This is only valid for tokens where <see cref="IToken.CanHaveMultipleValues"/> is false; see <see cref="IContext.GetValues(TokenKey)"/> otherwise.</summary>
        /// <param name="key">The token key.</param>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        /// <exception cref="ArgumentException">The specified key does includes or doesn't include a subkey, depending on <see cref="IToken.RequiresSubkeys"/>.</exception>
        /// <exception cref="InvalidOperationException">The specified token allows multiple values; see <see cref="IContext.GetValues(TokenKey)"/> instead.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        public string GetValue(TokenKey key)
        {
            return this.GetToken(key).GetValues().FirstOrDefault();
        }

        /// <summary>Get the current values of the given token for comparison.</summary>
        /// <param name="key">The token key.</param>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        public IEnumerable<string> GetValues(TokenKey key)
        {
            return key.Subkey != null
                ? this.GetToken(key).GetValues(key.Subkey)
                : this.GetToken(key).GetValues();
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
    }
}
