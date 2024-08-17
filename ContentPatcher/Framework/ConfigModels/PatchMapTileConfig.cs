using Newtonsoft.Json;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The input settings for a <see cref="PatchConfig.MapTiles"/> field.</summary>
    internal class PatchMapTileConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The map layer name to edit.</summary>
        public string? Layer { get; }

        /// <summary>The tile position to edit, relative to the top-left corner.</summary>
        public PatchPositionConfig? Position { get; }

        /// <summary>The tilesheet ID to set.</summary>
        public string? SetTilesheet { get; }

        /// <summary>The tilesheet index to apply, the string <c>false</c> to remove it, or null to leave it as-is.</summary>
        public string? SetIndex { get; }

        /// <summary>The tile properties to set.</summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Auto)]
        public InvariantDictionary<string?> SetProperties { get; }

        /// <summary>Whether to remove the current tile and all its properties.</summary>
        public string? Remove { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="layer">The map layer name to edit.</param>
        /// <param name="position">The tile position to edit, relative to the top-left corner.</param>
        /// <param name="setTilesheet">The tilesheet ID to set.</param>
        /// <param name="setIndex">The tilesheet index to apply, the string <c>false</c> to remove it, or null to leave it as-is.</param>
        /// <param name="setProperties">The tile properties to set.</param>
        /// <param name="remove">Whether to remove the current tile and all its properties.</param>
        [JsonConstructor]
        public PatchMapTileConfig(string? layer, PatchPositionConfig? position, string? setTilesheet, string? setIndex, InvariantDictionary<string?>? setProperties, string? remove)
        {
            this.Layer = layer;
            this.Position = position;
            this.SetTilesheet = setTilesheet;
            this.SetIndex = setIndex;
            this.SetProperties = setProperties ?? new InvariantDictionary<string?>();
            this.Remove = remove;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="other">The other instance to copy.</param>
        public PatchMapTileConfig(PatchMapTileConfig other)
            : this(
                  layer: other.Layer,
                  position: other.Position,
                  setTilesheet: other.SetTilesheet,
                  setIndex: other.SetIndex,
                  setProperties: other.SetProperties,
                  remove: other.Remove
            )
        { }
    }
}
