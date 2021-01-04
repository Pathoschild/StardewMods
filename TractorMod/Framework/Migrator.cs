using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;

namespace Pathoschild.Stardew.TractorMod.Framework
{
    /// <summary>Handles migrating legacy Tractor Mod data to the latest version.</summary>
    internal static class Migrator
    {
        /*********
        ** Fields
        *********/
        /// <summary>The key for the <see cref="Game1.CustomData"/> entry containing the last migrated version.</summary>
        private const string LastVersionKey = "Pathoschild.TractorMod/version";

        /// <summary>The <see cref="Building.maxOccupants"/> value which identified a tractor garage before Tractor Mod 4.13.</summary>
        private const int LegacyMaxOccupantsID = -794739;

        /// <summary>The absolute path to the mod file used to store info before Tractor Mod 4.7.</summary>
        private static string LegacySaveDataRelativePath => Path.Combine("data", $"{Constants.SaveFolderName}.json");

        /// <summary>The actions to perform when the current session is saved.</summary>
        private static readonly List<Action> OnSaved = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Migrate tractors and garages from older versions of the mod.</summary>
        /// <param name="helper">The main SMAPI API helper for Tractor Mod.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="currentVersion">The current Tractor Mod version.</param>
        /// <param name="getBlueprint">Get a blueprint to construct the tractor garage.</param>
        /// <remarks>The Robin construction logic is derived from <see cref="NPC.reloadSprite"/> and <see cref="Farm.resetForPlayerEntry"/>.</remarks>
        public static void AfterLoad(IModHelper helper, IMonitor monitor, ISemanticVersion currentVersion, Func<BluePrint> getBlueprint)
        {
            if (!Context.IsMainPlayer)
                return;

            // reset if the the previous migration wasn't saved
            Migrator.OnSaved.Clear();

            // get info
            ISemanticVersion lastVersion = Migrator.GetVersion(Migrator.LastVersionKey) ?? new SemanticVersion(4, 6, 0);
            Lazy<BuildableGameLocation[]> buildableLocations = new(() => CommonHelper.GetLocations().OfType<BuildableGameLocation>().ToArray());

            // apply migrations
            if (lastVersion.IsOlderThan("4.7.0"))
                Migrator.Migrate_to_4_7(helper, monitor, buildableLocations.Value, getBlueprint);
            if (lastVersion.IsOlderThan("4.13.0"))
                Migrator.Migrate_To_4_13(buildableLocations.Value);

            // update version
            Game1.CustomData[Migrator.LastVersionKey] = currentVersion.ToString();
        }

        /// <summary>Perform any cleanup needed before save.</summary>
        public static void AfterSave()
        {
            foreach (var migration in Migrator.OnSaved)
                migration();

            Migrator.OnSaved.Clear();
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Migrations
        ****/
        /// <summary>Migrate to Tractor Mod 4.7.</summary>
        /// <param name="helper">The main SMAPI API helper for Tractor Mod.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="locations">The locations to scan for tractors and garages.</param>
        /// <param name="getBlueprint">Get a blueprint to construct the tractor garage.</param>
        private static void Migrate_to_4_7(IModHelper helper, IMonitor monitor, BuildableGameLocation[] locations, Func<BluePrint> getBlueprint)
        {
            // fix building types
            // Tractor Mod 4.7 replaces custom TractorGarage buildings vanilla stables, with a flag
            // in the building.MaxOccupants field.
            foreach (BuildableGameLocation location in locations)
            {
                foreach (Stable stable in location.buildings.OfType<Stable>())
                {
                    if (stable.buildingType.Value == "TractorGarage")
                    {
                        monitor.LogOnce("Migrating custom buildings from Tractor Mod 4.6 or earlier.", LogLevel.Info);

                        stable.buildingType.Value = "Stable";
                        stable.maxOccupants.Value = Migrator.LegacyMaxOccupantsID;
                    }
                }
            }

            // load legacy save data
            // Tractor Mod 4.7 replaces custom data files with vanilla stables/horses.
            {
                LegacySaveDataBuilding[] legacyBuildings =
                    helper.Data.ReadSaveData<LegacySaveData>("tractors")?.Buildings // 4.6
                    ?? helper.Data.ReadJsonFile<LegacySaveData>(Migrator.LegacySaveDataRelativePath)?.Buildings; // pre-4.6

                if (legacyBuildings?.Any() == true)
                {
                    monitor.LogOnce("Migrating custom data from Tractor Mod 4.6 or earlier.", LogLevel.Info);

                    foreach (LegacySaveDataBuilding garageData in legacyBuildings)
                    {
                        // get location
                        BuildableGameLocation location = locations.FirstOrDefault(p => p.NameOrUniqueName == (garageData.Map ?? "Farm"));
                        if (location == null)
                        {
                            monitor.Log($"Ignored legacy tractor garage in unknown location '{garageData.Map}'.", LogLevel.Warn);
                            continue;
                        }

                        // add garage
                        Stable garage = location.buildings.OfType<Stable>().FirstOrDefault(p => p.tileX.Value == (int)garageData.Tile.X && p.tileY.Value == (int)garageData.Tile.Y);
                        if (garage == null)
                        {
                            garage = new Stable(garageData.TractorID, getBlueprint(), garageData.Tile);
                            garage.daysOfConstructionLeft.Value = 0;
                            location.buildings.Add(garage);
                        }
                        garage.maxOccupants.Value = Migrator.LegacyMaxOccupantsID;
                        garage.load();
                    }
                }
            }

            // remove legacy 4.6 data
            helper.Data.WriteSaveData<LegacySaveData>("tractors", null);

            // remove legacy pre-4.6 data
            Migrator.OnSaved.Add(() =>
            {
                FileInfo legacyFile = new FileInfo(Path.Combine(helper.DirectoryPath, Migrator.LegacySaveDataRelativePath));
                if (legacyFile.Exists)
                    legacyFile.Delete();
            });
        }

        /// <summary>Migrate to Tractor Mod 4.13.</summary>
        /// <param name="locations">The locations to scan for tractors and garages.</param>
        private static void Migrate_To_4_13(BuildableGameLocation[] locations)
        {
            // Tractor Mod 4.13 migrates to the modData field to track whether a stable/horse is
            // part of Tractor Mod.
            foreach (BuildableGameLocation location in locations)
            {
                foreach (Stable stable in location.buildings.OfType<Stable>())
                {
                    Horse horse = stable.getStableHorse();
                    if (horse != null && horse.Name.StartsWith("tractor/"))
                        TractorManager.SetTractorInfo(horse);
                }
            }
        }


        /****
        ** Helpers
        ****/
        /// <summary>Get a version value from a <see cref="Game1.CustomData"/> entry.</summary>
        /// <param name="key">The entry key to read.</param>
        private static ISemanticVersion GetVersion(string key)
        {
            return Game1.CustomData.TryGetValue(key, out string rawVersion) && SemanticVersion.TryParse(rawVersion, out ISemanticVersion version)
                ? version
                : null;
        }
    }
}
