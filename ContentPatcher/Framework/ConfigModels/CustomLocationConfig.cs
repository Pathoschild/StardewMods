using System.Runtime.Serialization;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>A custom location to add to the game.</summary>
    internal class CustomLocationConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique location name.</summary>
        public string Name { get; set; }

        /// <summary>The initial map file to load.</summary>
        public string FromMapFile { get; set; }

        /// <summary>The fallback location names to migrate if no location is found matching <see cref="Name"/>.</summary>
        public string[] MigrateLegacyNames { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Normalize the model after it's deserialized.</summary>
        /// <param name="context">The deserialization context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            this.MigrateLegacyNames ??= new string[0];
        }
    }
}
