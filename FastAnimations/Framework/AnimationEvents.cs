using System;
using System.Linq;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>Detects animation changes and raises appropriate events.</summary>
    internal class AnimationEvents
    {
        /*********
        ** Properties
        *********/
        /// <summary>Manages sprites and animations for the player's character.</summary>
        private readonly FarmerSprite Farmer;

        /// <summary>The last animation detected for the farmer.</summary>
        private AnimationKey? LastFarmerAnimation;


        /*********
        ** Accessors
        *********/
        /// <summary>Raised after an animation starts.</summary>
        public event Action<AnimationKey> OnAnimationStarted;

        /// <summary>Raised after an animation ends.</summary>
        public event Action<AnimationKey> OnAnimationEnded;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="farmer">Manages sprites and animations for the player's character.</param>
        public AnimationEvents(FarmerSprite farmer)
        {
            this.Farmer = farmer;
        }

        /// <summary>Check for animation changes.</summary>
        public void Update()
        {
            // animation ended
            if (this.Farmer.CurrentAnimation?.Any() != true)
            {
                if (this.LastFarmerAnimation != null)
                    this.OnAnimationEnded?.Invoke(this.LastFarmerAnimation.Value);
                this.LastFarmerAnimation = null;
                return;
            }

            // animation started
            AnimationKey key = new AnimationKey(this.Farmer.CurrentAnimation.First());
            if (!key.Equals(this.LastFarmerAnimation))
            {
                this.LastFarmerAnimation = key;
                this.OnAnimationStarted?.Invoke(key);
            }
        }
    }
}
