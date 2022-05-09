using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the horse flute animation.</summary>
    /// <remarks>See game logic in <see cref="StardewValley.Object.performUseAction"/> (search for <c>(O)911</c>).</remarks>
    internal class HorseFluteHandler : BaseAnimationHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        public HorseFluteHandler(float multiplier)
            : base(multiplier) { }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            if (!Context.IsWorldReady)
                return false;

            List<FarmerSprite.AnimationFrame>? animation = Game1.player.Sprite.CurrentAnimation;
            return
                animation?.Any() == true
                && animation[0].frame == 98;
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            // speed up animation
            this.SpeedUpPlayer();

            // reduce freeze time & horse summon time
            int reduceTimersBy = (int)(BaseAnimationHandler.MillisecondsPerFrame * this.Multiplier);
            Game1.player.freezePause = Math.Max(0, Game1.player.freezePause - reduceTimersBy);
            foreach (DelayedAction action in Game1.delayedActions)
                action.timeUntilAction = Math.Max(0, action.timeUntilAction - reduceTimersBy);
        }
    }
}
