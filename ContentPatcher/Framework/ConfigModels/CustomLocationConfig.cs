using System;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>A custom location to add to the game.</summary>
    internal class CustomLocationConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique location name.</summary>
        public string? Name { get; }

        /// <summary>The initial map file to load.</summary>
        public string? FromMapFile { get; }

        /// <summary>The fallback location names to migrate if no location is found matching <see cref="Name"/>.</summary>
        public string?[] MigrateLegacyNames { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The unique location name.</param>
        /// <param name="fromMapFile">The initial map file to load.</param>
        /// <param name="migrateLegacyNames">The fallback location names to migrate if no location is found matching <paramref name="name"/>.</param>
        public CustomLocationConfig(string? name, string? fromMapFile, string[]? migrateLegacyNames)
        {
            this.Name = name;
            this.FromMapFile = fromMapFile;
            this.MigrateLegacyNames = migrateLegacyNames ?? Array.Empty<string>();
        }
    }
}
