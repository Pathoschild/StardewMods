using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.FastAnimations
{
    /// <summary>The mod entry point.</summary>
    public class SkipIntroMod : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            GameEvents.GameLoaded += this.ReceiveGameLoaded;
            GameEvents.UpdateTick += this.ReceiveUpdateTick;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Events
        ****/
        /// <summary>The method invoked when the game begins loading.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveGameLoaded(object sender, EventArgs e)
        {
            // check for an updated version
            if (this.Config.CheckForUpdates)
            {
                Task.Factory.StartNew(() =>
                {
                    UpdateHelper.LogVersionCheck(this.Monitor, this.ModManifest.Version, "FastAnimations").Wait();
                });
            }
        }

        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ReceiveUpdateTick(object sender, EventArgs e)
        {
            if (this.Config.BreakGeodeSpeed > 1 && this.FastGeodeAnimation())
                return;
            if (this.Config.InstantEatAndDrink && this.SkipEatingAnimation())
                return;
            if (this.Config.InstantMilkPail && this.SkipMilkPailAnimation())
                return;
            if (this.Config.InstantShears && this.SkipShearAnimation())
                return;
        }


        /****
        ** Methods
        ****/
        /// <summary>Speed up the current break-geode animation for a given update.</summary>
        /// <returns>Returns whether a geode-breaking animation was skipped.</returns>
        /// <remarks>See original logic in <see cref="GeodeMenu.receiveLeftClick"/>.</remarks>
        private bool FastGeodeAnimation()
        {
            // get menu
            GeodeMenu menu = Game1.activeClickableMenu as GeodeMenu;
            if (menu == null || menu.geodeAnimationTimer <= 0)
                return false;

            // speed up animation
            for (int i = 1; i < this.Config.BreakGeodeSpeed; i++)
                menu.update(Game1.currentGameTime);
            return true;
        }

        /// <summary>Make the current eating animation instant.</summary>
        /// <returns>Returns whether an eating animation was skipped.</returns>
        /// <remarks>See original logic in <see cref="Game1.pressActionButton"/>, <see cref="FarmerSprite"/>'s private <c>animateOnce(Gametime)</c> method, and <see cref="Game1.doneEating"/>.</remarks>
        private bool SkipEatingAnimation()
        {
            if (!Game1.isEating || Game1.player.Sprite.CurrentAnimation == null)
                return false;

            // skip confirmation dialogue
            if (Game1.activeClickableMenu is DialogueBox eatMenu)
            {
                Response yes = this.Helper.Reflection.GetPrivateValue<List<Response>>(eatMenu, "responses")[0];
                Game1.currentLocation.answerDialogue(yes);
                eatMenu.closeDialogue();
            }

            // skip animation
            int animationID = this.GetAnimationID(Game1.player.sprite);
            Game1.playSound(animationID == FarmerSprite.drink ? "gulp" : "eat");
            Game1.doneEating();
            return true;
        }

        /// <summary>Make the current milking animation instant.</summary>
        /// <returns>Returns whether a milking animation was skipped.</returns>
        /// <remarks>See original logic in <see cref="StardewValley.Tools.MilkPail.beginUsing"/>.</remarks>
        private bool SkipMilkPailAnimation()
        {
            if (Game1.player.Sprite.CurrentAnimation == null)
                return false;

            // check current animation
            int animationID = this.GetAnimationID(Game1.player.sprite);
            if (animationID != FarmerSprite.milkDown && animationID != FarmerSprite.milkLeft && animationID != FarmerSprite.milkRight && animationID != FarmerSprite.milkUp)
                return false;

            // skip animation
            Game1.player.Sprite.StopAnimation();
            Game1.player.forceCanMove();
            Farmer.useTool(Game1.player);
            return true;
        }

        /// <summary>Make the current shearing animation instant.</summary>
        /// <returns>Returns whether a shearing animation was skipped.</returns>
        /// <remarks>See original logic in <see cref="StardewValley.Tools.Shears.beginUsing"/>.</remarks>
        private bool SkipShearAnimation()
        {
            if (Game1.player.Sprite.CurrentAnimation == null)
                return false;

            // check current animation
            int animationID = this.GetAnimationID(Game1.player.sprite);
            if (animationID != FarmerSprite.shearDown && animationID != FarmerSprite.shearLeft && animationID != FarmerSprite.shearRight && animationID != FarmerSprite.shearUp)
                return false;

            // skip animation
            Game1.player.Sprite.StopAnimation();
            Game1.player.forceCanMove();
            Farmer.useTool(Game1.player);
            return true;
        }

        /// <summary>Get an animation's internal ID.</summary>
        /// <param name="animation">The animation to check.</param>
        /// <returns>Returns the animation ID, or <c>-1</c> if none.</returns>
        private int GetAnimationID(AnimatedSprite animation)
        {
            return this.Helper.Reflection.GetPrivateValue<int>(animation, "currentSingleAnimation");
        }

        /// <summary>Get the animation's frame index.</summary>
        /// <param name="animation">The animation to check.</param>
        /// <returns>Returns the frame index, or <c>-1</c> if none.</returns>
        private int GetAnimationFrameIndex(AnimatedSprite animation)
        {
            return this.Helper.Reflection.GetPrivateValue<int>(animation, "currentAnimationIndex");
        }
    }
}
