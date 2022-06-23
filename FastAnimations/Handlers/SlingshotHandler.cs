using System;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using StardewValley.Tools;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the slingshot draw animation.</summary>
    internal class SlingshotHandler : BaseAnimationHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The <see cref="GameTime.TotalGameTime"/> in milliseconds when the handler started skipping the current animation.</summary>
        private int LastPullStartTime;

        /// <summary>The total number of milliseconds that were skipped for the current animation.</summary>
        private double SkippedMilliseconds;

        /// <summary>The number of milliseconds to elapse for each skip frame.</summary>
        private const double MillisecondsPerSkip = 1000d / 60; // 60 ticks per second


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        public SlingshotHandler(float multiplier)
            : base(multiplier) { }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return
                Game1.player.UsingTool
                && Game1.player.CurrentTool is Slingshot;
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            Slingshot slingshot = (Slingshot)Game1.player.CurrentTool;

            // start new animation
            {
                int startedSkipAt = (int)slingshot.pullStartTime;
                if (startedSkipAt != this.LastPullStartTime)
                {
                    this.LastPullStartTime = startedSkipAt;
                    this.SkippedMilliseconds = 0;
                }
            }

            // apply skips
            this.ApplySkips(
                run: () =>
                {
                    slingshot.pullStartTime -= SlingshotHandler.MillisecondsPerSkip;
                    this.LastPullStartTime = (int)slingshot.pullStartTime;
                    this.SkippedMilliseconds += SlingshotHandler.MillisecondsPerSkip;

                    TimeSpan skippedTime = TimeSpan.FromMilliseconds(this.SkippedMilliseconds);
                    GameTime time = new GameTime(Game1.currentGameTime.TotalGameTime.Add(skippedTime), Game1.currentGameTime.ElapsedGameTime.Add(skippedTime), Game1.currentGameTime.IsRunningSlowly);

                    Game1.player.CurrentTool.tickUpdate(time, Game1.player);
                },
                until: () => !this.IsEnabled(-1)
            );
        }
    }
}
