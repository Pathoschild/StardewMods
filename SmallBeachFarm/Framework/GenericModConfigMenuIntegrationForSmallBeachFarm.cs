using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.SmallBeachFarm.Framework.Config;
using StardewModdingAPI;
using StardewValley;

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

        /// <summary>The IDs and display names for each farm type that can be replaced.</summary>
        private readonly IDictionary<int, Func<string>> FarmChoices;


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
            this.FarmChoices = this.GetFarmNamesById();
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            // get config menu
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

            // register
            menu
                .Register(titleScreenOnly: true) // configuring in-game would have unintended effects like small beach farm logic being half-applied

                .AddCheckbox(
                    name: I18n.Config_Campfire_Name,
                    tooltip: I18n.Config_Campfire_Tooltip,
                    get: config => config.AddCampfire,
                    set: (config, value) => config.AddCampfire = value
                )
                .AddCheckbox(
                    name: I18n.Config_Islands_Name,
                    tooltip: I18n.Config_Islands_Tooltip,
                    get: config => config.EnableIslands,
                    set: (config, value) => config.EnableIslands = value
                )
                .AddCheckbox(
                    name: I18n.Config_BeachSounds_Name,
                    tooltip: I18n.Config_BeachSounds_Tooltip,
                    get: config => config.UseBeachMusic,
                    set: (config, value) => config.UseBeachMusic = value
                )
                .AddDropdown(
                    name: I18n.Config_FarmType_Name,
                    tooltip: I18n.Config_FarmType_Tooltip,
                    get: config => config.ReplaceFarmID.ToString(),
                    set: (config, value) => config.ReplaceFarmID = int.Parse(value),
                    allowedValues: this.FarmChoices.OrderBy(p => p.Value()).Select(p => p.Key.ToString()).ToArray(),
                    formatAllowedValue: value => this.GetFarmName(int.Parse(value))
                );
        }

        /// <summary>Get the IDs and display names for each farm type that can be replaced.</summary>
        private IDictionary<int, Func<string>> GetFarmNamesById()
        {
            Dictionary<int, Func<string>> farmNamesById = new();

            foreach (int id in new[] { Farm.default_layout, Farm.riverlands_layout, Farm.forest_layout, Farm.mountains_layout, Farm.combat_layout, Farm.fourCorners_layout, Farm.beach_layout })
            {
                string translationKey = id switch
                {
                    Farm.default_layout => "Character_FarmStandard",
                    Farm.riverlands_layout => "Character_FarmFishing",
                    Farm.forest_layout => "Character_FarmForaging",
                    Farm.mountains_layout => "Character_FarmMining",
                    Farm.combat_layout => "Character_FarmCombat",
                    Farm.fourCorners_layout => "Character_FarmFourCorners",
                    Farm.beach_layout => "Character_FarmBeach",
                    _ => throw new InvalidOperationException($"Unexpected farm ID {id}.")
                };

                farmNamesById[id] = () => Game1.content.LoadString(@$"Strings\UI:{translationKey}").Split('_')[0];
            }
            return farmNamesById;
        }

        /// <summary>Get the name of a farm type.</summary>
        /// <param name="id">The farm ID.</param>
        private string GetFarmName(int id)
        {
            if (this.FarmChoices.TryGetValue(id, out Func<string> getName))
                return getName();

            return id.ToString();
        }
    }
}
