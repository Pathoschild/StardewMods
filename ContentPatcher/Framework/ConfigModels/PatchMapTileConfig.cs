using System.Collections.Generic;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The input settings for a <see cref="PatchConfig.MapTiles"/> field.</summary>
    internal class PatchMapTileConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The map layer name to edit.</summary>
        public string Layer { get; set; }

        /// <summary>The tile position to edit, relative to the top-left corner.</summary>
        public PatchPositionConfig Position { get; set; }

        /// <summary>The tilesheet ID to set.</summary>
        public string SetTilesheet { get; set; }

        /// <summary>The tilesheet index to apply, the string <c>false</c> to remove it, or null to leave it as-is.</summary>
        public string SetIndex { get; set; }

        /// <summary>The tile properties to set.</summary>
        public IDictionary<string, string> SetProperties { get; set; }

        /// <summary>Whether to remove the current tile and all its properties.</summary>
        public string Remove { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public PatchMapTileConfig() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="other">The other instance to copy.</param>
        public PatchMapTileConfig(PatchMapTileConfig other)
        {
            this.Position = other.Position;
            this.Layer = other.Layer;
            this.SetIndex = other.SetIndex;
            this.SetTilesheet = other.SetTilesheet;
            this.SetProperties = other.SetProperties;
        }
    }
}
