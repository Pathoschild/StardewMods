using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.ItemScanning
{
    /// <summary>Scans the game world for owned items.</summary>
    public class WorldItemScanner
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to protected code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to protected code.</param>
        public WorldItemScanner(IReflectionHelper reflection)
        {
            this.Reflection = reflection;
        }

        /// <summary>Get all items owned by the player.</summary>
        /// <remarks>
        /// This is derived from <see cref="Utility.iterateAllItems"/> with some improvements:
        ///   * removed items held by other players, items floating on the ground, spawned forage, and output in a non-ready machine (except casks which can be emptied anytime);
        ///   * added hay in silos;
        ///   * added tool attachments;
        ///   * added recursive scanning (e.g. inside held chests) to support mods like Item Bag.
        /// </remarks>
        public IEnumerable<FoundItem> GetAllOwnedItems()
        {
            List<FoundItem> items = new List<FoundItem>();

            // in locations
            foreach (GameLocation location in CommonHelper.GetLocations())
            {
                // furniture
                if (location is DecoratableLocation decorableLocation)
                {
                    foreach (Furniture furniture in decorableLocation.furniture)
                        this.ScanAndTrack(items, furniture, isRootInWorld: true);
                }

                // farmhouse fridge
                if (location is FarmHouse house)
                    this.ScanAndTrack(items, house.fridge.Value.items);

                // character hats
                foreach (NPC npc in location.characters)
                {
                    Hat hat =
                        (npc as Child)?.hat.Value
                        ?? (npc as Horse)?.hat.Value;
                    this.ScanAndTrack(items, hat);
                }

                // building output
                if (location is BuildableGameLocation buildableLocation)
                {
                    foreach (var building in buildableLocation.buildings)
                    {
                        switch (building)
                        {
                            case Mill mill:
                                this.ScanAndTrack(items, mill.output.Value.items);
                                break;

                            case JunimoHut hut:
                                this.ScanAndTrack(items, hut.output.Value.items);
                                break;
                        }
                    }
                }

                // map objects
                foreach (SObject item in location.objects.Values)
                {
                    if (item is Chest || !this.IsSpawnedWorldItem(item))
                        this.ScanAndTrack(items, item, isRootInWorld: true);
                }
            }

            // inventory
            this.ScanAndTrack(items, Game1.player.Items, isInInventory: true);
            this.ScanAndTrack(
                items,
                new Item[]
                {
                    Game1.player.shirtItem.Value,
                    Game1.player.pantsItem.Value,
                    Game1.player.boots.Value,
                    Game1.player.hat.Value,
                    Game1.player.leftRing.Value,
                    Game1.player.rightRing.Value
                },
                isInInventory: true
            );

            // hay in silos
            int hayCount = Game1.getFarm()?.piecesOfHay.Value ?? 0;
            while (hayCount > 0)
            {
                SObject hay = new SObject(178, 1);
                hay.Stack = Math.Min(hayCount, hay.maximumStackSize());
                hayCount -= hay.Stack;
                this.ScanAndTrack(items, hay);
            }

            return items;
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

        /// <summary>Recursively find all items contained within a root item (including the root item itself) and add them to the <paramref name="tracked"/> list.</summary>
        /// <param name="tracked">The list to populate.</param>
        /// <param name="root">The root item to search.</param>
        /// <param name="isInInventory">Whether the item being scanned is in the current player's inventory.</param>
        /// <param name="isRootInWorld">Whether the item is placed directly in the world.</param>
        private void ScanAndTrack(List<FoundItem> tracked, Item root, bool isInInventory = false, bool isRootInWorld = false)
        {
            foreach (FoundItem found in this.Scan(root, isInInventory, isRootInWorld))
                tracked.Add(found);
        }

        /// <summary>Recursively find all items contained within a root item (including the root item itself) and add them to the <paramref name="tracked"/> list.</summary>
        /// <param name="tracked">The list to populate.</param>
        /// <param name="roots">The root items to search.</param>
        /// <param name="isInInventory">Whether the item being scanned is in the current player's inventory.</param>
        /// <param name="isRootInWorld">Whether the item is placed directly in the world.</param>
        private void ScanAndTrack(List<FoundItem> tracked, IEnumerable<Item> roots, bool isInInventory = false, bool isRootInWorld = false)
        {
            foreach (FoundItem found in roots.SelectMany(root => this.Scan(root, isInInventory, isRootInWorld)))
                tracked.Add(found);
        }

        /// <summary>Recursively find all items contained within a root item (including the root item itself).</summary>
        /// <param name="root">The root item to search.</param>
        /// <param name="isInInventory">Whether the item being scanned is in the current player's inventory.</param>
        /// <param name="isRootInWorld">Whether the item is placed directly in the world.</param>
        private IEnumerable<FoundItem> Scan(Item root, bool isInInventory, bool isRootInWorld)
        {
            if (root == null)
                yield break;

            // get root
            yield return new FoundItem(root, isInInventory);

            // get direct contents
            foreach (FoundItem child in this.GetDirectContents(root, isRootInWorld).SelectMany(p => this.Scan(p, isInInventory, isRootInWorld: false)))
                yield return child;
        }

        /// <summary>Get the items contained by an item. This is not recursive and may return null values.</summary>
        /// <param name="root">The root item to search.</param>
        /// <param name="isRootInWorld">Whether the item is placed directly in the world.</param>
        private IEnumerable<Item> GetDirectContents(Item root, bool isRootInWorld)
        {
            if (root == null)
                yield break;

            // held object
            if (root is SObject obj)
            {
                if (obj.MinutesUntilReady <= 0 || obj is Cask) // cask output can be retrieved anytime
                    yield return obj.heldObject.Value;
            }
            else if (this.IsCustomItemClass(root))
            {
                // convention for custom mod items
                Item heldItem =
                    this.Reflection.GetField<Item>(root, nameof(SObject.heldObject), required: false)?.GetValue()
                    ?? this.Reflection.GetProperty<Item>(root, nameof(SObject.heldObject), required: false)?.GetValue();
                if (heldItem != null)
                    yield return heldItem;
            }

            // inventories
            switch (root)
            {
                case StorageFurniture dresser:
                    foreach (Item item in dresser.heldItems)
                        yield return item;
                    break;

                case Chest chest when (!isRootInWorld || chest.playerChest.Value):
                    foreach (Item item in chest.items)
                        yield return item;
                    break;

                case Tool tool:
                    foreach (SObject item in tool.attachments)
                        yield return item;
                    break;
            }
        }

        /// <summary>Get whether an item instance is a custom mod subclass.</summary>
        /// <param name="item">The item to check.</param>
        private bool IsCustomItemClass(Item item)
        {
            string itemNamespace = item.GetType().Namespace ?? "";
            return itemNamespace != "StardewValley" && !itemNamespace.StartsWith("StardewValley.");
        }
    }
}
