using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Utilities;
using Pathoschild.Stardew.CropsInAnySeason.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Pathoschild.Stardew.CropsInAnySeason
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod, IAssetEditor
    {
        /*********
        ** Properties
        *********/
        /// <summary>The locations which have been greenhouseified.</summary>
        private readonly HashSet<GameLocation> ChangedLocations = new HashSet<GameLocation>(new ObjectReferenceComparer<GameLocation>());

        /// <summary>The seasons for which to override crops.</summary>
        private HashSet<string> EnabledSeasons;

        /// <summary>Whether crops should be overridden for the current day.</summary>
        private bool ShouldApply;

        /// <summary>The asset name for the crop data.</summary>
        private readonly string CropAssetName = "Data/Crops";

        /// <summary>The asset name for the winter dirt texture.</summary>
        private readonly string WinterDirtAssetName = "TerrainFeatures/hoeDirtSnow";


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            var config = helper.ReadConfig<ModConfig>();
            this.EnabledSeasons = new HashSet<string>(config.EnableInSeasons.GetEnabledSeasons(), StringComparer.InvariantCultureIgnoreCase);

            // hook events
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.World.LocationListChanged += this.OnLocationListChanged;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            helper.Events.GameLoop.Saving += this.OnSaving;

            // init
            this.Reset();
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return
                this.ShouldApply
                && (
                    asset.AssetNameEquals(this.CropAssetName)
                    || asset.AssetNameEquals(this.WinterDirtAssetName)
                );
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            // change crop seasons
            if (asset.AssetNameEquals(this.CropAssetName))
            {
                asset
                    .AsDictionary<int, string>()
                    .Set((id, data) =>
                    {
                        string[] fields = data.Split('/');
                        fields[1] = string.Join(" ", this.EnabledSeasons);
                        return string.Join("/", fields);
                    });
            }

            // change dirt texture
            else if (asset.AssetNameEquals(this.WinterDirtAssetName))
                asset.ReplaceWith(this.Helper.Content.Load<Texture2D>("TerrainFeatures/hoeDirt", ContentSource.GameContent));
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
            // apply changes
            this.UpdateContext(SDate.Now());
        }

        /// <summary>The method called after a location is added or removed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLocationListChanged(object sender, LocationListChangedEventArgs e)
        {
            // handle locations added after day start
            this.ApplyChanges(e.Added);
        }

        /// <summary>The method called before the day ends, before the game sets up the next day.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            // remove changes if needed (to let crops die if the next season is disabled)
            this.UpdateContext(SDate.Now().AddDays(1));
        }

        /// <summary>The method called before the day ends, before the game sets up the next day.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            // remove changes before save (to avoid making changes permanent if the mod is uninstalled)
            this.ClearChanges();
        }

        /// <summary>The method called after the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.Reset();
        }

        /****
        ** Methods
        ****/
        /// <summary>Reset state when starting a new session.</summary>
        private void Reset()
        {
            this.ShouldApply = true;
            this.ChangedLocations.Clear();
        }

        /// <summary>Update changes for the given date.</summary>
        /// <param name="date">The date for which to update the context.</param>
        private void UpdateContext(SDate date)
        {
            // check context
            bool wasApplied = this.ShouldApply;
            this.ShouldApply = this.EnabledSeasons.Contains(date.Season);

            // reset asset changes
            if (this.ShouldApply != wasApplied)
            {
                this.Monitor.Log($"{(this.ShouldApply ? "Enabling" : "Disabling")} for {date.Season}.", LogLevel.Trace);
                this.Helper.Content.InvalidateCache(this.CropAssetName);
                this.Helper.Content.InvalidateCache(this.WinterDirtAssetName);
            }
        }

        /// <summary>Apply changes to the given locations and track changes for later resetting.</summary>
        /// <param name="locations">The locations to handle.</param>
        private void ApplyChanges(IEnumerable<GameLocation> locations)
        {
            foreach (GameLocation location in locations)
            {
                if (this.ChangedLocations.Contains(location) || !location.IsOutdoors || location.IsGreenhouse)
                    continue;

                location.IsGreenhouse = true;
                this.ChangedLocations.Add(location);
            }
        }

        /// <summary>Reset changes to all locations.</summary>
        private void ClearChanges()
        {
            foreach (GameLocation location in this.ChangedLocations)
                location.IsGreenhouse = false;
            this.ChangedLocations.Clear();
        }
    }
}
