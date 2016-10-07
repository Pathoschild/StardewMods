using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChestsAnywhere.Common;
using ChestsAnywhere.Framework;
using ChestsAnywhere.Menus;
using ChestsAnywhere.Menus.Components;
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

        /// <summary>Whether the update-available message has been shown since the game started.</summary>
        private bool HasSeenUpdateWarning;

        /****
        ** State
        ****/
        /// <summary>The selected chest.</summary>
        private Chest SelectedChest;

        /// <summary>The context for the chest currently being viewed or edited.</summary>
        private EditChestContext EditContext;


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
            PlayerEvents.LoadedGame += (sender, e) => this.ReceiveGameLoaded();
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
            // validate version
            string versionError = this.ValidateGameVersion();
            if (versionError != null)
            {
                Log.Error(versionError);
                CommonHelper.ShowErrorMessage(versionError);
            }

            // check for an updated version
            if (this.Config.CheckForUpdates)
            {
                try
                {
                    Task.Factory.StartNew(() =>
                    {
                        GitRelease release = UpdateHelper.GetLatestReleaseAsync("Pathoschild/ChestsAnywhere").Result;
                        if (release.IsNewerThan(this.CurrentVersion))
                            this.NewRelease = release;
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
            ManagedChest cursorChest = ChestFactory.GetChestFromTile(Game1.currentCursorTile);
            if (cursorChest != null)
            {
                Vector2 tooltipPosition = new Vector2(Game1.getMouseX(), Game1.getMouseY()) + new Vector2(Game1.tileSize / 2f);
                CommonHelper.DrawHoverBox(Game1.spriteBatch, cursorChest.Name, tooltipPosition, Game1.viewport.Width - tooltipPosition.X - Game1.tileSize / 2f);
            }
        }

        /// <summary>The method invoked when the active menu changes.</summary>
        /// <param name="previousMenu">The previous menu (if any)</param>
        /// <param name="newMenu">The new menu (if any).</param>
        private void ReceiveMenuChanged(IClickableMenu previousMenu, IClickableMenu newMenu)
        {
            // open chest view (start context)
            if (this.EditContext == null)
            {
                if (newMenu is ItemGrabMenu)
                {
                    // find open chest
                    ManagedChest chest = ChestFactory.GetChests().FirstOrDefault(p => p.Chest.currentLidFrame == 135);
                    if (chest == null)
                        return;

                    // create edit button
                    Rectangle sprite = Sprites.Icons.SpeechBubble;
                    float zoom = Game1.pixelZoom / 2f;
                    int width = (int)(sprite.Width * zoom);
                    int height = (int)(sprite.Height * zoom);
                    int x = newMenu.xPositionOnScreen - width;
                    int y = newMenu.yPositionOnScreen;
                    var editButton = new EditButtonOverlay(new Rectangle(x, y, width, height), () => chest, this.Config, () => this.EditContext != null);

                    // track context
                    this.EditContext = new EditChestContext { Chest = () => chest, ViewMenu = newMenu, EditButton = editButton };
                }
                else if (newMenu is AccessChestMenu)
                {
                    AccessChestMenu menu = (AccessChestMenu)newMenu;
                    this.EditContext = new EditChestContext { Chest = () => menu.SelectedChest, ViewMenu = newMenu };
                }
            }

            // open edit
            else if (newMenu is EditChestForm)
            {
                this.EditContext.EditMenu = newMenu;
                if (this.EditContext.EditButton != null)
                    this.EditContext.EditButton.IsDisabled = true;
            }

            // close edit & resume view
            else if (previousMenu is EditChestForm && newMenu != this.EditContext.ViewMenu)
            {
                // close edit
                this.EditContext.EditMenu = null;
                if (this.EditContext.EditButton != null)
                    this.EditContext.EditButton.IsDisabled = false;

                // resume view
                if (this.EditContext.ViewMenu is AccessChestMenu)
                {
                    // reset to reflect changes
                    this.EditContext.Dispose();
                    this.EditContext = null;
                    this.OpenMenu();
                }
                else
                    Game1.activeClickableMenu = this.EditContext.ViewMenu;
            }

            // close menu (end context)
            else if (newMenu == null)
            {
                this.EditContext?.Dispose();
                this.EditContext = null;
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
                if (key.Equals(map.Toggle) && Game1.activeClickableMenu == null)
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
            ManagedChest[] chests = (
                from chest in ChestFactory.GetChests()
                where !chest.IsIgnored
                orderby chest.GetGroup() ascending, (chest.Order ?? int.MaxValue) ascending, chest.Name ascending
                select chest
            ).ToArray();
            ManagedChest selectedChest = chests.FirstOrDefault(p => p.Chest == this.SelectedChest) ?? chests.First();

            // render menu
            if (chests.Any())
            {
                AccessChestMenu menu = new AccessChestMenu(chests, selectedChest, this.Config);
                menu.OnChestSelected += chest => this.SelectedChest = chest.Chest; // remember selected chest on next load
                Game1.activeClickableMenu = menu;
            }
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
