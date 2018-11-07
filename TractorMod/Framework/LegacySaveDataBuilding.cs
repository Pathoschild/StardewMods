using System;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Metadata for a stashed building.</summary>
    internal class LegacySaveDataBuilding
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The tile location.</summary>
        public Vector2 Tile { get; }

        /// <summary>The associated tractor ID.</summary>
        public Guid TractorID { get; set; }

        /// <summary>The associated tractor's hat ID.</summary>
        public int? TractorHatID { get; set; }

        /// <summary>The building type.</summary>
        public string Type { get; }

        /// <summary>The number of days until construction ends.</summary>
        public int DaysOfConstructionLeft { get; }

        /// <summary>The name of the map containing the building.</summary>
        public string Map { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tile">The building type.</param>
        /// <param name="tractorID">The associated tractor ID.</param>
        /// <param name="tractorHatID">The associated tractor's hat ID.</param>
        /// <param name="type">The tile location.</param>
        /// <param name="map">The name of the map containing the building.</param>
        /// <param name="daysOfConstructionLeft">The number of days until construction ends.</param>
        public LegacySaveDataBuilding(Vector2 tile, Guid tractorID, int? tractorHatID, string type, string map, int daysOfConstructionLeft)
        {
            this.Tile = tile;
            this.TractorID = tractorID != Guid.Empty ? tractorID : Guid.NewGuid(); // assign ID for older data
            this.TractorHatID = tractorHatID;
            this.Type = type;
            this.Map = map;
            this.DaysOfConstructionLeft = daysOfConstructionLeft;
        }
    }
}
