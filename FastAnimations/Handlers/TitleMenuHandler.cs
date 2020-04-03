using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles title menu transitions.</summary>
    /// <remarks>See game logic in <see cref="TitleMenu"/>.</remarks>
    class TitleMenuHandler : BaseAnimationHandler
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
        public TitleMenuHandler(float multiplier, IReflectionHelper reflection)
            : base(multiplier)
        {
            this.Reflection = reflection;
        }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return
                Game1.activeClickableMenu is TitleMenu titleMenu
                && this.GetIsTransitionField(titleMenu).GetValue();
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            TitleMenu titleMenu = (TitleMenu)Game1.activeClickableMenu;
            var isTransition = this.GetIsTransitionField(titleMenu);

            this.ApplySkips(
                run: () => titleMenu.update(Game1.currentGameTime),
                until: () => !isTransition.GetValue()
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the private title menu field which indicates whether it's currently transitioning.</summary>
        /// <param name="menu">The title menu.</param>
        private IReflectedField<bool> GetIsTransitionField(TitleMenu menu)
        {
            return this.Reflection.GetField<bool>(menu, "isTransitioningButtons");
        }
    }
}
