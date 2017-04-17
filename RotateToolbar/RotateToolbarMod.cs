using System;
using System.Threading.Tasks;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.RotateToolbar.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Pathoschild.Stardew.RotateToolbar
{
    /// <summary>The mod entry point.</summary>
    public class RotateToolbarMod : Mod
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
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The method invoked when the player loads the game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveGameLoaded(object sender, EventArgs e)
        {
            // check for an updated version
            if (this.Config.CheckForUpdates)
            {
                Task.Factory.StartNew(() =>
                {
                    UpdateHelper.LogVersionCheck(this.Monitor, this.ModManifest.Version, "RotateToolbar").Wait();
                });
            }
        }
    }
}
