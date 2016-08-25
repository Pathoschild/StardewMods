using Microsoft.Xna.Framework;
using Pathoschild.LookupAnything.Framework.Subjects;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework
{
    /// <summary>A factory which extracts metadata from arbitrary objects.</summary>
    public class SubjectFactory
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get metadata for a Stardew object at a specified position.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="position">The object's tile position within the <paramref name="location"/>.</param>
        public ISubject GetSubject(GameLocation location, Vector2 position)
        {
            // map object
            if (location.objects.ContainsKey(position))
                return this.GetSubject(location.objects[position]);

            // terrain feature
            if (location.terrainFeatures.ContainsKey(position))
                return this.GetSubject(location.terrainFeatures[position]);

            // NPC
            if (location.isCharacterAtTile(position) != null)
                return this.GetSubject(location.isCharacterAtTile(position));

            return null;

            //    //// inventory
            //    //if (activeMenu is GameMenu)
            //    //{
            //    //    // get current tab
            //    //    GameMenu gameMenu = (GameMenu)activeMenu;
            //    //    List<IClickableMenu> tabs = (List<IClickableMenu>)typeof(GameMenu).GetField("pages", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameMenu);
            //    //    IClickableMenu curTab = tabs[gameMenu.currentTab];
            //    //    if (curTab is InventoryPage)
            //    //    {
            //    //        Item item = (Item)typeof(InventoryPage).GetField("hoveredItem", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(curTab);
            //    //        if(item != null)
            //    //            IClickableMenu.drawTextureBox(Game1.spriteBatch, "teeeest text", "teeeest title", item);
            //    //            //this.DrawHoverNote(Game1.smallFont, "teeeeest");
            //    //    }
            //    //    //if (curTab is CraftingPage)
            //    //    //{
            //    //    //    Item item = (Item)typeof(CraftingPage).GetField("hoverItem", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(curTab);
            //    //    //}
            //    //}
        }

        /// <summary>Get metadata for a Stardew object.</summary>
        /// <param name="obj">The underlying object.</param>
        public ISubject GetSubject(Object obj)
        {
            return new ObjectSubject(obj);
        }

        /// <summary>Get metadata for a Stardew object.</summary>
        /// <param name="terrainFeature">The underlying object.</param>
        public ISubject GetSubject(TerrainFeature terrainFeature)
        {
            // crop
            if (terrainFeature is HoeDirt)
            {
                Crop crop = ((HoeDirt)terrainFeature).crop;
                return crop != null
                    ? new CropSubject(crop, new Object(crop.indexOfHarvest, 1))
                    : null;
            }

            return null;
        }

        /// <summary>Get metadata for a Stardew object.</summary>
        /// <param name="npc">The underlying object.</param>
        public ISubject GetSubject(NPC npc)
        {
            return new CharacterSubject(npc);
        }
    }
}