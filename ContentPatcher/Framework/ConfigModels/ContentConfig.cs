using System;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The model for a content patch file.</summary>
    internal class ContentConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The format version.</summary>
        public ISemanticVersion? Format { get; }

        /// <summary>The user-defined tokens whose values may depend on other tokens.</summary>
        public DynamicTokenConfig[] DynamicTokens { get; }

        /// <summary>The user-defined alias token names.</summary>
        public InvariantDictionary<string?> AliasTokenNames { get; }

        /// <summary>The custom locations to add to the game.</summary>
        public CustomLocationConfig?[] CustomLocations { get; }

        /// <summary>The changes to make.</summary>
        public PatchConfig[] Changes { get; private set; }

        /// <summary>The schema for the <c>config.json</c> file (if any).</summary>
        public InvariantDictionary<ConfigSchemaFieldConfig?> ConfigSchema { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="format">The format version.</param>
        /// <param name="dynamicTokens">The user-defined tokens whose values may depend on other tokens.</param>
        /// <param name="aliasTokenNames">The user-defined alias token names.</param>
        /// <param name="customLocations">The custom locations to add to the game.</param>
        /// <param name="changes">The changes to make.</param>
        /// <param name="configSchema">The schema for the <c>config.json</c> file (if any).</param>
        public ContentConfig(ISemanticVersion? format, DynamicTokenConfig?[]? dynamicTokens, InvariantDictionary<string?>? aliasTokenNames, CustomLocationConfig?[]? customLocations, PatchConfig?[]? changes, InvariantDictionary<ConfigSchemaFieldConfig?>? configSchema)
        {
            this.Format = format;
            this.DynamicTokens = dynamicTokens?.WhereNotNull().ToArray() ?? Array.Empty<DynamicTokenConfig>();
            this.AliasTokenNames = aliasTokenNames ?? new InvariantDictionary<string?>();
            this.CustomLocations = customLocations ?? Array.Empty<CustomLocationConfig>();
            this.Changes = changes?.WhereNotNull().ToArray() ?? Array.Empty<PatchConfig>();
            this.ConfigSchema = configSchema ?? new InvariantDictionary<ConfigSchemaFieldConfig?>();
        }
    }
}
