using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;

namespace Pathoschild.Stardew.HorseFluteAnywhere.Framework
{
    /// <summary>Encapsulates Harmony patches for the <see cref="Utility"/> class.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
    internal static class UtilityPatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates logging for the Harmony patch.</summary>
        private static IMonitor Monitor;


        /*********
        ** Public methods
        *********/
        /// <summary>Initialize the Harmony patches.</summary>
        /// <param name="harmony">The Harmony patching API.</param>
        /// <param name="monitor">Encapsulates logging for the Harmony patch.</param>
        public static void Hook(HarmonyInstance harmony, IMonitor monitor)
        {
            UtilityPatcher.Monitor = monitor;

            // disable indoor warp restriction
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.GetHorseWarpRestrictionsForFarmer)),
                transpiler: new HarmonyMethod(typeof(UtilityPatcher), nameof(UtilityPatcher.Transpile_GetHorseWarpRestrictionsForFarmer))
            );

            // let game find horses indoors
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.findHorse)),
                postfix: new HarmonyMethod(typeof(UtilityPatcher), nameof(UtilityPatcher.After_FindHorse))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.findHorseForPlayer)),
                postfix: new HarmonyMethod(typeof(UtilityPatcher), nameof(UtilityPatcher.After_FindHorseForPlayer))
            );
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Patches
        ****/
        /// <summary>A method called via Harmony to modify <see cref="Utility.GetHorseWarpRestrictionsForFarmer"/>.</summary>
        /// <param name="oldInstructions">The method instructions to transpile.</param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "deliberately get original instructions if method fails")]
        public static IEnumerable<CodeInstruction> Transpile_GetHorseWarpRestrictionsForFarmer(IEnumerable<CodeInstruction> oldInstructions)
        {
            try
            {
                CodeInstruction[] instructions = oldInstructions.ToArray();

                // skip location restriction check
                for (int i = 0; i < instructions.Length; i++)
                {
                    // relevant CIL code:
                    //    if (location.IsOutdoors):
                    //       IL_0016: ldloc.0
                    //       IL_0017: callvirt instance bool StardewValley.GameLocation::get_IsOutdoors()
                    //       IL_001c: brtrue.s IL_0020
                    //    return 2:
                    //       IL_001e: ldc.i4.2
                    //       IL_001f: ret
                    bool isLocationCheck =
                        instructions[i].opcode == OpCodes.Callvirt
                        && instructions[i].operand is MethodInfo { Name: "get_IsOutdoors" }
                        && instructions.Length > i + 2
                        && instructions[i + 3].opcode == OpCodes.Ret;

                    if (isLocationCheck)
                    {
                        instructions[i + 2] = new CodeInstruction(OpCodes.Nop);
                        instructions[i + 3] = new CodeInstruction(OpCodes.Nop);
                        break;
                    }
                }

                return instructions;
            }
            catch (Exception ex)
            {
                UtilityPatcher.Monitor.Log($"Failed to patch {nameof(Utility)}.{nameof(Utility.GetHorseWarpRestrictionsForFarmer)}.\nTechnical details: {ex}", LogLevel.Error);
                return oldInstructions;
            }
        }

        /// <summary>A method called via Harmony after <see cref="Utility.findHorse"/>.</summary>
        /// <param name="horseId">The horse ID to find.</param>
        /// <param name="__result">The return value to use for the method.</param>
        private static void After_FindHorse(Guid horseId, ref Horse __result)
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
        private static void After_FindHorseForPlayer(long uid, ref Horse __result)
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
        private static Horse TryFindHorse(Func<Horse, bool> match)
        {
            return CommonHelper.GetLocations(includeTempLevels: true)
                .SelectMany(location => location.characters)
                .OfType<Horse>()
                .FirstOrDefault(match);
        }
    }
}
