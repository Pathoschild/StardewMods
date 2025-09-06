using Microsoft.Xna.Framework;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace Pathoschild.Stardew.FastAnimations.Handlers;

/// <summary>Handles the screen fade-to-black and fade-in animations.</summary>
/// <remarks>See game logic in <see cref="Game1._update"/>.</remarks>
internal sealed class FadeHandler : BaseAnimationHandler
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public FadeHandler(float multiplier)
        : base(multiplier) { }

    /// <inheritdoc />
    public override bool TryApply(int playerAnimationId)
    {
        return
            Game1.fadeToBlack
            && Game1.CurrentEvent is null // don't change fades in events, which (a) can cause freezes and (b) can change the impact of the cutscene
            && Game1.activeClickableMenu is null // e.g. when you arrive in a location by mine cart, the festival won't start on fade-in if the mine cart dialogue hasn't finished closing yet
            && this.ApplySkipsWhile(() =>
            {
                this.UpdateFadeToBlack();

                return Game1.fadeToBlack;
            });
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Handler the screen fade to black.</summary>
    /// <remarks>Derived from <see cref="ScreenFade.UpdateFadeAlpha"/>.</remarks>
    private void UpdateFadeToBlack()
    {
        GameTime time = Game1.currentGameTime;

        if (Game1.fadeIn)
            Game1.fadeToBlackAlpha += (Game1.eventUp || Game1.farmEvent != null ? 0.0008f : 0.0019f) * time.ElapsedGameTime.Milliseconds;
        else if (!Game1.messagePause && !Game1.dialogueUp)
            Game1.fadeToBlackAlpha -= (Game1.eventUp || Game1.farmEvent != null ? 0.0008f : 0.0019f) * time.ElapsedGameTime.Milliseconds;
    }
}
