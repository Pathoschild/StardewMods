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
using Pathoschild.Stardew.LookupAnything.Framework.Themes;
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
    private ModConfig Config = null!;

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
    /// <summary>Manages the theme for the menu appearance.</summary>
    private ThemeManager Theme = null!;

    /// <summary>Provides utility methods for interacting with the game code.</summary>
    private GameHelper? GameHelper;

    /// <summary>Finds and analyzes lookup targets in the world.</summary>
    private TargetFactory? TargetFactory;

    /// <summary>Draws debug information to the screen.</summary>
    private PerScreen<DebugInterface>? DebugInterface;

    /// <summary>The previous menus shown before the current lookup UI was opened.</summary>
    private readonly PerScreen<Stack<IClickableMenu>> PreviousMenus = new(() => new());

    /// <summary>The time of the last tap/click, used for double-tap detection on mobile.</summary>
    private double LastTapTime = 0;

    /// <summary>The screen position of the last tap, used for double-tap detection on mobile.</summary>
    private Vector2 LastTapPosition = Vector2.Zero;

    /// <summary>Maximum milliseconds between two taps to count as a double-tap.</summary>
    private const double DoubleTapThresholdMs = 400;
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        CommonHelper.RemoveObsoleteFiles(this, "LookupAnything.pdb");

        // load config
        this.Config = this.LoadConfig();

        // load translations
        I18n.Init(helper.Translation);

        // load theme
        this.Theme = new ThemeManager(this.Helper.Events, this.Helper.GameContent);
        this.Theme.SetCurrentTheme(this.Config.ThemeId);

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
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
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

        this.GameHelper = new GameHelper(this.Metadata, this.Monitor, this.Helper.ModRegistry, this.Helper.Reflection);
        this.TargetFactory = new TargetFactory(this.Helper.Reflection, this.GameHelper, () => this.Config, () => this.Config.EnableTileLookups);
        this.DebugInterface = new PerScreen<DebugInterface>(() => new DebugInterface(this.GameHelper, this.TargetFactory, () => this.Config, this.Monitor));

        this.RegisterConfigMenu();
        this.Theme.OnThemeDataChanged += this.RegisterConfigMenu;

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

            if (this.Config.HideOnKeyUp && keys.ToggleLookup.GetState() == SButtonState.Released)
                this.HideLookup();
        });
    }
    /// <summary>Handle a button press, detecting double-tap on mobile to trigger lookup at tapped position.</summary>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.IsDataValid)
            return;

        if (Constants.TargetPlatform != GamePlatform.Android)
            return;

        if (e.Button != SButton.MouseLeft)
            return;
      
        if (Game1.activeClickableMenu != null)
            return;

        double currentTime = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
        double elapsed = currentTime - this.LastTapTime;
        Vector2 tapPosition = new Vector2(Game1.getMouseX(), Game1.getMouseY());

        if (elapsed <= DoubleTapThresholdMs && elapsed > 0)
        {
            this.LastTapTime = 0;

                this.Monitor.InterceptErrors("handling double-tap lookup", () =>
                {
                    this.ShowLookupAtPosition(tapPosition);
                });
            }
        }
        else
        {
            this.LastTapTime = currentTime;
            this.LastTapPosition = tapPosition;
        }
    }

    /// <inheritdoc cref="IDisplayEvents.MenuChanged" />
    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
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
    private void ShowLookup(bool ignoreCursor = false)
    {
        if (!this.IsDataValid)
            return;

        StringBuilder logMessage = new("Received a lookup request...");
        this.Monitor.InterceptErrors("looking that up", () =>
        {
            try
            {
                ISubject? subject = this.GetSubject(logMessage, ignoreCursor);
                if (subject == null)
                {
                    this.Monitor.Log($"{logMessage} no target found.");
                    return;
                }

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

    /// <summary>Show the lookup UI for the subject at the given screen position.</summary>
    private void ShowLookupAtPosition(Vector2 screenPosition)
    {
        if (!this.IsDataValid)
            return;

        StringBuilder logMessage = new("Received a mobile tap lookup request...");
        this.Monitor.InterceptErrors("looking that up", () =>
        {
            try
            {
                Vector2 cursorPos = screenPosition;
                if (!Game1.uiMode)
                    cursorPos = Utility.ModifyCoordinatesForUIScale(cursorPos);

                ISubject? subject = null;

                if (Game1.activeClickableMenu != null)
                {
                    logMessage.Append($" searching the open '{Game1.activeClickableMenu.GetType().Name}' menu...");
                    subject = this.TargetFactory.GetSubjectFrom(Game1.activeClickableMenu, cursorPos);
                }
                else
                {
                    foreach (IClickableMenu menu in Game1.onScreenMenus)
                    {
                        if (menu.isWithinBounds((int)cursorPos.X, (int)cursorPos.Y))
                        {
                            logMessage.Append($" searching the on-screen '{menu.GetType().Name}' menu...");
                            subject = this.TargetFactory.GetSubjectFrom(menu, cursorPos);
                            break;
                        }
                    }

                    if (subject == null)
                    {
                        logMessage.Append(" searching the world at tap position...");
                        subject = this.TargetFactory.GetSubjectFrom(Game1.player, Game1.currentLocation, hasCursor: true);
                    }
                }

                if (subject == null)
                {
                    this.Monitor.Log($"{logMessage} no target found.");
                    return;
                }

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
                    theme: this.Theme,
                    scroll: this.Config.ScrollAmount,
                    showDataMinedValues: this.Config.ShowDataMiningFields,
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
        else if (Context.IsWorldReady)
            this.ShowSearch();
    }

    /// <summary>Show the search UI.</summary>
    private void ShowSearch()
    {
        if (!this.IsDataValid)
            return;

        this.PushMenu(
            new SearchMenu(this.TargetFactory.GetSearchSubjects(), this.ShowLookupFor, this.Monitor, this.Theme, scroll: this.Config.ScrollAmount, this.GameHelper.StardewAccess)
        );
    }

    /// <summary>Hide the search UI.</summary>
    private void HideSearch()
    {
        if (Game1.activeClickableMenu is SearchMenu)
        {
            Game1.playSound("bigDeSelect");
            Game1.activeClickableMenu = null;
        }
    }

    /****
    ** Generic helpers
    ****/
    /// <summary>Read the config file, migrating legacy settings if applicable.</summary>
    private ModConfig LoadConfig()
    {
        try
        {
            if (File.Exists(Path.Combine(this.Helper.DirectoryPath, "config.json")))
            {
                JObject model = this.Helper.ReadConfig<JObject>();

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

        return this.Helper.ReadConfig<ModConfig>();
    }

    /// <summary>Register or reset the config UI.</summary>
    private void RegisterConfigMenu()
    {
        this.AddGenericModConfigMenu(
            new GenericModConfigMenuIntegrationForLookupAnything(this.Theme),
            get: () => this.Config,
            set: config => this.Config = config
        );
    }

    /// <summary>Get the most relevant subject under the player's cursor.</summary>
    private ISubject? GetSubject(StringBuilder logMessage, bool ignoreCursor = false)
    {
        if (!this.IsDataValid)
            return null;

        Vector2 cursorPos = this.GameHelper.GetScreenCoordinatesFromCursor();
        if (!Game1.uiMode)
            cursorPos = Utility.ModifyCoordinatesForUIScale(cursorPos);

        bool hasCursor =
            !ignoreCursor
            && Constants.TargetPlatform != GamePlatform.Android
            && Game1.wasMouseVisibleThisFrame;

        if (Game1.activeClickableMenu != null)
        {
            logMessage.Append($" searching the open '{Game1.activeClickableMenu.GetType().Name}' menu...");
            return this.TargetFactory.GetSubjectFrom(Game1.activeClickableMenu, cursorPos);
        }

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

        logMessage.Append(" searching the world...");
        return this.TargetFactory.GetSubjectFrom(Game1.player, Game1.currentLocation, hasCursor);
    }

    /// <summary>Push a new menu onto the display stack, saving the previous menu if needed.</summary>
    private void PushMenu(IClickableMenu menu)
    {
        if (this.ShouldRestoreMenu(Game1.activeClickableMenu))
        {
            this.PreviousMenus.Value.Push(Game1.activeClickableMenu);
            this.Helper.Reflection.GetField<IClickableMenu>(typeof(Game1), "_activeClickableMenu").SetValue(menu);
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
    private bool ShouldRestoreMenu(IClickableMenu? menu)
    {
        if (menu == null)
            return false;

        if (this.Config.HideOnKeyUp && menu is LookupMenu)
            return false;

        return true;
    }
}
