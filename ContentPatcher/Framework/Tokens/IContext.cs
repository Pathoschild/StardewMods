using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>Provides access to contextual tokens.</summary>
    internal interface IContext
    {
        /// <summary>Get the underlying token which handles a key.</summary>
        /// <param name="key">The token key.</param>
        /// <returns>Returns the matching token, or <c>null</c> if none was found.</returns>
        IToken GetToken(TokenKey key);

        /// <summary>Get the underlying tokens.</summary>
        IEnumerable<IToken> GetTokens();

        /// <summary>Get the current value of the given token for comparison. This is only valid for tokens where <see cref="IToken.CanHaveMultipleValues"/> is false; see <see cref="GetValues(string)"/> otherwise.</summary>
        /// <param name="key">The token key. This may include a subkey, like <c>Relationship:Abigail</c>.</param>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        /// <exception cref="InvalidOperationException">The specified token allows multiple values; see <see cref="GetValues(string)"/> instead.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        string GetValue(string key);

        /// <summary>Get the current value of the given token for comparison. This is only valid for tokens where <see cref="IToken.CanHaveMultipleValues"/> is false; see <see cref="IContext.GetValues"/> otherwise.</summary>
        /// <param name="key">The token key.</param>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        /// <exception cref="ArgumentException">The specified key does includes or doesn't include a subkey, depending on <see cref="IToken.RequiresSubkeys"/>.</exception>
        /// <exception cref="InvalidOperationException">The specified token allows multiple values; see <see cref="IContext.GetValues"/> instead.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        string GetValue(TokenKey key);

        /// <summary>Get the current values of the given token for comparison.</summary>
        /// <param name="key">The token key. This may include a subkey, like <c>Relationship:Abigail</c>.</param>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        IEnumerable<string> GetValues(string key);

        /// <summary>Get the current values of the given token for comparison.</summary>
        /// <param name="key">The token key.</param>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        /// <exception cref="KeyNotFoundException">The specified token key doesn't exist.</exception>
        IEnumerable<string> GetValues(TokenKey key);

        /// <summary>Get the tokens that can only contain one value.</summary>
        InvariantDictionary<IToken> GetSingleValues();
    }
}
