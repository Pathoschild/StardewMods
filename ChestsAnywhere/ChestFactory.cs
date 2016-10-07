using System.Collections.Generic;
using System.Linq;
using ChestsAnywhere.Framework;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
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
                    foreach (Chest chest in location.Objects.Values.OfType<Chest>())
                        yield return new ManagedChest(chest, location.Name, $"Chest #{++namelessCount}");
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
                            yield return new ManagedChest(chest, building.nameOfIndoors.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9'), $"Chest #{++namelessCount}");
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
                return new ManagedChest(chest, Game1.currentLocation.Name, "Chest");
            return null;
        }
    }
}
