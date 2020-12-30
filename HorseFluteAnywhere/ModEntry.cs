using System.Collections.Generic;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.HorseFluteAnywhere.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;

namespace Pathoschild.Stardew.HorseFluteAnywhere
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // add patches
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            UtilityPatcher.Hook(harmony, this.Monitor);

            // hook events
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.World.LocationListChanged += this.OnLocationListChanged;
        }


        /*********
        ** Public methods
        *********/
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
