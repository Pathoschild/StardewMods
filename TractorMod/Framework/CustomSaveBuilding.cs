using Microsoft.Xna.Framework;

namespace TractorMod.Framework
{
    /// <summary>Metadata for a stashed building.</summary>
    internal class CustomSaveBuilding
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The building type.</summary>
        public string Type { get; }

        /// <summary>The tile location.</summary>
        public Vector2 Tile { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tile">The building type.</param>
        /// <param name="type">The tile location.</param>
        public CustomSaveBuilding(Vector2 tile, string type)
        {
            this.Tile = tile;
            this.Type = type;
        }
    }
}
