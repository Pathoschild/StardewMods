using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Pathoschild.NotesAnywhere.Components;
using Pathoschild.NotesAnywhere.Framework;
using Pathoschild.NotesAnywhere.Framework.Subjects;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.NotesAnywhere
{
    /// <summary>The mod entry point.</summary>
    class NotesAnywhereMod : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The previous menu shown before the encyclopedia UI was opened.</summary>
        private IClickableMenu PreviousMenu;


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        public override void Entry(params object[] objects)
        {
            ControlEvents.KeyPressed += (sender, e) => this.TryOpenMenu(Keys.F1, e.KeyPressed);
            MenuEvents.MenuClosed += (sender, e) => this.TryRestorePreviousMenu(e.PriorMenu);
        }



        /*********
        ** Private methods
        *********/
        /// <summary>Show the encyclopedia article for the hovered target if the control matches the configured control.</summary>
        /// <typeparam name="T">The input type.</typeparam>
        /// <param name="expected">The configured toggle input.</param>
        /// <param name="received">The received toggle input.</param>
        private void TryOpenMenu<T>(T expected, T received)
        {
            // check input
            if (!received.Equals(expected))
                return;

            // get subject
            ISubject subject = null;
            {
                GameLocation location = Game1.currentLocation;
                Vector2 cursorPos = Game1.currentCursorTile;
                SubjectFactory factory = new SubjectFactory();

                // map object
                if (location.objects.ContainsKey(cursorPos))
                    subject = factory.GetSubject(location.objects[cursorPos]);

                // terrain feature
                else if (location.terrainFeatures.ContainsKey(cursorPos))
                    subject = factory.GetSubject(location.terrainFeatures[cursorPos]);

                // NPC
                else if (location.isCharacterAtTile(cursorPos) != null)
                {
                    NPC character = Game1.currentLocation.isCharacterAtTile(cursorPos);
                    subject = factory.GetSubject(character);
                }

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

            // show encyclopedia
            if (subject != null)
            {
                this.PreviousMenu = Game1.activeClickableMenu;
                Game1.activeClickableMenu = new EncyclopediaMenu(subject);

            }
        }

        /// <summary>Restore the previous menu if it was hidden to display the encyclopedia menu.</summary>
        /// <param name="closedMenu">The menu which the player just closed.</param>
        private void TryRestorePreviousMenu(IClickableMenu closedMenu)
        {
            if (closedMenu is EncyclopediaMenu && this.PreviousMenu != null)
            {
                Game1.activeClickableMenu = this.PreviousMenu;
                this.PreviousMenu = null;
            }
        }
    }
}
