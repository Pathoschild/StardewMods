using Newtonsoft.Json;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The input settings for a tile position field in <see cref="PatchConfig"/>.</summary>
    internal class PatchPositionConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The X position.</summary>
        public string? X { get; }

        /// <summary>The Y position.</summary>
        public string? Y { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        [JsonConstructor]
        public PatchPositionConfig(string? x, string? y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="other">The other instance to copy.</param>
        public PatchPositionConfig(PatchRectangleConfig other)
            : this(other.X, other.Y) { }
    }
}
