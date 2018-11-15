using System.Collections.Generic;
using System.Linq;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Contains legacy data that's stored in the save file.</summary>
    internal class LegacySaveData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The custom buildings for this save.</summary>
        public LegacySaveDataBuilding[] Buildings { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <remarks>This constructor is needed to deserialise from JSON.</remarks>
        public LegacySaveData() { }

        /// <summary>Construct an instance.</summary>
        /// <param name="buildings">The custom buildings to save.</param>
        public LegacySaveData(IEnumerable<LegacySaveDataBuilding> buildings)
        {
            this.Buildings = buildings.ToArray();
        }
    }
}
