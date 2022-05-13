using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>Provides access to contextual tokens.</summary>
    internal interface IContext
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The number of context updates since the game was launched, including the current one if applicable.</summary>
        /// <remarks>This can be used to detect whether something has already been updated for the current context update, for cases where multiple tokens have common update logic.</remarks>
        int UpdateTick { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Get whether a mod is installed.</summary>
        /// <param name="id">The mod ID.</param>
        bool IsModInstalled(string id);

        /// <summary>Get whether the context contains the given token.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        bool Contains(string name, bool enforceContext);

        /// <summary>Get the underlying token which handles a key.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Returns the matching token, or <c>null</c> if none was found.</returns>
        IToken? GetToken(string name, bool enforceContext);

        /// <summary>Get the underlying tokens.</summary>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        IEnumerable<IToken> GetTokens(bool enforceContext);

        /// <summary>Get the current values of the given token for comparison.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="input">The input arguments.</param>
        /// <param name="enforceContext">Whether to only consider tokens that are available in the context.</param>
        /// <returns>Return the values of the matching token, or an empty list if the token doesn't exist.</returns>
        /// <exception cref="ArgumentNullException">The specified key is null.</exception>
        IImmutableSet<string> GetValues(string name, IInputArguments input, bool enforceContext);
    }
}
