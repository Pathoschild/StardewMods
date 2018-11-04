using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>Takes care of skipping or accelerating an animation.</summary>
    internal interface IAnimationHandler
    {
        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        bool IsEnabled(int playerAnimationID);

        /// <summary>Perform any updates needed when the player enters a new location.</summary>
        /// <param name="location">The new location.</param>
        void OnNewLocation(GameLocation location);

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        void Update(int playerAnimationID);
    }
}
