using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Pathoschild.Stardew.Common.Patching;
using Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Patches
{
    /// <summary>Encapsulates Harmony patches for <see cref="FruitTreePatcher"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
    internal class FruitTreePatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private static LocationConfigManager Config = null!; // set by first constructor


        /*********
        ** Public methods
        *********/
        /// <summary>Initialize the Harmony patches.</summary>
        /// <param name="config">The mod configuration.</param>
        public FruitTreePatcher(LocationConfigManager config)
        {
            FruitTreePatcher.Config = config;
        }

        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: typeof(FruitTree).GetMethod(nameof(FruitTree.GetCosmeticSeason)) ?? throw new InvalidOperationException($"Can't find method {nameof(FruitTree.GetCosmeticSeason)}"),
                postfix: this.GetHarmonyMethod(nameof(FruitTreePatcher.After_GetCosmeticSeason))
            );
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Patches
        ****/
        /// <summary>A method called via Harmony before <see cref="FruitTree.GetCosmeticSeason"/>.</summary>
        /// <param name="__instance">The fruit tree instance.</param>
        /// <param name="__result">The return value to use for the method.</param>
        [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Matches original code code")]
        private static void After_GetCosmeticSeason(FruitTree __instance, ref Season __result)
        {
            if (FruitTreePatcher.HasUnseasonalGreenhouseSprite(__instance.Location, __result) && FruitTreePatcher.Config.TryGetForLocation(__instance.Location, out PerLocationConfig? config) && config.UseFruitTreesSeasonalSprites)
                __result = Game1.season;
        }

        /****
        ** Methods
        ****/
        /// <summary>Get whether a fruit tree is displaying an out-of-season summer sprite due to the parent location's <see cref="GameLocation.SeedsIgnoreSeasonsHere"/> returning true.</summary>
        /// <param name="location">The location containing the fruit tree.</param>
        /// <param name="cosmeticSeason">The seasonal sprite selected by the fruit tree.</param>
        private static bool HasUnseasonalGreenhouseSprite(GameLocation? location, Season cosmeticSeason)
        {
            return
                location?.SeedsIgnoreSeasonsHere() is true
                && cosmeticSeason == Season.Summer
                && Game1.season != Season.Summer;
        }
    }
}
