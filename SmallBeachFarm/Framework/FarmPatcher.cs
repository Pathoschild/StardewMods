using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace Pathoschild.Stardew.SmallBeachFarm.Framework
{
    /// <summary>Encapsulates Harmony patches for the <see cref="Farm"/> instance.</summary>
    internal static class FarmPatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates logging for the Harmony patch.</summary>
        private static IMonitor Monitor;

        /// <summary>Use the beach's background music (i.e. wave sounds) on the beach farm.</summary>
        private static bool UseBeachMusic;

        /// <summary>Whether the mod is currently applying patch changes (to avoid infinite recursion).</summary>
        private static bool IsInPatch = false;

        /// <summary>Get whether the given location is the Small Beach Farm.</summary>
        private static Func<GameLocation, bool> IsSmallBeachFarm;

        /// <summary>Get whether a given position is ocean water.</summary>
        private static Func<Farm, int, int, bool> IsOceanTile;


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the Harmony patches.</summary>
        /// <param name="harmony">The Harmony patching API.</param>
        /// <param name="monitor">Encapsulates logging for the Harmony patch.</param>
        /// <param name="useBeachMusic">Use the beach's background music (i.e. wave sounds) on the beach farm.</param>
        /// <param name="isSmallBeachFarm">Get whether the given location is the Small Beach Farm.</param>
        /// <param name="isOceanTile">Get whether a given position is ocean water.</param>
        public static void Hook(HarmonyInstance harmony, IMonitor monitor, bool useBeachMusic, Func<GameLocation, bool> isSmallBeachFarm, Func<Farm, int, int, bool> isOceanTile)
        {
            FarmPatcher.Monitor = monitor;
            FarmPatcher.UseBeachMusic = useBeachMusic;
            FarmPatcher.IsSmallBeachFarm = isSmallBeachFarm;
            FarmPatcher.IsOceanTile = isOceanTile;

            harmony.Patch(
                original: AccessTools.Method(typeof(Farm), nameof(Farm.getFish)),
                prefix: new HarmonyMethod(typeof(FarmPatcher), nameof(FarmPatcher.Before_GetFish))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Farm), "resetLocalState"),
                prefix: new HarmonyMethod(typeof(FarmPatcher), nameof(FarmPatcher.After_ResetLocalState))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Farm), nameof(Farm.cleanupBeforePlayerExit)),
                prefix: new HarmonyMethod(typeof(FarmPatcher), nameof(FarmPatcher.After_CleanupBeforePlayerExit))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>A method called via Harmony before <see cref="Farm.getFish"/>, which gets ocean fish from the beach properties if fishing the ocean water.</summary>
        /// <param name="__instance">The farm instance.</param>
        /// <param name="millisecondsAfterNibble">An argument passed through to the underlying method.</param>
        /// <param name="bait">An argument passed through to the underlying method.</param>
        /// <param name="waterDepth">An argument passed through to the underlying method.</param>
        /// <param name="who">An argument passed through to the underlying method.</param>
        /// <param name="baitPotency">An argument passed through to the underlying method.</param>
        /// <param name="bobberTile">The tile containing the bobber.</param>
        /// <param name="__result">The return value to use for the method.</param>
        /// <returns>Returns <c>true</c> if the original logic should run, or <c>false</c> to use <paramref name="__result"/> as the return value.</returns>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static bool Before_GetFish(Farm __instance, float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, ref Object __result)
        {
            if (FarmPatcher.IsInPatch || !FarmPatcher.IsSmallBeachFarm(who?.currentLocation))
                return false;

            try
            {
                FarmPatcher.IsInPatch = true;

                // get ocean fish
                if (FarmPatcher.IsOceanTile(__instance, (int)bobberTile.X, (int)bobberTile.Y))
                {
                    __result = __instance.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Beach");
                    FarmPatcher.Monitor.VerboseLog($"Fishing ocean tile at ({bobberTile.X / Game1.tileSize}, {bobberTile.Y / Game1.tileSize}).");
                    return false;
                }

                // get default riverlands fish
                FarmPatcher.Monitor.VerboseLog($"Fishing river tile at ({bobberTile.X / Game1.tileSize}, {bobberTile.Y / Game1.tileSize}).");
                return true;
            }
            finally
            {
                FarmPatcher.IsInPatch = false;
            }
        }

        /// <summary>A method called via Harmony after <see cref="Farm.resetLocalState"/>, which changes the background soundtrack.</summary>
        /// <param name="__instance">The farm instance.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static void After_ResetLocalState(GameLocation __instance)
        {
            if (FarmPatcher.ShouldUseBeachMusic(__instance))
                Game1.changeMusicTrack("ocean", music_context: Game1.MusicContext.SubLocation);
        }

        /// <summary>A method called via Harmony after <see cref="Farm.cleanupBeforePlayerExit"/>, which resets the background soundtrack.</summary>
        /// <param name="__instance">The farm instance.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static void After_CleanupBeforePlayerExit(GameLocation __instance)
        {
            if (FarmPatcher.ShouldUseBeachMusic(__instance))
                Game1.changeMusicTrack("none", music_context: Game1.MusicContext.SubLocation);
        }

        /// <summary>Get whether the location's music should be overridden with the beach sounds.</summary>
        /// <param name="location">The location to check.</param>
        private static bool ShouldUseBeachMusic(GameLocation location)
        {
            return
                FarmPatcher.UseBeachMusic
                && !Game1.isRaining
                && FarmPatcher.IsSmallBeachFarm(location);
        }
    }
}
