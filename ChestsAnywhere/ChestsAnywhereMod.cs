using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Menus.Overlays;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

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

        /// <summary>Encapsulates logic for finding chests.</summary>
        private ChestFactory ChestFactory;


        /****
        ** State
        ****/
        /// <summary>The selected in-game container instance.</summary>
        private object SelectedTarget;

        /// <summary>The menu overlay which lets the player navigate and edit chests.</summary>
        private ManageChestOverlay ManageChestOverlay;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // initialise
            this.Config = helper.ReadConfig<RawModConfig>().GetParsed();
            this.ChestFactory = new ChestFactory(helper.Translation);

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

            // hook game events
            SaveEvents.AfterLoad += this.ReceiveAfterLoad;

            // validate translations
            if (!helper.Translation.GetTranslations().Any())
                this.Monitor.Log("The translation files in this mod's i18n folder seem to be missing. The mod will still work, but you'll see 'missing translation' messages. Try reinstalling the mod to fix this.", LogLevel.Warn);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked after the player loads a saved game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveAfterLoad(object sender, EventArgs e)
        {
            // validate game version
            string versionError = this.ValidateGameVersion();
            if (versionError != null)
            {
                this.Monitor.Log(versionError, LogLevel.Error);
                CommonHelper.ShowErrorMessage(versionError);
            }
        }

        /// <summary>The method invoked when the interface has finished rendering.</summary>
        private void ReceiveHudRendered()
        {
            // show chest label
            if (this.Config.ShowHoverTooltips)
            {
                ManagedChest cursorChest = this.ChestFactory.GetChestFromTile(Game1.currentCursorTile);
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
            if (newMenu is ItemGrabMenu chestMenu)
            {
                // get open chest
                ManagedChest chest = this.ChestFactory.GetChestFromMenu(chestMenu);
                if (chest == null)
                    return;

                // add overlay
                ManagedChest[] chests = this.ChestFactory.GetChestsForDisplay(selected: chest.Container).ToArray();
                this.ManageChestOverlay = new ManageChestOverlay(chestMenu, chest, chests, this.Config, this.Helper.Translation);
                this.ManageChestOverlay.OnChestSelected += selected =>
                {
                    this.SelectedTarget = selected.Container.Instance;
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
            ManagedChest[] chests = this.ChestFactory.GetChestsForDisplay().ToArray();
            ManagedChest selectedChest = chests.FirstOrDefault(p => p.Container.Instance == this.SelectedTarget) ?? chests.FirstOrDefault();

            // render menu
            if (selectedChest != null)
                Game1.activeClickableMenu = selectedChest.OpenMenu();
            else
                CommonHelper.ShowInfoMessage("You don't have any chests yet. :)", duration: 1000);
        }

        /// <summary>Validate that the game versions match the minimum requirements, and return an appropriate error message if not.</summary>
        private string ValidateGameVersion()
        {
            if (Constant.MinimumApiVersion.IsNewerThan(Constants.ApiVersion))
                return $"The Chests Anywhere mod requires a newer version of SMAPI. Please update SMAPI from {Constants.ApiVersion} to {Constant.MinimumApiVersion}.";

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
