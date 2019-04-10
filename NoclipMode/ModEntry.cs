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

        /// <summary>The key which toggles noclip mode.</summary>
        private SButton[] ToggleKey;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();
            this.ToggleKey = CommonHelper.ParseButtons(this.Config.ToggleKey, this.Monitor, nameof(this.Config.ToggleKey));

            // hook events
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
            if (Context.IsPlayerFree && this.ToggleKey.Contains(e.Button))
            {
                Game1.player.ignoreCollisions = !Game1.player.ignoreCollisions;
                if (Game1.player.ignoreCollisions)
                    CommonHelper.ShowInfoMessage(this.Helper.Translation.Get("enabled-message", new { button = e.Button }));
            }
        }
    }
}
