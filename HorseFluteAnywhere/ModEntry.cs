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
            I18n.Init(helper.Translation);

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
            {
                int[] warpRestrictions = Utility.GetHorseWarpRestrictionsForFarmer(Game1.player).ToArray();

                if (warpRestrictions.Length == 1 && warpRestrictions[0] == 2 && this.TryFallbackSummonHorse())
                {
                    // GetHorseWarpRestrictionsForFarmer patch failed (usually on macOS), but we
                    // were able to fallback to summoning the horse manually.
                }
                else
                    this.HorseFlute.Value.performUseAction(Game1.currentLocation);

                this.Helper.Input.SuppressActiveKeybinds(this.SummonKey);
            }
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

        /// <summary>Summon the horse manually if the <see cref="UtilityPatcher.After_GetHorseWarpRestrictionsForFarmer"/> patch isn't working for some reason.</summary>
        /// <remarks>On macOS, patches on <see cref="Utility.GetHorseWarpRestrictionsForFarmer"/> are never called for some unknown reason. Derived from <see cref="SObject.performUseAction"/> and <see cref="FarmerTeam.OnRequestHorseWarp"/>.</remarks>
        private bool TryFallbackSummonHorse()
        {
            // find horse
            Horse horse = Utility.findHorseForPlayer(Game1.player.UniqueMultiplayerID);
            if (horse == null)
                return false;

            // play flute
            this.Monitor.Log("Falling back to custom summon...");
            Game1.player.faceDirection(Game1.down);
            Game1.soundBank.PlayCue("horse_flute");
            Game1.player.FarmerSprite.animateOnce(new[]
            {
                new FarmerSprite.AnimationFrame(98, 400, true, false),
                new FarmerSprite.AnimationFrame(99, 200, true, false),
                new FarmerSprite.AnimationFrame(100, 200, true, false),
                new FarmerSprite.AnimationFrame(99, 200, true, false),
                new FarmerSprite.AnimationFrame(98, 400, true, false),
                new FarmerSprite.AnimationFrame(99, 200, true, false),
            });
            Game1.player.freezePause = 1500;

            // summon horse
            DelayedAction.functionAfterDelay(() =>
            {
                horse.mutex.RequestLock(() =>
                {
                    horse.mutex.ReleaseLock();

                    Multiplayer multiplayer = this.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
                    GameLocation location = horse.currentLocation;
                    Vector2 tile_location = horse.getTileLocation();

                    for (int i = 0; i < 8; i++)
                    {
                        multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(10, new Vector2(tile_location.X + Utility.RandomFloat(-1, 1), tile_location.Y + Utility.RandomFloat(-1, 0)) * Game1.tileSize, Color.White, 8, false, 50f)
                        {
                            layerDepth = 1f,
                            motion = new Vector2(Utility.RandomFloat(-0.5F, 0.5F), Utility.RandomFloat(-0.5F, 0.5F))
                        });
                    }

                    location.playSoundAt("wand", horse.getTileLocation());

                    location = Game1.player.currentLocation;
                    tile_location = Game1.player.getTileLocation();

                    location.playSoundAt("wand", tile_location);

                    for (int i = 0; i < 8; i++)
                    {
                        multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(10, new Vector2(tile_location.X + Utility.RandomFloat(-1, 1), tile_location.Y + Utility.RandomFloat(-1, 0)) * Game1.tileSize, Color.White, 8, false, 50f)
                        {
                            layerDepth = 1f,
                            motion = new Vector2(Utility.RandomFloat(-0.5F, 0.5F), Utility.RandomFloat(-0.5F, 0.5F))
                        });
                    }

                    Game1.warpCharacter(horse, Game1.player.currentLocation, tile_location);
                    int j = 0;
                    for (int x = (int)tile_location.X + 3; x >= (int)tile_location.X - 3; x--)
                    {
                        multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(6, new Vector2(x, tile_location.Y) * Game1.tileSize, Color.White, 8, false, 50f)
                        {
                            layerDepth = 1f,
                            delayBeforeAnimationStart = j * 25,
                            motion = new Vector2(-.25f, 0)
                        });
                        j++;
                    }
                });
            }, 1500);
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
                        ? $"Reset tractor {(hadName ? $"'{horse.Name}'" : "with no name")}."
                        : $"Reset horse '{(hadName ? $"'{horse.Name}'" : "with no name")}'. The next player who interacts with it will become the owner.";
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
        private bool ShouldIgnore(Horse horse)
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
