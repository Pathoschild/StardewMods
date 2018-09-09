using System;
using System.Collections.Generic;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token whose value may change depending on the current context.</summary>
    internal interface IToken
    {
        /*********
        ** Properties
        *********/
        /// <summary>The token name.</summary>
        string Name { get; }

        /// <summary>Whether the token is applicable in the current context.</summary>
        bool IsValidInContext { get; }

        /// <summary>Whether a token is restricted to 'true' or 'false'.</summary>
        bool IsBoolean { get; }

        /// <summary>Whether the token may contain multiple values.</summary>
        bool CanHaveMultipleValues { get; }

        /// <summary>Whether this token recognises subkeys (e.g. <c>Relationship:Abigail</c> is a <c>Relationship</c> token with a <c>Abigail</c> subkey).</summary>
        bool RequiresSubkeys { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
        void UpdateContext(IContext context);

        /// <summary>Get the current subkeys (if supported).</summary>
        IEnumerable<string> GetSubkeys();

        /// <summary>Get the current token values.</summary>
        /// <exception cref="InvalidOperationException">This token requires subkeys (see <see cref="IToken.RequiresSubkeys"/>).</exception>
        IEnumerable<string> GetValues();

        /// <summary>Get the current token values for a subkey, if <see cref="RequiresSubkeys"/> is true.</summary>
        /// <param name="subkey">The subkey to check.</param>
        /// <exception cref="InvalidOperationException">This token does not support subkeys (see <see cref="RequiresSubkeys"/>).</exception>
        IEnumerable<string> GetValues(string subkey);
    }
}
