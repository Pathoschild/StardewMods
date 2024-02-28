using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the load game menu animations.</summary>
    /// <remarks>See game logic in <see cref="LoadGameMenu"/>.</remarks>
    internal class LoadGameMenuHandler : BaseAnimationHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        public LoadGameMenuHandler(float multiplier)
            : base(multiplier) { }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return
                Game1.activeClickableMenu is TitleMenu
                && TitleMenu.subMenu is LoadGameMenu loadGameMenu
                && loadGameMenu.timerToLoad > 0;
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            LoadGameMenu menu = (LoadGameMenu)TitleMenu.subMenu;

            this.ApplySkips(
                run: () => menu.update(Game1.currentGameTime),
                until: () => menu.timerToLoad <= 0
            );
        }
    }
}
