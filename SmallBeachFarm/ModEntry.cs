using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Patching;
using Pathoschild.Stardew.SmallBeachFarm.Framework;
using Pathoschild.Stardew.SmallBeachFarm.Framework.Config;
using Pathoschild.Stardew.SmallBeachFarm.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Locations;
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
        private const string DataFileHash = "db6d8c6fb6cc1554c091430476513727";

        /// <summary>The mod configuration.</summary>
        private ModConfig Config = null!; // set in Entry

        /// <summary>The mod's hardcoded data.</summary>
        private ModData Data = null!; // set in Entry


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            CommonHelper.RemoveObsoleteFiles(this, "SmallBeachFarm.pdb"); // removed in 2.4.7

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
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            // hook Harmony patch
            HarmonyPatcher.Apply(this,
                new FarmPatcher(
                    config: this.Config,
                    isSmallBeachFarm: this.IsSmallBeachFarm
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
            const string farmKey = "Pathoschild_SmallBeachFarm";

            // add farm type
            if (e.NameWithoutLocale.IsEquivalentTo("Data/AdditionalFarms"))
            {
                e.Edit(editor =>
                {
                    var data = editor.GetData<List<ModFarmType>>();
                    data.Add(new()
                    {
                        Id = this.ModManifest.UniqueID,
                        TooltipStringPath = $"Strings/UI:{farmKey}_Description",
                        MapName = farmKey
                    });
                });
            }

            // add farm description
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/UI"))
            {
                e.Edit(editor =>
                {
                    var data = editor.AsDictionary<string, string>().Data;

                    // key used by the title screen
                    data[$"{farmKey}_Description"] = $"{I18n.Farm_Name()}_{I18n.Farm_Description()}";

                    // custom keys used in data.json
                    data[$"{farmKey}_Name"] = I18n.Farm_Name();
                    data[$"{farmKey}_FishArea_River"] = I18n.Farm_FishAreas_River();
                    data[$"{farmKey}_FishArea_Estuary"] = I18n.Farm_FishAreas_Estuary();
                    data[$"{farmKey}_FishArea_Ocean"] = I18n.Farm_FishAreas_Ocean();
                });
            }

            // add farm location data
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                e.Edit(editor =>
                {
                    var data = editor.AsDictionary<string, LocationData>().Data;
                    data[$"Farm_{this.ModManifest.UniqueID}"] = this.Data.LocationData;
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
                        Layer buildingsLayer = map.RequireLayer("Buildings");
                        Layer backLayer = map.RequireLayer("Back");
                        Layer behindBackLayer = map.RequireLayer("Back-1");

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
                            var groundTile = backLayer.Tiles[65, 23];

                            behindBackLayer.Tiles[65, 23] = new StaticTile(behindBackLayer, groundTile.TileSheet, groundTile.BlendMode, groundTile.TileIndex); // copy ground tile to layer under back, so we can put the driftwood pile on the back layer
                            backLayer.Tiles[65, 23] = new StaticTile(backLayer, map.GetTileSheet("zbeach"), BlendMode.Alpha, 157); // driftwood pile
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

                        return map;
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

        /// <summary>Get whether the given location is the Small Beach Farm.</summary>
        /// <param name="location">The location to check.</param>
        private bool IsSmallBeachFarm(GameLocation? location)
        {
            return
                Game1.whichModFarm?.Id == this.ModManifest.UniqueID
                && location?.Name == "Farm"
                && location is Farm;
        }
    }
}
