using System;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.SmallBeachFarm.Framework.Config;
using StardewModdingAPI;

namespace Pathoschild.Stardew.SmallBeachFarm.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForSmallBeachFarm
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Generic Mod Config Menu integration.</summary>
        private readonly GenericModConfigMenuIntegration<ModConfig> ConfigMenu;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="getConfig">Get the current config model.</param>
        /// <param name="reset">Reset the config model to the default values.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegrationForSmallBeachFarm(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
        {
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            // get config menu
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

            // register
            const int maxTileX = 79;
            const int maxTileY = 50;
            menu
                .Register(titleScreenOnly: true) // configuring in-game would have unintended effects like small beach farm logic being half-applied

                // farm options
                .AddSectionTitle(I18n.Config_FarmOptionsSection)
                .AddCheckbox(
                    name: I18n.Config_BeachSounds_Name,
                    tooltip: I18n.Config_BeachSounds_Tooltip,
                    get: config => config.UseBeachMusic,
                    set: (config, value) => config.UseBeachMusic = value
                )
                .AddCheckbox(
                    name: I18n.Config_SpawnMonsters_Name,
                    tooltip: I18n.Config_SpawnMonsters_Tooltip,
                    get: config => config.DefaultSpawnMonstersAtNight,
                    set: (config, value) => config.DefaultSpawnMonstersAtNight = value
                )

                // farm layout
                .AddSectionTitle(I18n.Config_FarmLayoutSection)
                .AddCheckbox(
                    name: I18n.Config_Campfire_Name,
                    tooltip: I18n.Config_Campfire_Tooltip,
                    get: config => config.AddCampfire,
                    set: (config, value) => config.AddCampfire = value
                )
                .AddCheckbox(
                    name: I18n.Config_Pier_Name,
                    tooltip: I18n.Config_Pier_Tooltip,
                    get: config => config.AddFishingPier,
                    set: (config, value) => config.AddFishingPier = value
                )
                .AddCheckbox(
                    name: I18n.Config_Islands_Name,
                    tooltip: I18n.Config_Islands_Tooltip,
                    get: config => config.EnableIslands,
                    set: (config, value) => config.EnableIslands = value
                )
                .AddCheckbox(
                    name: I18n.Config_ShippingBinPath_Name,
                    tooltip: I18n.Config_ShippingBinPath_Tooltip,
                    get: config => config.ShippingBinPath,
                    set: (config, value) => config.ShippingBinPath = value
                )

                // farm positions
                .AddSectionTitle(I18n.Config_FarmPositionsSection)
                .AddNumberField(
                    name: I18n.Config_PierX_Name,
                    tooltip: I18n.Config_PierX_Tooltip,
                    get: config => config.CustomFishingPierPosition.X,
                    set: (config, value) => config.CustomFishingPierPosition = new Point(value, config.CustomFishingPierPosition.Y),
                    min: 0,
                    max: maxTileX
                )
                .AddNumberField(
                    name: I18n.Config_PierY_Name,
                    tooltip: I18n.Config_PierY_Tooltip,
                    get: config => config.CustomFishingPierPosition.Y,
                    set: (config, value) => config.CustomFishingPierPosition = new Point(config.CustomFishingPierPosition.X, value),
                    min: 0,
                    max: maxTileY
                );
        }
    }
}
