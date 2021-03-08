using System.Runtime.Serialization;
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
        public ISemanticVersion Format { get; set; }

        /// <summary>The user-defined tokens whose values may depend on other tokens.</summary>
        public DynamicTokenConfig[] DynamicTokens { get; set; }

        /// <summary>The custom locations to add to the game.</summary>
        public CustomLocationConfig[] CustomLocations { get; set; }

        /// <summary>The changes to make.</summary>
        public PatchConfig[] Changes { get; set; }

        /// <summary>The schema for the <c>config.json</c> file (if any).</summary>
        public InvariantDictionary<ConfigSchemaFieldConfig> ConfigSchema { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            this.DynamicTokens ??= new DynamicTokenConfig[0];
            this.CustomLocations ??= new CustomLocationConfig[0];
            this.Changes ??= new PatchConfig[0];
            this.ConfigSchema ??= new InvariantDictionary<ConfigSchemaFieldConfig>();
        }
    }
}
