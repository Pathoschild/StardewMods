namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework
{
    /// <summary>The tile types to let the player till, beyond those normally allowed by the game.</summary>
    internal class ModConfigForceTillable
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Dirt tiles not normally allowed by the game.</summary>
        public bool Dirt { get; set; } = true;

        /// <summary>Grass tiles.</summary>
        public bool Grass { get; set; } = true;

        /// <summary>Stone tiles.</summary>
        public bool Stone { get; set; } = false;

        /// <summary>Any other non-grass tiles (like paths, indoor floors, etc).</summary>
        public bool Other { get; set; } = false;


        /*********
        ** Public methods
        *********/
        /// <summary>Whether any of the options are enabled.</summary>
        public bool IsAnyEnabled()
        {
            return
                this.Dirt
                || this.Grass
                || this.Stone
                || this.Other;
        }
    }
}
