using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Patching;
using Pathoschild.Stardew.SmallBeachFarm.Framework;
using Pathoschild.Stardew.SmallBeachFarm.Framework.Config;
using Pathoschild.Stardew.SmallBeachFarm.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.SmallBeachFarm
{
    /// <summary>The mod entry class loaded by SMAPI.</summary>
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
    {
        /*********
        ** Fields
        *********/
        /// <summary>The MD5 hash for the default data.json file.</summary>
        private const string DataFileHash = "641585fd329fac69e377cb911cf70862";

        /// <summary>The relative path to the folder containing tilesheet variants.</summary>
        private readonly string TilesheetsPath = Path.Combine("assets", "tilesheets");

        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The mod's hardcoded data.</summary>
        private ModData Data;

        /// <summary>A fake asset key prefix from which to load tilesheets.</summary>
        private string FakeAssetPrefix => PathUtilities.NormalizeAssetName($"Mods/{this.ModManifest.UniqueID}");


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);

            // read data
            this.Data = this.Helper.Data.ReadJsonFile<ModData>("assets/data.json");
            {
                string dataPath = Path.Combine(this.Helper.DirectoryPath, "assets", "data.json");
                if (this.Data == null || !File.Exists(dataPath))
                {
                    this.Monitor.Log("The mod's 'assets/data.json' file is missing, so this mod can't work correctly. Please reinstall the mod to fix this.", LogLevel.Error);
                    return;
                }
                if (CommonHelper.GetFileHash(dataPath) != ModEntry.DataFileHash)
                    this.Monitor.Log("Found edits to 'assets/data.json'.");
            }

            // read config
            this.Config = this.Helper.ReadConfig<ModConfig>();

            // hook events
            helper.Events.GameLoop.DayEnding += this.DayEnding;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            // hook Harmony patch
            HarmonyPatcher.Apply(this,
                new FarmPatcher(
                    this.Monitor,
                    addCampfire: this.Config.AddCampfire,
                    useBeachMusic: this.Config.UseBeachMusic,
                    isSmallBeachFarm: location => this.IsSmallBeachFarm(location, out _),
                    getFishType: this.GetFishType
                )
            );
        }

        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return
                asset.AssetNameEquals("Data/AdditionalFarms")
                || asset.AssetNameEquals("Strings/UI");
        }

        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            // add farm type
            if (asset.AssetNameEquals("Data/AdditionalFarms"))
            {
                var data = (List<ModFarmType>)asset.Data;
                data.Add(new()
                {
                    ID = this.ModManifest.UniqueID,
                    TooltipStringPath = "Strings/UI:Pathoschild_BeachFarm_Description",
                    MapName = "Pathoschild_SmallBeachFarm"
                });
            }

            // add farm description
            else if (asset.AssetNameEquals("Strings/UI"))
            {
                var data = asset.AsDictionary<string, string>().Data;
                data["Pathoschild_BeachFarm_Description"] = $"{I18n.Farm_Name()}_{I18n.Farm_Description()}";
            }
        }

        /// <inheritdoc />
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return
                asset.AssetNameEquals("Maps/Pathoschild_SmallBeachFarm")
                || asset.AssetName.StartsWith(this.FakeAssetPrefix);
        }

        /// <inheritdoc />
        public T Load<T>(IAssetInfo asset)
        {
            // load map
            if (asset.AssetNameEquals("Maps/Pathoschild_SmallBeachFarm"))
            {
                // load map
                Map map = this.Helper.Content.Load<Map>("assets/farm.tmx");

                // add islands
                if (this.Config.EnableIslands)
                {
                    Map islands = this.Helper.Content.Load<Map>("assets/islands.tmx");
                    this.Helper.Content.GetPatchHelper(map)
                        .AsMap()
                        .PatchMap(source: islands, targetArea: new Rectangle(0, 26, 56, 49));
                }

                // add campfire
                if (this.Config.AddCampfire)
                {
                    var buildingsLayer = map.GetLayer("Buildings");
                    buildingsLayer.Tiles[65, 23] = new StaticTile(buildingsLayer, map.GetTileSheet("zbeach"), BlendMode.Alpha, 157); // driftwood pile
                    buildingsLayer.Tiles[64, 22] = new StaticTile(buildingsLayer, map.GetTileSheet("untitled tile sheet"), BlendMode.Alpha, 242); // campfire
                }

                // apply tilesheet recolors
                string internalRootKey = this.Helper.Content.GetActualAssetKey($"{this.TilesheetsPath}/_default");
                foreach (TileSheet tilesheet in map.TileSheets)
                {
                    if (tilesheet.ImageSource.StartsWith(internalRootKey + PathUtilities.PreferredAssetSeparator))
                        tilesheet.ImageSource = this.Helper.Content.GetActualAssetKey($"{this.FakeAssetPrefix}/{Path.GetFileNameWithoutExtension(tilesheet.ImageSource)}", ContentSource.GameContent);
                }

                return (T)(object)map;
            }

            // load tilesheet
            if (asset.AssetName.StartsWith(this.FakeAssetPrefix))
            {
                string filename = Path.GetFileName(asset.AssetName);
                if (!Path.HasExtension(filename))
                    filename += ".png";

                // get relative path to load
                string relativePath = new DirectoryInfo(this.GetFullPath(this.TilesheetsPath))
                    .EnumerateDirectories()
                    .FirstOrDefault(p => p.Name != "_default" && this.Helper.ModRegistry.IsLoaded(p.Name))
                    ?.Name;
                relativePath = Path.Combine(this.TilesheetsPath, relativePath ?? "_default", filename);

                // load asset
                Texture2D tilesheet = this.Helper.Content.Load<Texture2D>(relativePath);
                return (T)(object)tilesheet;
            }

            // unknown asset
            throw new NotSupportedException($"Unexpected asset '{asset.AssetName}'.");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The event called after the first game update, once all mods are loaded.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // add Generic Mod Config Menu integration
            new GenericModConfigMenuIntegrationForSmallBeachFarm(
                getConfig: () => this.Config,
                reset: () =>
                {
                    this.Config = new ModConfig();
                    this.Helper.WriteConfig(this.Config);
                },
                saveAndApply: () => this.Helper.WriteConfig(this.Config),
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest
            ).Register();
        }

        /// <summary>Raised before the game ends the current day.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            if (!this.IsSmallBeachFarm(Game1.getFarm(), out Farm farm))
                return;

            // update ocean crab pots before the game does
            GameLocation beach = Game1.getLocationFromName("Beach");
            foreach (CrabPot pot in farm.objects.Values.OfType<CrabPot>())
            {
                if (this.GetFishType(farm, (int)pot.TileLocation.X, (int)pot.TileLocation.Y) == FishType.Ocean)
                    pot.DayUpdate(beach);
            }
        }

        /// <summary>Get the full path for a relative path.</summary>
        /// <param name="relative">The relative path.</param>
        private string GetFullPath(string relative)
        {
            return Path.Combine(this.Helper.DirectoryPath, relative);
        }

        /// <summary>Get whether the given location is the Small Beach Farm.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="farm">The farm instance.</param>
        private bool IsSmallBeachFarm(GameLocation location, out Farm farm)
        {
            if (Game1.whichModFarm?.ID == this.ModManifest.UniqueID && location is Farm { Name: "Farm" } farmInstance)
            {
                farm = farmInstance;
                return true;
            }

            farm = null;
            return false;
        }

        /// <summary>Get the fish that should be available from the given tile.</summary>
        /// <param name="farm">The farm instance.</param>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">The tile Y position.</param>
        private FishType GetFishType(Farm farm, int x, int y)
        {
            // not water
            // This should never happen since it's only called when catching a fish, but just in
            // case fallback to the default farm logic.
            if (farm.doesTileHaveProperty(x, y, "Water", "Back") == null)
                return FishType.Default;

            // mixed fish area
            if (this.Data.MixedFishAreas.Any(p => p.Contains(x, y)))
            {
                return Game1.random.Next(2) == 1
                    ? FishType.Ocean
                    : FishType.River;
            }

            // ocean or river
            string tilesheetId = farm.map
                ?.GetLayer("Back")
                ?.PickTile(new Location(x * Game1.tileSize, y * Game1.tileSize), Game1.viewport.Size)
                ?.TileSheet
                ?.Id;
            return tilesheetId is "zbeach" or "zbeach_farm"
                ? FishType.Ocean
                : FishType.River;
        }
    }
}
