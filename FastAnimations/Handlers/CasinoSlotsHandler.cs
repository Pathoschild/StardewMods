using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the casino slots minigame spin animation.</summary>
    /// <remarks>See game logic in <see cref="Slots"/>.</remarks>
    internal class CasinoSlotsHandler : BaseAnimationHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to private game code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        /// <param name="reflection">Simplifies access to private game code.</param>
        public CasinoSlotsHandler(float multiplier, IReflectionHelper reflection)
            : base(multiplier)
        {
            this.Reflection = reflection;
        }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return Game1.currentMinigame is Slots minigame && this.IsSpinning(minigame);
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            Slots minigame = (Slots)Game1.currentMinigame;

            this.ApplySkips(
                run: () => minigame.tick(Game1.currentGameTime),
                until: () => !this.IsSpinning(minigame)
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the minigame is playing the spinning-slots animation.</summary>
        /// <param name="slots">The casino slots minigame.</param>
        private bool IsSpinning(Slots slots)
        {
            return this.Reflection.GetField<bool>(slots, "spinning").GetValue();
        }
    }
}
