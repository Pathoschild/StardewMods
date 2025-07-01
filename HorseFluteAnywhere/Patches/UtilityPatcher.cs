using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Pathoschild.Stardew.Common.Patching;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.HorseFluteAnywhere.Patches;

/// <summary>Encapsulates Harmony patches for the <see cref="Utility"/> class.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
internal class UtilityPatcher : BasePatcher
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Apply(Harmony harmony, IMonitor monitor)
    {
        // disable indoor warp restriction
        harmony.Patch(
            original: this.RequireMethod<Utility>(nameof(Utility.GetHorseWarpRestrictionsForFarmer)),
            postfix: this.GetHarmonyMethod(nameof(UtilityPatcher.After_GetHorseWarpRestrictionsForFarmer))
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
}
