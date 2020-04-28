using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>Scans the game world for owned items.</summary>
    public class WorldItemScanner
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get all items owned by the player.</summary>
        /// <remarks>
        /// Derived from <see cref="Utility.iterateAllItems"/> with some differences:
        ///   * removed items held by other players, items floating on the ground, spawned forage, and output in a non-ready machine (except casks which can be emptied anytime);
        ///   * added hay in silos.
        /// </remarks>
        public IEnumerable<Item> GetAllOwnedItems()
        {
            List<Item> items = new List<Item>();

            // in locations
            foreach (GameLocation location in CommonHelper.GetLocations())
            {
                // furniture
                if (location is DecoratableLocation decorableLocation)
                {
                    foreach (Furniture furniture in decorableLocation.furniture)
                    {
                        items.Add(furniture);

                        if (furniture is StorageFurniture dresser)
                            items.AddRange(dresser.heldItems);
                        else
                            items.Add(furniture.heldObject.Value);
                    }
                }

                // farmhouse fridge
                if (location is FarmHouse house)
                    items.AddRange(house.fridge.Value.items);

                // character hats
                foreach (NPC npc in location.characters)
                {
                    items.Add(
                        (npc as Child)?.hat.Value
                        ?? (npc as Horse)?.hat.Value
                    );
                }

                // building output
                if (location is BuildableGameLocation buildableLocation)
                {
                    foreach (var building in buildableLocation.buildings)
                    {
                        if (building is Mill mill)
                            items.AddRange(mill.output.Value.items);
                        else if (building is JunimoHut hut)
                            items.AddRange(hut.output.Value.items);
                    }
                }

                // map objects
                foreach (SObject item in location.objects.Values)
                {
                    // chest
                    if (item is Chest chest)
                    {
                        if (chest.playerChest.Value)
                        {
                            items.Add(chest);
                            items.AddRange(chest.items);
                        }
                    }

                    // auto-grabber
                    else if (item.ParentSheetIndex == 165 && item.heldObject.Value is Chest grabberChest)
                    {
                        items.Add(item);
                        items.AddRange(grabberChest.items);
                    }

                    // craftable
                    else if (item.bigCraftable.Value)
                    {
                        items.Add(item);
                        if (item.MinutesUntilReady == 0 || item is Cask) // cask output can be retrieved anytime
                            items.Add(item.heldObject.Value);
                    }

                    // anything else
                    else if (!this.IsSpawnedWorldItem(item))
                    {
                        items.Add(item);
                        items.Add(item.heldObject.Value);
                    }
                }
            }

            // inventory
            items.AddRange(Game1.player.Items);
            items.AddRange(new Item[]
            {
                Game1.player.shirtItem.Value,
                Game1.player.pantsItem.Value,
                Game1.player.boots.Value,
                Game1.player.hat.Value,
                Game1.player.leftRing.Value,
                Game1.player.rightRing.Value
            });

            // hay in silos
            int hayCount = Game1.getFarm()?.piecesOfHay.Value ?? 0;
            while (hayCount > 0)
            {
                SObject hay = new SObject(178, 1);
                hay.Stack = Math.Min(hayCount, hay.maximumStackSize());
                hayCount -= hay.Stack;
                items.Add(hay);
            }

            return items.Where(p => p != null);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether an item was spawned automatically. This is heuristic and only applies for items placed in the world, not items in an inventory.</summary>
        /// <param name="item">The item to check.</param>
        /// <remarks>Derived from the <see cref="SObject"/> constructors.</remarks>
        private bool IsSpawnedWorldItem(Item item)
        {
            return
                item is SObject obj
                && (
                    obj.IsSpawnedObject
                    || obj.isForage(null) // location argument is only used to check if it's on the beach, in which case everything is forage
                    || (!(obj is Chest) && (obj.Name == "Weeds" || obj.Name == "Stone" || obj.Name == "Twig"))
                );
        }
    }
}
