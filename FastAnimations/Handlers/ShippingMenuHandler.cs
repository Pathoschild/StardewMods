using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles shipping menu transitions.</summary>
    /// <remarks>See game logic in <see cref="ShippingMenu"/>.</remarks>
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
        }

        /// <inheritdoc />
        public override bool TryApply(int playerAnimationId)
        {
            return
                this.IsTransitioning(Game1.activeClickableMenu as ShippingMenu)
                && this.ApplySkipsWhile(() =>
                {
                    if (Game1.activeClickableMenu is ShippingMenu menu && this.IsTransitioning(menu))
                    {
                        menu.update(Game1.currentGameTime);
                        return true;
                    }

                    return false;
                });
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the shipping menu is showing a transition animation.</summary>
        /// <param name="menu">The shipping menu.</param>
        private bool IsTransitioning(ShippingMenu? menu)
        {
            return
                menu is not null

                // is transitioning
                && (
                    this.Reflection.GetField<bool>(menu, "outro").GetValue()
                    || this.Reflection.GetField<int>(menu, "introTimer").GetValue() > 0
                )

                // not saving
                && (
                    this.Reflection.GetField<object?>(menu, "saveGameMenu").GetValue() is null
                    || this.Reflection.GetField<bool>(menu, "savedYet").GetValue()
                );
        }
    }
}
