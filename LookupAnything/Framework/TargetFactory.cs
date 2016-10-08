using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.LookupAnything.Framework.Data;
using Pathoschild.LookupAnything.Framework.Subjects;
using Pathoschild.LookupAnything.Framework.Targets;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>Finds and analyses lookup targets in the world.</summary>
    internal class TargetFactory
    {
        /*********
        ** Properties
        *********/
        /// <summary>Provides metadata that's not available from the game data directly.</summary>
        private readonly Metadata Metadata;


        /*********
        ** Public methods
        *********/
        /****
        ** Constructors
        ****/
        /// <summary>Construct an instance.</summary>
        /// <param name="metadata">Provides metadata that's not available from the game data directly.</param>
        public TargetFactory(Metadata metadata)
        {
            this.Metadata = metadata;
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

                yield return new CharacterTarget(type, npc, npc.getTileLocation());
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

                yield return new ObjectTarget(obj, spriteTile);
            }

            // terrain features
            foreach (var pair in location.terrainFeatures)
            {
                Vector2 spriteTile = pair.Key;
                TerrainFeature feature = pair.Value;

                if (!GameHelper.CouldSpriteOccludeTile(spriteTile, originTile))
                    continue;

                if ((feature as HoeDirt)?.crop != null)
                    yield return new CropTarget(feature, spriteTile);
                else if (feature is FruitTree)
                    yield return new FruitTreeTarget((FruitTree)feature, spriteTile);
                else if (feature is Tree)
                    yield return new TreeTarget((Tree)feature, spriteTile);
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

        /// <summary>Get the target at the specified coordinate.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        public ITarget GetTargetFrom(GameLocation location, Vector2 tile, Vector2 position)
        {
            // get target sprites overlapping cursor position
            Rectangle tileArea = GameHelper.GetScreenCoordinatesFromTile(tile);
            return (
                // select targets whose sprites may overlap the cursor position
                from target in this.GetNearbyTargets(location, tile)
                let spriteArea = target.GetSpriteArea()
                where target.Type != TargetType.Unknown
                where target.IsAtTile(tile) || spriteArea.Intersects(tileArea)

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
        /// <param name="location">The current location.</param>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        public ISubject GetSubjectFrom(GameLocation location, Vector2 tile, Vector2 position)
        {
            ITarget target = this.GetTargetFrom(location, tile, position);
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
                    return new CharacterSubject(target.GetValue<NPC>(), target.Type, this.Metadata);

                // player
                case TargetType.Farmer:
                    return new FarmerSubject(target.GetValue<Farmer>());

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
                    List<ClickableTextureComponent> calendarDays = GameHelper.GetPrivateField<List<ClickableTextureComponent>>(billboard, "calendarDays");
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
                NPC target = Utility.getAllCharacters().FirstOrDefault(p => p.birthday_Season == Game1.currentSeason && p.birthday_Day == selectedDay);
                if(target != null)
                    return new CharacterSubject(target, TargetType.Villager, this.Metadata);
            }

            // chest
            else if (menu is MenuWithInventory)
            {
                Item item = ((MenuWithInventory)menu).hoveredItem;
                if(item != null)
                    return new ItemSubject(item, ObjectContext.Inventory, knownQuality: true);
            }

            // inventory
            else if (menu is GameMenu)
            {
                // get current tab
                List<IClickableMenu> tabs = GameHelper.GetPrivateField<List<IClickableMenu>>(menu, "pages");
                IClickableMenu curTab = tabs[((GameMenu)menu).currentTab];
                if (curTab is InventoryPage)
                {
                    Item item = GameHelper.GetPrivateField<Item>(curTab, "hoveredItem");
                    if (item != null)
                        return new ItemSubject(item, ObjectContext.Inventory, knownQuality: true);
                }
                else if (curTab is CraftingPage)
                {
                    Item item = GameHelper.GetPrivateField<Item>(curTab, "hoverItem");
                    if (item != null)
                        return new ItemSubject(item, ObjectContext.Inventory, knownQuality: true);
                }
            }

            // by convention (for mod support)
            else
            {
                Item item = GameHelper.GetPrivateField<Item>(menu, "HoveredItem", required: false); // ChestsAnywhere
                if (item != null)
                    return new ItemSubject(item, ObjectContext.Inventory, knownQuality: true);
            }

            return null;
        }
    }
}