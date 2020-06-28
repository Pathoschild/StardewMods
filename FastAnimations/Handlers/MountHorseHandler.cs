using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the horse mount/dismount animation.</summary>
    internal class MountHorseHandler : BaseAnimationHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        public MountHorseHandler(float multiplier)
            : base(multiplier) { }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return this.IsMountingOrDismounting(Game1.player);
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            this.ApplySkips(
                run: () =>
                {
                    Game1.player.update(Game1.currentGameTime, Game1.player.currentLocation);
                    Game1.player.mount?.update(Game1.currentGameTime, Game1.player.currentLocation);
                },
                until: () => !this.IsMountingOrDismounting(Game1.player)
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the player is currently mounting or dismounting a horse.</summary>
        /// <param name="player">The player to check.</param>
        private bool IsMountingOrDismounting(Farmer player)
        {
            return
                Context.IsWorldReady
                && (
                    player.isAnimatingMount
                    || player.mount?.dismounting.Value == true
                );
        }
    }
}
