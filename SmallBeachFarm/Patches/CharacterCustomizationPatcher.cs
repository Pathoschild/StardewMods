using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Pathoschild.Stardew.Common.Patching;
using Pathoschild.Stardew.SmallBeachFarm.Framework.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.SmallBeachFarm.Patches
{
    /// <summary>Encapsulates Harmony patches for the <see cref="CharacterCustomization"/> menu.</summary>
    internal class CharacterCustomizationPatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private static ModConfig Config = null!; // set in constructor

        /// <summary>The farm type ID for Small Beach Farm.</summary>
        private static string FarmTypeId = null!; // set in constructor


        /*********
        ** Public methods
        *********/
        /// <summary>Initialize the patcher.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="farmTypeId">The farm type ID for Small Beach Farm.</param>
        public CharacterCustomizationPatcher(ModConfig config, string farmTypeId)
        {
            CharacterCustomizationPatcher.Config = config;
            CharacterCustomizationPatcher.FarmTypeId = farmTypeId;
        }

        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<CharacterCustomization>("optionButtonClick"),
                postfix: this.GetHarmonyMethod(nameof(CharacterCustomizationPatcher.After_OptionButtonClick))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>A method called via Harmony after <see cref="CharacterCustomization.optionButtonClick"/>.</summary>
        /// <param name="__instance">The menu instance.</param>
        /// <param name="name">The option name that was clicked.</param>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static void After_OptionButtonClick(CharacterCustomization __instance, string name)
        {
            if (name != $"ModFarm_{CharacterCustomizationPatcher.FarmTypeId}")
                return;

            // apply default options
            Game1.spawnMonstersAtNight = CharacterCustomizationPatcher.Config.DefaultSpawnMonstersAtNight;
        }
    }
}
