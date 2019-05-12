using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.NoclipMode.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pathoschild.Stardew.NoclipMode
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Fields
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

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.IsPlayerFree && this.Config.ToggleKey.Any(p => p == e.Button))
            {
                Game1.player.ignoreCollisions = !Game1.player.ignoreCollisions;
                if (Game1.player.ignoreCollisions)
                    CommonHelper.ShowInfoMessage(this.Helper.Translation.Get("enabled-message", new { button = e.Button }));
            }
        }
    }
}
