using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.SmallBeachFarm.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.SmallBeachFarm
{
    /// <summary>The mod entry class loaded by SMAPI.</summary>
    public class ModEntry : Mod, IAssetLoader
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates logging for the Harmony patch.</summary>
        private static IMonitor StaticMonitor;

        /// <summary>The pixel position at which to place the player after they arrive from Marnie's ranch.</summary>
        private readonly Vector2 MarnieWarpArrivalPixelPos = new Vector2(76, 21) * Game1.tileSize;

        /// <summary>The relative path to the folder containing tilesheet variants.</summary>
        private readonly string TilesheetsPath = Path.Combine("assets", "tilesheets");

        /// <summary>The mod configuration.</summary>
        private ModConfig Config;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();

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
            {
                // load map
                Map map = this.Helper.Content.Load<Map>(this.Config.EnableIslands
                    ? "assets/SmallBeachFarmWithIslands.tbin"
                    : "assets/SmallBeachFarm.tbin"
                );

                // apply tilesheet recolors
                DirectoryInfo compatFolder = this.GetCustomTilesheetFolder();
                if (compatFolder != null)
                {
                    this.Monitor.Log($"Applying map tilesheets from {Path.Combine(this.TilesheetsPath, compatFolder.Name)}.", LogLevel.Trace);
                    foreach (TileSheet tilesheet in map.TileSheets)
                    {
                        string assetFileName = Path.GetFileName(tilesheet.ImageSource);
                        if (File.Exists(Path.Combine(compatFolder.FullName, assetFileName)))
                            tilesheet.ImageSource = this.Helper.Content.GetActualAssetKey(Path.Combine(this.TilesheetsPath, compatFolder.Name, assetFileName));
                    }
                }

                return (T)(object)map;
            }

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
            // move player if they warp into the ocean (e.g. from Marnie's ranch)
            // note: getTileLocation() seems to be unreliable when mounted.
            if (e.IsLocalPlayer && ModEntry.IsSmallBeachFarm(e.NewLocation, out Farm farm))
            {
                Vector2 tile = e.Player.Position / Game1.tileSize;
                if (this.IsInvalidPosition(farm, (int)tile.X, (int)tile.Y))
                    Game1.player.Position = this.MarnieWarpArrivalPixelPos;
            }
        }

        /// <summary>Raised before the game ends the current day.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            if (!ModEntry.IsSmallBeachFarm(Game1.getFarm(), out Farm farm))
                return;

            // update ocean crabpots before the game does
            GameLocation beach = Game1.getLocationFromName("Beach");
            foreach (CrabPot pot in farm.objects.Values.OfType<CrabPot>())
            {
                if (ModEntry.IsOceanTile(farm, (int)pot.TileLocation.X, (int)pot.TileLocation.Y))
                    pot.DayUpdate(beach);
            }
        }

        /// <summary>Get the folder from which to load tilesheets for compatibility with another mod, if applicable.</summary>
        private DirectoryInfo GetCustomTilesheetFolder()
        {
            // get root compatibility folder
            DirectoryInfo compatFolder = new DirectoryInfo(Path.Combine(this.Helper.DirectoryPath, this.TilesheetsPath));
            if (!compatFolder.Exists)
                return null;

            // get first folder matching an installed mod
            foreach (DirectoryInfo folder in compatFolder.GetDirectories())
            {
                if (folder.Name != "_default" && this.Helper.ModRegistry.IsLoaded(folder.Name))
                    return folder;
            }

            return null;
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
            if (!ModEntry.IsSmallBeachFarm(who?.currentLocation, out _))
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
        /// <param name="farm">The farm instance.</param>
        private static bool IsSmallBeachFarm(GameLocation location, out Farm farm)
        {
            if (Game1.whichFarm == Farm.riverlands_layout && location is Farm farmInstance && farmInstance.Name == "Farm")
            {
                farm = farmInstance;
                return true;
            }

            farm = null;
            return false;
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
            return tilesheetId == "zbeach" || tilesheetId == "zbeach_farm";
        }

        /// <summary>Get whether the player shouldn't be able to access a given position.</summary>
        /// <param name="farm">The farm instance to check.</param>
        /// <param name="x">The tile X position.</param>
        /// <param name="y">The tile Y position.</param>
        private bool IsInvalidPosition(Farm farm, int x, int y)
        {
            return
                farm.doesTileHaveProperty(x, y, "Water", "Back") != null
                || (
                    !farm.isTilePassable(new Location(x, y), Game1.viewport)
                    && farm.doesTileHaveProperty(x, y, "Passable", "Buildings") != null
                );
        }
    }
}
