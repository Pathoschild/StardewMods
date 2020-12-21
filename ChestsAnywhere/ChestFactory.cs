using System;
using System.Collections.Generic;
using System.Linq;
using Common.Utilities;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.ChestsAnywhere
{
    /// <summary>Encapsulates logic for finding chests.</summary>
    internal class ChestFactory
    {
        /*********
        ** Fields
        *********/
        /// <summary>The item ID for auto-grabbers.</summary>
        private readonly int AutoGrabberID = 165;

        /// <summary>An API for reading and storing local mod data.</summary>
        private readonly IDataHelper DataHelper;

        /// <summary>Provides multiplayer utilities.</summary>
        private readonly IMultiplayerHelper Multiplayer;

        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>Whether to support access to the shipping bin.</summary>
        private readonly bool EnableShippingBin;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="dataHelper">An API for reading and storing local mod data.</param>
        /// <param name="multiplayer">Provides multiplayer utilities.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="enableShippingBin">Whether to support access to the shipping bin.</param>
        public ChestFactory(IDataHelper dataHelper, IMultiplayerHelper multiplayer, IReflectionHelper reflection, bool enableShippingBin)
        {
            this.DataHelper = dataHelper;
            this.Multiplayer = multiplayer;
            this.Reflection = reflection;
            this.EnableShippingBin = enableShippingBin;
        }

        /// <summary>Get all player chests.</summary>
        /// <param name="range">Determines whether given locations are in range of the player for remote chest access.</param>
        /// <param name="excludeHidden">Whether to exclude chests marked as hidden.</param>
        /// <param name="alwaysIncludeContainer">A container to include even if it would normally be hidden.</param>
        public IEnumerable<ManagedChest> GetChests(RangeHandler range, bool excludeHidden = false, IContainer alwaysIncludeContainer = null)
        {
            IEnumerable<ManagedChest> Search()
            {
                // get location info
                var locations =
                    (
                        from GameLocation location in this.GetAccessibleLocations()
                        select new
                        {
                            Location = location,
                            Category = this.GetCategory(location)
                        }
                    )
                    .ToArray();
                IDictionary<string, int> defaultCategories = locations
                    .GroupBy(p => p.Category)
                    .Where(p => p.Count() > 1)
                    .ToDictionary(p => p.Key, p => 0);

                // find chests
                foreach (var entry in locations)
                {
                    // get info
                    GameLocation location = entry.Location;
                    string category = defaultCategories.ContainsKey(entry.Category)
                        ? I18n.DefaultCategory_Duplicate(locationName: entry.Category, number: ++defaultCategories[entry.Category])
                        : entry.Category;

                    // chests in location
                    {
                        int namelessChests = 0;
                        int namelessGrabbers = 0;
                        int namelessHoppers = 0;
                        int namelessMiniShippingBins = 0;
                        int junimoChestCount = 0;
                        foreach (KeyValuePair<Vector2, SObject> pair in location.Objects.Pairs)
                        {
                            Vector2 tile = pair.Key;
                            SObject obj = pair.Value;

                            // chests
                            if (obj is Chest chest && chest.playerChest.Value)
                            {
                                if (chest.SpecialChestType == Chest.SpecialChestTypes.JunimoChest && ++junimoChestCount > 1)
                                    continue; // only list one Junimo Chest per location (since they share inventory)

                                yield return new ManagedChest(
                                    container: new ChestContainer(chest, context: chest, showColorPicker: this.CanShowColorPicker(chest, location), this.Reflection),
                                    location: location,
                                    tile: tile,
                                    defaultDisplayName: chest.SpecialChestType switch
                                    {
                                        Chest.SpecialChestTypes.AutoLoader => I18n.DefaultName_Other(name: GameI18n.GetBigCraftableName(275), number: ++namelessHoppers),
                                        Chest.SpecialChestTypes.JunimoChest => GameI18n.GetBigCraftableName(256),
                                        Chest.SpecialChestTypes.MiniShippingBin => I18n.DefaultName_Other(name: GameI18n.GetBigCraftableName(248), number: ++namelessMiniShippingBins),
                                        _ => I18n.DefaultName_Other(name: GameI18n.GetBigCraftableName(130), number: ++namelessChests)
                                    },
                                    defaultCategory: category
                                );
                            }

                            // auto-grabbers
                            else if (obj.ParentSheetIndex == this.AutoGrabberID && obj.heldObject.Value is Chest grabberChest)
                            {
                                yield return new ManagedChest(
                                    container: new AutoGrabberContainer(obj, grabberChest, context: obj, this.Reflection),
                                    location: location,
                                    tile: tile,
                                    defaultDisplayName: I18n.DefaultName_Other(name: GameI18n.GetBigCraftableName(165), number: ++namelessGrabbers),
                                    defaultCategory: category
                                );
                            }
                        }
                    }

                    // farmhouse fridge
                    {
                        Chest fridge = this.GetStaticFridge(location);
                        if (fridge != null)
                        {
                            yield return new ManagedChest(
                                container: new ChestContainer(fridge, context: fridge, showColorPicker: false, this.Reflection),
                                location: location,
                                tile: Vector2.Zero,
                                defaultDisplayName: I18n.DefaultName_Fridge(),
                                defaultCategory: category
                            );
                        }
                    }

                    // dressers
                    if (location is DecoratableLocation decoratableLocation)
                    {
                        var dresserCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        foreach (StorageFurniture furniture in decoratableLocation.furniture.OfType<StorageFurniture>())
                        {
                            var container = new StorageFurnitureContainer(furniture, this.Reflection);
                            dresserCounts[container.DefaultName] = dresserCounts.TryGetValue(container.DefaultName, out int count) ? ++count : (count = 1);
                            yield return new ManagedChest(
                                container: container,
                                location,
                                furniture.TileLocation,
                                defaultDisplayName: $"{container.DefaultName} #{count}",
                                defaultCategory: category
                            );
                        }
                    }

                    // buildings
                    if (location is BuildableGameLocation buildableLocation)
                    {
                        int namelessHuts = 0;
                        foreach (Building building in buildableLocation.buildings)
                        {
                            if (building is JunimoHut hut)
                            {
                                yield return new ManagedChest(
                                    container: new JunimoHutContainer(hut, this.Reflection),
                                    location: location,
                                    tile: new Vector2(hut.tileX.Value, hut.tileY.Value),
                                    defaultDisplayName: I18n.DefaultName_Other(name: GameI18n.GetBuildingName("Junimo Hut"), number: ++namelessHuts),
                                    defaultCategory: category
                                );
                            }
                        }
                    }

                    // shipping bin
                    if (this.HasShippingBin(location))
                    {
                        string shippingBinLabel = GameI18n.GetBuildingName("Shipping Bin");

                        if (Constants.TargetPlatform == GamePlatform.Android)
                        {
                            yield return new ManagedChest(
                                container: new ShippingBinContainer(location, this.DataHelper, ShippingBinMode.MobileStore),
                                location: location,
                                tile: Vector2.Zero,
                                defaultDisplayName: $"{shippingBinLabel} ({I18n.DefaultName_ShippingBin_Store()})",
                                defaultCategory: category
                            );
                            yield return new ManagedChest(
                                container: new ShippingBinContainer(location, this.DataHelper, ShippingBinMode.MobileTake),
                                location: location,
                                tile: Vector2.Zero,
                                defaultDisplayName: $"{shippingBinLabel} ({I18n.DefaultName_ShippingBin_Take()})",
                                defaultCategory: category
                            );
                        }
                        else
                        {
                            yield return new ManagedChest(
                                container: new ShippingBinContainer(location, this.DataHelper, ShippingBinMode.Normal),
                                location: location,
                                tile: Vector2.Zero,
                                defaultDisplayName: shippingBinLabel,
                                defaultCategory: category
                            );
                        }
                    }
                }
            }

            return Search()
                .OrderBy(chest => chest.Order ?? int.MaxValue)
                .ThenBy(chest => chest.DisplayName, HumanSortComparer.DefaultIgnoreCase)
                .Where(chest =>
                    chest.Container.IsSameAs(alwaysIncludeContainer)
                    || (
                        (!excludeHidden || !chest.IsIgnored)
                        && range.IsInRange(chest.Location)
                    )
                );
        }

        /// <summary>Get the player chest on the specified tile (if any).</summary>
        /// <param name="tile">The tile to check.</param>
        public ManagedChest GetChestFromTile(Vector2 tile)
        {
            if (!Game1.currentLocation.Objects.TryGetValue(tile, out SObject obj) || !(obj is Chest chest))
                return null;

            return this
                .GetChests(RangeHandler.CurrentLocation())
                .FirstOrDefault(p => p.Container.IsSameAs(this.GetChestInventory(chest)));
        }

        /// <summary>Get the player chest from the given menu, if any.</summary>
        /// <param name="menu">The menu to check.</param>
        public ManagedChest GetChestFromMenu(IClickableMenu menu)
        {
            // get inventory from menu
            IList<Item> inventory = null;
            GameLocation forLocation = null;
            switch (menu)
            {
                case ItemGrabMenu itemGrabMenu:
                    inventory = this.GetInventoryFromContext(itemGrabMenu.context);
                    forLocation = itemGrabMenu.context as GameLocation;
                    break;

                case ShopMenu shopMenu:
                    inventory = this.GetInventoryFromContext(shopMenu.source);
                    forLocation = shopMenu.source as GameLocation;
                    break;
            }
            if (inventory == null)
                return null;

            // get chest from inventory
            return this
                .GetChests(RangeHandler.Unlimited())
                .OrderByDescending(p => object.ReferenceEquals(forLocation, p.Location)) // shipping bin in different locations has the same inventory, so prioritize by location if possible;
                .FirstOrDefault(p => p.Container.IsSameAs(inventory));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the locations which are accessible to the current player (regardless of settings).</summary>
        private IEnumerable<GameLocation> GetAccessibleLocations()
        {
            return Context.IsMainPlayer
                ? CommonHelper.GetLocations()
                : this.Multiplayer.GetActiveLocations();
        }

        /// <summary>Get the inventory for a chest.</summary>
        /// <param name="chest">The chest instance.</param>
        private IList<Item> GetChestInventory(Chest chest)
        {
            return chest?.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
        }

        /// <summary>Get the underlying inventory for an <see cref="ItemGrabMenu.context"/> value.</summary>
        /// <param name="context">The menu context.</param>
        private IList<Item> GetInventoryFromContext(object context)
        {
            switch (context)
            {
                // chest
                case Chest chest:
                    return this.GetChestInventory(chest);

                // auto-grabber
                case SObject obj when obj.ParentSheetIndex == this.AutoGrabberID:
                    return this.GetChestInventory(obj.heldObject.Value as Chest);

                // buildings
                case JunimoHut hut:
                    return this.GetChestInventory(hut.output.Value);
                case Mill mill:
                    return this.GetChestInventory(mill.output.Value);

                // shipping bin
                case Farm _:
                case IslandWest _:
                case ShippingBin _:
                    return Game1.getFarm().getShippingBin(Game1.player);

                // dresser
                case StorageFurniture furniture:
                    return furniture.heldItems;

                // unsupported type
                default:
                    return null;
            }
        }

        /// <summary>Get the default category name for a location.</summary>
        /// <param name="location">The in-game location.</param>
        private string GetCategory(GameLocation location)
        {
            if (location is Cabin cabin)
            {
                return !string.IsNullOrWhiteSpace(cabin.owner?.Name)
                    ? I18n.DefaultCategory_OwnedCabin(owner: cabin.owner?.Name)
                    : I18n.DefaultCategory_UnownedCabin();
            }

            return location.Name;
        }

        /// <summary>Get whether it's safe to show a color picker for the given chest.</summary>
        /// <param name="chest">The chest instance.</param>
        /// <param name="location">The location containing the chest.</param>
        /// <remarks>The game is hardcoded to exit the chest menu if this is enabled and the chest isn't present in the player's *current* location (see <see cref="ItemGrabMenu.update"/>), except if its tile location is (0, 0).</remarks>
        private bool CanShowColorPicker(Chest chest, GameLocation location)
        {
            if (chest.TileLocation == Vector2.Zero)
                return true;

            return
                object.ReferenceEquals(Game1.currentLocation, location)
                && Game1.currentLocation.objects.TryGetValue(chest.TileLocation, out SObject obj)
                && object.ReferenceEquals(obj, chest);
        }

        /// <summary>Get the static fridge for a location, if any.</summary>
        /// <param name="location">The location to check.</param>
        private Chest GetStaticFridge(GameLocation location)
        {
            // main farmhouse or cabin
            if (location is FarmHouse house && Game1.player.HouseUpgradeLevel > 0)
                return house.fridge.Value;

            // island farmhouse
            if (location is IslandFarmHouse islandHouse && islandHouse.visited.Value)
                return islandHouse.fridge.Value;

            return null;
        }

        /// <summary>Whether the location has a predefined shipping bin.</summary>
        /// <param name="location">The location to check.</param>
        private bool HasShippingBin(GameLocation location)
        {
            if (!this.EnableShippingBin)
                return false;

            return location switch
            {
                Farm => true,
                IslandWest islandFarm => islandFarm.farmhouseRestored.Value,
                _ => false
            };
        }
    }
}
