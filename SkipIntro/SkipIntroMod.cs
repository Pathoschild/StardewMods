using System;
using System.Threading.Tasks;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.SkipIntro.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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

            MenuEvents.MenuChanged += this.ReceiveMenuChanged;
            GameEvents.GameLoaded += this.ReceiveGameLoaded;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player loads the game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveGameLoaded(object sender, EventArgs e)
        {
            // check for an updated version
            if (this.Config.CheckForUpdates)
            {
                Task.Factory.StartNew(() =>
                {
                    UpdateHelper.LogVersionCheck(this.Monitor, this.ModManifest.Version, "SkipIntro").Wait();
                });
            }
        }

        /// <summary>The method invoked when the game replaces one menu with another.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveMenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            try
            {
                // get menu
                TitleMenu menu = e.NewMenu as TitleMenu;
                if (menu == null)
                    return;

                // skip intro
                this.Helper.Reflection.GetPrivateField<int>(menu, "chuckleFishTimer").SetValue(0);
                menu.skipToTitleButtons();
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Couldn't skip the menu: {ex}", LogLevel.Error);
            }
        }
    }
}
