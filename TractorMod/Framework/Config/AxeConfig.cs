using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathoschild.Stardew.TractorMod.Framework.Config
{
    /// <summary>A set of Axe attachments.</summary>
    internal class AxeConfig
    {
        /// <summary>Whether or not to cut down Fruit Trees.</summary>
        public bool CutFruitTrees { get; set; } = false;

        /// <summary>Whether or not to cut down Tapped Trees.</summary>
        public bool CutTappedTrees { get; set; } = false;

        /// <summary>Whether or not to cut down Trees.</summary>
        public bool CutTrees { get; set; } = false;

        /// <summary>Whether or not to cut down Live Crops.</summary>
        public bool ClearLiveCrops { get; set; } = false;

        /// <summary>Whether or not to cut down Dead Crops.</summary>
        public bool ClearDeadCrops { get; set; } = true;

        /// <summary>Whether or not to clear Debris.</summary>
        public bool ClearDebris { get; set; } = true;
    }
}
