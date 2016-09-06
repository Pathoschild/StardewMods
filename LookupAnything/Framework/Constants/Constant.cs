using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Constants
{
    /// <summary>Constant values hardcoded by the game.</summary>
    public static class Constant
    {
        /// <summary>The number of friendship points per level for a farm animal.</summary>
        /// <remarks>Derived from <see cref="FarmAnimal.dayUpdate"/>.</remarks>
        public static int AnimalFriendshipPointsPerLevel = 200;

        /// <summary>The maximum number of friendship points for a farm animal.</summary>
        /// <remarks>Derived from <see cref="FarmAnimal.dayUpdate"/>.</remarks>
        public static int AnimalFriendshipMaxPoints = 5 * Constant.AnimalFriendshipPointsPerLevel;

        /// <summary>The number of days in each season.</summary>
        public static int DaysInSeason = 28;

        /// <summary>The fractional rate at which fences decay (calculated as minutes divided by this value).</summary>
        /// <remarks>Derived from <see cref="Fence.minutesElapsed"/>.</remarks>
        public static float FenceDecayRate = 1440f;
    }
}
