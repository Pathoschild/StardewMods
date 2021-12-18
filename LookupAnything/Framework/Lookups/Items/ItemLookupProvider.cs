using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.JsonAssets;
using Pathoschild.Stardew.Common.Items.ItemData;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Items
{
    /// <summary>Provides lookup data for in-game items.</summary>
    internal class ItemLookupProvider : BaseLookupProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>Provides methods for searching and constructing items.</summary>
        private readonly ItemRepository ItemRepository = new ItemRepository();

        /// <summary>The Json Assets API.</summary>
        private readonly JsonAssetsIntegration JsonAssets;

        /// <summary>The mod configuration.</summary>
        private readonly Func<ModConfig> Config;

        /// <summary>Provides subject entries.</summary>
        private readonly ISubjectRegistry Codex;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="codex">Provides subject entries.</param>
        /// <param name="jsonAssets">The Json Assets API.</param>
        public ItemLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, Func<ModConfig> config, ISubjectRegistry codex, JsonAssetsIntegration jsonAssets)
            : base(reflection, gameHelper)
        {
            this.Config = config;
            this.Codex = codex;
            this.JsonAssets = jsonAssets;
        }

        /// <inheritdoc />
        public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
        {
            // map objects
            foreach (KeyValuePair<Vector2, SObject> pair in location.objects.Pairs)
            {
                if (location is IslandShrine && pair.Value is ItemPedestal)
                    continue; // part of the Fern Islands shrine puzzle, which is handled by the tile lookup provider

                if (this.GameHelper.CouldSpriteOccludeTile(pair.Key, lookupTile))
                    yield return new ObjectTarget(this.GameHelper, pair.Value, pair.Key, this.Reflection, () => this.BuildSubject(pair.Value, ObjectContext.World, location, knownQuality: false));
            }

            // furniture
            foreach (var furniture in location.furniture)
            {
                Vector2 entityTile = furniture.TileLocation;
                if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    yield return new ObjectTarget(this.GameHelper, furniture, entityTile, this.Reflection, () => this.BuildSubject(furniture, ObjectContext.Inventory, location));
            }

            // crops
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
            {
                Vector2 entityTile = pair.Key;
                TerrainFeature feature = pair.Value;

                if (feature is HoeDirt dirt && dirt.crop != null && this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    yield return new CropTarget(this.GameHelper, dirt, entityTile, this.Reflection, this.JsonAssets, () => this.BuildSubject(dirt.crop, ObjectContext.World, dirt));
            }
        }

        /// <inheritdoc />
        public override ISubject GetSubject(IClickableMenu menu, int cursorX, int cursorY)
        {
            IClickableMenu targetMenu = (menu as GameMenu)?.GetCurrentPage() ?? menu;
            switch (targetMenu)
            {
                /****
                ** Inventory
                ****/
                // chest
                case MenuWithInventory inventoryMenu when menu is not (FieldOfficeMenu or TailoringMenu):
                    {
                        Item item = Game1.player.CursorSlotItem ?? inventoryMenu.heldItem ?? inventoryMenu.hoveredItem;
                        if (item != null)
                            return this.BuildSubject(item, ObjectContext.Inventory, null);
                    }
                    break;

                // inventory
                case InventoryPage inventory:
                    {
                        Item item = Game1.player.CursorSlotItem ?? this.Reflection.GetField<Item>(inventory, "hoveredItem").GetValue();
                        if (item != null)
                            return this.BuildSubject(item, ObjectContext.Inventory, null);
                    }
                    break;

                // shop
                case ShopMenu shopMenu:
                    // hovered item
                    {
                        ISalable entry = shopMenu.hoveredItem;
                        if (entry is Item item)
                            return this.BuildSubject(item, ObjectContext.Inventory, null);
                        if (entry is MovieConcession snack)
                            return new MovieSnackSubject(this.GameHelper, snack);
                    }

                    // inventory
                    return this.GetSubject(shopMenu.inventory, cursorX, cursorY);

                // tailoring
                case TailoringMenu tailoringMenu:
                    // cloth or spool slot
                    foreach (var slot in new[] { tailoringMenu.leftIngredientSpot, tailoringMenu.rightIngredientSpot, tailoringMenu.craftResultDisplay })
                    {
                        if (slot.containsPoint(cursorX, cursorY) && slot.item != null)
                            return this.BuildSubject(slot.item, ObjectContext.Inventory, null);
                    }

                    // inventory
                    return this.GetSubject(tailoringMenu.inventory, cursorX, cursorY);

                // toolbar
                case Toolbar _:
                    {
                        // find hovered slot
                        List<ClickableComponent> slots = this.Reflection.GetField<List<ClickableComponent>>(menu, "buttons").GetValue();
                        ClickableComponent hoveredSlot = slots.FirstOrDefault(slot => slot.containsPoint(cursorX, cursorY));
                        if (hoveredSlot == null)
                            return null;

                        // get inventory index
                        int index = slots.IndexOf(hoveredSlot);
                        if (index < 0 || index > Game1.player.Items.Count - 1)
                            return null;

                        // get hovered item
                        Item item = Game1.player.Items[index];
                        if (item != null)
                            return this.BuildSubject(item, ObjectContext.Inventory, null);
                    }
                    break;

                // inventory submenu
                case InventoryMenu inventory:
                    {
                        foreach (ClickableComponent slot in inventory.inventory)
                        {
                            if (slot.containsPoint(cursorX, cursorY))
                            {
                                if (int.TryParse(slot.name, out int index) && inventory.actualInventory.TryGetIndex(index, out Item item) && item != null)
                                    return this.BuildSubject(item, ObjectContext.Inventory, null);
                                break;
                            }
                        }
                    }
                    break;


                /****
                ** GameMenu
                ****/
                // collections menu
                // derived from CollectionsPage::performHoverAction
                case CollectionsPage collectionsTab:
                    {
                        int currentTab = collectionsTab.currentTab;
                        if (currentTab is CollectionsPage.achievementsTab or CollectionsPage.secretNotesTab or CollectionsPage.lettersTab)
                            break;

                        int currentPage = collectionsTab.currentPage;

                        foreach (ClickableTextureComponent component in collectionsTab.collections[currentTab][currentPage])
                        {
                            if (component.containsPoint(cursorX, cursorY))
                            {
                                int itemID = Convert.ToInt32(component.name.Split(' ')[0]);
                                SObject obj = this.GameHelper.GetObjectBySpriteIndex(itemID);
                                return this.BuildSubject(obj, ObjectContext.Inventory, null, knownQuality: false);
                            }
                        }
                    }
                    break;

                // crafting menu
                case CraftingPage crafting:
                    {
                        // player inventory item
                        {
                            Item item = this.Reflection.GetField<Item>(crafting, "hoverItem").GetValue();
                            if (item != null)
                                return this.BuildSubject(item, ObjectContext.Inventory, null);
                        }

                        // learned crafting recipe
                        {
                            CraftingRecipe recipe = this.Reflection.GetField<CraftingRecipe>(crafting, "hoverRecipe").GetValue();
                            if (recipe != null)
                                return this.BuildSubject(recipe.createItem(), ObjectContext.Inventory, null);
                        }

                        // undiscovered crafting recipe
                        {
                            int currentCraftingPage = this.Reflection.GetField<int>(crafting, "currentCraftingPage").GetValue();
                            if (crafting.pagesOfCraftingRecipes.TryGetIndex(currentCraftingPage, out var page))
                            {
                                foreach (var recipeSlot in page)
                                {
                                    if (!recipeSlot.Key.containsPoint(cursorX, cursorY))
                                        continue;

                                    var item = recipeSlot.Value?.createItem();
                                    if (item != null)
                                        return this.BuildSubject(recipeSlot.Value.createItem(), ObjectContext.Inventory, null);
                                    break;
                                }
                            }
                        }
                    }
                    break;

                // profile tab
                case ProfileMenu profileMenu:
                    {
                        // hovered item
                        Item item = profileMenu.hoveredItem;
                        if (item != null)
                            return this.BuildSubject(item, ObjectContext.Inventory, null);
                    }
                    break;

                /****
                ** Other menus
                ****/
                // community center bundle menu
                case JunimoNoteMenu bundleMenu:
                    {
                        // hovered inventory item
                        {
                            Item item = this.Reflection.GetField<Item>(menu, "hoveredItem").GetValue();
                            if (item != null)
                                return this.BuildSubject(item, ObjectContext.Inventory, null);
                        }

                        // list of required ingredients
                        for (int i = 0; i < bundleMenu.ingredientList.Count; i++)
                        {
                            if (bundleMenu.ingredientList[i].containsPoint(cursorX, cursorY))
                            {
                                Bundle bundle = this.Reflection.GetField<Bundle>(bundleMenu, "currentPageBundle").GetValue();
                                var ingredient = bundle.ingredients[i];
                                var item = this.GameHelper.GetObjectBySpriteIndex(ingredient.index, ingredient.stack);
                                item.Quality = ingredient.quality;
                                return this.BuildSubject(item, ObjectContext.Inventory, null);
                            }
                        }

                        // list of submitted ingredients
                        foreach (ClickableTextureComponent slot in bundleMenu.ingredientSlots)
                        {
                            if (slot.item != null && slot.containsPoint(cursorX, cursorY))
                                return this.BuildSubject(slot.item, ObjectContext.Inventory, null);
                        }
                    }
                    break;

                // Fern Islands field office menu
                case FieldOfficeMenu fieldOfficeMenu:
                    {
                        ClickableComponent slot = fieldOfficeMenu.pieceHolders.FirstOrDefault(p => p.containsPoint(cursorX, cursorY));
                        if (slot != null)
                        {
                            // donated item
                            if (slot.item != null)
                                return this.BuildSubject(slot.item, ObjectContext.Inventory, null, knownQuality: false);

                            // empty slot
                            if (int.TryParse(slot.label, out int itemId))
                                return this.BuildSubject(this.GameHelper.GetObjectBySpriteIndex(itemId), ObjectContext.Inventory, null, knownQuality: false);
                        }
                    }
                    break;

                /****
                ** Convention (for mod support)
                ****/
                default:
                    {
                        Item item = this.Reflection.GetField<Item>(menu, "HoveredItem", required: false)?.GetValue(); // ChestsAnywhere
                        if (item != null)
                            return this.BuildSubject(item, ObjectContext.Inventory, null);
                    }
                    break;
            }

            return null;
        }

        /// <inheritdoc />
        public override IEnumerable<ISubject> GetSearchSubjects()
        {
            foreach (SearchableItem item in this.ItemRepository.GetAll())
                yield return this.BuildSubject(item.Item, ObjectContext.World, null, knownQuality: false);
        }

        /// <inheritdoc />
        public override ISubject GetSubjectFor(object entity, GameLocation location)
        {
            return entity is Item item
                ? this.BuildSubject(item, ObjectContext.Any, location, knownQuality: false)
                : null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Build an item subject.</summary>
        /// <param name="target">The target instance.</param>
        /// <param name="context">The context of the object being looked up.</param>
        /// <param name="location">The location containing the item, if applicable.</param>
        /// <param name="knownQuality">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        private ISubject BuildSubject(Item target, ObjectContext context, GameLocation location, bool knownQuality = true)
        {
            var config = this.Config();
            return new ItemSubject(
                codex: this.Codex,
                gameHelper: this.GameHelper,
                progressionMode: config.ProgressionMode,
                highlightUnrevealedGiftTastes: config.HighlightUnrevealedGiftTastes,
                showAllGiftTastes: config.ShowAllGiftTastes,
                item: target,
                context: context,
                knownQuality: knownQuality,
                location: location,
                getCropSubject: this.BuildSubject
            );
        }

        /// <summary>Build a crop subject.</summary>
        /// <param name="target">The target instance.</param>
        /// <param name="context">The context of the object being looked up.</param>
        /// <param name="dirt">The dirt containing the crop, if applicable.</param>
        private ISubject BuildSubject(Crop target, ObjectContext context, HoeDirt dirt)
        {
            int indexOfHarvest = target.indexOfHarvest.Value;
            if (indexOfHarvest == 0 && target.forageCrop.Value)
            {
                indexOfHarvest = target.whichForageCrop.Value switch
                {
                    Crop.forageCrop_ginger => 829,
                    Crop.forageCrop_springOnion => 399,
                    _ => indexOfHarvest
                };
            }

            ModConfig config = this.Config();
            return new ItemSubject(
                codex: this.Codex,
                gameHelper: this.GameHelper,
                progressionMode: config.ProgressionMode,
                highlightUnrevealedGiftTastes: config.HighlightUnrevealedGiftTastes,
                showAllGiftTastes: config.ShowAllGiftTastes,
                item: this.GameHelper.GetObjectBySpriteIndex(indexOfHarvest),
                context: context,
                location: dirt.currentLocation,
                knownQuality: false,
                getCropSubject: this.BuildSubject,
                fromDirt: dirt
            );
        }
    }
}
