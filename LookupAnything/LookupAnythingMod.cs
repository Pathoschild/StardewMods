#define TEST_BUILD
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Pathoschild.LookupAnything.Components;
using Pathoschild.LookupAnything.Framework;
using Pathoschild.LookupAnything.Framework.Subjects;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.LookupAnything
{
    /// <summary>The mod entry point.</summary>
    class LookupAnythingMod : Mod
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

        /// <summary>Whether to check for a newer version of the mod.</summary>
        private bool HasSeenUpdateWarning;

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
        /// <summary>Initialise the mod.</summary>
        public override void Entry(params object[] objects)
        {
            // load config
            this.Config = new RawModConfig().InitializeConfig(this.BaseConfigPath).GetParsed();

            // load database
            this.LoadMetadata();
#if TEST_BUILD
            this.OverrideFileWatcher = new FileSystemWatcher(this.PathOnDisk, this.DatabaseFileName) { EnableRaisingEvents = true };
            this.OverrideFileWatcher.Changed += (sender, e) =>
            {
                this.LoadMetadata();
                this.TargetFactory = new TargetFactory(this.Metadata);
                this.DebugInterface = new DebugInterface(this.TargetFactory, this.Config) { Enabled = this.DebugInterface.Enabled };
            };
#endif

            // reset low-level cache once per day (used to store expensive query results that don't change within a day)
            PlayerEvents.LoadedGame += (sender, e) => this.ResetCache();
            TimeEvents.OnNewDay += (sender, e) => this.ResetCache();

            // initialise functionality
            this.CurrentVersion = UpdateHelper.GetSemanticVersion(this.Manifest.Version);
            this.TargetFactory = new TargetFactory(this.Metadata);
            this.DebugInterface = new DebugInterface(this.TargetFactory, this.Config);

            // hook up events
            PlayerEvents.LoadedGame += (sender, e) => this.ReceiveGameLoaded();
            ControlEvents.KeyPressed += (sender, e) => this.ReceiveInput(e.KeyPressed, this.Config.Keyboard);
            ControlEvents.ControllerButtonPressed += (sender, e) => this.ReceiveInput(e.ButtonPressed, this.Config.Controller);
            ControlEvents.ControllerTriggerPressed += (sender, e) => this.ReceiveInput(e.ButtonPressed, this.Config.Controller);
            MenuEvents.MenuClosed += (sender, e) => this.ReceiveMenuClosed(e.PriorMenu);
            GraphicsEvents.OnPostRenderHudEvent += (sender, e) => this.ReceiveInterfaceRendering(Game1.spriteBatch);
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
                    try
                    {
                        GitRelease release = UpdateHelper.GetLatestReleaseAsync("Pathoschild/LookupAnything").Result;
                        if (release.IsNewerThan(this.CurrentVersion))
                            this.NewRelease = release;
                    }
                    catch (Exception ex)
                    {
                        this.HandleError(ex, "checking for a newer version");
                    }
                });
            }
        }

        /// <summary>The method invoked when the player presses an input button.</summary>
        /// <typeparam name="TKey">The input type.</typeparam>
        /// <param name="key">The pressed input.</param>
        /// <param name="map">The configured input mapping.</param>
        private void ReceiveInput<TKey>(TKey key, InputMapConfiguration<TKey> map)
        {
            try
            {
                // perform bound action
                if (key.Equals(map.Lookup))
                    this.ShowLookup();
                if (key.Equals(map.ScrollUp))
                    (Game1.activeClickableMenu as LookupMenu)?.ScrollUp(this.Config.ScrollAmount);
                else if (key.Equals(map.ScrollDown))
                    (Game1.activeClickableMenu as LookupMenu)?.ScrollDown(this.Config.ScrollAmount);
                else if (key.Equals(map.ToggleDebugInfo))
                    this.DebugInterface.Enabled = !this.DebugInterface.Enabled;
            }
            catch (Exception ex)
            {
                this.HandleError(ex, $"handling input '{key}'.");
            }
        }

        /// <summary>The method invoked when the player closes a displayed menu.</summary>
        /// <param name="closedMenu">The menu which the player just closed.</param>
        private void ReceiveMenuClosed(IClickableMenu closedMenu)
        {
            try
            {
                // restore the previous menu if it was hidden to display the lookup UI.
                if (closedMenu is LookupMenu && this.PreviousMenu != null)
                {
                    Game1.activeClickableMenu = this.PreviousMenu;
                    this.PreviousMenu = null;
                }
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "restoring the previous menu");
            }
        }

        /// <summary>The method invoked when the interface is rendering.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        private void ReceiveInterfaceRendering(SpriteBatch spriteBatch)
        {
            // render debug interface
            if (this.DebugInterface.Enabled)
            {
                try
                {
                    this.DebugInterface.Draw(spriteBatch);
                }
                catch (Exception ex)
                {
                    GameHelper.ShowErrorMessage("Huh. Something went wrong drawing the debug info. The game error log has the technical details.");
                    Log.Error(ex.ToString());
                }
            }

            // render update warning
            if (this.Config.CheckForUpdates && !this.HasSeenUpdateWarning && this.NewRelease != null)
            {
                this.HasSeenUpdateWarning = true;
                GameHelper.ShowInfoMessage($"You can update LookupAnything from {this.CurrentVersion} to {this.NewRelease.Version}.");
            }
        }

        /****
        ** Helpers
        ****/
        /// <summary>Show the lookup UI for the current target.</summary>
        private void ShowLookup()
        {
            try
            {
                // show lookup UI
                ISubject subject = Game1.activeClickableMenu != null
                    ? this.TargetFactory.GetSubjectFrom(Game1.activeClickableMenu)
                    : this.TargetFactory.GetSubjectFrom(Game1.currentLocation, Game1.currentCursorTile, GameHelper.GetScreenCoordinatesFromCursor());
                if (subject != null)
                {
                    this.PreviousMenu = Game1.activeClickableMenu;
                    Game1.activeClickableMenu = new LookupMenu(subject);
                }
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "looking that up");
            }
        }

        /// <summary>Load the file containing metadata that's not available from the game directly.</summary>
        private void LoadMetadata()
        {
            try
            {
                string content = File.ReadAllText(Path.Combine(this.PathOnDisk, this.DatabaseFileName));
                this.Metadata = JsonConvert.DeserializeObject<Metadata>(content);
            }
            catch (Exception ex)
            {
                this.HandleError(ex, "loading metadata");
            }
        }

        /// <summary>Reset the low-level cache used to store expensive query results, so the data is recalculated on demand.</summary>
        private void ResetCache()
        {
            GameHelper.ResetCache();
        }

        /// <summary>Log an error and warn the user.</summary>
        /// <param name="ex">The exception to handle.</param>
        /// <param name="verb">The verb describing where the error occurred (e.g. "looking that up").</param>
        private void HandleError(Exception ex, string verb)
        {
            GameHelper.ShowErrorMessage($"Huh. Something went wrong {verb}. The game error log has the technical details.");
            Log.Error(ex.ToString());
        }
    }
}
