using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.FastAnimations.Framework;
using Pathoschild.Stardew.FastAnimations.Handlers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations
{
    /// <summary>The mod entry point.</summary>
    public class FastAnimationsMod : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The animation handlers which skip or accelerate specific animations.</summary>
        private IAnimationHandler[] Handlers;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            this.Handlers = this.GetHandlers(this.Config).ToArray();

            SaveEvents.AfterLoad += this.ReceiveAfterLoad;
            GameEvents.UpdateTick += this.ReceiveUpdateTick;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Events
        ****/
        /// <summary>The method invoked after the player loads a saved game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveAfterLoad(object sender, EventArgs e)
        {
            // check for updates
            if (this.Config.CheckForUpdates)
                UpdateHelper.LogVersionCheckAsync(this.Monitor, this.ModManifest, "FastAnimations");
        }

        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ReceiveUpdateTick(object sender, EventArgs e)
        {
            if (!Game1.hasLoadedGame || Game1.eventUp || !this.Handlers.Any())
                return;

            int playerAnimationID = this.Helper.Reflection.GetPrivateValue<int>(Game1.player.FarmerSprite, "currentSingleAnimation");
            foreach (IAnimationHandler handler in this.Handlers)
            {
                if (handler.IsEnabled(playerAnimationID))
                {
                    handler.Update(playerAnimationID);
                    break;
                }
            }
        }

        /****
        ** Methods
        ****/
        /// <summary>Get the enabled animation handlers.</summary>
        private IEnumerable<IAnimationHandler> GetHandlers(ModConfig config)
        {
            if (config.BreakGeodeSpeed > 1)
                yield return new BreakingGeodeHandler(config.BreakGeodeSpeed);
            if (config.EatAndDrinkSpeed > 1)
                yield return new EatingHandler(this.Helper.Reflection, config.EatAndDrinkSpeed);
            if (config.MilkSpeed > 1)
                yield return new MilkingHandler(config.MilkSpeed);
            if (config.ShearSpeed > 1)
                yield return new ShearingHandler(config.ShearSpeed);
        }
    }
}
