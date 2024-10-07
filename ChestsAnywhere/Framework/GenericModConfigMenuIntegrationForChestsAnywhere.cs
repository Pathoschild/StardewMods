using System;
using System.Linq;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForChestsAnywhere
    {
        /*********
        ** Fields
        *********/
        /// <summary>The Generic Mod Config Menu integration.</summary>
        private readonly GenericModConfigMenuIntegration<ModConfig> ConfigMenu;

        /// <summary>The name for the mines and Skull Cavern for the <see cref="ModConfig.DisabledInLocations"/> field.</summary>
        private const string MinesName = "UndergroundMine";



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
        public GenericModConfigMenuIntegrationForChestsAnywhere(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
        {
            this.ConfigMenu = new GenericModConfigMenuIntegration<ModConfig>(modRegistry, monitor, manifest, getConfig, reset, saveAndApply);
        }

        /// <summary>Register the config menu if available.</summary>
        public void Register()
        {
            var menu = this.ConfigMenu;
            if (!menu.IsLoaded)
                return;

            menu.Register();
            menu
                // general options
                .AddSectionTitle(I18n.Config_Title_GeneralOptions)
                .AddCheckbox(
                    name: I18n.Config_ShowHoverTooltips_Name,
                    tooltip: I18n.Config_ShowHoverTooltips_Desc,
                    get: config => config.ShowHoverTooltips,
                    set: (config, value) => config.ShowHoverTooltips = value
                )
                .AddCheckbox(
                    name: I18n.Config_EnableShippingBin_Name,
                    tooltip: I18n.Config_EnableShippingBin_Desc,
                    get: config => config.EnableShippingBin,
                    set: (config, value) => config.EnableShippingBin = value
                )
                .AddCheckbox(
                    name: I18n.Config_EnableSprinklerAttachments_Name,
                    tooltip: I18n.Config_EnableSprinklerAttachments_Desc,
                    get: config => config.EnableSprinklerAttachments,
                    set: (config, value) => config.EnableSprinklerAttachments = value
                )
                .AddCheckbox(
                    name: I18n.Config_AddOrganizePlayerItems_Name,
                    tooltip: I18n.Config_AddOrganizePlayerItems_Desc,
                    get: config => config.AddOrganizePlayerInventoryButton,
                    set: (config, value) => config.AddOrganizePlayerInventoryButton = value
                )

                // remote access
                .AddSectionTitle(I18n.Config_Title_RemoteAccess)
                .AddDropdown(
                    name: I18n.Config_Range_Name,
                    tooltip: I18n.Config_Range_Desc,
                    get: config => config.Range.ToString(),
                    set: (config, value) => config.Range = Enum.Parse<ChestRange>(value, ignoreCase: true),
                    allowedValues: Enum.GetNames<ChestRange>(),
                    formatAllowedValue: name => I18n.GetByKey($"config.range.{name}")
                )
                .AddCheckbox(
                    name: I18n.Config_DisableMines_Name,
                    tooltip: I18n.Config_DisableMines_Desc,
                    get: config => config.DisabledInLocations.Contains(MinesName),
                    set: (config, value) =>
                    {
                        if (value)
                            config.DisabledInLocations.Add(MinesName);
                        else
                            config.DisabledInLocations.Remove(MinesName);
                    }
                )
                .AddTextbox(
                    name: I18n.Config_DisableCustomNames_Name,
                    tooltip: I18n.Config_DisableCustomNames_Desc,
                    get: config => string.Join(", ", config.DisabledInLocations.Where(name => !name.Equals(MinesName, StringComparison.OrdinalIgnoreCase)).OrderBy(p => p, StringComparer.OrdinalIgnoreCase)),
                    set: (config, value) =>
                    {
                        MutableInvariantSet parsed = new(value.Split(',').Select(p => p.Trim()).Where(p => p != string.Empty));
                        if (config.DisabledInLocations.Contains(MinesName))
                            parsed.Add(MinesName);

                        config.DisabledInLocations.Clear();
                        config.DisabledInLocations.AddRange(parsed);
                    }
                )

                // hotkey to open chest UI
                .AddSectionTitle(I18n.Config_Title_OpenMenuHotkey)
                .AddKeyBinding(
                    name: I18n.Config_ToggleUiKey_Name,
                    tooltip: I18n.Config_ToggleUiKey_Desc,
                    get: config => config.Controls.Toggle,
                    set: (config, value) => config.Controls.Toggle = value
                )
                .AddCheckbox(
                    name: I18n.Config_ReopenLastChest_Name,
                    tooltip: I18n.Config_ReopenLastChest_Desc,
                    get: config => config.ReopenLastChest,
                    set: (config, value) => config.ReopenLastChest = value
                )
                .AddTextbox(
                    name: I18n.Config_DefaultCategory_Name,
                    tooltip: I18n.Config_DefaultCategory_Desc,
                    get: config => config.DefaultCategory ?? "",
                    set: (config, value) => config.DefaultCategory = !string.IsNullOrWhiteSpace(value) ? value : null
                )

                // menu controls
                .AddSectionTitle(I18n.Config_Title_MenuControls)
                .AddKeyBinding(
                    name: I18n.Config_EditChest_Name,
                    tooltip: I18n.Config_EditChest_Desc,
                    get: config => config.Controls.EditChest,
                    set: (config, value) => config.Controls.EditChest = value
                )
                .AddKeyBinding(
                    name: I18n.Config_SortItems_Name,
                    tooltip: I18n.Config_SortItems_Desc,
                    get: config => config.Controls.SortItems,
                    set: (config, value) => config.Controls.SortItems = value
                )
                .AddKeyBinding(
                    name: I18n.Config_NavigatePrevChest_Name,
                    tooltip: I18n.Config_NavigatePrevChest_Desc,
                    get: config => config.Controls.PrevChest,
                    set: (config, value) => config.Controls.PrevChest = value
                )
                .AddKeyBinding(
                    name: I18n.Config_NavigateNextChest_Name,
                    tooltip: I18n.Config_NavigateNextChest_Desc,
                    get: config => config.Controls.NextChest,
                    set: (config, value) => config.Controls.NextChest = value
                )
                .AddKeyBinding(
                    name: I18n.Config_NavigatePrevCategory_Name,
                    tooltip: I18n.Config_NavigatePrevCategory_Desc,
                    get: config => config.Controls.PrevCategory,
                    set: (config, value) => config.Controls.PrevCategory = value
                )
                .AddKeyBinding(
                    name: I18n.Config_NavigateNextCategory_Name,
                    tooltip: I18n.Config_NavigateNextCategory_Desc,
                    get: config => config.Controls.NextCategory,
                    set: (config, value) => config.Controls.NextCategory = value
                )
                .AddKeyBinding(
                    name: I18n.Config_HoldToScrollCategories_Name,
                    tooltip: I18n.Config_HoldToScrollCategories_Desc,
                    get: config => config.Controls.HoldToMouseWheelScrollCategories,
                    set: (config, value) => config.Controls.HoldToMouseWheelScrollCategories = value
                )
                .AddKeyBinding(
                    name: I18n.Config_HoldToScrollChests_Name,
                    tooltip: I18n.Config_HoldToScrollChests_Desc,
                    get: config => config.Controls.HoldToMouseWheelScrollChests,
                    set: (config, value) => config.Controls.HoldToMouseWheelScrollChests = value
                );
        }
    }
}
