using System.Collections.Generic;

namespace Pathoschild.Stardew.CropsInAnySeason.Framework
{
    /// <summary>The mod configuration for enabled seasons.</summary>
    public class ModSeasonsConfig
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to enable the mod in spring.</summary>
        public bool Spring { get; set; } = true;

        /// <summary>Whether to enable the mod in summer.</summary>
        public bool Summer { get; set; } = true;

        /// <summary>Whether to enable the mod in fall.</summary>
        public bool Fall { get; set; } = true;

        /// <summary>Whether to enable the mod in winter.</summary>
        public bool Winter { get; set; } = true;


        /*********
        ** Public methods
        *********/
        /// <summary>Get the enabled seasons.</summary>
        public IEnumerable<string> GetEnabledSeasons()
        {
            if (this.Spring)
                yield return "spring";
            if (this.Summer)
                yield return "summer";
            if (this.Fall)
                yield return "fall";
            if (this.Winter)
                yield return "winter";
        }
    }
}
