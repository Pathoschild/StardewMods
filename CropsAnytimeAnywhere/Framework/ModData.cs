using System.Collections.Generic;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework
{
    /// <summary>The model for the raw data file.</summary>
    internal class ModData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The tile types to use for back tile IDs which don't have a type property and aren't marked diggable. Indexed by tilesheet image source (without path or season) and type.</summary>
        public Dictionary<string, Dictionary<string, int[]>> FallbackTileTypes { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fallbackTileTypes">The tile types to use for back tile IDs which don't have a type property and aren't marked diggable. Indexed by tilesheet image source (without path or season) and type.</param>
        public ModData(Dictionary<string, Dictionary<string, int[]>>? fallbackTileTypes)
        {
            this.FallbackTileTypes = fallbackTileTypes ?? new();
        }
    }
}
