using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>The base class for animation handlers.</summary>
    internal abstract class BaseAnimationHandler : IAnimationHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>Tracks the number of updates.</summary>
        private float UpdateCounter = 0;

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

        /// <summary>Speed up the player by the given multiplier for the current update tick.</summary>
        /// <param name="multiplier">The multiplier to apply to the player.</param>
        protected void SpeedUpPlayer(float multiplier)
        {
            this.SpeedUpPlayer(multiplier, () => true);
        }

        /// <summary>Speed up the player by the given multiplier for the current update tick.</summary>
        /// <param name="multiplier">The multiplier to apply to the player.</param>
        /// <param name="isActive">A lambda which returns whether the animation is still active.</param>
        protected void SpeedUpPlayer(float multiplier, Func<bool> isActive)
        {
            // Account for one update that happens by default.
            this.UpdateCounter = Math.Max(0, this.UpdateCounter + multiplier - 1);

            GameTime gameTime = Game1.currentGameTime;
            GameLocation location = Game1.player.currentLocation;
            while (this.UpdateCounter >= 1)
            {
                --this.UpdateCounter;
                if (isActive())
                    Game1.player.Update(gameTime, location);
            }
        }
    }
}
