using System;
using System.Collections.Generic;
using System.Linq;
using Common.Integrations.GenericModConfigMenu;
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

        /// <summary>Get the possible farm choices.</summary>
        private readonly IDictionary<int, string> FarmChoices = new Dictionary<int, string>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="modData">The mod's hardcoded data.</param>
        /// <param name="getConfig">Get the current config model.</param>
        /// <param name="reset">Reset the config model to the default values.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegrationForSmallBeachFarm(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, ModData modData, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
        {
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
            foreach (ModFarmMapData farm in modData.FarmMaps)
                this.FarmChoices[farm.ID] = farm.Name ?? farm.Map;
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
                .RegisterConfig()

                .AddCheckbox(
                    label: "Add Campfire",
                    description: "Whether to add a functional campfire in front of the farmhouse.",
                    get: config => config.AddCampfire,
                    set: (config, value) => config.AddCampfire = value
                )
                .AddCheckbox(
                    label: "Add Islands",
                    description: "Whether to add ocean islands with extra land area.",
                    get: config => config.EnableIslands,
                    set: (config, value) => config.EnableIslands = value
                )
                .AddCheckbox(
                    label: "Play Beach Sounds",
                    description: "Use the beach's background music (i.e. wave sounds) on the beach farm.",
                    get: config => config.UseBeachMusic,
                    set: (config, value) => config.UseBeachMusic = value
                )
                .AddDropdown(
                    label: "Replace Farm Type",
                    description: "The farm layout to replace.",
                    get: config => this.GetFarmName(config.ReplaceFarmID),
                    set: (config, value) => config.ReplaceFarmID = this.GetFarmID(value, defaultValue: this.GetFarmID("Riverland", defaultValue: config.ReplaceFarmID)),
                    choices: this.FarmChoices.Values.OrderBy(p => p).ToArray()
                );
        }

        /// <summary>Get the name of a farm type.</summary>
        /// <param name="id">The farm ID.</param>
        private string GetFarmName(int id)
        {
            if (this.FarmChoices.TryGetValue(id, out string name))
                return name;

            return null;
        }

        /// <summary>Get the ID for a farm type.</summary>
        /// <param name="name">The farm name.</param>
        /// <param name="defaultValue">The default ID to return if no match is found.</param>
        private int GetFarmID(string name, int defaultValue = -1)
        {
            foreach (var pair in this.FarmChoices)
            {
                if (pair.Value == name)
                    return pair.Key;
            }

            return defaultValue;
        }
    }
}
