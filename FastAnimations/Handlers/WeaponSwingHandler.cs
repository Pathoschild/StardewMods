using Pathoschild.Stardew.FastAnimations.Framework;
using StardewValley;
using StardewValley.Tools;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the tool swinging animation.</summary>
    internal class WeaponSwingHandler : BaseAnimationHandler
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        public WeaponSwingHandler(int multiplier)
            : base(multiplier) { }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return Game1.player.UsingTool && Game1.player.CurrentTool is MeleeWeapon;
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            GameLocation location = Game1.player.currentLocation;

            for (int i = 1; i < this.Multiplier; i++)
                if (Game1.player.UsingTool)
                    Game1.player.Update(Game1.currentGameTime, location);
                else
                    break;
        }
    }
}
