using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config = null!; // set in Entry

        /// <summary>The animation handlers which skip or accelerate specific animations.</summary>
        private IAnimationHandler[] Handlers = null!; // set in Entry


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            CommonHelper.RemoveObsoleteFiles(this, "FastAnimations.pdb"); // removed in 1.11.6

            this.Config = helper.ReadConfig<ModConfig>();
            this.UpdateConfig();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
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
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // add Generic Mod Config Menu integration
            new GenericModConfigMenuIntegrationForFastAnimations(
                getConfig: () => this.Config,
                reset: () =>
                {
                    this.Config = new ModConfig();
                    this.Helper.WriteConfig(this.Config);
                    this.UpdateConfig();
                },
                saveAndApply: () =>
                {
                    this.Helper.WriteConfig(this.Config);
                    this.UpdateConfig();
                },
                modRegistry: this.Helper.ModRegistry,
                monitor: this.Monitor,
                manifest: this.ModManifest
            ).Register();
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            // initialize handlers
            foreach (IAnimationHandler handler in this.Handlers)
                handler.OnNewLocation(Game1.currentLocation);
        }

        /// <inheritdoc cref="IPlayerEvents.Warped"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.eventUp || !this.Handlers.Any() || !e.IsLocalPlayer)
                return;

            foreach (IAnimationHandler handler in this.Handlers)
                handler.OnNewLocation(e.NewLocation);
        }

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (Game1.eventUp || !this.Handlers.Any())
                return;

            int playerAnimationId = Game1.player.FarmerSprite.currentSingleAnimation;
            foreach (IAnimationHandler handler in this.Handlers)
            {
                if (handler.IsEnabled(playerAnimationId))
                {
                    handler.Update(playerAnimationId);
                    break;
                }
            }
        }

        /****
        ** Methods
        ****/
        /// <summary>Apply the mod configuration if it changed.</summary>
        [MemberNotNull(nameof(ModEntry.Handlers))]
        private void UpdateConfig()
        {
            this.Handlers = this.GetHandlers(this.Config).ToArray();
        }

        /// <summary>Get the enabled animation handlers.</summary>
        private IEnumerable<IAnimationHandler> GetHandlers(ModConfig config)
        {
            // player animations
            if (config.EatAndDrinkSpeed > 1 || config.DisableEatAndDrinkConfirmation)
                yield return new EatingHandler(config.EatAndDrinkSpeed, config.DisableEatAndDrinkConfirmation);
            if (config.FishingSpeed > 1)
                yield return new FishingHandler(config.FishingSpeed);
            if (config.HarvestSpeed > 1)
                yield return new HarvestHandler(config.HarvestSpeed);
            if (config.HorseFluteSpeed > 1)
                yield return new HorseFluteHandler(config.HorseFluteSpeed);
            if (config.MilkSpeed > 1)
                yield return new MilkingHandler(config.MilkSpeed);
            if (config.MountOrDismountSpeed > 1)
                yield return new MountHorseHandler(config.MountOrDismountSpeed);
            if (config.ShearSpeed > 1)
                yield return new ShearingHandler(config.ShearSpeed);
            if (config.UseSlingshotSpeed > 1)
                yield return new SlingshotHandler(config.UseSlingshotSpeed);
            if (config.ToolSwingSpeed > 1)
                yield return new ToolSwingHandler(config.ToolSwingSpeed);
            if (config.WeaponSwingSpeed > 1)
                yield return new WeaponSwingHandler(config.WeaponSwingSpeed);

            // world animations
            if (config.BreakGeodeSpeed > 1)
                yield return new BreakingGeodeHandler(config.BreakGeodeSpeed);
            if (config.CasinoSlotsSpeed > 1)
                yield return new CasinoSlotsHandler(config.CasinoSlotsSpeed);
            if (config.PamBusSpeed > 1)
                yield return new PamBusHandler(config.PamBusSpeed);
            if (config.TreeFallSpeed > 1)
                yield return new TreeFallingHandler(config.TreeFallSpeed);

            // UI animations
            if (config.TitleMenuTransitionSpeed > 1)
                yield return new TitleMenuHandler(config.TitleMenuTransitionSpeed);
            if (config.LoadGameBlinkSpeed > 1)
                yield return new LoadGameMenuHandler(config.LoadGameBlinkSpeed);
        }
    }
}
