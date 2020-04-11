using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the eating animation.</summary>
    /// <remarks>See game logic in <see cref="Game1.pressActionButton"/> (opens confirmation dialogue), <see cref="Farmer.showEatingItem"/> (main animation logic), and <see cref="FarmerSprite"/>'s private <c>animateOnce(GameTime)</c> method (runs animation + some logic).</remarks>
    internal class EatingHandler : BaseAnimationHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The temporary animations showing the item thrown into the air.</summary>
        private readonly HashSet<TemporaryAnimatedSprite> ItemAnimations = new HashSet<TemporaryAnimatedSprite>();

        /// <summary>Whether to disable the confirmation dialogue before eating or drinking.</summary>
        private readonly bool DisableConfirmation;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        /// <param name="disableConfirmation">Whether to disable the confirmation dialogue before eating or drinking.</param>
        public EatingHandler(IReflectionHelper reflection, float multiplier, bool disableConfirmation)
            : base(multiplier)
        {
            this.Reflection = reflection;
            this.DisableConfirmation = disableConfirmation;
        }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            // to allow disabling the confirmation even if the animation isn't sped up, the handler is still called with multiplier ≤ 1
            return
                (this.Multiplier > 1 && this.IsAnimationPlaying())
                || (this.DisableConfirmation && this.IsConfirmationShown(out _));
        }

        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {
            // disable confirmation
            if (this.DisableConfirmation && this.IsConfirmationShown(out DialogueBox eatMenu))
            {
                // When the animation starts, the game shows a yes/no dialogue asking the player to
                // confirm they really want to eat the item. This code answers 'yes' and closes the
                // dialogue.
                Response yes = this.Reflection.GetField<List<Response>>(eatMenu, "responses").GetValue()[0];
                Game1.currentLocation.answerDialogue(yes);
                eatMenu.closeDialogue();
            }

            // speed up animation
            if (this.Multiplier > 1 && this.IsAnimationPlaying())
            {
                // The farmer eating animation spins off two main temporary animations: the item being
                // held (at index 1) and the item being thrown into the air (at index 2). The drinking
                // animation only has one temporary animation (at index 1). This code runs after each
                // one is spawned, and adds it to the list of temporary animations to handle.
                int indexInAnimation = Game1.player.FarmerSprite.currentAnimationIndex;
                if (indexInAnimation <= 1)
                    this.ItemAnimations.Clear();
                if ((indexInAnimation == 1 || (indexInAnimation == 2 && playerAnimationID == FarmerSprite.eat)) && Game1.player.itemToEat is Object obj && obj.ParentSheetIndex != Object.stardrop)
                {
                    Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, obj.ParentSheetIndex, 16, 16);
                    TemporaryAnimatedSprite tempAnimation = Game1.player.currentLocation.TemporarySprites.LastOrDefault(p => p.Texture == Game1.objectSpriteSheet && p.sourceRect == sourceRect);
                    if (tempAnimation != null)
                        this.ItemAnimations.Add(tempAnimation);
                }

                // speed up animations
                GameTime gameTime = Game1.currentGameTime;
                GameLocation location = Game1.player.currentLocation;
                this.ApplySkips(() =>
                {
                    // temporary item animations
                    foreach (TemporaryAnimatedSprite animation in this.ItemAnimations.ToArray())
                    {
                        bool animationDone = animation.update(gameTime);
                        if (animationDone)
                        {
                            this.ItemAnimations.Remove(animation);
                            location.TemporarySprites.Remove(animation);
                        }
                    }

                    // eating animation
                    Game1.player.Update(gameTime, location);
                });
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get whether the eat/drink confirmation is being shown.</summary>
        /// <param name="menu">The confirmation menu.</param>
        private bool IsConfirmationShown(out DialogueBox menu)
        {
            if (Game1.player.itemToEat != null && Game1.activeClickableMenu is DialogueBox dialogue)
            {
                string actualLine = this.Reflection.GetField<List<string>>(dialogue, "dialogues").GetValue().FirstOrDefault();
                bool isConfirmation =
                    actualLine == Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3159", Game1.player.itemToEat.DisplayName) // drink
                    || actualLine == Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3160", Game1.player.itemToEat.DisplayName); // eat

                if (isConfirmation)
                {
                    menu = dialogue;
                    return true;
                }
            }

            menu = null;
            return false;

        }

        /// <summary>Get whether the eat/drink animation is being played.</summary>
        private bool IsAnimationPlaying()
        {
            return
                Context.IsWorldReady
                && Game1.player.isEating
                && Game1.player.Sprite.CurrentAnimation != null;
        }
    }
}
