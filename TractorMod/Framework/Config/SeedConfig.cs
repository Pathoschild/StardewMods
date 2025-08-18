using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    internal class SeedConfig
    {
        /// <summary>Whether to enable the attachment.</summary>
        public bool EnableSeeds { get; set; } = true;

        /// <summary>Whether to be able to plant tree seeds.</summary>
        public bool EnableTreeSeeds { get; set; } = false;

        /// <summary>Whether to use tree fertilizer from inventory when planting trees.</summary>
        public bool UseFertilizerForPlantingTrees { get; set; } = false;
    }
}
