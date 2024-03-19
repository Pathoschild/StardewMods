using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Patching;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;

namespace Pathoschild.Stardew.HorseFluteAnywhere.Patches
{
    /// <summary>Encapsulates Harmony patches for the <see cref="Utility"/> class.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
    internal class UtilityPatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates logging for the Harmony patch.</summary>
        private static IMonitor Monitor = null!; // set when constructor is called


        /*********
        ** Public methods
        *********/
        /// <summary>Initialize the Harmony patches.</summary>
        /// <param name="monitor">Encapsulates logging for the Harmony patch.</param>
        public UtilityPatcher(IMonitor monitor)
        {
            UtilityPatcher.Monitor = monitor;
        }

        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            // disable indoor warp restriction
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.GetHorseWarpRestrictionsForFarmer)),
                postfix: this.GetHarmonyMethod(nameof(UtilityPatcher.After_GetHorseWarpRestrictionsForFarmer))
            );

            // let game find horses indoors
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.findHorse)),
                postfix: this.GetHarmonyMethod(nameof(UtilityPatcher.After_FindHorse))
            );
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.findHorseForPlayer)),
                postfix: this.GetHarmonyMethod(nameof(UtilityPatcher.After_FindHorseForPlayer))
            );
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Patches
        ****/
        /// <summary>A method called via Harmony after <see cref="Utility.GetHorseWarpRestrictionsForFarmer"/>.</summary>
        /// <param name="__result">The return value to use for the method.</param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "deliberately get original instructions if method fails")]
        public static void After_GetHorseWarpRestrictionsForFarmer(ref Utility.HorseWarpRestrictions __result)
        {
            __result &= ~Utility.HorseWarpRestrictions.Indoors;
        }

        /// <summary>A method called via Harmony after <see cref="Utility.findHorse"/>.</summary>
        /// <param name="horseId">The horse ID to find.</param>
        /// <param name="__result">The return value to use for the method.</param>
        private static void After_FindHorse(Guid horseId, ref Horse? __result)
        {
            try
            {
                // if game didn't find a horse, check indoor locations
                __result ??= UtilityPatcher.TryFindHorse(horse => horse.HorseId == horseId);
            }
            catch (Exception ex)
            {
                UtilityPatcher.Monitor.Log($"Failed to patch {nameof(Utility)}.{nameof(Utility.findHorse)}.\nTechnical details: {ex}", LogLevel.Error);
            }
        }

        /// <summary>A method called via Harmony after <see cref="Utility.findHorseForPlayer"/>.</summary>
        /// <param name="uid">The unique player ID.</param>
        /// <param name="__result">The return value to use for the method.</param>
        private static void After_FindHorseForPlayer(long uid, ref Horse? __result)
        {
            try
            {
                // if game didn't find a horse, check indoor locations
                __result ??= UtilityPatcher.TryFindHorse(horse => horse.ownerId.Value == uid);
            }
            catch (Exception ex)
            {
                UtilityPatcher.Monitor.Log($"Failed to patch {nameof(Utility)}.{nameof(Utility.findHorseForPlayer)}.\nTechnical details: {ex}", LogLevel.Error);
            }
        }

        /****
        ** Methods
        ****/
        /// <summary>Get the first horse matching a condition.</summary>
        /// <param name="match">The condition to match.</param>
        private static Horse? TryFindHorse(Func<Horse, bool> match)
        {
            return CommonHelper.GetLocations(includeTempLevels: true)
                .SelectMany(location => location.characters)
                .OfType<Horse>()
                .FirstOrDefault(match);
        }
    }
}
