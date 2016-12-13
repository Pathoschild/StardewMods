using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Subjects;
using Pathoschild.Stardew.LookupAnything.Framework.Targets;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

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


        /*********
        ** Public methods
        *********/
        /****
        ** Constructors
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public TargetFactory(Metadata metadata, IReflectionHelper reflection)
        {
            this.Metadata = metadata;
            this.Reflection = reflection;
        }

        /****
        ** Targets
        ****/
        /// <summary>Get all potential lookup targets in the current location.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="originTile">The tile from which to search for targets.</param>
        public IEnumerable<ITarget> GetNearbyTargets(GameLocation location, Vector2 originTile)
        {
            // NPCs
            foreach (NPC npc in location.characters)
            {
                if (!GameHelper.CouldSpriteOccludeTile(npc.getTileLocation(), originTile))
                    continue;

                TargetType type = TargetType.Unknown;
                if (npc.isVillager())
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
                Object obj = pair.Value;

                if (!GameHelper.CouldSpriteOccludeTile(spriteTile, originTile))
                    continue;

                yield return new ObjectTarget(obj, spriteTile, this.Reflection);
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
                else if (feature is FruitTree)
                {
                    if (this.Reflection.GetPrivateValue<float>(feature, "alpha") < 0.8f)
                        continue; // ignore when tree is faded out (so player can lookup things behind it)
                    yield return new FruitTreeTarget((FruitTree)feature, spriteTile);
                }
                else if (feature is Tree)
                {
                    if (this.Reflection.GetPrivateValue<float>(feature, "alpha") < 0.8f)
                        continue; // ignore when tree is faded out (so player can lookup things behind it)
                    yield return new TreeTarget((Tree)feature, spriteTile, this.Reflection);
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
        }

        /// <summary>Get the target on the specified tile.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to search.</param>
        public ITarget GetTargetFromTile(GameLocation location, Vector2 tile)
        {
            return (
                from target in this.GetNearbyTargets(location, tile)
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
        public ITarget GetTargetFromScreenCoordinate(GameLocation location, Vector2 tile, Vector2 position)
        {
            // get target sprites overlapping cursor position
            Rectangle tileArea = GameHelper.GetScreenCoordinatesFromTile(tile);
            return (
                // select targets whose sprites may overlap the target position
                from target in this.GetNearbyTargets(location, tile)
                let spriteArea = target.GetSpriteArea()
                where
                    target.Type != TargetType.Unknown
                    && (target.IsAtTile(tile) || spriteArea.Intersects(tileArea))

                // sort targets by layer
                // (A higher Y value is closer to the foreground, and will occlude any sprites
                // behind it. If two sprites at the same Y coordinate overlap, assume the left
                // sprite occludes the right.)
                orderby spriteArea.Y descending, spriteArea.X ascending

                where target.SpriteIntersectsPixel(tile, position, spriteArea)

                select target
            ).FirstOrDefault();
        }

        /****
        ** Subjects
        ****/
        /// <summary>Get metadata for a Stardew object at the specified position.</summary>
        /// <param name="player">The player performing the lookup.</param>
        /// <param name="location">The current location.</param>
        /// <param name="lookupMode">The lookup target mode.</param>
        public ISubject GetSubjectFrom(Farmer player, GameLocation location, LookupMode lookupMode)
        {
            // get target
            ITarget target;
            switch (lookupMode)
            {
                // under cursor
                case LookupMode.Cursor:
                    target = this.GetTargetFromScreenCoordinate(location, Game1.currentCursorTile, GameHelper.GetScreenCoordinatesFromCursor());
                    break;

                // in front of player
                case LookupMode.FacingPlayer:
                    Vector2 tile = this.GetFacingTile(Game1.player);
                    target = this.GetTargetFromTile(location, tile);
                    break;

                default:
                    throw new NotImplementedException($"Unknown lookup mode '{lookupMode}'.");
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
                    return new CharacterSubject(target.GetValue<NPC>(), target.Type, this.Metadata, this.Reflection);

                // player
                case TargetType.Farmer:
                    return new FarmerSubject(target.GetValue<Farmer>(), this.Reflection);

                // animal
                case TargetType.FarmAnimal:
                    return new FarmAnimalSubject(target.GetValue<FarmAnimal>());

                // crop
                case TargetType.Crop:
                    Crop crop = target.GetValue<HoeDirt>().crop;
                    return new ItemSubject(GameHelper.GetObjectBySpriteIndex(crop.indexOfHarvest), ObjectContext.World, knownQuality: false, fromCrop: crop);

                // tree
                case TargetType.FruitTree:
                    return new FruitTreeSubject(target.GetValue<FruitTree>(), target.GetTile());
                case TargetType.WildTree:
                    return new TreeSubject(target.GetValue<Tree>(), target.GetTile());

                // object
                case TargetType.InventoryItem:
                    return new ItemSubject(target.GetValue<Item>(), ObjectContext.Inventory, knownQuality: false);
                case TargetType.Object:
                    return new ItemSubject(target.GetValue<Item>(), ObjectContext.World, knownQuality: false);
            }

            return null;
        }

        /// <summary>Get metadata for a menu element at the specified position.</summary>
        /// <param name="menu">The active menu.</param>
        /// <param name="cursorPosition">The cursor's viewport-relative coordinates.</param>
        public ISubject GetSubjectFrom(IClickableMenu menu, Vector2 cursorPosition)
        {
            // calendar
            if (menu is Billboard)
            {
                Billboard billboard = (Billboard)menu;

                // get target day
                int selectedDay = -1;
                {
                    List<ClickableTextureComponent> calendarDays = this.Reflection.GetPrivateValue<List<ClickableTextureComponent>>(billboard, "calendarDays");
                    for (int i = 0; i < calendarDays.Count; i++)
                    {
                        if (calendarDays[i].containsPoint((int)cursorPosition.X, (int)cursorPosition.Y))
                        {
                            selectedDay = i + 1;
                            break;
                        }
                    }
                }
                if (selectedDay == -1)
                    return null;

                // get villager with a birthday on that date
                NPC target = GameHelper.GetAllCharacters().FirstOrDefault(p => p.birthday_Season == Game1.currentSeason && p.birthday_Day == selectedDay);
                if (target != null)
                    return new CharacterSubject(target, TargetType.Villager, this.Metadata, this.Reflection);
            }

            // chest
            else if (menu is MenuWithInventory)
            {
                Item item = ((MenuWithInventory)menu).hoveredItem;
                if (item != null)
                    return new ItemSubject(item, ObjectContext.Inventory, knownQuality: true);
            }

            // inventory
            else if (menu is GameMenu)
            {
                // get current tab
                List<IClickableMenu> tabs = this.Reflection.GetPrivateValue<List<IClickableMenu>>(menu, "pages");
                IClickableMenu curTab = tabs[((GameMenu)menu).currentTab];
                if (curTab is InventoryPage)
                {
                    Item item = this.Reflection.GetPrivateValue<Item>(curTab, "hoveredItem");
                    if (item != null)
                        return new ItemSubject(item, ObjectContext.Inventory, knownQuality: true);
                }
                else if (curTab is CraftingPage)
                {
                    Item item = this.Reflection.GetPrivateValue<Item>(curTab, "hoverItem");
                    if (item != null)
                        return new ItemSubject(item, ObjectContext.Inventory, knownQuality: true);
                }
                else if (curTab is SocialPage)
                {
                    // get villagers on current page
                    int scrollOffset = this.Reflection.GetPrivateValue<int>(curTab, "slotPosition");
                    ClickableTextureComponent[] entries = this.Reflection
                        .GetPrivateValue<List<ClickableTextureComponent>>(curTab, "friendNames")
                        .Skip(scrollOffset)
                        .ToArray();

                    // find hovered villager
                    ClickableTextureComponent entry = entries.FirstOrDefault(p => p.containsPoint((int)cursorPosition.X, (int)cursorPosition.Y));
                    if (entry != null)
                    {
                        NPC npc = GameHelper.GetAllCharacters().FirstOrDefault(p => p.name == entry.name);
                        if (npc != null)
                            return new CharacterSubject(npc, TargetType.Villager, this.Metadata, this.Reflection);
                    }
                }
            }

            // shop
            else if (menu is ShopMenu)
            {
                Item item = this.Reflection.GetPrivateValue<Item>(menu, "hoveredItem");
                if (item != null)
                    return new ItemSubject(item.getOne(), ObjectContext.Inventory, knownQuality: true);
            }

            // toolbar
            else if (menu is Toolbar)
            {
                // find hovered slot
                List<ClickableComponent> slots = this.Reflection.GetPrivateValue<List<ClickableComponent>>(menu, "buttons");
                ClickableComponent hoveredSlot = slots.FirstOrDefault(slot => slot.containsPoint((int)cursorPosition.X, (int)cursorPosition.Y));
                if (hoveredSlot == null)
                    return null;

                // get inventory index
                int index = slots.IndexOf(hoveredSlot);
                if (index < 0 || index > Game1.player.items.Count - 1)
                    return null;

                // get hovered item
                Item item = Game1.player.items[index];
                if (item != null)
                    return new ItemSubject(item.getOne(), ObjectContext.Inventory, knownQuality: true);
            }

            // by convention (for mod support)
            else
            {
                Item item = this.Reflection.GetPrivateValue<Item>(menu, "HoveredItem", required: false); // ChestsAnywhere
                if (item != null)
                    return new ItemSubject(item, ObjectContext.Inventory, knownQuality: true);
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
                    throw new NotImplementedException($"Unknown facing direction {direction}");
            }
        }
    }
}