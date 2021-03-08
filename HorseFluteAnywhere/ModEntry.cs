using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.HorseFluteAnywhere.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.HorseFluteAnywhere
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Private methods
        *********/
        /// <summary>The unique item ID for a horse flute.</summary>
        private const int HorseFluteId = 911;

        /// <summary>The horse flute to play when the summon key is pressed.</summary>
        private readonly Lazy<SObject> HorseFlute = new(() => new SObject(ModEntry.HorseFluteId, 1));

        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The summon key binding.</summary>
        private KeybindList SummonKey => this.Config.SummonHorseKey;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // load config
            this.UpdateConfig();

            // add patches
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            UtilityPatcher.Hook(harmony, this.Monitor);

            // hook events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.World.LocationListChanged += this.OnLocationListChanged;

            // hook commands
            helper.ConsoleCommands.Add("reset_horses", "Reset the name and ownership for every horse in the game, so you can rename or reclaim a broken horse.", (_, _) => this.ResetHorsesCommand());
        }


        /*********
        ** Public methods
        *********/
        /// <summary>The event called after the first game update, once all mods are loaded.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // add Generic Mod Config Menu integration
            new GenericModConfigMenuIntegrationForHorseFluteAnywhere(
                getConfig: () => this.Config,
                reset: () =>
                {
                    this.Config = new ModConfig();
                    this.Helper.WriteConfig(this.Config);
                    this.UpdateConfig();
                },
                saveAndApply: () =>
                {
                    this.Helper.WriteConfig(this.Config);
                    this.UpdateConfig();
                },
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest
            ).Register();
        }

        /// <summary>Raised after the player presses any buttons on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (this.SummonKey.JustPressed() && this.CanPlayFlute(Game1.player))
                this.HorseFlute.Value.performUseAction(Game1.currentLocation);
        }

        /// <summary>The event called after the location list changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLocationListChanged(object sender, LocationListChangedEventArgs e)
        {
            // rescue lost horses
            if (Context.IsMainPlayer)
            {
                foreach (GameLocation location in e.Removed)
                {
                    foreach (Horse horse in this.GetHorsesIn(location).ToArray())
                        this.WarpHome(horse);
                }
            }
        }

        /// <summary>The event called after the player warps into a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer || !this.IsRidingHorse(Game1.player))
                return;

            // fix: warping onto a magic warp while mounted causes an infinite warp loop
            Vector2 tile = CommonHelper.GetPlayerTile(Game1.player);
            string touchAction = Game1.player.currentLocation.doesTileHaveProperty((int)tile.X, (int)tile.Y, "TouchAction", "Back");
            if (touchAction != null && touchAction.StartsWith("MagicWarp "))
                Game1.currentLocation.lastTouchActionLocation = tile;

            // fix: warping into an event may break the event (e.g. Mr Qi's event on mine level event for the 'Cryptic Note' quest)
            if (Game1.CurrentEvent != null)
                Game1.player.mount.dismount();
        }

        /// <summary>Reset all horse names and ownership, and log details to the SMAPI console.</summary>
        private void ResetHorsesCommand()
        {
            // validate
            if (!Context.IsWorldReady)
            {
                this.Monitor.Log("You must load a save to use this command.", LogLevel.Error);
                return;
            }
            if (!Context.IsMainPlayer)
            {
                this.Monitor.Log("You must be the main player to use this command.", LogLevel.Error);
                return;
            }

            // scan for horses
            Farm farm = Game1.getFarm();
            bool anyFound = false;
            foreach (Stable stable in farm.buildings.OfType<Stable>())
            {
                // get horse
                Horse horse = stable.getStableHorse();
                if (horse == null || this.IsTractor(horse))
                    continue;
                anyFound = true;

                // update name & ownership
                if (horse.Name?.Length == 0)
                    this.Monitor.Log("Skipped horse with no name or owner.", LogLevel.Info);
                else
                {
                    this.Monitor.Log($"Reset horse with name '{horse.Name}'. The next player who interacts with it will become the owner.", LogLevel.Info);
                    horse.Name = "";
                }
            }

            this.Monitor.Log(anyFound ? "Done!" : "No horses found to reset.", LogLevel.Info);
        }

        /// <summary>Update the mod configuration.</summary>
        private void UpdateConfig()
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
        }

        /// <summary>Get all horses in the given location.</summary>
        /// <param name="location">The location to scan.</param>
        private IEnumerable<Horse> GetHorsesIn(GameLocation location)
        {
            return location.characters
                .OfType<Horse>()
                .Where(p => !this.IsTractor(p));
        }

        /// <summary>Warp a horse back to its home.</summary>
        /// <param name="horse">The horse to warp.</param>
        private void WarpHome(Horse horse)
        {
            Farm farm = Game1.getFarm();
            Stable stable = farm.buildings.OfType<Stable>().FirstOrDefault(p => p.HorseId == horse.HorseId);

            Game1.warpCharacter(horse, farm, Vector2.Zero);
            stable?.grabHorse();
        }

        /// <summary>Get whether the player can play the flute.</summary>
        /// <param name="player">The player to check.</param>
        private bool CanPlayFlute(Farmer player)
        {
            return
                Context.IsPlayerFree
                && (
                    !this.Config.RequireHorseFlute
                    || player.Items.Any(p => Utility.IsNormalObjectAtParentSheetIndex(p, ModEntry.HorseFluteId))
                );
        }

        /// <summary>Get whether a player is riding a (non-tractor) horse.</summary>
        /// <param name="player">The player to check.</param>
        private bool IsRidingHorse(Farmer player)
        {
            return
                player.mount != null
                && !this.IsTractor(player.mount);
        }

        /// <summary>Get whether a horse is a tractor added by Tractor Mod, which manages the edge cases for tractor summoning automatically.</summary>
        /// <param name="horse">The horse to check.</param>
        private bool IsTractor(Horse horse)
        {
            return horse?.modData?.TryGetValue("Pathoschild.TractorMod", out _) == true;
        }
    }
}
