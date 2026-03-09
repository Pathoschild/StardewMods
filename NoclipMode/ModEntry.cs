using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.NoclipMode.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Pathoschild.Stardew.NoclipMode;

/// <summary>The mod entry point.</summary>
public class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /// <summary>The mod configuration.</summary>
    private ModConfig Config = null!; // set in Entry

    /// <summary>The keys which toggle noclip mode.</summary>
    private KeybindList ToggleKey => this.Config.ToggleKey;

    /// <summary>An arbitrary number which identifies messages from Noclip Mode.</summary>
    private const int MessageId = 91871825;


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        CommonHelper.RemoveObsoleteFiles(this, "NoclipMode.pdb"); // removed in 1.3.8

        // init
        I18n.Init(helper.Translation);
        this.Config = helper.ReadConfig<ModConfig>();

        // hook events
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }


    /*********
    ** Private methods
    *********/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForNoclipMode(),
            get: () => this.Config,
            set: config => this.Config = config
        );
    }

    /// <inheritdoc cref="IInputEvents.ButtonsChanged" />
    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.CanToggle() && this.ToggleKey.JustPressed())
        {
            bool enabled = Game1.player.ignoreCollisions = !Game1.player.ignoreCollisions;
            this.ShowConfirmationMessage(enabled, this.ToggleKey);
        }
    }

    /// <summary>Show a confirmation message for the given noclip mode, if enabled.</summary>
    /// <param name="noclipEnabled">Whether noclip was enabled; else noclip was disabled.</param>
    /// <param name="keybind">The keybind that was pressed.</param>
    private void ShowConfirmationMessage(bool noclipEnabled, KeybindList keybind)
    {
        // skip if message not enabled
        if (noclipEnabled && !this.Config.ShowEnabledMessage)
            return;
        if (!noclipEnabled && !this.Config.ShowDisabledMessage)
            return;

        // show message
        Game1.hudMessages.RemoveAll(p => p.number == ModEntry.MessageId);
        string? keybindStr = keybind.GetKeybindCurrentlyDown()?.ToString();
        string text = noclipEnabled ? I18n.EnabledMessage(keybindStr) : I18n.DisabledMessage(keybindStr);
        Game1.addHUDMessage(new HUDMessage(text, HUDMessage.error_type) { noIcon = true, number = ModEntry.MessageId });
    }

    /// <summary>Get whether noclip mode can be toggled in the current context.</summary>
    private bool CanToggle()
    {
        return
            Context.IsPlayerFree // free to move
            || (Context.IsWorldReady && Game1.eventUp); // in a cutscene (so players can get unstuck if something blocks scripted movement)
    }
}
