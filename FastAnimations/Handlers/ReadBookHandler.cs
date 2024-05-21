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
     ** Field
     *********/
    /// <summary>Whether the read-book animation is End.</summary>
    private bool IsReadingBookEnd;

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
        var player = Game1.player;

        // Handler remaining pause time after the end of the animation
        if (this.IsReadingBookEnd)
        {
            this.IsReadingBookEnd = false;
            player.forceCanMove();
        }

        return this.IsReadingBook(player);
    }

    /// <summary>Perform any logic needed on update while the animation is active.</summary>
    /// <param name="playerAnimationID">The player's current animation ID.</param>
    public override void Update(int playerAnimationID)
    {
        var player = Game1.player;
        var location = player.currentLocation;

        this.ApplySkips(
            () =>
            {
                // player animation
                player.Update(Game1.currentGameTime, location);

                this.IsReadingBookEnd = !this.IsReadingBook(player);
            },
            () => !this.IsReadingBook(player)
        );
    }

    /*********
     ** Private methods
     *********/
    /// <summary>Check whether the current player's animation is the animation of reading book.</summary>
    /// <remarks>Derived from <see cref="SObject.readBook"/>.</remarks>
    private bool IsReadingBook(Farmer player)
    {
        List<FarmerSprite.AnimationFrame>? currentAnimation = player.FarmerSprite.CurrentAnimation;

        return currentAnimation.Count >= 1 &&
               currentAnimation[0].frame == 57 && currentAnimation[0].milliseconds == 1000;
    }
}
