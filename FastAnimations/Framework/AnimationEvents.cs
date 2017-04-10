using System;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>Detects animation changes and raises appropriate events.</summary>
    internal class AnimationEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>The player's character sprite and animation manager.</summary>
        private readonly FarmerSprite Farmer;

        /// <summary>The last animation frame detected for the farmer.</summary>
        private AnimationFrame? LastFrame;


        /*********
        ** Accessors
        *********/
        /// <summary>Raised after a new animation frame starts.</summary>
        public event EventHandler<EventArgsAnimationFrame> OnNewFrame;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="farmer">The player's character sprite and animation manager.</param>
        public AnimationEvents(FarmerSprite farmer)
        {
            this.Farmer = farmer;
        }

        /// <summary>Check for animation changes.</summary>
        public void Update()
        {
            // no current animation
            if (this.Farmer.CurrentAnimation == null || this.Farmer.CurrentAnimation.Count == 0)
            {
                this.LastFrame = null;
                return;
            }

            // raise events if new frame
            AnimationFrame frame = new AnimationFrame(this.Farmer.SourceRect, this.Farmer.CurrentAnimation[0]);
            bool isNewAnimation = this.LastFrame == null || !frame.IsSameAnimation(this.LastFrame.Value);
            bool isNewFrame = isNewAnimation || !frame.IsSameFrame(this.LastFrame.Value);

            if (isNewFrame)
            {
                this.OnNewFrame?.Invoke(null, new EventArgsAnimationFrame(frame, isNewAnimation));
                this.LastFrame = frame;
            }
        }
    }
}
