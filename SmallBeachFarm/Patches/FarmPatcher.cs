using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Patching;
using Pathoschild.Stardew.SmallBeachFarm.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.SmallBeachFarm.Patches
{
    /// <summary>Encapsulates Harmony patches for the <see cref="Farm"/> instance.</summary>
    internal class FarmPatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private static ModConfig Config = null!; // set in constructor

        /// <summary>Get whether the given location is the Small Beach Farm.</summary>
        private static Func<GameLocation?, bool> IsSmallBeachFarm = null!; // set in constructor


        /*********
        ** Public methods
        *********/
        /// <summary>Initialize the patcher.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="isSmallBeachFarm">Get whether the given location is the Small Beach Farm.</param>
        public FarmPatcher(ModConfig config, Func<GameLocation?, bool> isSmallBeachFarm)
        {
            FarmPatcher.Config = config;
            FarmPatcher.IsSmallBeachFarm = isSmallBeachFarm;
        }

        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
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
        /// <summary>A method called via Harmony after <see cref="Farm.resetLocalState"/>.</summary>
        /// <param name="__instance">The farm instance.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static void After_ResetLocalState(GameLocation __instance)
        {
            if (!FarmPatcher.IsSmallBeachFarm(__instance))
                return;

            // change background track
            if (FarmPatcher.ShouldUseBeachMusic())
                Game1.changeMusicTrack("ocean", music_context: MusicContext.SubLocation);
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
                Game1.changeMusicTrack("none", music_context: MusicContext.SubLocation);
        }

        /// <summary>A method called via Harmony after <see cref="GameLocation.getRandomTile"/>.</summary>
        /// <param name="__instance">The farm instance.</param>
        /// <param name="__result">The return value to use for the method.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static void After_GetRandomTile(GameLocation __instance, ref Vector2 __result)
        {
            if (!FarmPatcher.IsSmallBeachFarm(__instance) || !FarmPatcher.IsOceanTile(__instance, __result))
                return;

            // get lowest Y position where ground is likely to appear
            int maxTileY;
            if (FarmPatcher.Config.EnableIslands)
                maxTileY = __instance.Map.Layers[0].LayerHeight; // allow selecting an island tile
            else
            {
                maxTileY = 0;
                foreach ((string id, FishAreaData area) in __instance.GetData().FishAreas)
                {
                    if (id == "Ocean" || area.Position == null)
                        continue;

                    int bottom = area.Position.Value.Bottom;
                    if (bottom > maxTileY)
                        maxTileY = bottom;
                }
            }

            // reduce chance of ocean tiles in random tile selection, which makes things like beach crates much less likely than vanilla
            for (int i = 0; i < 250 && FarmPatcher.IsOceanTile(__instance, __result); i++)
                __result = new Vector2(Game1.random.Next(__instance.Map.Layers[0].LayerWidth), Game1.random.Next(maxTileY));
        }

        /// <summary>Get whether the Small Beach Farm's music should be overridden with the beach sounds.</summary>
        private static bool ShouldUseBeachMusic()
        {
            return FarmPatcher.Config.UseBeachMusic && !Game1.isRaining;
        }

        /// <summary>Get whether a tile is in the ocean.</summary>
        /// <param name="location">The farm location.</param>
        /// <param name="tile">The tile coordinates.</param>
        private static bool IsOceanTile(GameLocation location, Vector2 tile)
        {
            return
                location.isWaterTile((int)tile.X, (int)tile.Y)
                && location.TryGetFishAreaForTile(tile, out string areaId, out _)
                && areaId == "Ocean";
        }
    }
}
