using System.Collections.Generic;
using System.Linq;
using ChestsAnywhere.Framework;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ChestsAnywhere
{
    /// <summary>Encapsulates logic for finding chests.</summary>
    internal static class ChestFactory
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get all player chests.</summary>
        public static IEnumerable<ManagedChest> GetChests()
        {
            foreach (GameLocation location in Game1.locations)
            {
                // chests in location
                {
                    int namelessCount = 0;
                    foreach (Chest chest in location.Objects.Values.OfType<Chest>().Where(p => p.playerChest))
                        yield return new ManagedChest(chest, ChestFactory.GetLocationName(location), $"Chest #{++namelessCount}");
                }

                // chests in constructed buildings
                if (location is BuildableGameLocation)
                {
                    foreach (Building building in (location as BuildableGameLocation).buildings)
                    {
                        int namelessCount = 0;
                        if (building.indoors == null)
                            continue;
                        foreach (Chest chest in building.indoors.Objects.Values.OfType<Chest>())
                            yield return new ManagedChest(chest, ChestFactory.GetLocationName(building), $"Chest #{++namelessCount}");
                    }
                }

                // farmhouse containers
                if (location is FarmHouse && Game1.player.HouseUpgradeLevel > 0)
                {
                    Chest fridge = (location as FarmHouse).fridge;
                    if (fridge != null)
                        yield return new ManagedChest(fridge, location.Name, "Fridge");
                }
            }
        }

        /// <summary>Get all player chests in the order they should be displayed.</summary>
        /// <param name="selectedChest">The chest to show even if it's ignored.</param>
        /// <param name="excludeIgnored">Whether to exclude chests marked as hidden.</param>
        public static IEnumerable<ManagedChest> GetChestsForDisplay(Chest selectedChest = null, bool excludeIgnored = true)
        {
            return ChestFactory.GetChests()
                .Where(chest => !excludeIgnored || !chest.IsIgnored || chest.Chest == selectedChest)
                .OrderBy(p => p.Order ?? int.MaxValue)
                .ThenBy(p => p.Name);
        }

        /// <summary>Get the player chest on the specified tile (if any).</summary>
        /// <param name="tile">The tile to check.</param>
        public static ManagedChest GetChestFromTile(Vector2 tile)
        {
            // get chest
            Chest chest;
            {
                Object obj;
                Game1.currentLocation.Objects.TryGetValue(tile, out obj);
                chest = obj as Chest;
            }

            // return if valid
            if (chest != null && chest.playerChest)
                return new ManagedChest(chest, ChestFactory.GetLocationName(Game1.currentLocation));
            return null;
        }

        /// <summary>Get the player chest from the specified menu (if any).</summary>
        /// <param name="menu">The menu to check.</param>
        public static ManagedChest GetChestFromMenu(ItemGrabMenu menu)
        {
            // from menu target
            Chest target = menu.behaviorOnItemGrab?.Target as Chest;
            ManagedChest chest = target != null
                ? ChestFactory.GetChests().FirstOrDefault(p => p.Chest == target)
                : null;
            if (chest != null)
                return chest;

            // fallback to open chest
            return ChestFactory.GetChests().FirstOrDefault(p => p.Chest.currentLidFrame == 135);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the display name for a location.</summary>
        /// <param name="location">The game location.</param>
        private static string GetLocationName(GameLocation location)
        {
            return location.Name;
        }

        /// <summary>Get the display name for a location.</summary>
        /// <param name="location">The game location.</param>
        private static string GetLocationName(Building location)
        {
            return location.nameOfIndoors.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        }
    }
}
