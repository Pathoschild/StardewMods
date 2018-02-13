using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Integrations.CustomFarmingRedux;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Subjects;
using Pathoschild.Stardew.LookupAnything.Framework.Targets;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.LookupAnything.Framework
{
    /// <summary>Finds and analyses lookup targets in the world.</summary>
    internal class TargetFactory
    {
        /*********
        ** Properties
        *********/
        /// <summary>Provides metadata that's not available from the game data directly.</summary>
        private readonly Metadata Metadata;

        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>Provides translations stored in the mod folder.</summary>
        private readonly ITranslationHelper Translations;

        /// <summary>Handles the logic for integrating with the Custom Farming Redux mod.</summary>
        private readonly CustomFarmingReduxIntegration CustomFarming;


        /*********
        ** Public methods
        *********/
        /****
        ** Constructors
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <param name="translations">Provides translations stored in the mod folder.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        /// <param name="customFarming">Handles the logic for integrating with the Custom Farming Redux mod.</param>
        public TargetFactory(Metadata metadata, ITranslationHelper translations, IReflectionHelper reflection, CustomFarmingReduxIntegration customFarming)
        {
            this.Metadata = metadata;
            this.Translations = translations;
            this.Reflection = reflection;
            this.CustomFarming = customFarming;
        }

        /****
        ** Targets
        ****/
        /// <summary>Get all potential lookup targets in the current location.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="originTile">The tile from which to search for targets.</param>
        /// <param name="includeMapTile">Whether to allow matching the map tile itself.</param>
        public IEnumerable<ITarget> GetNearbyTargets(GameLocation location, Vector2 originTile, bool includeMapTile)
        {
            // NPCs
            foreach (NPC npc in location.characters)
            {
                if (!GameHelper.CouldSpriteOccludeTile(npc.getTileLocation(), originTile))
                    continue;

                TargetType type = TargetType.Unknown;
                if (npc is Child || npc.isVillager())
                    type = TargetType.Villager;
                else if (npc is Horse)
                    type = TargetType.Horse;
                else if (npc is Junimo)
                    type = TargetType.Junimo;
                else if (npc is Pet)
                    type = TargetType.Pet;
                else if (npc is Monster)
                    type = TargetType.Monster;

                yield return new CharacterTarget(type, npc, npc.getTileLocation(), this.Reflection);
            }

            // animals
            foreach (FarmAnimal animal in (location as Farm)?.animals.Values ?? (location as AnimalHouse)?.animals.Values ?? Enumerable.Empty<FarmAnimal>())
            {
                if (!GameHelper.CouldSpriteOccludeTile(animal.getTileLocation(), originTile))
                    continue;

                yield return new FarmAnimalTarget(animal, animal.getTileLocation());
            }

            // map objects
            foreach (var pair in location.objects)
            {
                Vector2 spriteTile = pair.Key;
                SObject obj = pair.Value;

                if (!GameHelper.CouldSpriteOccludeTile(spriteTile, originTile))
                    continue;

                yield return this.CustomFarming.IsLoaded && this.CustomFarming.IsCustomObject(obj)
                    ? new CustomFarmingObjectTarget(obj, spriteTile, this.Reflection, this.CustomFarming)
                    : new ObjectTarget(obj, spriteTile, this.Reflection);
            }

            // furniture
            if (location is DecoratableLocation decoratableLocation)
            {
                foreach(var furniture in decoratableLocation.furniture)
                    yield return new ObjectTarget(furniture, furniture.TileLocation, this.Reflection);
            }

            // terrain features
            foreach (var pair in location.terrainFeatures)
            {
                Vector2 spriteTile = pair.Key;
                TerrainFeature feature = pair.Value;

                if (!GameHelper.CouldSpriteOccludeTile(spriteTile, originTile))
                    continue;

                if ((feature as HoeDirt)?.crop != null)
                    yield return new CropTarget(feature, spriteTile, this.Reflection);
                else if (feature is FruitTree fruitTree)
                {
                    if (this.Reflection.GetField<float>(feature, "alpha").GetValue() < 0.8f)
                        continue; // ignore when tree is faded out (so player can lookup things behind it)
                    yield return new FruitTreeTarget(fruitTree, spriteTile);
                }
                else if (feature is Tree wildTree)
                {
                    if (this.Reflection.GetField<float>(feature, "alpha").GetValue() < 0.8f)
                        continue; // ignore when tree is faded out (so player can lookup things behind it)
                    yield return new TreeTarget(wildTree, spriteTile, this.Reflection);
                }
                else
                    yield return new UnknownTarget(feature, spriteTile);
            }

            // players
            foreach (var farmer in new[] { Game1.player }.Union(location.farmers))
            {
                if (!GameHelper.CouldSpriteOccludeTile(farmer.getTileLocation(), originTile))
                    continue;

                yield return new FarmerTarget(farmer);
            }

            // tile
            if (includeMapTile)
                yield return new TileTarget(originTile);
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
                    target.Type != TargetType.Unknown
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
            Rectangle tileArea = GameHelper.GetScreenCoordinatesFromTile(tile);
            var candidates = (
                from target in this.GetNearbyTargets(location, tile, includeMapTile)
                let spriteArea = target.GetSpriteArea()
                let isAtTile = target.IsAtTile(tile)
                where
                    target.Type != TargetType.Unknown
                    && (isAtTile || spriteArea.Intersects(tileArea))
                orderby
                    target.Type != TargetType.Tile ? 0 : 1, // Tiles are always under anything else.
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
        /// <param name="lookupMode">The lookup target mode.</param>
        /// <param name="includeMapTile">Whether to allow matching the map tile itself.</param>
        public ISubject GetSubjectFrom(SFarmer player, GameLocation location, LookupMode lookupMode, bool includeMapTile)
        {
            // get target
            ITarget target;
            switch (lookupMode)
            {
                // under cursor
                case LookupMode.Cursor:
                    target = this.GetTargetFromScreenCoordinate(location, Game1.currentCursorTile, GameHelper.GetScreenCoordinatesFromCursor(), includeMapTile);
                    break;

                // in front of player
                case LookupMode.FacingPlayer:
                    Vector2 tile = this.GetFacingTile(Game1.player);
                    target = this.GetTargetFromTile(location, tile, includeMapTile);
                    break;

                default:
                    throw new NotSupportedException($"Unknown lookup mode '{lookupMode}'.");
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
                case TargetType.Horse:
                case TargetType.Junimo:
                case TargetType.Pet:
                case TargetType.Monster:
                case TargetType.Villager:
                    return new CharacterSubject(target.GetValue<NPC>(), target.Type, this.Metadata, this.Translations, this.Reflection);

                // player
                case TargetType.Farmer:
                    return new FarmerSubject(target.GetValue<SFarmer>(), this.Translations, this.Reflection);

                // animal
                case TargetType.FarmAnimal:
                    return new FarmAnimalSubject(target.GetValue<FarmAnimal>(), this.Translations);

                // crop
                case TargetType.Crop:
                    Crop crop = target.GetValue<HoeDirt>().crop;
                    return new ItemSubject(this.Translations, GameHelper.GetObjectBySpriteIndex(crop.indexOfHarvest), ObjectContext.World, knownQuality: false, fromCrop: crop);

                // tree
                case TargetType.FruitTree:
                    return new FruitTreeSubject(target.GetValue<FruitTree>(), target.GetTile(), this.Translations);
                case TargetType.WildTree:
                    return new TreeSubject(target.GetValue<Tree>(), target.GetTile(), this.Translations);

                // object
                case TargetType.InventoryItem:
                    return new ItemSubject(this.Translations, target.GetValue<Item>(), ObjectContext.Inventory, knownQuality: false);
                case TargetType.Object:
                    return new ItemSubject(this.Translations, target.GetValue<Item>(), ObjectContext.World, knownQuality: false);

                // tile
                case TargetType.Tile:
                    return new TileSubject(Game1.currentLocation, target.GetValue<Vector2>(), this.Translations);
            }

            return null;
        }

        /// <summary>Get metadata for a menu element at the specified position.</summary>
        /// <param name="menu">The active menu.</param>
        /// <param name="cursorPos">The cursor's viewport-relative coordinates.</param>
        public ISubject GetSubjectFrom(IClickableMenu menu, Vector2 cursorPos)
        {
            switch (menu)
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
                        NPC target = GameHelper.GetAllCharacters().FirstOrDefault(p => p.birthday_Season == Game1.currentSeason && p.birthday_Day == selectedDay);
                        if (target != null)
                            return new CharacterSubject(target, TargetType.Villager, this.Metadata, this.Translations, this.Reflection);
                    }
                    break;

                // chest
                case MenuWithInventory inventoryMenu:
                    {
                        Item item = inventoryMenu.hoveredItem;
                        if (item != null)
                            return new ItemSubject(this.Translations, item, ObjectContext.Inventory, knownQuality: true);
                    }
                    break;

                // inventory
                case GameMenu gameMenu:
                    {
                        List<IClickableMenu> tabs = this.Reflection.GetField<List<IClickableMenu>>(gameMenu, "pages").GetValue();
                        IClickableMenu curTab = tabs[gameMenu.currentTab];
                        switch (curTab)
                        {
                            case InventoryPage _:
                                {
                                    Item item = this.Reflection.GetField<Item>(curTab, "hoveredItem").GetValue();
                                    if (item != null)
                                        return new ItemSubject(this.Translations, item, ObjectContext.Inventory, knownQuality: true);
                                }
                                break;

                            case CraftingPage _:
                                {
                                    Item item = this.Reflection.GetField<Item>(curTab, "hoverItem").GetValue();
                                    if (item != null)
                                        return new ItemSubject(this.Translations, item, ObjectContext.Inventory, knownQuality: true);
                                }
                                break;

                            case SocialPage _:
                                {
                                    // get villagers on current page
                                    int scrollOffset = this.Reflection.GetField<int>(curTab, "slotPosition").GetValue();
                                    ClickableTextureComponent[] entries = this.Reflection
                                        .GetField<List<ClickableTextureComponent>>(curTab, "friendNames")
                                        .GetValue()
                                        .Skip(scrollOffset)
                                        .ToArray();

                                    // find hovered villager
                                    ClickableTextureComponent entry = entries.FirstOrDefault(p => p.containsPoint((int)cursorPos.X, (int)cursorPos.Y));
                                    if (entry != null)
                                    {
                                        NPC npc = GameHelper.GetAllCharacters().FirstOrDefault(p => p.name == entry.name);
                                        if (npc != null)
                                            return new CharacterSubject(npc, TargetType.Villager, this.Metadata, this.Translations, this.Reflection);
                                    }
                                }
                                break;
                        }
                    }
                    break;

                // Community Center bundle menu
                case JunimoNoteMenu bundleMenu:
                    {
                        // hovered inventory item
                        {
                            Item item = this.Reflection.GetField<Item>(menu, "hoveredItem").GetValue();
                            if (item != null)
                                return new ItemSubject(this.Translations, item.getOne(), ObjectContext.Inventory, knownQuality: true);
                        }

                        // list of required ingredients
                        for (int i = 0; i < bundleMenu.ingredientList.Count; i++)
                        {
                            if (bundleMenu.ingredientList[i].containsPoint((int)cursorPos.X, (int)cursorPos.Y))
                            {
                                Bundle bundle = this.Reflection.GetField<Bundle>(bundleMenu, "currentPageBundle").GetValue();
                                var ingredient = bundle.ingredients[i];
                                var item = GameHelper.GetObjectBySpriteIndex(ingredient.index, ingredient.stack);
                                item.quality = ingredient.quality;
                                return new ItemSubject(this.Translations, item, ObjectContext.Inventory, knownQuality: true);
                            }
                        }

                        // list of submitted ingredients
                        foreach (ClickableTextureComponent slot in bundleMenu.ingredientSlots)
                        {
                            if (slot.item != null && slot.containsPoint((int)cursorPos.X, (int)cursorPos.Y))
                                return new ItemSubject(this.Translations, slot.item, ObjectContext.Inventory, knownQuality: true);
                        }
                    }
                    break;

                // kitchen
                case CraftingPage _:
                    {
                        CraftingRecipe recipe = this.Reflection.GetField<CraftingRecipe>(menu, "hoverRecipe").GetValue();
                        if (recipe != null)
                            return new ItemSubject(this.Translations, recipe.createItem(), ObjectContext.Inventory, knownQuality: true);
                    }
                    break;


                // shop
                case ShopMenu _:
                    {
                        Item item = this.Reflection.GetField<Item>(menu, "hoveredItem").GetValue();
                        if (item != null)
                            return new ItemSubject(this.Translations, item.getOne(), ObjectContext.Inventory, knownQuality: true);
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
                        if (index < 0 || index > Game1.player.items.Count - 1)
                            return null;

                        // get hovered item
                        Item item = Game1.player.items[index];
                        if (item != null)
                            return new ItemSubject(this.Translations, item.getOne(), ObjectContext.Inventory, knownQuality: true);
                    }
                    break;

                // by convention (for mod support)
                default:
                    {
                        Item item = this.Reflection.GetField<Item>(menu, "HoveredItem", required: false).GetValue(); // ChestsAnywhere
                        if (item != null)
                            return new ItemSubject(this.Translations, item, ObjectContext.Inventory, knownQuality: true);
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
        private Vector2 GetFacingTile(SFarmer player)
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
