using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Dimensions;
using SObject = StardewValley.Object;

namespace SmallBeachFarm
{
    /// <summary>The mod entry class loaded by SMAPI.</summary>
    public class ModEntry : Mod, IAssetLoader
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates logging for the Harmony patch.</summary>
        private static IMonitor StaticMonitor;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // hook events
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.GameLoop.DayEnding += this.DayEnding;

            // hook Harmony patch
            ModEntry.StaticMonitor = this.Monitor;
            HarmonyInstance harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(Farm), nameof(Farm.getFish), new[] { typeof(float), typeof(int), typeof(int), typeof(Farmer), typeof(double) }),
                prefix: new HarmonyMethod(this.GetType(), nameof(ModEntry.GetFishPrefix))
            );
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Maps/Farm_Fishing");
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Maps/Farm_Fishing"))
                return this.Helper.Content.Load<T>("assets/[CP] SmallBeachFarm/assets/SmallBeachFarm.tbin");

            throw new NotSupportedException($"Unexpected asset '{asset.AssetName}'.");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer && ModEntry.IsSmallBeachFarm(e.NewLocation) && Game1.player.getTileLocation().Y > 29)
                Game1.player.Position = new Vector2(79, 21) * Game1.tileSize;
        }

        /// <summary>Raised before the game ends the current day.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            Farm farm = Game1.getFarm();
            if (!ModEntry.IsSmallBeachFarm(farm))
                return;

            // update ocean crabpots before the game does
            GameLocation beach = Game1.getLocationFromName("Beach");
            foreach (CrabPot pot in farm.objects.Values.OfType<CrabPot>())
            {
                if (ModEntry.IsOceanTile(farm, (int)pot.TileLocation.X, (int)pot.TileLocation.Y))
                    pot.DayUpdate(beach);
            }
        }

        /// <summary>A method called via Harmony before <see cref="Farm.getFish(float, int, int, Farmer, double)"/>, which gets ocean fish from the beach properties if fishing the ocean water.</summary>
        /// <param name="__instance">The farm instance.</param>
        /// <param name="millisecondsAfterNibble">An argument passed through to the underlying method.</param>
        /// <param name="bait">An argument passed through to the underlying method.</param>
        /// <param name="waterDepth">An argument passed through to the underlying method.</param>
        /// <param name="who">An argument passed through to the underlying method.</param>
        /// <param name="baitPotency">An argument passed through to the underlying method.</param>
        /// <param name="__result">The return value to use for the method.</param>
        /// <returns>Returns <c>true</c> if the original logic should run, or <c>false</c> to use <paramref name="__result"/> as the return value.</returns>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
        private static bool GetFishPrefix(Farm __instance, float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, ref SObject __result)
        {
            if (!ModEntry.IsSmallBeachFarm(who?.currentLocation))
                return false;

            // get tile being fished
            FishingRod rod = who.CurrentTool as FishingRod;
            if (rod == null)
                return false;
            Point tile = new Point((int)(rod.bobber.X / Game1.tileSize), (int)(rod.bobber.Y / Game1.tileSize));

            // get ocean fish
            if (ModEntry.IsOceanTile(__instance, tile.X, tile.Y))
            {
                __result = __instance.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, "Beach");
                ModEntry.StaticMonitor.VerboseLog($"Fishing ocean tile at ({rod.bobber.X / Game1.tileSize}, {rod.bobber.Y / Game1.tileSize}).");
                return false;
            }

            // get default riverlands fish
            ModEntry.StaticMonitor.VerboseLog($"Fishing river tile at ({rod.bobber.X / Game1.tileSize}, {rod.bobber.Y / Game1.tileSize}).");
            return true;
        }

        /// <summary>Get whether the given location is the Small Beach Farm.</summary>
        /// <param name="location">The location to check.</param>
        private static bool IsSmallBeachFarm(GameLocation location)
        {
            return Game1.whichFarm == Farm.riverlands_layout && location?.Name == "Farm";
        }

        /// <summary>Get whether a given position is ocean water.</summary>
        /// <param name="farm">The farm instance.</param>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">The tile Y position.</param>
        private static bool IsOceanTile(Farm farm, int x, int y)
        {
            // check water property
            if (farm.doesTileHaveProperty(x, y, "Water", "Back") == null)
                return false;

            // check for beach tilesheet
            string tilesheetId = farm.map
                ?.GetLayer("Back")
                ?.PickTile(new Location(x * Game1.tileSize, y * Game1.tileSize), Game1.viewport.Size)
                ?.TileSheet
                ?.Id;
            return tilesheetId == "zbeach" || tilesheetId == "zbeachplus";
        }
    }
}
