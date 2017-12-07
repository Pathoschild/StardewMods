using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>The configuration file for the legacy save format.</summary>
    internal class LegacySaveData
    {
        /// <summary>The data for each save.</summary>
        public LegacySaveEntry[] Saves { get; set; }

        /// <summary>The model for one save's data.</summary>
        internal class LegacySaveEntry
        {
            /// <summary>The player name.</summary>
            public string FarmerName { get; set; }

            /// <summary>The player's save seed.</summary>
            public long SaveSeed { get; set; }

            /// <summary>The positions of the player houses.</summary>
            public Vector2[] TractorHouse { get; set; }
        }
    }
}
