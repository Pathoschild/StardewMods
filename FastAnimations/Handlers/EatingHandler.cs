using System.Collections.Generic;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the eating animation.</summary>
    internal class EatingHandler : IAnimationHandler
    {
        /*********
        ** Properties
        *********/
        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private code.</param>
        public EatingHandler(IReflectionHelper reflection)
        {
            this.Reflection = reflection;
        }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public bool IsEnabled(int playerAnimationID)
        {
            return Game1.isEating && Game1.player.Sprite.CurrentAnimation != null;
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public void Update(int playerAnimationID)
        {
            // skip confirmation dialogue
            if (Game1.activeClickableMenu is DialogueBox eatMenu)
            {
                Response yes = this.Reflection.GetPrivateValue<List<Response>>(eatMenu, "responses")[0];
                Game1.currentLocation.answerDialogue(yes);
                eatMenu.closeDialogue();
            }

            // skip animation
            Game1.playSound(playerAnimationID == FarmerSprite.drink ? "gulp" : "eat");
            Game1.doneEating();
        }
    }
}
