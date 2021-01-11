using StardewValley;

namespace ContentPatcher.Framework.Constants
{
    /// <summary>A farm type.</summary>
    internal enum FarmType
    {
        /// <summary>The standard farm type.</summary>
        Standard = Farm.default_layout,

        /// <summary>The riverland farm type.</summary>
        Riverland = Farm.riverlands_layout,

        /// <summary>The forest farm type.</summary>
        Forest = Farm.forest_layout,

        /// <summary>The hill-top farm type.</summary>
        Hilltop = Farm.mountains_layout,

        /// <summary>The wilderness farm type.</summary>
        Wilderness = Farm.combat_layout,

        /// <summary>The Four Corners farm type.</summary>
        FourCorners = Farm.fourCorners_layout,

        /// <summary>The beach farm type.</summary>
        Beach = Farm.beach_layout,

        /// <summary>A custom farm type.</summary>
        Custom = 100
    }
}
