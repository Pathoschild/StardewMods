using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.TheLongNight
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod, IAssetEditor
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Whether the mod just skipped a 10-minute interval.</summary>
        private bool JustSkipped;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data/Fish.xnb");
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            // unlock any in-season fish after 2am
            asset
                .AsDictionary<int, string>()
                .Set((key, value) =>
                {
                    string[] fields = value.Split('/');
                    if (fields[1] == "trap")
                        return value; // ignore non-fish entries

                    fields[5] = $"{fields[5]} 2600 {int.MaxValue}".Trim();
                    return string.Join("/", fields);
                });
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked after the game updates (roughly 60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        /// <remarks>
        /// All times are shown in the game's internal format, which is essentially military time with support for
        /// times past midnight (e.g. 2400 is midnight, 2600 is 2am).
        /// 
        /// The game's logic for collapsing the player mostly happens in <see cref="Game1.performTenMinuteClockUpdate"/>.
        /// It has three cases which affect staying up:
        ///   - time = 2600: dismounts the player.
        ///   - time ≥ 2600: sets the <see cref="Game1.farmerShouldPassOut"/> flag, which causes <see cref="Game1.UpdateOther"/>
        ///     to initiate a player collapse (animation #293).
        ///   - time = 2800: initiates a player collapse.
        /// </remarks>
        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady || Game1.timeOfDay < 2550)
                return;

            // get clock details
            bool clockWillChangeNextTick = this.WillClockChangeNextTick();

            // 1. Right before the clock changes, freeze the player for interval + 1. That's too fast for the player to
            //    notice, but long enough to bypass the farmerShouldPassOut check (which skips if the player is frozen).
            Game1.farmerShouldPassOut = false;
            if (clockWillChangeNextTick)
            {
                Game1.player.freezePause = Game1.currentGameTime.ElapsedGameTime.Milliseconds + 1;
                this.Monitor.Log($"Adding freeze for {this.GetNextTime(Game1.timeOfDay)} next tick ({Game1.player.freezePause}ms).", LogLevel.Trace);
            }

            // 2. Right before the game updates the clock to 2600/2800, change it to the upcoming time. The game will then
            //    update to 2610/2810 instead, which has no special logic. Immediately afterwards, change the clock back to
            //    2600/2800. This happens faster than the player can see.
            if (clockWillChangeNextTick && (Game1.timeOfDay == 2550 || Game1.timeOfDay == 2750))
            {
                Game1.timeOfDay += 50;
                this.Monitor.Log($"Skipping {Game1.timeOfDay} next tick.", LogLevel.Trace);
                this.JustSkipped = true;
            }
            else if (this.JustSkipped)
            {
                Game1.timeOfDay -= 10;
                this.Monitor.Log($"Skip done, reset time to {Game1.timeOfDay}.", LogLevel.Trace);
                this.JustSkipped = false;
            }

            // 3. As a failsafe, if the collapse animation starts immediately remove it. This prevents the attached
            //    Farmer.passOutFromTired callback from being called.
            FarmerSprite sprite = (FarmerSprite)Game1.player.Sprite;
            var animation = sprite.CurrentAnimation;
            if (animation != null && animation.Any(frame => frame.frameBehavior == SFarmer.passOutFromTired))
            {
                this.Monitor.Log("Cancelling player collapse.", LogLevel.Trace);
                Game1.player.freezePause = 0;
                Game1.player.canMove = true;
                sprite.PauseForSingleAnimation = false;
                sprite.StopAnimation();
            }
        }

        /// <summary>Get whether the clock will change on the next update tick.</summary>
        /// <remarks>Derived from <see cref="Game1.performTenMinuteClockUpdate"/>.</remarks>
        private bool WillClockChangeNextTick()
        {
            return (Game1.gameTimeInterval + Game1.currentGameTime.ElapsedGameTime.Milliseconds) > 7000 + Game1.currentLocation.getExtraMillisecondsPerInGameMinuteForThisLocation();
        }

        /// <summary>Get the clock time that comes after the given value.</summary>
        /// <param name="current">The current time.</param>
        /// <remarks>Derived from <see cref="Game1.performTenMinuteClockUpdate"/>.</remarks>
        private int GetNextTime(int current)
        {
            int next = current + 10;
            if (next % 100 >= 60)
                next = next - (next % 100) + 100;
            return next;
        }
    }
}
