using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            this.EnabledSeasons = new HashSet<string>(this.Config.EnableInSeasons.GetEnabledSeasons(), StringComparer.InvariantCultureIgnoreCase);

            // hook events
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.World.LocationListChanged += this.OnLocationListChanged;
            if (this.OverrideTilling)
                helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // allow tilling more tiles
            // (The game has some messy logic for deciding which specific tile to till, but just marking the surrounding tiles diggable is sufficient for our purposes.)
            if (Context.IsWorldReady && e.Button.IsUseToolButton() && Game1.player.CurrentTool is Hoe)
            {
                GameLocation location = Game1.currentLocation;
                foreach (Vector2 tile in Utility.getSurroundingTileLocationsArray(e.Cursor.GrabTile).Concat(new[] { e.Cursor.GrabTile }))
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
        /// <param name="tile">The tile position to check.</param>
        private bool ShouldMakeTillable(GameLocation location, Vector2 tile)
        {
            ModConfigForceTillable config = this.Config.ForceTillable;
            switch (location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Type", "Back"))
            {
                case "Dirt":
                    return config.Dirt;
                case "Grass":
                    return config.Grass;
                case "Stone":
                    return config.Stone;
                default:
                    return config.Other;
            }
        }
    }
}
