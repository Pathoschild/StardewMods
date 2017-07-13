namespace Pathoschild.Stardew.FastAnimations.Framework
{
    /// <summary>The base class for animation handlers.</summary>
    internal abstract class BaseAnimationHandler : IAnimationHandler
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The animation speed multiplier to apply.</summary>
        protected readonly int Multiplier;


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public abstract bool IsEnabled(int playerAnimationID);

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public abstract void Update(int playerAnimationID);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        protected BaseAnimationHandler(int multiplier)
        {
            this.Multiplier = multiplier;
        }
    }
}
