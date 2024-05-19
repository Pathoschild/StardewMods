using System.Collections.Generic;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Handlers;

/// <summary>Handles the item-holdup animation.</summary>
/// <remarks>See game logic in <see cref="Farmer.holdUpItemThenMessage"/>.</remarks>
internal class HoldUpItemHandler : BaseAnimationHandler
{
    /*********
     ** Public methods
     *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="multiplier">The animation speed multiplier to apply.</param>
    public HoldUpItemHandler(float multiplier) : base(multiplier)
    {
    }

    /// <summary>Get whether the animation is currently active.</summary>
    /// <param name="playerAnimationID">The player's current animation ID.</param>
    public override bool IsEnabled(int playerAnimationID)
    {
        return this.IsHoldingUpItem();
    }

    /// <summary>Perform any logic needed on update while the animation is active.</summary>
    /// <param name="playerAnimationID">The player's current animation ID.</param>
    public override void Update(int playerAnimationID)
    {
        this.ApplySkips(
            () => Game1.player.Update(Game1.currentGameTime, Game1.player.currentLocation),
            () => !this.IsHoldingUpItem()
        );
    }

    private bool IsHoldingUpItem()
    {
        List<FarmerSprite.AnimationFrame>? currentAnimation = Game1.player.FarmerSprite.CurrentAnimation;

        return currentAnimation.Count >= 3 &&
               currentAnimation[0].frame == 57 && currentAnimation[0].milliseconds == 0 &&
               currentAnimation[1].frame == 57 && currentAnimation[1].milliseconds == 2500 &&
               currentAnimation[2].frame == 0 && currentAnimation[2].milliseconds == 500;
    }
}
