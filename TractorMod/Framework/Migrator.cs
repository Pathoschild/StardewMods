using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;

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

        /// <summary>The absolute path to the mod file used to store info before Tractor Mod 4.7.</summary>
        private static string LegacySaveDataRelativePath => Path.Combine("data", $"{Constants.SaveFolderName}.json");

        /// <summary>The unique ID for the stable building in <c>Data/Buildings</c>.</summary>
        private const string GarageBuildingId = "Pathoschild.TractorMod_Stable";

        /// <summary>The actions to perform when the current session is saved.</summary>
        private static readonly List<Action> OnSaved = new();


        /*********
        ** Public methods
        *********/
        /// <summary>Migrate tractors and garages from older versions of the mod.</summary>
        /// <param name="helper">The main SMAPI API helper for Tractor Mod.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="currentVersion">The current Tractor Mod version.</param>
        /// <remarks>The Robin construction logic is derived from <see cref="NPC.reloadSprite"/> and <see cref="Farm.resetForPlayerEntry"/>.</remarks>
        public static void AfterLoad(IModHelper helper, IMonitor monitor, ISemanticVersion currentVersion)
        {
            if (!Context.IsMainPlayer)
                return;

            // reset if the the previous migration wasn't saved
            Migrator.OnSaved.Clear();

            // get info
            ISemanticVersion lastVersion = Migrator.GetVersion(Migrator.LastVersionKey) ?? new SemanticVersion(4, 6, 0);
            Lazy<GameLocation[]> buildableLocations = new(() => CommonHelper.GetLocations().Where(p => p.IsBuildableLocation()).ToArray());

            // apply migrations
            if (lastVersion.IsOlderThan("4.7.0"))
                Migrator.Migrate_to_4_7(helper, monitor, buildableLocations.Value);
            if (lastVersion.IsOlderThan("4.13.0"))
                Migrator.Migrate_To_4_13(buildableLocations.Value);
            if (lastVersion.IsOlderThan("4.17.2"))
                Migrator.Migrate_To_4_17(buildableLocations.Value);

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
        private static void Migrate_to_4_7(IModHelper helper, IMonitor monitor, GameLocation[] locations)
        {
            // fix building types
            // Tractor Mod 4.7 replaces custom TractorGarage buildings with vanilla stables.
            foreach (GameLocation location in locations)
            {
                for (int i = location.buildings.Count - 1; i >= 0; i--)
                {
                    if (location.buildings[i] is not Stable stable)
                        continue;

                    if (stable.buildingType.Value == "TractorGarage")
                    {
                        monitor.LogOnce("Migrating custom buildings from Tractor Mod 4.6 or earlier.", LogLevel.Info);

                        Stable garage = Migrator.BuildGarage(stable.HorseId, new Vector2(stable.tileX.Value, stable.tileY.Value));
                        location.buildings.RemoveAt(i);
                        location.buildings.Add(garage);
                    }
                }
            }

            // load legacy save data
            // Tractor Mod 4.7 replaces custom data files with vanilla stables/horses.
            {
                LegacySaveDataBuilding[]? legacyBuildings =
                    helper.Data.ReadSaveData<LegacySaveData>("tractors")?.Buildings // 4.6
                    ?? helper.Data.ReadJsonFile<LegacySaveData>(Migrator.LegacySaveDataRelativePath)?.Buildings; // pre-4.6

                if (legacyBuildings?.Any() == true)
                {
                    monitor.LogOnce("Migrating custom data from Tractor Mod 4.6 or earlier.", LogLevel.Info);

                    foreach (LegacySaveDataBuilding garageData in legacyBuildings)
                    {
                        // get location
                        GameLocation? location = locations.FirstOrDefault(p => p.NameOrUniqueName == (garageData.Map ?? "Farm"));
                        if (location == null)
                        {
                            monitor.Log($"Ignored legacy tractor garage in unknown location '{garageData.Map}'.", LogLevel.Warn);
                            continue;
                        }

                        // add garage
                        Stable? garage = location.buildings.OfType<Stable>().FirstOrDefault(p => p.tileX.Value == (int)garageData.Tile.X && p.tileY.Value == (int)garageData.Tile.Y);
                        if (garage == null)
                        {
                            garage = Migrator.BuildGarage(garageData.TractorID, garageData.Tile);
                            location.buildings.Add(garage);
                        }
                        garage.load();
                    }
                }
            }

            // remove legacy 4.6 data
            helper.Data.WriteSaveData<LegacySaveData>("tractors", null);

            // remove legacy pre-4.6 data
            Migrator.OnSaved.Add(() =>
            {
                FileInfo legacyFile = new(Path.Combine(helper.DirectoryPath, Migrator.LegacySaveDataRelativePath));
                if (legacyFile.Exists)
                    legacyFile.Delete();
            });
        }

        /// <summary>Migrate to Tractor Mod 4.13.</summary>
        /// <param name="locations">The locations to scan for tractors and garages.</param>
        private static void Migrate_To_4_13(GameLocation[] locations)
        {
            // Tractor Mod 4.13 migrates to the modData field to track whether a stable/horse is
            // part of Tractor Mod.
            foreach (GameLocation location in locations)
            {
                foreach (Stable stable in location.buildings.OfType<Stable>())
                    Migrator.AddHorseIfNeeded(stable);
            }
        }

        /// <summary>Migrate to Tractor Mod 4.15.</summary>
        /// <param name="locations">The locations to scan for tractors and garages.</param>
        private static void Migrate_To_4_17(GameLocation[] locations)
        {
            // Tractor Mod 4.17 (for Stardew Valley 1.6) migrates from a vanilla stable to a new Data/Buildings
            // building.
            foreach (GameLocation location in locations)
            {
                for (int i = location.buildings.Count - 1; i >= 0; i--)
                {
                    if (location.buildings[i] is not Stable stable || !Migrator.IsTractor(stable.getStableHorse()))
                        continue;

                    Stable garage = Migrator.BuildGarage(stable.HorseId, new Vector2(stable.tileX.Value, stable.tileY.Value));
                    location.buildings.RemoveAt(i);
                    location.buildings.Add(garage);

                    Migrator.AddHorseIfNeeded(garage);
                }
            }
        }


        /****
        ** Helpers
        ****/
        /// <summary>Get a version value from a <see cref="Game1.CustomData"/> entry.</summary>
        /// <param name="key">The entry key to read.</param>
        private static ISemanticVersion? GetVersion(string key)
        {
            return Game1.CustomData.TryGetValue(key, out string? rawVersion) && SemanticVersion.TryParse(rawVersion, out ISemanticVersion? version)
                ? version
                : null;
        }

        /// <summary>Build a tractor garage instance.</summary>
        /// <param name="tractorId">The tractor ID.</param>
        /// <param name="tile">The garage's tile position.</param>
        private static Stable BuildGarage(Guid tractorId, Vector2 tile)
        {
            return new(tile, tractorId)
            {
                daysOfConstructionLeft = { Value = 0 },
                buildingType = { Value = Migrator.GarageBuildingId }
            };
        }

        /// <summary>Add a tractor to the garage if needed.</summary>
        /// <param name="garage">The garage instance.</param>
        private static void AddHorseIfNeeded(Stable garage)
        {
            Horse horse = garage.getStableHorse();
            if (Migrator.IsTractor(horse))
                TractorManager.SetTractorInfo(horse, TractorSoundType.Horse); // sound effects will be reset later
        }

        /// <summary>Get whether the given horse should be treated as a tractor, accounting for legacy tractor data.</summary>
        /// <param name="horse">The horse to check.</param>
        private static bool IsTractor(Horse horse)
        {
            return
                TractorManager.IsTractor(horse)
                || horse?.Name?.StartsWith("tractor/") is true; // pre-4.12.2 tractor
        }
    }
}
