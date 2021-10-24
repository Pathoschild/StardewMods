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
        private readonly IDictionary<int, string> FarmChoices;


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
                .RegisterConfig(canConfigureInGame: false) // configuring in-game would have unintended effects like small beach farm logic being half-applied

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
                    set: (config, value) => config.ReplaceFarmID = this.GetFarmID(value, defaultValue: new ModConfig().ReplaceFarmID),
                    choices: this.FarmChoices.Values.OrderBy(p => p).ToArray()
                );
        }

        /// <summary>Get the IDs and display names for each farm type that can be replaced.</summary>
        private IDictionary<int, string> GetFarmNamesById()
        {
            Dictionary<int, string> farmNamesById = new();

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

                farmNamesById[id] = Game1.content.LoadString(@$"Strings\UI:{translationKey}").Split('_')[0];
            }
            return farmNamesById;
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
