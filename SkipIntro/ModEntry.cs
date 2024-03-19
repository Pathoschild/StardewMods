using System;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.SkipIntro.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.SDKs;

namespace Pathoschild.Stardew.SkipIntro
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config = null!; // set in Entry

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
            I18n.Init(helper.Translation);
            CommonHelper.RemoveObsoleteFiles(this, "SkipIntro.pdb"); // removed in 1.9.13

            this.Config = this.LoadConfig();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // add Generic Mod Config Menu integration
            new GenericModConfigMenuIntegrationForSkipIntro(
                getConfig: () => this.Config,
                reset: () =>
                {
                    this.Config = new ModConfig();
                    this.Helper.WriteConfig(this.Config);
                },
                saveAndApply: () => this.Helper.WriteConfig(this.Config),
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest
            ).Register();
        }

        /// <inheritdoc cref="IDisplayEvents.MenuChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
                return; // return to title doesn't replay intro on Android

            if (e.NewMenu is TitleMenu)
                this.CurrentStage = Stage.SkipIntro;
        }

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            try
            {
                // wait until game window opens
                if (Game1.ticks <= 1)
                    return;

                // start intro skip on game launch
                if (!this.IsLaunched)
                {
                    if (Game1.activeClickableMenu is not TitleMenu)
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
                if (config.SkipTo is Screen.HostCoop or Screen.JoinCoop)
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

            // apply skip step
            return currentStage switch
            {
                Stage.SkipIntro => this.SkipToTitle(menu),
                Stage.TransitionToLoad => this.TransitionToLoad(menu),
                Stage.StartTransitionToCoop => this.StartTransitionToCoop(menu),
                Stage.TransitionToCoop => this.TransitionToCoop(menu),
                Stage.TransitionToCoopHost => this.TransitionToCoopHost(),
                _ => Stage.None
            };
        }

        /// <summary>Skip to the title screen.</summary>
        /// <param name="menu">The title menu.</param>
        /// <returns>Returns the next step in the skip logic.</returns>
        private Stage SkipToTitle(TitleMenu menu)
        {
            // skip to title screen
            menu.skipToTitleButtons();

            // avoid game crash since Game1.currentSong isn't set yet
            Game1.currentSong ??= Game1.soundBank.GetCue("MainTheme");

            // skip button transition
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                while (menu.isTransitioningButtons)
                    menu.update(Game1.currentGameTime);
            }
            else
            {
                while (menu.buttonsToShow < TitleMenu.numberOfButtons)
                    menu.update(Game1.currentGameTime);
            }

            // set next step
            switch (this.Config.SkipTo)
            {
                case Screen.Title:
                    return Stage.None;

                case Screen.Load:
                    return Stage.TransitionToLoad;

                case Screen.JoinCoop:
                case Screen.HostCoop:
                    return Stage.StartTransitionToCoop;

                default:
                    this.Monitor.Log($"Unrecognized skip option {this.Config.SkipTo}.", LogLevel.Warn);
                    return Stage.None;
            }
        }

        /// <summary>Skip from the title screen to the load menu.</summary>
        /// <param name="menu">The title menu.</param>
        /// <returns>Returns the next step in the skip logic.</returns>
        private Stage TransitionToLoad(TitleMenu menu)
        {
            // start transition
            menu.performButtonAction("Load");

            // skip animation
            while (TitleMenu.subMenu == null)
                menu.update(Game1.currentGameTime);

            return Stage.None;
        }

        /// <summary>Start transitioning from the title screen to the co-op section.</summary>
        /// <param name="menu">The title menu.</param>
        /// <returns>Returns the next step in the skip logic.</returns>
        private Stage StartTransitionToCoop(TitleMenu menu)
        {
            // wait until the game client SDK is ready, which is needed to load the co-op menus
            SDKHelper sdk = this.Helper.Reflection.GetProperty<SDKHelper>(typeof(Program), "sdk").GetValue();
            if (!sdk.ConnectionFinished)
                return Stage.StartTransitionToCoop;

            // start transition
            menu.performButtonAction("Co-op");

            // need a full game update before the next step to avoid crashes
            return Stage.TransitionToCoop;
        }

        /// <summary>Finish transitioning from the title screen to the co-op section.</summary>
        /// <param name="menu">The title menu.</param>
        /// <returns>Returns the next step in the skip logic.</returns>
        private Stage TransitionToCoop(TitleMenu menu)
        {
            // skip animation
            while (TitleMenu.subMenu == null)
                menu.update(Game1.currentGameTime);

            // prevent a crash in CoopMenu.gameWindowSizeChanged if it's called before connectionFinished
            if (TitleMenu.subMenu is CoopMenu { joinTab: null } coop)
            {
                coop.joinTab = new ClickableComponent(Rectangle.Empty, "");
                coop.hostTab = new ClickableComponent(Rectangle.Empty, "");
                coop.refreshButton = new ClickableComponent(Rectangle.Empty, "");
            }

            // set next step
            return this.Config.SkipTo == Screen.HostCoop
                ? Stage.TransitionToCoopHost
                : Stage.None;
        }

        /// <summary>Skip from the co-op section to the host screen.</summary>
        /// <returns>Returns the next step in the skip logic.</returns>
        private Stage TransitionToCoopHost()
        {
            // not applicable
            if (TitleMenu.subMenu is not CoopMenu submenu)
                return Stage.None;

            // not connected yet
            if (submenu.hostTab == null)
                return Stage.TransitionToCoopHost;

            // select host tab
            submenu.receiveLeftClick(submenu.hostTab.bounds.X, submenu.hostTab.bounds.Y, playSound: false);
            return Stage.None;
        }
    }
}
