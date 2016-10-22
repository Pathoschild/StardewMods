using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChestsAnywhere.Common;
using ChestsAnywhere.Framework;
using ChestsAnywhere.Menus.Overlays;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ChestsAnywhere
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

        /// <summary>The update check didn't pass through.</summary>
        private bool Errored;

        /// <summary>Whether the update-available message has been shown since the game started.</summary>
        private bool HasSeenUpdateWarning;

        /// <summary>Whether the game version verification  has been shown since the game started.</summary>
        private bool HasVerifiedGameVersion;

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
        /// <summary>Initialise the mod.</summary>
        public override void Entry(params object[] objects)
        {
            // read config
            this.Config = new RawModConfig().InitializeConfig(this.BaseConfigPath).GetParsed();
            this.CurrentVersion = UpdateHelper.GetSemanticVersion(this.Manifest.Version);

            // hook UI
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

            if (this.Config.CheckForUpdates)
                this.CheckforUpdates();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Checks if the current version of SMAPI or Stardew Valley can run the mod.</summary>
        private void VerifyGameVersion()
        {
            // validate version
            string versionError = this.ValidateGameVersion();
            if (versionError == null) return;

            Log.Error(versionError);
            CommonHelper.ShowErrorMessage(versionError);
        }
        
        /// <summary>Checks if the mod has any avilable updates.</summary>
        private void CheckforUpdates()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    GitRelease release = UpdateHelper.GetLatestReleaseAsync("Pathoschild/ChestsAnywhere").Result;

                    if (release.Errored) {
                        this.Errored = true;
                        return;
                    }

                    if (release.IsNewerThan(this.CurrentVersion))
                        this.NewRelease = release;
                });
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "checking for a new version");
            }
        }

        /// <summary>The method invoked when the interface has finished rendering.</summary>
        private void ReceiveHudRendered()
        {
            if (!this.HasVerifiedGameVersion)
                this.VerifyGameVersion();

            // checks and renders update information
            if (this.Config.CheckForUpdates && !this.HasSeenUpdateWarning)
            {
                this.HasSeenUpdateWarning = true;
                if (this.NewRelease != null)
                {
                    try
                    {
                        CommonHelper.ShowInfoMessage($"You can update Chests Anywhere from {this.CurrentVersion} to {this.NewRelease.Version}.");
                    }
                    catch (Exception ex)
                    {
                        this.HandleError(ex, "showing the new version available");
                    }
                }

                if (this.Errored)
                {
                    try
                    {
                        CommonHelper.ShowInfoMessage($"ChestsAnywhere: Updates check failed");
                    }
                    catch {}
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
                var gamepadOverride =
                    Game1.options.gamepadControls && // Is Gamepad Enabled?
                    Game1.activeClickableMenu is GameMenu && // Is the active menu the Game Menu?
                    ((GameMenu) Game1.activeClickableMenu).currentTab == 0; // Is the active tab in the Game Menu your Inventory Page?
                if (key.Equals(map.Toggle) && (Game1.activeClickableMenu == null || gamepadOverride))
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
            string apiVersion = Constants.Version.VersionString;

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
            CommonHelper.ShowErrorMessage($"Huh. Something went wrong {verb}. The game error log has the technical details.");
            Log.Error(ex.ToString());
        }
    }
}
