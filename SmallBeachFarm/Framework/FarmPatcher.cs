using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

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
        /// <param name="isSmallBeachFarm">Get whether the given location is the Small Beach Farm.</param>
        /// <param name="isOceanTile">Get whether a given position is ocean water.</param>
        public static void Hook(HarmonyInstance harmony, IMonitor monitor, Func<GameLocation, bool> isSmallBeachFarm, Func<Farm, int, int, bool> isOceanTile)
        {
            FarmPatcher.Monitor = monitor;
            FarmPatcher.IsSmallBeachFarm = isSmallBeachFarm;
            FarmPatcher.IsOceanTile = isOceanTile;

            harmony.Patch(
                original: AccessTools.Method(typeof(Farm), nameof(Farm.getFish), new[] { typeof(float), typeof(int), typeof(int), typeof(Farmer), typeof(double) }),
                prefix: new HarmonyMethod(typeof(FarmPatcher), nameof(FarmPatcher.GetFishPrefix))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>A method called via Harmony before <see cref="Farm.getFish(float, int, int, Farmer, double)"/>, which gets ocean fish from the beach properties if fishing the ocean water.</summary>
        /// <param name="__instance">The farm instance.</param>
        /// <param name="millisecondsAfterNibble">An argument passed through to the underlying method.</param>
        /// <param name="bait">An argument passed through to the underlying method.</param>
        /// <param name="waterDepth">An argument passed through to the underlying method.</param>
        /// <param name="who">An argument passed through to the underlying method.</param>
        /// <param name="baitPotency">An argument passed through to the underlying method.</param>
        /// <param name="__result">The return value to use for the method.</param>
        /// <returns>Returns <c>true</c> if the original logic should run, or <c>false</c> to use <paramref name="__result"/> as the return value.</returns>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static bool GetFishPrefix(Farm __instance, float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, ref SObject __result)
        {
            if (!FarmPatcher.IsSmallBeachFarm(who?.currentLocation))
                return false;

            // get tile being fished
            FishingRod rod = who.CurrentTool as FishingRod;
            if (rod == null)
                return false;
            Point tile = new Point((int)(rod.bobber.X / Game1.tileSize), (int)(rod.bobber.Y / Game1.tileSize));

            // get ocean fish
            if (FarmPatcher.IsOceanTile(__instance, tile.X, tile.Y))
            {
                __result = __instance.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, "Beach");
                FarmPatcher.Monitor.VerboseLog($"Fishing ocean tile at ({rod.bobber.X / Game1.tileSize}, {rod.bobber.Y / Game1.tileSize}).");
                return false;
            }

            // get default riverlands fish
            FarmPatcher.Monitor.VerboseLog($"Fishing river tile at ({rod.bobber.X / Game1.tileSize}, {rod.bobber.Y / Game1.tileSize}).");
            return true;
        }
    }
}
