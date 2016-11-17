using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Common;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Menus.Overlays;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace Pathoschild.Stardew.ChestsAnywhere
{
    /// <summary>The mod entry point.</summary>
    public class ChestsAnywhereMod : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /****
        ** Version check
        ****/
        /// <summary>The current semantic version.</summary>
        private string CurrentVersion;

        /// <summary>The newer release to notify the user about.</summary>
        private GitRelease NewRelease;

        /// <summary>Whether the update-available message has been shown since the game started.</summary>
        private bool HasSeenUpdateWarning;

        /****
        ** State
        ****/
        /// <summary>The selected chest.</summary>
        private Chest SelectedChest;

        /// <summary>The menu overlay which lets the player navigate and edit chests.</summary>
        private ManageChestOverlay ManageChestOverlay;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<RawModConfig>().GetParsed();
            this.CurrentVersion = UpdateHelper.GetSemanticVersion(this.Manifest.Version);

            // hook UI
            GameEvents.GameLoaded += (sender, e) => this.ReceiveGameLoaded();
            GraphicsEvents.OnPostRenderHudEvent += (sender, e) => this.ReceiveHudRendered();
            MenuEvents.MenuChanged += (sender, e) => this.ReceiveMenuChanged(e.PriorMenu, e.NewMenu);
            MenuEvents.MenuClosed += (sender, e) => this.ReceiveMenuClosed(e.PriorMenu);

            // hook input
            if (this.Config.Keyboard.HasAny())
                ControlEvents.KeyPressed += (sender, e) => this.ReceiveKeyPress(e.KeyPressed, this.Config.Keyboard);
            if (this.Config.Controller.HasAny())
            {
                ControlEvents.ControllerButtonPressed += (sender, e) => this.ReceiveKeyPress(e.ButtonPressed, this.Config.Controller);
                ControlEvents.ControllerTriggerPressed += (sender, e) => this.ReceiveKeyPress(e.ButtonPressed, this.Config.Controller);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player loads the game.</summary>
        private void ReceiveGameLoaded()
        {
            // validate game version
            string versionError = this.ValidateGameVersion();
            if (versionError != null)
            {
                this.Monitor.Log(versionError, LogLevel.Error);
                CommonHelper.ShowErrorMessage(versionError);
            }

            // check for mod update
            if (this.Config.CheckForUpdates)
            {
                try
                {
                    Task.Factory.StartNew(() =>
                    {
                        // get version
                        GitRelease release;
                        try
                        {
                            release = UpdateHelper.GetLatestReleaseAsync("Pathoschild/ChestsAnywhere").Result;
                        }
                        catch (Exception ex)
                        {
                            this.Monitor.Log("Couldn't check for an updated version. This won't affect your game, but you may not be notified of new versions if this keeps happening. Error details are shown in the log.", LogLevel.Warn);
                            this.Monitor.Log(ex.ToString(), LogLevel.Trace);
                            return;
                        }

                        // validate
                        if (release.IsNewerThan(this.CurrentVersion))
                        {
                            this.Monitor.Log($"Update to version {release.Name} available.", LogLevel.Alert);
                            this.NewRelease = release;
                        }
                        else
                            this.Monitor.Log("Checking for update... none found.", LogLevel.Trace);
                    });
                }
                catch (Exception ex)
                {
                    this.HandleError(ex, "checking for a new version");
                }
            }
        }

        /// <summary>The method invoked when the interface has finished rendering.</summary>
        private void ReceiveHudRendered()
        {
            // render update warning
            if (this.Config.CheckForUpdates && !this.HasSeenUpdateWarning && this.NewRelease != null)
            {
                try
                {
                    this.HasSeenUpdateWarning = true;
                    CommonHelper.ShowInfoMessage($"You can update Chests Anywhere from {this.CurrentVersion} to {this.NewRelease.Version}.");
                }
                catch (Exception ex)
                {
                    this.HandleError(ex, "showing the new version available");
                }
            }

            // show chest label
            if (this.Config.ShowHoverTooltips)
            {
                ManagedChest cursorChest = ChestFactory.GetChestFromTile(Game1.currentCursorTile);
                if (cursorChest != null)
                {
                    Vector2 tooltipPosition = new Vector2(Game1.getMouseX(), Game1.getMouseY()) + new Vector2(Game1.tileSize / 2f);
                    CommonHelper.DrawHoverBox(Game1.spriteBatch, cursorChest.Name, tooltipPosition, Game1.viewport.Width - tooltipPosition.X - Game1.tileSize / 2f);
                }
            }
        }

        /// <summary>The method invoked when the active menu changes.</summary>
        /// <param name="previousMenu">The previous menu (if any)</param>
        /// <param name="newMenu">The new menu (if any).</param>
        private void ReceiveMenuChanged(IClickableMenu previousMenu, IClickableMenu newMenu)
        {
            // remove overlay
            if (previousMenu is ItemGrabMenu)
            {
                this.ManageChestOverlay?.Dispose();
                this.ManageChestOverlay = null;
            }

            // add overlay
            if (newMenu is ItemGrabMenu)
            {
                // get open chest
                ItemGrabMenu chestMenu = (ItemGrabMenu)newMenu;
                ManagedChest chest = ChestFactory.GetChestFromMenu(chestMenu);
                if (chest == null)
                    return;

                // add overlay
                ManagedChest[] chests = ChestFactory.GetChestsForDisplay(selectedChest: chest.Chest).ToArray();
                this.ManageChestOverlay = new ManageChestOverlay(chestMenu, chest, chests, this.Config);
                this.ManageChestOverlay.OnChestSelected += selected =>
                {
                    this.SelectedChest = selected.Chest;
                    Game1.activeClickableMenu = selected.OpenMenu();
                };
            }
        }

        /// <summary>The method invoked when a menu is closed.</summary>
        /// <param name="closedMenu">The menu that was closed.</param>
        private void ReceiveMenuClosed(IClickableMenu closedMenu)
        {
            this.ReceiveMenuChanged(closedMenu, null);
        }

        /// <summary>The method invoked when the player presses an input button.</summary>
        /// <typeparam name="TKey">The input type.</typeparam>
        /// <param name="key">The pressed input.</param>
        /// <param name="map">The configured input mapping.</param>
        private void ReceiveKeyPress<TKey>(TKey key, InputMapConfiguration<TKey> map)
        {
            try
            {
                if (!map.IsValidKey(key))
                    return;

                // open menu
                if (key.Equals(map.Toggle) && (Game1.activeClickableMenu == null || (Game1.activeClickableMenu as GameMenu)?.currentTab == 0))
                    this.OpenMenu();
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "handling key input");
            }
        }

        /// <summary>Open the menu UI.</summary>
        private void OpenMenu()
        {
            // get chests
            ManagedChest[] chests = ChestFactory.GetChestsForDisplay().ToArray();
            ManagedChest selectedChest = chests.FirstOrDefault(p => p.Chest == this.SelectedChest) ?? chests.FirstOrDefault();

            // render menu
            if (selectedChest != null)
                Game1.activeClickableMenu = selectedChest.OpenMenu();
            else
                CommonHelper.ShowInfoMessage("You don't have any chests yet. :)", duration: 1000);
        }

        /// <summary>Validate that the game versions match the minimum requirements, and return an appropriate error message if not.</summary>
        private string ValidateGameVersion()
        {
            string gameVersion = Regex.Replace(Game1.version, "^([0-9.]+).*", "$1");
            string apiVersion = Constants.Version.ToString();

            if (string.Compare(gameVersion, Constant.MinimumGameVersion, StringComparison.InvariantCultureIgnoreCase) == -1)
                return $"The Chests Anywhere mod requires a newer version of the game. Please update Stardew Valley from {gameVersion} to {Constant.MinimumGameVersion}.";
            if (string.Compare(apiVersion, Constant.MinimumApiVersion, StringComparison.InvariantCultureIgnoreCase) == -1)
                return $"The Chests Anywhere mod requires a newer version of SMAPI. Please update SMAPI from {apiVersion} to {Constant.MinimumApiVersion}.";

            return null;
        }

        /// <summary>Log an error and warn the user.</summary>
        /// <param name="ex">The exception to handle.</param>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up").</param>
        private void HandleError(Exception ex, string verb)
        {
            this.Monitor.Log($"Something went wrong {verb}:\n{ex}", LogLevel.Error);
            CommonHelper.ShowErrorMessage($"Huh. Something went wrong {verb}. The error log has the technical details.");
        }
    }
}
