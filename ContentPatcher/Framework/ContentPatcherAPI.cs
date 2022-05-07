using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        /// <summary>A pattern which matches a valid token name.</summary>
        private static readonly Regex ValidNamePattern = new("^[a-z]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>The unique mod ID for Content Patcher.</summary>
        private readonly string ContentPatcherID;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;

        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The action to add a mod token.</summary>
        private readonly Action<ModProvidedToken> AddModToken;

        /// <summary>Whether the conditions API is initialized and ready for use.</summary>
        private readonly Func<bool> IsConditionsApiReadyImpl;

        /// <summary>Parse raw conditions for an API consumer.</summary>
        private readonly ParseConditionsDelegate ParseConditionsImpl;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public bool IsConditionsApiReady => this.IsConditionsApiReadyImpl();


        /*********
        ** Public delegates
        *********/
        /// <summary>Parse raw conditions for an API consumer.</summary>
        /// <param name="manifest">The manifest of the mod parsing the conditions.</param>
        /// <param name="rawConditions">The raw conditions to parse.</param>
        /// <param name="formatVersion">The format version for which to parse conditions.</param>
        /// <param name="assumeModIds">The unique IDs of mods whose custom tokens to allow in the <paramref name="rawConditions"/>.</param>
        internal delegate IManagedConditions ParseConditionsDelegate(IManifest manifest, InvariantDictionary<string?>? rawConditions, ISemanticVersion formatVersion, string[]? assumeModIds = null);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPatcherID">The unique mod ID for Content Patcher.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="addModToken">The action to add a mod token.</param>
        /// <param name="isConditionsApiReady">Whether the conditions API is initialized and ready for use.</param>
        /// <param name="parseConditions">Parse raw conditions for an API consumer.</param>
        internal ContentPatcherAPI(string contentPatcherID, IMonitor monitor, IReflectionHelper reflection, Action<ModProvidedToken> addModToken, Func<bool> isConditionsApiReady, ParseConditionsDelegate parseConditions)
        {
            this.ContentPatcherID = contentPatcherID;
            this.Monitor = monitor;
            this.Reflection = reflection;
            this.AddModToken = addModToken;
            this.IsConditionsApiReadyImpl = isConditionsApiReady;
            this.ParseConditionsImpl = parseConditions;
        }

        /// <inheritdoc />
        public IManagedConditions ParseConditions(IManifest manifest, IDictionary<string, string?>? rawConditions, ISemanticVersion formatVersion, string[]? assumeModIds = null)
        {
            // validate lifecycle
            if (!this.IsConditionsApiReady)
                throw new InvalidOperationException($"'{manifest.Name}' accessed Content Patcher's conditions API before it was ready to use. (For mod authors: see the documentation on {nameof(IContentPatcherAPI)}.{nameof(IContentPatcherAPI.IsConditionsApiReady)} for details.)");

            // validate dependency on Content Patcher
            if (!manifest.HasDependency(this.ContentPatcherID, out ISemanticVersion? minVersion, canBeOptional: false))
                throw new InvalidOperationException($"'{manifest.Name}' must list Content Patcher as a required dependency in its manifest.json to access the conditions API.");
            if (minVersion == null || minVersion.IsOlderThan("1.22.0"))
                throw new InvalidOperationException($"'{manifest.Name}' must specify Content Patcher 1.22.0 as the minimum required version in its manifest.json to access the conditions API.");

            // parse conditions
            InvariantDictionary<string?>? conditions = rawConditions?.Any() == true
               ? new(rawConditions)
               : null;
            return this.ParseConditionsImpl(manifest, conditions, formatVersion, assumeModIds);
        }

        /// <inheritdoc />
        public void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>?> getValue)
        {
            this.RegisterValueProvider(mod, new ModSimpleValueProvider(name, getValue));
        }

        /// <inheritdoc />
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
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse", Justification = "The validation ensures input values match the expected nullability.")]
        [SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier", Justification = "The validation ensures input values match the expected nullability.")]
        private void RegisterValueProvider(IManifest mod, IValueProvider valueProvider)
        {
            // validate token + mod
            if (valueProvider is null)
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
            string? name = valueProvider.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                this.Monitor.Log($"Rejected token added by {mod.Name} because the token has no name.", LogLevel.Error);
                return;
            }
            if (!ContentPatcherAPI.ValidNamePattern.IsMatch(name))
            {
                this.Monitor.Log($"Rejected token added by {mod.Name} because the token name is invalid (it can only contain alphabetical characters).", LogLevel.Error);
                return;
            }

            // format name
            ModProvidedToken token = new(name, mod, valueProvider, this.Monitor);

            // add token
            this.AddModToken(token);
        }

        /// <summary>Register a token which defines methods by duck-typing convention.</summary>
        /// <param name="mod">The manifest of the mod defining the token.</param>
        /// <param name="name">The token name.</param>
        /// <param name="provider">The token value provider.</param>
        private void RegisterValueProviderByConvention(IManifest mod, string name, object? provider)
        {
            // validate token
            if (provider == null)
            {
                this.Monitor.Log($"Rejected token '{name}' added by {mod.Name} because the token is null.", LogLevel.Error);
                return;
            }

            // get a strongly-typed wrapper
            if (!ConventionWrapper.TryCreate(provider, this.Reflection, out ConventionWrapper? wrapper, out string? error))
            {
                this.Monitor.Log($"Rejected token '{name}' added by {mod.Name} because it could not be mapped: {error}", LogLevel.Error);
                return;
            }

            // register
            this.RegisterValueProvider(mod, new ConventionValueProvider(name, wrapper));
        }
    }
}
