using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.LookupAnything.Framework.Subjects;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>A factory which extracts metadata from arbitrary objects.</summary>
    public class SubjectFactory
    {
        /*********
        ** Public methods
        *********/
        /****
        ** From context
        ****/
        /// <summary>Get metadata for a Stardew object at the specified position.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="position">The object's tile position within the <paramref name="location"/>.</param>
        public ISubject GetSubjectFrom(GameLocation location, Vector2 position)
        {
            // map object
            if (location.objects.ContainsKey(position))
                return this.GetSubject(location.objects[position]);

            // terrain feature
            if (location.terrainFeatures.ContainsKey(position))
                return this.GetSubject(location.terrainFeatures[position], position);

            // NPC
            if (location.isCharacterAtTile(position) != null)
                return this.GetSubject(location.isCharacterAtTile(position));

            // animals
            foreach (FarmAnimal animal in (location as Farm)?.animals.Values ?? (location as AnimalHouse)?.animals.Values ?? Enumerable.Empty<FarmAnimal>())
            {
                if (animal.getTileLocation() == position)
                    return this.GetSubject(animal);
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
                        return new ItemSubject(item, knownQuality: true);
                }
                else if (curTab is CraftingPage)
                {
                    Item item = GameHelper.GetPrivateField<Item>(curTab, "hoverItem");
                    if (item != null)
                        return new ItemSubject(item, knownQuality: true);
                }
            }

            return null;
        }

        /****
        ** For object
        ****/
        /// <summary>Get metadata for a Stardew object.</summary>
        /// <param name="obj">The underlying object.</param>
        public ISubject GetSubject(Object obj)
        {
            return new ItemSubject(obj, knownQuality: false);
        }

        /// <summary>Get metadata for a Stardew object.</summary>
        /// <param name="terrainFeature">The underlying object.</param>
        /// <param name="position">The underlying object's tile position within the current location.</param>
        public ISubject GetSubject(TerrainFeature terrainFeature, Vector2 position)
        {
            // crop
            if (terrainFeature is HoeDirt)
            {
                Crop crop = ((HoeDirt)terrainFeature).crop;
                return crop != null
                    ? new CropSubject(crop, GameHelper.GetObjectBySpriteIndex(crop.indexOfHarvest))
                    : null;
            }

            // tree
            if(terrainFeature is FruitTree)
                return new FruitTreeSubject(terrainFeature as FruitTree, position);
            if (terrainFeature is Tree)
                return new TreeSubject(terrainFeature as Tree, position);

            return null;
        }

        /// <summary>Get metadata for a Stardew object.</summary>
        /// <param name="animal">The underlying animal.</param>
        public ISubject GetSubject(FarmAnimal animal)
        {
            return new FarmAnimalSubject(animal);
        }

        /// <summary>Get metadata for a Stardew object.</summary>
        /// <param name="npc">The underlying object.</param>
        public ISubject GetSubject(NPC npc)
        {
            return new CharacterSubject(npc);
        }
    }
}