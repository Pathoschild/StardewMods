using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Input;
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

        /// <summary>The keys which toggle noclip mode.</summary>
        private KeyBinding ToggleKey;

        /// <summary>An arbitrary number which identifies messages from Noclip Mode.</summary>
        private const int MessageID = 91871825;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();
            this.ToggleKey = CommonHelper.ParseButtons(this.Config.ToggleKey, helper.Input, this.Monitor, nameof(this.Config.ToggleKey));

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
            if (this.CanToggle() && this.ToggleKey.JustPressedUnique())
            {
                bool enabled = Game1.player.ignoreCollisions = !Game1.player.ignoreCollisions;
                this.ShowConfirmationMessage(enabled, e.Button);
            }
        }

        /// <summary>Show a confirmation message for the given noclip mode, if enabled.</summary>
        /// <param name="noclipEnabled">Whether noclip was enabled; else noclip was disabled.</param>
        /// <param name="button">The toggle button that was pressed.</param>
        private void ShowConfirmationMessage(bool noclipEnabled, SButton button)
        {
            // skip if message not enabled
            if (noclipEnabled && !this.Config.ShowEnabledMessage)
                return;
            if (!noclipEnabled && !this.Config.ShowDisabledMessage)
                return;

            // show message
            Game1.hudMessages.RemoveAll(p => p.number == ModEntry.MessageID);
            string text = this.Helper.Translation.Get(noclipEnabled ? "enabled-message" : "disabled-message", new { button = button });
            Game1.addHUDMessage(new HUDMessage(text, HUDMessage.error_type) { noIcon = true, number = ModEntry.MessageID });
        }

        /// <summary>Get whether noclip mode can be toggled in the current context.</summary>
        private bool CanToggle()
        {
            return
                Context.IsPlayerFree // free to move
                || (Context.IsWorldReady && Game1.eventUp); // in a cutscene (so players can get unstuck if something blocks scripted movement)
        }
    }
}
