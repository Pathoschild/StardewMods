using System;

namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>Event arguments for an animation frame event.</summary>
    internal class EventArgsAnimationFrame : EventArgs
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The animation frame.</summary>
        public AnimationFrame Frame { get; }

        /// <summary>Whether this is the first frame of a new animation.</summary>
        public bool IsNewAnimation { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="frame">The animation frame.</param>
        /// <param name="isNewAnimation">Whether this is the first frame of a new animation.</param>
        public EventArgsAnimationFrame(AnimationFrame frame, bool isNewAnimation)
        {
            this.Frame = frame;
            this.IsNewAnimation = isNewAnimation;
        }
    }
}
