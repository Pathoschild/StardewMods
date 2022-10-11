using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Automate.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForAutomate
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Generic Mod Config Menu integration.</summary>
        private readonly GenericModConfigMenuIntegration<ModConfig> ConfigMenu;

        /// <summary>The internal mod data.</summary>
        private readonly DataModel Data;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">An API for fetching metadata about loaded mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="data">The internal mod data.</param>
        /// <param name="getConfig">Get the current config model.</param>
        /// <param name="reset">Reset the config model to the default values.</param>
        /// <param name="saveAndApply">Save and apply the current config model.</param>
        public GenericModConfigMenuIntegrationForAutomate(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, DataModel data, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
        {
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
            this.Data = data;
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            // get config menu
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

            menu.Register();

            // main options
            menu
                .AddSectionTitle(I18n.Config_Title_MainOptions)
                .AddCheckbox(
                    name: I18n.Config_Enabled_Name,
                    tooltip: I18n.Config_Enabled_Desc,
                    get: config => config.Enabled,
                    set: (config, value) => config.Enabled = value
                )
                .AddNumberField(
                    name: I18n.Config_AutomationInterval_Name,
                    tooltip: I18n.Config_AutomationInterval_Desc,
                    get: config => config.AutomationInterval,
                    set: (config, value) => config.AutomationInterval = value,
                    min: 1,
                    max: 600
                )
                .AddKeyBinding(
                    name: I18n.Config_ToggleOverlayKey_Name,
                    tooltip: I18n.Config_ToggleOverlayKey_Desc,
                    get: config => config.Controls.ToggleOverlay,
                    set: (config, value) => config.Controls.ToggleOverlay = value
                )
                .AddCheckbox(
                    name: I18n.Config_WarnForMissingBridgeMod_Name,
                    tooltip: I18n.Config_WarnForMissingBridgeMod_Desc,
                    get: config => config.WarnForMissingBridgeMod,
                    set: (config, value) => config.WarnForMissingBridgeMod = value
                );

            // connectors
            menu.AddSectionTitle(I18n.Config_Title_Connectors);
            foreach (DataModelFloor entry in this.Data.FloorNames.Values)
            {
                int itemId = entry.ItemId;

                menu.AddCheckbox(
                    name: () => GameI18n.GetObjectName(itemId),
                    tooltip: () => I18n.Config_Connector_Desc(itemName: GameI18n.GetObjectName(itemId)),
                    get: config => this.HasConnector(config, entry.Name),
                    set: (config, value) => this.SetConnector(config, entry.Name, value)
                );
            }
            menu.AddTextbox(
                name: I18n.Config_CustomConnectors_Name,
                tooltip: I18n.Config_CustomConnectors_Desc,
                get: config => string.Join(", ", config.ConnectorNames.Where(this.IsCustomConnector)),
                set: (config, value) => this.SetCustomConnectors(config, value.Split(',').Select(p => p.Trim()))
            );

            // Junimo huts
            menu.AddSectionTitle(I18n.Config_Title_JunimoHuts);
            this.AddJunimoHutBehaviorDropdown(
                menu,
                name: I18n.Config_JunimoHutGems_Name,
                tooltip: I18n.Config_JunimoHutGems_Desc,
                get: config => config.JunimoHutBehaviorForGems,
                set: (config, value) => config.JunimoHutBehaviorForGems = value
            );
            this.AddJunimoHutBehaviorDropdown(
                menu,
                name: I18n.Config_JunimoHutFertilizer_Name,
                tooltip: I18n.Config_JunimoHutFertilizer_Desc,
                get: config => config.JunimoHutBehaviorForFertilizer,
                set: (config, value) => config.JunimoHutBehaviorForFertilizer = value
            );
            this.AddJunimoHutBehaviorDropdown(
                menu,
                name: I18n.Config_JunimoHutSeeds_Name,
                tooltip: I18n.Config_JunimoHutSeeds_Desc,
                get: config => config.JunimoHutBehaviorForSeeds,
                set: (config, value) => config.JunimoHutBehaviorForSeeds = value
            );

            // machine overrides
            menu.AddSectionTitle(I18n.Config_Title_MachineOverrides);
            foreach (var entry in this.Data.DefaultMachineOverrides)
            {
                menu.AddCheckbox(
                    name: () => this.GetTranslatedMachineName(entry.Key),
                    tooltip: () => I18n.Config_Override_Desc(machineName: this.GetTranslatedMachineName(entry.Key)),
                    get: config => this.IsMachineEnabled(config, entry.Key),
                    set: (config, value) => this.SetCustomOverride(config, entry.Key, value)
                );
            }
            menu.AddParagraph(I18n.Config_CustomOverridesNote);
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Junimo huts
        ****/
        /// <summary>Add a dropdown to configure Junimo hut behavior for an item type.</summary>
        /// <param name="menu">The config menu to extend.</param>
        /// <param name="name">The label text to show in the form.</param>
        /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
        /// <param name="get">Get the current value from the mod config.</param>
        /// <param name="set">Set a new value in the mod config.</param>
        private void AddJunimoHutBehaviorDropdown(GenericModConfigMenuIntegration<ModConfig> menu, Func<string> name, Func<string> tooltip, Func<ModConfig, JunimoHutBehavior> get, Action<ModConfig, JunimoHutBehavior> set)
        {
            menu.AddDropdown(
                name: name,
                tooltip: tooltip,
                get: config => get(config).ToString(),
                set: (config, value) => set(config, Enum.Parse<JunimoHutBehavior>(value)),
                allowedValues: Enum.GetNames<JunimoHutBehavior>(),
                formatAllowedValue: value => value switch
                {
                    nameof(JunimoHutBehavior.AutoDetect) => I18n.Config_JunimoHuts_AutoDetect(),
                    nameof(JunimoHutBehavior.Ignore) => I18n.Config_JunimoHuts_Ignore(),
                    nameof(JunimoHutBehavior.MoveIntoChests) => I18n.Config_JunimoHuts_MoveIntoChests(),
                    nameof(JunimoHutBehavior.MoveIntoHut) => I18n.Config_JunimoHuts_MoveIntoHuts(),
                    _ => "???" // should never happen
                }
            );
        }

        /****
        ** Connectors
        ****/
        /// <summary>Get whether the given item name isn't one of the connectors listed in <see cref="DataModel.FloorNames"/>.</summary>
        /// <param name="name">The item name.</param>
        private bool IsCustomConnector(string name)
        {
            foreach (DataModelFloor floor in this.Data.FloorNames.Values)
            {
                if (string.Equals(floor.Name, name, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        /// <summary>Get whether the given item name is enabled as a connector.</summary>
        /// <param name="config">The mod configuration to check.</param>
        /// <param name="name">The item name.</param>
        private bool HasConnector(ModConfig config, string name)
        {
            return config.ConnectorNames.Contains(name);
        }

        /// <summary>Set whether the given item name is enabled as a connector.</summary>
        /// <param name="config">The mod configuration to check.</param>
        /// <param name="name">The item name.</param>
        /// <param name="enable">Whether the item should be enabled; else it should be disabled.</param>
        private void SetConnector(ModConfig config, string name, bool enable)
        {
            if (enable)
                config.ConnectorNames.Add(name);
            else
                config.ConnectorNames.Remove(name);
        }

        /// <summary>Set whether the given item name is enabled as a connector.</summary>
        /// <param name="config">The mod configuration to check.</param>
        /// <param name="rawNames">The raw connector names to set.</param>
        private void SetCustomConnectors(ModConfig config, IEnumerable<string> rawNames)
        {
            var names = new HashSet<string>(rawNames);

            foreach (string name in config.ConnectorNames)
            {
                if (!names.Contains(name) && this.IsCustomConnector(name))
                    config.ConnectorNames.Remove(name);
            }

            foreach (string name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                    config.ConnectorNames.Add(name);
            }
        }

        /****
        ** Machine overrides
        ****/
        /// <summary>Get the translated name for a machine.</summary>
        /// <param name="key">The unique machine key.</param>
        public string GetTranslatedMachineName(string key)
        {
            return I18n.GetByKey($"config.override.{key}-name").Default(key);
        }

        /// <summary>Get the custom override for a mod, if any.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="name">The machine name.</param>
        public ModConfigMachine? GetCustomOverride(ModConfig config, string name)
        {
            return config.MachineOverrides.TryGetValue(name, out ModConfigMachine? @override)
                ? @override
                : null;
        }

        /// <summary>Get the default override for a mod, if any.</summary>
        /// <param name="name">The machine name.</param>
        public ModConfigMachine? GetDefaultOverride(string name)
        {
            return this.Data.DefaultMachineOverrides.TryGetValue(name, out ModConfigMachine? @override)
            ? @override
            : null;
        }

        /// <summary>Get whether a machine is currently enabled.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="name">The machine name.</param>
        public bool IsMachineEnabled(ModConfig config, string name)
        {
            return
                this.GetCustomOverride(config, name)?.Enabled
                ?? this.GetDefaultOverride(name)?.Enabled
                ?? true;
        }

        /// <summary>Get the custom override for a mod, if any.</summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="name">The machine name.</param>
        /// <param name="enabled">Whether to set the machine to enabled.</param>
        public void SetCustomOverride(ModConfig config, string name, bool enabled)
        {
            // get updated settings
            ModConfigMachine options = this.GetCustomOverride(config, name) ?? new ModConfigMachine { Enabled = enabled };
            options.Enabled = enabled;

            // check if it matches the default
            ModConfigMachine? defaults = this.GetDefaultOverride(name);
            bool isDefault = defaults != null
                ? options.Enabled == defaults.Enabled && options.Priority == defaults.Priority
                : !options.GetCustomSettings().Any();

            // apply
            if (isDefault)
                config.MachineOverrides.Remove(name);
            else
                config.MachineOverrides[name] = options;
        }
    }
}
