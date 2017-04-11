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
            if (this.Config.InstantGeodes && this.SkipGeodeAnimation())
                return;
            if (this.Config.InstantEatAndDrink && this.SkipEatingAnimation())
                return;
            if (this.Config.InstantMilkPail && this.SkipMilkPailAnimation())
                return;
        }


        /****
        ** Methods
        ****/
        /// <summary>Make the current break-geode animation instant.</summary>
        private bool SkipGeodeAnimation()
        {
            // get menu
            GeodeMenu menu = Game1.activeClickableMenu as GeodeMenu;
            if (menu == null)
                return false;

            // skip animation
            if (menu.geodeAnimationTimer <= 0)
                return false;
            while (menu.geodeAnimationTimer > 0)
                menu.update(Game1.currentGameTime);
            return true;
        }

        /// <summary>Make the current eating animation instant.</summary>
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
            int animationID = this.GetAnimationID(Game1.player);
            Game1.playSound(animationID == FarmerSprite.drink ? "gulp" : "eat");
            Game1.doneEating();
            return true;
        }

        /// <summary>Make the current milking animation instant.</summary>
        /// <remarks>See original logic in <see cref="Game1.pressActionButton"/>, <see cref="FarmerSprite"/>'s private <c>animateOnce(Gametime)</c> method, and <see cref="Game1.doneEating"/>.</remarks>
        private bool SkipMilkPailAnimation()
        {
            if (Game1.player.Sprite.CurrentAnimation == null)
                return false;

            // check current animation
            int animationID = this.GetAnimationID(Game1.player);
            if (animationID != FarmerSprite.milkDown && animationID != FarmerSprite.milkLeft && animationID != FarmerSprite.milkRight && animationID != FarmerSprite.milkUp)
                return false;

            // skip animation
            Game1.player.Sprite.StopAnimation();
            Game1.player.forceCanMove();
            Farmer.useTool(Game1.player);
            return true;

        }

        /// <summary>Get the player's current animation ID.</summary>
        /// <param name="player">The player whose current animation to check.</param>
        /// <returns>Returns the animation ID, or <c>-1</c> if none.</returns>
        private int GetAnimationID(Farmer player)
        {
            return this.Helper.Reflection.GetPrivateValue<int>(player.sprite, "currentSingleAnimation");
        }
    }
}
