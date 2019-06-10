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
using StardewValley.Locations;
using StardewValley.Menus;

namespace Pathoschild.Stardew.ChestsAnywhere
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys;

        /// <summary>The internal mod settings.</summary>
        private ModData Data;

        /// <summary>Encapsulates logic for finding chests.</summary>
        private ChestFactory ChestFactory;


        /****
        ** State
        ****/
        /// <summary>The selected in-game inventory.</summary>
        private IList<Item> SelectedInventory;

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
            this.Keys = this.Config.Controls.ParseControls(this.Monitor);
            this.Data = helper.Data.ReadJsonFile<ModData>("data.json") ?? new ModData();
            this.ChestFactory = new ChestFactory(helper.Translation, helper.Data, this.Config.EnableShippingBin);

            // hook events
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

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
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // validate game version
            string versionError = this.ValidateGameVersion();
            if (versionError != null)
            {
                this.Monitor.Log(versionError, LogLevel.Error);
                CommonHelper.ShowErrorMessage(versionError);
            }

            // show multiplayer limitations warning
            if (!Context.IsMainPlayer)
                this.Monitor.Log("Multiplayer limitations: you can only access chests in your current location (since you're not the main player). This is due to limitations in the game's sync logic.", LogLevel.Info);
        }

        /// <summary>The method invoked when the interface has finished rendering.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            // show chest label
            if (this.Config.ShowHoverTooltips)
            {
                ManagedChest cursorChest = this.ChestFactory.GetChestFromTile(Game1.currentCursorTile);
                if (cursorChest != null && !cursorChest.HasDefaultName())
                {
                    Vector2 tooltipPosition = new Vector2(Game1.getMouseX(), Game1.getMouseY()) + new Vector2(Game1.tileSize / 2f);
                    CommonHelper.DrawHoverBox(e.SpriteBatch, cursorChest.DisplayName, tooltipPosition, Game1.viewport.Width - tooltipPosition.X - Game1.tileSize / 2f);
                }
            }
        }

        /// <summary>Raised before the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            this.ChangeOverlayIfNeeded();
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            this.ChangeOverlayIfNeeded();
        }

        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            try
            {
                ModConfigKeys keys = this.Keys;

                // open menu
                if (keys.Toggle.Contains(e.Button))
                {
                    // open if no conflict
                    if (Game1.activeClickableMenu == null)
                        this.OpenMenu();

                    // open from inventory if it's safe to close the inventory screen
                    else if (Game1.activeClickableMenu is GameMenu gameMenu && gameMenu.currentTab == GameMenu.inventoryTab)
                    {
                        IClickableMenu inventoryPage = this.Helper.Reflection.GetField<List<IClickableMenu>>(gameMenu, "pages").GetValue()[GameMenu.inventoryTab];
                        if (inventoryPage.readyToClose())
                            this.OpenMenu();
                    }
                }
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "handling key input");
            }
        }

        /// <summary>Change the chest UI overlay if needed to match the current menu.</summary>
        /// <remarks>Since the menu gets reopened whenever the chest inventory changes, this method needs to be called before/after tick to avoid a visible UI flicker.</remarks>
        private void ChangeOverlayIfNeeded()
        {
            // already matches menu
            if (this.ManageChestOverlay?.ForMenuInstance == Game1.activeClickableMenu)
                return;

            // remove old overlay
            if (this.ManageChestOverlay != null)
            {
                this.ManageChestOverlay?.Dispose();
                this.ManageChestOverlay = null;
            }

            // add new overlay
            if (Game1.activeClickableMenu is ItemGrabMenu chestMenu)
            {
                // get open chest
                ManagedChest chest = this.ChestFactory.GetChestFromMenu(chestMenu);
                if (chest == null)
                    return;

                // reopen shipping box in standard chest UI if needed
                // This is called in two cases:
                // - When the player opens the shipping bin directly, it opens the shipping bin view instead of the full chest view.
                // - When the player changes the items in the chest view, it reopens itself but loses the constructor args (e.g. highlight function).
                if (this.Config.EnableShippingBin && chest.Container is ShippingBinContainer && (!chestMenu.showReceivingMenu || !(chestMenu.inventory.highlightMethod?.Target is ShippingBinContainer)))
                {
                    chestMenu = chest.OpenMenu();
                    Game1.activeClickableMenu = chestMenu;
                }

                // add overlay
                RangeHandler range = this.GetCurrentRange();
                ManagedChest[] chests = this.ChestFactory.GetChests(range, excludeHidden: true, alwaysIncludeContainer: chest.Container).ToArray();
                bool isAutomateInstalled = this.Helper.ModRegistry.IsLoaded("Pathoschild.Automate");
                this.ManageChestOverlay = new ManageChestOverlay(chestMenu, chest, chests, this.Config, this.Keys, this.Helper.Events, this.Helper.Input, this.Helper.Translation, showAutomateOptions: isAutomateInstalled && chest.CanConfigureAutomate);
                this.ManageChestOverlay.OnChestSelected += selected =>
                {
                    this.SelectedInventory = selected.Container.Inventory;
                    Game1.activeClickableMenu = selected.OpenMenu();
                };
            }
        }

        /// <summary>Open the menu UI.</summary>
        private void OpenMenu()
        {
            if (this.Config.Range == ChestRange.None)
                return;

            // handle disabled location
            if (this.IsDisabledLocation(Game1.currentLocation))
            {
                CommonHelper.ShowInfoMessage(this.Helper.Translation.Get("errors.disabled-from-here"), duration: 1000);
                return;
            }

            // get chests
            RangeHandler range = this.GetCurrentRange();
            ManagedChest[] chests = this.ChestFactory.GetChests(range, excludeHidden: true).ToArray();
            ManagedChest selectedChest = chests.FirstOrDefault(p => p.Container.IsSameAs(this.SelectedInventory)) ?? chests.FirstOrDefault();

            // show error
            if (selectedChest == null)
            {
                string translationKey = this.GetNoChestsFoundErrorKey();
                CommonHelper.ShowInfoMessage(this.Helper.Translation.Get(translationKey), duration: 1000);
                return;
            }

            // render menu
            Game1.activeClickableMenu = selectedChest.OpenMenu();
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

        /// <summary>Get whether remote access is disabled from the given location.</summary>
        /// <param name="location">The game location.</param>
        private bool IsDisabledLocation(GameLocation location)
        {
            if (this.Config.DisabledInLocations == null)
                return false;

            return
                this.Config.DisabledInLocations.Contains(location.Name)
                || (location is MineShaft && location.Name.StartsWith("UndergroundMine") && this.Config.DisabledInLocations.Contains("UndergroundMine"));
        }

        /// <summary>Get the range for the current context.</summary>
        private RangeHandler GetCurrentRange()
        {
            ChestRange range = this.IsDisabledLocation(Game1.currentLocation)
                ? ChestRange.None
                : this.Config.Range;
            return new RangeHandler(this.Data.WorldAreas, range, Game1.currentLocation);
        }

        /// <summary>Get the error translation key to show if no chests were found.</summary>
        private string GetNoChestsFoundErrorKey()
        {
            if (this.Config.Range == ChestRange.CurrentLocation || !Context.IsMainPlayer)
                return "errors.no-chests-in-location";

            if (this.Config.Range != ChestRange.Unlimited)
                return "errors.no-chests-in-range";

            return "errors.no-chests";
        }
    }
}
