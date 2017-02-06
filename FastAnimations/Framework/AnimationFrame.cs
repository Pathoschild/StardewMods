using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>Metadata about a farmer animation frame.</summary>
    internal struct AnimationFrame
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The index of the first frame in the animation.</summary>
        public readonly int AnimationStartFrame;

        /// <summary>The duration of the first frame in milliseconds.</summary>
        public readonly int AnimationStartTime;

        /// <summary>The name of the method invoked when the animation frame ends.</summary>
        public readonly string AnimationStartBehaviour;

        /// <summary>The area of the spritesheet to show for this frame.</summary>
        public readonly Rectangle SourceRectangle;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="sourceRectangle">The area of the spritesheet to show for this frame.</param>
        /// <param name="startFrame">The first frame in the animation.</param>
        public AnimationFrame(Rectangle sourceRectangle, FarmerSprite.AnimationFrame startFrame)
        {
            this.AnimationStartFrame = startFrame.frame;
            this.AnimationStartTime = startFrame.milliseconds;
            this.AnimationStartBehaviour = startFrame.frameBehavior?.Method.ToString();
            this.SourceRectangle = sourceRectangle;
        }

        /// <summary>Get whether this frame is part of the same animation as another.</summary>
        /// <param name="other">The other frame to compare.</param>
        public bool IsSameAnimation(AnimationFrame other)
        {
            return
                this.AnimationStartFrame == other.AnimationStartFrame
                && this.AnimationStartTime == other.AnimationStartTime
                && this.AnimationStartBehaviour == other.AnimationStartBehaviour;
        }

        /// <summary>Get whether this frame is equivalent to another.</summary>
        /// <param name="other">The other frame to compare.</param>
        public bool IsSameFrame(AnimationFrame other)
        {
            return
                this.IsSameAnimation(other)
                || this.SourceRectangle == other.SourceRectangle;
        }
    }
}
