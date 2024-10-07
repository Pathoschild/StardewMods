using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Items;
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
        private readonly ItemRepository ItemRepository = new();

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
        public ItemLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, Func<ModConfig> config, ISubjectRegistry codex)
            : base(reflection, gameHelper)
        {
            this.Config = config;
            this.Codex = codex;
        }

        /// <inheritdoc />
        public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
        {
            // map objects
            foreach ((Vector2 tile, SObject obj) in location.objects.Pairs)
            {
                if (location is IslandShrine && obj is ItemPedestal)
                    continue; // part of the Fern Islands shrine puzzle, which is handled by the tile lookup provider

                if (this.GameHelper.CouldSpriteOccludeTile(tile, lookupTile))
                    yield return new ObjectTarget(this.GameHelper, obj, tile, () => this.BuildSubject(obj, ObjectContext.World, location, knownQuality: false));
            }

            // furniture
            foreach (Furniture furniture in location.furniture)
            {
                Vector2 entityTile = furniture.TileLocation;
                if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    yield return new ObjectTarget(this.GameHelper, furniture, entityTile, () => this.BuildSubject(furniture, ObjectContext.Inventory, location));
            }

            // crops
            foreach ((Vector2 tile, TerrainFeature feature) in location.terrainFeatures.Pairs)
            {
                if (feature is HoeDirt { crop: not null } dirt && this.GameHelper.CouldSpriteOccludeTile(tile, lookupTile))
                    yield return new CropTarget(this.GameHelper, dirt, tile, () => this.BuildSubject(dirt.crop, ObjectContext.World, dirt));
            }
        }

        /// <inheritdoc />
        public override ISubject? GetSubject(IClickableMenu menu, int cursorX, int cursorY)
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
                        Item? item = Game1.player.CursorSlotItem ?? inventoryMenu.heldItem ?? inventoryMenu.hoveredItem;
                        if (item != null)
                            return this.BuildSubject(item, ObjectContext.Inventory, null);
                    }
                    break;

                // inventory
                case InventoryPage inventory:
                    {
                        Item? item = Game1.player.CursorSlotItem ?? inventory.hoveredItem;
                        if (item != null)
                            return this.BuildSubject(item, ObjectContext.Inventory, null);
                    }
                    break;

                // shop
                case ShopMenu shopMenu:
                    // hovered item
                    {
                        ISalable? entry = shopMenu.hoveredItem;
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
                    foreach (ClickableTextureComponent slot in new[] { tailoringMenu.leftIngredientSpot, tailoringMenu.rightIngredientSpot, tailoringMenu.craftResultDisplay })
                    {
                        if (slot.containsPoint(cursorX, cursorY) && slot.item != null)
                            return this.BuildSubject(slot.item, ObjectContext.Inventory, null);
                    }

                    // inventory
                    return this.GetSubject(tailoringMenu.inventory, cursorX, cursorY);

                // toolbar
                case Toolbar toolbar:
                    {
                        // find hovered slot
                        ClickableComponent? hoveredSlot = toolbar.buttons.FirstOrDefault(slot => slot.containsPoint(cursorX, cursorY));
                        if (hoveredSlot == null)
                            return null;

                        // get inventory index
                        int index = toolbar.buttons.IndexOf(hoveredSlot);
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
                                if (int.TryParse(slot.name, out int index) && inventory.actualInventory.TryGetIndex(index, out Item? item) && item != null)
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
                                string itemID = component.name.Split(' ')[0];
                                Item item = ItemRegistry.Create(itemID);
                                return this.BuildSubject(item, ObjectContext.Inventory, null, knownQuality: false);
                            }
                        }
                    }
                    break;

                // crafting menu
                case CraftingPage crafting:
                    {
                        // player inventory item
                        {
                            Item? item = crafting.hoverItem;
                            if (item != null)
                                return this.BuildSubject(item, ObjectContext.Inventory, null);
                        }

                        // learned crafting recipe
                        {
                            CraftingRecipe? recipe = crafting.hoverRecipe;
                            if (recipe != null)
                                return this.BuildSubject(recipe.createItem(), ObjectContext.Inventory, null);
                        }

                        // undiscovered crafting recipe
                        {
                            if (crafting.pagesOfCraftingRecipes.TryGetIndex(crafting.currentCraftingPage, out Dictionary<ClickableTextureComponent, CraftingRecipe?>? page))
                            {
                                foreach ((ClickableTextureComponent sprite, CraftingRecipe? recipe) in page)
                                {
                                    if (!sprite.containsPoint(cursorX, cursorY))
                                        continue;

                                    Item? item = recipe?.createItem();
                                    if (item != null)
                                        return this.BuildSubject(recipe!.createItem(), ObjectContext.Inventory, null);
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
                            Item? item = bundleMenu.hoveredItem;
                            if (item != null)
                                return this.BuildSubject(item, ObjectContext.Inventory, null);
                        }

                        // list of required ingredients
                        for (int i = 0; i < bundleMenu.ingredientList.Count; i++)
                        {
                            if (bundleMenu.ingredientList[i].containsPoint(cursorX, cursorY))
                            {
                                Bundle bundle = bundleMenu.currentPageBundle;
                                var ingredient = bundle.ingredients[i];
                                Item? item = ItemRegistry.Create(ingredient.id, ingredient.stack);
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
                        ClickableComponent? slot = fieldOfficeMenu.pieceHolders.FirstOrDefault(p => p.containsPoint(cursorX, cursorY));
                        if (slot != null)
                        {
                            // donated item
                            if (slot.item != null)
                                return this.BuildSubject(slot.item, ObjectContext.Inventory, null, knownQuality: false);

                            // empty slot
                            if (CommonHelper.IsItemId(slot.label))
                                return this.BuildSubject(ItemRegistry.Create(slot.label), ObjectContext.Inventory, null, knownQuality: false);
                        }
                    }
                    break;

                /****
                ** By convention (for mod support)
                ****/
                default:
                    {
                        Item? item =
                            this.Reflection.GetField<Item?>(targetMenu, "hoveredItem", required: false)?.GetValue()
                            ?? this.Reflection.GetField<Item?>(targetMenu, "HoveredItem", required: false)?.GetValue();
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
        public override ISubject? GetSubjectFor(object entity, GameLocation? location)
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
        private ISubject BuildSubject(Item target, ObjectContext context, GameLocation? location, bool knownQuality = true)
        {
            var config = this.Config();
            return new ItemSubject(
                codex: this.Codex,
                gameHelper: this.GameHelper,
                showUnknownGiftTastes: config.ShowUnknownGiftTastes,
                showUnknownRecipes: config.ShowUnknownRecipes,
                showInvalidRecipes: config.ShowInvalidRecipes,
                highlightUnrevealedGiftTastes: config.HighlightUnrevealedGiftTastes,
                showGiftTastes: config.ShowGiftTastes,
                collapseFieldsConfig: config.CollapseLargeFields,
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
        private ISubject BuildSubject(Crop target, ObjectContext context, HoeDirt? dirt)
        {
            string indexOfHarvest = target.indexOfHarvest.Value;
            if (!CommonHelper.IsItemId(indexOfHarvest, allowZero: false) && target.forageCrop.Value)
            {
                if (target.whichForageCrop.Value == Crop.forageCrop_ginger.ToString())
                    indexOfHarvest = "829";
                else if (target.whichForageCrop.Value == Crop.forageCrop_springOnion.ToString())
                    indexOfHarvest = "399";
            }

            ModConfig config = this.Config();
            return new ItemSubject(
                codex: this.Codex,
                gameHelper: this.GameHelper,
                showUnknownGiftTastes: config.ShowUnknownGiftTastes,
                showUnknownRecipes: config.ShowUnknownRecipes,
                showInvalidRecipes: config.ShowInvalidRecipes,
                highlightUnrevealedGiftTastes: config.HighlightUnrevealedGiftTastes,
                showGiftTastes: config.ShowGiftTastes,
                collapseFieldsConfig: config.CollapseLargeFields,
                item: ItemRegistry.Create(indexOfHarvest),
                context: context,
                location: dirt?.Location,
                knownQuality: false,
                getCropSubject: this.BuildSubject,
                fromDirt: dirt
            );
        }
    }
}
