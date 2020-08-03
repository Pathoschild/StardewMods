using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.ObjectModel;
using xTile.Tiles;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>Whether to enable tilling for more tile types.</summary>
        private bool OverrideTilling;

        /// <summary>The seasons for which to override crops.</summary>
        private HashSet<string> EnabledSeasons;

        /// <summary>The tile types to use for tiles which don't have a type property and aren't marked diggable. Indexed by tilesheet image source (without path or season) and back tile ID.</summary>
        private IDictionary<string, IDictionary<int, string>> FallbackTileTypes;

        /// <summary>The locations which have been greenhouseified.</summary>
        private readonly HashSet<GameLocation> EnabledLocations = new HashSet<GameLocation>(new ObjectReferenceComparer<GameLocation>());


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();
            this.OverrideTilling = this.Config.ForceTillable.IsAnyEnabled();
            this.EnabledSeasons = new HashSet<string>(this.Config.EnableInSeasons.GetEnabledSeasons(), StringComparer.OrdinalIgnoreCase);

            // read data
            this.FallbackTileTypes = this.LoadFallbackTileTypes();

            // hook events
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.World.LocationListChanged += this.OnLocationListChanged;
            if (this.OverrideTilling)
                helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The method called before a tick update.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            GameLocation location = Game1.currentLocation;
            Farmer player = Game1.player;

            // mark tiles under hoe diggable
            if (Context.IsWorldReady && player.UsingTool && player.CurrentTool is Hoe hoe)
            {
                Vector2 tilePos = player.GetToolLocation() / Game1.tileSize;
                IList<Vector2> tilesAffected = this.Helper.Reflection.GetMethod(hoe, "tilesAffected").Invoke<List<Vector2>>(tilePos, player.toolPower, player);
                foreach (Vector2 tile in tilesAffected)
                {
                    if (this.ShouldMakeTillable(location, tile))
                        location.setTileProperty((int)tile.X, (int)tile.Y, "Back", "Diggable", "T");
                }
            }
        }

        /// <summary>The method called after a new day starts.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            // apply changes
            this.EnabledLocations.Clear();
            if (this.ShouldApply())
                this.SetCropMode(CommonHelper.GetLocations(), true, this.EnabledLocations);
        }

        /// <summary>The method called after a location is added or removed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLocationListChanged(object sender, LocationListChangedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            // set greenhouse mode if applicable
            if (this.ShouldApply())
                this.SetCropMode(e.Added, true, this.EnabledLocations);
        }

        /// <summary>The method called before the day ends, before the game sets up the next day.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            // remove changes before next day is calculated if tomorrow is out of season
            if (!this.ShouldApply(SDate.Now().AddDays(1)))
                this.ClearChanges();
        }

        /// <summary>The method called before the day ends, before the game sets up the next day.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            // remove changes before save (to avoid making changes permanent if the mod is uninstalled)
            if (this.EnabledLocations.Any())
            {
                this.ApplyTreeUpdates();
                this.ClearChanges();
            }
        }


        /****
        ** Methods
        ****/
        /// <summary>Whether the mod effects should be enabled for the current date.</summary>
        private bool ShouldApply()
        {
            return this.ShouldApply(SDate.Now());
        }

        /// <summary>Whether the mod effects should be enabled for a given date.</summary>
        /// <param name="date">The date to check.</param>
        private bool ShouldApply(SDate date)
        {
            return this.EnabledSeasons.Contains(date.Season);
        }

        /// <summary>Clear the mod changes in all locations.</summary>
        private void ClearChanges()
        {
            this.SetCropMode(this.EnabledLocations.ToArray(), false, this.EnabledLocations);
            this.EnabledLocations.Clear();
        }

        /// <summary>Set the crop mode for the given locations.</summary>
        /// <param name="locations">The locations to change.</param>
        /// <param name="value">True to enable crops , false to disable it.</param>
        /// <param name="greenhouseified">A list of locations which have been temporarily converted to greenhouses.</param>
        private void SetCropMode(IEnumerable<GameLocation> locations, bool value, HashSet<GameLocation> greenhouseified)
        {
            foreach (GameLocation location in locations)
            {
                if (!location.IsOutdoors || location.IsGreenhouse == value || (!this.Config.FarmAnyLocation && !(location is Farm)))
                    continue;

                // set mode
                this.Monitor.VerboseLog($"Set {location.Name} to {(value ? "greenhouse" : "non-greenhouse")}.");
                location.IsGreenhouse = value;
                foreach (FruitTree tree in location.terrainFeatures.Values.OfType<FruitTree>())
                    tree.GreenHouseTree = value;

                // track changes
                if (value)
                    greenhouseified.Add(location);
                else
                    greenhouseified.Remove(location);
            }
        }

        /// <summary>Add fruit to fruit trees in enabled locations, if they'd normally be out of season. This is a workaround for an issue where fruit trees don't check the <see cref="GameLocation.IsGreenhouse"/> field (see https://stardewvalleywiki.com/User:Pathoschild/Modding_wishlist#Small_changes .)</summary>
        [SuppressMessage("SMAPI", "AvoidNetField", Justification = "The location name can only be changed through the net field.")]
        private void ApplyTreeUpdates()
        {
            foreach (GameLocation location in this.EnabledLocations)
            {
                var trees =
                    (
                        from pair in location.terrainFeatures.Pairs
                        let tree = pair.Value as FruitTree
                        where tree != null
                        select new { Tile = pair.Key, Tree = tree }
                    )
                    .ToArray();

                if (trees.Any())
                {
                    string oldName = location.Name;
                    try
                    {
                        location.name.Value = "Greenhouse";
                        foreach (var pair in trees)
                        {
                            if (pair.Tree.fruitSeason.Value != Game1.currentSeason && pair.Tree.fruitsOnTree.Value < FruitTree.maxFruitsOnTrees)
                                pair.Tree.dayUpdate(location, pair.Tile);
                        }
                    }
                    finally
                    {
                        location.name.Value = oldName;
                    }
                }
            }
        }

        /// <summary>Get whether to override tilling for a given tile.</summary>
        /// <param name="location">The game location to check.</param>
        /// <param name="tilePos">The tile position to check.</param>
        private bool ShouldMakeTillable(GameLocation location, Vector2 tilePos)
        {
            ModConfigForceTillable config = this.Config.ForceTillable;

            // get tile
            Tile tile = location.Map.GetLayer("Back")?.Tiles[(int)tilePos.X, (int)tilePos.Y];
            if (tile?.TileSheet == null || this.GetProperty(tile, "Diggable") != null)
                return false;

            // get config for tile type
            string type = this.GetProperty(tile, "Type") ?? this.GetFallbackTileType(tile.TileSheet.ImageSource, tile.TileIndex);
            return type switch
            {
                "Dirt" => config.Dirt,
                "Grass" => config.Grass,
                "Stone" => config.Stone,
                _ => config.Other
            };
        }

        /// <summary>Get the value of a tile or tile index property.</summary>
        /// <param name="tile">The tile to check.</param>
        /// <param name="name">The property name.</param>
        /// <remarks>Derived from <see cref="GameLocation.doesTileHaveProperty(int, int, string, string)"/> with optimizations.</remarks>
        private string GetProperty(Tile tile, string name)
        {
            PropertyValue property = null;

            if (tile.TileIndexProperties?.TryGetValue(name, out property) == true)
            {
                string value = property?.ToString();
                if (value != null)
                    return value;
            }

            if (tile.Properties?.TryGetValue(name, out property) == true)
            {
                string value = property?.ToString();
                if (value != null)
                    return value;
            }

            return null;
        }

        /// <summary>Get the tile type override for a tile, if any.</summary>
        /// <param name="sheetImageSource">The tilesheet image source.</param>
        /// <param name="backTileId">The back tile ID.</param>
        private string GetFallbackTileType(string sheetImageSource, int backTileId)
        {
            if (sheetImageSource == null || this.FallbackTileTypes == null)
                return null;

            // get unique tilesheet key (e.g. "Maps/spring_outdoorsTileSheet" -> "outdoorsTileSheet")
            string sheetKey = Path.GetFileNameWithoutExtension(sheetImageSource);
            if (sheetKey.StartsWith("spring_") || sheetKey.StartsWith("summer_") || sheetKey.StartsWith("fall_") || sheetKey.StartsWith("winter_"))
                sheetKey = sheetKey.Substring(sheetKey.IndexOf("_", StringComparison.Ordinal) + 1);

            // get override
            string type = null;
            bool found = this.FallbackTileTypes.TryGetValue(sheetKey, out IDictionary<int, string> typeLookup) && typeLookup.TryGetValue(backTileId, out type);
            return found
                ? type
                : null;
        }

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
