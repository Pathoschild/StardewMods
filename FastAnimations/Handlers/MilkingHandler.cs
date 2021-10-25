using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the milking animation.</summary>
    /// <remarks>See game logic in <see cref="StardewValley.Tools.MilkPail.beginUsing"/>.</remarks>
    internal class MilkingHandler : BaseAnimationHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        public MilkingHandler(float multiplier)
            : base(multiplier) { }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return
                Context.IsWorldReady
                && Game1.player.Sprite.CurrentAnimation != null
                && playerAnimationID is FarmerSprite.milkDown or FarmerSprite.milkLeft or FarmerSprite.milkRight or FarmerSprite.milkUp;
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            this.SpeedUpPlayer();
        }
    }
}
