using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using StardewModdingAPI;

namespace ContentPatcher.Framework
{
    /// <summary>The Content Patcher API which other mods can access.</summary>
    internal class ContentPatcherAPI : IContentPatcherAPI
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
        public ContentPatcherAPI(string contentPatcherID, IMonitor monitor, Action<ModProvidedToken> addModToken)
        {
            this.ContentPatcherID = contentPatcherID;
            this.Monitor = monitor;
            this.AddModToken = addModToken;
        }

        /// <summary>Register a token.</summary>
        /// <param name="mod">The mod this token comes from.</param>
        /// <param name="name">The token name.</param>
        /// <param name="isReady">Get whether the token is valid in the current context.</param>
        /// <param name="updateContext">Update the token if needed, and return <c>true</c> if the token changed (so any tokens using it should be rechecked).</param>
        /// <param name="getValue">Get the current token value for a given input argument. If this returns <c>null</c> (not an empty string), the token will be marked unavailable in the current context.</param>
        /// <param name="allowsInput">Whether the value provider allows an input argument (like {{tokenName : inputArgument}}).</param>
        /// <param name="requiresInput">Whether an input argument is required when using this value provider.</param>
        public void RegisterToken(IManifest mod, string name, Func<bool> isReady, Func<bool> updateContext, Func<string, IEnumerable<string>> getValue, bool allowsInput, bool requiresInput)
        {
            this.RegisterToken(mod, new ModValueProvider(name, isReady, updateContext, getValue, allowsInput, requiresInput));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Register a token.</summary>
        /// <param name="mod">The mod this token comes from.</param>
        /// <param name="valueProvider">The token value provider.</param>
        protected void RegisterToken(IManifest mod, IValueProvider valueProvider)
        {
            // validate mod
            if (!mod.HasDependency(this.ContentPatcherID))
                throw new InvalidOperationException($"{mod.Name} can't register a Content Patcher token because it doesn't list Content Patcher as a dependency.");

            // validate name format
            string name = valueProvider.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Can't register a token with no name.", nameof(valueProvider));
            if (!this.ValidNameFormat.IsMatch(name))
                throw new ArgumentException("Can't register a token with an invalid name. Token names can only contain alphabetical characters.", nameof(valueProvider));

            // format name
            name = $"{mod.UniqueID}{InternalConstants.ModTokenSeparator}{name}";
            this.Monitor.Log($"{mod.Name} added a token: {name}", LogLevel.Trace);
            ModProvidedToken token = new ModProvidedToken(name, mod, valueProvider);
            this.AddModToken(token);
        }
    }
}
