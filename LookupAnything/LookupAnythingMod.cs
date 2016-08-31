using System;
using System;
using System.IO;
using Microsoft.Xna.Framework.Input;
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
        /// <summary>The previous menu shown before the lookup UI was opened.</summary>
        private IClickableMenu PreviousMenu;

        /// <summary>The name of the file containing data for the <see cref="Metadata"/> field.</summary>
        private readonly string DatabaseFileName = "data.json";

        /// <summary>Provides metadata that's not available from the game data directly.</summary>
        private Metadata Metadata;

#if TEST_BUILD
        /// <summary>Reloads the <see cref="Metadata"/> when the underlying file changes.</summary>
        private FileSystemWatcher OverrideFileWatcher;
#endif


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        public override void Entry(params object[] objects)
        {
            // load database
            this.LoadMetadata();
#if TEST_BUILD
            this.OverrideFileWatcher = new FileSystemWatcher(this.PathOnDisk, this.DatabaseFileName) { EnableRaisingEvents = true };
            this.OverrideFileWatcher.Changed += (sender, e) => this.LoadMetadata();
#endif

            // reset low-level cache once per day (used to store expensive query results that don't change within a day)
            PlayerEvents.LoadedGame += (sender, e) => this.ResetCache();
            TimeEvents.OnNewDay += (sender, e) => this.ResetCache();

            // hook up UI
            ControlEvents.KeyPressed += (sender, e) => this.TryOpenMenu(Keys.F1, e.KeyPressed);
            MenuEvents.MenuClosed += (sender, e) => this.TryRestorePreviousMenu(e.PriorMenu);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Show the lookup UI for the current target if the control matches the configured control.</summary>
        /// <typeparam name="T">The input type.</typeparam>
        /// <param name="expected">The configured toggle input.</param>
        /// <param name="received">The received toggle input.</param>
        private void TryOpenMenu<T>(T expected, T received)
        {
            try
            {
                // check input
                if (!received.Equals(expected))
                    return;

                // show lookup UI
                ISubject subject = Game1.activeClickableMenu != null
                    ? new SubjectFactory(this.Metadata).GetSubjectFrom(Game1.activeClickableMenu)
                    : new SubjectFactory(this.Metadata).GetSubjectFrom(Game1.currentLocation, Game1.currentCursorTile);
                if (subject != null)
                {
                    this.PreviousMenu = Game1.activeClickableMenu;
                    Game1.activeClickableMenu = new LookupMenu(subject);
                }
            }
            catch (Exception ex)
            {
                Game1.showRedMessage("Huh. Something went wrong looking that up. The game error log has the technical details.");
                Log.Error(ex.ToString());
            }
        }

        /// <summary>Restore the previous menu if it was hidden to display the lookup UI.</summary>
        /// <param name="closedMenu">The menu which the player just closed.</param>
        private void TryRestorePreviousMenu(IClickableMenu closedMenu)
        {
            if (closedMenu is LookupMenu && this.PreviousMenu != null)
            {
                Game1.activeClickableMenu = this.PreviousMenu;
                this.PreviousMenu = null;
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
                Log.Error($"Couldn't read {this.DatabaseFileName} file for {this.Manifest.Name} mod; some data may be missing. Error details:\n{ex}");
            }
        }

        /// <summary>Reset the low-level cache used to store expensive query results, so the data is recalculated on demand.</summary>
        private void ResetCache()
        {
            GameHelper.ResetCache();
        }
    }
}
