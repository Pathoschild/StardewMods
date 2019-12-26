using System;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models.FishData
{
    /// <summary>Available weathers for fish spawning.</summary>
    [Flags]
    internal enum FishSpawnWeather
    {
        /// <summary>No specified value.</summary>
        Unknown,

        /// <summary>The fish only spawns when it's sunny.</summary>
        Sunny = 1,

        /// <summary>The fish only spawns when it's raining or snowing.</summary>
        Rainy = 2,

        /// <summary>The fish spawns in any weather.</summary>
        Both = FishSpawnWeather.Sunny | FishSpawnWeather.Rainy
    }
}
