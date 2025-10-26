using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework;

/// <summary>Determines whether given locations are in range of the player for remote chest access.</summary>
internal class RangeHandler
{
    /*********
    ** Fields
    *********/
    /// <summary>The range within which chests should be accessible.</summary>
    private readonly ChestRange Range;

    /// <summary>A location => zone lookup if <see cref="Range"/> is <see cref="ChestRange.CurrentWorldArea"/>.</summary>
    private readonly Lazy<Dictionary<GameLocation, string>> WorldAreaZones;


    /*********
    ** Accessors
    *********/
    /// <summary>A range handler which disables remote access.</summary>
    public static readonly RangeHandler None = new(ChestRange.None, null);

    /// <summary>A range handler which restricts access to the current location.</summary>
    public static readonly RangeHandler CurrentLocation = new(ChestRange.CurrentLocation, null);

    /// <summary>A range handler which doesn't restrict the range.</summary>
    public static readonly RangeHandler Unlimited = new(ChestRange.Unlimited, null);


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="range">The range within which chests should be accessible.</param>
    /// <param name="worldAreas">The predefined world areas for <see cref="ChestRange.CurrentWorldArea"/>.</param>
    public RangeHandler(ChestRange range, IDictionary<string, HashSet<string>>? worldAreas)
    {
        this.Range = range;
        this.WorldAreaZones = new(() => this.GetWorldAreaZones(worldAreas));
    }

    /// <summary>Get the locations which match this range.</summary>
    /// <param name="multiplayerApi">The multiplayer API from which to get active locations, if applicable.</param>
    public IEnumerable<GameLocation> GetLocationsInRange(IMultiplayerHelper multiplayerApi)
    {
        switch (this.Range)
        {
            case ChestRange.None:
                return [];

            case ChestRange.CurrentLocation:
                {
                    GameLocation currentLocation = Game1.currentLocation;
                    return currentLocation != null
                        ? [currentLocation]
                        : [];
                }

            case ChestRange.CurrentWorldArea:
                {
                    string currentZone = this.GetZone(Game1.currentLocation);
                    return this
                        .GetAllLocations(multiplayerApi)
                        .Where(location => this.GetZone(location) == currentZone);
                }

            case ChestRange.Unlimited:
                return this.GetAllLocations(multiplayerApi);

            default:
                throw new NotSupportedException($"Unknown range '{this.Range}'.");
        }
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get all available locations (not only those in range).</summary>
    /// <param name="multiplayerApi">The multiplayer API from which to get active locations, if applicable.</param>
    private IEnumerable<GameLocation> GetAllLocations(IMultiplayerHelper multiplayerApi)
    {
        return Context.IsMainPlayer
            ? CommonHelper.GetLocations()
            : multiplayerApi.GetActiveLocations();
    }

    /// <summary>Get the zone key for a location.</summary>
    /// <param name="location">The location to check.</param>
    private string GetZone(GameLocation location)
    {
        return location switch
        {
            MineShaft mine => mine.mineLevel <= 120 ? "Mine" : "SkullCave",
            VolcanoDungeon => "VolcanoDungeon",
            _ => this.WorldAreaZones.Value.TryGetValue(location, out string? zone)
                ? zone
                : location.Name
        };
    }

    /// <summary>Get a lookup which matches locations to world area zones.</summary>
    /// <param name="worldAreas">The predefined world areas for <see cref="ChestRange.CurrentWorldArea"/>.</param>
    private Dictionary<GameLocation, string> GetWorldAreaZones(IDictionary<string, HashSet<string>>? worldAreas)
    {
        Dictionary<GameLocation, string> zones = [];

        foreach (GameLocation location in Game1.locations)
        {
            // get zone key
            string? explicitZone = (from area in worldAreas where area.Value.Contains(location.Name) select area.Key).FirstOrDefault();
            string zone = explicitZone ?? location.Name;

            // map locations to zone (unless already mapped through a parent location)
            if (!zones.ContainsKey(location) || explicitZone != null)
            {
                zones[location] = zone;
                foreach (GameLocation child in this.GetNestedLocations(location))
                    zones[child] = zone;
            }
        }

        return zones;
    }

    /// <summary>Get all locations nested within the given location.</summary>
    /// <param name="location">The root location to search.</param>
    private IEnumerable<GameLocation> GetNestedLocations(GameLocation location)
    {
        // building interiors
        foreach (GameLocation interior in location.buildings.Select(p => p.GetIndoors()).Where(p => p != null))
        {
            yield return interior;
            foreach (GameLocation child in this.GetNestedLocations(interior))
                yield return child;
        }

        // farmhouse/cabin
        if (location is FarmHouse house)
        {
            GameLocation cellar = Game1.getLocationFromName(house.GetCellarName());
            if (cellar != null)
            {
                yield return cellar;
                foreach (GameLocation child in this.GetNestedLocations(cellar))
                    yield return child;
            }
        }
    }
}
