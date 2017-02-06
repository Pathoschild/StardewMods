using System;
using System.Threading.Tasks;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.FastAnimations.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

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

        /// <summary>Raises events for animation changes.</summary>
        private AnimationEvents AnimationEvents;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            GameEvents.GameLoaded += this.ReceiveGameLoaded;
            SaveEvents.AfterLoad += this.ReceiveSaveLoaded;
            GameEvents.UpdateTick += this.ReceiveUpdateTick;
        }


        /*********
        ** Private methods
        *********/
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

        /// <summary>The method invoked after the player loads a save.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveSaveLoaded(object sender, EventArgs e)
        {
            // initialise animation events
            this.AnimationEvents = new AnimationEvents(Game1.player.FarmerSprite);
            this.AnimationEvents.OnAnimationStarted += this.ReceiveAnimationStarted;
        }

        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void ReceiveUpdateTick(object sender, EventArgs e)
        {
            this.AnimationEvents?.Update();
        }

        /// <summary>The method invoked when an animation starts.</summary>
        /// <param name="key">The animation key.</param>
        private void ReceiveAnimationStarted(AnimationKey key)
        {
        }
    }
}
