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
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>Whether the game has launched.</summary>
        private bool IsLaunched;

        /// <summary>The current step in the mod logic.</summary>
        private Stage CurrentStage = Stage.None;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.LoadConfig();

            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The method called when the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
                return; // return to title doesn't replay intro on Android

            if (e.NewMenu is TitleMenu)
                this.CurrentStage = Stage.SkipIntro;
        }

        /// <summary>Receives an update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            try
            {
                // start intro skip on game launch
                if (!this.IsLaunched)
                {
                    if (!(Game1.activeClickableMenu is TitleMenu))
                        return;

                    this.IsLaunched = true;
                    this.CurrentStage = Stage.SkipIntro;
                }

                // apply skip logic
                if (this.CurrentStage != Stage.None)
                {
                    this.CurrentStage = Game1.activeClickableMenu is TitleMenu menu
                        ? this.Skip(menu, this.CurrentStage)
                        : Stage.None;
                }
            }
            catch (Exception ex)
            {
                this.Monitor.InterceptError(ex, "skipping the intro");
                this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            }
        }

        /****
        ** Methods
        ****/
        /// <summary>Load the mod configuration.</summary>
        private ModConfig LoadConfig()
        {
            var config = this.Helper.ReadConfig<ModConfig>();

            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                if (config.SkipTo == Screen.HostCoop || config.SkipTo == Screen.JoinCoop)
                    config.SkipTo = Screen.Title; // no co-op on Android
            }

            return config;
        }

        /// <summary>Skip the intro if the game is ready.</summary>
        /// <param name="menu">The title menu whose intro to skip.</param>
        /// <param name="currentStage">The current step in the mod logic.</param>
        /// <returns>Returns the next step in the skip logic.</returns>
        private Stage Skip(TitleMenu menu, Stage currentStage)
        {
            // wait until the game is ready
            if (Game1.currentGameTime == null)
                return currentStage;

            // do nothing if a confirmation box is on-screen (e.g. multiplayer disconnect error)
            if (TitleMenu.subMenu is ConfirmationDialog)
                return Stage.None;

            // main skip logic
            if (currentStage == Stage.SkipIntro)
            {
                if (Constants.TargetPlatform == GamePlatform.Android)
                {
                    // skip to title screen
                    menu.skipToTitleButtons();

                    // skip button transition
                    while (this.Helper.Reflection.GetField<bool>(menu, "isTransitioningButtons").GetValue())
                        menu.update(Game1.currentGameTime);
                }
                else
                {
                    // skip to title screen
                    menu.receiveKeyPress(Keys.Escape);
                    menu.update(Game1.currentGameTime);

                    // skip button transition
                    while (this.Helper.Reflection.GetField<int>(menu, "buttonsToShow").GetValue() < TitleMenu.numberOfButtons)
                        menu.update(Game1.currentGameTime);
                }

                // skip to next screen
                switch (this.Config.SkipTo)
                {
                    case Screen.Title:
                        return Stage.None;

                    case Screen.Load:
                        // skip to load screen
                        menu.performButtonAction("Load");
                        while (TitleMenu.subMenu == null)
                            menu.update(Game1.currentGameTime);
                        return Stage.None;

                    case Screen.JoinCoop:
                    case Screen.HostCoop:
                        // skip to co-op screen
                        menu.performButtonAction("Co-op");
                        while (TitleMenu.subMenu == null)
                            menu.update(Game1.currentGameTime);

                        return this.Config.SkipTo == Screen.JoinCoop
                            ? Stage.None
                            : Stage.WaitingForConnection;
                }
            }

            // skip to host tab after connection is established
            if (currentStage == Stage.WaitingForConnection)
            {
                // not applicable
                if (this.Config.SkipTo != Screen.HostCoop || !(TitleMenu.subMenu is CoopMenu submenu))
                    return Stage.None;

                // not connected yet
                if (submenu.hostTab == null)
                    return currentStage;

                // select host tab
                submenu.receiveLeftClick(submenu.hostTab.bounds.X, submenu.hostTab.bounds.Y, playSound: false);
            }

            // ???
            return Stage.None;
        }
    }
}
