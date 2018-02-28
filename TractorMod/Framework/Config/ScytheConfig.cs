using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>A set of Scythe config.</summary>
    internal class ScytheConfig
    {
        /// <summary>Whether or not to harvest Forage.</summary>
        public bool HarvestForage { get; set; } = true;

        /// <summary>Whether or not to harvest Crops.</summary>
        public bool HarvestCrops { get; set; } = true;

        /// <summary>Whether or not to harvest Flowers.</summary>
        public bool HarvestFlowers { get; set; } = true;

        /// <summary>Whether or not to harvest Fruit Trees.</summary>
        public bool HarvestFruitTrees { get; set; } = true;

        /// <summary>Whether or not to cut down Grass.</summary>
        public bool HarvestGrass { get; set; } = true;

        /// <summary>Whether or not to clear Dead Crops.</summary>
        public bool ClearDeadCrops { get; set; } = true;

        /// <summary>Whether or not to clear Debris.</summary>
        public bool ClearWeeds { get; set; } = true;
    }
}
