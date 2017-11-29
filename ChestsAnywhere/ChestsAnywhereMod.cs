using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework;
using Pathoschild.Stardew.ChestsAnywhere.Framework.Containers;
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
        /// <summary>The selected in-game inventory.</summary>
        private List<Item> SelectedInventory;

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
            this.Config = helper.ReadConfig<ModConfig>();
            this.ChestFactory = new ChestFactory(helper.Translation, helper.Reflection);

            // hook UI
            GraphicsEvents.OnPostRenderHudEvent += this.GraphicsEvents_OnPostRenderHudEvent;
            MenuEvents.MenuChanged += this.MenuEvents_MenuChanged;
            MenuEvents.MenuClosed += this.MenuEvents_MenuClosed;

            // hook input
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;

            // hook game events
            SaveEvents.AfterLoad += this.SaveEvents_AfterLoad;

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
        private void SaveEvents_AfterLoad(object sender, EventArgs e)
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
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GraphicsEvents_OnPostRenderHudEvent(object sender, EventArgs e)
        {
            // show chest label
            if (this.Config.ShowHoverTooltips)
            {
                ManagedChest cursorChest = this.ChestFactory.GetChestFromTile(Game1.currentCursorTile);
                if (cursorChest != null && !cursorChest.HasDefaultName())
                {
                    Vector2 tooltipPosition = new Vector2(Game1.getMouseX(), Game1.getMouseY()) + new Vector2(Game1.tileSize / 2f);
                    CommonHelper.DrawHoverBox(Game1.spriteBatch, cursorChest.Name, tooltipPosition, Game1.viewport.Width - tooltipPosition.X - Game1.tileSize / 2f);
                }
            }
        }

        /// <summary>The method invoked when the active menu changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            // remove overlay
            if (e.PriorMenu is ItemGrabMenu)
            {
                this.ManageChestOverlay?.Dispose();
                this.ManageChestOverlay = null;
            }

            // add overlay
            if (e.NewMenu is ItemGrabMenu chestMenu)
            {
                // get open chest
                ManagedChest chest = this.ChestFactory.GetChestFromMenu(chestMenu);
                if (chest == null)
                    return;

                // reopen shipping box in standard chest UI
                if (chest.Container is ShippingBinContainer && !chestMenu.showReceivingMenu)
                    Game1.activeClickableMenu = chest.OpenMenu();

                // add overlay
                ManagedChest[] chests = this.ChestFactory.GetChestsForDisplay(selected: chest.Container).ToArray();
                this.ManageChestOverlay = new ManageChestOverlay(chestMenu, chest, chests, this.Config, this.Helper.Translation);
                this.ManageChestOverlay.OnChestSelected += selected =>
                {
                    this.SelectedInventory = selected.Container.Inventory;
                    Game1.activeClickableMenu = selected.OpenMenu();
                };
            }
        }

        /// <summary>The method invoked when a menu is closed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            this.MenuEvents_MenuChanged(sender, new EventArgsClickableMenuChanged(e.PriorMenu, null));
        }

        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            try
            {
                var controls = this.Config.Controls;

                // open menu
                if (controls.Toggle.Contains(e.Button) && (Game1.activeClickableMenu == null || (Game1.activeClickableMenu as GameMenu)?.currentTab == 0))
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
            ManagedChest selectedChest = chests.FirstOrDefault(p => p.Container.Inventory == this.SelectedInventory) ?? chests.FirstOrDefault();

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
