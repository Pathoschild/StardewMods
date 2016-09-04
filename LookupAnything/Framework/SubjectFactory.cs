using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.LookupAnything.Framework.Subjects;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>Extracts metadata from arbitrary objects.</summary>
    public class SubjectFactory
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
        public SubjectFactory(Metadata metadata)
        {
            this.Metadata = metadata;
        }


        /****
        ** From context
        ****/
        /// <summary>Get all potential lookup targets in the current location.</summary>
        /// <param name="location">The current location.</param>
        public IEnumerable<Target> GetAllTargets(GameLocation location)
        {
            // NPCs
            foreach (NPC npc in location.characters)
            {
                TargetType type = TargetType.Unknown;
                if (npc.isVillager())
                    type = TargetType.Villager;
                else if (npc is Pet)
                    type = TargetType.Pet;
                else if (npc is Monster)
                    type = TargetType.Monster;

                yield return new Target(type, npc, npc.getTileLocation());
            }

            // animals
            foreach (FarmAnimal animal in (location as Farm)?.animals.Values ?? (location as AnimalHouse)?.animals.Values ?? Enumerable.Empty<FarmAnimal>())
                yield return new Target(TargetType.FarmAnimal, animal, animal.getTileLocation());

            // map objects
            foreach (var pair in location.objects)
            {
                Vector2 position = pair.Key;
                Object obj = pair.Value;
                yield return new Target(TargetType.Object, obj, position);
            }

            // terrain features
            foreach (var pair in location.terrainFeatures)
            {
                Vector2 position = pair.Key;
                TerrainFeature feature = pair.Value;

                TargetType type = TargetType.Unknown;
                if ((feature as HoeDirt)?.crop != null)
                    type = TargetType.Crop;
                else if (feature is FruitTree)
                    type = TargetType.FruitTree;
                else if (feature is Tree)
                    type = TargetType.WildTree;

                yield return new Target(type, feature, position);
            }
        }

        /// <summary>Get metadata for a Stardew object at the specified position.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The object's tile position within the <paramref name="location"/>.</param>
        public ISubject GetSubjectFrom(GameLocation location, Vector2 tile)
        {
            IEnumerable<Target> targets = this.GetAllTargets(location).Where(p => p.IsAtTile(tile));
            foreach (Target target in targets)
            {
                switch (target.Type)
                {
                    // NPC
                    case TargetType.Pet:
                    case TargetType.Monster:
                    case TargetType.Villager:
                        return new CharacterSubject(target.ForType<NPC>(), this.Metadata);

                    // animal
                    case TargetType.FarmAnimal:
                        return new FarmAnimalSubject(target.ForType<FarmAnimal>());

                    // crop
                    case TargetType.Crop:
                        Crop crop = ((HoeDirt)target.Value).crop;
                        return new CropSubject(crop, GameHelper.GetObjectBySpriteIndex(crop.indexOfHarvest), this.Metadata);

                    // tree
                    case TargetType.FruitTree:
                        return new FruitTreeSubject(target.ForType<FruitTree>());
                    case TargetType.WildTree:
                        return new TreeSubject(target.ForType<Tree>());

                    // object
                    case TargetType.InventoryItem:
                    case TargetType.Object:
                        return new ItemSubject(target.ForType<Item>(), knownQuality: false, metadata: this.Metadata);
                }
            }

            return null;
        }

        /// <summary>Get metadata for a menu element at the specified position.</summary>
        /// <param name="activeMenu">The active menu.</param>
        public ISubject GetSubjectFrom(IClickableMenu activeMenu)
        {
            // inventory
            if (activeMenu is GameMenu)
            {
                // get current tab
                List<IClickableMenu> tabs = GameHelper.GetPrivateField<List<IClickableMenu>>(activeMenu, "pages");
                IClickableMenu curTab = tabs[((GameMenu)activeMenu).currentTab];
                if (curTab is InventoryPage)
                {
                    Item item = GameHelper.GetPrivateField<Item>(curTab, "hoveredItem");
                    if (item != null)
                        return new ItemSubject(new Target<Item>(TargetType.InventoryItem, item, null), knownQuality: true, metadata: this.Metadata);
                }
                else if (curTab is CraftingPage)
                {
                    Item item = GameHelper.GetPrivateField<Item>(curTab, "hoverItem");
                    if (item != null)
                        return new ItemSubject(new Target<Item>(TargetType.InventoryItem, item, null), knownQuality: true, metadata: this.Metadata);
                }
            }

            // by convention (for mod support)
            else
            {
                Item item = GameHelper.GetPrivateField<Item>(activeMenu, "HoveredItem", required: false); // ChestsAnywhere
                if (item != null)
                    return new ItemSubject(new Target<Item>(TargetType.InventoryItem, item, null), knownQuality: true, metadata: this.Metadata);
            }

            return null;
        }
    }
}