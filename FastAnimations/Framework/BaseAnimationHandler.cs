using System;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>The base class for animation handlers.</summary>
    internal abstract class BaseAnimationHandler : IAnimationHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>The fractional skips left to apply, so fractional multipliers can be smudged over multiple update ticks. For example, a multiplier of 1.5 will skip a frame every other tick.</summary>
        private float Remainder;


        /*********
        ** Accessors
        *********/
        /// <summary>The animation speed multiplier to apply.</summary>
        protected readonly float Multiplier;


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public abstract bool IsEnabled(int playerAnimationID);

        /// <summary>Perform any updates needed when the player enters a new location.</summary>
        /// <param name="location">The new location.</param>
        public virtual void OnNewLocation(GameLocation location) { }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public abstract void Update(int playerAnimationID);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        protected BaseAnimationHandler(float multiplier)
        {
            this.Multiplier = multiplier;
        }

        /// <summary>Get the number of frames to skip for the current tick.</summary>
        protected int GetSkipsThisTick()
        {
            if (this.Multiplier <= 1)
                return 0;

            float skips = this.Multiplier + this.Remainder - 1; // 1 is the default speed (i.e. skip zero frames), so subtract it to get the number of skips
            this.Remainder = skips % 1;
            return (int)skips;
        }

        /// <summary>Apply an animation update for each frame that should be skipped in the current tick.</summary>
        /// <param name="run">Run one animation frame.</param>
        /// <param name="until">Get whether the animation should stop being skipped.</param>
        protected void ApplySkips(Action run, Func<bool> until = null)
        {
            this.ApplySkips(this.GetSkipsThisTick(), run, until);
        }

        /// <summary>Apply an animation update for each frame that should be skipped.</summary>
        /// <param name="skips">The number of frames to skip for the current tick.</param>
        /// <param name="run">Run one animation frame.</param>
        /// <param name="until">Get whether the animation should stop being skipped.</param>
        protected void ApplySkips(int skips, Action run, Func<bool> until = null)
        {
            for (int i = 0; i < skips; i++)
            {
                if (until?.Invoke() == true)
                    break;

                run();
            }
        }

        /// <summary>Speed up the player by the given multiplier for the current update tick.</summary>
        /// <param name="until">Get whether the animation should stop being skipped.</param>
        protected void SpeedUpPlayer(Func<bool> until = null)
        {
            this.ApplySkips(
                run: () => Game1.player.Update(Game1.currentGameTime, Game1.player.currentLocation),
                until
            );
        }

        /// <summary>Get whether the current player is riding a tractor from Tractor Mod.</summary>
        protected bool IsRidingTractor()
        {
            return Game1.player?.mount?.Name?.StartsWith("tractor/") == true;
        }
    }
}
