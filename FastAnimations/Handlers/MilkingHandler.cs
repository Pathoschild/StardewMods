using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the milking animation.</summary>
    /// <remarks>See game logic in <see cref="StardewValley.Tools.MilkPail.beginUsing"/>.</remarks>
    internal class MilkingHandler : IAnimationHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public bool IsEnabled(int playerAnimationID)
        {
            return
                Game1.player.Sprite.CurrentAnimation != null
                && (
                    playerAnimationID == FarmerSprite.milkDown
                    || playerAnimationID == FarmerSprite.milkLeft
                    || playerAnimationID == FarmerSprite.milkRight
                    || playerAnimationID == FarmerSprite.milkUp
                );
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public void Update(int playerAnimationID)
        {
            // skip animation
            Game1.player.Sprite.StopAnimation();
            Game1.player.forceCanMove();
            Farmer.useTool(Game1.player);
        }
    }
}
