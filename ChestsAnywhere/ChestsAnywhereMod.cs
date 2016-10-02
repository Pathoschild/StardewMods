using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChestsAnywhere.Common;
using ChestsAnywhere.Components;
using ChestsAnywhere.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
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
        /// <summary>The selected chest.</summary>
        private Chest SelectedChest;

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
            GameEvents.GameLoaded += (sender, e) => this.ReceiveGameLoaded();
            GraphicsEvents.OnPostRenderHudEvent += (sender, e) => this.ReceiveInterfaceRendering(Game1.spriteBatch);
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

        /// <summary>The method invoked when the interface is rendering.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        private void ReceiveInterfaceRendering(SpriteBatch spriteBatch)
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
        }

        /// <summary>The method invoked when a menu is closed.</summary>
        private void ReceiveMenuClosed(IClickableMenu closedMenu)
        {
            if (closedMenu is AccessChestMenu)
            {
                AccessChestMenu menu = (AccessChestMenu)closedMenu;
                menu.Dispose();

            }
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
                from chest in this.GetChests()
                where !chest.IsIgnored
                orderby (this.Config.GroupByLocation ? chest.Location : null) ascending, (chest.Order ?? int.MaxValue) ascending, chest.Name ascending
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

        /// <summary>Get all player chests.</summary>
        private IEnumerable<ManagedChest> GetChests()
        {
            foreach (GameLocation location in Game1.locations)
            {
                // chests in location
                {
                    int namelessCount = 0;
                    foreach (Chest chest in location.Objects.Values.OfType<Chest>())
                        yield return new ManagedChest(chest, location.Name, $"Chest #{++namelessCount}");
                }

                // chests in constructed buildings
                if (location is BuildableGameLocation)
                {
                    foreach (Building building in (location as BuildableGameLocation).buildings)
                    {
                        int namelessCount = 0;
                        if (building.indoors == null)
                            continue;
                        foreach (Chest chest in building.indoors.Objects.Values.OfType<Chest>())
                            yield return new ManagedChest(chest, building.nameOfIndoorsWithoutUnique, $"Chest #{++namelessCount}");
                    }
                }

                // farmhouse containers
                if (location is FarmHouse)
                {
                    Chest fridge = (location as FarmHouse).fridge;
                    if (fridge != null)
                        yield return new ManagedChest(fridge, location.Name, "Fridge");
                }
            }
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
