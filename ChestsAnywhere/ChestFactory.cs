using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
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
        /// <summary>The qualified item ID for auto-grabbers.</summary>
        private readonly string AutoGrabberID = "(BC)165";

        /// <summary>Provides multiplayer utilities.</summary>
        private readonly IMultiplayerHelper Multiplayer;

        /// <summary>The mod settings to apply.</summary>
        private readonly Func<ModConfig> Config;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplayer">Provides multiplayer utilities.</param>
        /// <param name="config">The mod settings to apply.</param>
        public ChestFactory(IMultiplayerHelper multiplayer, Func<ModConfig> config)
        {
            this.Multiplayer = multiplayer;
            this.Config = config;
        }

        /// <summary>Get all player chests.</summary>
        /// <param name="range">Determines whether given locations are in range of the player for remote chest access.</param>
        /// <param name="excludeHidden">Whether to exclude chests marked as hidden.</param>
        /// <param name="alwaysInclude">A chest to include even if it would normally be hidden.</param>
        public IEnumerable<ManagedChest> GetChests(RangeHandler range, bool excludeHidden = false, ManagedChest? alwaysInclude = null)
        {
            ModConfig config = this.Config();

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
                    .ToDictionary(p => p.Key, _ => 0);

                // find chests
                foreach (var entry in locations)
                {
                    IDictionary<string, int> nameCounts = new Dictionary<string, int>();

                    // get info
                    GameLocation location = entry.Location;
                    string category = defaultCategories.ContainsKey(entry.Category)
                        ? I18n.DefaultCategory_Duplicate(locationName: entry.Category, number: ++defaultCategories[entry.Category])
                        : entry.Category;

                    // chests in location
                    foreach (KeyValuePair<Vector2, SObject> pair in location.Objects.Pairs)
                    {
                        Vector2 tile = pair.Key;
                        SObject obj = pair.Value;

                        // chests
                        if (obj is Chest chest && chest.playerChest.Value)
                        {
                            yield return new ManagedChest(
                                container: new ChestContainer(chest, context: chest, showColorPicker: this.CanShowColorPicker(chest, location)),
                                location: location,
                                tile: tile,
                                mapEntity: chest,
                                defaultDisplayName: this.GetDisambiguatedDefaultName(chest.DisplayName, nameCounts),
                                defaultCategory: category
                            );
                        }

                        // auto-grabbers
                        else if (obj.QualifiedItemId == this.AutoGrabberID && obj.heldObject.Value is Chest grabberChest)
                        {
                            yield return new ManagedChest(
                                container: new AutoGrabberContainer(obj, grabberChest, context: obj),
                                location: location,
                                tile: tile,
                                mapEntity: obj,
                                defaultDisplayName: this.GetDisambiguatedDefaultName(obj.DisplayName, nameCounts),
                                defaultCategory: category
                            );
                        }

                        // sprinkler attachments
                        else if (config.EnableSprinklerAttachments && obj.IsSprinkler() && obj.heldObject.Value is { } attachment)
                        {
                            Chest? attachmentChest = (attachment as Chest) ?? (attachment.heldObject.Value as Chest);
                            if (attachmentChest is not null)
                            {
                                string displayName = attachment.DisplayName;
                                if (displayName == ItemRegistry.GetDataOrErrorItem("(O)130").DisplayName) // if the display name is just "Chest", show the sprinkler name
                                    displayName = obj.DisplayName;

                                yield return new ManagedChest(
                                    container: new ChestContainer(attachmentChest, context: attachmentChest, showColorPicker: false),
                                    location: location,
                                    tile: tile,
                                    mapEntity: obj,
                                    defaultDisplayName: this.GetDisambiguatedDefaultName(displayName, nameCounts),
                                    defaultCategory: category
                                );
                            }
                        }
                    }

                    // farmhouse fridge
                    {
                        Chest? fridge = this.GetStaticFridge(location);
                        if (fridge != null)
                        {
                            yield return new ManagedChest(
                                container: new ChestContainer(fridge, context: fridge, showColorPicker: false),
                                location: location,
                                tile: Vector2.Zero,
                                mapEntity: null,
                                defaultDisplayName: I18n.DefaultName_Fridge(),
                                defaultCategory: category
                            );
                        }
                    }

                    // dressers
                    foreach (Furniture rawFurniture in location.furniture)
                    {
                        if (rawFurniture is not StorageFurniture furniture)
                            continue;

                        if (furniture.QualifiedItemId == "(F)CCFishTank" && location is CommunityCenter)
                            continue; // temporary fish tank

                        var container = new StorageFurnitureContainer(furniture);
                        yield return new ManagedChest(
                            container: container,
                            location,
                            furniture.TileLocation,
                            mapEntity: furniture,
                            defaultDisplayName: this.GetDisambiguatedDefaultName(furniture.DisplayName, nameCounts),
                            defaultCategory: category
                        );
                    }

                    // buildings
                    foreach (Building building in location.buildings)
                    {
                        if (building is JunimoHut hut)
                        {
                            yield return new ManagedChest(
                                container: new JunimoHutContainer(hut),
                                location: location,
                                tile: new Vector2(hut.tileX.Value, hut.tileY.Value),
                                mapEntity: building,
                                defaultDisplayName: this.GetDisambiguatedDefaultName(GameI18n.GetString("Strings\\Buildings:JunimoHut_Name"), nameCounts),
                                defaultCategory: category
                            );
                        }
                    }

                    // shipping bin
                    if (this.HasShippingBin(location))
                    {
                        string shippingBinLabel = GameI18n.GetString("Strings\\Buildings:ShippingBin_Name");

                        if (Constants.TargetPlatform == GamePlatform.Android)
                        {
                            yield return new ManagedChest(
                                container: new ShippingBinContainer(location, ShippingBinMode.MobileStore),
                                location: location,
                                tile: Vector2.Zero,
                                mapEntity: null,
                                defaultDisplayName: $"{shippingBinLabel} ({I18n.DefaultName_ShippingBin_Store()})",
                                defaultCategory: category
                            );
                            yield return new ManagedChest(
                                container: new ShippingBinContainer(location, ShippingBinMode.MobileTake),
                                location: location,
                                tile: Vector2.Zero,
                                mapEntity: null,
                                defaultDisplayName: $"{shippingBinLabel} ({I18n.DefaultName_ShippingBin_Take()})",
                                defaultCategory: category
                            );
                        }
                        else
                        {
                            yield return new ManagedChest(
                                container: new ShippingBinContainer(location, ShippingBinMode.Normal),
                                location: location,
                                tile: Vector2.Zero,
                                mapEntity: null,
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
                    (
                        alwaysInclude != null
                        && chest.Container.IsSameAs(alwaysInclude.Container)
                        && object.ReferenceEquals(chest.Location, alwaysInclude.Location)
                        && chest.Tile == alwaysInclude.Tile
                    )
                    || (
                        (!excludeHidden || !chest.IsIgnored)
                        && range.IsInRange(chest.Location)
                    )
                );
        }

        /// <summary>Get the player chest on the specified tile (if any).</summary>
        /// <param name="tile">The tile to check.</param>
        public ManagedChest? GetChestFromTile(Vector2 tile)
        {
            if (!Game1.currentLocation.Objects.TryGetValue(tile, out SObject obj) || obj is not Chest chest)
                return null;

            return ChestFactory.GetBestMatch(
                chests: this.GetChests(RangeHandler.CurrentLocation()),
                inventory: this.GetChestInventory(chest),
                location: Game1.currentLocation,
                tile: tile,
                mapEntity: chest
            );
        }

        /// <summary>Get the player chest from the given menu, if any.</summary>
        /// <param name="menu">The menu to check.</param>
        public ManagedChest? GetChestFromMenu(IClickableMenu menu)
        {
            // get inventory from menu
            IList<Item?>? inventory = null;
            GameLocation? forLocation = null;
            Vector2? tile = null;
            SObject? chest = null;
            switch (menu)
            {
                case ItemGrabMenu itemGrabMenu:
                    inventory = this.GetInventoryFromContext(itemGrabMenu.context);
                    forLocation = this.GetLocationFromContext(itemGrabMenu.context);
                    tile = this.GetTileFromContext(itemGrabMenu.context);
                    chest = itemGrabMenu.context as SObject;
                    break;

                case ShopMenu shopMenu:
                    inventory = this.GetInventoryFromContext(shopMenu.source);
                    forLocation = this.GetLocationFromContext(shopMenu.source);
                    tile = this.GetTileFromContext(shopMenu.source);
                    chest = shopMenu.source as SObject;
                    break;
            }
            if (inventory == null)
                return null;

            // get chest from inventory
            return ChestFactory.GetBestMatch(
                chests: this.GetChests(RangeHandler.Unlimited()),
                inventory: inventory,
                location: forLocation,
                tile: tile,
                mapEntity: chest
            );
        }

        /// <summary>Get the chest which contains the given inventory, prioritizing the closest match based on the available data.</summary>
        /// <param name="chests">The available chests to filter.</param>
        /// <param name="inventory">The chest inventory to match.</param>
        /// <param name="location">The chest location, if known.</param>
        /// <param name="tile">The chest tile, if known.</param>
        /// <param name="mapEntity">The map entity equivalent to the container (e.g. the object or furniture instance), if applicable.</param>
        public static ManagedChest? GetBestMatch(IEnumerable<ManagedChest> chests, IList<Item?> inventory, GameLocation? location, Vector2? tile, object? mapEntity)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            return
                (
                    from chest in chests
                    where chest.Container.IsSameAs(inventory)
                    orderby
                        mapEntity == null || object.ReferenceEquals(chest.MapEntity, mapEntity) descending,
                        location == null || object.ReferenceEquals(location, chest.Location) descending,
                        tile == null || chest.Tile == tile descending
                    select chest
                )
                .FirstOrDefault();
        }

        /// <summary>Get the chest which contains the given inventory, prioritizing the closest match for chests with shared inventory like Junimo chests.</summary>
        /// <param name="chests">The available chests to filter.</param>
        /// <param name="search">The chest to match.</param>
        public static ManagedChest? GetBestMatch(IEnumerable<ManagedChest> chests, ManagedChest? search)
        {
            // We can't just return the search chest here, since it may be a previously created
            // instance that's not in the list being searched.
            return search != null
                ? ChestFactory.GetBestMatch(chests, search.Container.Inventory, search.Location, search.Tile, search.MapEntity)
                : null;
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
        [return: NotNullIfNotNull("chest")]
        private IList<Item?>? GetChestInventory(Chest? chest)
        {
            return chest?.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
        }

        /// <summary>Get the container location from an <see cref="ItemGrabMenu.context"/>, if applicable.</summary>
        /// <param name="context">The menu context.</param>
        private GameLocation? GetLocationFromContext(object context)
        {
            return context switch
            {
                GameLocation location => location,
                ShippingBin => Game1.getFarm(),
                _ => null
            };
        }

        /// <summary>Get the container's tile position from an <see cref="ItemGrabMenu.context"/>, if applicable.</summary>
        /// <param name="context">The menu context.</param>
        private Vector2? GetTileFromContext(object context)
        {
            return context switch
            {
                SObject obj => obj.TileLocation,
                Building building => new Vector2(building.tileX.Value, building.tileY.Value),
                _ => null
            };
        }

        /// <summary>Get the underlying inventory for an <see cref="ItemGrabMenu.context"/> value.</summary>
        /// <param name="context">The menu context.</param>
        private IList<Item?>? GetInventoryFromContext(object context)
        {
            switch (context)
            {
                // chest
                case Chest chest:
                    return this.GetChestInventory(chest);

                // auto-grabber
                case SObject obj when obj.QualifiedItemId == this.AutoGrabberID:
                    return this.GetChestInventory(obj.heldObject.Value as Chest);

                // shipping bin
                case Farm:
                case IslandWest:
                case ShippingBin:
                    return Game1.getFarm().getShippingBin(Game1.player);

                // buildings
                case JunimoHut hut:
                    return this.GetChestInventory(hut.GetOutputChest());
                case Building building:
                    {
                        // 'output' convention (e.g. vanilla Mill)
                        Chest? output = building.GetBuildingChest("Output");
                        if (output != null)
                            return output.Items;

                        // else only chest
                        IList<Chest>? chests = building.buildingChests;
                        if (chests?.Count == 1)
                            return chests[0].Items;

                        // else no match
                        return null;
                    }

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
                    ? I18n.DefaultCategory_OwnedCabin(owner: cabin.owner?.Name ?? string.Empty)
                    : I18n.DefaultCategory_UnownedCabin();
            }

            return location.GetDisplayName() ?? location.Name;
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
        private Chest? GetStaticFridge(GameLocation location)
        {
            // main farmhouse or cabin
            if (location is FarmHouse house && house.fridgePosition != Point.Zero)
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
            if (!this.Config().EnableShippingBin)
                return false;

            return location switch
            {
                Farm => true,
                IslandWest islandFarm => islandFarm.farmhouseRestored.Value,
                _ => false
            };
        }

        /// <summary>Append an incrementing number to a default chest name.</summary>
        /// <param name="name">The name without the suffix.</param>
        /// <param name="nameCounts">The number of times names were previously disambiguated.</param>
        private string GetDisambiguatedDefaultName(string name, IDictionary<string, int> nameCounts)
        {
            if (!nameCounts.TryGetValue(name, out int prevNumber))
                prevNumber = 0;

            int number = prevNumber + 1;
            nameCounts[name] = number;

            return I18n.DefaultName_Other(name, number);
        }
    }
}
