using System;
using System.IO;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.SmallBeachFarm.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Pathoschild.Stardew.SmallBeachFarm
{
    /// <summary>The mod entry class loaded by SMAPI.</summary>
    public class ModEntry : Mod, IAssetLoader
    {
        /*********
        ** Fields
        *********/
        /// <summary>The pixel position at which to place the player after they arrive from Marnie's ranch.</summary>
        private readonly Vector2 MarnieWarpArrivalPixelPos = new Vector2(76, 21) * Game1.tileSize;

        /// <summary>The relative path to the folder containing tilesheet variants.</summary>
        private readonly string TilesheetsPath = Path.Combine("assets", "tilesheets");

        /// <summary>The relative path to the folder containing tilesheet overlays.</summary>
        private readonly string OverlaysPath = Path.Combine("assets", "overlays");

        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The mod's hardcoded data.</summary>
        private ModData Data;

        /// <summary>The minimum value to consider non-transparent.</summary>
        /// <remarks>On Linux/Mac, fully transparent pixels may have an alpha up to 4 for some reason.</remarks>
        private const byte MinOpacity = 5;

        /// <summary>A fake asset key prefix from which to load tilesheets.</summary>
        private string FakeAssetPrefix => Path.Combine("Mods", this.ModManifest.UniqueID);


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();
            this.Data = helper.Data.ReadJsonFile<ModData>("assets/data.json") ?? new ModData();

            // hook events
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.GameLoop.DayEnding += this.DayEnding;

            // hook Harmony patch
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            FarmPatcher.Hook(
                harmony,
                this.Monitor,
                addCampfire: this.Config.AddCampfire,
                useBeachMusic: this.Config.UseBeachMusic,
                isSmallBeachFarm: location => this.IsSmallBeachFarm(location, out _),
                getFishType: this.GetFishType
            );
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return
                asset.AssetNameEquals("Maps/Farm_Fishing")
                || asset.AssetName.StartsWith(this.FakeAssetPrefix);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            // load map
            if (asset.AssetNameEquals("Maps/Farm_Fishing"))
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
                string internalRootKey = this.Helper.Content.GetActualAssetKey(Path.Combine(this.TilesheetsPath, "_default"));
                foreach (TileSheet tilesheet in map.TileSheets)
                {
                    if (tilesheet.ImageSource.StartsWith(internalRootKey + Path.DirectorySeparatorChar))
                        tilesheet.ImageSource = this.Helper.Content.GetActualAssetKey(Path.Combine(this.FakeAssetPrefix, Path.GetFileNameWithoutExtension(tilesheet.ImageSource)), ContentSource.GameContent);
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
                var tilesheetPixels = new Lazy<Color[]>(() => this.GetPixels(tilesheet));

                // apply overlays
                foreach (DirectoryInfo folder in new DirectoryInfo(this.GetFullPath(this.OverlaysPath)).EnumerateDirectories())
                {
                    if (!this.Helper.ModRegistry.IsLoaded(folder.Name))
                        continue;

                    // get overlay
                    Texture2D overlay = this.Helper.Content.Load<Texture2D>(Path.Combine(this.OverlaysPath, folder.Name, filename));
                    Color[] overlayPixels = this.GetPixels(overlay);

                    // apply
                    Color[] target = tilesheetPixels.Value;
                    for (int i = 0; i < overlayPixels.Length; i++)
                    {
                        Color pixel = overlayPixels[i];
                        if (pixel.A >= ModEntry.MinOpacity)
                            target[i] = overlayPixels[i];
                    }
                }

                if (tilesheetPixels.IsValueCreated)
                    tilesheet.SetData(tilesheetPixels.Value);

                return (T)(object)tilesheet;
            }

            // unknown asset
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
            if (e.IsLocalPlayer && this.IsSmallBeachFarm(e.NewLocation, out Farm farm))
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
            if (!this.IsSmallBeachFarm(Game1.getFarm(), out Farm farm))
                return;

            // update ocean crabpots before the game does
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

        /// <summary>Get the pixel data for a texture.</summary>
        /// <param name="texture">The texture asset.</param>
        private Color[] GetPixels(Texture2D texture)
        {
            Color[] pixels = new Color[texture.Width * texture.Height];
            texture.GetData(pixels);
            return pixels;
        }

        /// <summary>Get whether the given location is the Small Beach Farm.</summary>
        /// <param name="location">The location to check.</param>
        /// <param name="farm">The farm instance.</param>
        private bool IsSmallBeachFarm(GameLocation location, out Farm farm)
        {
            if (Game1.whichFarm == Farm.riverlands_layout && location is Farm farmInstance && farmInstance.Name == "Farm")
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
                return FishType.River;

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
            return tilesheetId == "zbeach" || tilesheetId == "zbeach_farm"
                ? FishType.Ocean
                : FishType.River;
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
