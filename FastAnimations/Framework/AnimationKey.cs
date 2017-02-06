using System;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>Uniquely identifies an animation.</summary>
    internal struct AnimationKey : IEquatable<AnimationKey>, IEquatable<AnimationKey?>
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The index of the first frame.</summary>
        public readonly int StartFrame;

        /// <summary>The duration of the first frame in milliseconds.</summary>
        public readonly int StartTime;

        /// <summary>The name of the method invoked when the animation frame ends.</summary>
        public readonly string EndMethodName;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="frame">The initial animation frame.</param>
        public AnimationKey(FarmerSprite.AnimationFrame frame)
        {
            this.StartFrame = frame.frame;
            this.StartTime = frame.milliseconds;
            this.EndMethodName = frame.frameBehavior?.Method.ToString();
        }

        /// <summary>Get whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(AnimationKey other)
        {
            return this.StartFrame == other.StartFrame
                   && this.StartTime == other.StartTime
                   && this.EndMethodName == other.EndMethodName;
        }

        /// <summary>Get whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(AnimationKey? other)
        {
            return other != null && this.Equals(other.Value);
        }
    }
}