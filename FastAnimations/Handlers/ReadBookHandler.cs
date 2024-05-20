using System.Collections.Generic;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace Pathoschild.Stardew.FastAnimations.Handlers;

/// <summary>Handles the read-book animation.</summary>
/// <remarks>See game logic in <see cref="SObject.readBook"/>.</remarks>
internal class ReadBookHandler : BaseAnimationHandler
{
    /*********
     ** Public methods
     *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="multiplier">The animation speed multiplier to apply.</param>
    public ReadBookHandler(float multiplier) : base(multiplier)
    {
    }

    /// <summary>Get whether the animation is currently active.</summary>
    /// <param name="playerAnimationID">The player's current animation ID.</param>
    public override bool IsEnabled(int playerAnimationID)
    {
        return this.IsReadingBook();
    }

    /// <summary>Perform any logic needed on update while the animation is active.</summary>
    /// <param name="playerAnimationID">The player's current animation ID.</param>
    public override void Update(int playerAnimationID)
    {
        this.SpeedUpPlayer(() => !this.IsReadingBook());
    }

    /*********
     ** Private methods
     *********/
    private bool IsReadingBook()
    {
        List<FarmerSprite.AnimationFrame>? currentAnimation = Game1.player.FarmerSprite.CurrentAnimation;

        return currentAnimation.Count >= 1 &&
               currentAnimation[0].frame == 57 && currentAnimation[0].milliseconds == 1000;
    }
}
