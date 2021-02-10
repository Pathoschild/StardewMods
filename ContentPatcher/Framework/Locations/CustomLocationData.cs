using System.IO;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Locations
{
    /// <summary>A custom location to add to the game.</summary>
    internal class CustomLocationData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The unique location name.</summary>
        public string Name { get; }

        /// <summary>The initial map file to load.</summary>
        public string FromMapFile { get; }

        /// <summary>The public map path visible to the game.</summary>
        public string PublicMapPath { get; }

        /// <summary>The content pack which added the location.</summary>
        public IContentPack ContentPack { get; }

        /// <summary>Whether the location is enabled.</summary>
        public bool IsEnabled { get; private set; }

        /// <summary>The reason the custom location is disabled, if applicable.</summary>
        public string Error { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The unique location name.</param>
        /// <param name="fromMapFile">The initial map file to load.</param>
        /// <param name="contentPack">The content pack which added the location.</param>
        public CustomLocationData(string name, string fromMapFile, IContentPack contentPack)
        {
            this.Name = name;
            this.FromMapFile = fromMapFile;
            this.ContentPack = contentPack;
            this.PublicMapPath = PathUtilities.NormalizePath(Path.Combine("Maps", name));
            this.IsEnabled = true;
        }

        /// <summary>Mark the location as disabled.</summary>
        /// <param name="error">The error reason to set.</param>
        public void Disable(string error)
        {
            this.IsEnabled = false;
            this.Error = error;
        }
    }
}
