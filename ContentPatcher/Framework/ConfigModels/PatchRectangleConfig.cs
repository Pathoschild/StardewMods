using Newtonsoft.Json;

namespace ContentPatcher.Framework.ConfigModels
{
    /// <summary>The input settings for a Rectangle field in <see cref="PatchConfig"/>.</summary>
    internal class PatchRectangleConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The X position of the rectangle.</summary>
        public string? X { get; }

        /// <summary>The Y position of the rectangle.</summary>
        public string? Y { get; }

        /// <summary>The width of the rectangle.</summary>
        public string? Width { get; }

        /// <summary>The height of the rectangle.</summary>
        public string? Height { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="x">The X position of the rectangle.</param>
        /// <param name="y">The Y position of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        [JsonConstructor]
        public PatchRectangleConfig(string? x, string? y, string? width, string? height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="other">The other instance to copy.</param>
        public PatchRectangleConfig(PatchRectangleConfig other)
            : this(other.X, other.Y, other.Width, other.Height) { }
    }
}
