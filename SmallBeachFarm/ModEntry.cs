using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
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
using StardewValley.Locations;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.SmallBeachFarm
{
    /// <summary>The mod entry class loaded by SMAPI.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The MD5 hash for the default data.json file.</summary>
        private const string DataFileHash = "641585fd329fac69e377cb911cf70862";

        /// <summary>The relative path to the folder containing tilesheet variants.</summary>
        private readonly string TilesheetsPath = Path.Combine("assets", "tilesheets");

        /// <summary>The mod configuration.</summary>
        private ModConfig Config = null!; // set in Entry

        /// <summary>The mod's hardcoded data.</summary>
        private ModData Data = null!; // set in Entry

        /// <summary>A fake asset key prefix from which to load tilesheets.</summary>
        private string FakeAssetPrefix => PathUtilities.NormalizeAssetName($"Mods/{this.ModManifest.UniqueID}");


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);

            // read config
            this.Config = this.Helper.ReadConfig<ModConfig>();

            // read data
            ModData? data = this.Helper.Data.ReadJsonFile<ModData>("assets/data.json");
            this.Data = data ?? new ModData();
            {
                string dataPath = Path.Combine(this.Helper.DirectoryPath, "assets", "data.json");
                if (data == null || !File.Exists(dataPath))
                {
                    this.Monitor.Log("The mod's 'assets/data.json' file is missing, so this mod can't work correctly. Please reinstall the mod to fix this.", LogLevel.Error);
                    return;
                }
                if (CommonHelper.GetFileHash(dataPath) != ModEntry.DataFileHash)
                    this.Monitor.Log("Found edits to 'assets/data.json'.");
            }

            // hook events
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.DayEnding += this.DayEnding;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            // hook Harmony patch
            HarmonyPatcher.Apply(this,
                new FarmPatcher(
                    monitor: this.Monitor,
                    config: this.Config,
                    isSmallBeachFarm: location => this.IsSmallBeachFarm(location, out _),
                    getFishType: this.GetFishType
                ),
                new CharacterCustomizationPatcher(
                    config: this.Config,
                    farmTypeId: this.ModManifest.UniqueID
                )
            );
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IContentEvents.AssetRequested"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            // add farm type
            if (e.NameWithoutLocale.IsEquivalentTo("Data/AdditionalFarms"))
            {
                e.Edit(editor =>
                {
                    var data = editor.GetData<List<ModFarmType>>();
                    data.Add(new()
                    {
                        ID = this.ModManifest.UniqueID,
                        TooltipStringPath = "Strings/UI:Pathoschild_BeachFarm_Description",
                        MapName = "Pathoschild_SmallBeachFarm"
                    });
                });
            }

            // add farm description
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/UI"))
            {
                e.Edit(editor =>
                {
                    var data = editor.AsDictionary<string, string>().Data;
                    data["Pathoschild_BeachFarm_Description"] = $"{I18n.Farm_Name()}_{I18n.Farm_Description()}";
                });
            }

            // load map
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps/Pathoschild_SmallBeachFarm"))
            {
                e.LoadFrom(
                    () =>
                    {
                        // load map
                        Map map = this.Helper.ModContent.Load<Map>("assets/farm.tmx");
                        IAssetDataForMap editor = this.Helper.ModContent.GetPatchHelper(map).AsMap();
                        TileSheet outdoorTilesheet = map.GetTileSheet("untitled tile sheet");
                        Layer buildingsLayer = map.GetLayer("Buildings");
                        Layer backLayer = map.GetLayer("Back");

                        // add islands
                        if (this.Config.EnableIslands)
                        {
                            Map islands = this.Helper.ModContent.Load<Map>("assets/overlay_islands.tmx");
                            Size size = islands.GetSizeInTiles();

                            editor.PatchMap(source: islands, targetArea: new Rectangle(0, 26, size.Width, size.Height));
                        }

                        // add campfire
                        if (this.Config.AddCampfire)
                        {
                            buildingsLayer.Tiles[65, 23] = new StaticTile(buildingsLayer, map.GetTileSheet("zbeach"), BlendMode.Alpha, 157); // driftwood pile
                            buildingsLayer.Tiles[64, 22] = new StaticTile(buildingsLayer, outdoorTilesheet, BlendMode.Alpha, 242); // campfire
                        }

                        // remove shipping bin path
                        if (!this.Config.ShippingBinPath)
                        {
                            for (int x = 71; x <= 72; x++)
                            {
                                for (int y = 14; y <= 15; y++)
                                    backLayer.Tiles[x, y] = new StaticTile(backLayer, outdoorTilesheet, BlendMode.Alpha, 175); // grass tile
                            }
                        }

                        // add fishing pier
                        if (this.Config.AddFishingPier)
                        {
                            // load overlay
                            Map pier = this.Helper.ModContent.Load<Map>("assets/overlay_pier.tmx");
                            Size size = pier.GetSizeInTiles();

                            // get target position
                            Point position = this.Config.CustomFishingPierPosition;
                            if (position == Point.Zero)
                                position = new Point(70, 26);

                            // remove building tiles which block movement on the pier
                            {
                                var pierBack = pier.GetLayer("Back");
                                for (int x = 0; x < size.Width; x++)
                                {
                                    for (int y = 0; y < size.Height; y++)
                                    {
                                        if (pierBack.Tiles[x, y] is not null)
                                            buildingsLayer.Tiles[position.X + x, position.Y + y] = null;
                                    }
                                }
                            }

                            // apply overlay
                            editor.PatchMap(source: pier, targetArea: new Rectangle(position.X, position.Y, size.Width, size.Height));
                        }

                        // apply tilesheet recolors
                        foreach (TileSheet tilesheet in map.TileSheets)
                        {
                            IAssetName imageSource = this.Helper.GameContent.ParseAssetName(tilesheet.ImageSource);
                            if (imageSource.StartsWith($"{this.TilesheetsPath}/_default/"))
                                tilesheet.ImageSource = PathUtilities.NormalizeAssetName($"{this.FakeAssetPrefix}/{Path.GetFileNameWithoutExtension(tilesheet.ImageSource)}");
                        }

                        return map;
                    },
                    AssetLoadPriority.Exclusive
                );
            }

            // load tilesheet
            else if (e.NameWithoutLocale.StartsWith(this.FakeAssetPrefix))
            {
                e.LoadFrom(
                    () =>
                    {
                        string filename = Path.GetFileName(e.NameWithoutLocale.Name);
                        if (!Path.HasExtension(filename))
                            filename += ".png";

                        // get relative path to load
                        string? relativePath = new DirectoryInfo(this.GetFullPath(this.TilesheetsPath))
                            .EnumerateDirectories()
                            .FirstOrDefault(p => p.Name != "_default" && this.Helper.ModRegistry.IsLoaded(p.Name))
                            ?.Name;
                        relativePath = Path.Combine(this.TilesheetsPath, relativePath ?? "_default", filename);

                        // load asset
                        Texture2D tilesheet = this.Helper.ModContent.Load<Texture2D>(relativePath);
                        return tilesheet;
                    },
                    AssetLoadPriority.Exclusive
                );
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
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

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            // when the player first loads the save, fix the broken TV if needed
            if (Context.IsMainPlayer && Game1.currentLocation is FarmHouse farmhouse && Game1.dayOfMonth == 1 && Game1.currentSeason == "spring" && Game1.year == 1)
            {
                var brokenTvs = farmhouse.furniture
                    .Where(furniture => furniture.ParentSheetIndex == 1680 && furniture is not TV)
                    .ToArray();
                foreach (var tv in brokenTvs)
                {
                    farmhouse.furniture.Remove(tv);
                    farmhouse.furniture.Add(new TV(1680, tv.TileLocation));
                }

            }
        }

        /// <inheritdoc cref="IGameLoopEvents.DayEnding"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void DayEnding(object? sender, DayEndingEventArgs e)
        {
            if (!this.IsSmallBeachFarm(Game1.getFarm(), out Farm? farm))
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
        private bool IsSmallBeachFarm(GameLocation? location, [NotNullWhen(true)] out Farm? farm)
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
            string? tilesheetId = farm.map
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
