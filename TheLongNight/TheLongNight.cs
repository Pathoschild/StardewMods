using System;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.TheLongNight.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace Pathoschild.Stardew.TheLongNight
{
    /// <summary>The mod entry point.</summary>
    public class TheLongNight : Mod
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod configuration.</summary>
        private ModConfig Config;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            SaveEvents.AfterLoad += this.SaveEvents_AfterLoad;
            TimeEvents.TimeOfDayChanged += this.TimeEvents_TimeOfDayChanged;
        }


        /*********
        ** Public methods
        *********/
        /// <summary>The method invoked after the player loads a saved game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            // check for updates
            if (this.Config.CheckForUpdates)
                UpdateHelper.LogVersionCheckAsync(this.Monitor, this.ModManifest, "TheLongNight");
        }

        /// <summary>The method invoked when the in-game clock changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TimeEvents_TimeOfDayChanged(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady || Game1.timeOfDay < 2600)
                return;

            FarmerSprite sprite = (FarmerSprite)Game1.player.Sprite;
            var animation = sprite.CurrentAnimation;
            if (animation != null && animation.Any(frame => frame.frameBehavior == SFarmer.passOutFromTired))
            {
                Game1.player.freezePause = 0;
                Game1.player.canMove = true;
                sprite.PauseForSingleAnimation = false;
                sprite.StopAnimation();
            }
        }
    }
}
