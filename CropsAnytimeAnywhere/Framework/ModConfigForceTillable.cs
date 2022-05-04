using System.Text.Json.Serialization;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework
{
    /// <summary>The tile types to let the player till, beyond those normally allowed by the game.</summary>
    internal class ModConfigForceTillable
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether to allow tilling dirt tiles not normally allowed by the game.</summary>
        public bool Dirt { get; }

        /// <summary>Whether to allow tilling grass tiles.</summary>
        public bool Grass { get; }

        /// <summary>Whether to allow tilling stone tiles.</summary>
        public bool Stone { get; }

        /// <summary>Whether to allow tilling other tile types (like paths, indoor floors, etc).</summary>
        public bool Other { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="dirt">Whether to allow tilling dirt tiles not normally allowed by the game.</param>
        /// <param name="grass">Whether to allow tilling grass tiles.</param>
        /// <param name="stone">Whether to allow tilling stone tiles.</param>
        /// <param name="other">Whether to allow tilling other tile types (like paths, indoor floors, etc).</param>
        [JsonConstructor]
        public ModConfigForceTillable(bool dirt, bool grass, bool stone, bool other)
        {
            this.Dirt = dirt;
            this.Grass = grass;
            this.Stone = stone;
            this.Other = other;
        }

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
