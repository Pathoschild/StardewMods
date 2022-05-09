using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using HarmonyLib;
using Pathoschild.Stardew.Common.Patching;
using Pathoschild.Stardew.CropsAnytimeAnywhere.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.ObjectModel;
using xTile.Tiles;

namespace Pathoschild.Stardew.CropsAnytimeAnywhere.Patches
{
    /// <summary>Encapsulates Harmony patches for the <see cref="Farm"/> instance.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is defined by Harmony.")]
    internal class LocationPatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates logging for the Harmony patch.</summary>
        private static IMonitor Monitor = null!; // set by first constructor

        /// <summary>The mod configuration.</summary>
        private static LocationConfigManager Config = null!; // set by first constructor

        /// <summary>The tile types to use for tiles which don't have a type property and aren't marked diggable. Indexed by tilesheet image source (without path or season) and back tile ID.</summary>
        private static Dictionary<string, Dictionary<int, string>> FallbackTileTypes = null!; // set by first constructor

        /// <summary>Whether the patcher has already logged a tile error since the game launched.</summary>
        private static bool LoggedTileError;


        /*********
        ** Public methods
        *********/
        /// <summary>Initialize the Harmony patches.</summary>
        /// <param name="monitor">Encapsulates logging for the Harmony patch.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="fallbackTileTypes">The tile types to use for tiles which don't have a type property and aren't marked diggable. Indexed by tilesheet image source (without path or season) and back tile ID.</param>
        public LocationPatcher(IMonitor monitor, LocationConfigManager config, Dictionary<string, Dictionary<int, string>> fallbackTileTypes)
        {
            LocationPatcher.Monitor = monitor;
            LocationPatcher.Config = config;
            LocationPatcher.FallbackTileTypes = fallbackTileTypes;
        }

        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.CanPlantSeedsHere)),
                postfix: this.GetHarmonyMethod(nameof(LocationPatcher.After_CanPlantSeedsOrTreesHere))
            );

            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.CanPlantTreesHere)),
                postfix: this.GetHarmonyMethod(nameof(LocationPatcher.After_CanPlantSeedsOrTreesHere))
            );
            harmony.Patch(
                original: this.RequireMethod<Town>(nameof(Town.CanPlantTreesHere)), // need to override town separately since it doesn't check base
                postfix: this.GetHarmonyMethod(nameof(LocationPatcher.After_CanPlantSeedsOrTreesHere))
            );

            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.SeedsIgnoreSeasonsHere)),
                postfix: this.GetHarmonyMethod(nameof(LocationPatcher.After_SeedsIgnoreSeasonsHere))
            );

            if (LocationPatcher.Config.HasTillableOverrides())
            {
                harmony.Patch(
                    original: this.RequireMethod<GameLocation>(nameof(GameLocation.doesTileHaveProperty)),
                    postfix: this.GetHarmonyMethod(nameof(LocationPatcher.After_DoesTileHaveProperty))
                );
            }
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Patches
        ****/
        /// <summary>A method called via Harmony after <see cref="GameLocation.CanPlantSeedsHere"/> or <see cref="GameLocation.CanPlantTreesHere"/>.</summary>
        /// <param name="__instance">The farm instance.</param>
        /// <param name="__result">The return value to use for the method.</param>
        private static void After_CanPlantSeedsOrTreesHere(GameLocation __instance, ref bool __result)
        {
            if (LocationPatcher.Config.TryGetForLocation(__instance, out PerLocationConfig? config) && config.GrowCrops)
                __result = true;
        }

        /// <summary>A method called via Harmony after <see cref="GameLocation.SeedsIgnoreSeasonsHere"/>.</summary>
        /// <param name="__instance">The farm instance.</param>
        /// <param name="__result">The return value to use for the method.</param>
        private static void After_SeedsIgnoreSeasonsHere(GameLocation __instance, ref bool __result)
        {
            if (LocationPatcher.Config.TryGetForLocation(__instance, out PerLocationConfig? config) && config.GrowCrops && config.GrowCropsOutOfSeason)
            {
                if (!LocationPatcher.CallStackIncludes(typeof(GameLocation), nameof(GameLocation.DayUpdate))) // game skips tilled dirt decay on DayUpdate if we return true here
                    __result = true;
            }
        }

        /// <summary>A method called via Harmony after <see cref="GameLocation.doesTileHaveProperty"/>.</summary>
        /// <param name="__instance">The farm instance.</param>
        /// <param name="xTile">The x-coordinate of the map tile.</param>
        /// <param name="yTile">The y-coordinate of the map tile.</param>
        /// <param name="propertyName">The property name to match.</param>
        /// <param name="layerName">The map layer name to check.</param>
        /// <param name="__result">The return value to use for the method.</param>
        private static void After_DoesTileHaveProperty(GameLocation __instance, int xTile, int yTile, string propertyName, string layerName, ref string __result)
        {
            if (!Context.IsWorldReady || !__instance.farmers.Any())
                return; // don't affect game logic for spawning ores, etc

            if (propertyName == "Diggable" && layerName == "Back")
            {
                try
                {
                    if (LocationPatcher.ShouldMakeTillable(__instance, xTile, yTile))
                        __result = "T";
                }
                catch (Exception ex)
                {
                    if (!LocationPatcher.LoggedTileError)
                    {
                        LocationPatcher.LoggedTileError = true;
                        LocationPatcher.Monitor.Log($"Failed overriding {nameof(GameLocation)}.{nameof(GameLocation.doesTileHaveProperty)} for {__instance.Name} ({xTile}, {yTile}): {ex}", LogLevel.Error);
                    }
                }
            }
        }


        /****
        ** Methods
        ****/
        /// <summary>Get whether to override tilling for a given tile.</summary>
        /// <param name="location">The game location to check.</param>
        /// <param name="xTile">The x-coordinate of the map tile.</param>
        /// <param name="yTile">The y-coordinate of the map tile.</param>
        private static bool ShouldMakeTillable(GameLocation location, int xTile, int yTile)
        {
            // get tile config
            var config = LocationPatcher.Config.TryGetForLocation(location, out PerLocationConfig? locationConfig)
                ? locationConfig.ForceTillable
                : null;
            if (config?.IsAnyEnabled() != true)
                return false;

            // get tile
            Tile? tile = location.Map.GetLayer("Back")?.Tiles[xTile, yTile];
            if (tile?.TileSheet == null || LocationPatcher.GetProperty(tile, "Diggable") != null)
                return false;

            // get config for tile type
            string? type = LocationPatcher.GetProperty(tile, "Type") ?? LocationPatcher.GetFallbackTileType(tile.TileSheet.ImageSource, tile.TileIndex);
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
        private static string? GetProperty(Tile tile, string name)
        {
            if (tile.TileIndexProperties?.TryGetValue(name, out PropertyValue? property) == true)
            {
                string? value = property?.ToString();
                if (value != null)
                    return value;
            }

            if (tile.Properties?.TryGetValue(name, out property) == true)
            {
                string? value = property?.ToString();
                if (value != null)
                    return value;
            }

            return null;
        }

        /// <summary>Get the tile type override for a tile, if any.</summary>
        /// <param name="sheetImageSource">The tilesheet image source.</param>
        /// <param name="backTileId">The back tile ID.</param>
        private static string? GetFallbackTileType(string? sheetImageSource, int backTileId)
        {
            if (sheetImageSource == null || !LocationPatcher.FallbackTileTypes.Any())
                return null;

            // get unique tilesheet key (e.g. "Maps/spring_outdoorsTileSheet" -> "outdoorsTileSheet")
            string sheetKey = Path.GetFileNameWithoutExtension(sheetImageSource);
            if (sheetKey.StartsWith("spring_") || sheetKey.StartsWith("summer_") || sheetKey.StartsWith("fall_") || sheetKey.StartsWith("winter_"))
                sheetKey = sheetKey.Substring(sheetKey.IndexOf("_", StringComparison.Ordinal) + 1);

            // get override
            string? type = null;
            bool found = LocationPatcher.FallbackTileTypes.TryGetValue(sheetKey, out Dictionary<int, string>? typeLookup) && typeLookup.TryGetValue(backTileId, out type);
            return found
                ? type
                : null;
        }

        /// <summary>Get whether the given method appears in the call stack.</summary>
        /// <param name="type">The type which declared the method.</param>
        /// <param name="name">The method name.</param>
        private static bool CallStackIncludes(Type type, string name)
        {
            return new StackTrace()
                .GetFrames()
                .Any(frame =>
                    frame.GetMethod() is { } method
                    && method.DeclaringType == type
                    && method.Name == name
                );
        }
    }
}
