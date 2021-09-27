using System;
using System.Collections.Generic;
using Pathoschild.Stardew.Common.Patching;
using Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;
using Pathoschild.Stardew.CropsAnytimeAnywhere.Patches;
using StardewModdingAPI;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private LocationConfigManager Config;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = new LocationConfigManager(
                helper.ReadConfig<ModConfig>()
            );

            // read data
            var fallbackTileTypes = this.LoadFallbackTileTypes();

            // add patches
            HarmonyPatcher.Apply(this,
                new LocationPatcher(this.Monitor, this.Config, fallbackTileTypes)
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Load the fallback tile types.</summary>
        /// <returns>Returns the overrides if valid, else null.</returns>
        private IDictionary<string, IDictionary<int, string>> LoadFallbackTileTypes()
        {
            const string path = "assets/data.json";

            try
            {
                // load raw file
                var raw = this.Helper.Data.ReadJsonFile<ModData>(path);
                if (raw == null)
                {
                    this.Monitor.Log($"Can't find '{path}' file. Some features might not work; consider reinstalling the mod to fix this.", LogLevel.Warn);
                    return null;
                }

                // parse file
                var data = new Dictionary<string, IDictionary<int, string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var tilesheetGroup in raw.FallbackTileTypes)
                {
                    string tilesheetName = tilesheetGroup.Key;

                    var typeLookup = new Dictionary<int, string>();
                    foreach (var tileGroup in tilesheetGroup.Value)
                    {
                        foreach (int id in tileGroup.Value)
                            typeLookup[id] = tileGroup.Key;
                    }

                    data[tilesheetName] = typeLookup;
                }

                return data;
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Can't load '{path}' file (see log for details). Some features might not work; consider reinstalling the mod to fix this.", LogLevel.Warn);
                this.Monitor.Log(ex.ToString());
                return null;
            }
        }
    }
}
