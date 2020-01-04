using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.FastAnimations.Framework;
using Pathoschild.Stardew.FastAnimations.Handlers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pathoschild.Stardew.FastAnimations
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
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

            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Player.Warped += this.OnWarped;
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
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // initialize handlers
            foreach (IAnimationHandler handler in this.Handlers)
                handler.OnNewLocation(Game1.currentLocation);
        }

        /// <summary>The method invoked after the player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.eventUp || !this.Handlers.Any() || !e.IsLocalPlayer)
                return;

            foreach (IAnimationHandler handler in this.Handlers)
                handler.OnNewLocation(e.NewLocation);
        }

        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.eventUp || !this.Handlers.Any())
                return;

            int playerAnimationID = this.Helper.Reflection.GetField<int>(Game1.player.FarmerSprite, "currentSingleAnimation").GetValue();
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
            // player animations
            if (config.EatAndDrinkSpeed > 1 || config.DisableEatAndDrinkConfirmation)
                yield return new EatingHandler(this.Helper.Reflection, config.EatAndDrinkSpeed, config.DisableEatAndDrinkConfirmation);
            if (config.FishingSpeed > 1)
                yield return new FishingHandler(config.FishingSpeed);
            if (config.MilkSpeed > 1)
                yield return new MilkingHandler(config.MilkSpeed);
            if (config.ShearSpeed > 1)
                yield return new ShearingHandler(config.ShearSpeed);
            if (config.ToolSwingSpeed > 1)
                yield return new ToolSwingHandler(config.ToolSwingSpeed);
            if (config.WeaponSwingSpeed > 1)
                yield return new WeaponSwingHandler(config.WeaponSwingSpeed);

            // world animations
            if (config.BreakGeodeSpeed > 1)
                yield return new BreakingGeodeHandler(config.BreakGeodeSpeed);
            if (config.CasinoSlotsSpeed > 1)
                yield return new CasinoSlotsHandler(config.CasinoSlotsSpeed, this.Helper.Reflection);
            if (config.PamBusSpeed > 1)
                yield return new PamBusHandler(config.PamBusSpeed);
            if (config.TreeFallSpeed > 1)
                yield return new TreeFallingHandler(config.TreeFallSpeed, this.Helper.Reflection);

            // UI animations
            if (config.TitleMenuTransitionSpeed > 1)
                yield return new TitleMenuHandler(config.TitleMenuTransitionSpeed, this.Helper.Reflection);
            if (config.LoadGameBlinkSpeed > 1)
                yield return new LoadGameMenuHandler(config.LoadGameBlinkSpeed, this.Helper.Reflection);
        }
    }
}
