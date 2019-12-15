using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Commands;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;
using ContentPatcher.Framework.Validators;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;

[assembly: InternalsVisibleTo("Pathoschild.Stardew.Tests.Mods")]
namespace ContentPatcher
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The name of the file which contains patch metadata.</summary>
        private readonly string PatchFileName = "content.json";

        /// <summary>The name of the file which contains player settings.</summary>
        private readonly string ConfigFileName = "config.json";

        /// <summary>The supported format versions.</summary>
        private readonly string[] SupportedFormatVersions = { "1.0.0", "1.3.0", "1.4.0", "1.5.0", "1.6.0", "1.7.0", "1.8.0", "1.9.0", "1.10.0", "1.11.0" };

        /// <summary>The format version migrations to apply.</summary>
        private readonly Func<IMigration[]> Migrations = () => new IMigration[]
        {
            new Migration_1_3(),
            new Migration_1_4(),
            new Migration_1_5(),
            new Migration_1_6(),
            new Migration_1_7(),
            new Migration_1_8(),
            new Migration_1_9(),
            new Migration_1_10(),
            new Migration_1_11()
        };

        /// <summary>The special validation logic to apply to assets affected by patches.</summary>
        private readonly Func<IAssetValidator[]> AssetValidators = () => new IAssetValidator[]
        {
            new StardewValley_1_3_36_Validator()
        };

        /// <summary>Manages the available contextual tokens.</summary>
        private TokenManager TokenManager;

        /// <summary>Manages loaded patches.</summary>
        private PatchManager PatchManager;

        /// <summary>Handles the 'patch' console command.</summary>
        private CommandHandler CommandHandler;

        /// <summary>The mod configuration.</summary>
        private ModConfig Config;

        /// <summary>The configured key bindings.</summary>
        private ModConfigKeys Keys;

        /// <summary>The debug overlay (if enabled).</summary>
        private DebugOverlay DebugOverlay;

        /// <summary>The mod tokens queued for addition. This is null after the first update tick, when new tokens can no longer be added.</summary>
        private List<ModProvidedToken> QueuedModTokens = new List<ModProvidedToken>();

        /// <summary>Whether the next tick is the first one.</summary>
        private bool IsFirstTick = true;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            this.Keys = this.Config.Controls.ParseControls(this.Monitor);

            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        /// <summary>Get an API that other mods can access. This is always called after <see cref="Entry"/>.</summary>
        public override object GetApi()
        {
            return new ContentPatcherAPI(this.ModManifest.UniqueID, this.Monitor, this.AddModToken);
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>Raised after the game performs its overall update tick (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // initialize after first tick so other mods can register their tokens in SMAPI's GameLoop.GameLaunched event
            if (this.IsFirstTick)
            {
                this.IsFirstTick = false;
                this.Initialize();
            }
        }

        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this.Config.EnableDebugFeatures)
            {
                // toggle overlay
                if (this.Keys.ToggleDebug.Contains(e.Button))
                {
                    if (this.DebugOverlay == null)
                        this.DebugOverlay = new DebugOverlay(this.Helper.Events, this.Helper.Input, this.Helper.Content);
                    else
                    {
                        this.DebugOverlay.Dispose();
                        this.DebugOverlay = null;
                    }
                    return;
                }

                // cycle textures
                if (this.DebugOverlay != null)
                {
                    if (this.Keys.DebugPrevTexture.Contains(e.Button))
                        this.DebugOverlay.PrevTexture();
                    if (this.Keys.DebugNextTexture.Contains(e.Button))
                        this.DebugOverlay.NextTexture();
                }
            }
        }

        /// <summary>Raised when the low-level stage in the game's loading process has changed. This is an advanced event for mods which need to run code at specific points in the loading process. The available stages or when they happen might change without warning in future versions (e.g. due to changes in the game's load process), so mods using this event are more likely to break or have bugs.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnLoadStageChanged(object sender, LoadStageChangedEventArgs e)
        {
            switch (e.NewStage)
            {
                case LoadStage.CreatedBasicInfo:
                case LoadStage.SaveLoadedBasicInfo:
                case LoadStage.Loaded when Game1.dayOfMonth == 0: // handled by OnDayStarted if we're not creating a new save
                    this.Monitor.VerboseLog($"Updating context: load stage changed to {e.NewStage}.");
                    this.TokenManager.IsBasicInfoLoaded = true;
                    this.UpdateContext();
                    break;
            }
        }

        /// <summary>The method invoked when a new day starts.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            this.Monitor.VerboseLog("Updating context: new day started.");
            this.TokenManager.IsBasicInfoLoaded = true;
            this.UpdateContext();
        }

        /// <summary>The method invoked when the player warps.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            ConditionType[] affectedTokens = new[] { ConditionType.LocationName, ConditionType.IsOutdoors };
            this.Monitor.VerboseLog($"Updating context for {string.Join(", ", affectedTokens)}: player warped.");
            this.UpdateContext(affectedTokens);
        }

        /// <summary>The method invoked when the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.Monitor.VerboseLog("Updating context: returned to title.");
            this.TokenManager.IsBasicInfoLoaded = false;
            this.UpdateContext();
        }

        /****
        ** Methods
        ****/
        /// <summary>Initialize the mod and content packs.</summary>
        private void Initialize()
        {
            var helper = this.Helper;

            // init migrations
            IMigration[] migrations = this.Migrations();

            // fetch content packs
            RawContentPack[] contentPacks = this.GetContentPacks(migrations).ToArray();
            InvariantHashSet installedMods = new InvariantHashSet(
                (contentPacks.Select(p => p.Manifest.UniqueID))
                .Concat(helper.ModRegistry.GetAll().Select(p => p.Manifest.UniqueID))
                .OrderByIgnoreCase(p => p)
            );

            // load content packs and context
            this.TokenManager = new TokenManager(helper.Content, installedMods, this.QueuedModTokens, this.Helper.Reflection);
            this.PatchManager = new PatchManager(this.Monitor, this.TokenManager, this.AssetValidators());
            this.UpdateContext(); // set initial context before loading any custom mod tokens

            // load context
            this.LoadContentPacks(contentPacks, installedMods);
            this.UpdateContext(); // set initial context once patches + dynamic tokens + custom tokens are loaded

            // register patcher
            helper.Content.AssetLoaders.Add(this.PatchManager);
            helper.Content.AssetEditors.Add(this.PatchManager);

            // set up events
            if (this.Config.EnableDebugFeatures)
                helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.Specialized.LoadStageChanged += this.OnLoadStageChanged;

            // set up commands
            this.CommandHandler = new CommandHandler(this.TokenManager, this.PatchManager, this.Monitor, modID => modID == null ? this.TokenManager : this.TokenManager.GetContextFor(modID), () => this.UpdateContext());
            helper.ConsoleCommands.Add(this.CommandHandler.CommandName, $"Starts a Content Patcher command. Type '{this.CommandHandler.CommandName} help' for details.", (name, args) => this.CommandHandler.Handle(args));

            // can no longer queue tokens
            this.QueuedModTokens = null;
        }

        /// <summary>Add a mod-provided token.</summary>
        /// <param name="token">The token to add.</param>
        private void AddModToken(ModProvidedToken token)
        {
            if (!this.IsFirstTick)
            {
                this.Monitor.Log($"Rejected token added by {token.Mod.Name} because tokens can't be added after SMAPI's {nameof(this.Helper.Events.GameLoop)}.{nameof(this.Helper.Events.GameLoop.GameLaunched)} event.", LogLevel.Error);
                return;
            }

            this.Monitor.Log($"{token.Mod.Name} added custom token: {token.Name}", LogLevel.Trace);
            this.QueuedModTokens.Add(token);
        }

        /// <summary>Update the current context.</summary>
        /// <param name="affectedTokens">The specific tokens for which to update context, or <c>null</c> to affect all tokens</param>
        private void UpdateContext(ConditionType[] affectedTokens = null)
        {
            InvariantHashSet set = affectedTokens != null
                ? new InvariantHashSet(affectedTokens.Select(p => p.ToString()))
                : null;

            this.TokenManager.UpdateContext(set);
            this.PatchManager.UpdateContext(this.Helper.Content, set);
        }

        /// <summary>Load the registered content packs.</summary>
        /// <param name="migrations">The format version migrations to apply.</param>
        /// <returns>Returns the loaded content pack IDs.</returns>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure", Justification = "The value is used immediately, so this isn't an issue.")]
        private IEnumerable<RawContentPack> GetContentPacks(IMigration[] migrations)
        {
            this.Monitor.VerboseLog("Preloading content packs...");

            foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
            {
                RawContentPack rawContentPack;
                try
                {
                    // validate content.json has required fields
                    ContentConfig content = contentPack.ReadJsonFile<ContentConfig>(this.PatchFileName);
                    if (content == null)
                    {
                        this.Monitor.Log($"Ignored content pack '{contentPack.Manifest.Name}' because it has no {this.PatchFileName} file.", LogLevel.Error);
                        continue;
                    }
                    if (content.Format == null || content.Changes == null)
                    {
                        this.Monitor.Log($"Ignored content pack '{contentPack.Manifest.Name}' because it doesn't specify the required {nameof(ContentConfig.Format)} or {nameof(ContentConfig.Changes)} fields.", LogLevel.Error);
                        continue;
                    }

                    // apply migrations
                    IMigration migrator = new AggregateMigration(content.Format, this.SupportedFormatVersions, migrations);
                    if (!migrator.TryMigrate(content, out string error))
                    {
                        this.Monitor.Log($"Loading content pack '{contentPack.Manifest.Name}' failed: {error}.", LogLevel.Error);
                        continue;
                    }

                    // init
                    rawContentPack = new RawContentPack(new ManagedContentPack(contentPack), content, migrator);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Error preloading content pack '{contentPack.Manifest.Name}'. Technical details:\n{ex}", LogLevel.Error);
                    continue;
                }

                yield return rawContentPack;
            }
        }

        /// <summary>Load the patches from all registered content packs.</summary>
        /// <param name="contentPacks">The content packs to load.</param>
        /// <param name="installedMods">The mod IDs which are currently installed.</param>
        /// <returns>Returns the loaded content pack IDs.</returns>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure", Justification = "The value is used immediately, so this isn't an issue.")]
        private void LoadContentPacks(IEnumerable<RawContentPack> contentPacks, InvariantHashSet installedMods)
        {
            // load content packs
            ConfigFileHandler configFileHandler = new ConfigFileHandler(this.ConfigFileName, this.ParseCommaDelimitedField, (pack, label, reason) => this.Monitor.Log($"Ignored {pack.Manifest.Name} > {label}: {reason}", LogLevel.Warn));
            foreach (RawContentPack current in contentPacks)
            {
                this.Monitor.VerboseLog($"Loading content pack '{current.Manifest.Name}'...");

                try
                {
                    ContentConfig content = current.Content;

                    // load tokens
                    ModTokenContext modContext = this.TokenManager.TrackLocalTokens(current.ManagedPack);
                    TokenParser tokenParser = new TokenParser(modContext, current.Manifest, current.Migrator, installedMods);
                    {
                        // load config.json
                        InvariantDictionary<ConfigField> config = configFileHandler.Read(current.ManagedPack, content.ConfigSchema, current.Content.Format);
                        configFileHandler.Save(current.ManagedPack, config, this.Helper);
                        if (config.Any())
                            this.Monitor.VerboseLog($"   found config.json with {config.Count} fields...");

                        // load config tokens
                        foreach (KeyValuePair<string, ConfigField> pair in config)
                        {
                            ConfigField field = pair.Value;
                            modContext.Add(new ImmutableToken(pair.Key, field.Value, scope: current.Manifest.UniqueID, allowedValues: field.AllowValues, canHaveMultipleValues: field.AllowMultiple));
                        }

                        // load dynamic tokens
                        foreach (DynamicTokenConfig entry in content.DynamicTokens ?? new DynamicTokenConfig[0])
                        {
                            void LogSkip(string reason) => this.Monitor.Log($"Ignored {current.Manifest.Name} > dynamic token '{entry.Name}': {reason}", LogLevel.Warn);

                            // validate token key
                            if (string.IsNullOrWhiteSpace(entry.Name))
                            {
                                LogSkip("the token name can't be empty.");
                                continue;
                            }
                            if (entry.Name.Contains(InternalConstants.InputArgSeparator))
                            {
                                LogSkip($"the token name can't have an input argument ({InternalConstants.InputArgSeparator} character).");
                                continue;
                            }
                            if (Enum.TryParse<ConditionType>(entry.Name, true, out _))
                            {
                                LogSkip("the token name is already used by a global token.");
                                continue;
                            }
                            if (config.ContainsKey(entry.Name))
                            {
                                LogSkip("the token name is already used by a config token.");
                                continue;
                            }

                            // parse conditions
                            IList<Condition> conditions;
                            InvariantHashSet immutableRequiredModIDs;
                            {
                                if (!this.TryParseConditions(entry.When, tokenParser, out conditions, out immutableRequiredModIDs, out string conditionError))
                                {
                                    this.Monitor.Log($"Ignored {current.Manifest.Name} > '{entry.Name}' token: its {nameof(DynamicTokenConfig.When)} field is invalid: {conditionError}.", LogLevel.Warn);
                                    continue;
                                }
                            }

                            // parse values
                            IParsedTokenString values;
                            if (!string.IsNullOrWhiteSpace(entry.Value))
                            {
                                if (!tokenParser.TryParseStringTokens(entry.Value, immutableRequiredModIDs, out string valueError, out values))
                                {
                                    LogSkip($"the token value is invalid: {valueError}");
                                    continue;
                                }
                            }
                            else
                                values = new LiteralString("");

                            // add token
                            modContext.Add(new DynamicTokenValue(entry.Name, values, conditions));
                        }
                    }

                    // get fake patch context (so patch tokens are available in patch validation)
                    LocalContext fakePatchContext = new LocalContext(current.Manifest.UniqueID, parentContext: modContext);
                    fakePatchContext.SetLocalValue(ConditionType.Target.ToString(), "");
                    fakePatchContext.SetLocalValue(ConditionType.TargetWithoutPath.ToString(), "");
                    tokenParser = new TokenParser(fakePatchContext, current.Manifest, current.Migrator, installedMods);

                    // sanity check
                    {
                        int[] nullPositions = content.Changes
                            .Select((patch, index) => new { patch, index })
                            .Where(p => p.patch == null)
                            .Select(p => p.index + 1)
                            .ToArray();
                        if (nullPositions.Any())
                        {
                            this.Monitor.Log($"Error loading content pack '{current.Manifest.Name}'. Found null patch{(nullPositions.Length == 1 ? "" : "es")} at position{(nullPositions.Length == 1 ? "" : "s")} {string.Join(", ", nullPositions)}.", LogLevel.Error);
                            continue;
                        }
                    }

                    content.Changes = this.SplitPatches(content.Changes).ToArray();
                    this.NamePatches(current.ManagedPack, content.Changes);
                    foreach (PatchConfig patch in content.Changes)
                    {
                        this.Monitor.VerboseLog($"   loading {patch.LogName}...");
                        this.LoadPatch(current.ManagedPack, patch, tokenParser, logSkip: reasonPhrase => this.Monitor.Log($"Ignored {patch.LogName}: {reasonPhrase}", LogLevel.Warn));
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Error loading content pack '{current.Manifest.Name}'. Technical details:\n{ex}", LogLevel.Error);
                    continue;
                }
            }
        }

        /// <summary>Split patches with multiple target values.</summary>
        /// <param name="patches">The patches to split.</param>
        private IEnumerable<PatchConfig> SplitPatches(IEnumerable<PatchConfig> patches)
        {
            foreach (PatchConfig patch in patches)
            {
                if (string.IsNullOrWhiteSpace(patch.Target) || !patch.Target.Contains(","))
                {
                    yield return patch;
                    continue;
                }

                foreach (string target in patch.Target.Split(',').Select(p => p.Trim()).Distinct(StringComparer.InvariantCultureIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(target))
                        continue;

                    yield return new PatchConfig(patch)
                    {
                        LogName = !string.IsNullOrWhiteSpace(patch.LogName) ? $"{patch.LogName} > {target}" : "",
                        Target = target
                    };
                }
            }
        }

        /// <summary>Set a unique name for all patches in a content pack.</summary>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="patches">The patches to name.</param>
        private void NamePatches(ManagedContentPack contentPack, PatchConfig[] patches)
        {
            // add default log names
            foreach (PatchConfig patch in patches)
            {
                if (string.IsNullOrWhiteSpace(patch.LogName))
                    patch.LogName = $"{patch.Action} {patch.Target}";
            }

            // make names unique within content pack
            foreach (var patchGroup in patches.GroupBy(p => p.LogName, StringComparer.InvariantCultureIgnoreCase).Where(p => p.Count() > 1))
            {
                int i = 0;
                foreach (var patch in patchGroup)
                    patch.LogName += $" #{++i}";
            }

            // prefix with content pack name
            foreach (var patch in patches)
                patch.LogName = $"{contentPack.Manifest.Name} > {patch.LogName}";
        }

        /// <summary>Load one patch from a content pack's <c>content.json</c> file.</summary>
        /// <param name="pack">The content pack being loaded.</param>
        /// <param name="entry">The change to load.</param>
        /// <param name="tokenParser">Handles low-level parsing and validation for tokens.</param>
        /// <param name="logSkip">The callback to invoke with the error reason if loading it fails.</param>
        private bool LoadPatch(ManagedContentPack pack, PatchConfig entry, TokenParser tokenParser, Action<string> logSkip)
        {
            bool TrackSkip(string reason, bool warn = true)
            {
                reason = reason.TrimEnd('.', ' ');
                this.PatchManager.AddPermanentlyDisabled(new DisabledPatch(entry.LogName, entry.Action, entry.Target, pack, reason));
                if (warn)
                    logSkip(reason + '.');
                return false;
            }

            try
            {
                // normalize patch fields
                if (entry.When == null)
                    entry.When = new InvariantDictionary<string>();

                // parse action
                if (!Enum.TryParse(entry.Action, true, out PatchType action))
                {
                    return TrackSkip(string.IsNullOrWhiteSpace(entry.Action)
                        ? $"must set the {nameof(PatchConfig.Action)} field"
                        : $"invalid {nameof(PatchConfig.Action)} value '{entry.Action}', expected one of: {string.Join(", ", Enum.GetNames(typeof(PatchType)))}"
                    );
                }

                // parse conditions
                IList<Condition> conditions;
                InvariantHashSet immutableRequiredModIDs;
                {
                    if (!this.TryParseConditions(entry.When, tokenParser, out conditions, out immutableRequiredModIDs, out string error))
                        return TrackSkip($"the {nameof(PatchConfig.When)} field is invalid: {error}");
                }

                // parse target asset
                IParsedTokenString assetName;
                {
                    if (string.IsNullOrWhiteSpace(entry.Target))
                        return TrackSkip($"must set the {nameof(PatchConfig.Target)} field");
                    if (!tokenParser.TryParseStringTokens(entry.Target, immutableRequiredModIDs, out string error, out assetName))
                        return TrackSkip($"the {nameof(PatchConfig.Target)} is invalid: {error}");
                }

                // parse 'enabled'
                bool enabled = true;
                {
                    if (entry.Enabled != null && !this.TryParseEnabled(entry.Enabled, tokenParser, immutableRequiredModIDs, out string error, out enabled))
                        return TrackSkip($"invalid {nameof(PatchConfig.Enabled)} value '{entry.Enabled}': {error}");
                }

                // get patch instance
                IPatch patch;
                switch (action)
                {
                    // load asset
                    case PatchType.Load:
                        {
                            // init patch
                            if (!this.TryPrepareLocalAsset(entry.FromFile, tokenParser, immutableRequiredModIDs, out string error, out IParsedTokenString fromAsset))
                                return TrackSkip(error);
                            patch = new LoadPatch(entry.LogName, pack, assetName, conditions, fromAsset, this.Helper.Content.NormalizeAssetName);
                        }
                        break;

                    // edit data
                    case PatchType.EditData:
                        {
                            // validate
                            if (entry.Entries == null && entry.Fields == null && entry.MoveEntries == null && entry.FromFile == null)
                                return TrackSkip($"one of {nameof(PatchConfig.Entries)}, {nameof(PatchConfig.Fields)}, {nameof(PatchConfig.MoveEntries)}, or {nameof(PatchConfig.FromFile)} must be specified for an '{action}' change");
                            if (entry.FromFile != null && (entry.Entries != null || entry.Fields != null || entry.MoveEntries != null))
                                return TrackSkip($"{nameof(PatchConfig.FromFile)} is mutually exclusive with {nameof(PatchConfig.Entries)}, {nameof(PatchConfig.Fields)}, and {nameof(PatchConfig.MoveEntries)}");

                            // parse 'from file' field
                            IParsedTokenString fromAsset = null;
                            if (entry.FromFile != null && !this.TryPrepareLocalAsset(entry.FromFile, tokenParser, immutableRequiredModIDs, out string error, out fromAsset))
                                return TrackSkip(error);

                            // parse data changes
                            bool TryParseFields(IContext context, PatchConfig rawFields, out List<EditDataPatchRecord> parsedEntries, out List<EditDataPatchField> parsedFields, out List<EditDataPatchMoveRecord> parsedMoveEntries, out string parseError)
                            {
                                return this.TryParseEditDataFields(rawFields, tokenParser, immutableRequiredModIDs, out parsedEntries, out parsedFields, out parsedMoveEntries, out parseError);
                            }
                            List<EditDataPatchRecord> entries = null;
                            List<EditDataPatchField> fields = null;
                            List<EditDataPatchMoveRecord> moveEntries = null;
                            if (entry.FromFile == null)
                            {
                                if (!TryParseFields(tokenParser.Context, entry, out entries, out fields, out moveEntries, out error))
                                    return TrackSkip(error);
                            }

                            // save
                            patch = new EditDataPatch(
                                logName: entry.LogName,
                                contentPack: pack,
                                assetName: assetName,
                                conditions: conditions,
                                fromFile: fromAsset,
                                records: entries,
                                fields: fields,
                                moveRecords: moveEntries,
                                monitor: this.Monitor,
                                normalizeAssetName: this.Helper.Content.NormalizeAssetName,
                                tryParseFields: TryParseFields
                            );
                        }
                        break;

                    // edit image
                    case PatchType.EditImage:
                        {
                            // read patch mode
                            PatchMode patchMode = PatchMode.Replace;
                            if (!string.IsNullOrWhiteSpace(entry.PatchMode) && !Enum.TryParse(entry.PatchMode, true, out patchMode))
                                return TrackSkip($"the {nameof(PatchConfig.PatchMode)} is invalid. Expected one of these values: [{string.Join(", ", Enum.GetNames(typeof(PatchMode)))}]");

                            // read from area
                            TokenRectangle fromArea = null;
                            if (entry.FromArea != null && !this.TryParseRectangle(entry.FromArea, tokenParser, immutableRequiredModIDs, out string error, out fromArea))
                                return TrackSkip(error);

                            // read to area
                            TokenRectangle toArea = null;
                            if (entry.ToArea != null && !this.TryParseRectangle(entry.ToArea, tokenParser, immutableRequiredModIDs, out error, out toArea))
                                return TrackSkip(error);

                            // save
                            if (!this.TryPrepareLocalAsset(entry.FromFile, tokenParser, immutableRequiredModIDs, out error, out IParsedTokenString fromAsset))
                                return TrackSkip(error);
                            patch = new EditImagePatch(entry.LogName, pack, assetName, conditions, fromAsset, fromArea, toArea, patchMode, this.Monitor, this.Helper.Content.NormalizeAssetName);
                        }
                        break;

                    // edit map
                    case PatchType.EditMap:
                        {
                            // read map asset
                            IParsedTokenString fromAsset = null;
                            if (entry.FromFile != null && !this.TryPrepareLocalAsset(entry.FromFile, tokenParser, immutableRequiredModIDs, out string error, out fromAsset))
                                return TrackSkip(error);

                            // read map properties
                            List<EditMapPatchProperty> mapProperties = null;
                            if (entry.MapProperties?.Any() == true)
                            {
                                mapProperties = new List<EditMapPatchProperty>();
                                foreach (var pair in entry.MapProperties)
                                {
                                    if (!tokenParser.TryParseStringTokens(pair.Key, immutableRequiredModIDs, out error, out IParsedTokenString key))
                                        return TrackSkip($"{nameof(PatchConfig.MapProperties)} > '{pair.Key}' key is invalid: {error}");
                                    if (!tokenParser.TryParseStringTokens(pair.Value, immutableRequiredModIDs, out error, out IParsedTokenString value))
                                        return TrackSkip($"{nameof(PatchConfig.MapProperties)} > '{pair.Key}' value '{pair.Value}' is invalid: {error}");

                                    mapProperties.Add(new EditMapPatchProperty(key, value));
                                }
                            }

                            // read from/to asset areas
                            TokenRectangle fromArea = null;
                            if (entry.FromArea != null && !this.TryParseRectangle(entry.FromArea, tokenParser, immutableRequiredModIDs, out error, out fromArea))
                                return TrackSkip(error);
                            TokenRectangle toArea = null;
                            if (entry.ToArea != null && !this.TryParseRectangle(entry.ToArea, tokenParser, immutableRequiredModIDs, out error, out toArea))
                                return TrackSkip(error);

                            // validate
                            if (fromAsset == null && mapProperties == null)
                                return TrackSkip($"must specify at least one of {nameof(entry.FromFile)} or {entry.MapProperties}");
                            if (fromAsset != null && entry.ToArea == null)
                                return TrackSkip($"must specify {nameof(entry.ToArea)} when using {nameof(entry.FromFile)} (use \"Action\": \"Load\" if you want to replace the whole map file)");

                            // save
                            patch = new EditMapPatch(entry.LogName, pack, assetName, conditions, fromAsset, fromArea, toArea, mapProperties, this.Monitor, this.Helper.Content.NormalizeAssetName);
                        }
                        break;

                    default:
                        return TrackSkip($"unsupported patch type '{action}'");
                }

                // skip if not enabled
                // note: we process the patch even if it's disabled, so any errors are caught by the modder instead of only failing after the patch is enabled.
                if (!enabled)
                    return TrackSkip($"{nameof(PatchConfig.Enabled)} is false", warn: false);

                // save patch
                this.PatchManager.Add(patch);
                return true;
            }
            catch (Exception ex)
            {
                return TrackSkip($"error reading info. Technical details:\n{ex}");
            }
        }

        /// <summary>Parse the data change fields for an <see cref="PatchType.EditData"/> patch.</summary>
        /// <param name="entry">The change to load.</param>
        /// <param name="tokenParser">Handles low-level parsing and validation for tokens.</param>
        /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
        /// <param name="entries">The parsed data entry changes.</param>
        /// <param name="fields">The parsed data field changes.</param>
        /// <param name="moveEntries">The parsed move entry records.</param>
        /// <param name="error">The error message indicating why parsing failed, if applicable.</param>
        /// <returns>Returns whether parsing succeeded.</returns>
        bool TryParseEditDataFields(PatchConfig entry, TokenParser tokenParser, InvariantHashSet assumeModIds, out List<EditDataPatchRecord> entries, out List<EditDataPatchField> fields, out List<EditDataPatchMoveRecord> moveEntries, out string error)
        {
            entries = new List<EditDataPatchRecord>();
            fields = new List<EditDataPatchField>();
            moveEntries = new List<EditDataPatchMoveRecord>();

            bool Fail(string reason, out string outReason)
            {
                outReason = reason;
                return false;
            }

            // parse entries
            if (entry.Entries != null)
            {
                foreach (KeyValuePair<string, JToken> pair in entry.Entries)
                {
                    if (!tokenParser.TryParseStringTokens(pair.Key, assumeModIds, out string keyError, out IParsedTokenString key))
                        return Fail($"{nameof(PatchConfig.Entries)} > '{pair.Key}' key is invalid: {keyError}", out error);
                    if (!tokenParser.TryParseJsonTokens(pair.Value, assumeModIds, out string valueError, out TokenizableJToken value))
                        return Fail($"{nameof(PatchConfig.Entries)} > '{pair.Key}' value is invalid: {valueError}", out error);

                    entries.Add(new EditDataPatchRecord(key, value));
                }
            }

            // parse fields
            if (entry.Fields != null)
            {
                foreach (KeyValuePair<string, IDictionary<string, JToken>> recordPair in entry.Fields)
                {
                    // parse entry key
                    if (!tokenParser.TryParseStringTokens(recordPair.Key, assumeModIds, out string keyError, out IParsedTokenString key))
                        return Fail($"{nameof(PatchConfig.Fields)} > entry {recordPair.Key} is invalid: {keyError}", out error);

                    // parse fields
                    foreach (var fieldPair in recordPair.Value)
                    {
                        // parse field key
                        if (!tokenParser.TryParseStringTokens(fieldPair.Key, assumeModIds, out string fieldError, out IParsedTokenString fieldKey))
                            return Fail($"{nameof(PatchConfig.Fields)} > entry {recordPair.Key} > field {fieldPair.Key} key is invalid: {fieldError}", out error);

                        // parse value
                        if (!tokenParser.TryParseJsonTokens(fieldPair.Value, assumeModIds, out string valueError, out TokenizableJToken value))
                            return Fail($"{nameof(PatchConfig.Fields)} > entry {recordPair.Key} > field {fieldKey} is invalid: {valueError}", out error);
                        if (value?.Value is JValue jValue && jValue.Value<string>()?.Contains("/") == true)
                            return Fail($"{nameof(PatchConfig.Fields)} > entry {recordPair.Key} > field {fieldKey} is invalid: value can't contain field delimiter character '/'", out error);

                        fields.Add(new EditDataPatchField(key, fieldKey, value));
                    }
                }
            }

            // parse move entries
            if (entry.MoveEntries != null)
            {
                foreach (PatchMoveEntryConfig moveEntry in entry.MoveEntries)
                {
                    // validate
                    string[] targets = new[] { moveEntry.BeforeID, moveEntry.AfterID, moveEntry.ToPosition };
                    if (string.IsNullOrWhiteSpace(moveEntry.ID))
                        return Fail($"{nameof(PatchConfig.MoveEntries)} > move entry is invalid: must specify an {nameof(PatchMoveEntryConfig.ID)} value", out error);
                    if (targets.All(string.IsNullOrWhiteSpace))
                        return Fail($"{nameof(PatchConfig.MoveEntries)} > entry '{moveEntry.ID}' is invalid: must specify one of {nameof(PatchMoveEntryConfig.ToPosition)}, {nameof(PatchMoveEntryConfig.BeforeID)}, or {nameof(PatchMoveEntryConfig.AfterID)}", out error);
                    if (targets.Count(p => !string.IsNullOrWhiteSpace(p)) > 1)
                        return Fail($"{nameof(PatchConfig.MoveEntries)} > entry '{moveEntry.ID}' is invalid: must specify only one of {nameof(PatchMoveEntryConfig.ToPosition)}, {nameof(PatchMoveEntryConfig.BeforeID)}, and {nameof(PatchMoveEntryConfig.AfterID)}", out error);

                    // parse IDs
                    if (!tokenParser.TryParseStringTokens(moveEntry.ID, assumeModIds, out string idError, out IParsedTokenString moveId))
                        return Fail($"{nameof(PatchConfig.MoveEntries)} > entry '{moveEntry.ID}' > {nameof(PatchMoveEntryConfig.ID)} is invalid: {idError}", out error);
                    if (!tokenParser.TryParseStringTokens(moveEntry.BeforeID, assumeModIds, out string beforeIdError, out IParsedTokenString beforeId))
                        return Fail($"{nameof(PatchConfig.MoveEntries)} > entry '{moveEntry.ID}' > {nameof(PatchMoveEntryConfig.BeforeID)} is invalid: {beforeIdError}", out error);
                    if (!tokenParser.TryParseStringTokens(moveEntry.AfterID, assumeModIds, out string afterIdError, out IParsedTokenString afterId))
                        return Fail($"{nameof(PatchConfig.MoveEntries)} > entry '{moveEntry.ID}' > {nameof(PatchMoveEntryConfig.AfterID)} is invalid: {afterIdError}", out error);

                    // parse position
                    MoveEntryPosition toPosition = MoveEntryPosition.None;
                    if (!string.IsNullOrWhiteSpace(moveEntry.ToPosition) && (!Enum.TryParse(moveEntry.ToPosition, true, out toPosition) || toPosition == MoveEntryPosition.None))
                        return Fail($"{nameof(PatchConfig.MoveEntries)} > entry '{moveEntry.ID}' > {nameof(PatchMoveEntryConfig.ToPosition)} is invalid: must be one of {nameof(MoveEntryPosition.Bottom)} or {nameof(MoveEntryPosition.Top)}", out error);

                    // create move entry
                    moveEntries.Add(new EditDataPatchMoveRecord(moveId, beforeId, afterId, toPosition));
                }
            }

            error = null;
            return true;
        }

        /// <summary>Normalize and parse the given condition values.</summary>
        /// <param name="raw">The raw condition values to normalize.</param>
        /// <param name="tokenParser">Handles low-level parsing and validation for tokens.</param>
        /// <param name="conditions">The normalized conditions.</param>
        /// <param name="immutableRequiredModIDs">The immutable mod IDs always required by these conditions (if they're <see cref="ConditionType.HasMod"/> and immutable).</param>
        /// <param name="error">An error message indicating why normalization failed.</param>
        private bool TryParseConditions(InvariantDictionary<string> raw, TokenParser tokenParser, out IList<Condition> conditions, out InvariantHashSet immutableRequiredModIDs, out string error)
        {
            conditions = new List<Condition>();
            immutableRequiredModIDs = new InvariantHashSet();

            // no conditions
            if (raw == null || !raw.Any())
            {
                error = null;
                return true;
            }

            // parse conditions
            Lexer lexer = new Lexer();
            foreach (KeyValuePair<string, string> pair in raw)
            {
                if (!this.TryParseCondition(pair.Key, pair.Value, tokenParser, lexer, out Condition condition, out InvariantHashSet localImmutableRequiredModIDs, out error))
                {
                    conditions = null;
                    return false;
                }

                if (localImmutableRequiredModIDs != null)
                {
                    foreach (string id in localImmutableRequiredModIDs)
                        immutableRequiredModIDs.Add(id);
                }
                conditions.Add(condition);
            }

            error = null;
            return true;
        }

        /// <summary>Normalize and parse the given condition values.</summary>
        /// <param name="name">The raw condition name.</param>
        /// <param name="value">The raw condition value.</param>
        /// <param name="tokenParser">Handles low-level parsing and validation for tokens.</param>
        /// <param name="lexer">Handles parsing raw strings into tokens.</param>
        /// <param name="condition">The normalized condition.</param>
        /// <param name="immutableRequiredModIDs">The immutable mod IDs always required by this condition (if it's <see cref="ConditionType.HasMod"/> and immutable).</param>
        /// <param name="error">An error message indicating why normalization failed.</param>
        private bool TryParseCondition(string name, string value, TokenParser tokenParser, Lexer lexer, out Condition condition, out InvariantHashSet immutableRequiredModIDs, out string error)
        {
            bool Fail(string reason, out string setError, out Condition setCondition, out InvariantHashSet setImmutableRequiredModIDs)
            {
                setCondition = null;
                setImmutableRequiredModIDs = null;
                setError = reason;
                return false;
            }

            // get lexical tokens
            ILexToken[] lexTokens = lexer.ParseBits(name, impliedBraces: true).ToArray();
            for (int i = 0; i < lexTokens.Length; i++)
            {
                if (!tokenParser.Migrator.TryMigrate(ref lexTokens[0], out error))
                    return Fail(error, out error, out condition, out immutableRequiredModIDs);
            }

            // parse condition key
            if (lexTokens.Length != 1 || !(lexTokens[0] is LexTokenToken lexToken))
                return Fail($"'{name}' isn't a valid token name", out error, out condition, out immutableRequiredModIDs);

            ITokenString input = new TokenString(lexToken.InputArg, tokenParser.Context);

            // get token
            IToken token = tokenParser.Context.GetToken(lexToken.Name, enforceContext: false);
            if (token == null)
                return Fail($"'{name}' isn't a valid condition; must be one of {string.Join(", ", tokenParser.Context.GetTokens(enforceContext: false).Select(p => p.Name).OrderBy(p => p))}", out error, out condition, out immutableRequiredModIDs);
            if (!tokenParser.TryValidateToken(lexToken, assumeModIds: null, out error))
                return Fail(error, out error, out condition, out immutableRequiredModIDs);

            // validate input
            if (!token.TryValidateInput(input, out error))
                return Fail(error, out error, out condition, out immutableRequiredModIDs);

            // parse values
            if (string.IsNullOrWhiteSpace(value))
                return Fail($"can't parse condition {name}: value can't be empty", out error, out condition, out immutableRequiredModIDs);
            if (!tokenParser.TryParseStringTokens(value, assumeModIds: null, out error, out IParsedTokenString values))
                return Fail($"can't parse condition {name}: {error}", out error, out condition, out immutableRequiredModIDs);

            // validate token keys & values
            if (!values.IsMutable && !token.TryValidateValues(input, values.SplitValuesUnique(), tokenParser.Context, out string customError))
                return Fail($"invalid {lexToken.Name} condition: {customError}", out error, out condition, out immutableRequiredModIDs);

            // create condition
            condition = new Condition(name: token.Name, input: input, values: values);

            // extract HasMod required IDs if immutable
            immutableRequiredModIDs = null;
            if (condition.IsReady && !condition.IsMutable && condition.Is(ConditionType.HasMod))
            {
                if (!condition.HasInput())
                    immutableRequiredModIDs = condition.CurrentValues;
                else if (bool.TryParse(condition.Values.Value, out bool required) && required)
                    immutableRequiredModIDs = new InvariantHashSet(condition.Input.Value);
            }

            return true;
        }

        /// <summary>Parse a comma-delimited set of case-insensitive condition values.</summary>
        /// <param name="field">The field value to parse.</param>
        public InvariantHashSet ParseCommaDelimitedField(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
                return new InvariantHashSet();

            IEnumerable<string> values = (
                from value in field.Split(',')
                where !string.IsNullOrWhiteSpace(value)
                select value.Trim()
            );
            return new InvariantHashSet(values);
        }

        /// <summary>Parse a boolean <see cref="PatchConfig.Enabled"/> value from a string which can contain tokens, and validate that it's valid.</summary>
        /// <param name="rawValue">The raw string which may contain tokens.</param>
        /// <param name="tokenParser">The  tokens available for this content pack.</param>
        /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value.</param>
        private bool TryParseEnabled(string rawValue, TokenParser tokenParser, InvariantHashSet assumeModIds, out string error, out bool parsed)
        {
            parsed = false;

            // analyze string
            if (!tokenParser.TryParseStringTokens(rawValue, assumeModIds, out error, out IParsedTokenString tokenString))
                return false;

            // validate & extract tokens
            string text = rawValue;
            if (tokenString.HasAnyTokens)
            {
                // only one token allowed
                if (!tokenString.IsSingleTokenOnly)
                {
                    error = "can't be treated as a true/false value because it contains multiple tokens.";
                    return false;
                }

                // parse token
                LexTokenToken lexToken = tokenString.GetTokenPlaceholders(recursive: false).Single();
                IToken token = tokenParser.Context.GetToken(lexToken.Name, enforceContext: false);
                ITokenString input = new TokenString(lexToken.InputArg, tokenParser.Context);

                // check token options
                InvariantHashSet allowedValues = token?.GetAllowedValues(input);
                if (token == null || token.IsMutable || !token.IsReady)
                {
                    error = $"can only use static tokens in this field, consider using a {nameof(PatchConfig.When)} condition instead.";
                    return false;
                }
                if (allowedValues == null || !allowedValues.All(p => bool.TryParse(p, out _)))
                {
                    error = "that token isn't restricted to 'true' or 'false'.";
                    return false;
                }
                if (token.CanHaveMultipleValues(input))
                {
                    error = "can't be treated as a true/false value because that token can have multiple values.";
                    return false;
                }

                text = token.GetValues(input).First();
            }

            // parse text
            if (!bool.TryParse(text, out parsed))
            {
                error = $"can't parse {tokenString.Raw} as a true/false value.";
                return false;
            }
            return true;
        }

        /// <summary>Parse a tokenizable rectangle from its parts, and validate that it's valid.</summary>
        /// <param name="raw">The raw rectangle to parse.</param>
        /// <param name="tokenParser">The tokens available for this content pack.</param>
        /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value.</param>
        private bool TryParseRectangle(PatchRectangleConfig raw, TokenParser tokenParser, InvariantHashSet assumeModIds, out string error, out TokenRectangle parsed)
        {
            bool TryParseField(string rawField, string name, out ITokenString result, out string parseError)
            {
                if (!this.TryParseInt(rawField, tokenParser, assumeModIds, out parseError, out result))
                {
                    parseError = $"invalid {name}: {parseError}";
                    return false;
                }
                return true;
            }

            if (
                !TryParseField(raw.X, nameof(raw.X), out ITokenString x, out error)
                || !TryParseField(raw.Y, nameof(raw.Y), out ITokenString y, out error)
                || !TryParseField(raw.Width, nameof(raw.Width), out ITokenString width, out error)
                || !TryParseField(raw.Height, nameof(raw.Height), out ITokenString height, out error)
            )
            {
                parsed = null;
                return false;
            }

            parsed = new TokenRectangle(x, y, width, height);
            return true;
        }

        /// <summary>Parse an integer value from a string which can contain tokens, and validate that it's valid.</summary>
        /// <param name="rawString">The raw string which may contain tokens.</param>
        /// <param name="tokenParser">The  tokens available for this content pack.</param>
        /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
        /// <param name="error">An error phrase indicating why parsing failed (if applicable).</param>
        /// <param name="parsed">The parsed value.</param>
        private bool TryParseInt(string rawString, TokenParser tokenParser, InvariantHashSet assumeModIds, out string error, out ITokenString parsed)
        {
            parsed = null;

            // analyze string
            if (!tokenParser.TryParseStringTokens(rawString, assumeModIds, out error, out IParsedTokenString tokenString))
                return false;

            // validate tokens
            if (tokenString.HasAnyTokens)
            {
                // only one token allowed
                if (!tokenString.IsSingleTokenOnly)
                {
                    error = "can't be treated as a number because it contains multiple tokens.";
                    return false;
                }

                // parse token
                LexTokenToken lexToken = tokenString.GetTokenPlaceholders(recursive: false).Single();
                IToken token = tokenParser.Context.GetToken(lexToken.Name, enforceContext: false);
                ITokenString input = new TokenString(lexToken.InputArg, tokenParser.Context);

                // check token options
                InvariantHashSet allowedValues = token?.GetAllowedValues(input);
                if (allowedValues == null || !allowedValues.All(p => int.TryParse(p, out _)))
                {
                    error = "that token isn't restricted to integers.";
                    return false;
                }
                if (token.CanHaveMultipleValues(input))
                {
                    error = "can't be treated as a number because that token can have multiple values.";
                    return false;
                }
            }

            parsed = tokenString;

            return true;
        }

        /// <summary>Prepare a local asset file for a patch to use.</summary>
        /// <param name="path">The asset path in the content patch.</param>
        /// <param name="tokenParser">Handles low-level parsing and validation for tokens.</param>
        /// <param name="assumeModIds">Mod IDs to assume are installed for purposes of token validation.</param>
        /// <param name="error">The error reason if preparing the asset fails.</param>
        /// <param name="tokenedPath">The parsed value.</param>
        /// <returns>Returns whether the local asset was successfully prepared.</returns>
        private bool TryPrepareLocalAsset(string path, TokenParser tokenParser, InvariantHashSet assumeModIds, out string error, out IParsedTokenString tokenedPath)
        {
            // normalize raw value
            path = path?.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                error = $"must set the {nameof(PatchConfig.FromFile)} field for this action type.";
                tokenedPath = null;
                return false;
            }

            // tokenize
            if (!tokenParser.TryParseStringTokens(path, assumeModIds, out string tokenError, out tokenedPath))
            {
                error = $"the {nameof(PatchConfig.FromFile)} is invalid: {tokenError}";
                tokenedPath = null;
                return false;
            }

            // looks OK
            error = null;
            return true;
        }
    }
}
