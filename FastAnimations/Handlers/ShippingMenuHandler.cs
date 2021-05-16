using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles shipping menu transitions.</summary>
    /// <remarks>See game logic in <see cref="Shipping"/>.</remarks>
    internal class ShippingMenuHandler : BaseAnimationHandler
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
        public ShippingMenuHandler(float multiplier, IReflectionHelper reflection)
            : base(multiplier)
        {
            this.Reflection = reflection;
            this.NeedsUnvalidatedUpdateTick = true;
        }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return
                Game1.activeClickableMenu is ShippingMenu shippingMenu
                && !this.GetIsTransitionField(shippingMenu).GetValue();
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            ShippingMenu shippingMenu = (ShippingMenu)Game1.activeClickableMenu;
            var isTransition = this.GetIsTransitionField(shippingMenu);

            this.ApplySkips(
                run: () => shippingMenu.update(Game1.currentGameTime),
                until: () => isTransition.GetValue()
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the private shipping menu field which indicates whether it's currently transitioning.</summary>
        /// <param name="menu">The shipping menu.</param>
        private IReflectedField<bool> GetIsTransitionField(ShippingMenu menu)
        {
            return this.Reflection.GetField<bool>(menu, "savedYet");
        }
    }
}
