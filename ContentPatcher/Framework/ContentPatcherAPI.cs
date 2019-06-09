using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using StardewModdingAPI;

namespace ContentPatcher.Framework
{
    /// <summary>The Content Patcher API which other mods can access.</summary>
    public class ContentPatcherAPI : IContentPatcherAPI
    {
        /*********
        ** Fields
        *********/
        /// <summary>The unique mod ID for Content Patcher.</summary>
        private readonly string ContentPatcherID;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>The action to add a mod token.</summary>
        private readonly Action<ModProvidedToken> AddModToken;

        /// <summary>A regex pattern matching a valid token name.</summary>
        private readonly Regex ValidNameFormat = new Regex("^[a-z]+$", RegexOptions.IgnoreCase);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPatcherID">The unique mod ID for Content Patcher.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="addModToken">The action to add a mod token.</param>
        internal ContentPatcherAPI(string contentPatcherID, IMonitor monitor, Action<ModProvidedToken> addModToken)
        {
            this.ContentPatcherID = contentPatcherID;
            this.Monitor = monitor;
            this.AddModToken = addModToken;
        }

        /// <summary>Register a token.</summary>
        /// <param name="mod">The manifest of the mod defining the token (see <see cref="Mod.ModManifest"/> on your entry class).</param>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>Pathoschild.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="getValue">A function which returns the current token value. If this returns a null or empty list, the token is considered unavailable in the current context and any patches or dynamic tokens using it are disabled.</param>
        public void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>> getValue)
        {
            this.RegisterToken(mod, new ModSimpleValueProvider(name, getValue));
        }

        /// <summary>Register a token.</summary>
        /// <param name="mod">The manifest of the mod defining the token (see <see cref="Mod.ModManifest"/> on your entry class).</param>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>Pathoschild.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="updateContext">A function which updates the token value (if needed), and returns whether the token changed. Content Patcher will call this method once when it's updating the context (e.g. when a new day starts). The token is 'changed' if it may return a different value *for the same inputs* than before; it's important to report a change correctly, since Content Patcher will use this to decide whether patches need to be rechecked.</param>
        /// <param name="isReady">A function which returns whether the token is available for use. This is always called after <paramref name="updateContext"/>. If this returns false, any patches or dynamic tokens using this token will be disabled. (A token may return true and still have no value, in which case the token value is simply blank.)</param>
        /// <param name="getValue">A function which returns the current value for a given input argument (if any). For example, <c>{{your-mod-id/PlayerInitials}}</c> would result in a null input argument; <c>{{your-mod-id/PlayerInitials:{{PlayerName}}}}</c> would pass in the parsed string after token substitution, like <c>"John Smith"</c>. If the token doesn't use input arguments, you can simply ignore the input.</param>
        /// <param name="allowsInput">Whether the player can provide an input argument (see <paramref name="getValue"/>).</param>
        /// <param name="requiresInput">Whether the token can *only* be used with an input argument (see <paramref name="getValue"/>).</param>
        public void RegisterToken(IManifest mod, string name, Func<bool> updateContext, Func<bool> isReady, Func<string, IEnumerable<string>> getValue, bool allowsInput, bool requiresInput)
        {
            this.RegisterToken(mod, new ModComplexValueProvider(name, isReady, updateContext, getValue, allowsInput, requiresInput));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Register a token.</summary>
        /// <param name="mod">The manifest of the mod defining the token (see <see cref="Mod.ModManifest"/> on your entry class).</param>
        /// <param name="valueProvider">The token value provider.</param>
        private void RegisterToken(IManifest mod, IValueProvider valueProvider)
        {
            // validate mod
            if (!mod.HasDependency(this.ContentPatcherID))
            {
                this.Monitor.Log($"Rejected token added by {mod.Name} because that mod doesn't list Content Patcher as a dependency.", LogLevel.Error);
                return;
            }

            // validate name format
            string name = valueProvider.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                this.Monitor.Log($"Rejected token added by {mod.Name} because the token has no name.", LogLevel.Error);
                return;
            }
            if (!this.ValidNameFormat.IsMatch(name))
            {
                this.Monitor.Log($"Rejected token added by {mod.Name} because the token name is invalid (it can only contain alphabetical characters).", LogLevel.Error);
                return;
            }

            // format name
            name = $"{mod.UniqueID}{InternalConstants.ModTokenSeparator}{name}";
            this.Monitor.Log($"{mod.Name} added a token: {name}", LogLevel.Trace);
            ModProvidedToken token = new ModProvidedToken(name, mod, valueProvider);

            // add token
            this.AddModToken(token);
        }
    }
}
