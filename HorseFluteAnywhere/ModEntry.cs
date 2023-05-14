using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Patching;
using Pathoschild.Stardew.HorseFluteAnywhere.Framework;
using Pathoschild.Stardew.HorseFluteAnywhere.Patches;
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
        ** Fields
        *********/
        /// <summary>The unique item ID for a horse flute.</summary>
        private const string HorseFluteId = "911";

        /// <summary>The horse flute to play when the summon key is pressed.</summary>
        private readonly Lazy<SObject> HorseFlute = new(() => ItemRegistry.Create<SObject>("(O)" + ModEntry.HorseFluteId));

        /// <summary>The mod configuration.</summary>
        private ModConfig Config = null!; // set in Entry

        /// <summary>The summon key binding.</summary>
        private KeybindList SummonKey => this.Config.SummonHorseKey;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            CommonHelper.RemoveObsoleteFiles(this, "HorseFluteAnywhere.pdb"); // removed in 1.1.19

            // load config
            this.UpdateConfig();

            // add patches
            HarmonyPatcher.Apply(this,
                new UtilityPatcher(this.Monitor)
            );

            // hook events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.World.LocationListChanged += this.OnLocationListChanged;

            // hook commands
            helper.ConsoleCommands.Add("reset_horses", "Reset the name and ownership for every horse in the game, so you can rename or reclaim a broken horse.", (_, _) => this.ResetHorsesCommand());
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
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

        /// <inheritdoc cref="IInputEvents.ButtonsChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (this.SummonKey.JustPressed() && this.TryUseHorseFlute())
                this.Helper.Input.SuppressActiveKeybinds(this.SummonKey);
        }

        /// <inheritdoc cref="IWorldEvents.LocationListChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnLocationListChanged(object? sender, LocationListChangedEventArgs e)
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

        /// <inheritdoc cref="IPlayerEvents.Warped"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnWarped(object? sender, WarpedEventArgs e)
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

        /// <summary>Use the horse flute, if allowed in the current context.</summary>
        /// <returns>Returns whether the horse flute was used.</returns>
        private bool TryUseHorseFlute()
        {
            if (!this.CanPlayFlute(Game1.player))
                return false;

            this.HorseFlute.Value.performUseAction(Game1.currentLocation);
            return true;
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

            // reset horse instances
            Farm farm = Game1.getFarm();
            bool anyChanged = false;
            foreach (Stable stable in farm.buildings.OfType<Stable>())
            {
                bool curChanged = false;

                // fetch info
                Horse horse = stable.getStableHorse();
                bool isTractor = this.IsTractor(horse);
                bool hadName = !string.IsNullOrEmpty(horse?.Name);

                // fix stable
                if (stable.owner.Value != 0)
                {
                    stable.owner.Value = 0;
                    curChanged = true;
                }

                // fix horse
                if (horse != null)
                {
                    if (horse.ownerId.Value != 0)
                    {
                        horse.ownerId.Value = 0;
                        curChanged = true;
                    }

                    if (!isTractor && hadName)
                    {
                        horse.Name = "";
                        curChanged = true;
                    }
                }

                // log
                if (curChanged)
                {
                    string message = isTractor
                        ? $"Reset tractor {(hadName ? $"'{horse!.Name}'" : "with no name")}."
                        : $"Reset horse '{(hadName ? $"'{horse!.Name}'" : "with no name")}'. The next player who interacts with it will become the owner.";
                    this.Monitor.Log(message, LogLevel.Info);
                }
                anyChanged |= curChanged;
            }

            // reset player horse names
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                if (!string.IsNullOrEmpty(farmer.horseName.Value))
                {
                    farmer.horseName.Value = null;
                    anyChanged = true;
                    this.Monitor.Log($"Reset horse link for player '{farmer.Name}'.", LogLevel.Info);
                }
            }

            this.Monitor.Log(anyChanged ? "Done!" : "No horses found to reset.", LogLevel.Info);
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
                .Where(p => !this.ShouldIgnore(p));
        }

        /// <summary>Warp a horse back to its home.</summary>
        /// <param name="horse">The horse to warp.</param>
        private void WarpHome(Horse horse)
        {
            Farm farm = Game1.getFarm();
            Stable? stable = farm.buildings.OfType<Stable>().FirstOrDefault(p => p.HorseId == horse.HorseId);

            Game1.warpCharacter(horse, farm, Vector2.Zero);
            stable?.grabHorse();
        }

        /// <summary>Get whether the player can play the flute.</summary>
        /// <param name="player">The player to check.</param>
        private bool CanPlayFlute(Farmer player)
        {
            const string id = $"{ItemRegistry.type_object}{ModEntry.HorseFluteId}";
            return
                Context.IsPlayerFree
                && (
                    !this.Config.RequireHorseFlute
                    || player.Items.Any(p => p?.QualifiedItemId == id)
                );
        }

        /// <summary>Get whether a player is riding a non-ignored horse.</summary>
        /// <param name="player">The player to check.</param>
        private bool IsRidingHorse(Farmer player)
        {
            return
                player.mount != null
                && !this.ShouldIgnore(player.mount);
        }

        /// <summary>Get whether a horse should be ignored by the main horse logic.</summary>
        /// <param name="horse">The horse to check.</param>
        private bool ShouldIgnore(Horse? horse)
        {
            return
                horse == null
                || this.IsTractor(horse) // Tractor Mod tractor
                || horse.GetType().FullName?.StartsWith("DeepWoodsMod.") == true; // Deep Woods unicorn
        }

        /// <summary>Get whether a horse is a tractor added by Tractor Mod, which manages the edge cases for tractor summoning automatically.</summary>
        /// <param name="horse">The horse to check.</param>
        private bool IsTractor(Horse horse)
        {
            return horse.modData?.TryGetValue("Pathoschild.TractorMod", out _) == true;
        }
    }
}
