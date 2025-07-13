using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Patching;
using Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;
using Pathoschild.Stardew.CropsAnytimeAnywhere.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod configuration.</summary>
    private ConfigRuleManager Config = null!; // set in Entry


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        CommonHelper.RemoveObsoleteFiles(this, "CropsAnytimeAnywhere.pdb"); // removed in 1.4.7

        // read config & data
        this.Config = new ConfigRuleManager(helper.ReadConfig<ModConfig>());
        var fallbackTileTypes = this.LoadFallbackTileTypes();

        // init
        I18n.Init(helper.Translation);

        // hook up events
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        // add patches
        HarmonyPatcher.Apply(this,
            new FruitTreePatcher(this.Config),
            new LocationPatcher(this.Monitor, this.Config, fallbackTileTypes)
        );
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForCropsAnytimeAnywhere(this.Config.Config),
            get: () => this.Config.Config,
            set: config => this.Config.UpdateConfig(config),
            onSaved: () => this.Config.UpdateConfig(this.Config.Config)
        );
    }

    /// <summary>Load the fallback tile types.</summary>
    /// <returns>Returns the overrides if valid, else null.</returns>
    private Dictionary<string, Dictionary<int, string>> LoadFallbackTileTypes()
    {
        const string path = "assets/data.json";

        try
        {
            // load raw file
            var raw = this.Helper.Data.ReadJsonFile<ModData>(path);
            if (raw == null)
            {
                this.Monitor.Log($"Can't find '{path}' file. Some features might not work; consider reinstalling the mod to fix this.", LogLevel.Warn);
                return new();
            }

            // parse file
            var data = new Dictionary<string, Dictionary<int, string>>(StringComparer.OrdinalIgnoreCase);
            foreach ((string tilesheetName, Dictionary<string, int[]> tileGroups) in raw.FallbackTileTypes)
            {
                var typeLookup = new Dictionary<int, string>();
                foreach ((string type, int[] tileIds) in tileGroups)
                {
                    foreach (int id in tileIds)
                        typeLookup[id] = type;
                }

                data[tilesheetName] = typeLookup;
            }

            return data;
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Can't load '{path}' file (see log for details). Some features might not work; consider reinstalling the mod to fix this.", LogLevel.Warn);
            this.Monitor.Log(ex.ToString());
            return new();
        }
    }
}
