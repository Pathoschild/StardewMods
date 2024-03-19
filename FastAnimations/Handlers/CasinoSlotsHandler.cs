using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using StardewValley.Minigames;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the casino slots minigame spin animation.</summary>
    /// <remarks>See game logic in <see cref="Slots"/>.</remarks>
    internal class CasinoSlotsHandler : BaseAnimationHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        public CasinoSlotsHandler(float multiplier)
            : base(multiplier) { }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return Game1.currentMinigame is Slots { spinning: true };
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            Slots minigame = (Slots)Game1.currentMinigame;

            this.ApplySkips(
                run: () => minigame.tick(Game1.currentGameTime),
                until: () => !minigame.spinning
            );
        }
    }
}
