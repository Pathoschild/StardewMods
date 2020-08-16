using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Commands;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using ContentPatcher.Framework.Validators;
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

        /// <summary>The recognized format versions and their migrations.</summary>
        private readonly Func<ContentConfig, IMigration[]> GetFormatVersions = content => new IMigration[]
        {
            new Migration_1_0(),
            new Migration_1_3(),
            new Migration_1_4(),
            new Migration_1_5(),
            new Migration_1_6(),
            new Migration_1_7(),
            new Migration_1_8(),
            new Migration_1_9(),
            new Migration_1_10(),
            new Migration_1_11(),
            new Migration_1_13(),
            new Migration_1_14(),
            new Migration_1_15_Prevalidation(),
            new Migration_1_15_Rewrites(content),
            new Migration_1_16(),
            new Migration_1_17()
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

        /// <summary>Handles loading and unloading patches.</summary>
        private PatchLoader PatchLoader;

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

        /// <summary>The loaded content packs.</summary>
        private readonly IList<RawContentPack> RawContentPacks = new List<RawContentPack>();


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            this.Keys = this.Config.Controls.ParseControls(helper.Input, this.Monitor);

            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        /// <summary>Get an API that other mods can access. This is always called after <see cref="Entry"/>.</summary>
        public override object GetApi()
        {
            return new ContentPatcherAPI(this.ModManifest.UniqueID, this.Monitor, this.Helper.Reflection, this.AddModToken);
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Event handlers
        ****/
        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this.Config.EnableDebugFeatures)
            {
                // toggle overlay
                if (this.Keys.ToggleDebug.JustPressedUnique())
                {
                    if (this.DebugOverlay == null)
                        this.DebugOverlay = new DebugOverlay(this.Helper.Events, this.Helper.Input, this.Helper.Content, this.Helper.Reflection);
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
                    if (this.Keys.DebugPrevTexture.JustPressedUnique())
                        this.DebugOverlay.PrevTexture();
                    if (this.Keys.DebugNextTexture.JustPressedUnique())
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
                    this.UpdateContext(ContextUpdateType.All);
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
            this.UpdateContext(ContextUpdateType.All);
        }

        /// <summary>The method invoked when the player warps.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            this.Monitor.VerboseLog("Updating context: player warped.");
            this.UpdateContext(ContextUpdateType.OnLocationChange);
        }

        /// <summary>The method invoked when the player returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.Monitor.VerboseLog("Updating context: returned to title.");
            this.TokenManager.IsBasicInfoLoaded = false;
            this.UpdateContext(ContextUpdateType.All);
        }

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

        /****
        ** Methods
        ****/
        /// <summary>Initialize the mod and content packs.</summary>
        private void Initialize()
        {
            var helper = this.Helper;

            // fetch content packs
            RawContentPack[] contentPacks = this.GetContentPacks().ToArray();
            InvariantHashSet installedMods = new InvariantHashSet(
                (contentPacks.Select(p => p.Manifest.UniqueID))
                .Concat(helper.ModRegistry.GetAll().Select(p => p.Manifest.UniqueID))
                .OrderByIgnoreCase(p => p)
            );

            // log custom tokens
            {
                var tokensByMod = (
                    from token in this.QueuedModTokens
                    orderby token.Name
                    group token by token.Mod into modGroup
                    select new { ModName = modGroup.Key.Name, ModPrefix = modGroup.First().NamePrefix, TokenNames = modGroup.Select(p => p.NameWithoutPrefix).ToArray() }
                );
                foreach (var group in tokensByMod)
                    this.Monitor.Log($"{group.ModName} added {(group.TokenNames.Length == 1 ? "a custom token" : $"{group.TokenNames.Length} custom tokens")} with prefix '{group.ModPrefix}': {string.Join(", ", group.TokenNames)}.");
            }

            // load content packs and context
            this.TokenManager = new TokenManager(helper.Content, installedMods, this.QueuedModTokens, this.Helper.Reflection);
            this.PatchManager = new PatchManager(this.Monitor, this.TokenManager, this.AssetValidators());
            this.PatchLoader = new PatchLoader(this.PatchManager, this.TokenManager, this.Monitor, this.Helper.Reflection, installedMods, this.Helper.Content.NormalizeAssetName);
            this.UpdateContext(ContextUpdateType.All); // set initial context before loading any custom mod tokens

            // load context
            this.LoadContentPacks(contentPacks, installedMods);
            this.UpdateContext(ContextUpdateType.All); // set initial context once patches + dynamic tokens + custom tokens are loaded

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
            this.CommandHandler = new CommandHandler(this.TokenManager, this.PatchManager, this.PatchLoader, this.Monitor, this.RawContentPacks, modID => modID == null ? this.TokenManager : this.TokenManager.GetContextFor(modID), () => this.UpdateContext(ContextUpdateType.All));
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

            this.QueuedModTokens.Add(token);
        }

        /// <summary>Update the current context.</summary>
        /// <param name="updateType">The context update type.</param>
        private void UpdateContext(ContextUpdateType updateType)
        {
            this.TokenManager.UpdateContext(out InvariantHashSet changedGlobalTokens);
            this.PatchManager.UpdateContext(this.Helper.Content, changedGlobalTokens, updateType);
        }

        /// <summary>Load the registered content packs.</summary>
        /// <returns>Returns the loaded content pack IDs.</returns>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure", Justification = "The value is used immediately, so this isn't an issue.")]
        private IEnumerable<RawContentPack> GetContentPacks()
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
                    IMigration migrator = new AggregateMigration(content.Format, this.GetFormatVersions(content));
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
                LogPathBuilder path = new LogPathBuilder(current.Manifest.Name);

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
                            IValueProvider valueProvider = new ImmutableValueProvider(pair.Key, field.Value, allowedValues: field.AllowValues, canHaveMultipleValues: field.AllowMultiple);
                            modContext.AddLocalToken(new Token(valueProvider, scope: current.Manifest.UniqueID));
                        }

                        // load dynamic tokens
                        IDictionary<string, int> dynamicTokenCountByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        foreach (DynamicTokenConfig entry in content.DynamicTokens ?? new DynamicTokenConfig[0])
                        {
                            void LogSkip(string reason) => this.Monitor.Log($"Ignored {current.Manifest.Name} > dynamic token '{entry.Name}': {reason}", LogLevel.Warn);

                            // get path
                            LogPathBuilder localPath = path.With(nameof(content.DynamicTokens));
                            {
                                if (!dynamicTokenCountByName.ContainsKey(entry.Name))
                                    dynamicTokenCountByName[entry.Name] = -1;
                                int discriminator = ++dynamicTokenCountByName[entry.Name];
                                localPath = localPath.With($"{entry.Name} {discriminator}");
                            }

                            // validate token key
                            if (string.IsNullOrWhiteSpace(entry.Name))
                            {
                                LogSkip("the token name can't be empty.");
                                continue;
                            }
                            if (entry.Name.Contains(InternalConstants.PositionalInputArgSeparator))
                            {
                                LogSkip($"the token name can't have positional input arguments ({InternalConstants.PositionalInputArgSeparator} character).");
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
                                if (!this.PatchLoader.TryParseConditions(entry.When, tokenParser, localPath.With(nameof(entry.When)), out conditions, out immutableRequiredModIDs, out string conditionError))
                                {
                                    this.Monitor.Log($"Ignored {current.Manifest.Name} > '{entry.Name}' token: its {nameof(DynamicTokenConfig.When)} field is invalid: {conditionError}.", LogLevel.Warn);
                                    continue;
                                }
                            }

                            // parse values
                            IManagedTokenString values;
                            if (!string.IsNullOrWhiteSpace(entry.Value))
                            {
                                if (!tokenParser.TryParseString(entry.Value, immutableRequiredModIDs, localPath.With(nameof(entry.Value)), out string valueError, out values))
                                {
                                    LogSkip($"the token value is invalid: {valueError}");
                                    continue;
                                }
                            }
                            else
                                values = new LiteralString("", localPath.With(nameof(entry.Value)));

                            // add token
                            modContext.AddDynamicToken(entry.Name, values, conditions);
                        }
                    }

                    // load patches
                    this.PatchLoader.LoadPatches(current, content.Changes, path, reindex: false, parentPatch: null);

                    // add to content pack list
                    this.RawContentPacks.Add(current);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Error loading content pack '{current.Manifest.Name}'. Technical details:\n{ex}", LogLevel.Error);
                    continue;
                }
            }

            this.PatchManager.Reindex(patchListChanged: true);
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
    }
}
