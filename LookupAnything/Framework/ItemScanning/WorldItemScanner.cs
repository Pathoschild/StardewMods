using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.ItemScanning;

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
    /// This is derived from <see cref="Utility.ForEachItem"/> with some improvements:
    ///   * removed items held by other players, items floating on the ground, spawned forage, and output in a non-ready machine (except casks which can be emptied anytime);
    ///   * added hay in silos;
    ///   * added tool attachments;
    ///   * added recursive scanning (e.g. inside held chests) to support mods like Item Bag.
    /// </remarks>
    public IEnumerable<FoundItem> GetAllOwnedItems()
    {
        List<FoundItem> items = [];
        HashSet<Item> itemsSeen = new(new ObjectReferenceComparer<Item>());

        // in locations
        foreach (GameLocation location in CommonHelper.GetLocations())
        {
            // furniture
            foreach (Furniture furniture in location.furniture)
                this.ScanAndTrack(tracked: items, itemsSeen: itemsSeen, root: furniture, parent: location, isRootInWorld: true);

            // farmhouse fridge
            Chest? fridge = location switch
            {
                FarmHouse house => house.fridge.Value,
                IslandFarmHouse house => house.fridge.Value,
                _ => null
            };
            this.ScanAndTrack(tracked: items, itemsSeen: itemsSeen, root: fridge, parent: location, includeRoot: false);

            // character hats
            foreach (NPC npc in location.characters)
            {
                Hat? hat =
                    (npc as Child)?.hat.Value
                    ?? (npc as Horse)?.hat.Value;
                this.ScanAndTrack(tracked: items, itemsSeen: itemsSeen, root: hat, parent: npc);
            }

            // building output
            foreach (var building in location.buildings)
            {
                if (building is JunimoHut hut)
                    this.ScanAndTrack(tracked: items, itemsSeen: itemsSeen, root: hut.GetOutputChest(), parent: hut, includeRoot: false);

                foreach (Chest chest in building.buildingChests)
                    this.ScanAndTrack(tracked: items, itemsSeen: itemsSeen, root: chest, parent: building, includeRoot: false);
            }

            // map objects
            foreach (SObject item in location.objects.Values)
            {
                if (item is Chest || !this.IsSpawnedWorldItem(item))
                    this.ScanAndTrack(tracked: items, itemsSeen: itemsSeen, root: item, parent: location, isRootInWorld: true);
            }
        }

        // inventory
        this.ScanAndTrack(tracked: items, itemsSeen: itemsSeen, roots: Game1.player.Items, parent: Game1.player, isInInventory: true);
        this.ScanAndTrack(
            tracked: items,
            itemsSeen: itemsSeen,
            roots: [
                Game1.player.shirtItem.Value,
                    Game1.player.pantsItem.Value,
                    Game1.player.boots.Value,
                    Game1.player.hat.Value,
                    Game1.player.leftRing.Value,
                    Game1.player.rightRing.Value
            ],
            parent: Game1.player,
            isInInventory: true
        );

        // hay in silos
        Farm farm = Game1.getFarm();
        int hayCount = farm?.piecesOfHay.Value ?? 0;
        while (hayCount > 0)
        {
            Item hay = ItemRegistry.Create("(O)178");
            hay.Stack = Math.Min(hayCount, hay.maximumStackSize());
            hayCount -= hay.Stack;
            this.ScanAndTrack(tracked: items, itemsSeen: itemsSeen, root: hay, parent: farm);
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
                || obj.isForage()
                || obj.Category == SObject.litterCategory
            );
    }

    /// <summary>Recursively find all items contained within a root item (including the root item itself) and add them to the <paramref name="tracked"/> list.</summary>
    /// <param name="tracked">The list to populate.</param>
    /// <param name="itemsSeen">The items which have already been scanned.</param>
    /// <param name="root">The root item to search.</param>
    /// <param name="parent">The parent entity which contains the <paramref name="root"/> (e.g. location, chest, furniture, etc).</param>
    /// <param name="isInInventory">Whether the item being scanned is in the current player's inventory.</param>
    /// <param name="isRootInWorld">Whether the item is placed directly in the world.</param>
    /// <param name="includeRoot">Whether to include the root item in the returned values.</param>
    private void ScanAndTrack(List<FoundItem> tracked, ISet<Item> itemsSeen, Item? root, object? parent, bool isInInventory = false, bool isRootInWorld = false, bool includeRoot = true)
    {
        foreach (FoundItem found in this.Scan(itemsSeen, root, parent, isInInventory, isRootInWorld, includeRoot))
            tracked.Add(found);
    }

    /// <summary>Recursively find all items contained within a root item (including the root item itself) and add them to the <paramref name="tracked"/> list.</summary>
    /// <param name="tracked">The list to populate.</param>
    /// <param name="itemsSeen">The items which have already been scanned.</param>
    /// <param name="roots">The root items to search.</param>
    /// <param name="parent">The parent entity which contains the <paramref name="roots"/> (e.g. location, chest, furniture, etc).</param>
    /// <param name="isInInventory">Whether the item being scanned is in the current player's inventory.</param>
    /// <param name="isRootInWorld">Whether the item is placed directly in the world.</param>
    /// <param name="includeRoots">Whether to include the root items in the returned values.</param>
    private void ScanAndTrack(List<FoundItem> tracked, ISet<Item> itemsSeen, IEnumerable<Item> roots, object parent, bool isInInventory = false, bool isRootInWorld = false, bool includeRoots = true)
    {
        foreach (FoundItem found in roots.SelectMany(root => this.Scan(itemsSeen, root, parent, isInInventory, isRootInWorld, includeRoots)))
            tracked.Add(found);
    }

    /// <summary>Recursively find all items contained within a root item (including the root item itself).</summary>
    /// <param name="itemsSeen">The items which have already been scanned.</param>
    /// <param name="root">The root item to search.</param>
    /// <param name="parent">The parent entity which contains the <paramref name="root"/> (e.g. location, chest, furniture, etc).</param>
    /// <param name="isInInventory">Whether the item being scanned is in the current player's inventory.</param>
    /// <param name="isRootInWorld">Whether the item is placed directly in the world.</param>
    /// <param name="includeRoot">Whether to include the root item in the returned values.</param>
    private IEnumerable<FoundItem> Scan(ISet<Item> itemsSeen, Item? root, object? parent, bool isInInventory, bool isRootInWorld, bool includeRoot = true)
    {
        if (root == null || !itemsSeen.Add(root))
            yield break;

        // get root
        yield return new FoundItem(root, parent, isInInventory);

        // get direct contents
        foreach (FoundItem child in this.GetDirectContents(root, isRootInWorld).SelectMany(p => this.Scan(itemsSeen, p, root, isInInventory, isRootInWorld: false)))
            yield return child;
    }

    /// <summary>Get the items contained by an item. This is not recursive and may return null values.</summary>
    /// <param name="root">The root item to search.</param>
    /// <param name="isRootInWorld">Whether the item is placed directly in the world.</param>
    private IEnumerable<Item> GetDirectContents(Item? root, bool isRootInWorld)
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
            Item? heldItem =
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
                foreach (Item item in chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID))
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
