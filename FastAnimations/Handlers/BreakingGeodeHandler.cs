using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the geode-breaking animation.</summary>
    /// <remarks>See game logic in <see cref="GeodeMenu.receiveLeftClick"/>.</remarks>
    internal class BreakingGeodeHandler : IAnimationHandler
    {
        /*********
        ** Properties
        *********/
        /// <summary>The animation speed multiplier to apply.</summary>
        private readonly int Multiplier;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        public BreakingGeodeHandler(int multiplier)
        {
            this.Multiplier = multiplier;
        }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public bool IsEnabled(int playerAnimationID)
        {
            return Game1.activeClickableMenu is GeodeMenu menu && menu.geodeAnimationTimer > 0;
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public void Update(int playerAnimationID)
        {
            GeodeMenu menu = (GeodeMenu)Game1.activeClickableMenu;

            for (int i = 1; i < this.Multiplier; i++)
                menu.update(Game1.currentGameTime);
        }
    }
}
