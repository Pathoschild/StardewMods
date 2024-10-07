using System;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
    internal class GenericModConfigMenuIntegrationForLookupAnything
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
        public GenericModConfigMenuIntegrationForLookupAnything(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<ModConfig> getConfig, Action reset, Action saveAndApply)
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
            menu
                .Register()

                // main options
                .AddSectionTitle(I18n.Config_Title_MainOptions)
                .AddCheckbox(
                    name: I18n.Config_ForceFullScreen_Name,
                    tooltip: I18n.Config_ForceFullScreen_Desc,
                    get: config => config.ForceFullScreen,
                    set: (config, value) => config.ForceFullScreen = value
                )

                // progression mode
                .AddSectionTitle(I18n.Config_Title_Progression)
                .AddCheckbox(
                    name: I18n.Config_Progression_ShowUnknownGiftTastes_Name,
                    tooltip: I18n.Config_Progression_ShowUnknownGiftTastes_Desc,
                    get: config => config.ShowUnknownGiftTastes,
                    set: (config, value) => config.ShowUnknownGiftTastes = value
                )
                .AddCheckbox(
                    name: I18n.Config_Progression_ShowUnknownRecipes_Name,
                    tooltip: I18n.Config_Progression_ShowUnknownRecipes_Desc,
                    get: config => config.ShowUnknownRecipes,
                    set: (config, value) => config.ShowUnknownRecipes = value
                )
                .AddCheckbox(
                    name: I18n.Config_Progression_ShowPuzzleSolutions_Name,
                    tooltip: I18n.Config_Progression_ShowPuzzleSolutions_Desc,
                    get: config => config.ShowPuzzleSolutions,
                    set: (config, value) => config.ShowPuzzleSolutions = value
                )

                // gift tastes
                .AddSectionTitle(I18n.Config_Title_GiftTastes)
                .AddCheckbox(
                    name: I18n.Config_ShowGiftTastes_Loved_Name,
                    tooltip: I18n.Config_ShowGiftTastes_Loved_Desc,
                    get: config => config.ShowGiftTastes.Loved,
                    set: (config, value) => config.ShowGiftTastes.Loved = value
                )
                .AddCheckbox(
                    name: I18n.Config_ShowGiftTastes_Liked_Name,
                    tooltip: I18n.Config_ShowGiftTastes_Liked_Desc,
                    get: config => config.ShowGiftTastes.Liked,
                    set: (config, value) => config.ShowGiftTastes.Liked = value
                )
                .AddCheckbox(
                    name: I18n.Config_ShowGiftTastes_Neutral_Name,
                    tooltip: I18n.Config_ShowGiftTastes_Neutral_Desc,
                    get: config => config.ShowGiftTastes.Neutral,
                    set: (config, value) => config.ShowGiftTastes.Neutral = value
                )
                .AddCheckbox(
                    name: I18n.Config_ShowGiftTastes_Disliked_Name,
                    tooltip: I18n.Config_ShowGiftTastes_Disliked_Desc,
                    get: config => config.ShowGiftTastes.Disliked,
                    set: (config, value) => config.ShowGiftTastes.Disliked = value
                )
                .AddCheckbox(
                    name: I18n.Config_ShowGiftTastes_Hated_Name,
                    tooltip: I18n.Config_ShowGiftTastes_Hated_Desc,
                    get: config => config.ShowGiftTastes.Hated,
                    set: (config, value) => config.ShowGiftTastes.Hated = value
                )
                .AddCheckbox(
                    name: I18n.Config_ShowUnownedGifts_Name,
                    tooltip: I18n.Config_ShowUnownedGifts_Desc,
                    get: config => config.ShowUnownedGifts,
                    set: (config, value) => config.ShowUnownedGifts = value
                )
                .AddCheckbox(
                    name: I18n.Config_HighlightUnrevealedGiftTastes_Name,
                    tooltip: I18n.Config_HighlightUnrevealedGiftTastes_Desc,
                    get: config => config.HighlightUnrevealedGiftTastes,
                    set: (config, value) => config.HighlightUnrevealedGiftTastes = value
                )

                // advanced options
                .AddSectionTitle(I18n.Config_Title_CollapseFields)
                .AddCheckbox(
                    name: I18n.Config_CollapseFields_Enabled_Name,
                    tooltip: I18n.Config_CollapseFields_Enabled_Desc,
                    get: config => config.CollapseLargeFields.Enabled,
                    set: (config, value) => config.CollapseLargeFields.Enabled = value
                )
                .AddNumberField(
                    name: I18n.Config_CollapseFields_BuildingRecipes_Name,
                    tooltip: I18n.Config_CollapseFields_Any_Desc,
                    get: config => config.CollapseLargeFields.BuildingRecipes,
                    set: (config, value) => config.CollapseLargeFields.BuildingRecipes = value,
                    min: 1,
                    max: 1000
                )
                .AddNumberField(
                    name: I18n.Config_CollapseFields_ItemRecipes_Name,
                    tooltip: I18n.Config_CollapseFields_Any_Desc,
                    get: config => config.CollapseLargeFields.ItemRecipes,
                    set: (config, value) => config.CollapseLargeFields.ItemRecipes = value,
                    min: 1,
                    max: 1000
                )
                .AddNumberField(
                    name: I18n.Config_CollapseFields_NpcGiftTastes_Name,
                    tooltip: I18n.Config_CollapseFields_Any_Desc,
                    get: config => config.CollapseLargeFields.NpcGiftTastes,
                    set: (config, value) => config.CollapseLargeFields.NpcGiftTastes = value,
                    min: 1,
                    max: 1000
                )

                // advanced options
                .AddSectionTitle(I18n.Config_Title_AdvancedOptions)
                .AddCheckbox(
                    name: I18n.Config_TileLookups_Name,
                    tooltip: I18n.Config_TileLookups_Desc,
                    get: config => config.EnableTileLookups,
                    set: (config, value) => config.EnableTileLookups = value
                )
                .AddCheckbox(
                    name: I18n.Config_DataMiningFields_Name,
                    tooltip: I18n.Config_DataMiningFields_Desc,
                    get: config => config.ShowDataMiningFields,
                    set: (config, value) => config.ShowDataMiningFields = value
                )
                .AddCheckbox(
                    name: I18n.Config_ShowInvalidRecipes_Name,
                    tooltip: I18n.Config_ShowInvalidRecipes_Desc,
                    get: config => config.ShowInvalidRecipes,
                    set: (config, value) => config.ShowInvalidRecipes = value
                )
                .AddCheckbox(
                    name: I18n.Config_TargetRedirection_Name,
                    tooltip: I18n.Config_TargetRedirection_Desc,
                    get: config => config.EnableTargetRedirection,
                    set: (config, value) => config.EnableTargetRedirection = value
                )
                .AddNumberField(
                    name: I18n.Config_ScrollAmount_Name,
                    tooltip: I18n.Config_ScrollAmount_Desc,
                    get: config => config.ScrollAmount,
                    set: (config, value) => config.ScrollAmount = value,
                    min: 1,
                    max: 500
                )

                // controls
                .AddSectionTitle(I18n.Config_Title_Controls)
                .AddCheckbox(
                    name: I18n.Config_HideOnKeyUp_Name,
                    tooltip: I18n.Config_HideOnKeyUp_Desc,
                    get: config => config.HideOnKeyUp,
                    set: (config, value) => config.HideOnKeyUp = value
                )
                .AddKeyBinding(
                    name: I18n.Config_ToggleLookup_Name,
                    tooltip: I18n.Config_ToggleLookup_Desc,
                    get: config => config.Controls.ToggleLookup,
                    set: (config, value) => config.Controls.ToggleLookup = value
                )
                .AddKeyBinding(
                    name: I18n.Config_ToggleSearch_Name,
                    tooltip: I18n.Config_ToggleSearch_Desc,
                    get: config => config.Controls.ToggleSearch,
                    set: (config, value) => config.Controls.ToggleSearch = value
                )
                .AddKeyBinding(
                    name: I18n.Config_ScrollUp_Name,
                    tooltip: I18n.Config_ScrollUp_Desc,
                    get: config => config.Controls.ScrollUp,
                    set: (config, value) => config.Controls.ScrollUp = value
                )
                .AddKeyBinding(
                    name: I18n.Config_ScrollDown_Name,
                    tooltip: I18n.Config_ScrollDown_Desc,
                    get: config => config.Controls.ScrollDown,
                    set: (config, value) => config.Controls.ScrollDown = value
                )
                .AddKeyBinding(
                    name: I18n.Config_PageUp_Name,
                    tooltip: I18n.Config_PageUp_Desc,
                    get: config => config.Controls.PageUp,
                    set: (config, value) => config.Controls.PageUp = value
                )
                .AddKeyBinding(
                    name: I18n.Config_PageDown_Name,
                    tooltip: I18n.Config_PageDown_Desc,
                    get: config => config.Controls.PageDown,
                    set: (config, value) => config.Controls.PageDown = value
                )
                .AddKeyBinding(
                    name: I18n.Config_ToggleDebug_Name,
                    tooltip: I18n.Config_ToggleDebug_Desc,
                    get: config => config.Controls.ToggleDebug,
                    set: (config, value) => config.Controls.ToggleDebug = value
                );
        }
    }
}
