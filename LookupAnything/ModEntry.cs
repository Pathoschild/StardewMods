using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Integrations.IconicFramework;
using Pathoschild.Stardew.LookupAnything.Components;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything;

/// <summary>The mod entry point.</summary>
internal class ModEntry : Mod
{
    /*********
    ** Fields
    *********/
    /****
    ** Configuration
    ****/
    /// <summary>The mod configuration.</summary>
    private ModConfig Config = null!; // set in Entry

    /// <summary>The configured key bindings.</summary>
    private ModConfigKeys Keys => this.Config.Controls;

    /// <summary>Provides metadata that's not available from the game data directly.</summary>
    private Metadata? Metadata;

    /// <summary>The relative path to the file containing data for the <see cref="Metadata"/> field.</summary>
    private readonly string DatabaseFileName = "assets/data.json";

    /****
    ** Validation
    ****/
    /// <summary>Whether the metadata validation passed.</summary>
    [MemberNotNullWhen(true, nameof(ModEntry.Metadata), nameof(ModEntry.GameHelper), nameof(ModEntry.TargetFactory), nameof(ModEntry.DebugInterface))]
    private bool IsDataValid { get; set; }

    /****
    ** State
    ****/
    /// <summary>Provides utility methods for interacting with the game code.</summary>
    private GameHelper? GameHelper;

    /// <summary>Finds and analyzes lookup targets in the world.</summary>
    private TargetFactory? TargetFactory;

    /// <summary>Draws debug information to the screen.</summary>
    private PerScreen<DebugInterface>? DebugInterface;

    /// <summary>The previous menus shown before the current lookup UI was opened.</summary>
    private readonly PerScreen<Stack<IClickableMenu>> PreviousMenus = new(() => new());


    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        CommonHelper.RemoveObsoleteFiles(this, "LookupAnything.pdb"); // removed in 1.40.0

        // load config
        this.Config = this.LoadConfig();

        // load translations
        I18n.Init(helper.Translation);

        // load & validate database
        this.Metadata = this.LoadMetadata();
        this.IsDataValid = this.Metadata?.LooksValid() == true;
        if (!this.IsDataValid)
            this.Monitor.Log($"The {this.DatabaseFileName} file seems to be missing or corrupt. Lookups will be disabled.", LogLevel.Error);


        // validate translations
        if (!helper.Translation.GetTranslations().Any())
            this.Monitor.Log("The translation files in this mod's i18n folder seem to be missing. The mod will still work, but you'll see 'missing translation' messages. Try reinstalling the mod to fix this.", LogLevel.Warn);

        // hook up events
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.Display.RenderedHud += this.OnRenderedHud;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;
        helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }


    /*********
    ** Private methods
    *********/
    /****
    ** Event handlers
    ****/
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched" />
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        if (!this.IsDataValid)
            return;

        // initialize functionality
        this.GameHelper = new GameHelper(this.Metadata, this.Monitor, this.Helper.ModRegistry, this.Helper.Reflection);
        this.TargetFactory = new TargetFactory(this.Helper.Reflection, this.GameHelper, () => this.Config, () => this.Config.EnableTileLookups);
        this.DebugInterface = new PerScreen<DebugInterface>(() => new DebugInterface(this.GameHelper, this.TargetFactory, () => this.Config, this.Monitor));

        // add config UI
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForLookupAnything(),
            get: () => this.Config,
            set: config => this.Config = config
        );

        // add Iconic Framework integration
        IconicFrameworkIntegration iconicFramework = new(this.Helper.ModRegistry, this.Monitor);
        if (iconicFramework.IsLoaded)
        {
            iconicFramework.AddToolbarIcon(
                "LooseSprites/Cursors",
                new Rectangle(330, 357, 7, 13),
                I18n.Icon_ToggleSearch_Name,
                I18n.Icon_ToggleSearch_Desc,
                onClick: () => this.ShowLookup(ignoreCursor: true),
                onRightClick: this.TryToggleSearch
            );
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.DayStarted" />
    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (!this.IsDataValid)
            return;

        // reset low-level cache once per game day (used for expensive queries that don't change within a day)
        this.GameHelper.ResetCache(this.Monitor);
    }

    /// <inheritdoc cref="IInputEvents.ButtonsChanged" />
    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this.IsDataValid)
            return;

        this.Monitor.InterceptErrors("handling your input", () =>
        {
            ModConfigKeys keys = this.Keys;

            // pressed
            if (keys.ToggleSearch.JustPressed())
                this.TryToggleSearch();
            else if (keys.ToggleLookup.JustPressed())
                this.ToggleLookup();
            else if (keys.ScrollUp.JustPressed())
                (Game1.activeClickableMenu as IScrollableMenu)?.ScrollUp();
            else if (keys.ScrollDown.JustPressed())
                (Game1.activeClickableMenu as IScrollableMenu)?.ScrollDown();
            else if (keys.PageUp.JustPressed())
                (Game1.activeClickableMenu as IScrollableMenu)?.ScrollUp(Game1.activeClickableMenu.height);
            else if (keys.PageDown.JustPressed())
                (Game1.activeClickableMenu as IScrollableMenu)?.ScrollDown(Game1.activeClickableMenu.height);
            else if (keys.ToggleDebug.JustPressed() && Context.IsPlayerFree)
                this.DebugInterface.Value.Enabled = !this.DebugInterface.Value.Enabled;

            // released
            if (this.Config.HideOnKeyUp && keys.ToggleLookup.GetState() == SButtonState.Released)
                this.HideLookup();
        });
    }

    /// <inheritdoc cref="IDisplayEvents.MenuChanged" />
    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        // restore the previous menu if it was hidden to show the lookup UI
        this.Monitor.InterceptErrors("restoring the previous menu", () =>
        {
            if (e.NewMenu == null && (e.OldMenu is LookupMenu or SearchMenu) && this.PreviousMenus.Value.Any())
                Game1.activeClickableMenu = this.PreviousMenus.Value.Pop();
        });
    }

    /// <inheritdoc cref="IDisplayEvents.RenderedHud" />
    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!this.IsDataValid)
            return;

        // render debug interface
        if (this.DebugInterface.Value.Enabled)
            this.DebugInterface.Value.Draw(Game1.spriteBatch);
    }

    /****
    ** Lookup menu helpers
    ****/
    /// <summary>Show the lookup UI for the current target.</summary>
    private void ToggleLookup()
    {
        if (Game1.activeClickableMenu is LookupMenu)
            this.HideLookup();
        else
            this.ShowLookup();
    }

    /// <summary>Show the lookup UI for the current target.</summary>
    /// <param name="ignoreCursor">Whether to ignore the cursor position and search for a subject in front of the player.</param>
    private void ShowLookup(bool ignoreCursor = false)
    {
        if (!this.IsDataValid)
            return;

        // show menu
        StringBuilder logMessage = new("Received a lookup request...");
        this.Monitor.InterceptErrors("looking that up", () =>
        {
            try
            {
                // get target
                ISubject? subject = this.GetSubject(logMessage, ignoreCursor);
                if (subject == null)
                {
                    this.Monitor.Log($"{logMessage} no target found.");
                    return;
                }

                // show lookup UI
                this.Monitor.Log(logMessage.ToString());
                this.ShowLookupFor(subject);
            }
            catch
            {
                this.Monitor.Log($"{logMessage} an error occurred.");
                throw;
            }
        });
    }

    /// <summary>Show a lookup menu for the given subject.</summary>
    /// <param name="subject">The subject to look up.</param>
    internal void ShowLookupFor(ISubject subject)
    {
        this.Monitor.InterceptErrors("looking that up", () =>
        {
            this.Monitor.Log($"Showing {subject.GetType().Name}::{subject.Type}::{subject.Name}.");
            this.PushMenu(
                new LookupMenu(
                    subject: subject,
                    monitor: this.Monitor,
                    reflectionHelper: this.Helper.Reflection,
                    scroll: this.Config.ScrollAmount,
                    showDebugFields: this.Config.ShowDataMiningFields,
                    forceFullScreen: this.Config.ForceFullScreen,
                    showNewPage: this.ShowLookupFor
                )
            );
        });
    }

    /// <summary>Hide the lookup UI for the current target.</summary>
    private void HideLookup()
    {
        this.Monitor.InterceptErrors("closing the menu", () =>
        {
            if (Game1.activeClickableMenu is LookupMenu menu)
                menu.QueueExit();
        });
    }

    /****
    ** Search menu helpers
    ****/
    /// <summary>Toggle the search UI if applicable.</summary>
    private void TryToggleSearch()
    {
        if (Game1.activeClickableMenu is SearchMenu)
            this.HideSearch();
        else if (Context.IsWorldReady && Game1.activeClickableMenu is not LookupMenu)
            this.ShowSearch();
    }

    /// <summary>Show the search UI.</summary>
    private void ShowSearch()
    {
        if (!this.IsDataValid)
            return;

        this.PushMenu(
            new SearchMenu(this.TargetFactory.GetSearchSubjects(), this.ShowLookupFor, this.Monitor, scroll: this.Config.ScrollAmount)
        );
    }

    /// <summary>Hide the search UI.</summary>
    private void HideSearch()
    {
        if (Game1.activeClickableMenu is SearchMenu)
        {
            Game1.playSound("bigDeSelect"); // match default behaviour when closing a menu
            Game1.activeClickableMenu = null;
        }
    }

    /****
    ** Generic helpers
    ****/
    /// <summary>Read the config file, migrating legacy settings if applicable.</summary>
    private ModConfig LoadConfig()
    {
        // migrate legacy settings
        try
        {
            if (File.Exists(Path.Combine(this.Helper.DirectoryPath, "config.json")))
            {
                JObject model = this.Helper.ReadConfig<JObject>();

                // merge ToggleLookupInFrontOfPlayer bindings into ToggleLookup
                JObject? controls = model.Value<JObject?>("Controls");
                string? toggleLookup = controls?.Value<string>("ToggleLookup");
                string? toggleLookupInFrontOfPlayer = controls?.Value<string>("ToggleLookupInFrontOfPlayer");
                if (!string.IsNullOrWhiteSpace(toggleLookupInFrontOfPlayer))
                {
                    controls!.Remove("ToggleLookupInFrontOfPlayer");
                    controls["ToggleLookup"] = string.Join(", ", (toggleLookup ?? "").Split(',').Concat(toggleLookupInFrontOfPlayer.Split(',')).Select(p => p.Trim()).Where(p => p != "").Distinct());
                    this.Helper.WriteConfig(model);
                }
            }
        }
        catch (Exception ex)
        {
            this.Monitor.Log("Couldn't migrate legacy settings in config.json; they'll be removed instead.", LogLevel.Warn);
            this.Monitor.Log(ex.ToString());
        }

        // load config
        return this.Helper.ReadConfig<ModConfig>();
    }

    /// <summary>Get the most relevant subject under the player's cursor.</summary>
    /// <param name="logMessage">The log message to which to append search details.</param>
    /// <param name="ignoreCursor">Whether to ignore the cursor position and search for a subject in front of the player.</param>
    private ISubject? GetSubject(StringBuilder logMessage, bool ignoreCursor = false)
    {
        if (!this.IsDataValid)
            return null;

        // get context
        Vector2 cursorPos = this.GameHelper.GetScreenCoordinatesFromCursor();
        if (!Game1.uiMode)
            cursorPos = Utility.ModifyCoordinatesForUIScale(cursorPos); // menus use UI coordinates

        bool hasCursor =
            !ignoreCursor
            && Constants.TargetPlatform != GamePlatform.Android
            && Game1.wasMouseVisibleThisFrame; // note: only reliable when a menu isn't open

        // open menu
        if (Game1.activeClickableMenu != null)
        {
            logMessage.Append($" searching the open '{Game1.activeClickableMenu.GetType().Name}' menu...");
            return this.TargetFactory.GetSubjectFrom(Game1.activeClickableMenu, cursorPos);
        }

        // HUD under cursor
        if (hasCursor)
        {
            foreach (IClickableMenu menu in Game1.onScreenMenus)
            {
                if (menu.isWithinBounds((int)cursorPos.X, (int)cursorPos.Y))
                {
                    logMessage.Append($" searching the on-screen '{menu.GetType().Name}' menu...");
                    return this.TargetFactory.GetSubjectFrom(menu, cursorPos);
                }
            }
        }

        // world
        logMessage.Append(" searching the world...");
        return this.TargetFactory.GetSubjectFrom(Game1.player, Game1.currentLocation, hasCursor);
    }

    /// <summary>Push a new menu onto the display stack, saving the previous menu if needed.</summary>
    /// <param name="menu">The menu to show.</param>
    private void PushMenu(IClickableMenu menu)
    {
        if (this.ShouldRestoreMenu(Game1.activeClickableMenu))
        {
            this.PreviousMenus.Value.Push(Game1.activeClickableMenu);
            this.Helper.Reflection.GetField<IClickableMenu>(typeof(Game1), "_activeClickableMenu").SetValue(menu); // bypass Game1.activeClickableMenu, which disposes the previous menu
        }
        else
            Game1.activeClickableMenu = menu;
    }

    /// <summary>Load the file containing metadata that's not available from the game directly.</summary>
    private Metadata? LoadMetadata()
    {
        Metadata? metadata = null;

        this.Monitor.InterceptErrors("loading metadata", () =>
        {
            metadata = this.Helper.Data.ReadJsonFile<Metadata>(this.DatabaseFileName);
        });

        return metadata;
    }

    /// <summary>Get whether a given menu should be restored when the lookup ends.</summary>
    /// <param name="menu">The menu to check.</param>
    private bool ShouldRestoreMenu(IClickableMenu? menu)
    {
        // no menu
        if (menu == null)
            return false;

        // if 'hide on key up' is enabled, all lookups should close on key up
        if (this.Config.HideOnKeyUp && menu is LookupMenu)
            return false;

        return true;
    }
}
