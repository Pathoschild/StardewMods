using System;
using System.Collections.Generic;
using ContentPatcher.Framework;
using StardewModdingAPI;

namespace ContentPatcher
{
    /// <summary>The Content Patcher API which other mods can access.</summary>
    internal interface IContentPatcherAPI
    {
        /*********
        ** Methods
        *********/
        /// <summary>Register a token.</summary>
        /// <param name="mod">The mod this token comes from.</param>
        /// <param name="name">The token name.</param>
        /// <param name="isReady">Get whether the token is valid in the current context.</param>
        /// <param name="updateContext">Update the token if needed, and return <c>true</c> if the token changed (so any tokens using it should be rechecked).</param>
        /// <param name="getValue">Get the current token value when invoked. If this returns <c>null</c> (not an empty string), the token will be marked unavailable in the current context.</param>
        /// <param name="allowsInput">Whether the value provider allows an input argument (like {{tokenName : inputArgument}}).</param>
        /// <param name="requiresInput">Whether an input argument is required when using this value provider.</param>
        void RegisterToken(IManifest mod, string name, Func<bool> isReady, Func<bool> updateContext, Func<ITokenString, IEnumerable<string>> getValue, bool allowsInput, bool requiresInput);
    }
}
