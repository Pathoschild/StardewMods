using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using ContentPatcher.Framework.Tokens.ValueProviders.ModConvention;
using Pathoschild.Stardew.Common.Utilities;
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

        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The action to add a mod token.</summary>
        private readonly Action<ModProvidedToken> AddModToken;

        /// <summary>A regex pattern matching a valid token name.</summary>
        private readonly Regex ValidNameFormat = new Regex("^[a-z]+$", RegexOptions.IgnoreCase);

        /// <summary>A cache of logged deprecation warnings.</summary>
        private readonly InvariantHashSet LoggedDeprecationWarnings = new InvariantHashSet();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPatcherID">The unique mod ID for Content Patcher.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="addModToken">The action to add a mod token.</param>
        internal ContentPatcherAPI(string contentPatcherID, IMonitor monitor, IReflectionHelper reflection, Action<ModProvidedToken> addModToken)
        {
            this.ContentPatcherID = contentPatcherID;
            this.Monitor = monitor;
            this.Reflection = reflection;
            this.AddModToken = addModToken;
        }

        /// <summary>Register a simple token.</summary>
        /// <param name="mod">The manifest of the mod defining the token (see <see cref="Mod.ModManifest"/> on your entry class).</param>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>YourName.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="getValue">A function which returns the current token value. If this returns a null or empty list, the token is considered unavailable in the current context and any patches or dynamic tokens using it are disabled.</param>
        public void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>> getValue)
        {
            this.RegisterValueProvider(mod, new ModSimpleValueProvider(name, getValue));
        }

        /// <summary>Register a complex token.</summary>
        /// <param name="mod">The manifest of the mod defining the token (see <see cref="Mod.ModManifest"/> on your entry class).</param>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>Pathoschild.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="updateContext">A function which updates the token value (if needed), and returns whether the token changed. Content Patcher will call this method once when it's updating the context (e.g. when a new day starts). The token is 'changed' if it may return a different value *for the same inputs* than before; it's important to report a change correctly, since Content Patcher will use this to decide whether patches need to be rechecked.</param>
        /// <param name="isReady">A function which returns whether the token is available for use. This is always called after <paramref name="updateContext"/>. If this returns false, any patches or dynamic tokens using this token will be disabled. (A token may return true and still have no value, in which case the token value is simply blank.)</param>
        /// <param name="getValue">A function which returns the current value for a given input argument (if any). For example, <c>{{your-mod-id/PlayerInitials}}</c> would result in a null input argument; <c>{{your-mod-id/PlayerInitials:{{PlayerName}}}}</c> would pass in the parsed string after token substitution, like <c>"John Smith"</c>. If the token doesn't use input arguments, you can simply ignore the input.</param>
        /// <param name="allowsInput">Whether the player can provide an input argument (see <paramref name="getValue"/>).</param>
        /// <param name="requiresInput">Whether the token can *only* be used with an input argument (see <paramref name="getValue"/>).</param>
        [Obsolete]
        public void RegisterToken(IManifest mod, string name, Func<bool> updateContext, Func<bool> isReady, Func<string, IEnumerable<string>> getValue, bool allowsInput, bool requiresInput)
        {
            // log deprecation warning
            string warning = $"{mod.Name} used an experimental token API that will break in the next Content Patcher update.";
            if (this.LoggedDeprecationWarnings.Add(warning))
                this.Monitor.Log(warning, LogLevel.Warn);

            // hack to wrap legacy token
            ConventionWrapper wrapper = new ConventionWrapper();
            this.Reflection.GetField<ConventionDelegates.UpdateContext>(wrapper, $"{nameof(wrapper.UpdateContext)}Impl").SetValue(() => updateContext());
            this.Reflection.GetField<ConventionDelegates.IsReady>(wrapper, $"{nameof(wrapper.IsReady)}Impl").SetValue(() => isReady());
            this.Reflection.GetField<ConventionDelegates.GetValues>(wrapper, $"{nameof(wrapper.GetValues)}Impl").SetValue(input => getValue(input));
            this.Reflection.GetField<ConventionDelegates.AllowsInput>(wrapper, $"{nameof(wrapper.AllowsInput)}Impl").SetValue(() => allowsInput);
            this.Reflection.GetField<ConventionDelegates.RequiresInput>(wrapper, $"{nameof(wrapper.RequiresInput)}Impl").SetValue(() => requiresInput);
            this.RegisterValueProvider(mod, new ConventionValueProvider(name, wrapper));
        }

        /// <summary>Register a complex token. This is an advanced API; only use this method if you've read the documentation and are aware of the consequences.</summary>
        /// <param name="mod">The manifest of the mod defining the token (see <see cref="Mod.ModManifest"/> on your entry class).</param>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>YourName.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="token">An arbitrary class with one or more methods from <see cref="ConventionDelegates"/>.</param>
        public void RegisterToken(IManifest mod, string name, object token)
        {
            this.RegisterValueProviderByConvention(mod, name, token);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Register a token.</summary>
        /// <param name="mod">The manifest of the mod defining the token.</param>
        /// <param name="valueProvider">The token value provider.</param>
        private void RegisterValueProvider(IManifest mod, IValueProvider valueProvider)
        {
            // validate token + mod
            if (valueProvider == null)
            {
                this.Monitor.Log($"Rejected token added by {mod.Name} because the token is null.", LogLevel.Error);
                return;
            }
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

        /// <summary>Register a token which defines methods by duck-typing convention.</summary>
        /// <param name="mod">The manifest of the mod defining the token.</param>
        /// <param name="name">The token name.</param>
        /// <param name="provider">The token value provider.</param>
        private void RegisterValueProviderByConvention(IManifest mod, string name, object provider)
        {
            // validate token
            if (provider == null)
            {
                this.Monitor.Log($"Rejected token '{name}' added by {mod.Name} because the token is null.", LogLevel.Error);
                return;
            }

            // get a strongly-typed wrapper
            if (!ConventionWrapper.TryCreate(provider, this.Reflection, out ConventionWrapper wrapper, out string error))
            {
                this.Monitor.Log($"Rejected token '{name}' added by {mod.Name} because it could not be mapped: {error}", LogLevel.Error);
                return;
            }

            // register
            this.RegisterValueProvider(mod, new ConventionValueProvider(name, wrapper));
        }
    }
}
