using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Automate.Framework.Machines.Buildings;
using Pathoschild.Stardew.Automate.Framework.Machines.Objects;
using Pathoschild.Stardew.Automate.Framework.Machines.TerrainFeatures;
using Pathoschild.Stardew.Automate.Framework.Machines.Tiles;
using Pathoschild.Stardew.Automate.Framework.Models;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Items;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace Pathoschild.Stardew.Automate.Framework;

/// <summary>Registers the mod configuration with Generic Mod Config Menu.</summary>
internal class GenericModConfigMenuIntegrationForAutomate : IGenericModConfigMenuIntegrationFor<ModConfig>
{
    /*********
    ** Fields
    *********/
    /// <summary>The internal mod data.</summary>
    private readonly DataModel Data;


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="data">The internal mod data.</param>
    public GenericModConfigMenuIntegrationForAutomate(DataModel data)
    {
        this.Data = data;
    }

    /// <inheritdoc />
    public void Register(GenericModConfigMenuIntegration<ModConfig> menu, IMonitor monitor)
    {
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
            .AddNumberField(
                name: I18n.Config_MinMinutesForFairyDust_Name,
                tooltip: I18n.Config_MinMinutesForFairyDust_Desc,
                get: config => config.MinMinutesForFairyDust,
                set: (config, value) => config.MinMinutesForFairyDust = (int)(value / 10 * 10), // enforce 10-minute interval
                min: 10,
                max: 3000,
                interval: 10
            )
            .AddCheckbox(
                name: I18n.Config_WarnForMissingBridgeMod_Name,
                tooltip: I18n.Config_WarnForMissingBridgeMod_Desc,
                get: config => config.WarnForMissingBridgeMod,
                set: (config, value) => config.WarnForMissingBridgeMod = value
            );

        // connectors
        menu.AddSectionTitle(I18n.Config_Title_Connectors);
        foreach (FloorPathData entry in Game1.floorPathData.Values.OrderBy(p => GameI18n.GetObjectName(p.ItemId), StringComparer.OrdinalIgnoreCase)) // sort by English display name; it's not ideal, but we can't re-sort when the language is loaded
        {
            string itemId = entry.ItemId;
            string? internalName = ItemRegistry.GetData(entry.ItemId)?.InternalName;
            if (internalName is null)
                continue;

            menu.AddCheckbox(
                name: () => GameI18n.GetObjectName(itemId),
                tooltip: () => I18n.Config_Connector_Desc(itemName: GameI18n.GetObjectName(itemId)),
                get: config => this.HasConnector(config, internalName),
                set: (config, value) => this.SetConnector(config, internalName, value)
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

        // storage settings
        menu.AddSectionTitle(I18n.Config_Title_ChestSettings);
        menu.AddDropdown(
            name: I18n.Config_DefaultChestOverride_Name,
            tooltip: I18n.Config_DefaultChestOverride_Desc,
            get: config => config.ChestsEnabledByDefault.ToString(),
            set: (config, value) => config.ChestsEnabledByDefault = bool.Parse(value),
            allowedValues: [bool.TrueString, bool.FalseString],
            formatAllowedValue: value => bool.Parse(value)
                ? I18n.Config_ChestOverride_Values_Enabled()
                : I18n.Config_ChestOverride_Values_Disabled()
        );

        // per-storage settings
        foreach (ChestType chestType in this.GetChestTypes())
        {
            Func<string> getName = chestType.DisplayName;
            string itemId = chestType.QualifiedItemId;

            menu.AddDropdown(
                name: () => chestType.IsTakeOnly
                    ? I18n.Config_ChestOverride_Name_TakeOnly(chestName: getName())
                    : I18n.Config_ChestOverride_Name(chestName: getName()),
                tooltip: () =>
                {
                    string description = I18n.Config_ChestOverride_Desc(chestName: getName(), defaultBehaviorField: I18n.Config_DefaultChestOverride_Name());
                    if (chestType.IsTakeOnly)
                        description = I18n.Config_ChestOverride_Desc_TakeOnly(description: description);
                    return description;
                },
                get: config => this.GetChestOverride(config, itemId)?.Enabled.ToString() ?? string.Empty,
                set: (config, value) =>
                {
                    if (bool.TryParse(value, out bool parsed))
                        this.SetChestOverride(config, itemId, parsed);
                    else
                        this.SetChestOverride(config, itemId, null);
                },
                allowedValues: [string.Empty, bool.TrueString, bool.FalseString],
                formatAllowedValue: value =>
                {
                    if (bool.TryParse(value, out bool parsed))
                    {
                        return parsed
                            ? I18n.Config_ChestOverride_Values_Enabled()
                            : I18n.Config_ChestOverride_Values_Disabled();
                    }

                    return I18n.Config_ChestOverride_Values_Default();
                });
        }

        // per-machine settings
        string treeId = BaseMachine.GetDefaultMachineId<TreeMachine>();
        var machines = this
            .GetMachineIds()
            .OrderByDescending(p => p.Key == "MiniShippingBin")
            .ThenByDescending(p => p.Key == "ShippingBin")
            .ThenBy(p => p.Value(), new HumanSortComparer());
        foreach ((string machineId, Func<string> getName) in machines)
        {
            menu.AddSectionTitle(() => I18n.Config_Title_MachineSettings(machineName: getName()));
            menu.AddCheckbox(
                name: I18n.Config_MachineSettingsEnabled_Name,
                tooltip: () => I18n.Config_MachineSettingsEnabled_Desc(machineName: getName()),
                get: config => this.IsMachineEnabled(config, machineId),
                set: (config, value) => this.SetMachineOptions(config, machineId, options => options.Enabled = value)
            );

            if (machineId == treeId)
            {
                menu.AddCheckbox(
                    name: I18n.Config_Machines_WildTree_CollectMoss_Name,
                    tooltip: I18n.Config_Machines_WildTree_CollectMoss_Desc,
                    get: config => config.CollectTreeMoss,
                    set: (config, value) => config.CollectTreeMoss = value
                );
            }

            menu.AddNumberField(
                name: I18n.Config_MachineSettingsPriority_Name,
                tooltip: () => I18n.Config_MachineSettingsPriority_Desc(getName()),
                get: config => this.GetMachinePriority(config, machineId),
                set: (config, value) => this.SetMachineOptions(config, machineId, options => options.Priority = value),
                min: -100,
                max: 100
            );
        }
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
    /// <summary>Get whether the given item name isn't one of the connectors listed in <see cref="Game1.floorPathData"/>.</summary>
    /// <param name="name">The item name.</param>
    private bool IsCustomConnector(string name)
    {
        foreach (FloorPathData floor in Game1.floorPathData.Values)
        {
            string? internalName = ItemRegistry.GetData(floor.ItemId)?.InternalName;

            if (internalName != null && string.Equals(internalName, name, StringComparison.OrdinalIgnoreCase))
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
    ** Chest overrides
    ****/
    /// <summary>Get the chest IDs and display names to show in the config UI.</summary>
    private IEnumerable<ChestType> GetChestTypes()
    {
        var itemRepo = new ItemRepository();
        var itemEntries = itemRepo
            .GetAll(ItemRegistry.type_object, includeVariants: false)
            .Concat(itemRepo.GetAll(ItemRegistry.type_bigCraftable, includeVariants: false));

        foreach (SearchableItem entry in itemEntries)
        {
            if (!entry.Item.HasContextTag(AutomateConstants.StorageTag))
                continue;

            string itemId = entry.Item.QualifiedItemId;

            yield return new ChestType(
                QualifiedItemId: itemId,
                DisplayName: () => this.GetTranslatedChestName(itemId),
                IsTakeOnly: entry.Item.HasContextTag(AutomateConstants.StorageTakeOnlyTag)
            );
        }
    }

    /// <summary>Get the translated display name for a chest.</summary>
    /// <param name="itemId">The qualified item ID for the chest.</param>
    private string GetTranslatedChestName(string itemId)
    {
        ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
        return I18n
            .GetByKey($"config.storages.{data.InternalName.Replace(" ", "-")}")
            .Default(data.DisplayName);
    }

    /// <summary>Get the override for a given chest, if set.</summary>
    /// <param name="config">The config to read.</param>
    /// <param name="itemId">The qualified item ID for the chest.</param>
    private ModConfigStorage? GetChestOverride(ModConfig config, string itemId)
    {
        return config.ChestOverrides.GetValueOrDefault(itemId);
    }

    /// <summary>Set the override for a chest.</summary>
    /// <param name="config">The mod configuration.</param>
    /// <param name="itemId">The qualified item ID for the chest.</param>
    /// <param name="value">The value to set.</param>
    private void SetChestOverride(ModConfig config, string itemId, bool? value)
    {
        if (value is null)
            config.ChestOverrides.Remove(itemId);
        else
        {
            if (!config.ChestOverrides.TryGetValue(itemId, out ModConfigStorage? @override))
                config.ChestOverrides[itemId] = @override = new ModConfigStorage();

            @override.Enabled = value.Value;
        }
    }

    /****
    ** Machine overrides
    ****/
    /// <summary>Get the machine IDs and display names to show in the config UI.</summary>
    private Dictionary<string, Func<string>> GetMachineIds()
    {
        Dictionary<string, Func<string>> machineIds = [];

        // default overrides
        foreach (string rawId in this.Data.DefaultMachineOverrides.Keys)
        {
            string id = rawId; // avoid reference to loop variable
            machineIds[rawId] = () => this.GetTranslatedMachineName(id);
        }

        // Data/Machines
        foreach (string rawItemId in DataLoader.Machines(Game1.content).Keys)
        {
            string itemId = rawItemId;
            ParsedItemData? data = ItemRegistry.GetData(itemId);
            if (data is null)
                continue; // invalid entry

            string machineId = BaseMachine.GetDefaultMachineId(data.InternalName);
            machineIds[machineId] = () => this.GetMachineNameFromItemId(data.QualifiedItemId);
        }

        // Data/Buildings
        foreach ((string buildingType, BuildingData buildingData) in Game1.buildingData)
        {
            if (buildingData?.ItemConversions?.Count is not > 0)
                continue;

            string machineId = BaseMachine.GetDefaultMachineId(buildingType);
            machineIds[machineId] = () => TokenParser.ParseText(buildingData.Name) ?? buildingType;
        }

        // other building machines
        machineIds[BaseMachine.GetDefaultMachineId<FishPondMachine>()] = () => GameI18n.GetBuildingName("Fish Pond");
        machineIds[BaseMachine.GetDefaultMachineId<JunimoHutMachine>()] = () => GameI18n.GetBuildingName("Junimo Hut");

        // other object machines
        machineIds[BaseMachine.GetDefaultMachineId<AutoGrabberMachine>()] = () => this.GetMachineNameFromItemId("(BC)165");
        machineIds[BaseMachine.GetDefaultMachineId<CrabPotMachine>()] = () => this.GetMachineNameFromItemId("(O)710");
        machineIds[BaseMachine.GetDefaultMachineId<FeedHopperMachine>()] = () => this.GetMachineNameFromItemId("(BC)99");
        machineIds[BaseMachine.GetDefaultMachineId<MiniShippingBinMachine>()] = () => this.GetMachineNameFromItemId("(BC)248");

        // other terrain feature machines
        machineIds[BaseMachine.GetDefaultMachineId<BushMachine>()] = I18n.Config_Machines_Bush;
        machineIds[BaseMachine.GetDefaultMachineId<FruitTreeMachine>()] = I18n.Config_Machines_FruitTree;
        machineIds[BaseMachine.GetDefaultMachineId<TreeMachine>()] = I18n.Config_Machines_WildTree;
        machineIds[BaseMachine.GetDefaultMachineId<TrashCanMachine>()] = I18n.Config_Machines_TrashCan;

        return machineIds;
    }

    /// <summary>Get the translated name for a machine.</summary>
    /// <param name="key">The unique item ID or translation key.</param>
    private string GetTranslatedMachineName(string key)
    {
        switch (key)
        {
            case "ShippingBin":
                return GameI18n.GetBuildingName("Shipping Bin");

            case "MiniShippingBin":
                return this.GetMachineNameFromItemId("(BC)248");

            default:
                return ItemRegistry.GetData(key)?.DisplayName ?? key;
        }
    }

    /// <summary>Get the custom override for a mod, if any.</summary>
    /// <param name="config">The mod configuration.</param>
    /// <param name="name">The machine name.</param>
    private ModConfigMachine? GetMachineOverride(ModConfig config, string name)
    {
        return config.MachineOverrides.GetValueOrDefault(name);
    }

    /// <summary>Get the default override for a mod, if any.</summary>
    /// <param name="name">The machine name.</param>
    private ModConfigMachine? GetDefaultMachineOverride(string name)
    {
        return this.Data.DefaultMachineOverrides.GetValueOrDefault(name);
    }

    /// <summary>Get whether a machine is currently enabled.</summary>
    /// <param name="config">The mod configuration.</param>
    /// <param name="name">The machine name.</param>
    private bool IsMachineEnabled(ModConfig config, string name)
    {
        return
            this.GetMachineOverride(config, name)?.Enabled
            ?? this.GetDefaultMachineOverride(name)?.Enabled
            ?? true;
    }

    /// <summary>Get the display name for a machine item.</summary>
    /// <param name="itemId">The machine's item ID.</param>
    private string GetMachineNameFromItemId(string itemId)
    {
        ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
        return I18n
            .GetByKey($"config.machines.{data.InternalName.Replace(" ", "-")}")
            .Default(data.DisplayName);
    }

    /// <summary>Get the current machine priority.</summary>
    /// <param name="config">The mod configuration.</param>
    /// <param name="name">The machine name.</param>
    private int GetMachinePriority(ModConfig config, string name)
    {
        return
            this.GetMachineOverride(config, name)?.Priority
            ?? this.GetDefaultMachineOverride(name)?.Priority
            ?? 0;
    }

    /// <summary>Set the options for a machine.</summary>
    /// <param name="config">The mod configuration.</param>
    /// <param name="name">The machine name.</param>
    /// <param name="set">Set the machine options.</param>
    private void SetMachineOptions(ModConfig config, string name, Action<ModConfigMachine> set)
    {
        // get updated settings
        ModConfigMachine options = this.GetMachineOverride(config, name) ?? new();
        set(options);

        // check if it matches the default
        ModConfigMachine? defaults = this.GetDefaultMachineOverride(name);
        bool isDefault = defaults != null
            ? options.Enabled == defaults.Enabled && options.Priority == defaults.Priority
            : !options.GetCustomSettings().Any();

        // apply
        if (isDefault)
            config.MachineOverrides.Remove(name);
        else
            config.MachineOverrides[name] = options;
    }

    /// <summary>A chest type which can be configured.</summary>
    /// <param name="QualifiedItemId">The unique qualified item ID for the chest type.</param>
    /// <param name="DisplayName">The display name for the chest type.</param>
    /// <param name="IsTakeOnly">Whether items can only be retrieved from this chest type.</param>
    private record ChestType(string QualifiedItemId, Func<string> DisplayName, bool IsTakeOnly);
}
