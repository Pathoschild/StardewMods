using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>The base class for animation handlers.</summary>
    internal abstract class BaseAnimationHandler : IAnimationHandler
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The animation speed multiplier to apply.</summary>
        protected readonly int Multiplier;


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
        protected BaseAnimationHandler(int multiplier)
        {
            this.Multiplier = multiplier;
        }

        /// <summary>Speed up the player by the given multipler for the current update tick.</summary>
        /// <param name="multiplier">The multiplier to apply to the player.</param>
        protected void SpeedUpPlayer(int multiplier)
        {
            GameTime gameTime = Game1.currentGameTime;
            GameLocation location = Game1.player.currentLocation;
            for (int i = 1; i < multiplier; i++)
                Game1.player.Update(gameTime, location);
        }
    }
}
