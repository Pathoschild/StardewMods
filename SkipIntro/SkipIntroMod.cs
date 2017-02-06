using System;
using System.Threading.Tasks;
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

        /// <summary>The update ticks that have been processed.</summary>
        private int Step;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            GameEvents.GameLoaded += this.ReceiveGameLoaded;
            GameEvents.EighthUpdateTick += this.ReceiveUpdateTick;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
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

        /// <summary>Receives an update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveUpdateTick(object sender, EventArgs e)
        {
            try
            {
                TitleMenu menu = Game1.activeClickableMenu as TitleMenu;
                if (menu != null)
                {
                    if (!this.ApplySkip(menu, this.Step))
                        GameEvents.UpdateTick -= this.ReceiveUpdateTick;
                    this.Step++;
                }
            }
            catch (Exception ex)
            {
                this.Monitor.InterceptError(ex, $"skipping the menu (step {this.Step})");
                GameEvents.UpdateTick -= this.ReceiveUpdateTick;
            }
        }

        /****
        ** Methods
        ****/
        /// <summary>Apply the next skip step.</summary>
        /// <param name="menu">The title menu to update.</param>
        /// <param name="step">The step to apply (starting at 0).</param>
        /// <returns>Returns whether there are more skip steps.</returns>
        /// <remarks>The skip logic is applied over several update ticks to let the game update itself smoothly. This prevents a few issues like a long pause before the game window opens, or the title menu not resizing itself for full-screen display.</remarks>
        private bool ApplySkip(TitleMenu menu, int step)
        {
            switch (step)
            {
                // skip to main menu
                case 1:
                    this.Helper.Reflection.GetPrivateField<int>(menu, "chuckleFishTimer").SetValue(0);
                    menu.skipToTitleButtons();
                    return true;

                // skip to loading screen
                case 2:
                    if (this.Config.SkipToLoadScreen)
                        menu.performButtonAction("Load");
                    return true;

                default:
                    return false;
            }
        }
    }
}
