using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.CustomFarmingRedux;
using Pathoschild.Stardew.Common.Integrations.JsonAssets;
using Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod;
using Pathoschild.Stardew.LookupAnything.Components;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Subjects;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /****
        ** Configuration
        ****/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys;

        /// <summary>Provides metadata that's not available from the game data directly.</summary>
        private Metadata Metadata;

        /// <summary>The name of the file containing data for the <see cref="Metadata"/> field.</summary>
        private readonly string DatabaseFileName = "data.json";

        /****
        ** Validation
        ****/
        /// <summary>Whether the metadata validation passed.</summary>
        private bool IsDataValid;

        /****
        ** State
        ****/
        /// <summary>The previous menus shown before the current lookup UI was opened.</summary>
        private readonly Stack<IClickableMenu> PreviousMenus = new Stack<IClickableMenu>();

        /// <summary>Provides utility methods for interacting with the game code.</summary>
        private GameHelper GameHelper;

        /// <summary>Provides subject entries for target values.</summary>
        private SubjectFactory SubjectFactory;

        /// <summary>Finds and analyzes lookup targets in the world.</summary>
        private TargetFactory TargetFactory;

        /// <summary>Draws debug information to the screen.</summary>
        private DebugInterface DebugInterface;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // load config
            this.Config = this.LoadConfig();
            this.Keys = this.Config.Controls.ParseControls(helper.Input, this.Monitor);

            // load translations
            L10n.Init(helper.Translation);

            // load & validate database
            this.LoadMetadata();
            this.IsDataValid = this.Metadata.LooksValid();
            if (!this.IsDataValid)
            {
                this.Monitor.Log("The data.json file seems to be missing or corrupt. Lookups will be disabled.", LogLevel.Error);
                this.IsDataValid = false;
            }

            // validate translations
            if (!helper.Translation.GetTranslations().Any())
                this.Monitor.Log("The translation files in this mod's i18n folder seem to be missing. The mod will still work, but you'll see 'missing translation' messages. Try reinstalling the mod to fix this.", LogLevel.Warn);

            // hook up events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            if (this.Config.HideOnKeyUp)
                helper.Events.Input.ButtonReleased += this.OnButtonReleased;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The method invoked on the first update tick, once all mods are initialized.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (!this.IsDataValid)
                return;

            // get mod APIs
            JsonAssetsIntegration jsonAssets = new JsonAssetsIntegration(this.Helper.ModRegistry, this.Monitor);

            // initialize functionality
            var customFarming = new CustomFarmingReduxIntegration(this.Helper.ModRegistry, this.Monitor);
            var producerFramework = new ProducerFrameworkModIntegration(this.Helper.ModRegistry, this.Monitor);
            this.GameHelper = new GameHelper(customFarming, producerFramework, this.Metadata, this.Helper.Reflection);
            this.SubjectFactory = new SubjectFactory(this.Metadata, this.Helper.Translation, this.Helper.Reflection, this.GameHelper, this.Config);
            this.TargetFactory = new TargetFactory(this.Helper.Reflection, this.GameHelper, jsonAssets, this.SubjectFactory);
            this.DebugInterface = new DebugInterface(this.GameHelper, this.TargetFactory, this.Config, this.Monitor);
        }

        /// <summary>The method invoked when a new day starts.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // reset low-level cache once per game day (used for expensive queries that don't change within a day)
            this.GameHelper.ResetCache(this.Helper.Reflection, this.Monitor);
        }

        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            this.Monitor.InterceptErrors("handling your input", $"handling input '{e.Button}'", () =>
            {
                ModConfigKeys keys = this.Keys;

                if (keys.ToggleSearch.JustPressedUnique())
                    this.TryToggleSearch();
                else if (keys.ToggleLookup.JustPressedUnique())
                    this.ToggleLookup();
                else if (keys.ScrollUp.JustPressedUnique())
                    (Game1.activeClickableMenu as LookupMenu)?.ScrollUp();
                else if (keys.ScrollDown.JustPressedUnique())
                    (Game1.activeClickableMenu as LookupMenu)?.ScrollDown();
                else if (keys.ToggleDebug.JustPressedUnique() && Context.IsPlayerFree)
                    this.DebugInterface.Enabled = !this.DebugInterface.Enabled;
            });
        }

        /// <summary>The method invoked when the player releases a button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            // perform bound action
            this.Monitor.InterceptErrors("handling your input", $"handling input release '{e.Button}'", () =>
            {
                ModConfigKeys keys = this.Keys;

                if (keys.ToggleLookup.JustPressedUnique())
                    this.HideLookup();
            });
        }

        /// <summary>The method invoked when the player closes a displayed menu.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // restore the previous menu if it was hidden to show the lookup UI
            this.Monitor.InterceptErrors("restoring the previous menu", () =>
            {
                if (e.NewMenu == null && e.OldMenu is LookupMenu && this.PreviousMenus.Any())
                    Game1.activeClickableMenu = this.PreviousMenus.Pop();
            });
        }

        /// <summary>The method invoked when the interface is rendering.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            // render debug interface
            if (this.DebugInterface.Enabled)
                this.DebugInterface.Draw(Game1.spriteBatch);
        }

        /****
        ** Lookup menu helpers
        ****/
        /// <summary>Show the lookup UI for the current target.</summary>
        private void ToggleLookup()
        {
            if (Game1.activeClickableMenu is LookupMenu)
                this.HideLookup();
            else
                this.ShowLookup();
        }

        /// <summary>Show the lookup UI for the current target.</summary>
        private void ShowLookup()
        {
            // disable lookups if metadata is invalid
            if (!this.IsDataValid)
            {
                this.GameHelper.ShowErrorMessage("The mod doesn't seem to be installed correctly: its data.json file is missing or corrupt.");
                return;
            }

            // show menu
            StringBuilder logMessage = new StringBuilder("Received a lookup request...");
            this.Monitor.InterceptErrors("looking that up", () =>
            {
                try
                {
                    // get target
                    ISubject subject = this.GetSubject(logMessage);
                    if (subject == null)
                    {
                        this.Monitor.Log($"{logMessage} no target found.");
                        return;
                    }

                    // show lookup UI
                    this.Monitor.Log(logMessage.ToString());
                    this.ShowLookupFor(subject);
                }
                catch
                {
                    this.Monitor.Log($"{logMessage} an error occurred.");
                    throw;
                }
            });
        }

        /// <summary>Show a lookup menu for the given subject.</summary>
        /// <param name="subject">The subject to look up.</param>
        internal void ShowLookupFor(ISubject subject)
        {
            this.Monitor.InterceptErrors("looking that up", () =>
            {
                this.Monitor.Log($"Showing {subject.GetType().Name}::{subject.Type}::{subject.Name}.");
                this.PushMenu(
                    new LookupMenu(this.GameHelper, subject, this.Monitor, this.Helper.Reflection, this.Config.ScrollAmount, this.Config.ShowDataMiningFields, this.ShowLookupFor)
                );
            });
        }

        /// <summary>Hide the lookup UI for the current target.</summary>
        private void HideLookup()
        {
            this.Monitor.InterceptErrors("closing the menu", () =>
            {
                if (Game1.activeClickableMenu is LookupMenu menu)
                    menu.QueueExit();
            });
        }

        /****
        ** Search menu helpers
        ****/
        /// <summary>Toggle the search UI if applicable.</summary>
        private void TryToggleSearch()
        {
            if (Game1.activeClickableMenu is SearchMenu)
                this.HideSearch();
            else if (Context.IsPlayerFree)
                this.ShowSearch();
        }

        /// <summary>Show the search UI.</summary>
        private void ShowSearch()
        {
            this.PushMenu(
                new SearchMenu(this.SubjectFactory, this.ShowLookupFor, this.Monitor)
            );
        }

        /// <summary>Hide the search UI.</summary>
        private void HideSearch()
        {
            if (Game1.activeClickableMenu is SearchMenu)
            {
                Game1.playSound("bigDeSelect"); // match default behaviour when closing a menu
                Game1.activeClickableMenu = null;
            }
        }

        /****
        ** Generic helpers
        ****/
        /// <summary>Read the config file, migrating legacy settings if applicable.</summary>
        private ModConfig LoadConfig()
        {
            // migrate legacy settings
            try
            {
                if (File.Exists(Path.Combine(this.Helper.DirectoryPath, "config.json")))
                {
                    JObject model = this.Helper.ReadConfig<JObject>();

                    // merge ToggleLookupInFrontOfPlayer bindings into ToggleLookup
                    JObject controls = model.Value<JObject>("Controls");
                    string toggleLookup = controls?.Value<string>("ToggleLookup");
                    string toggleLookupInFrontOfPlayer = controls?.Value<string>("ToggleLookupInFrontOfPlayer");
                    if (!string.IsNullOrWhiteSpace(toggleLookupInFrontOfPlayer))
                    {
                        controls.Remove("ToggleLookupInFrontOfPlayer");
                        controls["ToggleLookup"] = string.Join(", ", (toggleLookup ?? "").Split(',').Concat(toggleLookupInFrontOfPlayer.Split(',')).Select(p => p.Trim()).Where(p => p != "").Distinct());
                        this.Helper.WriteConfig(model);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log("Couldn't migrate legacy settings in config.json; they'll be removed instead.", LogLevel.Warn);
                this.Monitor.Log(ex.ToString());
            }

            // load config
            return this.Helper.ReadConfig<ModConfig>();
        }

        /// <summary>Get the most relevant subject under the player's cursor.</summary>
        /// <param name="logMessage">The log message to which to append search details.</param>
        private ISubject GetSubject(StringBuilder logMessage)
        {
            // get context
            Vector2 cursorPos = this.GameHelper.GetScreenCoordinatesFromCursor();
            bool hasCursor = Constants.TargetPlatform != GamePlatform.Android && Game1.wasMouseVisibleThisFrame; // note: only reliable when a menu isn't open

            // open menu
            if (Game1.activeClickableMenu != null)
            {
                logMessage.Append($" searching the open '{Game1.activeClickableMenu.GetType().Name}' menu...");
                return this.TargetFactory.GetSubjectFrom(Game1.activeClickableMenu, cursorPos);
            }

            // HUD under cursor
            if (hasCursor)
            {
                foreach (IClickableMenu menu in Game1.onScreenMenus)
                {
                    if (menu.isWithinBounds((int)cursorPos.X, (int)cursorPos.Y))
                    {
                        logMessage.Append($" searching the on-screen '{menu.GetType().Name}' menu...");
                        return this.TargetFactory.GetSubjectFrom(menu, cursorPos);
                    }
                }
            }

            // world
            logMessage.Append(" searching the world...");
            return this.TargetFactory.GetSubjectFrom(Game1.player, Game1.currentLocation, this.Config.EnableTileLookups, hasCursor);
        }

        /// <summary>Push a new menu onto the display stack, saving the previous menu if needed.</summary>
        /// <param name="menu">The menu to show.</param>
        private void PushMenu(IClickableMenu menu)
        {
            if (this.ShouldRestoreMenu(Game1.activeClickableMenu))
            {
                this.PreviousMenus.Push(Game1.activeClickableMenu);
                this.Helper.Reflection.GetField<IClickableMenu>(typeof(Game1), "_activeClickableMenu").SetValue(menu); // bypass Game1.activeClickableMenu, which disposes the previous menu
            }
            else
                Game1.activeClickableMenu = menu;
        }

        /// <summary>Load the file containing metadata that's not available from the game directly.</summary>
        private void LoadMetadata()
        {
            this.Monitor.InterceptErrors("loading metadata", () =>
            {
                this.Metadata = this.Helper.Data.ReadJsonFile<Metadata>(this.DatabaseFileName);
            });
        }

        /// <summary>Get whether a given menu should be restored when the lookup ends.</summary>
        /// <param name="menu">The menu to check.</param>
        private bool ShouldRestoreMenu(IClickableMenu menu)
        {
            // no menu
            if (menu == null)
                return false;

            // if 'hide on key up' is enabled, all lookups should close on key up
            if (this.Config.HideOnKeyUp && menu is LookupMenu)
                return false;

            return true;
        }
    }
}
