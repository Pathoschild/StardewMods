https://github.com/ferrles124/StardewMods/blob/develop/LookupAnything/ModEntry.cs          this.ShowLookup();
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

    /// <summary>Show the lookup UI for the subject at the given screen position (used for mobile double-tap).</summary>
    /// <param name="screenPosition">The screen position that was tapped.</param>
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

                // open menu
                if (Game1.activeClickableMenu != null)
                {
                    logMessage.Append($" searching the open '{Game1.activeClickableMenu.GetType().Name}' menu...");
                    subject = this.TargetFactory.GetSubjectFrom(Game1.activeClickableMenu, cursorPos);
                }
                else
                {
                    // HUD under tap
                    foreach (IClickableMenu menu in Game1.onScreenMenus)
                    {
                        if (menu.isWithinBounds((int)cursorPos.X, (int)cursorPos.Y))
                        {
                            logMessage.Append($" searching the on-screen '{menu.GetType().Name}' menu...");
                            subject = this.TargetFactory.GetSubjectFrom(menu, cursorPos);
                            break;
                        }
                    }

                    // world - dokunulan konumu cursor gibi kullan
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
