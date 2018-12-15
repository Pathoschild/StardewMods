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

namespace Pathoschild.Stardew.CropsAnytimeAnywhere
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

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
            this.EnabledSeasons = new HashSet<string>(this.Config.Seasons.GetEnabledSeasons(), StringComparer.InvariantCultureIgnoreCase);

            // hook events
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.World.LocationListChanged += this.OnLocationListChanged;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            helper.Events.GameLoop.Saving += this.OnSaving;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
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
                if (!location.IsOutdoors || location.IsGreenhouse == value || (!this.Config.AllowCropsAnywhere && !(location is Farm)))
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
    }
}
