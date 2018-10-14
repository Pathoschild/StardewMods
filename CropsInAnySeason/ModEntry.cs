using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Utilities;
using Pathoschild.Stardew.CropsInAnySeason.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;

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

        /// <summary>The order that seasons should be listed in crop data.</summary>
        private readonly string[] SeasonOrder = { "spring", "summer", "fall", "winter" };

        /// <summary>The crop seasons from the game's crop data indexed by harvest IDs.</summary>
        /// <remarks>Crops don't store their crop ID, so we can only match them by the harvest ID they produce. This always works for vanilla crops, since different crops never produce the same harvest ID. It's more heuristic for modded crops, which might have duplicate harvest IDs; in that case we just union their seasons to be safe. This lets us revert crops to their normal seasons if the current season is disabled in the mod configuration.</remarks>
        private Lazy<IDictionary<int, string[]>> SeasonsByHarvestID;

        /// <summary>Whether the mod has reset crops for the current session.</summary>
        private bool IsInitialised;

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
            // remove changes before next day is calculated if tomorrow is out of season
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
            this.IsInitialised = false;
            this.ShouldApply = true;
            this.ChangedLocations.Clear();
            this.SeasonsByHarvestID = new Lazy<IDictionary<int, string[]>>(this.GetSeasonsByHarvestID);
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

            // reset crop changes
            if (!this.IsInitialised || this.ShouldApply != wasApplied)
            {
                this.SeasonsByHarvestID = new Lazy<IDictionary<int, string[]>>(this.GetSeasonsByHarvestID);
                this.IsInitialised = true;
                if (this.ShouldApply)
                    this.ApplyChanges(Game1.locations);
                else
                    this.ClearChanges();
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
                this.UpdateCrops(new[] { location });
                this.ChangedLocations.Add(location);
            }
        }

        /// <summary>Reset changes to all locations.</summary>
        private void ClearChanges()
        {
            foreach (GameLocation location in this.ChangedLocations)
                location.IsGreenhouse = false;
            this.UpdateCrops(this.ChangedLocations);
            this.ChangedLocations.Clear();
        }

        /// <summary>Update all crops to match their seasons in the crop data.</summary>
        /// <param name="locations">The game locations in which to update crops.</param>
        private void UpdateCrops(IEnumerable<GameLocation> locations)
        {
            foreach (GameLocation location in locations)
            {
                // skip indoor locations (which the mod never changes)
                if (!location.IsOutdoors)
                    continue;

                // update each crop
                foreach (HoeDirt dirt in location.terrainFeatures.Values.OfType<HoeDirt>())
                {
                    Crop crop = dirt.crop;
                    if (crop != null && this.SeasonsByHarvestID.Value.TryGetValue(crop.indexOfHarvest.Value, out string[] seasons) && crop.seasonsToGrowIn.Count != seasons.Length)
                    {
                        crop.seasonsToGrowIn.Clear();
                        crop.seasonsToGrowIn.AddRange(seasons);
                    }
                }
            }
        }

        /// <summary>Get the crop seasons from the game's crop data indexed by harvest IDs.</summary>
        /// <remarks>See remarks on <see cref="SeasonsByHarvestID"/>.</remarks>
        private IDictionary<int, string[]> GetSeasonsByHarvestID()
        {
            IDictionary<int, string> cropData = this.Helper.Content.Load<Dictionary<int, string>>(this.CropAssetName, ContentSource.GameContent);
            IDictionary<int, string[]> seasonsByHarvestID = new Dictionary<int, string[]>();
            foreach (KeyValuePair<int, string> data in cropData)
            {
                // parse data
                string[] fields = data.Value.Split('/');
                int harvestID = int.Parse(fields[3]);
                string[] rawSeasons = fields[1].Split(' ');

                // add seasons
                if (seasonsByHarvestID.TryGetValue(harvestID, out string[] cropSeasons))
                {
                    seasonsByHarvestID[harvestID] = cropSeasons
                        .Union(rawSeasons)
                        .OrderBy(season => Array.IndexOf(this.SeasonOrder, season))
                        .ToArray();
                }
                else
                    seasonsByHarvestID[harvestID] = rawSeasons;
            }

            return seasonsByHarvestID;
        }
    }
}
