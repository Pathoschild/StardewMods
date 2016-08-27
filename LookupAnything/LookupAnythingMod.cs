using System;
using Microsoft.Xna.Framework.Input;
using Pathoschild.LookupAnything.Components;
using Pathoschild.LookupAnything.Framework;
using Pathoschild.LookupAnything.Framework.Subjects;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.LookupAnything
{
    /// <summary>The mod entry point.</summary>
    class LookupAnythingMod : Mod
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
            try
            {
                // check input
                if (!received.Equals(expected))
                    return;

                // show encyclopedia
                ISubject subject = Game1.activeClickableMenu != null
                    ? new SubjectFactory().GetSubjectFrom(Game1.activeClickableMenu)
                    : new SubjectFactory().GetSubjectFrom(Game1.currentLocation, Game1.currentCursorTile);
                if (subject != null)
                {
                    this.PreviousMenu = Game1.activeClickableMenu;
                    Game1.activeClickableMenu = new EncyclopediaMenu(subject);
                }
            }
            catch (Exception ex)
            {
                Game1.showRedMessage("Huh. Something went wrong looking that up. The game error log has the technical details.");
                Log.Error(ex.ToString());
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
