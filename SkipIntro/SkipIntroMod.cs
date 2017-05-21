using System;
using Microsoft.Xna.Framework.Input;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.SkipIntro.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.SkipIntro
{
    /// <summary>The mod entry point.</summary>
    public class SkipIntroMod : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            SaveEvents.AfterLoad += this.ReceiveAfterLoad;
            MenuEvents.MenuChanged += this.ReceiveMenuChanged;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The method invoked after the player loads a saved game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveAfterLoad(object sender, EventArgs e)
        {
            // check for updates
            if (this.Config.CheckForUpdates)
                UpdateHelper.LogVersionCheckAsync(this.Monitor, this.ModManifest, "SkipIntro");
        }

        /// <summary>The method called when the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveMenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (e.NewMenu is TitleMenu)
                GameEvents.UpdateTick += this.ReceiveUpdateTick;
        }

        /// <summary>Receives an update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveUpdateTick(object sender, EventArgs e)
        {
            try
            {
                // get open title screen
                TitleMenu menu = Game1.activeClickableMenu as TitleMenu;
                if (menu == null)
                {
                    GameEvents.UpdateTick -= this.ReceiveUpdateTick;
                    return;
                }

                // skip intro
                if (this.TrySkipIntro(menu))
                    GameEvents.UpdateTick -= this.ReceiveUpdateTick;
            }
            catch (Exception ex)
            {
                this.Monitor.InterceptError(ex, "skipping the intro");
                GameEvents.UpdateTick -= this.ReceiveUpdateTick;
            }
        }

        /****
        ** Methods
        ****/
        /// <summary>Skip the intro if the game is ready.</summary>
        /// <param name="menu">The title menu whose intro to skip.</param>
        private bool TrySkipIntro(TitleMenu menu)
        {
            if (Game1.currentGameTime == null)
                return false; // game isn't ready yet

            // skip to title screen
            menu.receiveKeyPress(Keys.Escape);
            menu.update(Game1.currentGameTime);

            // skip to load screen
            if (this.Config.SkipToLoadScreen)
            {
                menu.performButtonAction("Load");
                while (TitleMenu.subMenu == null)
                    menu.update(Game1.currentGameTime);
            }

            return true;
        }
    }
}
