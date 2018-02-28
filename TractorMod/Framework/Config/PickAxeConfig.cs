using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>A set of PickAxe config.</summary>
    internal class PickAxeConfig
    {
        /// <summary>Whether or not to clear Debris.</summary>
        public bool ClearDebris { get; set; } = true;

        /// <summary>Whether or not to clear Dead Crops.</summary>
        public bool ClearDeadCrops { get; set; } = true;

        /// <summary>Whether or not to clear Tilled Dirt.</summary>
        public bool ClearDirt { get; set; } = true;

        /// <summary>Whether or not to clear Flooring.</summary>
        public bool ClearFlooring { get; set; } = false;

        /// <summary>Whether or not to clear Boulders / Meteorites.</summary>
        public bool ClearBoulders { get; set; } = true;
    }
}
