using StardewValley;

namespace ContentPatcher.Framework.Constants
{
    /// <summary>A general world area defined by the game.</summary>
    internal enum LocationContext
    {
        /// <summary>The Ginger Island areas.</summary>
        Island = GameLocation.LocationContext.Island,

        /// <summary>The valley (i.e. non-island) areas.</summary>
        Valley = GameLocation.LocationContext.Default
    }
}
