using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
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
        private readonly ModConfig Config;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="config">The mod configuration.</param>
        /// <param name="jsonAssets">The Json Assets API.</param>
        public ItemLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, ModConfig config, JsonAssetsIntegration jsonAssets)
            : base(reflection, gameHelper)
        {
            this.Config = config;
            this.JsonAssets = jsonAssets;
        }

        public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
        {
            // map objects
            foreach (KeyValuePair<Vector2, SObject> pair in location.objects.Pairs)
            {
                if (location is IslandShrine && pair.Value is ItemPedestal)
                    continue; // part of the Fern Islands shrine puzzle, which is handled by the tile lookup provider

                if (this.GameHelper.CouldSpriteOccludeTile(pair.Key, lookupTile))
                    yield return new ObjectTarget(this.GameHelper, pair.Value, pair.Key, this.Reflection, () => this.BuildSubject(pair.Value, ObjectContext.World, knownQuality: false));
            }

            // furniture
            foreach (var furniture in location.furniture)
            {
                Vector2 entityTile = furniture.TileLocation;
                if (this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    yield return new ObjectTarget(this.GameHelper, furniture, entityTile, this.Reflection, () => this.BuildSubject(furniture, ObjectContext.Inventory));
            }

            // crops
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
            {
                Vector2 entityTile = pair.Key;
                TerrainFeature feature = pair.Value;

                if (feature is HoeDirt dirt && dirt.crop != null && this.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
                    yield return new CropTarget(this.GameHelper, dirt, entityTile, this.Reflection, this.JsonAssets, () => this.BuildSubject(dirt.crop, ObjectContext.World));
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
                case MenuWithInventory inventoryMenu when !(menu is FieldOfficeMenu):
                    {
                        Item item = Game1.player.CursorSlotItem ?? inventoryMenu.heldItem ?? inventoryMenu.hoveredItem;
                        if (item != null)
                            return this.BuildSubject(item, ObjectContext.Inventory);
                    }
                    break;

                // inventory
                case InventoryPage inventory:
                    {
                        Item item = Game1.player.CursorSlotItem ?? this.Reflection.GetField<Item>(inventory, "hoveredItem").GetValue();
                        if (item != null)
                            return this.BuildSubject(item, ObjectContext.Inventory);
                    }
                    break;

                // shop
                case ShopMenu shopMenu:
                    {
                        ISalable entry = shopMenu.hoveredItem;
                        if (entry is Item item)
                            return this.BuildSubject(item, ObjectContext.Inventory);
                        if (entry is MovieConcession snack)
                            return new MovieSnackSubject(this.GameHelper, snack);
                    }
                    break;

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
                            return this.BuildSubject(item, ObjectContext.Inventory);
                    }
                    break;


                /****
                ** GameMenu
                ****/
                // collections menu
                // derived from CollectionsPage::performHoverAction
                case CollectionsPage collectionsTab:
                    {
                        int currentTab = this.Reflection.GetField<int>(collectionsTab, "currentTab").GetValue();
                        if (currentTab == CollectionsPage.achievementsTab || currentTab == CollectionsPage.secretNotesTab || currentTab == CollectionsPage.lettersTab)
                            break;

                        int currentPage = this.Reflection.GetField<int>(collectionsTab, "currentPage").GetValue();

                        foreach (ClickableTextureComponent component in collectionsTab.collections[currentTab][currentPage])
                        {
                            if (component.containsPoint(cursorX, cursorY))
                            {
                                int itemID = Convert.ToInt32(component.name.Split(' ')[0]);
                                SObject obj = new SObject(itemID, 1);
                                return this.BuildSubject(obj, ObjectContext.Inventory, knownQuality: false);
                            }
                        }
                    }
                    break;

                // crafting menu
                case CraftingPage crafting:
                    {
                        // player inventory item
                        Item item = this.Reflection.GetField<Item>(crafting, "hoverItem").GetValue();
                        if (item != null)
                            return this.BuildSubject(item, ObjectContext.Inventory);

                        // crafting recipe
                        CraftingRecipe recipe = this.Reflection.GetField<CraftingRecipe>(crafting, "hoverRecipe").GetValue();
                        if (recipe != null)
                            return this.BuildSubject(recipe.createItem(), ObjectContext.Inventory);
                    }
                    break;

                // profile tab
                case ProfileMenu profileMenu:
                    {
                        // hovered item
                        Item item = profileMenu.hoveredItem;
                        if (item != null)
                            return this.BuildSubject(item, ObjectContext.Inventory);
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
                                return this.BuildSubject(item, ObjectContext.Inventory);
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
                                return this.BuildSubject(item, ObjectContext.Inventory);
                            }
                        }

                        // list of submitted ingredients
                        foreach (ClickableTextureComponent slot in bundleMenu.ingredientSlots)
                        {
                            if (slot.item != null && slot.containsPoint(cursorX, cursorY))
                                return this.BuildSubject(slot.item, ObjectContext.Inventory);
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
                                return this.BuildSubject(slot.item, ObjectContext.Inventory, knownQuality: false);

                            // empty slot
                            if (int.TryParse(slot.label, out int itemId))
                                return this.BuildSubject(new SObject(itemId, 1), ObjectContext.Inventory, knownQuality: false);
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
                            return this.BuildSubject(item, ObjectContext.Inventory);
                    }
                    break;
            }

            return null;
        }

        /// <inheritdoc />
        public override IEnumerable<ISubject> GetSearchSubjects()
        {
            foreach (SearchableItem item in this.ItemRepository.GetAll())
                yield return this.BuildSubject(item.Item, ObjectContext.World, knownQuality: false);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Build an item subject.</summary>
        /// <param name="target">The target instance.</param>
        /// <param name="context">The context of the object being looked up.</param>
        /// <param name="knownQuality">Whether the item quality is known. This is <c>true</c> for an inventory item, <c>false</c> for a map object.</param>
        private ISubject BuildSubject(Item target, ObjectContext context, bool knownQuality = true)
        {
            return new ItemSubject(this.GameHelper, this.Config.ProgressionMode, this.Config.HighlightUnrevealedGiftTastes, target, context, knownQuality, this.BuildSubject);
        }

        /// <summary>Build a crop subject.</summary>
        /// <param name="target">The target instance.</param>
        /// <param name="context">The context of the object being looked up.</param>
        private ISubject BuildSubject(Crop target, ObjectContext context)
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

            return new ItemSubject(this.GameHelper, this.Config.ProgressionMode, this.Config.HighlightUnrevealedGiftTastes, this.GameHelper.GetObjectBySpriteIndex(indexOfHarvest), context, getCropSubject: this.BuildSubject, knownQuality: false, fromCrop: target);
        }
    }
}
