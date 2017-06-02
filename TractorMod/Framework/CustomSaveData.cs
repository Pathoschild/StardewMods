using System.Collections.Generic;
using System.Linq;

namespace TractorMod.Framework
{
    /// <summary>Contains custom data that's stored outside the save file to avoid issues.</summary>
    internal class CustomSaveData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The custom buildings for this save.</summary>
        public CustomSaveBuilding[] Buildings { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <remarks>This constructor is needed to deserialise from JSON.</remarks>
        public CustomSaveData() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="buildings">The custom buildings to save.</param>
        public CustomSaveData(IEnumerable<CustomSaveBuilding> buildings)
        {
            this.Buildings = buildings.ToArray();
        }
    }
}
