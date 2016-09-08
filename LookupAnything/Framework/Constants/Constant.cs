using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.LookupAnything.Framework.Constants
{
    /// <summary>Constant values hardcoded by the game.</summary>
    public static class Constant
    {
        /*********
        ** Accessors
        *********/
        /****
        ** Targeting
        ****/
        /// <summary>The largest expected sprite size (measured in tiles).</summary>
        /// <remarks>This is used to account for sprites that extend beyond their tile when searching for targets. These values should be large enough to cover the largest target sprites, but small enough to minimise expensive cursor collision checks.</remarks>
        public static readonly Vector2 MaxTargetSpriteSize = new Vector2(3, 5);

        /****
        ** Animals
        ****/
        /// <summary>The number of friendship points per level for a farm animal.</summary>
        /// <remarks>Derived from <see cref="FarmAnimal.dayUpdate"/>.</remarks>
        public const int AnimalFriendshipPointsPerLevel = 200;

        /// <summary>The maximum number of friendship points for a farm animal.</summary>
        /// <remarks>Derived from <see cref="FarmAnimal.dayUpdate"/>.</remarks>
        public const int AnimalFriendshipMaxPoints = 5 * Constant.AnimalFriendshipPointsPerLevel;

        /****
        ** NPCs
        ****/
        /// <summary>The villagers which have no social data (e.g. birthdays or gift tastes).</summary>
        public static readonly string[] DisableVillagerFields = { "Bouncer", "Gunther" };

        /****
        ** Time
        ****/
        /// <summary>The number of days in each season.</summary>
        public const int DaysInSeason = 28;

        /// <summary>The fractional rate at which fences decay (calculated as minutes divided by this value).</summary>
        /// <remarks>Derived from <see cref="Fence.minutesElapsed"/>.</remarks>
        public const float FenceDecayRate = 1440f;
    }
}
