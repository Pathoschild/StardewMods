using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace ContentPatcher
{
    /// <summary>The Content Patcher API which other mods can access.</summary>
    public interface IContentPatcherAPI
    {
        /*********
        ** Methods
        *********/
        /// <summary>Register a token.</summary>
        /// <param name="mod">The manifest of the mod defining the token (see <see cref="Mod.ModManifest"/> on your entry class).</param>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>Pathoschild.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="getValue">A function which returns the current token value. If this returns a null or empty list, the token is considered unavailable in the current context and any patches or dynamic tokens using it are disabled.</param>
        void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>> getValue);

        /// <summary>Register a token.</summary>
        /// <param name="mod">The manifest of the mod defining the token (see <see cref="Mod.ModManifest"/> on your entry class).</param>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>Pathoschild.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="updateContext">A function which updates the token value (if needed), and returns whether the token changed. Content Patcher will call this method once when it's updating the context (e.g. when a new day starts). The token is 'changed' if it may return a different value *for the same inputs* than before; it's important to report a change correctly, since Content Patcher will use this to decide whether patches need to be rechecked.</param>
        /// <param name="isReady">A function which returns whether the token is available for use. This is always called after <paramref name="updateContext"/>. If this returns false, any patches or dynamic tokens using this token will be disabled. (A token may return true and still have no value, in which case the token value is simply blank.)</param>
        /// <param name="getValue">A function which returns the current value for a given input argument (if any). For example, <c>{{your-mod-id/PlayerInitials}}</c> would result in a null input argument; <c>{{your-mod-id/PlayerInitials:{{PlayerName}}}}</c> would pass in the parsed string after token substitution, like <c>"John Smith"</c>. If the token doesn't use input arguments, you can simply ignore the input.</param>
        /// <param name="allowsInput">Whether the player can provide an input argument (see <paramref name="getValue"/>).</param>
        /// <param name="requiresInput">Whether the token can *only* be used with an input argument (see <paramref name="getValue"/>).</param>
        void RegisterToken(IManifest mod, string name, Func<bool> updateContext, Func<bool> isReady, Func<string, IEnumerable<string>> getValue, bool allowsInput, bool requiresInput);
    }
}
