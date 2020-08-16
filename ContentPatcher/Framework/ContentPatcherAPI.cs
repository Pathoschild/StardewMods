using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using ContentPatcher.Framework.Tokens.ValueProviders.ModConvention;
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
            ModProvidedToken token = new ModProvidedToken(name, mod, valueProvider, this.Monitor);

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
