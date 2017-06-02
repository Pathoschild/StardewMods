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

        /// <summary>The number of days until construction ends.</summary>
        public int DaysOfConstructionLeft { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tile">The building type.</param>
        /// <param name="type">The tile location.</param>
        /// <param name="daysOfConstructionLeft">The number of days until construction ends.</param>
        public CustomSaveBuilding(Vector2 tile, string type, int daysOfConstructionLeft)
        {
            this.Tile = tile;
            this.Type = type;
            this.DaysOfConstructionLeft = daysOfConstructionLeft;
        }
    }
}
