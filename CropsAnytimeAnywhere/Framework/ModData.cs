using System.Collections.Generic;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Framework
{
    /// <summary>The model for the raw data file.</summary>
    internal class ModData
    {
        /// <summary>The tile types to use for back tile IDs which don't have a type property and aren't marked diggable.Indexed by tilesheet image source(without path or season) and type.</summary>
        public IDictionary<string, IDictionary<string, int[]>> FallbackTileTypes { get; set; }
    }
}
