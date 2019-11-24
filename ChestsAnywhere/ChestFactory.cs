using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>Provides translations stored in the mod's folder.</summary>
        private readonly ITranslationHelper Translations;

        /// <summary>Whether to support access to the shipping bin.</summary>
        private readonly bool EnableShippingBin;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="dataHelper">An API for reading and storing local mod data.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="translations">Provides translations stored in the mod's folder.</param>
        /// <param name="enableShippingBin">Whether to support access to the shipping bin.</param>
        public ChestFactory(IDataHelper dataHelper, IReflectionHelper reflection, ITranslationHelper translations, bool enableShippingBin)
        {
            this.DataHelper = dataHelper;
            this.Reflection = reflection;
            this.Translations = translations;
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
                        ? this.Translations.Get("default-category.duplicate", new { locationName = entry.Category, number = ++defaultCategories[entry.Category] })
                        : entry.Category;

                    // chests in location
                    {
                        int namelessChests = 0;
                        int namelessGrabbers = 0;
                        foreach (KeyValuePair<Vector2, SObject> pair in location.Objects.Pairs)
                        {
                            Vector2 tile = pair.Key;
                            SObject obj = pair.Value;

                            // chests
                            if (obj is Chest chest && chest.playerChest.Value)
                            {
                                yield return new ManagedChest(
                                    container: new ChestContainer(chest, context: chest, this.Reflection),
                                    location: location,
                                    tile: tile,
                                    defaultDisplayName: this.Translations.Get("default-name.chest", new { number = ++namelessChests }),
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
                                    defaultDisplayName: this.Translations.Get("default-name.auto-grabber", new { number = ++namelessGrabbers }),
                                    defaultCategory: category
                                );
                            }
                        }
                    }

                    // farmhouse containers
                    if (location is FarmHouse house && Game1.player.HouseUpgradeLevel > 0)
                    {
                        Chest fridge = house.fridge.Value;
                        if (fridge != null)
                        {
                            yield return new ManagedChest(
                                container: new ChestContainer(fridge, context: fridge, this.Reflection),
                                location: location,
                                tile: Vector2.Zero,
                                defaultDisplayName: this.Translations.Get("default-name.fridge"),
                                defaultCategory: category
                            );
                        }
                    }

                    // dressers
                    if (location is DecoratableLocation decoratableLocation)
                    {
                        var dresserCounts = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
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
                                    defaultDisplayName: this.Translations.Get("default-name.junimo-hut", new { number = ++namelessHuts }),
                                    defaultCategory: category
                                );
                            }
                        }
                    }

                    // shipping bin
                    if (this.EnableShippingBin && location is Farm farm && object.ReferenceEquals(farm, Game1.getFarm()))
                    {
                        yield return new ManagedChest(
                            container: new ShippingBinContainer(farm, this.DataHelper),
                            location: farm,
                            tile: Vector2.Zero,
                            defaultDisplayName: this.Translations.Get("default-name.shipping-bin"),
                            defaultCategory: category
                        );
                    }
                }
            }

            return (
                from chest in Search()
                orderby chest.Order ?? int.MaxValue, chest.DisplayName
                where
                    chest.Container.IsSameAs(alwaysIncludeContainer)
                    || (
                        (!excludeHidden || !chest.IsIgnored)
                        && range.IsInRange(chest.Location)
                    )
                select chest
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
                .FirstOrDefault(p => p.Container.IsSameAs(chest.items));
        }

        /// <summary>Get the player chest from the given menu, if any.</summary>
        /// <param name="menu">The menu to check.</param>
        public ManagedChest GetChestFromMenu(IClickableMenu menu)
        {
            // get inventory from menu
            IList<Item> inventory = null;
            switch (menu)
            {
                case ItemGrabMenu itemGrabMenu:
                    inventory = this.GetInventoryFromContext(itemGrabMenu.context);
                    break;

                case ShopMenu shopMenu:
                    inventory = this.GetInventoryFromContext(shopMenu.source);
                    break;
            }
            if (inventory == null)
                return null;

            // get chest from inventory
            return this
                .GetChests(RangeHandler.Unlimited())
                .FirstOrDefault(p => p.Container.IsSameAs(inventory));
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the locations which are accessible to the current player (regardless of settings).</summary>
        private IEnumerable<GameLocation> GetAccessibleLocations()
        {
            // main player can access chests in any location
            if (Context.IsMainPlayer)
                return CommonHelper.GetLocations();

            // secondary player can only safely access chests in their current location
            // (changes to other locations aren't synced to the other players)
            return new[] { Game1.player.currentLocation };
        }

        /// <summary>Get the underlying inventory for an <see cref="ItemGrabMenu.context"/> value.</summary>
        /// <param name="context">The menu context.</param>
        private IList<Item> GetInventoryFromContext(object context)
        {
            switch (context)
            {
                // chest
                case Chest chest:
                    return chest.items;

                // auto-grabber
                case SObject obj when obj.ParentSheetIndex == this.AutoGrabberID:
                    return (obj.heldObject.Value as Chest)?.items;

                // buildings
                case JunimoHut hut:
                    return hut.output.Value?.items;
                case Mill mill:
                    return mill.output.Value?.items;

                // shipping bin
                case Farm _:
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
                    ? this.Translations.Get("default-category.owned-cabin", new { owner = cabin.owner?.Name })
                    : this.Translations.Get("default-category.unowned-cabin");
            }

            return location.Name;
        }
    }
}
