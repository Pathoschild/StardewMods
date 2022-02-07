using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Patching;
using Pathoschild.Stardew.SmallBeachFarm.Framework;
using Pathoschild.Stardew.SmallBeachFarm.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.SmallBeachFarm.Patches
{
    /// <summary>Encapsulates Harmony patches for the <see cref="Farm"/> instance.</summary>
    internal class FarmPatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates logging for the Harmony patch.</summary>
        private static IMonitor Monitor = null!; // set in constructor

        /// <summary>The mod configuration.</summary>
        private static ModConfig Config = null!; // set in constructor

        /// <summary>Get whether the given location is the Small Beach Farm.</summary>
        private static Func<GameLocation?, bool> IsSmallBeachFarm = null!; // set in constructor

        /// <summary>Get the fish that should be available from the given tile.</summary>
        private static Func<Farm, int, int, FishType> GetFishType = null!; // set in constructor

        /// <summary>Whether the mod is currently applying patch changes (to avoid infinite recursion),</summary>
        private static bool IsInPatch;


        /*********
        ** Public methods
        *********/
        /// <summary>Initialize the patcher.</summary>
        /// <param name="monitor">Encapsulates logging for the Harmony patch.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="isSmallBeachFarm">Get whether the given location is the Small Beach Farm.</param>
        /// <param name="getFishType">Get the fish that should be available from the given tile.</param>
        public FarmPatcher(IMonitor monitor, ModConfig config, Func<GameLocation?, bool> isSmallBeachFarm, Func<Farm, int, int, FishType> getFishType)
        {
            FarmPatcher.Monitor = monitor;
            FarmPatcher.Config = config;
            FarmPatcher.IsSmallBeachFarm = isSmallBeachFarm;
            FarmPatcher.GetFishType = getFishType;
        }

        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Farm>(nameof(Farm.getFish)),
                prefix: this.GetHarmonyMethod(nameof(FarmPatcher.Before_GetFish))
            );
            harmony.Patch(
                original: this.RequireMethod<Farm>("resetLocalState"),
                postfix: this.GetHarmonyMethod(nameof(FarmPatcher.After_ResetLocalState))
            );
            harmony.Patch(
                original: this.RequireMethod<Farm>("resetSharedState"),
                postfix: this.GetHarmonyMethod(nameof(FarmPatcher.After_ResetSharedState))
            );
            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.cleanupBeforePlayerExit)),
                postfix: this.GetHarmonyMethod(nameof(FarmPatcher.After_CleanupBeforePlayerExit))
            );
            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.getRandomTile)),
                postfix: this.GetHarmonyMethod(nameof(FarmPatcher.After_GetRandomTile))
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
        private static bool Before_GetFish(Farm __instance, float millisecondsAfterNibble, string bait, int waterDepth, Farmer? who, double baitPotency, Vector2 bobberTile, ref SObject __result)
        {
            if (FarmPatcher.IsInPatch || !FarmPatcher.IsSmallBeachFarm(who?.currentLocation))
                return true;

            try
            {
                FarmPatcher.IsInPatch = true;

                FishType type = FarmPatcher.GetFishType(__instance, (int)bobberTile.X, (int)bobberTile.Y);
                FarmPatcher.Monitor.VerboseLog($"Fishing {type.ToString().ToLower()} tile at ({bobberTile.X / Game1.tileSize}, {bobberTile.Y / Game1.tileSize}).");
                switch (type)
                {
                    case FishType.Ocean:
                        __result = __instance.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Beach");
                        return false;

                    case FishType.River:
                        // match riverland farm behavior
                        __result = Game1.random.NextDouble() < 0.3
                            ? __instance.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Forest")
                            : __instance.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "Town");
                        return false;

                    default:
                        return true; // run original method
                }
            }
            finally
            {
                FarmPatcher.IsInPatch = false;
            }
        }

        /// <summary>A method called via Harmony after <see cref="Farm.resetLocalState"/>.</summary>
        /// <param name="__instance">The farm instance.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static void After_ResetLocalState(GameLocation __instance)
        {
            if (!FarmPatcher.IsSmallBeachFarm(__instance))
                return;

            // change background track
            if (FarmPatcher.ShouldUseBeachMusic())
                Game1.changeMusicTrack("ocean", music_context: Game1.MusicContext.SubLocation);
        }

        /// <summary>A method called via Harmony after <see cref="Farm.resetSharedState"/>.</summary>
        /// <param name="__instance">The farm instance.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static void After_ResetSharedState(GameLocation __instance)
        {
            if (!FarmPatcher.IsSmallBeachFarm(__instance))
                return;

            // toggle campfire (derived from StardewValley.Locations.Mountain:resetSharedState
            Vector2 campfireTile = new Vector2(64, 22);
            if (FarmPatcher.Config.AddCampfire)
            {
                if (!__instance.objects.ContainsKey(campfireTile))
                {
                    __instance.objects.Add(campfireTile, new Torch("146", true)
                    {
                        IsOn = false,
                        Fragility = SObject.fragility_Indestructable
                    });
                }
            }
            else if (__instance.objects.TryGetValue(campfireTile, out SObject obj) && obj is Torch { QualifiedItemId: "(O)146" })
                __instance.objects.Remove(campfireTile);
        }

        /// <summary>A method called via Harmony after <see cref="Farm.cleanupBeforePlayerExit"/>.</summary>
        /// <param name="__instance">The farm instance.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static void After_CleanupBeforePlayerExit(GameLocation __instance)
        {
            if (!FarmPatcher.IsSmallBeachFarm(__instance))
                return;

            // change background track
            if (FarmPatcher.ShouldUseBeachMusic())
                Game1.changeMusicTrack("none", music_context: Game1.MusicContext.SubLocation);
        }

        /// <summary>A method called via Harmony after <see cref="GameLocation.getRandomTile"/>.</summary>
        /// <param name="__instance">The farm instance.</param>
        /// <param name="__result">The return value to use for the method.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static void After_GetRandomTile(GameLocation __instance, ref Vector2 __result)
        {
            if (!FarmPatcher.IsSmallBeachFarm(__instance))
                return;

            // reduce chance of ocean tiles in random tile selection, which makes things like beach crates much less likely than vanilla
            Farm farm = (Farm)__instance;
            int maxTileY = FarmPatcher.Config.EnableIslands ? farm.Map.Layers[0].LayerHeight : 31;
            for (int i = 0; FarmPatcher.GetFishType(farm, (int)__result.X, (int)__result.Y) == FishType.Ocean && i < 250; i++)
                __result = new Vector2(Game1.random.Next(farm.Map.Layers[0].LayerWidth), Game1.random.Next(maxTileY));
        }

        /// <summary>Get whether the Small Beach Farm's music should be overridden with the beach sounds.</summary>
        private static bool ShouldUseBeachMusic()
        {
            return FarmPatcher.Config.UseBeachMusic && !Game1.isRaining;
        }
    }
}
