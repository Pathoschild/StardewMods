using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.IO;

namespace Pathoschild.Stardew.FastAnimations.Handlers
{
    /// <summary>Handles the wool shearing animation.</summary>
    /// <remarks>See game logic in <see cref="StardewValley.Tools.Shears.beginUsing"/>.</remarks>
    internal class ToolHandler : BaseAnimationHandler
    {

        private readonly HashSet<TemporaryAnimatedSprite> ItemAnimations = new HashSet<TemporaryAnimatedSprite>();

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="multiplier">The animation speed multiplier to apply.</param>
        public ToolHandler(int multiplier)
            : base(multiplier) { }

        /// <summary>Get whether the animation is currently active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override bool IsEnabled(int playerAnimationID)
        {
            return
                Game1.player.Sprite.CurrentAnimation != null
                && (
                    playerAnimationID == FarmerSprite.toolDown
                        || playerAnimationID == FarmerSprite.toolRight
                        || playerAnimationID == FarmerSprite.toolUp
                        || playerAnimationID == FarmerSprite.toolLeft
                        || playerAnimationID == FarmerSprite.grabDown
                        || playerAnimationID == FarmerSprite.grabRight
                        || playerAnimationID == FarmerSprite.grabUp
                        || playerAnimationID == FarmerSprite.grabLeft
                        || playerAnimationID == FarmerSprite.toolChooseDown
                        || playerAnimationID == FarmerSprite.toolChooseRight
                        || playerAnimationID == FarmerSprite.toolChooseUp
                        || playerAnimationID == FarmerSprite.toolChooseLeft
                        || playerAnimationID == FarmerSprite.seedThrowDown
                        || playerAnimationID == FarmerSprite.seedThrowRight
                        || playerAnimationID == FarmerSprite.seedThrowUp
                        || playerAnimationID == FarmerSprite.seedThrowLeft
                        || playerAnimationID == FarmerSprite.eat
                        || playerAnimationID == FarmerSprite.sick
                        || playerAnimationID == FarmerSprite.swordswipeDown
                        || playerAnimationID == FarmerSprite.swordswipeRight
                        || playerAnimationID == FarmerSprite.swordswipeUp
                        || playerAnimationID == FarmerSprite.swordswipeLeft
                        || playerAnimationID == FarmerSprite.punchDown
                        || playerAnimationID == FarmerSprite.punchRight
                        || playerAnimationID == FarmerSprite.punchUp
                        || playerAnimationID == FarmerSprite.punchLeft
                        || playerAnimationID == FarmerSprite.harvestItemUp
                        || playerAnimationID == FarmerSprite.harvestItemRight
                        || playerAnimationID == FarmerSprite.harvestItemDown
                        || playerAnimationID == FarmerSprite.harvestItemLeft
                        || playerAnimationID == FarmerSprite.walkDown
                        || playerAnimationID == FarmerSprite.walkRight
                        || playerAnimationID == FarmerSprite.walkUp
                        || playerAnimationID == FarmerSprite.walkLeft
                        || playerAnimationID == FarmerSprite.runDown
                        || playerAnimationID == FarmerSprite.runRight
                        || playerAnimationID == FarmerSprite.runUp
                        || playerAnimationID == FarmerSprite.runLeft
                        || playerAnimationID == FarmerSprite.carryWalkDown
                        || playerAnimationID == FarmerSprite.carryWalkRight
                        || playerAnimationID == FarmerSprite.carryWalkUp
                        || playerAnimationID == FarmerSprite.carryWalkLeft
                        || playerAnimationID == FarmerSprite.carryRunDown
                        || playerAnimationID == FarmerSprite.carryRunRight
                        || playerAnimationID == FarmerSprite.carryRunUp
                        || playerAnimationID == FarmerSprite.carryRunLeft
                        || playerAnimationID == FarmerSprite.showHoldingEdible
                        || playerAnimationID == FarmerSprite.cheer
                );
        }



        /// <summary>Perform any logic needed on update while the animation is active.</summary>
        /// <param name="playerAnimationID">The player's current animation ID.</param>
        public override void Update(int playerAnimationID)
        {

            //Game1.player.FarmerSprite.intervalModifier = .1f;

            /*
            Attempt at speeding up any Temporary animations that an animation may spawn.
                Trying to speed up the water can animation was the primary motivation here.

            // speed up animations
            GameTime gameTime = Game1.currentGameTime;
            GameLocation location = Game1.player.currentLocation;
            for (int i = 1; i < this.Multiplier; i++)
            {
                // temporary item animations
                foreach (TemporaryAnimatedSprite animation in location.TemporarySprites.ToArray())
                {
                    bool animationDone = animation.update(gameTime);
                    if (animationDone)
                    {
                        this.ItemAnimations.Remove(animation);
                        location.TemporarySprites.Remove(animation);
                    }
                }
                Game1.player.Update(gameTime, location);
            }
            */

            this.SpeedUpPlayer(this.Multiplier);
        }
    }
}
