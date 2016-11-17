using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Common;
using Pathoschild.Stardew.LookupAnything.Components;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Subjects;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything
{
    /// <summary>The mod entry point.</summary>
    public class LookupAnythingMod : Mod
    {
        /*********
        ** Properties
        *********/
        /****
        ** Configuration
        ****/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>Provides metadata that's not available from the game data directly.</summary>
        private Metadata Metadata;

        /// <summary>The name of the file containing data for the <see cref="Metadata"/> field.</summary>
        private readonly string DatabaseFileName = "data.json";

#if TEST_BUILD
        /// <summary>Reloads the <see cref="Metadata"/> when the underlying file changes.</summary>
        private FileSystemWatcher OverrideFileWatcher;
#endif

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
        ** Validation
        ****/
        /// <summary>Whether the metadata validation passed.</summary>
        private bool IsDataValid;

        /****
        ** State
        ****/
        /// <summary>The previous menu shown before the lookup UI was opened.</summary>
        private IClickableMenu PreviousMenu;

        /// <summary>Finds and analyses lookup targets in the world.</summary>
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
            // validate version
            string versionError = GameHelper.ValidateGameVersion();
            if (versionError != null)
                this.Monitor.Log(versionError, LogLevel.Error);

            // load config
            this.Config = this.Helper.ReadConfig<RawModConfig>().GetParsed();
            this.Monitor.Log($"Initialising with lookup keys {this.Config.Controller.ToggleLookup} and {this.Config.Keyboard.ToggleLookup}.", LogLevel.Trace);

            // load database
            this.LoadMetadata();
#if TEST_BUILD
                this.OverrideFileWatcher = new FileSystemWatcher(this.PathOnDisk, this.DatabaseFileName)
                {
                    EnableRaisingEvents = true
                };
                this.OverrideFileWatcher.Changed += (sender, e) =>
                {
                    this.LoadMetadata();
                    this.TargetFactory = new TargetFactory(this.Metadata);
                    this.DebugInterface = new DebugInterface(this.TargetFactory, this.Config)
                    {
                        Enabled = this.DebugInterface.Enabled
                    };
                };
#endif

            // initialise functionality
            this.CurrentVersion = UpdateHelper.GetSemanticVersion(this.Manifest.Version);
            this.TargetFactory = new TargetFactory(this.Metadata);
            this.DebugInterface = new DebugInterface(this.TargetFactory, this.Config, this.Monitor);

            // hook up events
            {
                // reset low-level cache once per game day (used for expensive queries that don't change within a day)
                PlayerEvents.LoadedGame += (sender, e) => GameHelper.ResetCache(this.Metadata);
                TimeEvents.OnNewDay += (sender, e) => GameHelper.ResetCache(this.Metadata);

                // hook up game events
                GameEvents.GameLoaded += (sender, e) => this.ReceiveGameLoaded();
                GraphicsEvents.OnPostRenderHudEvent += (sender, e) => this.ReceiveInterfaceRendering(Game1.spriteBatch);
                MenuEvents.MenuClosed += (sender, e) => this.ReceiveMenuClosed(e.PriorMenu);
                if (this.Config.SuppressGameDebug)
                    GameEvents.HalfSecondTick += (sender, e) => this.SuppressGameDebug();

                // hook up keyboard
                if (this.Config.Keyboard.HasAny())
                {
                    ControlEvents.KeyPressed += (sender, e) => this.ReceiveKeyPress(e.KeyPressed, this.Config.Keyboard);
                    if (this.Config.HideOnKeyUp)
                        ControlEvents.KeyReleased += (sender, e) => this.ReceiveKeyRelease(e.KeyPressed, this.Config.Keyboard);
                }

                // hook up controller
                if (this.Config.Controller.HasAny())
                {
                    ControlEvents.ControllerButtonPressed += (sender, e) => this.ReceiveKeyPress(e.ButtonPressed, this.Config.Controller);
                    ControlEvents.ControllerTriggerPressed += (sender, e) => this.ReceiveKeyPress(e.ButtonPressed, this.Config.Controller);
                    if (this.Config.HideOnKeyUp)
                    {
                        ControlEvents.ControllerButtonReleased += (sender, e) => this.ReceiveKeyRelease(e.ButtonReleased, this.Config.Controller);
                        ControlEvents.ControllerTriggerReleased += (sender, e) => this.ReceiveKeyRelease(e.ButtonReleased, this.Config.Controller);
                    }
                }
            }

            // validate metadata
            this.IsDataValid = this.Metadata.LooksValid();
            if (!this.IsDataValid)
            {
                this.Monitor.Log("The data.json file seems to be missing or corrupt. Lookups will be disabled.", LogLevel.Error);
                this.IsDataValid = false;
            }
        }

        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The method invoked when the player loads the game.</summary>
        private void ReceiveGameLoaded()
        {
            // check for an updated version
            if (this.Config.CheckForUpdates)
            {
                Task.Factory.StartNew(() =>
                {
                    this.Monitor.InterceptErrors("checking for a newer version", () =>
                    {
                        // get version
                        GitRelease release;
                        try
                        {
                            release = UpdateHelper.GetLatestReleaseAsync("Pathoschild/LookupAnything").Result;
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
                });
            }
        }

        /// <summary>The method invoked when the player presses an input button.</summary>
        /// <typeparam name="TKey">The input type.</typeparam>
        /// <param name="key">The pressed input.</param>
        /// <param name="map">The configured input mapping.</param>
        private void ReceiveKeyPress<TKey>(TKey key, InputMapConfiguration<TKey> map)
        {
            if (!map.IsValidKey(key))
                return;

            // perform bound action
            this.Monitor.InterceptErrors("handling your input", $"handling input '{key}'", () =>
            {
                if (key.Equals(map.ToggleLookup))
                    this.ToggleLookup();
                if (key.Equals(map.ScrollUp))
                    (Game1.activeClickableMenu as LookupMenu)?.ScrollUp(this.Config.ScrollAmount);
                else if (key.Equals(map.ScrollDown))
                    (Game1.activeClickableMenu as LookupMenu)?.ScrollDown(this.Config.ScrollAmount);
                else if (key.Equals(map.ToggleDebug))
                    this.DebugInterface.Enabled = !this.DebugInterface.Enabled;
            });
        }

        /// <summary>The method invoked when the player presses an input button.</summary>
        /// <typeparam name="TKey">The input type.</typeparam>
        /// <param name="key">The pressed input.</param>
        /// <param name="map">The configured input mapping.</param>
        private void ReceiveKeyRelease<TKey>(TKey key, InputMapConfiguration<TKey> map)
        {
            if (!map.IsValidKey(key))
                return;

            // perform bound action
            this.Monitor.InterceptErrors("handling your input", $"handling input '{key}'", () =>
            {
                if (key.Equals(map.ToggleLookup))
                    this.HideLookup();
            });
        }

        /// <summary>The method invoked when the player closes a displayed menu.</summary>
        /// <param name="closedMenu">The menu which the player just closed.</param>
        private void ReceiveMenuClosed(IClickableMenu closedMenu)
        {
            // restore the previous menu if it was hidden to show the lookup UI
            this.Monitor.InterceptErrors("restoring the previous menu", () =>
            {
                if (closedMenu is LookupMenu && this.PreviousMenu != null)
                {
                    Game1.activeClickableMenu = this.PreviousMenu;
                    this.PreviousMenu = null;
                }
            });
        }

        /// <summary>The method invoked when the interface is rendering.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        private void ReceiveInterfaceRendering(SpriteBatch spriteBatch)
        {
            // render debug interface
            if (this.DebugInterface.Enabled)
                this.DebugInterface.Draw(spriteBatch);

            // render update warning
            if (this.Config.CheckForUpdates && !this.HasSeenUpdateWarning && this.NewRelease != null)
            {
                this.HasSeenUpdateWarning = true;
                GameHelper.ShowInfoMessage($"You can update Lookup Anything from {this.CurrentVersion} to {this.NewRelease.Version}.");
            }
        }

        /****
        ** Helpers
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
                GameHelper.ShowErrorMessage("The mod doesn't seem to be installed correctly: its data.json file is missing or corrupt.");
                return;
            }

            // show menu
            string logMessage = "Received a lookup request...";
            this.Monitor.InterceptErrors("looking that up", () =>
            {
                try
                {
                    // validate version
                    string versionError = GameHelper.ValidateGameVersion();
                    if (versionError != null)
                    {
                        GameHelper.ShowErrorMessage(versionError);
                        this.Monitor.Log(logMessage, LogLevel.Trace);
                        this.Monitor.Log(versionError, LogLevel.Error);
                        return;
                    }

                    // get target
                    ISubject subject;
                    if (Game1.activeClickableMenu != null)
                    {
                        logMessage += $" searching the open '{Game1.activeClickableMenu.GetType().Name}' menu...";
                        subject = this.TargetFactory.GetSubjectFrom(Game1.activeClickableMenu, GameHelper.GetScreenCoordinatesFromCursor());
                    }
                    else
                    {
                        logMessage += " searching the world...";
                        subject = this.TargetFactory.GetSubjectFrom(Game1.currentLocation, Game1.currentCursorTile, GameHelper.GetScreenCoordinatesFromCursor());
                    }

                    // validate target
                    if (subject == null)
                    {
                        this.Monitor.Log($"{logMessage} no target found.", LogLevel.Trace);
                        return;
                    }

                    // show lookup UI
                    this.Monitor.Log($"{logMessage} showing {subject.GetType().Name}::{subject.Type}::{subject.Name}.", LogLevel.Trace);
                    this.PreviousMenu = Game1.activeClickableMenu;
                    Game1.activeClickableMenu = new LookupMenu(subject, this.Metadata, this.Monitor);
                }
                catch
                {
                    this.Monitor.Log($"{logMessage} an error occurred.", LogLevel.Trace);
                    throw;
                }
            });
        }

        /// <summary>Show the lookup UI for the current target.</summary>
        private void HideLookup()
        {
            this.Monitor.InterceptErrors("closing the menu", () =>
            {
                if (Game1.activeClickableMenu is LookupMenu)
                {
                    Game1.playSound("bigDeSelect"); // match default behaviour when closing a menu
                    Game1.activeClickableMenu = null;
                }
            });
        }

        /// <summary>Load the file containing metadata that's not available from the game directly.</summary>
        private void LoadMetadata()
        {
            this.Monitor.InterceptErrors("loading metadata", () =>
            {
                this.Metadata = this.Helper.ReadJsonFile<Metadata>(this.DatabaseFileName);
            });
        }

        /// <summary>Immediately suppress the game's debug mode if it's enabled.</summary>
        private void SuppressGameDebug()
        {
            if (Game1.debugMode)
            {
                Game1.debugMode = false;
                Game1.addHUDMessage(new HUDMessage("Suppressed SMAPI F2 debug mode. (You can disable this in LookupAnything's config.json.)", 2) { timeLeft = 1000, transparency = 0.75f, color = Color.Gray });
            }
        }
    }
}
