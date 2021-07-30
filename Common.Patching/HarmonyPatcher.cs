using System;
using HarmonyLib;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Patching
{
    /// <summary>Simplifies applying <see cref="IPatcher"/> instances to the game.</summary>
    internal static class HarmonyPatcher
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Apply the given Harmony patchers.</summary>
        /// <param name="mod">The mod applying the patchers.</param>
        /// <param name="patchers">The patchers to apply.</param>
        public static Harmony Apply(Mod mod, params IPatcher[] patchers)
        {
            Harmony harmony = new Harmony(mod.ModManifest.UniqueID);

            foreach (IPatcher patcher in patchers)
            {
                try
                {
                    patcher.Apply(harmony, mod.Monitor);
                }
                catch (Exception ex)
                {
                    mod.Monitor.Log($"Failed to apply '{patcher.GetType().FullName}' patcher; some features may not work correctly. Technical details:\n{ex}", LogLevel.Error);
                }
            }

            return harmony;
        }
    }
}
