using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token whose value may change depending on the current context.</summary>
    internal interface IToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>The token name.</summary>
        TokenName Name { get; }

        /// <summary>Whether the token is applicable in the current context.</summary>
        bool IsValidInContext { get; }

        /// <summary>Whether the value can change after it's initialised.</summary>
        bool IsMutable { get; }

        /// <summary>Whether the token may contain multiple values.</summary>
        bool CanHaveMultipleValues { get; }

        /// <summary>Whether this token recognises subkeys (e.g. <c>Relationship:Abigail</c> is a <c>Relationship</c> token with a <c>Abigail</c> subkey).</summary>
        bool RequiresSubkeys { get; }

        /// <summary>The allowed values (or <c>null</c> if any value is allowed).</summary>
        InvariantHashSet AllowedValues { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        void UpdateContext(IContext context);

        /// <summary>Get the current subkeys (if supported).</summary>
        IEnumerable<TokenName> GetSubkeys();

        /// <summary>Get the current token values.</summary>
        /// <param name="name">The token name to check, if applicable.</param>
        /// <exception cref="InvalidOperationException">The key doesn't match this token, or the key does not respect <see cref="IToken.RequiresSubkeys"/>.</exception>
        IEnumerable<string> GetValues(TokenName? name = null);
    }
}
