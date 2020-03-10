using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Integrations.JsonAssets;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Subjects;
using Pathoschild.Stardew.LookupAnything.Framework.Targets;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>Finds and analyzes lookup targets in the world.</summary>
    internal class TargetFactory
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>Provides utility methods for interacting with the game code.</summary>
        private readonly GameHelper GameHelper;

        /// <summary>The Json Assets API.</summary>
        private readonly JsonAssetsIntegration JsonAssets;

        /// <summary>Constructs subjects for target values.</summary>
        private readonly SubjectFactory Codex;


        /*********
        ** Public methods
        *********/
        /****
        ** Constructors
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="gameHelper">Provides utility methods for interacting with the game code.</param>
        /// <param name="jsonAssets">The Json Assets API.</param>
        /// <param name="codex">Constructs subjects for target values.</param>
        public TargetFactory(IReflectionHelper reflection, GameHelper gameHelper, JsonAssetsIntegration jsonAssets, SubjectFactory codex)
        {
            this.Reflection = reflection;
            this.GameHelper = gameHelper;
            this.JsonAssets = jsonAssets;
            this.GameHelper = gameHelper;
            this.Codex = codex;
        }

        /****
        ** Targets
        ****/
        /// <summary>Get all potential lookup targets in the current location.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="originTile">The tile from which to search for targets.</param>
        /// <param name="includeMapTile">Whether to allow matching the map tile itself.</param>
        /// <remarks>Related to <see cref="SubjectFactory.GetSearchSubjects"/>.</remarks>
        public IEnumerable<ITarget> GetNearbyTargets(GameLocation location, Vector2 originTile, bool includeMapTile)
        {
            // NPCs
            foreach (NPC npc in location.characters)
            {
                if (!this.GameHelper.CouldSpriteOccludeTile(npc.getTileLocation(), originTile))
                    continue;

                yield return new CharacterTarget(this.GameHelper, this.Codex.GetSubjectType(npc), npc, npc.getTileLocation(), this.Reflection);
            }

            // animals
            foreach (FarmAnimal animal in (location as Farm)?.animals.Values ?? (location as AnimalHouse)?.animals.Values ?? Enumerable.Empty<FarmAnimal>())
            {
                if (!this.GameHelper.CouldSpriteOccludeTile(animal.getTileLocation(), originTile))
                    continue;

                yield return new FarmAnimalTarget(this.GameHelper, animal, animal.getTileLocation());
            }

            // map objects
            foreach (KeyValuePair<Vector2, SObject> pair in location.objects.Pairs)
            {
                Vector2 spriteTile = pair.Key;
                SObject obj = pair.Value;

                if (!this.GameHelper.CouldSpriteOccludeTile(spriteTile, originTile))
                    continue;

                yield return new ObjectTarget(this.GameHelper, obj, spriteTile, this.Reflection);
            }

            // furniture
            if (location is DecoratableLocation decoratableLocation)
            {
                foreach (var furniture in decoratableLocation.furniture)
                    yield return new ObjectTarget(this.GameHelper, furniture, furniture.TileLocation, this.Reflection);
            }

            // terrain features
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in location.terrainFeatures.Pairs)
            {
                Vector2 spriteTile = pair.Key;
                TerrainFeature feature = pair.Value;

                if (!this.GameHelper.CouldSpriteOccludeTile(spriteTile, originTile))
                    continue;

                switch (feature)
                {
                    case Bush bush: // planted bush
                        yield return new BushTarget(this.GameHelper, bush, this.Reflection);
                        break;

                    case HoeDirt dirt when dirt.crop != null:
                        yield return new CropTarget(this.GameHelper, dirt, spriteTile, this.Reflection, this.JsonAssets);
                        break;

                    case FruitTree fruitTree:
                        if (this.Reflection.GetField<float>(fruitTree, "alpha").GetValue() < 0.8f)
                            continue; // ignore when tree is faded out (so player can lookup things behind it)
                        yield return new FruitTreeTarget(this.GameHelper, fruitTree, this.JsonAssets, spriteTile);
                        break;

                    case Tree wildTree:
                        if (this.Reflection.GetField<float>(feature, "alpha").GetValue() < 0.8f)
                            continue; // ignore when tree is faded out (so player can lookup things behind it)
                        yield return new TreeTarget(this.GameHelper, wildTree, spriteTile, this.Reflection);
                        break;

                    default:
                        yield return new UnknownTarget(this.GameHelper, feature, spriteTile);
                        break;
                }
            }

            // large terrain features
            foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
            {
                Vector2 spriteTile = feature.tilePosition.Value;

                if (!this.GameHelper.CouldSpriteOccludeTile(spriteTile, originTile))
                    continue;

                switch (feature)
                {
                    case Bush bush: // wild bush
                        yield return new BushTarget(this.GameHelper, bush, this.Reflection);
                        break;
                }
            }

            // players
            foreach (Farmer farmer in location.farmers)
            {
                if (!this.GameHelper.CouldSpriteOccludeTile(farmer.getTileLocation(), originTile))
                    continue;

                yield return new FarmerTarget(this.GameHelper, farmer);
            }

            // buildings
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    if (!this.GameHelper.CouldSpriteOccludeTile(new Vector2(building.tileX.Value, building.tileY.Value + building.tilesHigh.Value), originTile, Constant.MaxBuildingTargetSpriteSize))
                        continue;

                    yield return new BuildingTarget(this.GameHelper, building);
                }
            }

            // tiles
            if (includeMapTile)
                yield return new TileTarget(this.GameHelper, originTile);
        }

        /// <summary>Get the target on the specified tile.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="includeMapTile">Whether to allow matching the map tile itself.</param>
        public ITarget GetTargetFromTile(GameLocation location, Vector2 tile, bool includeMapTile)
        {
            return (
                from target in this.GetNearbyTargets(location, tile, includeMapTile)
                where
                    target.Type != SubjectType.Unknown
                    && target.IsAtTile(tile)
                select target
            ).FirstOrDefault();
        }

        /// <summary>Get the target at the specified coordinate.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative pixel coordinate to search.</param>
        /// <param name="includeMapTile">Whether to allow matching the map tile itself.</param>
        public ITarget GetTargetFromScreenCoordinate(GameLocation location, Vector2 tile, Vector2 position, bool includeMapTile)
        {
            // get target sprites which might overlap cursor position (first approximation)
            Rectangle tileArea = this.GameHelper.GetScreenCoordinatesFromTile(tile);
            var candidates = (
                from target in this.GetNearbyTargets(location, tile, includeMapTile)
                let spriteArea = target.GetWorldArea()
                let isAtTile = target.IsAtTile(tile)
                where
                    target.Type != SubjectType.Unknown
                    && (isAtTile || spriteArea.Intersects(tileArea))
                orderby
                    target.Type != SubjectType.Tile ? 0 : 1, // Tiles are always under anything else.
                    spriteArea.Y descending,                // A higher Y value is closer to the foreground, and will occlude any sprites behind it.
                    spriteArea.X ascending                  // If two sprites at the same Y coordinate overlap, assume the left sprite occludes the right.

                select new { target, spriteArea, isAtTile }
            ).ToArray();

            // choose best match
            return
                candidates.FirstOrDefault(p => p.target.SpriteIntersectsPixel(tile, position, p.spriteArea))?.target // sprite pixel under cursor
                ?? candidates.FirstOrDefault(p => p.isAtTile)?.target; // tile under cursor
        }

        /****
        ** Subjects
        ****/
        /// <summary>Get metadata for a Stardew object at the specified position.</summary>
        /// <param name="player">The player performing the lookup.</param>
        /// <param name="location">The current location.</param>
        /// <param name="includeMapTile">Whether to allow matching the map tile itself.</param>
        /// <param name="hasCursor">Whether the player has a visible cursor.</param>
        public ISubject GetSubjectFrom(Farmer player, GameLocation location, bool includeMapTile, bool hasCursor)
        {
            // get target
            ITarget target;
            if (hasCursor)
                target = this.GetTargetFromScreenCoordinate(location, Game1.currentCursorTile, this.GameHelper.GetScreenCoordinatesFromCursor(), includeMapTile);
            else
            {
                Vector2 tile = this.GetFacingTile(player);
                target = this.GetTargetFromTile(location, tile, includeMapTile);
            }

            // get subject
            return target != null
                ? this.GetSubjectFrom(target)
                : null;
        }

        /// <summary>Get metadata for a Stardew object represented by a target.</summary>
        /// <param name="target">The target.</param>
        public ISubject GetSubjectFrom(ITarget target)
        {
            switch (target.Type)
            {
                // NPC
                case SubjectType.Horse:
                case SubjectType.Junimo:
                case SubjectType.Pet:
                case SubjectType.Monster:
                case SubjectType.Villager:
                    return this.Codex.GetCharacter(target.GetValue<NPC>());

                // player
                case SubjectType.Farmer:
                    return this.Codex.GetPlayer(target.GetValue<Farmer>());

                // animal
                case SubjectType.FarmAnimal:
                    return this.Codex.GetFarmAnimal(target.GetValue<FarmAnimal>());

                // crop
                case SubjectType.Crop:
                    return this.Codex.GetCrop(target.GetValue<HoeDirt>().crop, ObjectContext.World);

                // fruit tree
                case SubjectType.FruitTree:
                    return this.Codex.GetFruitTree(target.GetValue<FruitTree>(), target.GetTile());

                // wild tree
                case SubjectType.WildTree:
                    return this.Codex.GetWildTree(target.GetValue<Tree>(), target.GetTile());

                // inventory item
                case SubjectType.InventoryItem:
                case SubjectType.Object:
                    return this.Codex.GetItem(target.GetValue<Item>(), target.Type == SubjectType.InventoryItem ? ObjectContext.Inventory : ObjectContext.World, knownQuality: false);

                // building
                case SubjectType.Building:
                    return this.Codex.GetBuilding(target.GetValue<Building>(), target.GetSpritesheetArea());

                case SubjectType.Bush:
                    return this.Codex.GetBush(target.GetValue<Bush>());

                // tile
                case SubjectType.Tile:
                    return this.Codex.GetTile(Game1.currentLocation, target.GetValue<Vector2>());
            }

            return null;
        }

        /// <summary>Get metadata for a menu element at the specified position.</summary>
        /// <param name="menu">The active menu.</param>
        /// <param name="cursorPos">The cursor's viewport-relative coordinates.</param>
        public ISubject GetSubjectFrom(IClickableMenu menu, Vector2 cursorPos)
        {
            IClickableMenu targetMenu =
                (menu as GameMenu)?.GetCurrentPage()
                ?? menu;

            switch (targetMenu)
            {
                // calendar
                case Billboard billboard:
                    {
                        // get target day
                        int selectedDay = -1;
                        for (int i = 0; i < billboard.calendarDays.Count; i++)
                        {
                            if (billboard.calendarDays[i].containsPoint((int)cursorPos.X, (int)cursorPos.Y))
                            {
                                selectedDay = i + 1;
                                break;
                            }
                        }
                        if (selectedDay == -1)
                            return null;

                        // get villager with a birthday on that date
                        NPC target = this.GameHelper.GetAllCharacters().FirstOrDefault(p => p.Birthday_Season == Game1.currentSeason && p.Birthday_Day == selectedDay);
                        if (target != null)
                            return this.Codex.GetCharacter(target);
                    }
                    break;

                // chest
                case MenuWithInventory inventoryMenu:
                    {
                        Item item = Game1.player.CursorSlotItem ?? inventoryMenu.heldItem ?? inventoryMenu.hoveredItem;
                        if (item != null)
                            return this.Codex.GetItem(item, ObjectContext.Inventory);
                    }
                    break;

                // inventory
                case InventoryPage inventory:
                    {
                        Item item = Game1.player.CursorSlotItem ?? this.Reflection.GetField<Item>(inventory, "hoveredItem").GetValue();
                        if (item != null)
                            return this.Codex.GetItem(item, ObjectContext.Inventory);
                    }
                    break;

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
                            if (component.containsPoint((int)cursorPos.X, (int)cursorPos.Y))
                            {
                                int itemID = Convert.ToInt32(component.name.Split(' ')[0]);
                                SObject obj = new SObject(itemID, 1);
                                return this.Codex.GetItem(obj, ObjectContext.Inventory, knownQuality: false);
                            }
                        }
                    }
                    break;

                // cooking or crafting menu
                case CraftingPage crafting:
                    {
                        // player inventory item
                        Item item = this.Reflection.GetField<Item>(crafting, "hoverItem").GetValue();
                        if (item != null)
                            return this.Codex.GetItem(item, ObjectContext.Inventory);

                        // crafting recipe
                        CraftingRecipe recipe = this.Reflection.GetField<CraftingRecipe>(crafting, "hoverRecipe").GetValue();
                        if (recipe != null)
                            return this.Codex.GetItem(recipe.createItem(), ObjectContext.Inventory);
                    }
                    break;

                // social tab
                case SocialPage socialPage:
                    {
                        // get villagers on current page
                        int scrollOffset = this.Reflection.GetField<int>(socialPage, "slotPosition").GetValue();
                        ClickableTextureComponent[] entries = this.Reflection
                            .GetField<List<ClickableTextureComponent>>(socialPage, "sprites")
                            .GetValue()
                            .Skip(scrollOffset)
                            .ToArray();

                        // find hovered villager
                        ClickableTextureComponent entry = entries.FirstOrDefault(p => p.containsPoint((int)cursorPos.X, (int)cursorPos.Y));
                        if (entry != null)
                        {
                            int index = Array.IndexOf(entries, entry) + scrollOffset;
                            object socialID = this.Reflection.GetField<List<object>>(socialPage, "names").GetValue()[index];
                            if (socialID is long playerID)
                            {
                                Farmer player = Game1.getFarmer(playerID);
                                return this.Codex.GetPlayer(player);
                            }
                            else if (socialID is string villagerName)
                            {
                                NPC npc = this.GameHelper.GetAllCharacters().FirstOrDefault(p => p.isVillager() && p.Name == villagerName);
                                if (npc != null)
                                    return this.Codex.GetCharacter(npc);
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
                            return this.Codex.GetItem(item, ObjectContext.Inventory);

                        // NPC
                        if (profileMenu.GetCharacter() is NPC npc)
                            return this.Codex.GetCharacter(npc);
                        break;
                    }

                // skills tab
                case SkillsPage _:
                    return this.Codex.GetPlayer(Game1.player);

                // Community Center bundle menu
                case JunimoNoteMenu bundleMenu:
                    {
                        // hovered inventory item
                        {
                            Item item = this.Reflection.GetField<Item>(menu, "hoveredItem").GetValue();
                            if (item != null)
                                return this.Codex.GetItem(item, ObjectContext.Inventory);
                        }

                        // list of required ingredients
                        for (int i = 0; i < bundleMenu.ingredientList.Count; i++)
                        {
                            if (bundleMenu.ingredientList[i].containsPoint((int)cursorPos.X, (int)cursorPos.Y))
                            {
                                Bundle bundle = this.Reflection.GetField<Bundle>(bundleMenu, "currentPageBundle").GetValue();
                                var ingredient = bundle.ingredients[i];
                                var item = this.GameHelper.GetObjectBySpriteIndex(ingredient.index, ingredient.stack);
                                item.Quality = ingredient.quality;
                                return this.Codex.GetItem(item, ObjectContext.Inventory);
                            }
                        }

                        // list of submitted ingredients
                        foreach (ClickableTextureComponent slot in bundleMenu.ingredientSlots)
                        {
                            if (slot.item != null && slot.containsPoint((int)cursorPos.X, (int)cursorPos.Y))
                                return this.Codex.GetItem(slot.item, ObjectContext.Inventory);
                        }
                    }
                    break;

                // load menu
                case TitleMenu _ when TitleMenu.subMenu is LoadGameMenu loadMenu:
                    {
                        ClickableComponent button = loadMenu.slotButtons.FirstOrDefault(p => p.containsPoint((int)cursorPos.X, (int)cursorPos.Y));
                        if (button != null)
                        {
                            int index = this.Reflection.GetField<int>(loadMenu, "currentItemIndex").GetValue() + int.Parse(button.name);
                            var slots = this.Reflection.GetProperty<List<LoadGameMenu.MenuSlot>>(loadMenu, "MenuSlots").GetValue();
                            LoadGameMenu.SaveFileSlot slot = slots[index] as LoadGameMenu.SaveFileSlot;
                            if (slot?.Farmer != null)
                                return this.Codex.GetPlayer(slot.Farmer, isLoadMenu: true);
                        }
                    }
                    break;

                // shop
                case ShopMenu shopMenu:
                    {
                        ISalable entry = shopMenu.hoveredItem;
                        if (entry is Item item)
                            return this.Codex.GetItem(item, ObjectContext.Inventory);
                        if (entry is MovieConcession snack)
                            return this.Codex.GetMovieSnack(snack);
                    }
                    break;

                // toolbar
                case Toolbar _:
                    {
                        // find hovered slot
                        List<ClickableComponent> slots = this.Reflection.GetField<List<ClickableComponent>>(menu, "buttons").GetValue();
                        ClickableComponent hoveredSlot = slots.FirstOrDefault(slot => slot.containsPoint((int)cursorPos.X, (int)cursorPos.Y));
                        if (hoveredSlot == null)
                            return null;

                        // get inventory index
                        int index = slots.IndexOf(hoveredSlot);
                        if (index < 0 || index > Game1.player.Items.Count - 1)
                            return null;

                        // get hovered item
                        Item item = Game1.player.Items[index];
                        if (item != null)
                            return this.Codex.GetItem(item, ObjectContext.Inventory);
                    }
                    break;

                // by convention (for mod support)
                default:
                    {
                        Item item = this.Reflection.GetField<Item>(menu, "HoveredItem", required: false)?.GetValue(); // ChestsAnywhere
                        if (item != null)
                            return this.Codex.GetItem(item, ObjectContext.Inventory);
                    }
                    break;
            }

            return null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the tile the player is facing.</summary>
        /// <param name="player">The player to check.</param>
        private Vector2 GetFacingTile(Farmer player)
        {
            Vector2 tile = player.getTileLocation();
            FacingDirection direction = (FacingDirection)player.FacingDirection;
            switch (direction)
            {
                case FacingDirection.Up:
                    return tile + new Vector2(0, -1);
                case FacingDirection.Right:
                    return tile + new Vector2(1, 0);
                case FacingDirection.Down:
                    return tile + new Vector2(0, 1);
                case FacingDirection.Left:
                    return tile + new Vector2(-1, 0);
                default:
                    throw new NotSupportedException($"Unknown facing direction {direction}");
            }
        }
    }
}
